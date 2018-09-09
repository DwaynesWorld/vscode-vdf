using System.Collections.Generic;

namespace VDFServer.Data.Models
{
    public class HoverResult : CommandResult
    {
        public List<HoverItem> Items { get; set; }
    }

    public class HoverItem
    {
        public string Main { get; set; }
        public string Content { get; set; }
        public string Metadata { get; set; }
    }
}