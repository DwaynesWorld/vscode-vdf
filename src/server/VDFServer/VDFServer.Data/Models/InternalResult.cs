using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class InternalResult : CommandResult
    {
        public IPCMessage MessageType { get; set; }
        public string Message { get; set; } = "";
        public string MetaData { get; set; } = "";
    }
}