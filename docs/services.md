# Services Documentation

This document details the service layer in the CatalogoWeb application.

## Current State

| Service | Interface | Status | Methods |
|---------|-----------|--------|---------|
| `PedidoService` | `IPedidoService` | Implemented | 20 |
| `UsuarioService` | - | **Missing** | - |
| `ProductoService` | - | **Missing** | - |
| `CategoriaService` | - | **Missing** | - |
| `RolService` | - | **Missing** | - |

## 1. PedidoService (Implemented)

**File**: `/Service/PedidoService.cs`

**Interface**: `/Interfaces/IPedidoService.cs`

### Class Definition

```csharp
public class PedidoService : IPedidoService
{
    private readonly CatalogoWebContext _context;

    public PedidoService(CatalogoWebContext context)
    {
        _context = context;
    }
}
```

### Methods (20 total)

#### Query Methods

| Method | Return | Description |
|--------|--------|-------------|
| `ObtenerTodos()` | `Task<List<Pedido>>` | Get all orders with details and user |
| `ObtenerPorId(int id)` | `Task<Pedido?>` | Get order by ID with details |

#### Creation Methods

| Method | Return | Description |
|--------|--------|-------------|
| `CrearPedidoAsync(CrearPedidoDto dto)` | `Task<int>` | Create order from DTO |

#### Payment Methods

| Method | Return | Description |
|--------|--------|-------------|
| `PagarPedido(int pedidoId)` | `Task<int>` | Mark as paid (simplified) |
| `ProcesarPagoAsync(int, DatosPagoDto)` | `Task<ResultadoPagoDto>` | Full payment processing |
| `RechazarPago(int, string)` | `Task<int>` | Reject payment |

#### Status Transition Methods

| Method | Return | Description |
|--------|--------|-------------|
| `MarcarEnPreparacion(int)` | `Task<int>` | Pendiente → EnPreparacion |
| `MarcarEnviado(int, string)` | `Task<int>` | EnPreparacion/Pagado → Enviado |
| `MarcarEntregado(int)` | `Task<int>` | Enviado → Entregado |
| `MarcarCompletado(int)` | `Task<int>` | Entregado → Completado |
| `CancelarPedido(int)` | `Task<int>` | Cancel (if not shipped) |

#### Notification/Email Stubs

| Method | Return | Description |
|--------|--------|-------------|
| `NotificarVendedor(int)` | `Task<int>` | Stub - updates timestamp |
| `EnviarEmailConfirmacion(int)` | `Task<int>` | Stub - updates timestamp |
| `EnviarEmailEnvio(int)` | `Task<int>` | Stub - updates timestamp |
| `EnviarEmailEntrega(int)` | `Task<int>` | Stub - updates timestamp |
| `EnviarEmailValoracion(int)` | `Task<int>` | Stub - updates timestamp |

### IPedidoService Interface

