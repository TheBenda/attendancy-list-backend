using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ALB.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InviteUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstNames = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastNames = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InviteUsers_Email",
                table: "InviteUsers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InviteUsers");
        }
    }
}
