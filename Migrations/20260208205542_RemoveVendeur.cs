using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoutiqueElegance.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVendeur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_Users_VendeurId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_VendeurId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "VendeurId",
                table: "Restaurants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendeurId",
                table: "Restaurants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_VendeurId",
                table: "Restaurants",
                column: "VendeurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_Users_VendeurId",
                table: "Restaurants",
                column: "VendeurId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
