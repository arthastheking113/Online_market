using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Online_market.Data.Migrations
{
    public partial class Initial4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Rate",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Value",
                table: "Rate",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ItemAttachmentTypeId",
                table: "ItemAttachment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "ItemAttachment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RateValue",
                table: "Item",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ItemAttachmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAttachmentTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rate_ItemId",
                table: "Rate",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAttachment_ItemAttachmentTypeId",
                table: "ItemAttachment",
                column: "ItemAttachmentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemAttachment_ItemAttachmentTypes_ItemAttachmentTypeId",
                table: "ItemAttachment",
                column: "ItemAttachmentTypeId",
                principalTable: "ItemAttachmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rate_Item_ItemId",
                table: "Rate",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemAttachment_ItemAttachmentTypes_ItemAttachmentTypeId",
                table: "ItemAttachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Rate_Item_ItemId",
                table: "Rate");

            migrationBuilder.DropTable(
                name: "ItemAttachmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Rate_ItemId",
                table: "Rate");

            migrationBuilder.DropIndex(
                name: "IX_ItemAttachment_ItemAttachmentTypeId",
                table: "ItemAttachment");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Rate");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Rate");

            migrationBuilder.DropColumn(
                name: "ItemAttachmentTypeId",
                table: "ItemAttachment");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "ItemAttachment");

            migrationBuilder.DropColumn(
                name: "RateValue",
                table: "Item");
        }
    }
}
