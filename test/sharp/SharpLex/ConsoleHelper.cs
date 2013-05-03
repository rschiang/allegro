using System;
using System.Collections.Generic;
using System.Text;

namespace Allegro.Test.SharpLex
{
    internal class ConsoleHelper
    {
        public ConsoleHelper Text(string value)
        {
            Console.Write(value);
            return this;
        }

        public ConsoleHelper Text(ConsoleColor color, string value)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = original;
            return this;
        }

        public ConsoleHelper Line(string value)
        {
            Console.WriteLine(value);
            return this;
        }

        public ConsoleHelper Line(ConsoleColor color, string value)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = original;
            return this;
        }

        public ConsoleHelper Error(string value)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(value);
            Console.ForegroundColor = original;
            return this;
        }
    }
}
