using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProductoIdAPedidoDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "PedidoDetalle",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "PedidoDetalle");
        }
    }
}
