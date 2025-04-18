using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Leg> Legs { get; set; } = null!;
    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Leg>()
            .HasMany(l => l.Providers)
            .WithOne(p => p.Leg!)
            .HasForeignKey(p => p.LegId);
    }
}