using System;
using System.Collections.Generic;
using System.Linq;
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
                //Console.WriteLine("Skip list missing, generating one");
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
            "_lod1.wmo",
            "_lod2.wmo",
            "_lod3.wmo",
            "_lod.adt",
            "reporter",
            "warcraft",
            ".delete",
            ".signed",
            "pkginfo",
            ".dylib",
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
            DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory + "\\cache\\" + Managers.ConfigurationManager.Profile + "\\listfiles\\");
            FileInfo[] ExtractedListfiles = directory.GetFiles("*.txt");
            foreach (FileInfo fileInfo in ExtractedListfiles)
            {
                using (StreamReader sr = File.OpenText(fileInfo.FullName))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (!EndsWithOneOf(s.ToLower(), SkipList) && !IsWMOGroupFile(s))
                        {
                            MLF.Add(s.ToLower());
                        }
                    }
                }
            }
            //Console.WriteLine("Display list generated");
        }

        public static bool EndsWithOneOf(string value, IEnumerable<string> suffixes)
        {
            return suffixes.Any(suffix => value.EndsWith(suffix));
        }

        public static bool IsWMOGroupFile(string filename)
        {
            if (filename.ToLower().EndsWith(".wmo") && System.Text.RegularExpressions.Regex.Match(Path.GetFileNameWithoutExtension(filename), @"\d{3}$").Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void GenerateCASCList()
        {
            Listfile.Load();
            foreach (var line in Listfile.FDIDToFilename)
            {
                if (Managers.ArchiveManager.FileExists(line.Key))
                {
                    if (!EndsWithOneOf(line.Value, SkipList) && !IsWMOGroupFile(line.Value))
                    {
                        MLF.Add(line.Value);
                    }
                }
            }
            //Console.WriteLine("Display list generated");
        }

        public static void SortListFile()
        {
            MLF.Sort();
            //Console.WriteLine("Sorted");
        }
    }
}
