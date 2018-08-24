using Microsoft.EntityFrameworkCore;
using VDFServer.Data.Models;

namespace VDFServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}