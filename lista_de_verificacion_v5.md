# 🗺️ AIGEN — Derrotero de Desarrollo v5
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
Semana 12 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Microservicios + API Gateway (YARP)
Semana 13 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Stored Procedures + funciones BD
Semana 14 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Dockerización completa + Nginx
Semana 15 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Testing completo (Unit + Integration + E2E + Load)
Semana 16 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Seguridad + OWASP + Headers + Rate Limiting
Semana 17 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  CI/CD completo (GitHub Actions + SonarCloud + NuGet)
Semana 18 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  IA integrada como asistente de desarrollo
Semana 19 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Multi-frontend + paleta de colores + logo
Semana 20 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Documentación completa + empaquetado NuGet
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
- [x] **CRUD validado contra BD real Doc4UsAIGen** — GET paginado · GET by ID · POST · PUT · DELETE · PATCH toggle ✅
- [x] **0 errores · 0 warnings** ✅
- [x] Commit en github.com/creto/AIGEN ✅

---

## ✅ SEMANA 10 — Soft-Delete + PostgreSQL + Configuración avanzada
> **Estado: COMPLETADA ✅**

- [x] **Soft-delete via Estado** — árbol de decisión: `Eliminado` → soft flag · `Estado` → soft estado · sin ninguno → físico
- [x] **BaseQuery filtra `Estado == true`** — GET/GetPaged solo retorna registros activos
- [x] **Update solo sobre registros activos** — `e.Estado == true` en `FirstOrDefaultAsync`
- [x] **Swagger nullables** — `NullableSchemaFilter` en `program.scriban`
- [x] **`excludedPrefixes`** — `DatabaseConfig.cs` y `SchemaFilterService.cs` · `TH_` confirmado excluido ✅
- [x] **PostgreSQL support completo:**
  - `PostgreSqlSchemaReader` con `NamingConventionService` inyectado
  - `NormalizePostgresName()` — `creadoen` → `CreadoEn`
  - Aliases lowercase en `AuditColumns`
  - Validado contra BD `aigen_test` (PostgreSQL 18) · `AigenTest.sln` 0 errores ✅
  - Config: `aigen\configs\aigen_postgres.json` ✅
- [x] **GitHub** — `aigen.rar` (133MB) eliminado con `git filter-branch` + force push ✅
- [x] **0 errores · 0 warnings** ✅

---

## ✅ SEMANA 11 — Frontend Angular + JWT end-to-end + Naming fixes
> **Estado: COMPLETADA ✅** — 2077 archivos · 0 errores · 0 warnings

### Fixes de Naming
- [x] **KebabName acrónimos** — regex doble en `ToAngularFileName()` y `ToKebab()`: `MCDT`→`mcdt` · `MCDTAlgo`→`mcdt-algo` ✅
- [x] **Servicios duplicados** — `classNameSeen` en `GenerateAsync()` desambigua con sufijo de prefijo de tabla ✅
- [x] **Guard `skipFrontend`** — `TA_`/`TH_`/`TAR_` excluidas de `GenerateFrontendAsync` → 0 carpetas audit en frontend ✅
- [x] **Doble mayúscula inicial** — `EXpedientes` → `Expedientes` via regex `^([A-Z])([A-Z])([a-z])` ✅

### Feature 1 — Frontend Angular conectado al API real
- [x] `angular_environment.scriban` → `environments/environment.ts` · `environment.prod.ts` con `__API_URL__`
- [x] `angular_error_interceptor.scriban` → `core/interceptors/error.interceptor.ts` — toast PrimeNG (400/401/403/404/409/422/500)
- [x] `angular_auth_interceptor.scriban` → `core/interceptors/auth.interceptor.ts` — `Bearer token` automático
- [x] `angular_app_config.scriban` → `withInterceptors([authInterceptor, errorInterceptor])` + `MessageService` global
- [x] `FrontendConfig` ampliado: `ApiBaseUrl`, `ApiBaseProdUrl`

### Feature 2 — JWT end-to-end
- [x] **`SecurityConfig` parametrizable:** `JwtSource` · `UserTable` · `JwtKey` · `JwtIssuer` · `JwtAudience` · `JwtExpiresMinutes` · `RefreshExpiresDays` · `UseRefreshToken` · `OidcProvider` · `OidcAuthority` · `OidcClientId` · `OidcClientSecret`
- [x] **`program.scriban`** — `AddAuthentication().AddJwtBearer()` · `UseAuthentication()` · Swagger con botón Authorize Bearer
- [x] **`appsettings.scriban`** — sección `"Jwt"` completa
- [x] **`auth_controller.scriban`** — `POST /api/auth/login` · `POST /api/auth/refresh` · `POST /api/auth/logout` · `GET /api/auth/me`
  - Claims: `sub` · `email` · `name` · `preferred_username` · `jti` · `roles[]`
  - Refresh token en cookie HttpOnly + Secure + SameSite=Strict
