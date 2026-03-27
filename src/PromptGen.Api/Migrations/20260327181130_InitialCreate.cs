using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptGen.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LastName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsPremium = table.Column<bool>(type: "boolean", nullable: false),
                    DailyRequestCount = table.Column<int>(type: "integer", nullable: false),
                    LastRequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageHash = table.Column<string>(type: "text", nullable: false),
                    TelegramFileId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GeneratedPrompt = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "gpt-4o-vision"),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptLogs_ImageHash",
                table: "PromptLogs",
                column: "ImageHash");

            migrationBuilder.CreateIndex(
                name: "IX_PromptLogs_UserId",
                table: "PromptLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptLogs");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
