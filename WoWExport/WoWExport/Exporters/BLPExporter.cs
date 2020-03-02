using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WoWFormatLib.FileReaders;

namespace Exporters
{
    public class BLPExporter
    {
        public static void ExportBLP(string filename, string outdir)
        {
            BLPReader reader = new BLPReader();
            if (!File.Exists(Path.Combine(outdir, filename.Replace(".blp", ".png"))))
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename))))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(filename)));
                    }
                    reader.LoadBLP(Managers.ArchiveManager.ReadThisFile(filename));
                    reader.bmp.Save(Path.Combine(outdir, filename.Replace(".blp", ".png")));
                }
            }
        }
    }
}