- [x] **`api_csproj.scriban`** — `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0` + `System.IdentityModel.Tokens.Jwt 8.0.0`
- [x] **`angular_auth_service.scriban`** — signals: `isAuthenticated` · `currentUser` · `token` · `login()` · `logout()` · `refresh()` · `hasRole()`
- [x] **`angular_auth_guard.scriban`** — `authGuard` + `roleGuard(role)`
- [x] **`angular_login_component.scriban`** — PrimeNG: `p-card` · `pInputText` · `p-password` · `p-button` loading · `p-message` error

### Extra
- [x] `aigen.ps1` — launcher interactivo: menú BD (SqlServer/PostgreSQL/Ambos) × modo (5 perfiles)
- [x] **2077 archivos · 0 errores · 0 warnings · 0 kebabs rotos · 0 duplicados DI** ✅

---

## ⏳ SEMANA 12 — Microservicios + API Gateway
> **Estado: PENDIENTE**

### Feature — Arquitectura de Microservicios
- [ ] `separateSolutionPerService: true` en `aigen.json` — un `.sln` por prefijo de tabla
- [ ] Agrupación automática por dominio: `TM_` → `MovementService` · `TP_` → `ParameterService` · `TB_` → `BasicService`
- [ ] `Dockerfile.scriban` — imagen multistage por microservicio (.NET 8 SDK → runtime)
- [ ] `docker-compose.scriban` — orquestación base de todos los microservicios
- [ ] API Gateway con YARP — `gateway.scriban` con rutas hacia cada microservicio
- [ ] Health checks por servicio — `/health` y `/ready` endpoints generados
- [ ] Service discovery básico — variables de entorno por servicio en compose

**Nuevos archivos:** `solution_microservice.scriban`, `docker_compose.scriban`, `dockerfile.scriban`, `gateway.scriban`  
**Esfuerzo estimado:** 5-7 días

---

## ⏳ SEMANA 13 — Stored Procedures y Funciones BD
> **Estado: PENDIENTE**

### Feature — CRUD vía Stored Procedures
- [ ] `SqlServerSchemaReader` — leer `INFORMATION_SCHEMA.ROUTINES` (SPs y funciones escalares/tabla)
- [ ] `StoredProcedureMetadata` — nombre, parámetros IN/OUT, tipo de retorno, schema
- [ ] `aigen.json`: `crudStrategy: "direct" | "storedProcedures" | "mixed"`
- [ ] `repository_sp.scriban` — repositorio Dapper con ejecución de SPs (`ExecuteAsync`, `QueryAsync`)
- [ ] `sp_crud.scriban` — T-SQL: `sp_Insert_`, `sp_Update_`, `sp_Delete_`, `sp_GetById_`, `sp_GetPaged_`
- [ ] `PostgreSqlSchemaReader` — leer funciones PL/pgSQL (`information_schema.routines`)
- [ ] CLI: `aigen generate-sp-script` — genera solo script `.sql` sin código C#
- [ ] Output: script único ordenado con dependencias (primero SPs de parámetros, luego movimientos)

**Nuevos archivos:** `StoredProcedureMetadata.cs`, `repository_sp.scriban`, `sp_crud.scriban`  
**Esfuerzo estimado:** 4-5 días

---

## ⏳ SEMANA 14 — Dockerización Completa
> **Estado: PENDIENTE**

### Feature — Containerización de proyectos generados

#### Dockerfile por proyecto
- [ ] `dockerfile_api.scriban` — multistage build para .NET 8 API:
  - Stage 1 `build`: `mcr.microsoft.com/dotnet/sdk:8.0` — restore + publish
  - Stage 2 `runtime`: `mcr.microsoft.com/dotnet/aspnet:8.0` — imagen final mínima
  - Usuario no-root (`app`) por seguridad
  - Variables de entorno: `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`
- [ ] `dockerfile_frontend.scriban` — multistage para Angular:
  - Stage 1 `build`: `node:20-alpine` — `npm ci` + `ng build --configuration production`
  - Stage 2 `nginx`: `nginx:alpine` — servir dist/ con configuración customizada
- [ ] `nginx.conf.scriban` — reverse proxy: SPA routing + proxy `/api` → backend API + gzip + cache headers

