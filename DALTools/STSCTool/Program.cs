using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace STSCTool
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Error: Not Enough Arguments!");
                Console.WriteLine("  STSCTool {.bin/.txt}");
                Console.ReadKey(true);
                return;
            }
            STSCFile file = new STSCFile();
            for (int i = 0; i < args.Length; ++i)
            {
                if (Path.GetExtension(args[i]) == ".bin")
                {
                    file.Load(args[i]);
                    File.WriteAllLines(Path.ChangeExtension(args[i], ".txt"), STSCTextHandler.ConvertToText(file));
                }
                else
                {
                    STSCTextHandler.ConvertToObject(file, File.ReadAllLines(args[i]));
                    file.Save(Path.ChangeExtension(args[i], ".bin"));
                }
            }
        }
    }
}
