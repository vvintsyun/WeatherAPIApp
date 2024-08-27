using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherAppAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllowedRate",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRate",
                table: "Users");
        }
    }
}
