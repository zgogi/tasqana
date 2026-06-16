using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasqana.Migrations
{
    /// <inheritdoc />
    public partial class AddedClientCascadeOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_todo_medias_todos_TodoId",
                table: "todo_medias");

            migrationBuilder.DropForeignKey(
                name: "FK_todos_categories_CategoryId",
                table: "todos");

            migrationBuilder.DropForeignKey(
                name: "FK_todos_users_UserId",
                table: "todos");

            migrationBuilder.AddForeignKey(
                name: "FK_todo_medias_todos_TodoId",
                table: "todo_medias",
                column: "TodoId",
                principalTable: "todos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_todos_categories_CategoryId",
                table: "todos",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_todos_users_UserId",
                table: "todos",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_todo_medias_todos_TodoId",
                table: "todo_medias");

            migrationBuilder.DropForeignKey(
                name: "FK_todos_categories_CategoryId",
                table: "todos");

            migrationBuilder.DropForeignKey(
                name: "FK_todos_users_UserId",
                table: "todos");

            migrationBuilder.AddForeignKey(
                name: "FK_todo_medias_todos_TodoId",
                table: "todo_medias",
                column: "TodoId",
                principalTable: "todos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_todos_categories_CategoryId",
                table: "todos",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_todos_users_UserId",
                table: "todos",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
