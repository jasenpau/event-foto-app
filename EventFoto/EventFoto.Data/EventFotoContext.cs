using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data;

public class EventFotoContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserCredential> UserCredentials { get; set; }

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
            entity.HasMany(u => u.Credentials)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);
        });

        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.ToTable("UserCredentials");
            entity.HasKey(u => u.Id);
            entity.Property(uc => uc.Type)
                .IsRequired();
            entity.Property(uc => uc.HashedPassword)
                .IsRequired(false)
                .HasMaxLength(255);
        });
    }   
}
