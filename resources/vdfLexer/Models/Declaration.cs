namespace VdfLexer.Models {
    public class Declaration {
        public string Name { get; set; }
        public DeclarationType Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}