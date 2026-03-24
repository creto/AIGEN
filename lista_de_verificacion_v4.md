# 🗺️ AIGEN — Derrotero de Desarrollo v4.0
**Proyecto:** AI-Powered Software Generator (AIGEN)  
**Stack:** .NET Core 8 · Scriban · EF Core · Dapper · Angular 18 · PrimeNG  
**BD Objetivo:** SQL Server · PostgreSQL · Oracle (roadmap)  
**Convención BD:** Línea base de almacenamiento (TM_ / TB_ / TBR_ / TP_ / TR_ / TA_ / TS_ / TH_)  
**Repositorio:** github.com/creto/AIGEN  
**Última actualización:** Marzo 2026 — Semana 11 completada

---

## 📊 Progreso General

```
Semana 1  ████████████████████ 100% ✅  Fundamentos CLI y schema reader
Semana 2  ████████████████████ 100% ✅  Clasificación de tablas y convenciones
Semana 3  ████████████████████ 100% ✅  Motor de plantillas Scriban
Semana 4  ████████████████████ 100% ✅  EF Core + Dapper híbrido
Semana 5  ████████████████████ 100% ✅  Primera generación real Doc4UsAIGen
Semana 6  ████████████████████ 100% ✅  Corrección masiva 1075 → 0 errores
Semana 7  ████████████████████ 100% ✅  Calidad código + docs + Git
Semana 8  ████████████████████ 100% ✅  API funcional + Swagger + DI auto-registro
Semana 9  ████████████████████ 100% ✅  Endpoints CRUD + validación BD real
Semana 10 ████████████████████ 100% ✅  Soft-delete + PostgreSQL + excludedPrefixes
Semana 11 ████████████████████ 100% ✅  Frontend Angular + JWT end-to-end + Naming fixes
Semana 12 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Microservicios + API Gateway
Semana 13 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Stored Procedures + funciones BD
Semana 14 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  IA integrada como asistente de desarrollo
Semana 15 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Multi-frontend + paleta de colores + logo
Semana 16 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Documentación completa + empaquetado
```

---

## ✅ SEMANA 1 — Fundamentos del CLI y lectura de schema
> **Estado: COMPLETADA ✅**

- [x] Estructura del proyecto (Clean Architecture: Core, Templates, CLI)
- [x] `SqlServerSchemaReader` — lectura de tablas, columnas, PKs, FKs, índices
- [x] `ColumnMetadata` y `TableMetadata` — modelos de metadatos
- [x] `NamingConventionService` — PascalCase, camelCase, kebab-case, plural español
- [x] `GeneratorConfig` / `aigen.json` — configuración de proyectos
- [x] CLI base con comando `read-schema` — visualización de tablas
- [x] Suite de tests Semana 1 — **pasando ✅**

---

## ✅ SEMANA 2 — Clasificación de tablas y convenciones de BD
> **Estado: COMPLETADA ✅** — 38 tests pasando

- [x] `TableType` enum — Movement, Basic, BasicRelated, Parameter, Relational, Composition, Audit, System, Historical, Image, Dictionary
- [x] `TableMetadataExtensions` — `GetTableType()` con mapa de prefijos Línea base de almacenamiento + heurística
- [x] `HasFullCrud()` / `IsReadOnly()` / `IsRelationalOnly()` — capacidades derivadas del tipo
- [x] `HasEstadoField()` — detección de columna Estado BIT
- [x] Análisis y adopción del estándar de nomenclatura Línea base de almacenamiento (SQL Server 2008 R2)
- [x] Decisión híbrida: preservar prefijos, PascalCase, audit fields, Estado BIT; modernizar tipos deprecados
- [x] Suite de tests Semana 2 — **38 tests pasando ✅**

---

## ✅ SEMANA 3 — Motor de plantillas Scriban
> **Estado: COMPLETADA ✅** — 19 tests nuevos

- [x] `ScribanTemplateEngine` — renderizado de `.scriban` con TemplateContext en snake_case
- [x] `TemplateLocator` — resolución de rutas de plantillas por capa y tipo
- [x] `FileGeneratorService` — orquestación de generación por tabla según TableType
- [x] `TemplateContext` v1 — contexto completo inyectado en plantillas
- [x] `GenerateCommand` CLI — selección interactiva de tablas + barra de progreso
- [x] **13 plantillas Scriban backend:** entity, dto, irepository, service, repository_dapper, repository_ef, controller, validator, entity_configuration, dbcontext, program, appsettings, solution
- [x] **Plantillas Angular:** model, service, list.component, form.component
- [x] Suite de tests Semana 3 — **pasando ✅**

---

