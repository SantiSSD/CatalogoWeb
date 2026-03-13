# Documentación de la Capa de Datos

Este documento describe la configuración de Entity Framework Core, las relaciones entre entidades e identifica la falta del patrón Repository.

---

## CatalogoWebContext

**Ubicación**: `DataContext/CatalogoWebContext.cs`

### Configuración

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

### Convenciones de DbSet

- Utiliza nombres en plural (Usuario, Rol, Producto) - convención de ASP.NET Core MVC
- Mezclado con nombres en singular (Pedido, PedidoDetalle) - nomenclatura inconsistente

### Configuración de Precisión

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

## Relaciones entre Entidades

### 1. Producto → Categoria (Muchos a Uno)

```csharp
modelBuilder.Entity<Producto>()
    .HasOne(p => p.Categoria)
    .WithMany(c => c.Productos)
    .HasForeignKey(p => p.CategoriaId)
    .OnDelete(DeleteBehavior.Restrict);
```

**Configuración**:
- Una Categoria tiene muchos Productos
- Al eliminar: Restrict (impide eliminar categoría con productos)
- Navegación: Producto.Categoria, Categoria.Productos

### 2. Pedido → Usuario (Muchos a Uno, Opcional)

**NO CONFIGURADO en OnModelCreating**

La relación existe en el modelo pero no está configurada explícitamente:
```csharp
public int? UsuarioId { get; set; }
public Usuario? Usuario { get; set; }
```

Esto permite:
- Compra como invitado (UsuarioId = null)
- Pedidos de usuarios autenticados

### 3. Pedido → PedidoDetalle (Uno a Muchos)

**NO CONFIGURADO en OnModelCreating**

Relación implícita mediante navegación:
```csharp
public List<PedidoDetalle> Detalles { get; set; }
```

### 4. PedidoDetalle → Pedido (Muchos a Uno)

**NO CONFIGURADO en OnModelCreating**

Relación implícita:
```csharp
public int PedidoId { get; set; }
public Pedido Pedido { get; set; }
```

### 5. Usuario → Rol (Muchos a Uno)

**NO CONFIGURADO en OnModelCreating**

```csharp
public int RolId { get; set; }
public Rol Rol { get; set; } = null!;
```

---

## Relación Faltante: PedidoDetalle → Producto

**NO CONFIGURADO y NO PRESENTE**

```csharp
// Falta en PedidoDetalle
public int ProductoId { get; set; }
public Producto? Producto { get; set; }
```

El ProductoId existe pero no hay propiedad de navegación, requiriendo uniones manuales.

---

## Diagrama de Relaciones

```
┌─────────┐       ┌─────────┐       ┌─────────────┐
│   Rol   │       │ Usuario │       │  Categoria  │
└────┬────┘       └────┬────┘       └──────┬──────┘
     │                 │                  │
     └────────┬────────┘                  │
              │                            │
              ▼                            │
        ┌─────────┐              ┌─────────────┐
        │  Pedido │──────────────│  Producto   │
        └────┬────┘              └──────┬──────┘
             │                         │
             │                   (sin nav)
             ▼                         
        ┌─────────────┐                 
        │PedidoDetalle│                 
        └─────────────┘                 
```

---

## Patrón Repository Faltante

### Arquitectura Actual

```
Controladores → DbContext (directamente)
```

### Arquitectura Recomendada

```
Controladores → Servicios → Repositorios → DbContext
```

### Lo Que Debería Existir Pero No Existe

1. **IRepository\<TEntity\>** - Interfaz base genérica
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

2. **IPedidoRepository** - Operaciones específicas de pedidos
   ```csharp
   public interface IPedidoRepository
   {
       Task<Pedido> CreateWithDetailsAsync(Pedido pedido, List<PedidoDetalle> detalles);
       Task<Pedido?> GetWithDetailsAsync(int id);
       Task<IEnumerable<Pedido>> GetByUsuarioAsync(int usuarioId);
   }
   ```

3. **IProductoRepository** - Operaciones de productos
   ```csharp
   public interface IProductoRepository
   {
       Task<IEnumerable<Producto>> GetByCategoriaAsync(int categoriaId);
       Task<bool> UpdateStockAsync(int productoId, int cantidad);
   }
   ```

### Controladores Que Utilizan DbContext Directamente

| Controlador | DbContext Directo | Debería Tener |
|-------------|-------------------|---------------|
| PedidoController | Sí | PedidoService |
| CarritoController | Sí | CarritoService |
| ProductosController | Sí | ProductoService |
| CategoriasController | Sí | CategoriaService |
| UsuariosController | Sí | UsuarioService |
| RolsController | Sí | RolService |
| HomeController | Sí | (ninguno) |

---

## Problemas Identificados

1. **Sin configuraciones explícitas de FK** - La mayoría de las relaciones dependen de las convenciones de EF Core
2. **OnModelConfiguring inconsistente** - Solo Producto→Categoria está configurado explícitamente
3. **Faltan reglas de eliminación en cascada** - No especificadas para ninguna relación
4. **Sin eliminaciones suaves** - Todas las eliminaciones son eliminaciones duras
5. **Sin auditoría** - No hay seguimiento automático de fecha o usuario
6. **Faltan índices** - No hay índices explícitos definidos (Depende de índices de FK)
7. **Sin cadena de conexión en Context** - Depende de configuración de DI

---

## Recomendaciones

1. Configurar explícitamente todas las relaciones en OnModelCreating
2. Agregar reglas de eliminación en cascada apropiadas para cada relación
3. Implementar el patrón Repository para abstraer DbContext
4. Agregar campos de auditoría (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
5. Considerar el patrón de eliminación suave
6. Agregar índices explícitos para consultas comunes
