using VDFServer.Data.Enumerations;

namespace VDFServer.Data.Models
{
    public class CommandResult
    {
        public int RequestId { get; set; } = -1;
        public bool IsInternal = false;
        public IPCMessage MessageType { get; set; } = IPCMessage.None;
        public string Message { get; set; } = "";
        public string Metadata { get; set; } = "";
    }
}