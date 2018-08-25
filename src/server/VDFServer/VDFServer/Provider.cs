using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VDFServer.Data;
using VDFServer.Models;
using VDFServer.Parser;

namespace VDFServer
{
    public class Provider
    {
        public static volatile bool DoneIndexing = false;

        private ApplicationDbContext _ctx;
        private TagParser _parser;

        public Provider(string indexPath, string workspaceRootPath)
        {
            var indexFile = $"{Hasher.GetStringHash(workspaceRootPath)}.db";
            var indexFullName = Path.Combine(indexPath, indexFile);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={indexFullName}")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            _ctx = new ApplicationDbContext(options);
            _ctx.Database.EnsureCreated();

            _parser = new TagParser(_ctx, workspaceRootPath);

            Task.Run(() =>
            {
                _parser.Run(false);
                DoneIndexing = true;
            });
        }

        public string Provide(string incomingPayload)
        {
            if (!DoneIndexing)
                return "Indexing";

            var request = JsonConvert.DeserializeObject<Request>(incomingPayload);
            switch (request.Lookup)
            {
                case "4":
                    var results = ProvideDefinition(request);
                    if (results == null)
                        return "";
                    else
                        return JsonConvert.SerializeObject(results);
                default:
                    return "";
            }
        }

        private DefinitionResult ProvideDefinition(Request request)
        {
            var matches = _ctx.Tags
                .Where(t => t.Name.Equals(request.PossibleWord, StringComparison.OrdinalIgnoreCase));

            if (!matches.Any())
                return null;

            var results = new DefinitionResult();
            results.RequestId = request.Id;
            results.Definitions = new List<Definition>();
            foreach (var match in matches)
            {
                var def = new Definition();
                def.FileName = match.File.FileName;
                def.RawType = "";
                def.Text = "";
                def.Type = request.Lookup;
                def.Range = new DefinitionRange
                {
                    StartLine = match.Line,
                    EndLine = match.Line,
                    StartColumn = match.StartColumn,
                    EndColumn = match.EndColumn
                };

                results.Definitions.Add(def);
            }

            return results;
        }

    }
}