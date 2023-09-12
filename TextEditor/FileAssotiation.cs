using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public class FileAssociation
    {
        public void RegisterFileAssociation(string extension, string progId, string description, string exePath)
        {
            // Создание ключа для расширения файла
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(extension))
            {
                key.SetValue("", progId);
            }

            // Создание ключа для программы
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progId))
            {
                key.SetValue("", description);
                key.CreateSubKey("DefaultIcon").SetValue("", "\"" + exePath + "\",0");
                key.CreateSubKey(@"Shell\Open\Command").SetValue("", "\"" + exePath + "\" \"%1\"");
            }
        }
    }
}
