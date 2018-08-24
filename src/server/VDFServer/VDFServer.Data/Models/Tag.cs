using System;
using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TagType Type { get; set; }
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }

        public string FileId { get; set; }
        public SourceFile File { get; set; }
    }
}