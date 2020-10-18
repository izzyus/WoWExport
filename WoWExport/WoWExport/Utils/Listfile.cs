using CASCLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;

namespace WoWExport
{
    public static class Listfile
    {
        public static Dictionary<uint, string> FDIDToFilename = new Dictionary<uint, string>();
        public static Dictionary<string, uint> FilenameToFDID = new Dictionary<string, uint>();

        public static void Update()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = new MemoryStream())
                {
                    client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                    var responseStream = new GZipStream(client.OpenRead("https://wow.tools/casc/listfile/download/csv/unverified"), CompressionMode.Decompress);
                    responseStream.CopyTo(stream);

                    if (!Directory.Exists(Path.GetDirectoryName(Managers.ArchiveManager.listFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(Managers.ArchiveManager.listFilePath));
                    }
                    File.WriteAllBytes(Managers.ArchiveManager.listFilePath, stream.ToArray());

                    responseStream.Close();
                    responseStream.Dispose();
                }
            }
            catch (Exception e)
            {
                throw new Exception("A fatal error occured during downloading the listfile.\n\n" + e.Message + "\n\nPlease provide one manually to: \n." + Path.GetDirectoryName(Managers.ArchiveManager.listFilePath));
            }
        }

        public static void Load()
        {
            if (!File.Exists(Managers.ArchiveManager.listFilePath))
            {
                Update();
            }

            using (var listfile = File.Open(Managers.ArchiveManager.listFilePath, FileMode.Open))
            using (var reader = new StreamReader(listfile))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Split(';');

                    if (tokens.Length != 2)
                    {
                        Logger.WriteLine($"Invalid line in listfile: {line}");
                        continue;
                    }

                    if (!uint.TryParse(tokens[0], out uint fileDataId))
                    {
                        Logger.WriteLine($"Invalid line in listfile: {line}");
                        continue;
                    }

                    FDIDToFilename.Add(fileDataId, tokens[1]);

                    if (!FilenameToFDID.ContainsKey(tokens[1]))
                    {
                        FilenameToFDID.Add(tokens[1], fileDataId);
                    }
                }
            }
        }

        public static bool TryGetFileDataID(string filename, out uint fileDataID)
        {
            var cleaned = filename.ToLower().Replace('\\', '/');
            return FilenameToFDID.TryGetValue(cleaned, out fileDataID);
        }

        public static bool TryGetFilename(uint filedataid, out string filename)
        {
            return FDIDToFilename.TryGetValue(filedataid, out filename);
        }
    }
}