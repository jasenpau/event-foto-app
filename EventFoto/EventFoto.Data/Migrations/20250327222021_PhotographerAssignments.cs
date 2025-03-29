using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventFoto.Data.Migrations
{
    /// <inheritdoc />
    public partial class PhotographerAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventUser",
                columns: table => new
                {
                    AssignedPhotographerEventsId = table.Column<int>(type: "integer", nullable: false),
                    PhotographersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventUser", x => new { x.AssignedPhotographerEventsId, x.PhotographersId });
                    table.ForeignKey(
                        name: "FK_EventUser_Events_AssignedPhotographerEventsId",
                        column: x => x.AssignedPhotographerEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventUser_Users_PhotographersId",
                        column: x => x.PhotographersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventUser_PhotographersId",
                table: "EventUser",
                column: "PhotographersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventUser");
        }
    }
}
