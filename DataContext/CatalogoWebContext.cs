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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict); // comportamiento al borrar (opcional) }
        }


    }
}