## ✅ SEMANA 4 — EF Core + Dapper híbrido y lógica avanzada
> **Estado: COMPLETADA ✅**

- [x] ORM `EFCoreWithDapper` — estrategia híbrida (EF para CRUD simple, Dapper para queries complejas)
- [x] `entity_configuration.scriban` — mapeo Fluent API completo con FK relations
- [x] `DbContext` con todos los `DbSet<T>` y configuraciones
- [x] Generación selectiva por tipo de tabla — Audit/Historical solo generan Entity
- [x] Soporte `excludedTables` en `aigen.json`
- [x] Primera ejecución exitosa `generate` contra BD Doc4UsAIGen (287 tablas → 2131 archivos)
- [x] `Doc4Us.sln` generado con 4 proyectos: Domain, Application, Infrastructure, API

---

## ✅ SEMANA 5 — Primera generación real contra Doc4UsAIGen
> **Estado: COMPLETADA ✅**

- [x] Conexión a BD real: `200.31.22.7,1437` · `Doc4UsAIGen` · 272 tablas activas
- [x] Generación completa: 2131 archivos en `C:\DevOps\AIGEN\AIGEN\Generated`
- [x] `Doc4Us.Domain` ✅ — compila sin errores
- [x] `Doc4Us.Application` ✅ — compila sin errores
- [x] `Doc4Us.Infrastructure` ❌ — 1075 errores identificados (punto de partida Semana 6)
- [x] Diagnóstico inicial: errores CS0037, CS1061, CS0023 — patrones identificados

---

## ✅ SEMANA 6 — Corrección masiva de errores Infrastructure (1075 → 0)
> **Estado: COMPLETADA ✅** — hito mayor del proyecto

- [x] **Bug 1 — CS1061:** `.Value` en tipos NOT NULL → fix `is_value_type` en TemplateContext (1075 → 896)
- [x] **Bug 2 — CS1061:** UpdateRequest con `bool`/`int` no-nullable → dto.scriban v2 con `T?` (896 → 61)
- [x] **Bug 3 — Colisión ClassName:** `TH_Serie` y `TB_Serie` → sufijo `Hist` en NamingConventionService (61 → 15)
- [x] **Bug 4 — PK compuesta:** column names SQL en lugar de property names C# → `col_map` en entity_configuration.scriban (15 → 2)
- [x] **Bug 5 — ToggleEstado:** propiedad hardcodeada `entity.Estado` → `EstadoPropertyName` en TemplateContext (2 → **0**)
- [x] **`Doc4Us.sln` compila con 0 errores** ✅

---

## ✅ SEMANA 7 — Calidad del código generado y fixes residuales
> **Estado: COMPLETADA ✅**

- [x] Fix definitivo `EstadoPropertyName` en `TemplateContext` + `repository_ef.scriban`
- [x] `#nullable enable` en todas las plantillas .scriban — 894 → 0 warnings
- [x] Fix `byte[]` NOT NULL en `entity.scriban` → `= Array.Empty<byte>()`
- [x] Fix `hierarchyid` SQL Server → `"string"` en SqlServerSchemaReader
- [x] Fix `DateOnly`/`TimeOnly` en lista ValueTypes de TemplateContext
- [x] Plantillas de documentación: `architecture.scriban`, `deployment.scriban`
- [x] `FileGeneratorService` — generación automática de ARCHITECTURE.md y DEPLOYMENT.md
- [x] Script `rebuild_and_generate.ps1` v1 — 6 pasos: clean, build, test, generate, compile, CODE_SUMMARY.md
- [x] **Resultado: 2125 archivos · 221,940 líneas · 0 errores · 0 warnings** ✅
- [x] Git — conflicto de credenciales resuelto, push a github.com/creto/AIGEN ✅

---

## ✅ SEMANA 8 — API Funcional con Swagger
> **Estado: COMPLETADA ✅** — hito de ejecución end-to-end

- [x] `FileGeneratorService` — `HasRepository` / `HasService` flags en orden correcto
- [x] `TemplateContext` — propiedad `AllTablesScript` (List<ScriptObject>) para iterar tablas en Scriban
- [x] `program.scriban` — DI auto-registro de 135 repositorios + 135 servicios
- [x] `program.scriban` — usings de namespaces deduplicados con `ns_seen`
- [x] `AllTablesScript` expone: `RepositoryName`, `ServiceName`, `HasRepository`, `HasService`, `ClassNamePlural`
- [x] `controller.scriban` — fix constraint `{id:string}` inválido → condicional por `pk_type`
- [x] `Generated/` excluido del repositorio Git (.gitignore)
- [x] **`dotnet run` → `http://localhost:5000/swagger` con 135 controllers** ✅
- [x] **0 errores · 0 warnings de compilación** ✅

