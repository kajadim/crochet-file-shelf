using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "PendingRegistrations",
                newName: "LastName");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvatarUpdatedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "PendingRegistrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "PendingRegistrations",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                DELETE FROM "PendingRegistrations";
                """);

            migrationBuilder.Sql("""
                UPDATE "Users" SET "FirstName" = "LastName", "LastName" = '';
                """);

            migrationBuilder.Sql("""
                WITH base AS (
                    SELECT "Id", "CreatedAt",
                           CASE
                               WHEN length(regexp_replace(lower(split_part("Email", '@', 1)), '[^a-z0-9._-]', '', 'g')) < 3
                                   THEN regexp_replace(lower(split_part("Email", '@', 1)), '[^a-z0-9._-]', '', 'g') || 'user'
                               ELSE left(regexp_replace(lower(split_part("Email", '@', 1)), '[^a-z0-9._-]', '', 'g'), 26)
                           END AS name
                    FROM "Users"
                ),
                ranked AS (
                    SELECT "Id", name, row_number() OVER (PARTITION BY name ORDER BY "CreatedAt", "Id") AS rn
                    FROM base
                )
                UPDATE "Users" u
                SET "Username" = CASE WHEN r.rn = 1 THEN r.name ELSE r.name || r.rn::text END
                FROM ranked r
                WHERE u."Id" = r."Id";
                """);

            migrationBuilder.CreateTable(
                name: "UserAvatars",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAvatars", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserAvatars_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Users" SET "LastName" = btrim("FirstName" || ' ' || "LastName");
                """);

            migrationBuilder.DropTable(
                name: "UserAvatars");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarUpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "PendingRegistrations");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "PendingRegistrations");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "PendingRegistrations",
                newName: "DisplayName");
        }
    }
}
