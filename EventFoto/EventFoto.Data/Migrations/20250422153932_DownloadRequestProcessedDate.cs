using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class DownloadRequestProcessedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "DownloadRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedOn",
                table: "DownloadRequests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedOn",
                table: "DownloadRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "DownloadRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
