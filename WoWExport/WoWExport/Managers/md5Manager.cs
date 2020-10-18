using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Managers
{
    class md5Manager
    {
        public static List<String> lines = new List<String>();

        public static void LoadMD5()
        {
            using (StreamReader reader = new StreamReader(ArchiveManager.ReadThisFile(@"textures\minimap\md5translate.trs")))
            {
                string line;
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    if (!line.StartsWith("dir:")) //skip "dir: ..." entry
                    {
                        //lines.Add(line);
                        lines.Add(line.ToLower());
                    }
                }
            }
        }

        public static void ExportAllMinimaps(string to)
        {
            WoWFormatLib.FileReaders.BLPReader minireader = new WoWFormatLib.FileReaders.BLPReader();
            for (int i = 0; i < lines.Count(); i++)
            {
                string file = lines[i].Substring(0, lines[i].IndexOf("\t")); //path + filenames
                string hash = lines[i].Substring(lines[i].IndexOf("\t") + 1, lines[i].Length - lines[i].IndexOf("\t") - 1); //filehash
                try
                {
                    minireader.LoadBLP(ArchiveManager.ReadThisFile(@"textures\minimap\" + hash));
                    if (!Directory.Exists(Path.Combine(to + "\\", Path.GetDirectoryName(file))))
                    {
                        Directory.CreateDirectory(Path.Combine(to + "\\", Path.GetDirectoryName(file)));
                    }
                    minireader.bmp.Save(Path.Combine(to + "\\", Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file) + ".png"));
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public static void ExportThisFolder(string path, string to)
        {
            WoWFormatLib.FileReaders.BLPReader minireader = new WoWFormatLib.FileReaders.BLPReader();
            for (int i = 0; i < lines.Count(); i++)
            {
                string file = lines[i].Substring(0, lines[i].IndexOf("\t")); //path + filenames
                string hash = lines[i].Substring(lines[i].IndexOf("\t") + 1, lines[i].Length - lines[i].IndexOf("\t") - 1); //filehash

                if (file.StartsWith(path.ToLower() + "\\"))
                {
                    try
                    {
                        minireader.LoadBLP(ArchiveManager.ReadThisFile(@"textures\minimap\" + hash));
                        if (!Directory.Exists(Path.Combine(to + "\\", Path.GetDirectoryName(file))))
                        {
                            Directory.CreateDirectory(Path.Combine(to + "\\", Path.GetDirectoryName(file)));
                        }
                        minireader.bmp.Save(Path.Combine(to + "\\", Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file) + ".png"));
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }
        }

        public static String TranslateThisMap(string filename)
        {
            string filedirectory = Path.GetDirectoryName(filename);
            filedirectory = filedirectory.Substring(filedirectory.LastIndexOf("\\") + 1, filedirectory.Length - filedirectory.LastIndexOf("\\") - 1);

            filename = Path.GetFileNameWithoutExtension(filename.ToLower());
            filename = filename.Replace(filename.Substring(0, filename.IndexOf("_") + 1), "map");
            int index = lines.FindIndex(a => a.Contains(filedirectory + "\\" + filename));

            if (index != -1)
            {
                string hash = lines[index].Substring(lines[index].IndexOf("\t") + 1, lines[index].Length - lines[index].IndexOf("\t") - 1); //filehash
                return @"textures\minimap\" + hash;
            }
            else
            {
                return null;
            }
        }
    }
}