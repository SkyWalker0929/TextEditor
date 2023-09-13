using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpCompress;
using SharpCompress.Archives;

namespace TextEditor
{
    public partial class SourceForm : Form
    {
        string fileName = null;

        ExtendtionsLibrary currentExtendtionsLibrary;
        ExtendtionsCategories currentExtendtionCategory = ExtendtionsCategories.none;

        AxWMPLib.AxWindowsMediaPlayer windowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer { Dock = DockStyle.Fill };
        PictureBox pictureBox = new PictureBox();

        WebBrowser archiveExplorer = new WebBrowser();
        string archiveFolder = null;

        public SourceForm()
        {
            InitializeComponent();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                fileName = dialog.FileName;
                
                currentExtendtionsLibrary = ExtendtionsManager.GetExtendtionsLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
                if (ExtendtionsManager.ExtendtionExist(currentExtendtionsLibrary, Path.GetExtension(dialog.FileName)))
                {
                    OpenFile(fileName);
                }
                else
                {
                    UEM uem = new UEM();
                    ExtendtionsCategories extendtionsCategory = uem.GetExtendtion(Path.GetExtension(dialog.FileName));
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
                        else if (extendtionsCategory == ExtendtionsCategories.archive)
                        {
                            currentExtendtionsLibrary.archive.Add(Path.GetExtension(dialog.FileName));
                        }
                    }
                    uem.Dispose();

                    ExtendtionsManager.WriteExtendtionsLibraryToFile(currentExtendtionsLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");

                    OpenFile(fileName);
                }
            }
        }

        public async void OpenFile(string filePath)
        {
            fileName = filePath;
            archiveExplorerToolStripMenuItem.Visible = false;
            textBox.Controls.Clear();
            EnableFields(true);
            ExtendtionsCategories extendtionsCategories =
                        ExtendtionsManager.GetCategory(currentExtendtionsLibrary, Path.GetExtension(filePath));
            if (extendtionsCategories == ExtendtionsCategories.pictures)
            {
                pictureBox = new PictureBox { ErrorImage = Properties.Resources.logo, InitialImage = Properties.Resources.logo, BackColor = Color.Transparent, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Cursor = DefaultCursor };

                textBox.Controls.Add(pictureBox);
                pictureBox.ImageLocation = filePath;
                DisableTextFunctions(true);
            }
            else if (extendtionsCategories == ExtendtionsCategories.text)
            {
                textBox.Text = File.ReadAllText(fileName);
                DisableTextFunctions(false);
            }
            else if (extendtionsCategories == ExtendtionsCategories.video)
            {
                textBox.Controls.Add(windowsMediaPlayer);

                windowsMediaPlayer.CreateControl();
                windowsMediaPlayer.URL = filePath;
                windowsMediaPlayer.Ctlcontrols.play();
                DisableTextFunctions(true);
            }
            else if (extendtionsCategories == ExtendtionsCategories.archive)
            {
                DisableTextFunctions(true);

                string archiveDirectory = $"{Path.GetTempPath()}pnfile.{new Random().Next(1000000, 9999999)}";
                Directory.CreateDirectory(archiveDirectory);

                archiveExplorer = new WebBrowser { Dock = DockStyle.Fill };
                archiveExplorer.Navigate(archiveDirectory);
                textBox.Controls.Add(archiveExplorer);

                Progress progress = new Progress { TopMost = true };
                progress.mainLabel.Text = "Подготовка архива для быстрого редактирования...";
                progress.progressBar.Visible = false;
                progress.percentLabel.Visible = false;
                progress.Show();
                progress.Refresh();

                archiveExplorerToolStripMenuItem.Visible = true;

                try
                {
                    await Task.Run(() => {
                        SharpCompress.Archives.ArchiveFactory.WriteToDirectory(filePath, archiveDirectory, new SharpCompress.Common.ExtractionOptions { Overwrite = true, ExtractFullPath = true, PreserveFileTime = true });
                    });
                }
                catch { }

                archiveFolder = archiveDirectory;
                progress.progressBar.Value = 100;
                progress.Hide();
                progress.Dispose();
            }

            toolStripStatusLabel1.Text = fileName;
        }

        public void DisableTextFunctions(bool yon)
        {
            правкаToolStripMenuItem.Visible =
            форматToolStripMenuItem.Visible =
            сохранитьToolStripMenuItem.Visible = 
            печатьToolStripMenuItem.Visible = 
            toolStripSeparator2.Visible =
            параметрыПечатиToolStripMenuItem.Visible = !yon;

            if (fileName != null)
            {
                открытьКакToolStripMenuItem.Enabled = true;
            }
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
                    video = new List<string>(),
                    archive = new List<string>(),

                };
                ExtendtionsManager.WriteExtendtionsLibraryToFile(extendtionsLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
                currentExtendtionsLibrary = extendtionsLibrary;
            }
            else
            {
                currentExtendtionsLibrary = ExtendtionsManager.GetExtendtionsLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
            }
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                fileName = Environment.GetCommandLineArgs()[1];
                OpenFile(Environment.GetCommandLineArgs()[1]);
            }
        }

        private void открытьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentExtendtionsLibrary = ExtendtionsManager.RemoveExtendtionFromExtendtionsLibrary(currentExtendtionsLibrary, Path.GetExtension(fileName));
            ExtendtionsManager.WriteExtendtionsLibraryToFile(currentExtendtionsLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
        }

        private void кнопкиWindowsMediaPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            кнопкиWindowsMediaPlayerToolStripMenuItem.Checked = !кнопкиWindowsMediaPlayerToolStripMenuItem.Checked;

            if (!кнопкиWindowsMediaPlayerToolStripMenuItem.Checked)
            {
                windowsMediaPlayer.uiMode = "none";
            }
            else
            {
                windowsMediaPlayer.uiMode = "full";
            }
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            archiveExplorer.GoBack();
        }

        private void forwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            archiveExplorer.GoForward();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            archiveExplorer.Refresh();
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void zoomToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void centerImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private async void clearTEMPAndExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dr = MessageBox.Show("Delete temporary folder before exiting?", "Exiting ArchiveExplorer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

            if (dr == DialogResult.Cancel)
            {
                return;
            }

            if (dr == DialogResult.Yes)
            {
                Progress progress = new Progress { TopMost = true };
                progress.mainLabel.Text = "Deleting temporary files...";
                progress.progressBar.Visible = false;
                progress.percentLabel.Visible = false;
                progress.Show();
                progress.Refresh();

                await Task.Run(() => {
                    Directory.Delete(archiveFolder, true);
                });

                progress.Hide();
                progress.Dispose();
            }

            ExitFile();
        }

        void ExitFile()
        {
            DisableTextFunctions(true);
            archiveExplorerToolStripMenuItem.Visible = false;
            textBox.Text = null;
            textBox.Controls.Clear();
            fileName = null;
            toolStripStatusLabel1.Text = "Файл не выбран";
        }

        private void textBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void textBox_DragLeave(object sender, EventArgs e)
        {
            
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExitFile();
            OpenFile(fileList[0]);
        }

        private void выходИзФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitFile();
        }
    }
}
