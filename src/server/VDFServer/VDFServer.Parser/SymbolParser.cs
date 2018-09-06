using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VDFServer.Data;
using VDFServer.Data.Constants;
using VDFServer.Data.Entities;
using VDFServer.Data.Enumerations;
using VDFServer.Data.Models;
using VDFServer.Parser.Services;

namespace VDFServer.Parser
{
    public class WorkspaceSymbolParser : IWorkspaceSymbolParser
    {
        public static volatile bool DoneIndexing = false;

        private readonly ApplicationDbContext _ctx;
        private readonly IInternalParser _parser;
        private readonly IVDFServerSerializer _serializer;

        private readonly string[] _vdfExtensions = { ".VW", ".RV", ".SL", ".DG", ".SRC", ".DD", ".PKG", ".MOD", ".CLS", ".CLS", ".BPO", ".RPT", ".MNU", ".CAL", ".CON" };

        public WorkspaceSymbolParser(
            ApplicationDbContext ctx,
            IInternalParser parser,
            IVDFServerSerializer serializer)
        {
            _ctx = ctx;
            _parser = parser;
            _serializer = serializer;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                BuildIndex();
                watch.Stop();

                DoneIndexing = true;

                var results = new InternalResult
                {
                    MessageType = IPCMessage.LanguageServerIndexingComplete,
                    Message = ServerConstants.LANGUAGE_SERVER_INDEXING_COMPLETE,
                    MetaData = $"{watch.ElapsedMilliseconds / 1000}"
                };

                Console.WriteLine(_serializer.Serialize(results));
                Console.Out.Flush();

                Clean();
            });
        }

        public async void Clean()
        {
            var filePaths = Directory
                .EnumerateFiles(_ctx.WorkspaceRootFolder, "*", SearchOption.AllDirectories)
                .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper()));

            if (await _ctx.SourceFiles.CountAsync() != filePaths.Count())
                CleanupIndex();
        }

        private async void BuildIndex(bool reindex = true)
        {
            foreach (var path in Directory
                    .EnumerateFiles(_ctx.WorkspaceRootFolder, "*", SearchOption.AllDirectories)
                    .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper())))
            {
                var fileInfo = new FileInfo(path);
                var sourceFile = await _ctx.SourceFiles
                    .Include(s => s.Symbols)
                    .Where(src => src.FilePath.ToUpper() == path.ToUpper())
                    .SingleOrDefaultAsync();

                if (sourceFile != null)
                {
                    if (reindex || sourceFile.LastWriteTime != fileInfo.LastWriteTime)
                    {
                        if (sourceFile.Symbols == null)
                            sourceFile.Symbols = new List<LanguageSymbol>();
                        else
                            _ctx.Symbols.RemoveRange(sourceFile.Symbols);

                        var symbols = _parser.ParseFile(path);
                        sourceFile.Symbols.AddRange(symbols);
                        sourceFile.LastWriteTime = fileInfo.LastWriteTime;
                        sourceFile.LastUpdated = DateTime.Now;
                        _ctx.SourceFiles.Update(sourceFile);
                    }
                }
                else
                {
                    sourceFile = new SourceFile();

                    if (sourceFile.Symbols == null)
                        sourceFile.Symbols = new List<LanguageSymbol>();

                    var symbols = _parser.ParseFile(path);
                    sourceFile.Symbols.AddRange(symbols);
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