# Application Layers

This document describes the layered architecture of the CatalogoWeb application.

## Overview

The application follows a modified MVC pattern with three main layers:

1. **Presentation Layer** - Controllers and Views
2. **Business Logic Layer** - Services
3. **Data Access Layer** - DbContext and Models

## Presentation Layer (Controllers/)

**Location**: `/Controllers/`

The presentation layer handles HTTP requests and responses. It contains 7 controllers:

| Controller | Responsibility | DbContext Access |
|------------|---------------|------------------|
| `PedidoController` | Order management, payment processing | Via IPedidoService |
| `CarritoController` | Shopping cart operations | Mixed (direct + service) |
| `ProductosController` | Product CRUD | Direct |
| `UsuariosController` | User management | Direct |
| `CategoriasController` | Category management | Direct |
| `RolsController` | Role management | Direct |
| `HomeController` | Home page, product browsing | Direct |

### Issues Identified

- **5 of 7 controllers** inject `CatalogoWebContext` directly
- Only `PedidoController` uses proper service abstraction
- `CarritoController` uses a mixed pattern (direct + service)

## Business Logic Layer (Services/)

**Location**: `/Service/`

Contains business logic and orchestration:

| Service | Interface | Status | Methods |
|---------|-----------|--------|---------|
| `PedidoService` | `IPedidoService` | Implemented | 20+ |
| `UsuarioService` | - | Missing | - |
| `ProductoService` | - | Missing | - |
| `CategoriaService` | - | Missing | - |
| `RolService` | - | Missing | - |

### PedidoService Details

The `PedidoService` implements the `IPedidoService` interface with 20+ methods:

**Order Management:**
- `ObtenerTodos()` - Get all orders
- `ObtenerPorId(int id)` - Get order by ID
- `CrearPedidoAsync(CrearPedidoDto dto)` - Create new order

**Payment Processing:**
- `PagarPedido(int pedidoId)` - Mark order as paid
- `ProcesarPagoAsync(int pedidoId, DatosPagoDto)` - Process payment with gateway
- `RechazarPago(int pedidoId, string mensaje)` - Reject payment

**Order Status:**
- `MarcarEnPreparacion(int pedidoId)` - Mark in preparation
- `MarcarEnviado(int pedidoId, string tracking)` - Mark as shipped
- `MarcarEntregado(int pedidoId)` - Mark as delivered
- `MarcarCompletado(int pedidoId)` - Mark as completed
- `CancelarPedido(int pedidoId)` - Cancel order

**Notifications (Stubs):**
- `NotificarVendedor(int pedidoId)` - Stub
- `EnviarEmailConfirmacion(int pedidoId)` - Stub
- `EnviarEmailEnvio(int pedidoId)` - Stub
- `EnviarEmailEntrega(int pedidoId)` - Stub
- `EnviarEmailValoracion(int pedidoId)` - Stub

### God Interface Issue

`IPedidoService` is a **God Interface** with 20+ methods violating the Interface Segregation Principle (ISP). Recommendations:
- Split into smaller interfaces: `IOrderQueryService`, `IOrderCommandService`, `IPaymentService`, `INotificationService`

## Data Access Layer

**Location**: `/DataContext/CatalogoWebContext.cs`

The `CatalogoWebContext` is the EF Core DbContext managing all database operations:

### DbSets

```csharp
public DbSet<Usuario> Usuario { get; set; }
public DbSet<Rol> Rol { get; set; }
public DbSet<Producto> Producto { get; set; }
public DbSet<Categoria> Categoria { get; set; }
public DbSet<Pedido> Pedido { get; set; }
public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
```

### Entity Relationships

- `Producto` → `Categoria` (Many-to-One, DeleteBehavior.Restrict)
- `Pedido` → `Usuario` (Many-to-One, optional)
- `Pedido` → `PedidoDetalle` (One-to-Many)
- `Usuario` → `Rol` (Many-to-One)

### Precision Configuration

- `Pedido.Total`: `decimal(18,2)`
- `PedidoDetalle.PrecioUnitario`: `decimal(18,2)`
- `PedidoDetalle.SubTotal`: `decimal(18,2)`

## Models Layer

**Location**: `/Models/`

Contains 8 entity models:

| Model | Purpose |
|-------|---------|
| `Pedido` | Order entity with status tracking |
| `PedidoDetalle` | Line items for orders |
| `Producto` | Product catalog items |
| `Categoria` | Product categories |
| `Usuario` | User accounts |
| `Rol` | User roles |
| `CarritoItem` | Shopping cart items (session-based) |
| `EstadoPedido` | Enum for order states |

## Dependency Flow (Current vs. Recommended)

### Current (Violating Clean Architecture)

```
Controllers → DbContext (Direct)
```

### Recommended

```
Controllers → Services → Repositories → DbContext
```

## Missing Components

1. **Repository Pattern** - No abstraction over DbContext
2. **Unit of Work** - No transaction management
3. **Missing Services** - UsuarioService, ProductoService, CategoriaService, RolService
4. **Email Service** - No implementation (all stubs)
