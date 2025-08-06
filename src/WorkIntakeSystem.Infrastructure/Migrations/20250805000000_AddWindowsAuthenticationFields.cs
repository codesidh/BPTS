using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkIntakeSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWindowsAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WindowsIdentity",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiveDirectorySid",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWindowsAuthenticated",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAdSync",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdGroups",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            // Add indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_Users_WindowsIdentity",
                table: "Users",
                column: "WindowsIdentity",
                unique: true,
                filter: "[WindowsIdentity] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveDirectorySid",
                table: "Users",
                column: "ActiveDirectorySid",
                unique: true,
                filter: "[ActiveDirectorySid] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveDirectorySid",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_WindowsIdentity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AdGroups",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastAdSync",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsWindowsAuthenticated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveDirectorySid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WindowsIdentity",
                table: "Users");
        }
    }
} 