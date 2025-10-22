using LogCompilerBeta.Entities.YourProjectName.Models;
using Microsoft.EntityFrameworkCore;

namespace LogCompilerBeta.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<OriginalMessage> OriginalMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OriginalMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message)
                      .IsRequired()
                      .HasColumnType("nvarchar(MAX)");

                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}