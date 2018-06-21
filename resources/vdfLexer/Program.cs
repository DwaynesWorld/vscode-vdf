using System;
using System.IO;

namespace VdfLexer {
    public class Program {
        static void Main(string[] args) {
            if (args.Length != 2) return;

            string sourceFolder = args[0];
            string indexFile = args[1];
            if (!Directory.Exists(sourceFolder)) return;

            var lexer = new Lexer(sourceFolder, indexFile);
            lexer.Run();
        }
    }
}