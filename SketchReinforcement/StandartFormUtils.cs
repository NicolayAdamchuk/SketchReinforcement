using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;

namespace SketchReinforcement
{

    /// <summary>
    /// Положения крюков  
    /// </summary>
    public enum HookPosition
    {
        /// <summary>
        /// Внизу справа
        /// </summary>
        down,
        /// <summary>
        /// Вверху справа
        /// </summary>
        up,
        /// <summary>
        /// Внизу слева  
        /// </summary>
        downleft,
        /// <summary>
        /// Вверху слева  
        /// </summary>
        upleft,
        /// <summary>
        /// Внизу справа (вертикально)
        /// </summary>
        v_down,
        /// <summary>
        /// Вверху справа (вертикально)
        /// </summary>
        v_up,
        /// <summary>
        /// Внизу слева  (вертикально)
        /// </summary>
        v_downleft,
        /// <summary>
        /// Вверху слева  (вертикально)
        /// </summary>
        v_upleft,
        /// <summary>
        /// Вверху слева  (вертикально) под 45
        /// </summary>
        v_upleft45,
        /// <summary>
        /// Вверху слева  (вертикально) под 45
        /// </summary>
        v_up45,
        /// <summary>
        /// Вверху слева  (вертикально) под -45
        /// </summary>
        v_upleft_45,
        /// <summary>
        /// Вверху слева  (вертикально) под -45
        /// </summary>
        v_up_45,
         /// <summary>
        /// Вверху слева  (вертикально) под -45
        /// </summary>
        v_downleft_45,
        /// <summary>
        /// Вверху слева  (вертикально) под -45
        /// </summary>
        v_down_45,
        /// <summary>
        /// Дуга справа
        /// </summary>
        arc,
        /// <summary>
        /// Дуга слева
        /// </summary>
        arc_left
    }

    class StandartFormUtils
    {
        /// <summary>
        /// Получить имя первого сегмента
        /// </summary>
        /// <param name="picture">Холст для рисования</param>
        public static string GetNameFirstSegment(Rebar rebar)
        {
            Document doc = rebar.Document;
            RebarShape rs = rebar.Document.GetElement(rebar.GetShapeId()) as RebarShape;
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;
            RebarShapeSegment segment = rsds.GetSegment(0);                           // определяем сегмент          
            IList<RebarShapeConstraint> ILrsc = segment.GetConstraints();             // параметры сегмента             
             

            foreach (RebarShapeConstraint rsc in ILrsc)                               // разбираем каждый сегмент в отдельности
            {
                // получим сегмент-длину
                RebarShapeConstraintSegmentLength l = rsc as RebarShapeConstraintSegmentLength;
                if (l != null)
                {
                    ElementId pid = l.GetParamId();
                    return doc.GetElement(pid).Name;                    
                }
            }
            return "";
        }