---

## ✅ SEMANA 9 — Validación End-to-End contra BD Real
> **Estado: COMPLETADA ✅**

- [x] **Scaffolding Angular completo** — 12 plantillas generadas, ng build 0 errores 0 warnings
- [x] `KebabName` agregado a `AllTablesScript` en `TemplateContext`
- [x] `PrimaryColor` / `SecondaryColor` / `DbName` en `FrontendConfig` y `TemplateContext`
- [x] Script `rebuild_and_generate.ps1` v2.0 — 8 pasos con flags `-SkipTests`, `-SkipFrontend`, `-RunApi`
- [x] **Fix FK duplicadas** — `col_seen` en `entity.scriban` para deduplicar por `column_name`
- [x] **Fix Shadow properties EF Core** — bloque `HasOne().WithMany()` eliminado de `entity_configuration.scriban`
- [x] **Fix string nullable** — `string?` en `entity.scriban` para columnas string nullable
- [x] **Fix DTO string nullable** — `string?` en `dto.scriban` Create/Update (causa 400 Bad Request)
- [x] **Fix angular_list_component** — rutas `../../` + `error: (_: unknown) =>`
- [x] **Fix angular_form_component** — patrón `if/else` para subscribe (TS2349)
- [x] `NullableSchemaFilter` en Swagger — `int?` muestra `null` en lugar de `0`
- [x] **CRUD validado contra BD real Doc4UsAIGen:**
  - GET paginado → 200 + 270 registros reales ✅
  - GET by ID → 200 + registro correcto ✅
  - POST crear → 201 + ID generado ✅
  - PUT actualizar → 204 ✅
  - DELETE → 204 ✅
  - PATCH toggle estado → 204 ✅
- [x] **0 errores · 0 warnings** ✅
- [x] Commit en github.com/creto/AIGEN ✅

---

## ✅ SEMANA 10 — Soft-Delete + PostgreSQL + Configuración avanzada
> **Estado: COMPLETADA ✅**

- [x] **Soft-delete via Estado** — árbol de decisión en `repository_ef.scriban`:
  - Tiene `Eliminado` → `Eliminado = true` (soft-delete)
  - Tiene `Estado` → `Estado = false` (soft-delete via estado)
  - Sin ninguno → delete físico real
- [x] **BaseQuery filtra `Estado == true`** — GET/GetPaged solo retorna registros activos
- [x] **Update solo sobre registros activos** — `e.Estado == true` en `FirstOrDefaultAsync`
- [x] **DELETE validado** — registro inactivo retorna 404 en GET ✅
- [x] **Swagger nullables** — `NullableSchemaFilter` agregado en `program.scriban` después de `app.Run()`
- [x] **`excludedPrefixes`** — implementado en `DatabaseConfig.cs` y `SchemaFilterService.cs`
  - `aigen.json`: `"excludedPrefixes": ["TH_"]` excluye todas las tablas con ese prefijo
  - Tablas `TH_` confirmadas excluidas del output generado ✅
- [x] **PostgreSQL support completo:**
  - `PostgreSqlSchemaReader` inyecta `NamingConventionService` (igual que SqlServer)
  - Prefijos `TB_`, `TP_` etc. eliminados correctamente en naming
  - `NormalizePostgresName()` — normaliza columnas lowercase de PostgreSQL (`creadoen` → `CreadoEn`)
  - Aliases lowercase en `AuditColumns` para reconocer audit fields de PostgreSQL
  - Generación validada contra BD `aigen_test` (PostgreSQL 18 local) ✅
  - `AigenTest.sln` compila con 0 errores ✅
  - Config de prueba en `aigen\configs\aigen_postgres.json` ✅
- [x] **GitHub** — archivo `aigen.rar` (133MB) eliminado del historial con `git filter-branch` + force push ✅
- [x] **0 errores · 0 warnings** ✅

---

## ✅ SEMANA 11 — Frontend Angular + JWT end-to-end + Naming fixes
> **Estado: COMPLETADA ✅** — 2077 archivos · 0 errores · 0 warnings

### Fix 1 — KebabName con acrónimos
- [x] `NamingConventionService.ToAngularFileName()` — regex doble: `([A-Z]+)([A-Z][a-z])` + `([a-z\d])([A-Z])`
- [x] `TemplateContext.ToKebab()` — mismo fix aplicado al método privado usado en `AngularFileName` y `AngularSelector`
- [x] Validado: `MCDT` → `mcdt` ✅ · `PERU` → `peru` ✅ · `MCDTAlgo` → `mcdt-algo` ✅ · `OrdenCompra` → `orden-compra` ✅

