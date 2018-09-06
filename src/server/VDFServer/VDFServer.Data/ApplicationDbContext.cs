using Microsoft.EntityFrameworkCore;
using VDFServer.Data.Entities;

namespace VDFServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        private static string _indexPath = "";
        private static string _workspaceRootFolder = "";

        public string IndexPath { get => _indexPath; private set => _indexPath = value; }
        public string WorkspaceRootFolder { get => _workspaceRootFolder; private set => _workspaceRootFolder = value; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            System.Diagnostics.Debug.WriteLine("New Context Created.");
        }

        public void InitializeDatabase(string indexPath, string workspaceRootFolder)
        {
            IndexPath = indexPath;
            WorkspaceRootFolder = workspaceRootFolder;

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