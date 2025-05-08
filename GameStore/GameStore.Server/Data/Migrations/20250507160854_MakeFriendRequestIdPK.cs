using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeFriendRequestIdPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropPrimaryKey(
        name: "PK_FriendRequests",
        table: "FriendRequests");


    migrationBuilder.DropColumn(
        name: "Id",
        table: "FriendRequests");


    migrationBuilder.AddColumn<int>(
        name: "Id",
        table: "FriendRequests",
        type: "int",
        nullable: false,
        defaultValue: 0)
        .Annotation("SqlServer:Identity", "1, 1");


    migrationBuilder.AddPrimaryKey(
        name: "PK_FriendRequests",
        table: "FriendRequests",
        column: "Id");

    migrationBuilder.CreateIndex(
        name: "IX_FriendRequests_SenderId_ReceiverId",
        table: "FriendRequests",
        columns: new[] { "SenderId", "ReceiverId" },
        unique: true);
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FriendRequests",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_SenderId_ReceiverId",
                table: "FriendRequests");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercentage",
                table: "Games",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FriendRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FriendRequests",
                table: "FriendRequests",
                columns: new[] { "SenderId", "ReceiverId" });
        }
    }
}
