using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    internal static class Program
    {
        public static DebugLog debugLog = null;
        public static Mods modsManager = null;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                debugLog = new DebugLog();
                modsManager = new Mods();

                Application.Run(new SourceForm());
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Обнаружено непредвиденное исключение. Запустить систему автоматического восстановления?\n\nТекст исключения:\n" + ex.Message, "Ошибка", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes) 
                {
                    try
                    {
                        Progress progress = new Progress { TopMost = true };
                        progress.mainLabel.Text = "Обнаружено исключение. Загрузка PlacNoteRecovery...";
                        progress.percentLabel.Visible = false;
                        progress.progressBar.Style = ProgressBarStyle.Marquee;
                        progress.Show();
                        progress.Refresh();

                        using (var client = new HttpClient())
                        {
                            using (var s = client.GetStreamAsync("https://f2lk.me/F0_22bVIV91WSe"))
                            {
                                using (var fs = new FileStream("PlacNoteRecovery.exe", FileMode.OpenOrCreate))
                                {
                                    s.Result.CopyTo(fs);

                                    fs.Close();
                                    fs.Dispose();
                                }

                                s.Dispose();
                            }

                            client.Dispose();
                        }

                        progress.Hide();
                        progress.Dispose();

                        Process.Start("PlacNoteRecovery.exe");
                    }
                    catch
                    {
                        MessageBox.Show("Запуск системы восстановления невозможен. Дальнейшая работа программы невозможна. Установите пакет приложения повторно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
