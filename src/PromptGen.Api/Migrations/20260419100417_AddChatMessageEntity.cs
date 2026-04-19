using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptGen.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromptLogs_Users_UserId",
                table: "PromptLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromptLogs",
                table: "PromptLogs");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "PromptLogs");

            migrationBuilder.RenameTable(
                name: "PromptLogs",
                newName: "PromptLog");

            migrationBuilder.RenameIndex(
                name: "IX_PromptLogs_UserId",
                table: "PromptLog",
                newName: "IX_PromptLog_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PromptLogs_ImageHash",
                table: "PromptLog",
                newName: "IX_PromptLog_ImageHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromptLog",
                table: "PromptLog",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_PromptLog_Users_UserId",
                table: "PromptLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromptLog_Users_UserId",
                table: "PromptLog");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromptLog",
                table: "PromptLog");

            migrationBuilder.RenameTable(
                name: "PromptLog",
                newName: "PromptLogs");

            migrationBuilder.RenameIndex(
                name: "IX_PromptLog_UserId",
                table: "PromptLogs",
                newName: "IX_PromptLogs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PromptLog_ImageHash",
                table: "PromptLogs",
                newName: "IX_PromptLogs_ImageHash");

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "PromptLogs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "gpt-4o-vision");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromptLogs",
                table: "PromptLogs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromptLogs_Users_UserId",
                table: "PromptLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
