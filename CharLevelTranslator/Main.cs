using System;
using System.IO;

namespace CharLevelTranslator
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var builder = new System.Text.StringBuilder();
            using (var reader = File.OpenText(args[0]))
            {
                int i;
                while ((i = reader.Read()) != -1)
                {
                    char c = (char)i;
                    switch(c)
                    {
                        case ' ':
                            builder.Append('S');
                            break;
                        case '\t':
                            builder.Append('T');
                            break;
                        case '\n':
                            builder.AppendLine();
                            break;
                    }
                }
            }
            Console.WriteLine(builder.ToString());
        }
    }
}
