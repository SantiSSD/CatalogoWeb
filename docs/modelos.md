# Documentación de Modelos de Entidad

Este documento describe los 8 modelos de entidad en la aplicación CatalogoWeb.

---

## 1. Pedido (Order)

**Ubicación**: `Models/Pedido.cs`

Representa un pedido de un cliente en el sistema.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| Fecha | DateTime | Fecha de creación del pedido |
| Total | decimal | Monto total del pedido |
| Detalles | List\<PedidoDetalle\> | Línea de artículos del pedido |
| Estado | EstadoPedido | Enumeración del estado del pedido |
| UsuarioId | int? | Clave foránea a Usuario (anulable para compra como invitado) |
| Usuario | Usuario | Propiedad de navegación |
| Nombre | string | Nombre del cliente |
| Direccion | string | Dirección de entrega |
| Telefono | string | Teléfono de contacto |
| Ciudad | string | Ciudad de entrega |
| CodigoPostal | string | Código postal |
| MetodoPago | string | Método de pago |
| FechaPago | DateTime? | Fecha de pago |
| MensajeErrorPago | string? | Mensaje de error de pago |
| NumeroTracking | string? | Número de seguimiento del envío |
| FechaEnvio | DateTime? | Fecha de envío |
| FechaNotificacionVendedor | DateTime? | Marca de tiempo de notificación al vendedor |
| FechaEmailConfirmacion | DateTime? | Marca de tiempo del email de confirmación enviado |
| FechaEmailEnvio | DateTime? | Marca de tiempo del email de envío enviado |
| FechaEmailEntrega | DateTime? | Marca de tiempo del email de entrega enviado |
| FechaEmailValoracion | DateTime? | Marca de tiempo del email de solicitud de valoración |

---

## 2. PedidoDetalle (Order Detail)

**Ubicación**: `Models/PedidoDetalle.cs`

Representa una línea individual dentro de un pedido.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| PedidoId | int | Clave foránea a Pedido |
| Pedido | Pedido | Propiedad de navegación |
| ProductoId | int | Clave foránea a Producto |
| NombreProducto | string | Nombre del producto (desnormalizado) |
| DescripcionProducto | string | Descripción del producto (desnormalizado) |
| PrecioUnitario | decimal | Precio unitario al momento del pedido |
| Cantidad | int | Cantidad ordenada |
| SubTotal | decimal | Total de la línea (Cantidad × PrecioUnitario) |

---

## 3. Producto (Product)

**Ubicación**: `Models/Producto.cs`

Representa un producto en el catálogo.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| Nombre | string | Nombre del producto |
| Descripcion | string | Descripción del producto |
| Precio | decimal | Precio del producto (precisión: 10,2) |
| Stock | int | Inventario disponible |
| ImagenUrl | string | URL de la imagen del producto |
| CategoriaId | int | Clave foránea a Categoria |
| Categoria | Categoria? | Propiedad de navegación |

---

## 4. Categoria (Category)

**Ubicación**: `Models/Categoria.cs`

Representa una categoría de productos.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| Nombre | string | Nombre de la categoría |
| Descripcion | string | Descripción de la categoría |
| Productos | ICollection\<Producto\> | Propiedad de navegación a productos |

---

## 5. Usuario (User)

**Ubicación**: `Models/Usuario.cs`

Representa un usuario del sistema.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| Nombre | string | Nombre completo del usuario |
| Email | string | Dirección de correo electrónico del usuario |
| PasswordHash | string | Hash de la contraseña |
| RolId | int | Clave foránea a Rol |
| Rol | Rol | Propiedad de navegación |
| FechaRegistro | DateTime | Fecha de registro |

---

## 6. Rol (Role)

**Ubicación**: `Models/Rol.cs`

Representa un rol/nivel de permiso del usuario.

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| Id | int | Clave primaria |
| Nombre | string | Nombre del rol |
| Descripcion | string | Descripción del rol |

---

## 7. CarritoItem (Cart Item)

**Ubicación**: `Models/CarritoItem.cs`

Representa un artículo en el carrito de compras (basado en sesión, no persistido).

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| ProductoId | int | Identificador del producto |
| NombreProducto | string | Nombre del producto |
| PrecioUnitario | decimal | Precio unitario |
| Cantidad | int | Cantidad en el carrito |

---

## 8. EstadoPedido (Order Status)

**Ubicación**: `Models/EstadoPedido.cs`

Enumeración que representa los estados del ciclo de vida del pedido.

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 1 | Pendiente | Pedido creado, esperando pago |
| 2 | Pagado | Pago confirmado |
| 3 | PagoRechazado | Pago fallido |
| 4 | EnPreparacion | Pedido en preparación |
| 5 | Enviado | Pedido enviado |
| 6 | Entregado | Pedido entregado |
| 7 | Completado | Pedido completamente finalizado |
| 8 | Cancelado | Pedido cancelado |

---

## Resumen de Relaciones entre Modelos

```
Usuario (1) ────< Pedido (M)
Usuario (1) ────< Rol (1)
Categoria (1) ────< Producto (M)
Pedido (1) ────< PedidoDetalle (M)
```

---

## Problemas Identificados

1. **Desnormalización en PedidoDetalle**: NombreProducto y DescripcionProducto se almacenan de forma redundante en lugar de recuperarse mediante navegación
2. **Falta navegación en PedidoDetalle**: No existe navegación directa a la entidad Producto
3. **UsuarioId anulable**: Permite compra como invitado pero crea estado inconsistente
4. **Sin seguimiento de campos de auditoría**: Faltan CreatedAt, UpdatedAt, CreatedBy, UpdatedBy en la mayoría de las entidades
