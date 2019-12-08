using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Silicon.Helpers
{
    static class ImageHelper
    {
        public static Stream Invert(Stream stream)
        {
            var image = Image.FromStream(stream);
            using var map = new Bitmap(image);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    Color col = map.GetPixel(x, y);
                    map.SetPixel(x, y, Color.FromArgb(255 - col.R, 255 - col.G, 255 - col.B));
                }
            }
            var result = new MemoryStream();
            map.Save(result, ImageFormat.Png);
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}
