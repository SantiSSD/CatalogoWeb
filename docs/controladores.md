# Documentación de Controladores

Este documento detalla los 7 controladores de la aplicación CatalogoWeb.

## Visión General

La aplicación tiene 7 controladores con distintos niveles de calidad arquitectónica:

| # | Controlador | Nivel de Abstracción | Líneas |
|---|------------|---------------------|--------|
| 1 | PedidoController | Adecuada (IPedidoService) | 129 |
| 2 | CarritoController | Mixto (DbContext + IPedidoService) | 190 |
| 3 | ProductosController | DbContext Directo | 191 |
| 4 | UsuariosController | DbContext Directo | 178 |
| 5 | CategoriasController | DbContext Directo | 177 |
| 6 | RolsController | DbContext Directo | 157 |
| 7 | HomeController | DbContext Directo | 51 |

## 1. PedidoController (Abstracción Adecuada)

**Archivo**: `/Controllers/PedidoController.cs`

**Patrón**: Mejor práctica - usa abstracción de servicio mediante `IPedidoService`

### Dependencias

```csharp
private readonly IPedidoService _pedidoService;

public PedidoController(IPedidoService pedidoService)
{
    _pedidoService = pedidoService;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Listar todos los pedidos |
| Details | GET | Mostrar detalles del pedido |
| CompraExitosa | GET | Mostrar página de éxito |
| CompraFallida | GET | Mostrar página de fracaso |
| ReintentarPagoForm | GET | Mostrar formulario de reintento de pago |
| ReintentarPago | POST | Procesar reintento de pago |
| Pagar | GET | Marcar pedido como pagado |
| MarcarEnPreparacion | GET | Establecer estado: En Preparación |
| Enviar | GET | Establecer estado: Enviado (con seguimiento) |
| MarcarEntregado | GET | Establecer estado: Entregado |
| MarcarCompletado | GET | Establecer estado: Completado |
| Cancelar | GET | Cancelar pedido |

### Calidad del Código

- **Pros**: Inyección de dependencias adecuada, separación limpia de responsabilidades
- **Cons**: Expone demasiadas acciones de cambio de estado (podrían consolidarse)

---

## 2. CarritoController (Patrón Mixto)

**Archivo**: `/Controllers/CarritoController.cs`

**Patrón**: Mixto - usa tanto `CatalogoWebContext` directo como `IPedidoService`

### Dependencias

```csharp
private readonly CatalogoWebContext _context;
private readonly IPedidoService _pedidoService;

