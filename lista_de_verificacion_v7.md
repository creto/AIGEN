# 🗺️ AIGEN — Derrotero de Desarrollo v7
**Proyecto:** AI-Powered Software Generator (AIGEN)  
**Stack:** .NET Core 8 · Scriban · EF Core · Dapper · Angular 18 · PrimeNG  
**BD Objetivo:** SQL Server · PostgreSQL · Oracle (roadmap)  
**Convención BD:** Línea base de almacenamiento (TM_ / TB_ / TBR_ / TP_ / TR_ / TA_ / TS_ / TH_)  
**Repositorio:** github.com/creto/AIGEN  
**Última actualización:** Marzo 2026 — Semanas 1–14 completadas (con deuda técnica identificada en S15)

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
Semana 12 ████████████████████  85% ✅  Microservicios + API Gateway YARP (deuda: health endpoints)
Semana 13 ████████████████░░░░  40% ⚠️  Auditoría básica + Caché (deuda técnica importante — ver S15)
Semana 14 ██████████████░░░░░░  65% ⚠️  Stored Procedures CRUD (deuda: mixed, PostgreSQL SP, sp_ToggleEstado)
Semana 15 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Menús dinámicos + UX + Deuda técnica S12/S13/S14
Semana 16 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Notificaciones + Mensajería + SignalR
Semana 17 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Dockerización completa + Nginx
Semana 18 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Testing completo (Unit + Integration + E2E + Load)
Semana 19 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Seguridad + OWASP + Headers + Rate Limiting
Semana 20 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  CI/CD completo (GitHub Actions + SonarCloud + NuGet)
Semana 21 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  IA: AI Assistant + Agente Autónomo + MCP Server
Semana 22 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Multi-frontend + Temas visuales + Logo
Semana 23 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Documentación completa + empaquetado NuGet
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
- [x] `Doc4Us.Infrastructure` — 1075 errores identificados (punto de partida Semana 6)
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

- [x] `program.scriban` — DI completo: DbContext, repositorios, servicios, Swagger, CORS, Serilog
- [x] `DependenciasController` generado → `GET /api/dependencias` responde HTTP 200
- [x] Swagger UI activo en `http://localhost:5000/swagger`
- [x] Auto-registro de repositorios y servicios en DI (sin lista manual)
- [x] Fix CS0311 — constraint `where TEntity : class` en `IRepository<T>`
- [x] Fix CS8604 — `string?` → `string` en `GetRequiredService<T>()`
- [x] **Milestone:** API levantada, endpoints funcionando contra BD real ✅

---

## ✅ SEMANA 9 — Endpoints CRUD completos + validación BD real
> **Estado: COMPLETADA ✅**

- [x] `GET /api/dependencias?page=1&pageSize=20` → 268 registros reales de Doc4UsAIGen
- [x] `POST /api/dependencias` → crea registro y retorna 201 Created
- [x] `PUT /api/dependencias/{id}` → actualiza registro existente
- [x] `DELETE /api/dependencias/{id}` → soft-delete (Estado=0) + validación 404
- [x] `PATCH /api/dependencias/{id}/toggle-estado` → activa/desactiva
- [x] Fix shadow properties — `entity_configuration.scriban` genera solo columnas explícitas
- [x] Fix string nullable — `[Required]` solo cuando `is_nullable = false AND type != string`
- [x] Fix FK deduplicación — `col_seen` HashSet en `entity.scriban`
- [x] Fix composite index syntax — `HasIndex(e => new { e.Col1, e.Col2 })`
- [x] **Validado en Swagger con datos reales de Doc4UsAIGen ✅**

---

## ✅ SEMANA 10 — Soft-delete + PostgreSQL + excludedPrefixes
> **Estado: COMPLETADA ✅**

- [x] Árbol de decisión soft-delete: `Eliminado` → `Eliminado=true`; `Estado` → `Estado=false`; ninguno → delete físico
- [x] `BaseQuery` auto-filtra `Estado == true` en todas las queries
- [x] Update restringido a registros activos (`Estado == true`)
- [x] `excludedPrefixes` en `aigen.json` — filtrar `TH_`, `TA_`, etc.
- [x] `PostgreSqlSchemaReader` — soporte completo con `NormalizePostgresName()`
- [x] Fix columnas PostgreSQL lowercase sin separadores
- [x] Fix `RawIdx`/`RawFK` record corruption en `SqlServerSchemaReader`
- [x] `ForeignKeyMetadata` — tipos `LocalFkCSharpType` / `ReferencedPkCSharpType`
- [x] **BD PostgreSQL `aigen_test` — genera y compila sin errores ✅**

---

## ✅ SEMANA 11 — Frontend Angular + JWT end-to-end + Naming fixes
> **Estado: COMPLETADA ✅**

### Naming fixes
- [x] KebabName con acrónimos (MCDT → mcdt, no m-c-d-t) — doble regex en `ToAngularFileName()`
- [x] Doble mayúscula inicial (EXpedientes → Expedientes) — regex `^([A-Z])([A-Z])([a-z])`
- [x] Servicios duplicados cuando `ClassNamePlural` coincide — HashSet `classNameSeen`
- [x] Guard `skipFrontend` para TA_/TH_/TAR_ — no generan módulos Angular

### Frontend Angular conectado al API real
- [x] `angular_environment.scriban` → `environments/environment.ts` con `apiUrl`
- [x] `angular_error_interceptor.scriban` → toast PrimeNG por código HTTP
- [x] `angular_auth_interceptor.scriban` → Bearer token automático en cabeceras
- [x] `angular_app_config.scriban` → `withInterceptors([authInterceptor, errorInterceptor])`
- [x] `FrontendConfig` ampliado: `ApiBaseUrl`, `ApiBaseProdUrl`

