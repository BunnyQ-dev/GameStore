using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Games_GameId",
                table: "Achievements");

            migrationBuilder.RenameColumn(
                name: "UnlockedDate",
                table: "UserAchievements",
                newName: "UnlockedAt");

            migrationBuilder.RenameColumn(
                name: "PointsValue",
                table: "Achievements",
                newName: "Points");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Achievements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSecret",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Games_GameId",
                table: "Achievements",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Games_GameId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "IsSecret",
                table: "Achievements");

            migrationBuilder.RenameColumn(
                name: "UnlockedAt",
                table: "UserAchievements",
                newName: "UnlockedDate");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Achievements",
                newName: "PointsValue");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Achievements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Achievements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Games_GameId",
                table: "Achievements",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");
        }
    }
}
