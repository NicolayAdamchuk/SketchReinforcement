using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
 
using Autodesk.Revit.UI;

namespace SketchReinforcement
{

    

    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class SketchReinforcementApp : IExternalApplication
    {
        /// <summary>
        /// Имя шрифта по умолчанию
        /// </summary>
        public static Font drawFont30 = new Font("Mipgost", 30);
        public static Font drawFont = new Font("Mipgost", 48);
        public static Font drawFontH = new Font("Mipgost", 36);
        public static Font drawFontG = new Font("Mipgost", 26);
        public static float shift_font = 0.0f;
        public static float shift_font_arc = 0.0f;
        public static float size_font = 0.0f;
        public static Autodesk.Revit.ApplicationServices.LanguageType lt;
        static string AddInPath = typeof(SketchReinforcementApp).Assembly.Location;
        // Button icons directory
        public static string ButtonIconsFolder = Path.GetDirectoryName(AddInPath);
        

        #region IExternalApplication Members
        /// <summary>
        /// Implements the OnShutdown event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {
            
            lt = application.ControlledApplication.Language;
            if (lt.ToString() == "Russian") Resourses.Strings.Texts.Culture = new System.Globalization.CultureInfo("ru-RU");

            RibbonPanel ribbonPanel = application.CreateRibbonPanel(Resourses.Strings.Texts.NamePanel);
            PushButtonData styleSettingButton = new PushButtonData("DatumStyle", Resourses.Strings.Texts.NameImage, AddInPath, "SketchReinforcement.SketchCommand");
            styleSettingButton.LargeImage = new BitmapImage(new Uri(Path.Combine(ButtonIconsFolder + "\\Resources\\Images\\", "Rebar.png"), UriKind.Absolute)); ;

            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            ContextualHelp ch2 = new ContextualHelp(ContextualHelpType.ChmFile, path + Resourses.Strings.Texts.pathToHelp);
            styleSettingButton.SetContextualHelp(ch2);
            ribbonPanel.AddItem(styleSettingButton);
          
            return Result.Succeeded;
        }

        #endregion
    }
}
