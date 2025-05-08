using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameStore.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCartBundle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanInvites");

            migrationBuilder.DropTable(
                name: "ClanMembers");

            migrationBuilder.DropTable(
                name: "PublisherDevelopers");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.CreateTable(
                name: "CartBundles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    BundleId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartBundles_Bundles_BundleId",
                        column: x => x.BundleId,
                        principalTable: "Bundles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartBundles_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartBundles_BundleId",
                table: "CartBundles",
                column: "BundleId");

            migrationBuilder.CreateIndex(
                name: "IX_CartBundles_CartId",
                table: "CartBundles",
                column: "CartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartBundles");

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clans_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublisherDevelopers",
                columns: table => new
                {
                    PublisherId = table.Column<int>(type: "int", nullable: false),
                    DeveloperId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherDevelopers", x => new { x.PublisherId, x.DeveloperId });
                    table.ForeignKey(
                        name: "FK_PublisherDevelopers_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublisherDevelopers_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClanInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClanId = table.Column<int>(type: "int", nullable: false),
                    InvitedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvitedUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Accepted = table.Column<bool>(type: "bit", nullable: false),
                    ClanId1 = table.Column<int>(type: "int", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClanInvites_AspNetUsers_InvitedById",
                        column: x => x.InvitedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClanInvites_AspNetUsers_InvitedUserId",
                        column: x => x.InvitedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClanInvites_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanInvites_Clans_ClanId1",
                        column: x => x.ClanId1,
                        principalTable: "Clans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClanMembers",
                columns: table => new
                {
                    ClanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanMembers", x => new { x.ClanId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ClanMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClanMembers_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_ClanId",
                table: "ClanInvites",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_ClanId1",
                table: "ClanInvites",
                column: "ClanId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_InvitedById",
                table: "ClanInvites",
                column: "InvitedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_InvitedUserId",
                table: "ClanInvites",
                column: "InvitedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_UserId",
                table: "ClanMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_OwnerId",
                table: "Clans",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PublisherDevelopers_DeveloperId",
                table: "PublisherDevelopers",
                column: "DeveloperId");
        }
    }
}
