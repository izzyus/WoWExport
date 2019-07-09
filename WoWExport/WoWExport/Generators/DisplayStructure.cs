using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WoWExport.Generators
{
    class DisplayStructure
    {
        public static List<String> MLF = new List<string>();
        public static string[] Skip =
        {
            "_obj0.adt",
            "_obj1.adt",
            "_tex0.adt",
            "_tex1.adt",
            "_lod.adt",
            ".manifest",
            ".anim",
            ".skin",
            ".blob",
            ".wdt",
            ".wdl",
            ".lst",
            ".txt",
            ".tex",
            ".dll",
            ".exe",
            ".bls",
            ".cfg",
            ".wtf",
            ".trs",
            ".db",

            ".mp3",
            ".ogg",
            ".blp"
        };

        public static void GenerateList()
        {
            DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles\\"); //hardcoded for debug only
            FileInfo[] ExtractedListfiles = directory.GetFiles("*.txt");
            foreach (FileInfo fileInfo in ExtractedListfiles)
            {
                using (StreamReader sr = File.OpenText(fileInfo.FullName))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        //MLF.Add(s.ToLower());
                        if (!EndsWithOneOf(s.ToLower(), Skip))
                        {
                            MLF.Add(s.ToLower());
                        }
                    }
                }
            }
        }

        public static bool EndsWithOneOf(string value, IEnumerable<string> suffixes)
        {
            return suffixes.Any(suffix => value.EndsWith(suffix));
        }
    }
}
