using VDFServer.Data.Enumerations;

namespace VDFServer.Models
{
    public class Definition
    {

        public string RawType { get; set; }
        public CommandType Type { get; set; }
        public string Text { get; set; }
        public string FilePath { get; set; }
        public DefinitionRange Range { get; set; }
    }
}