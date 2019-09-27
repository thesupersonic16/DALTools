using DALLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
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
        public virtual void Load(Stream stream)
        {
            Load(new ExtendedBinaryReader(stream));
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
