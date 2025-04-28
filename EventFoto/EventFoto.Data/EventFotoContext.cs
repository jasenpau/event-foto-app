using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data;

public class EventFotoContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventPhoto> EventPhotos { get; set; }
    public DbSet<Gallery> Galleries { get; set; }
    public DbSet<DownloadRequest> DownloadRequests { get; set; }
    public DbSet<UploadBatch> UploadBatches { get; set; }
    public DbSet<Watermark> Watermarks { get; set; }
    public DbSet<PhotographerAssignment> PhotographerAssignments { get; set; }

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
            entity.Property(u => u.IsActive)
                .IsRequired()
                .HasDefaultValue(false);
            entity.Property(u => u.InvitationKey)
                .IsRequired(false)
                .HasMaxLength(255);
            entity.HasIndex(u => u.InvitationKey)
                .IsUnique();
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
            entity.HasOne(e => e.DefaultGallery)
                .WithOne()
                .HasForeignKey<Event>(e => e.DefaultGalleryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Watermark)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable("Events");
        });

        modelBuilder.Entity<EventPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.UploadDate)
                .IsRequired();
            entity.Property(e => e.CaptureDate)
                .IsRequired();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(u => u.Id);
            entity.HasOne(e => e.Gallery)
                .WithMany(e => e.Photos)
                .HasForeignKey(e => e.GalleryId)
                .HasPrincipalKey(e => e.Id);
            entity.HasOne(e => e.UploadBatch)
                .WithMany(e => e.EventPhotos)
                .HasForeignKey(e => e.UploadBatchId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable("EventPhotos");
        });

        modelBuilder.Entity<Gallery>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            entity.HasOne(g => g.Event)
                .WithMany(e => e.Galleries)
                .HasForeignKey(g => g.EventId)
                .HasPrincipalKey(e => e.Id);
            entity.HasOne(g => g.Watermark)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            entity.ToTable("Gallery");
        });

        modelBuilder.Entity<DownloadRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.DownloadImages)
                .WithOne(e => e.DownloadRequest);
            entity.ToTable("DownloadRequests");
        });

        modelBuilder.Entity<DownloadImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.EventPhoto)
                .WithMany()
                .HasForeignKey(e => e.EventPhotoId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.DownloadRequest)
                .WithMany(r => r.DownloadImages)
                .HasForeignKey(e => e.DownloadRequestId);
            entity.ToTable("DownloadImages");
        });

        modelBuilder.Entity<UploadBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId);
            entity.ToTable("UploadBatches");
        });

        modelBuilder.Entity<PhotographerAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Gallery)
                .WithMany(u => u.Assignments)
                .HasForeignKey(e => e.GalleryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
