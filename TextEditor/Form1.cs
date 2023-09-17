using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using AxWMPLib;

namespace TextEditor
{
    public partial class SourceForm : Form
    {
        string fileName = null;
        string archiveFolder = null;

        bool keyShift = false;

        // Скорость прокрутки
        int kScroll = 30;

        // Библиотека расширений
        ExtensionLibrary currentExtensionLibrary;
        ExtensionCategories currentExtensionCategory = ExtensionCategories.none;

        // Контролы нетекстовых расширений
        public AxWindowsMediaPlayer windowsMediaPlayer = new AxWindowsMediaPlayer { Dock = DockStyle.Fill };
        public PictureBox pictureBox = new PictureBox { Dock = DockStyle.Fill };
        public WebBrowser archiveExplorer = new WebBrowser { Dock = DockStyle.Fill };
        public TextBox textBox = new TextBox { Dock = DockStyle.Fill };

        // Контролы разработки
        DebugLog debugLog = new DebugLog();

        public SourceForm()
        {
            InitializeComponent();
        }

        void this_MouseWheel(object sender, MouseEventArgs e)
        {
            if (sender == null)
                return;

            Control control = sender as Control;
            Size controlSize = control.Size;
            control.Dock = DockStyle.None;
            control.Size = controlSize;

            debugLog.Log(keyShift.ToString());

            int k = 1;
            if (e.Delta > 0) k = 1; else k = -1;

            if (Control.ModifierKeys == Keys.Shift)
            {
                control.Location = new Point(control.Location.X + k * kScroll, control.Location.Y);
            }
            else if (Control.ModifierKeys == Keys.Control)
            {
                ZoomInOut(k > 0 ? true : false, pictureBox);
            }
            else
            {
                control.Location = new Point(control.Location.X, control.Location.Y + k * kScroll);
            }
        }

        private void ZoomInOut(bool zoom, Control zoomControl)
        {
            //Zoom ratio by which the images will be zoomed by default
            int zoomRatio = 10;
            //Set the zoomed width and height
            int widthZoom = zoomControl.Width * zoomRatio / 100;
            int heightZoom = zoomControl.Height * zoomRatio / 100;
            //zoom = true --> zoom in
            //zoom = false --> zoom out
            if (!zoom)
            {
                widthZoom *= -1;
                heightZoom *= -1;
            }
            //Add the width and height to the picture box dimensions
            zoomControl.Width += widthZoom;
            zoomControl.Height += heightZoom;

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenFileAndCheckExtensionRegistration(dialog.FileName);
            }
        }

