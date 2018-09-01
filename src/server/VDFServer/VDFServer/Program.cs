using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VDFServer.Data;
using VDFServer.Parser;

namespace VDFServer
{
    class Program
    {
        private static string _indexPath;
        private static string _workspaceRootPath;
        private static DbContextOptions<ApplicationDbContext> _options;

        static void Main(string[] args)
        {
            WaitForDebugger();
            HandleArguments(args);
            InitializeDatabase();

            var parser = new SymbolParser(_options, _workspaceRootPath);
            parser.Start();

            int length;
            var buffer = new byte[1024];
            var input = Console.OpenStandardInput();
            while (input.CanRead && (length = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var message = new byte[length];
                Buffer.BlockCopy(buffer, 0, message, 0, length);
                var load = Encoding.UTF8.GetString(message);

                System.Diagnostics.Debug.WriteLine(load);
                var payloads = load.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                foreach (var payload in payloads)
                {
                    if (payload.Trim().Length > 0)
                    {
                        Task.Run(() =>
                        {
                            var provider = new Provider(_options);
                            Console.WriteLine(provider.Provide(payload));
                        });
                    }
                }

                Console.Out.Flush();
            }
        }

        private static void HandleArguments(string[] args)
        {
            if (args.Length < 1)
                throw new ApplicationException("Required arguments missing: IndexPath, WorkspaceRootPath");

            if (args.Length < 2)
                throw new ApplicationException("Required argument missing: WorkspaceRootPath");

            _indexPath = args[0];
            _workspaceRootPath = args[1];

            if (!Directory.Exists(_indexPath))
                Directory.CreateDirectory(_indexPath);

            if (!Directory.Exists(_indexPath))
                throw new ApplicationException($"Unable to create required directory: {_indexPath}");

            if (!Directory.Exists(_workspaceRootPath))
                throw new ApplicationException($"Unable to locate workspace root folder: {_workspaceRootPath}");
        }

        private static void InitializeDatabase()
        {
            var indexFile = $"{Hasher.GetStringHash(_workspaceRootPath)}.db";
            var indexFullName = Path.Combine(_indexPath, indexFile);

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite($"Data Source={indexFullName}")
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            using (var ctx = new ApplicationDbContext(_options))
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void WaitForDebugger()
        {
            System.Threading.Thread.Sleep(10000);
        }
    }
}