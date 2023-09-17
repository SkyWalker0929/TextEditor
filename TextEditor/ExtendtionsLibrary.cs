using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Runtime.Remoting.Lifetime;

namespace TextEditor
{
    [Serializable]
    public class ExtensionLibrary
    {
        public List<string> pictures { get; set; }
        public List<string> text { get; set; }
        public List<string> video { get; set; }
        public List<string> archive { get; set; }
    }

    public enum ExtensionCategories
    {
        pictures,
        text,
        video,
        archive,
        none
    }

    public class ExtensionManager
    {
        public static ExtensionLibrary RemoveExtensionFromExtensionLibrary(ExtensionLibrary ExtensionLibrary, string Extension)
        {
            Program.debugLog.Log($" [Extension Manager] Removing an extension {Extension}");

            ExtensionLibrary _ExtensionLibrary = ExtensionLibrary;

            _ExtensionLibrary.pictures.Remove(Extension);
            _ExtensionLibrary.text.Remove(Extension);
            _ExtensionLibrary.video.Remove(Extension);

            return _ExtensionLibrary;
        }
        public static bool ExtensionExist(ExtensionLibrary ExtensionLibrary, string Extension)
        {
            Program.debugLog.Log($" [Extension Manager] Checking the extension {Extension}");

            foreach (string ext in ExtensionLibrary.pictures)
            {
                if (ext.Contains(Extension)) return true;
            }

            foreach (string ext in ExtensionLibrary.text)
            {
                if (ext.Contains(Extension)) return true;
            }

            foreach (string ext in ExtensionLibrary.video)
            {
                if (ext.Contains(Extension)) return true;
            }

            foreach (string ext in ExtensionLibrary.archive)
            {
                if (ext.Contains(Extension)) return true;
            }

            return false;

            //return (ExtensionLibrary.pictures.Contains(Extension)
            //     && ExtensionLibrary.text.Contains(Extension)
            //     && ExtensionLibrary.video.Contains(Extension));
        }

        public static ExtensionCategories GetCategory(ExtensionLibrary ExtensionLibrary, string extention)
        {
            Program.debugLog.Log($" [Extension Manager] Getting extension type {extention}");

            if (ExtensionLibrary.pictures.Contains(extention))
                return ExtensionCategories.pictures;

            if (ExtensionLibrary.text.Contains(extention))
                return ExtensionCategories.text;

            if (ExtensionLibrary.video.Contains(extention))
                return ExtensionCategories.video;

            if (ExtensionLibrary.archive.Contains(extention))
                return ExtensionCategories.archive;

            return ExtensionCategories.none;
        }
        public static ExtensionLibrary GetExtensionLibraryFromFile(string filePath)
        {
            Program.debugLog.Log($" [Extension Manager] Reading library {filePath}");

            return JsonSerializer.Deserialize<ExtensionLibrary>(File.ReadAllText(filePath));
        }
        public static void WriteExtensionLibraryToFile(ExtensionLibrary ExtensionLibrary, string filePath)
        {
            Program.debugLog.Log($" [Extension Manager] Write to file {filePath}");

            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(ExtensionLibrary));
            } 
            else
            {
                new Exception("ты даун");
            }
        }
    }
}
