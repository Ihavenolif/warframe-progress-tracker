using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:invitation_status", "pending,accepted,declined,canceled");

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    item_class = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    xp_required = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("item_pkey", x => x.unique_name);
                });

            migrationBuilder.CreateTable(
                name: "missions",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    planet = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    mastery_xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("missions_pkey", x => x.unique_name);
                });

            migrationBuilder.CreateTable(
                name: "player",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    mastery_rank = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    duviri_skills = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    railjack_skills = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_mastery_xp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recipe",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    result_item = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("recipe_pkey", x => x.unique_name);
                    table.ForeignKey(
                        name: "recipe_result_item_fkey",
                        column: x => x.result_item,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "recipe_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clan",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    leader_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("clan_pkey", x => x.id);
                    table.ForeignKey(
                        name: "clan_leader_id_fkey",
                        column: x => x.leader_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_items",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    item_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_items_pkey", x => new { x.unique_name, x.player_id });
                    table.ForeignKey(
                        name: "player_items_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "player_items_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_items_mastery",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    xp_gained = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_items_mastery_pkey", x => new { x.unique_name, x.player_id });
                    table.ForeignKey(
                        name: "player_items_mastery_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "player_items_mastery_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_mission_completion",
                columns: table => new
                {
                    unique_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    completes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    sp_complete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_mission_completion_pkey", x => new { x.unique_name, x.player_id });
                    table.ForeignKey(
                        name: "player_mission_completion_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "player_mission_completion_unique_name_fkey",
                        column: x => x.unique_name,
                        principalTable: "missions",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "registered_user",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    roles = table.Column<List<string>>(type: "text[]", nullable: false, defaultValue: new List<string>())
                },
                constraints: table =>
                {
                    table.PrimaryKey("registered_user_pkey", x => x.id);
                    table.ForeignKey(
                        name: "registered_user_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    recipe_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    item_ingredient = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ingredient_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("recipe_ingredients_pkey", x => new { x.recipe_name, x.item_ingredient });
                    table.ForeignKey(
                        name: "recipe_ingredients_item_ingredient_fkey",
                        column: x => x.item_ingredient,
                        principalTable: "item",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "recipe_ingredients_recipe_name_fkey",
                        column: x => x.recipe_name,
                        principalTable: "recipe",
                        principalColumn: "unique_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clan_invitation",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clan_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false),
                    invitation_status = table.Column<string>(type: "text", nullable: false, defaultValue: "PENDING")
                },
                constraints: table =>
                {
                    table.PrimaryKey("clan_invitation_pkey", x => x.id);
                    table.ForeignKey(
                        name: "clan_invitation_clan_id_fkey",
                        column: x => x.clan_id,
                        principalTable: "clan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "clan_invitation_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_clan",
                columns: table => new
                {
                    clan_id = table.Column<int>(type: "integer", nullable: false),
                    player_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("player_clan_pkey", x => new { x.clan_id, x.player_id });
                    table.ForeignKey(
                        name: "player_clan_clan_id_fkey",
                        column: x => x.clan_id,
                        principalTable: "clan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "player_clan_player_id_fkey",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    token = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    issued = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValue: new DateTime(2026, 1, 8, 19, 30, 44, 756, DateTimeKind.Unspecified).AddTicks(1691)),
                    expires = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    issued_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("refresh_token_pkey", x => x.token);
                    table.ForeignKey(
                        name: "refresh_token_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "registered_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "clan_name_key",
                table: "clan",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clan_leader_id",
                table: "clan",
                column: "leader_id");

            migrationBuilder.CreateIndex(
                name: "IX_clan_invitation_clan_id",
                table: "clan_invitation",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_clan_invitation_player_id",
                table: "clan_invitation",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "player_username_key",
                table: "player",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_clan_player_id",
                table: "player_clan",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_items_player_id",
                table: "player_items",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_items_mastery_player_id",
                table: "player_items_mastery",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_mission_completion_player_id",
                table: "player_mission_completion",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_result_item",
                table: "recipe",
                column: "result_item");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_item_ingredient",
                table: "recipe_ingredients",
                column: "item_ingredient");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_registered_user_player_id",
                table: "registered_user",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "registered_user_username_key",
                table: "registered_user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clan_invitation");

            migrationBuilder.DropTable(
                name: "player_clan");

            migrationBuilder.DropTable(
                name: "player_items");

            migrationBuilder.DropTable(
                name: "player_items_mastery");

            migrationBuilder.DropTable(
                name: "player_mission_completion");

            migrationBuilder.DropTable(
                name: "recipe_ingredients");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "clan");

            migrationBuilder.DropTable(
                name: "missions");

            migrationBuilder.DropTable(
                name: "recipe");

            migrationBuilder.DropTable(
                name: "registered_user");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "player");
        }
    }
}
