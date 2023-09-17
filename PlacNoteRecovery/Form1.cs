using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlacNoteRecovery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
            this.Refresh();

            string exepath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!File.Exists("AxInterop.WMPLib.dll")
                || !File.Exists("Interop.WMPLib.dll")
                || !File.Exists("SharpCompress.dll")
                || !File.Exists("System.Text.Json.dll"))
            {
                label1.Text = "Скачивание зависимостей...";

                await Task.Run(() =>
                {

                    using (var client = new HttpClient())
                    {
                        using (var s = client.GetStreamAsync("https://f2lk.me/S7nYUzpWUsEiXA"))
                        {
                            using (var fs = new FileStream("lib.zip", FileMode.OpenOrCreate))
                            {
                                s.Result.CopyTo(fs);

                                fs.Close();
                                fs.Dispose();
                            }

                            s.Dispose();
                        }

                        client.Dispose();
                    }

                });
                

                label1.Text = "Установка зависимостей...";

                try
                {
                    Directory.CreateDirectory(exepath + "\\tmp");
                    ZipFile.ExtractToDirectory("lib.zip", exepath + "\\tmp");

                    DirectoryInfo directoryInfo = new DirectoryInfo(exepath + "\\tmp");
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        if (!File.Exists(exepath + "\\" + Path.GetFileName(fileInfo.FullName)))
                        {
                            File.Copy(fileInfo.FullName, exepath + "\\" + Path.GetFileName(fileInfo.FullName), true);
                        }
                    }

                    Directory.Delete(exepath + "\\tmp", true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Во время инициализации произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                await Task.Run(() => { Thread.Sleep(1000); });

                Process.Start(Assembly.GetExecutingAssembly().Location);

                Environment.Exit(0);
                return;
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(exepath));

                foreach (FileInfo filePath in di.GetFiles())
                {
                    if (filePath.FullName != "pnr.exe")
                    {
                        Process.Start(filePath.FullName);

                        Environment.Exit(0);
                    }
                }

                await Task.Run(() => { Thread.Sleep(1000); });

                label1.Text = "Проблемы не обнаружены";

                pictureBox1.Visible = false;
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = 100;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ( MessageBox.Show($"Завершить работу системы восстановления?", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }
    }
}
