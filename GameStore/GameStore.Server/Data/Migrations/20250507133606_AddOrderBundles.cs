using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderBundles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    BundleId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PriceAtPurchase = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderBundles_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderBundles_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderBundles_BundleId",
                table: "OrderBundles",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBundles_OrderId",
                table: "OrderBundles",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderBundles");
        }
    }
}