### JWT end-to-end
- [x] `SecurityConfig` ampliado: `JwtSource`, `UserTable`, `JwtKey`, `JwtIssuer`, `JwtAudience`, `JwtExpiresMinutes`, `RefreshExpiresDays`, `UseRefreshToken`, `OidcProvider`, `OidcAuthority/ClientId/Secret`
- [x] `program.scriban`: `AddAuthentication().AddJwtBearer()` + `UseAuthentication()` + Swagger Bearer button
- [x] `appsettings.scriban`: sección `"Jwt"` completa con key configurable
- [x] `auth_controller.scriban`: `POST /api/auth/login` · `POST /api/auth/refresh` · `POST /api/auth/logout` · `GET /api/auth/me`
- [x] `api_csproj.scriban`: `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0` + `System.IdentityModel.Tokens.Jwt 8.0.0`
- [x] Angular: `angular_auth_service.scriban` (signals), `angular_auth_guard.scriban` (authGuard + roleGuard), `angular_login_component.scriban` (PrimeNG)
- [x] `aigen.ps1` v1 — launcher interactivo con menú BD × modo
- [x] `rebuild_and_generate.ps1` v2 — perfiles SqlServer/Postgres
- [x] **Resultado: 2080 archivos · 0 errores · 0 warnings ✅**

---

## ✅ SEMANA 12 — Microservicios + API Gateway YARP
> **Estado: COMPLETADA AL 85% ✅** — deuda técnica identificada

### Implementado ✅
- [x] `ArchitectureConfig` ampliado: `SeparateSolutionPerService`, `TablePrefixGrouping`
- [x] `FileGeneratorService.GenerateMicroservicesAsync()` — agrupa tablas por prefijo, genera `.sln` por microservicio
- [x] `solution_microservice.scriban` — `.sln` por microservicio
- [x] `dockerfile.scriban` — multistage build, usuario non-root
- [x] `docker_compose.scriban` — orquestación 4 servicios + Gateway + BD SQL Server
- [x] `gateway_program.scriban` — Program.cs del Gateway YARP
- [x] `gateway_yarp.scriban` — rutas + health checks en YARP config
- [x] `gateway_csproj.scriban` — YARP 2.1.0
- [x] **Filtro FKs externas** — `entity.scriban` omite navigation properties hacia entidades de otros microservicios *(extra vs plan)*
- [x] `ScribanTemplateEngine` — `is_microservice_mode` + `microservice_class_names_array` como ScriptObject nativo *(extra vs plan)*
- [x] `Banner.cs` — `Markup.Escape()` en todos los mensajes (fix Spectre.Console)
- [x] `rebuild_and_generate.ps1` v3 — perfil `Microservices` + compilación multi-sln
- [x] `aigen_microservices.json` — config con 4 dominios (Basic/Document/Parameter/Relational)
- [x] **BasicService / DocumentService / ParameterService / RelationalService — 0 errores 0 warnings ✅**
- [x] **Gateway YARP — compila ✅**
- [x] **3225 archivos generados ✅**
- [x] **Monolito SqlServer: sin regresión ✅**

### Deuda técnica S12 → completar en S15
- [ ] Health endpoints `/health` y `/ready` dedicados generados en cada microservicio (actualmente solo en docker-compose)
- [ ] `healthcheck.scriban` — endpoint dedicado por microservicio
- [ ] Service discovery completo — variables de entorno configurables por servicio desde `aigen_microservices.json`

---

## ⚠️ SEMANA 13 — Auditoría de negocio + Caché
> **Estado: COMPLETADA AL 40% ⚠️** — deuda técnica importante

### Implementado ✅
- [x] `AuditConfig` en `ConfigModels.cs`: `Provider` (EFInterceptor|StoredProcedure|Both), `GlobalEnabled`, `AuditTable`, `Tables` dict
- [x] `GeneratorConfig.Audit` — propiedad agregada
- [x] `audit_interceptor.scriban`:
  - `ICurrentUserContext` — interfaz limpia sin dependencia HttpContext en Infrastructure
  - `SystemUserContext` — implementación default ("system")
  - `AuditSaveChangesInterceptor` — captura Added/Modified/Deleted campo por campo
  - Escribe en `TA_AuditoriaTransacciones` via `ExecuteSqlRawAsync`
- [x] `cache_service.scriban`:
  - `ICacheService` — abstracción genérica
  - `MemoryCacheService` — IMemoryCache con tracking de keys para RemoveByPrefix
  - `RedisCacheService` — IDistributedCache + JSON serialization
- [x] `infrastructure_csproj.scriban` — paquetes Redis condicionales (`StackExchangeRedis`)
- [x] `program.scriban` — `AddInterceptors()` + `ICurrentUserContext` + Cache DI condicional
- [x] `FileGeneratorService` — genera audit + cache según config
- [x] `aigen.json` — sección `"audit"` completa con provider, auditTable, tables
- [x] **TemplateContext:** `AuditEnabled`, `UseEfInterceptor`, `UseAuditSp`, `UseCache`, `UseRedis`, `UseMemoryCache`
- [x] **2765 archivos · 0 errores · API validada: 268 registros ✅**

### Deuda técnica S13 → completar en S15

#### Auditoría granular — pendiente
- [ ] Filtro por campos configurados en `auditing.tables` — actualmente audita todos los campos sin distinción
- [ ] `ICurrentUserContext` HTTP — implementación real que lea claims JWT del `HttpContext` (actualmente `SystemUserContext` retorna "system")
- [ ] **Provider `StoredProcedure`** — `sp_audit.scriban` — SP `sp_Audit_{TableName}` para compatibilidad con generador antiguo
- [ ] **Provider `Both`** — EF Interceptor + SP simultáneo para migración gradual
- [ ] `audit_table.scriban` — script SQL para crear `TA_AuditoriaTransacciones` si no existe
- [ ] `audit_query_service.scriban` — servicios de consulta:
  - `GetAuditTrailAsync(tableName, recordId)` — historial completo de un registro
  - `GetAuditByUserAsync(userId, dateRange)` — auditoría por usuario
  - `GetAuditByDateAsync(dateRange)` — auditoría por período
- [ ] `audit_controller.scriban` — endpoints REST de consulta de auditoría (solo rol Admin)
- [ ] Angular: `audit-trail.component.scriban` — timeline visual PrimeNG `p-timeline`

