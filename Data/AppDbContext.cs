using DezReunionWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace DezReunionWebsite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<EventItem> Events => Set<EventItem>();
    public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventItem>().Property(e => e.Price).HasPrecision(10, 2);
    }
}
