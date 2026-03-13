# Architecture Documentation - CatalogoWeb

## Overview

CatalogoWeb is an ASP.NET Core MVC e-commerce application that follows a basic Model-View-Controller pattern. The application manages products, categories, users, orders (pedidos), and shopping cart functionality.

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 10.0.2 |
| Database | SQL Server | - |
| UI Pattern | MVC (Model-View-Controller) | - |
| Session Management | ASP.NET Core Session | - |
| Validation | FluentValidation | - |

---

## Project Folder Structure

```
CatalogoWeb/
├── Controllers/          # MVC Controllers (7 controllers)
│   ├── CarritoController.cs
│   ├── CategoriasController.cs
│   ├── HomeController.cs
│   ├── PedidoController.cs
│   ├── ProductosController.cs
│   ├── RolsController.cs
│   └── UsuariosController.cs
├── DataContext/          # Entity Framework DbContext
│   └── CatalogoWebContext.cs
├── Interfaces/           # Service Interfaces
│   └── IPedidoService.cs
├── Models/               # Entity Models and DTOs
│   ├── CarritoItem.cs
│   ├── Categoria.cs
│   ├── EstadoPedido.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Rol.cs
│   ├── Usuario.cs
│   ├── ErrorViewModel.cs
│   └── Dtos/
│       ├── CrearPedidoDto.cs
│       ├── DatosPagoDto.cs
│       ├── ItemPedidoDto.cs
│       └── ResultadoPagoDto.cs
├── Service/              # Business Logic Services
│   └── PedidoService.cs
├── Migrations/           # EF Core Migrations
├── Views/                # Razor Views
├── Program.cs            # Application Entry Point
└── appsettings.json      # Configuration
```

---

## MVC Pattern Implementation

### Model Layer
- **Entity Models**: 8 domain entities (Pedido, PedidoDetalle, Producto, Categoria, Usuario, Rol, CarritoItem, EstadoPedido)
- **DTOs**: 4 Data Transfer Objects for API communication
- **DbContext**: Single `CatalogoWebContext` managing all database operations

### View Layer
- ASP.NET Core Razor Views
- Session-based shopping cart

### Controller Layer
- 7 controllers handling HTTP requests
- Mix of proper abstraction and direct database access

---

## Architectural Decisions

### Dependency Injection Configuration (Program.cs)

```csharp
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddSession();
```

### Current State

| Aspect | Decision | Status |
|--------|----------|--------|
| Service Layer | Only PedidoService exists | Partial |
| Repository Pattern | Not implemented | Missing |
| Interface Usage | Only IPedidoService | Incomplete |
| DI Container | Built-in .NET DI | Implemented |
| Unit of Work | Not implemented | Missing |

### Controller Injection Patterns

| Controller | Dependency | Pattern |
|------------|------------|---------|
| PedidoController | IPedidoService | **Proper** - Abstracted |
| CarritoController | CatalogoWebContext + IPedidoService | **Mixed** |
| ProductosController | CatalogoWebContext | **Direct** |
| UsuariosController | CatalogoWebContext | **Direct** |
| CategoriasController | CatalogoWebContext | **Direct** |
| RolsController | CatalogoWebContext | **Direct** |
| HomeController | CatalogoWebContext | **Direct** |

---

## Data Flow

### Current (Problematic)
```
Controllers → DbContext → Database
```

### Expected (Clean Architecture)
```
Controllers → Services → Repositories → DbContext → Database
```

---

## Entity Relationships

```
Producto → Categoria (Many-to-One)
Pedido → Usuario (Many-to-One)
Pedido → PedidoDetalle (One-to-Many)
PedidoDetalle → Producto (Many-to-One)
Usuario → Rol (Many-to-One)
```

---

## Issues Identified

1. **Incomplete Service Layer**: Only 1 service exists for 7 controllers
2. **Direct DbContext Usage**: 5 out of 7 controllers directly inject DbContext
3. **No Repository Pattern**: Missing abstraction layer for data access
4. **God Interface**: IPedidoService has 18 methods (violates Interface Segregation)
5. **Business Logic in Controllers**: Filtering, querying embedded in controllers
6. **No Unit of Work**: Missing transaction management

---

## Recommendations

### Immediate
- Create service interfaces for Producto, Categoria, Usuario, Rol
- Refactor controllers to use services instead of DbContext

### Long-term
- Implement Repository pattern
- Add Unit of Work for transactions
- Split IPedidoService into smaller focused interfaces