### Fix 2 — Servicios duplicados ClassNamePlural
- [x] `FileGeneratorService.GenerateAsync()` — bloque `classNameSeen` detecta colisiones de `ClassName`
- [x] Tabla con `ClassName` duplicado recibe sufijo del prefijo de tabla: `TB_Serie` + `TR_Serie` → `Serie` + `SerieTr`
- [x] Recalcula: `ClassNamePlural`, `ObjectName`, `ServiceName`, `RepositoryName`, `ControllerName`, `ApiRoute`
- [x] Validado: `Sin duplicados en DI` ✅

### Fix A — Tablas TA_* no generan frontend
- [x] Guard `skipFrontend` en loop principal de `GenerateAsync()` — `TA_`, `TH_`, `TAR_` excluidas de `GenerateFrontendAsync`
- [x] Antes: ~45 carpetas `tm-*`, `tb-*`, `tbr-*` incorrectas en frontend → Ahora: 0 ✅

### Fix B — Doble mayúscula inicial (EXpedientes → Expedientes)
- [x] `NamingConventionService.ToClassName()` — regex `^([A-Z])([A-Z])([a-z])` normaliza error de capitalización en BD
- [x] `TM_EXpedientesAsociados` → `ExpedientesAsociados` · kebab: `expedientes-asociados-tm` ✅

### Feature 1 — Frontend Angular conectado al API real
- [x] `angular_environment.scriban` → `src/environments/environment.ts` con `apiUrl: 'http://localhost:5000'`
- [x] `angular_environment_prod.scriban` → `src/environments/environment.prod.ts` con `apiUrl: '__API_URL__'`
- [x] `angular_error_interceptor.scriban` → `core/interceptors/error.interceptor.ts` — toast PrimeNG por código HTTP (400/401/403/404/409/422/500)
- [x] `angular_auth_interceptor.scriban` → `core/interceptors/auth.interceptor.ts` — inyecta `Bearer token` en cada request
- [x] `angular_app_config.scriban` actualizado → `provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))` + `MessageService` global
- [x] `FrontendConfig` ampliado: `ApiBaseUrl`, `ApiBaseProdUrl` en `ConfigModels.cs` y `TemplateContext`
- [x] Paginación lazy `p-table` conectada a `GetPagedAsync` — ya operativa desde S9 ✅
- [x] Servicios Angular con rutas relativas `/api/...` — funciona con `proxy.conf.json` en dev ✅

### Feature 2 — JWT end-to-end
#### Backend
- [x] `SecurityConfig` ampliado en `ConfigModels.cs`:
  - `JwtSource` — `"DatabaseTable"` | `"Hardcoded"` | `"OIDC"`
  - `UserTable` — tabla de usuarios configurable (default: `"TB_Funcionario"`)
  - `JwtKey`, `JwtIssuer`, `JwtAudience` — parámetros del token
  - `JwtExpiresMinutes` — expiración configurable (default: 60)
  - `RefreshExpiresDays` — duración del refresh token (default: 7)
  - `UseRefreshToken` — activar/desactivar refresh token
  - `OidcProvider` — `"None"` | `"Keycloak"` | `"AzureAD"` | `"Auth0"`
  - `OidcAuthority`, `OidcClientId`, `OidcClientSecret` — config OIDC
- [x] `TemplateContext` expone todas las propiedades JWT al contexto Scriban
- [x] `program.scriban` actualizado:
  - `AddAuthentication(JwtBearerDefaults)` + `AddJwtBearer()` con `TokenValidationParameters` completo
  - `app.UseAuthentication()` antes de `app.UseAuthorization()`
  - Swagger con `AddSecurityDefinition("Bearer")` + `AddSecurityRequirement` → botón Authorize en UI
  - CORS con `AllowCredentials()` para soporte de cookies (refresh token)
- [x] `appsettings.scriban` — sección `"Jwt"` generada: Key, Issuer, Audience, ExpiresMinutes, RefreshExpiresDays
- [x] `auth_controller.scriban` — nuevo `AuthController` generado automáticamente:
  - `POST /api/auth/login` — valida credenciales contra BD, retorna access token + refresh token cookie HttpOnly
  - `POST /api/auth/refresh` — renueva access token usando refresh token cookie
  - `POST /api/auth/logout` — elimina cookie de refresh token
  - `GET /api/auth/me` — retorna claims del usuario autenticado
  - Claims generados: `sub`, `email`, `name`, `preferred_username`, `jti`, `roles[]`
  - Refresh token en cookie HttpOnly + Secure + SameSite=Strict
- [x] `api_csproj.scriban` — paquetes NuGet agregados: `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0` + `System.IdentityModel.Tokens.Jwt 8.0.0`

