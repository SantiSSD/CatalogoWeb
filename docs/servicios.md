# Documentación de Servicios

Este documento detalla la capa de servicios de la aplicación CatalogoWeb.

## Estado Actual

| Servicio | Interfaz | Estado | Métodos |
|----------|----------|--------|---------|
| `PedidoService` | `IPedidoService` | Implementado | 20 |
| `UsuarioService` | - | **Faltante** | - |
| `ProductoService` | - | **Faltante** | - |
| `CategoriaService` | - | **Faltante** | - |
| `RolService` | - | **Faltante** | - |

## 1. PedidoService (Implementado)

**Archivo**: `/Service/PedidoService.cs`

**Interfaz**: `/Interfaces/IPedidoService.cs`

### Definición de Clase

```csharp
public class PedidoService : IPedidoService
{
    private readonly CatalogoWebContext _context;

    public PedidoService(CatalogoWebContext context)
    {
        _context = context;
    }
}
```

### Métodos (20 en total)

#### Métodos de Consulta

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `ObtenerTodos()` | `Task<List<Pedido>>` | Obtener todos los pedidos con detalles y usuario |
| `ObtenerPorId(int id)` | `Task<Pedido?>` | Obtener pedido por ID con detalles |

#### Métodos de Creación

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `CrearPedidoAsync(CrearPedidoDto dto)` | `Task<int>` | Crear pedido desde DTO |

#### Métodos de Pago

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `PagarPedido(int pedidoId)` | `Task<int>` | Marcar como pagado (simplificado) |
| `ProcesarPagoAsync(int, DatosPagoDto)` | `Task<ResultadoPagoDto>` | Procesamiento completo de pago |
| `RechazarPago(int, string)` | `Task<int>` | Rechazar pago |

#### Métodos de Transición de Estado

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `MarcarEnPreparacion(int)` | `Task<int>` | Pendiente → EnPreparacion |
| `MarcarEnviado(int, string)` | `Task<int>` | EnPreparacion/Pagado → Enviado |
| `MarcarEntregado(int)` | `Task<int>` | Enviado → Entregado |
| `MarcarCompletado(int)` | `Task<int>` | Entregado → Completado |
| `CancelarPedido(int)` | `Task<int>` | Cancelar (si no ha sido enviado) |

#### Stubs de Notificación/Email

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `NotificarVendedor(int)` | `Task<int>` | Stub - actualiza marca de tiempo |
| `EnviarEmailConfirmacion(int)` | `Task<int>` | Stub - actualiza marca de tiempo |
| `EnviarEmailEnvio(int)` | `Task<int>` | Stub - actualiza marca de tiempo |
| `EnviarEmailEntrega(int)` | `Task<int>` | Stub - actualiza marca de tiempo |
| `EnviarEmailValoracion(int)` | `Task<int>` | Stub - actualiza marca de tiempo |

### Interfaz IPedidoService

