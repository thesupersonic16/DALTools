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
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  PCKTool {filePath/Directory}");
                Console.WriteLine("  Switches: ");
                Console.WriteLine("    -s           Build all archives using small signatures");
                Console.ReadKey(true);
                return;
            }

            bool useSmallSig = false;

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("-") && args[i].Length > 1)
                {
                    switch (args[i][1])
                    {
                        case 's': // Use smallSigs
                            useSmallSig = true;
                            break;
                        default:
                            break;
                    }
                    continue;
                }
                PCKArchive arc = new PCKArchive();
                arc.UseSmallSig = useSmallSig;
                var attr = File.GetAttributes(args[i]);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    arc.Data.AddRange(GetFilesFromDir(args[i], true));
                    arc.Save(args[i] + ".pck", true);
                }
                else
                {
                    arc.Load(args[i]);
                    arc.Extract(Path.GetFileNameWithoutExtension(args[i]));
                }
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