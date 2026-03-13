# Project Overview - CatalogoWeb

## Business Domain

CatalogoWeb is an e-commerce web application that provides:
- Product catalog browsing and search
- Shopping cart functionality
- Order processing and tracking
- User and role management

## Key Entities

### Pedido (Order)

Represents a customer order with complete lifecycle management.

```
Properties:
- Id: int (PK)
- Fecha: DateTime
- Total: decimal (precision 18,2)
- Estado: EstadoPedido (enum)
- UsuarioId: int? (FK → Usuario)
- Nombre, Direccion, Telefono, Ciudad, CodigoPostal: Customer info
- MetodoPago: string
- FechaPago: DateTime?
- MensajeErrorPago: string?
- NumeroTracking: string?
- FechaEnvio: DateTime?
- Email timestamps: FechaEmailConfirmacion, FechaEmailEnvio, FechaEmailEntrega, FechaEmailValoracion
- Detalles: List<PedidoDetalle>

Relationships:
- One-to-Many with PedidoDetalle
- Many-to-One with Usuario (optional)
```

### PedidoDetalle (Order Detail)

Line items within an order.

```
Properties:
- Id: int (PK)
- PedidoId: int (FK → Pedido)
- ProductoId: int
- NombreProducto, DescripcionProducto: string (denormalized)
- PrecioUnitario: decimal
- Cantidad: int
- SubTotal: decimal (calculated)
```

### Producto (Product)

Catalog items available for purchase.

```
Properties:
- Id: int (PK)
- Nombre, Descripcion: string
- Precio: decimal (column type decimal(10,2))
- Stock: int
- ImagenUrl: string
- CategoriaId: int (FK → Categoria)

Relationships:
- Many-to-One with Categoria
```

### Categoria (Category)

Product categorization.

```
Properties:
- Id: int (PK)
- Nombre, Descripcion: string

Relationships:
- One-to-Many with Producto
```

### Usuario (User)

Registered customers and administrators.

```
Properties:
- Id: int (PK)
- Nombre, Email: string
- PasswordHash: string
- RolId: int (FK → Rol)
- FechaRegistro: DateTime

Relationships:
- Many-to-One with Rol
- One-to-Many with Pedido
```

### Rol (Role)

User roles for authorization.

```
Properties:
- Id: int (PK)
- Nombre, Descripcion: string

Relationships:
- One-to-Many with Usuario
```

### CarritoItem (Cart Item)

Shopping cart items stored in session.

```
Properties:
- ProductoId: int
- NombreProducto: string
- PrecioUnitario: decimal
- Cantidad: int
```

### EstadoPedido (Order Status)

Enum with 8 states:

1. Pendiente
2. Pagado
3. PagoRechazado
4. EnPreparacion
5. Enviado
6. Entregado
7. Completado
8. Cancelado

## Project Structure

```
CatalogoWeb/
├── Controllers/
│   ├── HomeController.cs          # Home page, product listing with filters
│   ├── ProductosController.cs    # CRUD for products (direct DbContext)
│   ├── CategoriasController.cs   # CRUD for categories (direct DbContext)
│   ├── UsuariosController.cs     # CRUD for users (direct DbContext)
│   ├── RolsController.cs         # CRUD for roles (direct DbContext)
│   ├── PedidoController.cs       # Order management (IPedidoService)
│   └── CarritoController.cs       # Cart + checkout (mixed DbContext/IPedidoService)
│
├── Service/
│   └── PedidoService.cs           # 20+ order-related methods
│
├── Interfaces/
│   └── IPedidoService.cs          # Order service interface
│
├── DataContext/
│   └── CatalogoWebContext.cs      # EF Core DbContext
│
├── Models/
│   ├── Dtos/
│   │   ├── CrearPedidoDto.cs      # Order creation input
│   │   ├── ItemPedidoDto.cs       # Order line item input
│   │   ├── DatosPagoDto.cs        # Payment data input
│   │   └── ResultadoPagoDto.cs    # Payment result output
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── CarritoItem.cs
│   └── EstadoPedido.cs
│
├── Views/                         # Razor views
├── wwwroot/                       # Static assets
├── Migrations/                    # EF Core migrations
├── Program.cs                     # App configuration & DI setup
└── CatalogoWeb.csproj             # Project file
```

## Dependency Flow

**Current (Violates Clean Architecture)**:
```
Controllers → DbContext (directly)
```

**Expected (Clean Architecture)**:
```
Controllers → Services → Repositories → DbContext
```

## Database Relationships

```
Usuario (1) ←——→ (N) Rol
Categoria (1) ←——→ (N) Producto
Usuario (1) ←——→ (N) Pedido
Pedido (1) ←——→ (N) PedidoDetalle
```

## Order Workflow

```
Pendiente → Pagado → EnPreparacion → Enviado → Entregado → Completado
     ↓           ↓
PagoRechazado   Cancelado
```

## DTOs Usage

| DTO | Purpose | Usage |
|-----|---------|-------|
| CrearPedidoDto | Create new order | CarritoController.FinalizarCompra |
| ItemPedidoDto | Order line item | CrearPedidoDto.Items |
| DatosPagoDto | Payment details | ProcesarPagoAsync |
| ResultadoPagoDto | Payment result | Returns success/failure |
