using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationWebApplication.Migrations
{
    /// <inheritdoc />
    public partial class addedmfa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMfaEnabled",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MfaSecretKey",
                table: "User",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMfaEnabled",
                table: "User");

            migrationBuilder.DropColumn(
                name: "MfaSecretKey",
                table: "User");
        }
    }
}
