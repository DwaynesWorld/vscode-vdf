using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using VdfLexer.Models;

namespace VdfLexer {
    public class Lexer {
        private const string OBJECT_PATTERN = "(\\bobject\\b|\\bcd_popup_object\\b|\\bhcss_Ccd_object\\b)(?=\\s+\\w+\\s+is)";
        private const string PROCEDURE_PATTERN = "(\\bprocedure\\b)(?=\\s+)";
        private const string FUNCTION_PATTERN = "(\\bfunction\\b)(?=(\\s+\\w+)+(\\s+\\breturns\\b\\s+)\\s*)";
        private string SourceFolder;
        private string IndexFile;
        private LanguageIndex Index;

        public Lexer(string sourceFolder, string indexFile) {
            SourceFolder = sourceFolder;
            IndexFile = indexFile;
        }

        public void Run(bool reindex = false) {
            LoadIndex();
            CreateIndex(reindex);
            OutputIndex();
        }

        private void LoadIndex() {
            if (File.Exists(IndexFile)) {
                var indexText = File.ReadAllText(IndexFile);
                Index = JsonConvert.DeserializeObject<LanguageIndex>(indexText);
            } else {
                Index = new LanguageIndex();
            }
        }

        private void CreateIndex(bool reindex) {
            var sourceFileDictionary = Index.Files.ToDictionary(f => f.FilePath);

            foreach (var filePath in Directory.EnumerateFiles(SourceFolder, "*.*", SearchOption.AllDirectories)) {
                var sourceFile = new SourceFile();
                var hash = GetChecksum(filePath);

                var hasKey = sourceFileDictionary.ContainsKey(filePath);

                if (reindex || !hasKey || sourceFileDictionary[filePath].Hash != hash) {
                    var fileInfo = new FileInfo(filePath);

                    (sourceFile.Objects, sourceFile.Procedures, sourceFile.Functions) = AnalyzeFile(filePath);
                    sourceFile.FilePath = filePath;
                    sourceFile.FileName = fileInfo.Name;
                    sourceFile.Hash = hash;
                    sourceFile.LastModified = DateTime.Now;

                    if (hasKey)
                        sourceFileDictionary[filePath] = sourceFile;
                    else
                        sourceFileDictionary.Add(filePath, sourceFile);
                }
            }

            Index.Files = sourceFileDictionary.Values.ToList();
            Index.LastUpdated = DateTime.Now;
        }

        private void OutputIndex() {
            var indexText = JsonConvert.SerializeObject(Index, Formatting.Indented);
            File.WriteAllText(IndexFile, indexText);
        }

        private(List<Definition> Objects, List<Definition> Procedures, List<Definition> Functions) AnalyzeFile(string filePath) {
            var objects = new List<Definition>();
            var procedures = new List<Definition>();
            var functions = new List<Definition>();

            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++) {
                var originalLine = lines[i];
                var line = lines[i].Trim();
                var commentPos = line.IndexOf("//");
                if (commentPos != -1)
                    line = line.Remove(commentPos);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var definition = ParseLine(line, originalLine);

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

        private Definition ParseLine(string line, string originalLine) {
            if (Regex.IsMatch(line, OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseObjectDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line, originalLine);

            return null;
        }

        private Definition ParseObjectDeclaration(string line, string originalLine) {
            var definition = new Definition();
            definition.Type = DefinitionType.Object;
            //Get Object Name
            //Get Object Name Column in orginal line
            return definition;
        }

        private Definition ParseProcedureDeclaration(string line, string originalLine) {
            var definition = new Definition();
            definition.Type = DefinitionType.Procedure;

            return definition;
        }

        private Definition ParseFunctionDeclaration(string line, string originalLine) {
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