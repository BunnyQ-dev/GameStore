using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAverageRatingToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AverageRating",
                table: "Games",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Games",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Games");
        }
    }
}