#### Angular
- [x] `angular_auth_service.scriban` → `core/services/auth.service.ts`:
  - Signals reactivos: `isAuthenticated`, `currentUser`, `token`
  - Métodos: `login()`, `logout()`, `refresh()`, `hasRole(role)`
  - Parseo automático de claims desde JWT (sub, email, name, roles, preferred_username)
  - Sesión persistida en `localStorage`
- [x] `angular_auth_guard.scriban` → `core/guards/auth.guard.ts`:
  - `authGuard` — redirige a `/login` si no autenticado
  - `roleGuard(role)` — redirige a `/forbidden` si no tiene el rol requerido
- [x] `angular_login_component.scriban` → `features/login/login.component.ts`:
  - Formulario PrimeNG: `p-card`, `pInputText`, `p-password` (con toggle mask), `p-button` con loading state
  - Manejo de error con `p-message`
  - Navegación automática al dashboard tras login exitoso

### Extra — Launcher interactivo
- [x] `aigen.ps1` — menú interactivo con selección de BD (SqlServer/PostgreSQL/Ambos) y modo (completo/rápido/solo backend/+API/solo compilar)
- [x] Modo directo: `.\aigen.ps1 -Db Postgres -Mode fast` sin menú

### Resultado final S11
- [x] **2077 archivos generados** ✅
- [x] **0 errores · 0 warnings compilación** ✅
- [x] **0 kebabs rotos** (acrónimos) ✅
- [x] **0 duplicados en DI** ✅
- [x] **0 carpetas audit en frontend** ✅

---

## ⏳ SEMANA 12 — Microservicios + API Gateway
> **Estado: PENDIENTE**

### Feature 4 — Arquitectura de Microservicios
- [ ] `separateSolutionPerService: true` en `aigen.json` — un `.sln` por prefijo de tabla
- [ ] Agrupación automática: `TM_` → `MovementService`, `TP_` → `ParameterService`, etc.
- [ ] `docker-compose.scriban` — orquestación de todos los microservicios generados
- [ ] `Dockerfile.scriban` — imagen por microservicio
- [ ] API Gateway con YARP — `gateway.scriban` con rutas hacia cada microservicio
- [ ] Health checks por servicio — `/health` endpoint en cada API
- [ ] Service discovery básico — variables de entorno por servicio

**Nuevos archivos:** `solution_microservice.scriban`, `docker_compose.scriban`, `dockerfile.scriban`, `gateway.scriban`  
**Esfuerzo estimado:** 5-7 días

---

## ⏳ SEMANA 13 — Stored Procedures y Funciones
> **Estado: PENDIENTE**

### Feature 5 — CRUD vía Stored Procedures
- [ ] `SqlServerSchemaReader` — leer `INFORMATION_SCHEMA.ROUTINES` (SPs y funciones)
- [ ] `StoredProcedureMetadata` — nombre, parámetros IN/OUT, tipo de retorno
- [ ] Nueva opción en `aigen.json`: `crudStrategy: "direct" | "storedProcedures" | "mixed"`
- [ ] `repository_sp.scriban` — repositorio Dapper con llamadas a SPs
- [ ] `sp_crud.scriban` — plantilla T-SQL: `sp_Insert_`, `sp_Update_`, `sp_Delete_`, `sp_GetById_`
- [ ] Opción de descarga de script `.sql` para ejecución manual en la BD
- [ ] CLI: comando `aigen generate-sp-script` — solo genera scripts sin código C#

**Nuevos archivos:** `StoredProcedureMetadata.cs`, `repository_sp.scriban`, `sp_crud.scriban`  
**Esfuerzo estimado:** 4-5 días

---

## ⏳ SEMANA 14 — IA Integrada como Asistente de Desarrollo
> **Estado: PENDIENTE**

### Feature 6 — AIGEN AI Assistant
- [ ] Implementar `Aigen.AI.csproj` con SDK Anthropic .NET
- [ ] System prompt base: conoce convención Línea base de almacenamiento, stack .NET/Angular, plantillas AIGEN
- [ ] Modelos configurables: Claude API (primario), OpenAI GPT-4 (fallback), Ollama local (offline)
- [ ] Comando `aigen ai-assist` — modo conversacional para refinar código generado
- [ ] Comando `aigen ai-review [archivo]` — revisión de calidad del código
- [ ] Comando `aigen ai-explain [entidad]` — explicación en lenguaje natural
- [ ] RAG sobre el código generado: indexar entidades en Qdrant para contexto semántico
- [ ] Capacidad de generar plantillas `.scriban` personalizadas via prompt
- [ ] Historial de conversaciones por proyecto (local)

**Nuevos archivos:** `Aigen.AI.csproj`, `AigenAssistant.cs`, `ai_prompt_base.md`  
**Esfuerzo estimado:** 7-10 días

