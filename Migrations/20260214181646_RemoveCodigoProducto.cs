using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCodigoProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProducto",
                table: "PedidoDetalle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoProducto",
                table: "PedidoDetalle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
