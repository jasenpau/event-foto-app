using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUploadBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadBatchId",
                table: "EventPhotos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UploadBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReady = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadBatches_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventPhotos_UploadBatchId",
                table: "EventPhotos",
                column: "UploadBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadBatches_UserId",
                table: "UploadBatches",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventPhotos_UploadBatches_UploadBatchId",
                table: "EventPhotos",
                column: "UploadBatchId",
                principalTable: "UploadBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventPhotos_UploadBatches_UploadBatchId",
                table: "EventPhotos");

            migrationBuilder.DropTable(
                name: "UploadBatches");

            migrationBuilder.DropIndex(
                name: "IX_EventPhotos_UploadBatchId",
                table: "EventPhotos");

            migrationBuilder.DropColumn(
                name: "UploadBatchId",
                table: "EventPhotos");
        }
    }
}