public CarritoController(CatalogoWebContext context, IPedidoService pedidoService)
{
    _context = context;
    _pedidoService = pedidoService;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Mostrar carrito de compras |
| Checkout | GET | Mostrar página de checkout |
| FinalizarCompra | POST | Procesar pedido y pago |
| ReintentarPago | POST | Reintentar pago fallido |
| Agregar | GET | Agregar ítem al carrito |
| Restar | GET | Disminuir cantidad del ítem |
| Eliminar | GET | Eliminar ítem del carrito |

### Uso Directo de DbContext

**Línea 122**: Agregar producto al carrito
```csharp
var producto = _context.Producto.FirstOrDefault(p => p.Id == id);
```

### Problemas

- **Abstracción mixta**: Usa servicio para pedidos pero DbContext directo para productos
- **Gestión de sesión**: Carrito almacenado en sesión (apropiado para este caso de uso)
- **Filtración de lógica de negocio**: Verificación de existencia de producto en controlador

---

## 3. ProductosController (DbContext Directo)

**Archivo**: `/Controllers/ProductosController.cs`

**Patrón**: Inyección de DbContext directo (viola Clean Architecture)

### Dependencias

```csharp
private readonly CatalogoWebContext _context;

public ProductosController(CatalogoWebContext context)
{
    _context = context;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Listar productos (con filtro/búsqueda) |
| Details | GET | Mostrar detalles del producto |
| Create | GET/POST | Crear nuevo producto |
| Edit | GET/POST | Editar producto |
| Delete | GET/POST | Eliminar producto |

### Métodos Clave

**Index (Líneas 23-52)**:
- Filtrar por `CategoriaId`
- Buscar por `Nombre`
- Carga eager de la propiedad de navegación `Categoria`

**Create (Líneas 87-97)**:
- Usa `[Bind]` para protección contra overposting
- Valida `ModelState.IsValid`

### Problemas

- **Sin capa de servicio**: Todas las operaciones de base de datos en el controlador
- **Lógica duplicada**: Carga de categorías repetida en múltiples acciones
- **Manejo de errores faltante**: Sin try-catch alrededor de operaciones de base de datos

---

## 4. UsuariosController (DbContext Directo)

**Archivo**: `/Controllers/UsuariosController.cs`

**Patrón**: Inyección de DbContext directo

### Dependencias

```csharp
private readonly CatalogoWebContext _context;

public UsuariosController(CatalogoWebContext context)
{
    _context = context;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Listar usuarios (con búsqueda) |
| Details | GET | Mostrar detalles del usuario |
| Create | GET/POST | Crear nuevo usuario |
| Edit | GET/POST | Editar usuario |
| Delete | GET/POST | Eliminar usuario |

### Problemas Clave

- **Línea 65**: Enlace de SelectList incorrecto (`"Id"` en lugar de `"Nombre"`)
- **Línea 99**: Mismo problema con el dropdown de Rol
- **Sin hash de contraseña**: `PasswordHash` almacenado en texto plano (problema de seguridad)
- **Servicio faltante**: Lógica de negocio específica del usuario embebida en el controlador

---

## 5. CategoriasController (DbContext Directo)

**Archivo**: `/Controllers/CategoriasController.cs`

**Patrón**: Inyección de DbContext directo

### Dependencias

```csharp
private readonly CatalogoWebContext _context;

public CategoriasController(CatalogoWebContext context)
{
    _context = context;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Listar categorías (con filtro) |
| Details | GET | Mostrar detalles de categoría |
| Create | GET/POST | Crear nueva categoría |
| Edit | GET/POST | Editar categoría |
| Delete | GET/POST | Eliminar categoría |

### Problemas

- CRUD simple sin lógica de negocio
- Se beneficiaría de un `CategoriaService` para consistencia

---

## 6. RolsController (DbContext Directo)

**Archivo**: `/Controllers/RolsController.cs`

**Patrón**: Inyección de DbContext directo

### Dependencias

```csharp
private readonly CatalogoWebContext _context;

public CategoriasController(CatalogoWebContext context)
{
    _context = context;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Listar roles |
| Details | GET | Mostrar detalles del rol |
| Create | GET/POST | Crear nuevo rol |
| Edit | GET/POST | Editar rol |
| Delete | GET/POST | Eliminar rol |

### Problemas

- Patrón CRUD simple
- Debería tener un `RolService` para consistencia

---

## 7. HomeController (DbContext Directo)

**Archivo**: `/Controllers/HomeController.cs`

**Patrón**: Inyección de DbContext directo

### Dependencias

```csharp
private readonly CatalogoWebContext _context;

public HomeController(CatalogoWebContext context)
{
    _context = context;
}
```

### Acciones

| Acción | Método | Descripción |
|--------|--------|-------------|
| Index | GET | Página principal con listado de productos |
| PruebaCarousel | GET | Vista de prueba del carrusel |

### Lógica Clave (Líneas 17-44)

La acción `Index` duplica la lógica de filtrado de `ProductosController`:
- Filtrar por `CategoriaId`
- Buscar por `Nombre`
- Carga eager de `Categoria`

### Problemas

- **Duplicación de código**: Misma lógica de filtrado en dos controladores
- **Demasiadas responsabilidades**: El controlador de inicio solo debería manejar enrutamiento de páginas

---

## Tabla Resumen

| Controlador | Líneas | Métodos | DbContext | Servicio | Problemas |
|-------------|--------|---------|-----------|----------|-----------|
| PedidoController | 129 | 12 | No | Sí | Interfaz Dios |
| CarritoController | 190 | 7 | Sí | Sí | Patrón mixto |
| ProductosController | 191 | 7 | Sí | No | Sin servicio |
| UsuariosController | 178 | 7 | Sí | No | Sin servicio |
| CategoriasController | 177 | 7 | Sí | No | Sin servicio |
| RolsController | 157 | 7 | Sí | No | Sin servicio |
| HomeController | 51 | 2 | Sí | No | Duplicación |

## Recomendaciones

1. **Crear servicios** para todas las entidades: `UsuarioService`, `ProductoService`, `CategoriaService`, `RolService`
2. **Refactorizar CarritoController** para usar servicio para búsquedas de productos
3. **Extraer lógica compartida** (filtrado, búsqueda) a clase base o servicio
4. **Corregir enlaces de SelectList** en UsuariosController
5. **Agregar hash de contraseña** a la creación de usuarios (seguridad)
