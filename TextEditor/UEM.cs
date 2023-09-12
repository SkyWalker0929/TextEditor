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

        string currentExtendtion = null;
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

        public ExtendtionsCategories GetExtendtion(string extendtion)
        {
            this.ShowDialog();

            currentExtendtion = extendtion;

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
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Enabled)
            {
                new FileAssociation().RegisterFileAssociation(currentExtendtion, "PlacNote.File", currentExtendtion, Assembly.GetExecutingAssembly().Location);
            }
        }

        private void UEM_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                extendtionsCategories = ExtendtionsCategories.none;
                done = true;
                this.Hide();
            }
        }
    }
}
