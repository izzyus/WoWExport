using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using StormLibSharp;

namespace WoWExport.Managers
{
    class ArchiveManager
    {
        public List<String> MainListFile = new List<String>();
        public String GameDir;

        public void GenerateMainListFile()
        {
            DirectoryInfo directory = new DirectoryInfo(@"D:\test\"); //hardcoded for debug only
            FileInfo[] ExtractedListfiles = directory.GetFiles("*.txt"); //hardcoded for debug only
            foreach (FileInfo fileInfo in ExtractedListfiles)
            {
                using (StreamReader sr = File.OpenText(fileInfo.FullName))
                {
                    string s = String.Empty;
                    while ((s = sr.ReadLine()) != null)
                    {
                        MainListFile.Add(s + ";" + fileInfo.Name.Replace(".txt",".mpq"));
                    }
                }
            }
        }

        public Stream ReadThisFile(string filename)
        {
            //Find the goddamn file in the archive hell
            int index = MainListFile.FindIndex(a => a.Contains(filename));
            Stream stream;

            //Get the archive in which the requested file resides
            string archive = MainListFile[index];
            archive = archive.Substring(archive.LastIndexOf(';', archive.Length - 2) + 1);

            //Get the file (+path) 
            string file = MainListFile[index];
            file = file.Substring(0, file.LastIndexOf(';'));

            //Open the archive
            MpqArchive CurrentArchive = new MpqArchive(GameDir + "\\data\\" + archive, FileAccess.Read);
            stream = CurrentArchive.OpenFile(filename);

            return stream;
        }
    }
}
