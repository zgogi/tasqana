using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasqana.Migrations;

public partial class AddSyncIds : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "categories", "todos", "todos_checkitems", "todo_files" })
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SyncId", table: table, type: "uuid", nullable: false,
                defaultValueSql: "gen_random_uuid()");
            migrationBuilder.CreateIndex(
                name: $"IX_{table}_SyncId", table: table, column: "SyncId", unique: true);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var table in new[] { "categories", "todos", "todos_checkitems", "todo_files" })
        {
            migrationBuilder.DropIndex(name: $"IX_{table}_SyncId", table: table);
            migrationBuilder.DropColumn(name: "SyncId", table: table);
        }
    }
}
