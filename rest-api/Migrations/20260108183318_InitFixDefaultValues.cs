using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rest_api.Migrations
{
    /// <inheritdoc />
    public partial class InitFixDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "roles",
                table: "registered_user",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldDefaultValue: new List<string>());

            migrationBuilder.AlterColumn<DateTime>(
                name: "issued",
                table: "refresh_token",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldDefaultValue: new DateTime(2026, 1, 8, 19, 30, 44, 756, DateTimeKind.Unspecified).AddTicks(1691));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "roles",
                table: "registered_user",
                type: "text[]",
                nullable: false,
                defaultValue: new List<string>(),
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<DateTime>(
                name: "issued",
                table: "refresh_token",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 8, 19, 30, 44, 756, DateTimeKind.Unspecified).AddTicks(1691),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
