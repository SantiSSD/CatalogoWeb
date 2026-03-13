# Capas de la Aplicación

Este documento describe la arquitectura por capas de la aplicación CatalogoWeb.

## Visión General

La aplicación sigue un patrón MVC modificado con tres capas principales:

1. **Capa de Presentación** - Controladores y Vistas
2. **Capa de Lógica de Negocio** - Servicios
3. **Capa de Acceso a Datos** - DbContext y Modelos

## Capa de Presentación (Controllers/)

**Ubicación**: `/Controllers/`

La capa de presentación maneja las solicitudes y respuestas HTTP. Contiene 7 controladores:

| Controlador | Responsabilidad | Acceso a DbContext |
|-------------|-----------------|-------------------|
| `PedidoController` | Gestión de pedidos, procesamiento de pagos | Via IPedidoService |
| `CarritoController` | Operaciones del carrito de compras | Mixto (directo + servicio) |
| `ProductosController` | CRUD de productos | Directo |
| `UsuariosController` | Gestión de usuarios | Directo |
| `CategoriasController` | Gestión de categorías | Directo |
| `RolsController` | Gestión de roles | Directo |
| `HomeController` | Página principal, navegación de productos | Directo |

### Problemas Identificados

- **5 de 7 controladores** inyectan `CatalogoWebContext` directamente
- Solo `PedidoController` usa abstracción de servicio adecuada
- `CarritoController` usa un patrón mixto (directo + servicio)

## Capa de Lógica de Negocio (Services/)

**Ubicación**: `/Service/`

Contiene la lógica de negocio y orquestación:

| Servicio | Interfaz | Estado | Métodos |
|----------|----------|--------|---------|
| `PedidoService` | `IPedidoService` | Implementado | 20+ |
| `UsuarioService` | - | Faltante | - |
| `ProductoService` | - | Faltante | - |
| `CategoriaService` | - | Faltante | - |
| `RolService` | - | Faltante | - |

### Detalles de PedidoService

El `PedidoService` implementa la interfaz `IPedidoService` con más de 20 métodos:

**Gestión de Pedidos:**
- `ObtenerTodos()` - Obtener todos los pedidos
- `ObtenerPorId(int id)` - Obtener pedido por ID
- `CrearPedidoAsync(CrearPedidoDto dto)` - Crear nuevo pedido

**Procesamiento de Pagos:**
- `PagarPedido(int pedidoId)` - Marcar pedido como pagado
- `ProcesarPagoAsync(int pedidoId, DatosPagoDto)` - Procesar pago con pasarela
- `RechazarPago(int pedidoId, string mensaje)` - Rechazar pago

**Estado del Pedido:**
- `MarcarEnPreparacion(int pedidoId)` - Marcar en preparación
- `MarcarEnviado(int pedidoId, string tracking)` - Marcar como enviado
- `MarcarEntregado(int pedidoId)` - Marcar como entregado
- `MarcarCompletado(int pedidoId)` - Marcar como completado
- `CancelarPedido(int pedidoId)` - Cancelar pedido

**Notificaciones (Stubs):**
- `NotificarVendedor(int pedidoId)` - Stub
- `EnviarEmailConfirmacion(int pedidoId)` - Stub
- `EnviarEmailEnvio(int pedidoId)` - Stub
- `EnviarEmailEntrega(int pedidoId)` - Stub
- `EnviarEmailValoracion(int pedidoId)` - Stub

### Problema de Interfaz Dios

`IPedidoService` es una **Interfaz Dios** con más de 20 métodos violando el **Principio de Segregación de Interfaz (ISP)**. Recomendaciones:
- Dividir en interfaces más pequeñas: `IOrderQueryService`, `IOrderCommandService`, `IPaymentService`, `INotificationService`

## Capa de Acceso a Datos

**Ubicación**: `/DataContext/CatalogoWebContext.cs`

El `CatalogoWebContext` es el DbContext de EF Core que gestiona todas las operaciones de base de datos:

### DbSets

```csharp
public DbSet<Usuario> Usuario { get; set; }
public DbSet<Rol> Rol { get; set; }
public DbSet<Producto> Producto { get; set; }
public DbSet<Categoria> Categoria { get; set; }
public DbSet<Pedido> Pedido { get; set; }
public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
```

### Relaciones entre Entidades

- `Producto` → `Categoria` (Muchos a Uno, DeleteBehavior.Restrict)
- `Pedido` → `Usuario` (Muchos a Uno, opcional)
- `Pedido` → `PedidoDetalle` (Uno a Muchos)
- `Usuario` → `Rol` (Muchos a Uno)

### Configuración de Precisión

- `Pedido.Total`: `decimal(18,2)`
- `PedidoDetalle.PrecioUnitario`: `decimal(18,2)`
- `PedidoDetalle.SubTotal`: `decimal(18,2)`

## Capa de Modelos

**Ubicación**: `/Models/`

Contiene 8 modelos de entidad:

| Modelo | Propósito |
|--------|-----------|
| `Pedido` | Entidad de pedido con seguimiento de estado |
| `PedidoDetalle` | Líneas de detalle para pedidos |
| `Producto` | Ítems del catálogo de productos |
| `Categoria` | Categorías de productos |
| `Usuario` | Cuentas de usuario |
| `Rol` | Roles de usuario |
| `CarritoItem` | Ítems del carrito de compras (basado en sesión) |
| `EstadoPedido` | Enum para estados de pedido |

## Flujo de Dependencias (Actual vs. Recomendado)

### Actual (Violando Clean Architecture)

```
Controladores → DbContext (Directo)
```

### Recomendado

```
Controladores → Servicios → Repositorios → DbContext
```

## Componentes Faltantes

1. **Patrón Repository** - Sin abstracción sobre DbContext
2. **Unit of Work** - Sin gestión de transacciones
3. **Servicios Faltantes** - UsuarioService, ProductoService, CategoriaService, RolService
4. **Servicio de Email** - Sin implementación (todos son stubs)