#### Logging enriquecido — pendiente
- [ ] `logging_config.scriban` — Serilog sinks: File rotación diaria + Seq opcional + ApplicationInsights opcional
- [ ] Enriquecer logs con: `UserId`, `RequestId`, `Environment`, `MachineName`
- [ ] Logging de eventos de autenticación: login · logout · token refresh · acceso denegado
- [ ] Logging de operaciones CRUD: entidad + ID + usuario + duración

#### Caché granular — pendiente
- [ ] `cacheByTable` en `aigen.json` — expiry por tabla individual
- [ ] `cache_decorator.scriban` — decorator pattern sobre repositorios:
  - `GetByIdAsync` → cache hit/miss con key `{Entity}:{id}`
  - `GetPagedAsync` — cache de primera página de tablas TP_* por 1h
  - Invalidación automática en `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- [ ] `cache_health.scriban` — healthcheck de Redis en `/ready` endpoint
- [ ] Cache de menús por rol — `GetMenusByRoleAsync` cacheado (integrar con S15)

---

## ⚠️ SEMANA 14 — Stored Procedures + CRUD vía SP
> **Estado: COMPLETADA AL 65% ⚠️** — deuda técnica identificada

### Análisis del generador histórico (2014-2026) ✅
- [x] Análisis completo de SPs generados por `PA_DBA_GeneraCRUDTablaMVC` / `PA_DBA_RecreaCRUDTablaMVC`
- [x] Patrones históricos TiGlobal SAS identificados y modernizados:
  - Schema `[API]` para SPs (separación de responsabilidades en BD)
  - Prefijo `PA_TablaOperacion` — configurable vía `SpPrefix`
  - `@auditoria VarChar(MAX)` como parámetro en cada SP
  - `[dbo].[ValidarDependencias]` antes de Delete
  - GetAll sin BLOBs + GetAllFull con todos los campos
  - `Estado=1` parametrizable con default en Insert
  - `and Estado=1` en WHERE del Update
- [x] Mejoras sobre el generador histórico:
  - `SCOPE_IDENTITY()` en lugar de `@@identity` (thread-safe)
  - Paginación `OFFSET/FETCH` en lugar de `@pag*@TamPag+1`
  - `sp_executesql` en GetByFilter para sargability de índices
  - Templates en archivos Git vs tabla `API.paramGen` en BD

### Implementado ✅
- [x] `StoredProcedureMetadata.cs` — `SpParameter`, `SpType`, `SpCrudType` enums
- [x] `BackendConfig` ampliado: `CrudStrategy` (`direct`|`storedProcedures`|`mixed`) + `SpPrefix` + `SpSchema`
- [x] `SqlServerSchemaReader.ReadStoredProceduresAsync()` — lee SPs via `INFORMATION_SCHEMA.ROUTINES` + parámetros con SqlCommand nativo
- [x] `DetectCrudType()`, `ExtractTableFromSpName()`, `SqlTypeToCSharp()` helpers
- [x] **`sp_crud.scriban`** — 7 SPs por tabla con estándar TiGlobal modernizado:
  - `PA_TablaAdd` — INSERT con `@Estado bit = 1`, retorna registro con `SCOPE_IDENTITY()`
  - `PA_TablaUpdate` — UPDATE parámetros opcionales `ISNULL` + `and Estado=1`
  - `PA_TablaDelete` — soft-delete condicional + `ValidarDependencias` + físico fallback
  - `PA_TablaGetAll` — SELECT sin BLOBs, activos, orden DESC
  - `PA_TablaGetAllFull` — SELECT * incluyendo BLOBs
  - `PA_TablaGetById` — SELECT por PK
  - `PA_TablaGetByFilter` — `sp_executesql` + `OFFSET/FETCH` + filtros opcionales por tipo
- [x] `repository_sp.scriban` — repositorio Dapper via `CommandType.StoredProcedure`
- [x] `TemplateContext`: `CrudStrategy`, `UseStoredProcs`, `SpPrefix`, `SpSchema`, `HasAuditoriaColumn`, `is_blob`
- [x] `ForeignKeysScript` — ScriptObject explícito en `TemplateContext` (sin depender del renamer automático)
- [x] `FileGeneratorService` — bifurcación `crudStrategy`: SP → `repository_sp` + `sp_crud`; EF → `repository_ef`; Dapper → `repository_dapper`
- [x] `GenerateCommand` — `--no-interactive|-y` flag — usa JSON sin menús interactivos
- [x] `aigen_sp.json` — config de prueba con `spPrefix=PA_`, `spSchema=API`, `crudStrategy=storedProcedures`
- [x] `rebuild_and_generate.ps1` v4 — perfil `SP`
- [x] `aigen.ps1` v2 — menú con opciones SP y Microservices + modos `backend`/`compile`
- [x] **DependenciaRepository usa Dapper + CommandType.StoredProcedure ✅**
- [x] **1750 archivos generados modo SP · 0 errores ✅**

### Deuda técnica S14 → completar en S15
- [ ] **`mixed` strategy** — SPs solo en tablas marcadas con `"useSP": true` en `aigen.json` (flag existe, implementación pendiente en `FileGeneratorService`)
- [ ] **`sp_ToggleEstado_{TableName}`** — SP de activar/desactivar faltante en `sp_crud.scriban`
- [ ] **`sp_crud_postgres.scriban`** — PL/pgSQL equivalente para PostgreSQL
- [ ] **`PostgreSqlSchemaReader.ReadStoredProceduresAsync()`** — leer funciones PL/pgSQL
- [ ] **`aigen generate-sp-script`** — subcomando CLI para generar solo el `.sql` sin código C#
- [ ] Script SQL ordenado por dependencias (TP_ primero, TB_, luego TM_)

---

## ⏳ SEMANA 15 — Menús dinámicos + UX + Deuda técnica S12/S13/S14
> **Estado: PENDIENTE ⏳** — semana combinada: features nuevos + completar deuda

### Feature principal — Menús dinámicos por roles
- [ ] `TB_Menu` / `TB_MenuItem` — entidades de menú generadas o seeded
- [ ] `MenuSeed.sql` auto-generado — un ítem por controller AIGEN
- [ ] `GET /api/menu` — endpoint que retorna árbol de menú filtrado por rol del JWT
- [ ] Angular: `MenuService` consume `/api/menu` al login
- [ ] Angular: `AppComponent` renderiza menú dinámico desde API (no hardcodeado en routes)
- [ ] `authGuard` + `roleGuard` + `permissionGuard` — granularidad completa
- [ ] PrimeNG `p-menubar` / `p-panelmenu` con árbol jerárquico
- [ ] `[Authorize(Roles = "...")]` en controllers generados
- [ ] Cache de menús por rol — `GetMenusByRoleAsync` (integrar con deuda S13)

### Deuda técnica S12 → completar
- [ ] Health endpoints `/health` y `/ready` dedicados en cada microservicio
- [ ] `healthcheck.scriban` — endpoint por microservicio (no solo en docker-compose)
- [ ] Service discovery — variables de entorno configurables por servicio

### Deuda técnica S13 — Auditoría completa
- [ ] `ICurrentUserContext` HTTP — leer claims JWT del `HttpContext` real
- [ ] Filtro de auditoría por campos configurados en `auditing.tables`
- [ ] `sp_audit.scriban` — Provider `StoredProcedure`
- [ ] Provider `Both` — EF Interceptor + SP simultáneo
- [ ] `audit_table.scriban` — script SQL `CREATE TABLE TA_AuditoriaTransacciones`
- [ ] `audit_query_service.scriban` — `GetAuditTrailAsync`, `GetAuditByUserAsync`, `GetAuditByDateAsync`
- [ ] `audit_controller.scriban` — endpoints REST de auditoría (solo Admin)
- [ ] Angular: `audit-trail.component.scriban` — timeline PrimeNG `p-timeline`
- [ ] Logging enriquecido: Serilog sinks File/Seq/AppInsights + UserId/RequestId/Environment
- [ ] Logging CRUD: entidad + ID + usuario + duración
- [ ] Logging autenticación: login/logout/token refresh/acceso denegado
- [ ] `cache_decorator.scriban` — decorator sobre repositorios con invalidación automática
- [ ] `cache_health.scriban` — healthcheck Redis en `/ready`
- [ ] `cacheByTable` granular en `aigen.json`

### Deuda técnica S14 — SP completo
- [ ] `mixed` strategy — SPs en tablas marcadas individualmente
- [ ] `sp_ToggleEstado_{TableName}` en `sp_crud.scriban`
- [ ] `sp_crud_postgres.scriban` — PL/pgSQL
- [ ] `PostgreSqlSchemaReader.ReadStoredProceduresAsync()`
- [ ] `aigen generate-sp-script` — subcomando CLI
- [ ] Script SQL ordenado por dependencias (TP_ → TB_ → TM_)

---

## ⏳ SEMANA 16 — Notificaciones + Mensajería + SignalR
> **Estado: PENDIENTE ⏳**

- [ ] `signalr_hub.scriban` — Hub de notificaciones generado por proyecto
- [ ] `angular_signalr_service.scriban` — cliente Angular SignalR
- [ ] Toast notifications PrimeNG conectadas a SignalR
- [ ] `program.scriban` — registro de SignalR Hub en DI y pipeline
- [ ] `angular_app_config.scriban` — registro de SignalR (fix deuda S11)
- [ ] MassTransit — abstracción sobre RabbitMQ/Azure Service Bus
- [ ] `message_consumer.scriban` — consumer base generado
- [ ] `SecurityConfig.TwoFactor` — implementación 2FA: TOTP / SMS / Email

---

## ⏳ SEMANA 17 — Dockerización completa + Nginx
> **Estado: PENDIENTE ⏳**

- [ ] `dockerfile.scriban` refinado — health checks `/health` `/ready` en ENTRYPOINT
- [ ] `docker_compose.scriban` refinado — Nginx reverse proxy + SSL termination
- [ ] `nginx.conf.scriban` — upstream hacia API con rate limiting básico
- [ ] Variables de entorno en lugar de `appsettings.json` en producción
- [ ] Docker secrets para connection strings y JWT keys
- [ ] `docker_compose_override.scriban` — configuración local de desarrollo
- [ ] `.dockerignore` generado por proyecto

---

## ⏳ SEMANA 18 — Testing completo
> **Estado: PENDIENTE ⏳**

- [ ] Tests unitarios: `NamingConventionService`, `TableMetadataExtensions`, `TemplateContext`, validadores
- [ ] Tests de integración: generación end-to-end contra BD real con TestContainers
- [ ] Tests de repositorios: EF Core + Dapper contra BD en Docker
- [ ] Tests E2E: Playwright — login, CRUD, paginación, toggle-estado
- [ ] Load testing: k6 — endpoints paginados, p95 < 200ms objetivo
- [ ] `rebuild_and_generate.ps1` — paso `-RunTests` completo habilitado
- [ ] Coverage mínimo: 80% en Core y Templates

---

## ⏳ SEMANA 19 — Seguridad + OWASP + Headers + Rate Limiting
> **Estado: PENDIENTE ⏳**

- [ ] Rate limiting .NET 8 built-in — `/api/auth/login` máx 5 req/min por IP
- [ ] CORS restrictivo — solo orígenes configurados en `security.corsOrigins`
- [ ] Security headers: CSP, HSTS, X-Frame-Options, X-Content-Type-Options
- [ ] HTTPS redirect middleware
- [ ] Semgrep SAST — reglas OWASP .NET + Angular
- [ ] `dotnet audit` + `npm audit` automatizados
- [ ] OWASP ZAP DAST básico en CI
- [ ] FluentValidation en todos los DTOs Create/Update
- [ ] Sanitización outputs: nunca retornar campos de auditoría/password en DTOs

---

## ⏳ SEMANA 20 — CI/CD completo
> **Estado: PENDIENTE ⏳**

- [ ] GitHub Actions: build → test → generate → compile → push NuGet
- [ ] SonarCloud — quality gate en PR (coverage + issues)
- [ ] gitleaks — secrets scanning en cada commit
- [ ] Dependabot — actualizaciones automáticas de dependencias
- [ ] OWASP Dependency-Check en CI
- [ ] Badge de estado en README (build, coverage, quality gate)
- [ ] Artefactos de generación como parte del pipeline

---

## ⏳ SEMANA 21 — IA: AI Assistant + Agente Autónomo + MCP Server
> **Estado: PENDIENTE ⏳**

- [ ] `IA Assistant` — enriquece entidades con descripciones y ejemplos usando LLM
- [ ] `Agente Autónomo` — genera, compila, detecta errores, corrige en loop con confirmación humana
- [ ] `MCP Server AIGEN` — expone AIGEN como herramienta para Claude Desktop/Cursor/VS Code
- [ ] RAG con Qdrant — semántica sobre código generado para respuestas precisas
- [ ] OWASP LLM Top 10 guardrails implementados
- [ ] Cache de respuestas IA — mismo prompt → misma respuesta sin llamar al modelo (integrar con S13 cache)
- [ ] Modelo base configurable: Claude primario · OpenAI fallback · Ollama offline

---

## ⏳ SEMANA 22 — Multi-frontend + Temas visuales
> **Estado: PENDIENTE ⏳**

- [ ] Soporte React (Vite + shadcn/ui)
- [ ] Soporte Vue 3 (Vite + PrimeVue)
- [ ] Sistema de temas PrimeNG — variables CSS por proyecto
- [ ] Logo AIGEN + splash screen CLI
- [ ] Dark/Light mode generado por defecto
- [ ] `FrontendConfig.Framework` — `Angular` | `React` | `Vue`

---

## ⏳ SEMANA 23 — Documentación completa + empaquetado NuGet
> **Estado: PENDIENTE ⏳**

- [ ] README.md completo con ejemplos y badges
- [ ] Wiki GitHub — arquitectura, configuración, plantillas personalizadas
- [ ] `aigen.nupkg` — AIGEN como herramienta global dotnet
- [ ] `dotnet tool install -g aigen` funcional
- [ ] Video demo (5 min: schema → API funcionando)
- [ ] Guía de migración desde generador histórico `PA_DBA_GeneraCRUDTablaMVC`

---

## 🗂️ Estrategias de generación CRUD en AIGEN

### Tabla de estrategias

| `crudStrategy` | `orm` | Repository generado | SPs generados | Cuándo usar |
|---|---|---|---|---|
| `"direct"` | `EFCoreWithDapper` | `repository_ef.scriban` | ❌ No | **Default** — tablas simples, prototipado rápido |
| `"direct"` | `Dapper` | `repository_dapper.scriban` | ❌ No | Performance con queries manuales |
| `"direct"` | `EntityFrameworkCore` | `repository_ef.scriban` | ❌ No | EF puro con migrations |
| `"storedProcedures"` | cualquiera | `repository_sp.scriban` | ✅ Sí (`sp_crud.scriban`) | BDs corporativas con políticas de acceso, estándar TiGlobal |
| `"mixed"` ⚠️ pendiente | `EFCoreWithDapper` | `repository_ef.scriban` | ✅ Sí (tablas marcadas) | Híbrido — EF CRUD simple, SPs operaciones complejas |

### Performance por estrategia

| Estrategia | Performance GET | Performance Write | Mantenimiento | Ideal para |
|---|---|---|---|---|
| EF Core solo | ⭐⭐⭐ Bueno | ⭐⭐⭐ Bueno | ⭐⭐⭐⭐⭐ Muy fácil | Proyectos nuevos, prototipado |
| Dapper directo | ⭐⭐⭐⭐ Muy bueno | ⭐⭐⭐⭐ Muy bueno | ⭐⭐⭐ Medio | APIs de alta carga, queries complejas |
| SPs + Dapper | ⭐⭐⭐⭐⭐ Excelente | ⭐⭐⭐⭐⭐ Excelente | ⭐⭐ Requiere DBA | BDs corporativas, estándar histórico TiGlobal |
| EF + SPs mixed | ⭐⭐⭐⭐ Muy bueno | ⭐⭐⭐⭐ Muy bueno | ⭐⭐⭐ Medio | Migración gradual de SPs a EF Core |

### Generación SP por tipo de tabla

| Prefijo | TableType | HasFullCrud | IsReadOnly | SPs generados |
|---|---|---|---|---|
| `TM_` | Movement | ✅ | ❌ | ✅ Add/Update/Delete/GetAll/GetAllFull/GetById/GetByFilter |
| `TB_` | Basic | ✅ | ❌ | ✅ |
| `TBR_` | BasicRelated | ✅ | ❌ | ✅ |
| `TP_` | Parameter | ✅ | ❌ | ✅ |
| `TR_` | Relational | ❌ | ❌ | ⚠️ Solo Insert/Delete/Select |
| `TC_` | Composition | ✅ | ❌ | ✅ |
| `TS_` | System | ❌ | ✅ | ❌ Solo entidad |
| `TA_` | Audit | ❌ | ✅ | ❌ Solo entidad |
| `TH_` | Historical | ❌ | ✅ | ❌ Solo entidad |

### Flujo de ejecución por estrategia

```
Controller → Service → IRepository (contrato — no sabe nada de SPs ni EF)
                            ↓
                    Repository (Infrastructure)
                            ↓
             ┌─── direct ──────────→ EF Core / Dapper SQL directo
             │                              ↓
             │                      SELECT * FROM tabla
             │
             └─── storedProcedures ──→ Dapper + CommandType.StoredProcedure
                                              ↓
                                    [API].[PA_TablaAdd]
                                    [API].[PA_TablaUpdate]
                                    [API].[PA_TablaDelete]
                                    [API].[PA_TablaGetAll]
                                    [API].[PA_TablaGetAllFull]
                                    [API].[PA_TablaGetById]
                                    [API].[PA_TablaGetByFilter]
