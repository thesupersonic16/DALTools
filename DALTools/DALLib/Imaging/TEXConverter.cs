using DALLib.Exceptions;
using DALLib.File;
using DALLib.IO;
using Scarlet.Drawing;
using Scarlet.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DALLib.File.TEXFile;

namespace DALLib.Imaging
{
    /// <summary>
    /// Class containing functions for decoding data from textures
    /// </summary>
    public static class TEXConverter
    {
        public static void Decode(this TEXFile file, Format format, ExtendedBinaryReader reader)
        {
            Decode(file, format, LoaderType.Default, reader);
        }

        public static void Decode(this TEXFile file, Format format, LoaderType loader, ExtendedBinaryReader reader)
        {
            ImageBinary image;
            var endian = file.UseBigEndian ? Endian.BigEndian : Endian.LittleEndian;
            
            if ((format & Format.PNG) != 0 || (loader & LoaderType.PNG) != 0)
            {
                using (var endStream = new MemoryStream())
                using (var stream = new MemoryStream(file.SheetData))
                {
                    var imagePNG = new Bitmap(Image.FromStream(stream));
                    var bitmap = imagePNG.LockBits(new Rectangle(0, 0, file.SheetWidth, file.SheetHeight), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    file.SheetData = new byte[file.SheetWidth * file.SheetHeight * 4];
                    Marshal.Copy(bitmap.Scan0, file.SheetData, 0, file.SheetWidth * file.SheetHeight * 4);
                    // Flip Red and Blue channels as this converter does not support ARGB
                    if (file.UseBigEndian || (loader & LoaderType.PNG) != 0)
                        ImageTools.FlipColors(file.SheetData);
                    imagePNG.UnlockBits(bitmap);
                    imagePNG.Dispose();
                }
            }
            else if ((format & Format.DXT1) != 0)
            {
                image = new ImageBinary(file.SheetWidth, file.SheetHeight, PixelDataFormat.FormatDXT1Rgba,
                    endian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, file.SheetData);
                file.SheetData = image.GetOutputPixelData(0);
            }
            else if ((format & Format.DXT5) != 0 || (format & Format.Large) != 0)
            {
                image = new ImageBinary(file.SheetWidth, file.SheetHeight, PixelDataFormat.FormatDXT5,
                    endian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, file.SheetData);
                file.SheetData = image.GetOutputPixelData(0);
            }
            else if ((format & Format.Luminance8) != 0)
            {
                image = new ImageBinary(file.SheetWidth, file.SheetHeight, PixelDataFormat.FormatLuminance8,
                    endian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, file.SheetData);
                file.SheetData = image.GetOutputPixelData(0);
            }
            else if ((format & Format.Luminance4) != 0)
            {
                image = new ImageBinary(file.SheetWidth, file.SheetHeight, PixelDataFormat.FormatLuminance4,
                    endian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, file.SheetData);
                file.SheetData = image.GetOutputPixelData(0);
            }
            else if ((format & Format.Unknown) != 0)
            {
                throw new InvalidTextureFormatException((int)format);
            }
            else if ((format & Format.Raster) != 0)
            {
                // Read Colour Palette
                reader.JumpBehind(256 * 4);
                var colorpalette = reader.ReadBytes(256 * 4);

                var indies = file.SheetData;
                file.SheetData = new byte[file.SheetWidth * file.SheetHeight * 4];
                for (int i = 0; i < file.SheetWidth * file.SheetHeight; ++i)
                {
                    file.SheetData[i * 4 + 0] = colorpalette[indies[i] * 4 + 0];
                    file.SheetData[i * 4 + 1] = colorpalette[indies[i] * 4 + 1];
                    file.SheetData[i * 4 + 2] = colorpalette[indies[i] * 4 + 2];
                    file.SheetData[i * 4 + 3] = colorpalette[indies[i] * 4 + 3];
                }
            }
            else if ((format & Format.BGRA) != 0)
            {
                return;
            }
            else
            {
                throw new InvalidTextureFormatException((int)format);
            }
        }
    }
}
