using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing;


namespace SketchReinforcement
{
    public partial class SketchCommand : IExternalCommand
    {
        /// <summary>
        /// Выполнить создание эскизов для арматуры
        /// </summary>

        ///// <param name="doc">Документ текущего проекта</param>
        ///// <param name="sortedrebar">Сортированный список стержней для создания эскизов</param>
        ///// <param name="rebar">Cписок стержней для создания эскизов</param>
        ///// <param name="rebars">Полный список стержней проекта</param>
        ///// <param name="path_name">Папка с эскизами</param>
        ///// <param name="template">Шаблон проекта</param>
        ///// <param name="template">Все эскизы проекта</param>

        // void SketchReinforcement(Document doc, SortedList<string, CodeImage> sortedrebar, List<Element> rebar, List<Element> rebars, DataForm dataform, Template template, List<Element> all_images, StreamWriter writer = null)
        void SketchReinforcement()
        {            

            // открываем цикл по сортированному списку стержней
            // foreach (CodeImage ci in sortedrebar.Values)
            foreach (CodeImage ci in sortedImages.Values)
            {

                // Start = DateTime.Now;

                // получить стержень текущего списка
                Element bar = doc.GetElement(ci.element);
                // получить имя файла для первого стержня группы  
                string image = FolderImages + "\\" + ci.element.IntegerValue.ToString() + ".png";
                ElementId imageId = null;

                if (dataform.ByRazdel)       // выполняем обработку выбранного РАЗДЕЛА
                {
                    image = FolderImages + "\\" + doc.GetElement(ci.element).get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString() + "-" +
                        doc.GetElement(ci.element).get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString() +
                        ".png";
                }

                if (!dataform.AllRebars)
                {
                    // исходный эскиз для стержня
                    string image_old = ci.element.IntegerValue.ToString() + ".png";
                    if (dataform.ByRazdel)       // выполняем обработку выбранного РАЗДЕЛА
                    {
                        image_old = doc.GetElement(ci.element).get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString() + "-" +
                            doc.GetElement(ci.element).get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString() +
                            ".png";
                    }

                    // определяем число стержней во всем проекте с такой картинкой   

                    int eid_prj = name_skeths.Count(x => x == image_old);
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //if (writer != null) writer.WriteLine("*Время выполнения процедуры 1 этапа");
                    //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                    //Start = DateTime.Now;

                    if (eid_prj == 0)   // не используется уже. 
                    {
                        // проверим - возможно такой файл уже загружен
                        // тогда используем его
                        foreach (Element el in all_images)
                        {
                            ImageType it = el as ImageType;
                            if (it != null)
                            {
                                try { if (it.Path == image) imageId = it.Id; }
                                catch { continue; }
                            }
                        }

                        goto create_sketch;
                    }


                    // пытаемся получить эскиз для стержня                       
                    imageId = bar.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId();   // записать Id картинки 

                    if (imageId.IntegerValue > 0)  // какая-то картинка существует  
                    {
                        // определяем число стержней во всем проекте с такой картинкой
                        eid_prj = id_sketchs.Count(x => x == imageId.IntegerValue);
                        // определяем число стержней в текущей выборке с такой картинкой                                
                        int eid_sel = id_sketch.Count(x => x == imageId.IntegerValue);
                        // все стержни проекта в текущей выборке - данное имя файла можно использовать
                        if (eid_prj == eid_sel) goto create_sketch;
                    }
                    imageId = null;
                    // получим новое имя файла для данной группы                                 
                    int i = 0;
                    string new_path_name = image;
                    do
                    {
                        bool find_image = false;
                        foreach (Element el in all_images)
                        {
                            ImageType it = el as ImageType;
                            try
                            {
                                if (it.Path == new_path_name) { find_image = true; break; }
                            }
                            catch { continue; }
                        }
                        if (find_image)
                        {
                            new_path_name = image.Replace(".png", i.ToString() + ".png");
                            i++;
                        }
                        else
                        {
                            // это имя файла будем использовать
                            image = new_path_name;
                            break;
                        }
                    } while (true);

                }
            create_sketch:



                CreateImage(ci, image, imageId);
              
            }
        }

        /// <summary>
        /// Создание эскиза
        /// </summary>
        /// <param name="ci">Текущий стержень</param>
        /// <param name="image">Имя эскиза</param>
        /// <param name="imageId">Id эскиза</param>      

        void CreateImage(CodeImage ci,string image, ElementId imageId)
        {            
            //DateTime Start = DateTime.Now;
            //DateTime Stoped = DateTime.Now;
            //TimeSpan Elapsed = TimeSpan.Zero;
            //Start = DateTime.Now;

            if (Eskis.CreateBitmapRebar(doc.GetElement(ci.element), template, dataform, image, null))  // здесь будет создан файл image
            {
                //Stoped = DateTime.Now;
                //Elapsed = Stoped.Subtract(Start);
                //if (writer != null) writer.WriteLine("*Эскиз создан");
                //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));

                try
                {
                    //Start = DateTime.Now;

                    if (imageId != null) images_Id_delete.Add(imageId); //  doc.Delete(imageId);

                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //if (writer != null) writer.WriteLine("*Время удаления старого эскиза");
                    //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                    //Start = DateTime.Now;

                    // создавать нужно всегда для обновления картинки
                    ImageTypeOptions itp = new ImageTypeOptions(image, true, ImageTypeSource.Import);
                    imageId = ImageType.Create(doc, itp).Id;
                    ci.image = imageId;

                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //if (writer != null) writer.WriteLine("*Время создания чистого эскиза");
                    //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                }
                catch
                {
                    return;
                }
            }
        }
    }
    ///// <summary>
    ///// Implements the interface IFailuresPreprocessor
    ///// </summary>
    //public class FailurePreproccessor : IFailuresPreprocessor
    //{
    //    public bool status = true;
    //    /// <summary>
    //    /// This method is called when there have been failures found at the end of a transaction and Revit is about to start processing them. 
    //    /// </summary>
    //    /// <param name="failuresAccessor">The Interface class that provides access to the failure information. </param>
    //    /// <returns></returns>
    //    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    //    {
    //        IList<FailureMessageAccessor> fmas = failuresAccessor.GetFailureMessages();
    //        if (fmas.Count == 0)
    //        {
    //            return FailureProcessingResult.Continue;
    //        }
    //        else
    //        {
    //            status = false;
    //        }

    //        //failuresAccessor.DeleteAllWarnings();


    //        //foreach (FailureMessageAccessor fma in fmas)
    //        //{
    //        //    FailureDefinitionId id = fma.GetFailureDefinitionId();

    //        //    string st = fma.GetDescriptionText();

    //        //    //if (id == Command.m_idWarning)
    //        //    //{
    //        //    //    failuresAccessor.DeleteWarning(fma);
    //        //    //}
    //        //}

    //        //    failuresAccessor.RollBackPendingTransaction();
    //            return FailureProcessingResult.ProceedWithRollBack; // .ProceedWithCommit;            
    //    }
    //}
    ///// <summary>
    ///// A failure preprocessor to hide the warning about duplicate types being pasted.
    ///// </summary>
    //class HideNewTypeAssembly : IFailuresPreprocessor
    //{
    //    #region IFailuresPreprocessor Members

    //    /// <summary>
    //    /// Implementation of the IFailuresPreprocessor.
    //    /// </summary>
    //    /// <param name="failuresAccessor"></param>
    //    /// <returns></returns>
    //    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    //    {
    //        failuresAccessor.DeleteAllWarnings();
    //        //foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
    //        //{
    //        //    // Delete any "Can't paste duplicate types.  Only non duplicate types will be pasted." warnings
    //        //    //if (failure.GetFailureDefinitionId() ==  BuiltInFailures.CopyPasteFailures.CannotCopyDuplicates)
    //        //    if (failure.GetFailureDefinitionId() == BuiltInFailures.AssemblyFailures.AssemblyNewTypeWarn)
    //        //    {
    //        //    failuresAccessor.DeleteWarning(failure);
    //        //    }


    //        //}

    //        // Handle any other errors interactively
    //        return FailureProcessingResult.Continue;
    //    }

    //    #endregion
    //}

    public enum ConsoleColor
    {
        Black = 0,
        Blue = 1,
        Green = 2,
        Cyan = 3,
        Red = 4,
        Magenta = 5,
        Brown = 6,
        Yellow = 7,
        White = 8
    };
       
    ///// <summary>
    ///// Режим работы
    ///// </summary>
    //public enum Mode
    //{
    //    /// <summary>
    //    /// Все стержни
    //    /// </summary>
    //    All,
    //    /// <summary>
    //    /// Отдельный стержень
    //    /// </summary>
    //    Single
    //}
    /// <summary>
    /// Признак шаблона  
    /// </summary>
    public enum Template
    {
        /// <summary>
        /// Русский
        /// </summary>
        Rus,
        /// <summary>
        /// Прочий
        /// </summary>
        Other
    }

    /// <summary>
    /// Признак наклона надписи  
    /// </summary>
    public enum InclineText
    {
        /// <summary>
        /// Горизонтально
        /// </summary>
        Horiz,
        /// <summary>
        /// Вертикально
        /// </summary>
        Vertic,
        /// <summary>
        /// Под уголом
        /// </summary>
        Incline,
        /// <summary>
        /// Радиус
        /// </summary>
        Radius

    }
    /// <summary>
    /// Плоские линии для чертежей
    /// </summary>
    public class Line2D
    {
        /// <summary>
        /// Точки линии
        /// </summary>
        public PointF p1F, p2F;
        /// <summary>
        /// Точки линии
        /// </summary>
        public XYZ p1, p2;
        /// <summary>
        /// Линия 2D - Z=0;
        /// </summary>        
        public Line line
        {
            get 
            {
                if (p1.DistanceTo(p2) < 0.001) return null;   // линия слишком короткая
                return Line.CreateBound(p1, p2); 
            }
        }

        /// <summary>
        /// Получить плоскую линию для чертежа
        /// </summary>
        /// <param name="p1">Начальная точка</param>
        /// <param name="p2">Конечная точка</param>         
        /// <returns>Плоская линия Z=0</returns> 
        public Line2D(PointF p1, PointF p2)
        {
            this.p1F = p1;
            this.p2F = p2;
            this.p1 = new XYZ(p1.X, p1.Y, 0);
            this.p2 = new XYZ(p2.X, p2.Y, 0);
        }
        
    }

    ///// <summary>
    ///// Данные диалога
    ///// </summary>
    //public class DataThread
    //{
    //    public Document doc;
    //    /// <summary>
    //    /// Сортированный список стержней для создания эскизов
    //    /// </summary>
    //    public SortedList<string, CodeImage> sortedImages = new SortedList<string, CodeImage>();
    //    public List<Element> all_rebar = new List<Element>();
    //    public List<Element> all_rebars = new List<Element>();
    //    public DataForm dataform = new DataForm();
    //    public Template template = Template.Other;
    //    public List<Element> all_images = new List<Element>();        
    //}
        /// <summary>
        /// Данные диалога
        /// </summary>
        public class DataForm
    {
        /// <summary>
        /// Текущий цвет линии и шрифта
        /// </summary>
        public ConsoleColor color = ConsoleColor.Black;
        /// <summary>
        /// Индекс выбора цвета линии и шрифта
        /// </summary>
        public int index_color = 0;
        /// <summary>
        /// Размер внещней границы
        /// </summary>
        public int border = 10;

        /// <summary>
        /// Единицы проекта
        /// </summary>
        public Units units;
        /// <summary>
        /// Разделы арматуры
        /// </summary>
        public List<string> Razdels=new List<string>();
        /// <summary>
        ///  Выбранный Раздел
        /// </summary>
        public string SelectRazdel = "";
        /// <summary>
        ///  Выбранные Разделы
        /// </summary>
        public List<string> SelectRazdels = new List<string>();
        /// <summary>
        ///  Признак цвета подложки
        /// </summary>
        public bool BackGroundColor = false;
        /// <summary>
        ///  Признак выбора всех разделов
        /// </summary>
        public bool IsAllRazdel = false;
        /// <summary>
        /// Доступность Разделов для диалога
        /// </summary>
        public bool IsRazdel
        {
            get
            { 
                return (Razdels.Count>0) ? true : false;
            }
        }
        /// <summary>
        /// Размеры по осям
        /// </summary>
        public bool ByAxis = false;
        /// <summary>
        /// Признак папки
        /// </summary>
        public bool IspathFolder = false;
        /// <summary>
        /// Папка для эскизов
        /// </summary>
        public string pathFolder = "";
        /// <summary>
        /// Показать длину крюков
        /// </summary>
        public bool HooksLength=false;
        /// <summary>
        /// Использовать все стержни модели
        /// </summary>
        public bool AllRebars=true;
        /// <summary>
        /// Показать радиус загиба
        /// </summary>
        public bool BendingRadius=false;
        /// <summary>
        /// Показать размер угла для свободной формы
        /// </summary>
        public bool Angle = false;
        /// <summary>
        /// Обновить ручные исправления
        /// </summary>
        public bool UpdateSingleRebar=false;
        /// <summary>
        /// Получить эскизы по виду
        /// </summary>
        public bool ByView = false;
        /// <summary>
        /// Получить эскизы по виду
        /// </summary>
        public bool EnabledByView = true;
        /// <summary>
        /// Получить эскизы по разделу
        /// </summary>
        public bool ByRazdel = false;
        /// <summary>
        /// Получить эскизы по выбору
        /// </summary>
        public bool EnabledBySelect = true;
        /// <summary>
        /// Признак удаления ненужных эскизов
        /// </summary>
        public bool IsDeleteSketch = false;
        ///// <summary>
        ///// Список шрифтов проекта
        ///// </summary>
        //public List<TextNoteType> Fonts = new List<TextNoteType>();
        ///// <summary>
        ///// Шрифт по умолчанию
        ///// </summary>
        //public int Font_default = 0;
        /// <summary>
        /// Имя шрифта в проекте по умолчанию
        /// </summary>
        public string Font_default_name = "Mipgost";
        /// <summary>
        /// Размер шрифта в проекте по умолчанию
        /// </summary>
        public float Font_size = 48;
       
        /// <summary>
        /// Смещение шрифта от линии
        /// </summary>
        public int Font_shift = 5;
        /// <summary>
        /// Максимальная длина стержня
        /// </summary>
        public double Max_Lenght = 39.37008;
        /// <summary>
        /// Коэффициент нахлеста
        /// </summary>
        public int coef_diam = 50;
         
        /// <summary>
        /// Признак режима максимальной длины
        /// </summary>
        public bool mode_shape=true;
        /// <summary>
        /// Признак простановки размеров
        /// </summary>
        public bool Is_dim_lines = false;
        /// <summary>
        /// Смещение текста сверху
        /// </summary>
        public int shiftUp=0;
        /// <summary>
        /// Смещение текста сверху (текст вертикально)
        /// </summary>
        public int shiftUpVertical = 0;
        /// <summary>
        /// Смещение текста снизу
        /// </summary>
        public int shiftDown = 0;
        /// <summary>
        /// Смещение текста слева
        /// </summary>
        public int shiftLeft = 0;
        /// <summary>
        /// Смещение текста слева (положение текста - по вертикали)
        /// </summary>
        public int shiftLeftVertical = 0;

    }

    /// <summary>
    /// Фильтр выбора для элементов армирования
    /// </summary>
    public class TargetElementSelectionFilter : ISelectionFilter
    {

        public bool AllowElement(Element element)
        {

            if (element.GetType().Name.Equals("Rebar")) //  || element.GetType().Name.Equals("RebarInSystem"))
            {
                RebarShape rs = null;
                Rebar rebarOne = element as Rebar;
                RebarInSystem rebarIn = element as RebarInSystem;
                // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
                // получить данные по форме стержня
                if (rebarOne != null) rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                if (rebarIn != null) rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;

                RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
                RebarShapeDefinitionByArc rarc = rsd as RebarShapeDefinitionByArc;
                RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;

                if (rsds == null && rarc == null) return false;   // формы не определяются
                // if (rarc != null) return false;                   // арочную форму пропускаем      
                return true;
            }


            return false;

        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
    }

    /// <summary>
    /// Маркировка арматуры
    /// </summary>
    class MarkR : IEquatable<MarkR>
    {
        public double Length;
        public string bar, forma, segments;


        public MarkR(string segments, string bar, string forma, double Length)
        {
            this.bar = bar;
            this.Length = Length;
            this.forma = forma;
            this.segments = segments;
        }

        public bool Equals(MarkR other)
        {

            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            // return Length.Equals(other.Length) && bar.Equals(other.bar) && forma.Equals(other.forma) && segments.Equals(other.segments);
            return bar.Equals(other.bar) && forma.Equals(other.forma) && segments.Equals(other.segments);
        }


        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode()
        {

            ////Get hash code for the Name field if it is not null.
            //int hashProductGost = Length == null ? 0 : Length.GetHashCode();

            int hashProductClass = forma == null ? 0 : forma.GetHashCode();

            int hashProductBar = bar == null ? 0 : bar.GetHashCode();

            int hashProductSegment = segments == null ? 0 : segments.GetHashCode();

            //Calculate the hash code for the product.
            // return hashProductGost ^ hashProductClass ^ hashProductBar ^ hashProductSegment;
            return hashProductClass ^ hashProductBar ^ hashProductSegment;
        }

    }


    
    /// <summary>
    /// Guid участков арматурных стержней
    /// </summary>
    public class LegGuid
    {
        public Guid A, B, C, D, E, F, G ,H, J, h1,h2;


        public LegGuid()
        {
            
                A = new Guid("b5ef18b4-453e-49bd-b26c-dfb3bd3ca79c");                
                h1 = new Guid("a4d54aaa-6132-4af4-84ce-8638096c9941");  // Крюк прямого стержня                 
                h2 = new Guid("bb67f21c-3436-4e0e-ae86-12a7b20567c9");  // Крюк прямого стержня
                B = new Guid("bef64550-0992-4b59-a616-1acaa2e24065");
                C = new Guid("4d1d1719-6bd9-4357-9378-a1d77871e0fd");
                D = new Guid("93ddaf87-08af-4bb9-b48f-87994feec729");
                F = new Guid("99509457-fdd5-40cf-a4cd-522b20acdd64");
                E = new Guid("ba55593e-d70c-410c-ba60-6e935aa1c169");
                G = new Guid("64aa0034-0c4d-400a-b048-d40e47637914");
                H = new Guid("098420cf-d8fe-4c71-939b-fc441b9ffcae");
                J = new Guid("750b510b-4034-403d-afa7-436272cffa36");
        }
    }
    /// <summary>
    /// Получение парметров и констант
    /// </summary>
    class SketchTools
    {
        public static void TestParameter()
        {

            return;
        }

        /// <summary>
        /// Получить луч по двум точкам
        /// </summary>
        /// <param name="line">Отрезок прямой</param>               
        /// <returns>Луч</returns>
        public static Pen GetPen(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return new Pen(System.Drawing.Color.Black, 5);
                case ConsoleColor.Blue:
                    return new Pen(System.Drawing.Color.Blue, 5);
                case ConsoleColor.Brown:
                    return new Pen(System.Drawing.Color.Brown, 5);
                case ConsoleColor.Cyan:
                    return new Pen(System.Drawing.Color.Cyan, 5);
                case ConsoleColor.Green:
                    return new Pen(System.Drawing.Color.Green, 5);
                case ConsoleColor.Magenta:
                    return new Pen(System.Drawing.Color.Magenta, 5);
                case ConsoleColor.Red:
                    return new Pen(System.Drawing.Color.Red, 5);
                case ConsoleColor.White:
                    return new Pen(System.Drawing.Color.White, 5);
                case ConsoleColor.Yellow:
                    return new Pen(System.Drawing.Color.Yellow, 5);
                default:
                    return new Pen(System.Drawing.Color.Black, 5);
            }
        }

