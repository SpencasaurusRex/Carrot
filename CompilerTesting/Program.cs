using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing2
{

    class Program
    {
        const string FILE_PATH = @"C:\Users\Spencer\Documents\Downloads\Stuffs\TestProgram.hop";
        static StringBuilder buffer = new StringBuilder();

        static void Main(string[] args)
        {
            string file = System.IO.File.ReadAllText(FILE_PATH);
            {
                Tokenizer tokenizer = new Tokenizer(file);
                foreach (var token in tokenizer.GetTokens())
                {
                    Console.WriteLine(token.text);
                }
            }
            Console.ReadKey();
            {
                Tokenizer tokenizer = new Tokenizer(file);
                Parser parser = new Parser(tokenizer);
                parser.Parse();
                Console.ReadKey();
            }
        }
    }
}
