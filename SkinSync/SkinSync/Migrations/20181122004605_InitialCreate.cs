using Microsoft.EntityFrameworkCore.Migrations;

namespace SkinSync.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LesionItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    Diameter = table.Column<int>(nullable: false),
                    Uploaded = table.Column<string>(nullable: true),
                    Width = table.Column<string>(nullable: true),
                    Height = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LesionItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LesionItem");
        }
    }
}