        /// <summary>
        /// Получить Brushes
        /// </summary>
        /// <param name="line">Отрезок прямой</param>               
        /// <returns>Луч</returns>
        public static Brush GetBrushes(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.Black:
                    return Brushes.Black;
                case ConsoleColor.Blue:
                    return Brushes.Blue;
                case ConsoleColor.Brown:
                    return Brushes.Brown;
                case ConsoleColor.Cyan:
                    return Brushes.Cyan;
                case ConsoleColor.Green:
                    return Brushes.Green;
                case ConsoleColor.Magenta:
                    return Brushes.Magenta;
                case ConsoleColor.Red:
                    return Brushes.Red;
                case ConsoleColor.White:
                    return Brushes.White;
                case ConsoleColor.Yellow:
                    return Brushes.Yellow;
                default:
                    return Brushes.Black;
            }
        }

        /// <summary>
        /// Получить луч по двум точкам
        /// </summary>
        /// <param name="line">Отрезок прямой</param>               
        /// <returns>Луч</returns>

        public static Line GetRay(Line line)
        {
            XYZ p0 = line.GetEndPoint(0);
            XYZ p1 = line.GetEndPoint(1);
            XYZ pr1 = p0 + (p0 - p1).Normalize() * 10000;
            XYZ pr2 = p0 + (p1 - p0).Normalize() * 10000;

            Line l1 = Line.CreateBound(pr1, pr2);
            return l1;

        }
        /// <summary>
        /// Получить точку пересечения линий
        /// </summary>
        /// <param name="l1">Линия (луч) 1 </param>
        /// <param name="l2">Линия (луч) 2 </param>        
        /// <returns>Точка пересечения</returns>
        public static XYZ CrossRay(Line l1, Line l2)
        {
            IntersectionResultArray ir;
            l1.Intersect(l2, out ir);
            try
            {
                IEnumerator i = ir.GetEnumerator();
                i.Reset();
                int ii = 0;

                while (i.MoveNext())
                {
                    // точка пересечения
                    XYZ ip = ir.get_Item(ii).XYZPoint;
                    if (ip != null) return ip;
                    ii++;

                }
            }
            catch
            {
                return null;
            }

            return null;
        }
        /// <summary>
        /// Получить максимальную и минимальную длину сегмента стержня для указанного параметра
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="param">Параметр</param>
        /// <param name="min">Минимальная длина сегмента</param>
        /// <returns>Максимальная длина сегмента</returns>
        public static double GetMaxMinValue(Rebar rebar, Parameter param, out double min)
        {
            min = 0;  // минимальная длина сегмента
            if (param.HasValue) return rebar.get_Parameter(param.Definition).AsDouble();
            int segments = rebar.NumberOfBarPositions;
            if (segments <= 0) return 0;
            DoubleParameterValue minV = rebar.GetParameterValueAtIndex(param.Id, segments - 1) as DoubleParameterValue;
            DoubleParameterValue maxV = rebar.GetParameterValueAtIndex(param.Id, 0) as DoubleParameterValue;
            if (maxV.Value > minV.Value) { min = minV.Value; return maxV.Value; }
            else { min = maxV.Value; return minV.Value; }

        }

        /// <summary>
        /// Получить длину крюка стержня - внешний габарит крюка
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        /// <returns>Длина крюка</returns>
        public static double GetLengthHook(Rebar rebar, ElementId hookId)
        {
            if (hookId.IntegerValue < 0) return 0;
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            RebarBendData rbd = rebar.GetBendData();

            if (hookId.Equals(hook_start))
            {
                return rbd.HookLength0 + rbd.HookBendRadius + rbd.BarModelDiameter;      
            }
            if (hookId.Equals(hook_end))
            {
                return rbd.HookLength1 + rbd.HookBendRadius + rbd.BarModelDiameter;
            }           
            return 0;
        }

        /// <summary>
        /// Получить полную длину крюка стержня НАЧАЛО СТЕРЖНЯ (прямой участок + загиб)
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="orient_hook">Ориентация крюка изменена</param>
        /// <returns>Длина крюка</returns>
        public static double GetFullLengthHook(Rebar rebar, bool orient_hook=false)
        {
            double bend_diametr = rebar.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER).AsDouble();
            double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            if (!orient_hook)
            {
                ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                if (hook_start.IntegerValue < 0) return 0;
                IList<Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
                return all_segments[0].Length + all_segments[1].Length - bend_diametr/2 - diam;
            }
            else
            {
                ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                if (hook_start.IntegerValue < 0) return 0;
                IList<Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
                int num_segments = all_segments.Count;
                return all_segments[num_segments-1].Length + all_segments[num_segments - 2].Length - bend_diametr / 2 - diam;
            }
        }

