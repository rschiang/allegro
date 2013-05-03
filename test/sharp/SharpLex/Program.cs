using System;
using System.IO;

namespace Allegro.Test.SharpLex
{
    internal class Program
    {
        static ConsoleHelper buffer = new ConsoleHelper();

        static void Main(string[] args)
        {
            TextReader reader = Console.In;
            LexicalProcessor lexer = null;
            try
            {
                if (args.Length > 0) {
                    if (args[0].ToLowerInvariant().Equals("-h"))
                        PrintUsage();
                    reader = new StreamReader(args[0]);
                }

                lexer = new LexicalProcessor(reader);
                while (lexer.Read())
                {
                    LexicalToken token = lexer.Current;
                    buffer.Text(ConsoleColor.DarkYellow, String.Format("[{0}]", GetEnumName(token.Type)))
                          .Text(" ")
                          .Text(ConsoleColor.Gray, "\'")
                          .Text(token.Value)
                          .Text(ConsoleColor.Gray, "\'")
                          .Text(" ")
                          .Text(ConsoleColor.Gray, "\'")
                          .Text(token.Tag.ToString())
                          .Text(ConsoleColor.Gray, "\'")
                          .Line();
                }
            }
            catch (Exception ex) {
                Exception e = ex;
                while (e != null) {
                    buffer.Error(String.Format("sharplex: {0} ({1})", e.Message, e.GetType().Name));
                    e = ex.InnerException;
                }

                if (lexer != null)
                    buffer.Error(String.Format("\t at line {0}, column {1}", lexer.Line, lexer.Column));

                Environment.Exit(Math.Min(-ex.GetHashCode(), -1));
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine(
                @"Usage: sharplex [file] | -h"
            );
            Environment.Exit(0);
        }

        static string GetEnumName<T>(T value)
        {
            return Enum.GetName(typeof(T), value);
        }
    }
}
