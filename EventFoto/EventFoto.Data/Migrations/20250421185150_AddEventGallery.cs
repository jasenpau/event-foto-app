using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhotos_Events_EventId",
                table: "EventPhotos");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "EventPhotos",
                newName: "GalleryId");

            migrationBuilder.RenameIndex(
                name: "IX_EventPhotos_EventId",
                table: "EventPhotos",
                newName: "IX_EventPhotos_GalleryId");

            migrationBuilder.AddColumn<int>(
                name: "DefaultGalleryId",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Gallery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gallery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gallery_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_DefaultGalleryId",
                table: "Events",
                column: "DefaultGalleryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_EventId",
                table: "Gallery",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhotos_Gallery_GalleryId",
                table: "EventPhotos",
                column: "GalleryId",
                principalTable: "Gallery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Gallery_DefaultGalleryId",
                table: "Events",
                column: "DefaultGalleryId",
                principalTable: "Gallery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                ALTER TABLE ""Events""
                DROP CONSTRAINT IF EXISTS ""FK_Events_Gallery_DefaultGalleryId"";

                ALTER TABLE ""Events""
                ADD CONSTRAINT ""FK_Events_Gallery_DefaultGalleryId""
                FOREIGN KEY (""DefaultGalleryId"") REFERENCES ""Gallery""(""Id"")
                ON DELETE RESTRICT
                DEFERRABLE INITIALLY DEFERRED;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhotos_Gallery_GalleryId",
                table: "EventPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Gallery_DefaultGalleryId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Gallery");

            migrationBuilder.DropIndex(
                name: "IX_Events_DefaultGalleryId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "DefaultGalleryId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "GalleryId",
                table: "EventPhotos",
                newName: "EventId");

            migrationBuilder.RenameIndex(
                name: "IX_EventPhotos_GalleryId",
                table: "EventPhotos",
                newName: "IX_EventPhotos_EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhotos_Events_EventId",
                table: "EventPhotos",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
