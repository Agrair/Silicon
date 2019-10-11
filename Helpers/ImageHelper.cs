using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace Silicon.Helpers
{
    static class ImageHelper
    {
#pragma warning disable IDE0052 // Remove unread private members
        private static readonly Timer clearTimer = new Timer(_ =>
        {
            Directory.Delete("temp", true);
            Directory.CreateDirectory("temp");
        }, null, TimeSpan.FromSeconds(0), TimeSpan.FromMinutes(60));
#pragma warning restore IDE0052 // Remove unread private members

        public static Stream Invert(Stream stream, string name)
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
            var path = $"temp/inverted_{name}";
            if (File.Exists(path)) File.Delete(path);
            map.Save(path);
            return File.OpenRead(path);
        }
    }
}