```

---

## 🏗️ Arquitectura de archivos generados

### Modo Monolito (`crudStrategy: direct`)
```
Generated/
├── Doc4Us.sln
├── src/
│   ├── Doc4Us.Domain/
│   │   └── Entities/              ← 265 entidades C#
│   ├── Doc4Us.Application/
│   │   ├── {Entidad}s/DTOs/       ← Dto, CreateRequest, UpdateRequest, PagedResponse
│   │   └── Interfaces/            ← IRepository por entidad
│   ├── Doc4Us.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Repositories/      ← EF Core + Dapper híbrido
│   │   │   ├── Configurations/    ← Fluent API por entidad
│   │   │   └── Interceptors/      ← AuditSaveChangesInterceptor
│   │   └── Cache/                 ← ICacheService, Memory/Redis
│   └── Doc4Us.API/
│       ├── Controllers/           ← REST CRUD por entidad
│       ├── Program.cs             ← DI, Swagger, JWT, CORS, Serilog, Cache, Audit
│       └── appsettings.json       ← ConnectionString, Jwt, Logging
└── frontend/
    └── src/app/features/          ← Módulos Angular por entidad
```

### Modo SP (`crudStrategy: storedProcedures`)
```
GeneratedSP/
├── Doc4Us.sln
├── src/
│   └── Doc4Us.Infrastructure/
│       └── Persistence/
│           └── Repositories/      ← Dapper + CommandType.StoredProcedure
└── sql/
    └── sp_{Tabla}.sql             ← 7 SPs por tabla