#### docker-compose completo
- [ ] `docker_compose.scriban` — `docker-compose.yml` base (producción):
  - Servicio `api` — imagen .NET + healthcheck `curl /health`
  - Servicio `frontend` — imagen Nginx + proxy al API
  - Servicio `db` — SQL Server 2022 o PostgreSQL 16 según config
  - Red interna `app-network` (bridge)
  - Volúmenes persistentes para BD
  - `restart: unless-stopped` en todos los servicios
- [ ] `docker_compose_override.scriban` — `docker-compose.override.yml` (desarrollo):
  - Montar código fuente como volumen (hot reload)
  - Exponer puertos extras para debugging
  - Variables de entorno de dev (connection strings locales)
  - `watch` mode para frontend
- [ ] `docker_compose_prod.scriban` — `docker-compose.prod.yml`:
  - Secrets via Docker secrets o variables de entorno externas
  - Límites de recursos (CPU/memoria) por servicio
  - Logging drivers (json-file con rotación)
  - No exponer puertos de BD al host

#### Health checks y utilidades
- [ ] `healthcheck.scriban` — endpoint `/health` y `/ready` en el API generado:
  - `/health` — servicio vivo (liveness)
  - `/ready` — BD conectada (readiness): verifica conexión con `SELECT 1`
- [ ] `wait_for_it.scriban` — script `wait-for-it.sh` en compose para esperar BD antes de arrancar API
- [ ] `.dockerignore.scriban` — excluir `bin/`, `obj/`, `node_modules/`, `.git/`, archivos de dev

#### Variables de entorno y secretos
- [ ] `.env.example.scriban` — plantilla de variables de entorno documentada
- [ ] `docker_secrets.scriban` — instrucciones para Docker Swarm secrets vs compose env
- [ ] Soporte para leer `ConnectionStrings__Default` desde variables de entorno (ASP.NET Core convention)

**Nuevos archivos:** `dockerfile_api.scriban`, `dockerfile_frontend.scriban`, `nginx.conf.scriban`, `docker_compose.scriban`, `docker_compose_override.scriban`, `docker_compose_prod.scriban`, `healthcheck.scriban`, `wait_for_it.scriban`, `.dockerignore.scriban`, `.env.example.scriban`  
**Esfuerzo estimado:** 5-6 días

---

## ⏳ SEMANA 15 — Testing Completo Generado Automáticamente
> **Estado: PENDIENTE**

### Feature — Suite de Tests por Capa

#### Tests unitarios — xUnit
- [ ] `test_service_unit.scriban` — tests unitarios por `ServiceName`:
  - Mock de `IRepository` con Moq
  - Test `GetPagedAsync()` — verifica paginación y filtros
  - Test `GetByIdAsync()` — verifica 404 cuando no existe
  - Test `CreateAsync()` — verifica llamada al repositorio
  - Test `UpdateAsync()` — verifica 404 + actualización
  - Test `DeleteAsync()` — verifica soft-delete vs físico según configuración
  - Test `ToggleEstadoAsync()` — si tabla tiene campo Estado
- [ ] `test_naming_convention.scriban` — tests de `NamingConventionService`:
  - Casos PascalCase, camelCase, kebab con acrónimos
  - Plural en español (ción→ciones, z→ces, etc.)
- [ ] `test_project_unit.scriban` — `.csproj` para proyecto `{ProjectName}.Tests.Unit`

#### Tests de integración — WebApplicationFactory
- [ ] `test_integration_controller.scriban` — tests de integración por controller:
  - `GET /api/{entity}` — status 200 + paginación
  - `GET /api/{entity}/{id}` — status 200 con ID válido · 404 con ID inexistente
  - `POST /api/{entity}` — status 201 + Location header
  - `PUT /api/{entity}/{id}` — status 204
  - `DELETE /api/{entity}/{id}` — status 204
  - `POST /api/auth/login` — status 200 con token · 401 con credenciales inválidas
- [ ] `test_webfactory.scriban` — `CustomWebApplicationFactory` con BD en memoria (EF InMemory) o TestContainers
- [ ] `test_project_integration.scriban` — `.csproj` para `{ProjectName}.Tests.Integration` con TestContainers
- [ ] Soporte TestContainers: `Testcontainers.SqlServer` + `Testcontainers.PostgreSql` según config

#### Tests E2E — Playwright
- [ ] `playwright_config.scriban` — `playwright.config.ts` con baseURL apuntando al frontend
- [ ] `test_e2e_login.scriban` — test E2E de flujo de autenticación:
  - Navegar a `/login`
  - Ingresar credenciales
  - Verificar redirección al dashboard
  - Verificar token en localStorage
- [ ] `test_e2e_crud.scriban` — test E2E por entidad:
  - Navegar al listado
  - Crear nuevo registro
  - Editar registro
  - Eliminar con confirmación
