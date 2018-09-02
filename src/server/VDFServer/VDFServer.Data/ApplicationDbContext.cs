using Microsoft.EntityFrameworkCore;
using VDFServer.Data.Models;

namespace VDFServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public static string WorkspaceRootFolder = "";
        public static string IndexPath = "";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguageSymbol>().HasIndex(s => s.Name);
            modelBuilder.Entity<SourceFile>().HasIndex(s => s.FilePath).IsUnique();
        }

        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<LanguageSymbol> Symbols { get; set; }
    }
}