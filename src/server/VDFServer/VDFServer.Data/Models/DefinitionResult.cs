using System.Collections.Generic;

namespace VDFServer.Data.Models
{
    public class DefinitionResult : CommandResult
    {
        public List<Definition> Definitions { get; set; }
    }
}