---

## ⏳ SEMANA 15 — Multi-Frontend + Paleta de Colores + Logo
> **Estado: PENDIENTE**

### Feature 7 — Multi-Frontend
- [ ] `frontend.framework: "angular" | "react" | "vue" | "blazor"` en `aigen.json`
- [ ] `TemplateLocator` — resolver carpeta de frontend según framework configurado
- [ ] **React:** componentes con hooks, Axios, Ant Design — `ListPage.tsx`, `FormPage.tsx`
- [ ] **Vue 3:** Composition API, Axios, PrimeVue — `ListView.vue`, `FormView.vue`
- [ ] **Blazor:** componentes .razor, MudBlazor — `ListPage.razor`, `FormPage.razor`

### Feature 8 — Paleta de Colores Configurable
- [ ] Propiedades en `aigen.json`: `primaryColor`, `secondaryColor`, `accentColor`, `darkMode`
- [ ] Angular: generar `styles.scss` con variables CSS de PrimeNG personalizadas
- [ ] React/Vue: generar `theme.ts` con tokens de diseño
- [ ] `theme.scriban` — inyecta colores en el sistema de diseño del framework
- [ ] Validación de contraste WCAG AA

### Feature 9 — Logo e Imagen de Marca
- [ ] Propiedad `logoPath` en `aigen.json` — ruta a imagen PNG/SVG/JPG/ICO
- [ ] CLI: validar que el archivo existe y es imagen válida al leer el config
- [ ] Copiar imagen a `assets/` del frontend generado
- [ ] Inyectar logo en navbar, sidebar y pantalla de login generados
- [ ] Generar `favicon.ico` automáticamente desde PNG proporcionado
- [ ] Soporte formatos: PNG, SVG, JPG, ICO

**Esfuerzo estimado:** React 3d + Vue 3d + Blazor 3d + Branding 3d = ~10 días

---

## ⏳ SEMANA 16 — Documentación Completa + Empaquetado
> **Estado: PENDIENTE**

### Feature 10 — Documentación Auto-Actualizada (incremental S7-S16)
- [ ] `ARCHITECTURE.md` — diagrama ASCII art de capas + lista de entidades por dominio
- [ ] `DEPLOYMENT.md` — instrucciones por BD (SQL Server, PostgreSQL, Oracle)
- [ ] `CODE_SUMMARY.md` — desglose por prefijo, estadísticas CRUD, cobertura tests
- [ ] `CHANGELOG.md` — comparar generación actual vs anterior (diff de archivos)
- [ ] `api_catalog.scriban` — catálogo de endpoints con ejemplos de request/response
- [ ] `db_dictionary.scriban` — diccionario de datos completo de todas las tablas

### Feature 11 — Documentación Completa Auto-Generada
- [ ] `README.md` completo: instalación, configuración, ejemplos de uso
- [ ] API Reference: Swagger/OpenAPI exportado como Markdown y HTML estático
- [ ] Developer Guide: extensión de plantillas, convenciones, contribución
- [ ] Entity Reference: una página por entidad con campos, validaciones, relaciones
- [ ] Exportación a múltiples formatos: Markdown, HTML estático, PDF (WeasyPrint)
- [ ] Comando CLI: `aigen generate-docs --format pdf|html|md`
- [ ] CHANGELOG automático desde commits Git (Conventional Commits)

### Empaquetado y CI/CD
- [ ] `dotnet tool install -g aigen` — empaquetado como herramienta global .NET
- [ ] GitHub Actions CI/CD: build + test en cada push
- [ ] Pruebas contra 3+ BDs de clientes reales
- [ ] Publicación en NuGet

---

## 🚀 Nuevas Features — Resumen y Roadmap

| # | Feature | Semana | Impacto | Estado |
|---|---------|--------|---------|--------|
| 1 | Multi-schema: selección de schemas de BD | S10 | Alto | ✅ Completado |
| 2 | Soporte PostgreSQL completo | S10 | Alto | ✅ Completado |
| 3 | excludedPrefixes en aigen.json | S10 | Medio | ✅ Completado |
| 4 | Soft-delete via Estado / Eliminado | S10 | Alto | ✅ Completado |
| 5 | Swagger nullables (int? → null) | S10 | Bajo | ✅ Completado |
| 6 | KebabName con acrónimos (MCDT, PERU) | S11 | Bajo | ✅ Completado |
| 7 | Servicios duplicados ClassNamePlural | S11 | Bajo | ✅ Completado |
| 8 | Frontend Angular conectado al API real | S11 | Alto | ✅ Completado |
| 9 | JWT end-to-end (DatabaseTable mode) | S11 | Alto | ✅ Completado |
| 10 | SecurityConfig parametrizable (JWT/OIDC) | S11 | Alto | ✅ Completado |
| 11 | Launcher interactivo aigen.ps1 | S11 | Medio | ✅ Completado |
| 12 | Microservicios + API Gateway (YARP) | S12 | Alto | ⏳ Pendiente |
| 13 | Stored Procedures + script SQL | S13 | Medio | ⏳ Pendiente |
| 14 | IA integrada como asistente | S14 | Alto | ⏳ Pendiente |
| 15 | Multi-frontend: React, Vue, Blazor | S15 | Alto | ⏳ Pendiente |
| 16 | Paleta de colores + Logo | S15 | Medio | ⏳ Pendiente |
| 17 | Docs completa + empaquetado NuGet | S16 | Alto | ⏳ Pendiente |

