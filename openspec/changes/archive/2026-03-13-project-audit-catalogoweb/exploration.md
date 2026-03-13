# Exploration: Project Audit - CatalogoWeb Architecture

### Current State

CatalogoWeb is an ASP.NET Core MVC e-commerce application using Entity Framework Core with SQL Server. The project follows a basic MVC pattern but has significant architectural issues preventing clean separation of concerns.

### Affected Areas

- **Controllers/**: 7 controllers - 5 directly inject DbContext instead of services
- **Service/**: Only 1 service (PedidoService) exists
- **Interfaces/**: Only 1 interface (IPedidoService) exists
- **Models/**: 8 entity models + 4 DTOs
- **DataContext/**: Single DbContext handling all entities
- **Program.cs**: Partial DI configuration

---

## Detailed Analysis

### 1. Architecture Analysis

#### Current Structure
| Layer | Components | Status |
|-------|-----------|--------|
| Presentation | Controllers (7), Views | ⚠️ Leaking business logic |
| Business Logic | PedidoService (1) | ⚠️ Only partial |
| Data Access | CatalogoWebContext | ✓ Centralized |
| Models | Entities + DTOs | ✓ Exists |

#### Technology Stack
- .NET 10.0
- Entity Framework Core 10.0.2 (SQL Server)
- Session-based shopping cart
- ASP.NET Core MVC

---

### 2. Clean Architecture Verification

#### ❌ Separation of Concerns **FAILED**
- Controllers directly use `CatalogoWebContext` in 5 out of 7 controllers
- Business logic mixed in Controllers (filtering, querying, state changes)

#### ❌ Dependency Flow **VIOLATED**
- Lower layers (Controllers) depend on higher layers (Data Access)
- Should be: Controllers → Services → Repositories → DataContext

#### ⚠️ SOLID Principles

**S - Single Responsibility**: ❌ VIOLATED
- `IPedidoService` has 20+ methods (God Interface)
- Controllers handle both HTTP and business logic

**O - Open/Closed**: ⚠️ Partial
- DTOs exist but not consistently used

**L - Liskov Substitution**: N/A (no inheritance chains)

**I - Interface Segregation**: ❌ VIOLATED
- `IPedidoService` is too large

**D - Dependency Inversion**: ⚠️ Partial
- Only `IPedidoService` is properly abstracted
- 5 controllers directly depend on `CatalogoWebContext`

---

### 3. Layer Analysis

#### Presentation Layer (Controllers)
| Controller | Injected Dependencies | Issues |
|------------|----------------------|--------|
| PedidoController | IPedidoService | ✓ Proper abstraction |
| CarritoController | CatalogoWebContext + IPedidoService | ⚠️ Mixed |
| ProductosController | CatalogoWebContext | ❌ Direct DB access |
| UsuariosController | CatalogoWebContext | ❌ Direct DB access |
| CategoriasController | CatalogoWebContext | ❌ Direct DB access |
| RolsController | CatalogoWebContext | ❌ Direct DB access |
| HomeController | CatalogoWebContext | ❌ Direct DB access |

#### Business Logic Layer (Services)
- **Only 1 service exists**: `PedidoService`
- **Missing services**: `UsuarioService`, `ProductoService`, `CategoriaService`, `RolService`
- Email/notification methods are stubs (only update timestamps)

#### Data Access Layer
- **DbContext**: `CatalogoWebContext` - properly configured
- **No repositories**: Direct DbContext usage in controllers
- **Relationships configured**: Producto→Categoria, Pedido→Usuario

#### Model Layer
- **Entities**: Pedido, PedidoDetalle, Producto, Categoria, Usuario, Rol, CarritoItem, EstadoPedido
- **DTOs**: CrearPedidoDto, ItemPedidoDto, DatosPagoDto, ResultadoPagoDto
- **Issue**: Inconsistent usage - some endpoints accept entities directly

---

### 4. Dependency Analysis

#### Current DI Configuration (Program.cs)
```csharp
builder.Services.AddDbContext<CatalogoWebContext>(...);
builder.Services.AddScoped<IPedidoService, PedidoService>();
```

#### Issues
1. Only `IPedidoService` is registered as interface
2. Most controllers receive concrete `CatalogoWebContext`
3. No interface segregation for other services
4. No Unit of Work pattern

#### Dependency Inversion
- ✅ Pedido follows DIP (depends on IPedidoService interface)
- ❌ All other controllers depend on concrete DbContext

---

### 5. Problems Detected

#### Naming Issues
| Issue | Location | Recommendation |
|-------|----------|----------------|
| `RolsController` | Controllers/ | Should be `RolesController` |
| Inconsistent pluralization | Controllers/ | Standardize (e.g., "Productos" not "Producto") |
| Missing I prefix | Interfaces/ | All interfaces should have "I" prefix |

#### Organization Issues
| Issue | Impact | Recommendation |
|-------|--------|----------------|
| Only 1 service exists | Business logic in controllers | Create services for all entities |
| No Repository pattern | No data access abstraction | Add repositories |
| No Unit of Work | No transaction management | Consider UoW pattern |

#### Responsibility Issues
| Issue | Location | Recommendation |
|-------|----------|----------------|
| CRUD in controllers | Productos, Usuarios, Categorias, Rols | Move to services |
| Session management in controller | CarritoController | Extract to separate class |
| Email stubs | PedidoService | Implement or remove |

#### Architecture Problems
1. **Tight Coupling**: Controllers → DbContext directly
2. **No Abstraction**: Missing interfaces for most operations
3. **Incomplete Layering**: Business logic in presentation layer
4. **God Interface**: IPedidoService with 20+ methods

#### Layer Problems
1. Controllers contain business logic (filtering with `.Where()`, `.Include()`)
2. No service layer for: Producto, Categoria, Usuario, Rol
3. Missing repository layer

---

## Recommendation

### Priority Actions

1. **High Priority**
   - Create service interfaces and implementations for: Producto, Categoria, Usuario, Rol
   - Refactor controllers to depend on services, not DbContext
   - Implement Repository pattern for data access

2. **Medium Priority**
   - Split `IPedidoService` into smaller interfaces (e.g., `IPedidoQueryService`, `IPedidoCommandsService`)
   - Extract Carrito logic to separate service
   - Fix `RolsController` naming

3. **Low Priority**
   - Implement proper email/notification service (or remove stubs)
   - Add Unit of Work pattern for transactions
   - Consider Mediator pattern for complex operations

### Readiness for Change
**Yes** - The project needs architectural refactoring before adding new features to maintain code quality and testability.

---

## Risks

- **Breaking Changes**: Refactoring will require updates to controllers and views
- **Testing Difficulty**: Current architecture makes unit testing hard
- **Maintenance**: Business logic scattered across layers
- **Scalability**: Difficult to extend with new features in current state
- **No Transaction Support**: Complex operations lack atomicity

---

## Artifacts

- This exploration report

## Next Recommended

1. Create services for all entities (Usuario, Producto, Categoria, Rol)
2. Implement Repository pattern
3. Refactor controllers to use services
4. Split IPedidoService into smaller interfaces
