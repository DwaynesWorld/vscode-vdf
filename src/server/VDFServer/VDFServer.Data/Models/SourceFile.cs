using System;
using System.Collections.Generic;

namespace VDFServer.Data.Models
{
    public class SourceFile
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<LanguageSymbol> Symbols { get; set; }
    }
}