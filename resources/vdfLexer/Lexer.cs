using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using VdfLexer.Models;

namespace VdfLexer {
    public class Lexer {
        private const string OBJECT_PATTERN = "(object|cd_popup_object)\\s(?=\\w+\\sis)";
        private const string PROCEDURE_PATTERN = "(object|cd_popup_object)\\s(?=\\w+\\sis)";
        private const string FUNCTION_PATTERN = "(object|cd_popup_object)\\s(?=\\w+\\sis)";
        private string SourceFolder;
        private string IndexFile;
        private LanguageIndex Index;

        public Lexer(string sourceFolder, string indexFile) {
            SourceFolder = sourceFolder;
            IndexFile = indexFile;
            LoadIndex();
        }

        public void Run(bool reindex = false) {
            foreach (var filePath in Directory.EnumerateFiles(SourceFolder, "*.*", SearchOption.AllDirectories)) {
                var sourceFile = new SourceFile();
                var hash = GetChecksum(filePath);
                var hasKey = Index.Files.ContainsKey(filePath);

                if (reindex || !hasKey || Index.Files[filePath].Hash != hash) {
                    var fileInfo = new FileInfo(filePath);

                    (sourceFile.Objects, sourceFile.Procedures, sourceFile.Functions) = AnalyzeFile(filePath);
                    sourceFile.FilePath = filePath;
                    sourceFile.FileName = fileInfo.Name;
                    sourceFile.Hash = hash;
                    sourceFile.LastModified = DateTime.Now;
                }

                if (hasKey)
                    Index.Files[filePath] = sourceFile;
                else
                    Index.Files.Add(filePath, sourceFile);
            }

            Index.LastUpdated = DateTime.Now;
        }

        private void LoadIndex() {
            if (File.Exists(IndexFile)) {
                Index = JsonConvert.DeserializeObject(IndexFile) as LanguageIndex;
            } else {
                Index = new LanguageIndex();
            }
        }

        private(List<Declaration> Objects, List<Declaration> Procedures, List<Declaration> Functions) AnalyzeFile(string filePath) {
            var objects = new List<Declaration>();
            var procedures = new List<Declaration>();
            var functions = new List<Declaration>();

            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++) {
                var line = lines[i];
                line = line.Trim().Remove(line.IndexOf("//"));

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var declaration = ParseLine(line);

                if (declaration == null)
                    continue;

                declaration.Line = i + 1;

                switch (declaration.Type) {
                    case DeclarationType.Object:
                        objects.Add(declaration);
                        break;
                    case DeclarationType.Procedure:
                        procedures.Add(declaration);
                        break;
                    case DeclarationType.Function:
                        functions.Add(declaration);
                        break;
                    default:
                        throw new Exception("Unknown declaration type.");
                }

            }

            return (objects, procedures, functions);
        }

        private Declaration ParseLine(string line) {
            if (Regex.IsMatch(line, OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseObjectDeclaration(line);
            else if (Regex.IsMatch(line, PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line);
            else if (Regex.IsMatch(line, FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line);

            return null;
        }

        private Declaration ParseObjectDeclaration(string line) {
            var declaration = new Declaration();
            declaration.Type = DeclarationType.Object;

            return declaration;
        }

        private Declaration ParseProcedureDeclaration(string line) {
            var declaration = new Declaration();
            declaration.Type = DeclarationType.Procedure;

            return declaration;
        }

        private Declaration ParseFunctionDeclaration(string line) {
            var declaration = new Declaration();
            declaration.Type = DeclarationType.Function;

            return declaration;
        }

        private string GetChecksum(string filePath) {
            using(FileStream stream = File.OpenRead(filePath)) {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}