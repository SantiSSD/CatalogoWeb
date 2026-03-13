# AGENTS.md - Guía de Contexto para LLM

## 1. Descripción General del Proyecto

**CatalogoWeb** es una aplicación web de comercio electrónico desarrollada con ASP.NET Core MVC. Permite a los usuarios navegar productos por categorías, agregarlos al carrito, realizar pedidos y gestionar el proceso de pago. El sistema incluye gestión de pedidos con múltiples estados (pendiente, pagado, en preparación, enviado, entregado, completado, cancelado).

## 2. Tecnologías Usadas

- **Framework**: ASP.NET Core 8.0 (MVC)
- **Base de datos**: SQL Server con Entity Framework Core
- **ORM**: Entity Framework Core 8.x
- **Lenguaje**: C#
- **Frontend**: Razor Pages, HTML, CSS, JavaScript
- **Gestión de dependencias**: NuGet
- **Migraciones**: EF Core Migrations
- **Validación**: FluentValidation (configurado en Program.cs)

## 3. Arquitectura Actual

### MVC (Modelo-Vista-Controlador)
El proyecto sigue el patrón MVC típico de ASP.NET Core:
- **Models**: Entidades y DTOs
- **Views**: Páginas Razor
- **Controllers**: Controladores HTTP

### Clean Architecture - Violaciones Detectadas

El proyecto presenta varias violaciones de Clean Architecture que deben corregirse:

1. **Dependencia directa de DbContext en Controllers**: Los controllers injectan `CatalogoWebContext` directamente, violando el principio de inversión de dependencias.

2. **Lógica de negocio en Controllers**: Algunos controllers contain lógica de negocio que debería estar en servicios.

3. **Servicios incompletos**: Solo existe `PedidoService` con interfaz. Otros servicios faltan (ej: `IUsuarioService`, `IProductoService`, etc.).

4. **Sin separación de capas**: No hay una capa de aplicación o dominio claramente definida.

5. **DTOs en carpeta incorrecta**: Los DTOs están en `Models/Dtos/` en lugar de una carpeta separada.

6. **Models mezclados con DTOs**: Todas las clases de modelo están en la misma carpeta `Models/`.

## 4. Capas del Sistema

### Estructura actual:
```
CatalogoWeb/
├── Controllers/      # Capa de presentación (HTTP)
├── Models/           # Entidades + DTOs (sin separación)
├── DataContext/      # DbContext de EF
├── Service/          # Servicios con lógica de negocio
└── Interfaces/       # Contratos de servicios
```

### Capas ideales (Clean Architecture):
```
CatalogoWeb/
├── Domain/           # Entidades, value objects, interfaces de dominio
├── Application/      # Casos de uso, DTOs, servicios de aplicación
├── Infrastructure/   # DbContext, implementaciones de repositorios
└── Presentation/     # Controllers, Views, DTOs de API
```

## 5. Flujo de Datos

```
Usuario → Controller → Service → DbContext → SQL Server
                ↑                         ↓
                └─────────────────────────┘
                      (respuesta)
```

### Flujo de un pedido:
1. Usuario agrega productos al carrito (session)
2. Usuario inicia checkout → `CarritoController.FinalizarCompra`
3. Se crea `Pedido` con estado "Pendiente"
4. Se procesa pago → `PedidoService.ProcesarPagoAsync`
5. Si pago exitoso → estado "Pagado", se descuenta stock
6. Admin puede cambiar estado: En Preparación → Enviado → Entregado → Completado

## 6. Estructura de Carpetas

```
/home/santiago/Proyectos/CatalogoWeb/
├── Controllers/              # 7 controllers
│   ├── HomeController.cs
│   ├── ProductosController.cs
│   ├── CategoriasController.cs
│   ├── PedidoController.cs
│   ├── CarritoController.cs
│   ├── UsuariosController.cs
│   └── RolsController.cs
├── Models/
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── EstadoPedido.cs      # Enum
│   ├── CarritoItem.cs
│   └── Dtos/
│       ├── CrearPedidoDto.cs
│       ├── ItemPedidoDto.cs
│       ├── DatosPagoDto.cs
│       └── ResultadoPagoDto.cs
├── Service/
│   └── PedidoService.cs
├── Interfaces/
│   └── IPedidoService.cs
├── DataContext/
│   └── CatalogoWebContext.cs
├── Migrations/               # Migraciones de EF
├── Program.cs                # Configuración de startup
└── AGENTS.md                # Este archivo
```

## 7. Controllers (7 en total)

