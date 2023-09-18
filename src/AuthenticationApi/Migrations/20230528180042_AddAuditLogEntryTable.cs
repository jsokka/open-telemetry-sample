using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthenticationApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogEntryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ApiKeyId = table.Column<int>(type: "int", nullable: false),
                    ApiKeyTargetId = table.Column<int>(type: "int", nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogEntries_ApiKeyTarget_ApiKeyTargetId",
                        column: x => x.ApiKeyTargetId,
                        principalTable: "ApiKeyTarget",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditLogEntries_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEntries_ApiKeyId",
                table: "AuditLogEntries",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEntries_ApiKeyTargetId",
                table: "AuditLogEntries",
                column: "ApiKeyTargetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogEntries");
        }
    }
}
