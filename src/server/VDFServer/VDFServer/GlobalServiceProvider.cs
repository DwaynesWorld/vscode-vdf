using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VDFServer.Data;
using VDFServer.Parser;
using VDFServer.Parser.Service;

namespace VDFServer
{
    public sealed class GlobalServiceProvider
    {
        private static GlobalServiceProvider _instance = null;
        private static readonly object _lock = new object();

        public ServiceProvider ServiceProvider { get; private set; }

        public static GlobalServiceProvider Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GlobalServiceProvider();
                    }
                    return _instance;
                }
            }
        }

        GlobalServiceProvider()
        {
            var indexFile = $"{Hasher.GetStringHash(ApplicationDbContext.WorkspaceRootFolder)}.db";
            var indexFullName = Path.Combine(ApplicationDbContext.IndexPath, indexFile);

            // Setup DI
            ServiceProvider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlite($"Data Source={indexFullName}")
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
                    ServiceLifetime.Transient,
                    ServiceLifetime.Singleton)
                .AddTransient<IProvider, Provider>()
                .AddTransient<ISymbolParser, SymbolParser>()
                .AddTransient<IInternalParser, InternalParser>()
                .BuildServiceProvider();
        }

        public class Hasher
        {
            public static string GetStringHash(string value)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in GetHash(value))
                    sb.Append(b.ToString("X2"));

                return sb.ToString();
            }

            private static byte[] GetHash(string value)
            {
                HashAlgorithm algorithm = SHA256.Create();
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
        }
    }
}
