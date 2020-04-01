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
        public static string[] SkipList;


        public static void LoadSkipList()
        {
            if (File.Exists(Environment.CurrentDirectory + "\\settings\\skiplist.txt"))
            {
                SkipList = File.ReadAllLines(Environment.CurrentDirectory + "\\settings\\skiplist.txt");
            }
            else
            {
                Console.WriteLine("Skip list missing, generating one");
                GenerateSkipList();
            }
        }

        public static void GenerateSkipList()
        {
            SkipList = new string[]
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
            ".signed",
            "pkginfo",
            ".plist",
            "gaming",
            ".anim",
            ".blob",
            ".bone",
            ".html",
            ".icns",
            ".lock",
            ".phys",
            ".rsrc",
            ".skel",
            ".skin",
            ".test",
            ".what",
            ".avi",
            ".bls",
            ".cfg",
            ".csp",
            ".css",
            ".dll",
            ".exe",
            ".htm",
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

            File.WriteAllLines(Environment.CurrentDirectory + "\\settings\\skiplist.txt", SkipList);
        }


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
                        if (!EndsWithOneOf(s.ToLower(), SkipList))
                        {
                            MLF.Add(s.ToLower());
                        }
                    }
                }
            }
            Console.WriteLine("Display list generated");
        }

        public static bool EndsWithOneOf(string value, IEnumerable<string> suffixes)
        {
            return suffixes.Any(suffix => value.EndsWith(suffix));
        }

        public static void GenerateCASCList()
        {
            Listfile.Load();
            foreach (var line in Listfile.FDIDToFilename)
            {
                //Why does it not work with "line.Key"? To be investigated...
                if (Managers.ArchiveManager.cascHandler.FileExists(line.Value))
                {
                    if (!EndsWithOneOf(line.Value, SkipList))
                    {
                        MLF.Add(line.Value);
                    }
                }
            }
            Console.WriteLine("Display list generated");
        }

        public static void SortListFile()
        {
            MLF.Sort();
            Console.WriteLine("Sorted");
        }
    }
}
