using Microsoft.EntityFrameworkCore.Migrations;

namespace Online_market.Data.Migrations
{
    public partial class Initial8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ItemId",
                table: "AspNetUsers",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Item_ItemId",
                table: "AspNetUsers",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Item_ItemId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ItemId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "AspNetUsers");
        }
    }
}
