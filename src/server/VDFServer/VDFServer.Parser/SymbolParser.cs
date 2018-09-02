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
using VDFServer.Data.Enumerations;
using VDFServer.Data.Models;
using VDFServer.Parser.Service;

namespace VDFServer.Parser
{
    public class SymbolParser : ISymbolParser
    {
        public static volatile bool DoneIndexing = false;

        private readonly ApplicationDbContext _ctx;
        private readonly IInternalParser _parser;
        private readonly string[] _vdfExtensions = { ".VW", ".RV", ".SL", ".DG", ".SRC", ".DD", ".PKG", ".MOD", ".CLS", ".CLS", ".BPO", ".RPT", ".MNU", ".CAL", ".CON" };

        public SymbolParser(
            ApplicationDbContext ctx,
            IInternalParser parser)
        {
            _ctx = ctx;
            _parser = parser;
        }

        public void Start()
        {
            Task.Run(() =>
           {
               var watch = System.Diagnostics.Stopwatch.StartNew();

               BuildIndex();

               watch.Stop();
               System.Diagnostics.Debug.WriteLine(watch.ElapsedMilliseconds);

               DoneIndexing = true;
               Console.WriteLine($"{ServerConstants.LANGUAGE_SERVER_INDEXING_COMPLETE} - {watch.ElapsedMilliseconds / 1000}");

               Clean();
           });
        }

        public async void Clean()
        {
            var filePaths = Directory
                .EnumerateFiles(ApplicationDbContext.WorkspaceRootFolder, "*", SearchOption.AllDirectories)
                .Where(f => _vdfExtensions.Contains(Path.GetExtension(f).ToUpper()));

            if (await _ctx.SourceFiles.CountAsync() != filePaths.Count())
                CleanupIndex();
        }

        private async void BuildIndex(bool reindex = true)
        {
            foreach (var path in Directory
                .EnumerateFiles(ApplicationDbContext.WorkspaceRootFolder, "*", SearchOption.AllDirectories)
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