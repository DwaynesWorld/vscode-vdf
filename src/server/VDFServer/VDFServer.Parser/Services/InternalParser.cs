using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VDFServer.Data.Enumerations;
using VDFServer.Data.Models;
using VDFServer.Parser;

namespace VDFServer.Parser.Service
{
    public class InternalParser
    {
        private string[] _methodSkiplist;

        public InternalParser()
        {
            _methodSkiplist = new string[] { };
        }

        public InternalParser(string[] methodSkipList)
        {
            _methodSkiplist = methodSkipList;
        }

        public List<Tag> ParseFile(string filePath)
        {
            var tags = new List<Tag>();
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

                var tag = ParseLine(line, originalLine);

                if (tag == null)
                    continue;

                tag.Line = i;
                tags.Add(tag);
            }

            return tags;
        }

        public Tag ParseLine(string line, string originalLine)
        {
            if (Regex.IsMatch(line, Language.FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseClassObjectDeclaration(line, originalLine, false);
            else if (Regex.IsMatch(line, Language.CLASS_PATTERN, RegexOptions.IgnoreCase))
                return ParseClassObjectDeclaration(line, originalLine, true);
            else if (Regex.IsMatch(line, Language.STRUCT_PATTERN, RegexOptions.IgnoreCase))
                return ParseStructDeclaration(line, originalLine);

            return null;
        }

        public Tag ParseClassObjectDeclaration(string line, string originalLine, bool isClass)
        {
            var nameMatch = Regex.Match(line, Language.CLASS_OBJECT_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Value != null)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Value;

                tag.Type = isClass ? TagType.Class : TagType.Object;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }

        public Tag ParseProcedureDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.PROCEDURE_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Groups[1].Value;

                if (_methodSkiplist.Contains(tag.Name.ToUpper()))
                    return null;

                tag.Type = TagType.Procedure;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }

        public Tag ParseFunctionDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.FUNCTION_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Groups[1].Value;

                if (_methodSkiplist.Contains(tag.Name.ToUpper()))
                    return null;

                tag.Type = TagType.Function;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }

        public Tag ParseStructDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.STRUCT_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Groups[1].Value;

                tag.Type = TagType.Struct;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }
    }
}