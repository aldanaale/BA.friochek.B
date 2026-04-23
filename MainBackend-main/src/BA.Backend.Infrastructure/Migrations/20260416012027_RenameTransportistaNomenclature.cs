using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTransportistaNomenclature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Routes_Users_TransportistId')
                BEGIN
                    ALTER TABLE [Routes] DROP CONSTRAINT [FK_Routes_Users_TransportistId];
                END
            ");

            migrationBuilder.RenameColumn(
                name: "TransportistId",
                table: "Routes",
                newName: "TransportistaId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_TransportistId",
                table: "Routes",
                newName: "IX_Routes_TransportistaId");

            migrationBuilder.RenameColumn(
                name: "TransportistId",
                table: "Mermas",
                newName: "TransportistaId");

            migrationBuilder.CreateTable(
                name: "ClientNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientNotes_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientNotes_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_AuthorId",
                table: "ClientNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_StoreId",
                table: "ClientNotes",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_TenantId",
                table: "ClientNotes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Users_TransportistaId",
                table: "Routes",
                column: "TransportistaId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Users_TransportistaId",
                table: "Routes");

            migrationBuilder.DropTable(
                name: "ClientNotes");

            migrationBuilder.RenameColumn(
                name: "TransportistaId",
                table: "Routes",
                newName: "TransportistId");

            migrationBuilder.RenameIndex(
                name: "IX_Routes_TransportistaId",
                table: "Routes",
                newName: "IX_Routes_TransportistId");

            migrationBuilder.RenameColumn(
                name: "TransportistaId",
                table: "Mermas",
                newName: "TransportistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Users_TransportistId",
                table: "Routes",
                column: "TransportistId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
