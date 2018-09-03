using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VDFServer.Data;
using VDFServer.Parser;
using VDFServer.Parser.Services;

namespace VDFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //WaitForDebugging();
            (string indexPath, string workspaceRootFolder) = HandleArguments(args);

            var gsm = GlobalServiceManager.Instance;
            gsm.Initialize(indexPath, workspaceRootFolder);

            // Initialize Database
            gsm.ServiceProvider
                .GetService<ApplicationDbContext>()
                .InitializeDatabase(indexPath, workspaceRootFolder);

            // Start Parsing Workspace
            gsm.ServiceProvider
                .GetService<ISymbolParser>()
                .Start();

            int length;
            var buffer = new byte[1024];
            var input = Console.OpenStandardInput();
            while (input.CanRead && (length = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var message = new byte[length];
                Buffer.BlockCopy(buffer, 0, message, 0, length);
                var load = Encoding.UTF8.GetString(message);

                System.Diagnostics.Debug.WriteLine(load);
                var payloads = load.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var payload in payloads)
                {
                    Task.Run(() =>
                    {
                        var provider = GlobalServiceManager.Instance
                            .ServiceProvider
                            .GetService<IProvider>();

                        Console.WriteLine(provider.Provide(payload.Trim()));
                        Console.Out.Flush();
                    });
                }
            }
        }

        private static (string indexPath, string workspaceRootPath) HandleArguments(string[] args)
        {
            if (args.Length < 1)
                throw new ApplicationException("Required arguments missing: IndexPath, WorkspaceRootPath");

            if (args.Length < 2)
                throw new ApplicationException("Required argument missing: WorkspaceRootPath");

            var indexPath = args[0];
            var workspaceRootPath = args[1];

            if (!Directory.Exists(indexPath))
                Directory.CreateDirectory(indexPath);

            if (!Directory.Exists(indexPath))
                throw new ApplicationException($"Unable to create required directory: {indexPath}");

            if (!Directory.Exists(workspaceRootPath))
                throw new ApplicationException($"Unable to locate workspace root folder: {workspaceRootPath}");

            return (indexPath, workspaceRootPath);
        }



        [System.Diagnostics.Conditional("DEBUG")]
        private static void WaitForDebugging()
        {
            System.Threading.Thread.Sleep(15000);
        }
    }
}