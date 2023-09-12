﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TextEditor
{
    public partial class SourceForm : Form
    {
        string fileName;
        ExtendtionsLibrary currentExtendtionsLibrary;
        ExtendtionsCategories currentExtendtionCategory = ExtendtionsCategories.none;

        public SourceForm()
        {
            InitializeComponent();
        }

        private async void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fileName = dialog.FileName;
                EnableFields(true);

                textBox.Controls.Clear();
                currentExtendtionsLibrary = ExtendtionsManager.GetExtendtionsLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
                if (ExtendtionsManager.ExtendtionExist(currentExtendtionsLibrary, Path.GetExtension(dialog.FileName)))
                {
                    OpenFile(fileName);
                }
                else
                {
                    UEM uem = new UEM();
                    ExtendtionsCategories extendtionsCategory = uem.GetExtendtion();
                    if (extendtionsCategory != ExtendtionsCategories.none)
                    {
                        if (extendtionsCategory == ExtendtionsCategories.pictures)
                        {
                            currentExtendtionsLibrary.pictures.Add(Path.GetExtension(dialog.FileName));
                        }
                        else if (extendtionsCategory == ExtendtionsCategories.text)
                        {
                            currentExtendtionsLibrary.text.Add(Path.GetExtension(dialog.FileName));
                        }
                        else if (extendtionsCategory == ExtendtionsCategories.video)
                        {
                            currentExtendtionsLibrary.video.Add(Path.GetExtension(dialog.FileName));
                        }
                    }
                    uem.Dispose();

                    ExtendtionsManager.WriteExtendtionsLibraryToFile(currentExtendtionsLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");

                    OpenFile(fileName);
                }
            }
        }

        public void OpenFile(string filePath)
        {
            ExtendtionsCategories extendtionsCategories =
                        ExtendtionsManager.GetCategory(currentExtendtionsLibrary, Path.GetExtension(filePath));
            if (extendtionsCategories == ExtendtionsCategories.pictures)
            {
                PictureBox pictureBox = new PictureBox { ErrorImage = Properties.Resources.logo, InitialImage = Properties.Resources.logo, BackColor = Color.Black, Dock = DockStyle.Fill, ImageLocation = filePath, SizeMode = PictureBoxSizeMode.Zoom, Cursor = DefaultCursor };
                textBox.Controls.Add(pictureBox);
                DisableTextFunctions(true);
            }
            else if (extendtionsCategories == ExtendtionsCategories.text)
            {
                textBox.Text = File.ReadAllText(fileName);
                DisableTextFunctions(false);
            }
            else if (extendtionsCategories == ExtendtionsCategories.video)
            {
                DisableTextFunctions(true);
            }
        }

        public void DisableTextFunctions(bool yon)
        {
            правкаToolStripMenuItem.Enabled =
            видToolStripMenuItem.Enabled =
            форматToolStripMenuItem.Enabled =
            сохранитьToolStripMenuItem.Enabled = 
            печатьToolStripMenuItem.Enabled = 
            параметрыПечатиToolStripMenuItem.Enabled = !yon;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.WriteAllText(fileName, textBox.Text);
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                fileName = dialog.FileName;
                File.Create(fileName).Dispose();
                File.WriteAllText(fileName, textBox.Text);
            }
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.CheckFileExists = false;
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                fileName = dialog.FileName;
                File.Create(fileName).Dispose();
                textBox.Text = "";
            }
        }

        private void удалитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Вы точно хотите удалить текущий файл?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                File.Delete(fileName);
                удалитьФайлToolStripMenuItem.Enabled = false;
                textBox.Text = "";
                EnableFields(false);
            }
        }

        private void EnableFields(bool enable)
        {
             сохранитьToolStripMenuItem.Enabled = удалитьФайлToolStripMenuItem.Enabled = enable;
        }

        private void новоеОкноToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new SourceForm();
            form.Show();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Undo();
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Cut();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Copy();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Paste();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = "";
        }

        private void правкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(textBox.SelectedText))
            {
                копироватьToolStripMenuItem.Enabled = false;
                удалитьToolStripMenuItem.Enabled = false;
                вырезатьToolStripMenuItem.Enabled = false;             
            } else
            {
                копироватьToolStripMenuItem.Enabled = true;
                удалитьToolStripMenuItem.Enabled = true;
                вырезатьToolStripMenuItem.Enabled = true;
            }

        }

        private void выделитьВсёToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void времяИДатаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime data = DateTime.Now;
            textBox.Paste(data.ToString());
        }

        private void переносПоСтрокамToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.WordWrap = !textBox.WordWrap;
        }

        private void шрифтToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Font = fontDialog.Font;
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            infoForm form = new infoForm();
            form.ShowDialog();
        }

        private void печатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog dialog = new PrintDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                
            }
        }

        private void увеличитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void уменьшитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void восстановитьToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void строкаСостоянияToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            statusStrip1.Visible = строкаСостоянияToolStripMenuItem.Checked;
        }

        private void строкаСостоянияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            строкаСостоянияToolStripMenuItem.Checked = !строкаСостоянияToolStripMenuItem.Checked;
        }

        private void SourceForm_Load(object sender, EventArgs e)
        {
            DisableTextFunctions(true);
            if (!File.Exists($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json"))
            {
                File.Create($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json").Dispose();
                ExtendtionsLibrary extendtionsLibrary = new ExtendtionsLibrary {

                    pictures = new List<string>(),
                    text = new List<string>(),
                    video = new List<string>()

                };
                ExtendtionsManager.WriteExtendtionsLibraryToFile(extendtionsLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
                currentExtendtionsLibrary = extendtionsLibrary;
            }
            else
            {
                currentExtendtionsLibrary = ExtendtionsManager.GetExtendtionsLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
            }
        }
    }
}
