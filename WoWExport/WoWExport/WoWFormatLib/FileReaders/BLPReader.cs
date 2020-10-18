using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using WoWFormatLib.SereniaBLPLib;

namespace WoWFormatLib.FileReaders
{
    public class BLPReader
    {
        public Bitmap bmp;

        public MemoryStream asBitmapStream()
        {
            MemoryStream bitmapstream = new MemoryStream();
            bmp.Save(bitmapstream, ImageFormat.Bmp);
            return bitmapstream;
        }
        public void LoadBLP(Stream filename)
        {
            var blp = new BlpFile(filename);
            {
                bmp = blp.GetBitmap(0);
            }
        }
    }
}