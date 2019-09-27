using DALLib.File;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Imaging
{
    public static class ImageTools
    {

        /// <summary>
        /// Flips the Red and Blue channels in the byte array
        /// </summary>
        /// <param name="array"></param>
        public static void FlipColors(byte[] array)
        {
            for (int i = 0; i < array.Length; i += 4)
            {
                array[i + 0] = array[i + 2];
                array[i + 2] = array[i + 0];
            }
        }

        /// <summary>
        /// Flips the Red and Blue channels in the byte array
        /// </summary>
        /// <param name="array"></param>
        /// <returns>An array of the colours flipped</returns>
        public static byte[] FlipColorsCopy(byte[] array)
        {
            byte[] newArray = new byte[array.Length];
            Array.Copy(array, newArray, array.Length);
            for (int i = 0; i < array.Length; i += 4)
            {
                newArray[i + 0] = array[i + 2];
                newArray[i + 2] = array[i + 0];
            }
            return newArray;
        }


        /// <summary>
        /// Saves an RGBA byte array to an image file
        /// </summary>
        /// <param name="path">Path to save the file to</param>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="bytes">array containing the RGBA data</param>
        public static void SaveImage(string path, int width, int height, byte[] bytes)
        {
            var Image = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = Image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, Image.PixelFormat);
            Marshal.Copy(FlipColorsCopy(bytes), 0, bitmap.Scan0, width * height * 4);
            Image.UnlockBits(bitmap);
            Image.Save(path);
        }

        /// <summary>
        /// Loads an image file and writes its colours into the data array
        /// </summary>
        /// <param name="path">Path to the image file to be loaded</param>
        /// <param name="width">Reference of the width of the loaded image</param>
        /// <param name="height">Reference of the height of the loaded image</param>
        /// <param name="data">Reference of the array of colours in RGBA format</param>
        public static void LoadImage(string path, ref int width, ref int height, ref byte[] data)
        {
            var image = new Bitmap(path);
            width = image.Width;
            height = image.Height;
            data = new byte[width * height * 4];
            var bitmap = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(bitmap.Scan0, data, 0, width * height * 4);
            image.UnlockBits(bitmap);
            FlipColors(data);
        }

        /// <summary>
        /// Creates a Bitmap from a TEXFile sheet
        /// </summary>
        /// <param name="file">TEXFile to load the image data from</param>
        /// <returns>The converted Bitmap</returns>
        public static Bitmap CreateBitmap(this TEXFile file)
        {
            var image = new Bitmap(file.SheetWidth, file.SheetHeight, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = image.LockBits(new Rectangle(0, 0, file.SheetWidth, file.SheetHeight), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(FlipColorsCopy(file.SheetData), 0, bitmap.Scan0, file.SheetWidth * file.SheetHeight * 4);
            image.UnlockBits(bitmap);
            return image;
        }

        /// <summary>
        /// Creates a Bitmap from a TEXFile frame
        /// </summary>
        /// <param name="file">TEXFile to load the image data from</param>
        /// <param name="frame">The ID of the frame to use</param>
        /// <returns>The converted Bitmap</returns>
        public static Bitmap CreateBitmapFromFrame(this TEXFile file, int frame)
        {
            int x = (int)(file.Frames[frame].LeftScale * file.SheetWidth);
            int y = (int)(file.Frames[frame].TopScale * file.SheetHeight);
            int width = (int)file.Frames[frame].FrameWidth;
            int height = (int)file.Frames[frame].FrameHeight;

            return CreateBitmap(file, x, y, width, height);
        }

        /// <summary>
        /// Creates a Bitmap from a TEXFile region
        /// </summary>
        /// <param name="file">TEXFile to load the image data from</param>
        /// <param name="x">The left(X) position of the region</param>
        /// <param name="y">The top(Y) position of the region</param>
        /// <param name="width">The width of the region</param>
        /// <param name="height">The height of the region</param>
        /// <returns>The converted Bitmap</returns>
        public static Bitmap CreateBitmap(this TEXFile file, int x, int y, int width, int height)
        {
            var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, image.PixelFormat);
            Marshal.Copy(file.SplitImage(x, y, width, height), 0, bitmap.Scan0, width * height * 4);
            image.UnlockBits(bitmap);
            return image;
        }

        /// <summary>
        /// Splits the image from a TEXFile region
        /// </summary>
        /// <param name="file">TEXFile to load the image data from</param>
        /// <param name="x">The left(X) position of the region</param>
        /// <param name="y">The top(Y) position of the region</param>
        /// <param name="width">The width of the region</param>
        /// <param name="height">The height of the region</param>
        /// <returns>Array of colours</returns>
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


    }
}
