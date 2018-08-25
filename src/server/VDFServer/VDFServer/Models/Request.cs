namespace VDFServer.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Lookup { get; set; }
        public string PossibleWord { get; set; }
        public string Path { get; set; }
        public string Source { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string WorkspacePath { get; set; }
    }
}