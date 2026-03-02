using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoutiqueElegance.Migrations
{
    /// <inheritdoc />
    public partial class ImagesRestaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Restaurants",
                newName: "CardImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Restaurants");

            migrationBuilder.RenameColumn(
                name: "CardImageUrl",
                table: "Restaurants",
                newName: "ImageUrl");
        }
    }
}
