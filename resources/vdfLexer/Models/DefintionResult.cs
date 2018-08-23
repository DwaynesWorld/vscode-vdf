namespace VdfLexer.VSCode.Models
{

    public class CommandResult
    {
        public int RequestId { get; set; }
    }

    public class DefinitionResult : CommandResult
    {
        public Definition[] Definitions { get; set; }
    }

    public class Definition
    {

        public string RawType { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }
        public string FileName { get; set; }
        public DefinitionRange Range { get; set; }
    }

    public class DefinitionRange
    {
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
    }
}