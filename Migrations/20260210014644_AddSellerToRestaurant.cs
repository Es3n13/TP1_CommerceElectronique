using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoutiqueElegance.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerToRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Restaurants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Users_SellerId",
                table: "Restaurants",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Users_SellerId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Restaurants");
        }
    }
}
