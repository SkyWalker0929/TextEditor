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
    public class ExtendtionsLibrary
    {
        public List<string> pictures { get; set; }
        public List<string> text { get; set; }
        public List<string> video { get; set; }
    }

    public enum ExtendtionsCategories
    {
        pictures,
        text,
        video,
        none
    }

    public class ExtendtionsManager
    {
        public static ExtendtionsLibrary RemoveExtendtionFromExtendtionsLibrary(ExtendtionsLibrary extendtionsLibrary, string extendtion)
        {
            ExtendtionsLibrary _extendtionsLibrary = extendtionsLibrary;

            _extendtionsLibrary.pictures.Remove(extendtion);
            _extendtionsLibrary.text.Remove(extendtion);
            _extendtionsLibrary.video.Remove(extendtion);

            return _extendtionsLibrary;
        }
        public static bool ExtendtionExist(ExtendtionsLibrary extendtionsLibrary, string extendtion)
        {
            foreach (string ext in extendtionsLibrary.pictures)
            {
                if (ext.Contains(extendtion)) return true;
            }

            foreach (string ext in extendtionsLibrary.text)
            {
                if (ext.Contains(extendtion)) return true;
            }

            foreach (string ext in extendtionsLibrary.video)
            {
                if (ext.Contains(extendtion)) return true;
            }

            return false;

            //return (extendtionsLibrary.pictures.Contains(extendtion)
            //     && extendtionsLibrary.text.Contains(extendtion)
            //     && extendtionsLibrary.video.Contains(extendtion));
        }

        public static ExtendtionsCategories GetCategory(ExtendtionsLibrary extendtionsLibrary, string extention)
        {
            if (extendtionsLibrary.pictures.Contains(extention))
                return ExtendtionsCategories.pictures;

            if (extendtionsLibrary.text.Contains(extention))
                return ExtendtionsCategories.text;

            if (extendtionsLibrary.video.Contains(extention))
                return ExtendtionsCategories.video;

            return ExtendtionsCategories.none;
        }
        public static ExtendtionsLibrary GetExtendtionsLibraryFromFile(string filePath)
        {
            return JsonSerializer.Deserialize<ExtendtionsLibrary>(File.ReadAllText(filePath));
        }
        public static void WriteExtendtionsLibraryToFile(ExtendtionsLibrary extendtionsLibrary, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonSerializer.Serialize(extendtionsLibrary));
            } 
            else
            {
                new Exception("ты даун");
            }
        }
    }
}
