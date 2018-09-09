using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class Diagnostic
    {
        public string Message { get; set; }
        public DiagnosticSeverity Serverity { get; set; }
        public Range Range { get; set; }
    }
}