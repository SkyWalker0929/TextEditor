using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class Mods : Form
    {
        public Mods()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("mods");
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Библиотеки (*.dll)|*.dll" };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    listBox1.Items.Add(Path.GetFileName(openFileDialog.FileName));
                    File.Copy(openFileDialog.FileName, "mods\\" + Path.GetFileName(openFileDialog.FileName));
                    panel1.Visible = true;
                }
                catch { }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label3.Visible = listBox1.Items.Count > 0 ? false : true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                File.Delete("mods\\" + Path.GetFileName(listBox1.Items[listBox1.SelectedIndex].ToString()));
                listBox1.Items.Remove(listBox1.SelectedItems[0]);
                panel1.Visible = true;
            }
        }

        private void Mods_Load(object sender, EventArgs e)
        {
            if (Directory.Exists("mods"))
            {
                listBox1.Items.Clear();
                DirectoryInfo directoryInfo = new DirectoryInfo("mods");
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    listBox1.Items.Add(Path.GetFileName(file.FullName));
                }
            }
        }

        private void Mods_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
    }
}
