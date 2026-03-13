# Dependency Flow Documentation

## Overview

This document analyzes the current Dependency Injection (DI) configuration in `Program.cs`, identifies Dependency Inversion Principle (DIP) violations, and provides a recommended dependency flow architecture.

## Current DI Configuration

**Location**: `Program.cs` (lines 1-16)

```csharp
using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Database context registration
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoWebContext") 
        ?? throw new InvalidOperationException("Connection string 'CatalogoWebContext' not found.")));

// Service registration
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Other services
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddValidation();
```

---

## Current Dependency Flow

```
Controllers (Presentation Layer)
    ↓
Direct DbContext OR Service Layer
    ↓
CatalogoWebContext (Data Layer)
    ↓
SQL Server Database
```

### Actual State

| Component | Depends On | Status |
|-----------|------------|--------|
| `PedidoController` | `IPedidoService` | ✓ Proper abstraction |
| `CarritoController` | Mixed (Service + DbContext) | ⚠ Partial |
| `ProductosController` | `CatalogoWebContext` | ✗ Direct dependency |
| `UsuariosController` | `CatalogoWebContext` | ✗ Direct dependency |
| `CategoriasController` | `CatalogoWebContext` | ✗ Direct dependency |
| `RolsController` | `CatalogoWebContext` | ✗ Direct dependency |
| `HomeController` | `CatalogoWebContext` | ✗ Direct dependency |

---

## Dependency Inversion Principle (DIP) Violations

### What is DIP?

The Dependency Inversion Principle states:
1. High-level modules should not depend on low-level modules. Both should depend on abstractions.
2. Abstractions should not depend on details. Details should depend on abstractions.

### Violations Identified

#### 1. Controllers Directly Injecting DbContext

**Location**: Multiple controllers

```csharp
public class ProductosController : Controller
{
    private readonly CatalogoWebContext _context;
    
    public ProductosController(CatalogoWebContext context)
    {
        _context = context;  // ❌ Direct dependency on low-level detail
    }
}
```

**Problems**:
- Controllers depend on concrete `DbContext` instead of abstractions
- Business logic leaks into controllers
- Difficult to unit test (requires database)
- Violates separation of concerns

#### 2. Missing Repository Pattern Abstraction

Current: Controllers → DbContext → Database

The `CatalogoWebContext` IS the repository (implements Unit of Work pattern internally), but:
- No interface abstraction for data access
- Controllers tightly coupled to EF Core specifics
- Cannot swap data source without refactoring controllers

#### 3. Service Layer Incomplete

Only `PedidoService` exists as a service. Missing:
- `IProductoService`
- `IUsuarioService`
- `ICategoriaService`
- `IRolService`

---

## Recommended Dependency Flow

### Target Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                          │
│  Controllers → ViewModels/DTOs → Views                         │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    APPLICATION LAYER                           │
│  Services (Business Logic)                                     │
│  IPedidoService → IPagoService → INotificacionService          │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    DOMAIN/INFRASTRUCTURE LAYER                 │
│  Repositories (Data Access)                                    │
│  IPedidoRepository, IProductoRepository, IUsuarioRepository   │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    DATA LAYER                                   │
│  CatalogoWebContext (EF Core) → SQL Server                     │
└─────────────────────────────────────────────────────────────────┘
```

### Recommended Dependency Registration

```csharp
// Data Layer
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(connectionString));

// Repositories (Abstractions + Implementations)
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Services (Abstractions + Implementations)
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
```

### Dependency Rules

| From (Depends) | To (Abstraction) | Example |
|----------------|-------------------|---------|
| Controller | Service Interface | `IPedidoService` |
| Service | Repository Interface | `IPedidoRepository` |
| Repository | DbContext | `CatalogoWebContext` |
| Service | Other Services | `IPagoService`, `INotificacionService` |

---

## Migration Steps

### Phase 1: Add Repository Interfaces

```csharp
public interface IPedidoRepository
{
    Task<List<Pedido>> ObtenerTodosAsync();
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<Pedido> AddAsync(Pedido pedido);
    Task UpdateAsync(Pedido pedido);
    Task DeleteAsync(int id);
}
```

### Phase 2: Create Repository Implementations

```csharp
public class PedidoRepository : IPedidoRepository
{
    private readonly CatalogoWebContext _context;
    
    public PedidoRepository(CatalogoWebContext context)
    {
        _context = context;
    }
    
    // Implement CRUD operations
}
```

### Phase 3: Update Services to Use Repositories

```csharp
public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    
    public PedidoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }
}
```

### Phase 4: Refactor Controllers

```csharp
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;
    
    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }
}
```

---

## Files Affected

- `Program.cs` - Add new DI registrations
- New: `Interfaces/IPedidoRepository.cs`
- New: `Interfaces/IProductoRepository.cs`
- New: `Interfaces/IUsuarioRepository.cs`
- New: `Repositories/PedidoRepository.cs`
- New: `Repositories/ProductoRepository.cs`
- New: `Repositories/UsuarioRepository.cs`
- `Service/PedidoService.cs` - Inject repository instead of DbContext
- Controllers - Refactor to use services

---

## Benefits of Recommended Architecture

| Benefit | Description |
|---------|-------------|
| Testability | Mock repositories for unit tests |
| Flexibility | Swap data sources (SQL → MongoDB) |
| Maintainability | Clear separation of concerns |
| Reusability | Services can be used by multiple consumers |
| DIP Compliance | High-level modules depend on abstractions |

---

## Related Documentation

- [interfaces.md](interfaces.md) - Interface analysis and splitting recommendations
- [improvements.md](improvements.md) - Prioritized improvement recommendations
- [clean-architecture-report.md](clean-architecture-report.md) - Full SOLID analysis
