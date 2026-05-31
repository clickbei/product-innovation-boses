using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BosesApp.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredLanguageToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredLanguage",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "UserProfiles");
        }
    }
}
