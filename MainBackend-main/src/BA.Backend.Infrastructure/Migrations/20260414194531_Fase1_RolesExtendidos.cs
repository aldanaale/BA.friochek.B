using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BA.Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fase1_RolesExtendidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Products");

            migrationBuilder.AddColumn<byte>(
                name: "ClientType",
                table: "Users",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TransportType",
                table: "Users",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "TransportType",
                table: "Transportistas",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderUrl",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntegrationConfigJson",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntegrationType",
                table: "Tenants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RedirectTemplate",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSku",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalStatus",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "Transportistas");

            migrationBuilder.DropColumn(
                name: "ExternalOrderUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IntegrationConfigJson",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IntegrationType",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RedirectTemplate",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ExternalSku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalStatus",
                table: "Orders");

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
