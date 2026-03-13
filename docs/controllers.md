# Controllers Documentation

This document details all 7 controllers in the CatalogoWeb application.

## Overview

The application has 7 controllers with varying levels of architectural quality:

| # | Controller | Abstraction Level | Lines |
|---|------------|-------------------|-------|
| 1 | PedidoController | Proper (IPedidoService) | 129 |
| 2 | CarritoController | Mixed (DbContext + IPedidoService) | 190 |
| 3 | ProductosController | Direct DbContext | 191 |
| 4 | UsuariosController | Direct DbContext | 178 |
| 5 | CategoriasController | Direct DbContext | 177 |
| 6 | RolsController | Direct DbContext | 157 |
| 7 | HomeController | Direct DbContext | 51 |

## 1. PedidoController (Proper Abstraction)

**File**: `/Controllers/PedidoController.cs`

**Pattern**: Best practice - uses service abstraction via `IPedidoService`

### Dependencies

```csharp
private readonly IPedidoService _pedidoService;

public PedidoController(IPedidoService pedidoService)
{
    _pedidoService = pedidoService;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | List all orders |
| Details | GET | Show order details |
| CompraExitosa | GET | Show success page |
| CompraFallida | GET | Show failure page |
| ReintentarPagoForm | GET | Show retry payment form |
| ReintentarPago | POST | Process retry payment |
| Pagar | GET | Mark order as paid |
| MarcarEnPreparacion | GET | Set status: In Preparation |
| Enviar | GET | Set status: Shipped (with tracking) |
| MarcarEntregado | GET | Set status: Delivered |
| MarcarCompletado | GET | Set status: Completed |
| Cancelar | GET | Cancel order |

### Code Quality

- **Pros**: Proper dependency injection, clean separation of concerns
- **Cons**: Exposes too many status-changing actions (could be consolidated)

---

## 2. CarritoController (Mixed Pattern)

**File**: `/Controllers/CarritoController.cs`

**Pattern**: Mixed - uses both direct `CatalogoWebContext` and `IPedidoService`

### Dependencies

```csharp
private readonly CatalogoWebContext _context;
private readonly IPedidoService _pedidoService;

public CarritoController(CatalogoWebContext context, IPedidoService pedidoService)
{
    _context = context;
    _pedidoService = pedidoService;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | Show shopping cart |
| Checkout | GET | Show checkout page |
| FinalizarCompra | POST | Process order and payment |
| ReintentarPago | POST | Retry failed payment |
| Agregar | GET | Add item to cart |
| Restar | GET | Decrease item quantity |
| Eliminar | GET | Remove item from cart |

### Direct DbContext Usage

**Line 122**: Adding product to cart
```csharp
var producto = _context.Producto.FirstOrDefault(p => p.Id == id);
```

### Issues

- **Mixed abstraction**: Uses service for orders but direct DbContext for products
- **Session management**: Cart stored in session (appropriate for this use case)
- **Business logic leak**: Product existence check in controller

---

## 3. ProductosController (Direct DbContext)

**File**: `/Controllers/ProductosController.cs`

**Pattern**: Direct DbContext injection (violates Clean Architecture)

### Dependencies

```csharp
private readonly CatalogoWebContext _context;

public ProductosController(CatalogoWebContext context)
{
    _context = context;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | List products (with filter/search) |
| Details | GET | Show product details |
| Create | GET/POST | Create new product |
| Edit | GET/POST | Edit product |
| Delete | GET/POST | Delete product |

### Key Methods

**Index (Lines 23-52)**:
- Filter by `CategoriaId`
- Search by `Nombre`
- Eager loads `Categoria` navigation property

**Create (Lines 87-97)**:
- Uses `[Bind]` for overposting protection
- Validates `ModelState.IsValid`

### Issues

- **No service layer**: All database operations in controller
- **Duplicate logic**: Category loading repeated in multiple actions
- **Missing error handling**: No try-catch around database operations

---

## 4. UsuariosController (Direct DbContext)

**File**: `/Controllers/UsuariosController.cs`

**Pattern**: Direct DbContext injection

### Dependencies

```csharp
private readonly CatalogoWebContext _context;

public UsuariosController(CatalogoWebContext context)
{
    _context = context;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | List users (with search) |
| Details | GET | Show user details |
| Create | GET/POST | Create new user |
| Edit | GET/POST | Edit user |
| Delete | GET/POST | Delete user |

### Key Issues

- **Line 65**: Incorrect SelectList binding (`"Id"` instead of `"Nombre"`)
- **Line 99**: Same issue with Role dropdown
- **No password hashing**: `PasswordHash` stored in plain text (security issue)
- **Missing service**: User-specific business logic embedded in controller

---

## 5. CategoriasController (Direct DbContext)

**File**: `/Controllers/CategoriasController.cs`

**Pattern**: Direct DbContext injection

### Dependencies

```csharp
private readonly CatalogoWebContext _context;

public CategoriasController(CatalogoWebContext context)
{
    _context = context;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | List categories (with filter) |
| Details | GET | Show category details |
| Create | GET/POST | Create new category |
| Edit | GET/POST | Edit category |
| Delete | GET/POST | Delete category |

### Issues

- Simple CRUD with no business logic
- Would benefit from a `CategoriaService` for consistency

---

## 6. RolsController (Direct DbContext)

**File**: `/Controllers/RolsController.cs`

**Pattern**: Direct DbContext injection

### Dependencies

```csharp
private readonly CatalogoWebContext _context;

public CategoriasController(CatalogoWebContext context)
{
    _context = context;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | List roles |
| Details | GET | Show role details |
| Create | GET/POST | Create new role |
| Edit | GET/POST | Edit role |
| Delete | GET/POST | Delete role |

### Issues

- Simple CRUD pattern
- Should have a `RolService` for consistency

---

## 7. HomeController (Direct DbContext)

**File**: `/Controllers/HomeController.cs`

**Pattern**: Direct DbContext injection

### Dependencies

```csharp
private readonly CatalogoWebContext _context;

public HomeController(CatalogoWebContext context)
{
    _context = context;
}
```

### Actions

| Action | Method | Description |
|--------|--------|-------------|
| Index | GET | Home page with product listing |
| PruebaCarousel | GET | Test carousel view |

### Key Logic (Lines 17-44)

The `Index` action duplicates filtering logic from `ProductosController`:
- Filter by `CategoriaId`
- Search by `Nombre`
- Eager loads `Categoria`

### Issues

- **Code duplication**: Same filtering logic in two controllers
- **Too many responsibilities**: Home controller should only handle page routing

---

## Summary Table

| Controller | Lines | Methods | DbContext | Service | Issues |
|------------|-------|---------|-----------|---------|--------|
| PedidoController | 129 | 12 | No | Yes | God Interface |
| CarritoController | 190 | 7 | Yes | Yes | Mixed pattern |
| ProductosController | 191 | 7 | Yes | No | No service |
| UsuariosController | 178 | 7 | Yes | No | No service |
| CategoriasController | 177 | 7 | Yes | No | No service |
| RolsController | 157 | 7 | Yes | No | No service |
| HomeController | 51 | 2 | Yes | No | Duplication |

## Recommendations

1. **Create services** for all entities: `UsuarioService`, `ProductoService`, `CategoriaService`, `RolService`
2. **Refactor CarritoController** to use service for product lookups
3. **Extract shared logic** (filtering, search) to base class or service
4. **Fix SelectList bindings** in UsuariosController
5. **Add password hashing** to user creation (security)
