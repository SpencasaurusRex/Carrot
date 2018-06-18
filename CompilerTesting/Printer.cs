using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testing2;

namespace CompilerTesting
{
    public class Printer
    {
        static void Tab(int index)
        {
            for (int i = 0; i < index; i++)
            {
                Console.Write("    ");
            }
        }

        public static void PrettyPrint(Function function, int i)
        {
            Tab(i);
            var proto = function.prototype;
            Console.WriteLine("func {0} -> {1} {", proto.name, proto.returnType);
        }
    }
}
