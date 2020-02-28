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
            "(patch_metadata)",
            "signaturefile",
            ".manifest",
            "_obj0.adt",
            "_obj1.adt",
            "_tex0.adt",
            "_tex1.adt",
            "_lod.adt",
            "reporter",
            "warcraft",
            ".delete",
            "pkginfo",
            ".plist",
            "gaming",
            ".anim",
            ".blob",
            ".html",
            ".icns",
            ".lock",
            ".phys",
            ".rsrc",
            ".skin",
            ".test",
            ".what",
            ".avi",
            ".bls",
            ".cfg",
            ".css",
            ".dll",
            ".exe",
            ".ini",
            ".lit",
            ".lst",
            ".lua",
            ".nib",
            ".not",
            ".pak",
            ".pdf",
            ".sbt",
            ".sig",
            ".tex",
            ".toc",
            ".trs",
            ".ttf",
            ".txt",
            ".url",
            ".uvw",
            ".wdl",
            ".wdt",
            ".wfx",
            ".wlm",
            ".wlq",
            ".wlw",
            ".wtf",
            ".xib",
            ".xml",
            ".xsd",
            ".zmp",
            ".js",

            ".db",
            ".db2",
            ".dbc",

            ".gif",
            ".jpg",
            ".png",

            ".mp3",
            ".ogg",
            ".wav"
            
            //".adt"
            //".blp"
            //".m2"
            //".wmo"
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
