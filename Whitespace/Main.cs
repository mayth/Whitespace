using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Whitespace
{
    class Program
    {
        static void Main(string[] args)
        {
            ICommand proc;
            switch (args[1])
            {
                case "Translator":
                    proc = new Translator();
                    break;
                case "Machine":
                    proc = new Machine();
                    break;
                default:
                    proc = new Machine();
                    break;
            }
            var isStep = args.Length >= 3 && !string.IsNullOrWhiteSpace(args[2]) && args[2] == "debug";
            Interpreter.Run(File.OpenRead(args[0]), proc, isStep);
            Console.WriteLine("=== End of Program ===");
        }
    }
}