---

## 🐛 Backlog Técnico

| # | Archivo | Descripción | Impacto | Estado |
|---|---------|-------------|---------|--------|
| 1 | `controller.scriban` | Constraint `{id:string}` inválido | Bajo | ✅ Resuelto S8 |
| 2 | `repository_ef.scriban` | `ToggleEstado` con `EstadoPropertyName` | Bajo | ✅ Resuelto S7 |
| 3 | `aigen.json` | No soporta `excludedPrefixes` | Medio | ✅ Resuelto S10 |
| 4 | `entity.scriban` | FK duplicadas sin `col_seen` | Medio | ✅ Resuelto S9 |
| 5 | `entity_configuration.scriban` | Shadow properties por `HasOne()` | Alto | ✅ Resuelto S9 |
| 6 | `dto.scriban` | `string` no-nullable causa 400 Bad Request | Alto | ✅ Resuelto S9 |
| 7 | `repository_ef.scriban` | Delete físico en tablas con Estado | Alto | ✅ Resuelto S10 |
| 8 | `PostgreSqlSchemaReader` | No usaba `NamingConventionService` | Alto | ✅ Resuelto S10 |
| 9 | `NamingConventionService` | Columnas PostgreSQL lowercase sin separadores | Medio | ✅ Resuelto S10 |
| 10 | `NamingConventionService` | KebabName con acrónimos (MCDT → m-c-d-t) | Bajo | ✅ Resuelto S11 |
| 11 | `FileGeneratorService` | Servicios duplicados cuando `ClassNamePlural` coincide | Bajo | ✅ Resuelto S11 |
| 12 | `FileGeneratorService` | Tablas TA_* generaban frontend incorrectamente | Medio | ✅ Resuelto S11 |
| 13 | `NamingConventionService` | Doble mayúscula inicial (EXpedientes → e-xpedientes) | Bajo | ✅ Resuelto S11 |
| 14 | `program.scriban` | Swagger solo en Development | Bajo | ✅ Resuelto S11 |
| 15 | `api_csproj.scriban` | Faltaban paquetes NuGet JWT | Alto | ✅ Resuelto S11 |

---

## 🔐 Modelo de Autenticación AIGEN — Arquitectura Parametrizable

### Modos soportados (`security.jwtSource` en `aigen.json`)

| Modo | Descripción | Generado |
|------|-------------|----------|
| `DatabaseTable` | JWT propio validado contra tabla de usuarios en BD | AuthController completo + JwtService |
| `Hardcoded` | Credenciales en appsettings — solo para demo/dev | AuthController simplificado |
| `OIDC` | Token externo validado, perfil local en BD | Middleware OIDC + mapeo de claims |

### Claims JWT generados

| Claim | Descripción | Fuente |
|-------|-------------|--------|
| `sub` | ID único del usuario | BD |
| `email` | Correo electrónico | BD |
| `name` | Nombre completo | BD |
| `preferred_username` | Nombre de usuario / login | BD / OIDC |
| `given_name` / `family_name` | Nombre y apellido | BD / OIDC |
| `roles[]` | Array de roles del sistema | BD / OIDC scopes |
| `jti` | ID único del token (para blacklist) | Generado |

### Proveedores OIDC soportados (`security.oidcProvider`)

| Valor | Proveedor | Claims externos mapeados |
|-------|-----------|--------------------------|
| `None` | JWT propio (default) | — |
| `Keycloak` | Keycloak / Red Hat SSO | `preferred_username`, `realm_access.roles` |
| `AzureAD` | Microsoft Entra ID | `upn`, `roles`, `groups` |
| `Auth0` | Auth0 | `nickname`, `https://namespace/roles` |
| `Google` | Google Identity | `email`, `name`, `given_name`, `family_name` |
| `ServiciosCiudadanos` | Servicios Ciudadanos Digitales (Colombia) | `documento`, `tipoDocumento`, identificador único |