```

### Modo Microservicios
```
GeneratedMicroservices/
├── BasicService/                  ← TB_ tables (0 errores ✅)
├── DocumentService/               ← TM_ document tables (0 errores ✅)
├── ParameterService/              ← TP_ tables (0 errores ✅)
├── RelationalService/             ← TBR_/TR_ tables (0 errores ✅)
└── Gateway/                       ← YARP API Gateway (0 errores ✅)
    ├── Program.cs
    └── yarp.json
```

---

## 🔧 Scripts y configuración

### rebuild_and_generate.ps1 v4 — Perfiles y flags

```powershell
.\rebuild_and_generate.ps1                                    # SqlServer monolito completo
.\rebuild_and_generate.ps1 -Db Postgres                       # PostgreSQL completo
.\rebuild_and_generate.ps1 -Db Microservices                  # 4 microservicios + Gateway
.\rebuild_and_generate.ps1 -Db SP                             # Stored Procedures
.\rebuild_and_generate.ps1 -SkipTests                         # Sin tests
.\rebuild_and_generate.ps1 -SkipFrontend                      # Sin Angular
.\rebuild_and_generate.ps1 -SkipGenerate                      # Solo compilar sin regenerar
.\rebuild_and_generate.ps1 -RunApi                            # Levanta API al finalizar
.\rebuild_and_generate.ps1 -SkipTests -SkipFrontend -RunApi   # Ciclo rápido + API
```

### aigen.ps1 v2 — Modos directos disponibles

```powershell
.\aigen.ps1                                   # Menú interactivo (default)
.\aigen.ps1 -Db SqlServer  -Mode fast         # SqlServer rápido (sin tests)
.\aigen.ps1 -Db Postgres   -Mode nofront      # PostgreSQL sin frontend
.\aigen.ps1 -Db SqlServer  -Mode api          # SqlServer + RunApi
.\aigen.ps1 -Db SP         -Mode backend      # Stored Procedures solo backend
.\aigen.ps1 -Db Microservices -Mode backend   # Microservicios solo backend
.\aigen.ps1 -Db SqlServer  -Mode compile      # Solo compilar sin regenerar
```

### CLI AIGEN — Flags disponibles

```powershell
aigen generate --config aigen.json                  # Interactivo completo
aigen generate --config aigen.json --no-interactive # Usa JSON sin preguntas (CI/CD)
aigen generate --config aigen.json --dry-run        # Muestra sin escribir
aigen generate --config aigen.json --verbose        # Errores detallados
aigen generate --config aigen.json -y               # Alias de --no-interactive
```

### Rutas clave del proyecto

| Recurso | Ruta |
|---|---|
| AIGEN source | `C:\DevOps\AIGEN\AIGEN\aigen\` |
| Templates Backend | `...\aigen\src\Aigen.Templates\Templates\Backend\` |
| Templates Solution | `...\aigen\src\Aigen.Templates\Templates\Solution\` |
| Generated monolito | `C:\DevOps\AIGEN\AIGEN\Generated\` |
| Generated microservicios | `C:\DevOps\AIGEN\AIGEN\GeneratedMicroservices\` |
| Generated SP | `C:\DevOps\AIGEN\AIGEN\GeneratedSP\` |
| Config SqlServer | `C:\DevOps\AIGEN\AIGEN\Generated\aigen.json` |
| Config Postgres | `...\aigen\configs\aigen_postgres.json` |
| Config Microservicios | `...\aigen\configs\aigen_microservices.json` |
| Config SP | `...\aigen\configs\aigen_sp.json` |

---

## 🐛 Registro de bugs resueltos y pendientes (acumulado S1-S14)

| # | Archivo/Componente | Descripción | Impacto | Estado |
|---|---|---|---|---|
| 1 | `repository_ef.scriban` | `.Value` en tipos NOT NULL → CS1061 | Crítico | ✅ S6 |
| 2 | `dto.scriban` | `bool`/`int` no-nullable en UpdateRequest | Alto | ✅ S6 |
| 3 | `NamingConventionService` | Colisión ClassName TH_Serie / TB_Serie | Alto | ✅ S6 |
| 4 | `entity_configuration.scriban` | PK compuesta con column names SQL | Alto | ✅ S6 |
| 5 | `entity_configuration.scriban` | Shadow properties por `HasOne()` | Alto | ✅ S9 |
| 6 | `dto.scriban` | `string` no-nullable causa 400 Bad Request | Alto | ✅ S9 |
| 7 | `repository_ef.scriban` | Delete físico en tablas con Estado | Alto | ✅ S10 |
| 8 | `PostgreSqlSchemaReader` | No usaba `NamingConventionService` | Alto | ✅ S10 |
| 9 | `NamingConventionService` | Columnas PostgreSQL lowercase sin separadores | Medio | ✅ S10 |
| 10 | `NamingConventionService` | KebabName con acrónimos (MCDT → m-c-d-t) | Bajo | ✅ S11 |
| 11 | `FileGeneratorService` | Servicios duplicados cuando `ClassNamePlural` coincide | Bajo | ✅ S11 |
| 12 | `FileGeneratorService` | Tablas TA_* generaban frontend incorrectamente | Medio | ✅ S11 |
| 13 | `NamingConventionService` | Doble mayúscula inicial (EXpedientes) | Bajo | ✅ S11 |
| 14 | `program.scriban` | Swagger solo en Development | Bajo | ✅ S11 |
| 15 | `api_csproj.scriban` | Faltaban paquetes NuGet JWT | Alto | ✅ S11 |
| 16 | `Banner.cs` | Nombres microservicios interpretados como colores Spectre | Alto | ✅ S12 |
| 17 | `entity.scriban` | Navigation properties hacia entidades externas en microservicios | Alto | ✅ S12 |
| 18 | `ScribanTemplateEngine` | `microservice_class_names_array` no expuesto como ScriptObject nativo | Alto | ✅ S12 |
| 19 | `ForeignKeyMetadata` | Tipos FK/PK incompatibles (int→string) crasheaban EF Core | Crítico | ✅ S14 |
| 20 | `ForeignKeysScript` | `ScriptObject.Import` no renombraba propiedades anidadas | Alto | ✅ S14 |
| 21 | `FileGeneratorService` | `NavigationPropertyName` no se sincronizaba tras desambiguación | Alto | ✅ S14 |
| 22 | `TemplateContext` | `JwtKey` no expuesto — key vacía en appsettings.json generado | Alto | ✅ S14 |
| 23 | `infrastructure_csproj.scriban` | SDK.Web incorrecto para librería — CS5001 | Crítico | ✅ S13 |
| 24 | `audit_interceptor.scriban` | `IHttpContextAccessor` no disponible en Infrastructure | Alto | ✅ S13 |
| 25 | `FileGeneratorService` | Bloques huérfanos repository_ef sobreescribían repository_sp | Crítico | ✅ S14 |
| 26 | `SqlServerSchemaReader` | `RawIdx`/`RawFK`/`RawKey` records corrompidos durante edición | Crítico | ✅ S14 |
| 27 | `FeaturesConfig` | Cache solo como flag, sin implementación | Alto | ✅ S13 |
| 28 | `program.scriban` | Sin middleware de auditoría de negocio | Alto | ✅ S13 |
| 29 | `app.routes.ts` | Rutas estáticas — no dinámicas por rol | Alto | ⏳ S15 |
| 30 | `SecurityConfig` | TwoFactor definido pero no implementado | Medio | ⏳ S16 |
| 31 | `angular_app_config.scriban` | Sin registro de SignalR | Medio | ⏳ S16 |
| 32 | `AuditSaveChangesInterceptor` | No filtra por campos configurados en `auditing.tables` | Medio | ⏳ S15 |
| 33 | `ICurrentUserContext` | `SystemUserContext` retorna "system" — no lee claims JWT reales | Alto | ⏳ S15 |
| 34 | `sp_crud.scriban` | `sp_ToggleEstado` faltante en generación | Medio | ⏳ S15 |
| 35 | `FileGeneratorService` | `mixed` strategy definida pero no implementada en bifurcación | Alto | ⏳ S15 |

---

## 🔐 Modelo de Autenticación AIGEN — Arquitectura Parametrizable

### Modos soportados (`security.jwtSource`)
| Modo | Descripción | Generado |
|------|-------------|----------|
| `DatabaseTable` | JWT propio contra tabla de usuarios en BD | AuthController completo |
| `Hardcoded` | Credenciales en appsettings — solo demo/dev | AuthController simplificado |
| `OIDC` | Token externo validado + perfil local en BD | Middleware OIDC + claim mapping |

### Claims JWT generados
| Claim | Descripción | Fuente |
|-------|-------------|--------|
| `sub` | ID único del usuario | BD |
| `email` | Correo electrónico | BD / OIDC |
| `name` | Nombre completo | BD / OIDC |
| `preferred_username` | Login / username | BD / OIDC |
| `given_name` / `family_name` | Nombre y apellido | BD / OIDC |
| `roles[]` | Array de roles del sistema | BD / OIDC scopes |
| `jti` | ID único del token (blacklist) | Generado |

### Proveedores OIDC soportados
| Valor | Proveedor | Claims mapeados |
|-------|-----------|-----------------|
| `None` | JWT propio (default) | — |
| `Keycloak` | Keycloak / Red Hat SSO | `preferred_username`, `realm_access.roles` |
| `AzureAD` | Microsoft Entra ID | `upn`, `roles`, `groups` |
| `Auth0` | Auth0 | `nickname`, `https://ns/roles` |
| `Google` | Google Identity | `email`, `name`, `given_name`, `family_name` |
| `ServiciosCiudadanos` | Servicios Ciudadanos Digitales Colombia | `documento`, `tipoDocumento`, UID |

