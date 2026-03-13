# Interfaces Documentation

## Overview

This document details the existing interface `IPedidoService`, analyzes Interface Segregation Principle (ISP) violations, and provides recommendations for splitting into smaller, focused interfaces.

## Current IPedidoService Interface

**Location**: `Interfaces/IPedidoService.cs`

```csharp
public interface IPedidoService
{
    // Order lifecycle operations
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<int> PagarPedido(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);

    // Payment processing
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> RechazarPago(int pedidoId, string mensajeError);

    // State transitions
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);

    // Notifications (VICTIM OF ISP VIOLATION)
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

**Total Methods**: 20

---

## Interface Segregation Principle (ISP) Violations

### What is ISP?

The Interface Segregation Principle states: "Clients should not be forced to depend on methods they do not use." Instead of one large interface, multiple smaller, focused interfaces are preferable.

### Violations Identified

#### 1. God Interface (20 methods)

The `IPedidoService` interface contains 20 methods covering multiple unrelated responsibilities:

- **Order Management**: 6 methods (create, cancel, pay, send, get all, get by id)
- **Payment Processing**: 2 methods (process payment, reject payment)
- **State Transitions**: 4 methods (mark in preparation, shipped, delivered, completed)
- **Notifications**: 6 methods (notify seller, confirmation email, shipping email, delivery email, rating email)
- **Email Tracking**: 2 methods (send confirmation, send shipping)

#### 2. Notification Responsibility Leak

The notification methods violate ISP because:

- Any consumer of `IPedidoService` that only needs order CRUD operations is forced to implement or depend on notification functionality
- A controller that just needs to create orders must also expose email-sending methods
- The `PedidoService` implementation couples order business logic with email logic (lines 234-287 in `Service/PedidoService.cs`)

#### 3. Tight Coupling Example

```csharp
// A controller that only needs to create orders
public class PedidoController : Controller
{
    private readonly IPedidoService _pedidoService;
    
    // Forces dependency on ALL 20 methods, including email notifications
    // even if controller never sends emails
}
```

---

## Recommendations for Interface Splitting

### Proposed Interface Architecture

Split `IPedidoService` into focused, single-responsibility interfaces:

```csharp
// Core order operations (CRUD + lifecycle)
public interface IPedidoService
{
    Task<int> CrearPedidoAsync(CrearPedidoDto dto);
    Task<int> CancelarPedido(int pedidoId);
    Task<List<Pedido>> ObtenerTodos();
    Task<Pedido?> ObtenerPorId(int id);
}

// Payment operations
public interface IPagoService
{
    Task<int> PagarPedido(int pedidoId);
    Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
    Task<int> RechazarPago(int pedidoId, string mensajeError);
}

// Order state transitions
public interface IEstadoPedidoService
{
    Task<int> MarcarEnPreparacion(int pedidoId);
    Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
    Task<int> MarcarEntregado(int pedidoId);
    Task<int> MarcarCompletado(int pedidoId);
    Task<int> EnviarPedido(int pedidoId);
}

// Notification operations (SPLIT OFF)
public interface INotificacionService
{
    Task<int> NotificarVendedor(int pedidoId);
    Task<int> EnviarEmailConfirmacion(int pedidoId);
    Task<int> EnviarEmailEnvio(int pedidoId);
    Task<int> EnviarEmailEntrega(int pedidoId);
    Task<int> EnviarEmailValoracion(int pedidoId);
}
```

### Implementation Benefits

| Aspect | Before | After |
|--------|--------|-------|
| Methods per interface | 20 | 4-6 |
| Dependencies | Forced on all | Selective |
| Testing | Complex mocking | Focused unit tests |
| Coupling | High | Low |
| SRP Compliance | Violated | Compliant |

### Migration Strategy

1. Create new smaller interfaces (`IPagoService`, `IEstadoPedidoService`, `INotificacionService`)
2. Extract notification logic to separate `NotificacionService` class
3. Update `PedidoService` to inject and delegate to specialized services
4. Refactor controllers to depend only on needed interfaces
5. Update DI registration in `Program.cs`

---

## Files Affected

- `Interfaces/IPedidoService.cs` - Split into multiple interfaces
- `Service/PedidoService.cs` - Refactor to use composed services
- `Program.cs` - Update DI registrations
- Controllers using `IPedidoService` - Update dependencies

---

## Related Documentation

- [dependency-flow.md](dependency-flow.md) - Dependency injection configuration
- [improvements.md](improvements.md) - Prioritized recommendations
- [clean-architecture-report.md](clean-architecture-report.md) - SOLID violations analysis