- [ ] `test_e2e_project.scriban` — `package.json` con dependencias Playwright

#### Tests de carga — k6
- [ ] `test_load_smoke.scriban` — smoke test (1 usuario, 1 minuto): verifica que el sistema arranca
- [ ] `test_load_average.scriban` — carga promedio (10-50 usuarios): verifica tiempos de respuesta normales
- [ ] `test_load_stress.scriban` — stress test (ramp-up hasta 200 usuarios): encuentra el punto de quiebre
- [ ] `test_load_spike.scriban` — spike test (0→500 usuarios en 10s): verifica comportamiento ante picos
- [ ] `test_load_soak.scriban` — soak test (50 usuarios, 2 horas): detecta memory leaks
- [ ] Thresholds configurables: `p95 < 500ms`, `error_rate < 1%`, `rps > 100`
- [ ] Output: JSON + HTML report con `k6-reporter`

#### Tests de contrato — Schema Validation
- [ ] `test_contract_openapi.scriban` — valida que la API generada cumple su propio contrato OpenAPI:
  - Exporta `swagger.json` en tiempo de test
  - Verifica que todos los endpoints documentados responden
  - Verifica tipos de respuesta (schema matching)
- [ ] `test_contract_pact.scriban` — Pact consumer-driven contracts entre Angular y .NET API (opcional)

**Nuevos archivos:** `test_service_unit.scriban`, `test_integration_controller.scriban`, `test_webfactory.scriban`, `playwright_config.scriban`, `test_e2e_login.scriban`, `test_e2e_crud.scriban`, `test_load_smoke.scriban`, `test_load_average.scriban`, `test_load_stress.scriban`, `test_load_spike.scriban`, `test_load_soak.scriban`, `test_contract_openapi.scriban`  
**Esfuerzo estimado:** 7-9 días

---

## ⏳ SEMANA 16 — Seguridad + OWASP + Hardening
> **Estado: PENDIENTE**

### Feature — Seguridad en código generado y pipeline

#### Headers de seguridad HTTP
- [ ] `security_middleware.scriban` — middleware `SecurityHeadersMiddleware` generado en `program.scriban`:
  - `Content-Security-Policy` (CSP) — configurable por proyecto
  - `Strict-Transport-Security` (HSTS) — `max-age=31536000; includeSubDomains`
  - `X-Frame-Options: DENY`
  - `X-Content-Type-Options: nosniff`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy` — deshabilitar features innecesarias
- [ ] `nginx_security.scriban` — headers de seguridad en Nginx para el frontend
- [ ] `angular_csp.scriban` — `meta[http-equiv=Content-Security-Policy]` en `index.html`

#### Rate Limiting
- [ ] `rate_limiting.scriban` — configuración de Rate Limiting en `program.scriban` (.NET 8 built-in):
  - Policy `"fixed"` — 100 req/min por IP (endpoints generales)
  - Policy `"auth"` — 5 req/min por IP (endpoints `/api/auth/login`)
  - Policy `"strict"` — 10 req/min por IP (endpoints sensibles)
  - `[EnableRateLimiting("fixed")]` en `controller.scriban`
  - `[EnableRateLimiting("auth")]` en `auth_controller.scriban`
- [ ] Response 429 con `Retry-After` header

#### OWASP Top 10 — Mitigaciones generadas
- [ ] **A01 Broken Access Control** — `[Authorize]` en todos los controllers · `[AllowAnonymous]` solo en auth
- [ ] **A02 Cryptographic Failures** — JWT con HS256 mínimo 32 chars · Refresh token con CSPRNG · Passwords con BCrypt/Argon2
- [ ] **A03 Injection** — EF Core parameterizado por defecto · Dapper con `@param` · Sin concatenación de strings en queries
- [ ] **A05 Security Misconfiguration** — CORS restrictivo · HTTPS forzado · Swagger solo en Development
- [ ] **A07 Auth Failures** — Rate limiting en login · Token expiración configurable · HttpOnly cookies para refresh
- [ ] **A09 Logging Failures** — Serilog con structured logging · Eventos de auth logueados · Sin datos sensibles en logs

#### OWASP para IA (OWASP Top 10 for LLM)
- [ ] **LLM01 Prompt Injection** — sanitización de inputs antes de enviar al modelo · prompts de sistema no modificables por usuario
- [ ] **LLM02 Insecure Output Handling** — validar y sanitizar respuestas del modelo antes de ejecutar código generado
- [ ] **LLM06 Sensitive Information Disclosure** — no incluir connection strings, keys o PII en prompts · filtro de datos sensibles
- [ ] **LLM08 Excessive Agency** — AIGEN AI Assistant con permisos mínimos · no ejecutar código generado por IA sin revisión humana
- [ ] **LLM09 Overreliance** — advertencias en código generado por IA · marcar con comentario `// AI-generated — review required`
- [ ] Guardrails configurables en `aigen.json`: `ai.maxPromptLength`, `ai.allowCodeExecution`, `ai.sanitizeOutputs`

