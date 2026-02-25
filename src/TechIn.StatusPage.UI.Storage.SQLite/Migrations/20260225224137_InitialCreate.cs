using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechIn.StatusPage.UI.Storage.SQLite.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "status_page");

        migrationBuilder.CreateTable(
            name: "snapshots",
            schema: "status_page",
            columns: table => new
            {
                id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                service_name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                timestamp = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                latency = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                description = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_snapshots", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_snapshots_service_name",
            schema: "status_page",
            table: "snapshots",
            column: "service_name");

        migrationBuilder.CreateIndex(
            name: "ix_snapshots_service_name_timestamp",
            schema: "status_page",
            table: "snapshots",
            columns: new[] { "service_name", "timestamp" });

        migrationBuilder.CreateIndex(
            name: "ix_snapshots_timestamp",
            schema: "status_page",
            table: "snapshots",
            column: "timestamp");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "snapshots",
            schema: "status_page");
    }
}
