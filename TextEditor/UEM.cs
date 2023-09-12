using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        ExtendtionsCategories extendtionsCategories = ExtendtionsCategories.none;
        bool done = false;

        private void Wait(double seconds)
        {
            int ticks = System.Environment.TickCount + (int)Math.Round(seconds * 1000.0);
            while (System.Environment.TickCount < ticks)
            {
                Application.DoEvents();
            }
        }

        public ExtendtionsCategories GetExtendtion()
        {
            this.ShowDialog();

            while (!done)
            {
                Wait(0.1);
            }

            return extendtionsCategories;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            extendtionsCategories = ExtendtionsCategories.text;
            done = true;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            extendtionsCategories = ExtendtionsCategories.pictures;
            done = true;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            extendtionsCategories = ExtendtionsCategories.video;
            done = true;
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            extendtionsCategories = ExtendtionsCategories.none;
            done = true;
            this.Hide();
        }
    }
}
