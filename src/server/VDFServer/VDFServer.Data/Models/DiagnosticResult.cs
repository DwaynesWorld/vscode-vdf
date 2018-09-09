using System.Collections.Generic;

namespace VDFServer.Data.Models
{
    public class DiagnosticResult : CommandResult
    {
        public string FilePath { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
    }
}