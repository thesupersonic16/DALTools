using DALLib.IO;
using DALLib.Misc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    // Reader class for STSCFileDatabase
    public partial class STSCFileDatabase
    {

        public override void Load(ExtendedBinaryReader fileReader)
        {
            // Read STSC
            base.Load(fileReader);

            StreamBlock block;

            // System text
            block = GetStreamBlockAndJump(0, fileReader);
            while (!EndOfBlock(fileReader, block))
                SystemText.Add(fileReader.ReadStringElsewhere());

            // CGs
            block = GetStreamBlockAndJump(1, fileReader);
            while (!EndOfBlock(fileReader, block))
                CGs.Add(fileReader.ReadStruct<CGEntry>());

            // Movies
            block = GetStreamBlockAndJump(2, fileReader);
            while (!EndOfBlock(fileReader, block))
                Movies.Add(fileReader.ReadStruct<MovieEntry>());
            
            // Memories
            block = GetStreamBlockAndJump(3, fileReader);
            while (!EndOfBlock(fileReader, block))
                Memories.Add(fileReader.ReadStruct<MemoryEntry>());
            
            // Characters
            block = GetStreamBlockAndJump(4, fileReader);
            while (!EndOfBlock(fileReader, block))
                Characters.Add(fileReader.ReadStruct<CharacterEntry>());
            
            // Unknown2
            block = GetStreamBlockAndJump(5, fileReader);
            while (!EndOfBlock(fileReader, block))
                Unknown2.Add(fileReader.ReadStruct<Unknown2Entry>());
            
            // Unknown3
            block = GetStreamBlockAndJump(6, fileReader);
            while (!EndOfBlock(fileReader, block))
                Unknown3.Add(fileReader.ReadStruct<Unknown3Entry>());
            
            // Voices
            block = GetStreamBlockAndJump(7, fileReader);
            while (!EndOfBlock(fileReader, block))
                Voices.Add(fileReader.ReadStruct<VoiceEntry>());
            
            // Unknown4
            block = GetStreamBlockAndJump(8, fileReader);
            while (!EndOfBlock(fileReader, block))
                Unknown4.Add(fileReader.ReadStruct<Unknown4Entry>());
            
            // Art Book Page
            block = GetStreamBlockAndJump(9, fileReader);
            while (!EndOfBlock(fileReader, block))
                ArtBookPages.Add(fileReader.ReadStruct<ArtBookPageEntry>());
            
            // DramaCDs
            block = GetStreamBlockAndJump(10, fileReader);
            while (!EndOfBlock(fileReader, block))
                DramaCDs.Add(fileReader.ReadStruct<DramaCDEntry>());

        }

        /// <summary>
        /// Gets the StreamBlock from instruction and jumps to it
        /// </summary>
        /// <param name="index">Database Param Index</param>
        /// <param name="reader">The reader used to jump to the block</param>
        /// <returns>a StreamBlock containing the range of data to access</returns>
        public StreamBlock GetStreamBlockAndJump(int index, ExtendedBinaryReader reader)
        {
            var streamBlock = Instructions[index].GetArgument<StreamBlock>(1);
            reader.JumpTo(streamBlock);
            return streamBlock;
        }

        /// <summary>
        /// Helper for checking if we are at the end of the stream block
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool EndOfBlock(ExtendedBinaryReader reader, StreamBlock streamBlock)
        {
            return reader.BaseStream.Position >= streamBlock.Length;
        }

    }
}
