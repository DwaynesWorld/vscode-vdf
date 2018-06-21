using System;
using System.Collections.Generic;

namespace VdfLexer.Models {
    public class SourceFile {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
        public List<Declaration> Objects { get; set; }
        public List<Declaration> Procedures { get; set; }
        public List<Declaration> Functions { get; set; }
    }
}