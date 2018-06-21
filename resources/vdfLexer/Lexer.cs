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

        private(List<Definition> Objects, List<Definition> Procedures, List<Definition> Functions) AnalyzeFile(string filePath) {
            var objects = new List<Definition>();
            var procedures = new List<Definition>();
            var functions = new List<Definition>();

            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++) {
                var line = lines[i];
                line = line.Trim().Remove(line.IndexOf("//"));

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var definition = ParseLine(line);

                if (definition == null)
                    continue;

                definition.Line = i + 1;

                switch (definition.Type) {
                    case DefinitionType.Object:
                        objects.Add(definition);
                        break;
                    case DefinitionType.Procedure:
                        procedures.Add(definition);
                        break;
                    case DefinitionType.Function:
                        functions.Add(definition);
                        break;
                    default:
                        throw new Exception("Unknown declaration type.");
                }

            }

            return (objects, procedures, functions);
        }

        private Definition ParseLine(string line) {
            if (Regex.IsMatch(line, OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseObjectDeclaration(line);
            else if (Regex.IsMatch(line, PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line);
            else if (Regex.IsMatch(line, FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line);

            return null;
        }

        private Definition ParseObjectDeclaration(string line) {
            var definition = new Definition();
            definition.Type = DefinitionType.Object;

            return definition;
        }

        private Definition ParseProcedureDeclaration(string line) {
            var definition = new Definition();
            definition.Type = DefinitionType.Procedure;

            return definition;
        }

        private Definition ParseFunctionDeclaration(string line) {
            var definition = new Definition();
            definition.Type = DefinitionType.Function;

            return definition;
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