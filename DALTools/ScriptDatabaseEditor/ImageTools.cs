using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using TEXTool;

namespace ScriptDatabaseEditor
{
    public static class ImageTools
    {
        public static BitmapSource ConvertToSource(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public static Bitmap GetBitmap(this TEXFile file)
        {
            var image = new Bitmap(file.SheetWidth, file.SheetHeight, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = image.LockBits(new Rectangle(0, 0, file.SheetWidth, file.SheetHeight), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(TEXFile.FlipColorsNew(file.SheetData), 0, bitmap.Scan0, file.SheetWidth * file.SheetHeight * 4);
            image.UnlockBits(bitmap);
            return image;
        }

        public static Bitmap GetBitmap(this TEXFile file, int x, int y, int width, int height)
        {
            var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(file.SplitImage(x, y, width, height), 0, bitmap.Scan0, width * height * 4);
            image.UnlockBits(bitmap);
            return image;
        }

        public static Bitmap GetBitmap(this TEXFile file, int frame)
        {
            int x = (int)(file.Frames[frame].LeftScale * file.SheetWidth);
            int y = (int)(file.Frames[frame].TopScale * file.SheetHeight);
            int width = (int)file.Frames[frame].FrameWidth;
            int height = (int)file.Frames[frame].FrameHeight;

            return GetBitmap(file, x, y, width, height);
        }

        public static byte[] SplitImage(this TEXFile tex, int x, int y, int width, int height)
        {
            var output = new byte[width * height * 4];
            for (int ix = 0; ix < width; ++ix)
            {
                for (int iy = 0; iy < height; ++iy)
                {
                    if ((y + iy) >= tex.SheetHeight)
                        continue;
                    if ((x + ix) >= tex.SheetWidth)
                        continue;
                    if ((x + ix) < 0)
                        continue;
                    if ((y + iy) < 0)
                        continue;
                    output[(ix + iy * width) * 4 + 0] = tex.SheetData[(x + ix + (y + iy) * tex.SheetWidth) * 4 + 2];
                    output[(ix + iy * width) * 4 + 1] = tex.SheetData[(x + ix + (y + iy) * tex.SheetWidth) * 4 + 1];
                    output[(ix + iy * width) * 4 + 2] = tex.SheetData[(x + ix + (y + iy) * tex.SheetWidth) * 4 + 0];
                    output[(ix + iy * width) * 4 + 3] = tex.SheetData[(x + ix + (y + iy) * tex.SheetWidth) * 4 + 3];
                }
            }
            return output;
        }

        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);
    }
}
