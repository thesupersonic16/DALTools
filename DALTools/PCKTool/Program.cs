using DALLib.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCKTool
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  PCKTool {filePath(s)/Directory}");
                Console.WriteLine("  Switches: ");
                Console.WriteLine("    -s           Build all archives using small signatures");
                Console.WriteLine("    -e           Read/Write in Big Endian (PS3) Default is Little Endian (PC/PS4)");
                Console.ReadKey(true);
                return;
            }

            bool useSmallSig = false;
            bool useBigEndian = false;

            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i].StartsWith("-") && args[i].Length > 1)
                {
                    switch (args[i][1])
                    {
                        case 's': // Use smallSigs
                            useSmallSig = true;
                            break;
                        case 'e': // Use Big Endian
                            useBigEndian = true;
                            break;
                        default:
                            break;
                    }
                    continue;
                }

                using (var arc = new PCKFile { UseSmallSig = useSmallSig, UseBigEndian = useBigEndian })
                {
                    var attr = File.GetAttributes(args[i]);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        arc.AddAllFiles(args[i]);
                        arc.Save(args[i] + ".pck");
                    }
                    else
                    {
                        arc.Load(args[i], true);
                        arc.ExtractAllFiles(Path.GetFileNameWithoutExtension(args[i]));
                    }
                }
            }
        }
    }
}