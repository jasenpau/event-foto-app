using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloadQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quality",
                table: "DownloadRequests",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quality",
                table: "DownloadRequests");
        }
    }
}