```csharp
public interface IPedidoService
{
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<int> PagarPedido(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);
    Task<int> RechazarPago(int pedidoId, string mensajeError);
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

### God Interface Issue

The `IPedidoService` interface has **20 methods** violating the **Interface Segregation Principle (ISP)**.

**Problems**:
1. Clients forced to implement/depend on methods they don't use
2. Hard to test (too many dependencies)
3. Difficult to maintain
4. Cannot have different implementations for different scenarios

**Recommended Split**:

| Interface | Methods | Purpose |
|-----------|---------|---------|
| `IOrderQueryService` | `ObtenerTodos()`, `ObtenerPorId(int)` | Read operations |
| `IOrderCommandService` | `CrearPedidoAsync()`, `CancelarPedido()` | Write operations |
| `IPaymentService` | `PagarPedido()`, `ProcesarPagoAsync()`, `RechazarPago()` | Payment logic |
| `IOrderStatusService` | `MarcarEnPreparacion()`, `MarcarEnviado()`, `MarcarEntregado()`, `MarcarCompletado()` | Status changes |
| `INotificationService` | All email/notification stubs | Notifications |

---

## 2. Missing Services

The following services are needed to complete the Clean Architecture refactoring:

### UsuarioService (Missing)

**Recommended Interface**:

```csharp
public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerTodos();
    Task<Usuario?> ObtenerPorId(int id);
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<int> Crear(Usuario usuario);
    Task<int> Actualizar(Usuario usuario);
    Task<int> Eliminar(int id);
    Task<bool> ValidarCredenciales(string email, string password);
    Task<string> HashPassword(string password);
}
```

**Current State**: All user operations in `UsuariosController` with direct DbContext

---

### ProductoService (Missing)

**Recommended Interface**:

```csharp
public interface IProductoService
{
    Task<List<Producto>> ObtenerTodos();
    Task<List<Producto>> ObtenerPorCategoria(int categoriaId);
    Task<List<Producto>> Buscar(string termino);
    Task<Producto?> ObtenerPorId(int id);
    Task<int> Crear(Producto producto);
    Task<int> Actualizar(Producto producto);
    Task<int> Eliminar(int id);
    Task<int> ActualizarStock(int id, int cantidad);
}
```

**Current State**: All product operations in `ProductosController` with direct DbContext

---

### CategoriaService (Missing)

**Recommended Interface**:

```csharp
public interface ICategoriaService
{
    Task<List<Categoria>> ObtenerTodos();
    Task<Categoria?> ObtenerPorId(int id);
    Task<Categoria?> ObtenerPorNombre(string nombre);
    Task<int> Crear(Categoria categoria);
    Task<int> Actualizar(Categoria categoria);
    Task<int> Eliminar(int id);
    Task<int> ObtenerProductoCount(int categoriaId);
}
```

**Current State**: All category operations in `CategoriasController` with direct DbContext

---

### RolService (Missing)

**Recommended Interface**:

```csharp
public interface IRolService
{
    Task<List<Rol>> ObtenerTodos();
    Task<Rol?> ObtenerPorId(int id);
    Task<Rol?> ObtenerPorNombre(string nombre);
    Task<int> Crear(Rol rol);
    Task<int> Actualizar(Rol rol);
    Task<int> Eliminar(int id);
}
```

**Current State**: All role operations in `RolsController` with direct DbContext

---

## 3. Email/Notification Stubs

All email and notification methods in `PedidoService` are **stubs**:

### Current Implementation Pattern

```csharp
public async Task<int> EnviarEmailConfirmacion(int pedidoId)
{
    var pedido = await _context.Pedido.FindAsync(pedidoId);
    if (pedido == null)
        throw new Exception("Pedido no encontrado");

    pedido.FechaEmailConfirmacion = DateTime.Now;
    await _context.SaveChangesAsync();
    return pedidoId;
}
```

### Issues

1. **No actual email sending**: Only updates database timestamp
2. **Side effect in service**: Updates entity while "sending" email
3. **No abstraction**: Cannot swap email providers
4. **No error handling**: No retry logic, no failure notifications

### Recommended Email Service

```csharp
public interface IEmailService
{
    Task<bool> EnviarConfirmacionPedido(Pedido pedido);
    Task<bool> EnviarNotificacionEnvio(Pedido pedido, string tracking);
    Task<bool> EnviarNotificacionEntrega(Pedido pedido);
    Task<bool> EnviarSolicitudValoracion(Pedido pedido);
}
```

**Implementation Options**:
- SendGrid
- Mailgun
- Amazon SES
- SMTP (built-in)

---

## 4. Service Dependency Flow

### Current (Incorrect)

```
Controllers → DbContext (Direct)
PedidoController → IPedidoService → DbContext
```

### Recommended

```
Controllers → Services → Repositories → DbContext
                    ↓
              IEmailService
```

---

## 5. DI Registration (Program.cs)

Current registration (inferred):

```csharp
services.AddDbContext<CatalogoWebContext>();
services.AddScoped<IPedidoService, PedidoService>();
```

### Recommended Registration

```csharp
// DbContext
services.AddDbContext<CatalogoWebContext>();

// Repositories (to be added)
services.AddScoped<IUsuarioRepository, UsuarioRepository>();
services.AddScoped<IProductoRepository, ProductoRepository>();
services.AddScoped<ICategoriaRepository, CategoriaRepository>();
services.AddScoped<IRolRepository, RolRepository>();
services.AddScoped<IPedidoRepository, PedidoRepository>();

// Services (split interfaces)
services.AddScoped<IOrderQueryService, PedidoService>();
services.AddScoped<IOrderCommandService, PedidoService>();
services.AddScoped<IPaymentService, PedidoService>();
services.AddScoped<IOrderStatusService, PedidoService>();

// Email
services.AddScoped<IEmailService, EmailService>();
```

---

## Summary

| Component | Status | Issue |
|-----------|--------|-------|
| PedidoService | Implemented | God Interface (20 methods) |
| UsuarioService | Missing | Need to create |
| ProductoService | Missing | Need to create |
| CategoriaService | Missing | Need to create |
| RolService | Missing | Need to create |
| EmailService | Stub only | Need real implementation |

## Priority Recommendations

1. **High**: Create `UsuarioService`, `ProductoService`, `CategoriaService`, `RolService`
2. **High**: Split `IPedidoService` into smaller interfaces
3. **Medium**: Implement real `IEmailService`
4. **Low**: Add Unit of Work pattern for transactions
