# Documentación de Flujo de Dependencias

## Visión General

Este documento analiza la configuración actual de Inyección de Dependencias (DI) en `Program.cs`, identifica las violaciones del Principio de Inversión de Dependencias (DIP) y proporciona una arquitectura de flujo de dependencias recomendada.

## Configuración Actual de DI

**Ubicación**: `Program.cs` (líneas 1-16)

```csharp
using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registro del contexto de base de datos
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoWebContext") 
        ?? throw new InvalidOperationException("Connection string 'CatalogoWebContext' not found.")));

// Registro de servicios
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Otros servicios
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddValidation();
```

---

## Flujo de Dependencias Actual

```
Controladores (Capa de Presentación)
    ↓
DbContext Directo O Capa de Servicios
    ↓
CatalogoWebContext (Capa de Datos)
    ↓
Base de Datos SQL Server
```

### Estado Actual

| Componente | Depende De | Estado |
|-----------|------------|--------|
| `PedidoController` | `IPedidoService` | ✓ Abstracción correcta |
| `CarritoController` | Mixto (Service + DbContext) | ⚠ Parcial |
| `ProductosController` | `CatalogoWebContext` | ✗ Dependencia directa |
| `UsuariosController` | `CatalogoWebContext` | ✗ Dependencia directa |
| `CategoriasController` | `CatalogoWebContext` | ✗ Dependencia directa |
| `RolsController` | `CatalogoWebContext` | ✗ Dependencia directa |
| `HomeController` | `CatalogoWebContext` | ✗ Dependencia directa |

---

## Violaciones del Principio de Inversión de Dependencias (DIP)

### ¿Qué es el DIP?

El Principio de Inversión de Dependencias establece:
1. Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.
2. Las abstracciones no deben depender de los detalles. Los detalles deben depender de abstracciones.

### Violaciones Identificadas

#### 1. Controladores Inyectando DbContext Directamente

**Ubicación**: Múltiples controladores

```csharp
public class ProductosController : Controller
{
    private readonly CatalogoWebContext _context;
    
    public ProductosController(CatalogoWebContext context)
    {
        _context = context;  // ❌ Dependencia directa de detalle de bajo nivel
    }
}
```

**Problemas**:
- Los controladores dependen de `DbContext` concreto en lugar de abstracciones
- La lógica de negocio se filtra hacia los controladores
- Dificultad para realizar pruebas unitarias (requiere base de datos)
- Viola la separación de responsabilidades

#### 2. Falta de Abstracción del Patrón Repository

Actual: Controladores → DbContext → Base de Datos

El `CatalogoWebContext` ES el repositorio (implementa el patrón Unit of Work internamente), pero:
- No hay abstracción de interfaz para acceso a datos
- Controladores fuertemente acoplados a specifics de EF Core
- No se puede intercambiar fuente de datos sin refactorizar controladores

#### 3. Capa de Servicios Incompleta

Solo existe `PedidoService` como servicio. Faltan:
- `IProductoService`
- `IUsuarioService`
- `ICategoriaService`
- `IRolService`

---

## Flujo de Dependencias Recomendado

### Arquitectura Objetivo

```
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA DE PRESENTACIÓN                         │
│  Controladores → ViewModels/DTOs → Vistas                      │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA DE APLICACIÓN                           │
│  Servicios (Lógica de Negocio)                                  │
│  IPedidoService → IPagoService → INotificacionService          │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA DE DOMINIO/INFRAESTRUCTURA              │
│  Repositorios (Acceso a Datos)                                  │
│  IPedidoRepository, IProductoRepository, IUsuarioRepository    │
└─────────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────────┐
│                    CAPA DE DATOS                                 │
│  CatalogoWebContext (EF Core) → SQL Server                      │
└─────────────────────────────────────────────────────────────────┘
```

### Registro de Dependencias Recomendado

```csharp
// Capa de Datos
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(connectionString));

// Repositorios (Abstracciones + Implementaciones)
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Servicios (Abstracciones + Implementaciones)
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<INotificacionService, NotificacionService>();
```

### Reglas de Dependencias

| Desde (Depende) | Hacia (Abstracción) | Ejemplo |
|-----------------|---------------------|---------|
| Controlador | Interfaz de Servicio | `IPedidoService` |
| Servicio | Interfaz de Repositorio | `IPedidoRepository` |
| Repositorio | DbContext | `CatalogoWebContext` |
| Servicio | Otros Servicios | `IPagoService`, `INotificacionService` |

---

## Pasos de Migración

### Fase 1: Añadir Interfaces de Repositorio

```csharp
public interface IPedidoRepository
{
    Task<List<Pedido>> ObtenerTodosAsync();
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<Pedido> AddAsync(Pedido pedido);
    Task UpdateAsync(Pedido pedido);
    Task DeleteAsync(int id);
}
```

### Fase 2: Crear Implementaciones de Repositorio

```csharp
public class PedidoRepository : IPedidoRepository
{
    private readonly CatalogoWebContext _context;
    
    public PedidoRepository(CatalogoWebContext context)
    {
        _context = context;
    }
    
    // Implementar operaciones CRUD
}
```

### Fase 3: Actualizar Servicios para Usar Repositorios

```csharp
public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    
    public PedidoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }
}
```

### Fase 4: Refactorizar Controladores

```csharp
public class ProductosController : Controller
{
    private readonly IProductoService _productoService;
    
    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }
}
```

---

## Archivos Afectados

- `Program.cs` - Añadir nuevos registros de DI
- Nuevo: `Interfaces/IPedidoRepository.cs`
- Nuevo: `Interfaces/IProductoRepository.cs`
- Nuevo: `Interfaces/IUsuarioRepository.cs`
- Nuevo: `Repositories/PedidoRepository.cs`
- Nuevo: `Repositories/ProductoRepository.cs`
- Nuevo: `Repositories/UsuarioRepository.cs`
- `Service/PedidoService.cs` - Inyectar repositorio en lugar de DbContext
- Controladores - Refactorizar para usar servicios

---

## Beneficios de la Arquitectura Recomendada

| Beneficio | Descripción |
|-----------|-------------|
| Testabilidad | Simular repositorios para pruebas unitarias |
| Flexibilidad | Intercambiar fuentes de datos (SQL → MongoDB) |
| Mantenibilidad | Separación clara de responsabilidades |
| Reusabilidad | Los servicios pueden ser utilizados por múltiples consumidores |
| Cumplimiento DIP | Módulos de alto nivel dependen de abstracciones |

---

## Documentación Relacionada

- [interfaces.md](interfaces.md) - Análisis de interfaces y recomendaciones de división
- [mejoras.md](mejoras.md) - Recomendaciones de mejora priorizadas
- [reporte-clean-architecture.md](reporte-clean-architecture.md) - Análisis completo de SOLID
