using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class DebugLog : Form
    {
        public DebugLog()
        {
            InitializeComponent();
        }

        int DebugMessagesCount = 0;

        public void Log(string text)
        {
            richTextBox1.Text += $"[{DateTime.Now.Date} {DateTime.Now.TimeOfDay.TotalMilliseconds}]: {text}\n\r";
            richTextBox1.Select(richTextBox1.Text.Length - 1, richTextBox1.Text.Length - 1);
            richTextBox1.ScrollToCaret();

            DebugMessagesCount++;
        }
        public void CriticalErrorLog(string text)
        {
            label1.Text = "Обнаружена критическая ошибка. Выполнение программы приостановлено.";
            button1.Enabled = false;
            ControlBox = true;

            Log(text);

            this.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = null;
            DebugMessagesCount = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Text = $"-LOGTIME:{DateTime.Now.ToString()};~~{DebugMessagesCount}";
        }

        private void DebugLog_Load(object sender, EventArgs e)
        {

        }

        private void DebugLog_Resize(object sender, EventArgs e)
        {
            richTextBox1.Size = new Size(this.Width - 40, this.Height - 90);
            label1.Text = "ST_SIZEMODE=SIZEBOX";
        }

        private void DebugLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
