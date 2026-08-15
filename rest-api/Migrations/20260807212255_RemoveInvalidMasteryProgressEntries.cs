using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInvalidMasteryProgressEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM mastery_progress_entry
                WHERE previous_total_mastery_xp = 0
                   OR current_total_mastery_xp <= previous_total_mastery_xp;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
