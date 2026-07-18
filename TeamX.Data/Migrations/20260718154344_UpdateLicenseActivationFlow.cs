using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamX.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLicenseActivationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActivated",
                table: "Licenses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxDevices",
                table: "Licenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActivated",
                table: "Licenses");

            migrationBuilder.DropColumn(
                name: "MaxDevices",
                table: "Licenses");
        }
    }
}
