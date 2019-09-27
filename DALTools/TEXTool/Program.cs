using DALLib;
using DALLib.File;
using DALLib.Imaging;
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
using System.Xml.Serialization;

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
                Console.WriteLine("  TEXTool [Switches] [-o {path}] {tex file}");
                Console.WriteLine("  Switches: ");
                Console.WriteLine("    -p                             Exports all frames");
                Console.WriteLine("    -s                             Exports Sheet as PNG");
                Console.WriteLine("    -i                             Exports frame information");
                Console.WriteLine("    -f {id}                        Exports a single frame");
                Console.WriteLine("    -b {sheet.png} {frame.xml}     Build TEX using sheet and FrameXML");
                Console.WriteLine("    -m {path}                      Build TEX using sheet and FrameXML (Search by name) (Recommended over -b)");
                Console.WriteLine("  Examples: ");
                Console.WriteLine("    TEXTool -p title.tex           Extracts all frames");
                Console.WriteLine("    TEXTool -s title.tex           Extracts sheet from TEX");
                Console.WriteLine("    TEXTool -i title.tex           Extracts frame information");
                Console.WriteLine("    TEXTool -b title.png title.xml Builds TEX using sheet");
                Console.WriteLine("    TEXTool -m title               Builds TEX using frames");
                Console.WriteLine("    TEXTool -f 0 title             Extracts the first frame");
                Console.WriteLine("    TEXTool -p -o frames title.tex Extracts all frames into a folder called frames");
                Console.ReadKey(true);
                return;
            }

            if (args.Length > 1)
            {
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
                                tex.Load(args[args.Length - 1]);
                                if (string.IsNullOrEmpty(path))
                                    path = RemoveExtension(args[args.Length - 1]);
                                Directory.CreateDirectory(path);
                                for (int ii = 0; ii < tex.Frames.Count; ++ii)
                                    SaveFrame(tex, ii, Path.Combine(path, $"frame{ii:d2}.png"));
                                break;
                            case 's':
                                tex.Load(args[args.Length - 1]);
                                if (string.IsNullOrEmpty(path))
                                    path = Path.ChangeExtension(args[args.Length - 1], ".png");
                                if (CheckForOverWrite(path))
                                    tex.SaveSheetImage(path);
                                break;
                            case 'i':
                                tex.Load(args[args.Length - 1]);
                                if (string.IsNullOrEmpty(path))
                                    path = Path.ChangeExtension(args[args.Length - 1], ".xml");
                                SaveAllFrameInformation(tex, path);
                                break;
                            case 'f':
                                tex.Load(args[args.Length - 1]);
                                if (CheckForOverWrite(path))
                                    SaveFrame(tex, int.Parse(args[i + 1]), path);
                                break;
                            case 'b':
                                BuildTEX(ref tex, args[i + 1], args[i + 2]);
                                if (string.IsNullOrEmpty(path))
                                    path = Path.ChangeExtension(args[args.Length - 1], ".tex");
                                if (CheckForOverWrite(path))
                                    tex.Save(path);
                                break;
                            case 'm':
                                BuildTEX(ref tex, args[i + 1]);
                                if (string.IsNullOrEmpty(path))
                                    path = Path.ChangeExtension(args[args.Length - 1], ".tex");
                                if (CheckForOverWrite(path))
                                    tex.Save(path);
                                break;
                        }
                    }
                }
            }
            else
            {
                if (Path.GetExtension(args[0]) == ".png" || Path.GetExtension(args[0]) == ".xml" || Directory.Exists(args[0]))
                {
                    Console.WriteLine("Building Files detected, Building...");
                    BuildTEX(ref tex, args[0]);
                    string path = Path.ChangeExtension(args[0], ".tex");
                    if (CheckForOverWrite(path))
                        tex.Save(path);
                }
                else
                {
                    tex.Load(args[0]);
                    tex.SaveSheetImage(Path.ChangeExtension(args[0], ".png"));
                    SaveAllFrameInformation(tex, Path.ChangeExtension(args[0], ".xml"));
                }
            }
        }

        public static bool CheckForOverWrite(string path)
        {
            if (File.Exists(path))
            {
                Console.Write("WARNING: You are about to overwrite a file. Continue? [Y/A] ");
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                    Console.WriteLine("Continue");
                else
                {
                    Console.WriteLine("Abort");
                    return false;
                }
            }

            return true;
        }

        public static string RemoveExtension(string path)
        {
            if (Path.HasExtension(path))
                return path.Substring(0, path.LastIndexOf('.'));
            return path;
        }

        public static void BuildTEX(ref TEXFile tex, string name)
        {
            // Remove Extension if it has one
            if (Path.HasExtension(name))
                name = RemoveExtension(name);
            var serializer = new XmlSerializer(typeof(TEXFile));
            if (File.Exists(name + ".xml"))
                using (var stream = File.OpenRead(name + ".xml"))
                    tex = serializer.Deserialize(stream) as TEXFile;
            else
                Console.WriteLine("WARNING: No Frame Data was found! Please Extract it using the -i switch.");
            if (Directory.Exists(name))
                BuildSheet(tex, Path.Combine(name, "frame{0:d2}.png"));
            else if (File.Exists(name + ".png"))
                tex.LoadSheetImage(name + ".png");
            else
                Console.WriteLine("WARNING: No Image was found! Please Extract it using the -s or -p switch.");
        }

        public static void BuildTEX(ref TEXFile tex, string sheetpath, string frameXML)
        {
            var serializer = new XmlSerializer(typeof(TEXFile));
            using (var stream = File.OpenRead(frameXML))
                tex = serializer.Deserialize(stream) as TEXFile;
            tex.LoadSheetImage(sheetpath);
        }

        public static void BuildSheet(TEXFile tex, string path)
        {
            byte[] pixels = new byte[tex.SheetWidth * tex.SheetHeight * 4];
            tex.SheetData = new byte[tex.SheetWidth * tex.SheetHeight * 4];
            for (int i = 0; i < tex.Frames.Count; ++i)
            {
                var frame = tex.Frames[i];
                var data = LoadImageBytes(string.Format(path, i));
                int l = (int)Math.Round((tex.SheetWidth) * frame.LeftScale);
                int t = (int)Math.Round((tex.SheetHeight) * frame.TopScale);
                int r = (int)Math.Round((tex.SheetWidth) * frame.RightScale);
                int b = (int)Math.Round((tex.SheetHeight) * frame.BottomScale);
                int w = (int)frame.FrameWidth;
                
                for (int x = l; x < r; ++x)
                {
                    for (int y = t; y < b; ++y)
                    {
                        tex.SheetData[(y * tex.SheetWidth + x) * 4 + 0] = data[((y - t) * w + (x - l)) * 4 + 0];
                        tex.SheetData[(y * tex.SheetWidth + x) * 4 + 1] = data[((y - t) * w + (x - l)) * 4 + 1];
                        tex.SheetData[(y * tex.SheetWidth + x) * 4 + 2] = data[((y - t) * w + (x - l)) * 4 + 2];
                        tex.SheetData[(y * tex.SheetWidth + x) * 4 + 3] = data[((y - t) * w + (x - l)) * 4 + 3];
                    }
                }
            }
            ImageTools.FlipColors(tex.SheetData);
        }

        public static byte[] LoadImageBytes(string path)
        {
            var image = new Bitmap(path);
            byte[] pixels = new byte[image.Width * image.Height * 4];
            var bitmap = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(bitmap.Scan0, pixels, 0, image.Width * image.Height * 4);
            image.UnlockBits(bitmap);
            return pixels;
        }

        public static void SaveAllFrameInformation(TEXFile tex, string filepath)
        {
            if (!Path.HasExtension(filepath))
                filepath += ".xml";
            var serializer = new XmlSerializer(typeof(TEXFile));
            using (var stream = File.Create(filepath))
                serializer.Serialize(stream, tex);
        }

        public static void SaveFrame(TEXFile tex, int index, string filename)
        {
            var frame = tex.Frames[index];
            int l = (int)Math.Round((tex.SheetWidth)  * frame.LeftScale);
            int t = (int)Math.Round((tex.SheetHeight) * frame.TopScale);
            int r = (int)Math.Round((tex.SheetWidth)  * frame.RightScale);
            int b = (int)Math.Round((tex.SheetHeight) * frame.BottomScale);
            int w = (int)frame.FrameWidth;
            int h = (int)frame.FrameHeight;
            byte[] pixels = new byte[w * h * 4];

            for (int x = l; x < r; ++x)
            {
                for (int y = t; y < b; ++y)
                {
                    pixels[((y - t) * w + (x - l)) * 4 + 0] = tex.SheetData[(y * tex.SheetWidth + x) * 4 + 0];
                    pixels[((y - t) * w + (x - l)) * 4 + 1] = tex.SheetData[(y * tex.SheetWidth + x) * 4 + 1];
                    pixels[((y - t) * w + (x - l)) * 4 + 2] = tex.SheetData[(y * tex.SheetWidth + x) * 4 + 2];
                    pixels[((y - t) * w + (x - l)) * 4 + 3] = tex.SheetData[(y * tex.SheetWidth + x) * 4 + 3];
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