#### OWASP Dependency Check
- [ ] `owasp_dependency_check.scriban` — `nuget-audit.yml` para NuGet: `dotnet list package --vulnerable`
- [ ] `npm_audit.scriban` — `npm audit --audit-level=high` en el frontend generado
- [ ] Integración en CI/CD: fallo del pipeline si vulnerabilidades críticas/altas

#### Análisis de Secrets
- [ ] `gitleaks_config.scriban` — `.gitleaks.toml` configurado para el proyecto:
  - Detectar API keys, tokens JWT, connection strings, passwords
  - Reglas custom para patrones de la BD Línea base de almacenamiento
  - Exclusiones para archivos de test y ejemplos documentados
- [ ] `pre_commit_hooks.scriban` — `.pre-commit-config.yaml` con gitleaks + detect-secrets

#### SAST — Análisis Estático
- [ ] `semgrep_config.scriban` — `.semgrep.yml` con ruleset C# + Angular:
  - Reglas OWASP para .NET (SQL injection, XSS, insecure deserialization)
  - Reglas custom para patrones AIGEN (validar que auth está en controllers)
  - Integración con GitHub Actions: PR bloqueado si SAST falla

#### DAST — Análisis Dinámico
- [ ] `zap_config.scriban` — `zap-baseline.yml` para OWASP ZAP:
  - Baseline scan contra la API generada en CI
  - Active scan en entorno de staging
  - Generación de reporte HTML de vulnerabilidades
  - Targets configurables: API (`http://localhost:5000`) + Frontend (`http://localhost:4200`)
- [ ] `zap_auth.scriban` — script de autenticación para ZAP: login → obtener token → incluir en scans

**Nuevos archivos:** `security_middleware.scriban`, `nginx_security.scriban`, `rate_limiting.scriban`, `owasp_dependency_check.scriban`, `gitleaks_config.scriban`, `semgrep_config.scriban`, `zap_config.scriban`, `zap_auth.scriban`, `pre_commit_hooks.scriban`  
**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 17 — CI/CD Completo
> **Estado: PENDIENTE**

### Feature — Pipeline GitHub Actions end-to-end

#### Pipeline principal — `ci.yml`
- [ ] `github_actions_ci.scriban` — workflow `ci.yml` con jobs encadenados:
  ```
  build → unit-tests → integration-tests → security-scan → docker-build → deploy
  ```
- [ ] **Job `build`:**
  - `dotnet restore` + `dotnet build --no-restore -c Release`
  - `npm ci` + `ng build --configuration production`
  - Cache de NuGet packages y node_modules
- [ ] **Job `unit-tests`:**
  - `dotnet test --no-build --filter Category=Unit`
  - Upload de resultados `*.trx` como artifact
  - Coverage con Coverlet → upload a Codecov
- [ ] **Job `integration-tests`:**
  - Levantar SQL Server / PostgreSQL con `services:` de GitHub Actions
  - `dotnet test --filter Category=Integration`
  - TestContainers si se usa en lugar de services
- [ ] **Job `e2e-tests`:**
  - Levantar API + frontend en background
  - `npx playwright test`
  - Upload screenshots/videos de fallos como artifact
- [ ] **Job `load-tests`** (solo en rama main/release):
  - Levantar stack con docker-compose
  - Ejecutar smoke test k6
  - Fallo si p95 > 500ms

#### Pipeline de seguridad — `security.yml`
- [ ] `github_actions_security.scriban` — workflow `security.yml` (ejecuta en cada PR):
  - **Secrets scan:** `gitleaks detect --source . --report-format json`
  - **SAST:** `semgrep --config=.semgrep.yml --error`
  - **Dependency check NuGet:** `dotnet list package --vulnerable --include-transitive`
  - **Dependency check npm:** `npm audit --audit-level=high`
  - **OWASP ZAP baseline:** contra stack levantado en CI
  - Resumen de resultados en PR comment

#### Pipeline de calidad — SonarCloud
- [ ] `github_actions_sonar.scriban` — workflow integrado con SonarCloud:
  - `sonar-scanner` con token configurado en GitHub Secrets
  - Métricas: coverage mínimo 70% · duplicación < 3% · deuda técnica < 1h
  - Quality Gate: bloquea PR si no pasa
  - Badge en README generado

