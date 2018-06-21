using System;
using System.Collections.Generic;

namespace VdfLexer.Models {
    public class SourceFile {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
        public List<Definition> Objects { get; set; }
        public List<Definition> Procedures { get; set; }
        public List<Definition> Functions { get; set; }
    }
}