# Entity Models Documentation

This document describes all 8 entity models in the CatalogoWeb application.

---

## 1. Pedido (Order)

**Location**: `Models/Pedido.cs`

Represents a customer order in the system.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Fecha | DateTime | Order creation date |
| Total | decimal | Order total amount |
| Detalles | List\<PedidoDetalle\> | Order line items |
| Estado | EstadoPedido | Order status enum |
| UsuarioId | int? | Foreign key to Usuario (nullable for guest checkout) |
| Usuario | Usuario | Navigation property |
| Nombre | string | Customer name |
| Direccion | string | Delivery address |
| Telefono | string | Contact phone |
| Ciudad | string | Delivery city |
| CodigoPostal | string | Postal code |
| MetodoPago | string | Payment method |
| FechaPago | DateTime? | Payment date |
| MensajeErrorPago | string? | Payment error message |
| NumeroTracking | string? | Shipping tracking number |
| FechaEnvio | DateTime? | Shipping date |
| FechaNotificacionVendedor | DateTime? | Seller notification timestamp |
| FechaEmailConfirmacion | DateTime? | Confirmation email sent timestamp |
| FechaEmailEnvio | DateTime? | Shipping email sent timestamp |
| FechaEmailEntrega | DateTime? | Delivery email sent timestamp |
| FechaEmailValoracion | DateTime? | Rating request email timestamp |

---

## 2. PedidoDetalle (Order Detail)

**Location**: `Models/PedidoDetalle.cs`

Represents a single line item within an order.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| PedidoId | int | Foreign key to Pedido |
| Pedido | Pedido | Navigation property |
| ProductoId | int | Foreign key to Producto |
| NombreProducto | string | Product name (denormalized) |
| DescripcionProducto | string | Product description (denormalized) |
| PrecioUnitario | decimal | Unit price at time of order |
| Cantidad | int | Quantity ordered |
| SubTotal | decimal | Line total (Cantidad × PrecioUnitario) |

---

## 3. Producto (Product)

**Location**: `Models/Producto.cs`

Represents a product in the catalog.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Nombre | string | Product name |
| Descripcion | string | Product description |
| Precio | decimal | Product price (precision: 10,2) |
| Stock | int | Available inventory |
| ImagenUrl | string | Product image URL |
| CategoriaId | int | Foreign key to Categoria |
| Categoria | Categoria? | Navigation property |

---

## 4. Categoria (Category)

**Location**: `Models/Categoria.cs`

Represents a product category.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Nombre | string | Category name |
| Descripcion | string | Category description |
| Productos | ICollection\<Producto\> | Navigation property to products |

---

## 5. Usuario (User)

**Location**: `Models/Usuario.cs`

Represents a system user.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Nombre | string | User's full name |
| Email | string | User's email address |
| PasswordHash | string | Hashed password |
| RolId | int | Foreign key to Rol |
| Rol | Rol | Navigation property |
| FechaRegistro | DateTime | Registration date |

---

## 6. Rol (Role)

**Location**: `Models/Rol.cs`

Represents a user role/permission level.

| Property | Type | Description |
|----------|------|-------------|
| Id | int | Primary key |
| Nombre | string | Role name |
| Descripcion | string | Role description |

---

## 7. CarritoItem (Cart Item)

**Location**: `Models/CarritoItem.cs`

Represents an item in the shopping cart (session-based, not persisted).

| Property | Type | Description |
|----------|------|-------------|
| ProductoId | int | Product identifier |
| NombreProducto | string | Product name |
| PrecioUnitario | decimal | Unit price |
| Cantidad | int | Quantity in cart |

---

## 8. EstadoPedido (Order Status)

**Location**: `Models/EstadoPedido.cs`

Enumeration representing order lifecycle states.

| Value | Name | Description |
|-------|------|-------------|
| 1 | Pendiente | Order created, awaiting payment |
| 2 | Pagado | Payment confirmed |
| 3 | PagoRechazado | Payment failed |
| 4 | EnPreparacion | Order being prepared |
| 5 | Enviado | Order shipped |
| 6 | Entregado | Order delivered |
| 7 | Completado | Order fully completed |
| 8 | Cancelado | Order cancelled |

---

## Model Relationships Summary

```
Usuario (1) ────< Pedido (M)
Usuario (1) ────< Rol (1)
Categoria (1) ────< Producto (M)
Pedido (1) ────< PedidoDetalle (M)
```

---

## Issues Identified

1. **Denormalization in PedidoDetalle**: NombreProducto and DescripcionProducto are stored redundantly rather than being fetched via navigation
2. **Missing navigation in PedidoDetalle**: No direct navigation to Producto entity
3. **Nullable UsuarioId**: Allows guest checkout but creates inconsistent state
4. **No tracking of audit fields**: Missing CreatedAt, UpdatedAt, CreatedBy, UpdatedBy on most entities