---

## 🔐 Modelo de Seguridad AIGEN — Capas de Protección

```
+-------------------------------------------------------------+
|  PIPELINE CI/CD (S20)                                       |
|  gitleaks · semgrep · OWASP dep-check · ZAP DAST · Sonar  |
+-------------------------------------------------------------+
|  INFRAESTRUCTURA (S17)                                      |
|  Nginx headers · Docker non-root · Secrets externos        |
+-------------------------------------------------------------+
|  API GENERADA (S19)                                         |
|  Rate limiting · CORS · HTTPS · CSP · HSTS · X-Frame      |
+-------------------------------------------------------------+
|  AUTORIZACIÓN (S15)                                         |
|  Menús por rol · Permisos granulares · AuthGuard           |
+-------------------------------------------------------------+
|  AUTENTICACIÓN (S11)                                        |
|  JWT · Refresh HttpOnly · BCrypt · OWASP A01-A09           |
+-------------------------------------------------------------+
|  AUDITORÍA (S13 parcial → S15 completa)                     |
|  EF Interceptor básico · SP Auditoría · por campo/tabla    |
+-------------------------------------------------------------+
|  IA (S21)                                                   |
|  OWASP LLM Top 10 · Guardrails · Audit log · No PII       |
+-------------------------------------------------------------+
```

