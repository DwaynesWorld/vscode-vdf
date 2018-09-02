using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VDFServer.Data;
using VDFServer.Data.Constants;
using VDFServer.Data.Enumerations;
using VDFServer.Data.Models;
using VDFServer.Models;
using VDFServer.Parser;
using VDFServer.Parser.Service;

namespace VDFServer
{
    public class Provider : IDisposable
    {
        private ApplicationDbContext _ctx;

        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public Provider(DbContextOptions<ApplicationDbContext> options)
        {
            _ctx = new ApplicationDbContext(options);
        }

        public string Provide(string incomingPayload)
        {
            var request = JsonConvert.DeserializeObject<Request>(incomingPayload);

            if (!SymbolParser.DoneIndexing)
                return HandlePreIndexRequest(request);

            // We are not handling anything in a try/catch
            // because we want the program to crash
            // it will be handled by the parent process
            var results = ServerConstants.TAG_NOT_FOUND;
            switch (request.Lookup)
            {
                case CommandType.Definitions:
                    var definitionResults = ProvideDefinition(request);
                    if (definitionResults != null)
                        results = JsonConvert.SerializeObject(definitionResults, _serializerSettings);
                    break;
                case CommandType.Symbols:
                    var symbolResults = ProvideSymbols(request);
                    if (symbolResults != null)
                        results = JsonConvert.SerializeObject(symbolResults, _serializerSettings);
                    break;
                default:
                    results = ServerConstants.NO_PROVIDER_FOUND;
                    break;
            }

            return results;
        }

        private string HandlePreIndexRequest(Request request)
        {
            if (request.Lookup == CommandType.Symbols)
            {
                var parser = new InternalParser();
                var symbols = parser.ParseFile(request.Path);
                return JsonConvert.SerializeObject(GetSymbolResults(request, symbols), _serializerSettings);
            }
            else
            {
                return ServerConstants.LANGUAGE_SERVER_INDEXING;
            }
        }

        private DefinitionResult ProvideSymbols(Request request)
        {
            var symbols = _ctx.Symbols
                    .Include(s => s.File)
                    .Where(s => s.File.FilePath.ToUpper() == request.Path.ToUpper());

            if (!symbols.Any())
                return null;

            return GetSymbolResults(request, symbols);
        }

        private DefinitionResult GetSymbolResults(Request request, IEnumerable<LanguageSymbol> symbols)
        {
            var results = new DefinitionResult();
            results.RequestId = request.Id;
            results.Definitions = new List<Definition>();
            foreach (var symbol in symbols)
            {
                var def = new Definition();
                def.FilePath = request.Path;
                def.RawType = "";
                def.Text = symbol.Name;
                def.Kind = symbol.Type;
                def.Container = symbol.Container;
                def.Type = request.Lookup;
                def.Range = new DefinitionRange
                {
                    StartLine = symbol.StartLine,
                    EndLine = symbol.EndLine,
                    StartColumn = symbol.StartColumn,
                    EndColumn = symbol.EndColumn
                };

                results.Definitions.Add(def);
            }
            return results;
        }

        private DefinitionResult ProvideDefinition(Request request)
        {
            var matches = _ctx.Symbols
                .Include(t => t.File)
                .Where(t => t.Name.ToUpper() == request.PossibleWord.ToUpper());

            if (!matches.Any())
                return null;

            var results = new DefinitionResult();
            results.RequestId = request.Id;
            results.Definitions = new List<Definition>();
            foreach (var match in matches)
            {
                var def = new Definition();
                def.FilePath = match.File.FilePath;
                def.RawType = "";
                def.Text = "";
                def.Type = request.Lookup;
                def.Range = new DefinitionRange
                {
                    StartLine = match.StartLine,
                    EndLine = match.EndLine,
                    StartColumn = match.StartColumn,
                    EndColumn = match.EndColumn
                };

                results.Definitions.Add(def);
            }

            return results;
        }

        public void Dispose()
        {
            if (_ctx != null)
            {
                _ctx.Dispose();
                _ctx = null;
            }
        }
    }
}