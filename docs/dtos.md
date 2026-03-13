# DTOs Documentation

This document describes the Data Transfer Objects used in CatalogoWeb and identifies usage inconsistencies.

---

## 1. CrearPedidoDto

**Location**: `Models/Dtos/CrearPedidoDto.cs`

Used for creating new orders via API.

```csharp
public class CrearPedidoDto
{
    public List<ItemPedidoDto> Items { get; set; } = new List<ItemPedidoDto>();
    public int? UsuarioId { get; set; }
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Ciudad { get; set; }
    public string? CodigoPostal { get; set; }
}
```

### Issues:
- Uses nullable types for all fields except Items, but no validation distinguishes guest vs authenticated orders
- No MetodoPago field - payment method must be handled separately

---

## 2. ItemPedidoDto

**Location**: `Models/Dtos/ItemPedidoDto.cs`

Represents a single line item when creating an order.

```csharp
public class ItemPedidoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
```

### Issues:
- Missing PrecioUnitario - price must be fetched from database
- Missing NombreProducto - requires additional lookup
- Tightly coupled to CrearPedidoDto - cannot be reused independently

---

## 3. DatosPagoDto

**Location**: `Models/Dtos/DatosPagoDto.cs`

Contains raw payment card information.

```csharp
public class DatosPagoDto
{
    public string MetodoPago { get; set; } = string.Empty;
    public string NumeroTarjeta { get; set; } = string.Empty;
    public string MesExpiracion { get; set; } = string.Empty;
    public string AnoExpiracion { get; set; } = string.Empty;
    public string CVV { get; set; } = string.Empty;
    public string NombreTitular { get; set; } = string.Empty;
}
```

### Critical Security Issues:
- **Stores raw credit card data** - CVV, card number, expiration stored directly
- **PCI DSS violation** - Should never store or process raw card data
- Should use payment gateway tokens instead

---

## 4. ResultadoPagoDto

**Location**: `Models/Dtos/ResultadoPagoDto.cs`

Response object for payment processing results.

```csharp
public class ResultadoPagoDto
{
    public bool Exito { get; set; }
    public string? MensajeError { get; set; }
    public int? PedidoId { get; set; }
}
```

---

## Inconsistent Usage Patterns

### Pattern 1: CrearPedidoDto vs direct model binding

**In PedidoController**:
- Uses `CrearPedidoDto` as input for API endpoint
- But also accepts `Pedido` model directly in other actions

**Example**:
```csharp
// API approach (correct)
[HttpPost]
public IActionResult Crear([FromBody] CrearPedidoDto crearPedidoDto)

// MVC approach (inconsistent)
[HttpPost]
public IActionResult Create(Pedido pedido)
```

### Pattern 2: Missing DTOs for responses

- No response DTOs defined
- Controllers return raw entities (e.g., `return View(pedido)`)
- No separation between internal models and API contracts

### Pattern 3: CarritoItem vs ItemPedidoDto duplication

Both represent line items with nearly identical structure:

| CarritoItem | ItemPedidoDto |
|-------------|---------------|
| ProductoId | ProductoId |
| NombreProducto | (missing) |
| PrecioUnitario | (missing) |
| Cantidad | Cantidad |

CarritoItem is used for shopping cart session, ItemPedidoDto for order creation - no shared base type.

### Pattern 4: DatosPagoDto exposed to controllers

- Payment data flows through controllers instead of being handled by dedicated payment service
- No separation between payment processing and order management

---

## Recommendations

1. **Remove DatosPagoDto** - Replace with payment gateway token approach
2. **Create response DTOs** - Separate API contracts from internal models
3. **Unify line item representation** - Create shared CartItemDto or use inheritance
4. **Add validation DTOs** - Separate input validation from business DTOs
5. **Remove raw card data** - Implement proper payment tokenization
