using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Windows.Forms;
using System.IO;


namespace SketchReinforcement
{
    
    public class Eskis
    {
        /// <summary>
        /// Коэффициент перевода единиц
        /// </summary>
        const float unit = (float)0.00328;

        /// <summary>
        /// Создание эскиза
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <param name="rebar">Список стержней</param>
        /// <param name="path_name">Путь к папке сохранения эскизов</param>
        /// <param name="template">Шаблон проекта</param>
        /// <param name="trans">Транзакция</param>
        /// <param name="dataform">Данные из диалога</param>
                
        static public void CreateImage(Document doc, CodeImage ci, Template template, DataForm dataform, string image, ElementId imageId, StreamWriter writer = null)
        {
            DateTime Start = DateTime.Now;
            DateTime Stoped = DateTime.Now;
            TimeSpan Elapsed = TimeSpan.Zero;
            Start = DateTime.Now;

            if (CreateBitmapRebar(doc.GetElement(ci.element), template, dataform, image, null))  // здесь будет создан файл image
            {
                Stoped = DateTime.Now;
                Elapsed = Stoped.Subtract(Start);
                if (writer != null) writer.WriteLine("*Эскиз создан");
                if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                
                try
                {
                    Start = DateTime.Now;
                    if (imageId != null) doc.Delete(imageId);
                    Stoped = DateTime.Now;
                    Elapsed = Stoped.Subtract(Start);
                    if (writer != null) writer.WriteLine("*Время удаления старого эскиза");
                    if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                    Start = DateTime.Now;
                    // создавать нужно всегда для обновления картинки
                    ImageTypeOptions itp = new ImageTypeOptions(image,true,ImageTypeSource.Import);                    
                    imageId = ImageType.Create(doc,itp).Id; 
                    ci.image = imageId;
                    Stoped = DateTime.Now;
                    Elapsed = Stoped.Subtract(Start);
                    if (writer != null) writer.WriteLine("*Время создания чистого эскиза");
                    if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                }
                catch
                {
                    return;
                }
            }           
        }

        /// <summary>
        /// Создание эскиза арматурных стержней
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="path_name">Путь к папке для сохранения эскизов</param>

        static public bool CreateBitmapRebar(Element rebar, Template template, DataForm dataform, string image, StreamWriter writer = null)
        {

            DateTime Start = DateTime.Now;
            DateTime Stoped = DateTime.Now;
            TimeSpan Elapsed = TimeSpan.Zero;

            Start = DateTime.Now;

            Document doc = rebar.Document;
            if (doc == null) return false;
            // определяем крюки формы
            ElementId hook_start = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
           
            RebarShape rs = null;
            Rebar rebarOne = rebar as Rebar;            
            RebarInSystem rebarIn = rebar as RebarInSystem;
            // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
            // получить данные по форме стержня
            if (rebarOne != null) rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
            if (rebarIn != null) rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;

            if (!dataform.ByAxis)
            {

                // пытаемся найти файл эскиза в папке стандартов
                // string sketch = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsString();
                ElementId sketchId = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsElementId();
                string sketch = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsValueString();

                // если эскиз не найден для стержня типа rebarOne
                // пытемся отключить крюки и установить все-таки тип картинки
                if (sketchId == null && rebarOne != null)
                {
                    if (rebarOne.IsRebarFreeForm()) return false;  
                    using (SubTransaction subTransaction = new SubTransaction(doc))
                    {

                        if (subTransaction.Start() == TransactionStatus.Started)
                        {
                            // убираем крюки
                            if (hook_start.IntegerValue > 0) SketchTools.SetParameter(rebar, BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE, new ElementId(-1));
                            if (hook_end.IntegerValue > 0) SketchTools.SetParameter(rebar, BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE, new ElementId(-1));
                            if (hook_start.IntegerValue > 0 || hook_end.IntegerValue > 0) doc.Regenerate();
                            // получить данные по форме стержня после отключения крюков
                            if (rebarOne != null) rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                            if (rebarIn != null) rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                            sketch = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsValueString();
                            sketchId = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsElementId();
                            if (sketchId.IntegerValue < 0) sketch = rs.get_Parameter(BuiltInParameter.REBAR_SHAPE_IMAGE).AsString();
                            subTransaction.RollBack();
                            // получить данные по форме стержня в действительном проекте
                            if (rebarOne != null) rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;
                            if (rebarIn != null) rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                        }

                    }
                }

                if (rebarIn != null)
                {
                    sketch = "RebarIn - (BS8666-2005).png";
                    DateTime d = DateTime.Now;
                    if (CreateBitmapStandart(rebar, sketch, dataform.HooksLength, image, dataform.coef_diam,dataform.Is_dim_lines)) return true;
                }

                if (sketchId.IntegerValue > 0 && sketch.Length > 0)
                {  
                    bool isbit = CreateBitmapStandart(rebar, sketch, dataform.HooksLength, image, dataform.coef_diam,dataform.Is_dim_lines);

                    if (isbit) return true;

                }
            }

            // стандарт определить не удалось или запрос по осям - строим произвольные формы
            if (rebarOne != null)
            {
                if (rebarOne.IsRebarFreeForm()) return false;
            }
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();           
            RebarShapeDefinitionByArc rarc = rsd as RebarShapeDefinitionByArc;
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;          
            
            if (rsds == null && rarc == null) return false;   // формы не определяются

            if (rarc != null)
            {                
                if(CreateBitmapRebarArcs(rebar, template, dataform, image)) return true;                   // готовим арочную форму     
            }
            else
            {
                if (dataform.ByAxis)
                {
                    if (CreateBitmapRebarSegmentsByAxis(rebar, dataform, image)) return true;       // готовим сегментную форму 
                }
                else
                {
                    Stoped = DateTime.Now;
                    Elapsed = Stoped.Subtract(Start);
                    if (writer != null) writer.WriteLine("**Время подготовки эскиза по сегментам");
                    if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                    Start = DateTime.Now;
                    if (CreateBitmapRebarSegments(rebar, template, dataform, image))        // готовим сегментную форму 
                    {
                        Stoped = DateTime.Now;
                        Elapsed = Stoped.Subtract(Start);
                        if (writer != null) writer.WriteLine("**Время выполнения создания эскиза по сегментам");
                        if (writer != null) writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
                        return true;
                    }
                }
            }

            return false;
        }

        #region Создание эскиза по отрезкам

