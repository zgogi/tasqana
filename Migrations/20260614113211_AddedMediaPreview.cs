using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasqana.Migrations
{
    /// <inheritdoc />
    public partial class AddedMediaPreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewFileName",
                table: "todo_medias",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviewFileSize",
                table: "todo_medias",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewFileName",
                table: "todo_medias");

            migrationBuilder.DropColumn(
                name: "PreviewFileSize",
                table: "todo_medias");
        }
    }
}
