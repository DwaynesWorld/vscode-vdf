using System;
using System.Linq;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VdfLexer.VSCode.Models;
using VdfLexer.Models;

namespace VdfLexer
{
    public class Program
    {
        private static volatile bool _doneIndexing = false;
        private static JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static LanguageIndex _index;
        private static bool _indexLoaded = false;
        private static bool _reindex = false;
        private static string _indexFile = "";
        private static string _src = "";

        static void Main(string[] args)
        {
            Init(args);

            Task.Run(() =>
            {
                var lexer = new Lexer(_src, _indexFile);
                lexer.Run(_reindex);
                _doneIndexing = true;
            });

            int length;
            var buffer = new byte[1024];
            var input = Console.OpenStandardInput();
            while (input.CanRead && (length = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (_doneIndexing)
                {
                    if (!_indexLoaded)
                        LoadIndex();

                    var message = new byte[length];
                    Buffer.BlockCopy(buffer, 0, message, 0, length);
                    var payload = Encoding.UTF8.GetString(message);
                    var requestPayload = JsonConvert.DeserializeObject<RequestPayload>(payload);

                    System.Diagnostics.Debug.WriteLine(payload);

                    switch (requestPayload.Lookup)
                    {
                        case "4":
                            Console.Write(ProvideDefinition(requestPayload));
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Console.Write("Indexing in progress");
                }

                Console.Out.Flush();
            }
        }

        private static void Init(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ApplicationException("Required arguments missing.");
            }

            _src = args[0];
            _indexFile = args[1];

            if (!Directory.Exists(_src))
            {
                throw new ApplicationException("Required arguments missing.");
            }

            if (args.Length == 3)
            {
                _reindex = args[2].ToUpper() == "TRUE";
            }
        }

        private static void LoadIndex()
        {
            var indexText = File.ReadAllText(_indexFile);
            _index = JsonConvert.DeserializeObject<LanguageIndex>(indexText);
        }

        private static string ProvideDefinition(RequestPayload request)
        {
            //var files = _index.Files.Where(f => f.Functions.Where(fn => fn.Name.Equals(request.PossibleWord, StringComparison.OrdinalIgnoreCase)))
            var files = Directory
                        .EnumerateFiles(request.WorkspacePath, "*", SearchOption.AllDirectories)
                        .Where(f => Path.GetExtension(f).ToUpper() == ".PKG")
                        .Take(2);

            if (files.Count() > 0)
            {
                var result = new DefinitionResult
                {
                    RequestId = request.Id,
                    Definitions = new VSCode.Models.Definition[] {
                        new VSCode.Models.Definition {
                            RawType = "",
                            Type = request.Lookup,
                            Text = "",
                            FileName = files.First(),
                            Range = new DefinitionRange {
                                StartLine = 0,
                                EndLine = 0,
                                StartColumn = 0,
                                EndColumn = 1
                            }
                        },
                        new VSCode.Models.Definition {
                            RawType = "",
                            Type = request.Lookup,
                            Text = "",
                            FileName = files.Last(),
                            Range = new DefinitionRange {
                                StartLine = 1,
                                EndLine = 2,
                                StartColumn = 0,
                                EndColumn = 1
                            }
                        }
                    }
                };

                return JsonConvert.SerializeObject(result, _serializerSettings);
            }

            return "";
        }
    }
}