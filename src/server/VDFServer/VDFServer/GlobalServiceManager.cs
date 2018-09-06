using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VDFServer.Data;
using VDFServer.Parser;
using VDFServer.Parser.Services;

namespace VDFServer
{
    public sealed partial class GlobalServiceManager
    {
        private static GlobalServiceManager _instance = null;
        private static readonly object _lock = new object();

        public ServiceProvider ServiceProvider { get; private set; }

        public static GlobalServiceManager Instance
        {
            get
            {
                lock(_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new GlobalServiceManager();
                    }
                    return _instance;
                }
            }
        }

        GlobalServiceManager() { }

        public void Initialize(string indexPath, string workspaceRootFolder)
        {
            var indexFile = $"{Hasher.GetStringHash(workspaceRootFolder)}.db";
            var indexFullPath = Path.Combine(indexPath, indexFile);

            // Setup DI
            ServiceProvider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                    options
                    .UseSqlite($"Data Source={indexFullPath}")
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking),
                    ServiceLifetime.Transient,
                    ServiceLifetime.Singleton)
                .AddTransient<IProvider, Provider>()
                .AddTransient<IWorkspaceSymbolParser, WorkspaceSymbolParser>()
                .AddTransient<IInternalParser, InternalParser>()
                .AddSingleton<IVDFServerSerializer, VDFServerSerializer>()
                .BuildServiceProvider();
        }
    }
}