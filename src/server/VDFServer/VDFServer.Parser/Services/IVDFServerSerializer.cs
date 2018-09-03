namespace VDFServer.Parser.Services
{
    public interface IVDFServerSerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string obj);
    }
}