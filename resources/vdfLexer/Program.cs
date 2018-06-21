using System;
using System.IO;

namespace VdfLexer {
    public class Program {
        static void Main(string[] args) {
            if (args.Length < 2) return;

            string sourceFolder = args[0];
            string indexFile = args[1];
            if (!Directory.Exists(sourceFolder)) return;

            bool reindex = false;
            if (args.Length == 3)
                reindex = args[2].ToUpper() == "TRUE";

            var lexer = new Lexer(sourceFolder, indexFile);
            lexer.Run(reindex);
        }
    }
}