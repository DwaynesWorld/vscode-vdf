using System.Collections.Generic;
using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class HoverResult : CommandResult
    {
        public string Main { get; set; }
        public string Contents { get; set; } = "";
        public string HoverMetadata { get; set; } = "";

        public string GetStyledMainSection(string name, SymbolKind type)
        {
            var t = type == SymbolKind.Method ? "Procedure" : type.ToString();
            return $"({t}) | {name}";
        }

        public string GetStyledMetaDataSection(string metadata)
        {
            // Add some markdown
            return $"{metadata}";
        }
    }
}