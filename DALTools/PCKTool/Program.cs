using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.Archives;

namespace PCKTool
{
    class Program
    {
        public static void Main(string[] args)
        {
            PCKArchive arc = new PCKArchive();
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  PCKTool {filePath/Directory}");
                Console.ReadKey(true);
                return;
            }
            var attr = File.GetAttributes(args[0]);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                arc.Data.AddRange(GetFilesFromDir(args[0], true));
                arc.Save(args[0] + ".pck", true);
            }
            else
            {
                arc.Load(args[0]);
                arc.Extract(Path.GetFileNameWithoutExtension(args[0]));
            }
        }


        // Workaround for a bug in HedgeLib that does not set dir names
        public static List<ArchiveData> GetFilesFromDir(string dir,
            bool includeSubDirectories = false)
        {
            // Add each file in the current sub-directory
            var data = new List<ArchiveData>();
            foreach (string filePath in Directory.GetFiles(dir))
            {
                data.Add(new ArchiveFile(filePath));
            }

            // Repeat for each sub directory
            if (includeSubDirectories)
            {
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    data.Add(new ArchiveDirectory()
                    {
                        Data = GetFilesFromDir(subDir, includeSubDirectories),
                        Name = Path.GetFileName(subDir)
                    });
                }
            }

            return data;
        }

    }
}