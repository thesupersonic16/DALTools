using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TEXTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var tex = new TEXFile();

            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  TEXTool [Switchs] [-o {path}] {tex file}");
                Console.WriteLine("  Switches: ");
                Console.WriteLine("    -p      Exports all frames (Output must be a directory)");
                Console.WriteLine("    -s      Exports Sheet as PNG");
                Console.WriteLine("    -f {id} Exports a single frame");
                Console.ReadKey(true);
                return;
            }

            if (args.Length > 1)
            {
                tex.Load(args[args.Length - 1]);
                string path = "";
                for (int i = args.Length - 1; i >= 0; --i)
                {
                    if (args[i].StartsWith("-") && args[i].Length > 1)
                    {
                        switch (args[i][1])
                        {
                            case 'o':
                                path = args[i + 1];
                                break;
                            case 'p':
                                for (int ii = 0; ii < tex.Frames.Count; ++ii)
                                    SaveFrame(tex, ii, Path.Combine(path, "frame" + i + ".png"));
                                break;
                            case 's':
                                tex.SaveImage(path);
                                break;
                            case 'f':
                                SaveFrame(tex, int.Parse(args[i + 1]), path);
                                break;
                        }
                    }
                }
            }
            else
            {
                tex.Load(args[0]);
                tex.SaveImage(Path.ChangeExtension(args[0], ".png"));
            }
        }

        public static void SaveFrame(TEXFile tex, int index, string filename)
        {
            var frame = tex.Frames[index];
            int l = (int)Math.Round((tex.SheetWidth)  * frame.LeftScale);
            int t = (int)Math.Round((tex.SheetHeight) * frame.TopScale);
            int r = (int)Math.Round((tex.SheetWidth)  * frame.RightScale);
            int b = (int)Math.Round((tex.SheetHeight) * frame.BottomScale);
            int w = (int)Math.Round((tex.SheetWidth)  * (frame.RightScale -  frame.LeftScale));
            int h = (int)Math.Round((tex.SheetHeight) * (frame.BottomScale - frame.TopScale));
            byte[] pixels = new byte[w * h * 4];

            for (int x = l; x < r; ++x)
            {
                for (int y = t; y < b; ++y)
                {
                    pixels[((y - t) * w + (x - l)) * 4 + 0] = tex.SheetPixels[(y * w + x) * 4 + 0];
                    pixels[((y - t) * w + (x - l)) * 4 + 1] = tex.SheetPixels[(y * w + x) * 4 + 1];
                    pixels[((y - t) * w + (x - l)) * 4 + 2] = tex.SheetPixels[(y * w + x) * 4 + 2];
                    pixels[((y - t) * w + (x - l)) * 4 + 3] = tex.SheetPixels[(y * w + x) * 4 + 3];
                }
            }
            SaveImage(filename, w, h, pixels);
        }

        public static void SaveImage(string path, int width, int height, byte[] pixels)
        {
            var Image = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, Image.PixelFormat);
            int bitmapDataSize = bitmap.Stride * bitmap.Height;
            byte[] buffer = new byte[4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                buffer[0] = pixels[i + 0];
                buffer[1] = pixels[i + 1];
                buffer[2] = pixels[i + 2];
                buffer[3] = pixels[i + 3];
                pixels[i + 0] = buffer[2];
                pixels[i + 1] = buffer[1];
                pixels[i + 2] = buffer[0];
                pixels[i + 3] = buffer[3];
            }

            Marshal.Copy(pixels, 0, bitmap.Scan0, width * height * 4);
            Image.UnlockBits(bitmap);
            Image.Save(path);
        }
    }
}
