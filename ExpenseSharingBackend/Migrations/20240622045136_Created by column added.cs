using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseSharingBackend.Migrations
{
    public partial class Createdbycolumnadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Groups");
        }
    }
}
