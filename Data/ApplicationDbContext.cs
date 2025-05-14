using Microsoft.EntityFrameworkCore;
using Sommus.DengueApi.Models;

namespace Sommus.DengueApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<DengueDados> DengueDados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DengueDados>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SemanaEpidemiologica)
                .HasMaxLength(10)
                .IsRequired();
            entity.HasIndex(e => e.SemanaEpidemiologica)
                .IsUnique();
        });
    }
}