        /// <summary>
        /// Создание эскиза сегментых арматурных стержней 
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="path_name">Путь к папке сохранения эскизов</param>
        static bool CreateBitmapRebarSegments(Element rebar, Template template, DataForm dataform,string image)
        {
            // Параметры построения чертежа
            BuildImage buildImage = new BuildImage(rebar, template);
            // размер рисунка уменьшаем при дополнительном смещении надписи от линии
            buildImage.canva = (float)dataform.border;
            buildImage.canva = buildImage.canva + (float) dataform.Font_shift;

            // if (buildImage.lg.Max(x => x.value <= 0)) return false;

            // buildImage.transaction = trans;

            Graphics flagGraphics;

            // БЕЗ ПРЕДВАРИТЕЛЬНОГО ПРОСМОТРА
            //if (!dataform.AllRebars)
            //{
            //    // предварительный просмотр чертежа арматуры                
            //buildImage.UpdateImage();
            //SketchReinforcement.PreviewRebar preview = new PreviewRebar(buildImage);
            //if (preview.ShowDialog() == DialogResult.Cancel) return false;
            //buildImage.UpdateImage();
            //}
            //else
            //{

            buildImage.dataform = dataform;    // запишем параметры диалога           
            if (dataform.BendingRadius) buildImage.bending = true;
            if (dataform.Angle) buildImage.showangle = true;
            if (dataform.HooksLength) buildImage.hooks_length = true;
            else buildImage.hooks_length = false;
            buildImage.UpdateImage();
            if (buildImage.flag == null) return false;  // не удалось создать рисунок
            // Возможная причина: не удалось получить нормальные прямые сегменты
            // Например длина участка П-стержня оказывается слишкой малой из-за реального загиба
            // Рекомендуется использовать метод стандартных рисунков
            //}

            flagGraphics = Graphics.FromImage(buildImage.flag);
            flagGraphics.Clear(System.Drawing.Color.White);
            buildImage.graphic = flagGraphics;
            if (buildImage.flag != null)
            {
                PreparedImage(buildImage);   // готовим рисунок
                // buildImage.flag.MakeTransparent(System.Drawing.Color.Black);
                // Bitmap flag = buildImage.flag;                
                // сохраняем файл чертежа
                // if (dataform.AllRebars) buildImage.flag.Save(@path_name + "\\" + rebar.Id.IntegerValue.ToString() + ".png");
                // else buildImage.flag.Save(@path_name + "\\" + rebar.Id.IntegerValue.ToString() + "fixed" + ".png");
                                
                buildImage.flag.Save(image);
                buildImage.flag.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion

        #region Создание эскиза по отрезкам - по оси стержня

        /// <summary>
        /// Создание эскиза сегментых арматурных стержней по осям стержням
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        /// <param name="path_name">Путь к папке сохранения эскизов</param>
        static bool CreateBitmapRebarSegmentsByAxis(Element rebar, DataForm dataform, string image)
        {
            // Параметры построения чертежа
            BuildImageByAxis buildImage = new BuildImageByAxis(rebar);
            buildImage.dataform = dataform;
            if (dataform.BendingRadius) buildImage.bending = true;
            if (dataform.HooksLength) buildImage.hooks_length = true;
            else buildImage.hooks_length = false;

            buildImage.UpdateImage();
         
            if (buildImage.flag != null)
            {
                PreparedImageAxis(buildImage);   // готовим рисунок               
                buildImage.flag.Save(image);
                buildImage.flag.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion

        /// <summary>
        /// Создание эскиза дуговых арматурных стержней 
        /// </summary>

        public static void PreparedImage(BuildImage buildImage, bool preview = false, float Y = 0, bool st = true)
        {            
            foreach (TextOnArc tor in buildImage.lg_arc_sorted)
            {
                tor.arc = true;
            }
            foreach (TextOnRebar tor in buildImage.lg)
            {
                tor.repeat = false;
            }
            foreach (TextOnRebar tor in buildImage.hooks)
            {
                tor.arc = true;
            }

            if (!preview)
            {                
                buildImage.graphic = Graphics.FromImage(buildImage.flag);
                if(buildImage.dataform.BackGroundColor)  buildImage.graphic.Clear(System.Drawing.Color.Transparent);
                else                            buildImage.graphic.Clear(System.Drawing.Color.White);
                // buildImage.graphic.Clear(System.Drawing.Color.Transparent); // System.Drawing.Color.White);
            }
            else
            {
                // buildImage.graphic.Clear(System.Drawing.Color.White);
            }

            //buildImage.graphic.Clear(System.Drawing.Color.Transparent);
            //buildImage.graphic.TranslateTransform(1500,1500);
            //buildImage.graphic.DrawString("10", SketchReinforcementApp.drawFont, Brushes.Black, new PointF(0.0f, 0.0f));
            //buildImage.graphic.RotateTransform(45);
            //buildImage.graphic.DrawString("145", SketchReinforcementApp.drawFont, Brushes.Black, new PointF(0.0f, 0.0f));
            //buildImage.graphic.RotateTransform(-45);
            //buildImage.graphic.RotateTransform(90);
            //buildImage.graphic.DrawString("190", SketchReinforcementApp.drawFont, Brushes.Black, new PointF(0.0f, 0.0f));
            //buildImage.graphic.RotateTransform(-90);
            //buildImage.graphic.RotateTransform(180);
            //buildImage.graphic.DrawString("1180", SketchReinforcementApp.drawFont, Brushes.Black, new PointF(0.0f, 0.0f));
            //buildImage.graphic.RotateTransform(-180);
            //buildImage.graphic.RotateTransform(270);
            //buildImage.graphic.DrawString("1270", SketchReinforcementApp.drawFont, Brushes.Black, new PointF(0.0f, 0.0f));
            //buildImage.graphic.RotateTransform(-270);

            //return;

            //buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
            //buildImage.graphic.DrawLines(Pens.Yellow, buildImage.pointDF.ToArray());
            //buildImage.graphic.DrawEllipse(Pens.Red, 10, 10, 2, 2);
            //return;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            // Font drawFont = new Font("Mipgost", 48);
            // Font drawFont = new Font("Mipgost", 30);
            List<System.Drawing.Drawing2D.GraphicsPath> gp = new List<System.Drawing.Drawing2D.GraphicsPath>();
            //if (st)
            //{
            if (preview) buildImage.graphic.ScaleTransform(0.5F, 0.5F);
            buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
            //}
            // новая область занятая текстом
            System.Drawing.Drawing2D.GraphicsPath new_region = new System.Drawing.Drawing2D.GraphicsPath();

            Pen pen = SketchTools.GetPen(buildImage.dataform.color);
            Brush brush = SketchTools.GetBrushes(buildImage.dataform.color);

            buildImage.graphic.DrawLines(pen, buildImage.pointDF.ToArray());                                 // рисуем арматуру

            // пробуем рисовать начиная с длинных сегментов - вероятно там больше места
            if (buildImage.bending)   // если в диалоге имеется отметка
            {
                

                foreach (TextOnArc tor in buildImage.lg_arc_sorted)
                {
                    if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
                    PointF initial_point = new PointF(tor.positionF.X, tor.positionF.Y);
                    // Длина надписи
                    SizeF sf = buildImage.graphic.MeasureString("r" + tor.value_str, SketchReinforcementApp.drawFont30);
                    sf.Height = buildImage.dataform.shiftDown - buildImage.dataform.shiftUp;
                    tor.size = sf;
                    // flagGraphics.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 2, 2);
                    int sign = 1;
                    // выполняем наклонные надписи
                    if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
                    {
                        //if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                        //else tor.angle = (float)(tor.angle - Math.PI);
                        //// изменим точку вставки надписи

                        //tor.positionF = new PointF((float)(tor.positionF.X - Math.Cos(tor.angle) * sf.Width),
                        //                            (float)(tor.positionF.Y - Math.Sin(tor.angle) * sf.Width));
                        // flagGraphics.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 2, 2);
                        sign = -1;
                    }
                    //Size new_size = new Size((int)(sf.Width * Math.Cos(tor.angle)), (int)(sf.Width * Math.Sin(tor.angle) + sf.Height * Math.Cos(tor.angle)));
                    //buildImage.graphic.DrawRectangle(Pens.Green, initial_point.X, initial_point.Y, sf.Width, sf.Height);

                    // сдвиг на середину условной линии направления радиуса (центр текста по линии)
                    tor.positionF = new PointF((float)(tor.positionF.X + (buildImage.dataform.shiftUp + sf.Height / 2) * (float)Math.Sin(tor.angle)), (float)(tor.positionF.Y - (buildImage.dataform.shiftUp + sf.Height / 2) * Math.Cos(tor.angle)));
                    PointF point;
                    bool is_move_position = true;
                    
                    if (GetRadiusVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out point, tor.angle, buildImage.lg_arc.Count(x => x.arc == true), sign, buildImage.sizeX, buildImage.sizeY, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10), out is_move_position, buildImage.dataform.shiftUp, buildImage.dataform.shiftLeft, out new_region))
                    {
                        tor.positionF = point;                             // изменим на новое положение
                        buildImage.graphic.RotateTransform(tor.angle_grad);
                        buildImage.graphic.DrawString("r" + tor.value_str, SketchReinforcementApp.drawFont30, brush, tor.positionF_rotate);
                        buildImage.graphic.RotateTransform(-tor.angle_grad);

                        // добавим новую занятую зону                                                    
                        // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Incline, tor.angle));
                        gp.Add(new_region);
                        foreach (TextOnArc toa in buildImage.lg_arc)
                        {
                            if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные радиусы больше не указываем
                        }
                    }
                }
            }
            // отображаем текстовые надписи - только горизонтальные или вертикальные
            // int count_segment = 0;
            foreach (TextOnRebar tor in buildImage.lg)
            {
                //count_segment++;
                //if(count_segment>4) continue;
                if (tor.repeat) continue;
                // Длина надписи
                SizeF sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                sf.Height = buildImage.dataform.shiftDown - buildImage.dataform.shiftUp;                 
                tor.size = sf;                                                             // фиксируем размер надписи
                switch (tor.incline)
                {
                    case InclineText.Horiz:

                        // buildImage.graphic.DrawString("TEST", drawFont, Brushes.Black, new PointF(20, 20));
                        // buildImage.graphic.DrawLine(Pens.Red,tor.positionF.X, tor.positionF.Y, tor.positionF.X+20, tor.positionF.Y);
                        // точку вставки текста поднимем немного выше - в зависимости от размера шрифта

                        if (GeHorizontalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva), (float)(buildImage.moveY + buildImage.canva), buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftLeft,out new_region))
                        {
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, brush, tor.positionF);

                            //buildImage.graphic.DrawLine(Pens.Blue, 0.0f, 0.0f, 200.0f, 0.0f);
                            //buildImage.graphic.DrawLine(Pens.Green, 0.0f, 100.0f, 10.0f, 100.0f);
                            //buildImage.graphic.DrawLine(Pens.Cyan, 0.0f, -100.0f, 10.0f, -100.0f);
                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(new PointF(tor.positionF.X, tor.positionF.Y - buildImage.dataform.shiftUp), sf, InclineText.Horiz));
                            gp.Add(new_region);
                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {
                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            tor.dialog = true;              // текущий оставить  
                        }

                        break;
                    case InclineText.Vertic:
                        
                        // для вертикальной линии сдвиг влево
                        // tor.positionF = new PointF(tor.positionF.X - 10, tor.positionF.Y);

                        if (GetVerticalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10), buildImage.dataform.shiftLeftVertical, buildImage.dataform.Font_shift, buildImage.dataform.shiftUpVertical,out new_region))
                        {
                            // if ((tor.positionF.Y + sf.Width + canva / 2) > sizeY) break;
                            
                            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
                            
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, brush, tor.positionF, drawFormat);
                            
                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Vertic));
                            gp.Add(new_region);
                        

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {

                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            // сделаем отметку для диалога
                            tor.dialog = true;
                        }
                        break;
                    default:
                        continue;

                }
            }

            
            // отображаем текстовые надписи - только наклонные
            foreach (TextOnRebar tor in buildImage.lg)
            {
                if (tor.repeat) continue;
                if (tor.value == 0) continue;
                // Длина надписи
                SizeF sf;
                                                                             
                switch (tor.incline)
                {
                    case InclineText.Horiz:
                        continue;
                    case InclineText.Vertic:
                        continue;
                    default:
                        // сохраняем позицию вставки текста для сегмента
                        PointF initial_pos = new PointF(tor.positionF.X, tor.positionF.Y);
                        PointF point;
                        PointF Incline_pos = new PointF(0, 0);
                        // buildImage.graphic.DrawEllipse(Pens.Yellow, Incline_pos.X, Incline_pos.Y, 5, 5);
                        // центр вращения переместим на середину стержня
                        Incline_pos.X = tor.positionF.X;
                        Incline_pos.Y = tor.positionF.Y;

                        sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                        sf.Height = buildImage.dataform.shiftDown - buildImage.dataform.shiftUp;
                        tor.size = sf;
                         
                        // buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 5,5);

                        //Incline_pos.X = tor.positionF.X * (float) Math.Cos(tor.angle) - tor.positionF.Y * (float) Math.Sin(tor.angle);
                        //Incline_pos.Y = tor.positionF.X * (float) Math.Sin(tor.angle) + tor.positionF.Y * (float) Math.Cos(tor.angle);

                        //buildImage.graphic.DrawEllipse(Pens.Red, Incline_pos.X, Incline_pos.Y, 5, 5);

                        // выполняем наклонные надписи
                        if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
                        {
                            if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                            else tor.angle = (float)(tor.angle - Math.PI);

                            //// сдвиг на высоту надписи
                            //Incline_pos = new PointF((float)(tor.positionF.X + (buildImage.dataform.shiftUp - 5) * (float) Math.Sin(tor.angle)), (float)(tor.positionF.Y - (buildImage.dataform.shiftUp - 5) * Math.Cos(tor.angle)));
                            //buildImage.graphic.DrawEllipse(Pens.Red, Incline_pos.X, Incline_pos.Y, 5, 5);

                            //if (!GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, Incline_pos, sf, out point, tor.angle))
                            //{
                            //    Incline_pos = new PointF(tor.positionF.X, tor.positionF.Y);
                            //    buildImage.graphic.DrawEllipse(Pens.Red, Incline_pos.X, Incline_pos.Y, 5, 5);
                            //}
                        }
                        else
                        {
                            //Incline_pos = new PointF(tor.positionF.X, tor.positionF.Y);
                            //if (!GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, Incline_pos, sf, out point, tor.angle))
                            //{
                            //    Incline_pos = new PointF((float)(tor.positionF.X + sf.Height * Math.Sin(tor.angle)), (float)(tor.positionF.Y - sf.Height * Math.Cos(tor.angle)));
                            //}
                        }

                        // сдвиг вверх к линии из-за размеров шрифта
                        // +дополнительное смещение от линии
                        Incline_pos = new PointF((float)(tor.positionF.X + (buildImage.dataform.shiftUp - 5 - buildImage.dataform.Font_shift) * (float)Math.Sin(tor.angle)), (float)(tor.positionF.Y - (buildImage.dataform.shiftUp - 5 - buildImage.dataform.Font_shift) * Math.Cos(tor.angle)));
                        
                        // Incline_pos = new PointF(tor.positionF.X,tor.positionF.Y);
                        // buildImage.graphic.DrawEllipse(Pens.Red, Incline_pos.X, Incline_pos.Y, 5, 5);


                        tor.positionF = new PointF(Incline_pos.X, Incline_pos.Y);   // новая точка вставки текста с учетом наклона
                                               
                        if (GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, tor.angle, buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftLeft, out new_region))
                        {
                            tor.positionF = point;                             // изменим на новое положение
                            buildImage.graphic.RotateTransform(tor.angle_grad);
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont, brush, tor.positionF_rotate);
                            buildImage.graphic.RotateTransform(-tor.angle_grad);
                            // сделаем отметку для диалога
                            tor.dialog = true;
                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(point, sf, InclineText.Incline, tor.angle));
                            gp.Add(new_region);
                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {

                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            // сделаем отметку для диалога
                            tor.dialog = true;

                        }

                        // восстановим значение
                        tor.positionF = new PointF(initial_pos.X, initial_pos.Y);
                        // выполняем горизонтальные проекционные надписи
                        if (tor.valueH > 0)
                        {
                            sf = buildImage.graphic.MeasureString(tor.valueH_str, SketchReinforcementApp.drawFont);
                            tor.size = sf;                                                             // фиксируем размер надписи
                            if (GeTHorizontalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, buildImage.sizeY - buildImage.canva - sf.Height - buildImage.moveY, out new_region))
                            {
                                buildImage.graphic.DrawString(tor.valueH_str, SketchReinforcementApp.drawFont, brush, point);
                                // добавим новую занятую зону                                                    
                                // gp.Add(GetPathRegionText(point, sf, InclineText.Horiz));
                                gp.Add(new_region);
                            }
                        }
                        // выполняем вертикальные проекционные надписи
                        if (tor.valueV > 0)
                        {
                            sf = buildImage.graphic.MeasureString(tor.valueV_str, SketchReinforcementApp.drawFont);
                            tor.size = sf;                                                             // фиксируем размер надписи
                            if (GeTVerticalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, buildImage.sizeX, (float)(buildImage.canva + buildImage.moveX), out new_region))
                            {
                                buildImage.graphic.DrawString(tor.valueV_str, SketchReinforcementApp.drawFont, brush, point, drawFormat);
                                // добавим новую занятую зону                                                    
                                // gp.Add(GetPathRegionText(point, sf, InclineText.Vertic));
                                gp.Add(new_region);
                            }
                        }


                        break;
                }



            }









            // отображаем текстовые надписи - крюки
            if (buildImage.hooks_length)
            {
                foreach (TextOnRebar tor in buildImage.hooks)
                {
                    if (!tor.arc) continue;   // пропускаем одинанаковые крюки

                    //// сохраняем позицию вставки текста для сегмента
                    //PointF initial_pos = new PointF(tor.positionF.X, tor.positionF.Y);
                    PointF point;
                    //PointF Incline_pos = new PointF(0, 0);
                    //// buildImage.graphic.DrawEllipse(Pens.Yellow, Incline_pos.X, Incline_pos.Y, 5, 5);
                    //// центр вращения переместим на середину стержня
                    //Incline_pos.X = tor.positionF.X;
                    //Incline_pos.Y = tor.positionF.Y;
                                       
                    SizeF sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                    sf.Height = buildImage.dataform.shiftDown - buildImage.dataform.shiftUp;
                    tor.size = sf;

                    // рассчитаем угол наклона надписи
                    // получить угол наклона надписи в градусах
                    double dAY = (double)(tor.endF.Y - tor.startF.Y);
                    double dAX = (double)(tor.endF.X - tor.startF.X);
                    //if (dAX == 0) tor.angle = 0;
                    //else tor.angle = (float)Math.Atan2(dAY, dAX);
                    tor.angle = (float) Math.Round(Math.Atan2(dAY, dAX),3);

                    // выполняем наклонные надписи
                    if (Math.Abs(tor.angle) > Math.Round(Math.PI / 2,3) && Math.Abs(tor.angle) <= Math.Round(Math.PI,3))
                    {
                        if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                        else tor.angle = (float)(tor.angle - Math.Round(Math.PI,3));
                    }

                    // сдвиг вверх к линии из-за размеров шрифта
                    // +дополнительное смещение от линии
                    PointF Incline_pos = new PointF((float)(tor.positionF.X + (buildImage.dataform.shiftUp - 5 - buildImage.dataform.Font_shift) * (float)Math.Sin(tor.angle)), (float)(tor.positionF.Y - (buildImage.dataform.shiftUp - 5 - buildImage.dataform.Font_shift) * Math.Cos(tor.angle)));
                    tor.positionF = new PointF(Incline_pos.X, Incline_pos.Y);   // новая точка вставки текста с учетом наклона



                    // tor.positionF = new PointF(tor.positionF.X, tor.positionF.Y - 5.0f);

                    if (GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, tor.angle, buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftLeft, out new_region))
                    {
                        tor.positionF = point;                             // изменим на новое положение
                        buildImage.graphic.RotateTransform(tor.angle_grad);
                        buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont, brush, tor.positionF_rotate);
                        buildImage.graphic.RotateTransform(-tor.angle_grad);
                        // добавим новую занятую зону   
                        gp.Add(new_region);
                        // сделаем отметку для диалога
                        tor.dialog = true;
                    }

                    foreach (TextOnRebar toa in buildImage.hooks)
                    {
                        if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные крюки
                    }
                }
            }

            // отображаем текстовые надписи - УГЛЫ
            // сначала посторим и посмотрим: где были дополнительные сдвиги
            // на чертеже показываем в первую очередь УГЛЫ без сдвигов

            if (buildImage.showangle)
            {
               
                foreach (TextOnArc tor in buildImage.lg_angles)
                {
                    // if (!tor.arc) continue;       // отработанные элементы пропускаем
                    if (tor.value <= 90 || tor.value == 180) continue;
                     
                    //// Длина надписи
                    //string s = tor.value.ToString() + "°";
                    //SizeF sf = buildImage.graphic.MeasureString(s, drawFont);
                    //tor.size = sf;
                    //tor.positionF.Y = tor.positionF.Y - sf.Height*3/4;
                    //if(tor.startF.X<tor.positionF.X) tor.positionF.X = tor.positionF.X - sf.Width;
                    //buildImage.graphic.DrawString(tor.value.ToString()+"°", drawFont, Brushes.DarkGreen, tor.positionF);
                    //buildImage.graphic.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 5, 5);

                    // PointF initial_point = new PointF(tor.positionF.X, tor.positionF.Y);
                    // Длина надписи
                    SizeF sf = buildImage.graphic.MeasureString(tor.value.ToString() + "°", SketchReinforcementApp.drawFont);
                    sf.Height = buildImage.dataform.shiftDown - buildImage.dataform.shiftUp;
                    tor.size = sf;                    
                    int sign = 1;
                    // buildImage.graphic.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 1, 1);
                    
                    // выполняем наклонные надписи
                    if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
                    {
                        //if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                        //else tor.angle = (float)(tor.angle - Math.PI);
                        //// изменим точку вставки надписи

                        //tor.positionF = new PointF((float)(tor.positionF.X - Math.Cos(tor.angle) * sf.Width),
                        //                            (float)(tor.positionF.Y - Math.Sin(tor.angle) * sf.Width));

                        sign = -1;
                    }


                    

                    // сдвиг на середину условной линии направления радиуса (центр текста по линии)
                    tor.positionF = new PointF((float)(tor.positionF.X + (buildImage.dataform.shiftUp + sf.Height/2) * (float)Math.Sin(tor.angle)), (float)(tor.positionF.Y - (buildImage.dataform.shiftUp + sf.Height / 2) * Math.Cos(tor.angle)));
                    // buildImage.graphic.DrawEllipse(Pens.Green, tor.positionF.X, tor.positionF.Y, 2, 2);
                    

                    //buildImage.graphic.DrawEllipse(Pens.Yellow, tor.startF.X, tor.startF.Y, 2, 2);
                    //buildImage.graphic.DrawEllipse(Pens.Coral, tor.endF.X, tor.endF.Y, 2, 2);
                    PointF point;
                    bool is_move_position = true;    // признак поиска нового положения
                    if (GetRadiusVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out point, tor.angle, buildImage.lg_arc.Count(x => x.arc == true), sign, buildImage.sizeX, buildImage.sizeY, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10), out is_move_position, buildImage.dataform.shiftUp, buildImage.dataform.shiftLeft, out new_region))
                    {
                        tor.positionF = point;                             // изменим на новое положение
                        tor.is_move_position = is_move_position;
                        //buildImage.graphic.RotateTransform(tor.angle_grad);
                        //buildImage.graphic.DrawString(tor.value.ToString() + "°", drawFont, Brushes.Black, tor.positionF_rotate);
                        //buildImage.graphic.RotateTransform(-tor.angle_grad);

                        // добавим новую занятую зону  
                        gp.Add(new_region);

                        //gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Incline, tor.angle));
                        //foreach (TextOnArc toa in buildImage.lg_arc)
                        //{
                        //    if (toa.value == tor.value) toa.arc = false;  // все подобные радиусы больше не указываем
                        //}
                    }
                    else
                    {
                        tor.arc = false;    // места нет. Не показываем
                    }

                    //foreach (TextOnArc toa in buildImage.lg_angles)
                    //{
                    //    if (toa.value == tor.value) toa.arc = false;  // все подобные углы не показываем
                    //}

                    

                }

              

                // отсортируем список по длине примыкающих прямых сегментов   
                buildImage.lg_arc_sorted.Clear();
                buildImage.lg_arc_sorted = buildImage.lg_angles.OrderByDescending(x => !x.is_move_position).ToList();

                foreach (TextOnArc tor in buildImage.lg_arc_sorted)
                {
                    if (!tor.arc) continue;       // отработанные элементы пропускаем
                    if (tor.value <= 90 || tor.value == 180) continue;



                    if (Math.Abs(tor.angle) > Math.PI / 2)
                    {
                        // размер надписи - полный
                        SizeF sf = buildImage.graphic.MeasureString(tor.value.ToString() + "°", SketchReinforcementApp.drawFont);
                        sf.Height = sf.Height - buildImage.dataform.shiftUp;
                        PointF new_point = new PointF();
                        float new_angle = 0;
                        // buildImage.graphic.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 2, 2);
                        SketchTools.GetMirrorText(tor.positionF, tor.angle, sf, out new_point, out new_angle);

                        tor.angle = new_angle;
                        tor.positionF = new_point;
                        // buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 2, 2);
                    }

                    buildImage.graphic.RotateTransform(tor.angle_grad);
                    buildImage.graphic.DrawString(tor.value.ToString() + "°", SketchReinforcementApp.drawFont30, brush, tor.positionF_rotate);
                    buildImage.graphic.RotateTransform(-tor.angle_grad);

                    foreach (TextOnArc toa in buildImage.lg_arc_sorted)
                    {
                        if (toa.value == tor.value) toa.arc = false;  // все подобные углы не показываем
                    }
                }

                }
        }

        /// <summary>
        /// Создание эскиза арматурных стержней по осям
        /// </summary>

        static void PreparedImageAxis(BuildImageByAxis buildImage, bool preview = false, float Y = 0, bool st = true)
        {
            foreach (TextOnArc tor in buildImage.lg_arc_sorted)
            {
                tor.arc = true;
            }
            foreach (TextOnRebar tor in buildImage.lg)
            {
                tor.repeat = false;
            }
            foreach (TextOnRebar tor in buildImage.hooks)
            {
                tor.arc = true;
            }

            if (!preview)
            {
                buildImage.graphic = Graphics.FromImage(buildImage.flag);
                // buildImage.graphic.Clear(System.Drawing.Color.White);
            }
            else
            {
                // buildImage.graphic.Clear(System.Drawing.Color.White);
            }
            buildImage.graphic.Clear(System.Drawing.Color.Transparent);

            //buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
            //buildImage.graphic.DrawLines(Pens.Yellow, buildImage.pointDF.ToArray());
            //buildImage.graphic.DrawEllipse(Pens.Red, 10, 10, 2, 2);
            //return;
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
            // Font drawFont = new Font("Mipgost", 48);
            // Font drawFont = new Font("Mipgost", 30);
            System.Drawing.Drawing2D.GraphicsPath new_region = new System.Drawing.Drawing2D.GraphicsPath();
            List<System.Drawing.Drawing2D.GraphicsPath> gp = new List<System.Drawing.Drawing2D.GraphicsPath>();
            //if (st)
            //{
            if (preview) buildImage.graphic.ScaleTransform(0.5F, 0.5F);
            buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
            //}

            Pen pen = new Pen(System.Drawing.Color.Black, 5);
            buildImage.graphic.DrawLines(pen, buildImage.pointDF.ToArray());                                 // рисуем арматуру

            // пробуем рисовать начиная с длинных сегментов - вероятно там больше места
            if (buildImage.bending)   // если в диалоге имеется отметка
            {
                foreach (TextOnArc tor in buildImage.lg_arc_sorted)
                {
                    if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
                    PointF initial_point = new PointF(tor.positionF.X, tor.positionF.Y);
                    // Длина надписи
                    SizeF sf = buildImage.graphic.MeasureString("r" + tor.value_str, SketchReinforcementApp.drawFont30);
                    tor.size = sf;
                    // flagGraphics.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 2, 2);
                    int sign = 1;
                    // выполняем наклонные надписи
                    if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
                    {
                        if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                        else tor.angle = (float)(tor.angle - Math.PI);
                        // изменим точку вставки надписи

                        tor.positionF = new PointF((float)(tor.positionF.X - Math.Cos(tor.angle) * sf.Width),
                                                    (float)(tor.positionF.Y - Math.Sin(tor.angle) * sf.Width));
                        // flagGraphics.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 2, 2);
                        sign = -1;
                    }
                    //Size new_size = new Size((int)(sf.Width * Math.Cos(tor.angle)), (int)(sf.Width * Math.Sin(tor.angle) + sf.Height * Math.Cos(tor.angle)));
                    //buildImage.graphic.DrawRectangle(Pens.Green, initial_point.X, initial_point.Y, sf.Width, sf.Height);
                    PointF point;
                    bool is_move_position = true;    // признак поиска нового положения
                    if (GetRadiusVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out point, tor.angle, buildImage.lg_arc.Count(x => x.arc == true), sign, buildImage.sizeX, buildImage.sizeY, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10),out is_move_position, buildImage.dataform.shiftUp, buildImage.dataform.shiftLeft, out new_region))
                    {
                        tor.positionF = point;                             // изменим на новое положение
                        buildImage.graphic.RotateTransform(tor.angle_grad);
                        buildImage.graphic.DrawString("r" + tor.value_str, SketchReinforcementApp.drawFont30, Brushes.Black, tor.positionF_rotate);
                        buildImage.graphic.RotateTransform(-tor.angle_grad);

                        // добавим новую занятую зону                                                    
                        // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Incline, tor.angle));
                        gp.Add(new_region);

                        foreach (TextOnArc toa in buildImage.lg_arc)
                        {
                            if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные радиусы больше не указываем
                        }
                    }
                }
            }
            // отображаем текстовые надписи - только горизонтальные или вертикальные
            foreach (TextOnRebar tor in buildImage.lg)
            {
                if (tor.repeat) continue;
                // Длина надписи
                SizeF sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                tor.size = sf;                                                             // фиксируем размер надписи
                switch (tor.incline)
                {
                    case InclineText.Horiz:

                        // buildImage.graphic.DrawString("TEST", drawFont, Brushes.Black, new PointF(20, 20));

                        if (GeHorizontalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10),buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftLeft, out new_region))
                        {
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, Brushes.Black, tor.positionF);

                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Horiz));
                            gp.Add(new_region);

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {
                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            tor.dialog = true;              // текущий оставить  
                        }

                        break;
                    case InclineText.Vertic:
                        // для вертикальной линии сдвиг влево
                        tor.positionF = new PointF(tor.positionF.X - 10, tor.positionF.Y);
                        if (GetVerticalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10), buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftUpVertical, out new_region))
                        {
                            // if ((tor.positionF.Y + sf.Width + canva / 2) > sizeY) break;
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, Brushes.Black, tor.positionF, drawFormat);

                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Vertic));
                            gp.Add(new_region);

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {

                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            // сделаем отметку для диалога
                            tor.dialog = true;
                        }
                        break;
                    default:
                        continue;

                }
            }

            // отображаем текстовые надписи - только наклонные
            foreach (TextOnRebar tor in buildImage.lg)
            {
                if (tor.repeat) continue;
                if (tor.value == 0) continue;
                // Длина надписи
                SizeF sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                tor.size = sf;                                                             // фиксируем размер надписи
                switch (tor.incline)
                {
                    case InclineText.Horiz:
                        continue;
                    case InclineText.Vertic:
                        continue;
                    default:

                        // buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 5, 5);
                        // выполняем наклонные надписи
                        if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
                        {
                            if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                            else tor.angle = (float)(tor.angle - Math.PI);
                            // сдвиг на высоту надписи
                            tor.positionF = new PointF((float)(tor.positionF.X + sf.Height * Math.Sin(tor.angle)), (float)(tor.positionF.Y - sf.Height * Math.Cos(tor.angle)));
                            // buildImage.graphic.DrawEllipse(Pens.Brown, tor.positionF.X, tor.positionF.Y, 5, 5);
                        }

                        PointF point, positionF;
                        positionF = tor.positionF;
                        if (GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, tor.angle, buildImage.dataform.shiftUp, buildImage.dataform.Font_shift, buildImage.dataform.shiftLeft, out new_region))
                        {
                            tor.positionF = point;                             // изменим на новое положение 
                            //buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 5, 5);
                            buildImage.graphic.RotateTransform(tor.angle_grad);
                            buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, Brushes.Black, tor.positionF_rotate);
                            buildImage.graphic.RotateTransform(-tor.angle_grad);
                            // сделаем отметку для диалога
                            tor.dialog = true;
                            // добавим новую занятую зону                                                    
                            // gp.Add(GetPathRegionText(point, sf, InclineText.Incline, tor.angle));
                            gp.Add(new_region);

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline)
                                {
                                    torR.repeat = true;

                                }
                            }

                            // отключаем такие же размеры сходного направления
                            foreach (TextOnRebar torR in buildImage.lg)
                            {
                                if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
                                {

                                    torR.dialog = false;    // для всех отключить из диалога
                                }
                            }

                            // сделаем отметку для диалога
                            tor.dialog = true;

                        }
                        // выполняем горизонтальные проекционные надписи
                        if (tor.valueH > 0)
                        {
                            sf = buildImage.graphic.MeasureString(tor.valueH_str, SketchReinforcementApp.drawFont30);
                            tor.size = sf;                                                             // фиксируем размер надписи
                            if (GeTHorizontalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, positionF, sf, out point, buildImage.sizeY - buildImage.canva - sf.Height - buildImage.moveY, out new_region))
                            {
                                //if ((point.X + moveX + canva + 10) < 0) break;
                                //if ((point.X + sf.Width + moveX + canva - 10) > sizeX) break;
                                //if ((point.Y + moveY + canva + 10) < 0) break;
                                //if ((point.Y + sf.Height + moveY + canva - 10) > sizeY) break;
                                buildImage.graphic.DrawString(tor.valueH_str, SketchReinforcementApp.drawFont30, Brushes.Black, point);
                                // добавим новую занятую зону                                                    
                                // gp.Add(GetPathRegionText(point, sf, InclineText.Horiz));
                                gp.Add(new_region);
                            }
                        }
                        // выполняем вертикальные проекционные надписи
                        if (tor.valueV > 0)
                        {
                            sf = buildImage.graphic.MeasureString(tor.valueV_str, SketchReinforcementApp.drawFont30);
                            tor.size = sf;                                                             // фиксируем размер надписи
                            if (GeTVerticalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, positionF, sf, out point, buildImage.sizeX, (float)(buildImage.canva + buildImage.moveX), out new_region))
                            {
                                //if ((point.Y + moveY + canva + 10) < 0) break;
                                //if ((point.Y + sf.Width + canva + moveY - 10) > sizeY) break;
                                //if ((point.X + moveX + canva + 10) < 0) break;
                                //if ((point.X + sf.Height + canva + moveX + 10) > sizeX) break;
                                buildImage.graphic.DrawString(tor.valueV_str, SketchReinforcementApp.drawFont30, Brushes.Black, point, drawFormat);
                                // добавим новую занятую зону                                                    
                                // gp.Add(GetPathRegionText(point, sf, InclineText.Vertic));
                                gp.Add(new_region);
                            }
                        }

                        break;
                }



            }

            // отображаем текстовые надписи - крюки
            if (buildImage.hooks_length)
            {
                foreach (TextOnRebar tor in buildImage.hooks)
                {
                    if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
                    // Длина надписи
                    SizeF sf = buildImage.graphic.MeasureString(tor.value_str, SketchReinforcementApp.drawFont30);
                    tor.size = sf;

                    if (GeHookVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, new PointF((tor.endF.X - tor.startF.X), (tor.endF.Y - tor.startF.Y)), tor.distF, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10), out new_region))
                    {
                        //flagGraphics.DrawEllipse(Pens.Black, tor.positionF.X + 10, tor.positionF.Y + 10, 2, 2);

                        // if ((tor.positionF.X + moveX + canva + 10) < 0) break;
                        //if ((tor.positionF.X + moveX + canva + 10 + sf.Width - 30 ) > sizeX) break;
                        //if ((tor.positionF.Y + moveY + canva + 10) < 0) break;                   
                        //if ((tor.positionF.Y + moveY + canva + 10 + sf.Height -30  ) > sizeY) break;

                        buildImage.graphic.DrawString(tor.value_str, SketchReinforcementApp.drawFont30, Brushes.Black, tor.positionF);

                        // gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Horiz));
                        gp.Add(new_region);

                        foreach (TextOnRebar toa in buildImage.hooks)
                        {
                            if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные крюки
                        }
                    }
                }
            }


        }

        ///// <summary>
        ///// Создание эскиза дуговых арматурных стержней 
        ///// </summary>

        //public static void PreparedImage(BuildImage buildImage, bool preview = false, float Y=0, bool st=true)
        //{
        //    foreach (TextOnArc tor in buildImage.lg_arc_sorted)
        //    {
        //        tor.arc=true;
        //    }
        //    foreach (TextOnRebar tor in buildImage.lg)
        //    {
        //        tor.repeat=false;
        //    }
        //    foreach (TextOnRebar tor in buildImage.hooks)
        //    {
        //        tor.arc=true;
        //    }

        //    if (!preview)
        //    {
        //        buildImage.graphic = Graphics.FromImage(buildImage.flag);
        //        buildImage.graphic.Clear(System.Drawing.Color.White);
        //    }
        //    else
        //    {
        //        buildImage.graphic.Clear(System.Drawing.Color.White);
        //    }
        //    //buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
        //    //buildImage.graphic.DrawLines(Pens.Yellow, buildImage.pointDF.ToArray());
        //    //buildImage.graphic.DrawEllipse(Pens.Red, 10, 10, 2, 2);
        //    //return;
        //    StringFormat drawFormat = new StringFormat();
        //    drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;
        //    Font    drawFont = new Font("Mipgost", 48);
        //    List<System.Drawing.Drawing2D.GraphicsPath> gp = new List<System.Drawing.Drawing2D.GraphicsPath>();
        //    //if (st)
        //    //{
        //        if (preview) buildImage.graphic.ScaleTransform(0.5F, 0.5F);
        //        buildImage.graphic.TranslateTransform(buildImage.canva + buildImage.moveX, buildImage.canva + buildImage.moveY);
        //    //}

        //        Pen pen = new Pen(System.Drawing.Color.Black, 5);
        //        buildImage.graphic.DrawLines(pen, buildImage.pointDF.ToArray());                                 // рисуем арматуру

        //        // пробуем рисовать начиная с длинных сегментов - вероятно там больше места
        //        if (buildImage.bending)   // если в диалоге имеется отметка
        //        {
        //            foreach (TextOnArc tor in buildImage.lg_arc_sorted)
        //            {
        //                if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
        //                PointF initial_point = new PointF(tor.positionF.X, tor.positionF.Y);
        //                // Длина надписи
        //                SizeF sf = buildImage.graphic.MeasureString("r" + tor.value_str, drawFont);
        //                tor.size = sf;
        //                // flagGraphics.DrawEllipse(Pens.Red, tor.positionF.X, tor.positionF.Y, 2, 2);
        //                int sign = 1;
        //                // выполняем наклонные надписи
        //                if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
        //                {
        //                    if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
        //                    else tor.angle = (float)(tor.angle - Math.PI);
        //                    // изменим точку вставки надписи

        //                    tor.positionF = new PointF((float)(tor.positionF.X - Math.Cos(tor.angle) * sf.Width),
        //                                                (float)(tor.positionF.Y - Math.Sin(tor.angle) * sf.Width));
        //                    // flagGraphics.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 2, 2);
        //                    sign = -1;
        //                }
        //                //Size new_size = new Size((int)(sf.Width * Math.Cos(tor.angle)), (int)(sf.Width * Math.Sin(tor.angle) + sf.Height * Math.Cos(tor.angle)));
        //                //buildImage.graphic.DrawRectangle(Pens.Green, initial_point.X, initial_point.Y, sf.Width, sf.Height);
        //                PointF point;
        //                if (GetRadiusVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out point, tor.angle, buildImage.lg_arc.Count(x => x.arc == true), sign, buildImage.sizeX, buildImage.sizeY, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10)))
        //                {
        //                    tor.positionF = point;                             // изменим на новое положение
        //                    buildImage.graphic.RotateTransform(tor.angle_grad);
        //                    buildImage.graphic.DrawString("r" + tor.value_str, drawFont, Brushes.Black, tor.positionF_rotate);
        //                    buildImage.graphic.RotateTransform(-tor.angle_grad);

        //                    // добавим новую занятую зону                                                    
        //                    gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Incline, tor.angle));
        //                    foreach (TextOnArc toa in buildImage.lg_arc)
        //                    {
        //                        if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные радиусы больше не указываем
        //                    }
        //                }
        //            }
        //        }
        //    // отображаем текстовые надписи - только горизонтальные или вертикальные
        //    foreach (TextOnRebar tor in buildImage.lg)
        //    {
        //        if (tor.repeat) continue;
        //        // Длина надписи
        //        SizeF sf = buildImage.graphic.MeasureString(tor.value_str, drawFont);
        //        tor.size = sf;                                                             // фиксируем размер надписи
        //        switch (tor.incline)
        //        {
        //            case InclineText.Horiz:

        //                // buildImage.graphic.DrawString("TEST", drawFont, Brushes.Black, new PointF(20, 20));

        //                if (GeHorizontalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10)))
        //                {
        //                    buildImage.graphic.DrawString(tor.value_str, drawFont, Brushes.Black, tor.positionF);

        //                    // добавим новую занятую зону                                                    
        //                    gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Horiz));

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline)
        //                        {
        //                            torR.repeat = true;     

        //                        }
        //                    }

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
        //                        {                                   
        //                            torR.dialog = false;    // для всех отключить из диалога
        //                        }
        //                    }

        //                    tor.dialog = true;              // текущий оставить  
        //                }

        //                break;
        //            case InclineText.Vertic:
        //                // для вертикальной линии сдвиг влево
        //                tor.positionF = new PointF(tor.positionF.X - 10, tor.positionF.Y);
        //                if (GetVerticalVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, buildImage.move - 20, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10)))
        //                {
        //                    // if ((tor.positionF.Y + sf.Width + canva / 2) > sizeY) break;
        //                    buildImage.graphic.DrawString(tor.value_str, drawFont, Brushes.Black, tor.positionF, drawFormat);

        //                    // добавим новую занятую зону                                                    
        //                    gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Vertic));

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline)
        //                        { torR.repeat = true;

        //                        }
        //                    }

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
        //                        {

        //                            torR.dialog = false;    // для всех отключить из диалога
        //                        }
        //                    }

        //                    // сделаем отметку для диалога
        //                    tor.dialog = true;
        //                }
        //                break;
        //            default:
        //                continue;

        //        }
        //    }

        //    // отображаем текстовые надписи - только наклонные
        //    foreach (TextOnRebar tor in buildImage.lg)
        //    {
        //        if (tor.repeat) continue;
        //        if (tor.value == 0) continue;
        //        // Длина надписи
        //        SizeF sf = buildImage.graphic.MeasureString(tor.value_str, drawFont);
        //        tor.size = sf;                                                             // фиксируем размер надписи
        //        switch (tor.incline)
        //        {
        //            case InclineText.Horiz:
        //                continue;
        //            case InclineText.Vertic:
        //                continue;
        //            default:

        //                // buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 5, 5);
        //                // выполняем наклонные надписи
        //                if (Math.Abs(tor.angle) > Math.PI / 2 && Math.Abs(tor.angle) <= Math.PI)
        //                {
        //                    if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
        //                    else tor.angle = (float)(tor.angle - Math.PI);
        //                    // сдвиг на высоту надписи
        //                    tor.positionF = new PointF((float)(tor.positionF.X + sf.Height * Math.Sin(tor.angle)), (float)(tor.positionF.Y - sf.Height * Math.Cos(tor.angle)));
        //                    // buildImage.graphic.DrawEllipse(Pens.Brown, tor.positionF.X, tor.positionF.Y, 5, 5);
        //                }

        //                PointF point, positionF;
        //                positionF = tor.positionF;
        //                if (GetInclineVariantPositionText(buildImage.graphic, gp, buildImage.line2D_L, tor.positionF, sf, out point, tor.angle))
        //                {
        //                    tor.positionF = point;                             // изменим на новое положение 
        //                    //buildImage.graphic.DrawEllipse(Pens.Blue, tor.positionF.X, tor.positionF.Y, 5, 5);
        //                    buildImage.graphic.RotateTransform(tor.angle_grad);
        //                    buildImage.graphic.DrawString(tor.value_str, drawFont, Brushes.Black, tor.positionF_rotate);
        //                    buildImage.graphic.RotateTransform(-tor.angle_grad);
        //                    // сделаем отметку для диалога
        //                    tor.dialog = true;
        //                    // добавим новую занятую зону                                                    
        //                    gp.Add(GetPathRegionText(point, sf, InclineText.Incline, tor.angle));

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline)
        //                        { 
        //                            torR.repeat = true;

        //                        }
        //                    }

        //                    // отключаем такие же размеры сходного направления
        //                    foreach (TextOnRebar torR in buildImage.lg)
        //                    {
        //                        if (torR.value_str == tor.value_str && torR.incline == tor.incline && torR.name == tor.name)
        //                        {

        //                            torR.dialog = false;    // для всех отключить из диалога
        //                        }
        //                    }

        //                    // сделаем отметку для диалога
        //                    tor.dialog = true;

        //                }
        //                // выполняем горизонтальные проекционные надписи
        //                if (tor.valueH > 0)
        //                {
        //                    sf = buildImage.graphic.MeasureString(tor.valueH_str, drawFont);
        //                    tor.size = sf;                                                             // фиксируем размер надписи
        //                    if (GeTHorizontalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, positionF, sf, out point, buildImage.sizeY - buildImage.canva - sf.Height - buildImage.moveY))
        //                    {
        //                        //if ((point.X + moveX + canva + 10) < 0) break;
        //                        //if ((point.X + sf.Width + moveX + canva - 10) > sizeX) break;
        //                        //if ((point.Y + moveY + canva + 10) < 0) break;
        //                        //if ((point.Y + sf.Height + moveY + canva - 10) > sizeY) break;
        //                        buildImage.graphic.DrawString(tor.valueH_str, drawFont, Brushes.Blue, point);
        //                        // добавим новую занятую зону                                                    
        //                        gp.Add(GetPathRegionText(point, sf, InclineText.Horiz));
        //                    }
        //                }
        //                // выполняем вертикальные проекционные надписи
        //                if (tor.valueV > 0)
        //                {
        //                    sf = buildImage.graphic.MeasureString(tor.valueV_str, drawFont);
        //                    tor.size = sf;                                                             // фиксируем размер надписи
        //                    if (GeTVerticalProjectPositionText(buildImage.graphic, gp, buildImage.line2D_L, positionF, sf, out point, buildImage.sizeX, (float)(buildImage.canva + buildImage.moveX)))
        //                    {
        //                        //if ((point.Y + moveY + canva + 10) < 0) break;
        //                        //if ((point.Y + sf.Width + canva + moveY - 10) > sizeY) break;
        //                        //if ((point.X + moveX + canva + 10) < 0) break;
        //                        //if ((point.X + sf.Height + canva + moveX + 10) > sizeX) break;
        //                        buildImage.graphic.DrawString(tor.valueV_str, drawFont, Brushes.Blue, point, drawFormat);
        //                        // добавим новую занятую зону                                                    
        //                        gp.Add(GetPathRegionText(point, sf, InclineText.Vertic));
        //                    }
        //                }

        //                break;
        //        }



        //    }

        //    // отображаем текстовые надписи - крюки
        //    if (buildImage.hooks_length)
        //    {
        //        foreach (TextOnRebar tor in buildImage.hooks)
        //        {
        //            if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
        //            // Длина надписи
        //            SizeF sf = buildImage.graphic.MeasureString(tor.value_str, drawFont);
        //            tor.size = sf;

        //            if (GeHookVariantPositionText(buildImage.graphic, gp, buildImage.line2D, tor.positionF, sf, out tor.positionF, buildImage.sizeX, buildImage.sizeY, new PointF((tor.endF.X - tor.startF.X), (tor.endF.Y - tor.startF.Y)), tor.distF, (float)(buildImage.moveX + buildImage.canva + 10), (float)(buildImage.moveY + buildImage.canva + 10)))
        //            {
        //                //flagGraphics.DrawEllipse(Pens.Black, tor.positionF.X + 10, tor.positionF.Y + 10, 2, 2);

        //                // if ((tor.positionF.X + moveX + canva + 10) < 0) break;
        //                //if ((tor.positionF.X + moveX + canva + 10 + sf.Width - 30 ) > sizeX) break;
        //                //if ((tor.positionF.Y + moveY + canva + 10) < 0) break;                   
        //                //if ((tor.positionF.Y + moveY + canva + 10 + sf.Height -30  ) > sizeY) break;

        //                buildImage.graphic.DrawString(tor.value_str, drawFont, Brushes.DarkGreen, tor.positionF);

        //                gp.Add(GetPathRegionText(tor.positionF, sf, InclineText.Horiz));

        //                foreach (TextOnRebar toa in buildImage.hooks)
        //                {
        //                    if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные крюки
        //                }
        //            }
        //        }
        //    }


        //}

        /// <summary>
        /// Создание эскиза по стандарту 
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>        
        /// <param name="sketch">Имя стандартного рисунка</param>
        /// <param name="image">Имя файла стандарта</param>     
        /// <param name="show_length_hooks">Показать длину крюков</param>
        static bool CreateBitmapStandart(Element rebar, string sketch, bool show_length_hooks, string image, int coef_diam, bool IsDimLines)
        {
            //StartF = DateTime.Now;
            //DateTime Stoped0 = DateTime.Now;
            //TimeSpan Elapsed0 = Stoped0.Subtract(Start0);
            //writer.WriteLine("Вошли в процедуру: ");
            //writer.WriteLine(Convert.ToString(Elapsed0.TotalSeconds));

            //DateTime Start = DateTime.Now;

            Bitmap bitmap;

            // имя папки
            int start = sketch.IndexOf('(');
            int end = sketch.IndexOf(')');
            if (start < 0 || end < 0) return false;   // папка не найдена
            string folder_code = sketch.Substring(start + 1, end - start - 1);

            string UserFolderImage = ""; // рабочее значение. Удалить  в дальнейшем

            //string UserFolderImage = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
            //UserFolderImage = UserFolderImage + "\\Autodesk\\Downloaded Content\\Ar-Cadia\\SketchReinforcement\\"+folder_code+"\\";           // путь к папке рисунков пользователя

            string FolderImage = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string FolderHook = FolderImage + "\\Resources\\Images\\Hooks";

            // if(folder_code == "Gost21-501") folder_code="BS8666-2005";

            if (!IsDimLines)
                FolderImage = FolderImage + "\\Resources\\Images\\" + folder_code + "\\";            // путь к папке рисунков
            else
            {
                // UserFolderImage = FolderImage + "\\Resources\\UserImagesDL\\" + folder_code + "\\";
                FolderImage = FolderImage + "\\Resources\\ImagesDL\\" + folder_code + "\\";                
            }


            switch (sketch)
            {
                #region SPANISH
                case "M-00 (ESP).png":
                case "M-01 (ESP).png":
                case "M-02 (ESP).png":
                case "M-08 (ESP).png":
                case "M-18 (ESP).png":
                case "M-T9 (ESP).png":
                case "M-24 (ESP).png":
                    Form1 formM_00 = new Form1(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam,"B");
                    bitmap = formM_00.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-03 (ESP).png":
                case "M-04 (ESP).png":
                    Form46 formM_03 = new Form46(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,"B","C","D","H","F");
                    bitmap = formM_03.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-05 (ESP).png":
                case "M-06 (ESP).png":                
                    Form26 formM_05 = new Form26(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "B","C","D","H");
                    bitmap = formM_05.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-07 (ESP).png":
                    Form25 formM_07 = new Form25(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "C","E","H","H","D");
                    bitmap = formM_07.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-09 (ESP).png":
                case "M-T4 (ESP).png":
                    Form67 formM_09 = new Form67(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "B", "O","H","R");
                    bitmap = formM_09.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-10 (ESP).png":
                    Form13 formM_10 = new Form13(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam, "A","O","C");
                    bitmap = formM_10.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-11 (ESP).png":
                    Form12 formM_11 = new Form12(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam, "O","H");
                    bitmap = formM_11.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-12 (ESP).png":
                    Form27 formM_12 = new Form27(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "D","C","B","H");
                    bitmap = formM_12.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-13 (ESP).png":
                    Form14 formM_13 = new Form14(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "B","D","H");
                    bitmap = formM_13.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-14 (ESP).png":
                    FormM_14 formM_14 = new FormM_14(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = formM_14.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-16 (ESP).png":
                    Form27 formM_27 = new Form27(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "D", "C", "B", "H");
                    bitmap = formM_27.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-17 (ESP).png":
                case "M-S1 (ESP).png":
                case "M-S2 (ESP).png":
                case "M-S3 (ESP).png":
                case "M-S4 (ESP).png":
                case "M-S5 (ESP).png":
                case "M-S6 (ESP).png":
                    Form21 formM_17 = new Form21(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam, "B","C","D");
                    bitmap = formM_17.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-17A (ESP).png":
                    Form11 formM_17A = new Form11(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam,"B","C");
                    bitmap = formM_17A.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-19 (ESP).png":
                    FormM_19 formM_19 = new FormM_19(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = formM_19.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-20 (ESP).png":
                    FormM_20 formM_20 = new FormM_20(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,"B","C","D");
                    bitmap = formM_20.bitmap;
                    if (bitmap == null) return false;                    
                    break;
                case "M-22 (ESP).png":
                case "M-23 (ESP).png":
                    FormM_22 formM_22 = new FormM_22(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = formM_22.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-SP (ESP).png":
                    Form77 formM_77 = new Form77(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,"O","K");
                    bitmap = formM_77.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-T1 (ESP).png":
                case "M-T2 (ESP).png":
                case "M-T6 (ESP).png":
                    Form31 formM_T1 = new Form31(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,"B","C","D","E");
                    bitmap = formM_T1.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-T1 closed (ESP).png":
                    if (IsDimLines) show_length_hooks = true;
                    FormT1_closed formM_T1closed = new FormT1_closed(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, "B", "C");
                    bitmap = formM_T1closed.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-T7 (ESP).png":
                    FormM_T7 formM_T7 = new FormM_T7(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = formM_T7.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-T8 (ESP).png":
                    FormM_T8 formM_T8 = new FormM_T8(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = formM_T8.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "M-T3 (ESP).png":
                    Form75 formM_T3 = new Form75(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,"O","G");
                    bitmap = formM_T3.bitmap;
                    if (bitmap == null) return false;
                    break;
                #endregion

                case "RebarIn - (BS8666-2005).png":
                    FormRebarIn formIn = new FormRebarIn(rebar, show_length_hooks, "00 - (BS8666-2005).png", FolderImage, UserFolderImage, FolderHook);
                    bitmap = formIn.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "00 - (BS8666-2005).png":
                    Form1 form1 = new Form1(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,coef_diam);
                    bitmap = form1.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "1 - (Gost21-501).png":
                    // DateTime Start1 = DateTime.Now;
                    form1 = new Form1(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,coef_diam);
                    bitmap = form1.bitmap;
                    if (bitmap == null) return false;
                    // DateTime Stoped1 = DateTime.Now;
                    //TimeSpan Elapsed1 = Stoped1.Subtract(Start1);
                    //writer.WriteLine("Форма 1 - стандарт: ");
                    //writer.WriteLine(Convert.ToString(Elapsed1.TotalSeconds));
                    break;
                case "01 - (BS8666-2005).png":
                    Form1 form01 = new Form1(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook,coef_diam);
                    bitmap = form01.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "04 - (CIS).png":
                    Form04 form04 = new Form04(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form04.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "11 - (BS8666-2005).png":
                    Form11 form11 = new Form11(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form11.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "2 - (Gost21-501).png":
                    form11 = new Form11(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form11.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "12 - (BS8666-2005).png":
                    Form12 form12 = new Form12(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form12.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "3 - (Gost21-501).png":
                    form12 = new Form12(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form12.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "13 - (BS8666-2005).png":
                    Form13 form13 = new Form13(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form13.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "14 - (BS8666-2005).png":
                    Form14 form14 = new Form14(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form14.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "15 - (BS8666-2005).png":
                    Form15 form15 = new Form15(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form15.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "21 - (BS8666-2005).png":
                    Form21 form21 = new Form21(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form21.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "4 - (Gost21-501).png":
                    form21 = new Form21(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form21.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "5 - (Gost21-501).png":
                    Form5 form5 = new Form5(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form5.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "6 - (Gost21-501).png":
                    form21 = new Form21(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form21.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "7 - (Gost21-501).png":
                    Form25 form25 = new Form25(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form25.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "8 - (Gost21-501).png":
                    Form26 form26 = new Form26(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form26.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "9 - (Gost21-501).png":
                    Form31 form31 = new Form31(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form31.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "10 - (Gost21-501).png":
                    Form51 form51 = new Form51(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form51.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "10T - (Gost21-501).png":
                    Form51T form51T = new Form51T(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form51T.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "11 - (Gost21-501).png":
                    Form41 form41 = new Form41(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form41.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "12 - (Gost21-501).png":
                    Form44 form44 = new Form44(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form44.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "13 - (Gost21-501).png":
                    Form13Gost form13Gost = new Form13Gost(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form13Gost.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "14 - (Gost21-501).png":
                    Form46 form46 = new Form46(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form46.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "15 - (Gost21-501).png":
                    Form15Gost form15Gost = new Form15Gost(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form15Gost.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "16 - (Gost21-501).png":
                    Form67 form67 = new Form67(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form67.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "22 - (BS8666-2005).png":
                    Form22 form22 = new Form22(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form22.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "23 - (BS8666-2005).png":
                    Form23 form23 = new Form23(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form23.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "24 - (BS8666-2005).png":
                    Form24 form24 = new Form24(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form24.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "24 - (Gost21-501).png":
                    Form24Gost form24Gost = new Form24Gost(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form24Gost.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "25 - (BS8666-2005).png":
                    form25 = new Form25(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form25.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "25 - (Gost21-501).png":
                    form13 = new Form13(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook, coef_diam);
                    bitmap = form13.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "26 - (BS8666-2005).png":
                    form26 = new Form26(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form26.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "26 - (Gost21-501).png":
                    Form26Gost form26Gost = new Form26Gost(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form26Gost.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "27 - (BS8666-2005).png":
                    Form27 form27 = new Form27(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form27.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "27 - (Gost21-501).png":
                    Form27Gost form27Gost = new Form27Gost(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form27Gost.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "28 - (BS8666-2005).png":
                    Form28 form28 = new Form28(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form28.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "29 - (BS8666-2005).png":
                    Form29 form29 = new Form29(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form29.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "31 - (BS8666-2005).png":
                    form31 = new Form31(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form31.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "32 - (BS8666-2005).png":
                    Form32 form32 = new Form32(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form32.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "33 - (BS8666-2005).png":
                    Form33 form33 = new Form33(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form33.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "34 - (BS8666-2005).png":
                    Form34 form34 = new Form34(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form34.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "35 - (BS8666-2005).png":
                    Form35 form35 = new Form35(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form35.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "36 - (BS8666-2005).png":
                    Form36 form36 = new Form36(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form36.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "41 - (BS8666-2005).png":
                    form41 = new Form41(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form41.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "44 - (BS8666-2005).png":
                    form44 = new Form44(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form44.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "46 - (Slovakia).png":
                    Form46S form46S = new Form46S(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form46S.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "46 - (BS8666-2005).png":
                    form46 = new Form46(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form46.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "47 - (BS8666-2005).png":
                    Form47 form47 = new Form47(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form47.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "51 - (BS8666-2005).png":
                    form51 = new Form51(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form51.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "56 - (BS8666-2005).png":
                    Form56 form56 = new Form56(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form56.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "63 - (BS8666-2005).png":
                    Form63 form63 = new Form63(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form63.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "64 - (BS8666-2005).png":
                    Form64 form64 = new Form64(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form64.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "67 - (BS8666-2005).png":
                    form67 = new Form67(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form67.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "75 - (BS8666-2005).png":
                    Form75 form75 = new Form75(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form75.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "75 - (Gost21-501).png":
                    form75 = new Form75(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form75.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "77 - (BS8666-2005).png":
                    Form77 form77 = new Form77(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form77.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "77 - (Gost21-501).png":
                    form77 = new Form77(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form77.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "77 - (Chili).png":
                    Form77Chili form77Chili = new Form77Chili(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form77Chili.bitmap;
                    if (bitmap == null) return false;
                    break;
                case "87 - (Chili).png":
                    Form87 form87 = new Form87(rebar, show_length_hooks, sketch, FolderImage, UserFolderImage, FolderHook);
                    bitmap = form87.bitmap;
                    if (bitmap == null) return false;
                    break;
               
                default:
                    return false;       // если форма не найдена

            }
             
            // bitmap.Save(@path_name + "\\" + rebar.Id.IntegerValue.ToString() + ".png");

           
            bitmap.Save(image);
            //DateTime Stoped = DateTime.Now;
            //TimeSpan Elapsed = Stoped.Subtract(Start);
            //writer.WriteLine("Форма 1 - сохранена: ");
            //writer.WriteLine(Convert.ToString(Elapsed.TotalSeconds));
            //StartF = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Создание эскиза дуговых арматурных стержней 
        /// </summary>
        /// <param name="rebar">Арматурный стержень</param>
        
        static bool CreateBitmapRebarArcs(Element rebar, Template template, DataForm dataform, string image)
        {
            #region Параметры 
            // тексты для отображения
            TextOnRebar X_max = new TextOnRebar();
            TextOnRebar X_max_d = new TextOnRebar();
            TextOnRebar Y_max = new TextOnRebar();
            TextOnRebar Y_max_d = new TextOnRebar();

            // повтор с новыми параметрами
            int repeat = 0;
            // размеры фигуры
            int sizeX, sizeY;           
            float scale,scaleX,scaleY;   // определим масштаб
            // готовим рисунок
            Bitmap flag;
            // получить координаты крайних точек
            float minX, minY, maxX, maxY;
            float dX = 0;
            float dY = 0;
            Graphics flagGraphics;
            Pen pen = new Pen(System.Drawing.Color.Black, 5);
            // Font drawFont = new Font("Mipgost", 48);
            // Font drawFontR = new Font("Mipgost", 24);
            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical; 
            Document doc = null;
            RebarShape rs = null;
            #endregion Параметры

            
            
            // направление оси Z - перпендикулярно плоскости стержня
            Vector4 zAxis = new Vector4(XYZ.Zero);

            // список сегментов для стержня проекта   
            RebarBendData rbd = null;
            IList<Curve> ilc = new List<Curve>();
            Rebar rebarOne = rebar as Rebar;
            RebarInSystem rebarIn = rebar as RebarInSystem;
            // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
            if (rebarOne != null)
            {
                ilc = rebarOne.GetCenterlineCurves(false, false, false,MultiplanarOption.IncludeOnlyPlanarCurves,rebarOne.NumberOfBarPositions-1); doc = rebarOne.Document;
                // получить данные по форме стержня
                rs = rebarOne.Document.GetElement(rebarOne.GetShapeId()) as RebarShape;                
                zAxis = new Vector4(rebarOne.GetShapeDrivenAccessor().Normal);
                rbd = rebarOne.GetBendData();
            }

            if (rebarIn != null)
            {
                ilc = rebarIn.GetCenterlineCurves(false, false, false); doc = rebarIn.Document;
                // получить данные по форме стержня
                rs = rebarIn.Document.GetElement(rebarIn.RebarShapeId) as RebarShape;
                zAxis = new Vector4(rebarIn.Normal);
                rbd = rebarIn.GetBendData();
            }

            if (doc == null) return false;
            zAxis.Normalize();

            // ПОДГОТОВКА ДАННЫХ ДЛЯ ТЕКСТОВЫХ НАДПИСЕЙ
            RebarShapeDefinition rsd = rs.GetRebarShapeDefinition();
            RebarShapeDefinitionByArc rarc = rsd as RebarShapeDefinitionByArc;
            RebarShapeDefinitionBySegments rsds = rsd as RebarShapeDefinitionBySegments;

            if (rsds == null && rarc == null) return false;   // формы не определяются

            List<Line2D> line2D = new List<Line2D>();                                      // список плоских линий для чертежа                       
            List<System.Drawing.PointF> pointDF_arc = new List<System.Drawing.PointF>();      // список точек для картинки
            List<System.Drawing.Drawing2D.GraphicsPath> gp = new List<System.Drawing.Drawing2D.GraphicsPath>();

            ElementId hook_start_arc = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_START_TYPE).AsElementId();
            ElementId hook_end_arc = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_HOOK_END_TYPE).AsElementId();
            List<TextOnRebar> hooks = new List<TextOnRebar>();                                         // Список параметров для крюков 
            int ilc_start = 0;
            // получить информацию по крюкам (начало)
            if (hook_start_arc.IntegerValue > 0)
            {
                hooks.Add(GetHookStart(ilc, rebar,rbd));                                                    // добавим информацию по крюку
                ilc_start = 2;                                                      
            }
            if (hook_end_arc.IntegerValue > 0)
            {
                hooks.Add(GetHookEnd(ilc, rebar,rbd));                                                        // добавим информацию по крюку                 
            }
            new_form:

            if (hooks.Count() > 0 && repeat==1)   // выполним масштабирование по крюкам            
            {

                // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
                if (rebarOne != null)
                {
                    ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                }

                if (rebarIn != null)
                {
                    ilc = rebarIn.GetCenterlineCurves(false, false, false);
                }
                hooks.Clear();
                // получить информацию по крюкам (начало)
                if (hook_start_arc.IntegerValue > 0)
                {
                    hooks.Add(GetHookStart(ilc, rebar,rbd));                                                    // добавим информацию по крюку                                                                      
                }
                if (hook_end_arc.IntegerValue > 0)
                {
                    hooks.Add(GetHookEnd(ilc, rebar,rbd));                                                        // добавим информацию по крюку                 
                }

                repeat = 2;
                // получить максимальную длину крюка
                double max_hook = hooks.Max(x => x.value);
                ParameterSet pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
                IList<RebarShapeConstraint> rcs = rarc.GetConstraints();
                Parameter paramChord = null;
                Parameter paramArc = null;
                Parameter paramRadius = null;
                double Chord = 0, ArcA = 20*max_hook, Radius = 0;
                

                switch (rarc.Type)
                {
                    case RebarShapeDefinitionByArcType.Spiral:                       
                        // для спирали крюки не показываем                        
                        break;
                    case RebarShapeDefinitionByArcType.Arc:
                    
                    foreach (RebarShapeConstraint rsc in rcs)                                         // разбираем каждый сегмент в отдельности
                    {                     
                        if (rsc.GetType().Name == "RebarShapeConstraintChordLength")
                        {
                            ElementId pid = rsc.GetParamId();
                            Element elem = doc.GetElement(pid);
                            foreach (Parameter pr in pset)
                            {
                                if (pr.Definition.Name == elem.Name)
                                {
                                    paramChord = pr;
                                    Chord = rebar.get_Parameter(paramChord.GUID).AsDouble();
                                }
                            }
                        }

                        if (rsc.GetType().Name == "RebarShapeConstraintArcLength")
                        {
                            ElementId pid = rsc.GetParamId();
                            Element elem = doc.GetElement(pid);
                            foreach (Parameter pr in pset)
                            {
                                if (pr.Definition.Name == elem.Name)
                                {
                                    paramArc = pr;
                                    ArcA = rebar.get_Parameter(paramArc.GUID).AsDouble();
                                }
                            }
                        }
                        
                    }

                    if (ArcA > 25 * max_hook)
                    {    
                        double ArcA_new = 25 *max_hook;                                              
                        double Chord_new = ArcA / 2;

                        SubTransaction st = new SubTransaction(doc);
                        st.Start();
                            //SketchTools.SetParameter(rebar, paramChord.GUID, ArcA_new);
                            //SketchTools.SetParameter(rebar, paramArc.GUID, Chord_new);
                            SketchTools.SetParameter(rebar, paramArc.GUID, ArcA_new);
                            SketchTools.SetParameter(rebar, paramChord.GUID, Chord_new);
                            doc.Regenerate();  
                                        
                        // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
                        if (rebarOne != null)
                        {
                            ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                        }

                        if (rebarIn != null)
                        {
                            ilc = rebarIn.GetCenterlineCurves(false, false, false);
                        }

                        hooks.Clear();
                        // получить информацию по крюкам (начало)
                        if (hook_start_arc.IntegerValue > 0)
                        {
                            hooks.Add(GetHookStart(ilc, rebar,rbd));                                                    // добавим информацию по крюку                                                                      
                        }
                        if (hook_end_arc.IntegerValue > 0)
                        {
                            hooks.Add(GetHookEnd(ilc, rebar,rbd));                                                        // добавим информацию по крюку                 
                        }
                        st.RollBack();                        
                        // восстановить форму стержня
                        SketchTools.SetParameter(rebar, paramArc.GUID, ArcA);
                        SketchTools.SetParameter(rebar, paramChord.GUID, Chord);
                        doc.Regenerate(); 
                    }

                        break;
                    case RebarShapeDefinitionByArcType.LappedCircle:
                    
                    pset = rebar.Parameters;                                              // набор параметров для текущего стержня (версия 2015) 
                    rcs= rarc.GetConstraints();
                    paramRadius=null;
                    paramArc = null;
                    Radius=0;
                    double  Arc=0;
                    foreach (RebarShapeConstraint rsc in rcs)                                         // разбираем каждый сегмент в отдельности
                    {
                        if (rsc.GetType().Name == "RebarShapeConstraintRadius")
                        {
                            ElementId pid = rsc.GetParamId();
                            Element elem = doc.GetElement(pid);
                            foreach (Parameter pr in pset)
                            {
                                if (pr.Definition.Name == elem.Name)
                                {
                                    paramRadius = pr;
                                    Radius = rebar.get_Parameter(paramRadius.GUID).AsDouble();
                                }
                            }
                        }

                        if (rsc.GetType().Name == "RebarShapeConstraintArcLength")
                        {
                            ElementId pid = rsc.GetParamId();
                            Element elem = doc.GetElement(pid);
                            foreach (Parameter pr in pset)
                            {
                                if (pr.Definition.Name == elem.Name)
                                {
                                    paramArc = pr;
                                    Arc = rebar.get_Parameter(paramArc.GUID).AsDouble();
                                }
                            }
                        }
                        
                    }
                    if (Radius > 3 * max_hook)
                    {
                        double Radius_new = 3 * max_hook;
                        double Arc_new  = 2*max_hook; 
                        SubTransaction st = new SubTransaction(doc);
                        st.Start();
                        SketchTools.SetParameter(rebar, paramRadius.GUID, Radius_new);
                        SketchTools.SetParameter(rebar, paramArc.GUID, Arc_new);
                        doc.Regenerate();                       
                       
                        // здесь выполняем разделение по типам возможного армирования: отдельные стержни или стержни в системе
                        if (rebarOne != null)
                        {
                            ilc = rebarOne.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebarOne.NumberOfBarPositions - 1);
                        }

                        if (rebarIn != null)
                        {
                            ilc = rebarIn.GetCenterlineCurves(false, false, false);
                        }
                        hooks.Clear();
                        // получить информацию по крюкам (начало)
                        if (hook_start_arc.IntegerValue > 0)
                        {
                            hooks.Add(GetHookStart(ilc, rebar,rbd));                                                    // добавим информацию по крюку                                                                      
                        }
                        if (hook_end_arc.IntegerValue > 0)
                        {
                            hooks.Add(GetHookEnd(ilc, rebar,rbd));                                                        // добавим информацию по крюку                 
                        }
                        st.RollBack();
                        // восстановить форму стержня
                        SketchTools.SetParameter(rebar, paramRadius.GUID, Radius);
                        SketchTools.SetParameter(rebar, paramArc.GUID, Arc);
                        doc.Regenerate();
                    }
                        break;
                }
            }
            
            // получить матрицу преобразований координат: из общей системы в локальную систему стержня                
            // начало системы координат принимаем в произвольной точке стержня 
            Vector4 origin_arc = new Vector4(ilc[ilc_start].GetEndPoint(0));
            // направление оси Х 
            Vector4 xAxis_arc = new Vector4((ilc[ilc_start].GetEndPoint(1) - ilc[0].GetEndPoint(0)));
            xAxis_arc.Normalize();
            // направление оси Y стены
            Vector4 yAxis_arc = new Vector4(XYZ.Zero);
            yAxis_arc = Vector4.CrossProduct(xAxis_arc, zAxis);
            yAxis_arc.Normalize();

            Matrix4 MatrixMain_arc;

            if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
            {
                MatrixMain_arc = new Matrix4(zAxis, yAxis_arc, xAxis_arc, origin_arc);
            }
            else
            {
                MatrixMain_arc = new Matrix4(xAxis_arc, yAxis_arc, zAxis, origin_arc);

            }

            MatrixMain_arc = MatrixMain_arc.Inverse();

            // выполним расчет точек для чертежа линий арматуры
            foreach (Curve c in ilc)
            {
                IList<XYZ> tp = c.Tessellate();
                for (int i = 0; i < tp.Count - 1; i++)
                {
                    XYZ p1 = tp[i];
                    XYZ p2 = tp[i + 1];
                    Vector4 p_new1 = MatrixMain_arc.Transform(new Vector4(p1));                      // получить точку в локальной системе координат
                    PointF p_new1F = new System.Drawing.PointF(p_new1.X, p_new1.Y);
                    pointDF_arc.Add(p_new1F);                                                        // получить точку для картинки
                    Vector4 p_new2 = MatrixMain_arc.Transform(new Vector4(p2));                      // получить точку в локальной системе координат
                    PointF p_new2F = new System.Drawing.PointF(p_new2.X, p_new2.Y);
                    pointDF_arc.Add(p_new2F);                                                       // получить точку для картинки
                    Line2D line = new Line2D(p_new1F, p_new2F);
                    line2D.Add(line);
                    
                }
            }


            foreach (TextOnRebar tor in hooks)
            {
                Vector4 p_new = MatrixMain_arc.Transform(new Vector4(tor.position));                       // получить точку в локальной системе координат                     
                tor.positionF = new System.Drawing.PointF(p_new.X, p_new.Y);                              // получить точку для картинки
                p_new = MatrixMain_arc.Transform(new Vector4(tor.start));                                    // получить точку в локальной системе координат
                tor.startF = new System.Drawing.PointF(p_new.X, p_new.Y);        // получить точку для картинки
                p_new = MatrixMain_arc.Transform(new Vector4(tor.end));                                    // получить точку в локальной системе координат
                tor.endF = new System.Drawing.PointF(p_new.X, p_new.Y);        // получить точку для картинки
            }

            SketchTools.GetExtremePoints(pointDF_arc, out minX, out minY, out maxX, out maxY);
            // все точки должны быть в 1 четверти
            if (minX < 0)
            {
                for (int i = 0; i < pointDF_arc.Count(); i++)
                {
                    pointDF_arc[i] = new PointF(pointDF_arc[i].X - minX, pointDF_arc[i].Y);
                    dX = minX;
                }
                foreach (TextOnRebar tor in hooks)
                {
                    tor.positionF = new PointF(tor.positionF.X - minX, tor.positionF.Y);
                    tor.startF = new PointF(tor.startF.X - minX, tor.startF.Y);
                    tor.endF = new PointF(tor.endF.X - minX, tor.endF.Y);
                }
            }
            if (minY < 0)
            {
                for (int i = 0; i < pointDF_arc.Count(); i++)
                {
                    pointDF_arc[i] = new PointF(pointDF_arc[i].X, pointDF_arc[i].Y - minY);
                    dY = minY;
                }
                foreach (TextOnRebar tor in hooks)
                {
                    tor.positionF = new PointF(tor.positionF.X, tor.positionF.Y - minY);
                    tor.startF = new PointF(tor.startF.X, tor.startF.Y - minY);
                    tor.endF = new PointF(tor.endF.X, tor.endF.Y - minY);
                }

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

            SketchTools.GetExtremePoints(pointDF_arc, out minX, out minY, out maxX, out maxY);


            // получить диаметр стержня
            double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
 
            // размеры фигуры
            sizeX = 1000;
            sizeY = 300;
            int canva = 77;
            // float move = 90;                              // смещение (размер) надписи от линии по умолчанию -высота шрифта 48
            // определим масштаб
            scaleX = (float)((sizeX - 2 * canva) / maxX);
            scaleY = (float)((sizeY - 2 * canva) / maxY);

            scale = Math.Min(scaleX, scaleY);

            for (int i = 0; i < pointDF_arc.Count; i++)
            {
                pointDF_arc[i] = new PointF(pointDF_arc[i].X * scale, pointDF_arc[i].Y * scale);
            }
            foreach (TextOnRebar tor in hooks)
            {
                tor.positionF = new PointF(tor.positionF.X * scale + 1, tor.positionF.Y * scale + 1);
                tor.startF = new PointF(tor.startF.X * scale + 1, tor.startF.Y * scale + 1);
                tor.endF = new PointF(tor.endF.X * scale + 1, tor.endF.Y * scale + 1);
            }

            for (int i = 0; i < line2D.Count; i++)
            {
                line2D[i] = new Line2D(new PointF(line2D[i].p1F.X * scale, line2D[i].p1F.Y * scale), new PointF(line2D[i].p2F.X * scale, line2D[i].p2F.Y * scale));
            }

            float minXscale, minYscale, maxXscale, maxYscale;
            SketchTools.GetExtremePoints(pointDF_arc, out minXscale, out minYscale, out maxXscale, out maxYscale);

            // готовим рисунок
            flag = new Bitmap(sizeX, sizeY);
            flagGraphics = Graphics.FromImage(flag);

            // flagGraphics.Clear(System.Drawing.Color.White);

            flagGraphics.Clear(System.Drawing.Color.Transparent);

            float moveX = (sizeX - 2 * canva - maxXscale) / 2;
            float moveY = (sizeY - 2 * canva - maxYscale) / 2;
            flagGraphics.TranslateTransform(canva + moveX, canva + moveY);

            float middle_X = sizeX / 2 - canva - moveX;
            float middle_Y = sizeY / 2 - canva - moveY;
            float bottom_Y = sizeY - canva - moveY + 10;
            float left_X = -canva - moveX -10;

            
            #region Тексты для отображения
            if (repeat == 0)
            {
                X_max.rebar = rebar;
                if (repeat == 0) X_max.value = maxX;
                if (template == Template.Rus && rarc.Type == RebarShapeDefinitionByArcType.LappedCircle) X_max.value = maxX - d;
                SizeF sf = flagGraphics.MeasureString(X_max.value_str, SketchReinforcementApp.drawFont);
                X_max.size = sf;
                X_max.positionF = new PointF((float)(middle_X - sf.Width / 2), (float)(bottom_Y - sf.Height + SketchReinforcementApp.shift_font_arc));
                // X_max.positionF = new PointF((float)(middle_X - sf.Width / 2), 0.0f);
                X_max.incline = InclineText.Horiz;


                X_max_d.rebar = rebar;
                if (repeat == 0) X_max_d.value = maxX + d;
                sf = flagGraphics.MeasureString(X_max_d.value_str, SketchReinforcementApp.drawFont);
                X_max_d.size = sf;
                X_max_d.positionF = new PointF((float)(middle_X - sf.Width / 2), (float)(bottom_Y - sf.Height + SketchReinforcementApp.shift_font_arc));
                // X_max_d.positionF = new PointF((float)(middle_X - sf.Width / 2), 0.0f);
                X_max_d.incline = InclineText.Horiz;


                Y_max.rebar = rebar;
                if (repeat == 0) Y_max.value = maxY;
                sf = flagGraphics.MeasureString(Y_max.value_str, SketchReinforcementApp.drawFont);
                Y_max.size = sf;
                Y_max.positionF = new PointF((float)(left_X), (float)(middle_Y - sf.Width / 2));
                Y_max.incline = InclineText.Vertic;


                Y_max_d.rebar = rebar;
                if (repeat == 0) Y_max_d.value = maxY + d;
                sf = flagGraphics.MeasureString(Y_max_d.value_str, SketchReinforcementApp.drawFont);
                Y_max_d.size = sf;
                Y_max_d.positionF = new PointF((float)(left_X), (float)(middle_Y - sf.Width / 2));
                Y_max_d.incline = InclineText.Vertic;
            }
            #endregion Тексты для отображения

            if (hooks.Count() > 0 && repeat < 2)   // выполним масштабирование по крюкам
            {
                repeat = 1;
                pointDF_arc.Clear();                
                ilc.Clear();
                line2D.Clear();
                goto new_form;
            }

            List<TextOnRebar> tor_arc = new List<TextOnRebar>();
            if (rarc.Type == RebarShapeDefinitionByArcType.Arc)
            {
                tor_arc.Add(X_max);
                tor_arc.Add(Y_max);
            }

            if (rarc.Type == RebarShapeDefinitionByArcType.Spiral)
            {
                tor_arc.Add(X_max);
                tor_arc.Add(Y_max_d);
            }

            if (rarc.Type == RebarShapeDefinitionByArcType.LappedCircle)
            {
                tor_arc.Add(X_max);
            }
            
            // показать тексты
            foreach (TextOnRebar tor in tor_arc)
            {
                if (tor.incline == InclineText.Horiz)
                    flagGraphics.DrawString(tor.value_str, SketchReinforcementApp.drawFont, Brushes.Black, tor.positionF);
                else
                    flagGraphics.DrawString(tor.value_str, SketchReinforcementApp.drawFont, Brushes.Black, tor.positionF, drawFormat);
            }
            System.Drawing.Drawing2D.GraphicsPath new_region = new System.Drawing.Drawing2D.GraphicsPath();
            // отображаем текстовые надписи - крюки
            if (dataform.HooksLength)
            {
                // отображаем текстовые надписи - крюки
                foreach (TextOnRebar tor in hooks)
                {
                    if (!tor.arc) continue;       // отработанные элементы (радиусы) пропускаем
                                                  // Длина надписи
                    SizeF sf_arc = flagGraphics.MeasureString(tor.value_str, SketchReinforcementApp.drawFont);
                    sf_arc.Height = dataform.shiftDown - dataform.shiftUp;
                    tor.size = sf_arc;

                    // рассчитаем угол наклона надписи
                    // получить угол наклона надписи в градусах
                    double dAY = (double)(tor.endF.Y - tor.startF.Y);
                    double dAX = (double)(tor.endF.X - tor.startF.X);                    
                    tor.angle = (float)Math.Round(Math.Atan2(dAY, dAX), 3);
                    PointF point;
                    // выполняем наклонные надписи
                    if (Math.Abs(tor.angle) > Math.Round(Math.PI / 2, 3) && Math.Abs(tor.angle) <= Math.Round(Math.PI, 3))
                    {
                        if (tor.angle < 0) tor.angle = (float)(Math.PI + tor.angle);
                        else tor.angle = (float)(tor.angle - Math.Round(Math.PI, 3));
                    }
                    
                    if (GetInclineVariantPositionText(flagGraphics, gp, line2D, tor.positionF, sf_arc, out point, tor.angle, dataform.shiftUp, dataform.Font_shift, dataform.shiftLeft, out new_region))
                    {
                        tor.positionF = point;                             // изменим на новое положение
                        flagGraphics.RotateTransform(tor.angle_grad);
                        flagGraphics.DrawString(tor.value_str, SketchReinforcementApp.drawFont, Brushes.Black, tor.positionF_rotate);
                        flagGraphics.RotateTransform(-tor.angle_grad);
                        // добавим новую занятую зону   
                        gp.Add(new_region);
                        foreach (TextOnRebar toa in hooks)
                        {
                            if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные крюки
                        }
                    }

                    //if (GeHookVariantPositionText(flagGraphics, gp, line2D, tor.positionF, sf_arc, out tor.positionF, sizeX, sizeY, new PointF((tor.endF.X - tor.startF.X), (tor.endF.Y - tor.startF.Y)), tor.distF, (float)(canva + moveX), (float)(canva + moveY), out new_region))
                    //{

                    //    flagGraphics.DrawString(tor.value_str, SketchReinforcementApp.drawFont, Brushes.Black, tor.positionF);
                    //    // добавим новую занятую зону           
                    //    gp.Add(new_region);
                    //    foreach (TextOnRebar toa in hooks)
                    //    {
                    //        if (toa.value_str == tor.value_str) toa.arc = false;  // все подобные крюки
                    //    }
                    //}
                }
            }
            flagGraphics.DrawLines(pen, pointDF_arc.ToArray());
            // flag.Save(@path_name + "\\" + rebar.Id.IntegerValue.ToString() + ".png");
            flag.Save(image);
            flagGraphics.Dispose();
            flag.Dispose();
            return true;

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

            LegGuid guid= new LegGuid();
           
            switch (form)
            { 
                
                case "10":
                    // минус два диаметра - для получения внутреннего размера хомута
                    return -2 * diam;                   
                case "26":
                    // минус два диаметра - для получения внутреннего размера хомута
                    if(parameter.Equals(guid.A)) return -2 * diam;
                    if(parameter.Equals(guid.D)) return -2 * diam;
                    break;
                default:
                    return 0;
            }
            return 0;
        }

        /// <summary>
        /// Получить область для текста
        /// </summary>
        /// <param name="point">Точка вставки</param>
        /// <param name="size">Размер</param>
        /// <param name="it">Тип</param>
        /// <param name="it">angle</param>
        /// <returns>Область текста</returns> 
        static System.Drawing.Drawing2D.GraphicsPath GetPathRegionText(PointF point, SizeF size, InclineText it, out List<Line2D> lines, double angle=0)
        {
            lines = new List<Line2D>();
            float move = 0;
            
            PointF p1,p2, p3, p4;          

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.StartFigure();

            switch (it)
            {
                case InclineText.Horiz:
                    p1 = new PointF(point.X + move, point.Y + move);
                    p2 = new PointF(point.X + size.Width - move, point.Y + move);
                    p3 = new PointF(point.X + size.Width - move, point.Y + size.Height - 2*move);
                    p4 = new PointF(point.X+ move, point.Y + size.Height- 2*move); 
                    break;
                case InclineText.Vertic:
                    p1 = new PointF(point.X + 2* move, point.Y + move);
                    p2 = new PointF(point.X + size.Height - move, point.Y+move);
                    p3 = new PointF(point.X + size.Height - move , point.Y + size.Width - move);
                    p4 = new PointF(point.X + 2 * move, point.Y + size.Width - move);                    
                    break;
                default:
                    // перенесем точку p1                   
                    float moveX = (float) (move*Math.Cos(angle) - move*Math.Sin(angle));
                    float moveY = (float) (move*Math.Sin(angle) + move*Math.Cos(angle));

                    p1 = new PointF( (float) (point.X + moveX), (float) (point.Y + moveY));
                    p2 = new PointF( (float) ((size.Width - 2* move) * Math.Cos(angle) + p1.X), 
                                     (float) ((size.Width - 2* move) * Math.Sin(angle) + p1.Y));
                    p3 = new PointF((float)((size.Width - 2* move) * Math.Cos(angle) - (size.Height - 3*move) *Math.Sin(angle) + p1.X),
                                    (float)((size.Width - 2* move) * Math.Sin(angle) + (size.Height - 3* move) *Math.Cos(angle) + p1.Y));
                    p4 = new PointF((float)(-(size.Height - 3 *move) * Math.Sin(angle) + p1.X),
                                    (float)( (size.Height - 3 *move) * Math.Cos(angle) + p1.Y));

                     
                    break;
            }
            gp.AddLine(p1,p2);
            lines.Add(new Line2D(p1, p2));
            gp.AddLine(p2,p3);
            lines.Add(new Line2D(p2, p3));
            gp.AddLine(p3,p4);
            lines.Add(new Line2D(p3, p4));
            gp.CloseFigure();
            lines.Add(new Line2D(p4, p1));

            return gp;
        }

        /// <summary>
        /// Получить область для текста
        /// </summary>
        /// <param name="point">Точка вставки</param>
        /// <param name="size">Размер</param>
        /// <param name="it">Тип</param>
        /// <param name="it">angle</param>
        /// <returns>Область текста</returns> 
        static System.Drawing.Drawing2D.GraphicsPath GetPathRegionText(PointF point, SizeF size, InclineText it, double angle = 0)
        {
           
            float move = 0;

            PointF p1, p2, p3, p4;

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.StartFigure();

            switch (it)
            {
                case InclineText.Horiz:
                    p1 = new PointF(point.X + move, point.Y + move);
                    p2 = new PointF(point.X + size.Width - move, point.Y + move);
                    p3 = new PointF(point.X + size.Width - move, point.Y + size.Height - 2 * move);
                    p4 = new PointF(point.X + move, point.Y + size.Height - 2 * move);
                    break;
                case InclineText.Vertic:
                    p1 = new PointF(point.X + 2 * move, point.Y + move);
                    p2 = new PointF(point.X + size.Height - move, point.Y + move);
                    p3 = new PointF(point.X + size.Height - move, point.Y + size.Width - move);
                    p4 = new PointF(point.X + 2 * move, point.Y + size.Width - move);
                    break;
                default:
                    // перенесем точку p1                   
                    float moveX = (float)(move * Math.Cos(angle) - move * Math.Sin(angle));
                    float moveY = (float)(move * Math.Sin(angle) + move * Math.Cos(angle));

                    p1 = new PointF( (float) (point.X + moveX), (float) (point.Y + moveY));
                    p2 = new PointF( (float) ((size.Width - 2* move) * Math.Cos(angle) + p1.X), 
                                     (float) ((size.Width - 2* move) * Math.Sin(angle) + p1.Y));
                    p3 = new PointF((float)((size.Width - 2* move) * Math.Cos(angle) - (size.Height - 3*move) *Math.Sin(angle) + p1.X),
                                    (float)((size.Width - 2* move) * Math.Sin(angle) + (size.Height - 3* move) *Math.Cos(angle) + p1.Y));
                    p4 = new PointF((float)(-(size.Height - 3 *move) * Math.Sin(angle) + p1.X),
                                    (float)( (size.Height - 3 *move) * Math.Cos(angle) + p1.Y));


                    break;
            }
            gp.AddLine(p1, p2);
        
            gp.AddLine(p2, p3);
            
            gp.AddLine(p3, p4);
            
            gp.CloseFigure();
          

            return gp;
        }
        /// <summary>
        /// Найти варианты положений текстовой надписи (радиус)
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="angle">Угол наклона надписи</param>  
        /// <param name="count">Число элементов образмеривания</param>  
        /// <returns>Найдено свободное место или нет</returns>  
        static bool GetRadiusVariantPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float angle, int count, int sign, float max_sizeX, float max_sizeY, float canvaX, float canvaY, out bool is_move_position, float shiftUp, float shiftLeft, out System.Drawing.Drawing2D.GraphicsPath new_region )
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            is_move_position = true;
            // Size new_size = new Size((int)(size.Width * Math.Cos(angle) + size.Width * Math.Sin(Math.Abs(angle))), (int)(size.Width * Math.Sin(Math.Abs(angle)) + size.Height * Math.Cos(angle)));
            //// сдвиг на половину высоты надписи
            
            current_p = point;
            PointF rect_text;      // прямоугольник описывающий текст
            rect_text = new PointF((float)(current_p.X - shiftUp * (float)Math.Sin(angle)), (float)(current_p.Y + shiftUp * Math.Cos(angle)));
            // graphic.DrawEllipse(Pens.Green, rect_text.X, rect_text.Y, 2, 2);
            rect_text = new PointF((float)(rect_text.X + shiftLeft * Math.Cos(angle)), (float)(rect_text.Y + shiftLeft * Math.Sin(angle)));
            SizeF rect_size = new SizeF(size);
            rect_size.Width = rect_size.Width - 2 * shiftLeft;

            // graphic.DrawEllipse(Pens.Red, rect_text.X, rect_text.Y, 5, 5);

            //current_p = new PointF( (float) (point.X + size.Height / 2 * Math.Sin(angle)) , (float) (point.Y - size.Height / 2 * Math.Cos(angle)));
            //graphic.DrawEllipse(Pens.DarkSeaGreen, current_p.X, current_p.Y, 2, 2);

            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect_size, InclineText.Radius, out new_region, angle)) { is_move_position = false; return true; }
            //{
            //    PointF new_point;
            //    if (angle >= 0)
            //    {
            //        new_point = current_p;
            //    }
            //    else
            //    {
            //        new_point = new PointF(current_p.X, (float)(current_p.Y + size.Width * Math.Sin(angle)));
            //    }
            //    if (IsTextInsidePicture(new_point, new_size, canvaX, canvaY, max_sizeX, max_sizeY)) { is_move_position = false; return true; }
            //}

            is_move_position = true;

            for (int i = 0; i < 4; i++)
            {
                // при наличии нескольких элементов попробуем найти место для следующего
                // if (count > 1) return false;
                // сдвинем по линии надписи на 1/8 ширины
                current_p = new PointF((float)(current_p.X + size.Width / 8 * Math.Cos(angle)), (float)(current_p.Y +  size.Width / 8 * Math.Sin(angle)));
                rect_text = new PointF((float)(rect_text.X + size.Width / 8 * Math.Cos(angle)), (float)(rect_text.Y +  size.Width / 8 * Math.Sin(angle)));

                // rect_text = new PointF((float)(current_p.X - shiftUp * (float)Math.Sin(angle)), (float)(current_p.Y + shiftUp * Math.Cos(angle)));
                // graphic.DrawEllipse(Pens.Green, rect_text.X, rect_text.Y, 2, 2);
                // rect_text = new PointF((float)(rect_text.X + shiftLeft * Math.Cos(angle)), (float)(rect_text.Y + shiftLeft * Math.Sin(angle)));
                // graphic.DrawEllipse(Pens.Blue, current_p.X, current_p.Y, 2, 2);

                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect_size, InclineText.Radius, out new_region, angle)) return true;
                //{
                //    if (IsTextInsidePicture(rect_text, rect_size, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;                    
                //}
            }
            //{
            //    PointF new_point;
            //    if (angle >= 0)
            //    {
            //        new_point = current_p;
            //    }
            //    else
            //    {
            //        new_point = new PointF(current_p.X, (float)(current_p.Y + size.Width * Math.Sin(angle)));
            //    }
            //    if (IsTextInsidePicture(new_point, new_size, canvaX, canvaY, max_sizeX, max_sizeY)) return true; 
            //}
            //// сдвинем по линии надписи на 0.25 ширины
            //current_p = new PointF((float)(current_p.X + sign * size.Width / 4 * Math.Cos(angle)), (float)(current_p.Y + sign * size.Width / 4 * Math.Sin(angle)));
            //// graphic.DrawEllipse(Pens.Cyan, current_p.X, current_p.Y, 2, 2);
            
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, out new_region, angle))
            //{
            //    PointF new_point;
            //    if (angle >= 0)
            //    {
            //        new_point = current_p;
            //    }
            //    else
            //    {
            //        new_point= new PointF(current_p.X, (float) (current_p.Y+ size.Width * Math.Sin(angle)));                     
            //    }
            //    if (IsTextInsidePicture(new_point, new_size, canvaX, canvaY, max_sizeX, max_sizeY)) return true; 
            //}   // проверим пересечение с созданными областями

            //// сдвинем по линии надписи на 0.5 ширины
            //current_p = new PointF((float)(current_p.X + sign * size.Width / 2 * Math.Cos(angle)), (float)(current_p.Y + sign * size.Width / 2 * Math.Sin(angle)));
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, out new_region, angle))
            //{
            //    PointF new_point;
            //    if (angle >= 0)
            //    {
            //        new_point = current_p;
            //    }
            //    else
            //    {
            //        new_point = new PointF(current_p.X, (float)(current_p.Y + size.Width * Math.Sin(angle)));
            //    }
            //    if (IsTextInsidePicture(new_point, new_size, canvaX, canvaY, max_sizeX, max_sizeY)) return true; 
            //}
            // проверим пересечение с созданными областями
            //// сдвинем по линии надписи на 5/8 ширины
            //current_p = new PointF((float)(current_p.X - size.Width * 5 / 8 * Math.Cos(angle)), (float)(current_p.Y - size.Width *5 / 8 * Math.Sin(angle)));
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, angle)) return true;       // проверим пересечение с созданными областями
            //// сдвинем по линии надписи на 0.75 ширины
            //current_p = new PointF((float)(current_p.X - size.Width * 3 / 4 * Math.Cos(angle)), (float)(current_p.Y - size.Width * 3 / 4 * Math.Sin(angle)));
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, angle)) return true;       // проверим пересечение с созданными областями
            //// сдвинем по линии надписи на 0.75 ширины
            //current_p = new PointF((float)(current_p.X - size.Width * 7 / 8 * Math.Cos(angle)), (float)(current_p.Y - size.Width * 7 / 8 * Math.Sin(angle)));
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, angle)) return true;       // проверим пересечение с созданными областями
            //// сдвинем по линии надписи на 1 ширины
            //// сдвинем по линии надписи на 1 ширины
            //current_p = new PointF((float)(current_p.X - size.Width * Math.Cos(angle)), (float)(current_p.Y -    size.Width * Math.Sin(angle)));
            //if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Radius, angle)) return true;       // проверим пересечение с созданными областями

            return false;
        }

        /// <summary>
        /// Найти варианты положений наклонной текстовой надписи
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="angle">Угол наклона надписи</param>         
        /// <returns>Найдено свободное место или нет</returns>  
        static bool GetInclineVariantPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float angle, float shiftUp, float shiftFont, float shiftLeft, out System.Drawing.Drawing2D.GraphicsPath new_region)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();

            // сместим надпись в середину

            //if (angle < 0)
            //    current_p = new PointF((float)(point.X - size.Width / 2 * Math.Cos(angle)), (float)(point.Y - size.Width / 2 * Math.Sin(angle)));
            //else
                current_p = new PointF((float)(point.X - size.Width / 2 * Math.Cos(angle)), (float)(point.Y - size.Width / 2 * Math.Sin(angle)));

            // graphic.DrawEllipse(Pens.Red, current_p.X, current_p.Y, 2, 2);
            // реальная точка вставки текста
            // сдвиг вверх к линии из-за размеров шрифта
            PointF rect_text;
            //if(angle>0)
            //rect_text = new PointF((float)(current_p.X - shiftUp * (float)Math.Sin(angle)), (float)(current_p.Y + shiftUp * Math.Cos(angle)));
            //else
            rect_text = new PointF((float)(current_p.X - shiftUp * (float)Math.Sin(angle)), (float)(current_p.Y + shiftUp * Math.Cos(angle)));
            // graphic.DrawEllipse(Pens.Green, rect_text.X, rect_text.Y, 2, 2);
            rect_text = new PointF((float)(rect_text.X + shiftLeft * Math.Cos(angle)), (float)(rect_text.Y + shiftLeft * Math.Sin(angle)));
            SizeF rect_size = new SizeF(size);
            rect_size.Width = rect_size.Width - 2 * shiftLeft;
            // graphic.DrawEllipse(Pens.Brown, rect_text.X, rect_text.Y, 2, 2);


            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect_size, InclineText.Incline, out new_region, angle,false)) return true;
            {
                // пытаемся сместить надпись выше линии сегмента (на другую сторону)
                float total_shift = shiftUp + size.Height + 10 + shiftFont;
                current_p = new PointF((float)(current_p.X + total_shift * (float)Math.Sin(angle)), (float)(current_p.Y - total_shift * Math.Cos(angle)));
                rect_text = new PointF((float)(rect_text.X + total_shift * (float)Math.Sin(angle)), (float)(rect_text.Y - total_shift * Math.Cos(angle)));
                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect_size, InclineText.Incline, out new_region, angle, false)) return true;
            }
            return false;
        }

        /// <summary>
        /// Найти варианты положений вертикальной текстовой надписи
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="current_p">Позиция вставки надписи</param>
        /// <param name="max_size">Максимальный размер рисунка</param>
        /// <param name="move">Смещение надписи по умолчанию</param>
        /// <param name="canva">Начальный сдвиг рисунка</param>
        /// <returns>Найдено свободное место или нет</returns>  
        static bool GetVerticalVariantPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float max_sizeX, float max_sizeY, float move, float canvaX, float canvaY,float shiftLeft, float shiftFont, float shiftUp, out System.Drawing.Drawing2D.GraphicsPath new_region)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            SizeF rect = new SizeF(size);
            rect.Width = rect.Width - 2 * shiftUp;
            // вариант 0: сдвиг на ширину надписи
            // current_p = new PointF(point.X, point.Y - size.Width / 2);
            current_p = new PointF(point.X - shiftLeft + 7 + shiftFont, point.Y - size.Width / 2);
            // точка верхнего угла прямоугольной зоны реального текста
            PointF rect_text = new PointF(point.X + 7, point.Y - size.Width / 2 + shiftUp);
            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic,out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;

            }

            // вариант 1: сдвиг влево по оси Х
            current_p = new PointF(point.X - shiftLeft - size.Height - 7 - shiftFont, point.Y - size.Width / 2);
            rect_text = new PointF(point.X - size.Height - 6, point.Y - size.Width / 2 + shiftUp);
                            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic,out new_region))         // проверим пересечение с созданными областями
                            {
                                if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;
                            }
            // правее линии - варианты выше
            
                            for (int i = 1; i <= 8; i++)
                            {
                                current_p = new PointF(point.X - shiftLeft + 7 + shiftFont, point.Y - size.Width / 2 - size.Width * i * 1 / 8);
                                rect_text = new PointF(point.X + 7, point.Y - size.Width / 2 - size.Width * i * 1 / 8 + shiftUp);
                                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic, out new_region))         // проверим пересечение с созданными областями
                                {
                                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;
                                }
                            }
            // правее линии - варианты ниже
                            for (int i = 1; i <= 8; i++)
                            {
                                current_p = new PointF(point.X - shiftLeft + 7 + shiftFont, point.Y - size.Width / 2  + size.Width * i * 1 / 8);
                                rect_text = new PointF(point.X + 7, point.Y - size.Width / 2 + size.Width * i * 1 / 8 + shiftUp);
                                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic, out new_region))         // проверим пересечение с созданными областями
                                {
                                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;
                                }
                            }
            // левее линии - варианты выше
                            for (int i = 1; i <= 8; i++)
                            {
                                current_p = new PointF(point.X - shiftLeft - size.Height - 7 - shiftFont, point.Y - size.Width / 2 - size.Width * i * 1 / 8);
                                rect_text = new PointF(point.X - size.Height - 6, point.Y - size.Width / 2 - size.Width * i * 1 / 8 + shiftUp);
                                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic, out new_region))         // проверим пересечение с созданными областями
                                {
                                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;
                                }
                            }
            // левее линии - варианты ниже
                            for (int i = 1; i <= 8; i++)
                            {
                                current_p = new PointF(point.X - shiftLeft - size.Height - 7 - shiftFont, point.Y - size.Width / 2 + size.Width * i * 1 / 8);
                                rect_text = new PointF(point.X - size.Height - 6, point.Y - size.Width / 2 + size.Width * i * 1 / 8 + shiftUp);
                                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Vertic, out new_region))         // проверим пересечение с созданными областями
                                {
                                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY, InclineText.Vertic)) return true;
                                }
                            }

                                                    
                        
                            return false;
        }


        /// <summary>
        /// Найти варианты положений горизонтальной текстовой надписи
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="current_p">Позиция вставки надписи</param>
        /// <param name="max_size">Максимальный размер рисунка</param>
        /// <param name="move">Смещение надписи по умолчанию</param>
        /// <param name="canvaX">Начальный сдвиг рисунка</param>
        /// <param name="canvaY">Начальный сдвиг рисунка</param>
        /// <param name="shiftUp">Смещение надписи для выбранного шрифта</param>
        /// <returns>Найдено свободное место или нет</returns> 
        static bool GeHorizontalVariantPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float max_sizeX, float max_sizeY, float move, float canvaX, float canvaY, float shiftUp, float shiftFont, float shiftLeft, out System.Drawing.Drawing2D.GraphicsPath new_region)

        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            SizeF rect = new SizeF(size);
            rect.Width = rect.Width - 2 * shiftLeft;
            // оптимальный вариант в середине отрезка
            // вариант 0: по середине
            current_p = new PointF(point.X - size.Width / 2, point.Y - shiftUp + 5 + shiftFont); // физическая точка вставки текста - выше из-за размеров шрифта и вставки текста           
            // точка верхнего угла прямоугольной зоны реального текста
            PointF rect_text = new PointF(point.X - size.Width / 2 + shiftLeft, point.Y + 5);
            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz,out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY))
                {
                    // добавим зону, занятую данным тестом

                    return true;
                }
            }
            
            // вариант 1: по середине, но выше на размер шрифта
            current_p = new PointF(point.X - size.Width / 2, point.Y - shiftUp - size.Height - 8 - shiftFont);
            rect_text = new PointF(point.X - size.Width / 2 + shiftLeft, point.Y - size.Height - 8);
            if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz,out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }

           
            // уходим влево по Х - еще на размер ширины надписи - ниже линии
            for (int i = 1; i <= 8; i++)
            {
                current_p = new PointF(point.X - size.Width / 2 - size.Width * i * 1 / 8  , point.Y - shiftUp + 5 + shiftFont);
                // точка верхнего угла прямоугольной зоны реального текста
                rect_text = new PointF(point.X - size.Width / 2 - size.Width * i * 1 / 8 + shiftLeft, point.Y + 5);
                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz,out new_region))         // проверим пересечение с созданными областями
                {
                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
                }
            }            


            // уходим вправо  по Х - еще на размер ширины надписи - ниже линии
            for (int i = 1; i <= 8; i++)
            {
                current_p = new PointF(point.X - size.Width / 2 + size.Width * i * 1 / 8, point.Y - shiftUp + 5 + shiftFont);
                // точка верхнего угла прямоугольной зоны реального текста
                rect_text = new PointF(point.X - size.Width / 2 + size.Width * i * 1 / 8 + shiftLeft, point.Y + 5);
                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
                {
                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
                }
            }

            // уходим влево по Х - еще на размер ширины надписи - выше линии
            for (int i = 1; i <= 8; i++)
            {
                current_p = new PointF(point.X - size.Width / 2 - size.Width * i * 1 / 8, point.Y - shiftUp - size.Height - 8 - shiftFont);
                // точка верхнего угла прямоугольной зоны реального текста
                rect_text = new PointF(point.X - size.Width / 2 - size.Width * i * 1 / 8 + shiftLeft, point.Y - size.Height - 8);
                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
                {
                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
                }
            }

            // уходим вправо  по Х - еще на размер ширины надписи - выше линии
            for (int i = 1; i <= 8; i++)
            {
                current_p = new PointF(point.X - size.Width / 2 + size.Width * i * 1 / 8, point.Y - shiftUp - size.Height - 8 - shiftFont);
                // точка верхнего угла прямоугольной зоны реального текста
                rect_text = new PointF(point.X - size.Width / 2 + size.Width * i * 1 / 8 + shiftLeft, point.Y - size.Height - 8);
                if (!IsIntersectWithLines(graphic, regions, lines, rect_text, rect, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
                {
                    if (IsTextInsidePicture(rect_text, rect, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Находится ли текст внутри картинки
        /// </summary>

        static bool IsTextInsidePicture(PointF point, SizeF size, float canvaX, float canvaY, float max_sizeX, float max_sizeY, InclineText incline=InclineText.Horiz)
        {
           
            float w = size.Width;
            float h = size.Height;
            if (incline == InclineText.Vertic)
            {
                w = size.Height;
                h = size.Width;
            }
            if ((point.X + w + canvaX) > max_sizeX) return false ;                                // пересечения нет, но вышли за пределы рисунка
            if ((point.X + canvaX) < 0) return false; // точка X выходит за начало чертежа
            if ((point.Y + canvaY) < 0) return false; // точка Y выходит за начало чертежа
            if ((point.Y + canvaY + h) > max_sizeY) return false;
            return true;
        }


        /// <summary>
        /// Найти варианты положений текстовой надписи для крюков
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="current_p">Позиция вставки надписи</param>
        /// <param name="max_size">Максимальный размер рисунка</param>
        /// <param name="dir">Направление крюка</param>
        /// <param name="canva">Начальный сдвиг рисунка</param>
        /// <returns>Найдено свободное место или нет</returns> 
        static bool GeHookVariantPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float max_sizeX, float max_sizeY, PointF dir, float distF, float canvaX, float canvaY, out System.Drawing.Drawing2D.GraphicsPath new_region)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            // оптимальный вариант в точке вставки надписи - нижнее положение     
            current_p = new PointF(point.X + 1, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))                  // проверим пересечение с созданными областями
            {
                if(IsTextInsidePicture(current_p,size,canvaX,canvaY,max_sizeX,max_sizeY)) return true;                
            }        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width / 4, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }         
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width / 2, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width * 3 / 4, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
         
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
         
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height/2);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region ))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
         
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height *3 / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width / 4, point.Y - size.Height / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width/2, point.Y - size.Height/2);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width * 3 / 4, point.Y - size.Height * 3/ 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
       
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width, point.Y - size.Height);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        

            point = new PointF(point.X + dir.X, point.Y + dir.Y);                                          // перенос на другую сторону крюка


            // оптимальный вариант в точке вставки надписи - нижнее положение     
            current_p = new PointF(point.X + 1, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width / 4, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width/2, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width *3 / 4, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - нижнее положение слева    
            current_p = new PointF(point.X - size.Width, point.Y + 1);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height/2);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
        
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height * 3 / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
       
            // оптимальный вариант в точке вставки надписи - верхнее положение справа    
            current_p = new PointF(point.X + 1, point.Y - size.Height);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
         
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width / 4, point.Y - size.Height / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
       
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width/2, point.Y - size.Height/2);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
       
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width * 3 / 4, point.Y - size.Height * 3 / 4);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }
         
            // оптимальный вариант в точке вставки надписи - верхнее положение слева  
            current_p = new PointF(point.X - size.Width, point.Y - size.Height);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region))         // проверим пересечение с созданными областями
            {
                if (IsTextInsidePicture(current_p, size, canvaX, canvaY, max_sizeX, max_sizeY)) return true;
            }

            return false;
        }


        /// <summary>
        /// Найти варианты положений горизонтальной текстовой надписи (проекция)
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="current_p">Позиция вставки надписи</param>
        /// <param name="max_size">Максимальный размер рисунка</param>
        /// <returns>Найдено свободное место или нет</returns> 
        static bool GeTHorizontalProjectPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float max_size, out System.Drawing.Drawing2D.GraphicsPath new_region)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            // сдвиг на ширину надписи; устaнавливаем внизу чертежа
            current_p = new PointF(point.X - size.Width / 2, max_size);            
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Horiz, out new_region)) return true;      // проверим пересечение с созданными областями
            // return true;
            return false;
        }

        /// <summary>
        /// Найти варианты положений вертикальной текстовой надписи (проекция)
        /// </summary>
        /// <param name="graphic">Графический объект</param>
        /// <param name="regions">Зоны занятые текстовыми надписями</param>
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Начальная точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="current_p">Позиция вставки надписи</param>
        /// <param name="max_size">Максимальный размер рисунка</param>
        /// <returns>Найдено свободное место или нет</returns> 
        static bool GeTVerticalProjectPositionText(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, out PointF current_p, float max_size, float canva, out System.Drawing.Drawing2D.GraphicsPath new_region)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();
            // сдвиг на ширину надписи; устaнавливаем слева или справа чертежа
            float X = -canva;    // пытаемся установить в крайнем положении слева            
            if (point.X > (max_size / 2 - canva)) X = max_size - canva - size.Height; // или справа             
            current_p = new PointF(X, point.Y - size.Width / 2);
            if (!IsIntersectWithLines(graphic, regions, lines, current_p, size, InclineText.Vertic, out new_region)) return true;      // проверим пересечение с созданными областями
            
            // поиск новых позиций не выполняем
            return false;
        }

        /// <summary>
        /// Получить зоны для линий чертежа
        /// </summary>
        /// <param name="points">Точки линий чертежа</param>
        /// <returns>Список зон</returns> 
        static List<System.Drawing.Drawing2D.GraphicsPath> GetGraphicsPath(List<PointF> points)
        {
            List<System.Drawing.Drawing2D.GraphicsPath> regions = new List<System.Drawing.Drawing2D.GraphicsPath>();
            // зоны получаем по линиям чертежа с условным смещением от линии на 1
            for (int i = 0; i < points.Count - 1; i++)
            {                 
                System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.StartFigure();
                gp.AddLine(points[i], points[i + 1]);
                gp.AddLine(points[i + 1], new PointF(points[i + 1].X + 1, points[i + 1].Y + 1));
                gp.AddLine(new PointF(points[i + 1].X + 1, points[i + 1].Y + 1), new PointF(points[i].X + 1, points[i].Y + 1));
                gp.CloseFigure();
                regions.Add(gp);
                i++;              // точку i+1 пропускаем. Берем по 2 точки на отрезок
            }

            return regions;
        }


        /// <summary>
        /// Проверить пересечение надписи и линий чертежа
        /// </summary>
        /// <param name="graphic">Чертеж</param>
        /// <param name="regions">Зоны занятые другими надписями</param>         
        /// <param name="lines">Линии чертежа</param>
        /// <param name="point">Точка вставки надписи</param>
        /// <param name="size">Размер надписи</param>
        /// <param name="it">Тип положения надписи</param>       
        /// <returns>Да, если пересечение обнаружено</returns> 
        static bool IsIntersectWithLines(Graphics graphic, List<System.Drawing.Drawing2D.GraphicsPath> regions, List<Line2D> lines, PointF point, SizeF size, InclineText it, out System.Drawing.Drawing2D.GraphicsPath new_region, double angle=0, bool show_lines = false)
        {
            new_region = new System.Drawing.Drawing2D.GraphicsPath();  // зона обрамления текста
            List<Line2D> lines_bound = new List<Line2D>();   // линии вокруг текствой надписи
            Region current_region = new Region();
           
            switch (it)
            {
                case InclineText.Horiz:                   
                    new_region = GetPathRegionText(point, size, InclineText.Horiz, out lines_bound);
                    current_region = new Region(new_region);

                    lines_bound = GetLinesAroundText(current_region.GetBounds(graphic));
                    if (show_lines)
                    {
                        foreach (Line2D line in lines_bound)
                        {
                            graphic.DrawLine(Pens.Red, line.p1F, line.p2F);
                        }
                    }
                    break;
                case InclineText.Vertic:
                    new_region = GetPathRegionText(point, size, InclineText.Vertic, out lines_bound);
                    current_region = new Region(new_region);
                    lines_bound = GetLinesAroundText(current_region.GetBounds(graphic));
                    if (show_lines)
                    {
                        foreach (Line2D line in lines_bound)
                        {
                            graphic.DrawLine(Pens.Gold, line.p1F, line.p2F);
                        }
                    }
                    break;
                default:
                    new_region = GetPathRegionText(point, size, InclineText.Incline, out lines_bound, angle);
                    current_region = new Region(new_region);
                    //graphic.DrawPath(Pens.Indigo,GetPathRegionText(point, size, InclineText.Incline, out lines_bound, angle));
                    //graphic.DrawEllipse(Pens.Blue, point.X, point.Y, 2, 2);
                    //// Size new_size = new Size((int)(size.Width * Math.Cos(angle)), (int)(size.Width * Math.Sin(angle) + size.Height * Math.Cos(angle)));
                    //Size new_size = new Size((int)(size.Width * Math.Cos(angle)), (int)(size.Width * Math.Cos(angle)));
                    //if (!IsTextInsidePicture(point, new_size, 397, 73, 1000, 300)) return true;

                    //lines_bound = GetLinesAroundText(point, size, angle);

                    if (show_lines)
                    {
                        foreach (Line2D line in lines_bound)
                        {
                            graphic.DrawLine(Pens.Cyan, line.p1F, line.p2F);
                        }
                    }

                    //Size new_size = new Size((int)(size.Width * Math.Cos(angle)), (int)(size.Width * Math.Sin(angle) + size.Height * Math.Cos(angle)));
                    // if (!IsTextInsidePicture(point, new_size, 397, 73, 1000, 300)) return true;
                    //// рисуем точки
                    //graphic.DrawEllipse(Pens.DarkBlue, lines_bound[0].p1F.X,lines_bound[0].p1F.Y,2,2);
                    //graphic.DrawEllipse(Pens.Red, lines_bound[0].p2F.X, lines_bound[0].p2F.Y, 2, 2);
                    //graphic.DrawEllipse(Pens.DarkGoldenrod, lines_bound[2].p1F.X,lines_bound[2].p1F.Y,2,2);
                    //graphic.DrawEllipse(Pens.DarkGreen, lines_bound[2].p2F.X, lines_bound[2].p2F.Y, 2, 2);

                    break;
            }


            // проверим пересечение надписи с линиями чертежа  
            // return false;
            if (IsCrossTextLines(graphic,lines, lines_bound)) return true;
            // if (it != InclineText.Radius) { if (IsCrossTextLines(lines, lines_bound)) return true; }
            // if (it == InclineText.Radius && count>1) { if (IsCrossTextLines(lines, lines_bound)) return true; }

            // проверим перечение с другими текстовыми надписями
            foreach (System.Drawing.Drawing2D.GraphicsPath gp in regions)
            {

                Region region = current_region.Clone();           

                //switch (it)
                //{
                //    case InclineText.Horiz:
                //        region = new Region(current_region.GetBounds(graphic));
                       
                //        break;
                //    case InclineText.Vertic:
                //        region = new Region(current_region.GetBounds(graphic));
                //        break;
                //    default:
                //        region = new Region(GetPathRegionText(point, size, InclineText.Incline, angle));
                        
                //        break;
                //}


                region.Intersect(gp);

                if (!region.IsEmpty(graphic))
                {
                    //if (show_lines)
                    //{
                    //    foreach (Line2D line in lines_bound)
                    //    {
                    //        graphic.DrawLine(Pens.Red, line.p1F, line.p2F);
                    //    }
                    //    graphic.DrawPath(Pens.DarkOrange, gp);
                    //    graphic.FillRegion(Brushes.Aqua, region);

                    //}

                    RectangleF rf = region.GetBounds(graphic);
                    if (rf.Height * rf.Width < 25) continue;   // пересечение незначительно


                    //graphic.DrawPath(Pens.Aquamarine, GetPathRegionText(point, size, InclineText.Incline, angle));

                    return true;
                }                  // есть пересечение областей
            }

            return false;                                                   // пересечение областей не обнаружено
        }

        /// <summary>
        /// Проверка пересечения контура текста с линиями чертежа
        /// </summary>
        /// <param name="line2D">Линии чертежа</param>
        /// <param name="line2D_text">Линии обрамляющие надпись</param>
        /// <returns>Да, если пересечение обнаружено</returns> 
        static bool IsCrossTextLines(Graphics graphic,List<Line2D> line2D, List<Line2D> line2D_text)
        {
             

                foreach (Line2D line in line2D)
                {
                    
                    // проверим пересечение отрезков
                    foreach (Line2D line_text in line2D_text)
                    {
                        if (GetIntersection(line, line_text)) return true;  // обнаружено пересечение линий с тестом
                        // graphic.DrawLine(Pens.Yellow, line_text.p1F, line_text.p2F);
                    }

                    bool p1 = IsPointInsideContour(line.p1F, line2D_text); // находится ли точка внутри контура текста
                    bool p2 = IsPointInsideContour(line.p2F, line2D_text);
                    if (p1 && p2) { return true; }  // линия оказалась внутри контура
                    if (!p1 && p2) { return true; } // если одна из точек вне контура, а другая внутри контура - есть пересечение
                    if (p1 && !p2) { return true; }                                         
                }
      
            return false;
        }


        /// <summary>
        /// Получить линии обрамляющие надпись
        /// </summary>
        /// <param name="point">Точка вставки надписи</param>
        /// <param name="sf">Размеры надписи</param>
        /// <returns>Массив линий</returns> 
        static List<Line2D> GetLinesAroundText(RectangleF rectangle)
        {
            float move = 0;
            PointF p1=new PointF(rectangle.Location.X + move,rectangle.Location.Y+move);
            PointF p2 = new PointF(rectangle.Location.X + rectangle.Width - move, rectangle.Location.Y + move);
            PointF p3 = new PointF(rectangle.Location.X + rectangle.Width - move, rectangle.Location.Y + rectangle.Height - 2*move);
            PointF p4 = new PointF(rectangle.Location.X + move, rectangle.Location.Y + rectangle.Height - 2* move);

            List<Line2D> lines = new List<Line2D>();             // линии обрамляющие надпись             
            lines.Add(new Line2D(p1, p2));
            lines.Add(new Line2D(p2,p3));
            lines.Add(new Line2D(p3,p4));
            lines.Add(new Line2D(p4,p1));

            return lines;
        }

        /// <summary>
        /// Получить линии обрамляющие надпись
        /// </summary>
        /// <param name="point">Точка вставки надписи</param>
        /// <param name="sf">Размеры надписи</param>
        /// <param name="angle">Угол наклона надписи</param>
        /// <returns>Массив линий</returns> 
        static List<Line2D> GetLinesAroundText(PointF point, SizeF size, double angle)
        {
            // float move = 20;
            List<Line2D> lines = new List<Line2D>();             // линии обрамляющие надпись             
            PointF p1 = point;
            PointF p2, p3, p4;
            // поворот относительно точки 1 и смещение
            p2 = new PointF((float)((size.Width) * Math.Cos(angle)) + p1.X,
                                     (float)((size.Width) * Math.Sin(angle)) + p1.Y);
            p3 = new PointF((float)(size.Width * Math.Cos(angle) - size.Height * Math.Sin(angle) + p1.X),
                            (float)(size.Width * Math.Sin(angle) + size.Height * Math.Cos(angle) + p1.Y));
            p4 = new PointF((float)(-size.Height * Math.Sin(angle) + p1.X),
                            (float)(size.Height * Math.Cos(angle) + p1.Y));
            lines.Add(new Line2D(p1, p2));
            lines.Add(new Line2D(p2, p3));
            lines.Add(new Line2D(p3, p4));
            lines.Add(new Line2D(p4, p1));
            return lines;
        }


        /// <summary>
        /// Проверка нахождения точки внутри контура 
        /// </summary>
        /// <param name="point">Координаты точки</param>
        /// <param name="lines">Линии контура</param>
        /// <returns>Да, если точка внутри или на границе контура</returns>
        public static bool IsPointInsideContour(PointF point, List<Line2D> lines)
        {
            // проверим совпадение точки с вершинами контура
            foreach (Line2D line in lines)
            {
                if (point.X.CompareTo(line.p1F.X) == 0 && point.Y.CompareTo(line.p1F.Y) == 0) return true;
                if (point.X.CompareTo(line.p2F.X) == 0 && point.Y.CompareTo(line.p2F.Y) == 0) return true;
            }
            // создаем проивольный вертикальный луч из исходной точки
            Line2D ray = new Line2D(new PointF(point.X, 1E5f), new PointF(point.X, point.Y));
            int intersection = 0;
            foreach (Line2D line in lines)
            {
                if (GetIntersection(line, ray)) intersection++;  // обнаружено пересечение линий с тестом
            }
            return intersection % 2 == 1;
        }

        /// <summary>
        /// Получить пересечение двух плоских линий
        /// </summary>
        /// <param name="line1">Линия 1</param>
        /// <param name="line1">Линия 2</param>
        /// <returns>Да, если пересечение обнаружено</returns> 
        public static bool GetIntersection(Line2D line1D, Line2D line2D)
        {
            Line line1 = line1D.line;
            Line line2 = line2D.line;
            if (line1 == null || line2 == null) return false;    // линия слишком короткая - пересечения нет
            IntersectionResultArray results;
            Autodesk.Revit.DB.SetComparisonResult result = line1.Intersect(line2, out results);

            if (result != Autodesk.Revit.DB.SetComparisonResult.Overlap) return false;    // throw new InvalidOperationException("Input lines did not intersect.");

            if (results == null || results.Size != 1) return false;                       //  throw new InvalidOperationException("Could not extract intersection point for lines.");

                                                                             // обнаружена точка пересечения
            IntersectionResult iResult = results.get_Item(0);
            XYZ intersectionPoint = iResult.XYZPoint;
            // если точка совпадает с вершинами линии 2, то считаем, что пересечения нет
            if (SketchTools.CompareXYZ(intersectionPoint, line2D.p1)) return false;
            // если точка совпадает с вершинами линии 2, то считаем, что пересечения нет
            if (SketchTools.CompareXYZ(intersectionPoint, line2D.p2)) return false;
            
            return true; 
        }

        /// <summary>
        /// Получить параметры для начального крюка
        /// </summary>
        /// <param name="curves">Линии стержня</param>
        /// <param name="rebar">Элемент стержян</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnRebar GetHookStart(IList<Curve> curves, Element rebar,RebarBendData rbd)
        {            
            Curve c_straight = curves[0];
            Curve c_arc = curves[1];
            TextOnRebar tor = new TextOnRebar();
            tor.rebar = rebar;
            // tor.position = c_straight.GetEndPoint(0);
            tor.value = rbd.HookLength0 + rbd.HookBendRadius + rbd.BarModelDiameter;
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
        static TextOnRebar GetHookEnd(IList<Curve> curves, Element rebar, RebarBendData rbd)
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

        /// <summary>
        /// Получить параметры для дугового сегмента
        /// </summary>
        /// <param name="curves">Линия стержня</param>
        /// <param name="rebar">Элемент стержня</param>
        /// <param name="i">Текущий номер линии</param>
        /// <returns>Текстовая надпись</returns> 
        static TextOnArc GetArcSegment(IList<Curve> curves, Element rebar, int i)
        {
            // получить диаметр стержня
            double d = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble();
            TextOnArc toa = new TextOnArc();
            toa.rebar = rebar;
            Arc arc = curves[i] as Arc;
            // запишем координаты, направление сегмента и радиус
            toa.position = arc.Center;                                         // запишем координаты центра дуги 
            toa.start = (arc.GetEndPoint(0) + arc.GetEndPoint(1)) / 2;            // начальная точка сегмента
            toa.end = arc.Center;                                               // конечная точка сегмента    
            toa.value = arc.Radius - d / 2;                                      // запишем радиус дуги (по внутреннему контуру)
            // получить длину примыкающих прямых сегментов
            double l1, l2;
            l1 = l2 = 0;
            if ((i - 1) >= 0) l1 = curves[i - 1].Length;
            if ((i + 1) < curves.Count) l2 = curves[i + 1].Length;
            toa.nearestL = l1 + l2;
            return toa;
        }

        /// <summary>
        /// Получить координаты точек, тип надписи и угол
        /// </summary>
        /// <param name="matrix">Матрица преобразований</param>
        /// <param name="tr">Элемент дуги</param>
        /// <param name="dX">Сдвиг по координате Х</param>
        /// <param name="dY">Сдвиг по координате Y</param>
        /// <returns>Текстовая надпись для арки</returns> 
        static TextOnArc RecalculatePointPosition(Matrix4 matrix, TextOnArc tr, float dX, float dY)
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
        static TextOnRebar RecalculatePointPosition(Matrix4 matrix, TextOnRebar tr, float dX, float dY, bool hook=false)
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

    }
      
}


     



    
