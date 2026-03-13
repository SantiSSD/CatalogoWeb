# Descripción General del Proyecto - CatalogoWeb

## Dominio de Negocio

CatalogoWeb es una aplicación web de comercio electrónico que proporciona:

- Navegación y búsqueda en el catálogo de productos
- Funcionalidad de carrito de compras
- Procesamiento y seguimiento de pedidos
- Gestión de usuarios y roles

## Entidades Principales

### Pedido

Representa un pedido de cliente con gestión completa del ciclo de vida.

```
Propiedades:
- Id: int (PK)
- Fecha: DateTime
- Total: decimal (precisión 18,2)
- Estado: EstadoPedido (enum)
- UsuarioId: int? (FK → Usuario)
- Nombre, Direccion, Telefono, Ciudad, CodigoPostal: Información del cliente
- MetodoPago: string
- FechaPago: DateTime?
- MensajeErrorPago: string?
- NumeroTracking: string?
- FechaEnvio: DateTime?
- Marcas de tiempo de email: FechaEmailConfirmacion, FechaEmailEnvio, FechaEmailEntrega, FechaEmailValoracion
- Detalles: List<PedidoDetalle>

Relaciones:
- Uno a Muchos con PedidoDetalle
- Muchos a Uno con Usuario (opcional)
```

### PedidoDetalle

Líneas de detalle dentro de un pedido.

```
Propiedades:
- Id: int (PK)
- PedidoId: int (FK → Pedido)
- ProductoId: int
- NombreProducto, DescripcionProducto: string (desnormalizado)
- PrecioUnitario: decimal
- Cantidad: int
- SubTotal: decimal (calculado)
```

### Producto

Artículos del catálogo disponibles para compra.

```
Propiedades:
- Id: int (PK)
- Nombre, Descripcion: string
- Precio: decimal (tipo de columna decimal(10,2))
- Stock: int
- ImagenUrl: string
- CategoriaId: int (FK → Categoria)

Relaciones:
- Muchos a Uno con Categoria
```

### Categoria

Categorización de productos.

```
Propiedades:
- Id: int (PK)
- Nombre, Descripcion: string

Relaciones:
- Uno a Muchos con Producto
```

### Usuario

Clientes registrados y administradores.

```
Propiedades:
- Id: int (PK)
- Nombre, Email: string
- PasswordHash: string
- RolId: int (FK → Rol)
- FechaRegistro: DateTime

Relaciones:
- Muchos a Uno con Rol
- Uno a Muchos con Pedido
```

### Rol

Roles de usuario para autorización.

```
Propiedades:
- Id: int (PK)
- Nombre, Descripcion: string

Relaciones:
- Uno a Muchos con Usuario
```

### CarritoItem

Artículos del carrito de compras almacenados en sesión.

```
Propiedades:
- ProductoId: int
- NombreProducto: string
- PrecioUnitario: decimal
- Cantidad: int
```

### EstadoPedido

Enum con 8 estados:

1. Pendiente
2. Pagado
3. PagoRechazado
4. EnPreparacion
5. Enviado
6. Entregado
7. Completado
8. Cancelado

## Estructura del Proyecto

```
CatalogoWeb/
├── Controllers/
│   ├── HomeController.cs          # Página principal, listado de productos con filtros
│   ├── ProductosController.cs    # CRUD de productos (DbContext directo)
│   ├── CategoriasController.cs   # CRUD de categorías (DbContext directo)
│   ├── UsuariosController.cs     # CRUD de usuarios (DbContext directo)
│   ├── RolsController.cs         # CRUD de roles (DbContext directo)
│   ├── PedidoController.cs       # Gestión de pedidos (IPedidoService)
│   └── CarritoController.cs       # Carrito + checkout (DbContext/IPedidoService mixto)
│
├── Service/
│   └── PedidoService.cs           # Más de 20 métodos relacionados con pedidos
│
├── Interfaces/
│   └── IPedidoService.cs          # Interfaz del servicio de pedidos
│
├── DataContext/
│   └── CatalogoWebContext.cs      # DbContext de EF Core
│
├── Models/
│   ├── Dtos/
│   │   ├── CrearPedidoDto.cs      # Entrada para creación de pedido
│   │   ├── ItemPedidoDto.cs       # Entrada de línea de pedido
│   │   ├── DatosPagoDto.cs        # Entrada de datos de pago
│   │   └── ResultadoPagoDto.cs    # Salida del resultado de pago
│   ├── Pedido.cs
│   ├── PedidoDetalle.cs
│   ├── Producto.cs
│   ├── Categoria.cs
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── CarritoItem.cs
│   └── EstadoPedido.cs
│
├── Views/                         # Vistas Razor
├── wwwroot/                       # Activos estáticos
├── Migrations/                    # Migraciones de EF Core
├── Program.cs                     # Configuración de la app y configuración de DI
└── CatalogoWeb.csproj             # Archivo del proyecto
```

## Flujo de Dependencias

**Actual (Viola Arquitectura Limpia)**:
```
Controladores → DbContext (directamente)
```

**Esperado (Arquitectura Limpia)**:
```
Controladores → Servicios → Repositorios → DbContext
```

## Relaciones de la Base de Datos

```
Usuario (1) ←——→ (N) Rol
Categoria (1) ←——→ (N) Producto
Usuario (1) ←——→ (N) Pedido
Pedido (1) ←——→ (N) PedidoDetalle
```

## Flujo de Trabajo de Pedidos

```
Pendiente → Pagado → EnPreparacion → Enviado → Entregado → Completado
     ↓           ↓
PagoRechazado   Cancelado
```

## Uso de DTOs

| DTO | Propósito | Uso |
|-----|-----------|-----|
| CrearPedidoDto | Crear nuevo pedido | CarritoController.FinalizarCompra |
| ItemPedidoDto | Línea de pedido | CrearPedidoDto.Items |
| DatosPagoDto | Detalles de pago | ProcesarPagoAsync |
| ResultadoPagoDto | Resultado del pago | Retorna éxito/fracaso |
