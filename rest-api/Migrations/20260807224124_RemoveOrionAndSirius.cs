using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrionAndSirius : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM item
                WHERE unique_name = '/Lotus/Powersuits/SiriusOrion/OrionSuit';

                REFRESH MATERIALIZED VIEW xp_items_with_recipes_and_components;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