        public void OpenFileAndCheckExtensionRegistration(string filePath)
        {
            fileName = filePath;

            currentExtensionLibrary = ExtensionManager.GetExtensionLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
            if (ExtensionManager.ExtensionExist(currentExtensionLibrary, Path.GetExtension(filePath)))
            {
                OpenFile(fileName);
            }
            else
            {
                UEM uem = new UEM();
                ExtensionCategories ExtensionCategory = uem.GetExtension(Path.GetExtension(filePath));
                if (ExtensionCategory != ExtensionCategories.none)
                {
                    if (ExtensionCategory == ExtensionCategories.pictures)
                    {
                        currentExtensionLibrary.pictures.Add(Path.GetExtension(filePath));
                    }
                    else if (ExtensionCategory == ExtensionCategories.text)
                    {
                        currentExtensionLibrary.text.Add(Path.GetExtension(filePath));
                    }
                    else if (ExtensionCategory == ExtensionCategories.video)
                    {
                        currentExtensionLibrary.video.Add(Path.GetExtension(filePath));
                    }
                    else if (ExtensionCategory == ExtensionCategories.archive)
                    {
                        currentExtensionLibrary.archive.Add(Path.GetExtension(filePath));
                    }
                }
                uem.Dispose();

                ExtensionManager.WriteExtensionLibraryToFile(currentExtensionLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");

                OpenFile(fileName);
            }
        }

        public async void OpenFile(string filePath)
        {
            fileName = filePath;
            archiveExplorerToolStripMenuItem.Visible = false;
            panel1.Controls.Clear();
            EnableFields(true);
            ExtensionCategories ExtensionCategories =
                        ExtensionManager.GetCategory(currentExtensionLibrary, Path.GetExtension(filePath));

            if (ExtensionCategories == ExtensionCategories.pictures)
            {
                pictureBox = new PictureBox { ErrorImage = Properties.Resources.logo, InitialImage = Properties.Resources.logo, BackColor = Color.Transparent, Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, Cursor = DefaultCursor };

                panel1.Controls.Add(pictureBox);
                pictureBox.ImageLocation = filePath;
                pictureBox.LoadCompleted += new System.ComponentModel.AsyncCompletedEventHandler(PictureBox_LoadCompleted);

                // PictureBox поддерживает масштабирование
                pictureBox.MouseWheel += new MouseEventHandler(this_MouseWheel);

                // При наведении на PictureBox все остальные контролы становятся неактивными
                pictureBox.MouseHover += Controls_EnabledFalse;
                pictureBox.MouseLeave += Controls_EnabledTrue;

                DisableTextFunctions(true);
            }
            else if (ExtensionCategories == ExtensionCategories.text)
            {
                textBox.Text = File.ReadAllText(fileName);
                panel1.Controls.Add(textBox);

                toolStripStatusLabel2.Text = $"-CC:{textBox.Text.Length}-S:{new System.IO.FileInfo(filePath).Length}B";

                DisableTextFunctions(false);
            }
            else if (ExtensionCategories == ExtensionCategories.video)
            {
                panel1.Controls.Add(windowsMediaPlayer);

                windowsMediaPlayer.CreateControl();
                windowsMediaPlayer.URL = filePath;
                windowsMediaPlayer.Ctlcontrols.play();
                windowsMediaPlayer.StatusChange += WindowsMediaPlayer_StatusChange;

                DisableTextFunctions(true);
            }
            else if (ExtensionCategories == ExtensionCategories.archive)
            {
                DisableTextFunctions(true);
                clearTEMPAndExitToolStripMenuItem.Enabled = false;

                string archiveDirectory = $"{Path.GetTempPath()}pnfile.{new Random().Next(1000000, 9999999)}";
                Directory.CreateDirectory(archiveDirectory);

                archiveExplorer = new WebBrowser { Dock = DockStyle.Fill };
                archiveExplorer.Navigate(archiveDirectory);
                panel1.Controls.Add(archiveExplorer);

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
                catch (Exception ex)
                {
                    MessageBox.Show($"Во время инициализации произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                toolStripStatusLabel2.Text = $"-S:{new System.IO.FileInfo(filePath).Length}B";

                clearTEMPAndExitToolStripMenuItem.Enabled = true;
                archiveFolder = archiveDirectory;
                progress.progressBar.Value = 100;
                progress.Hide();
                progress.Dispose();
            }

            toolStripStatusLabel1.Text = fileName;
        }

        private void Controls_EnabledFalse(object sender, EventArgs e)
        {
            debugLog.Log("menuStrip,statusStrip1,textBox :: Enabled = false");
            menuStrip.Enabled = statusStrip1.Enabled = textBox.Enabled = false;
        }

        private void Controls_EnabledTrue(object sender, EventArgs e)
        {
            debugLog.Log("menuStrip,statusStrip1,textBox :: Enabled = true");
            menuStrip.Enabled = statusStrip1.Enabled = textBox.Enabled = true;
        }

        private void WindowsMediaPlayer_StatusChange(object sender, EventArgs e)
        {
            toolStripStatusLabel2.Text = $"-ST:{windowsMediaPlayer.status}-S:{new System.IO.FileInfo(fileName).Length}";
        }

        private void PictureBox_LoadCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            toolStripStatusLabel2.Text = $"{pictureBox.Image.Width}x{pictureBox.Image.Height}@-PF:{pictureBox.Image.PixelFormat}-RF:{pictureBox.Image.RawFormat}";
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
            Process.Start(Assembly.GetExecutingAssembly().Location);
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
            fontDialog.Font = textBox.Font;
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

        private async void SourceForm_Load(object sender, EventArgs e)
        {
            if (   !File.Exists("AxInterop.WMPLib.dll") 
                || !File.Exists("Interop.WMPLib.dll")
                || !File.Exists("SharpCompress.dll")
                || !File.Exists("System.Text.Json.dll"))
            {
                Process.Start("PlacNoteRecovery.exe");
                Environment.Exit(0);
                return;
            }

            DisableTextFunctions(true);
            if (!File.Exists($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json"))
            {
                File.Create($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json").Dispose();
                ExtensionLibrary ExtensionLibrary = new ExtensionLibrary {

                    pictures = new List<string>(),
                    text = new List<string>(),
                    video = new List<string>(),
                    archive = new List<string>(),

                };
                ExtensionManager.WriteExtensionLibraryToFile(ExtensionLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
                currentExtensionLibrary = ExtensionLibrary;
            }
            else
            {
                currentExtensionLibrary = ExtensionManager.GetExtensionLibraryFromFile($"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
            }

            if (Environment.GetCommandLineArgs().Length > 1)
            {
                fileName = Environment.GetCommandLineArgs()[1];
                OpenFileAndCheckExtensionRegistration(Environment.GetCommandLineArgs()[1]);
            }
            this.Show();

            this.textBox.AllowDrop = true;
            this.textBox.BackColor = System.Drawing.Color.White;
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.textBox.Location = new System.Drawing.Point(0, 24);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(800, 404);
            this.textBox.TabIndex = 1;
            this.textBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FDragDrop);
            this.textBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FDragEnter);
            this.textBox.DragLeave += new System.EventHandler(this.FDragLeave);

            panel1.Controls.Add(textBox);
            textBox.BackColor = Color.FromArgb(150, 150, 255);
            FadeOutAnimaton();
        }

        private void открытьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentExtensionLibrary = ExtensionManager.RemoveExtensionFromExtensionLibrary(currentExtensionLibrary, Path.GetExtension(fileName));
            ExtensionManager.WriteExtensionLibraryToFile(currentExtensionLibrary, $"C:\\Users\\{Environment.UserName}\\PlacNoteConfig.json");
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
            try
            {
                windowsMediaPlayer.Ctlcontrols.stop();
            }
            catch { }

            DisableTextFunctions(true);
            toolStripStatusLabel2.Text = "0x0";
            archiveExplorerToolStripMenuItem.Visible = false;
            textBox.Text = null;
            panel1.Controls.Clear();
            fileName = null;
            toolStripStatusLabel1.Text = "Файл не выбран";
        }

        private void выходИзФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitFile();
        }

        private void FDragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExitFile();
            OpenFileAndCheckExtensionRegistration(fileList[0]);

            if (fileList.Length > 1)
            {
                for (int i = 1; i < fileList.Length; i++)
                {
                    Process.Start(Assembly.GetExecutingAssembly().Location, $"\"{fileList[i]}\"");
                }
            }

            FadeOutAnimaton();
        }

        private async void FDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;

            FadeInAnimaton();
        }

        private async void FDragLeave(object sender, EventArgs e)
        {
            FadeOutAnimaton();
        }
        private void Wait(double seconds)
        {
            int ticks = System.Environment.TickCount + (int)Math.Round(seconds * 1000.0);
            while (System.Environment.TickCount < ticks)
            {
                Application.DoEvents();
            }
        }

        int animationID = 0;
        public async void FadeInAnimaton()
        {
            await Task.Run(() => {
                int localAnimationID = animationID = new Random().Next(1000000, 9999999);
                for (int i = textBox.BackColor.R; animationID == localAnimationID && i >= 150; i -= 5)
                {
                    textBox.BackColor = Color.FromArgb(i, i, 255);
                    Wait(0.001);
                    this.Refresh();
                }
            });
        }

        public async void FadeOutAnimaton()
        {
            await Task.Run(() => {
                int localAnimationID = animationID = new Random().Next(1000000, 9999999);
                for (int i = textBox.BackColor.R; animationID == localAnimationID && i <= 255; i += 5)
                {
                    textBox.BackColor = Color.FromArgb(i, i, 255);
                    Wait(0.001);
                    this.Refresh();
                }
            });
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Progress progress = new Progress { TopMost = true };
            progress.mainLabel.Text = "Сохранение архива...";
            progress.progressBar.Visible = false;
            progress.percentLabel.Visible = false;
            progress.Show();
            progress.Refresh();

            await Task.Run(() =>
            {
                File.Delete(fileName);
                ZipFile.CreateFromDirectory(archiveFolder, fileName);
            });

            progress.Hide();
            progress.Dispose();
        }

        private void SourceForm_Resize(object sender, EventArgs e)
        {
            ControlToTheCenter(pictureBox);
        }

        public void ControlToTheCenter(Control control)
        {
            control.Left = (this.ClientSize.Width - control.Width) / 2;
            control.Top = (this.ClientSize.Height - control.Height) / 2;
        }

        private void SourceForm_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void SourceForm_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void debugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
