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
            PCKArchive arc = new PCKArchive();
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  PCKTool {filePath/Directory}");
                Console.ReadKey(true);
            }
            var attr = File.GetAttributes(args[0]);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                arc.AddDirectory(args[0]);
                arc.Save(args[0] + ".pck");
            }
            else
            {
                arc.Load(args[0]);
                arc.Extract(Path.GetFileNameWithoutExtension(args[0]));
            }
        }


    }
}