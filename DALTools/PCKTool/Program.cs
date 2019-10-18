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
                ShowHelp("Not Enough Arguments!");
                return;
            }

            bool useSmallSig = false;
            bool useBigEndian = false;
            string filePath = "";

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
                else
                    filePath = args[i];
            }
            if (!string.IsNullOrEmpty(filePath))
            {
                using (var arc = new PCKFile { UseSmallSig = useSmallSig, UseBigEndian = useBigEndian })
                {
                    var attr = File.GetAttributes(filePath);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        arc.AddAllFiles(filePath);
                        arc.Save(filePath + ".pck");
                    }
                    else
                    {
                        arc.Load(filePath, true);
                        arc.ExtractAllFiles(Path.GetFileNameWithoutExtension(filePath));
                    }
                }
            }
            else
                ShowHelp("No Path was Given!");

        }

        public static void ShowHelp(string error = "")
        {
            if (!string.IsNullOrEmpty(error))
                Console.WriteLine("Error: {0}", error);
            Console.WriteLine("  PCKTool [Switches] {filePath(s)/Directory}");
            Console.WriteLine("  Switches: ");
            Console.WriteLine("    -s           Build all archives using small signatures");
            Console.WriteLine("    -e           Read/Write in Big Endian (PS3) Default is Little Endian (PC/PS4)");
            Console.ReadKey(true);
        }
    }
}