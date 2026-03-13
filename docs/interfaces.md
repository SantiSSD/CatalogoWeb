# Documentación de Interfaces

## Visión General

Este documento detalla la interfaz existente `IPedidoService`, analiza las violaciones del Principio de Segregación de Interfaces (ISP) y proporciona recomendaciones para dividirla en interfaces más pequeñas y enfocadas.

## Interfaz IPedidoService Actual

**Ubicación**: `Interfaces/IPedidoService.cs`

```csharp
public interface IPedidoService
{
    // Operaciones del ciclo de vida del pedido
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<int> PagarPedido(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);

    // Procesamiento de pagos
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> RechazarPago(int pedidoId, string mensajeError);

    // Transiciones de estado
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);

    // Notificaciones (VÍCTIMA DE VIOLACIÓN ISP)
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

**Total de Métodos**: 20

---

## Violaciones del Principio de Segregación de Interfaces (ISP)

### ¿Qué es el ISP?

El Principio de Segregación de Interfaces establece: "Los clientes no deben verse forzados a depender de métodos que no utilizan." En lugar de una interfaz grande, es preferible tener múltiples interfaces más pequeñas y enfocadas.

### Violaciones Identificadas

#### 1. Interfaz Dios (20 métodos)

La interfaz `IPedidoService` contiene 20 métodos que cubren múltiples responsabilidades no relacionadas:

- **Gestión de Pedidos**: 6 métodos (crear, cancelar, pagar, enviar, obtener todos, obtener por id)
- **Procesamiento de Pagos**: 2 métodos (procesar pago, rechazar pago)
- **Transiciones de Estado**: 4 métodos (marcar en preparación, enviado, entregado, completado)
- **Notificaciones**: 6 métodos (notificar vendedor, email de confirmación, email de envío, email de entrega, email de valoración)
- **Seguimiento por Email**: 2 métodos (enviar confirmación, enviar envío)

#### 2. Filtración de Responsabilidad de Notificaciones

Los métodos de notificación violan el ISP porque:

- Cualquier consumidor de `IPedidoService` que solo necesita operaciones CRUD de pedidos se ve obligado a implementar o depender de funcionalidad de notificaciones
- Un controlador que solo necesita crear pedidos también debe exponer métodos de envío de emails
- La implementación de `PedidoService` acopla la lógica de negocio de pedidos con la lógica de emails (líneas 234-287 en `Service/PedidoService.cs`)

#### 3. Ejemplo de Acoplamiento Estrecho

```csharp
// Un controlador que solo necesita crear pedidos
public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;
    
    // Fuerza dependencia de TODOS los 20 métodos, incluyendo notificaciones por email
    // incluso si el controlador nunca envía emails
}
```

---

## Recomendaciones para la División de Interfaces

### Arquitectura de Interfaces Propuesta

Dividir `IPedidoService` en interfaces enfocadas con responsabilidad única:

```csharp
// Operaciones principales de pedidos (CRUD + ciclo de vida)
public interface IPedidoService
{
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);
}

// Operaciones de pago
public interface IPagoService
{
    Task<int> PagarPedido(int pedidoId);
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> RechazarPago(int pedidoId, string mensajeError);
}

// Transiciones de estado del pedido
public interface IEstadoPedidoService
{
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
}

// Operaciones de notificaciones (SEPARADA)
public interface INotificacionService
{
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

### Beneficios de la Implementación

| Aspecto | Antes | Después |
|---------|-------|---------|
| Métodos por interfaz | 20 | 4-6 |
| Dependencias | Forzadas a todas | Selectivas |
| Pruebas | Simulación compleja | Pruebas unitarias enfocadas |
| Acoplamiento | Alto | Bajo |
| Cumplimiento SRP | Violado | Conforme |

### Estrategia de Migración

1. Crear nuevas interfaces más pequeñas (`IPagoService`, `IEstadoPedidoService`, `INotificacionService`)
2. Extraer la lógica de notificaciones a una clase separada `NotificacionService`
3. Actualizar `PedidoService` para inyectar y delegar a servicios especializados
4. Refactorizar los controladores para depender solo de las interfaces necesarias
5. Actualizar el registro de DI en `Program.cs`

---

## Archivos Afectados

- `Interfaces/IPedidoService.cs` - Dividir en múltiples interfaces
- `Service/PedidoService.cs` - Refactorizar para usar servicios compuestos
- `Program.cs` - Actualizar registros de DI
- Controladores que usan `IPedidoService` - Actualizar dependencias

---

## Documentación Relacionada

- [flujo-dependencias.md](flujo-dependencias.md) - Configuración de inyección de dependencias
- [mejoras.md](mejoras.md) - Recomendaciones priorizadas
- [reporte-clean-architecture.md](reporte-clean-architecture.md) - Análisis de violaciones SOLID