| Controller | Responsabilidad |
|------------|-----------------|
| `HomeController` | Página principal, listado de productos con filtros |
| `ProductosController` | CRUD completo de productos |
| `CategoriasController` | CRUD completo de categorías |
| `PedidoController` | Gestión de pedidos, pagos, cambios de estado |
| `CarritoController` | Carrito de compras, checkout, proceso de compra |
| `UsuariosController` | CRUD completo de usuarios |
| `RolsController` | CRUD completo de roles |

### Patrón de los Controllers:
- Constructor con inyección de `CatalogoWebContext` (excepto `PedidoController` que usa servicio)
- Métodos: Index, Details, Create, Edit, Delete
- Uso de `async/await` para operaciones de BD
- Validación con `ModelState.IsValid`

## 8. Services

### Service existente:
- `PedidoService` → `IPedidoService` ✓ (tiene interfaz)

### Services faltantes (deben crearse):
- `IUsuarioService` / `UsuarioService` - lógica de usuarios
- `IProductoService` / `ProductoService` - lógica de productos
- `ICategoriaService` / `CategoriaService` - lógica de categorías
- `IRolService` / `RolService` - lógica de roles
- `IAuthService` / `AuthService` - autenticación
- `ICarritoService` / `CarritoService` - lógica de carrito
- `IEmailService` / `EmailService` - notificaciones email (implementar真正的envío)
- `IPagoService` / `PagoService` - integración con gateway de pago

## 9. Interfaces

### Interfaces existentes:
- `IPedidoService` - contrato para operaciones de pedidos

### Interfaces a crear:
- `IUsuarioService`
- `IProductoService`
- `ICategoriaService`
- `IRolService`
- `IAuthService`
- `ICarritoService`
- `IEmailService`
- `IPagoService`

## 10. Models (8 entidades)

| Entidad | Propiedades principales |
|---------|------------------------|
| `Usuario` | Id, Nombre, Email, PasswordHash, RolId, FechaRegistro |
| `Rol` | Id, Nombre, Descripcion |
| `Producto` | Id, Nombre, Descripcion, Precio, Stock, ImagenUrl, CategoriaId |
| `Categoria` | Id, Nombre, Descripcion, Productos (colección) |
| `Pedido` | Id, Fecha, Total, Estado, UsuarioId, datos envío, datos pago, fechas de notificación |
| `PedidoDetalle` | Id, PedidoId, ProductoId, NombreProducto, PrecioUnitario, Cantidad, SubTotal |
| `EstadoPedido` | Enum: Pendiente, Pagado, PagoRechazado, EnPreparacion, Enviado, Entregado, Completado, Cancelado |
| `CarritoItem` | ProductoId, NombreProducto, PrecioUnitario, Cantidad (usado en sesión) |

### Relaciones:
- Usuario 1→N Rol (cada usuario tiene un rol)
- Categoria 1→N Producto
- Pedido 1→N PedidoDetalle
- Pedido 0..1→ Usuario

## 11. DTOs

| DTO | Uso |
|-----|-----|
| `CrearPedidoDto` | Datos para crear un nuevo pedido (items, datos del cliente) |
| `ItemPedidoDto` | Item individual en un pedido (ProductoId, Cantidad) |
| `DatosPagoDto` | Información de tarjeta para procesamiento de pago |
| `ResultadoPagoDto` | Resultado del procesamiento de pago (Exito, MensajeError, PedidoId) |

## 12. DbContext

**Archivo**: `DataContext/CatalogoWebContext.cs`

```csharp
public class CatalogoWebContext : DbContext
{
    public DbSet<Usuario> Usuario { get; set; }
    public DbSet<Rol> Rol { get; set; }
    public DbSet<Producto> Producto { get; set; }
    public DbSet<Categoria> Categoria { get; set; }
    public DbSet<Pedido> Pedido { get; set; }
    public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
}
```

### Configuración en OnModelCreating:
- Relación Producto → Categoria (1:N, DeleteBehavior.Restrict)
- Precisión decimal en Pedido.Total (18,2)
- Precisión decimal en PedidoDetalle.PrecioUnitario y SubTotal (18,2)

## 13. Dependencias

### Paquetes NuGet principales:
- `Microsoft.EntityFrameworkCore` (8.x)
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`
- `FluentValidation.AspNetCore`

### Configuración en Program.cs:
```csharp
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddValidation();
```

## 14. Inyección de Dependencias

### Registro actual en Program.cs:
```csharp
// DbContext
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoWebContext")));

