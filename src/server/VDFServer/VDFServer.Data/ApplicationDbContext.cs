using Microsoft.EntityFrameworkCore;
using VDFServer.Data.Models;

namespace VDFServer.Data
{
    public class ApplicationDbContext : DbContext
    {

        public const int CurrentVersion = 1;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name);
            modelBuilder.Entity<SourceFile>().HasIndex(s => s.FilePath).IsUnique();
            modelBuilder.Entity<IndexVersion>().HasKey(i => i.Version);
        }

        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<IndexVersion> IndexVersion { get; set; }
    }
}