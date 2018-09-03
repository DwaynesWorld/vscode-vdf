using System.Collections.Generic;
using VDFServer.Data.Entities;
using VDFServer.Data.Enumerations;

namespace VDFServer.Parser.Services
{
    public interface IInternalParser
    {
        LanguageSymbol EndSymbolDeclaration(SymbolKind kind);
        LanguageSymbol ParseClassObjectDeclaration(string line, string originalLine, bool isClass);
        List<LanguageSymbol> ParseFile(string filePath);
        LanguageSymbol ParseFunctionDeclaration(string line, string originalLine);
        LanguageSymbol ParseLine(string line, string originalLine);
        LanguageSymbol ParseProcedureDeclaration(string line, string originalLine);
        LanguageSymbol ParseStructDeclaration(string line, string originalLine);
    }
}