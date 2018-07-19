﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

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

            bool doneIndexing = false;
            Task.Run(() => {
                var lexer = new Lexer(sourceFolder, indexFile);
                lexer.Run(reindex);
                doneIndexing = true;
            });

            var input = Console.OpenStandardInput();
            var buffer = new byte[1024];
            int length;
            while (input.CanRead && (length = input.Read(buffer, 0, buffer.Length)) > 0) {
                if (doneIndexing) {
                    var message = new byte[length];
                    Buffer.BlockCopy(buffer, 0, message, 0, length);
                    var payload = Encoding.UTF8.GetString(message);
                    Console.Write("Receiving: " + payload);
                } else {
                    Console.Write("Receiving: Indexing in progress");
                }
                Console.Out.Flush();
            }
        }
    }
}