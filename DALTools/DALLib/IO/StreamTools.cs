using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.IO
{
    public static class StreamTools
    {
        public static int BLOCKSIZE = 32;

        /// <summary>
        /// Copies all data from the input stream to a new MemoryStream
        /// <para/>
        /// Note: This does not reset the position of the stream
        /// </summary>
        /// <param name="stream">The stream to read all the data from</param>
        /// <returns>A MemoryStream containing the data read</returns>
        public static MemoryStream CacheStream(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            byte[] buf = new byte[BLOCKSIZE];
            while (true)
            {
                int read = stream.Read(buf, 0, BLOCKSIZE);
                memoryStream.Write(buf, 0, read);
                if (read == 0)
                    break;
            }
            // Reset position
            memoryStream.Position = 0;
            // Return the buffered stream
            return memoryStream;
        }

        /// <summary>
        /// Copies all data from one stream and compresses it and writes it in the output stream
        /// <para/>
        /// Note: A seekable stream is required for this method to work!
        /// </summary>
        /// <param name="inputStream">Stream to read the data from</param>
        /// <param name="outputStream">Stream to write the compressed data to (Stream is kept open after this function has been called)</param>
        /// <returns>The compressed size</returns>
        public static long DeflateCompress(this Stream inputStream, Stream outputStream)
        {
            // Stores current position of the output stream
            long currentPosition = outputStream.Position;
            // Creates the stream for deflating
            var deflateStream = new DeflateStream(outputStream, CompressionLevel.Optimal, true);
            // Resets the input stream so we can start reading from it
            inputStream.Position = 0;
            // Copies all the data to the deflate stream and writes the compressed data to the output stream
            inputStream.CopyTo(deflateStream);
            // Closes and Disposes the stream 
            deflateStream.Close();
            // Returns the size of the compressed data
            return outputStream.Position - currentPosition;
        }

        /// <summary>
        /// Writes a ZLIB header to the main stream from the writer and swaps the writer stream to a buffer
        /// </summary>
        /// <param name="writer">Writer to write the header to and to swap to the buffer</param>
        /// <returns>The main stream (the previous stream)</returns>
        public static Stream StartDeflateEncapsulation(this ExtendedBinaryWriter writer)
        {
            // Store the main stream
            var mainStream = writer.BaseStream;
            // Write ZLIB Header
            writer.WriteSignature("ZLIB");
            writer.AddOffset("UncompressedSize");
            writer.AddOffset("CompressedSize");
            // Write ZLIB flags
            writer.Write((byte)0x78);
            writer.Write((byte)0xDA);
            // Set stream to buffer
            writer.SetStream(new MemoryStream());
            // Return the main stream as we will be needing it
            return mainStream;
        }

        /// <summary>
        /// Starts compressing the data into the main stream, fills in the zlib header information and then returns the main stream back to the writer
        /// <para/>
        /// Note: This must only be called when <see cref="StartDeflateEncapsulation"/> is used
        /// </summary>
        /// <param name="writer">The writer to work with</param>
        /// <param name="mainStream">The main stream which will contain the compressed data</param>
        public static void EndDeflateEncapsulation(this ExtendedBinaryWriter writer, Stream mainStream)
        {
            // Record the amount of uncompressed data
            uint uncompressedSize = (uint)writer.BaseStream.Position;
            // Compress the data into the main stream and store the size
            uint compressedSize = (uint)writer.BaseStream.DeflateCompress(mainStream);
            // Return the main stream back to the writer
            writer.SetStream(mainStream);
            // Fill in the sizes into the header
            writer.FillInOffset("UncompressedSize", uncompressedSize);
            writer.FillInOffset("CompressedSize", compressedSize);
        }
    }
}