```csharp
public interface IPedidoService
{
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<int> PagarPedido(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);
    Task<int> RechazarPago(int pedidoId, string mensajeError);
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

### Problema de Interfaz Dios

La interfaz `IPedidoService` tiene **20 métodos** violando el **Principio de Segregación de Interfaz (ISP)**.

**Problemas**:
1. Clientes forzados a implementar/depender de métodos que no usan
2. Difícil de probar (demasiadas dependencias)
3. Difícil de mantener
4. No pueden tener diferentes implementaciones para diferentes escenarios

**División Recomendada**:

| Interfaz | Métodos | Propósito |
|----------|---------|-----------|
| `IOrderQueryService` | `ObtenerTodos()`, `ObtenerPorId(int)` | Operaciones de lectura |
| `IOrderCommandService` | `CrearPedidoAsync()`, `CancelarPedido()` | Operaciones de escritura |
| `IPaymentService` | `PagarPedido()`, `ProcesarPagoAsync()`, `RechazarPago()` | Lógica de pagos |
| `IOrderStatusService` | `MarcarEnPreparacion()`, `MarcarEnviado()`, `MarcarEntregado()`, `MarcarCompletado()` | Cambios de estado |
| `INotificationService` | Todos los stubs de email/notificación | Notificaciones |

---

## 2. Servicios Faltantes

Los siguientes servicios son necesarios para completar la refactorización de Clean Architecture:

### UsuarioService (Faltante)

**Interfaz Recomendada**:

```csharp
public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerTodos();
    Task<Usuario?> ObtenerPorId(int id);
    Task<Usuario?> ObtenerPorEmail(string email);
    Task<int> Crear(Usuario usuario);
    Task<int> Actualizar(Usuario usuario);
    Task<int> Eliminar(int id);
    Task<bool> ValidarCredenciales(string email, string password);
    Task<string> HashPassword(string password);
}
```

**Estado Actual**: Todas las operaciones de usuario en `UsuariosController` con DbContext directo

---

### ProductoService (Faltante)

**Interfaz Recomendada**:

```csharp
public interface IProductoService
{
    Task<List<Producto>> ObtenerTodos();
    Task<List<Producto>> ObtenerPorCategoria(int categoriaId);
    Task<List<Producto>> Buscar(string termino);
    Task<Producto?> ObtenerPorId(int id);
    Task<int> Crear(Producto producto);
    Task<int> Actualizar(Producto producto);
    Task<int> Eliminar(int id);
    Task<int> ActualizarStock(int id, int cantidad);
}
```

**Estado Actual**: Todas las operaciones de producto en `ProductosController` con DbContext directo

---

### CategoriaService (Faltante)

**Interfaz Recomendada**:

```csharp
public interface ICategoriaService
{
    Task<List<Categoria>> ObtenerTodos();
    Task<Categoria?> ObtenerPorId(int id);
    Task<Categoria?> ObtenerPorNombre(string nombre);
    Task<int> Crear(Categoria categoria);
    Task<int> Actualizar(Categoria categoria);
    Task<int> Eliminar(int id);
    Task<int> ObtenerProductoCount(int categoriaId);
}
```

**Estado Actual**: Todas las operaciones de categoría en `CategoriasController` con DbContext directo

---

### RolService (Faltante)

**Interfaz Recomendada**:

```csharp
public interface IRolService
{
    Task<List<Rol>> ObtenerTodos();
    Task<Rol?> ObtenerPorId(int id);
    Task<Rol?> ObtenerPorNombre(string nombre);
    Task<int> Crear(Rol rol);
    Task<int> Actualizar(Rol rol);
    Task<int> Eliminar(int id);
}
```

**Estado Actual**: Todas las operaciones de rol en `RolsController` con DbContext directo

---

## 3. Stubs de Email/Notificación

Todos los métodos de email y notificación en `PedidoService` son **stubs**:

### Patrón de Implementación Actual

```csharp
public async Task<int> EnviarEmailConfirmacion(int pedidoId)
{
    var pedido = await _context.Pedido.FindAsync(pedidoId);
    if (pedido == null)
        throw new Exception("Pedido no encontrado");

    pedido.FechaEmailConfirmacion = DateTime.Now;
    await _context.SaveChangesAsync();
    return pedidoId;
}
```

### Problemas

1. **Sin envío real de email**: Solo actualiza la marca de tiempo en la base de datos
2. **Efecto secundario en servicio**: Actualiza entidad mientras "envía" email
3. **Sin abstracción**: No se pueden cambiar proveedores de email
4. **Sin manejo de errores**: Sin lógica de reintento, sin notificaciones de fallo

### Servicio de Email Recomendado

```csharp
public interface IEmailService
{
    Task<bool> EnviarConfirmacionPedido(Pedido pedido);
    Task<bool> EnviarNotificacionEnvio(Pedido pedido, string tracking);
    Task<bool> EnviarNotificacionEntrega(Pedido pedido);
    Task<bool> EnviarSolicitudValoracion(Pedido pedido);
}
```

**Opciones de Implementación**:
- SendGrid
- Mailgun
- Amazon SES
- SMTP (incorporado)

---

## 4. Flujo de Dependencias de Servicios

### Actual (Incorrecto)

```
Controladores → DbContext (Directo)
PedidoController → IPedidoService → DbContext
```

### Recomendado

```
Controladores → Servicios → Repositorios → DbContext
                    ↓
              IEmailService
```

---

## 5. Registro de DI (Program.cs)

Registro actual (inferido):

```csharp
services.AddDbContext<CatalogoWebContext>();
services.AddScoped<IPedidoService, PedidoService>();
```

### Registro Recomendado

```csharp
// DbContext
services.AddDbContext<CatalogoWebContext>();

// Repositorios (por agregar)
services.AddScoped<IUsuarioRepository, UsuarioRepository>();
services.AddScoped<IProductoRepository, ProductoRepository>();
services.AddScoped<ICategoriaRepository, CategoriaRepository>();
services.AddScoped<IRolRepository, RolRepository>();
services.AddScoped<IPedidoRepository, PedidoRepository>();

// Servicios (interfaces divididas)
services.AddScoped<IOrderQueryService, PedidoService>();
services.AddScoped<IOrderCommandService, PedidoService>();
services.AddScoped<IPaymentService, PedidoService>();
services.AddScoped<IOrderStatusService, PedidoService>();

// Email
services.AddScoped<IEmailService, EmailService>();
```

---

## Resumen

| Componente | Estado | Problema |
|------------|--------|----------|
| PedidoService | Implementado | Interfaz Dios (20 métodos) |
| UsuarioService | Faltante | Necesita crearse |
| ProductoService | Faltante | Necesita crearse |
| CategoriaService | Faltante | Necesita crearse |
| RolService | Faltante | Necesita crearse |
| EmailService | Solo stub | Necesita implementación real |

## Recomendaciones de Prioridad

1. **Alta**: Crear `UsuarioService`, `ProductoService`, `CategoriaService`, `RolService`
2. **Alta**: Dividir `IPedidoService` en interfaces más pequeñas
3. **Media**: Implementar `IEmailService` real
4. **Baja**: Agregar patrón Unit of Work para transacciones