### OWASP Top 10 × Semana de implementación
| OWASP | Vulnerabilidad | Semana | Mitigación generada |
|-------|---------------|--------|---------------------|
| A01 | Broken Access Control | S11+S15+S19 | `[Authorize]` · Menús por rol · Rate limiting |
| A02 | Cryptographic Failures | S11 | JWT HS256 · BCrypt · HttpOnly cookies |
| A03 | Injection | S4+S14+S19 | EF parameterizado · Dapper `@param` · `sp_executesql` tipado |
| A04 | Insecure Design | S19 | Security middleware · CSP · HSTS |
| A05 | Security Misconfiguration | S11+S19 | CORS restrictivo · Swagger solo dev |
| A06 | Vulnerable Components | S20 | `dotnet audit` + `npm audit` en CI |
| A07 | Auth Failures | S11+S19 | Rate limit login · Token expiry · HttpOnly |
| A08 | Software Integrity | S20 | Dependency check · Supply chain |
| A09 | Logging Failures | S7+S13+S15 | Serilog structured · Auditoría de negocio · EF Interceptor |
| A10 | SSRF | S19 | Validación URLs · Whitelist dominios |

### OWASP LLM Top 10 (S21)
| LLM | Vulnerabilidad | Guardrail en AIGEN |
|-----|---------------|---------------------|
| LLM01 | Prompt Injection | Sanitización inputs · system prompt inmutable |
| LLM02 | Insecure Output | Validar/sanitizar antes de ejecutar |
| LLM06 | Sensitive Disclosure | Sin conn strings/keys/PII en prompts |
| LLM08 | Excessive Agency | Confirmación humana antes de acciones |
| LLM09 | Overreliance | `// AI-generated — review required` |

