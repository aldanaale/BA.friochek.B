using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fase6_AsignacionTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TechnicianId",
                table: "TechSupportRequests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TechSupportRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechSupportRequests_TechnicianId",
                table: "TechSupportRequests",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_TechSupportRequests_Users_TechnicianId",
                table: "TechSupportRequests",
                column: "TechnicianId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechSupportRequests_Users_TechnicianId",
                table: "TechSupportRequests");

            migrationBuilder.DropIndex(
                name: "IX_TechSupportRequests_TechnicianId",
                table: "TechSupportRequests");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "TechSupportRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TechSupportRequests");
        }
    }
}
