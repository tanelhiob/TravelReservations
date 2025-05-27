using Microsoft.EntityFrameworkCore;

namespace TravelReservations.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PriceList> PriceLists { get; set; } = null!;
    public DbSet<Leg> Legs { get; set; } = null!;
    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<PriceList>()
            .HasMany(p => p.Legs)
            .WithOne(l => l.PriceList!)
            .HasForeignKey(l => l.PriceListId);
            
        modelBuilder.Entity<Leg>()
            .HasMany(l => l.Providers)
            .WithOne(p => p.Leg!)
            .HasForeignKey(p => p.LegId);
            
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.PriceList!)
            .WithMany()
            .HasForeignKey(r => r.PriceListId);
    }
}