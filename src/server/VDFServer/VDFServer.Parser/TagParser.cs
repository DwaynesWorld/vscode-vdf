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
using VDFServer.Parser.Service;

namespace VDFServer.Parser
{
    public class TagParser
    {
        private ApplicationDbContext _ctx;
        private InternalParser _parser;
        private string _workspaceRootFolder;
        private string[] _vdfExtensions = { ".VW", ".RV", ".SL", ".DG", ".SRC", ".DD", ".PKG", ".MOD", ".CLS", ".CLS", ".BPO", ".RPT", ".MNU", ".CAL", ".CON" };
        private string[] _methodSkiplist = { "SET", "ONCLICK", "ACTIVATE", "ACTIVATING" };

        public TagParser(ApplicationDbContext ctx, string workspaceRootFolder)
        {
            _ctx = ctx;
            _workspaceRootFolder = workspaceRootFolder;
            _parser = new InternalParser(_methodSkiplist);
        }

        public void Run(bool reindex)
        {
            BuildIndex(reindex);
        }

        public async void Clean()
        {
            var filePaths = Directory
                .EnumerateFiles(_workspaceRootFolder, "*", SearchOption.AllDirectories)
                .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper()));

            if (await _ctx.SourceFiles.CountAsync() != filePaths.Count())
                CleanupIndex();
        }

        private async void BuildIndex(bool reindex)
        {
            var filePaths = Directory
                .EnumerateFiles(_workspaceRootFolder, "*", SearchOption.AllDirectories)
                .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper()));

            foreach (var path in filePaths)
            {
                var fileInfo = new FileInfo(path);
                var sourceFile = await _ctx.SourceFiles
                    .Include(s => s.Tags)
                    .Where(src => src.FilePath.ToUpper() == path.ToUpper())
                    .SingleOrDefaultAsync();

                if (sourceFile != null)
                {
                    if (reindex || sourceFile.LastWriteTime != fileInfo.LastWriteTime)
                    {
                        if (sourceFile.Tags == null)
                            sourceFile.Tags = new List<Tag>();
                        else
                            _ctx.Tags.RemoveRange(sourceFile.Tags);

                        var tags = _parser.ParseFile(path);
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

                    var tags = _parser.ParseFile(path);
                    sourceFile.Tags.AddRange(tags);
                    sourceFile.FilePath = path;
                    sourceFile.FileName = fileInfo.Name;
                    sourceFile.LastWriteTime = fileInfo.LastWriteTime;
                    sourceFile.LastUpdated = DateTime.Now;
                    _ctx.SourceFiles.Add(sourceFile);
                }
            }

            await _ctx.SaveChangesAsync();
        }

        private async void CleanupIndex()
        {
            await _ctx.SourceFiles.ForEachAsync(s =>
            {
                if (!File.Exists(s.FilePath))
                    _ctx.Remove(s);
            });
            await _ctx.SaveChangesAsync();
        }
    }
}