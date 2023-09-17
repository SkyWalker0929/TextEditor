using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
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
            richTextBox1.Text += $"[{DateTime.Now.ToString()}]: {text}\n\r";
            richTextBox1.Select(richTextBox1.Text.Length - 1, richTextBox1.Text.Length - 1);
            richTextBox1.ScrollToCaret();

            DebugMessagesCount++;
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
    }
}
