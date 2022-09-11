using APIServer.Models;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DailyCaseSummary>()
                .ToTable("covid_data")
                .HasKey(nameof(DailyCaseSummary.date), nameof(DailyCaseSummary.region));
        }

        public DbSet<DailyCaseSummary> DailyCaseSummaries { get; set; }
    }
}