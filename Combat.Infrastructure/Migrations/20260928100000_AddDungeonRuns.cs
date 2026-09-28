using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Combat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDungeonRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DungeonRuns",
                columns: table => new
                {
                    DungeonRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Seed = table.Column<string>(
                        type: "character varying(13)",
                        maxLength: 13,
                        nullable: false
                    ),
                    GeneratorVersion = table.Column<int>(type: "integer", nullable: false),
                    RoomCount = table.Column<int>(type: "integer", nullable: false),
                    FloorCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    CurrentFloor = table.Column<int>(type: "integer", nullable: false),
                    HeroX = table.Column<int>(type: "integer", nullable: false),
                    HeroY = table.Column<int>(type: "integer", nullable: false),
                    Turn = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DungeonRuns", x => x.DungeonRunId);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_DungeonRuns_GameSessionId",
                table: "DungeonRuns",
                column: "GameSessionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_DungeonRuns_Seed",
                table: "DungeonRuns",
                column: "Seed"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DungeonRuns");
        }
    }
}