#### Pipeline de release — `release.yml`
- [ ] `github_actions_release.scriban` — workflow `release.yml` (tag `v*.*.*`):
  - Build + test completo
  - `docker build` + `docker push` a GitHub Container Registry (ghcr.io)
  - `dotnet pack` + `dotnet nuget push` a NuGet.org
  - Creación automática de GitHub Release con changelog

#### Deploy automático
- [ ] `github_actions_deploy_ssh.scriban` — deploy vía SSH a servidor:
  - `docker-compose pull && docker-compose up -d`
  - Verificación post-deploy: curl al `/health` endpoint
  - Rollback automático si healthcheck falla
- [ ] `github_actions_deploy_azure.scriban` — deploy a Azure App Service o AKS:
  - `az webapp deploy` o `kubectl apply`
  - Slot swap para zero-downtime deployment
  - Variables de entorno desde Azure Key Vault

#### Archivos de configuración del repositorio
- [ ] `github_dependabot.scriban` — `dependabot.yml`: actualizaciones automáticas NuGet + npm + GitHub Actions
- [ ] `github_pr_template.scriban` — `pull_request_template.md`: checklist de seguridad, tests, docs
- [ ] `github_codeowners.scriban` — `CODEOWNERS`: revisores automáticos por área del proyecto
- [ ] `editorconfig.scriban` — `.editorconfig` con reglas C# + TypeScript consistentes

**Nuevos archivos:** `github_actions_ci.scriban`, `github_actions_security.scriban`, `github_actions_sonar.scriban`, `github_actions_release.scriban`, `github_actions_deploy_ssh.scriban`, `github_actions_deploy_azure.scriban`, `github_dependabot.scriban`, `github_pr_template.scriban`  
**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 18 — IA Integrada como Asistente de Desarrollo
> **Estado: PENDIENTE**

### Feature — AIGEN AI Assistant
- [ ] Implementar `Aigen.AI.csproj` con SDK Anthropic .NET
- [ ] System prompt base: conoce convención Línea base de almacenamiento, stack .NET/Angular, plantillas AIGEN
- [ ] Modelos configurables: Claude API (primario), OpenAI GPT-4 (fallback), Ollama local (offline)
- [ ] Comando `aigen ai-assist` — modo conversacional para refinar código generado
- [ ] Comando `aigen ai-review [archivo]` — revisión de calidad + seguridad del código
- [ ] Comando `aigen ai-explain [entidad]` — explicación en lenguaje natural
- [ ] RAG sobre el código generado: indexar entidades en Qdrant para contexto semántico
- [ ] Capacidad de generar plantillas `.scriban` personalizadas via prompt
- [ ] Historial de conversaciones por proyecto (local SQLite)
- [ ] Guardrails OWASP LLM: sanitización de inputs/outputs · sin PII en prompts · audit log de consultas

**Nuevos archivos:** `Aigen.AI.csproj`, `AigenAssistant.cs`, `ai_prompt_base.md`, `ai_guardrails.cs`  
**Esfuerzo estimado:** 7-10 días

---

## ⏳ SEMANA 19 — Multi-Frontend + Paleta de Colores + Logo
> **Estado: PENDIENTE**

### Feature — Multi-Frontend
- [ ] `frontend.framework: "angular" | "react" | "vue" | "blazor"` en `aigen.json`
- [ ] `TemplateLocator` — resolver carpeta de frontend según framework configurado
- [ ] **React:** hooks + Axios + Ant Design — `ListPage.tsx`, `FormPage.tsx`, `AuthService.ts`
- [ ] **Vue 3:** Composition API + Axios + PrimeVue — `ListView.vue`, `FormView.vue`
- [ ] **Blazor:** componentes .razor + MudBlazor — `ListPage.razor`, `FormPage.razor`

### Feature — Paleta de Colores Configurable
- [ ] `primaryColor`, `secondaryColor`, `accentColor`, `darkMode` en `aigen.json`
- [ ] Angular: `styles.scss` con variables CSS PrimeNG personalizadas
- [ ] React/Vue: `theme.ts` con tokens de diseño
- [ ] Validación de contraste WCAG AA

### Feature — Logo e Imagen de Marca
- [ ] `logoPath` en `aigen.json` — PNG/SVG/JPG/ICO
- [ ] Copiar imagen a `assets/` + inyectar en navbar, sidebar y login
- [ ] Generar `favicon.ico` automáticamente desde PNG

**Esfuerzo estimado:** ~10 días

---

## ⏳ SEMANA 20 — Documentación Completa + Empaquetado
> **Estado: PENDIENTE**

