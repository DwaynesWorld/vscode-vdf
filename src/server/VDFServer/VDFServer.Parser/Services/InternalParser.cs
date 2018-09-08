using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VDFServer.Data.Enumerations;
using VDFServer.Data.Entities;
using VDFServer.Parser;

namespace VDFServer.Parser.Services
{
    public class InternalParser : IInternalParser
    {
        public InternalParser()
        {
        }

        public List<LanguageSymbol> ParseFile(string filePath)
        {
            var containers = new Stack<LanguageSymbol>();
            var inProcessSymbols = new Stack<LanguageSymbol>();
            var symbols = new List<LanguageSymbol>();

            var lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                var originalLine = lines[i];
                var line = lines[i].Trim();
                var commentPos = line.IndexOf("//");
                if (commentPos != -1)
                    line = line.Remove(commentPos);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var symbol = ParseLine(line, originalLine);

                if (symbol == null)
                    continue;

                if (!symbol.IsEndDeclaration)
                {
                    symbol.StartLine = i;
                    if (containers.Any())
                    {
                        var lastContainer = containers.Peek();
                        symbol.Container = lastContainer.Name;
                        symbol.ContainerType = lastContainer.Type;
                        symbol.ContainerLine = lastContainer.StartLine;
                    }

                    inProcessSymbols.Push(symbol);

                    if (symbol.IsContainer)
                        containers.Push(symbol);
                }
                else
                {
                    if (symbol.IsContainer && containers.Any())
                        containers.Pop();

                    if (inProcessSymbols.Any())
                    {
                        var currentSymbol = inProcessSymbols.Pop();
                        currentSymbol.EndLine = i;
                        symbols.Add(currentSymbol);
                    }
                }
            }

            return symbols;
        }

        public LanguageSymbol ParseLine(string line, string originalLine)
        {
            if (Regex.IsMatch(line, Language.FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.END_FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return EndSymbolDeclaration(SymbolKind.Function);

            else if (Regex.IsMatch(line, Language.PROCEDURE_SET_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line, originalLine, true);
            else if (Regex.IsMatch(line, Language.PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line, originalLine, false);
            else if (Regex.IsMatch(line, Language.END_PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return EndSymbolDeclaration(SymbolKind.Method);

            else if (Regex.IsMatch(line, Language.OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseClassObjectDeclaration(line, originalLine, false);
            else if (Regex.IsMatch(line, Language.END_OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return EndSymbolDeclaration(SymbolKind.Object);

            else if (Regex.IsMatch(line, Language.CLASS_PATTERN, RegexOptions.IgnoreCase))
                return ParseClassObjectDeclaration(line, originalLine, true);
            else if (Regex.IsMatch(line, Language.END_CLASS_PATTERN, RegexOptions.IgnoreCase))
                return EndSymbolDeclaration(SymbolKind.Class);

            else if (Regex.IsMatch(line, Language.STRUCT_PATTERN, RegexOptions.IgnoreCase))
                return ParseStructDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.END_STRUCT_PATTERN, RegexOptions.IgnoreCase))
                return EndSymbolDeclaration(SymbolKind.Struct);

            return null;
        }

        public LanguageSymbol EndSymbolDeclaration(SymbolKind kind)
        {
            return new LanguageSymbol
            {
                Type = kind,
                IsEndDeclaration = true,
                IsContainer = kind == SymbolKind.Class || kind == SymbolKind.Object ? true : false
            };
        }

        public LanguageSymbol ParseClassObjectDeclaration(string line, string originalLine, bool isClass)
        {
            var nameMatch = isClass
                                ? Regex.Match(line, Language.CLASS_NAME_PATTERN, RegexOptions.IgnoreCase)
                                : Regex.Match(line, Language.OBJECT_NAME_PATTERN, RegexOptions.IgnoreCase);

            if (nameMatch.Groups.Count > 1)
            {
                var symbol = new LanguageSymbol();
                symbol.Name = nameMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(symbol.Name))
                    return null;

                symbol.Type = isClass ? SymbolKind.Class : SymbolKind.Object;
                symbol.StartColumn = originalLine.IndexOf(symbol.Name);
                symbol.EndColumn = symbol.StartColumn + symbol.Name.Length;
                symbol.IsContainer = true;
                return symbol;
            }

            return null;
        }

        public LanguageSymbol ParseProcedureDeclaration(string line, string originalLine, bool isProcedureSet)
        {
            var nameMatch = isProcedureSet
                                ? Regex.Match(line, Language.PROCEDURE_SET_NAME_PATTERN, RegexOptions.IgnoreCase)
                                : Regex.Match(line, Language.PROCEDURE_NAME_PATTERN, RegexOptions.IgnoreCase);

            if (nameMatch.Groups.Count > 1)
            {
                var symbol = new LanguageSymbol();
                symbol.Name = nameMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(symbol.Name))
                    return null;

                symbol.Type = SymbolKind.Method;
                symbol.StartColumn = originalLine.IndexOf(symbol.Name);
                symbol.EndColumn = symbol.StartColumn + symbol.Name.Length;
                return symbol;
            }

            return null;
        }

        public LanguageSymbol ParseFunctionDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.FUNCTION_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var symbol = new LanguageSymbol();
                symbol.Name = nameMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(symbol.Name))
                    return null;

                symbol.Type = SymbolKind.Function;
                symbol.StartColumn = originalLine.IndexOf(symbol.Name);
                symbol.EndColumn = symbol.StartColumn + symbol.Name.Length;
                return symbol;
            }

            return null;
        }

        public LanguageSymbol ParseStructDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.STRUCT_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var symbol = new LanguageSymbol();
                symbol.Name = nameMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(symbol.Name))
                    return null;

                symbol.Type = SymbolKind.Struct;
                symbol.StartColumn = originalLine.IndexOf(symbol.Name);
                symbol.EndColumn = symbol.StartColumn + symbol.Name.Length;
                return symbol;
            }

            return null;
        }
    }
}