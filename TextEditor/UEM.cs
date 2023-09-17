using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class UEM : Form
    {
        public UEM()
        {
            InitializeComponent();
        }

        string currentExtension = null;
        ExtensionCategories ExtensionCategories = ExtensionCategories.none;
        bool done = false;

        private void Wait(double seconds)
        {
            int ticks = System.Environment.TickCount + (int)Math.Round(seconds * 1000.0);
            while (System.Environment.TickCount < ticks)
            {
                Application.DoEvents();
            }
        }

        public ExtensionCategories GetExtension(string Extension)
        {
            this.ShowDialog();

            currentExtension = Extension;

            while (!done)
            {
                Wait(0.1);
            }

            return ExtensionCategories;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ExtensionCategories = ExtensionCategories.text;
            done = true;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ExtensionCategories = ExtensionCategories.pictures;
            done = true;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExtensionCategories = ExtensionCategories.video;
            done = true;
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Enabled)
            {
                new FileAssociation().RegisterFileAssociation(currentExtension, "PlacNote.File", currentExtension, Assembly.GetExecutingAssembly().Location);
            }
        }

        private void UEM_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                ExtensionCategories = ExtensionCategories.none;
                done = true;
                this.Hide();
            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            ExtensionCategories = ExtensionCategories.archive;
            done = true;
            this.Hide();
        }
    }
}
