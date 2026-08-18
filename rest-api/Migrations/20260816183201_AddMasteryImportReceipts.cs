using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMasteryImportReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mastery_import_receipt",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    imported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    resulting_mastery_rank = table.Column<int>(type: "integer", nullable: false),
                    resulting_total_mastery_xp = table.Column<int>(type: "integer", nullable: false),
                    changed = table.Column<bool>(type: "boolean", nullable: false),
                    source_version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    skipped_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mastery_import_receipt_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mastery_import_receipt_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "mastery_import_receipt_player_imported_at_idx",
                table: "mastery_import_receipt",
                columns: new[] { "player_id", "imported_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mastery_import_receipt");
        }
    }
}