### Feature — Documentación Auto-Generada
- [ ] `ARCHITECTURE.md` — diagrama ASCII art de capas + lista de entidades por dominio
- [ ] `DEPLOYMENT.md` — instrucciones Docker + BD (SQL Server, PostgreSQL) + variables de entorno
- [ ] `SECURITY.md` — política de seguridad, vulnerabilidades conocidas, contacto responsable
- [ ] `CODE_SUMMARY.md` — desglose por prefijo, estadísticas CRUD, cobertura tests
- [ ] `CHANGELOG.md` — diff de archivos entre generaciones · Conventional Commits
- [ ] `api_catalog.scriban` — catálogo de endpoints con ejemplos request/response + auth
- [ ] `db_dictionary.scriban` — diccionario de datos completo de todas las tablas
- [ ] API Reference: OpenAPI exportado como Markdown y HTML estático
- [ ] Comando CLI: `aigen generate-docs --format pdf|html|md`

### Empaquetado y CI/CD
- [ ] `dotnet tool install -g aigen` — empaquetado como herramienta global .NET
- [ ] Pruebas contra 3+ BDs de clientes reales antes de publicar
- [ ] Publicación en NuGet con versionado semántico automático

**Esfuerzo estimado:** 5-6 días

---

## 🚀 Nuevas Features — Resumen y Roadmap

| # | Feature | Semana | Impacto | Estado |
|---|---------|--------|---------|--------|
| 1 | Multi-schema + schemas de BD | S10 | Alto | ✅ Completado |
| 2 | Soporte PostgreSQL completo | S10 | Alto | ✅ Completado |
| 3 | excludedPrefixes en aigen.json | S10 | Medio | ✅ Completado |
| 4 | Soft-delete via Estado / Eliminado | S10 | Alto | ✅ Completado |
| 5 | Swagger nullables (int? → null) | S10 | Bajo | ✅ Completado |
| 6 | KebabName con acrónimos | S11 | Bajo | ✅ Completado |
| 7 | Servicios duplicados ClassNamePlural | S11 | Bajo | ✅ Completado |
| 8 | Frontend Angular conectado al API real | S11 | Alto | ✅ Completado |
| 9 | JWT end-to-end (DatabaseTable mode) | S11 | Alto | ✅ Completado |
| 10 | SecurityConfig parametrizable JWT/OIDC | S11 | Alto | ✅ Completado |
| 11 | Launcher interactivo aigen.ps1 | S11 | Medio | ✅ Completado |
| 12 | Microservicios + API Gateway YARP | S12 | Alto | ⏳ Pendiente |
| 13 | Stored Procedures + script SQL | S13 | Medio | ⏳ Pendiente |
| 14 | Dockerización completa + Nginx | S14 | Alto | ⏳ Pendiente |
| 15 | Testing Unit + Integration + E2E + Load | S15 | Alto | ⏳ Pendiente |
| 16 | Seguridad OWASP + Headers + Rate Limit | S16 | Alto | ⏳ Pendiente |
| 17 | OWASP para IA (LLM Top 10) | S16 | Alto | ⏳ Pendiente |
| 18 | CI/CD completo GitHub Actions | S17 | Alto | ⏳ Pendiente |
| 19 | SonarCloud + calidad de código | S17 | Medio | ⏳ Pendiente |
| 20 | Deploy SSH + Azure automático | S17 | Alto | ⏳ Pendiente |
| 21 | IA integrada como asistente | S18 | Alto | ⏳ Pendiente |
| 22 | Multi-frontend React/Vue/Blazor | S19 | Alto | ⏳ Pendiente |
| 23 | Paleta de colores + Logo + Favicon | S19 | Medio | ⏳ Pendiente |
| 24 | Docs completa + empaquetado NuGet | S20 | Alto | ⏳ Pendiente |

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

## 🔒 Modelo de Seguridad AIGEN — Capas de Protección

```
┌─────────────────────────────────────────────────────────┐
│  PIPELINE CI/CD (S17)                                   │
│  gitleaks · semgrep · OWASP dep-check · ZAP DAST       │
├─────────────────────────────────────────────────────────┤
│  INFRAESTRUCTURA (S14)                                  │
│  Nginx headers · Docker non-root · Secrets externos    │
├─────────────────────────────────────────────────────────┤
│  API GENERADA (S16)                                     │
│  Rate limiting · CORS · HTTPS · CSP · HSTS · X-Frame  │
├─────────────────────────────────────────────────────────┤
│  AUTENTICACIÓN (S11)                                    │
│  JWT · Refresh HttpOnly · BCrypt · OWASP A01-A09       │
├─────────────────────────────────────────────────────────┤
│  IA (S18)                                               │
│  OWASP LLM Top 10 · Guardrails · Audit log · No PII   │
└─────────────────────────────────────────────────────────┘
```

