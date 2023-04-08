using CodingCompetitionPlatform.Model;
using Microsoft.EntityFrameworkCore;

namespace CodingCompetitionPlatform.Services
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns(); 
        }

        public DbSet<CompetitorModel> Competitors { get; set; }
        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<ProblemStatusModel> ProblemStatuses { get; set; }
    }
}
