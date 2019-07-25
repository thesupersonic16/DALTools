using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEXTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var tex = new TEXFile();

            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  TEXTool {tex file}");
                Console.ReadKey(true);
                return;
            }
            tex.Load(args[0]);
            tex.SaveImage(Path.ChangeExtension(args[0], ".png"));
        }
    }
}
