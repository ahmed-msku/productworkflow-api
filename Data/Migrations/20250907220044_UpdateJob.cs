using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductWorkflow.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "ProcessingJob",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "ProcessingJob");
        }
    }
}
