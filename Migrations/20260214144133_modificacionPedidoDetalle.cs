using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoWeb.Migrations
{
    /// <inheritdoc />
    public partial class modificacionPedidoDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "PedidoDetalle");

            migrationBuilder.AddColumn<string>(
                name: "CodigoProducto",
                table: "PedidoDetalle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescripcionProducto",
                table: "PedidoDetalle",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProducto",
                table: "PedidoDetalle");

            migrationBuilder.DropColumn(
                name: "DescripcionProducto",
                table: "PedidoDetalle");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "PedidoDetalle",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
