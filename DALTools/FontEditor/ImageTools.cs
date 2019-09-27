using DALLib.File;
using DALLib.Imaging;
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

namespace FontEditor
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

        // TODO: Test Font Spacing, Should be removed.
        public static Bitmap RenderFont(this FontFile file,  TEXFile texture, string text)
        {
            int width = 0;
            byte[][] fontPixels = new byte[text.Length][];

            for (int i = 0; i < text.Length; ++i)
            {
                var entry = file.Characters.FirstOrDefault(t => t.Character == text[i]);
                if (entry == null)
                    continue;
                width += entry.Width;
                if (entry.Character == 'p')
                    width += 0;
                float x = entry.XScale * texture.SheetWidth;
                float y = entry.YScale * texture.SheetHeight;
                fontPixels[i] = texture.SplitImage((int)x, (int)y, entry.Width + entry.Kerning, file.CharacterHeight);
            }

            byte[] finalBytes = new byte[width * file.CharacterHeight * 4];
            int xPosition = 0;
            for (int i = 0; i < fontPixels.Length; ++i)
            {
                var entry = file.Characters.FirstOrDefault(t => t.Character == text[i]);
                if (entry == null)
                    continue;
                if (fontPixels[i] == null)
                    continue;

                for (int xx = 0; xx < (entry.Width + entry.Kerning); ++xx)
                {
                    for (int yy = 0; yy < file.CharacterHeight; ++yy)
                    {
                        if (fontPixels[i][(xx + yy * (entry.Width + entry.Kerning)) * 4 + 3] == 0)
                            continue;
                        finalBytes[(xPosition - entry.Kerning + xx + yy * width) * 4 + 0] = fontPixels[i][(xx + yy * (entry.Width + entry.Kerning)) * 4 + 0];
                        finalBytes[(xPosition - entry.Kerning + xx + yy * width) * 4 + 1] = fontPixels[i][(xx + yy * (entry.Width + entry.Kerning)) * 4 + 1];
                        finalBytes[(xPosition - entry.Kerning + xx + yy * width) * 4 + 2] = fontPixels[i][(xx + yy * (entry.Width + entry.Kerning)) * 4 + 2];
                        finalBytes[(xPosition - entry.Kerning + xx + yy * width) * 4 + 3] = fontPixels[i][(xx + yy * (entry.Width + entry.Kerning)) * 4 + 3];
                    }
                }
                //xPosition -= entry.Kerning;
                xPosition += entry.Width;

            }

            if (width <= 0)
                return null;
            var image = new Bitmap(width, file.CharacterHeight, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = image.LockBits(new Rectangle(0, 0, width, file.CharacterHeight), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(finalBytes, 0, bitmap.Scan0, finalBytes.Length);
            image.UnlockBits(bitmap);
            return image;
        }

        [DllImport("gdi32")]
        public static extern int DeleteObject(IntPtr o);
    }
}
