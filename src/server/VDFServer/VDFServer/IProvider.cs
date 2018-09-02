using System;

namespace VDFServer
{
    public interface IProvider
    {
        string Provide(string incomingPayload);
    }
}