using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conexiones.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConnectionTypeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "connection_types",
                newName: "Contact");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "connection_types",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "connection_types",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "connection_types");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "connection_types");

            migrationBuilder.RenameColumn(
                name: "Contact",
                table: "connection_types",
                newName: "Description");
        }
    }
}