// Services
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Otros
builder.Services.AddSession();
builder.Services.AddControllersWithViews();
builder.Services.AddValidation();
```

### Scope recomendado:
- `DbContext`: Scoped (por request)
- `Services`: Scoped
- `Controllers`: Transient (creados por el framework)

## 15. Reglas de Clean Architecture

### Principio de Responsabilidad Única (SRP)
- Cada clase debe tener una única responsabilidad
- Los controllers deben solo recibir requests y delegar a servicios
- La lógica de negocio va en servicios

### Principio de Inversión de Dependencias (DIP)
- Depender de abstracciones, no de concreciones
- Controllers deben depender de interfaces, no de DbContext directamente

### Separación de capas
1. **Domain**: Entidades, interfaces de repositorio
2. **Application**: Casos de uso, DTOs, mapeo
3. **Infrastructure**: DbContext, implementaciones de repositorio
4. **Presentation**: Controllers, Views

### Regla de dependencia
Las capas superiores no pueden conocer las inferiores. Domain no puede saber nada de Infrastructure.

## 16. Reglas del Proyecto

### Naming conventions:
- Clases: PascalCase (ej: `UsuarioController`, `PedidoService`)
- Interfaces: I + PascalCase (ej: `IPedidoService`)
- Métodos: PascalCase
- Propiedades: PascalCase
- Variables: camelCase
- Archivos: PascalCase.cs

### Estructura de archivos:
- Un archivo por clase
- Carpetas por tipo (Controllers, Models, Services, Interfaces)
- DTOs en subcarpeta `Models/Dtos/`

### Validación:
- Usar `ModelState.IsValid` en controllers
- Atributos de validación en modelos (required, range, etc.)
- Considerar FluentValidation para reglas complejas

### Errores:
- No usar excepciones para control de flujo
- Capturar excepciones específicas
- Devolver códigos de error apropiados

## 17. Convenciones de Código

### C#:
```csharp
namespace CatalogoWeb.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _pedidoService.ObtenerTodos();
            return View(pedidos);
        }
    }
}
```

### Properties:
- Usar property syntax `public int Id { get; set; }`
- Inicializar colecciones: `public ICollection<Producto> Productos { get; set; } = new List<Producto>();`

### Async:
- Siempre usar `async/await` para operaciones de BD
- Métodos de servicio retornar `Task<T>`
- Controllers usar `async Task<IActionResult>`

### Null safety:
- Usar null-conditional operator `?.`
- Inicializar strings con `string.Empty`
- Usar nullable reference types donde corresponda

## 18. Reglas para IA

### Antes de modificar código:
1. **Leer specs**: Consultar `openspec/changes/` para entender el contexto del cambio
2. **Leer diseño**: Revisar `design.md` del change para entender la aproximación técnica
3. **Leer código existente**: Entender patrones actuales antes de implementar

### Durante implementación:
1. **Seguir convenciones**: Mantener consistencia con código existente
2. **No freelancear**: Seguir la especificación, no implementarfeatures adicionales
3. **Documentar**: Añadir comments cuando la lógica sea compleja

### Después de implementar:
1. **Verificar**: Asegurar que la implementación cumple los specs
2. **Testear**: Ejecutar la aplicación y verificar funcionalidad
3. **Actualizar**: Marcar tareas como completadas en `tasks.md`

### Cosas a evitar:
- No crear archivos sin entender su propósito
- No modificar código que no está relacionado con la tarea
- No asumir que el usuario quiere más de lo que pide
- No introducir deuda técnica innecesaria

## 19. Reglas SDD (Spec-Driven Development)

### Flujo de trabajo:
1. El orchestrator crea un change con proposal, specs y design
2. Se implementan las tareas definidas en tasks.md
3. Se marca cada tarea como completada [x]
4. Al final, se verifica que todo matches las specs

### Archivos de un change:
- `proposal.md`: Propósito, alcance, aproximación
- `specs/`: Especificaciones detalladas del comportamiento
- `design.md`: Decisiones técnicas
- `tasks.md`: Lista de tareas a implementar

### Mode de artifact store:
- Configurado en `openspec/config.yaml`
- Modo actual: `openspec`
- Actualizar `tasks.md` al completar tareas

## 20. Cómo Trabajar en Este Proyecto

### Primeros pasos:
1. Clonar el repositorio
2. Restaurar paquetes: `dotnet restore`
3. Verificar conexión a SQL Server en `appsettings.json`
4. Ejecutar migraciones: `dotnet ef database update`
5. Ejecutar: `dotnet run`

### Estructura típica de una feature:
1. Crear/actualizar entidad en `Models/`
2. Crear DTO en `Models/Dtos/`
3. Crear interfaz en `Interfaces/`
4. Implementar servicio en `Service/`
5. Registrar en `Program.cs`
6. Crear/endpoints en Controller
7. Crear Views si es necesario

### Testing:
- Probar manualmente con `dotnet run`
- Verificar endpoints con Postman o navegador
- Revisar logs en caso de errores

## 21. Cómo Agregar Features

### Pasos:
1. **Analizar requerimiento**: Entender qué debe hacer la feature
2. **Diseñar solución**: Definir entidades, servicios, endpoints necesarios
3. **Implementar**: Seguir las reglas del proyecto
4. **Testear**: Verificar que funciona correctamente
5. **Documentar**: Actualizar AGENTS.md si es necesario

### Ejemplo: Agregar sistema de autenticación
1. Crear entidad `AuthToken` si es necesario
2. Crear `IAuthService` / `AuthService`
3. Agregar métodos de login/register
4. Crear controller `AuthController`
5. Configurar JWT en Program.cs
6. Agregar middleware de autenticación

## 22. Cómo Agregar Servicios

### Template:
```csharp
// Interfaces/INuevoService.cs
namespace CatalogoWeb.Interfaces
{
    public interface INuevoService
    {
        Task<TipoRetorno> MetodoAsync(parametros);
    }
}

