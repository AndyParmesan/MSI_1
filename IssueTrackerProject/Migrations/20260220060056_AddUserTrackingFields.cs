using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssueTrackerProject.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "Issues",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ReportedBy",
                table: "Issues",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "ReportedBy",
                table: "Issues");
        }
    }
}
