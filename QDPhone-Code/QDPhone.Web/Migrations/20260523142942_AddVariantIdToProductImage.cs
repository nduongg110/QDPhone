using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QDPhone.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantIdToProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "ProductImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" },
                unique: true,
                filter: "[VariantId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_VariantId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "VariantId", "SortOrder" },
                unique: true,
                filter: "[VariantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_VariantId",
                table: "ProductImages",
                column: "VariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductVariants_VariantId",
                table: "ProductImages",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductVariants_VariantId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_VariantId_SortOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_VariantId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "ProductImages");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_SortOrder",
                table: "ProductImages",
                columns: new[] { "ProductId", "SortOrder" },
                unique: true);
        }
    }
}
