using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.Drawing;
using System.IO;
using Autodesk.Revit.DB; 
using Autodesk.Revit.DB.Structure;
using System.Drawing.Text;

namespace SketchReinforcement
{
    
    // 2000 мм - 180 пкс
    /// <summary>
    /// Класс для создания формы №4 
    /// </summary>
    class Form04
    {        
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(516, 93);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(744, 217);
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s=SketchTools.GetRoundLenghtSegment(rebar, A);
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s= s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap=null;               
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));                
            }
        }        
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get 
            {                
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get 
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        } 
        
        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form04(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
             
            // string kode_form = "04";
            doc=element.Document;                      
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            // string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();                
            }

            if (A <= 0 || B <= 0 || C <= 0) return;
            

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            
 
                if (SketchTools.CompareDoubleMore(A,C)) 
                { 
                    double c = C; C = A; A = c;
                    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                }

                //string file = folder + "\\" + kode_form + "\\" + kode_form + ".png";
                string file = UserFolderImage + image;

                if (SketchTools.CompareDouble(A,C))
                {
                    image = "4EQ - (CIS).png";
                    p_start = new PointF(p_end.X, p_start.Y);
                    // file = folder + "\\" + kode_form + "\\" + kode_form + "EQ.png";
                    file = FolderImage + "\\"+image; 
                }

                FileInfo fileinfo = new FileInfo(file);
                if (fileinfo.Exists)
                {
                    bitmap = new Bitmap(file);
                }
                else return;

                picture = Graphics.FromImage(bitmap);
                

                PointF Apos = new PointF(433 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
                PointF Bpos = new PointF(300 - 90, 150 + SketchReinforcementApp.shift_font);
                PointF Cpos = new PointF(540 - Cf.Width / 2, 215 + SketchReinforcementApp.shift_font);


                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
                // не показывать А при симметричном стержне
                //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
                if (!SketchTools.CompareDouble(A, C)) picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
                picture.RotateTransform(-90);
                //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
                picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
                picture.RotateTransform(90);
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height-30);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
           
           
                // при наличии крюков
                if (Hook_start.IntegerValue > 0) 
                {
                StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 33);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_start.X, p_start.Y);
                if(show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }                
            
                if (Hook_end.IntegerValue > 0)
                {
                StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width / 2, p_end.Y - 90);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_end.X, p_end.Y - 50);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }
        }       
    }


    /// <summary>
    /// Класс для создания формы rebarIn
    /// </summary>
    class FormRebarIn
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(212, 124);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(794, 124);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        RebarInSystem rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Начальный крюк - длина
        /// </summary>
        double Hook_start_value = 0;
        /// <summary>
        /// Конечный крюк - длина
        /// </summary>
        double Hook_end_value=0;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                if (rbd.HookAngle0 == 0) return RebarHookOrientation.Left;
                return rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                if (rbd.HookAngle1 == 0) return RebarHookOrientation.Left;
                return rbd.HookOrient1;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public FormRebarIn(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            
            // string kode_form = "00";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as RebarInSystem;
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();

            }


            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();


            // string file = folder + "\\" + kode_form + "\\" + kode_form + ".png";
            string file = UserFolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);


            PointF Apos = new PointF(500 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            // при наличии крюков
            float shift = 33.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.up, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_end.X - Hook_length_start_f.Width / 2, p_end.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.upleft, FolderHook);
                }
                PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width / 2, p_start.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №1 
    /// </summary>
    class Form1
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(212, 124);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(795, 124);
        #endregion 
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Остаток сегмента  - справа
        /// </summary>
        string Astr_short = "";
        /// <summary>
        /// Остаток сегмента  - слева
        /// </summary>
        string Bstr_short = "";
        /// <summary>
        /// Основной сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if(segments >= 2) s=segments.ToString()+"x"+ SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length< 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0 && segments==0)
                {
                    if (smin.Length< 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Afshort
        {
            get
            { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Bfshort
        {
            get
            { return picture.MeasureString(Bstr_short, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);         
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>        
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Начальный крюк - длина
        /// </summary>
        double Hook_start_value = 0;
        /// <summary>
        /// Конечный крюк - длина
        /// </summary>
        double Hook_end_value = 0;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                if (rbd.HookAngle0 == 0) return RebarHookOrientation.Left;
                return rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                if (rbd.HookAngle1 == 0) return RebarHookOrientation.Left;
                return rbd.HookOrient1;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя участка - по умолчанию А</param>            
        public Form1(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook,int coef_diam, string seg1="A")
        {
            // вариант эскиза
            int v_sketch = 0;
            // координаты ввода текста
            float X = 500.0f;
            float Y = 70.0f;
            float Xm = 700.0f;
            float Ym = 85.0f;
            float Xm2 = 240.0f;
            float Ym2 = 85.0f;

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
          
            rebar = element as Rebar;

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            

            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            foreach (Parameter pr in pset)
            {

                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Adef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
            }
            double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня
            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length>0 && max_length> SketchCommand.max_length)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                
                segments = (int) Math.Ceiling(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {
                    double A_new = 0;
                    double B_new = 0;

                    //if(rebar.NumberOfBarPositions>1)    // стержни с переменной длиной не обрабатываем
                    //{
                    //    segments = 0;
                    //    goto createImage;
                    //}

                    //// первый случай: (два крюка) длина более двух максимальных - не реализуем
                    //if (Hook_start.IntegerValue> 0 && Hook_end.IntegerValue > 0 && max_length> (2* SketchCommand.max_length - diam * coef_diam))
                    //{
                    //    segments = 0;
                    //    goto createImage;
                    //}

                    // второй случай: два крюка
                    if (Hook_start.IntegerValue > 0 && Hook_end.IntegerValue > 0)
                    {
                        // получить длину крюков
                        Hook_start_value = SketchTools.GetFullLengthHook(rebar);
                        Hook_end_value = SketchTools.GetFullLengthHook(rebar, true);

                        if (segments == 2)
                        {                            
                            A_new = SketchCommand.max_length - Hook_start_value;   // длина 1 участка - максимальная
                            Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new - Hook_end_value  + diam * coef_diam); // длина 2 участка - остаток
                            A = A_new;
                            Bstr_short = ""; // третьего участка нет                           
                            goto changePosText;
                        }
                        else
                        {
                            B_new = SketchCommand.max_length - Hook_start_value;   // длина 1 участка - максимальная                            
                            Bstr_short = SketchTools.GetRoundLenghtSegment(rebar,B_new); // левый короткий участок
                            segments = segments-2;   // минус 2 крайних сегмента
                            A_new = segments * SketchCommand.max_length - (segments - 1) * diam * coef_diam; // габаритная длина среднего участка
                            Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new - B_new + 2* diam * coef_diam); // правый короткий участок
                            A = SketchCommand.max_length * segments;
                            p_start = new PointF(212, 137);                            
                            if(segments!=2) p_end = new PointF(796, 137);
                            v_sketch =1;
                            goto createImage;
                        }
                    }

                    // третий случай: если есть один крюк и он первый, то ставим как второй.
                    if (Hook_start.IntegerValue > 0)  // не перемещать выше
                    {
                        Hook_start = ElementId.InvalidElementId;
                        Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();                      
                    }
                    // начинаем с прямого участка
                    segments--;
                    A_new = SketchCommand.max_length * segments;
                    Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                    Bstr_short = ""; // показываем только правый короткий сегмент. Левый пустой - не показываем
                    A = A_new;

                changePosText:
                    // изменим параметры для ввода текста
                    X = 400.0f;
                    p_end = new PointF(796, 137);
                }
            }

            
            createImage:
                      
            string file = FolderImage + image;
            if (segments>0 && Bstr_short=="")
            {
                image = image.Replace(".", "multi.");
                file =  FolderImage +"\\" + image;                
            }
            if (segments == 1 && v_sketch==1)
            {
                image = image.Replace(".", "multi3.");
                file = FolderImage + "\\" + image;
            }

            if (segments == 2 && v_sketch == 1)
            {
                image = image.Replace(".", "multi4.");
                file = FolderImage + "\\" + image;
            }

            if (segments > 2 && v_sketch == 1)
            {
                image = image.Replace(".", "multiS.");
                file = FolderImage + "\\" + image;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);


            PointF Apos = new PointF(X - Af.Width / 2, Y - Af.Height + SketchReinforcementApp.shift_font);         
            // picture.FillRectangle(Brushes.White,Apos.X,Apos.Y,Af.Width,Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            if(segments>0)
            {
                Apos = new PointF(Xm - Afshort.Width / 2, Ym - Afshort.Height + SketchReinforcementApp.shift_font);
                picture.DrawString(Astr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);                
            }
            if (Bstr_short!="")
            {
                Apos = new PointF(Xm2 - Bfshort.Width / 2, Ym2 - Bfshort.Height + SketchReinforcementApp.shift_font);
                picture.DrawString(Bstr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }

            // при наличии крюков
            float shift = 33.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + shift);                 
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                // if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    // StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width / 2, p_end.Y + shift);                
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }

        ///// <summary>
        /////  Получить округленные длины участков стержней, число участков по максимальной длине стержня
        ///// </summary>
        /////         
        //void GetRoundSegments()
        //{
        //    //string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
        //    //string s = SketchTools.GetRoundLenghtSegment(rebar, A, out A);
        //    segments = Convert.ToInt32(A / SketchCommand.max_length);
        //    if(segments>0) Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - segments* SketchCommand.max_length);
        //    if (A == 0) { Astr = s; return; } 
        //    if (s.Length < 2) { Astr = s; return; }
        //    if (s.Substring(0, 2) == "0.") s = s.Substring(1);
        //    if (Amin > 0)
        //    {
        //        if (smin.Length < 2) { Astr = smin;  return }
        //        if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
        //        s = s + "..." + smin;
        //    }
        //    Astr = s;
        //}
    }

    /// <summary>
    /// Класс для создания формы №11 
    /// </summary>
    class Form11
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_end = new PointF(767, 73);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_start = new PointF(243, 163);
        #endregion
        #region Сегменты стержня
        ///// <summary>
        ///// Сегмент стержня А 
        ///// </summary>
        //SizeF Afshort
        //{
        //    get
        //    { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        //}
         
        ///// <summary>
        ///// Остаток сегмента участка А
        ///// </summary>
        //string Astr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А1 
        /// </summary>
        double A1;
        /// <summary>
        /// Сегмент стержня А2 
        /// </summary>
        double A2;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {                
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А1
        /// </summary>
        string A1str
        {
            get
            {

                string s = SketchTools.GetRoundLenghtSegment(rebar,A1);
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);                 
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А2
        /// </summary>
        string A2str
        {
            get
            {

                string s = SketchTools.GetRoundLenghtSegment(rebar, A2);
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }


        /// <summary>
        /// Сегмент стержня A1
        /// </summary>
        SizeF A1f
        {
            get
            { return picture.MeasureString(A1str, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня A2
        /// </summary>
        SizeF A2f
        {
            get
            { return picture.MeasureString(A2str, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        ///// <summary>
        ///// Сегмент стержня А 
        ///// </summary>
        //SizeF Afshort
        //{
        //    get
        //    { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        //}

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);                
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)( Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159,0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form11(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1="A", string seg2="B")
        {
            bool IsSofistic = false;
            string v_sketch = "0";
            int num_overlap = 0;  // число нахлестов
            // координаты ввода текста
            float X = 500.0f; 
            float Y = 185.0f;

            // последний участок
            float Xm2 = 650.0f;
            float Ym2 = 140.0f;
            // первый участок
            float Xm1 = 370.0f;
            float Ym1 = 140.0f;

            ElementId first_hook = ElementId.InvalidElementId;  // первый крюк

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
           
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
                                 
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: первый сегмент и второй сегмент - из параметров
            // приведем порядок сегментов в параметрах к фактическому порядку в стержне

            // если первый сегмент не "А"
            if (segment1 == seg2 || segment1 == seg2.ToLower() || segment1 == "Бdef")
            {
                string s = seg1;   // поменяем местами
                seg1 = seg2;
                seg2 = s;          
            }            

            //// если первый сегмент А
            //if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            //{                
            //    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            //    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //}
            //else
            //{
            //    orient_hook = true;                
            //    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            //}

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name.Contains("SOFiSTiK")) IsSofistic = true;
            }

            // по умолчанию 1 сегмент (имя А) - самый длинный
            // на чертеже А - горизонтальный участок, В - вертикальный
            // если длина В оказалась больше А - то меняем местами. По умолчанию первый крюк при А
            if (SketchTools.CompareDoubleMore(B,A))     
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                // orient_hook = orient_hook ? false : true;
                orient_hook = true;
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
            }
          

            double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0 && max_length > SketchCommand.max_length)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();              

                segments = (int)Math.Ceiling(max_length / (SketchCommand.max_length - diam * coef_diam / 2));

                if (segments > 0)
                {
                    if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
                    {
                        segments = 0;
                        goto createImage;
                    }

                    // отсчет сегментов начинаем с короткой стороны
                    // получить полную длину крюка с короткой стороны (крюк вместе с последним участком стержня)
                    double Hook_end_value = 0;
                    if(!orient_hook) Hook_end_value=
                        SketchTools.GetFullLastSegment(rebar);
                    else Hook_end_value =
                        SketchTools.GetFullFirstSegment(rebar);

                    segments--;  // один сегмент уходит на правый край в любом случае
                    num_overlap = segments;  // число нахлестов                    
                    // отсчет начинаем с короткого участка
                    A2 = SketchCommand.max_length - Hook_end_value;
                    if (IsSofistic) A2 = SketchCommand.max_length - B;

                    double Hook_start_length = SketchTools.GetLengthHook(rebar, Hook_start);   // длина только крюка
                    double Aost = A - A2 - Hook_start_length + diam * coef_diam;   // длина оставшегося сегмента
                   
                    if (Aost > SketchCommand.max_length)    // делим оставшийся сегмент
                    {
                        A = A - A2 + diam * coef_diam; 
                        if(Hook_start_length>0)    // есть крюк на конце. Появляется обязательный сегмент на конце
                        {
                            segments--;                           
                            double seg_middle= segments * SketchCommand.max_length - (num_overlap-1) * diam * coef_diam;
                            A1 = A - seg_middle - SketchTools.GetFullLengthHook(rebar, orient_hook);
                            A = SketchCommand.max_length;
                        }
                        else
                        {
                            // проверим, чтобы оставшийся участок содержал кратное число сегментов
                            double seg_middle = segments * SketchCommand.max_length - (num_overlap-1) * diam * coef_diam;
                            if(SketchTools.GetRoundLenghtSegmentValue(rebar,A - seg_middle)==0)
                            {
                                num_overlap--;
                                A = SketchCommand.max_length;
                            }
                            else
                            {
                                segments--;                               
                                double seg_middle2 = segments * SketchCommand.max_length - (num_overlap - 1) * diam * coef_diam;
                                A1 = A - seg_middle2;
                                A = SketchCommand.max_length;
                            }
                        }
                    }
                    else
                    {
                        A = Aost;
                    }

                    // изменим параметры для ввода текста
                    // X = 420.0f;
                    p_end = new PointF(767,83);
                    if (num_overlap == 1)
                    {
                        p_start = new PointF(243, 163);
                        v_sketch = "1";
                    }
                    if (num_overlap == 2)
                    {                        
                        p_start = new PointF(243, 172);
                        v_sketch = "2";
                    }
                     
                    if (num_overlap > 2) v_sketch = "3";

                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
            {
                switch (image)
                {
                    case "M-17A (ESP).png":
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        image = "M-17AEQ (ESP).png";
                        break;
                    case "11 - (BS8666-2005).png":
                    image = "11EQ - (BS8666-2005).png";
                    break;
                    default :
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                    image = "2EQ - (Gost21-501).png";
                    break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if(num_overlap>0)
                {
                    image = image.Replace(".", "multi" + v_sketch + ".");
                    file = FolderImage + "\\" + image;
                }
            }
           
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(950 - 90, 121 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Apos = new PointF(X - Af.Width / 2, Y + SketchReinforcementApp.shift_font);
            

            if (SketchTools.CompareDouble(A,B))
            {
                p_end = new PointF(580, 54);
                p_start = new PointF(417, 217); 
                Bpos = new PointF(730 - 90, 131 + SketchReinforcementApp.shift_font);
                Apos = new PointF(498 - Af.Width / 2, 326 - 90 + SketchReinforcementApp.shift_font);
                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            }

            picture.RotateTransform(-90);          
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);                      
            picture.RotateTransform(90);


            if (v_sketch == "0")
            {
                picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }
            else
            {                 
                    if(A>0) picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

                    // А1 - показываем, если больше 0
                    Apos = new PointF(Xm1 - A1f.Width / 2, Ym1 - A1f.Height);
                    if (A1 > 0) picture.DrawString(A1str, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

                    // А2 - показываем в любом случае
                    Apos = new PointF(Xm2 - A2f.Width / 2, Ym1 - A2f.Height);
                    picture.DrawString(A2str, SketchReinforcementApp.drawFont, Brushes.Black, Apos);                
            }

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                //else                                                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);


                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);

                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                //if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                //else                                                  StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);


                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);


                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №11 
    /// </summary>
    class Form11_old_start
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_end = new PointF(767, 73);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_start = new PointF(243, 163);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Afshort
        {
            get
            { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Остаток сегмента участка B 
        /// </summary>
        string Astr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        ///// <summary>
        ///// Сегмент стержня А 
        ///// </summary>
        //SizeF Afshort
        //{
        //    get
        //    { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        //}

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form11_old_start(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1 = "A", string seg2 = "B")
        {
            // координаты ввода текста
            float X = 500.0f;
            float Y = 180.0f;
            float Xm = 700.0f;
            float Ym = 70.0f;

            ElementId first_hook = ElementId.InvalidElementId;  // первый крюк

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: первый сегмент и второй сегмент - из параметров
            // приведем порядок сегментов в параметрах к фактическому порядку в стержне

            // если первый сегмент не "А"
            if (segment1 == seg2 || segment1 == seg2.ToLower() || segment1 == "Бdef")
            {
                string s = seg1;   // поменяем местами
                seg1 = seg2;
                seg2 = s;
            }

            //// если первый сегмент А
            //if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            //{                
            //    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            //    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //}
            //else
            //{
            //    orient_hook = true;                
            //    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            //}

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
            }

            // по умолчанию 1 сегмент (имя А) - самый длинный
            // на чертеже А - горизонтальный участок, В - вертикальный
            if (SketchTools.CompareDoubleMore(B, A))
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                // orient_hook = orient_hook ? false : true;
                orient_hook = true;
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
            }

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня
                segments = (int)Math.Truncate(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {
                    double A_new = 0;


                    if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
                    {
                        segments = 0;
                        goto createImage;
                    }
                    //    if (rebar.NumberOfBarPositions > 1)    // стержни с переменной длиной не обрабатываем
                    //{
                    //    segments = 0;
                    //    goto createImage;
                    //}

                    // первый случай: крюк в начале и длина более двух максимальных - не реализуем
                    if (Hook_start.IntegerValue > 0 && max_length > (2 * SketchCommand.max_length + diam * coef_diam))
                    {
                        segments = 0;
                        goto createImage;
                    }

                    // второй случай: крюк в начале 
                    if (Hook_start.IntegerValue > 0)
                    {
                        // получить полную длину крюка
                        double Hook_start_value = SketchTools.GetFullLengthHook(rebar, orient_hook);
                        A_new = SketchCommand.max_length - Hook_start_value;   // длина участка - максимальная длина
                        Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                        A = A_new;
                        goto changePosText;
                    }

                    // начинаем с прямого участка
                    A_new = SketchCommand.max_length * segments;
                    Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                    A = A_new;

                changePosText:
                    // изменим параметры для ввода текста
                    X = 450.0f;
                    p_end = new PointF(767, 83);
                    // p_end = new PointF(767, 73);
                    // new PointF(243, 163);
                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
            {
                switch (image)
                {
                    case "M-17A (ESP).png":
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        image = "M-17AEQ (ESP).png";
                        break;
                    case "11 - (BS8666-2005).png":
                        image = "11EQ - (BS8666-2005).png";
                        break;
                    default:
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                        image = "2EQ - (Gost21-501).png";
                        break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if (segments > 0)
                {
                    image = image.Replace(".", "multi.");
                    file = FolderImage + "\\" + image;
                }
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(950 - 90, 121 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90

            PointF Apos = new PointF(X - Af.Width / 2, Y + SketchReinforcementApp.shift_font);


            if (SketchTools.CompareDouble(A, B))
            {
                p_end = new PointF(580, 54);
                p_start = new PointF(417, 217);
                Bpos = new PointF(730 - 90, 131 + SketchReinforcementApp.shift_font);
                Apos = new PointF(498 - Af.Width / 2, 326 - 90 + SketchReinforcementApp.shift_font);
                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            }

            picture.RotateTransform(-90);
            // picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            // picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);


            if (segments > 0)
            {
                Apos = new PointF(Xm - Afshort.Width / 2, Ym - Afshort.Height);
                picture.DrawString(Astr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                //else                                                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);


                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);

                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                //if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                //else                                                  StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);


                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);


                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №12 
    /// </summary>
    class Form12
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_end = new PointF(767, 73);   // 767
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_start = new PointF(243, 163);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Afshort
        {
            get
            { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Остаток сегмента участка B 
        /// </summary>
        string Astr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = true;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }       

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);                
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if(s.Length<2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string RstrR
        {
            get
            {
#if Rtype
                string s = "R "+SketchTools.GetRoundLenghtSegment2(rebar, R);
#else
                string s = "R " + SketchTools.GetRoundLenghtSegment(rebar, R);
#endif
                if (R == 0) return s;                 
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }


        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;       
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form12(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1 = "A", string seg2 = "B")
        {
            bool IsSofistic = false;
            // координаты ввода текста
            float X = 500.0f;
            float Y = 185.0f;
            float Xm = 600.0f;
            float Ym = 180.0f;

            // ElementId first_hook = ElementId.InvalidElementId;  // первый крюк

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: первый сегмент и второй сегмент - из параметров
            // приведем порядок сегментов в параметрах к фактическому порядку в стержне

            // если первый сегмент не "А"
            if (segment1 == seg2 || segment1 == seg2.ToLower() || segment1 == "Бdef")
            {
                string s = seg1;   // поменяем местами
                seg1 = seg2;
                seg2 = s;
            }            

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == "R" || pr.Definition.Name == "r") R = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name.Contains("SOFiSTiK")) IsSofistic = true;
            }

#if Rtype
            R = 0;
            // В данной версии R принимаем как параметр типа
            ElementId typeId = rebar.GetTypeId();
            RebarBarType rbt = doc.GetElement(typeId) as RebarBarType;
            if (rbt != null) R = rbt.get_Parameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER).AsDouble();
#endif

            // по умолчанию 1 сегмент (имя А) - самый длинный
            // на чертеже А - горизонтальный участок, В - вертикальный
            if (SketchTools.CompareDoubleMore(B, A))
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;               
                orient_hook = false;
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
            }

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня
                segments = (int)Math.Truncate(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {
                    double A_new = 0;
                    

                    if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
                    {
                        segments = 0;
                        goto createImage;
                    }

                    // получить полную длину крюка
                    double Hook_start_value = 0;
                    if (orient_hook) Hook_start_value = SketchTools.GetFullLastSegment(rebar);
                    else Hook_start_value = SketchTools.GetFullFirstSegment(rebar);

                    // остаток на конце
                    A_new = max_length + diam * coef_diam - SketchCommand.max_length - Hook_start_value;
                    // основной сегмент - ближний к загибу
                    Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new);
                    A = A_new;

                    // изменим параметры для ввода текста
                    X = 340.0f;
                    p_end = new PointF(766, 83);                    
                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
            {
                switch (image)
                {
                    case "12 - (BS8666-2005).png":
                        image = "12EQ - (BS8666-2005).png";
                        break;
                    case "M-12 (ESP).png":
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        image = "M-12EQ (ESP).png";
                        break;
                    default:
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                        image = "3EQ - (Gost21-501).png";
                        break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if (segments > 0)
                {
                    image = image.Replace(".", "multi.");
                    file = FolderImage + "\\" + image;
                }
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(950 - 90, 121 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Apos = new PointF(X - Af.Width / 2, Y + SketchReinforcementApp.shift_font);
            PointF Rpos = new PointF(640 - Rf.Width, 35 + SketchReinforcementApp.shift_font);

            if (SketchTools.CompareDouble(A, B))
            {
                p_end = new PointF(580, 54);
                p_start = new PointF(417, 217);
                Bpos = new PointF(730 - 90, 131 + SketchReinforcementApp.shift_font);
                Apos = new PointF(498 - Af.Width / 2, 326 - 90 + SketchReinforcementApp.shift_font);
                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            }

            picture.RotateTransform(-90);             
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);             
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.DrawString(RstrR, SketchReinforcementApp.drawFont, Brushes.Black, Rpos);

            if (segments > 0)
            {
                Apos = new PointF(Xm - Afshort.Width / 2, Ym);
                picture.DrawString(Astr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {                
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);  // HookPosition.v_upleft


                PointF Hookpos = new PointF(p_end.X, p_end.Y - 70);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }
    /// <summary>
    /// Класс для создания формы №12 
    /// </summary>
    class Form12_old_start
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_end = new PointF(767, 73);   // 767
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_start = new PointF(243, 163);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Afshort
        {
            get
            { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Остаток сегмента участка B 
        /// </summary>
        string Astr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string RstrR
        {
            get
            {
#if Rtype
                string s = "R "+SketchTools.GetRoundLenghtSegment2(rebar, R);
#else
                string s = "R " + SketchTools.GetRoundLenghtSegment(rebar, R);
#endif
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }


        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form12_old_start(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1 = "A", string seg2 = "B")
        {
            // координаты ввода текста
            float X = 500.0f;
            float Y = 185.0f;
            float Xm = 680.0f;
            float Ym = 180.0f;

            // ElementId first_hook = ElementId.InvalidElementId;  // первый крюк

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: первый сегмент и второй сегмент - из параметров
            // приведем порядок сегментов в параметрах к фактическому порядку в стержне

            // если первый сегмент не "А"
            if (segment1 == seg2 || segment1 == seg2.ToLower() || segment1 == "Бdef")
            {
                string s = seg1;   // поменяем местами
                seg1 = seg2;
                seg2 = s;
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == "R" || pr.Definition.Name == "r") R = rebar.get_Parameter(pr.Definition).AsDouble();
            }

#if Rtype
            R = 0;
            // В данной версии R принимаем как параметр типа
            ElementId typeId = rebar.GetTypeId();
            RebarBarType rbt = doc.GetElement(typeId) as RebarBarType;
            if (rbt != null) R = rbt.get_Parameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER).AsDouble();
#endif

            // по умолчанию 1 сегмент (имя А) - самый длинный
            // на чертеже А - горизонтальный участок, В - вертикальный
            if (SketchTools.CompareDoubleMore(B, A))
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = true;
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
            }

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня
                segments = (int)Math.Truncate(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {
                    double A_new = 0;


                    if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
                    {
                        segments = 0;
                        goto createImage;
                    }
                    //if (rebar.NumberOfBarPositions > 1)    // стержни с переменной длиной не обрабатываем
                    //{
                    //    segments = 0;
                    //    goto createImage;
                    //}

                    // первый случай: крюк в начале и длина более двух максимальных - не реализуем
                    if (Hook_start.IntegerValue > 0 && max_length > (2 * SketchCommand.max_length + diam * coef_diam))
                    {
                        segments = 0;
                        goto createImage;
                    }

                    // второй случай: крюк в начале 
                    if (Hook_start.IntegerValue > 0)
                    {
                        // получить полную длину крюка
                        double Hook_start_value = SketchTools.GetFullLengthHook(rebar, orient_hook);
                        A_new = SketchCommand.max_length - Hook_start_value;   // длина участка - максимальная длина
                        Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                        A = A_new;
                        goto changePosText;
                    }

                    // начинаем с прямого участка
                    A_new = SketchCommand.max_length * segments;
                    Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                    A = A_new;

                changePosText:
                    // изменим параметры для ввода текста
                    X = 430.0f;
                    p_end = new PointF(766, 83);
                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, B))     // если А=В - спецрисунки - нахлест не учитываем
            {
                switch (image)
                {
                    case "12 - (BS8666-2005).png":
                        image = "12EQ - (BS8666-2005).png";
                        break;
                    case "M-12 (ESP).png":
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        image = "M-12EQ (ESP).png";
                        break;
                    default:
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                        image = "3EQ - (Gost21-501).png";
                        break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if (segments > 0)
                {
                    image = image.Replace(".", "multi.");
                    file = FolderImage + "\\" + image;
                }
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(950 - 90, 121 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Apos = new PointF(X - Af.Width / 2, Y + SketchReinforcementApp.shift_font);
            PointF Rpos = new PointF(640 - Rf.Width, 35 + SketchReinforcementApp.shift_font);

            if (SketchTools.CompareDouble(A, B))
            {
                p_end = new PointF(580, 54);
                p_start = new PointF(417, 217);
                Bpos = new PointF(730 - 90, 131 + SketchReinforcementApp.shift_font);
                Apos = new PointF(498 - Af.Width / 2, 326 - 90 + SketchReinforcementApp.shift_font);
                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            }

            picture.RotateTransform(-90);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.DrawString(RstrR, SketchReinforcementApp.drawFont, Brushes.Black, Rpos);

            if (segments > 0)
            {
                Apos = new PointF(Xm - Afshort.Width / 2, Ym);
                picture.DrawString(Astr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);


                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №12_old
    /// </summary>
    class Form12_old
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(767, 73);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(243, 163);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string Rstr
        {
            get
            {                
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня R 
        /// </summary>
        string RstrR
        {
            get
            {
#if Rtype
                string s = "R "+SketchTools.GetRoundLenghtSegment2(rebar, R);
#else
                string s = "R "+SketchTools.GetRoundLenghtSegment(rebar, R);
#endif
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form12_old(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B")
        {
             
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
             
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента


            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            {
                
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower()  || pr.Definition.Name == "Бdef" ) B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == "R" || pr.Definition.Name == "r") R = rebar.get_Parameter(pr.Definition).AsDouble();
            }
#if Rtype
            R = 0;
            // В данной версии R принимаем как параметр типа
            ElementId typeId = rebar.GetTypeId();
            RebarBarType rbt = doc.GetElement(typeId) as RebarBarType;
            if (rbt != null) R = rbt.get_Parameter(BuiltInParameter.REBAR_STANDARD_BEND_DIAMETER).AsDouble();
#endif
            if (SketchTools.CompareDoubleMore(A, B))
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
            }

            string file = UserFolderImage + image;
            if (SketchTools.CompareDouble(A, B))
            {
                switch (image)
                {
                    case "12 - (BS8666-2005).png":
                    image = "12EQ - (BS8666-2005).png";
                    break;
                    case "M-12 (ESP).png":
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                    image = "M-12EQ (ESP).png";
                    break;
                    default :
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Default";
                    image = "3EQ - (Gost21-501).png";
                    break;
                }
                file = FolderImage + "\\" + image;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            // PointF Apos = new PointF(950 - 90, 121);
            PointF Apos = new PointF(925 - Af.Width / 2, 121);
            PointF Bpos = new PointF(505 - Bf.Width / 2, 270 - 90);
            // PointF Rpos = new PointF(700 - Rf.Width, 70);
            PointF Rpos = new PointF(640 - Rf.Width, 35);

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90

            if (SketchTools.CompareDouble(A, B))
            {
                p_start = new PointF(580, 54);
                p_end = new PointF(417, 217);
                // Apos = new PointF(730 - 90, 131);
                Apos = new PointF(730 - Af.Width / 2, 131);
                Bpos = new PointF(498 - Bf.Width / 2, 326 - 90);
                Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90
                // Rpos = new PointF(510 - Rf.Width, 120);
                Rpos = new PointF(450 - Rf.Width, 85);
            }

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y +10, Af.Width, Af.Height-30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.RotateTransform(90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y+10, Bf.Width, Bf.Height-30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            // показать радиус
            //picture.FillRectangle(Brushes.White, Rpos.X, Rpos.Y+10, Rf.Width, Rf.Height-30);
            picture.DrawString(RstrR, SketchReinforcementApp.drawFont, Brushes.Black, Rpos);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                else                                                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                else                                                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y - 40);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// Класс для создания формы №13
    /// </summary>
    class Form13_old
    {
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(516, 93);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(745, 217);
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию А</param>
        /// <param name="seg2">Имя сегмента по умолчанию В</param>
        /// <param name="seg3">Имя сегмента по умолчанию С</param>         
        public Form13_old(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C")
        {
            
            // string kode_form = "13";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            {
               
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            if (image == "25 - (Gost21-501).png")
            {
                foreach (Parameter pr in pset)
                {
                    if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) C = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                }
            }
            else
            {
                foreach (Parameter pr in pset)
                {
                    if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                }
            }
            if (A <= 0 || B <= 0 || C <= 0) return;

            if (SketchTools.CompareDoubleMore(A, C))
            {
                double c = C; C = A; A = c;
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
            }

            string file = UserFolderImage + image;

            if (SketchTools.CompareDouble(A, C))
            {
                p_start = new PointF(p_end.X, p_start.Y);
                switch (image)
                {
                    case "13 - (BS8666-2005).png":
                    image = "13EQ - (BS8666-2005).png";
                    break;
                    case "M-10 (ESP).png":
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                    image = "M-10EQ (ESP).png";
                    break;
                    default :
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Default";
                    image = "25EQ - (Gost21-501).png";
                    break;                
                 }
                file = FolderImage + "\\" + image;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);


            PointF Apos = new PointF(433 - Af.Width / 2, 80 - 90);
            PointF Bpos = new PointF(300 - 90, 150);
            PointF Cpos = new PointF(540 - Cf.Width / 2, 285 - 70);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            // не показывать А при симметричном стержне
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            if (!SketchTools.CompareDouble(A, C)) picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 33);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_start.X, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else                                                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width / 2, p_end.Y - 90);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_end.X, p_end.Y - 50);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №13 
    /// </summary>
    class Form13
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(745,217);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(516,93);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Afshort
        {
            get
            { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Остаток сегмента участка B 
        /// </summary>
        string Astr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        ///// <summary>
        ///// Сегмент стержня А 
        ///// </summary>
        //SizeF Afshort
        //{
        //    get
        //    { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        //}

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form13(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1 = "A", string seg2 = "B", string seg3 = "C")
        {
            bool IsSofistic = false;
            // координаты ввода текста A и Адоп
            float X = 550.0f;
            float Y = 220.0f;
            float Xm = 400.0f;
            float Ym = 220.0f;
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: первый сегмент и второй сегмент - из параметров
            // приведем порядок сегментов в параметрах к фактическому порядку в стержне

            // если первый сегмент не "А"
            if (segment1 == seg3 || segment1 == seg3.ToLower() || segment1 == "Cdef")
            {
                string s = seg1;   // поменяем местами А и С
                seg1 = seg3;
                seg3 = s;
            }
            
                foreach (Parameter pr in pset)
                {
                    if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name.Contains("SOFiSTiK")) IsSofistic = true;
            }
          
            if (A <= 0 || B <= 0 || C <= 0) return;
                       

            // по умолчанию 1 сегмент (имя А) - самый длинный
            // на чертеже А - горизонтальный участок, В - вертикальный, С-горизонтальный короткий
            if (SketchTools.CompareDoubleMore(C, A))
            {
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                // orient_hook = orient_hook ? false : true;
                orient_hook = true;
                double temp = A; A = C; C = temp;                
            }

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня
                segments = (int)Math.Truncate(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {
                    double A_new = 0;
                    

                    if (SketchTools.CompareDouble(A, C))     // если А=C - спецрисунки - нахлест не учитываем
                    {
                        segments = 0;
                        goto createImage;
                    }
                    //if (rebar.NumberOfBarPositions > 1)    // стержни с переменной длиной не обрабатываем
                    //{
                    //    segments = 0;
                    //    goto createImage;
                    //}

                    // первый случай: крюк в начале и длина более двух максимальных - не реализуем
                    if (Hook_start.IntegerValue > 0 && max_length > (2 * SketchCommand.max_length + diam * coef_diam))
                    {
                        segments = 0;
                        goto createImage;
                    }

                    // второй случай: крюк в начале 
                    if (Hook_start.IntegerValue > 0)
                    {
                        // получить полную длину крюка
                        double Hook_start_value = SketchTools.GetFullLengthHook(rebar, orient_hook);
                        A_new = SketchCommand.max_length - Hook_start_value;   // длина участка - максимальная длина
                        Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                        A = A_new;
                        goto changePosText;
                    }

                    // начинаем с прямого участка
                    A_new = SketchCommand.max_length * segments;
                    Astr_short = SketchTools.GetRoundLenghtSegment(rebar, A - A_new + segments * diam * coef_diam);
                    A = A_new;

                changePosText:
                    // изменим параметры для ввода текста
                    X = 600.0f;
                    Y = 230.0f;
                    p_start = new PointF(745,226);
                    // PointF p_end = new PointF(745, 217);
                    // p_end = new PointF(767, 73);
                    // new PointF(243, 163);
                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, C))     // если А=C - спецрисунки - нахлест не учитываем
            {
                p_start = new PointF(p_end.X, p_start.Y);
                switch (image)
                {
                    case "13 - (BS8666-2005).png":
                        image = "13EQ - (BS8666-2005).png";
                        break;
                    case "M-10 (ESP).png":
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        image = "M-10EQ (ESP).png";
                        break;
                    default:
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                        image = "25EQ - (Gost21-501).png";
                        break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if (segments > 0)
                {
                    image = image.Replace(".", "multi.");
                    file = FolderImage + "\\" + image;
                }
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(X - Af.Width / 2,Y + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(300 - 90, 150 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(425 - Cf.Width / 2,-5 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
                                   

            if (SketchTools.CompareDouble(A, C))
            {
                p_start = new PointF(745,217);
                p_end = new PointF(745, 93  );
                Bpos = new PointF(300 - 90, 150 + SketchReinforcementApp.shift_font);
                Apos = new PointF(550 - Af.Width / 2, 220 + SketchReinforcementApp.shift_font);
                Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            }

            picture.RotateTransform(-90);            
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            if (!SketchTools.CompareDouble(A, C)) picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            
            if (segments > 0)
            {
                Apos = new PointF(Xm - Afshort.Width / 2, Ym);
                picture.DrawString(Astr_short, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            }

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {                

                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.up, FolderHook);

                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 50);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_start.X, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!SketchTools.CompareDouble(A, C)) Hookpos = new PointF(p_end.X, p_end.Y - 50);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }
    /// <summary>
    /// Класс для создания формы №14
    /// </summary>
    class Form14
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        //PointF p_start = new PointF(622, 56);
        PointF p_start = new PointF(627, 61);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(354, 200);
        
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        public Form14(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C")
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

           

            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            {
               

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            }
            else
            {

                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();

            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            if (SketchTools.CompareDoubleMore(A, C))
            {
                double temp = A; A = C; C = temp;
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
            }

            string file = FolderImage + image;           

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(740, -5 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(240 - Bf.Width / 2, 127 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(525 - Cf.Width/2, 197 + SketchReinforcementApp.shift_font);

            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90           

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            // показать радиус
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //picture.FillRectangle(Brushes.White, 618, 52, 20, 20);
                PointF p=new PointF(p_start.X,p_start.Y);
                if (Angle1 > 90) p = new PointF(p.X + 12, p.Y + 10);
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_up45, FolderHook);
                    else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width -  25 , p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                                                                 else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №15
    /// </summary>
    class Form15
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        // PointF p_start = new PointF(323, 49);
        PointF p_start = new PointF(335, 61);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(835, 213);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form15(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
           
            // string kode_form = "15";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

           

            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef")
            {
               
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            if (SketchTools.CompareDoubleMore(A, C))
            {
                double temp = A; A = C; C = temp;
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(450, 50 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(250 - Bf.Width / 2, 130 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(680 - Cf.Width / 2, 210 + SketchReinforcementApp.shift_font);

            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90           

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);
            // показать радиус
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                PointF p = new PointF(p_start.X, p_start.Y);
                if (Angle1 == 90)
                {
                    p = new PointF(323, 49);
                }
                //picture.FillRectangle(Brushes.White, 320, 45, 15,15);
                if (HookOrientationStart == RebarHookOrientation.Right)
                {
                    if (Angle1 > 90) p = new PointF(p.X - 12, p.Y - 11);
                    StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_upleft45, FolderHook);
                }
                else StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_up45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width - 15, p_start.Y - 60);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №21 
    /// </summary>
    class Form21
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(312, 29);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(718, 73);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Bfshort
        {
            get
            { return picture.MeasureString(Bstr_short, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Остаток сегмента участка B 
        /// </summary>
        string Bstr_short = "";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// Число сегментов (по максимальной длине стержня)
        /// </summary>
        int segments;
        /// <summary>
        /// Сегмент стержня B (первый участок)
        /// </summary>
        double B1=0;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B (второй участок)
        /// </summary>
        double B2=0;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                // string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                //if (Amin > 0)
                //{
                //    if (smin.Length < 2) return smin;
                //    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                //    s = s + "..." + smin;
                //}
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF B1f
        {
            get
            { return picture.MeasureString(B1str, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF B2f
        {
            get
            { return picture.MeasureString(B2str, SketchReinforcementApp.drawFont); }
        }


        ///// <summary>
        ///// Сегмент стержня А 
        ///// </summary>
        //SizeF Afshort
        //{
        //    get
        //    { return picture.MeasureString(Astr_short, SketchReinforcementApp.drawFont); }
        //}

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                // string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (segments > 1) s = segments.ToString() + "x" + SketchTools.GetRoundLenghtSegment(rebar, SketchCommand.max_length);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                //if (Bmin > 0)
                //{
                //    if (smin.Length < 2) return smin;
                //    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                //    s = s + "..." + smin;
                //}
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B1
        /// </summary>
        string B1str
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B1);
                if (B1 == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B2
        /// </summary>
        string B2str
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B2);
                if (B2 == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form21(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, int coef_diam, string seg1 = "A", string seg2 = "B", string seg3 = "C")
        {
            bool IsSofistic = false;

            string v_sketch = "";  // тип эскиза
           
            // координаты ввода текста В и Вдоп
            float X = 513.0f;
            float Y = 210.0f;
            float XB1 = 380.0f;
            float YB1 = 80.0f;
            float XB2 = 650.0f;
            float YB2 = 80.0f;

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            // по умолчанию: ОСНОВНОЙ СЕГМЕНТ - В             

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name.Contains("SOFiSTiK")) IsSofistic = true;
            }

            if (A <= 0 || B <= 0 || C <= 0) return;


            // по умолчанию: ОСНОВНОЙ СЕГМЕНТ - В 
            // на чертеже В - горизонтальный участок, А и С  - вертикальный            
            double max_length = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LENGTH).AsDouble(); // максимальная длина стержня

            // проверим по максимальной длине участка стержня
            if (SketchCommand.max_length > 0 && max_length > SketchCommand.max_length)
            {
                // получить диаметр стержня 
                double diam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                
                // segments = (int)Math.Truncate(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                segments = (int)Math.Ceiling(max_length / (SketchCommand.max_length - diam * coef_diam / 2));
                if (segments > 0)
                {                   
                    p_end = new PointF(718, 35);
                    // получить полную длину первого сегмента стержня
                    double Hook_start_value = SketchTools.GetFullFirstSegment(rebar);

                    // длина более максимальной - не реализуем
                    if (Hook_start_value > SketchCommand.max_length)
                    {
                        segments = 0;
                        goto createImage;
                    }
                    
                    double Hook_end_value = SketchTools.GetFullLastSegment(rebar);

                    // длина более максимальной - не реализуем
                    if (Hook_end_value > SketchCommand.max_length)
                    {
                        segments = 0;
                        goto createImage;
                    }

                    B1 = SketchCommand.max_length - Hook_start_value;
                    if(IsSofistic) B1 = SketchCommand.max_length - A;
                    B2 = B - B1 - (segments - 2)* SketchCommand.max_length + (segments-1)*diam * coef_diam;
                    B = SketchCommand.max_length;

                    // первый случай: один нахлест в середине участка В
                    if (segments ==2)
                    {   
                        B = 0;  // участок В не показываем
                        v_sketch = "0";
                        // изменим параметры для ввода текста
                       
                        goto createImage;
                    }

                    if (segments == 3)
                    {     
                        v_sketch = "1";
                        segments = 1;   // один сегмент по середине
                        // изменим параметры для ввода текста
                      
                        goto createImage;
                    }

                    if (segments == 4)
                    {      
                        v_sketch = "2";
                        segments = 2;   // два сегмента по середине
                        // изменим параметры для ввода текста
                      
                        goto createImage;
                    }

                    if (segments > 4)
                    {
                        v_sketch = "3";
                        segments = segments-2;   // три сегмента по середине
                        // изменим параметры для ввода текста
                      
                        goto createImage;
                    }
                }
            }

        createImage:

            string file = FolderImage + image;

            if (SketchTools.CompareDouble(A, C) && segments==0)     // если А=C - спецрисунки - нахлест не учитываем
            {
                p_end = new PointF(p_end.X, p_start.Y);
                switch (image)
                {
                    case "M-17 (ESP).png":
                        image = "M-17EQ (ESP).png";
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        break;
                    case "21 - (BS8666-2005).png":
                        image = "21EQ - (BS8666-2005).png";
                        break;
                    default:
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Gost21-501";
                        image = "4EQ - (Gost21-501).png";
                        break;
                }
                file = FolderImage + "\\" + image;
            }
            else
            {
                if (segments > 0)
                {
                    image = image.Replace(".", "multi"+v_sketch+".");
                    file = FolderImage + "\\" + image;                    
                }
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(231 - 100, 115 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(X - Bf.Width / 2,Y + SketchReinforcementApp.shift_font);
            PointF B1pos = new PointF(XB1 - B1f.Width / 2, YB1 + SketchReinforcementApp.shift_font);
            PointF B2pos = new PointF(XB2 - B2f.Width / 2, YB2 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(763 + 30, 130 + SketchReinforcementApp.shift_font);

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            picture.RotateTransform(-90);             
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);            
            if (!SketchTools.CompareDouble(A, C) || segments>0) picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);

            if(B>0) picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            if (B1 > 0) picture.DrawString(B1str, SketchReinforcementApp.drawFont, Brushes.Black, B1pos);
            if (B2 > 0) picture.DrawString(B2str, SketchReinforcementApp.drawFont, Brushes.Black, B2pos);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_start.X - 1, p_start.Y), Angle1, HookPosition.v_upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 30, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }


            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X + 1, p_end.Y), Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 30, p_end.Y - 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }
    /// <summary>
    /// Класс для создания формы №21
    /// </summary>
    class Form21_old
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(312, 29);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(718, 73);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double Cmin;
        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Cmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Cmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")"; 
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        public Form21_old(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C")
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
         
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

           

            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" )
            {
                
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower()) A = SketchTools.GetMaxMinValue(rebar, pr, out Amin);
                if (pr.Definition.Name == "Аdef") { A = SketchTools.GetMaxMinValue(rebar, pr, out Amin); C = A; }
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower()) C = SketchTools.GetMaxMinValue(rebar, pr, out Cmin);
            }

            if (SketchTools.CompareDoubleMore(C, A))
            {
                double temp = A; A = C; C = temp;
                temp = Amin; Amin = Cmin; Cmin = temp;
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
            }

            string file = UserFolderImage + image;
            if (SketchTools.CompareDouble(A, C))
            {
                p_end = new PointF(p_end.X, p_start.Y);
                switch (image)
                {
                    case "M-17 (ESP).png":
                        image = "M-17EQ (ESP).png";
                        // путь к папке рисунков
                        FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "ESP";
                        break;
                    case "21 - (BS8666-2005).png":
                    image = "21EQ - (BS8666-2005).png";
                    break;
                    default : 
                    // путь к папке рисунков
                    FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Default";
                    image = "4EQ - (Gost21-501).png";
                    break;
                }
                file = FolderImage + "\\" + image;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(231 - 100, 115);
            PointF Bpos = new PointF(513 - Bf.Width / 2, 272 - 80);
            PointF Cpos = new PointF(763 + 30, 130);

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
            if (!SketchTools.CompareDouble(A, C)) picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y+5, Bf.Width, Bf.Height-15);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            
            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_start.X-1,p_start.Y), Angle1, HookPosition.v_upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 30, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X + 1, p_end.Y), Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 30, p_end.Y - 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }
    /// <summary>
    /// Класс для создания формы №22
    /// </summary>
    class Form22
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(735, 244);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(651, 202);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        

        
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        /// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form22(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 =  StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            // получить параметры формы
            if (segment1 != "A")
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }
             

            string file = FolderImage + image;
            if (SketchTools.CompareDoubleMore(D, B))
            {
                p_start = new PointF(735, 252);
                p_end = new PointF(815, 202);
                image = "22MO - (BS8666-2005).png";
                file = FolderImage + "\\" + image;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(800, 169 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(500 - Bf.Width / 2, 0 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(130, 150 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(457 - Df.Width/2, 205 + SketchReinforcementApp.shift_font);

            if (SketchTools.CompareDoubleMore(D, B))
            {
                Apos = new PointF(920, 169 + SketchReinforcementApp.shift_font);
                Dpos = new PointF(540 - Df.Width / 2, 205 + SketchReinforcementApp.shift_font);

            }

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 5, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y + 5, Df.Width, Df.Height - 15);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_start.X -1,p_start.Y), Angle1, HookPosition.v_down, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width/2, p_end.Y + 45);
                if (SketchTools.CompareDoubleMore(D, B))
                {
                    Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_end.Y - 50);
                    
                }
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X, p_end.Y), Angle2, HookPosition.down, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y - 80);
                if (SketchTools.CompareDoubleMore(D, B))
                {
                    Hookpos = new PointF(p_start.X, p_end.Y - 80);
                    
                }
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №M_20
    /// </summary>
    class FormM_20
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(212, 124);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(794, 124);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        public FormM_20(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "A", string seg2 = "B", string seg3 = "C")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();

            }
            // string file = folder + "\\" + kode_form + "\\" + kode_form + ".png";
            string file = FolderImage + image;

            //if (Angle1 != 90 || Angle2 != 90)
            //{
            //    file = UserFolderImage + "00 - (BS8666-2005).png";
            //}
            //if (HookOrientationStart == HookOrientationEnd)
            //{
            //    file = UserFolderImage + "00 - (BS8666-2005).png";
            //}
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;


            picture = Graphics.FromImage(bitmap);


            PointF Bpos = new PointF(500 - Bf.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            //if ((Angle1 != 90 || Angle2 != 90) || (HookOrientationStart == HookOrientationEnd))
            //{
                // при наличии крюков
                if (Hook_start.IntegerValue > 0)
                {
                    if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.up, FolderHook);
                    else StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.down, FolderHook);
                    // показать длину прямого участка крюка                 
                    PointF Hookpos = new PointF(p_end.X - Hook_length_start_f.Width / 2, p_end.Y + 33);
                    if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }

                if (Hook_end.IntegerValue > 0)
                {
                    if (HookOrientationEnd == RebarHookOrientation.Left) StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.downleft, FolderHook);
                    else StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.upleft, FolderHook);
                    PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width / 2, p_start.Y);
                    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }
            //}
            //else
            //{
                PointF Apos = new PointF(96, 114 + SketchReinforcementApp.shift_font);
                PointF Cpos = new PointF(849, 181 + SketchReinforcementApp.shift_font);
                Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90  
                Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
                picture.RotateTransform(-90);
                //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
                picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                picture.RotateTransform(90);
            //}
        }
    }

    /// <summary>
    /// Класс для создания формы №23
    /// </summary>
    class Form23
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(212, 124);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(794, 124);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        public Form23(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "A", string seg2 = "B", string seg3 = "C")
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();

            }
            // string file = folder + "\\" + kode_form + "\\" + kode_form + ".png";
            string file = FolderImage + image;

            if (Angle1 != 90 || Angle2 != 90)
            {
                file = FolderImage + "00 - (BS8666-2005).png";
            }
            if (HookOrientationStart == HookOrientationEnd)
            {
                file = FolderImage + "00 - (BS8666-2005).png";
            }
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;


            picture = Graphics.FromImage(bitmap);


            PointF Bpos = new PointF(500 - Bf.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            if ((Angle1 != 90 || Angle2 != 90) || (HookOrientationStart == HookOrientationEnd))
            {
                // при наличии крюков
                if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_end.X - Hook_length_start_f.Width / 2, p_end.Y + 33);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Left) StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.upleft, FolderHook);
                PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width / 2, p_start.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
            }
            else
            {
                PointF Apos = new PointF(96, 114 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(849, 181 + SketchReinforcementApp.shift_font);
            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №24
    /// </summary>
    class Form24
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(357,199);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(677, 20);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form24(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {
                
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();

            }
             
            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(492 - Af.Width / 2, 205 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(712, 223 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Dpos = new PointF(460, 193 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(740, 89 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y, Dpos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);

                // при наличии крюков
                if (Hook_start.IntegerValue > 0)
                {
                    if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                    else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                    // показать длину прямого участка крюка                 
                    PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 60);
                    if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }

                if (Hook_end.IntegerValue > 0)
                {
                    if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                    else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width -30, p_end.Y - 30);
                    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }
             
        }
    }

    /// <summary>
    /// Класс для создания формы №25
    /// </summary>
    class Form25
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(181, 74);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(858, 64);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию А</param>
        /// <param name="seg2">Имя сегмента по умолчанию В</param>
        /// <param name="seg3">Имя сегмента по умолчанию С</param>
        /// <param name="seg4">Имя сегмента по умолчанию D</param>
        /// <param name="seg5">Имя сегмента по умолчанию E</param>
        public Form25(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook,
            string seg1 = "A", string seg2 = "B", string seg3 = "C", string seg4 = "D", string seg5 = "E")
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {
                
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg5 || pr.Definition.Name == seg5.ToLower()) E = rebar.get_Parameter(pr.Definition).AsDouble();

            }

            if (image == "7 - (Gost21-501).png")
            {
                foreach (Parameter pr in pset)
                {
                    if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) E = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "D" || pr.Definition.Name == "d") C = rebar.get_Parameter(pr.Definition).AsDouble();
                    if (pr.Definition.Name == "E" || pr.Definition.Name == "e") D = rebar.get_Parameter(pr.Definition).AsDouble();
                }
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(285, 30 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(700 - Bf.Width, 80 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height-20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Epos = new PointF(510 - Ef.Width/2, 210 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y +10, Ef.Width, Ef.Height-30);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);

            PointF Dpos = new PointF(938, 133 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(0, 133 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //picture.FillRectangle(Brushes.White, 166, 60, 15, 15);

                PointF p;               
                if (Angle1 == 90)
                {
                    p = new PointF(p_start.X - 11, p_start.Y - 11);
                }
                else
                {
                    p = new PointF(p_start.X + 2, p_start.Y + 2);
                }
                if (HookOrientationStart == RebarHookOrientation.Right)
                {
                     
                        p = new PointF(p_start.X - 7, p_start.Y - 7);
                    
                    StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_upleft45, FolderHook); 
                }
                else StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_up45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 70);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
               
                //picture.FillRectangle(Brushes.White, 847, 60, 15, 15);
                if (HookOrientationEnd == RebarHookOrientation.Right)
                {
                    PointF p = new PointF(p_end.X - 28, p_end.Y + 24);
                    if (Angle2 == 180) p = new PointF(p_end.X - 28, p_end.Y + 34);
                    if (Angle2 == 135) p = new PointF(p_end.X - 18, p_end.Y + 33);
                    StandartFormUtils.DrawHook(picture, p, Angle2, HookPosition.v_up_45, FolderHook); 
                }
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft_45, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 70);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №26
    /// </summary>
    class Form26
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(186, 225);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(807, 95);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию А</param>
        /// <param name="seg2">Имя сегмента по умолчанию В</param>
        /// <param name="seg3">Имя сегмента по умолчанию С</param>
        /// <param name="seg4">Имя сегмента по умолчанию D</param>
        public Form26(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3= "C", string seg4= "D")
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Арм_А")
            {
                 
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Арм_А") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Арм_Б") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Арм_В") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower() || pr.Definition.Name == "Арм_Г") D = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(297 - Af.Width/2, 230 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(470     - Bf.Width, 74 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(700 - Cf.Width/2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);            

            PointF Dpos = new PointF(625, 126 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);          

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X, p_end.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №M_19
    /// </summary>
    class FormM_19
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(229, 285);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(800, 27);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double H;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double K;

        #endregion
        #region Прочие параметры класса

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Hstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, H);
                if (H == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Hf
        {
            get
            { return picture.MeasureString(Hstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Kstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, K);
                if (K == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Kf
        {
            get
            { return picture.MeasureString(Kstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
         
        public FormM_19(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
             
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "B" || segment1 == "b")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "H" || pr.Definition.Name == "h") H = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "K" || pr.Definition.Name == "k") K = rebar.get_Parameter(pr.Definition).AsDouble();                
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);
                       

            //PointF Bpos = new PointF(250 - Bf.Width,70);            
            //picture.DrawString(Bstr, drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(500 - Cf.Width / 2, 205 + SketchReinforcementApp.shift_font);            
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            //PointF Dpos = new PointF(720 - Df.Width, -5);          
            //picture.DrawString(Dstr, drawFont, Brushes.Black, Dpos);

            PointF Kpos = new PointF(270 - Kf.Width / 2, 30 + SketchReinforcementApp.shift_font);
            picture.DrawString(Kstr, SketchReinforcementApp.drawFont, Brushes.Black, Kpos);

            Kpos = new PointF(747 - Kf.Width / 2, 205 + SketchReinforcementApp.shift_font);
            picture.DrawString(Kstr, SketchReinforcementApp.drawFont, Brushes.Black, Kpos);

            PointF Hpos = new PointF(-10, 215 + SketchReinforcementApp.shift_font);
            Hpos = new PointF(-Hpos.Y - Hf.Width / 2, Hpos.X);       // для поворота на 90  
            picture.RotateTransform(-90);
            picture.DrawString(Hstr, SketchReinforcementApp.drawFont, Brushes.Black, Hpos);

            Hpos = new PointF(990 - Hf.Width /2  , 90 + SketchReinforcementApp.shift_font);
            Hpos = new PointF(-Hpos.Y - Hf.Width/2 , Hpos.X);       // для поворота на 90  
            picture.DrawString(Hstr, SketchReinforcementApp.drawFont, Brushes.Black, Hpos);
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft_45, FolderHook);
                else
                {
                    // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft_45, FolderHook);
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_down_45, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft_45, FolderHook);
                else
                {
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_down_45, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X, p_end.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }



    /// <summary>
    /// Класс для создания формы №27
    /// </summary>
    class Form27
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        // PointF p_start = new PointF(213, 280);
        PointF p_start = new PointF(243, 284);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(707, 122);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                if (Hook_end.IntegerValue > 0 && Angle2 == 90) return true;
                return false;
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public Form27(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B",string seg3="C",string seg4="D")
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {
               
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) D = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            if (!IsMainVariant && seg1=="A") file = FolderImage + "\\27a - (BS8666-2005).png";

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(236 - Af.Width, 114 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(540 - Bf.Width/2, SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(760, 160 + SketchReinforcementApp.shift_font);           
            PointF Dpos = new PointF(869, 202 + SketchReinforcementApp.shift_font);           
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            if(IsMainVariant || seg1!="A") picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //picture.FillRectangle(Brushes.White, 209, 274, 10, 10);

                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft_45, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_down_45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X+20, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }


            if (Hook_end.IntegerValue > 0 && seg1!="A")
            {
                p_end.X = p_end.X + 1;
                p_end.Y = p_end.Y + 84;

                if (HookOrientationEnd == RebarHookOrientation.Right)
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                else
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_downleft, FolderHook);

                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width / 2, p_end.Y + 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if ( Hook_end.IntegerValue > 0 && !IsMainVariant && seg1=="A")
            {
                if (HookOrientationEnd == RebarHookOrientation.Right)
                     StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                else
                     StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);

                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width/2, p_end.Y + 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №28
    /// </summary>
    class Form28
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(213, 280);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(707, 122);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                if (Hook_end.IntegerValue > 0 && Angle2 == 90) return true;
                return false;
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form28(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {           
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            if (!IsMainVariant) file = FolderImage + "\\28a - (BS8666-2005).png";

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(236 - Af.Width, 114 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(540 - Bf.Width / 2, SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(760, 75 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(420, 175 + SketchReinforcementApp.shift_font);
            // Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90            
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            if (IsMainVariant) picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_down_45, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft_45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 20, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 && !IsMainVariant)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №29
    /// </summary>
    class Form29
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(370, 198);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(370, 93);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form29(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;            

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(531 - Af.Width/2, 240 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(727, 80 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(515 - Cf.Width/2, -13 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Dpos = new PointF(210,150 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);                        
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 20, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X + 20, p_end.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №31
    /// </summary>
    class Form31
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(549, 98);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(630, 75);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public Form31(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C", string seg4="D")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
             
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Арм_R") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Арм_H2") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower() || pr.Definition.Name == "Арм_H2") D = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;
           

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(441 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);             
           
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Cpos = new PointF(477 - Cf.Width / 2, 235 + SketchReinforcementApp.shift_font);
            
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Bpos = new PointF(237, 156 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Dpos = new PointF(732, 145 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
             
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_start.X+2,p_start.Y), Angle1, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width+20, p_start.Y + 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X+1,p_end.Y), Angle2, HookPosition.v_up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 60);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №31
    /// </summary>
    class FormT1_closed
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(549, 98);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(630, 75);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public FormT1_closed(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "A", string seg2 = "B")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Арм_R") B = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            if (A < B)
            {
                double temp = A;
                A = B;
                B = temp;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(500 - Af.Width / 2, 210 + SketchReinforcementApp.shift_font);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Bpos = new PointF(660, 145 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90             
            picture.RotateTransform(-90);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0 && Hook_end.IntegerValue > 0)
            {
                if (Hook_length_start == Hook_length_end)
                {
                    if (Angle1 == Angle2 && Angle1 == 135)
                    {
                        //if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_start.X + 2, p_start.Y), Angle1, HookPosition.up, FolderHook);
                        //else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                        // показать длину прямого участка крюка                 
                        // PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width + 20, p_start.Y + 30);                    

                        SizeF sizeF = picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFont);
                        PointF Hookpos = new PointF(370, -sizeF.Height + SketchReinforcementApp.shift_font); // - SketchReinforcementApp.size_font/2);                        
                        Hookpos = new PointF(Hookpos.X * 0.707f + Hookpos.Y * 0.707f + sizeF.Width / 2 + 48f - SketchReinforcementApp.size_font, -Hookpos.X * 0.707f + Hookpos.Y * 0.707f);       // для поворота на 45
                        picture.RotateTransform(45);
                        if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                        picture.RotateTransform(-45);
                    }
                }
            }

            //if (Hook_end.IntegerValue > 0)
            //{
            //    if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X + 1, p_end.Y), Angle2, HookPosition.v_up, FolderHook);
            //    else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X, p_end.Y - 60);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            //}

        }
    }



    /// <summary>
    /// Класс для создания формы №32
    /// </summary>
    class Form32
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(353,97);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(668, 172);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                
                if (Hook_start.IntegerValue > 0 && Angle1 == 90 && Hook_end.IntegerValue > 0 && Angle2 == 90) 
                {
                    if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd ==RebarHookOrientation.Left) return false;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form32(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "B")
            {
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
               
               
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            if (!IsMainVariant) file = FolderImage + "\\32a - (BS8666-2005).png";

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(380 - Af.Width/2, 19 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            if(IsMainVariant) picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Cpos = new PointF(500 - Cf.Width / 2, 242 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                
            PointF Bpos = new PointF(203,132 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Dpos = new PointF(718, 199 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            if (IsMainVariant) picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);            
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0 && !IsMainVariant)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X, p_start.Y - 50);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 && !IsMainVariant)
            {
                if (HookOrientationEnd == RebarHookOrientation.Left) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №33
    /// </summary>
    class Form33
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(571, 199);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(571, 93);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form33(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
         
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {
               
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();

            }
            
            string file = FolderImage + image;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(500 - Af.Width/2, 240 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(670 - Cf.Width/2 , SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Bpos = new PointF(160,144 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);

            
                // при наличии крюков
                if (Hook_start.IntegerValue > 0)
                {
                    StandartFormUtils.DrawHook(picture, p_end, Angle1, HookPosition.downleft, FolderHook);
                    // показать длину прямого участка крюка                 
                    PointF Hookpos = new PointF(p_end.X - Hook_length_start_f.Width, p_end.Y - 10);
                    if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }

                if (Hook_end.IntegerValue > 0)
                {
                    StandartFormUtils.DrawHook(picture, p_start, Angle2, HookPosition.upleft, FolderHook);
                    PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width, p_start.Y - 45 );
                    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }             
        }
    }

    /// <summary>
    /// Класс для создания формы №34
    /// </summary>
    class Form34
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_end = new PointF(145,214);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_start = new PointF(644,97);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                if (HookOrientationStart == RebarHookOrientation.Right) return false;
                if (Hook_start.IntegerValue > 0 && Angle1 == 90) return true;
                return false;
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form34(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "C")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            if (!IsMainVariant) file = FolderImage + "\\34a - (BS8666-2005).png";

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(234 - Af.Width/2, 242 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(364 - Bf.Width, 77 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(556 - Cf.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Dpos = new PointF(833, 158);
            PointF Epos = new PointF(700, 136);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            Epos = new PointF(-Epos.Y - Ef.Width / 2, Epos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 10);
            if (IsMainVariant) picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0 && !IsMainVariant)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y + 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y - 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №35
    /// </summary>
    class Form35
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(145, 201);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(644, 84);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                if (Hook_end.IntegerValue > 0 && Angle2 == 90) return true;
                return false;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form35(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            if (!IsMainVariant) file = FolderImage + "\\35a - (BS8666-2005).png";

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(224 - Af.Width / 2, 240 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(364 - Bf.Width, 60 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(640 - Cf.Width, -10 + SketchReinforcementApp.shift_font);
            if (!IsMainVariant) Cpos = new PointF(562 - Cf.Width/2, 128);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width -20, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Dpos = new PointF(833, 141 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(700, -10 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
            Epos = new PointF(-Epos.Y - Ef.Width, Epos.X);       // для поворота на 90
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 10);
            if (IsMainVariant) picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 60);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 && !IsMainVariant)
            {
                StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width / 2, p_end.Y - 80);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №36
    /// </summary>
    class Form36
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(738, 127);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(538,94);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Проверка основного варианта
        /// </summary>
        bool IsMainVariant
        {
            get
            {
                if (Hook_end.IntegerValue > 0 && Angle2 == 90) return true;
                return false;
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form36(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;             

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(500 - Bf.Width/2, 241 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Dpos = new PointF(448 - Df.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 20);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);

            PointF Apos = new PointF(600, 140 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(214, 151 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(816, 168 + SketchReinforcementApp.shift_font);
            Apos = new PointF(Apos.X * 0.707f - Apos.Y * 0.707f - Af.Width / 4 * 0.707f, Apos.X * 0.707f + Apos.Y * 0.707f - Af.Height / 2 * 0.707f);       // для поворота на 45
            // Apos = new PointF(Apos.X * 0.707f - Apos.Y * 0.707f, Apos.X * 0.707f + Apos.Y * 0.707f - Af.Height / 2 * 0.707f);       // для поворота на 45
            picture.RotateTransform(-45);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            picture.RotateTransform(45);

            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90  
            Epos = new PointF(-Epos.Y - Ef.Width /2, Epos.X);           // для поворота на 90
            picture.RotateTransform(-90);            
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 20, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 10);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                //picture.FillRectangle(Brushes.White, 726, 123 , 15,15);
                if (HookOrientationStart == RebarHookOrientation.Right)
                {
                    PointF p = p_start;
                    if (Angle1 == 90) p = new PointF(p.X - 28, p.Y + 24);
                    if (Angle1 == 135) p = new PointF(p.X - 18, p.Y + 33);
                    if (Angle1 == 180) p = new PointF(p.X - 28, p.Y + 34);
                    StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_up_45, FolderHook);
                }
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft_45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - 20, p_start.Y - 70);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 )
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, new PointF(p_end.X+2,p_end.Y), Angle2, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №41
    /// </summary>
    class Form41
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(433, 105);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(585,137);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form41(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            if (SketchTools.CompareDoubleMore(D, B))
            {
                double temp = B; B = D; D = temp;
                temp = A; A = E; E = temp;
                ElementId eid = Hook_start; Hook_start = Hook_end; Hook_end = eid;
                orient_hook = orient_hook ? false : true;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(356 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);             
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Epos = new PointF(680 - Ef.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 20);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            PointF Cpos = new PointF(510 - Cf.Width / 2, 239 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Bpos = new PointF(155, 152 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Dpos = new PointF(890, 169 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);


            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.up, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width + 20, p_start.Y + 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.downleft, FolderHook);
                PointF Hookpos = new PointF(p_end.X + 30, p_end.Y);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №44
    /// </summary>
    class Form44
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(228,105);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(758,121);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form44(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            if (SketchTools.CompareDoubleMore(D, B))
            {
                double temp = B; B = D; D = temp;
                temp = A; A = E; E = temp;
                ElementId eid = Hook_start; Hook_start = Hook_end; Hook_end = eid;
            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(290 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Epos = new PointF(710 - Ef.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 20);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            PointF Cpos = new PointF(505 - Cf.Width / 2, 240 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Bpos = new PointF(102, 142 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            PointF Dpos = new PointF(814, 156 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);


            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - 20, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else 
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №46S (Словакия - по осям стержня)
    /// </summary>
    class Form46S
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(180, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(718, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Угол загиба
        /// </summary>
        double Alfa;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Alfastr
        {
            get
            {
                string s = Alfa.ToString();
                if (Alfa == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Alfaf
        {
            get
            { return picture.MeasureString(Alfastr, SketchReinforcementApp.drawFont); }
        }

        string Gradstr = "o";
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Gradf
        {
            get
            { return picture.MeasureString(Gradstr, SketchReinforcementApp.drawFontG); }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для градусов
        ///// </summary>
        //Font drawFontG = new Font("Mipgost", 26);
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form46S(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;

            RebarShapeDrivenAccessor rsda = rebar.GetShapeDrivenAccessor();
            IList<Curve> ILrsda= rsda.ComputeDrivingCurves();

            if (ILrsda.Count != 5) return;   // число участков равно 5

            XYZ normal=rsda.Normal;
            XYZ p1 = ILrsda[0].GetEndPoint(0);
            XYZ p2 = ILrsda[0].GetEndPoint(1);
            XYZ p3 = ILrsda[0].GetEndPoint(0)+normal;

            XYZ proj = SketchTools.ProjectPointOnWorkPlane(p1, p2, p3, ILrsda[1].GetEndPoint(1));
             
            if(proj==null) return;   // проекции нет

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                A = ILrsda[0].Length;
                B = ILrsda[1].Length;
                C = ILrsda[2].Length;
                E = ILrsda[4].Length;             
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                E = ILrsda[0].Length;
                B = ILrsda[1].Length;
                C = ILrsda[2].Length;
                A = ILrsda[4].Length;
            }

            // Расчетные параметры D и F
            D = ILrsda[1].GetEndPoint(1).DistanceTo(proj);
            F = ILrsda[0].GetEndPoint(1).DistanceTo(proj);
            XYZ part1 = (ILrsda[0].GetEndPoint(1) - ILrsda[0].GetEndPoint(0)).Normalize();
            XYZ part2 = (ILrsda[1].GetEndPoint(1) - ILrsda[1].GetEndPoint(0)).Normalize();
            Alfa = part1.AngleTo(part2);
            Alfa = 180 - Math.Round(180/Math.PI * Alfa, 0);
                
          //foreach (Parameter pr in pset)
          //{
          //    if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
          //    if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
          //    if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
          //    if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
          //    if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
          //}

          string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Alfapos = new PointF(240 - Alfaf.Width / 2, 130 + SketchReinforcementApp.shift_font);
            // picture.FillRectangle(Brushes.White, Alfapos.X, Alfapos.Y, Alfaf.Width, Alfaf.Height - 20);
            picture.DrawString(Alfastr, SketchReinforcementApp.drawFontH, Brushes.Black, Alfapos);

            PointF Gradpos = new PointF(195 + Alfaf.Width/2, 125 + SketchReinforcementApp.shift_font);           
            picture.DrawString(Gradstr, SketchReinforcementApp.drawFontG, Brushes.Black, Gradpos);


            PointF Apos = new PointF(242 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Bpos = new PointF(365, 81 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            PointF Epos = new PointF(653 - Ef.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 20);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            PointF Cpos = new PointF(444 - Cf.Width / 2, 218 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Fpos = new PointF(520 , 218 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Fpos.X, Fpos.Y, Ff.Width, Ff.Height - 20);
            picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);


            PointF Dpos = new PointF(781, 141 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);

            // угол наклона отрезка

          

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №M_22
    /// </summary>
    class FormM_22
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(175, 101);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(730, 101);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня H 
        /// </summary>
        double H;
        /// Сегмент стержня K 
        /// </summary>
        double K;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня H 
        /// </summary>
        string Hstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, H);
                if (H == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня H 
        /// </summary>
        SizeF Hf
        {
            get
            { return picture.MeasureString(Hstr, SketchReinforcementApp.drawFont); }
        }

        
        /// <summary>
        /// Сегмент стержня K
        /// </summary>
        string Kstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, K);
                if (K == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня H 
        /// </summary>
        SizeF Kf
        {
            get
            { return picture.MeasureString(Kstr, SketchReinforcementApp.drawFont); }
        }


        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }


        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию B</param>
        /// <param name="seg2">Имя сегмента по умолчанию C</param>
        /// <param name="seg3">Имя сегмента по умолчанию D</param>
        /// <param name="seg4">Имя сегмента по умолчанию E</param>
        /// <param name="seg5">Имя сегмента по умолчанию F</param>
        ///  <param name="seg6">Имя сегмента по умолчанию H</param>
        ///  <param name="seg7">Имя сегмента по умолчанию K</param>
        public FormM_22(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="B", string seg2 = "C", string seg3 = "D", string seg4 = "E", string seg5 = "F", string seg6 = "H", string seg7 = "K")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;           
            
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                 
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower()) D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg5 || pr.Definition.Name == seg5.ToLower()) F = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg6 || pr.Definition.Name == seg6.ToLower()) H = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg7 || pr.Definition.Name == seg7.ToLower()) K = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(242 - Bf.Width / 2, -10 + SketchReinforcementApp.shift_font);             
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(365,75);           
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                       
            PointF Fpos = new PointF(653 - Ff.Width / 2, -10 + SketchReinforcementApp.shift_font);            
            picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);
            
            PointF Dpos = new PointF(444 - Df.Width / 2, 218 + SketchReinforcementApp.shift_font);            
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
                       
             
            PointF Kpos = new PointF(781, 141 + SketchReinforcementApp.shift_font);
            Kpos = new PointF(-Kpos.Y - Kf.Width / 2, Kpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);      
            picture.DrawString(Kstr, SketchReinforcementApp.drawFont, Brushes.Black, Kpos);

            PointF Hpos = new PointF(25, 141 + SketchReinforcementApp.shift_font);
            Hpos = new PointF(-Hpos.Y - Hf.Width / 2, Hpos.X);       // для поворота на 90            
            picture.DrawString(Hstr, SketchReinforcementApp.drawFont, Brushes.Black, Hpos);

            picture.RotateTransform(90);


            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width/2, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }



    class Form46
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(180, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(718, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию А</param>
        /// <param name="seg2">Имя сегмента по умолчанию В</param>
        /// <param name="seg3">Имя сегмента по умолчанию С</param>
        /// <param name="seg4">Имя сегмента по умолчанию D</param>
        /// <param name="seg5">Имя сегмента по умолчанию E</param>
        public Form46(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "A", string seg2 = "B", string seg3 = "C", string seg4 = "D", string seg5 = "E")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg5 || pr.Definition.Name == seg5.ToLower()) E = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(242 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Bpos = new PointF(365, 81 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            PointF Epos = new PointF(653 - Ef.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 20);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            PointF Cpos = new PointF(444 - Cf.Width / 2, 218 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);


            PointF Dpos = new PointF(781, 141 + SketchReinforcementApp.shift_font);
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            picture.RotateTransform(90);


            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №M_14
    /// </summary>
    class FormM_14
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(784, 22);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(198, 22);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня O
        /// </summary>
        double O;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня O
        /// </summary>
        string Ostr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, O);
                if (O == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня O
        /// </summary>
        SizeF Of
        {
            get
            { return picture.MeasureString(Ostr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
       
        public FormM_14(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;          
            rebar = element as Rebar;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "O" || pr.Definition.Name == "o") O = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

                      

            PointF Bpos = new PointF(740, 160 + SketchReinforcementApp.shift_font);           
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Cpos = new PointF(480 - Cf.Width / 2, 225 + SketchReinforcementApp.shift_font);           
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);

            PointF Dpos = new PointF(210 - Df.Width, 160 + SketchReinforcementApp.shift_font);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);

            PointF Opos = new PointF(480 - Of.Width / 2, SketchReinforcementApp.shift_font);
            picture.DrawString(Ostr, SketchReinforcementApp.drawFont, Brushes.Black, Opos);

            PointF Apos = new PointF(978 - Af.Width/2, 25 + SketchReinforcementApp.shift_font);
            Apos = new PointF(-Apos.Y - Af.Width*3/4, Apos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Epos = new PointF(70 - Ef.Width / 2, 25 + SketchReinforcementApp.shift_font);
            Epos = new PointF(-Epos.Y - Ef.Width*3/4, Epos.X);       // для поворота на 90 
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);

            picture.RotateTransform(90);

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }
    /// <summary>
    /// Класс для создания формы №47
    /// </summary>
    class Form47
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(180, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(717, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "("+s+")";
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form47(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            // string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            //// получить параметры формы
            //if (segment1 == "A")
            //{

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //}
            //else
            //{

            //    Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            //    Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            //}

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                //if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file="";
            if (Angle1 == 180 && Angle2 == 180)
            {
                if(HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                file = FolderImage + image;
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47rr - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
            }
            if (Angle1 == 135 && Angle2 == 135)
            {
                if (HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                {
                    image = "47aa - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47aarr - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
                
            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                if (HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                {
                    image = "47bb - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47bbrr - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }

            }
            if ((Angle1 == 0 && Angle2 == 90) || (Angle1 == 90 && Angle2 == 0))
            {
                if (C == 0) C = D;

                if (HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                {
                    image = "47b - (BS8666-2005).png";                    
                    file = FolderImage + "\\" + image;
                }
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47br - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }

            }
            if ((Angle1 == 0 && Angle2 == 135) || (Angle1 == 135 && Angle2 == 0))
            {
                if (C == 0) C = D;
                if (HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                {
                    image = "47a - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47ar - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }

            }
            if ((Angle1 == 0 && Angle2 == 180) || (Angle1 == 180 && Angle2 == 0))
            {
                if (C == 0) C = D;
                if (HookOrientationStart == RebarHookOrientation.Left && HookOrientationEnd == RebarHookOrientation.Left)
                {
                    image = "47c - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }
                if (HookOrientationStart == RebarHookOrientation.Right && HookOrientationEnd == RebarHookOrientation.Right)
                {
                    image = "47cr - (BS8666-2005).png";
                    file = FolderImage + "\\" + image;
                }

            }

            if (Angle1 == 0 && Angle2 == 0 )
            {
                image = "470 - (BS8666-2005).png";                 
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(500 - Bf.Width / 2, 243 + SketchReinforcementApp.shift_font);
            PointF Apos = new PointF(48, 133 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(420,91 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(542,91 + SketchReinforcementApp.shift_font);

            switch (image)
            {
                case "47b - (BS8666-2005).png":
                Bpos = new PointF(500 - Bf.Width / 2, 243 + SketchReinforcementApp.shift_font);
                // Apos = new PointF(48, 152);
                Cpos = new PointF(304 - Cf.Width /2, 6 + SketchReinforcementApp.shift_font);
                Dpos = new PointF(717 - Df.Width /2, 6 + SketchReinforcementApp.shift_font);
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                break;
                case "47br - (BS8666-2005).png":
                Bpos = new PointF(500 - Bf.Width / 2, 243 + SketchReinforcementApp.shift_font);
                //Apos = new PointF(130, 152);
                Cpos = new PointF(227 - Cf.Width / 2, 6 + SketchReinforcementApp.shift_font);
                Dpos = new PointF(717 - Df.Width / 2, 6 + SketchReinforcementApp.shift_font);
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                break;
                case "47bb - (BS8666-2005).png":
                Bpos = new PointF(500 - Bf.Width / 2, 243 + SketchReinforcementApp.shift_font);
                //Apos = new PointF(130, 152);
                Cpos = new PointF(304 - Cf.Width /2, 6 + SketchReinforcementApp.shift_font);
                Dpos = new PointF(717 - Df.Width /2, 6 + SketchReinforcementApp.shift_font);
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 20);
                picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
                    break;
                case "47bbrr - (BS8666-2005).png":
                    Bpos = new PointF(500 - Bf.Width / 2, 243 + SketchReinforcementApp.shift_font);
                    //Apos = new PointF(130, 152);
                    Cpos = new PointF(227 - Cf.Width / 2, 6 + SketchReinforcementApp.shift_font);
                    Dpos = new PointF(793 - Df.Width / 2, 6 + SketchReinforcementApp.shift_font);
                    //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                    picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                    //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 20);
                    picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
                    break;
                default:
                Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90 
                Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
                if (image != "470 - (BS8666-2005).png")
                {
                picture.RotateTransform(-90);                 
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 10);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                picture.RotateTransform(90);
                }
                break;
            }


            //if (image == "47b - (BS8666-2005).png" || image == "47bb - (BS8666-2005).png")
            //{
            //    Bpos = new PointF(500 - Bf.Width / 2, 243);
            //    Apos = new PointF(130, 152);
            //    Cpos = new PointF(304 - Cf.Width /2, 6);
            //    Dpos = new PointF(717 - Df.Width /2, 6);
            //    picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
            //    picture.DrawString(Cstr, drawFont, Brushes.Black, Cpos);
            //    if (image == "47bb - (BS8666-2005).png")
            //    {
            //        picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 20);
            //        picture.DrawString(Dstr, drawFont, Brushes.Black, Dpos);
            //    }
            //}
            //else
            //{
            //    Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90 
            //    Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            //    if (image != "470 - (BS8666-2005).png")
            //    {
            //    picture.RotateTransform(-90);                 
            //    picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 10);
            //    picture.DrawString(Cstr, drawFont, Brushes.Black, Cpos);
            //    picture.RotateTransform(90);
            //    }
            //}

            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos); 
            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90            
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            if (image == "47a - (BS8666-2005).png" || image == "47b - (BS8666-2005).png" || image == "47c - (BS8666-2005).png" || image == "47ar - (BS8666-2005).png" || image == "47br - (BS8666-2005).png" || image == "47cr - (BS8666-2005).png")
            {
                // D - не показываем
            }
            else
            {
                if (image != "470 - (BS8666-2005).png")
                {
                    //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 10);
                    picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
                }
            }
            picture.RotateTransform(90);


            //// при наличии крюков
            //if (Hook_start.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
            //    // показать длину прямого участка крюка                 
            //    PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 30);
            //    if (show_length_hooks) picture.DrawString(Hook_length_start, drawFontH, Brushes.Black, Hookpos);
            //}

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}

        }
    }

    /// <summary>
    /// Класс для создания формы №51
    /// </summary>
    class Form51
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(442,110);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(397, 45);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double Cmin;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double Dmin;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double Emin;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0) 
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Cmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Cmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")"; 
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Dmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Dmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")"; 
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Emin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Emin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form51(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin); // A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = SketchTools.GetMaxMinValue(rebar, pr, out Cmin);
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = SketchTools.GetMaxMinValue(rebar, pr, out Dmin);               
            }

            if (SketchTools.CompareDoubleMore(A, B))
            {
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
                temp = Cmin; Cmin = Dmin; Dmin = temp;
                temp = C; C = D; D = temp;
                ElementId eid = Hook_start; Hook_start = Hook_end; Hook_end = eid;
            }
            string file = ""; 
            if (Angle1 == 135 && Angle2 == 135)
            {
                file = FolderImage + image; 
                if (C == 0 || D == 0)
                {

                    switch (image)
                    {
                        case "51 - (BS8666-2005).png":
                        image = "51CD - (BS8666-2005).png";                         
                        break;
                        default :
                        //// путь к папке рисунков
                        //FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Default";
                        // значение А и В уменьшаем на 2*диаметр стержня
                        A = A - 2*rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                        B = B - 2*rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                        Amin = Amin - 2*rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                        Bmin = Bmin - 2*rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
                        image = "10CD - (Gost21-501).png";
                        break;
                    }
                    file = FolderImage + "\\" + image;
                }
                

            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "51a - (BS8666-2005).png";
                if (C == 0 || D == 0) image = "51a0 - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "51b - (BS8666-2005).png";
                if (C == 0 || D == 0) image = "51b0 - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
             
            if (Angle1 == 0 && Angle2 == 0) 
            {
                image = "510 - (BS8666-2005).png";                
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(500 - Bf.Width / 2, 227 + SketchReinforcementApp.shift_font);
            PointF Apos = new PointF(675, 149 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(275, 98 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(392 - Df.Width / 2, SketchReinforcementApp.shift_font);

            Cpos = new PointF(-Cpos.Y - Cf.Width*0.9f, Cpos.X);       // для поворота на 90 
            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90

            if (Angle1 != 0 && D>0)
            {
                //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 10);
                picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            }
            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            if (Angle1 != 0 && C>0)
            {
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            }
            picture.RotateTransform(90);



            if (C == 0 && D == 0)
            {
                // при наличии крюков
                if (Hook_start.IntegerValue > 0 )
                {
                    // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                    // показать длину прямого участка крюка                 
                    PointF Hookpos = new PointF(p_start.X - 20, p_start.Y);
                    if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
                }

                //if (Hook_end.IntegerValue > 0)
                //{
                //    // StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                //    PointF Hookpos = new PointF(p_end.X, p_end.Y);
                //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
                //}
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №51T
    /// </summary>
    class Form51T
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(442, 110);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(397, 45);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double Amin;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double Bmin;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double Cmin;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double Dmin;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double Emin;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Amin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Amin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Bmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Bmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Cmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Cmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Dmin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Dmin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Estr
        {
            get
            {
                string smin = SketchTools.GetRoundLenghtSegment(rebar, Emin);
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (Emin > 0)
                {
                    if (smin.Length < 2) return smin;
                    if (smin.Substring(0, 2) == "0.") smin = smin.Substring(1);
                    s = s + "..." + smin;
                }
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 36);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form51T(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }



            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = SketchTools.GetMaxMinValue(rebar, pr, out Amin); // A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = SketchTools.GetMaxMinValue(rebar, pr, out Bmin);
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = SketchTools.GetMaxMinValue(rebar, pr, out Cmin);
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = SketchTools.GetMaxMinValue(rebar, pr, out Dmin);
            }

            if (SketchTools.CompareDoubleMore(A, B))
            {
                double temp = A; A = B; B = temp;
                temp = Amin; Amin = Bmin; Bmin = temp;
                temp = Cmin; Cmin = Dmin; Dmin = temp;
                temp = C; C = D; D = temp;
                ElementId eid = Hook_start; Hook_start = Hook_end; Hook_end = eid;
            }
            string file = "";

            // значение А и В уменьшаем на 2*диаметр стержня
            A = A - 2 * rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            B = B - 2 * rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            Amin = Amin - 2 * rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            Bmin = Bmin - 2 * rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();

            //// путь к папке рисунков
            //FolderImage = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "Default"; 

            if (Angle1 == 135 && Angle2 == 135)
            {
                file = FolderImage + image;
                if (C == 0)
                {                                     
                            image = "10CD - (Gost21-501).png";                  
                            file = FolderImage + "\\" + image;
                }


            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "10T180 - (Gost21-501).png";
                if (C == 0) image = "10TC180 - (Gost21-501).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "10T90 - (Gost21-501).png";
                if (C == 0) image = "10TC90 - (Gost21-501).png";
                file = FolderImage + "\\" + image;

            }

            if (Angle1 == 0 && Angle2 == 0)
            {
                image = "10T0 - (Gost21-501).png";
                if (C == 0) image = "10TC0 - (Gost21-501).png";
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Bpos = new PointF(500 - Bf.Width / 2, 227 + SketchReinforcementApp.shift_font);
            PointF Apos = new PointF(675, 149 + SketchReinforcementApp.shift_font);
            PointF CposV = new PointF(415 - Cf.Width /2, 1 + SketchReinforcementApp.shift_font);
            PointF CposH = new PointF(270, 139 + SketchReinforcementApp.shift_font);
      
            CposH = new PointF(-CposH.Y - Cf.Width /2 , CposH.X); // для поворота на 90 
            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90
                      
            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            if (C > 0)
            {                
                //picture.FillRectangle(Brushes.White, CposV.X, CposV.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, CposV);
            }

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            if (C > 0)
            {
                //picture.FillRectangle(Brushes.White, CposH.X, CposH.Y, Cf.Width, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, CposH);             
            }
            picture.RotateTransform(90);



            //if (C == 0 && D == 0)
            //{
            //    // при наличии крюков
            //    if (Hook_start.IntegerValue > 0)
            //    {
            //        // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
            //        // показать длину прямого участка крюка                 
            //        PointF Hookpos = new PointF(p_start.X - 20, p_start.Y);
            //        if (show_length_hooks) picture.DrawString(Hook_length_start, drawFontH, Brushes.Black, Hookpos);
            //    }

            //    //if (Hook_end.IntegerValue > 0)
            //    //{
            //    //    // StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
            //    //    PointF Hookpos = new PointF(p_end.X, p_end.Y);
            //    //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //    //}
            //}

        }
    }

    /// <summary>
    /// Класс для создания формы №56
    /// </summary>
    class Form56
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(180, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(717, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public Form56(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C", string seg4="D")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;


            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) D = rebar.get_Parameter(pr.Definition).AsDouble();

                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "F" || pr.Definition.Name == "f") F = rebar.get_Parameter(pr.Definition).AsDouble();
            }
                        
            string file = "";
             
            if (Angle1 == 135 && Angle2 == 135)
            {
                file = FolderImage + image;
            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "M-T7a (ESP).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "M-T7b (ESP).png";
                file = FolderImage + "\\" + image;

            }

            if (Angle1 == 0 && Angle2 == 0)
            {
                image = "M-T70 (ESP).png";
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);
            PointF Apos = new PointF(515 - Af.Width / 2, 240 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(500 - Cf.Width / 2, -12 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(710, 40 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(395, 160 + SketchReinforcementApp.shift_font);

            PointF Bpos = new PointF(150,130 + SketchReinforcementApp.shift_font);
            PointF Fpos = new PointF(246,133 + SketchReinforcementApp.shift_font);
            Bpos = new PointF(-Bpos.Y - Bf.Width /2 , Bpos.X);       // для поворота на 90 
            Fpos = new PointF(-Fpos.Y - Ff.Width / 2, Fpos.X);       // для поворота на 90
            
            if (Angle1 != 0)
            {
                //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width - 10, Ef.Height - 10);
                picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            }

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);


            

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            if (Angle1 != 0)
            {
                //picture.FillRectangle(Brushes.White, Fpos.X, Fpos.Y, Ff.Width, Ff.Height - 10);
                picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);
            }
            picture.RotateTransform(90);

            //// при наличии крюков
            //if (Hook_start.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
            //    // показать длину прямого участка крюка                 
            //    PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 30);
            //    if (show_length_hooks) picture.DrawString(Hook_length_start, drawFontH, Brushes.Black, Hookpos);
            //}

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}

        }
    }


    /// <summary>
    /// Класс для создания формы №M_T7
    /// </summary>
    class FormM_T7
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(300, 95);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(452,123);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public FormM_T7(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "B", string seg2 = "C", string seg3 = "D", string seg4 = "E")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;


            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) E = rebar.get_Parameter(pr.Definition).AsDouble();

            
            }

            string file = "";
            file = FolderImage + image;
            
            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);
            
            
            

            PointF Bpos = new PointF(370 - Bf.Width, 140 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(570 - Cf.Width / 2, 225 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(710, 40 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(450 - Ef.Width / 2, -8 + SketchReinforcementApp.shift_font);

            // Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90            
             
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);            
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);            
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
                                 
            // picture.RotateTransform(-90);            
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);             
            // picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }




    /// <summary>
    /// Класс для создания формы №M_T8
    /// </summary>
    class FormM_T8
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(327, 109);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(420, 171);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        /// <param name="seg3">Имя сегмента</param>
        /// <param name="seg4">Имя сегмента</param>
        public FormM_T8(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1 = "B", string seg2 = "C", string seg3 = "D", string seg4 = "E")
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;


            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == seg1 || segment1 == seg1.ToLower() || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) E = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = "";
            file = FolderImage + image;

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);




            PointF Bpos = new PointF(107, 160 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(480 - Ef.Width / 2, -8 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(710, 40 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(550 - Cf.Width / 2, 205 + SketchReinforcementApp.shift_font);

            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90            

            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);

            picture.RotateTransform(-90);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.upleft, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}

        }
    }

    /// <summary>
    /// Класс для создания формы №63
    /// </summary>
    class Form63
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(180, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(717, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form63(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();               
            }

            string file = "";
            if (Angle1 == 135 && Angle2 == 135)
            {
                file = FolderImage + image;
            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "63a - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "63b - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }

            if (Angle1 == 0 && Angle2 == 0)
            {
                image = "630 - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);
            PointF Apos = new PointF(233, 183 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(500 - Bf.Width / 2, -3 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(335,116 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(625,118 + SketchReinforcementApp.shift_font);
         
            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90 
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90

            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            picture.RotateTransform(-90);
            if (Angle1 != 0)
            {
                //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
                picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
                //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 10);
                picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            }

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            picture.RotateTransform(90);
        }
    }

    /// <summary>
    /// Класс для создания формы №64
    /// </summary>
    class Form64
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(360, 158);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(717, 102);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                if (SketchReinforcementApp.lt.ToString() == "Russian") return s;
                return "(" + s + ")";
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form64(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;


            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "F" || pr.Definition.Name == "f") F = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = "";
            if (Angle2 == 180)
            {
                file = FolderImage + image;
            }
            if (Angle2 == 135)
            {
                image = "64a - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle2 == 90)
            {
                image = "64b - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }

            if (Angle2 == 0)
            {
                image = "64c - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);
            PointF Apos = new PointF(170,118 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(469 - Bf.Width / 2, 222 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(905,136 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(500 - Df.Width / 2, -10 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(800,158 + SketchReinforcementApp.shift_font);            
            PointF Fpos = new PointF(690,145 + SketchReinforcementApp.shift_font);

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90 
            Fpos = new PointF(-Fpos.Y - Ff.Width / 2, Fpos.X);       // для поворота на 90
            Epos = new PointF(-Epos.Y - Ef.Width / 2, Epos.X);       // для поворота на 90
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width - 10, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            picture.RotateTransform(-90);
           
            if (Angle2 != 0)
            {
                //picture.FillRectangle(Brushes.White, Fpos.X, Fpos.Y, Ff.Width, Ff.Height - 10);
                picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);
            }

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width - 10, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width - 10, Ef.Height - 10);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);

            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft, FolderHook);
                else StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_down, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 30, p_start.Y - 30);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + 30);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}

        }
    }

    /// <summary>
    /// Класс для создания формы №67
    /// </summary>
    class Form67
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(348,170);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(647,170);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

         

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return "R "+s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента по умолчанию А</param>
        /// <param name="seg2">Имя сегмента по умолчанию В</param>
        /// <param name="seg3">Имя сегмента по умолчанию С</param>
        /// <param name="seg4">Имя сегмента по умолчанию R</param>
        /// 
        public Form67(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B", string seg3="C", string seg4="R")
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
             

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
 

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg3 || pr.Definition.Name == seg3.ToLower() || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg4 || pr.Definition.Name == seg4.ToLower()) R = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(366 - Af.Width, 10 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(500 - Bf.Width / 2, 240 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(740, 93 + SketchReinforcementApp.shift_font);
            PointF Rpos = new PointF(470 - Rf.Width, 170 + SketchReinforcementApp.shift_font);

            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90           

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Rpos.X, Rpos.Y + 10, Rf.Width, Rf.Height - 30);
            picture.DrawString(Rstr, SketchReinforcementApp.drawFont, Brushes.Black, Rpos);

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);
            
            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.arc, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X + 20, p_start.Y - 80);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.arc_left, FolderHook);
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 5, p_end.Y - 80);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №75
    /// </summary>
    class Form75
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(560, 126);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(647, 170);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }



        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return "R " + s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }


        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form75(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A", string seg2="B")
        {
            // путь к папке рисунков
            FolderImage = FolderImage.Replace("Gost21-501","BS8666-2005");

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;


            ParameterSet pset = rebar.Parameters;
             

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                       

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
             
            }

             
            string file = "";
            if (Angle1 == 0 && Angle2 == 0)
            {
                file = FolderImage + image;
            }

           
            if (Angle1 == 135 && Angle2 == 135)
            {
                image = "75b - (BS8666-2005).png";
                file = FolderImage +  image;

            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "75a - (BS8666-2005).png";
                file = FolderImage +  image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "75c - (BS8666-2005).png";
                file = FolderImage +  image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(274, 134 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(693, 134 + SketchReinforcementApp.shift_font);
           

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90           
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
             

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.arc, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width + 10, p_start.Y - 15);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.arc_left, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 5, p_end.Y - 80);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}
        }
    }

    /// <summary>
    /// Класс для создания формы №87
    /// </summary>
    class Form87
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(560, 126);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(647, 170);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }



        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return "R " + s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form87(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            //string kode_form = "14";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            //string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            //// получить параметры формы
            //if (segment1 == "A")
            //{
            //    foreach (Parameter pr in pset)
            //    {
            //        if (pr.Definition.Name == "A") A = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            //    }

            //}
            //else
            //{
            //    foreach (Parameter pr in pset)
            //    {
            //        if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "A") C = rebar.get_Parameter(pr.Definition).AsDouble();
            //    }
            //}

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "R" || pr.Definition.Name == "r") A = 2 * rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();

            }


            string file = "";
            if (Angle1 == 0 && Angle2 == 0)
            {
                file = FolderImage + image;
            }
            if (Angle1 == 135 && Angle2 == 135)
            {
                image = "87b - (Chili).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 180 && Angle2 == 180)
            {
                image = "87a - (Chili).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 && Angle2 == 90)
            {
                image = "87c - (Chili).png";
                file = FolderImage + "\\" + image;

            }

            if (file == "") return;
            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(274, 134 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(693, 134 + SketchReinforcementApp.shift_font);


            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90           
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90


            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(90);    

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.arc, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width + 10, p_start.Y - 15);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            //if (Hook_end.IntegerValue > 0)
            //{
            //    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.arc_left, FolderHook);
            //    PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 5, p_end.Y - 80);
            //    if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
            //}
        }
    }

    /// <summary>
    /// Класс для создания формы №77
    /// </summary>
    class Form77
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(266, 179);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(647, 170);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }



        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return "R " + s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        /// <param name="seg1">Имя сегмента</param>
        /// <param name="seg2">Имя сегмента</param>
        public Form77(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook, string seg1="A",string seg2="B")
        {
            // путь к папке рисунков
            FolderImage = FolderImage.Replace("Gost21-501", "BS8666-2005");

            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
             

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                        

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == seg1 || pr.Definition.Name == seg1.ToLower() || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == seg2 || pr.Definition.Name == seg2.ToLower() || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();

            }


            string file = "";
            if (Angle1 == 0 || Angle2 == 0)
            {
                file = FolderImage + image;
            }

            //// путь к папке рисунков
            //string FolderImageDefault = FolderImage.Substring(0, FolderImage.LastIndexOf("\\")) + "\\" + "BS8666-2005";

            if (Angle1 == 135 || Angle2 == 135)
            {
                image = "77b - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 180 || Angle2 == 180)
            {
                image = "77a - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 || Angle2 == 90)
            {
                image = "77c - (BS8666-2005).png";
                file = FolderImage + "\\" + image;

            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(69,99 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(590 - Bf.Width / 2, 220 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90           
            
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.arc, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width/2, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 && Hook_start.IntegerValue < 0)
            {
                // StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.arc_left, FolderHook);
                PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width / 2, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №77 Chili
    /// </summary>
    class Form77Chili
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(266, 179);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(647, 170);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        double R;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }



        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня R
        /// </summary>
        string Rstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, R);
                if (R == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return "R " + s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Rf
        {
            get
            { return picture.MeasureString(Rstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form77Chili(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            //string kode_form = "14";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            //string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            //// получить параметры формы
            //if (segment1 == "A")
            //{
            //    foreach (Parameter pr in pset)
            //    {
            //        if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            //    }

            //}
            //else
            //{
            //    foreach (Parameter pr in pset)
            //    {
            //        if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
            //        if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            //    }
            //}

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();              

            }


            string file = "";
            if (Angle1 == 0 || Angle2 == 0)
            {
                file = FolderImage + image;
            }
            if (Angle1 == 135 || Angle2 == 135)
            {
                image = "77b - (Chili).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 180 || Angle2 == 180)
            {
                image = "77a - (Chili).png";
                file = FolderImage + "\\" + image;

            }
            if (Angle1 == 90 || Angle2 == 90)
            {
                image = "77c - (Chili).png";
                file = FolderImage + "\\" + image;

            }

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(77, 84 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(590 - Bf.Width / 2, 149 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            PointF Cpos = new PointF(590 - Cf.Width / 2, 230 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);


            Apos = new PointF(-Apos.Y - Af.Width / 2, Apos.X);       // для поворота на 90           

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                // StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.arc, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0 && Hook_start.IntegerValue < 0)
            {
                // StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.arc_left, FolderHook);
                PointF Hookpos = new PointF(p_start.X - Hook_length_end_f.Width / 2, p_start.Y);
                if (show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }

    /// <summary>
    /// Класс для создания формы №5 (ГОСТ)
    /// </summary>
    class Form5
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(336, 62);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(835, 213);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня C 
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }


        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form5(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            // string kode_form = "15";
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            //string folder_hook = FolderImage + "\\Hooks";
            rebar = element as Rebar;
            ParameterSet pset = rebar.Parameters;
            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента

           

            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();

            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            if (SketchTools.CompareDoubleMore(A, B))
            {
                double temp = A; A = B; B = temp;
                ElementId tempId = Hook_start; Hook_start = Hook_end; Hook_end = tempId;
                orient_hook = orient_hook ? false : true;
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(450, 50 + SketchReinforcementApp.shift_font);            
            PointF Bpos = new PointF(680 - Bf.Width / 2, 212 + SketchReinforcementApp.shift_font);
            PointF Cpos = new PointF(185, 130 + SketchReinforcementApp.shift_font);
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90           

            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y + 10, Af.Width, Af.Height - 30);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y + 10, Bf.Width, Bf.Height - 30);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y + 10, Cf.Width, Cf.Height - 30);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);

            // при наличии крюков
            if (Hook_start.IntegerValue > 0)
            {
                PointF p = new PointF(p_start.X, p_start.Y);
                if (Angle1 == 90)
                {
                    p = new PointF(p_start.X - 12, p_start.Y - 12);
                }
                //picture.FillRectangle(Brushes.White, 319,46, 15, 15);
                if (HookOrientationStart == RebarHookOrientation.Right)
                {                    
                    p = new PointF(p_start.X - 12, p_start.Y - 12);                     
                    StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_upleft45, FolderHook);
                }
                else StandartFormUtils.DrawHook(picture, p, Angle1, HookPosition.v_up45, FolderHook);
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width - 15, p_start.Y - 67);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                PointF p = new PointF(p_end.X, p_end.Y);
                if (HookOrientationEnd == RebarHookOrientation.Right)
                {
                    if (Angle2 > 90)
                    {
                        p = new PointF(p_end.X - 3 , p_end.Y);
                    }
                    StandartFormUtils.DrawHook(picture, p, Angle2, HookPosition.down, FolderHook); 
                }
                else StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                PointF Hookpos = new PointF(p_end.X, p_end.Y - 90);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }
        }
    }
    ///// <summary>
    ///// Класс для создания формы №7 (ГОСТ)
    ///// </summary>
    //class Form7
    //{
    //    #region Начальные координаты стержня
    //    /// <summary>
    //    /// Координаты начала 
    //    /// </summary>
    //    PointF p_start = new PointF(210, 104);
    //    /// <summary>
    //    /// Координаты конца
    //    /// </summary>
    //    PointF p_end = new PointF(858, 64);
    //    #endregion
    //    #region Сегменты стержня
    //    /// <summary>
    //    /// Сегмент стержня А 
    //    /// </summary>
    //    double A;
    //    /// <summary>
    //    /// Сегмент стержня B
    //    /// </summary>
    //    double B;
    //    /// <summary>
    //    /// Сегмент стержня C
    //    /// </summary>
    //    double C;
    //    /// <summary>
    //    /// Сегмент стержня D
    //    /// </summary>
    //    double D;
    //    /// <summary>
    //    /// Сегмент стержня E
    //    /// </summary>
    //    double E;

    //    #endregion
    //    #region Прочие параметры класса
    //    /// <summary>
    //    /// Сегмент стержня А 
    //    /// </summary>
    //    string Astr
    //    {
    //        get
    //        {
    //            string s = SketchTools.GetRoundLenghtSegment(rebar, A);
    //            if (A == 0) return s;
    //            if (s.Substring(0, 2) == "0.") s = s.Substring(1);
    //            return s;
    //        }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня А 
    //    /// </summary>
    //    SizeF Af
    //    {
    //        get
    //        { return picture.MeasureString(Astr, drawFont); }
    //    }
    //    /// <summary>
    //    /// Сегмент стержня B 
    //    /// </summary>
    //    string Bstr
    //    {
    //        get
    //        {
    //            string s = SketchTools.GetRoundLenghtSegment(rebar, B);
    //            if (B == 0) return s;
    //            if (s.Substring(0, 2) == "0.") s = s.Substring(1);
    //            return s;
    //        }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня B
    //    /// </summary>
    //    SizeF Bf
    //    {
    //        get
    //        { return picture.MeasureString(Bstr, drawFont); }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня C
    //    /// </summary>
    //    string Cstr
    //    {
    //        get
    //        {
    //            string s = SketchTools.GetRoundLenghtSegment(rebar, C);
    //            if (C == 0) return s;
    //            if (s.Substring(0, 2) == "0.") s = s.Substring(1);
    //            return s;
    //        }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня B
    //    /// </summary>
    //    SizeF Cf
    //    {
    //        get
    //        { return picture.MeasureString(Cstr, drawFont); }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня D
    //    /// </summary>
    //    string Dstr
    //    {
    //        get
    //        {
    //            string s = SketchTools.GetRoundLenghtSegment(rebar, D);
    //            if (D == 0) return s;
    //            if (s.Substring(0, 2) == "0.") s = s.Substring(1);
    //            return s;
    //        }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня E
    //    /// </summary>
    //    SizeF Df
    //    {
    //        get
    //        { return picture.MeasureString(Dstr, drawFont); }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня D
    //    /// </summary>
    //    string Estr
    //    {
    //        get
    //        {
    //            string s = SketchTools.GetRoundLenghtSegment(rebar, E);
    //            if (E == 0) return s;
    //            if (s.Substring(0, 2) == "0.") s = s.Substring(1);
    //            return s;
    //        }
    //    }

    //    /// <summary>
    //    /// Сегмент стержня D
    //    /// </summary>
    //    SizeF Ef
    //    {
    //        get
    //        { return picture.MeasureString(Estr, drawFont); }
    //    }
    //    /// <summary>
    //    /// Картинка для рисования 
    //    /// </summary>
    //    Graphics picture = null;
    //    /// <summary>
    //    /// Шрифт для крюков 
    //    /// </summary>
    //    Font drawFontH = new Font("Mipgost", 36);
    //    /// <summary>
    //    /// Шрифт для сегментов
    //    /// </summary>
    //    Font drawFont = new Font("Mipgost", 48);
    //    /// <summary>
    //    /// Документ проекта 
    //    /// </summary>
    //    Document doc;
    //    /// <summary>
    //    /// Исходный стержень 
    //    /// </summary>
    //    Rebar rebar;
    //    /// <summary>
    //    /// Картинка формы  
    //    /// </summary>
    //    public Bitmap bitmap = null;
    //    /// <summary>
    //    /// Угол 1 крюка
    //    /// </summary>
    //    int Angle1
    //    {
    //        get
    //        {
    //            RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
    //            if (rht == null) return 0;
    //            return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
    //        }
    //    }
    //    /// <summary>
    //    /// Угол 2 крюка
    //    /// </summary>
    //    int Angle2
    //    {
    //        get
    //        {
    //            RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
    //            if (rht == null) return 0;
    //            return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
    //        }
    //    }
    //    /// <summary>
    //    /// Признак одинаковых крюков
    //    /// </summary>
    //    bool IsHooksEqual
    //    {
    //        get
    //        {
    //            if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
    //            return false;
    //        }
    //    }
    //    /// <summary>
    //    /// Начальный крюк
    //    /// </summary>
    //    ElementId Hook_start;
    //    /// <summary>
    //    /// Конечный крюк
    //    /// </summary>
    //    ElementId Hook_end;
    //    /// <summary>
    //    /// Длина начального крюка
    //    /// </summary>
    //    string Hook_length_start
    //    {
    //        get
    //        {
    //            if (Hook_start.IntegerValue < 0) return "";
    //            return StandartFormUtils.GetHookLength(rebar, Hook_start);
    //        }
    //    }
    //    /// <summary>
    //    /// Длина конечного крюка
    //    /// </summary>
    //    string Hook_length_end
    //    {
    //        get
    //        {
    //            if (Hook_end.IntegerValue < 0) return "";
    //            return StandartFormUtils.GetHookLength(rebar, Hook_end);
    //        }
    //    }
    //    /// <summary>
    //    /// Длина надписи начального крюка
    //    /// </summary>
    //    SizeF Hook_length_start_f
    //    {
    //        get
    //        {
    //            return picture.MeasureString(Hook_length_start, drawFontH);
    //        }
    //    }
    //    /// <summary>
    //    /// Длина надписи конечного крюка
    //    /// </summary>
    //    SizeF Hook_length_end_f
    //    {
    //        get
    //        {
    //            return picture.MeasureString(Hook_length_end, drawFontH);
    //        }
    //    }
    //    #endregion

    //    /// <summary>
    //    /// Конструктор эскиза по стандарту 
    //    /// </summary>
    //    /// <param name="element">Арматурный стержень как элемент</param>
    //    /// <param name="image">Имя файла рисунка</param>
    //    /// <param name="show_length_hooks">Показать  длину крюков</param> 
    //    /// <param name="FolderImage">Папка для общих файлов</param> 
    //    /// <param name="UserFolderImage">Папка для файлов пользователя</param>
    //    public Form7(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
    //    {

    //        doc = element.Document;
    //        StringFormat drawFormat = new StringFormat();
    //        drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;           
    //        rebar = element as Rebar;
    //        string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
    //        ParameterSet pset = rebar.Parameters;
    //        // получить параметры формы
    //        if (segment1 == "A")
    //        {                
    //            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
    //            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                
    //        }
    //        else
    //        {                 
    //            Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
    //            Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                 
    //        }

    //        foreach (Parameter pr in pset)
    //        {
    //            if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
    //            if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) C = rebar.get_Parameter(pr.Definition).AsDouble();
    //            if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") D = rebar.get_Parameter(pr.Definition).AsDouble();
    //            if (pr.Definition.Name == "D" || pr.Definition.Name == "d") E = rebar.get_Parameter(pr.Definition).AsDouble();
    //            if (pr.Definition.Name == "E" || pr.Definition.Name == "e") B = rebar.get_Parameter(pr.Definition).AsDouble();
    //        }

    //        string file = UserFolderImage + image;

    //        FileInfo fileinfo = new FileInfo(file);
    //        if (fileinfo.Exists)
    //        {
    //            bitmap = new Bitmap(file);
    //        }
    //        else return;

    //        picture = Graphics.FromImage(bitmap);

    //        PointF Apos = new PointF(285, 30);
    //        picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
    //        picture.DrawString(Astr, drawFont, Brushes.Black, Apos);

    //        PointF Bpos = new PointF(700 - Bf.Width, 80);
    //        picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 20);
    //        picture.DrawString(Bstr, drawFont, Brushes.Black, Bpos);

    //        PointF Epos = new PointF(510 - Ef.Width / 2, 210);
    //        picture.FillRectangle(Brushes.White, Epos.X, Epos.Y + 10, Ef.Width, Ef.Height - 30);
    //        picture.DrawString(Estr, drawFont, Brushes.Black, Epos);

    //        PointF Dpos = new PointF(920, 133);
    //        PointF Cpos = new PointF(55, 133);
    //        Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90  
    //        Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90
    //        picture.RotateTransform(-90);
    //        picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
    //        picture.DrawString(Dstr, drawFont, Brushes.Black, Dpos);
    //        picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height);
    //        picture.DrawString(Cstr, drawFont, Brushes.Black, Cpos);
    //        picture.RotateTransform(90);

    //        // при наличии крюков
    //        if (Hook_start.IntegerValue > 0)
    //        {
    //            StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up45, FolderHook);
    //            // показать длину прямого участка крюка                 
    //            PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 70);
    //            if (show_length_hooks) picture.DrawString(Hook_length_start, drawFontH, Brushes.Black, Hookpos);
    //        }

    //        if (Hook_end.IntegerValue > 0)
    //        {
    //            StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft_45, FolderHook);
    //            PointF Hookpos = new PointF(p_end.X, p_end.Y - 70);
    //            if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, drawFontH, Brushes.Black, Hookpos);
    //        }

    //    }
    //}

    /// <summary>
    /// Класс для создания формы №13 (ГОСТ)
    /// </summary>
    class Form13Gost
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(179, 102);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(841, 101);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        double G;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }
        
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }
        
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        string Gstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, G);
                if (G == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        SizeF Gf
        {
            get
            { return picture.MeasureString(Gstr, SketchReinforcementApp.drawFont); }
        }
        
        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }



        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form13Gost(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;            
            rebar = element as Rebar;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar);        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "A" || segment1 == "a" || segment1 == "Аdef" || segment1 == "Аdef")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "F" || pr.Definition.Name == "f") F = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "G" || pr.Definition.Name == "g") G = rebar.get_Parameter(pr.Definition).AsDouble();
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(242 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Bpos = new PointF(360, 65 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X + 10, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);

            PointF Dpos = new PointF(624 - Df.Width, 112 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            
            PointF Epos = new PointF(777 - Ef.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 20);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);

            PointF Cpos = new PointF(508 - Cf.Width / 2, 221 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);


            PointF Fpos = new PointF(40, 140 + SketchReinforcementApp.shift_font);
            Fpos = new PointF(-Fpos.Y - Ff.Width / 2, Fpos.X);       // для поворота на 90 
            PointF Gpos = new PointF(900, 140 + SketchReinforcementApp.shift_font);
            Gpos = new PointF(-Gpos.Y - Gf.Width / 2, Gpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Fpos.X, Fpos.Y, Ff.Width, Ff.Height - 10);
            picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);
            //picture.FillRectangle(Brushes.White, Gpos.X, Gpos.Y, Gf.Width, Gf.Height - 10);
            picture.DrawString(Gstr, SketchReinforcementApp.drawFont, Brushes.Black, Gpos);
            picture.RotateTransform(90);
            
            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.down, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №15 (ГОСТ)
    /// </summary>
    class Form15Gost
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(542,91);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(629, 174);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        double G;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        string Gstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, G);
                if (G == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        SizeF Gf
        {
            get
            { return picture.MeasureString(Gstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form15Gost(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "C")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "F" || pr.Definition.Name == "f") F = rebar.get_Parameter(pr.Definition).AsDouble();
                 
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(511 - Af.Width / 2, 240 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            PointF Fpos = new PointF(360 - Ff.Width, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Fpos.X, Fpos.Y, Ff.Width, Ff.Height - 10);
            picture.DrawString(Fstr, SketchReinforcementApp.drawFont, Brushes.Black, Fpos);

            PointF Cpos = new PointF(686, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 20);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);


            PointF Dpos = new PointF(727,133 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(238,148 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(845,145 + SketchReinforcementApp.shift_font);

            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90 
            Epos = new PointF(-Epos.Y - Ef.Width / 2, Epos.X);       // для поворота на 90 
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90 
            picture.RotateTransform(-90);            
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 10);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.RotateTransform(90);

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.upleft, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y - 40);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_down, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 15, p_end.Y -30);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №24 (ГОСТ)
    /// </summary>
    class Form24Gost
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(603,218);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(587, 90);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        double G;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        string Gstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, G);
                if (G == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        SizeF Gf
        {
            get
            { return picture.MeasureString(Gstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }

        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form24Gost(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
           
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            rebar = element as Rebar;

            // if (HookOrientationStart == RebarHookOrientation.Right || HookOrientationEnd == RebarHookOrientation.Right) return;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "B")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "E" || pr.Definition.Name == "e") E = rebar.get_Parameter(pr.Definition).AsDouble();
              
            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(460 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);           

            PointF Dpos = new PointF(464 - Df.Width/2, 240 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 20);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);


            PointF Cpos = new PointF(635, 147 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(750, 178 + SketchReinforcementApp.shift_font);
            PointF Epos = new PointF(198, 166 + SketchReinforcementApp.shift_font);

            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90 
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90
            Epos = new PointF(-Epos.Y - Ef.Width / 2, Epos.X);       // для поворота на 90 

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Epos.X, Epos.Y, Ef.Width, Ef.Height - 10);
            picture.DrawString(Estr, SketchReinforcementApp.drawFont, Brushes.Black, Epos);
            picture.RotateTransform(90);

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_down, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width / 2 + 25, p_start.Y + shift);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_upleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_up, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y + shift);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }

    /// <summary>
    /// Класс для создания формы №26 (ГОСТ)
    /// </summary>
    class Form26Gost
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(605, 109);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(604,238);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        double G;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        string Gstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, G);
                if (G == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        SizeF Gf
        {
            get
            { return picture.MeasureString(Gstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookOrient1;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form26Gost(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            rebar = element as Rebar;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "C")
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            }
            else
            {

                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();
                 

            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(460 - Af.Width / 2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);

            


            PointF Cpos = new PointF(640, 160 + SketchReinforcementApp.shift_font);
            PointF Bpos = new PointF(750, 180 + SketchReinforcementApp.shift_font);
            PointF Dpos = new PointF(204, 175 + SketchReinforcementApp.shift_font);

            Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90              
            Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90 
            Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            picture.RotateTransform(-90);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            picture.RotateTransform(90);

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width, p_start.Y -25);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_down, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width, p_end.Y - 25);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }


    /// <summary>
    /// Класс для создания формы №27 (ГОСТ)
    /// </summary>
    class Form27Gost
    {
        #region Начальные координаты стержня
        /// <summary>
        /// Координаты начала 
        /// </summary>
        PointF p_start = new PointF(605, 99);
        /// <summary>
        /// Координаты конца
        /// </summary>
        PointF p_end = new PointF(605, 280);
        #endregion
        #region Сегменты стержня
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        double A;
        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        double B;
        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        double C;
        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        double D;
        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        double E;
        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        double F;
        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        double G;

        #endregion
        #region Прочие параметры класса
        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        string Astr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, A);
                if (A == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня А 
        /// </summary>
        SizeF Af
        {
            get
            { return picture.MeasureString(Astr, SketchReinforcementApp.drawFont); }
        }
        /// <summary>
        /// Сегмент стержня B 
        /// </summary>
        string Bstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, B);
                if (B == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Bf
        {
            get
            { return picture.MeasureString(Bstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня C
        /// </summary>
        string Cstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, C);
                if (C == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня B
        /// </summary>
        SizeF Cf
        {
            get
            { return picture.MeasureString(Cstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        string Dstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, D);
                if (D == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня D
        /// </summary>
        SizeF Df
        {
            get
            { return picture.MeasureString(Dstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        string Estr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, E);
                if (E == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня E
        /// </summary>
        SizeF Ef
        {
            get
            { return picture.MeasureString(Estr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        string Fstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, F);
                if (F == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня F
        /// </summary>
        SizeF Ff
        {
            get
            { return picture.MeasureString(Fstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        string Gstr
        {
            get
            {
                string s = SketchTools.GetRoundLenghtSegment(rebar, G);
                if (G == 0) return s;
                if (s.Length < 2) return s;
                if (s.Substring(0, 2) == "0.") s = s.Substring(1);
                return s;
            }
        }

        /// <summary>
        /// Сегмент стержня G
        /// </summary>
        SizeF Gf
        {
            get
            { return picture.MeasureString(Gstr, SketchReinforcementApp.drawFont); }
        }

        /// <summary>
        /// Картинка для рисования 
        /// </summary>
        Graphics picture = null;
        ///// <summary>
        ///// Шрифт для крюков 
        ///// </summary>
        //Font drawFontH = new Font("Mipgost", 36);
        ///// <summary>
        ///// Шрифт для сегментов
        ///// </summary>
        //Font drawFont = new Font("Mipgost", 48);
        /// <summary>
        /// Документ проекта 
        /// </summary>
        Document doc;
        /// <summary>
        /// Исходный стержень 
        /// </summary>
        Rebar rebar;
        /// <summary>
        /// Картинка формы  
        /// </summary>
        public Bitmap bitmap = null;
        /// <summary>
        /// Угол 1 крюка
        /// </summary>
        int Angle1
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_start) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Угол 2 крюка
        /// </summary>
        int Angle2
        {
            get
            {
                RebarHookType rht = doc.GetElement(Hook_end) as RebarHookType;
                if (rht == null) return 0;
                return (int)(Math.Round(180 * rht.get_Parameter(BuiltInParameter.REBAR_HOOK_ANGLE).AsDouble() / 3.14159, 0));
            }
        }
        /// <summary>
        /// Признак одинаковых крюков
        /// </summary>
        bool IsHooksEqual
        {
            get
            {
                if (Angle1 == Angle2 && Hook_length_start == Hook_length_end) return true;
                return false;
            }
        }
        /// <summary>
        /// Начальный крюк
        /// </summary>
        ElementId Hook_start;
        /// <summary>
        /// Конечный крюк
        /// </summary>
        ElementId Hook_end;
        /// <summary>
        /// Длина начального крюка
        /// </summary>
        string Hook_length_start
        {
            get
            {
                if (Hook_start.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_start);
            }
        }
        /// <summary>
        /// Длина конечного крюка
        /// </summary>
        string Hook_length_end
        {
            get
            {
                if (Hook_end.IntegerValue < 0) return "";
                return StandartFormUtils.GetHookLength(rebar, Hook_end);
            }
        }
        /// <summary>
        /// Длина надписи начального крюка
        /// </summary>
        SizeF Hook_length_start_f
        {
            get
            {
                return picture.MeasureString(Hook_length_start, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Длина надписи конечного крюка
        /// </summary>
        SizeF Hook_length_end_f
        {
            get
            {
                return picture.MeasureString(Hook_length_end, SketchReinforcementApp.drawFontH);
            }
        }
        /// <summary>
        /// Ориентация крюков - изменена (ДА/НЕТ)
        /// </summary>
        public bool orient_hook = false;                  // по умолчанию
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_start
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle0 == 0 ? RebarHookOrientation.Left : rbd.HookOrient0;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation rho_end
        {
            get
            {
                RebarBendData rbd = rebar.GetBendData();
                return rbd.HookAngle1 == 0 ? RebarHookOrientation.Left : rbd.HookOrient1;
            }
        }
        /// <summary>
        /// Ориентация крюка - начало
        /// </summary>
        RebarHookOrientation HookOrientationStart
        {
            get
            {
                return !orient_hook ? rho_start : rho_end;
            }
        }
        /// <summary>
        /// Ориентация крюка - конец
        /// </summary>
        RebarHookOrientation HookOrientationEnd
        {
            get
            {
                return !orient_hook ? rho_end : rho_start;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор эскиза по стандарту 
        /// </summary>
        /// <param name="element">Арматурный стержень как элемент</param>
        /// <param name="image">Имя файла рисунка</param>
        /// <param name="show_length_hooks">Показать  длину крюков</param> 
        /// <param name="FolderImage">Папка для общих файлов</param> 
        /// <param name="UserFolderImage">Папка для файлов пользователя</param>
        public Form27Gost(Element element, bool show_length_hooks, string image, string FolderImage, string UserFolderImage, string FolderHook)
        {
            
            doc = element.Document;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            rebar = element as Rebar;

            string segment1 = StandartFormUtils.GetNameFirstSegment(rebar).ToUpper();        // имя первого сегмента
            ParameterSet pset = rebar.Parameters;
            // получить параметры формы
            if (segment1 == "D")
            {
                orient_hook = true;
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            }
            else
            {
                Hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
                Hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
               
            }

            foreach (Parameter pr in pset)
            {
                if (pr.Definition.Name == "A" || pr.Definition.Name == "a" || pr.Definition.Name == "Аdef") A = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "B" || pr.Definition.Name == "b"  || pr.Definition.Name == "Бdef" ) B = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "C" || pr.Definition.Name == "c" || pr.Definition.Name == "Cdef") C = rebar.get_Parameter(pr.Definition).AsDouble();
                if (pr.Definition.Name == "D" || pr.Definition.Name == "d") D = rebar.get_Parameter(pr.Definition).AsDouble();


            }

            string file = FolderImage + image;

            FileInfo fileinfo = new FileInfo(file);
            if (fileinfo.Exists)
            {
                bitmap = new Bitmap(file);
            }
            else return;

            picture = Graphics.FromImage(bitmap);

            PointF Apos = new PointF(717, 102 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Apos.X, Apos.Y, Af.Width, Af.Height - 20);
            picture.DrawString(Astr, SketchReinforcementApp.drawFont, Brushes.Black, Apos);
            PointF Bpos = new PointF(467 - Bf.Width/2, -10 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Bpos.X, Bpos.Y, Bf.Width, Bf.Height - 10);
            picture.DrawString(Bstr, SketchReinforcementApp.drawFont, Brushes.Black, Bpos);
            PointF Cpos = new PointF(249 - Cf.Width, 162 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Cpos.X, Cpos.Y, Cf.Width, Cf.Height - 10);
            picture.DrawString(Cstr, SketchReinforcementApp.drawFont, Brushes.Black, Cpos);
            PointF Dpos = new PointF(717, 216 + SketchReinforcementApp.shift_font);
            //picture.FillRectangle(Brushes.White, Dpos.X, Dpos.Y, Df.Width, Df.Height - 10);
            picture.DrawString(Dstr, SketchReinforcementApp.drawFont, Brushes.Black, Dpos);

            //Dpos = new PointF(-Dpos.Y - Df.Width / 2, Dpos.X);       // для поворота на 90              
            //Bpos = new PointF(-Bpos.Y - Bf.Width / 2, Bpos.X);       // для поворота на 90 
            //Cpos = new PointF(-Cpos.Y - Cf.Width / 2, Cpos.X);       // для поворота на 90

            // picture.RotateTransform(-90);
            
            
           
            // picture.RotateTransform(90);

            // при наличии крюков
            float shift = 30.0f;
            if (Hook_start.IntegerValue > 0)
            {
                if (HookOrientationStart == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_upleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_start, Angle1, HookPosition.v_up, FolderHook);
                }
                // показать длину прямого участка крюка                 
                PointF Hookpos = new PointF(p_start.X - Hook_length_start_f.Width - 30, p_start.Y + 10);
                if (show_length_hooks) picture.DrawString(Hook_length_start, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

            if (Hook_end.IntegerValue > 0)
            {
                if (HookOrientationEnd == RebarHookOrientation.Right) StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_downleft, FolderHook);
                else
                {
                    shift = 0;
                    StandartFormUtils.DrawHook(picture, p_end, Angle2, HookPosition.v_down, FolderHook);
                }
                PointF Hookpos = new PointF(p_end.X - Hook_length_end_f.Width - 30 , p_end.Y - 60);
                if (!IsHooksEqual && show_length_hooks) picture.DrawString(Hook_length_end, SketchReinforcementApp.drawFontH, Brushes.Black, Hookpos);
            }

        }
    }
}
