using System;
using System.Collections.Generic;
using System.Linq;

namespace VdfLexer.Models {
    public class LanguageIndex {
        public DateTime LastUpdated { get; set; }
        public Dictionary<string, SourceFile> Files { get; set; }
    }
}