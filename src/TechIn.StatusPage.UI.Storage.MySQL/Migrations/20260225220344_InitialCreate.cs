using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TechIn.StatusPage.UI.Storage.MySQL.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "status_page");

        migrationBuilder.AlterDatabase()
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "snapshots",
            schema: "status_page",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                service_name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                timestamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                latency = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                description = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_snapshots", x => x.id);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

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