---

## 🔑 Decisiones Arquitectónicas Tomadas

| Decisión | Justificación |
|----------|---------------|
| Scriban como motor de plantillas | Sintaxis limpia, soporte .NET nativo, templates versionables en Git |
| Prefijos de tabla como señales de generación | Estándar Línea base de almacenamiento |
| Clean Architecture por defecto | Separación de responsabilidades, testabilidad |
| EF Core + Dapper híbrido | EF para CRUD simple, Dapper para queries complejas |
| `ISchemaReader` como interfaz | Permite PostgreSQL/Oracle sin cambiar el generador |
| Soft-delete árbol de decisión | `Eliminado` → flag; `Estado` → estado; ninguno → físico |
| JWT parametrizable por `jwtSource` | `DatabaseTable` / `Hardcoded` / `OIDC` |
| Refresh token en cookie HttpOnly | Más seguro que localStorage |
| Angular Signals para estado de auth | Reactividad moderna Angular 17+ |
| `authGuard` + `roleGuard` + `permissionGuard` | Granularidad: autenticación · rol · permiso específico |
| Menús dinámicos desde API | Rutas Angular generadas en runtime según rol — no hardcodeadas |
| `MenuSeed.sql` auto-generado | Cada controller AIGEN = ítem de menú inicial automático |
| Auditoría por campo/tabla configurable | Compatibilidad con generador antiguo + flexibilidad nueva |
| `ICurrentUserContext` en Infrastructure | Sin dependencia de HttpContext en capa de infraestructura |
| EF Interceptor como provider de auditoría | Sin SPs extra, captura automática en SaveChanges |
| Caché por tabla en `aigen.json` | Tablas TP_* (parámetros) ideales para cache largo · TM_* no |
| SignalR para notificaciones real-time | Nativo .NET, escalable con Redis backplane en microservicios |
| MassTransit para mensajería | Abstracción sobre RabbitMQ/Azure SB — cambiar provider sin código |
| `crudStrategy` configurable | `direct` (default) · `storedProcedures` · `mixed` |
| Schema `[API]` para SPs | Separación de responsabilidades en BD — usuarios solo tienen EXECUTE |
| `sp_executesql` en GetByFilter | Sargability de índices — mejor que `ISNULL(col, '') = ISNULL(@col, col)` |
| `SCOPE_IDENTITY()` en lugar de `@@identity` | Thread-safe en concurrencia — `@@identity` afectado por triggers |
| Templates en archivos vs tabla `API.paramGen` | Versionables en Git, debuggeables, reproducibles |
| `--no-interactive` flag en CLI | Permite uso en scripts CI/CD sin intervención manual |
| Docker multistage build | Imagen final mínima — reduce superficie de ataque |
| k6 para load testing | JS scripting, CI-native, métricas p95/p99 |
| Playwright para E2E | Multi-browser, mejor integración CI que Cypress |
| TestContainers para integración | BD real en Docker durante tests — sin mocks de BD |
| Rate limiting .NET 8 built-in | Sin dependencias externas |
| Semgrep para SAST | Reglas OWASP .NET + Angular, pocos falsos positivos |
| OWASP LLM guardrails en agente | AIGEN genera código — responsabilidad de no amplificar vulnerabilidades |
| MCP Server AIGEN | Expone AIGEN como herramienta para Claude Desktop/Cursor/VS Code |
| Agente autónomo con confirmación humana | OWASP LLM08 — no ejecutar acciones destructivas sin aprobación |
| IA con modelo base configurable | Claude primario · OpenAI fallback · Ollama offline |
| RAG con Qdrant | Semántica sobre código generado para respuestas precisas |
| Microservicios agrupados por prefijo | TM_→MovementService · TP_→ParameterService |
| `ForeignKeysScript` como ScriptObject explícito | `ScriptObject.Import` no renombra propiedades anidadas en snake_case |
| `NavigationPropertyName` sync tras desambiguación | FK navigation properties apuntan al nombre correcto tras rename |
| `GetAll` sin BLOBs + `GetAllFull` con BLOBs | Optimización de red — patrón histórico TiGlobal preservado en SP |
| Deuda técnica documentada en checklist | Visibilidad completa de lo pendiente — evita acumulación silenciosa |
