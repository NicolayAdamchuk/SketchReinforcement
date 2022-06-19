using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SketchReinforcement
{
    public partial class SelectRebars : System.Windows.Forms.Form
    {
        DataForm dataform;
        System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
        public SelectRebars(DataForm dataform)
        {
            InitializeComponent();
            this.dataform = dataform;
            this.Text = SketchReinforcement.Resourses.Strings.Texts.NameImage;
            this.BySelect.Text = SketchReinforcement.Resourses.Strings.Texts.RadioSelectSingle;
            this.AllRebars.Text = SketchReinforcement.Resourses.Strings.Texts.RadioSelectAll;
            this.RadiusBending.Text = SketchReinforcement.Resourses.Strings.Texts.RadiusBending;
            this.UpdateManual.Text = SketchReinforcement.Resourses.Strings.Texts.UpdateManual;
            this.HooksLength.Text = SketchReinforcement.Resourses.Strings.Texts.HooksVisible;
            this.ByView.Text = SketchReinforcement.Resourses.Strings.Texts.ByView;
            this.byRazdel.Text = SketchReinforcement.Resourses.Strings.Texts.ByPartition;
            this.DeleteSketch.Text = SketchReinforcement.Resourses.Strings.Texts.DeleteSketch;
            this.ByAxis.Text = SketchReinforcement.Resourses.Strings.Texts.ByAxis;
            this.ShowAngle.Text = SketchReinforcement.Resourses.Strings.Texts.ShowAngle;
            this.LabelFont.Text = SketchReinforcement.Resourses.Strings.Texts.LabelFont;
            this.LabelColor.Text = SketchReinforcement.Resourses.Strings.Texts.Color;
            this.LabelMax.Text = SketchReinforcement.Resourses.Strings.Texts.MaxLength;
            this.LabelDiam.Text = SketchReinforcement.Resourses.Strings.Texts.Overlap;
            this.label_Shift_From_Line.Text= SketchReinforcement.Resourses.Strings.Texts.ShiftFromLine;
            this.checkBoxBackGround.Text = SketchReinforcement.Resourses.Strings.Texts.BackGroundColor;

            this.checkBoxBackGround.Checked = dataform.BackGroundColor;
            
            this.ShowAngle.Checked = dataform.Angle;

            this.numericUpDown1.Value = (decimal) dataform.Font_size;
            this.ShiftFromLine.Value = (decimal)dataform.Font_shift;
            this.numericBorder.Value = (decimal)dataform.border;

            this.HooksLength.Checked = dataform.HooksLength;
            // длина стержня по умолчанию 12000 мм = 39.37008 футов
            this.MaxLenght.Text = Autodesk.Revit.DB.UnitFormatUtils.Format(dataform.units, Autodesk.Revit.DB.SpecTypeId.ReinforcementLength, dataform.Max_Lenght, false);
            this.CoefDiam.Text = dataform.coef_diam.ToString();
            this.ShowDimLines.Checked = dataform.Is_dim_lines;
            this.ShowDimLines.Text = SketchReinforcement.Resourses.Strings.Texts.ShowDimLines;
            this.CheckShape.Checked = dataform.mode_shape;
            this.tabControl1.TabPages[0].Text = SketchReinforcement.Resourses.Strings.Texts.TabControl1;
            this.tabControl1.TabPages[1].Text = SketchReinforcement.Resourses.Strings.Texts.TabControl2;
            this.buttonFolder.Text = SketchReinforcement.Resourses.Strings.Texts.ButtonFolder;
            this.checkFolder.Text = SketchReinforcement.Resourses.Strings.Texts.CheckFolder;
            this.groupFolder.Text = SketchReinforcement.Resourses.Strings.Texts.GroupFolder;
            this.labelBorder.Text = SketchReinforcement.Resourses.Strings.Texts.Border;
            this.checkBoxAllRazdel.Text = SketchReinforcement.Resourses.Strings.Texts.SelectAll;

            if (dataform.Razdels.Count == 0)
            {
                this.byRazdel.Enabled = false;
                this.RazdelCombo.Visible = false;
                this.checkedListRazdel.Visible = false;
                this.checkBoxAllRazdel.Visible = false;
            }

            //this.checkedListRazdel.SetItemChecked(0, true);

            if (CheckShape.Checked)
            {
                CoefDiam.Enabled = true;
                MaxLenght.Enabled = true;
                LabelDiam.Enabled = true;
                LabelMax.Enabled = true;
                Label2.Enabled = true;
            }
            else
            {
                CoefDiam.Enabled = false;
                MaxLenght.Enabled = false;
                LabelDiam.Enabled = false;
                LabelMax.Enabled = false;
                Label2.Enabled = false;
            }

            if(dataform.pathFolder=="")
            {
                checkFolder.Checked = true;
                labelFolder.Visible = false;
                labelFolder.Text = "";
                buttonFolder.Enabled = false;
                dataform.IspathFolder = false;
            }
            else
            {
                checkFolder.Checked = false;
                labelFolder.Visible = true;
                labelFolder.Text = dataform.pathFolder;
                buttonFolder.Enabled = true;
                dataform.IspathFolder = true;
            }

             

#if Rtype
            this.DeleteSketch.Visible = true;
#endif
            //foreach (Autodesk.Revit.DB.TextNoteType tnt in dataform.Fonts)
            //{
            //    comboFonts.Items.Add(tnt.Name);
            //}

            FontFamily[] families = ifc.Families;
             
            int Font_default = -1;            
            for (int i = 0; i < ifc.Families.Length; i++)
            {
                comboFonts.Items.Add(ifc.Families[i].Name);
                 
                if (ifc.Families[i].Name == dataform.Font_default_name)
                {
                    Font_default = i;
                }
            }
                         
            comboColor.Items.Add(ConsoleColor.Black.ToString());
            comboColor.Items.Add(ConsoleColor.Blue.ToString());
            comboColor.Items.Add(ConsoleColor.Green.ToString());
            comboColor.Items.Add(ConsoleColor.Cyan.ToString());
            comboColor.Items.Add(ConsoleColor.Red.ToString());
            comboColor.Items.Add(ConsoleColor.Magenta.ToString());
            comboColor.Items.Add(ConsoleColor.Brown.ToString());
            comboColor.Items.Add(ConsoleColor.Yellow.ToString());
            comboColor.Items.Add(ConsoleColor.White.ToString());

            comboColor.SelectedIndex = dataform.index_color;

            if (Font_default >= 0)
            {
                comboFonts.SelectedIndex = Font_default;
            }
            else
            {
                comboFonts.SelectedIndex = 0;                 
            }         
        


            if (!dataform.EnabledBySelect)
            {
                this.BySelect.Enabled = false;
            }

            if (!dataform.EnabledByView)
            {
                this.ByView.Enabled = false;
            }
            
            //if (dataform.IsRazdel)
            //{
            //    this.byRazdel.Enabled = true;
            //    this.RazdelCombo.Visible = true;
            //    this.RazdelCombo.Enabled = false;

            //    foreach(string s in dataform.Razdels)
            //    {
            //        this.RazdelCombo.Items.Add(s);
            //    }
            //    // this.RazdelCombo.SelectionStart = 1;
            //    this.RazdelCombo.SelectedIndex = 0;
            //    dataform.SelectRazdel = dataform.Razdels[0];
            //}
            //else
            //{
            //    this.byRazdel.Enabled = false;
            //    this.RazdelCombo.Visible = false;
            //}

            if (dataform.IsRazdel)
            {
                foreach (string s in dataform.Razdels)
                {
                    this.checkedListRazdel.Items.Add(s);
                }    
            }
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //if (AllRebars.Checked)
            //{
            //    RadiusBending.Visible = true;
            //    UpdateManual.Visible = true;
            //    HooksLength.Visible = true;
            //}
            //else
            //{
            //    RadiusBending.Visible = false;
            //    UpdateManual.Visible = false;
            //    HooksLength.Visible = false;
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataform.AllRebars = AllRebars.Checked;
            dataform.BendingRadius = RadiusBending.Checked;
            dataform.UpdateSingleRebar = BySelect.Checked;
            dataform.HooksLength = HooksLength.Checked;
            dataform.ByView = ByView.Checked;
            dataform.ByRazdel = byRazdel.Checked;
            dataform.ByAxis = ByAxis.Checked;

            dataform.SelectRazdels.Clear();
            for (int i = 0; i < this.checkedListRazdel.Items.Count; i++)
            {
                if (this.checkedListRazdel.GetItemChecked(i))
                {
                    dataform.SelectRazdels.Add(this.checkedListRazdel.Items[i].ToString());
                }
            }
        }

        private void RadiusBending_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void SelectRebars_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataform.SelectRazdel = RazdelCombo.SelectedItem.ToString();
        }

        private void byRazdel_CheckedChanged(object sender, EventArgs e)
        {            
            if (dataform.Razdels.Count > 0)
            {
                if (byRazdel.Checked)
                {
                    this.checkedListRazdel.Visible = true;
                    this.checkBoxAllRazdel.Enabled = true;
                    this.checkBoxAllRazdel.Visible = true;
                }
                else
                {
                    this.checkedListRazdel.Visible = false;
                    this.checkBoxAllRazdel.Enabled = false;
                    this.checkBoxAllRazdel.Visible = false;
                }
            }
        }

        private void HooksLength_CheckedChanged(object sender, EventArgs e)
        {
            dataform.HooksLength = HooksLength.Checked;
        }

        private void DeleteSketch_CheckedChanged(object sender, EventArgs e)
        {
            dataform.IsDeleteSketch = DeleteSketch.Checked;
        }

        private void ByAxis_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ShowAngle_CheckedChanged(object sender, EventArgs e)
        {
            // угол для свободной формы
            dataform.Angle = ShowAngle.Checked;
        }

        private void ComboFonts_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataform.Font_default_name = ifc.Families[comboFonts.SelectedIndex].Name;
        }

        private void MaxLenght_TextChanged(object sender, EventArgs e)
        {
            //double value;
            //Autodesk.Revit.DB.UnitFormatUtils.TryParse(dataform.units, Autodesk.Revit.DB.UnitType.UT_Reinforcement_Length, MaxLenght.Text, out value);
            //this.MaxLenght.Text = Autodesk.Revit.DB.UnitFormatUtils.Format(dataform.units, Autodesk.Revit.DB.UnitType.UT_Reinforcement_Length, value, false, false);
            //dataform.Max_Lenght = value;
        }

        private void MaxLenght_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая а ASCII
            {
                e.Handled = true;
            }
        }

        private void MaxLenght_Validating(object sender, CancelEventArgs e)
        {
            double value;
            //double value = Convert.ToDouble(MaxLenght.Text);
            //value = value * 0.003281;
            Autodesk.Revit.DB.UnitFormatUtils.TryParse(dataform.units, Autodesk.Revit.DB.SpecTypeId.ReinforcementLength, MaxLenght.Text, out value);
            this.MaxLenght.Text = Autodesk.Revit.DB.UnitFormatUtils.Format(dataform.units, Autodesk.Revit.DB.SpecTypeId.ReinforcementLength, value, false);
            dataform.Max_Lenght = value;
        }

        private void CoefDiam_TextChanged(object sender, EventArgs e)
        {
            dataform.coef_diam = Convert.ToInt32(this.CoefDiam.Text);
        }

        private void CoefDiam_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8)
            {
                e.Handled = true;
            }
        }

        private void CheckShape_CheckedChanged(object sender, EventArgs e)
        {
            dataform.mode_shape = CheckShape.Checked;
            if(CheckShape.Checked)
            {
                CoefDiam.Enabled = true;
                MaxLenght.Enabled = true;
                LabelDiam.Enabled = true;
                LabelMax.Enabled = true;
                Label2.Enabled = true;
            }
            else
            {
                CoefDiam.Enabled = false;
                MaxLenght.Enabled = false;
                LabelDiam.Enabled = false;
                LabelMax.Enabled = false;
                Label2.Enabled = false;
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            dataform.Font_size = (float) this.numericUpDown1.Value;
        }

        private void ShowDimLines_CheckedChanged(object sender, EventArgs e)
        {
            dataform.Is_dim_lines = ShowDimLines.Checked;
        }

        //private void Help_Click(object sender, EventArgs e)
        //{
        //    Help.ShowHelp(this, "c:\\ProgramData\\Autodesk\\ApplicationPlugins\\ADNPlugin-SketchReinforcement.bundle\\Contents\\Resources\\Help\\Sketch reinforcement_ru.chm", "newtopic1.htm");
        //}

        private void SelectRebars_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            Help.ShowHelp(this, SketchReinforcement.Resourses.Strings.Texts.PathToHelpFile, SketchReinforcement.Resourses.Strings.Texts.HelpSection);
        }

        private void ShiftFromLine_ValueChanged(object sender, EventArgs e)
        {
            dataform.Font_shift = (int)this.ShiftFromLine.Value;
        }

        private void ButtonFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if(folderBrowserDialog.ShowDialog()==DialogResult.OK)
            {
                labelFolder.Text = folderBrowserDialog.SelectedPath;
                dataform.pathFolder = labelFolder.Text;
            }
        }

        private void CheckFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (checkFolder.Checked)
            {
                labelFolder.Visible = false;
                buttonFolder.Enabled = false;
                dataform.IspathFolder = false;

            }
            else
            {
                labelFolder.Visible = true;
                buttonFolder.Enabled = true;
                dataform.IspathFolder = true;
            }
        }

        private void NumericBorder_ValueChanged(object sender, EventArgs e)
        {
            dataform.border = (int) numericBorder.Value;
        }

        private void LabelBorder_Click(object sender, EventArgs e)
        {

        }

        private void ComboColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataform.color = (ConsoleColor) comboColor.SelectedIndex;
            dataform.index_color = comboColor.SelectedIndex;
        }

        private void CheckBoxAllRazdel_CheckedChanged(object sender, EventArgs e)
        {
            dataform.SelectRazdels.Clear();
            if (this.checkBoxAllRazdel.Checked)
            {
                 for(int i=0; i<this.checkedListRazdel.Items.Count;i++)
                {
                    this.checkedListRazdel.SetItemChecked(i, true);
                    dataform.SelectRazdels.Add(this.checkedListRazdel.Items[i].ToString());
                }
            }
                else
            {
                for (int i = 0; i < this.checkedListRazdel.Items.Count; i++)
                {
                    this.checkedListRazdel.SetItemChecked(i, false);
                }
            }

            if (checkBoxAllRazdel.Checked) dataform.IsAllRazdel = true;

            else dataform.IsAllRazdel = false;

        }

        private void CheckedListRazdel_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void CheckBoxBackGround_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxBackGround.Checked) dataform.BackGroundColor = true;

            else  dataform.BackGroundColor = false;


        }
    }
}
