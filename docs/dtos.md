# Documentación de DTOs

Este documento describe los Objetos de Transferencia de Datos utilizados en CatalogoWeb e identifica las inconsistencias de uso.

---

## 1. CrearPedidoDto

**Ubicación**: `Models/Dtos/CrearPedidoDto.cs`

Utilizado para crear nuevos pedidos a través de la API.

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

### Problemas:
- Utiliza tipos anulables para todos los campos excepto Items, pero no hay validación que distinga entre pedidos de invitados vs pedidos autenticados
- No tiene campo MetodoPago - el método de pago debe manejarse por separado

---

## 2. ItemPedidoDto

**Ubicación**: `Models/Dtos/ItemPedidoDto.cs`

Representa una línea individual al crear un pedido.

```csharp
public class ItemPedidoDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}
```

### Problemas:
- Falta PrecioUnitario - el precio debe obtenerse de la base de datos
- Falta NombreProducto - requiere búsqueda adicional
- Fuertemente acoplado a CrearPedidoDto - no puede reutilizarse independientemente

---

## 3. DatosPagoDto

**Ubicación**: `Models/Dtos/DatosPagoDto.cs`

Contiene información sin procesar de la tarjeta de pago.

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

### Problemas de Seguridad Críticos:
- **Almacena datos de tarjeta de crédito sin procesar** - CVV, número de tarjeta, expiración almacenados directamente
- **Violación de PCI DSS** - Nunca se debe almacenar o procesar datos de tarjeta sin procesar
- Debería utilizar tokens de pasarela de pago en su lugar

---

## 4. ResultadoPagoDto

**Ubicación**: `Models/Dtos/ResultadoPagoDto.cs`

Objeto de respuesta para resultados del procesamiento de pago.

```csharp
public class ResultadoPagoDto
{
    public bool Exito { get; set; }
    public string? MensajeError { get; set; }
    public int? PedidoId { get; set; }
}
```

---

## Patrones de Uso Inconsistentes

### Patrón 1: CrearPedidoDto vs enlace directo a modelo

**En PedidoController**:
- Utiliza `CrearPedidoDto` como entrada para el endpoint de API
- Pero también acepta el modelo `Pedido` directamente en otras acciones

**Ejemplo**:
```csharp
// Enfoque API (correcto)
[HttpPost]
public IActionResult Crear([FromBody] CrearPedidoDto crearPedidoDto)

// Enfoque MVC (inconsistente)
[HttpPost]
public IActionResult Create(Pedido pedido)
```

### Patrón 2: Faltan DTOs para respuestas

- No hay DTOs de respuesta definidos
- Los controladores devuelven entidades sin procesar (ej. `return View(pedido)`)
- No hay separación entre modelos internos y contratos de API

### Patrón 3: Duplicación de CarritoItem vs ItemPedidoDto

Ambos representan líneas de artículos con estructura casi idéntica:

| CarritoItem | ItemPedidoDto |
|-------------|---------------|
| ProductoId | ProductoId |
| NombreProducto | (falta) |
| PrecioUnitario | (falta) |
| Cantidad | Cantidad |

CarritoItem se usa para la sesión del carrito de compras, ItemPedidoDto para la creación de pedidos - no hay tipo base compartido.

### Patrón 4: DatosPagoDto expuesto a controladores

- Los datos de pago fluyen a través de los controladores en lugar de ser manejados por un servicio de pago dedicado
- No hay separación entre el procesamiento de pagos y la gestión de pedidos

---

## Recomendaciones

1. **Eliminar DatosPagoDto** - Reemplazar con enfoque de token de pasarela de pago
2. **Crear DTOs de respuesta** - Separar contratos de API de modelos internos
3. **Unificar representación de líneas de artículos** - Crear CartItemDto compartido o usar herencia
4. **Agregar DTOs de validación** - Separar validación de entrada de DTOs de negocio
5. **Eliminar datos de tarjeta sin procesar** - Implementar tokenización de pago adecuada
