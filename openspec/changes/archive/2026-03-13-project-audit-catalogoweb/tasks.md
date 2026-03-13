# Tasks: Project Audit - CatalogoWeb

## Phase 1: Foundation / Setup

- [x] 1.1 Create README.md with project overview, tech stack (.NET 10.0, EF Core 10.0.2, SQL Server, ASP.NET Core MVC), architecture summary, and setup instructions
- [x] 1.2 Create docs/project-overview.md with business domain overview, key entities (Pedido, Producto, Usuario, Categoria, Rol), and project structure

## Phase 2: Core Architecture Documentation

- [x] 2.1 Create docs/architecture.md documenting the current MVC pattern, technology stack, project folder structure, and architectural decisions
- [x] 2.2 Create docs/clean-architecture-report.md documenting Clean Architecture violations: separation of concerns failure (5/7 controllers inject DbContext), dependency flow violations, SOLID principle violations (S, I, D), and specific violation locations

## Phase 3: Layer Documentation

- [x] 3.1 Create docs/layers.md documenting all application layers: Presentation (Controllers/Views), Business Logic (Services/), Data Access (CatalogoWebContext), and Models
- [x] 3.2 Create docs/controllers.md documenting all 7 controllers: PedidoController (proper abstraction), CarritoController (mixed), and 5 controllers with direct DbContext (Productos, Usuarios, Categorias, Rols, Home)
- [x] 3.3 Create docs/services.md documenting existing PedidoService (20+ methods, God Interface issue), missing services (UsuarioService, ProductoService, CategoriaService, RolService), and email/notification stubs

## Phase 4: Data and Models Documentation

- [x] 4.1 Create docs/models.md documenting all 8 entity models: Pedido, PedidoDetalle, Producto, Categoria, Usuario, Rol, CarritoItem, EstadoPedido
- [x] 4.2 Create docs/dtos.md documenting existing DTOs (CrearPedidoDto, ItemPedidoDto, DatosPagoDto, ResultadoPagoDto) and inconsistent usage patterns
- [x] 4.3 Create docs/data.md documenting CatalogoWebContext configuration, entity relationships (Producto→Categoria, Pedido→Usuario), and missing repository pattern

## Phase 5: Advanced Documentation

- [x] 5.1 Create docs/interfaces.md documenting existing IPedidoService interface, interface segregation violations, and recommendations for splitting into smaller interfaces
- [x] 5.2 Create docs/dependency-flow.md documenting current DI configuration in Program.cs, dependency inversion violations, and recommended dependency flow (Controllers → Services → Repositories → DataContext)
- [x] 5.3 Create docs/improvements.md with prioritized recommendations: High (create services, refactor controllers, implement Repository), Medium (split IPedidoService, fix naming), Low (email service, Unit of Work), including effort estimates

---

**Total Tasks**: 13 documentation deliverables organized in 5 phases

**Implementation Order**: Start with Phase 1 (foundation docs) to establish context, then Phase 2 (architecture analysis), followed by Phases 3-5 (detailed layer and improvement documentation). This follows natural dependency: understand project overview → architecture → specific components → recommendations.

**Next Step**: Ready for implementation (sdd-apply)
