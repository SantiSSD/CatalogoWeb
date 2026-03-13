# CatalogoWeb

Aplicación web de comercio electrónico construida con ASP.NET Core MVC para la gestión de catálogos de productos y procesamiento de pedidos.

## Stack Tecnológico

- **Marco de trabajo**: .NET 10.0
- **ORM**: Entity Framework Core 10.0.2
- **Base de datos**: SQL Server
- **Interfaz**: ASP.NET Core MVC con Vistas Razor
- **Gestión de sesiones**: Almacenamiento en memoria

## Arquitectura

El proyecto sigue un patrón ASP.NET Core MVC con abstracción parcial de servicios:

```
Controladores → Servicios → DbContext → SQL Server
```

- **Controladores**: Manejan las solicitudes y respuestas HTTP
- **Services**: Capa de lógica de negocio (actualmente solo hay PedidoService implementado)
- **Acceso a datos**: CatalogoWebContext (DbContext de EF Core)

### Estado Actual

- **1 Servicio con Interfaz**: `PedidoService` / `IPedidoService` (abstracción correcta)
- **5 Controladores con DbContext directo**: ProductosController, UsuariosController, CategoriasController, RolsController, HomeController
- **1 Controlador Mixto**: CarritoController (usa tanto DbContext como IPedidoService)
- **1 Controlador Correcto**: PedidoController (usa IPedidoService)

Esto representa una **violación de Arquitectura Limpia** - los controladores no deberían acceder directamente al DbContext.

## Estructura de Carpetas

```
CatalogoWeb/
├── Controllers/           # Controladores MVC (7 en total)
│   ├── HomeController.cs
│   ├── ProductosController.cs
│   ├── CategoriasController.cs
│   ├── UsuariosController.cs
│   ├── RolsController.cs
│   ├── PedidoController.cs
│   └── CarritoController.cs
├── DataContext/
│   └── CatalogoWebContext.cs
├── Interfaces/
│   └── IPedidoService.cs
├── Models/
│   ├── Dtos/
│   │   ├── CrearPedidoDto.cs
│   │   ├── ItemPedidoDto.cs
│   │   ├── DatosPagoDto.cs
│   │   └── ResultadoPagoDto.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── CarritoItem.cs
│   └── EstadoPedido.cs
├── Service/
│   └── PedidoService.cs
├── Views/
├── wwwroot/
├── Migrations/
├── Program.cs
└── CatalogoWeb.csproj
```

## Entidades Principales

| Entidad | Descripción |
|---------|-------------|
| **Pedido** | Pedido con seguimiento de estado, información de pago y detalles de envío |
| **PedidoDetalle** | Líneas de pedido con información del producto |
| **Producto** | Producto con precio, stock y categoría |
| **Categoria** | Categoría de productos |
| **Usuario** | Usuario con asignación de rol |
| **Rol** | Rol de usuario (Admin, Cliente, etc.) |
| **CarritoItem** | Artículo del carrito de compras (basado en sesión) |
| **EstadoPedido** | Enum de estado del pedido (8 estados) |

## Instrucciones de Configuración

### Requisitos Previos

- .NET 10.0 SDK
- SQL Server (LocalDB o instancia completa)

### Configuración de la Base de Datos

1. Actualiza la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CatalogoWebContext": "Server=(localdb)\\mssqllocaldb;Database=CatalogoWeb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

2. Aplica las migraciones:

```bash
dotnet ef database update
```

O crea la migración inicial:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Ejecutar la Aplicación

```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001` o `http://localhost:5000`.

### Compilación

```bash
dotnet build
```

### Limpiar y Recompilar

```bash
dotnet clean
dotnet build
```

## Funcionalidades

- Catálogo de productos con filtrado por categoría
- Carrito de compras (basado en sesión)
- Procesamiento de pedidos con stub de pasarela de pago
- Flujo de estado de pedido (Pendiente → Pagado → EnPreparacion → Enviado → Entregado → Completado)
- Stubs de notificación por email (confirmación, envío, entrega)
- Gestión de usuarios y roles

## Problemas Conocidos

- Los controladores inyectan DbContext directamente en lugar de usar servicios (viola Arquitectura Limpia)
- Servicios faltantes: UsuarioService, ProductoService, CategoriaService, RolService
- IPedidoService tiene demasiados métodos (Interfaz Dios)
- No hay implementación del patrón Repository
- Los servicios de email son stubs que solo registran marcas de tiempo

## Mejoras Futuras

Consulta `docs/mejoras.md` para recomendaciones priorizadas.
