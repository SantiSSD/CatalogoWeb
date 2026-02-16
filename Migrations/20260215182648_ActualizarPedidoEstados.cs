using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoWeb.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarPedidoEstados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ciudad",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoPostal",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEmailConfirmacion",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEmailEntrega",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEmailEnvio",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEmailValoracion",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvio",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNotificacionVendedor",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPago",
                table: "Pedido",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MensajeErrorPago",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetodoPago",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroTracking",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "Pedido",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Pedido",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_UsuarioId",
                table: "Pedido",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Usuario_UsuarioId",
                table: "Pedido",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Usuario_UsuarioId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_UsuarioId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Ciudad",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "CodigoPostal",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaEmailConfirmacion",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaEmailEntrega",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaEmailEnvio",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaEmailValoracion",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaEnvio",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaNotificacionVendedor",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "FechaPago",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "MensajeErrorPago",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "MetodoPago",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "NumeroTracking",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Pedido");
        }
    }
}
