using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class AddRelics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "relic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    era = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("relic_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relic_reward",
                columns: table => new
                {
                    relic_id = table.Column<int>(type: "integer", nullable: false),
                    reward_unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    rarity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    item_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("relic_reward_pkey", x => new { x.relic_id, x.reward_unique_name });
                    table.ForeignKey(
                        name: "relic_reward_relic_id_fkey",
                        column: x => x.relic_id,
                        principalTable: "relic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "relic_reward_reward_unique_name_fkey",
                        column: x => x.reward_unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "relic_variant",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    relic_id = table.Column<int>(type: "integer", nullable: false),
                    refinement = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("relic_variant_pkey", x => x.unique_name);
                    table.ForeignKey(
                        name: "relic_variant_relic_id_fkey",
                        column: x => x.relic_id,
                        principalTable: "relic",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "relic_variant_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "relic_name_key",
                table: "relic",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_relic_reward_reward_unique_name",
                table: "relic_reward",
                column: "reward_unique_name");

            migrationBuilder.CreateIndex(
                name: "IX_relic_variant_relic_id_refinement",
                table: "relic_variant",
                columns: new[] { "relic_id", "refinement" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "relic_reward");

            migrationBuilder.DropTable(
                name: "relic_variant");

            migrationBuilder.DropTable(
                name: "relic");
        }
    }
}
