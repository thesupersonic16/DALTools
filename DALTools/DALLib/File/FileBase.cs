using DALLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    /// <summary>
    /// A base class for storing abstract functions of each parser
    /// </summary>
    public class FileBase
    {

        /// <summary>
        /// Should the file be read/written in Big Endian (PS3)
        /// <para/>
        /// Default is false as DALLib is intended to be used on Little Endian (PC/PS4) games
        /// </summary>
        public bool UseBigEndian = false;

        /// <summary>
        /// Load file from disk (At the end this calls Load(Stream))
        /// </summary>
        /// <param name="path">Path to the file to load</param>
        /// <param name="keepOpen">Should the file stream be kept open?</param>
        public virtual void Load(string path, bool keepOpen = false)
        {
            if (keepOpen)
                Load(System.IO.File.OpenRead(path));
            else
            {
                using (var stream = System.IO.File.OpenRead(path))
                {
                    Load(stream);
                }
            }
        }

        /// <summary>
        /// Load file from Stream
        /// </summary>
        /// <param name="stream">The stream to read data from</param>
        /// <param name="autoDecompress">Automatically check for full file compression</param>
        public virtual void Load(Stream stream, bool autoDecompress = true)
        {
            var reader = new ExtendedBinaryReader(stream);
            if (autoDecompress)
            {
                // Decompress Zlib stream
                if (reader.PeekSignature() == "ZLIB")
                {
                    // Skip ZLIB Header
                    reader.JumpAhead(14);
                    // Decompress stream
                    using (var deflate = new DeflateStream(reader.BaseStream, CompressionMode.Decompress, false))
                        reader.SetStream(deflate.CacheStream());
                    // Set Endianness of the reader
                    reader.SetEndian(UseBigEndian);
                    // Parse file
                    Load(reader);
                    return;
                }
            }
            // Set Endianness of the reader
            reader.SetEndian(UseBigEndian);
            // Parse File
            Load(reader);
        }

        /// <summary>
        /// Load file using an ExtendedBinaryReader
        /// </summary>
        /// <param name="reader">The reader the parser is to use</param>
        public virtual void Load(ExtendedBinaryReader reader)
        {
        }

        /// <summary>
        /// Save file to disk (At the end this calls Save(Stream))
        /// </summary>
        /// <param name="path">Path to where you want the file to be saved on disk</param>
        /// <param name="keepOpen">Should the file stream be kept open?</param>
        public virtual void Save(string path, bool keepOpen = false)
        {
            if (keepOpen)
                Save(System.IO.File.Create(path));
            else
            {
                using (var stream = System.IO.File.Create(path))
                {
                    Save(stream);
                }
            }
        }

        /// <summary>
        /// Save file to a Stream
        /// </summary>
        /// <param name="stream">The stream to write data to</param>
        public virtual void Save(Stream stream)
        {
            Save(new ExtendedBinaryWriter(stream));
        }

        /// <summary>
        /// Save file using an ExtendedBinaryWriter
        /// </summary>
        /// <param name="writer">The writer the parser is to use</param>
        public virtual void Save(ExtendedBinaryWriter writer)
        {
        }

    }
}