        /// <summary>
        /// Получить полную длину крюка стержня НАЧАЛО СТЕРЖНЯ (прямой участок + загиб)
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>         
        /// <returns>Длина крюка</returns>
        public static double GetFullLengthHookStart(Rebar rebar)
        {
            double bend_diametr = rebar.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER).AsDouble();
            double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            if (hook_start.IntegerValue < 0) return 0;
            IList<Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            return all_segments[0].Length + all_segments[1].Length - bend_diametr / 2 - diam;            
        }
        /// <summary>
        /// Получить полную длину крюка стержня КОНЕЦ СТЕРЖНЯ (прямой участок + загиб)
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>         
        /// <returns>Длина крюка</returns>
        public static double GetFullLengthHookEnd(Rebar rebar)
        {
            double bend_diametr = rebar.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER).AsDouble();
            double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            if (hook_end.IntegerValue < 0) return 0;
            IList<Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            int num_segments = all_segments.Count;
            return all_segments[num_segments - 1].Length + all_segments[num_segments - 2].Length - bend_diametr / 2 - diam;
        }


        /// <summary>
        /// Получить полную длину первого участка стержня - прямой участок + загиб
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="orient_hook">Ориентация крюка изменена</param>
        /// <returns>Длина крюка</returns>
        public static double GetFullFirstSegment(Rebar rebar)
        {
            double bend_diametr = rebar.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER).AsDouble();
            double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

            IList <Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            if (hook_start.IntegerValue < 0)
            {
                return all_segments[0].Length + all_segments[1].Length - bend_diametr/2 -diam ;
            }
            return all_segments[0].Length + all_segments[1].Length + all_segments[2].Length + all_segments[3].Length - bend_diametr / 2 - diam;
        }

        /// <summary>
        /// Получить полную длину последнего участка стержня - прямой участок + загиб
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="orient_hook">Ориентация крюка изменена</param>
        /// <returns>Длина крюка</returns>
        public static double GetFullLastSegment(Rebar rebar)
        {
            double bend_diametr = rebar.get_Parameter(BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER).AsDouble();
            double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

            IList<Curve> all_segments = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            int all = all_segments.Count;
            if (hook_end.IntegerValue < 0)
            {
                return all_segments[all-1].Length + all_segments[all-2].Length - bend_diametr / 2 - diam;
            }
            return all_segments[all-1].Length + all_segments[all-2].Length + all_segments[all-3].Length + all_segments[all-4].Length - bend_diametr / 2 - diam;
        }


        /// <summary>
        /// Получить длину крюка стержня
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        /// <returns>Длина крюка</returns>
        public static double GetLengthHook(RebarInSystem rebar, ElementId hookId)
        {
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            RebarBendData rbd = rebar.GetBendData();
            if (hookId.Equals(hook_start)) return rbd.HookLength0;
            if (hookId.Equals(hook_end)) return rbd.HookLength1;             
            return 0;
        }

        /// <summary>
        /// Минимальное значение при сравнении
        /// </summary>
        public const double Double_Epsilon = 0.01;       
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, ElementId value)
        {

            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Rebar fi, BuiltInParameter guid, ElementId value)
        {

            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, double value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }
        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, BuiltInParameter guid, string value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }

        /// <summary>
        /// Получить угол (в градусах) между прямыми отрезками
        /// </summary>
        public static double GetAngleBetweenLine2D(Line2D line1, Line2D line2)
        {
            if (line1.line == null || line2.line == null) return 0;   // линии слишком короткие
            double x1 = line1.p1.X; double y1 = line1.p1.Y;           // линии лежат на одной прямой
            double x2 = line1.p2.X; double y2 = line1.p2.Y;
            double x3 = line2.p2.X; double y3 = line2.p2.Y;
            if (CompareDouble(x1, x2) && CompareDouble(x1, x3)) return 0;
            if (CompareDouble(y1, y2) && CompareDouble(y1, y3)) return 0;
            if (CompareDouble((y2 - y1) / (y3 - y1), (x2 - x1) / (x3 - x1))) return 0; // точки на одной прямой
            Line2D line3 = new Line2D(line1.p1F, line2.p2F);
            double a = line3.line.Length;
            double b = line1.line.Length;
            double c = line2.line.Length;
            double p = (a + b + c) / 2;
            double r = Math.Sqrt(((p - a) * (p - b) * (p - c) / p));
            return  Math.Round( 180.00/Math.PI*Math.Atan(2 * r / (p - a)), 0);             
        }
        public static bool CompareVertexFloat(PointF pnt1, PointF pnt2)
        {
            return (CompareFloat(pnt1.X, pnt2.X) &&
                    CompareFloat(pnt1.Y, pnt2.Y));

        }
        public static bool CompareXYZ(Autodesk.Revit.DB.XYZ pnt1, Autodesk.Revit.DB.XYZ pnt2)
        {
            return (CompareDouble(pnt1.X, pnt2.X) &&
                    CompareDouble(pnt1.Y, pnt2.Y) &&
                    CompareDouble(pnt1.Z, pnt2.Z));

        }
        /// <summary>
        /// compare whether 2 double is equal using internal precision
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <returns>Да если A=B</returns>
        public static bool CompareFloat(float d1, float d2)
        {
            return (Math.Abs(d1 - d2) < Double_Epsilon && (Math.Abs(d2) > Double_Epsilon ? d1 / d2 > 0 : true));
        }
        /// <summary>
        /// compare whether 2 double is equal using internal precision
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <returns>Да если A=B</returns>
        public static bool CompareDouble(double d1, double d2)
        {
            return ( Math.Abs(d1 - d2) < Double_Epsilon && (Math.Abs(d2) > Double_Epsilon ? d1 / d2 > 0 : true));
        }

        /// <summary>
        /// compare whether 2 double is equal using internal precision
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <returns>Да если A>B</returns>
        public static bool CompareDoubleMore(double d1, double d2)
        {
            return ( d1 - d2 > Double_Epsilon ? true : false);
        }

        /// <summary>
        /// Получить координаты крайних точек из списка 
        /// </summary>
        /// <param name="pointDF">Список точек</param>
        /// <returns>Координаты крайних точек</returns> 
        /// 
        public static void GetExtremePoints(List<PointF> pointDF, out float minX, out float minY, out float maxX, out float maxY)
        {

            minX = maxX = pointDF[0].X;
            minY = maxY = pointDF[0].Y;

            for (int i = 0; i < pointDF.Count(); i++)
            {
                float fx = pointDF[i].X;
                float fy = pointDF[i].Y;
                minX = Math.Min(minX, fx);
                maxX = Math.Max(maxX, fx);
                minY = Math.Min(minY, fy);
                maxY = Math.Max(maxY, fy);



            }
        }


        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина стержня в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundLenghtRebar(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment.ToString();

            double precision = rrm.ApplicableTotalLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                // UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }

                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        unit = unit * precision;
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableTotalLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            // return UnitFormatUtils.Format(projectUnit, UnitType.UT_Reinforcement_Length, round_value, false, false);
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }



        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина сегмента в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundLenghtSegment(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment.ToString();

            double precision = rrm.ApplicableSegmentLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }
                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableSegmentLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }

        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина сегмента в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static double GetRoundLenghtSegmentValue(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment;

            double precision = rrm.ApplicableSegmentLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }
                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableSegmentLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            return round_value;
        }

        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина сегмента в единицах Revit</param>
        /// <returns>Округленное строковое значение и округленную длину</returns>       
        public static string GetRoundLenghtSegment(Element rebar, double length_segment, out double round_length)
        {
            // длину стержня округляем до 1 мм во всех случаях
            round_length = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return round_length.ToString();

            double precision = rrm.ApplicableSegmentLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();


                if (m_LengthUnitType == UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }

                //FormatOptions formatOption = projectUnit.GetFormatOptions(UnitType.UT_Reinforcement_Length);
                //DisplayUnitType m_LengthUnitType = formatOption.DisplayUnits;
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.DUT_METERS_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableSegmentLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            round_length = round_value;
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }

        /// <summary>
        /// Получить округленную длину сегмента. Для округления используются настройки программы
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="length_segment">Длина сегмента в единицах Revit</param>
        /// <returns>Округленное строковое значение</returns>       
        public static string GetRoundLenghtSegment2(Element rebar, double length_segment)
        {
            // длину стержня окрругляем до 1 мм во всех случаях
            length_segment = Math.Round(length_segment * 304.8, 0) / 304.8;
            // получить менеджер текущего стержня
            RebarRoundingManager rrm = null;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null) rrm = rebarOne.GetReinforcementRoundingManager();
            if (rebarIn != null) rrm = rebarIn.GetReinforcementRoundingManager();

            Document doc = rebar.Document;
            DisplayUnit du = doc.DisplayUnitSystem;
            Units projectUnit = doc.GetUnits();

            if (rrm == null) return length_segment.ToString();

            double precision = 1; // rrm.ApplicableSegmentLengthRounding;

            double unit = 0;
            if (du == DisplayUnit.IMPERIAL)
            {
                length_segment = length_segment * 12;  // все перевести в десятичные дюймы
                // величина до которой следует округлить - в единицах Revit
                UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
            }
            else
            {
                FormatOptions formatOption = projectUnit.GetFormatOptions(SpecTypeId.ReinforcementLength);
                ForgeTypeId m_LengthUnitType = formatOption.GetUnitTypeId();
                 
                if(m_LengthUnitType==UnitTypeId.MetersCentimeters ||
                   m_LengthUnitType == UnitTypeId.Decimeters ||
                   m_LengthUnitType == UnitTypeId.Meters ||
                   m_LengthUnitType == UnitTypeId.Centimeters)
                {
                    unit = 0.003280839895 * precision;
                }
                else
                {
                    //        // величина до которой следует округлить - в единицах Revit
                    UnitFormatUtils.TryParse(projectUnit, SpecTypeId.ReinforcementLength, precision.ToString(), out unit);
                }
                //switch (m_LengthUnitType)
                //{
                //    case DisplayUnitType.MetersCentimeters:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_DECIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_METERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    case DisplayUnitType.DUT_CENTIMETERS:
                //        unit = 0.003280839895 * precision;
                //        break;
                //    default:
                //        // величина до которой следует округлить - в единицах Revit
                //        UnitFormatUtils.TryParse(projectUnit, UnitType.UT_Reinforcement_Length, precision.ToString(), out unit);
                //        break;
                //}
            }

            if (unit == 0) unit = rrm.ApplicableSegmentLengthRounding;
            double round_value = 0;
            if (unit == 0) round_value = length_segment;
            else round_value = Math.Round(length_segment / unit, 0) * unit;
            if (du == DisplayUnit.IMPERIAL) round_value = round_value / 12;                       // перевести в десятичные футы
            // return UnitFormatUtils.Format(projectUnit, UnitType.UT_Reinforcement_Length, round_value, false, false);
            return UnitFormatUtils.Format(projectUnit, SpecTypeId.ReinforcementLength, round_value, false);
        }

        /// <summary>
        /// Назначить параметр
        /// </summary>
        public static bool SetParameter(Element fi, Guid guid, double value)
        {
            Parameter parameter = fi.get_Parameter(guid);
            if (parameter == null)
                return false;
            if (parameter.IsReadOnly)
                return false;
            return parameter.Set(value);
        }

        /// <summary>
        /// Получить зеркальное отображение текстовой надписи
        /// </summary>
        public static void GetMirrorText (PointF point, float angle, SizeF size_text, out PointF new_point, out float new_angle)
        {
            new_point = point;
            new_angle = angle;

            // выполним смещение на ширину надписи
            new_point = new PointF((float)(new_point.X + size_text.Width * Math.Cos(angle)), (float)(new_point.Y + size_text.Width * Math.Sin(angle)));
            // выполним смещение на высоту надписи           
            new_point = new PointF((float)(new_point.X - size_text.Height * (float)Math.Sin(angle)), (float)(new_point.Y + size_text.Height * Math.Cos(angle)));
            new_angle =  (float) Math.PI + angle;
            return;
        }


        /// <summary>
        /// Данные по участкам армирования
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>         
        /// <returns>Данные по сегментам стержня</returns>
        public static string DataBySegments(Element element)
        {
             
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = element;

            Document doc = element.Document;
            Rebar rebarOne = element as Rebar;
            RebarInSystem rebarIn = element as RebarInSystem;
            RebarShape rs = null;
            string segments = "";

            // здесь выполняем 
            if (rebarOne != null)
            {
                               
                rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
               
            }
            if (rebarIn != null)
            {
                
                // получить данные по форме стержня
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                
                 
            }
        
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            ParameterSet pset = element.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 

            if (rsds != null)
            {
                // Цикл по сегментам в данной форме rsds.NumberOfSegments
                for (int i = 0; i < rsds.NumberOfSegments; i++)
                {
                    RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент
                    IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента               

                    foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
                    {
                        // получим длину сегмента
                        RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;
                        if (l != null)
                        {

                            ElementId pid = l.GetParamId();
                            Element elem = doc.GetElement(pid);
                            foreach (Parameter pr in pset)
                            {
                                if (pr.Definition.Name == elem.Name)
                                {
                                    
                                    tor.value = element.get_Parameter(pr.Definition).AsDouble();
                                    // segments = segments + Math.Round(element.get_Parameter(pr.Definition).AsDouble(), 2) + " ;";                                   
                                    if(tor.value>0) segments = segments + tor.value_str + " ;";

                                }
                            }
                        }
                    }
                }
            }

             
             
            tor.value = element.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble();
            return segments = segments + tor.value_str_total;

        }

        /// <summary>
        /// Получить проекцию точки на плоскость, заданную тремя точками
        /// </summary>
        /// <param name="PointPlane1">Точка 1 плоскости</param>
        /// <param name="PointPlane2">Точка 2 плоскости</param>
        /// <param name="PointPlane3">Точка 3 плоскости</param>
        /// <param name="p">Проекцируемая точка</param>         
        /// <returns>Точка проекции</returns>
        public static XYZ ProjectPointOnWorkPlane(XYZ PointPlane1, XYZ PointPlane2, XYZ PointPlane3, XYZ p)
        {


            XYZ a = PointPlane1 - PointPlane2;
            XYZ b = PointPlane1 - PointPlane3;
            XYZ c = p - PointPlane1;



            XYZ normal = (a.CrossProduct(b));

            try
            {
                normal = normal.Normalize();
            }
            catch (Exception)
            {
                normal = XYZ.Zero;
            }

            XYZ retProjectedPoint = p - (normal.DotProduct(c)) * normal;
            return retProjectedPoint;

        }

    }



    /// <summary>
    /// Надписи над прямыми участками стержней
    /// </summary>
    public class TextOnRebar : IEquatable<TextOnRebar>
    {
        /// <summary>
        /// Признак отображения в диалоге
        /// </summary>
        public bool dialog = true;
        /// <summary>
        /// Множитель значения
        /// </summary>
        public double coeff = 1;
        /// <summary>
        /// Повторный размер
        /// </summary>
        public bool repeat=false;
        /// <summary>
        /// Размер надписи
        /// </summary>
        public SizeF size;
        /// <summary>
        /// Стержень для которого выполняется надпись
        /// </summary>
        public Element rebar = null;        
        /// <summary>
        /// Guid параметра
        /// </summary>
        public Guid guid;
        /// <summary>
        /// Guid параметра (проекция вертикальная)
        /// </summary>
        public Guid guidV;
        /// <summary>
        /// Guid параметра (проекция горизонтальная)
        /// </summary>
        public Guid guidH;
        /// <summary>
        /// Начальная точка сегмента
        /// </summary>
        public XYZ start;
        /// <summary>
        /// Конечная точка сегмента
        /// </summary>
        public XYZ end;
        /// <summary>
        /// Начальная точка сегмента
        /// </summary>
        public PointF startF;
        /// <summary>
        /// Конечная точка сегмента
        /// </summary>
        public PointF endF;
        /// <summary>
        /// Расстояние между точками
        /// </summary>
        public float distF
        {
            get { return (float)(Math.Sqrt(Math.Pow((startF.X - endF.X), 2) + Math.Pow((startF.Y - endF.Y), 2))); }
        }
        /// <summary>
        /// Позиция текста в общей системе координат
        /// </summary>
        public XYZ position;
        /// <summary>
        /// Позиция текста в локальной системе координат чертежа
        /// </summary>
        public PointF positionF;
        /// <summary>
        /// Позиция текста  после возможного поворота
        /// </summary>
        public PointF positionF_rotate
        {
            get
            {
                // координаты новой точки после поворота на угол "angle_rotate"
                float X_new = (float)(positionF.X * Math.Cos(angle) + positionF.Y * Math.Sin(angle));
                float Y_new = (float)(-positionF.X * Math.Sin(angle) + positionF.Y * Math.Cos(angle));
                // сдвиг на длину надписи 
                // return new PointF(X_new - size.Width / 2, Y_new);
                return new PointF(X_new, Y_new);
            }
        }
        /// <summary>
        /// Значение параметра в эскизе
        /// </summary>
        public double value_sketch = 0; 
        /// <summary>
        /// Значение параметра
        /// </summary>
        public double value = 0;
        /// Значение параметра - минимальная длина стержня
        /// </summary>
        public double value_min = 0;

        /// <summary>
        /// Значение параметра начальное
        /// </summary>
        public double value_initial = 0;

        /// <summary>
        /// Значение параметра начальное - минимальная длина стержня
        /// </summary>
        public double value_initial_min = 0;

        /// <summary>
        /// Признак отображения имени сегмента
        /// </summary>
        public bool IsNameShow = false;

        /// <summary>
        /// Округленное строковое значение параметра
        /// </summary>
        public string value_str
        {
            get
            {
                if (IsNameShow) return name;
                string v_min = "";
                string v = SketchTools.GetRoundLenghtSegment(rebar, value);
                if (value_min > 0)
                {
                    v_min = SketchTools.GetRoundLenghtSegment(rebar, value_min);
                    v = v_min + "-" + v;
                }
                if (v.Length<2) return v;
                if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                return v;
            }
        }

        /// <summary>
        /// Округленное строковое значение параметра (для всей длины стержня)
        /// </summary>
        public string value_str_total
        {
            get
            {
                string v = SketchTools.GetRoundLenghtRebar(rebar, value);
                if (v.Length < 2) return v;
                if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                return v;
            }
        }

        /// <summary>
        /// Имя параметра
        /// </summary>
        public string name = "";        
        /// <summary>
        /// Значение параметра
        /// </summary>
        public double valueV = 0;
        /// <summary>
        /// Значение параметра
        /// </summary>
        public string valueV_str
        {
            get
            {
                string v=SketchTools.GetRoundLenghtSegment(rebar, valueV);
                if (v.Length < 2) return v;
                if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                return v;
            }
        }

        /// <summary>
        /// Имя параметра
        /// </summary>
        public string nameV = "";
        /// <summary>
        /// Значение параметра
        /// </summary>
        public double valueH = 0;
        /// <summary>
        /// Значение параметра
        /// </summary>
        public string valueH_str
        {
            get
            {
                string v = SketchTools.GetRoundLenghtSegment(rebar, valueH);
                if (v.Length < 2) return v;
                if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                return v;
            }
        }
        /// <summary>
        /// Имя параметра
        /// </summary>
        public string nameH = "";
        /// <summary>
        /// Признак дуги (арки)
        /// </summary>
        public bool arc = false;
        /// <summary>
        /// Угол наклона надписи в градусах
        /// </summary>
        public float angle_grad
        {
            get { return (float)(180 / Math.PI * angle); }
        }
        /// <summary>
        /// Угол наклона надписи
        /// </summary>
        public float angle = 0;
        /// <summary>
        /// Признак наклона надписи
        /// </summary>
        public InclineText incline = InclineText.Horiz;       // по умолчанию
        /// <summary>
        /// Получить координаты с учетом масштаба
        /// </summary>
        public void UsingScale(float scale)
        {
            positionF = new PointF(positionF.X * scale, positionF.Y * scale);
            startF = new PointF(startF.X * scale, startF.Y * scale);
            endF = new PointF(endF.X * scale, endF.Y * scale);
        }

        public bool Equals(TextOnRebar other)
        {

            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return name.Equals(other.name) || nameV.Equals(other.nameV) || nameH.Equals(other.nameH);
        }


        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            int hashProductGost = name == null ? 0 : name.GetHashCode();

            int hashProductClass = nameV == null ? 0 : nameV.GetHashCode();

            int hashProductBar = nameH == null ? 0 : nameH.GetHashCode();

            //Calculate the hash code for the product.
            return hashProductGost ^ hashProductClass ^ hashProductBar;
        }


    }

    /// <summary>
    /// Построение чертежа 
    /// </summary>
    public class BuildImage
    {
        public BuildImage()
        {
        }
        public BuildImage(Element rebar, Template template)
        {
            this.rebar = rebar; this.template = template;
            DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
            GetInfoAboutHooks();                           // получить данные по крюкам
            // запишем реальные данные по гнутым участкам
            // точки вставки возможно будут изменены при масштабировании стержня
            lg_arc.Clear();
            lg_angles.Clear();
            for (int i = curve_start; i < curve_end; i++)
            {
                Curve c = ilc[i];

                // гнутые участки записываем для указания радиуса или угла загиба 
                if (c.GetType().Name == "Arc") lg_arc.Add(GetArcSegment(ilc, rebar, i));                  // для участка типа дуга
                if (c.GetType().Name == "Arc") lg_angles.Add(GetArcAngle(c as Arc));               // для участка типа дуга
            }
            DataBySegments();                              // Формирование данных по участкам армирования
        }


        /// <summary>
        /// Выполнение построения чертежа 
        /// </summary>
        public void UpdateImage()
        {
            DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
            GetInfoAboutHooks();                           // получить данные по крюкам
            // масштабируем стержень: по крюккам или короткому сегменту
            // фактически создаем новый стержень с искаженными длинами сегментов 
            ChangeParametersRebar();                       // изменить параметры стержня
            if (status)
            {
                IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов
                if (IsRebarCorrect)
                {
                    GetPointsAndLinesForDrawing();             // Инициализация данных для сегментов, радиусов загиба 
                    DrawPicture();
                }

            }
        }
        #region Параметры        
        /// <summary>
        /// <summary>
        /// Цвет линии и текста
        /// </summary>
        public ConsoleColor color = ConsoleColor.Black;
        /// <summary>
        /// Статус создания стержня
        /// </summary>
        public bool status = true;
        /// <summary>
        /// Текущая транзакция
        /// </summary>
        public Transaction transaction;
        /// <summary>
        /// Текущий шаблон проекта
        /// </summary>
        public Template template;
        /// <summary>
        /// Чертеж
        /// </summary>
        public Graphics graphic;
        /// <summary>
        /// Элемент арматурного стержня
        /// </summary>
        public Element rebar;
        /// <summary>
        /// Данные по арматурному стержню
        /// </summary>
        public RebarBendData rbd;
        /// <summary>
        /// Размер рисунка по оси Х
        /// </summary>
        public int sizeX = 1000;              // по умолчанию
        /// <summary>
        /// Размер рисунка по оси Y
        /// </summary>
        public int sizeY = 300;               // по умолчанию
        /// <summary>
        /// Размер шрифта
        /// </summary>
        public float move = 90;             // по умолчанию
        /// <summary>
        /// Размер канвы
        /// </summary>
        public float canva = 10;            // по умолчанию
        /// <summary>
        /// Коэффициент перевода единиц
        /// </summary>
        const float unit = (float)0.00328;
        #endregion Параметры

        #region Инициализация массивов
        /// <summary>
        /// Параметры диалога
        /// </summary>
        public DataForm dataform=null;
        /// <summary>
        /// Видимость длины крюков
        /// </summary>
        public bool hooks_length = true;
        /// <summary>
        /// Видимость радиусов загиба
        /// </summary>
        public bool bending = false;
        /// <summary>
        /// Видимость УГЛОВ
        /// </summary>
        public bool showangle = false;
        /// <summary>
        /// Коэффициенты для крюков
        /// </summary>
        public double coef_hook = 1;
        ///// <summary>
        ///// Коэффициенты для сегментов стержня
        ///// </summary>
        //public double[] coef = { 1, 1, 1, 1, 1, 1, 1, 1 };
        /// <summary>
        /// Параметры для крюков
        /// </summary>
        public List<TextOnRebar> hooks = new List<TextOnRebar>();                                // Список параметров для крюков           
        /// <summary>
        /// Линии чертежа (только прямые)
        /// </summary>
        public List<Line2D> line2D_L = new List<Line2D>();                                       // список плоских линий для чертежа (только прямые)
        /// <summary>
        /// Линии чертежа
        /// </summary>
        public List<Line2D> line2D = new List<Line2D>();                                       // список плоских линий для чертежа
        /// <summary>
        /// Линии арматуры
        /// </summary>
        public List<PointF> pointDF = new List<PointF>();
        ///// <summary>
        ///// Список параметров для угловых значений
        ///// </summary> 
        //public List<TextOnRebar> angles = new List<TextOnRebar>();
        /// <summary>
        /// Список параметров для прямых сегментов
        /// </summary> 
        public List<TextOnRebar> lg = new List<TextOnRebar>();
        /// <summary>
        /// Текстовые надписи (радиусы)
        /// </summary>
        public List<TextOnArc> lg_arc_sorted = new List<TextOnArc>();
        /// <summary>
        /// Текстовые надписи для указания радиуса
        /// </summary>
        public List<TextOnArc> lg_arc = new List<TextOnArc>();
        /// <summary>
        /// Текстовые надписи для угла загиба
        /// </summary>
        public List<TextOnArc> lg_angles = new List<TextOnArc>();
        ///// <summary>
        ///// Надписи над отрезками
        ///// </summary>
        //public List<TextOnRebar> Llg = new List<TextOnRebar>();
        /// <summary>
        /// Cписок сегментов для стержня проекта
        /// </summary>
        IList<Curve> ilc = new List<Curve>();
        #endregion Инициализация массивов

        /// <summary>
        /// Файл рисунка
        /// </summary>
        public Bitmap flag;
        //{
        //    get
        //    {
        //        return new Bitmap(sizeX,sizeY);
        //    }           
        //}
        /// <summary>
        /// Признак правильного создания стержня
        /// </summary>
        public bool IsRebarCorrect;
        /// <summary>
        /// Направление оси Z - перпендикулярно плоскости стержня
        /// </summary>
        Vector4 zAxis = new Vector4(XYZ.Zero);
        /// <summary>
        /// Максимум модели по оси Х
        /// </summary>
        public float maxX = 1;
        /// <summary>
        /// Максимум модели по оси Y
        /// </summary>
        public float maxY = 1;
        /// <summary>
        /// Минимум модели по оси Х
        /// </summary>
        public float minX = 1;
        /// <summary>
        /// Минимум модели по оси Y
        /// </summary>
        public float minY = 1;
        /// <summary>
        /// Коэффициент масштаба
        /// </summary>
        float scale
        {
            get
            {
                float scaleX = (float)((sizeX - 2 * canva) / maxX);
                float scaleY = (float)(sizeY - 2 * canva) / maxY;
                return Math.Min(scaleX, scaleY);
            }
        }
        /// <summary>
        /// Сдвиг по оси Х
        /// </summary>
        public float moveX;
        /// <summary>
        /// Сдвиг по оси Y
        /// </summary>
        public float moveY;
        /// <summary>
        /// Текущий документ
        /// </summary>
        Document doc;
        /// <summary>
        /// Форма стержня
        /// </summary>
        RebarShape rs = null;
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_start
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId().IntegerValue; }
        }
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_end
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId().IntegerValue; }
        }

        /// <summary>
        /// Номер начального кривой (без крюка)
        /// </summary>
        int curve_start
        {
            get
            {
                if (hook_start > 0) return 2;
                else return 0;
            }
        }

        /// <summary>
        /// Номер конечной кривой (без крюка)
        /// </summary>
        int curve_end
        {
            get
            {
                if (hook_end > 0) return ilc.Count - 2;
                else return ilc.Count;
            }
        }

        /// <summary>
        /// Коэффициент минимальной длины по крюку
        /// </summary>
        int min = 5;
        /// <summary>
        /// Коэффициент максимальной длины по крюку
        /// </summary>
        int max = 15;
        /// <summary>
        /// Начальная точка
        /// </summary>
        XYZ p_initial = null;
        /// <summary>
        /// Основное направление - по оси Х
        /// </summary>
        XYZ dir_major = null;

        /// <summary>
        /// Получить параметры для дугового сегмента
        /// </summary>
        /// <param name="curves">Линия стержня</param>
        /// <param name="rebar">Элемент стержня</param>
        /// <param name="i">Текущий номер линии</param>
        /// <param name="value">Значение параметра</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnArc GetArcSegment(IList<Curve> curves, Element rebar, int i, double value=0)
        {
             
            // получить диаметр стержня
            double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            TextOnArc toa = new TextOnArc();
            toa.rebar = rebar;
            Arc arc = curves[i] as Arc;
            // запишем координаты, направление сегмента и радиус
            // toa.position = arc.Center;                                         // запишем координаты центра дуги
            toa.position = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;
            toa.start = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;         // начальная точка сегмента
            toa.end = arc.Center;                                              // конечная точка сегмента 
           
                if (value == 0) toa.value = arc.Radius - d / 2;                    // запишем радиус дуги (по внутреннему контуру)
                else toa.value = value;
                // получить длину примыкающих прямых сегментов
                double l1, l2;
                l1 = l2 = 0;
                if ((i - 1) >= 0) l1 = curves[i - 1].Length;
                if ((i + 1) < curves.Count) l2 = curves[i + 1].Length;
                toa.nearestL = l1 + l2;
             
            return toa;
        }

        /// <summary>
        /// Получить параметры для дугового сегмента - угол загиба
        /// </summary>
        /// <param name="curves">Линия стержня</param>      
        /// <returns>Текстовая надпись</returns> 
        TextOnArc GetArcAngle(Arc arс)
        {             
            TextOnArc toa = new TextOnArc();
            // запишем координаты, направление сегмента и радиус             
            // toa.position = arс.Center;                                         // запишем координаты центра дуги 
            toa.position = (arс.GetEndPoint(0) + arс.GetEndPoint(1)) / 2;
            // по начальной и конечной точкам - определяем направление текста
            // для дуги - от середині дуги к ее центру
            toa.start = (arс.GetEndPoint(0) + arс.GetEndPoint(1)) /2 ;         // начальная точка сегмента
            toa.end = arс.Center;                                              // конечная точка сегмента
            toa.value = 180 - Math.Round(arс.Length * 360 / (2 * Math.PI * arс.Radius), 0);             
            return toa;
        }
        /// <summary>
        /// Изменить для дугового сегмента
        /// </summary>
        /// <param name="curves">Линия стержня</param>      
        /// <returns>Текстовая надпись</returns> 
        TextOnArc ChangeArcPosition(Arc arс, TextOnArc toa)
        {
            // запишем координаты, направление сегмента и радиус             
            // toa.position = arс.Center;                                         // запишем координаты центра дуги 
            toa.position = (arс.GetEndPoint(0) + arс.GetEndPoint(1)) / 2;
            // по начальной и конечной точкам - определяем направление текста
            // для дуги - от середині дуги к ее центру
            toa.start = (arс.GetEndPoint(0) + arс.GetEndPoint(1)) / 2;         // начальная точка сегмента
            toa.end = arс.Center;                                              // конечная точка сегмента
            return toa;
        }
        /// <summary>
        /// Создать чертеж
        /// </summary>
        void DrawPicture()
        {
            // готовим рисунок
            flag = new Bitmap(sizeX, sizeY);
            // flag.MakeTransparent(System.Drawing.Color.Black);

            #region Получим точки с учетом масштаба
            for (int i = 0; i < line2D_L.Count; i++)
            {
                line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X * scale, line2D_L[i].p1F.Y * scale), new PointF(line2D_L[i].p2F.X * scale, line2D_L[i].p2F.Y * scale));
            }

            for (int i = 0; i < line2D.Count; i++)
            {
                line2D[i] = new Line2D(new PointF(line2D[i].p1F.X * scale, line2D[i].p1F.Y * scale), new PointF(line2D[i].p2F.X * scale, line2D[i].p2F.Y * scale));
            }

            for (int i = 0; i < pointDF.Count; i++)
            {
                pointDF[i] = new PointF(pointDF[i].X * scale, pointDF[i].Y * scale);
            }

            foreach (TextOnRebar tor in lg) { tor.UsingScale(scale); }
            foreach (TextOnArc tor in lg_arc) { tor.UsingScale(scale); }
            foreach (TextOnRebar tor in hooks) { tor.UsingScale(scale); }
            foreach (TextOnArc tor in lg_angles) { tor.UsingScale(scale); }

            #endregion Получим точки с учетом масштаба
            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

            moveX = (sizeX - 2 * canva - maxX) / 2;
            moveY = (sizeY - 2 * canva - maxY) / 2;

            //graphic = Graphics.FromImage(flag);
            //graphic.Clear(System.Drawing.Color.White);
            // отсортируем список по длине примыкающих прямых сегментов            
            lg_arc_sorted = lg_arc.OrderByDescending(x => x.nearestL).ToList();
        }
        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        void GetPointsAndLinesForDrawing()
        {
            // определяем крюки формы
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            float dX = 0;           // смещение центра рисунка
            float dY = 0;
            // приведем пространственную систему координат стержня в плоскую систему
            // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
            // начало системы координат принимаем в произвольной точке стержня 

            Vector4 origin = new Vector4(p_initial);
            // направление оси Х          
            Vector4 xAxis = new Vector4(dir_major);
            xAxis.Normalize();
            // направление оси Y 
            Vector4 yAxis = new Vector4(XYZ.Zero);
            yAxis = Vector4.CrossProduct(xAxis, zAxis);
            yAxis.Normalize();

            Matrix4 MatrixMain = new Matrix4(xAxis, yAxis, zAxis, origin);
            // после выполнения инверсии в TRANSFORM можем подставлять ГЛОБАЛЬНЫЕ КООРДИНАТЫ и получать ЛОКАЛЬНЫЕ
            MatrixMain = MatrixMain.Inverse();
            pointDF.Clear();
            line2D.Clear();
            line2D_L.Clear();
            // выполним расчет точек для чертежа линий арматуры
            foreach (Curve c in ilc)
            {
                IList<XYZ> tp = c.Tessellate();
                foreach (XYZ p in tp)
                {
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p));                      // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    pointDF.Add(p_new1F);                                                        // получить точку для картинки                     
                }

                tp = c.Tessellate();
                // получим линии чертежа арматуры
                for (int i = 0; i < tp.Count - 1; i++)
                {
                    XYZ p1 = tp[i];
                    XYZ p2 = tp[i + 1];
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
                    PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
                    Line2D line = new Line2D(p_new1F, p_new2F);
                    line2D.Add(line);                                                            // добавить линию к списку

                }                

                //if (c.GetType().Name == "Arc")
                //{
                //    Arc a = c as Arc;
                //    XYZ dir1 = (a.Center - a.GetEndPoint(0));
                //    XYZ dir2 = (a.Center - a.GetEndPoint(1));
                //    XYZ dir = a.Center + (dir1 + dir2);                     
                //    TextOnRebar t = new TextOnRebar();
                //    t.value = 180 - Math.Round(a.Length*360/(2*Math.PI*a.Radius),0);                    
                //    Vector4 p_new1 = MatrixMain.Transform(new Vector4(a.Center));                        // получить точку в локальной системе координат
                //    Vector4 dirF = MatrixMain.Transform(new Vector4(dir));                               // получить точку в локальной системе координат
                //    t.position = a.Center;
                //    t.positionF = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                //    t.startF = new System.Drawing.PointF(dirF.X / unit, dirF.Y / unit);                  // вектор напдписи
                //    t.arc = true;
                //    angles.Add(t);
                //    continue;                                                                            // для участка типа дуга
                //}

                              

                tp = c.Tessellate();
                // получим линии чертежа арматуры
                for (int i = 0; i < tp.Count - 1; i++)
                {
                    XYZ p1 = tp[i];
                    XYZ p2 = tp[i + 1];
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
                    PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
                    Line2D line = new Line2D(p_new1F, p_new2F);
                    line2D_L.Add(line);                                                            // добавить линию к списку

                }

            }

            //if (hook_start.IntegerValue > 0 && angles.Count > 0) angles.RemoveAt(0);
            //if (hook_end.IntegerValue > 0 && angles.Count > 0) angles.RemoveAt(angles.Count - 1);
                      

            pointDF = pointDF.ToList();

            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);
            // все точки должны быть в 1 четверти
            if (minX < 0)
                for (int i = 0; i < pointDF.Count(); i++)
                {
                    pointDF[i] = new PointF(pointDF[i].X - minX, pointDF[i].Y);
                    dX = minX;
                }
            if (minY < 0)
                for (int i = 0; i < pointDF.Count(); i++)
                {
                    pointDF[i] = new PointF(pointDF[i].X, pointDF[i].Y - minY);
                    dY = minY;
                }

            if (minX < 0)
                for (int i = 0; i < line2D_L.Count(); i++)
                {
                    line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X - minX, line2D_L[i].p1F.Y), new PointF(line2D_L[i].p2F.X - minX, line2D_L[i].p2F.Y));
                }
            if (minY < 0)
                for (int i = 0; i < line2D_L.Count(); i++)
                {
                    line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X, line2D_L[i].p1F.Y - minY), new PointF(line2D_L[i].p2F.X, line2D_L[i].p2F.Y - minY));

                }

            if (minX < 0)
                for (int i = 0; i < line2D.Count(); i++)
                {
                    line2D[i] = new Line2D(new PointF(line2D[i].p1F.X - minX, line2D[i].p1F.Y), new PointF(line2D[i].p2F.X - minX, line2D[i].p2F.Y));
                }
            if (minY < 0)
                for (int i = 0; i < line2D.Count(); i++)
                {
                    line2D[i] = new Line2D(new PointF(line2D[i].p1F.X, line2D[i].p1F.Y - minY), new PointF(line2D[i].p2F.X, line2D[i].p2F.Y - minY));

                }
            //if (minX < 0)
            //    for (int i = 0; i < angles.Count(); i++)
            //    {
            //        angles[i].positionF.X = angles[i].positionF.X - minX;
            //        angles[i].startF.X = angles[i].startF.X - minX;
            //    }
            //if (minY < 0)
            //    for (int i = 0; i < angles.Count(); i++)
            //    {
            //        angles[i].positionF.Y = angles[i].positionF.Y - minY;
            //        angles[i].startF.Y = angles[i].startF.Y - minY;

            //    }


            //// выполнить расчет координат точек для вставки значений угла
            //for(int i=0; i<line2D_L.Count-1; i++)
            //{

            //    if(SketchTools.CompareVertexFloat(line2D_L[i].p2F,line2D_L[i+1].p1F))   // если есть общая вершина у двух отрезков
            //    {
            //        // если угол более 0
            //        double a = SketchTools.GetAngleBetweenLine2D(line2D_L[i], line2D_L[i + 1]);
            //        TextOnRebar t = new TextOnRebar();
            //        t.value = a;
            //        if (a > 0)
            //        {
            //            t.positionF = line2D_L[i].p2F; // точка вставки размера угла
            //            angles.Add(t);
            //        }
            //    }
            //}

            //for (int i = 0; i < line2D.Count; i++)
            //{

            //    if (SketchTools.CompareVertexFloat(line2D_L[i].p2F, line2D_L[i + 1].p1F))   // если есть общая вершина у двух отрезков
            //    {
            //        // если угол более 0
            //        double a = SketchTools.GetAngleBetweenLine2D(line2D_L[i], line2D_L[i + 1]);
            //        TextOnRebar t = new TextOnRebar();
            //        t.value = a;
            //        if (a > 0)
            //        {
            //            t.positionF = line2D_L[i].p2F; // точка вставки размера угла
            //            angles.Add(t);
            //        }
            //    }
            //}

            // выполнить расчет координат точек для вставки текста
            for (int i = 0; i < lg.Count; i++) { lg[i] = RecalculatePointPosition(MatrixMain, lg[i], dX, dY); }

            //// выполнить расчет координат точек для вставки текста
            //for (int i = 0; i < angles.Count; i++) { angles[i] = RecalculatePointPosition(MatrixMain, angles[i], dX, dY); }

            // выполнить расчет координат точек для вставки текста (дуги)
            for (int i = 0; i < lg_arc.Count; i++) { lg_arc[i] = RecalculatePointPosition(MatrixMain, lg_arc[i], dX, dY); }

            // выполнить расчет координат точек для вставки текста (дуги - угол загиба)
            for (int i = 0; i < lg_angles.Count; i++) { lg_angles[i] = RecalculatePointPosition(MatrixMain, lg_angles[i], dX, dY); }

            // выполнить расчет координат точек для вставки текста (крюки)
            for (int i = 0; i < hooks.Count; i++) { hooks[i] = RecalculatePointPosition(MatrixMain, hooks[i], dX, dY, true); }

            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

        }

        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <returns>Текстовая надпись для арки</returns> 
        TextOnArc RecalculatePointPosition(Matrix4 matrix, TextOnArc tr, float dX, float dY)
        {
            Vector4 p_new = matrix.Transform(new Vector4(tr.position));                                  // получить точку в локальной системе координат
            tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.start));                                             // получить точку в локальной системе координат
            tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);             // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.end));                                               // получить точку в локальной системе координат
            tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);               // получить точку для картинки
            tr.incline = InclineText.Incline;                                                            // получить направление надписи
            // получить угол наклона надписи в градусах
            double dAY = (double)(tr.endF.Y - tr.startF.Y);
            double dAX = (double)(tr.endF.X - tr.startF.X);
            if (dAX == 0) tr.angle = 0;
            else tr.angle = (float)Math.Atan2(dAY, dAX);
            return tr;
        }

        ///// <summary>
        ///// Получить координаты точки, значение и направление для надписи-угол
        ///// </summary>
        ///// <param name="matrix">Матрица преобразований</param>
        ///// <param name="tr">Элемент дуги</param>
        ///// <param name="dX">Сдвиг по координате Х</param>
        ///// <param name="dY">Сдвиг по координате Y</param>
        ///// <param name="hook">Признак расчета для крюков</param>
        ///// <returns>Текстовая надпись для прямого сегмента</returns> 
        //TextOnRebar CalculateAnglePosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook = false)
        //{
        //    Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
        //    tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
        //    tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
        //    tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    if (hook) return tr;
        //    // получить направление надписи
        //    if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
        //    {
        //        // дополнительный сдвиг для вертикальной надписи в сторону линии
        //        tr.incline = InclineText.Vertic; return tr;
        //    }
        //    if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
        //    tr.incline = InclineText.Incline;
        //    // получить угол наклона надписи в градусах
        //    double dAY = (double)(tr.endF.Y - tr.startF.Y);
        //    double dAX = (double)(tr.endF.X - tr.startF.X);
        //    if (dAX == 0) tr.angle = 0;
        //    else tr.angle = (float)Math.Atan2(dAY, dAX);
        //    return tr;
        //}

        ///// <summary>
        ///// Получить координату точки для размещения углового размера
        ///// </summary>
        ///// <param name="matrix">Матрица преобразований</param>
        ///// <param name="tr">Элемент углового измерения</param>
        ///// <param name="dX">Сдвиг по координате Х</param>
        ///// <param name="dY">Сдвиг по координате Y</param>        
        ///// <returns>Текстовая надпись для углового размера</returns> 
        //TextOnRebar RecalculateAnglePosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY)
        //{
        //    Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
        //    tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
        //    tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
        //    p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
        //    tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            
        //    // получить направление надписи
        //    if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
        //    {
        //        // дополнительный сдвиг для вертикальной надписи в сторону линии
        //        tr.incline = InclineText.Vertic; return tr;
        //    }
        //    if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
        //    tr.incline = InclineText.Incline;
        //    // получить угол наклона надписи в градусах
        //    double dAY = (double)(tr.endF.Y - tr.startF.Y);
        //    double dAX = (double)(tr.endF.X - tr.startF.X);
        //    if (dAX == 0) tr.angle = 0;
        //    else tr.angle = (float)Math.Atan2(dAY, dAX);
        //    return tr;
        //}


        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <param name="hook">Признак расчета для крюков</param>
        /// <returns>Текстовая надпись для прямого сегмента</returns> 
        TextOnRebar RecalculatePointPosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook = false)
        {
            if (tr.start == null) return tr;   // если точки начала и конца сегмента не установлены

            Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
            tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
            tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
            tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            if (hook) return tr;
            // получить направление надписи
            if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
            {
                // дополнительный сдвиг для вертикальной надписи в сторону линии
                tr.incline = InclineText.Vertic; return tr;
            }
            if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
            tr.incline = InclineText.Incline;
            // получить угол наклона надписи в градусах
            double dAY = (double)(tr.endF.Y - tr.startF.Y);
            double dAX = (double)(tr.endF.X - tr.startF.X);
            if (dAX == 0) tr.angle = 0;
            else tr.angle = (float)Math.Atan2(dAY, dAX);
            return tr;
        }

        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        bool InitialDataForSegments()
        {
            double max_length = 0;
            int num_segment = 0;
            int num_arc = 0;
            for (int i = curve_start; i < curve_end; i++)
            {
                Curve c = ilc[i];

                // гнутые участки записываем с сохранением радиуса загиба
                if (c.GetType().Name == "Arc")                                     // для участка типа дуга
                {
                    lg_arc[num_arc] = GetArcSegment(ilc, rebar, i, lg_arc[num_arc].value);
                    num_arc++;
                }

                // некоторые гнутые участки необходимо пропускать. Это стандартные гнутые участки, которые не имеют фактических сегментов
                if (c.GetType().Name == "Line" && lg[num_segment].arc) continue;
                if (c.GetType().Name == "Arc" && !lg[num_segment].arc) continue;

                if (max_length < c.Length && lg[num_segment].incline != InclineText.Incline)  // наклонные участки не рассматриваем как основные
                {
                    max_length = c.Length;
                    p_initial = c.GetEndPoint(0);
                    dir_major = (c.GetEndPoint(1) - p_initial).Normalize();
                }

                if (c.GetType().Name == "Arc")                       // для участка типа дуга
                {
                    lg[num_segment].arc = true;
                    Arc arc = c as Arc;
                    // запишем координаты и направление сегмента
                    lg[num_segment].position = arc.Center + arc.YDirection * arc.Radius;       // запишем координаты центра дуги 
                    //lg[num_segment].start = arc.Center;                                        // начальная точка сегмента
                    //lg[num_segment].end = arc.Center + arc.XDirection;                         // конечная точка сегмента
                    //lg[num_segment].value = 180 - Math.Round(arc.Length*360/(2*Math.PI* arc.Radius),0);                    
                    //lg[num_segment].position = arc.Center;                                       // запишем координаты центра дуги 
                    lg[num_segment].start = arc.GetEndPoint(0);                                  // начальная точка сегмента
                    lg[num_segment].end = arc.GetEndPoint(1);

                }
                else
                {
                    // запишем координаты и направление сегмента
                    lg[num_segment].position = (c.GetEndPoint(0) + c.GetEndPoint(1)) / 2;
                    lg[num_segment].start = c.GetEndPoint(0);
                    lg[num_segment].end = c.GetEndPoint(1);
                }

                num_segment++;
            }
            // проверка наличия позиций у сегментов. Если позиции нет - картинки не будет. Какое-то несоответствие. Возможно стержень 3d или самопальное семейство
            foreach (TextOnRebar tor in lg)
            {
                if (tor.position == null) return false;
            }
            if (p_initial == null || dir_major == null) return false;                        // картинки не будет
            // Llg = lg.ToList();
            return true;
        }
        /// <summary>
        /// Разделить по типам арматурных стержней: получить кривые и вектор Z
        /// </summary>
        void DivideByTypeRebar()
        {
            ilc.Clear();                    // готовим новые линии для вычерчивания стержня
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;
            // здесь выполняем 
            if (rebarOne != null)
            {                
                doc = rebarOne.Document;
                // получить данные по форме стержня
                ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                // rebarOne.get_Parameter(BuiltInParameter.REBAR_GEOMETRY_TYPE)                
                zAxis = new Vector4(rebarOne.GetShapeDrivenAccessor().Normal);
                rbd = rebarOne.GetBendData();
            }
            if (rebarIn != null)
            {
                doc = rebarIn.Document;
                // получить данные по форме стержня
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                ilc = rebarIn.GetCenterlineCurves(false, false, false);
                zAxis = new Vector4(rebarIn.Normal);
                rbd = rebarIn.GetBendData();
            }
            zAxis.Normalize();
        }

        /// <summary>
        /// Формирование данных по участкам армирования
        /// </summary>
        void DataBySegments()
        {

            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
            lg.Clear();
            // Цикл по сегментам в данной форме rsds.NumberOfSegments
            for (int i = 0; i < rsds.NumberOfSegments; i++)
            {
                TextOnRebar tor = new TextOnRebar();                                      // создаем будущую надпись над сегментом
                tor.rebar = rebar;                                                 // запишем текущий стержень
                RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент

                IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента                

                foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
                {
                    // получим длину сегмента
                    RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;

                    if (l != null)
                    {
                        ElementId pid = l.GetParamId();
                        Element elem = doc.GetElement(pid);
                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guid = pr.GUID;
                                // с учетом локальных особенностей
                                tor.value_initial = rebar.get_Parameter(pr.Definition).AsDouble();
                                // если длина стержня переменная, то показываем диапазон: минимум-максимум
                                if(tor.value_initial==0 &&
                                     (
                                      rebar.get_Parameter(BuiltInParameter.REBAR_MAX_LENGTH).AsDouble()!=
                                      rebar.get_Parameter(BuiltInParameter.REBAR_MIN_LENGTH).AsDouble())
                                     )
                                {
                                    // для нескольких участков показываем просто имя участка. Т.к. показать переменную длину пока невозможно
                                    if (rsds.NumberOfSegments > 1) tor.IsNameShow = true; 
                                    // длина участка должна быть получена с учетом возможных крюков
                                    double R = rbd.BendRadius + 0.5 * rbd.BarModelDiameter;
                                    double dist1 = 0;
                                    double dist2 = 0;
                                    if (hook_start > 0)
                                    {
                                        dist1 = R + 0.5 * rbd.BarModelDiameter - rbd.HookLength0 - R*rbd.HookAngle0/180*3.14159;
                                    }
                                    if (hook_end > 0)
                                    {
                                        dist2 = R + 0.5 * rbd.BarModelDiameter - rbd.HookLength1 - R * rbd.HookAngle1 / 180 * 3.14159;
                                    }

                                    tor.value_initial = rebar.get_Parameter(BuiltInParameter.REBAR_MAX_LENGTH).AsDouble() + dist1 + dist2;
                                    tor.value_initial_min = rebar.get_Parameter(BuiltInParameter.REBAR_MIN_LENGTH).AsDouble() + dist1 + dist2;                                    
                                }



                                tor.value = tor.value_initial + GetAddValue(template, tor.guid, rebar as Rebar);
                                if (tor.value <= 0) tor.value = tor.value_initial;
                                tor.value_min = tor.value_initial_min;
                                if (tor.value > 0) break;
                            }
                        }

                        // запишем для контроля имя параметра
                        tor.name = elem.Name;                                                                          // добавим метку
                        // определяем, что данный сегмент является дугой
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeDefaultBend");
                            tor.arc = true;
                        }
                        catch { }
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendRadius");
                            tor.arc = true;
                        }
                        catch { }

                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendArcLength");
                            tor.arc = true;
                        }
                        catch { }
                        continue;
                    }

                    // определяем, что данный участок наклонный
                    RebarShapeConstraintProjectedSegmentLength proj = rsc as RebarShapeConstraintProjectedSegmentLength;
                    // работаем с вертикальной проекцией текущего участка
                    if (proj != null && tor.guidV.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.V), 0) == 1)
                    {
                        tor.incline = InclineText.Incline;
                        ElementId pid = proj.GetParamId();
                        Element elem = doc.GetElement(pid);
                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guidV = pr.GUID;
                                tor.valueV = rebar.get_Parameter(pr.Definition).AsDouble();
                                if (tor.valueV > 0) break;
                            }
                        }
                        tor.nameV = elem.Name;                                                                          // добавим метку                       
                        continue;

                    }

                    if (proj != null && tor.guidH.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.U), 0) == 1)
                    {
                        tor.incline = InclineText.Incline;
                        ElementId pid = proj.GetParamId();
                        Element elem = doc.GetElement(pid);

                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guidH = pr.GUID;
                                tor.valueH = rebar.get_Parameter(pr.Definition).AsDouble();
                                if (tor.valueH > 0) break;

                            }
                        }
                        tor.nameH = elem.Name;                                                                          // добавим метку

                        continue;
                    }
                }

                // если проекция наклонного участока практически равна базовому участку, то наклонный не показываем
                if (tor.value_str == tor.valueH_str) tor.valueH = 0;
                if (tor.value_str == tor.valueV_str) tor.valueV = 0;
                lg.Add(tor);   // внесем сегмент в общий список
            }

        }





        /// <summary>
        /// Получить дополнительное локальное значение
        /// </summary>
        /// <param name="template">Шаблон проекта</param>
        /// <param name="parameter">Параметр</param>        
        /// <returns>Дополнительное значение</returns>         

        static double GetAddValue(Template template, Guid parameter, Element rebar)
        {
            if (template == Template.Other) return 0;
            string form = "";
            double diam = 0;
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;

            // получить менеджер текущего стержня
            if (rebarOne != null)
            {
                form = rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
                diam = rebarOne.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            }
            if (rebarIn != null)
            {
                form = rebarIn.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
                diam = rebarIn.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            }

            LegGuid guid = new LegGuid();

            switch (form)
            {

                case "10":
                    // минус два диаметра - для получения внутреннего размера хомута
                    return -2 * diam;
                case "26":
                    // минус два диаметра - для получения внутреннего размера хомута
                    if (parameter.Equals(guid.A)) return -2 * diam;
                    if (parameter.Equals(guid.D)) return -2 * diam;
                    break;
                default:
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// Изменить параметры стержня
        /// </summary>
        void ChangeParametersRebar()
        {
            // попытаемся обойтись без транзакций
            //transaction.Commit();     // запишем исходное состояние
            //FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
            //FailurePreproccessor preproccessor = new FailurePreproccessor();
            //options.SetFailuresPreprocessor(preproccessor);
            //transaction.SetFailureHandlingOptions(options);
            //transaction.Start();

            // failureOptions.SetFailuresPreprocessor(new HideNewTypeAssembly());

            SubTransaction sudst = new SubTransaction(doc);
            sudst.Start();
            // определяем принцип масштабирования
            bool by_segment = true;

            if (hooks.Count > 0)   // при наличии крюков - масштабируем по крюкам или по самому короткому сегменту
            {
                // получить максимальную длину крюка
                double max_hook = hooks.Min(x => x.value);
                // минимальная длина сегмента
                double min_segment = lg.Min(x => x.value);
                double max_segment = lg.Max(x => x.value);
                max_hook = Math.Min(max_hook, min_segment);
                // if (max_hook < min_segment) by_segment = false;
                if (max_hook < max_segment/max) by_segment = false;
            }

            if (!by_segment)   // при наличии крюков - масштабируем по крюкам или по самому короткому сегменту
            {
                // получить максимальную длину крюка
                double max_hook = hooks.Min(x => x.value);
                // максимальная длина сегмента
                double max_segment = lg.Max(x => x.value);
                double coeff = (max * max_hook) / max_segment;
                if (max_segment == 0) coeff = 1;
                coeff = coeff / coef_hook;
                // изменим параметры
                int i = 0;
                foreach (TextOnRebar tor in lg)
                {
                    // double coeff = tor.value / max_segment * coef_hook;

                    double value = tor.value * coeff;
                    //if (value < min * max_hook) value = min * max_hook * coef_hook;
                    //if (value > max * max_hook) value = max * max_hook * coef_hook;
                    if (tor.dialog) value = value * 1.0; // coef[i];                              // изменяем только для параметров включенных в диалог
                    else { i++; continue; }
                    SketchTools.SetParameter(rebar, tor.guid, value);
                    // изменить длины проекций
                    if (tor.valueH > 0)
                    {
                        value = tor.valueH * coeff;
                        //if (value < min * max_hook) value = min * max_hook * coef_hook;
                        //if (value > max * max_hook) value = max * max_hook * coef_hook;
                        // if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidH, value);
                    }
                    // изменить длины проекций
                    if (tor.valueV > 0)
                    {
                        value = tor.valueV * coeff;
                        //if (value < min * max_hook) value = min * max_hook * coef_hook;
                        //if (value > max * max_hook) value = max * max_hook * coef_hook;
                        // if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidV, value);
                    }
                    i++;
                }

                // doc.Regenerate();
            }
            else
            {      
                double max_segment = lg.Max(x => x.value);
                // изменим параметры
                int i = 0;
                foreach (TextOnRebar tor in lg)
                {
                    double new_value = tor.value;
                    // if (new_value < max_segment / 4) new_value = max_segment / 4;
                    if (new_value < max_segment / max) new_value = max_segment / max;
                    if (tor.dialog) new_value = new_value * 1.0; // coef[i];   // изменяем только для параметров включенных в диалог
                    else { i++; continue; }
                    SketchTools.SetParameter(rebar, tor.guid, new_value);
                    i++;
                }

                // doc.Regenerate();
            }


            doc.Regenerate();
            DivideByTypeRebar();       // получить новые кривые 
            GetInfoAboutHooks();       // обновить информацию по крюкам

            // данные по гнутым участкам: изменяем точки вставки текста
            int arc_num = 0;
            for (int i = curve_start; i < curve_end; i++)
            {
                Curve c = ilc[i];
                // гнутые участки записываем для указания радиуса или угла загиба                                 
                if (c.GetType().Name == "Arc")
                {
                    ChangeArcPosition(c as Arc, lg_angles[arc_num]);
                    arc_num++;
                }
            }

            sudst.RollBack();
            
            // восстановить старые значения сегментов
            foreach (TextOnRebar tor in lg)
            {
                SketchTools.SetParameter(rebar, tor.guid, tor.value_initial);

            }            

            doc.Regenerate();

            if (ilc.Count == 0)           // если не удалось создать измененную форму стержня - чертим как есть
            {
                DivideByTypeRebar();       // получить кривые 
                GetInfoAboutHooks();       // обновить информацию по крюкам
            }

            // transaction.RollBack();

            //if (!preproccessor.status) status = false;
            //else
            //{
            //GetInfoAboutHooks();       // обновить информацию по крюкам 
            //status = true;
            //}
            //transaction.Start();


        }

        void GetInfoAboutHooks()
        {
            // обновить информацию по крюкам
            hooks.Clear();

            // получить информацию по крюкам (начало)
            if (hook_start > 0 && ilc.Count>0)
            {
                hooks.Add(GetHookStart(ilc, rebar));                                                    // добавим информацию по крюку
            }
            if (hook_end > 0 && ilc.Count>0)
            {
                hooks.Add(GetHookEnd(ilc, rebar));                                                     // добавим информацию по крюку
            }
        }

        /// <summary>
        /// Получить параметры для начального крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnRebar GetHookStart(IList<Curve> curves, Element rebar)
        {            
            Curve c_straight = curves[0];
            Curve c_arc = curves[1];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar;
            // tor.position = c_straight.GetEndPoint(0);
            tor.value = rbd.HookLength0 + rbd.HookBendRadius+rbd.BarModelDiameter;
            tor.start = c_straight.GetEndPoint(0);
            tor.end = c_straight.GetEndPoint(1);
            tor.position = (tor.start + tor.end) / 2;
            tor.arc = true;
            return tor;
        }
        /// <summary>
        /// Получить параметры для конечного крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnRebar GetHookEnd(IList<Curve> curves, Element rebar)
        {
            Curve c_straight = curves.Last();
            Curve c_arc = curves[curves.Count - 2];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar;
            // tor.position = c_straight.GetEndPoint(1);
            tor.value = rbd.HookLength1 + rbd.HookBendRadius + rbd.BarModelDiameter;
            tor.start = c_straight.GetEndPoint(1);
            tor.end = c_straight.GetEndPoint(0);
            tor.position = (tor.start + tor.end) / 2;
            tor.arc = true;
            return tor;
        }


    }

    /// <summary>
    /// Построение чертежа 
    /// </summary>
    public class BuildImageByAxis
    {
        public BuildImageByAxis(Element rebar)
        {
            this.rebar = rebar;
            PreDivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
            GetInfoAboutHooks();                           // получить данные по крюкам

            // запишем данные по гнутым участкам
            lg_arc.Clear();
            for (int i = curve_start; i < curve_end; i++)
            {
                Curve c = ilc[i];

                // гнутые участки записываем для указания радиуса загиба
                if (c.GetType().Name == "Arc") lg_arc.Add(GetArcSegment(ilc, rebar, i));        // для участка типа дуга
            }

            DataBySegments();                              // Формирование данных по участкам армирования

        }
        public void UpdateImage()
        {
            // DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
            // GetInfoAboutHooks();                           // получить данные по крюкам           
            ChangeParametersRebar();                          // изменить параметры стержня
            if (status)
            {
                IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов
                if (IsRebarCorrect)
                {
                    GetPointsAndLinesForDrawing();             // Инициализация данных для сегментов, радиусов загиба 
                    DrawPicture();
                }

            }
        }
        #region Параметры
        /// <summary>
        /// Статус создания стержня
        /// </summary>
        public bool status = true;
        /// <summary>
        /// Текущая транзакция
        /// </summary>
        public Transaction transaction;      
        /// <summary>
        /// Чертеж
        /// </summary>
        public Graphics graphic;
        /// <summary>
        /// Элемент арматурного стержня
        /// </summary>
        public Element rebar;
        /// <summary>
        /// Размер рисунка по оси Х
        /// </summary>
        public int sizeX = 1000;              // по умолчанию
        /// <summary>
        /// Размер рисунка по оси Y
        /// </summary>
        public int sizeY = 300;               // по умолчанию
        /// <summary>
        /// Размер шрифта
        /// </summary>
        public float move = 90;             // по умолчанию
        /// <summary>
        /// Размер канвы
        /// </summary>
        public float canva = 63;            // по умолчанию
        /// <summary>
        /// Коэффициент перевода единиц
        /// </summary>
        const float unit = (float)0.00328;
        #endregion Параметры

        #region Инициализация массивов
        /// <summary>
        /// Параметры диалога
        /// </summary>
        public DataForm dataform=null;
        /// <summary>
        /// Видимость длины крюков
        /// </summary>
        public bool hooks_length = true;
        /// <summary>
        /// Видимость радиусов загиба
        /// </summary>
        public bool bending = false;
        /// <summary>
        /// Коэффициенты для крюков
        /// </summary>
        public double coef_hook = 1;
        /// <summary>
        /// Коэффициенты для сегментов стержня
        /// </summary>
        public double[] coef = { 1, 1, 1, 1, 1, 1, 1 };
        /// <summary>
        /// Параметры для крюков
        /// </summary>
        public List<TextOnRebar> hooks = new List<TextOnRebar>();                                // Список параметров для крюков           
        /// <summary>
        /// Линии чертежа (только прямые)
        /// </summary>
        public List<Line2D> line2D_L = new List<Line2D>();                                       // список плоских линий для чертежа (только прямые)
        /// <summary>
        /// Линии чертежа
        /// </summary>
        public List<Line2D> line2D = new List<Line2D>();                                       // список плоских линий для чертежа
        /// <summary>
        /// Линии арматуры
        /// </summary>
        public List<PointF> pointDF = new List<PointF>();
        /// <summary>
        /// Список параметров для прямых сегментов
        /// </summary> 
        public List<TextOnRebar> lg = new List<TextOnRebar>();
        /// <summary>
        /// Текстовые надписи (радиусы)
        /// </summary>
        public List<TextOnArc> lg_arc_sorted = new List<TextOnArc>();
        /// <summary>
        /// Линии чертежа
        /// </summary>
        public List<TextOnArc> lg_arc = new List<TextOnArc>();
        ///// <summary>
        ///// Надписи над отрезками
        ///// </summary>
        //public List<TextOnRebar> Llg = new List<TextOnRebar>();
        /// <summary>
        /// Cписок только прямых сегментов для стержня проекта
        /// </summary>
        IList<Curve> ilc = new List<Curve>();
        /// <summary>
        /// Cписок сегментов для стержня проекта
        /// </summary>
        IList<Curve> ilc_line = new List<Curve>();
        #endregion Инициализация массивов

        /// <summary>
        /// Файл рисунка
        /// </summary>
        public Bitmap flag;
        //{
        //    get
        //    {
        //        return new Bitmap(sizeX,sizeY);
        //    }           
        //}
        /// <summary>
        /// Признак правильного создания стержня
        /// </summary>
        public bool IsRebarCorrect;
        /// <summary>
        /// Направление оси Z - перпендикулярно плоскости стержня
        /// </summary>
        Vector4 zAxis = new Vector4(XYZ.Zero);
        /// <summary>
        /// Максимум модели по оси Х
        /// </summary>
        public float maxX = 1;
        /// <summary>
        /// Максимум модели по оси Y
        /// </summary>
        public float maxY = 1;
        /// <summary>
        /// Минимум модели по оси Х
        /// </summary>
        public float minX = 1;
        /// <summary>
        /// Минимум модели по оси Y
        /// </summary>
        public float minY = 1;
        /// <summary>
        /// Коэффициент масштаба
        /// </summary>
        float scale
        {
            get
            {
                float scaleX = (float)((sizeX - 2 * canva) / maxX);
                float scaleY = (float)(sizeY - 2 * canva) / maxY;
                return Math.Min(scaleX, scaleY);
            }
        }
        /// <summary>
        /// Сдвиг по оси Х
        /// </summary>
        public float moveX;
        /// <summary>
        /// Сдвиг по оси Y
        /// </summary>
        public float moveY;
        /// <summary>
        /// Текущий документ
        /// </summary>
        Document doc;
        /// <summary>
        /// Форма стержня
        /// </summary>
        RebarShape rs = null;
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_start
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId().IntegerValue; }
        }
        /// <summary>
        /// Крюк в начале стержня
        /// </summary>
        int hook_end
        {
            get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId().IntegerValue; }
        }

        /// <summary>
        /// Номер начального кривой (без крюка)
        /// </summary>
        int curve_start
        {
            get
            {
                if (hook_start > 0) return 2;
                else return 0;
            }
        }

        /// <summary>
        /// Номер конечной кривой (без крюка)
        /// </summary>
        int curve_end
        {
            get
            {
                if (hook_end > 0) return ilc.Count - 2;
                else return ilc.Count;
            }
        }

        /// <summary>
        /// Коэффициент минимальной длины по крюку
        /// </summary>
        int min = 5;
        /// <summary>
        /// Коэффициент максимальной длины по крюку
        /// </summary>
        int max = 15;
        /// <summary>
        /// Начальная точка
        /// </summary>
        XYZ p_initial = null;
        /// <summary>
        /// Основное направление - по оси Х
        /// </summary>
        XYZ dir_major = null;

        /// <summary>
        /// Получить параметры для дугового сегмента
        /// </summary>
        /// <param name="curves">Линия стержня</param>
        /// <param name="rebar">Элемент стержня</param>
        /// <param name="i">Текущий номер линии</param>
        /// <param name="value">Значение параметра</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnArc GetArcSegment(IList<Curve> curves, Element rebar, int i, double value = 0)
        {
            // получить диаметр стержня
            double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            TextOnArc toa = new TextOnArc();
            toa.rebar = rebar;
            Arc arc = curves[i] as Arc;
            // запишем координаты, направление сегмента и радиус
            toa.position = arc.Center;                                         // запишем координаты центра дуги 
            toa.start = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;         // начальная точка сегмента
            toa.end = arc.Center;                                              // конечная точка сегмента    
            if (value == 0) toa.value = arc.Radius - d / 2;                    // запишем радиус дуги (по внутреннему контуру)
            else toa.value = value;
            // получить длину примыкающих прямых сегментов
            double l1, l2;
            l1 = l2 = 0;
            if ((i - 1) >= 0) l1 = curves[i - 1].Length;
            if ((i + 1) < curves.Count) l2 = curves[i + 1].Length;
            toa.nearestL = l1 + l2;
            return toa;
        }
        /// <summary>
        /// Создать чертеж
        /// </summary>
        void DrawPicture()
        {
            // готовим рисунок
            flag = new Bitmap(sizeX, sizeY);

            #region Получим точки с учетом масштаба
            for (int i = 0; i < line2D_L.Count; i++)
            {
                line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X * scale, line2D_L[i].p1F.Y * scale), new PointF(line2D_L[i].p2F.X * scale, line2D_L[i].p2F.Y * scale));
            }

            for (int i = 0; i < line2D.Count; i++)
            {
                line2D[i] = new Line2D(new PointF(line2D[i].p1F.X * scale, line2D[i].p1F.Y * scale), new PointF(line2D[i].p2F.X * scale, line2D[i].p2F.Y * scale));
            }

            for (int i = 0; i < pointDF.Count; i++)
            {
                pointDF[i] = new PointF(pointDF[i].X * scale, pointDF[i].Y * scale);
            }
            foreach (TextOnRebar tor in lg) { tor.UsingScale(scale); }
            foreach (TextOnArc tor in lg_arc) { tor.UsingScale(scale); }
            foreach (TextOnRebar tor in hooks) { tor.UsingScale(scale); }

            #endregion Получим точки с учетом масштаба
            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

            moveX = (sizeX - 2 * canva - maxX) / 2;
            moveY = (sizeY - 2 * canva - maxY) / 2;

            //graphic = Graphics.FromImage(flag);
            //graphic.Clear(System.Drawing.Color.White);
            // отсортируем список по длине примыкающих прямых сегментов            
            lg_arc_sorted = lg_arc.OrderByDescending(x => x.nearestL).ToList();
        }
        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        void GetPointsAndLinesForDrawing()
        {
            float dX = 0;           // смещение центра рисунка
            float dY = 0;
            // приведем пространственную систему координат стержня в плоскую систему
            // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
            // начало системы координат принимаем в произвольной точке стержня 

            Vector4 origin = new Vector4(p_initial);
            // направление оси Х          
            Vector4 xAxis = new Vector4(dir_major);
            xAxis.Normalize();
            // направление оси Y 
            Vector4 yAxis = new Vector4(XYZ.Zero);
            yAxis = Vector4.CrossProduct(xAxis, zAxis);
            yAxis.Normalize();

            Matrix4 MatrixMain = new Matrix4(xAxis, yAxis, zAxis, origin);
            // после выполнения инверсии в TRANSFORM можем подставлять ГЛОБАЛЬНЫЕ КООРДИНАТЫ и получать ЛОКАЛЬНЫЕ
            MatrixMain = MatrixMain.Inverse();
            pointDF.Clear();
            line2D.Clear();
            line2D_L.Clear();
            // выполним расчет точек для чертежа линий арматуры
            foreach (Curve c in ilc)
            {
                IList<XYZ> tp = c.Tessellate();
                foreach (XYZ p in tp)
                {
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p));                      // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    pointDF.Add(p_new1F);                                                        // получить точку для картинки                     
                }

                tp = c.Tessellate();
                // получим линии чертежа арматуры
                for (int i = 0; i < tp.Count - 1; i++)
                {
                    XYZ p1 = tp[i];
                    XYZ p2 = tp[i + 1];
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
                    PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
                    Line2D line = new Line2D(p_new1F, p_new2F);
                    line2D.Add(line);                                                            // добавить линию к списку

                }

                if (c.GetType().Name == "Arc") continue;                                        // для участка типа дуга

                tp = c.Tessellate();
                // получим линии чертежа арматуры
                for (int i = 0; i < tp.Count - 1; i++)
                {
                    XYZ p1 = tp[i];
                    XYZ p2 = tp[i + 1];
                    Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
                    Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
                    PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
                    Line2D line = new Line2D(p_new1F, p_new2F);
                    line2D_L.Add(line);                                                            // добавить линию к списку

                }

            }

            pointDF = pointDF.ToList();

            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);
            // все точки должны быть в 1 четверти
            if (minX < 0)
                for (int i = 0; i < pointDF.Count(); i++)
                {
                    pointDF[i] = new PointF(pointDF[i].X - minX, pointDF[i].Y);
                    dX = minX;
                }
            if (minY < 0)
                for (int i = 0; i < pointDF.Count(); i++)
                {
                    pointDF[i] = new PointF(pointDF[i].X, pointDF[i].Y - minY);
                    dY = minY;
                }

            if (minX < 0)
                for (int i = 0; i < line2D_L.Count(); i++)
                {
                    line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X - minX, line2D_L[i].p1F.Y), new PointF(line2D_L[i].p2F.X - minX, line2D_L[i].p2F.Y));
                }
            if (minY < 0)
                for (int i = 0; i < line2D_L.Count(); i++)
                {
                    line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X, line2D_L[i].p1F.Y - minY), new PointF(line2D_L[i].p2F.X, line2D_L[i].p2F.Y - minY));

                }

            if (minX < 0)
                for (int i = 0; i < line2D.Count(); i++)
                {
                    line2D[i] = new Line2D(new PointF(line2D[i].p1F.X - minX, line2D[i].p1F.Y), new PointF(line2D[i].p2F.X - minX, line2D[i].p2F.Y));
                }
            if (minY < 0)
                for (int i = 0; i < line2D.Count(); i++)
                {
                    line2D[i] = new Line2D(new PointF(line2D[i].p1F.X, line2D[i].p1F.Y - minY), new PointF(line2D[i].p2F.X, line2D[i].p2F.Y - minY));

                }

            // повторы будем убирать после размешения размеров
            // Llg = lg.ToList();

            // выполнить расчет координат точек для вставки текста
            for (int i = 0; i < lg.Count; i++) { lg[i] = RecalculatePointPosition(MatrixMain, lg[i], dX, dY); }

            // выполнить расчет координат точек для вставки текста (дуги)
            for (int i = 0; i < lg_arc.Count; i++) { lg_arc[i] = RecalculatePointPosition(MatrixMain, lg_arc[i], dX, dY); }
                       
            // выполнить расчет координат точек для вставки текста (крюки)
            for (int i = 0; i < hooks.Count; i++) { hooks[i] = RecalculatePointPosition(MatrixMain, hooks[i], dX, dY, true); }

            SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

        }

        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <returns>Текстовая надпись для арки</returns> 
        TextOnArc RecalculatePointPosition(Matrix4 matrix, TextOnArc tr, float dX, float dY)
        {
            Vector4 p_new = matrix.Transform(new Vector4(tr.position));                         // получить точку в локальной системе координат
            tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
            tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
            tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            tr.incline = InclineText.Incline;                                                     // получить направление надписи
            // получить угол наклона надписи в градусах
            double dAY = (double)(tr.endF.Y - tr.startF.Y);
            double dAX = (double)(tr.endF.X - tr.startF.X);
            if (dAX == 0) tr.angle = 0;
            else tr.angle = (float)Math.Atan2(dAY, dAX);
            return tr;
        }

        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <param name="hook">Признак расчета для крюков</param>
        /// <returns>Текстовая надпись для прямого сегмента</returns> 
        TextOnRebar RecalculatePointPosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook = false)
        {
            Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
            tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
            tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
            tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
            if (hook) return tr;
            // получить направление надписи
            if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
            {
                // дополнительный сдвиг для вертикальной надписи в сторону линии
                tr.incline = InclineText.Vertic; return tr;
            }
            if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
            tr.incline = InclineText.Incline;
            // получить угол наклона надписи в градусах
            double dAY = (double)(tr.endF.Y - tr.startF.Y);
            double dAX = (double)(tr.endF.X - tr.startF.X);
            if (dAX == 0) tr.angle = 0;
            else tr.angle = (float)Math.Atan2(dAY, dAX);
            return tr;
        }

        /// <summary>
        /// Инициализация данных для сегментов, радиусов загиба
        /// </summary>
        bool InitialDataForSegments()
        {
            double max_length = 0;
            int num_segment = 0;
            int num_arc = 0;
            for (int i = curve_start; i < curve_end; i++)
            {
                Curve c = ilc[i];

                // гнутые участки записываем с сохранением радиуса загиба
                if (c.GetType().Name == "Arc")                                     // для участка типа дуга
                {
                    lg_arc[num_arc] = GetArcSegment(ilc, rebar, i, lg_arc[num_arc].value);
                    num_arc++;
                }

                // некоторые гнутые участки необходимо пропускать. Это стандартные гнутые участки, которые не имеют фактических сегментов
                if (c.GetType().Name == "Line" && lg[num_segment].arc) continue;
                if (c.GetType().Name == "Arc" && !lg[num_segment].arc) continue;

                if (max_length < c.Length && lg[num_segment].incline != InclineText.Incline)  // наклонные участки не рассматриваем как основные
                {
                    max_length = c.Length;
                    p_initial = c.GetEndPoint(0);
                    dir_major = (c.GetEndPoint(1) - p_initial).Normalize();
                }

                if (c.GetType().Name == "Arc")                       // для участка типа дуга
                {
                    lg[num_segment].arc = true;
                    Arc arc = c as Arc;
                    // запишем координаты и направление сегмента
                    lg[num_segment].position = arc.Center + arc.YDirection * arc.Radius;       // запишем координаты центра дуги 
                    lg[num_segment].start = arc.Center;                                        // начальная точка сегмента
                    lg[num_segment].end = arc.Center + arc.XDirection;                         // конечная точка сегмента

                }
                else
                {
                    // запишем координаты и направление сегмента
                    lg[num_segment].position = (c.GetEndPoint(0) + c.GetEndPoint(1)) / 2;
                    lg[num_segment].start = c.GetEndPoint(0);
                    lg[num_segment].end = c.GetEndPoint(1);
                }

                num_segment++;
            }
            // проверка наличия позиций у сегментов. Если позиции нет - картинки не будет. Какое-то несоответствие. Возможно стержень 3d или самопальное семейство
            foreach (TextOnRebar tor in lg)
            {
                if (tor.position == null) return false;
            }
            if (p_initial == null || dir_major == null) return false;                        // картинки не будет
            // Llg = lg.ToList();
            return true;
        }
        /// <summary>
        /// Предварительное деление по типам арматурных стержней: получить участки прямых и вектор Z
        /// </summary>
        void PreDivideByTypeRebar()
        {
            ilc_line.Clear();             
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;
            // здесь выполняем 
            if (rebarOne != null)
            {
                doc = rebarOne.Document;
                // получить данные по форме стержня
                ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;                            
                zAxis = new Vector4(rebarOne.GetShapeDrivenAccessor().Normal);
            }
            if (rebarIn != null)
            {
                doc = rebarIn.Document;
                // получить данные по форме стержня
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                ilc = rebarIn.GetCenterlineCurves(false, false, false);
                zAxis = new Vector4(rebarIn.Normal);
            }
            zAxis.Normalize();
            // существующие участки заменим на прямые участки
            // правило: все дуговые сегменты убираем; ищем пересечение прямых участков
            int start_segment = 0;
            int end_segment = ilc.Count;
            if (hook_start > 0) start_segment = 2;
            if (hook_end > 0) end_segment = end_segment - 2;

            for (int c = start_segment; c < end_segment; c++)
            {
                Line line = ilc[c] as Line;
                if (line == null) continue;    // начинаем всегда с прямой линии
                if (c == end_segment - 1) { ilc_line.Add(ilc[c]); continue; }  // добавляем последний сегмент
                // переходим к следуюшему сегменту
                int n = c + 1;
                Line line_next = ilc[n] as Line;                                     // следующий фрагмент
                if (line_next != null) { ilc_line.Add(ilc[c]); continue; }           // добавляем текущий сегмент
                // переходим к следуюшему сегменту
                int m = n + 1;
                if (m == end_segment) break;                                            // вышли за пределы цикла
                line_next = ilc[m] as Line;                                             // следующий фрагмент
                if (line_next == null) { c++;  continue; }                              // рассматривается только прямой участок
                // ищем пересечение двух отрезков как двух лучей
                XYZ cross = SketchTools.CrossRay(SketchTools.GetRay(line), SketchTools.GetRay(line_next));
                if (cross == null)
                {   // если пересечения нет, то полагаем, что это параллельные отрезки. Их длина - до вершины арки, а именно радиус
                    Arc arc = ilc[n] as Arc;
                    double R = arc.Radius;                     
                    XYZ dir1 = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
                    // добавляем текущий отрезок
                    ilc_line.Add(Line.CreateBound(line.GetEndPoint(0), line.GetEndPoint(0) + dir1*(line.Length+R)));                    
                    // изменим последующий фрагмент
                    XYZ dir2 = (line_next.GetEndPoint(1) - line_next.GetEndPoint(0)).Normalize();
                    ilc[m] = Line.CreateBound(line_next.GetEndPoint(0) - dir2*R,line_next.GetEndPoint(1));
                    // добавить участок длины дуги
                    ilc_line.Add(Line.CreateBound(line.GetEndPoint(0) + dir1 * (line.Length + R), line_next.GetEndPoint(0) - dir2 * R));

                } // пересечения нет. следующий фрагмент
                else
                {
                    // добавляем текущий отрезок
                    ilc_line.Add(Line.CreateBound(line.GetEndPoint(0), cross));
                    // изменим последующий фрагмент
                    ilc[m] = Line.CreateBound(cross, ilc[m].GetEndPoint(1));
                }
                c++;
            }
        }

        /// <summary>
        /// Деление по типам арматурных стержней: получить участки прямых и вектор Z
        /// </summary>
        void DivideByTypeRebar()
        {           
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;
            // здесь выполняем 
            if (rebarOne != null)
            {
                doc = rebarOne.Document;
                // получить данные по форме стержня
                ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
              
                
            }
            if (rebarIn != null)
            {
                doc = rebarIn.Document;                
                ilc = rebarIn.GetCenterlineCurves(false, false, false);
            
            }          
        }
        
        /// <summary>
        /// Формирование данных по участкам армирования
        /// </summary>
        void DataBySegments()
        {
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
            lg.Clear();
            // Цикл по сегментам в данной форме rsds.NumberOfSegments
            for (int i = 0; i < rsds.NumberOfSegments; i++)
            {
                TextOnRebar tor = new TextOnRebar();                                      // создаем будущую надпись над сегментом
                tor.rebar = rebar;                                                        // запишем текущий стержень
                RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент

                IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента                

                foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
                {
                    // получим длину сегмента
                    RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;

                    if (l != null)
                    {
                        ElementId pid = l.GetParamId();
                        Element elem = doc.GetElement(pid);
                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guid = pr.GUID;
                                // с учетом локальных особенностей
                                tor.value_initial = rebar.get_Parameter(pr.Definition).AsDouble();
                                tor.value = tor.value_initial;
                                if (tor.value <= 0) tor.value = tor.value_initial;
                                if (tor.value > 0) break;
                            }
                        }

                        // запишем для контроля имя параметра
                        tor.name = elem.Name;                                                                          // добавим метку
                        // определяем, что данный сегмент является дугой
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeDefaultBend");
                            tor.arc = true;
                        }
                        catch { }
                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendRadius");
                            tor.arc = true;
                        }
                        catch { }

                        try
                        {
                            RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendArcLength");
                            tor.arc = true;
                        }
                        catch { }
                        continue;
                    }

                    // определяем, что данный участок наклонный
                    RebarShapeConstraintProjectedSegmentLength proj = rsc as RebarShapeConstraintProjectedSegmentLength;
                    // работаем с вертикальной проекцией текущего участка
                    if (proj != null && tor.guidV.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.V), 0) == 1)
                    {
                        tor.incline = InclineText.Incline;
                        ElementId pid = proj.GetParamId();
                        Element elem = doc.GetElement(pid);
                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guidV = pr.GUID;
                                tor.valueV = rebar.get_Parameter(pr.Definition).AsDouble();
                                if (tor.valueV > 0) break;
                            }
                        }
                        tor.nameV = elem.Name;                                                                          // добавим метку                       
                        continue;

                    }

                    if (proj != null && tor.guidH.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.U), 0) == 1)
                    {
                        tor.incline = InclineText.Incline;
                        ElementId pid = proj.GetParamId();
                        Element elem = doc.GetElement(pid);

                        foreach (Parameter pr in pset)
                        {
                            if (pr.Definition.Name == elem.Name)
                            {
                                tor.guidH = pr.GUID;
                                tor.valueH = rebar.get_Parameter(pr.Definition).AsDouble();
                                if (tor.valueH > 0) break;

                            }
                        }
                        tor.nameH = elem.Name;                                                                          // добавим метку

                        continue;
                    }
                }
                // if (tor.arc) continue;         // такие участки не берем
                // если проекция наклонного участока практически равна базовому участку, то наклонный не показываем
                if (tor.value_str == tor.valueH_str) tor.valueH = 0;
                if (tor.value_str == tor.valueV_str) tor.valueV = 0;
                lg.Add(tor);   // внесем сегмент в общий список
            }

        }





        ///// <summary>
        ///// Получить дополнительное локальное значение
        ///// </summary>
        ///// <param name="template">Шаблон проекта</param>
        ///// <param name="parameter">Параметр</param>        
        ///// <returns>Дополнительное значение</returns>         

        //static double GetAddValue(Template template, Guid parameter, Element rebar)
        //{
        //    if (template == Template.Other) return 0;
        //    string form = "";
        //    double diam = 0;
        //    Rebar rebarOne = rebar as Rebar;
        //    RebarInSystem rebarIn = rebar as RebarInSystem;

        //    // получить менеджер текущего стержня
        //    if (rebarOne != null)
        //    {
        //        form = rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
        //        diam = rebarOne.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
        //    }
        //    if (rebarIn != null)
        //    {
        //        form = rebarIn.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
        //        diam = rebarIn.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
        //    }

        //    LegGuid guid = new LegGuid();

        //    switch (form)
        //    {

        //        case "10":
        //            // минус два диаметра - для получения внутреннего размера хомута
        //            return -2 * diam;
        //        case "26":
        //            // минус два диаметра - для получения внутреннего размера хомута
        //            if (parameter.Equals(guid.A)) return -2 * diam;
        //            if (parameter.Equals(guid.D)) return -2 * diam;
        //            break;
        //        default:
        //            return 0;
        //    }
        //    return 0;
        //}

        /// <summary>
        /// Изменить параметры стержня
        /// </summary>
        void ChangeParametersRebar()
        {
            SubTransaction sudst = new SubTransaction(doc);
            sudst.Start();
            // определяем принцип масштабирования
            bool by_segment = true;

            if (hooks.Count > 0)   // при наличии крюков - масштабируем по крюкам или по самому короткому сегменту
            {
                // получить максимальную длину крюка
                double max_hook = hooks.Min(x => x.value);
                // минимальная длина сегмента
                double min_segment = lg.Min(x => x.value);
                max_hook = Math.Min(max_hook, min_segment);
                if (max_hook < min_segment) by_segment = false;
            }

            if (!by_segment)   // при наличии крюков - масштабируем по крюкам или по самому короткому сегменту
            {
                // получить максимальную длину крюка
                double max_hook = hooks.Min(x => x.value);
                // максимальная длина сегмента
                double max_segment = lg.Max(x => x.value);
                double coeff = (max * max_hook) / max_segment;
                if (max_segment == 0) coeff = 1;
                coeff = coeff / coef_hook;
                // изменим параметры
                int i = 0;
                foreach (TextOnRebar tor in lg)
                {                   
                    double value = tor.value * coeff;                    
                    //if (tor.dialog) value = value * coef[i];                              // изменяем только для параметров включенных в диалог
                    //else { i++; continue; }
                    SketchTools.SetParameter(rebar, tor.guid, value);
                                                                 
                    // изменить длины проекций
                    if (tor.valueH > 0)
                    {
                        value = tor.valueH * coeff;                     
                        //if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidH, value);
                        double alfa = tor.valueH / tor.value;
                        tor.valueH = ilc_line[i].Length*alfa;
                    }
                    // изменить длины проекций
                    if (tor.valueV > 0)
                    {
                        value = tor.valueV * coeff;                       
                        //if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidV, value);
                        double alfa = tor.valueV / tor.value;
                        tor.valueV = ilc_line[i].Length * alfa;
                    }
                    tor.value = ilc_line[i].Length;    // длина по оси
                    i++;
                }

   
            }
            else
            {
                double max_segment = lg.Max(x => x.value);
                // изменим параметры
                int i = 0;              
                foreach (TextOnRebar tor in lg)
                {
                    double new_value = tor.value;
                    if (new_value < max_segment / 4) new_value = max_segment / 4;
                    double coeff = new_value / tor.value;
                    //if (tor.dialog) new_value = new_value * coef[i];   // изменяем только для параметров включенных в диалог
                    //else { i++; continue; }
                    SketchTools.SetParameter(rebar, tor.guid, new_value);

                    // изменить длины проекций
                    if (tor.valueH > 0)
                    {
                        double value = tor.valueH * coeff;
                        //if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidH, value);
                        double alfa = tor.valueH / tor.value;
                        tor.valueH = ilc_line[i].Length * alfa;
                    }
                    // изменить длины проекций
                    if (tor.valueV > 0)
                    {
                        double value = tor.valueV * coeff;
                        //if (tor.dialog) value = value * coef[i];
                        SketchTools.SetParameter(rebar, tor.guidV, value);
                        double alfa = tor.valueV / tor.value;
                        tor.valueV = ilc_line[i].Length * alfa;
                    }

                    tor.value = ilc_line[i].Length;    // длина по оси
                    i++;
                }       
            }

            doc.Regenerate();
            DivideByTypeRebar();       // получить новые кривые 
            UpdateInfoAboutHooks();       // обновить информацию по крюкам
            sudst.RollBack();

            // восстановить старые значения сегментов
            foreach (TextOnRebar tor in lg)
            {
                SketchTools.SetParameter(rebar, tor.guid, tor.value_initial);
            }
            doc.Regenerate();
        
        }

        void GetInfoAboutHooks()
        {
            // обновить информацию по крюкам
            hooks.Clear();

            // получить информацию по крюкам (начало)
            if (hook_start > 0)
            {
                hooks.Add(GetHookStart(ilc, rebar));                                                    // добавим информацию по крюку
            }
            if (hook_end > 0)
            {
                hooks.Add(GetHookEnd(ilc, rebar));                                                     // добавим информацию по крюку
            }
        }

        void UpdateInfoAboutHooks()
        {
            int n = 0;
            // получить информацию по крюкам (начало)
            if (hook_start > 0)
            {                 
                Line line1 = ilc[0] as Line;
                Line line2 = ilc[2] as Line;
                XYZ cross = SketchTools.CrossRay(SketchTools.GetRay(line1), SketchTools.GetRay(line2));
                hooks[0].position = line1.GetEndPoint(0);
                hooks[0].start = line1.GetEndPoint(0);
                hooks[0].end = cross;
                n++;
            }
            if (hook_end > 0)
            {                
                Line line2 = ilc[ilc.Count - 3] as Line;
                Line line1 = ilc[ilc.Count - 1] as Line;
                XYZ cross = SketchTools.CrossRay(SketchTools.GetRay(line1), SketchTools.GetRay(line2));  
                hooks[n].position = line1.GetEndPoint(1);              
                hooks[n].start = cross;
                hooks[n].end = line1.GetEndPoint(1);                
            }
        }

        /// <summary>
        /// Получить параметры для начального крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержня</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnRebar GetHookStart(IList<Curve> curves, Element rebar)
        {
            Line line1 = curves[0] as Line;
            Line line2 = curves[2] as Line;
            XYZ cross = SketchTools.CrossRay(SketchTools.GetRay(line1), SketchTools.GetRay(line2));
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar;
            tor.position = line1.GetEndPoint(0);
            tor.value = line1.GetEndPoint(0).DistanceTo(cross);
            tor.start = line1.GetEndPoint(0);
            tor.end = cross;
            tor.arc = false;
            return tor;
        }
        /// <summary>
        /// Получить параметры для конечного крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        TextOnRebar GetHookEnd(IList<Curve> curves, Element rebar)
        {
            Line line2 = curves[curves.Count - 3] as Line;
            Line line1 = curves[curves.Count - 1] as Line;
            XYZ cross = SketchTools.CrossRay(SketchTools.GetRay(line1), SketchTools.GetRay(line2));
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar;
            tor.position = line1.GetEndPoint(1);
            tor.value = line1.GetEndPoint(1).DistanceTo(cross);
            tor.start = cross;
            tor.end = line1.GetEndPoint(1);
            tor.arc = false;
            return tor;
        }


    }


    ///// <summary>
    ///// Построение чертежа 
    ///// </summary>
    //public class BuildImage
    //{
    //    public BuildImage()
    //    {
    //    }
    //    public BuildImage(Element rebar, Template template)
    //    {
    //        this.rebar = rebar; this.template = template;
    //        DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
    //        GetInfoAboutHooks();                           // получить данные по крюкам
    //        // запишем данные по гнутым участкам
    //        lg_arc.Clear();
    //        for (int i = curve_start; i < curve_end; i++)
    //        {
    //            Curve c = ilc[i];

    //            // гнутые участки записываем для указания радиуса загиба
    //            if (c.GetType().Name == "Arc") lg_arc.Add(GetArcSegment(ilc, rebar, i));        // для участка типа дуга
    //        }
    //        DataBySegments();                              // Формирование данных по участкам армирования
    //    }
    //    public void UpdateImage()
    //    {
    //        DivideByTypeRebar();                           // разделить по типам стержней, получить кривые и вектор Z
    //        GetInfoAboutHooks();                           // получить данные по крюкам
    //        ChangeParametersRebar();                       // изменить параметры стержня
    //        if (status)
    //        {
    //            IsRebarCorrect = InitialDataForSegments();       // инициализация данных для сегментов
    //            if (IsRebarCorrect)
    //            {
    //                GetPointsAndLinesForDrawing();             // Инициализация данных для сегментов, радиусов загиба 
    //                DrawPicture();
    //            }        
                
    //        }
    //    }
    //    #region Параметры
    //    /// <summary>
    //    /// Статус создания стержня
    //    /// </summary>
    //    public bool status=true;
    //    /// <summary>
    //    /// Текущая транзакция
    //    /// </summary>
    //    public Transaction transaction;
    //    /// <summary>
    //    /// Текущий шаблон проекта
    //    /// </summary>
    //    public Template template;
    //    /// <summary>
    //    /// Чертеж
    //    /// </summary>
    //    public Graphics graphic;
    //    /// <summary>
    //    /// Элемент арматурного стержня
    //    /// </summary>
    //    public Element rebar;
    //    /// <summary>
    //    /// Размер рисунка по оси Х
    //    /// </summary>
    //    public int sizeX=1000;              // по умолчанию
    //    /// <summary>
    //    /// Размер рисунка по оси Y
    //    /// </summary>
    //    public int sizeY=300;               // по умолчанию
    //    /// <summary>
    //    /// Размер шрифта
    //    /// </summary>
    //    public float move = 90;             // по умолчанию
    //    /// <summary>
    //    /// Размер канвы
    //    /// </summary>
    //    public float canva = 63;            // по умолчанию
    //    /// <summary>
    //    /// Коэффициент перевода единиц
    //    /// </summary>
    //    const float unit = (float)0.00328;
    //    #endregion Параметры

    //    #region Инициализация массивов
    //    /// <summary>
    //    /// Видимость длины крюков
    //    /// </summary>
    //    public bool hooks_length = true;
    //    /// <summary>
    //    /// Видимость радиусов загиба
    //    /// </summary>
    //    public bool bending = false;
    //    /// <summary>
    //    /// Коэффициенты для крюков
    //    /// </summary>
    //    public double coef_hook = 1;
    //    /// <summary>
    //    /// Коэффициенты для сегментов стержня
    //    /// </summary>
    //    public double[] coef = { 1, 1, 1, 1, 1, 1, 1 };
    //    /// <summary>
    //    /// Параметры для крюков
    //    /// </summary>
    //    public List<TextOnRebar> hooks = new List<TextOnRebar>();                                // Список параметров для крюков           
    //    /// <summary>
    //    /// Линии чертежа (только прямые)
    //    /// </summary>
    //    public List<Line2D> line2D_L = new List<Line2D>();                                       // список плоских линий для чертежа (только прямые)
    //    /// <summary>
    //    /// Линии чертежа
    //    /// </summary>
    //    public List<Line2D> line2D = new List<Line2D>();                                       // список плоских линий для чертежа
    //    /// <summary>
    //    /// Линии арматуры
    //    /// </summary>
    //    public List<PointF> pointDF=new List<PointF>();
    //    /// <summary>
    //    /// Список параметров для прямых сегментов
    //    /// </summary> 
    //    public List<TextOnRebar> lg = new List<TextOnRebar>();   
    //    /// <summary>
    //    /// Текстовые надписи (радиусы)
    //    /// </summary>
    //    public List<TextOnArc> lg_arc_sorted = new List<TextOnArc>();
    //    /// <summary>
    //    /// Линии чертежа
    //    /// </summary>
    //    public List<TextOnArc> lg_arc = new List<TextOnArc>();
    //    /// <summary>
    //    /// Надписи над отрезками
    //    /// </summary>
    //    public List<TextOnRebar> Llg = new List<TextOnRebar>();
    //    /// <summary>
    //    /// Cписок сегментов для стержня проекта
    //    /// </summary>
    //    IList<Curve> ilc = new List<Curve>();
    //    #endregion Инициализация массивов

    //    /// <summary>
    //    /// Файл рисунка
    //    /// </summary>
    //    public Bitmap flag;
    //    //{
    //    //    get
    //    //    {
    //    //        return new Bitmap(sizeX,sizeY);
    //    //    }           
    //    //}
    //    /// <summary>
    //    /// Признак правильного создания стержня
    //    /// </summary>
    //    public bool IsRebarCorrect;
    //    /// <summary>
    //    /// Направление оси Z - перпендикулярно плоскости стержня
    //    /// </summary>
    //    Vector4 zAxis = new Vector4(XYZ.Zero);
    //    /// <summary>
    //    /// Максимум модели по оси Х
    //    /// </summary>
    //    public float maxX=1;
    //    /// <summary>
    //    /// Максимум модели по оси Y
    //    /// </summary>
    //    public float maxY=1;
    //    /// <summary>
    //    /// Минимум модели по оси Х
    //    /// </summary>
    //    public float minX = 1;
    //    /// <summary>
    //    /// Минимум модели по оси Y
    //    /// </summary>
    //    public float minY = 1; 
    //    /// <summary>
    //    /// Коэффициент масштаба
    //    /// </summary>
    //    float scale
    //    {
    //        get
    //        {
    //            float scaleX = (float)((sizeX - 2 * canva) / maxX);
    //            float scaleY = (float)(sizeY - 2 * canva) / maxY;
    //            return Math.Min(scaleX, scaleY);
    //        }
    //    }
    //    /// <summary>
    //    /// Сдвиг по оси Х
    //    /// </summary>
    //    public float moveX;
    //    /// <summary>
    //    /// Сдвиг по оси Y
    //    /// </summary>
    //    public float moveY;
    //    /// <summary>
    //    /// Текущий документ
    //    /// </summary>
    //    Document doc;
    //    /// <summary>
    //    /// Форма стержня
    //    /// </summary>
    //    RebarShape rs = null;
    //    /// <summary>
    //    /// Крюк в начале стержня
    //    /// </summary>
    //    int hook_start 
    //    {
    //        get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId().IntegerValue;}
    //    }
    //    /// <summary>
    //    /// Крюк в начале стержня
    //    /// </summary>
    //    int hook_end
    //    {
    //        get { return rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId().IntegerValue; }
    //    }

    //    /// <summary>
    //    /// Номер начального кривой (без крюка)
    //    /// </summary>
    //    int curve_start
    //    {
    //        get
    //        {
    //            if (hook_start > 0) return 2;
    //            else return 0;
    //        }
    //    }

    //    /// <summary>
    //    /// Номер конечной кривой (без крюка)
    //    /// </summary>
    //    int curve_end
    //    {
    //        get
    //        {
    //            if (hook_end > 0) return ilc.Count - 2;
    //            else return ilc.Count;
    //        }
    //    }

    //    /// <summary>
    //    /// Коэффициент минимальной длины по крюку
    //    /// </summary>
    //    int min = 5;
    //    /// <summary>
    //    /// Коэффициент максимальной длины по крюку
    //    /// </summary>
    //    int max = 15;
    //    /// <summary>
    //    /// Начальная точка
    //    /// </summary>
    //    XYZ p_initial = null;                                                       
    //    /// <summary>
    //    /// Основное направление - по оси Х
    //    /// </summary>
    //    XYZ dir_major = null;                                                        

    //    /// <summary>
    //    /// Получить параметры для дугового сегмента
    //    /// </summary>
    //    /// <param name="curves">Линия стержня</param>
    //    /// <param name="rebar">Элемент стержня</param>
    //    /// <param name="i">Текущий номер линии</param>
    //    /// <param name="value">Значение параметра</param>
    //    /// <returns>Текстовая надпись</returns> 
    //    TextOnArc GetArcSegment(IList<Curve> curves, Element rebar, int i, double value=0)
    //    {
    //        // получить диаметр стержня
    //        double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
    //        TextOnArc toa = new TextOnArc();
    //        toa.rebar = rebar;
    //        Arc arc = curves[i] as Arc;
    //        // запишем координаты, направление сегмента и радиус
    //        toa.position = arc.Center;                                         // запишем координаты центра дуги 
    //        toa.start = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;         // начальная точка сегмента
    //        toa.end = arc.Center;                                              // конечная точка сегмента    
    //        if (value == 0) toa.value = arc.Radius - d / 2;                    // запишем радиус дуги (по внутреннему контуру)
    //        else toa.value = value;
    //        // получить длину примыкающих прямых сегментов
    //        double l1, l2;
    //        l1 = l2 = 0;
    //        if ((i - 1) >= 0) l1 = curves[i - 1].Length;
    //        if ((i + 1) < curves.Count) l2 = curves[i + 1].Length;
    //        toa.nearestL = l1 + l2;
    //        return toa;
    //    }
    //    /// <summary>
    //    /// Создать чертеж
    //    /// </summary>
    //    void DrawPicture()
    //    {
    //        // готовим рисунок
    //        flag = new Bitmap(sizeX, sizeY);

    //        #region Получим точки с учетом масштаба
    //        for (int i = 0; i < line2D_L.Count; i++)
    //        {
    //            line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X * scale, line2D_L[i].p1F.Y * scale), new PointF(line2D_L[i].p2F.X * scale, line2D_L[i].p2F.Y * scale));
    //        }

    //        for (int i = 0; i < line2D.Count; i++)
    //        {
    //            line2D[i] = new Line2D(new PointF(line2D[i].p1F.X * scale, line2D[i].p1F.Y * scale), new PointF(line2D[i].p2F.X * scale, line2D[i].p2F.Y * scale));
    //        }

    //        for (int i = 0; i < pointDF.Count; i++)
    //        {
    //            pointDF[i] = new PointF(pointDF[i].X * scale, pointDF[i].Y * scale);
    //        }
    //        foreach (TextOnRebar tor in lg) { tor.UsingScale(scale); }
    //        foreach (TextOnArc tor in lg_arc) { tor.UsingScale(scale); }
    //        foreach (TextOnRebar tor in hooks) { tor.UsingScale(scale); }

    //        #endregion Получим точки с учетом масштаба
    //        SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

    //        moveX = (sizeX - 2 * canva - maxX) / 2;
    //        moveY = (sizeY - 2 * canva - maxY) / 2;

    //        //graphic = Graphics.FromImage(flag);
    //        //graphic.Clear(System.Drawing.Color.White);
    //        // отсортируем список по длине примыкающих прямых сегментов            
    //        lg_arc_sorted = lg_arc.OrderByDescending(x => x.nearestL).ToList();
    //    }
    //    /// <summary>
    //    /// Инициализация данных для сегментов, радиусов загиба
    //    /// </summary>
    //    void GetPointsAndLinesForDrawing()
    //    {
    //        float dX = 0;           // смещение центра рисунка
    //        float dY = 0;
    //        // приведем пространственную систему координат стержня в плоскую систему
    //        // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
    //        // начало системы координат принимаем в произвольной точке стержня 
           
    //        Vector4 origin = new Vector4(p_initial);
    //        // направление оси Х          
    //        Vector4 xAxis = new Vector4(dir_major);
    //        xAxis.Normalize();
    //        // направление оси Y 
    //        Vector4 yAxis = new Vector4(XYZ.Zero);
    //        yAxis = Vector4.CrossProduct(xAxis, zAxis);
    //        yAxis.Normalize();

    //        Matrix4 MatrixMain = new Matrix4(xAxis, yAxis, zAxis, origin);
    //        // после выполнения инверсии в TRANSFORM можем подставлять ГЛОБАЛЬНЫЕ КООРДИНАТЫ и получать ЛОКАЛЬНЫЕ
    //        MatrixMain = MatrixMain.Inverse();
    //        pointDF.Clear();
    //        line2D.Clear();
    //        line2D_L.Clear();
    //        // выполним расчет точек для чертежа линий арматуры
    //        foreach (Curve c in ilc)
    //        {
    //            IList<XYZ> tp = c.Tessellate();
    //            foreach (XYZ p in tp)
    //            {
    //                Vector4 p_new1 = MatrixMain.Transform(new Vector4(p));                      // получить точку в локальной системе координат
    //                PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
    //                pointDF.Add(p_new1F);                                                        // получить точку для картинки                     
    //            }

    //            tp = c.Tessellate();
    //            // получим линии чертежа арматуры
    //            for (int i = 0; i < tp.Count - 1; i++)
    //            {
    //                XYZ p1 = tp[i];
    //                XYZ p2 = tp[i + 1];
    //                Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
    //                PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
    //                Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
    //                PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
    //                Line2D line = new Line2D(p_new1F, p_new2F);
    //                line2D.Add(line);                                                            // добавить линию к списку

    //            }

    //            if (c.GetType().Name == "Arc") continue;                                        // для участка типа дуга

    //            tp = c.Tessellate();
    //            // получим линии чертежа арматуры
    //            for (int i = 0; i < tp.Count - 1; i++)
    //            {
    //                XYZ p1 = tp[i];
    //                XYZ p2 = tp[i + 1];
    //                Vector4 p_new1 = MatrixMain.Transform(new Vector4(p1));                        // получить точку в локальной системе координат
    //                PointF p_new1F = new System.Drawing.PointF(p_new1.X / unit, p_new1.Y / unit);
    //                Vector4 p_new2 = MatrixMain.Transform(new Vector4(p2));                        // получить точку в локальной системе координат
    //                PointF p_new2F = new System.Drawing.PointF(p_new2.X / unit, p_new2.Y / unit);
    //                Line2D line = new Line2D(p_new1F, p_new2F);
    //                line2D_L.Add(line);                                                            // добавить линию к списку

    //            }

    //        }

    //        pointDF = pointDF.ToList();

    //        SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);
    //        // все точки должны быть в 1 четверти
    //        if (minX < 0)
    //            for (int i = 0; i < pointDF.Count(); i++)
    //            {
    //                pointDF[i] = new PointF(pointDF[i].X - minX, pointDF[i].Y);
    //                dX = minX;
    //            }
    //        if (minY < 0)
    //            for (int i = 0; i < pointDF.Count(); i++)
    //            {
    //                pointDF[i] = new PointF(pointDF[i].X, pointDF[i].Y - minY);
    //                dY = minY;
    //            }

    //        if (minX < 0)
    //            for (int i = 0; i < line2D_L.Count(); i++)
    //            {
    //                line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X - minX, line2D_L[i].p1F.Y), new PointF(line2D_L[i].p2F.X - minX, line2D_L[i].p2F.Y));
    //            }
    //        if (minY < 0)
    //            for (int i = 0; i < line2D_L.Count(); i++)
    //            {
    //                line2D_L[i] = new Line2D(new PointF(line2D_L[i].p1F.X, line2D_L[i].p1F.Y - minY), new PointF(line2D_L[i].p2F.X, line2D_L[i].p2F.Y - minY));

    //            }

    //        if (minX < 0)
    //            for (int i = 0; i < line2D.Count(); i++)
    //            {
    //                line2D[i] = new Line2D(new PointF(line2D[i].p1F.X - minX, line2D[i].p1F.Y), new PointF(line2D[i].p2F.X - minX, line2D[i].p2F.Y));
    //            }
    //        if (minY < 0)
    //            for (int i = 0; i < line2D.Count(); i++)
    //            {
    //                line2D[i] = new Line2D(new PointF(line2D[i].p1F.X, line2D[i].p1F.Y - minY), new PointF(line2D[i].p2F.X, line2D[i].p2F.Y - minY));

    //            }

    //        // повторы будем убирать после размешения размеров
    //        // Llg = lg.ToList();

    //        // выполнить расчет координат точек для вставки текста
    //        for (int i = 0; i < lg.Count; i++) { lg[i] = RecalculatePointPosition(MatrixMain, lg[i], dX, dY); }

    //        // выполнить расчет координат точек для вставки текста (дуги)
    //        for (int i = 0; i < lg_arc.Count; i++) { lg_arc[i] = RecalculatePointPosition(MatrixMain, lg_arc[i], dX, dY); }

    //        // выполнить расчет координат точек для вставки текста (крюки)
    //        for (int i = 0; i < hooks.Count; i++) { hooks[i] = RecalculatePointPosition(MatrixMain, hooks[i], dX, dY, true); }

    //        SketchTools.GetExtremePoints(pointDF, out minX, out minY, out maxX, out maxY);

    //    }

    //    /// <summary>
    //    /// Получить координаты точек, тип надписи и угол
    //    /// </summary>
    //    /// <param name="matrix">Матрица преобразований</param>
    //    /// <param name="tr">Элемент дуги</param>
    //    /// <param name="dX">Сдвиг по координате Х</param>
    //    /// <param name="dY">Сдвиг по координате Y</param>
    //    /// <returns>Текстовая надпись для арки</returns> 
    //    TextOnArc RecalculatePointPosition(Matrix4 matrix, TextOnArc tr, float dX, float dY)
    //    {
    //        Vector4 p_new = matrix.Transform(new Vector4(tr.position));                         // получить точку в локальной системе координат
    //        tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
    //        p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
    //        tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
    //        p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
    //        tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
    //        tr.incline = InclineText.Incline;                                                     // получить направление надписи
    //        // получить угол наклона надписи в градусах
    //        double dAY = (double)(tr.endF.Y - tr.startF.Y);
    //        double dAX = (double)(tr.endF.X - tr.startF.X);
    //        if (dAX == 0) tr.angle = 0;
    //        else tr.angle = (float)Math.Atan2(dAY, dAX);
    //        return tr;
    //    }

    //    /// <summary>
    //    /// Получить координаты точек, тип надписи и угол
    //    /// </summary>
    //    /// <param name="matrix">Матрица преобразований</param>
    //    /// <param name="tr">Элемент дуги</param>
    //    /// <param name="dX">Сдвиг по координате Х</param>
    //    /// <param name="dY">Сдвиг по координате Y</param>
    //    /// <param name="hook">Признак расчета для крюков</param>
    //    /// <returns>Текстовая надпись для прямого сегмента</returns> 
    //    TextOnRebar RecalculatePointPosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook = false)
    //    {
    //        Vector4 p_new = matrix.Transform(new Vector4(tr.position));                              // получить точку в локальной системе координат
    //        tr.positionF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);          // получить точку для картинки
    //        p_new = matrix.Transform(new Vector4(tr.start));                                    // получить точку в локальной системе координат
    //        tr.startF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
    //        p_new = matrix.Transform(new Vector4(tr.end));                                    // получить точку в локальной системе координат
    //        tr.endF = new System.Drawing.PointF(p_new.X / unit - dX, p_new.Y / unit - dY);        // получить точку для картинки
    //        if (hook) return tr;
    //        // получить направление надписи
    //        if (tr.startF.X.Equals(tr.endF.X) && !tr.startF.Y.Equals(tr.endF.Y))
    //        {
    //            // дополнительный сдвиг для вертикальной надписи в сторону линии
    //            tr.incline = InclineText.Vertic; return tr;
    //        }
    //        if (!tr.startF.X.Equals(tr.endF.X) && tr.startF.Y.Equals(tr.endF.Y)) { tr.incline = InclineText.Horiz; return tr; }
    //        tr.incline = InclineText.Incline;
    //        // получить угол наклона надписи в градусах
    //        double dAY = (double)(tr.endF.Y - tr.startF.Y);
    //        double dAX = (double)(tr.endF.X - tr.startF.X);
    //        if (dAX == 0) tr.angle = 0;
    //        else tr.angle = (float)Math.Atan2(dAY, dAX);
    //        return tr;
    //    }

    //    /// <summary>
    //    /// Инициализация данных для сегментов, радиусов загиба
    //    /// </summary>
    //    bool InitialDataForSegments()
    //    {
    //        double max_length = 0;            
    //        int num_segment = 0;
    //        int num_arc = 0;
    //        for (int i = curve_start; i < curve_end; i++)
    //        {
    //            Curve c = ilc[i];

    //            // гнутые участки записываем с сохранением радиуса загиба
    //            if (c.GetType().Name == "Arc")                                     // для участка типа дуга
    //            {
    //                lg_arc[num_arc] = GetArcSegment(ilc, rebar, i, lg_arc[num_arc].value);
    //                num_arc++;
    //            }     

    //            // некоторые гнутые участки необходимо пропускать. Это стандартные гнутые участки, которые не имеют фактических сегментов
    //            if (c.GetType().Name == "Line" && lg[num_segment].arc) continue;
    //            if (c.GetType().Name == "Arc" && !lg[num_segment].arc) continue;

    //            if (max_length < c.Length && lg[num_segment].incline != InclineText.Incline)  // наклонные участки не рассматриваем как основные
    //            {
    //                max_length = c.Length;
    //                p_initial = c.GetEndPoint(0);
    //                dir_major = (c.GetEndPoint(1) - p_initial).Normalize();
    //            }

    //            if (c.GetType().Name == "Arc")                       // для участка типа дуга
    //            {
    //                lg[num_segment].arc = true;
    //                Arc arc = c as Arc;
    //                // запишем координаты и направление сегмента
    //                lg[num_segment].position = arc.Center + arc.YDirection * arc.Radius;       // запишем координаты центра дуги 
    //                lg[num_segment].start = arc.Center;                                        // начальная точка сегмента
    //                lg[num_segment].end = arc.Center + arc.XDirection;                         // конечная точка сегмента

    //            }
    //            else
    //            {
    //                // запишем координаты и направление сегмента
    //                lg[num_segment].position = (c.GetEndPoint(0) + c.GetEndPoint(1)) / 2;
    //                lg[num_segment].start = c.GetEndPoint(0);
    //                lg[num_segment].end = c.GetEndPoint(1);
    //            }

    //            num_segment++;
    //        }
    //        // проверка наличия позиций у сегментов. Если позиции нет - картинки не будет. Какое-то несоответствие. Возможно стержень 3d или самопальное семейство
    //        foreach (TextOnRebar tor in lg)
    //        {                
    //            if (tor.position == null) return false;
    //        }
    //        if (p_initial == null || dir_major == null) return false;                        // картинки не будет
    //        // Llg = lg.ToList();
    //        return true;
    //    }
    //    /// <summary>
    //    /// Разделить по типам арматурных стержней: получить кривые и вектор Z
    //    /// </summary>
    //    void DivideByTypeRebar()
    //    {
    //        ilc.Clear();                    // готовим новые линии для вычерчивания стержня
    //        Rebar rebarOne = rebar as Rebar;
    //        RebarInSystem rebarIn = rebar as RebarInSystem;
    //        // здесь выполняем 
    //        if (rebarOne != null)
    //        {
    //            doc = rebarOne.Document;
    //            // получить данные по форме стержня
    //            ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
    //            rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
    //            zAxis = new Vector4(rebarOne.GetShapeDrivenAccessor().Normal);                
    //        }
    //        if (rebarIn != null)
    //        {
    //            doc = rebarIn.Document;
    //            // получить данные по форме стержня
    //            rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
    //            ilc = rebarIn.GetCenterlineCurves(false, false, false);
    //            zAxis = new Vector4(rebarIn.Normal);
    //        }
    //        zAxis.Normalize();            
    //    }

    //    /// <summary>
    //    /// Формирование данных по участкам армирования
    //    /// </summary>
    //    void DataBySegments()
    //    {
            
    //        RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();             
    //        RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
    //        ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
    //        lg.Clear();
    //        // Цикл по сегментам в данной форме rsds.NumberOfSegments
    //        for (int i = 0; i < rsds.NumberOfSegments; i++)
    //        {
    //            TextOnRebar tor = new TextOnRebar();                                      // создаем будущую надпись над сегментом
    //            tor.rebar = rebar;                                                 // запишем текущий стержень
    //            RebarShapeSegment segment = rsds.GetSegment(i);                           // определяем сегмент

    //            IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента                

    //            foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
    //            {
    //                // получим длину сегмента
    //                RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;

    //                if (l != null)
    //                {
    //                    ElementId pid = l.GetParamId();
    //                    Element elem = doc.GetElement(pid);
    //                    foreach (Parameter pr in pset)
    //                    {
    //                        if (pr.Definition.Name == elem.Name)
    //                        {
    //                            tor.guid = pr.GUID;
    //                            // с учетом локальных особенностей
    //                            tor.value_initial = rebar.get_Parameter(pr.Definition).AsDouble();
    //                            tor.value = tor.value_initial + GetAddValue(template, tor.guid, rebar as Rebar);
    //                            if (tor.value <= 0) tor.value = tor.value_initial;
    //                            if (tor.value > 0) break;
    //                        }
    //                    }

    //                    // запишем для контроля имя параметра
    //                    tor.name = elem.Name;                                                                          // добавим метку
    //                    // определяем, что данный сегмент является дугой
    //                    try
    //                    {
    //                        RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeDefaultBend");
    //                        tor.arc = true;
    //                    }
    //                    catch { }
    //                    try
    //                    {
    //                        RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendRadius");
    //                        tor.arc = true;
    //                    }
    //                    catch { }

    //                    try
    //                    {
    //                        RebarShapeConstraint bend = ILrsc.First(x => x.GetType().Name == "RebarShapeConstraint180DegreeBendArcLength");
    //                        tor.arc = true;
    //                    }
    //                    catch { }
    //                    continue;
    //                }

    //                // определяем, что данный участок наклонный
    //                RebarShapeConstraintProjectedSegmentLength proj = rsc as RebarShapeConstraintProjectedSegmentLength;
    //                // работаем с вертикальной проекцией текущего участка
    //                if (proj != null && tor.guidV.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.V), 0) == 1)
    //                {
    //                    tor.incline = InclineText.Incline;
    //                    ElementId pid = proj.GetParamId();
    //                    Element elem = doc.GetElement(pid);
    //                    foreach (Parameter pr in pset)
    //                    {
    //                        if (pr.Definition.Name == elem.Name)
    //                        {
    //                            tor.guidV = pr.GUID;
    //                            tor.valueV = rebar.get_Parameter(pr.Definition).AsDouble();
    //                            if (tor.valueV > 0) break;
    //                        }
    //                    }
    //                    tor.nameV = elem.Name;                                                                          // добавим метку                       
    //                    continue;

    //                }

    //                if (proj != null && tor.guidH.ToString() == "00000000-0000-0000-0000-000000000000" && Math.Round(Math.Abs(proj.Direction.U), 0) == 1)
    //                {
    //                    tor.incline = InclineText.Incline;
    //                    ElementId pid = proj.GetParamId();
    //                    Element elem = doc.GetElement(pid);

    //                    foreach (Parameter pr in pset)
    //                    {
    //                        if (pr.Definition.Name == elem.Name)
    //                        {
    //                            tor.guidH = pr.GUID;
    //                            tor.valueH = rebar.get_Parameter(pr.Definition).AsDouble();
    //                            if (tor.valueH > 0) break;

    //                        }
    //                    }
    //                    tor.nameH = elem.Name;                                                                          // добавим метку

    //                    continue;
    //                }
    //            }

    //            // если проекция наклонного участока практически равна базовому участку, то наклонный не показываем
    //            if (tor.value_str == tor.valueH_str) tor.valueH = 0;
    //            if (tor.value_str == tor.valueV_str) tor.valueV = 0;
    //            lg.Add(tor);   // внесем сегмент в общий список
    //        }

    //    }



        

    //    /// <summary>
    //    /// Получить дополнительное локальное значение
    //    /// </summary>
    //    /// <param name="template">Шаблон проекта</param>
    //    /// <param name="parameter">Параметр</param>        
    //    /// <returns>Дополнительное значение</returns>         

    //    static double GetAddValue(Template template, Guid parameter, Element rebar)
    //    {
    //        if (template == Template.Other) return 0;
    //        string form = "";
    //        double diam = 0;
    //        Rebar rebarOne = rebar as Rebar;
    //        RebarInSystem rebarIn = rebar as RebarInSystem;

    //        // получить менеджер текущего стержня
    //        if (rebarOne != null)
    //        {
    //            form = rebarOne.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
    //            diam = rebarOne.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
    //        }
    //        if (rebarIn != null)
    //        {
    //            form = rebarIn.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsValueString();   // форма стержня
    //            diam = rebarIn.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
    //        }

    //        LegGuid guid = new LegGuid();

    //        switch (form)
    //        {

    //            case "10":
    //                // минус два диаметра - для получения внутреннего размера хомута
    //                return -2 * diam;
    //            case "26":
    //                // минус два диаметра - для получения внутреннего размера хомута
    //                if (parameter.Equals(guid.A)) return -2 * diam;
    //                if (parameter.Equals(guid.D)) return -2 * diam;
    //                break;
    //            default:
    //                return 0;
    //        }
    //        return 0;
    //    }

    //    /// <summary>
    //    /// Изменить параметры стержня
    //    /// </summary>
    //    void ChangeParametersRebar()
    //    {
            
             

             
            

             

    //        // попытаемся обойтись без транзакций
    //        //transaction.Commit();     // запишем исходное состояние
    //        //FailureHandlingOptions options = transaction.GetFailureHandlingOptions();
    //        //FailurePreproccessor preproccessor = new FailurePreproccessor();
    //        //options.SetFailuresPreprocessor(preproccessor);
    //        //transaction.SetFailureHandlingOptions(options);
    //        //transaction.Start();
            
    //        // failureOptions.SetFailuresPreprocessor(new HideNewTypeAssembly());

    //        SubTransaction sudst = new SubTransaction(doc);
    //        sudst.Start();

    //        if (hooks.Count > 0)   // при наличии крюков - масштабируем по крюкам
    //        {
             
    //            // получить максимальную длину крюка
    //            double max_hook = hooks.Min(x => x.value);
    //            double max_segment = lg.Max(x => x.value);
    //            double coeff = (max * max_hook) / max_segment;
    //            if (max_segment == 0) coeff = 1;
    //            coeff = coeff / coef_hook;
    //            // изменим параметры
    //            int i=0;
    //            foreach (TextOnRebar tor in lg)
    //            {
    //                // double coeff = tor.value / max_segment * coef_hook;
                         
    //                double value = tor.value * coeff;
    //                //if (value < min * max_hook) value = min * max_hook * coef_hook;
    //                //if (value > max * max_hook) value = max * max_hook * coef_hook;
    //                if (tor.dialog) value = value * coef[i];                              // изменяем только для параметров включенных в диалог
    //                else { i++; continue; }
    //                SketchTools.SetParameter(rebar, tor.guid, value); 
    //                // изменить длины проекций
    //                if (tor.valueH > 0)
    //                {
    //                    value = tor.valueH * coeff;
    //                    //if (value < min * max_hook) value = min * max_hook * coef_hook;
    //                    //if (value > max * max_hook) value = max * max_hook * coef_hook;
    //                    if (tor.dialog)  value = value * coef[i];
    //                    SketchTools.SetParameter(rebar, tor.guidH, value); 
    //                }
    //                // изменить длины проекций
    //                if (tor.valueV > 0)
    //                {
    //                    value = tor.valueV * coeff;
    //                    //if (value < min * max_hook) value = min * max_hook * coef_hook;
    //                    //if (value > max * max_hook) value = max * max_hook * coef_hook;
    //                    if (tor.dialog) value = value * coef[i];
    //                    SketchTools.SetParameter(rebar, tor.guidV, value);
    //                }
    //                i++;
    //            }

    //            // doc.Regenerate();
    //        }
    //        else
    //        {
    //            double max_segment = lg.Max(x => x.value);
    //            // изменим параметры
    //            int i=0;
    //            foreach (TextOnRebar tor in lg)
    //            {
                           
    //                double new_value = tor.value;
    //                if (new_value < max_segment / 4) new_value = max_segment / 4;
    //                if (tor.dialog) new_value = new_value * coef[i];   // изменяем только для параметров включенных в диалог
    //                else { i++; continue; }
    //                SketchTools.SetParameter(rebar, tor.guid, new_value); 
    //                i++;
    //            }

    //            // doc.Regenerate();
    //        }

           
    //            doc.Regenerate();                
    //            DivideByTypeRebar();       // получить новые кривые 
    //            GetInfoAboutHooks();       // обновить информацию по крюкам
    //            sudst.RollBack();

    //            // восстановить старые значения сегментов
    //            foreach (TextOnRebar tor in lg)
    //            {
    //                SketchTools.SetParameter(rebar, tor.guid, tor.value_initial);

    //            }
    //            doc.Regenerate();
           

    //            // transaction.RollBack();

    //            //if (!preproccessor.status) status = false;
    //            //else
    //            //{
    //            //GetInfoAboutHooks();       // обновить информацию по крюкам 
    //            //status = true;
    //            //}
    //            //transaction.Start();

               
    //    }

    //    void GetInfoAboutHooks()
    //    {
    //        // обновить информацию по крюкам
    //        hooks.Clear();
             
    //        // получить информацию по крюкам (начало)
    //        if (hook_start  > 0)
    //        {                
    //            hooks.Add(GetHookStart(ilc, rebar));                                                    // добавим информацию по крюку
    //        }
    //        if (hook_end  > 0)
    //        {                
    //            hooks.Add(GetHookEnd(ilc, rebar));                                                     // добавим информацию по крюку
    //        }
    //    }

    //    /// <summary>
    //    /// Получить параметры для начального крюка
    //    /// </summary>
    //    /// <param name="curves">Линии стержня</param>
    //    /// <param name="rebar">Элемент стержян</param>
    //    /// <returns>Текстовая надпись</returns> 
    //    static TextOnRebar GetHookStart(IList<Curve> curves, Element rebar)
    //    {
    //        Curve c_straight = curves[0];
    //        Curve c_arc = curves[1];
    //        TextOnRebar tor = new TextOnRebar();
    //        tor.rebar = rebar;
    //        tor.position = c_straight.GetEndPoint(0);
    //        tor.value = c_straight.Length;
    //        tor.start = c_arc.GetEndPoint(0);
    //        tor.end = c_arc.GetEndPoint(1);
    //        tor.arc = true;
    //        return tor;
    //    }
    //    /// <summary>
    //    /// Получить параметры для конечного крюка
    //    /// </summary>
    //    /// <param name="curves">Линии стержня</param>
    //    /// <param name="rebar">Элемент стержян</param>
    //    /// <returns>Текстовая надпись</returns> 
    //    static TextOnRebar GetHookEnd(IList<Curve> curves, Element rebar)
    //    {
    //        Curve c_straight = curves.Last();
    //        Curve c_arc = curves[curves.Count - 2];
    //        TextOnRebar tor = new TextOnRebar();
    //        tor.rebar = rebar;
    //        tor.position = c_straight.GetEndPoint(1);
    //        tor.value = c_straight.Length;
    //        tor.start = c_arc.GetEndPoint(1);
    //        tor.end = c_arc.GetEndPoint(0);
    //        tor.arc = true;
    //        return tor;
    //    }


    //}

    /// <summary>
    /// Надписи над стержнями 
    /// </summary>
    public class TextOnArc
    {
        /// <summary>
        /// Размер надписи
        /// </summary>
        public SizeF size;
        /// <summary>
        /// Стержень для которого выполняется надпись
        /// </summary>
        public Element rebar;  
        /// <summary>
        /// Начальная точка сегмента
        /// </summary>
        public XYZ start;
        /// <summary>
        /// Конечная точка сегмента
        /// </summary>
        public XYZ end;
        /// <summary>
        /// Начальная точка сегмента
        /// </summary>
        public PointF startF;
        /// <summary>
        /// Конечная точка сегмента
        /// </summary>
        public PointF endF; 
        /// <summary>
        /// Позиция параметра
        /// </summary>
        public XYZ position;
        /// <summary>
        /// Позиция параметра
        /// </summary>
        public PointF positionF;
        /// <summary>
        /// Признак поиска нового положения для текста
        /// </summary>
        public bool is_move_position;
        /// <summary>
        /// Позиция параметра после возможного поворота
        /// </summary>
        public PointF positionF_rotate
        {
            get
            {
                // координаты новой точки после поворота на угол "angle_rotate"
                float X_new = (float)(positionF.X * Math.Cos(angle) + positionF.Y * Math.Sin(angle));
                float Y_new = (float)(-positionF.X * Math.Sin(angle) + positionF.Y * Math.Cos(angle));                 
                return new PointF(X_new, Y_new);
                 
            }
        }
        /// <summary>
        /// Длина примыкающих прямых участков
        /// </summary>
        public double nearestL = 0;
        /// <summary>
        /// Значение параметра
        /// </summary>
        public double value = 0;
        /// <summary>
        /// Округленное строковое значение параметра
        /// </summary>
        public string value_str
        {
            get
            {
                string v = SketchTools.GetRoundLenghtSegment(rebar, value);
                if (v.Length > 2)
                {
                    if (v.Substring(0, 2) == "0.") v = v.Substring(1);
                }
                return v;
            }
        }
         
        /// <summary>
        /// Признак дуги (арки)
        /// </summary>
        public bool arc = true;
        /// <summary>
        /// Угол наклона надписи в градусах
        /// </summary>
        public float angle_grad
        {
            get
            {
                return (float)(180 / Math.PI * angle);
            }
        }
        /// <summary>
        /// Угол наклона надписи
        /// </summary>
        public float angle = 0;
        /// <summary>
        /// Признак наклона надписи
        /// </summary>
        public InclineText incline = InclineText.Incline;       // по умолчанию

        /// <summary>
        /// Получить координаты с учетом масштаба
        /// </summary>
        public void UsingScale(float scale)
        {
            positionF = new PointF(positionF.X * scale, positionF.Y * scale);
            startF = new PointF(startF.X * scale, startF.Y * scale);
            endF = new PointF(endF.X * scale, endF.Y * scale);
        }
      

    }

   

}
