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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>().HasIndex(t => t.Name);
            modelBuilder.Entity<SourceFile>().HasIndex(s => s.FilePath).IsUnique();
        }

        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}