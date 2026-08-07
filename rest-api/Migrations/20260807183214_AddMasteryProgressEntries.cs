using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMasteryProgressEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mastery_progress_entry",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    previous_total_mastery_xp = table.Column<int>(type: "integer", nullable: false),
                    current_total_mastery_xp = table.Column<int>(type: "integer", nullable: false),
                    mastery_xp_gained = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mastery_progress_entry_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mastery_progress_entry_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mastery_progress_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mastery_progress_entry_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    previous_xp = table.Column<int>(type: "integer", nullable: false),
                    current_xp = table.Column<int>(type: "integer", nullable: false),
                    mastery_xp_gained = table.Column<int>(type: "integer", nullable: false),
                    item_unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mastery_progress_item_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mastery_progress_item_entry_id_fkey",
                        column: x => x.mastery_progress_entry_id,
                        principalTable: "mastery_progress_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "mastery_progress_item_unique_name_fkey",
                        column: x => x.item_unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mastery_progress_mission",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mastery_progress_entry_id = table.Column<int>(type: "integer", nullable: false),
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    planet = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    previous_completion_count = table.Column<int>(type: "integer", nullable: false),
                    current_completion_count = table.Column<int>(type: "integer", nullable: false),
                    previous_sp_complete = table.Column<bool>(type: "boolean", nullable: false),
                    current_sp_complete = table.Column<bool>(type: "boolean", nullable: false),
                    mastery_xp_gained = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("mastery_progress_mission_pkey", x => x.id);
                    table.ForeignKey(
                        name: "mastery_progress_mission_entry_id_fkey",
                        column: x => x.mastery_progress_entry_id,
                        principalTable: "mastery_progress_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "mastery_progress_mission_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "missions",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mastery_progress_entry_player_id",
                table: "mastery_progress_entry",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_mastery_progress_item_item_unique_name",
                table: "mastery_progress_item",
                column: "item_unique_name");

            migrationBuilder.CreateIndex(
                name: "IX_mastery_progress_item_mastery_progress_entry_id",
                table: "mastery_progress_item",
                column: "mastery_progress_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_mastery_progress_mission_mastery_progress_entry_id",
                table: "mastery_progress_mission",
                column: "mastery_progress_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_mastery_progress_mission_unique_name",
                table: "mastery_progress_mission",
                column: "unique_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mastery_progress_item");

            migrationBuilder.DropTable(
                name: "mastery_progress_mission");

            migrationBuilder.DropTable(
                name: "mastery_progress_entry");
        }
    }
}
