using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLanguage
{

    class Program
    {
        const string FILE_PATH = @"C:\Users\Spencer\Documents\Downloads\Stuffs\TestProgram.hop";
        static StringBuilder buffer = new StringBuilder();

        static void Main(string[] args)
        {
            while (true)
            {
                string file = System.IO.File.ReadAllText(FILE_PATH);
                Tokenizer tokenizer = new Tokenizer(file);
                // TODO: Fix split
                Parser parser = new Parser(tokenizer, file.Split('\n'));
                var functions = parser.Parse();
                var transpiledLua = new StringBuilder();
                foreach (var function in functions)
                {
                    LuaTranspile.Transpiler.Function(transpiledLua, function, 0);
                }
                Console.WriteLine(transpiledLua.ToString());

                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}