// Service/NuevoService.cs
namespace CatalogoWeb.Service
{
    public class NuevoService : INuevoService
    {
        private readonly CatalogoWebContext _context;

        public NuevoService(CatalogoWebContext context)
        {
            _context = context;
        }

        public async Task<TipoRetorno> MetodoAsync(parametros)
        {
            // lógica
        }
    }
}

// Program.cs
builder.Services.AddScoped<INuevoService, NuevoService>();
```

### Reglas:
- Siempre crear interfaz primero
- Usar inyección de dependencias
- Preferir async/await
- Manejar errores apropiadamente

## 23. Cómo Agregar Controllers

### Template:
```csharp
namespace CatalogoWeb.Controllers
{
    public class NuevoController : Controller
    {
        private readonly INuevoService _nuevoService;

        public NuevoController(INuevoService nuevoService)
        {
            _nuevoService = nuevoService;
        }

        public async Task<IActionResult> Index()
        {
            var datos = await _nuevoService.ObtenerTodos();
            return View(datos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var dato = await _nuevoService.ObtenerPorId(id.Value);
            if (dato == null) return NotFound();
            return View(dato);
        }
    }
}
```

### Reglas:
- Usar inyección de dependencias (servicio, no DbContext directamente)
- Siempre usar async/await
- Validar parámetros
- Devolver tipos apropiados (View, Redirect, NotFound, etc.)

## 24. Cómo Agregar DTOs

### Ubicación:
`Models/Dtos/NombreDto.cs`

### Tipos de DTOs:
- **Request DTOs**: Para recibir datos del cliente
- **Response DTOs**: Para devolver datos al cliente
- **Command DTOs**: Para operaciones que modifican estado

### Ejemplo:
```csharp
namespace CatalogoWeb.Models.Dtos
{
    public class NuevoDto
    {
        public int Campo1 { get; set; }
        public string Campo2 { get; set; } = string.Empty;
        public decimal Campo3 { get; set; }
    }
}
```

### Reglas:
- No incluir propiedades que no se necesiten
- Usar tipos apropiados (int, string, decimal, etc.)
- Considerar validación con atributos

## 25. Cómo Agregar Entidades

### Ubicación:
`Models/NombreEntidad.cs`

### Template:
```csharp
namespace CatalogoWeb.Models
{
    public class NombreEntidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        
        // Relaciones
        public int OtraEntidadId { get; set; }
        public OtraEntidad? OtraEntidad { get; set; }
        
        // Colecciones
        public ICollection<Relacion> Relaciones { get; set; } = new List<Relacion>();
    }
}
```

### Después de crear entidad:
1. Agregar DbSet en `CatalogoWebContext`
2. Crear migración: `dotnet ef migrations add NombreMigration`
3. Ejecutar migración: `dotnet ef database update`
4. Crear servicio si tiene lógica de negocio

### Consideraciones:
- Usar tipos apropiados para propiedades (decimal para dinero, DateTime para fechas)
- Configurar restricciones en OnModelCreating si es necesario
- Considerar soft deletes si aplica

---

*Este documento fue generado automáticamente para proporcionar contexto a agentes LLM que trabajen en este proyecto.*