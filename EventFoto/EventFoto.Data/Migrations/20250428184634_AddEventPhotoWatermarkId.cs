using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEventPhotoWatermarkId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WatermarkId",
                table: "EventPhotos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WatermarkId",
                table: "EventPhotos");
        }
    }
}
