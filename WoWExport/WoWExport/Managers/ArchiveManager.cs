using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using StormLibSharp;

namespace Managers
{
    class ArchiveManager
    {
        public static List<String> MainListFile = new List<String>();
        public static String GameDir;
        public static MpqArchive art = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\art.mpq", FileAccess.Read);
        public static MpqArchive expansion1 = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\expansion1.MPQ", FileAccess.Read);
        public static MpqArchive expansion2 = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\expansion2.MPQ", FileAccess.Read);
        public static MpqArchive expansion3 = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\expansion3.MPQ", FileAccess.Read);
        public static MpqArchive world = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\world.mpq", FileAccess.Read);
        public static MpqArchive world2 = new MpqArchive(@"D:\World of Warcraft - Cataclysm\Data\world2.mpq", FileAccess.Read);

        public static void GenerateMainListFileFromTXT()
        {
            //Only generate list if empty
            if (MainListFile.Count == 0)
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
                            MainListFile.Add(s.ToLower() + ";" + fileInfo.Name.Replace(".txt", ".mpq"));
                        }
                    }
                }
            }
        }

        public static void GenerateMainListFileFromMPQ()
        {
            //Only generate list if empty
            if (MainListFile.Count == 0)
            {
                DirectoryInfo directory = new DirectoryInfo(GameDir + "\\data\\");
                FileInfo[] Archives = directory.GetFiles("*.mpq");
                string listFile = null;

                foreach (FileInfo fileinfo in Archives)
                {
                    using (MpqArchive archive = new MpqArchive(fileinfo.FullName, FileAccess.Read))
                    {
                        using (MpqFileStream file = archive.OpenFile("(listfile)"))
                        using (StreamReader sr = new StreamReader(file))
                        {
                            listFile = sr.ReadToEnd();
                            Console.WriteLine(listFile);
                            MainListFile.Add(listFile.ToLower() + ";" + fileinfo.Name);
                        }
                    }
                }
            }
        }

        public static Stream ReadThisFile(string filename)
        {
            //Find the goddamn file in the archive hell
            int index = MainListFile.FindIndex(a => a.Contains(filename.ToLower()));
            //Stream stream;
            Stream stream = null;

            if (index != -1)
            {
                //Get the archive in which the requested file resides
                string archive = MainListFile[index];
                archive = archive.Substring(archive.LastIndexOf(';', archive.Length - 2) + 1);

                //Old manager:
                /*
                //i just realised how obsolete this one is... (string filename) <-- seems familiar?
                //Get the file (+path) 
                
                //string file = MainListFile[index];
                //file = file.Substring(0, file.LastIndexOf(';'));
                

                //Try to open the archive and read the requested file
                try
                {
                    MpqArchive CurrentArchive = new MpqArchive(GameDir + "\\data\\" + archive, FileAccess.Read);
                    stream = CurrentArchive.OpenFile(filename);
                }
                catch
                {
                    //Error while opening the archive / reading the file
                    stream = null;
                }
                */

                switch (archive.Substring(0, archive.Length - 4))
                {
                    case "art":
                        stream = art.OpenFile(filename);
                        break;
                    case "expansion1":
                        stream = expansion1.OpenFile(filename);
                        break;
                    case "expansion2":
                        stream = expansion2.OpenFile(filename);
                        break;
                    case "expansion3":
                        stream = expansion3.OpenFile(filename);
                        break;
                    case "world":
                        stream = world.OpenFile(filename);
                        break;
                    case "world2":
                        stream = world2.OpenFile(filename);
                        break;
                }
            }
            else
            {
                //File is missing...
                stream = null;
            }
            return stream;
        }

        public static void ExtractListfiles(string to)
        {
            string listFile = null;

            DirectoryInfo directory = new DirectoryInfo(GameDir + "\\data\\");
            FileInfo[] Archives = directory.GetFiles("*.mpq");
            foreach (FileInfo fileinfo in Archives)
            {
                using (MpqArchive archive = new MpqArchive(fileinfo.FullName, FileAccess.Read))
                {
                    using (MpqFileStream file = archive.OpenFile("(listfile)"))
                    using (StreamReader sr = new StreamReader(file))
                    {
                        listFile = sr.ReadToEnd();
                        Console.WriteLine(listFile);
                    }
                    archive.ExtractFile("(listfile)", to + "\\" + fileinfo.Name.Replace(".MPQ", ".txt"));
                }
            }
        }

        public static Boolean FileExists(string filename)
        {
            bool exists;
            if (MainListFile.FindIndex(a => a.Contains(filename.ToLower())) != -1) // it returns -1 if the file is not found
            {
                exists = true;
            }
            else
            {
                exists = false;
            }
            return exists;
        }

    }
}
