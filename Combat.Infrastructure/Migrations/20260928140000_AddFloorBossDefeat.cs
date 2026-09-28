using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorBossDefeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFloorBossDefeated",
                table: "DungeonRuns",
                type: "boolean",
                nullable: false,
                defaultValue: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsFloorBossDefeated", table: "DungeonRuns");
        }
    }
}
