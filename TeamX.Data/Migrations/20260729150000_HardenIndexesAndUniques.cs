using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamX.Data.Migrations;

public partial class HardenIndexesAndUniques : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotência webhook
        migrationBuilder.CreateIndex(
            name: "IX_Orders_TransactionId",
            table: "Orders",
            column: "TransactionId",
            unique: true);

        // Cliente único por e-mail
        migrationBuilder.CreateIndex(
            name: "IX_Customers_Email",
            table: "Customers",
            column: "Email",
            unique: true);

        // Variant Eremby único
        migrationBuilder.CreateIndex(
            name: "IX_Plans_Code",
            table: "Plans",
            column: "Code",
            unique: true);

        // JWT blacklist
        migrationBuilder.CreateIndex(
            name: "IX_RevokedTokens_Jti",
            table: "RevokedTokens",
            column: "Jti",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RevokedTokens_ExpiresAt",
            table: "RevokedTokens",
            column: "ExpiresAt");

        // Performance
        migrationBuilder.CreateIndex(
            name: "IX_Licenses_Status",
            table: "Licenses",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_LicenseDevices_HardwareId",
            table: "LicenseDevices",
            column: "HardwareId");

        migrationBuilder.CreateIndex(
            name: "IX_LicenseDevices_LastSeen",
            table: "LicenseDevices",
            column: "LastSeen");

        migrationBuilder.CreateIndex(
            name: "IX_LicenseAuditLogs_LicenseId_CreatedAt",
            table: "LicenseAuditLogs",
            columns: new[] { "LicenseId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_LicenseAuditLogs_EventType",
            table: "LicenseAuditLogs",
            column: "EventType");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Orders_TransactionId", table: "Orders");
        migrationBuilder.DropIndex(name: "IX_Customers_Email", table: "Customers");
        migrationBuilder.DropIndex(name: "IX_Plans_Code", table: "Plans");
        migrationBuilder.DropIndex(name: "IX_RevokedTokens_Jti", table: "RevokedTokens");
        migrationBuilder.DropIndex(name: "IX_RevokedTokens_ExpiresAt", table: "RevokedTokens");
        migrationBuilder.DropIndex(name: "IX_Licenses_Status", table: "Licenses");
        migrationBuilder.DropIndex(name: "IX_LicenseDevices_HardwareId", table: "LicenseDevices");
        migrationBuilder.DropIndex(name: "IX_LicenseDevices_LastSeen", table: "LicenseDevices");
        migrationBuilder.DropIndex(name: "IX_LicenseAuditLogs_LicenseId_CreatedAt", table: "LicenseAuditLogs");
        migrationBuilder.DropIndex(name: "IX_LicenseAuditLogs_EventType", table: "LicenseAuditLogs");
    }
}