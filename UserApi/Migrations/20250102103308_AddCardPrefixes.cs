using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UserApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCardPrefixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardPrefixes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Prefix = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardPrefixes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CardPrefixes",
                columns: new[] { "Id", "BankName", "Prefix" },
                values: new object[,]
                {
                    { 1, "بانک ملی ایران", "603799" },
                    { 2, "بانک سپه", "589210" },
                    { 3, "بانک اقتصاد نوین", "627412" },
                    { 4, "بانک سامان", "621986" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardPrefixes");
        }
    }
}
