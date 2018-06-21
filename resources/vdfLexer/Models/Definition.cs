namespace VdfLexer.Models {
    public class Definition {
        public string Name { get; set; }
        public DefinitionType Type { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}