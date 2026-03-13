# Data Layer Documentation

This document describes the Entity Framework Core configuration, entity relationships, and identifies the missing repository pattern.

---

## CatalogoWebContext

**Location**: `DataContext/CatalogoWebContext.cs`

### Configuration

```csharp
public class CatalogoWebContext : DbContext
{
    public CatalogoWebContext(DbContextOptions<CatalogoWebContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Rol> Rol { get; set; }
    public DbSet<Producto> Producto { get; set; }
    public DbSet<Categoria> Categoria { get; set; }
    public DbSet<Pedido> Pedido { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
}
```

### DbSet Conventions

- Uses pluralized names (Usuario, Rol, Producto) - ASP.NET Core MVC convention
- Mixed with singular names (Pedido, PedidoDetalle) - inconsistent naming

### Precision Configuration

```csharp
modelBuilder.Entity<Pedido>()
    .Property(p => p.Total)
    .HasPrecision(18, 2);

modelBuilder.Entity<PedidoDetalle>()
    .Property(p => p.PrecioUnitario)
    .HasPrecision(18, 2);

modelBuilder.Entity<PedidoDetalle>()
    .Property(p => p.SubTotal)
    .HasPrecision(18, 2);
```

---

## Entity Relationships

### 1. Producto → Categoria (Many-to-One)

```csharp
modelBuilder.Entity<Producto>()
    .HasOne(p => p.Categoria)
    .WithMany(c => c.Productos)
    .HasForeignKey(p => p.CategoriaId)
    .OnDelete(DeleteBehavior.Restrict);
```

**Configuration**:
- One Categoria has many Productos
- On delete: Restrict (prevents deleting category with products)
- Navigation: Producto.Categoria, Categoria.Productos

### 2. Pedido → Usuario (Many-to-One, Optional)

**NOT CONFIGURED in OnModelCreating**

The relationship exists in the model but is not explicitly configured:
```csharp
public int? UsuarioId { get; set; }
public Usuario? Usuario { get; set; }
```

This allows:
- Guest checkout (UsuarioId = null)
- Authenticated user orders

### 3. Pedido → PedidoDetalle (One-to-Many)

**NOT CONFIGURED in OnModelCreating**

Implicit relationship via navigation:
```csharp
public List<PedidoDetalle> Detalles { get; set; }
```

### 4. PedidoDetalle → Pedido (Many-to-One)

**NOT CONFIGURED in OnModelCreating**

Implicit relationship:
```csharp
public int PedidoId { get; set; }
public Pedido Pedido { get; set; }
```

### 5. Usuario → Rol (Many-to-One)

**NOT CONFIGURED in OnModelCreating**

```csharp
public int RolId { get; set; }
public Rol Rol { get; set; } = null!;
```

---

## Missing Relationship: PedidoDetalle → Producto

**NOT CONFIGURED and NOT PRESENT**

```csharp
// Missing in PedidoDetalle
public int ProductoId { get; set; }
public Producto? Producto { get; set; }
```

The ProductoId exists but there's no navigation property, requiring manual joins.

---

## Relationship Diagram

```
┌─────────┐       ┌─────────┐       ┌─────────────┐
│   Rol   │       │ Usuario │       │   Categoria │
└────┬────┘       └────┬────┘       └──────┬──────┘
     │                 │                  │
     └────────┬────────┘                  │
              │                            │
              ▼                            │
        ┌─────────┐              ┌─────────────┐
        │  Pedido │──────────────│  Producto   │
        └────┬────┘              └──────┬──────┘
             │                         │
             │                   (no nav)
             ▼                         
       ┌─────────────┐                 
       │PedidoDetalle│                 
       └─────────────┘                 
```

---

## Missing Repository Pattern

### Current Architecture

```
Controllers → DbContext (directly)
```

### Recommended Architecture

```
Controllers → Services → Repositories → DbContext
```

### What Should Exist But Doesn't

1. **IRepository\<TEntity\>** - Generic base interface
   ```csharp
   public interface IRepository<T> where T : class
   {
       Task<IEnumerable<T>> GetAllAsync();
       Task<T?> GetByIdAsync(int id);
       Task AddAsync(T entity);
       void Update(T entity);
       void Delete(T entity);
   }
   ```

2. **IPedidoRepository** - Order-specific operations
   ```csharp
   public interface IPedidoRepository
   {
       Task<Pedido> CreateWithDetailsAsync(Pedido pedido, List<PedidoDetalle> detalles);
       Task<Pedido?> GetWithDetailsAsync(int id);
       Task<IEnumerable<Pedido>> GetByUsuarioAsync(int usuarioId);
   }
   ```

3. **IProductoRepository** - Product operations
   ```csharp
   public interface IProductoRepository
   {
       Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId);
       Task<bool> UpdateStockAsync(int productoId, int cantidad);
   }
   ```

### Controllers Currently Using Direct DbContext

| Controller | Direct DbContext | Should Have |
|------------|------------------|-------------|
| PedidoController | Yes | PedidoService |
| CarritoController | Yes | CarritoService |
| ProductosController | Yes | ProductoService |
| CategoriasController | Yes | CategoriaService |
| UsuariosController | Yes | UsuarioService |
| RolsController | Yes | RolService |
| HomeController | Yes | (none) |

---

## Issues Identified

1. **No explicit FK configurations** - Most relationships rely on EF Core conventions
2. **Inconsistent OnModelConfiguring** - Only Producto→Categoria is explicitly configured
3. **Missing cascade delete rules** - Not specified for any relationship
4. **No soft deletes** - All deletes are hard deletes
5. **No auditing** - No automatic timestamp or user tracking
6. **Missing indexes** - No explicit indexes defined (Relies on FK indexes)
7. **No connection string in Context** - Depends on DI configuration

---

## Recommendations

1. Explicitly configure all relationships in OnModelCreating
2. Add cascade delete rules appropriate to each relationship
3. Implement Repository pattern to abstract DbContext
4. Add audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
5. Consider soft delete pattern
6. Add explicit indexes for common queries
