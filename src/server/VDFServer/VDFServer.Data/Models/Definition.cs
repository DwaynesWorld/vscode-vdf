using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class Definition
    {
        public string RawType { get; set; }
        public CommandType Type { get; set; }
        public string Text { get; set; }
        public SymbolKind Kind { get; set; }
        public string Container { get; set; }
        public string FilePath { get; set; }
        public DefinitionRange Range { get; set; }
    }
}