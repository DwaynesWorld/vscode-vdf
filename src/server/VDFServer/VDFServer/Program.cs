using System;
using System.IO;
using System.Text;
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
        static void Main(string[] args)
        {
            HandleArguments(args);
            var provider = new Provider(_indexPath, _workspaceRootPath);

            int length;
            var buffer = new byte[1024];
            var input = Console.OpenStandardInput();
            while (input.CanRead && (length = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                var message = new byte[length];
                Buffer.BlockCopy(buffer, 0, message, 0, length);
                var payload = Encoding.UTF8.GetString(message);

                System.Diagnostics.Debug.WriteLine(payload);

                Console.Write(provider.Provide(payload));
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
    }
}