using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Collections;

using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing;

namespace SketchReinforcement
{

    /// <summary>
    /// Implements the Revit add-in interface IExternalCommand
    /// </summary>    
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    public partial class SketchCommand : IExternalCommand
    {        
        Document doc;
        /// <summary>
        /// Общие данные. Данные для диалога
        /// </summary>
        DataForm dataform = new DataForm();
        /// <summary>
        /// Общий список стержней проекта
        /// </summary>
        List<Element> all_rebars = new List<Element>();
        /// Cписок стержней для создания текущих эскизов
        /// </summary>
        List<Element> all_rebar = new List<Element>();
        /// <summary>
        /// Список ID эскизов для всех стержней проекта
        /// </summary>
        List<int> id_sketchs = new List<int>();
        /// <summary>
        /// Список ID эскизов для текущей выборки
        /// </summary>
        List<int> id_sketch = new List<int>();
        /// <summary>        
        /// Cписок существующих images в всем документе
        /// </summary>
        List<Element> all_images = new List<Element>();
        /// <summary>        
        /// Cписок images для удаления
        /// </summary>
        List<ElementId> images_Id_delete = new List<ElementId>();
        /// <summary>
        /// Список наименований эскизов
        /// </summary>         
        List<string> name_skeths = new List<string>();
        /// <summary>
        /// Сортированный список стержней для создания эскизов
        /// </summary>
        SortedList<string, CodeImage> sortedImages = new SortedList<string, CodeImage>();
        /// <summary>
        /// Путь к папке эскизов
        /// </summary>
        public static string FolderImages = "";
        /// <summary>
        /// Хранилище ID эскизов
        /// </summary>
        DataStorage ds;
        /// <summary>
        /// Существующие ID эскизы записанные в базе проекте
        /// </summary>
        IList<int> SketchIds=new List<int>();
        /// <summary>
        /// Новые (обновленные) ID эскизы 
        /// </summary>
        IList<int> SketchIdsNew = new List<int>();
        /// <summary>
        /// Схема хранения параметров
        /// </summary>
        Schema schema_sketchs;
        Guid SchemaSketchs = new Guid("72AB8884-3F6C-4319-91A3-17F40D93F9FF");
        /// <summary>
        /// Максимальная длина стержня
        /// </summary>
        public static double max_length = 0;
        // Tип шаблона
        Template template = Template.Other;


        //public StreamWriter writer = new StreamWriter("E:\\sketch.txt");
        //public Stopwatch stopWatch = new Stopwatch();
        //public DateTime Start;
        //public DateTime Stoped; //Время окончания
        //public TimeSpan Elapsed = new TimeSpan(); // Разница



        public virtual Result Execute(ExternalCommandData commandData
           , ref string message, ElementSet elements)
        {
            string VN = commandData.Application.Application.VersionNumber;

#if RVT2024

            if (Convert.ToInt32(VN) > 2024)
            {
                MessageBox.Show(Resourses.Strings.Texts.VersionNumber);
                return Result.Cancelled;
            }
#endif
            Document document = commandData.Application.ActiveUIDocument.Document;
            dataform.units = document.GetUnits();

            doc = commandData.Application.ActiveUIDocument.Document;

            // получить стержни проекта
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            all_rebars = collector.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().ToList();

            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
            List<Element> rebar_in_system = collector2.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().ToList();

            foreach (Element eid in rebar_in_system)
            {
                all_rebars.Add(eid);
            }

            if (all_rebars.Count() == 0) return Result.Failed;   // нет армирования в проекте


            // получить список наименований эскизов
            List<string> name_skeths = new List<string>();
            foreach (Element e in all_rebars)
            {
                name_skeths.Add(e.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsValueString());
            }

            // получить список ID эскизов             
            foreach (Element e in all_rebars)
            {
                id_sketchs.Add(e.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().IntegerValue);
            }

            // получить список РАЗДЕЛОВ арматуры                  
            //IEnumerable<string> razdels =
            //    all_rebars.Select(x =>
            //    x.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString()).Distinct();

            IEnumerable<string> razdels =
                 (from r in all_rebars
                  orderby r.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString() ascending
                  select r.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString()).Distinct();
        
            

            // записать список в данные приложения
            foreach (string s in razdels)
            {
                if (s.Length > 0) dataform.Razdels.Add(s);
            }

            // получить текущий вид. Спецификация, листы и пр. не рассматривается в диалоге
            if (doc.ActiveView.Category == null)
            {
                dataform.EnabledByView = false;
                dataform.EnabledBySelect = false;
            }
            else
            {
                if (doc.ActiveView.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Views)
                {
                    dataform.EnabledByView = false;
                    dataform.EnabledBySelect = false;
                }
                if (doc.ActiveView.GetType().Name == "ViewDrafting")
                {
                    dataform.EnabledByView = false;
                    dataform.EnabledBySelect = false;
                }
            }

            // получить данные по шрифтам
            //ElementId font_default = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            //dataform.Font_default_name = doc.GetElement(font_default).get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
                        

            // прочитать данные из проекта: шрифт, размер шрифта, макс длина, коэфф.диаметра, признак наличия размеров
            dataform = GetDataProject(dataform);
            
            System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
            System.Drawing.FontFamily[] families = ifc.Families;
            int Font_default = -1;
            // проверим актуальность шрифта
            for (int i = 0; i < ifc.Families.Length; i++)
            {
                if (ifc.Families[i].Name == dataform.Font_default_name)
                {
                    Font_default = i;
                    break;
                }
            }
            if (Font_default == -1)
            {
                ElementId font_default = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                dataform.Font_default_name = doc.GetElement(font_default).get_Parameter(BuiltInParameter.TEXT_FONT).AsString();
            }

            SelectRebars initial_form = new SelectRebars(dataform);
                      

            if (initial_form.ShowDialog() == DialogResult.OK)
            {
                SketchReinforcementApp.drawFont30 = new System.Drawing.Font(dataform.Font_default_name, dataform.Font_size); //  30);
                SketchReinforcementApp.drawFont = new System.Drawing.Font(dataform.Font_default_name, dataform.Font_size); // 48);
                SketchReinforcementApp.drawFontH = new System.Drawing.Font(dataform.Font_default_name, dataform.Font_size); // 36);
                SketchReinforcementApp.drawFontG = new System.Drawing.Font(dataform.Font_default_name, dataform.Font_size); // 26);
                SketchReinforcementApp.shift_font = 15.0f/28.0f *(48.0f- dataform.Font_size); // 48.0f -  dataform.Font_size;
                SketchReinforcementApp.shift_font_arc = 20.0f / 28.0f * (dataform.Font_size -20.0f);
                SketchReinforcementApp.size_font = dataform.Font_size;
                if (dataform.mode_shape) max_length = dataform.Max_Lenght;
                else max_length = 0.0;

                if (!dataform.IspathFolder) dataform.pathFolder = "";

                FolderImages = GetSetFolderImages(doc, dataform.pathFolder);
                if (FolderImages == "") return Result.Failed;

                // рассчитать реальную высоту и смещение для шрифта
                CalculateFontParameters();
                
                // Start = DateTime.Now;
                // настройка параметров проекта

                // установить тип шаблона
                template = GetTemplate(all_rebars[0]);

                   // получить список существующих images в всем документе
                   FilteredElementCollector collector_all_images = new FilteredElementCollector(doc);
                   all_images = collector_all_images.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();
                        

                   if (dataform.AllRebars)       // выполняем обработку всех стержней
                   {

                    //DateTime Start = DateTime.Now;
                    //DateTime Stoped = DateTime.Now;
                    //TimeSpan Elapsed = Stoped.Subtract(Start);

                    //using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
                    //{
                    //ICollection<ElementId> elementIds = new List<ElementId>();
                    // t.Start();

                    //// удалить все images из проекта
                    //Start = DateTime.Now;
                    //foreach (Element el in all_images)
                    //{
                    //    ImageType it = el as ImageType;
                    //    // if (!it.Name.Contains("_sketch.png")) continue;
                    //    // doc.Delete(el.Id);
                    //    elementIds.Add(el.Id);
                    //}
                    //doc.Delete(elementIds);
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("Время удаления эскизов");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));


                    //for (int i=0; i<30000; i++)
                    //{                             
                    //    SketchTools.SetParameter(all_rebars[i], BuiltInParameter.ALL_MODEL_IMAGE, new ElementId(-1));
                    //}
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("Время выполнения расчетов 30000 элементов");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds /30000));

