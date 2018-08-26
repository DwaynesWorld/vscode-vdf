using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using VDFServer.Data;
using VDFServer.Data.Enumerations;
using VDFServer.Data.Models;

namespace VDFServer.Parser
{
    public class TagParser
    {
        private ApplicationDbContext _ctx;
        private string _workspaceRootFolder;
        private string[] _vdfExtensions = { ".VW", ".RV", ".SL", ".DG", ".SRC", ".DD", ".PKG", ".MOD", ".CLS", ".CLS", ".BPO", ".RPT", ".MNU", ".CAL", ".CON" };
        private string[] _skiplist = { "SET", "ONCLICK", "ACTIVATE", "ACTIVATING" };

        public TagParser(ApplicationDbContext ctx, string workspaceRootFolder)
        {
            _ctx = ctx;
            _workspaceRootFolder = workspaceRootFolder;
        }

        public void Run(bool reindex)
        {
            BuildIndex(reindex);
        }

        private void BuildIndex(bool reindex)
        {
            var filePaths = Directory
                 .EnumerateFiles(_workspaceRootFolder, "*", SearchOption.AllDirectories)
                 .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper()));

            foreach (var path in filePaths)
            {
                var fileInfo = new FileInfo(path);
                var sourceFile = _ctx.SourceFiles
                    .Include(s => s.Tags)
                    .Where(src => src.FilePath.ToUpper() == path.ToUpper())
                    .SingleOrDefault();

                if (sourceFile != null)
                {
                    if (reindex || sourceFile.LastWriteTime != fileInfo.LastWriteTime)
                    {
                        if (sourceFile.Tags == null)
                            sourceFile.Tags = new List<Tag>();
                        else
                            _ctx.Tags.RemoveRange(sourceFile.Tags);

                        var tags = ParseFile(path, sourceFile.Id);
                        sourceFile.Tags.AddRange(tags);
                        sourceFile.LastWriteTime = fileInfo.LastWriteTime;
                        sourceFile.LastUpdated = DateTime.Now;
                        _ctx.SourceFiles.Update(sourceFile);
                    }
                }
                else
                {
                    sourceFile = new SourceFile();

                    if (sourceFile.Tags == null)
                        sourceFile.Tags = new List<Tag>();

                    var tags = ParseFile(path, sourceFile.Id);
                    sourceFile.Tags.AddRange(tags);
                    sourceFile.FilePath = path;
                    sourceFile.FileName = fileInfo.Name;
                    sourceFile.LastWriteTime = fileInfo.LastWriteTime;
                    sourceFile.LastUpdated = DateTime.Now;
                    _ctx.SourceFiles.Add(sourceFile);
                }
            }

            _ctx.SaveChanges();
        }

        private List<Tag> ParseFile(string filePath, string fileId)
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

        private Tag ParseLine(string line, string originalLine)
        {
            if (Regex.IsMatch(line, Language.OBJECT_PATTERN, RegexOptions.IgnoreCase))
                return ParseObjectDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.PROCEDURE_PATTERN, RegexOptions.IgnoreCase))
                return ParseProcedureDeclaration(line, originalLine);
            else if (Regex.IsMatch(line, Language.FUNCTION_PATTERN, RegexOptions.IgnoreCase))
                return ParseFunctionDeclaration(line, originalLine);

            return null;
        }

        private Tag ParseObjectDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.OBJECT_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Value != null)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Value;

                if (_skiplist.Contains(tag.Name.ToUpper()))
                    return null;

                tag.Type = TagType.Object;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }

        private Tag ParseProcedureDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.PROCEDURE_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Groups[1].Value;

                if (_skiplist.Contains(tag.Name.ToUpper()))
                    return null;

                tag.Type = TagType.Procedure;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }

        private Tag ParseFunctionDeclaration(string line, string originalLine)
        {
            var nameMatch = Regex.Match(line, Language.FUNCTION_NAME_PATTERN, RegexOptions.IgnoreCase);
            if (nameMatch.Groups.Count > 1)
            {
                var tag = new Tag();
                tag.Name = nameMatch.Groups[1].Value;

                if (_skiplist.Contains(tag.Name.ToUpper()))
                    return null;

                tag.Type = TagType.Function;
                tag.StartColumn = originalLine.IndexOf(tag.Name);
                tag.EndColumn = tag.StartColumn + tag.Name.Length;
                return tag;
            }

            return null;
        }
    }
}