### Configuración completa en `aigen.json`

```json
"security": {
  "authenticationType": "JWT",
  "jwtSource": "DatabaseTable",
  "userTable": "TB_Funcionario",
  "jwtKey": "CAMBIAR_EN_PRODUCCION_MIN_32_CARACTERES",
  "jwtIssuer": "MiProyectoAPI",
  "jwtAudience": "MiProyectoClient",
  "jwtExpiresMinutes": 60,
  "refreshExpiresDays": 7,
  "useRefreshToken": true,
  "oidcProvider": "None",
  "oidcAuthority": null,
  "oidcClientId": null,
  "oidcClientSecret": null,
  "corsOrigins": "http://localhost:4200",
  "forceHttps": true,
  "enableCors": true
}
```

### Plantillas generadas por modo de autenticación

| Plantilla | DatabaseTable | Hardcoded | OIDC |
|-----------|:---:|:---:|:---:|
| `auth_controller.scriban` | ✅ | ✅ | ✅ |
| `angular_auth_service.scriban` | ✅ | ✅ | ✅ |
| `angular_auth_guard.scriban` | ✅ | ✅ | ✅ |
| `angular_auth_interceptor.scriban` | ✅ | ✅ | ✅ |
| `angular_login_component.scriban` | ✅ | ✅ | ❌ (redirige al proveedor) |
| `oidc_callback.scriban` | ❌ | ❌ | ✅ |

---

## 🔑 Decisiones Arquitectónicas Tomadas

| Decisión | Justificación |
|----------|---------------|
| Scriban como motor de plantillas | Sintaxis limpia, soporte .NET nativo, sin dependencias pesadas |
| Prefijos de tabla como señales de generación | Estándar Línea base de almacenamiento — el prefijo define qué CRUD se genera |
| `TH_`/`TAR_` → sufijo `Hist` en ClassName | Evita colisiones entre tablas históricas y sus contrapartes activas |
| UpdateRequest con `T?` para todos los value types | Permite updates parciales — cliente solo envía campos que cambia |
| EF Core + Dapper híbrido | EF para CRUD simple, Dapper para queries complejas y stored procedures |
| Clean Architecture por defecto | Separación de responsabilidades, testabilidad, independencia de frameworks |
| `AllTablesScript` para DI auto-registro | Scriban no itera `List<T>` directamente — necesita `List<ScriptObject>` |
| `ISchemaReader` como interfaz | Permite agregar PostgreSQL/Oracle sin cambiar el generador |
| Soft-delete árbol de decisión | `Eliminado` → soft por flag; `Estado` → soft por estado; ninguno → físico |
| `excludedPrefixes` en config | Excluir grupos de tablas por prefijo sin listarlas individualmente |
| `NamingConventionService` en PostgreSqlSchemaReader | Naming consistente entre SqlServer y PostgreSQL — un solo lugar de verdad |
| `NormalizePostgresName()` para lowercase | PostgreSQL guarda nombres en lowercase — normalizar antes del PascalCase |
| Kebab con regex doble (acrónimos) | `([A-Z]+)([A-Z][a-z])` + `([a-z\d])([A-Z])` — maneja PascalCase Y acrónimos correctamente |
| `skipFrontend` guard en FileGeneratorService | Tablas TA_/TH_/TAR_ solo generan Entity en backend — nunca frontend |
| `classNameSeen` para desambiguar colisiones | Detecta colisiones de ClassName antes de generar y añade sufijo de prefijo de tabla |
| JWT parametrizable por `jwtSource` | `DatabaseTable` / `Hardcoded` / `OIDC` — sin cambiar el generador, solo config |
| Refresh token en cookie HttpOnly | Más seguro que localStorage — no accesible desde JavaScript |
| Claims JWT con `preferred_username` | Compatibilidad con OIDC estándar (OpenID Connect Core 1.0) |
| `SecurityConfig` con `OidcProvider` | Soporta Keycloak, AzureAD, Auth0, Google, Servicios Ciudadanos Digitales |
| Angular Signals para estado de auth | Reactividad moderna sin RxJS/BehaviorSubject — patrón Angular 17+ |
| `authGuard` + `roleGuard(role)` separados | Flexibilidad: proteger por autenticación O por rol específico |
| IA con modelo base configurable | Claude primario, OpenAI fallback, Ollama offline — sin vendor lock-in |
| RAG con Qdrant para contexto de IA | Semántica sobre el código generado para respuestas precisas del asistente |
| Microservicios agrupados por prefijo | TM_ → un dominio, TP_ → otro dominio — mapeo natural del estándar Línea base de almacenamiento |
