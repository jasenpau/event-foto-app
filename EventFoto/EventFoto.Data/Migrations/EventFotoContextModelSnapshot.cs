﻿// <auto-generated />
using System;
using EventFoto.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventFoto.Data.Migrations
{
    [DbContext(typeof(EventFotoContext))]
    partial class EventFotoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("EventFoto.Data.Models.DownloadImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("DownloadRequestId")
                        .HasColumnType("integer");

                    b.Property<int>("EventPhotoId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DownloadRequestId");

                    b.HasIndex("EventPhotoId");

                    b.ToTable("DownloadImages", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.DownloadRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("DownloadProcessedPhotos")
                        .HasColumnType("boolean");

                    b.Property<string>("Filename")
                        .HasColumnType("text");

                    b.Property<bool>("IsReady")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("ProcessedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("Quality")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("DownloadRequests", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.Event", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ArchiveName")
                        .HasColumnType("text");

                    b.Property<Guid>("CreatedBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DefaultGalleryId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<string>("Location")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Note")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("WatermarkId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("DefaultGalleryId")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("WatermarkId");

                    b.ToTable("Events", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.EventPhoto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CaptureDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("GalleryId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean");

                    b.Property<string>("ProcessedFilename")
                        .HasColumnType("text");

                    b.Property<int?>("UploadBatchId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<int?>("WatermarkId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GalleryId");

                    b.HasIndex("UploadBatchId");

                    b.HasIndex("UserId");

                    b.ToTable("EventPhotos", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.Gallery", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("EventId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int?>("WatermarkId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("EventId");

                    b.HasIndex("WatermarkId");

                    b.ToTable("Gallery", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.PhotographerAssignment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("GalleryId")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("GalleryId");

                    b.HasIndex("UserId");

                    b.ToTable("PhotographerAssignments");
                });

            modelBuilder.Entity("EventFoto.Data.Models.UploadBatch", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsReady")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("ProcessedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UploadBatches", (string)null);
                });

            modelBuilder.Entity("EventFoto.Data.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<Guid?>("GroupAssignment")
                        .HasColumnType("uuid");

                    b.Property<string>("InvitationKey")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<DateTime>("InvitedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("InvitationKey")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("EventFoto.Data.Models.Watermark", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Filename")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Watermarks");
                });

            modelBuilder.Entity("EventFoto.Data.Models.DownloadImage", b =>
                {
                    b.HasOne("EventFoto.Data.Models.DownloadRequest", "DownloadRequest")
                        .WithMany("DownloadImages")
                        .HasForeignKey("DownloadRequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.EventPhoto", "EventPhoto")
                        .WithMany()
                        .HasForeignKey("EventPhotoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DownloadRequest");

                    b.Navigation("EventPhoto");
                });

            modelBuilder.Entity("EventFoto.Data.Models.DownloadRequest", b =>
                {
                    b.HasOne("EventFoto.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("EventFoto.Data.Models.Event", b =>
                {
                    b.HasOne("EventFoto.Data.Models.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedBy")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.Gallery", "DefaultGallery")
                        .WithOne()
                        .HasForeignKey("EventFoto.Data.Models.Event", "DefaultGalleryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.Watermark", "Watermark")
                        .WithMany()
                        .HasForeignKey("WatermarkId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("CreatedByUser");

                    b.Navigation("DefaultGallery");

                    b.Navigation("Watermark");
                });

            modelBuilder.Entity("EventFoto.Data.Models.EventPhoto", b =>
                {
                    b.HasOne("EventFoto.Data.Models.Gallery", "Gallery")
                        .WithMany("Photos")
                        .HasForeignKey("GalleryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.UploadBatch", "UploadBatch")
                        .WithMany("EventPhotos")
                        .HasForeignKey("UploadBatchId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("EventFoto.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gallery");

                    b.Navigation("UploadBatch");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EventFoto.Data.Models.Gallery", b =>
                {
                    b.HasOne("EventFoto.Data.Models.Event", "Event")
                        .WithMany("Galleries")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.Watermark", "Watermark")
                        .WithMany()
                        .HasForeignKey("WatermarkId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Event");

                    b.Navigation("Watermark");
                });

            modelBuilder.Entity("EventFoto.Data.Models.PhotographerAssignment", b =>
                {
                    b.HasOne("EventFoto.Data.Models.Gallery", "Gallery")
                        .WithMany("Assignments")
                        .HasForeignKey("GalleryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EventFoto.Data.Models.User", "User")
                        .WithMany("Assignments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Gallery");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EventFoto.Data.Models.UploadBatch", b =>
                {
                    b.HasOne("EventFoto.Data.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("EventFoto.Data.Models.DownloadRequest", b =>
                {
                    b.Navigation("DownloadImages");
                });

            modelBuilder.Entity("EventFoto.Data.Models.Event", b =>
                {
                    b.Navigation("Galleries");
                });

            modelBuilder.Entity("EventFoto.Data.Models.Gallery", b =>
                {
                    b.Navigation("Assignments");

                    b.Navigation("Photos");
                });

            modelBuilder.Entity("EventFoto.Data.Models.UploadBatch", b =>
                {
                    b.Navigation("EventPhotos");
                });

            modelBuilder.Entity("EventFoto.Data.Models.User", b =>
                {
                    b.Navigation("Assignments");
                });
#pragma warning restore 612, 618
        }
    }
}