                    //Start = DateTime.Now;
                    //t.Commit();
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("Время сохранения 30000 элементов");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds/30000));


                    //t.Start();
                    //for (int i = 100; i < 300; i++)
                    //{
                    //    SketchTools.SetParameter(all_rebars[i], BuiltInParameter.ALL_MODEL_IMAGE, new ElementId(-1));
                    //}
                    //Start = DateTime.Now;
                    //t.Commit();
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("Время выполнения 200 элементов");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds/200));


                    //t.Start();
                    //for (int i = 300; i < 1300; i++)
                    //{
                    //    SketchTools.SetParameter(all_rebars[i], BuiltInParameter.ALL_MODEL_IMAGE, new ElementId(-1));
                    //}
                    //Start = DateTime.Now;
                    //t.Commit();
                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("Время выполнения 1000 элементов");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds/1000));
                    //    writer.Close();
                    //    return Result.Succeeded;
                    //}

                    // получить список стержней для создания эскиза
                    all_rebar = all_rebars;

                    // получить список ID эскизов текущей выбоки - сейчас это все равно все стержни             
                    foreach (Element e in all_rebar)
                    {
                        id_sketch.Add(e.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().IntegerValue);
                    }

                    using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
                    {
                        t.Start();

                        // при включении флага - удаляем все эскизв из проекта
                        if (dataform.IsDeleteSketch)
                        {
                            foreach (Element e in all_rebars)
                            {
                                ElementId elmentId = e.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId();
                                if(doc.GetElement(elmentId)!=null)
                                images_Id_delete.Add(e.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId());
                            }
                            doc.Delete(images_Id_delete);
                            t.Commit();
                            return Result.Succeeded;
                        }

                        // получить существующие ID эскизов из базы проекта
                        // при необходимости создать базу для хранения
                        GetStorageIdPictures();

                        // GetStorageIdPictures(all_images);  


                        // получить сортированный список стержней для создания эскизов
                        // GetSortListRebars(doc, all_rebar, sortedImages);
                        GetSortListRebars();

                        // получить эскизы                        
                        // SketchReinforcement(doc, sortedImages, all_rebar, null, dataform, template, all_images);                         
                        SketchReinforcement();
                        // записать полученные эскизы
                        // SketchIdsNew = WriteImages(doc, all_rebar, sortedImages);
                        WriteImages();
                        WriteIdImagesToProject();
                        t.Commit();                         
                        RemoveSketch();    // удалить ненужные эскизы                        
                   }

                    return Result.Succeeded;                     
                }

                   if (dataform.ByRazdel)       // выполняем обработку выбранного РАЗДЕЛА
                   {                    

                    //DateTime Start = DateTime.Now;
                    //DateTime Stoped = DateTime.Now;
                    //TimeSpan Elapsed = Stoped.Subtract(Start);
                  

                    foreach (string Razdel in dataform.SelectRazdels)
                    {

                        // элементы, включенные в группу не обрабатываются
                        FilteredElementCollector collectorV = new FilteredElementCollector(doc);
                        all_rebar = collectorV.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().ToList();

                        FilteredElementCollector collector2V = new FilteredElementCollector(doc);
                        List<Element> rebar_in_systemV = collector2V.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().ToList();

                        foreach (Element eid in rebar_in_systemV)
                        {
                            all_rebar.Add(eid);
                        }
                        // убрать все, что не относится к выбранному разделу
                        all_rebar.RemoveAll(x => x.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString() != Razdel);
                         
                        //// разделим все на 2 потока
                        //List<Element> all_rebar1 = new List<Element>();
                        //List<Element> all_rebar2 = new List<Element>();
                        //int count_all = all_rebar.Count() / 2;

                        //for (int i=0;i<all_rebar.Count();i++)
                        //{
                        //    if(i<=count_all)
                        //    {
                        //        all_rebar1.Add(all_rebar[i]);
                        //    }
                        //    else
                        //    {
                        //        all_rebar2.Add(all_rebar[i]);
                        //    }
                        //}

                        using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
                        {
                            t.Start();
                           
                            // GetStorageIdPictures(all_images);                                       // получить существующие ID эскизов                                                                                                   
                            GetStorageIdPictures();                                       // получить существующие ID эскизов                                                                                                   
                            GetSortListRebars();                                          // получить сортированный список стержней для создания эскизов

                            // получить эскизы
                            // Start = DateTime.Now;
                            // SketchReinforcement(doc, sortedImages, all_rebar, all_rebars, dataform, template, all_images, null);
                            SketchReinforcement();
                            //Stoped = DateTime.Now;
                            //Elapsed = Stoped.Subtract(Start);
                            //if (writer != null) writer.WriteLine("Эскизы подготовлены");
                            //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));

                            //DataThread dataThread = new DataThread();
                            //dataThread.doc = doc;
                            //dataThread.sortedImages = sortedImages;
                            //dataThread.all_rebar = all_rebar1;
                            //dataThread.all_rebars = all_rebars;
                            //dataThread.template = template;
                            //dataThread.all_images = all_images;
                            //Thread mythread = new Thread(fornewthread);
                            //mythread.Priority = ThreadPriority.Highest;
                            //mythread.Start(dataThread);

                            //DataThread dataThread2 = new DataThread();
                            //dataThread2.doc = doc;
                            //dataThread2.sortedImages = sortedImages;
                            //dataThread2.all_rebar = all_rebar2;
                            //dataThread2.all_rebars = all_rebars;
                            //dataThread2.template = template;
                            //dataThread2.all_images = all_images;
                            //Thread mythread2 = new Thread(fornewthread);
                            //mythread2.Priority = ThreadPriority.Highest;
                            //mythread2.Start(dataThread2);

                            // записать полученные эскизы
                            // Start = DateTime.Now;
                            WriteImages();
                            WriteIdImagesToProject();
                            t.Commit();
                            //Stoped = DateTime.Now;
                            //Elapsed = Stoped.Subtract(Start);
                            //if (writer != null) writer.WriteLine("Время выполнения процедуры создания раздела; число стержней - " + all_rebar.Count().ToString()+ " -- "+ (Elapsed.TotalSeconds/ all_rebar.Count()).ToString()); 
                            //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                        }                      
                    }

                   

                    // Start = DateTime.Now;

                    RemoveSketch();   // удаляем ненужные эскизы

                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //if (writer != null) writer.WriteLine("Время выполнения процедуры удаления эскизов");
                    //if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));

                    //writer.Close();

                    
                    return Result.Succeeded;
                   }

                if (dataform.ByView)       // выполняем обработку стержней текущего вида
                {
                    // элементы, включенные в группу не обрабатываются
                    FilteredElementCollector collectorV = new FilteredElementCollector(doc,doc.ActiveView.Id);
                    all_rebar = collectorV.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().ToList();

                    FilteredElementCollector collector2V = new FilteredElementCollector(doc, doc.ActiveView.Id);
                    List<Element> rebar_in_systemV = collector2V.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().ToList();

                    foreach (Element eid in rebar_in_systemV)
                    {
                        all_rebar.Add(eid);
                    }


                    using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
                    {
                        t.Start();
                        // GetStorageIdPictures(all_images);                                       // получить существующие ID эскизов
                        GetStorageIdPictures();                                       // получить существующие ID эскизов
                        // получить сортированный список стержней для создания эскизов
                        GetSortListRebars();
                        // получить эскизы
                        // SketchReinforcement(doc, sortedImages, all_rebar, all_rebars, dataform, template, all_images);
                        SketchReinforcement();
                        // записать полученные эскизы
                        WriteImages();
                        WriteIdImagesToProject();
                        t.Commit();
                    }
                    RemoveSketch();   // удаляем ненужные эскизы
                    return Result.Succeeded;
                }
                if (dataform.UpdateSingleRebar)       // выполняем обработку выбранных стержней
                {
                    IList<Reference> reference = null;
                    try
                    {
                        // выполнить выбор арматурного стержня          
                        reference = commandData.Application.ActiveUIDocument.Selection.PickObjects(ObjectType.Element,
                                                                                      new TargetElementSelectionFilter(),
                                                                                      Resourses.Strings.Texts.SelectRebar);
                    }
                    catch { }

                    if (reference == null) return Result.Cancelled;
                    if (reference.Count == 0) return Result.Cancelled;

                    // List<Element> all_rebar_select = new List<Element>();


                    foreach (Reference eid in reference)
                    {
                        Element element = doc.GetElement(eid) as Element;

                        //if (element.GroupId.IntegerValue > 0) continue;
                        //// стержни свободной формы пропускаем
                        //if (element.get_Parameter(BuiltInParameter.REBAR_GEOMETRY_TYPE).AsInteger() == 1) continue;

                        all_rebar.Add(element);

                        //// выбрать все подобные стержни
                        //FilteredElementCollector collectorS = new FilteredElementCollector(doc);
                        //if (element.GetType().Name == "Rebar")
                        //{
                        //    all_rebar = collectorS.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().Where(
                        //        x => x.GroupId.IntegerValue <= 0 &&
                        //        doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == doc.GetElement(element.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() &&
                        //        doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == doc.GetElement(element.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name &&
                        //        x.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble() == element.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble() &&
                        //        Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == Math.Round(element.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0)).ToList();
                        //}
                        //if (element.GetType().Name == "RebarInSystem")
                        //{
                        //    all_rebar = collectorS.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().Where(
                        //        x => x.GroupId.IntegerValue <= 0 &&
                        //        doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == doc.GetElement(element.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() &&
                        //        doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == doc.GetElement(element.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name &&
                        //        x.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble() == element.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble() &&
                        //        Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == Math.Round(element.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0)).ToList();
                        //}

                        //foreach (Element el in all_rebar)
                        //{
                        //    all_rebar_select.Add(el);
                        //}


                    }

                    if (all_rebar.Count == 0) return Result.Succeeded;

                    using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
                    {
                        t.Start();
                        // GetStorageIdPictures(all_images);                                       // получить существующие ID эскизов
                        GetStorageIdPictures();                                       // получить существующие ID эскизов
                        // получить сортированный список стержней для создания эскизов
                        GetSortListRebars();
                        // получить эскизы
                        // SketchReinforcement(doc, sortedImages, all_rebar, all_rebars, dataform, template, all_images);
                        SketchReinforcement();
                        // записать полученные эскизы
                        WriteImages();
                        WriteIdImagesToProject();
                        t.Commit();
                    }
                    RemoveSketch();   // удаляем ненужные эскизы
                    return Result.Succeeded;
                }
            }

            return Result.Cancelled;            
        }

        /// <summary>
        /// Получить данные по шрифту
        /// </summary>        
        void CalculateFontParameters()
        {
            Bitmap temp_font = new Bitmap(100,100);
            Graphics graphic = Graphics.FromImage(temp_font);
            graphic.Clear(System.Drawing.Color.Transparent);             
            graphic.DrawString("0", SketchReinforcementApp.drawFont, Brushes.Black,0.0f,0.0f); 

            // найти начало текста по высоте
            for (int y=0; y<100; y++)
            {
                for(int x=0; x<100; x++)
                {
                    System.Drawing.Color color= temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        dataform.shiftUp = y;
                        goto ToMaxY;
                    }
                }
            }
            ToMaxY:
            // найти конец текста по высоте
            for (int y = 99; y > 0; y--)
            {
                for (int x = 0; x < 100; x++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        dataform.shiftDown = y;
                        goto ToMinX;
                    }
                }
            }
            ToMinX:
            // найти начало текста по ширине
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        dataform.shiftLeft = x;
                        goto ToMinXVert;
                    }
                }
            }

            ToMinXVert:
            // temp_font.Save("e:\\proba.prn");

            graphic.Clear(System.Drawing.Color.Transparent);
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            graphic.DrawString("0", SketchReinforcementApp.drawFont, Brushes.Black, 0.0f, 0.0f, drawFormat);

            // найти начало текста по высоте
            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        dataform.shiftUpVertical = y;
                        goto ToMinUpVertical;
                    }
                }
            }

            ToMinUpVertical:

            // найти начало текста по ширине
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    System.Drawing.Color color = temp_font.GetPixel(x, y);
                    if (color.A > (byte)0)
                    {
                        dataform.shiftLeftVertical = x;
                        return;
                    }
                }
            }



            return;
        }

        /// <summary>
        /// Получить данные из проекта
        /// </summary>   
        DataForm GetDataProject(DataForm dataform)
        {
            // получить схему хранения данных
            schema_sketchs = Schema.Lookup(SchemaSketchs);
            if (null == schema_sketchs) return dataform;

            // проверяем наличие ранее созданного вида набора данных
            FilteredElementCollector collectorV = new FilteredElementCollector(doc);
            collectorV.WherePasses(new ElementClassFilter(typeof(DataStorage)));
            var storage = from element in collectorV where element.Name == "SketchStorage" select element;
            if (storage.Count() == 0) return dataform;

            ds = storage.First() as DataStorage;
            // получить данные по схеме хранения
            Entity ent_storage = ds.GetEntity(schema_sketchs);
            if (ent_storage == null) return dataform;            

            if (ent_storage.Schema != null)
            {
                string name_font_project = "Mipgost";
                name_font_project = ent_storage.Get<string>("Font");
                if (name_font_project.Length > 0) dataform.Font_default_name = name_font_project;

                int font_size = 48;
                font_size = ent_storage.Get<int>("FontSize");
                if (font_size > 0) dataform.Font_size = (float) font_size;

                int index_color = 0;
                index_color = ent_storage.Get<int>("Index_color");
                dataform.index_color = index_color;

                int out_border = 10;
                out_border = ent_storage.Get<int>("OutBorder");
                if (out_border > 0) dataform.border = out_border;

                int coef_diam = 50;
                coef_diam = ent_storage.Get<int>("CoefDiam");
                if (font_size > 0) dataform.coef_diam = coef_diam;

                dataform.Font_shift = ent_storage.Get<int>("ShiftFromLine");

                double max_lenth = 39.37008;
                string sss = ent_storage.Get<string>("MaxLength");
                max_lenth = Convert.ToDouble(sss); // ,SpecTypeId.Custom));
                if (max_lenth > 0) dataform.Max_Lenght = max_lenth;

                bool is_dim_lines = false;
                is_dim_lines = ent_storage.Get<bool>("IsDimLines");
                dataform.Is_dim_lines = is_dim_lines;

                bool is_all_razdel = false;
                is_all_razdel = ent_storage.Get<bool>("IsAllRazdel");
                dataform.IsAllRazdel = is_all_razdel;

                bool back_ground_color = true;
                back_ground_color = ent_storage.Get<bool>("BackGroundColor");
                dataform.BackGroundColor = back_ground_color;

                bool mode_shape = true;
                mode_shape = ent_storage.Get<bool>("IsModeShape");
                dataform.mode_shape = mode_shape;

                bool show_angle = false;
                show_angle = ent_storage.Get<bool>("IsShowAngle");
                dataform.Angle = show_angle;

                bool hooks_length = false;
                hooks_length = ent_storage.Get<bool>("IsShowHooksLength");
                dataform.HooksLength = hooks_length;
                string pathFolder = "";
                pathFolder = ent_storage.Get<string>("PathFolder");
                dataform.pathFolder = pathFolder;

            }
            return dataform;
        }

        /// <summary>
        /// Получить ID картинки проекта
        /// </summary>   
        // void GetStorageIdPictures(List<Element> all_images)
        void GetStorageIdPictures()
        {
            // получить схему хранения данных
            schema_sketchs = Schema.Lookup(SchemaSketchs);
            if (null == schema_sketchs) PreparedSchemaSketchs();

            // проверяем наличие ранее созданного вида набора данных
            FilteredElementCollector collectorV = new FilteredElementCollector(doc);
            collectorV.WherePasses(new ElementClassFilter(typeof(DataStorage)));
            var storage = from element in collectorV where element.Name == "SketchStorage" select element;
            if (storage.Count() == 0)
            {
                ds = DataStorage.Create(doc);
                ds.Name = "SketchStorage";
                return; 
            }
            else ds = storage.First() as DataStorage;
           
            // получить данные по схеме хранения
            Entity ent_storage = ds.GetEntity(schema_sketchs);
            if (ent_storage == null) return;
            // получить данные по ID эскизов
            if(ent_storage.Schema!=null)
            SketchIds = ent_storage.Get<IList<int>>("SketchIds");

            //if (dataform.IsDeleteSketch)   // удаляем все картинки из проекта
            //{                
            //    foreach (Element el in all_images)
            //    {
            //        doc.Delete(el.Id);                     // удаляем из проекта 
            //    }
            //    all_images.Clear();
            //    SketchIds.Clear();
            //}

        }


        /// <summary>
        /// Подготовить шаблон схемы для записи общих данных проекта
        /// </summary>
        void PreparedSchemaSketchs()
        {
            SchemaBuilder schemaBuilder = new SchemaBuilder(SchemaSketchs);  // создать схему данных

                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemaBuilder.SetSchemaName("Id_sketchs");

                FieldBuilder
                fieldBuilder = schemaBuilder.AddArrayField("SketchIds", typeof(int));
                fieldBuilder.SetDocumentation("Id эскизов");
                
                fieldBuilder = schemaBuilder.AddSimpleField("Font", typeof(string));
                fieldBuilder.SetDocumentation("Шрифт эскизов");

                fieldBuilder = schemaBuilder.AddSimpleField("FontSize", typeof(int)); 
                fieldBuilder.SetDocumentation("Размер шрифта");

                fieldBuilder = schemaBuilder.AddSimpleField("Index_color", typeof(int));
                fieldBuilder.SetDocumentation("Индекс выбраного цвета");

                fieldBuilder = schemaBuilder.AddSimpleField("OutBorder", typeof(int));
                fieldBuilder.SetDocumentation("Размер внешней границы");

                fieldBuilder = schemaBuilder.AddSimpleField("CoefDiam", typeof(int));
                fieldBuilder.SetDocumentation("Коэффициент диаметра");

                fieldBuilder = schemaBuilder.AddSimpleField("ShiftFromLine", typeof(int));
                fieldBuilder.SetDocumentation("Смещение от линии");

                fieldBuilder = schemaBuilder.AddSimpleField("MaxLength", typeof(string));
                // fieldBuilder.SetSpec(SpecTypeId.Length);
                fieldBuilder.SetDocumentation("Максимальная длина стержня");

                fieldBuilder = schemaBuilder.AddSimpleField("IsDimLines", typeof(bool));
                fieldBuilder.SetDocumentation("Признак простановки размеров");

                fieldBuilder = schemaBuilder.AddSimpleField("IsAllRazdel", typeof(bool));
                fieldBuilder.SetDocumentation("Признак обработки всех разделов");

                fieldBuilder = schemaBuilder.AddSimpleField("BackGroundColor", typeof(bool));
                fieldBuilder.SetDocumentation("Признак прозрачности фона");

                fieldBuilder = schemaBuilder.AddSimpleField("IsModeShape", typeof(bool));
                fieldBuilder.SetDocumentation("Признак максимальной длины");

                fieldBuilder = schemaBuilder.AddSimpleField("IsShowAngle", typeof(bool));
                fieldBuilder.SetDocumentation("Признак отображения угла загиба");

                fieldBuilder = schemaBuilder.AddSimpleField("IsShowHooksLength", typeof(bool));
                fieldBuilder.SetDocumentation("Признак отображения длины крюков");

                fieldBuilder = schemaBuilder.AddSimpleField("PathFolder", typeof(string));
                fieldBuilder.SetDocumentation("Имя папки для сохранения эскизов");



            schema_sketchs = schemaBuilder.Finish();
           
        }        
        
        /// <summary>
        /// Удалить ненужные эскизы
        /// </summary>   
        void RemoveSketch()
        {
            // получить список существующих images в всем документе
            FilteredElementCollector collector_all_images = new FilteredElementCollector(doc);
            List<Element> all_images = collector_all_images.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();
            
            using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction1))
            {
             
                t.Start();
                SortedList all_images_id = new SortedList();

                // Start = DateTime.Now;

                // all_images = collector_all_images.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();   // все картинки

                

                FilteredElementCollector ALL_REBARS = new FilteredElementCollector(doc);
                IEnumerable<Rebar> R = ALL_REBARS.WherePasses(new ElementClassFilter(typeof(Rebar))).Cast<Rebar>();

                foreach (Rebar r in R)
                {
                    int i = r.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().IntegerValue;
                    if (i>0)
                        try
                        {
                            all_images_id.Add(i, i);
                        }
                        catch { }
                }


                FilteredElementCollector ALL_REBARS_IN_SYSTEM = new FilteredElementCollector(doc);
                IEnumerable<RebarInSystem> RIn = ALL_REBARS_IN_SYSTEM.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).Cast<RebarInSystem>();

                foreach (RebarInSystem r in RIn)
                {
                    int i = r.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().IntegerValue;
                    if (i > 0)
                        try
                        {
                            all_images_id.Add(i, i);
                        }
                        catch { }
                }

                //Stoped = DateTime.Now;
                //Elapsed = Stoped.Subtract(Start);
                //writer.WriteLine("RemoveSketch загружены данные: ");
                //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));

                images_Id_delete.Clear();

                foreach (Element el in all_images)
                {
                    // string el_number = el.Id.IntegerValue.ToString();
                 
                    // Start = DateTime.Now;
                    
                    ImageType it = el as ImageType;
                    int it_int = el.Id.IntegerValue;
                    if (SketchIds.Count(x => x == it_int) == 0) continue;   // это не наша картинка
                    if (all_images_id.ContainsKey(it_int))
                    {
                        if (!File.Exists(it.Path)) continue;
                        it.Reload();  continue;
                    }    // это нужная наша картинка

                    //FilteredElementCollector ALL_REBARS = new FilteredElementCollector(doc);
                    //ALL_REBARS.WherePasses(new ElementClassFilter(typeof(Rebar))).Where(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id));

                    //FilteredElementCollector ALL_REBARS_IN_SYSTEM = new FilteredElementCollector(doc);
                    //ALL_REBARS_IN_SYSTEM.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).Where(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id));

                    //IEnumerable<Element> v1 = ALL_REBARS.Where(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id));
                    //IEnumerable<Element> v2 = ALL_REBARS_IN_SYSTEM.Where(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id));

                    //if (ALL_REBARS.Count() + ALL_REBARS_IN_SYSTEM.Count() == 0)
                    //{
                    SketchIds.Remove(el.Id.IntegerValue);  // удаляем из списка
                    images_Id_delete.Add(el.Id);

                    // doc.Delete(el.Id);                     // удаляем из проекта       
                    //}

                    //// получить стержни проекта                                
                    //var ViewResultRebars = from element in ALL_REBARS where element.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id) select element;
                    //var ViewResultRebarsInSystem = from element in ALL_REBARS_IN_SYSTEM where element.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id) select element;

                    //IEnumerable<Rebar> ViewResultRebars = from elem in ((new FilteredElementCollector(doc)).WherePasses(new ElementClassFilter(typeof(Rebar))).ToElements())
                    //                                let roomTag = elem as Rebar
                    //                                      where (roomTag.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id))
                    //                                select roomTag;

                    //IEnumerable<RebarInSystem> ViewResultRebarsInSystem = from elem in ((new FilteredElementCollector(doc)).WherePasses(new ElementClassFilter(typeof(RebarInSystem))).ToElements())
                    //                                              let roomTag = elem as RebarInSystem
                    //                                      where (roomTag.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().Equals(it.Id))
                    //                                      select roomTag;

                    // if (ViewResultRebars.Count() + ViewResultRebarsInSystem.Count() == 0)
                    //if (ViewResultRebars.Count() == 0)
                    //{
                    //    //SketchIds.Remove(el.Id.IntegerValue);  // удаляем из списка
                    //    //doc.Delete(el.Id);                     // удаляем из проекта                       
                    //}



                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("RemoveSketch выполнен запрос для элемента: " + el_number);
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));


                }

                doc.Delete(images_Id_delete);

                //Start = DateTime.Now;
                // сохраним список в проекте
                Field field_current = schema_sketchs.GetField("SketchIds");
                // получить данные по схеме хранения
                Entity ent = new Entity(schema_sketchs);
                if (ent.Schema == null) { t.RollBack(); return; }
                ent.Set<IList<int>>(field_current, SketchIds);
                // запишем имя шрифта в проект
                field_current = schema_sketchs.GetField("Font");
                ent.Set<string>(field_current,dataform.Font_default_name);
                // запишем размер шрифта
                field_current = schema_sketchs.GetField("FontSize");
                ent.Set<int>(field_current, (int) dataform.Font_size);
                // запишем индекс выбраного цвета
                field_current = schema_sketchs.GetField("Index_color");
                ent.Set<int>(field_current, (int)dataform.index_color);
                // запишем размер внешней границы  
                field_current = schema_sketchs.GetField("OutBorder");
                ent.Set<int>(field_current, (int)dataform.border); 
                // запишем коэфф.диаметра
                field_current = schema_sketchs.GetField("CoefDiam");
                ent.Set<int>(field_current, dataform.coef_diam);
                // смещение от линии
                field_current = schema_sketchs.GetField("ShiftFromLine");
                ent.Set<int>(field_current, dataform.Font_shift);
                // запишем максимальную длину
                field_current = schema_sketchs.GetField("MaxLength");
                ent.Set<string>(field_current, Convert.ToString(dataform.Max_Lenght)); //, SpecTypeId.Custom);
                // запишем признак простановки размеров
                field_current = schema_sketchs.GetField("IsDimLines");
                ent.Set<bool>(field_current, dataform.Is_dim_lines);
                // запишем признак прозрачности фона
                field_current = schema_sketchs.GetField("BackGroundColor");
                ent.Set<bool>(field_current, dataform.BackGroundColor);
                // запишем признак обработки всех разделов
                field_current = schema_sketchs.GetField("IsAllRazdel");
                ent.Set<bool>(field_current, dataform.IsAllRazdel);
                // запишем признак использования максимальной  длины
                field_current = schema_sketchs.GetField("IsModeShape");
                ent.Set<bool>(field_current, dataform.mode_shape);
                // запишем признак отображения угла загиба для свободной формы
                field_current = schema_sketchs.GetField("IsShowAngle");
                ent.Set<bool>(field_current, dataform.Angle);
                // запишем признак отображения длины крюков
                field_current = schema_sketchs.GetField("IsShowHooksLength");
                ent.Set<bool>(field_current, dataform.HooksLength);
                // запишем имя папкми для сохранения эскизов
                field_current = schema_sketchs.GetField("PathFolder");
                ent.Set<string>(field_current, dataform.pathFolder);


                try
                {
                    ds.SetEntity(ent);

                    //Stoped = DateTime.Now;
                    //Elapsed = Stoped.Subtract(Start);
                    //writer.WriteLine("RemoveSketch данные записаны: ");
                    //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                    t.Commit();
                }
                catch { t.RollBack(); return; }
                // получить список существующих images в всем документе
                all_images = collector_all_images.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();
            
            }
        }


        /// <summary>
        /// Установить текущий шаблон проекта
        /// </summary>   
        /// <param name="bar">Стержень для установления шаблона</param>
        public static Template GetTemplate(Element bar)
        {
            Template template = Template.Rus;                         // шаблон по умолчанию
            
            try
            {
                // русский шаблон 
                double Ad = bar.get_Parameter(new Guid("b5ef18b4-453e-49bd-b26c-dfb3bd3ca79c")).AsDouble();
            }
            catch
            {
                template = Template.Other;                  // изменим номер шаблона                                                 
            }

            return template;
        }

        /// <summary>
        /// Создать/получить папку для размещения рисунков
        /// </summary>     
        public static string GetSetFolderImages(Document doc, string pathFolder)
        {
            try
            {
                if (pathFolder.Length>0)

                {
                    // возвращаем папку указанную в диалоге
                    DirectoryInfo dir_user = new DirectoryInfo(pathFolder);
                    if (!dir_user.Exists) { Autodesk.Revit.UI.TaskDialog.Show(Resourses.Strings.Texts.Title1, Resourses.Strings.Texts.Folder); return ""; }
                    return pathFolder;
                }

                // создать папку для размещения рисунков
                // папка должна быть обязательно
                string path_name = doc.PathName;
                if (path_name == "")
                { Autodesk.Revit.UI.TaskDialog.Show(Resourses.Strings.Texts.Title1, Resourses.Strings.Texts.Info2); return ""; }

                path_name = path_name.Remove(doc.PathName.LastIndexOf("\\"));
                path_name = path_name + "\\Images - " + doc.Title;

                DirectoryInfo dir = new DirectoryInfo(path_name);
                if (!dir.Exists) dir.Create();                             // создаем папку с рисунками при ее отсутствии

                return path_name;
            }
            catch
            {
                MessageBox.Show(Resourses.Strings.Texts.Folder);
                return "";
            }
        }
        /// <summary>
        /// Ключ для сортировки стержней
        /// </summary>     
        /// <param name="all_rebar">Список стержней для создания эскизов</param>

        public static string SortKey(Document doc,Element element)
        {

            // убрать все, что не относится к выбранному разделу
            string razdel = element.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString();
            string nomer = element.get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString();
            // номера может и не быть - для стержней с переменной длиной
            if (nomer == null)
            {
                // тогда сортируем по другим параметрам
                string bar = doc.GetElement(element.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();                
                string forma = doc.GetElement(element.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name;
                string length = Math.Round(element.get_Parameter(BuiltInParameter.REBAR_MAX_LENGTH).AsDouble(),3).ToString();
                nomer = bar + ";" + forma+";"+length;
            }
            return razdel + ";" + nomer;

            //string segments = SketchTools.DataBySegments(element);
            //string bar = doc.GetElement(element.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString();
            //string forma = doc.GetElement(element.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name;
            //return segments + ";" + bar + ";" + forma;
        }

        /// <summary>
        /// Получить сортированный список стержней для создания эскизов
        /// </summary>     

        // public static void GetSortListRebars(Document doc,List<Element> all_rebar, SortedList<string, CodeImage> sortedImages)
        void GetSortListRebars()
        {
            sortedImages.Clear();

            // выполняем выборку стержней по определенным признакам 
            foreach (Element el in all_rebar)
            {
                if (el.GroupId.IntegerValue > 0) continue;
               
                //// стержни свободной формы пропускаем
                //if (el.get_Parameter(BuiltInParameter.REBAR_GEOMETRY_TYPE).AsInteger() == 1) continue;

                try
                {
                    string key = SortKey(doc, el);
                    int kvp = sortedImages.IndexOfKey(key);
                    if (kvp >= 0) continue;
                    // убрать все, что не относится к выбранному разделу
                    //string razdel = el.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString();
                    //string nomer = el.get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString();
                    // sortedImages.Add(el.Id.IntegerValue.ToString(), new CodeImage(el.Id, new ElementId(-1)));
                    sortedImages.Add(key, new CodeImage(el.Id, new ElementId(-1)));
                    // sortedImages.Add(razdel + ";" + nomer, new CodeImage(el.Id, new ElementId(-1)));
                }
                catch
                {
                    // запись уже существует
                }

            }

            return;
        }

        /// <summary>
        /// Записать ID эскизов в проект
        /// </summary>  
        void WriteIdImagesToProject()
        {
            foreach (int i in SketchIdsNew)
            {
                if (SketchIds.Count(x => x == i) > 0) continue;  // такой Id уже есть
                SketchIds.Add(i);                                // добавим к списку
            }


        }

        /// <summary>
        /// Записать эскизы для арматуры
        /// </summary>  
        // List<int> WriteImages(Document doc, List<Element> all_rebar, SortedList<string, CodeImage> sortedImages)
        void WriteImages()
        {
            // удалить более ненужные эскизы
            doc.Delete(images_Id_delete);

            // List<int> NewSketchs = new List<int>();

            foreach (Element el in all_rebar)
            {
                if (el.GroupId.IntegerValue > 0) continue;  // стержни, включенные в группу не рассматриваются 
                string key = SortKey(doc,el);
                int index = sortedImages.IndexOfKey(key);
                if (index > -1)
                {
                    CodeImage eidCode = new CodeImage(new ElementId(-1),new ElementId(-1));
                    sortedImages.TryGetValue(key, out eidCode);
                    SketchTools.SetParameter(el, BuiltInParameter.ALL_MODEL_IMAGE, eidCode.image);   // записать Id картинки 
                    SketchIdsNew.Add(eidCode.image.IntegerValue);     // записать Id картинки 
                }
            }
            return;
        }

        //public static void fornewthread(object data)
        //{
        //    DataThread dataThread = data as DataThread;             
        //    //в действительности здесь должны находиться инструкции, которые будут выполняться в нашем новом потоке
        //    SketchReinforcement(dataThread.doc,
        //                        dataThread.sortedImages,
        //                        dataThread.all_rebar,
        //                        dataThread.all_rebars,
        //                        dataThread.dataform,
        //                        dataThread.template,
        //                        dataThread.all_images);
        //}

       
        
    }

    /// <summary>
    /// Для выборки из списка
    /// </summary>
    public class CodeImage
    {
        /// <summary>
        /// Арматурный стержень
        /// </summary>
        public ElementId element;
        /// <summary>
        /// Эскиз стержня
        /// </summary>
        public ElementId image;
        public CodeImage(ElementId element, ElementId image)
        {
            this.element = element;
            this.image = image;
        }

    }

    ///// <summary>
    ///// Implements the Revit add-in interface IExternalCommand
    ///// </summary>
    //[Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    //[Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]

    //public class SketchCommandWithoutDialog : IExternalCommand
    //{
    //    Document doc;
    //    /// <summary>
    //    /// Общие данные. Данные для диалога
    //    /// </summary>
    //    DataForm dataform = new DataForm();
    //    /// <summary>
    //    /// Общий список стержней проекта
    //    /// </summary>
    //    List<Element> all_rebars = new List<Element>();
    //    ///// <summary>
    //    ///// Cписок стержней для создания эскизов
    //    ///// </summary>
    //    //List<Element> all_rebar = new List<Element>();
    //    /// <summary>
    //    /// Сортированный список стержней для создания эскизов
    //    /// </summary>
    //    SortedList<string, CodeImage> sortedImages = new SortedList<string, CodeImage>();
    //    /// <summary>
    //    /// Путь к папке эскизов
    //    /// </summary>
    //    public static string FolderImages = "";
    //    /// Признак наличия лицензии
    //    /// </summary>
    //    public bool isCommerc = false;
    //    string CurrentFolder = "";

    //    public virtual Result Execute(ExternalCommandData commandData
    //       , ref string message, ElementSet elements)
    //    {
    //        doc = commandData.Application.ActiveUIDocument.Document;
    //        FolderImages = SketchCommand.GetSetFolderImages(doc,"");
    //        if (FolderImages == "") return Result.Failed;

    //        // получить стержни проекта
    //        FilteredElementCollector collector = new FilteredElementCollector(doc);
    //        all_rebars = collector.WherePasses(new ElementClassFilter(typeof(Rebar))).OfType<Element>().ToList();

    //        FilteredElementCollector collector2 = new FilteredElementCollector(doc);
    //        List<Element> rebar_in_system = collector2.WherePasses(new ElementClassFilter(typeof(RebarInSystem))).OfType<Element>().ToList();

    //        foreach (Element eid in rebar_in_system)
    //        {
    //            all_rebars.Add(eid);
    //        }

    //        if (all_rebars.Count() == 0) return Result.Failed;   // нет армирования в проекте


    //        // уставновить тип шаблона
    //        Template template = SketchCommand.GetTemplate(all_rebars[0]);

    //        // получить список существующих images в всем документе
    //        FilteredElementCollector collector_all_images = new FilteredElementCollector(doc);
    //        List<Element> all_images = collector_all_images.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();


           
    //            //// получить список стержней для создания эскиза
    //            //all_rebar = all_rebars;

    //            using (Transaction t = new Transaction(doc, Resourses.Strings.Texts.NameTransaction))
    //            {
    //                t.Start();
                  
    //                    // удалить все images из проекта               
    //                    foreach (Element el in all_images)
    //                    {
    //                        ImageType it = el as ImageType;
    //                        if (it.Path.Contains(FolderImages)) doc.Delete(el.Id);                            // удаляем все картинки
    //                    }                    

    //                // получить сортированный список стержней для создания эскизов
    //                SketchCommand.GetSortListRebars(doc,all_rebars, sortedImages);
    //                // получить эскизы                    
    //                SketchCommand.SketchReinforcement(doc, sortedImages, all_rebars, null, dataform, template, all_images,null);
    //                // записать полученные эскизы
    //                SketchCommand.WriteImages(doc,all_rebars,sortedImages);
    //                t.Commit();
    //            }

    //            return Result.Succeeded;
 
    //    }


    //    private void CheckLicense()
    //    {
    //        isCommerc = true; return;
    //        string lic = "";
    //        string PublicKey = KeyUtils.GetIdMB();
    //        PublicKey = KeyUtils.GetHesh(PublicKey + KeyUtils.CreatePK());
    //        FileInfo fi = new FileInfo(CurrentFolder);

    //        if (fi.Exists)
    //        {
    //            using (StreamReader reader = new StreamReader(CurrentFolder))
    //            {
    //                lic = reader.ReadLine();
    //            }
    //        }


    //        if (PublicKey == lic)
    //        { isCommerc = true; return; }
    //        else MessageBox.Show("Лицензии нет. Эскизы в демо-режиме не создаются");   //"Лицензии нет. Работа в демо-режиме."



    //    }

    //    /// <summary>
    //    /// Выполнить создание эскизов для арматуры
    //    /// </summary>
    //    /// <param name="doc">Документ текущего проекта</param>
    //    /// <param name="rebar">Список стержней для создания эскизов</param>
    //    /// <param name="check_lic">Признак наличия лицензии</param>
    //    /// <param name="trans">Текущая транзакция</param>
    //    public static void SketchReinforcement(Document doc, List<Element> rebar, bool check_lic, Transaction trans, DataForm dataform)
    //    {
    //        // 1
    //        Template template = Template.Rus;                         // шаблон по умолчанию

    //        // создать папку для размещения рисунков
    //        // папка должна быть обязательно
    //        string path_name = doc.PathName;
    //        if (path_name == "")
    //        { Autodesk.Revit.UI.TaskDialog.Show(Resourses.Strings.Texts.Title1, Resourses.Strings.Texts.Info2); return; }

    //        path_name = path_name.Remove(doc.PathName.LastIndexOf("\\"));
    //        path_name = path_name + "\\Images - " + doc.Title;

    //        if (rebar.Count > 0)
    //        {
    //            DirectoryInfo dir = new DirectoryInfo(path_name);
    //            if (!dir.Exists) dir.Create();                             // создаем папку с рисунками при ее отсутствии
    //        }

    //        // получить список существующих images
    //        FilteredElementCollector collector = new FilteredElementCollector(doc);
    //        List<Element> all_images = collector.WherePasses(new ElementClassFilter(typeof(ImageType))).OfType<Element>().ToList();


    //        //if (dataform.AllRebars)                                       // при использовании режима ВСЕ СТЕРЖНИ
    //        //{
                
    //            // удалить images из проекта
    //            foreach (Element el in all_images)
    //            {
    //                ImageType it = el as ImageType;
    //                if (it.Path.Contains(path_name)) doc.Delete(el.Id);                            // удаляем все картинки
    //            }

               

    //            //// для идентификации стержней запишем в текстовом виде значения существующих параметров
    //            //foreach (Element el in rebar)
    //            //{
    //            //    string num_rebar = SketchTools.DataBySegments(el);
    //            //    // num_rebar = el.get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString();
    //            //    SketchTools.SetParameter(el, BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, num_rebar);   // записать комментарий
    //            //}

    //            // получить параметры сравнения арматуры  (длина с округлением до 1 мм)           
    //            IEnumerable<MarkR> mark = rebar.Select(x => new MarkR(
    //                SketchTools.DataBySegments(x),
    //                doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString(),
    //                doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name, 0)).Distinct();

    //                // Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0))).Distinct();
    //            IEnumerable<MarkR> mark_sorted = (from MarkR in mark orderby MarkR.Length descending, MarkR.bar select MarkR);
                
    //            foreach (MarkR mp in mark_sorted)
    //            {
    //                List<Element> rb = rebar.FindAll(x =>
    //                    SketchTools.DataBySegments(x) == mp.segments &&
    //                    doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == mp.bar &&
    //                    doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == mp.forma);
    //                    //&&
    //                    //Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == mp.Length);

    //                //if (rb.Count > 0)
    //                //{
    //                //    // получить имя файла для первого стержня группы
    //                //    string image = path_name + "\\" + rb[0].Id.IntegerValue.ToString() + ".png";
    //                //    Eskis.CreateImage(doc, rb, path_name, template, trans, dataform, image,null);
    //                //}

                   
    //            }

    //        //}

    //        //if (dataform.ByView)                                        
    //        //{
    //        //    // получить параметры сравнения арматуры  (длина с округлением до 1 мм)           
    //        //    IEnumerable<MarkR> mark = rebar.Select(x => new MarkR(
    //        //        SketchTools.DataBySegments(x),
    //        //        doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString(),
    //        //        doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name, 0)).Distinct();

    //        //    // Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0))).Distinct();
    //        //    IEnumerable<MarkR> mark_sorted = (from MarkR in mark orderby MarkR.Length descending, MarkR.bar select MarkR);

    //        //    foreach (MarkR mp in mark_sorted)
    //        //    {
    //        //        List<Element> rb = rebar.FindAll(x =>
    //        //            SketchTools.DataBySegments(x) == mp.segments &&
    //        //            doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_NAME).AsString() == mp.bar &&
    //        //            doc.GetElement(x.get_Parameter(BuiltInParameter.REBAR_SHAPE).AsElementId()).Name == mp.forma);
    //        //        //&&
    //        //        //Math.Round(x.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble() / 0.00328, 0) == mp.Length);

    //        //        if (rb.Count > 0)
    //        //        {
    //        //            // эскиз для данной группы стержней
    //        //            ElementId eid = rb[0].get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId();   // записать Id картинки 
    //        //            int eid_count = 0;
    //        //            if (eid.IntegerValue > 0)
    //        //            {
    //        //                eid_count = all_images.Count(x => x.Id.IntegerValue == eid.IntegerValue);
    //        //                // если число эскизов выборки равно числу эскизов в документе, то данный эскиз можно удалить
    //        //                if (rb.Count == eid_count) doc.Delete(eid);
    //        //            }

    //        //            Eskis.CreateImage(doc, rb, path_name, template, trans, dataform);
    //        //        }


    //        //    }

    //        //}

    //        //if (dataform.UpdateSingleRebar)                     // при режиме работы ОТДЕЛЬНЫЙ СТЕРЖЕНЬ
    //        //{
    //        //    // удалить картинку
    //        //    foreach (Element el in rebar)
    //        //    {
    //        //        ElementId eid = el.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId();

    //        //        if (eid.IntegerValue > 0)
    //        //        {
    //        //            int eid_count = all_images.Count(x => x.Id.IntegerValue == eid.IntegerValue);
    //        //            int rebar_count = rebar.Count(x => x.get_Parameter(BuiltInParameter.ALL_MODEL_IMAGE).AsElementId().IntegerValue == eid.IntegerValue);
    //        //            // если число эскизов выборки равно числу эскизов в документе, то данный эскиз можно удалить
    //        //            if (rebar_count == eid_count) doc.Delete(eid);
    //        //        }
    //        //    }

    //        //    Eskis.CreateImage(doc, rebar, path_name, template, trans, dataform);
    //        //}
    //    }

    //}
    
}
