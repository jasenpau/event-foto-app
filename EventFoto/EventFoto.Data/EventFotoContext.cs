using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data;

public class EventFotoContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email)
                .IsUnique();
            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.HasIndex(e => e.Name)
                .IsUnique();
            entity.Property(e => e.Note)
                .IsRequired(false)
                .HasMaxLength(500);
            entity.Property(e => e.Location)
                .IsRequired(false)
                .HasMaxLength(255);
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .HasPrincipalKey(u => u.Id);
            entity.HasMany(e => e.Photographers)
                .WithMany(u => u.AssignedPhotographerEvents);
            entity.ToTable("Events");
        });
    }   
}
