using System.Collections.Generic;

namespace VDFServer.Models
{
    public class DefinitionResult : CommandResult
    {
        public List<Definition> Definitions { get; set; }
    }
}