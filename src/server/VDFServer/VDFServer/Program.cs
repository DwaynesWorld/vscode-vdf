using System;
using Microsoft.EntityFrameworkCore;
using VDFServer.Data;
using VDFServer.Parser;

namespace VDFServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlite("Data Source=Test.db")
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .Options;

            using (var ctx = new ApplicationDbContext(options))
            {
                ctx.Database.EnsureCreated();
                var parser = new TagParser(ctx, "/Users/KT/Dev/HeavyBid");

                var watch = System.Diagnostics.Stopwatch.StartNew();

                parser.Run(false);

                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;

                Console.WriteLine($"Time: {elapsedMs}");
            }
        }
    }
}
