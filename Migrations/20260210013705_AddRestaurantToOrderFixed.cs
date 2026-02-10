using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoutiqueElegance.Migrations
{
    public partial class AddRestaurantToOrderFixed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Ajouter la colonne RestaurantId si elle n'existe pas encore
            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 2) Créer un index sur RestaurantId (optionnel mais recommandé)
            migrationBuilder.CreateIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders",
                column: "RestaurantId");

            // 3) Ajouter la clé étrangère sans cascade
            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Restaurants_RestaurantId",
                table: "Orders",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Inverse : supprimer la FK, l'index, puis la colonne
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Restaurants_RestaurantId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Orders");
        }
    }
}