### OWASP Top 10 — Cobertura por semana
| OWASP | Vulnerabilidad | Semana | Mitigación |
|-------|---------------|--------|------------|
| A01 | Broken Access Control | S11+S16 | `[Authorize]` + Rate limiting |
| A02 | Cryptographic Failures | S11 | JWT HS256 · BCrypt · HttpOnly cookies |
| A03 | Injection | S4+S16 | EF Core parameterizado · Dapper `@param` |
| A04 | Insecure Design | S16 | Security middleware · CSP · HSTS |
| A05 | Security Misconfiguration | S11+S16 | CORS restrictivo · Swagger solo dev |
| A06 | Vulnerable Components | S17 | `dotnet audit` + `npm audit` en CI |
| A07 | Auth Failures | S11+S16 | Rate limit login · Token expiry · HttpOnly |
| A08 | Software Integrity | S17 | Dependency check · Supply chain |
| A09 | Logging Failures | S7+S16 | Serilog structured · Sin PII en logs |
| A10 | SSRF | S16 | Validación URLs · Whitelist de dominios |

### OWASP LLM Top 10 — Cobertura (S18)
| LLM | Vulnerabilidad | Mitigación en AIGEN |
|-----|---------------|---------------------|
| LLM01 | Prompt Injection | Sanitización inputs · System prompt inmutable |
| LLM02 | Insecure Output | Validar/sanitizar respuestas antes de ejecutar |
| LLM06 | Sensitive Disclosure | Sin conn strings/keys en prompts · Filtro PII |
| LLM08 | Excessive Agency | Permisos mínimos · Sin auto-ejecución de código |
| LLM09 | Overreliance | Advertencias · `// AI-generated — review required` |

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
| `NamingConventionService` en PostgreSqlSchemaReader | Naming consistente entre SqlServer y PostgreSQL |
| `NormalizePostgresName()` para lowercase | PostgreSQL guarda nombres en lowercase — normalizar antes del PascalCase |
| Kebab con regex doble (acrónimos) | `([A-Z]+)([A-Z][a-z])` + `([a-z\d])([A-Z])` — PascalCase Y acrónimos |
| `skipFrontend` guard en FileGeneratorService | Tablas TA_/TH_/TAR_ solo generan Entity — nunca frontend |
| `classNameSeen` para desambiguar colisiones | Detecta colisiones antes de generar, añade sufijo de prefijo de tabla |
| JWT parametrizable por `jwtSource` | `DatabaseTable` / `Hardcoded` / `OIDC` — sin cambiar el generador |
| Refresh token en cookie HttpOnly | Más seguro que localStorage — no accesible desde JavaScript |
| Claims JWT con `preferred_username` | Compatibilidad con OIDC estándar (OpenID Connect Core 1.0) |
| `SecurityConfig` con `OidcProvider` | Keycloak, AzureAD, Auth0, Google, Servicios Ciudadanos Digitales |
| Angular Signals para estado de auth | Reactividad moderna sin RxJS/BehaviorSubject — Angular 17+ |
| `authGuard` + `roleGuard(role)` separados | Flexibilidad: proteger por autenticación O por rol específico |
| Docker multistage build | Imagen final mínima (solo runtime) — reduce superficie de ataque |
| Nginx como reverse proxy | SPA routing + proxy API + headers de seguridad en un solo lugar |
| k6 para load testing | Scripting en JS, integración nativa con CI/CD, métricas p95/p99 |
| Playwright para E2E | Más rápido que Cypress, soporte multi-browser, mejor CI integration |
| TestContainers para integración | BD real en Docker durante tests — sin mocks de BD |
| Rate limiting .NET 8 built-in | Sin dependencias externas — `AddRateLimiter()` nativo |
| gitleaks para secrets | Prevención en pre-commit + CI — detección antes de push |
| Semgrep para SAST | Reglas OWASP .NET + Angular — sin falsos positivos excesivos |
| OWASP ZAP para DAST | Estándar de industria — integración directa con GitHub Actions |
| SonarCloud para calidad | Quality Gate en PR — bloquea deuda técnica acumulada |
| OWASP LLM guardrails en S18 | AIGEN genera código con IA — responsabilidad de no amplificar vulnerabilidades |
| IA con modelo base configurable | Claude primario, OpenAI fallback, Ollama offline — sin vendor lock-in |
| RAG con Qdrant para contexto de IA | Semántica sobre el código generado para respuestas precisas |
| Microservicios agrupados por prefijo | TM_ → un dominio, TP_ → otro — mapeo natural del estándar Línea base |
