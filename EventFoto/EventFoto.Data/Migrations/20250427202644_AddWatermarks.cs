using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWatermarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WatermarkId",
                table: "Gallery",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WatermarkId",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Watermarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Filename = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watermarks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gallery_WatermarkId",
                table: "Gallery",
                column: "WatermarkId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_WatermarkId",
                table: "Events",
                column: "WatermarkId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Watermarks_WatermarkId",
                table: "Events",
                column: "WatermarkId",
                principalTable: "Watermarks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Gallery_Watermarks_WatermarkId",
                table: "Gallery",
                column: "WatermarkId",
                principalTable: "Watermarks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Watermarks_WatermarkId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Gallery_Watermarks_WatermarkId",
                table: "Gallery");

            migrationBuilder.DropTable(
                name: "Watermarks");

            migrationBuilder.DropIndex(
                name: "IX_Gallery_WatermarkId",
                table: "Gallery");

            migrationBuilder.DropIndex(
                name: "IX_Events_WatermarkId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "WatermarkId",
                table: "Gallery");

            migrationBuilder.DropColumn(
                name: "WatermarkId",
                table: "Events");
        }
    }
}
