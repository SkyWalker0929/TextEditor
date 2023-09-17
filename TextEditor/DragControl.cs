using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TextEditor
{
    public class DragControl
    {
        private Point MouseDownLocation;
        public void Control_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                MouseDownLocation = e.Location;
            }
        }
        public void Control_MouseMove(object sender, MouseEventArgs e)
        {
            Control control = sender as Control;
            
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (control.Dock != DockStyle.None)
                {
                    Point backLocaton = control.Location;
                    Size backSize = control.Size;
                    control.Dock = DockStyle.None;
                    control.Size = backSize;
                    control.Location = backLocaton;
                }
                
                control.Left = e.X + control.Left - MouseDownLocation.X;
                control.Top = e.Y + control.Top - MouseDownLocation.Y;
            }
        }
    }
}