        /// <summary>
        /// Рисование крюка 
        /// </summary>
        /// <param name="picture">Холст для рисования</param>
        /// <param name="point">Точка вставки крюка</param>
        /// <param name="angle">Угол крюка</param> 
        /// <param name="pos">Положение крюка</param> 
        /// <param name="folder_hook">Папка для чтения файлов крюка</param> 
        public static bool DrawHook(Graphics picture, PointF point, int angle, HookPosition pos, string folder_hook)
        {
            string file_hook = "";
            FileInfo fileinfo;
            Bitmap hook_map;
            PointF point_insert=new PointF(0,0);
            string label = "u";
            string incline = "";
            bool orient = true;   // правая ориентация           

            switch (pos)
            {
                case HookPosition.down:
                    point_insert = new PointF(point.X - 46, point.Y - 5);
                    label = "d";
                    break;
                case HookPosition.downleft:
                    point_insert = new PointF(point.X - 13, point.Y - 2);
                    label = "d";
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.up:
                    point_insert = new PointF(point.X - 46, point.Y - 38);                    
                    break;
                case HookPosition.upleft:
                    point_insert = new PointF(point.X - 13, point.Y - 38); 
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.v_down:
                    // point_insert = new PointF(point.X - 46, point.Y - 5);
                    point_insert = new PointF(point.X - 5, point.Y - 24);
                    label = "vd";
                    break;
                case HookPosition.v_downleft:
                    point_insert = new PointF(point.X - 16 - 38, point.Y - 46);
                    orient = false;   // левая ориентация
                    label = "vd";
                    break;
                case HookPosition.v_up:
                    point_insert = new PointF(point.X - 6, point.Y);
                    label = "vu";
                    break;
                case HookPosition.v_upleft:
                    point_insert = new PointF(point.X - 53, point.Y - 10);
                    label = "vu";                    
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.v_upleft45:
                    point_insert = new PointF(point.X - 35, point.Y - 3);
                    label = "u";
                    incline = "_45";
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.v_up45:
                    point_insert = new PointF(point.X - 8, point.Y - 28);
                    label = "u";
                    incline = "_45";                    
                    break;
                case HookPosition.v_upleft_45:
                    point_insert = new PointF(point.X - 40, point.Y - 32);
                    label = "u";
                    incline = "_-45";
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.v_up_45:
                    point_insert = new PointF(point.X - 8, point.Y - 28);
                    label = "u";
                    incline = "_-45";
                    break;
                case HookPosition.v_downleft_45:
                    point_insert = new PointF(point.X - 28, point.Y - 44);
                    label = "vd";
                    incline = "_-45";
                    orient = false;   // левая ориентация
                    break;
                case HookPosition.v_down_45:
                    point_insert = new PointF(point.X - 28, point.Y - 44);
                    label = "vd";
                    incline = "_-45";
                    orient = true;   // левая ориентация
                    break;
                case HookPosition.arc:
                    point_insert = new PointF(point.X - 8, point.Y - 52);
                    label = "_arc";
                    incline = "";
                    orient = true;   // левая ориентация
                    break;
                case HookPosition.arc_left:
                    point_insert = new PointF(point.X - 48, point.Y - 52);
                    label = "_arc";
                    incline = "";
                    orient = false;   // левая ориентация
                    break;
            }           

            switch (angle)
            {
                case 180:
                    file_hook = folder_hook + "\\" + "H180" + label + incline + ".png";
                    if (!orient) file_hook = folder_hook + "\\" + "H180" + label + "_left"+incline+".png";
                    break;
                case 135:
                    file_hook = folder_hook + "\\" + "H135" + label + incline + ".png";
                    if (!orient) file_hook = folder_hook + "\\" + "H135" + label + "_left" + incline + ".png";
                    fileinfo = new FileInfo(file_hook);
                    break;
                default:
                    file_hook = folder_hook + "\\" + "H90" + label + incline + ".png";
                    if (!orient) file_hook = folder_hook + "\\" + "H90" + label + "_left" + incline + ".png";
                    fileinfo = new FileInfo(file_hook);
                    break;
            }

            fileinfo = new FileInfo(file_hook);
            if (fileinfo.Exists)
            {
                hook_map = new Bitmap(file_hook);
                picture.DrawImage(hook_map, point_insert);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Получить длину крюка 
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        public static string GetHookLength(Rebar rebar, ElementId hookId)
        {
            double hook_length = SketchTools.GetLengthHook(rebar, hookId);
            string hook_length_str = SketchTools.GetRoundLenghtSegment(rebar, hook_length);
            if (hook_length_str.Substring(0, 2) == "0.") hook_length_str = hook_length_str.Substring(1);
            return hook_length_str;
        }

        /// <summary>
        /// Получить длину крюка 
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        public static string GetHookLength(RebarInSystem rebar, ElementId hookId)
        {
            double hook_length = SketchTools.GetLengthHook(rebar, hookId);
            string hook_length_str = SketchTools.GetRoundLenghtSegment(rebar, hook_length);
            if (hook_length_str.Substring(0, 2) == "0.") hook_length_str = hook_length_str.Substring(1);
            return hook_length_str;
        }

        /// <summary>
        /// Получить имя примыкающего сегмента
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="hookId">Id крюка</param>
        public static string GetNameSegment(Rebar rebar, ElementId hookId)
        {
            double hook_length = SketchTools.GetLengthHook(rebar, hookId);
            string hook_length_str = SketchTools.GetRoundLenghtSegment(rebar, hook_length);
            if (hook_length_str.Substring(0, 2) == "0.") hook_length_str = hook_length_str.Substring(1);
            return hook_length_str;
        }

        ///// <summary>
        ///// Получить длину крюка и имя примыкающего сегмента
        ///// </summary>
        ///// <param name="rebar">Арматурный стержень</param>
        ///// <param name="hookId">Id крюка</param>
        //public static string GetLength_SegmentName_Hook(Rebar rebar, ElementId hookId, out string name_segment)
        //{
        //    name_segment = "";
        //    double hook_length = SketchTools.GetLengthHook(rebar, hookId);
        //    string hook_length_str = SketchTools.GetRoundLenghtSegment(rebar, hook_length);
        //    if (hook_length_str.Substring(0, 2) == "0.") hook_length_str = hook_length_str.Substring(1);
        //    return hook_length_str;
        //}

    }
}
