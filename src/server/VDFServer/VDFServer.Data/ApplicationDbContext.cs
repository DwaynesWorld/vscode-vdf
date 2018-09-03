using Microsoft.EntityFrameworkCore;
using VDFServer.Data.Entities;

namespace VDFServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public string IndexPath { get; private set; } = "";
        public string WorkspaceRootFolder { get; private set; } = "";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            System.Diagnostics.Debug.WriteLine("New Context Created.");
        }

        public void InitializeDatabase(string indexPath, string workspaceRootFolder)
        {
            IndexPath = indexPath;
            WorkspaceRootFolder = WorkspaceRootFolder;

            Database.EnsureDeleted();
            Database.EnsureCreated();
            System.Diagnostics.Debug.WriteLine("Database Initialized.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LanguageSymbol>().HasIndex(s => s.Name);
            modelBuilder.Entity<SourceFile>().HasIndex(s => s.FilePath).IsUnique();
        }

        public DbSet<SourceFile> SourceFiles { get; set; }
        public DbSet<LanguageSymbol> Symbols { get; set; }
    }
}