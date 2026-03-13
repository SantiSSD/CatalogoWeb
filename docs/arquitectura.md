# Documentación de Arquitectura - CatalogoWeb

## Visión General

CatalogoWeb es una aplicación de comercio electrónico ASP.NET Core MVC que sigue un patrón básico Modelo-Vista-Controlador. La aplicación gestiona productos, categorías, usuarios, pedidos y funcionalidad de carrito de compras.

---

## Stack Tecnológico

| Componente | Tecnología | Versión |
|------------|------------|---------|
| Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 10.0.2 |
| Base de Datos | SQL Server | - |
| Patrón de UI | MVC (Modelo-Vista-Controlador) | - |
| Gestión de Sesiones | ASP.NET Core Session | - |
| Validación | FluentValidation | - |

---

## Estructura de Carpetas del Proyecto

```
CatalogoWeb/
├── Controllers/          # Controladores MVC (7 controladores)
│   ├── CarritoController.cs
│   ├── CategoriasController.cs
│   ├── HomeController.cs
│   ├── PedidoController.cs
│   ├── ProductosController.cs
│   ├── RolsController.cs
│   └── UsuariosController.cs
├── DataContext/          # DbContext de Entity Framework
│   └── CatalogoWebContext.cs
├── Interfaces/           # Interfaces de Servicios
│   └── IPedidoService.cs
├── Models/               # Modelos de Entidad y DTOs
│   ├── CarritoItem.cs
│   ├── Categoria.cs
│   ├── EstadoPedido.cs
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Rol.cs
│   ├── Usuario.cs
│   ├── ErrorViewModel.cs
│   └── Dtos/
│       ├── CrearPedidoDto.cs
│       ├── DatosPagoDto.cs
│       ├── ItemPedidoDto.cs
│       └── ResultadoPagoDto.cs
├── Service/              # Servicios de Lógica de Negocio
│   └── PedidoService.cs
├── Migrations/           # Migraciones de EF Core
├── Views/                # Vistas Razor
├── Program.cs            # Punto de Entrada de la Aplicación
└── appsettings.json      # Configuración
```

---

## Implementación del Patrón MVC

### Capa de Modelos
- **Modelos de Entidad**: 8 entidades de dominio (Pedido, PedidoDetalle, Producto, Categoria, Usuario, Rol, CarritoItem, EstadoPedido)
- **DTOs**: 4 Objetos de Transferencia de Datos para comunicación de API
- **DbContext**: Único `CatalogoWebContext` gestionando todas las operaciones de base de datos

### Capa de Vistas
- Vistas Razor de ASP.NET Core
- Carrito de compras basado en sesiones

### Capa de Controladores
- 7 controladores manejando solicitudes HTTP
- Mezcla de abstracción adecuada y acceso directo a base de datos

---

## Decisiones de Arquitectura

### Configuración de Inyección de Dependencias (Program.cs)

```csharp
builder.Services.AddDbContext<CatalogoWebContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddSession();
```

### Estado Actual

| Aspecto | Decisión | Estado |
|---------|----------|--------|
| Capa de Servicios | Solo existe PedidoService | Parcial |
| Patrón Repository | No implementado | Falta |
| Uso de Interfaces | Solo IPedidoService | Incompleto |
| Contenedor DI | DI integrado de .NET | Implementado |
| Unit of Work | No implementado | Falta |

### Patrones de Inyección en Controladores

| Controlador | Dependencia | Patrón |
|-------------|--------------|--------|
| PedidoController | IPedidoService | **Adecuado** - Abstraído |
| CarritoController | CatalogoWebContext + IPedidoService | **Mixto** |
| ProductosController | CatalogoWebContext | **Directo** |
| UsuariosController | CatalogoWebContext | **Directo** |
| CategoriasController | CatalogoWebContext | **Directo** |
| RolsController | CatalogoWebContext | **Directo** |
| HomeController | CatalogoWebContext | **Directo** |

---

## Flujo de Datos

### Actual (Problemático)
```
Controladores → DbContext → Base de Datos
```

### Esperado (Arquitectura Limpia)
```
Controladores → Servicios → Repositorios → DbContext → Base de Datos
```

---

## Relaciones de Entidades

```
Producto → Categoria (Muchos a Uno)
Pedido → Usuario (Muchos a Uno)
Pedido → PedidoDetalle (Uno a Muchos)
PedidoDetalle → Producto (Muchos a Uno)
Usuario → Rol (Muchos a Uno)
```

---

## Problemas Identificados

1. **Capa de Servicios Incompleta**: Solo existe 1 servicio para 7 controladores
2. **Uso Directo de DbContext**: 5 de 7 controladores injectan DbContext directamente
3. **Sin Patrón Repository**: Falta capa de abstracción para acceso a datos
4. **Interfaz Dios**: IPedidoService tiene 18 métodos (viola Segregación de Interfaces)
5. **Lógica de Negocio en Controladores**: Filtrado, consultas embebidas en controladores
6. **Sin Unit of Work**: Falta gestión de transacciones

---

## Recomendaciones

### Inmediato
- Crear interfaces de servicio para Producto, Categoria, Usuario, Rol
- Refactorizar controladores para usar servicios en lugar de DbContext

### A Largo Plazo
- Implementar patrón Repository
- Agregar Unit of Work para transacciones
- Dividir IPedidoService en interfaces más pequeñas y enfocadas
