using DALLib.Exceptions;
using DALLib.File;
using DALLib.Scripting;
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
            STSCMacros.Fill();
            for (int i = 0; i < args.Length; ++i)
            {
                var file = new STSCFile();
                if (Path.GetExtension(args[i]) == ".bin")
                {
                    try
                    {
                        file.Load(args[i]);
                    }catch (STSCDisassembleException e)
                    {
                        Console.WriteLine("Error: {0}.", e.Message);
                        Console.WriteLine("This usually means the Script is corrupt or one or more STSCFile's definitions are incorrect.");
                        Console.WriteLine("or the opcode being read is not yet implemented");
                        Console.WriteLine("Please check the output file for finding out what instruction went wrong.");
                        Console.WriteLine("Disassembler must abort now!");
                        Console.ReadKey(true);
                    }
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
