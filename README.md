# CatalogoWeb

E-commerce web application built with ASP.NET Core MVC for product catalog management and order processing.

## Tech Stack

- **Framework**: .NET 10.0
- **ORM**: Entity Framework Core 10.0.2
- **Database**: SQL Server
- **Frontend**: ASP.NET Core MVC with Razor Views
- **Session Management**: In-memory session storage

## Architecture

The project follows an ASP.NET Core MVC pattern with partial service abstraction:

```
Controllers → Services → DbContext → SQL Server
```

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic layer (currently only PedidoService implemented)
- **Data Access**: CatalogoWebContext (EF Core DbContext)

### Current State

- **1 Service with Interface**: `PedidoService` / `IPedidoService` (proper abstraction)
- **5 Controllers with direct DbContext**: ProductosController, UsuariosController, CategoriasController, RolsController, HomeController
- **1 Mixed Controller**: CarritoController (uses both DbContext and IPedidoService)
- **1 Proper Controller**: PedidoController (uses IPedidoService)

This represents a **Clean Architecture violation** - controllers should not directly access DbContext.

## Folder Structure

```
CatalogoWeb/
├── Controllers/           # MVC Controllers (7 total)
│   ├── HomeController.cs
│   ├── ProductosController.cs
│   ├── CategoriasController.cs
│   ├── UsuariosController.cs
│   ├── RolsController.cs
│   ├── PedidoController.cs
│   └── CarritoController.cs
├── DataContext/
│   └── CatalogoWebContext.cs
├── Interfaces/
│   └── IPedidoService.cs
├── Models/
│   ├── Dtos/
│   │   ├── CrearPedidoDto.cs
│   │   ├── ItemPedidoDto.cs
│   │   ├── DatosPagoDto.cs
│   │   └── ResultadoPagoDto.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── CarritoItem.cs
│   └── EstadoPedido.cs
├── Service/
│   └── PedidoService.cs
├── Views/
├── wwwroot/
├── Migrations/
├── Program.cs
└── CatalogoWeb.csproj
```

## Key Entities

| Entity | Description |
|--------|-------------|
| **Pedido** | Order with status tracking, payment info, and shipping details |
| **PedidoDetalle** | Order line items with product info |
| **Producto** | Product with price, stock, and category |
| **Categoria** | Product category |
| **Usuario** | User with role assignment |
| **Rol** | User role (Admin, Customer, etc.) |
| **CarritoItem** | Shopping cart item (session-based) |
| **EstadoPedido** | Order status enum (8 states) |

## Setup Instructions

### Prerequisites

- .NET 10.0 SDK
- SQL Server (LocalDB or full instance)

### Database Setup

1. Update connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CatalogoWebContext": "Server=(localdb)\\mssqllocaldb;Database=CatalogoWeb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. Apply migrations:

```bash
dotnet ef database update
```

Or create initial migration:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

### Build

```bash
dotnet build
```

### Clean and Rebuild

```bash
dotnet clean
dotnet build
```

## Features

- Product catalog with category filtering
- Shopping cart (session-based)
- Order processing with payment gateway stub
- Order status workflow (Pendiente → Pagado → EnPreparacion → Enviado → Entregado → Completado)
- Email notification stubs (confirmación, envío, entrega)
- User and role management

## Known Issues

- Controllers directly inject DbContext instead of using services (violates Clean Architecture)
- Missing services: UsuarioService, ProductoService, CategoriaService, RolService
- IPedidoService has too many methods (God Interface)
- No repository pattern implementation
- Email services are stubs that only record timestamps

## Future Improvements

See `docs/improvements.md` for prioritized recommendations.
