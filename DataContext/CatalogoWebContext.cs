using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CatalogoWeb.Models;

namespace CatalogoWeb.Data
{
    public class CatalogoWebContext : DbContext
    {
        public CatalogoWebContext(DbContextOptions<CatalogoWebContext> options)
            : base(options)
        {
        }

        public DbSet<CatalogoWeb.Models.Usuario> Usuario { get; set; } = default!;
        public DbSet<CatalogoWeb.Models.Rol> Rol { get; set; } = default!;
        public DbSet<CatalogoWeb.Models.Producto> Producto { get; set; } = default!;
        public DbSet<CatalogoWeb.Models.Categoria> Categoria { get; set; } = default!;

        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // comportamiento al borrar (opcional) }

            modelBuilder.Entity<Pedido>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PedidoDetalle>()
                .Property(p => p.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PedidoDetalle>()
                .Property(p => p.SubTotal)
                .HasPrecision(18, 2);


        }


    }
}


