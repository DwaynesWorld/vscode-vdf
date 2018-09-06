using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Entities
{
    public class LanguageSymbol
    {
        public string Id { get; set; }
        public SymbolKind Type { get; set; }
        public string Name { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public bool IsContainer { get; set; } = false;

        public string FileId { get; set; }
        public SourceFile File { get; set; }

        public string Container { get; set; } = null;
        public SymbolKind? ContainerType { get; set; } = null;
        public int? ContainerLine { get; set; } = null;


        [NotMapped]
        [JsonIgnore]
        public bool IsEndDeclaration { get; set; }
    }
}