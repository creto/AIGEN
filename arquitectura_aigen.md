# 🏗️ AIGEN — Arquitectura del Sistema
**Versión:** 1.5 (Semana 15)  
**Última actualización:** Marzo 2026  
**Autor:** TiGlobal SAS  

---

## 1. ¿Qué es AIGEN?

AIGEN es un **generador de código fuente** que lee el schema de una base de datos relacional y produce una solución .NET 8 + Angular 18 completa y compilable, siguiendo Clean Architecture, los estándares históricos de TiGlobal SAS y las mejores prácticas modernas.

```
Base de datos  ──►  AIGEN  ──►  Solución .NET 8 + Angular 18 + SQL Scripts
(SQL Server /         │               listo para compilar y ejecutar
 PostgreSQL)          │
                      ▼
              0 errores · 0 warnings
              2767 archivos generados
```

---

## 2. Arquitectura interna de AIGEN

AIGEN está construido sobre Clean Architecture dividida en 4 proyectos:

```
AIGEN.sln
├── Aigen.Core           ← Modelos de metadatos + Configuración + Interfaces
├── Aigen.Templates      ← Motor Scriban + TemplateContext + FileGeneratorService
├── Aigen.CLI            ← Comandos Spectre.Console (generate, read-schema, new)
└── Aigen.Tests          ← Tests unitarios e integración
```

### 2.1 Aigen.Core

Contiene el modelo de dominio del generador:

```
Aigen.Core/
├── Config/
│   ├── GeneratorConfig.cs        ← Raíz de configuración (deserializa aigen.json)
│   ├── ConfigModels.cs           ← BackendConfig, FrontendConfig, SecurityConfig,
│   │                                FeaturesConfig, AuditConfig, AuditTableConfig,
│   │                                AIConfig, OutputConfig
│   └── GeneratorConfig.cs        ← ProjectConfig, DatabaseConfig, ArchitectureConfig
├── Config/Enums/
│   ├── OrmType.cs                ← EFCoreWithDapper | Dapper | EntityFrameworkCore
│   ├── CacheProvider.cs          ← None | InMemory | Redis
│   ├── AuthenticationType.cs     ← Jwt | ApiKey | None
│   └── AIProviderType.cs         ← None | Claude | OpenAI | Ollama
├── Interfaces/
│   └── ISchemaReader.cs          ← Contrato para leer schema de BD
├── Metadata/
│   ├── TableMetadata.cs          ← Tabla: columnas, PKs, FKs, índices
│   ├── ColumnMetadata.cs         ← Columna: tipo, nullable, PK, audit, blob
│   ├── ForeignKeyMetadata.cs     ← FK: tabla referenciada, tipos C#
│   ├── StoredProcedureMetadata.cs← SP: parámetros, tipo retorno, schema
│   ├── TableType.cs              ← Enum: Movement, Basic, Parameter, etc.
│   └── TableMetadataExtensions.cs← HasFullCrud(), IsReadOnly(), GetTableType()
└── Services/
    ├── NamingConventionService.cs ← PascalCase, camelCase, kebab-case, plural español
    └── SqlServerSchemaReader.cs   ← Lee schema SQL Server + stored procedures
    └── PostgreSqlSchemaReader.cs  ← Lee schema PostgreSQL con normalización lowercase
```

### 2.2 Aigen.Templates

Contiene el motor de generación:

```
Aigen.Templates/
├── Engine/
│   ├── ScribanTemplateEngine.cs  ← Renderiza .scriban con contexto snake_case
│   ├── TemplateContext.cs        ← Contexto completo inyectado en plantillas
│   │                                (snake_case automático via renamer)
│   ├── TemplateLocator.cs        ← Resuelve rutas: Backend/ Frontend/ Solution/
│   └── ScribanFunctions.cs       ← Funciones utilitarias disponibles en plantillas
├── FileGeneratorService.cs       ← Orquesta generación por tabla y por solución
│                                    Bifurca: SP vs EF vs Dapper vs Mixed
└── Templates/
    ├── Backend/                  ← Plantillas por entidad (generadas N veces)
    │   ├── entity.scriban
    │   ├── dto.scriban
    │   ├── irepository.scriban
    │   ├── service.scriban
    │   ├── controller.scriban
    │   ├── validator.scriban
    │   ├── entity_configuration.scriban
    │   ├── repository_ef.scriban
    │   ├── repository_dapper.scriban
    │   ├── repository_sp.scriban   ← Dapper + CommandType.StoredProcedure
    │   ├── sp_crud.scriban         ← 8 SPs T-SQL por tabla (estándar TiGlobal)
    │   ├── audit_interceptor.scriban ← ICurrentUserContext + HttpCurrentUserContext
    │   └── cache_service.scriban    ← ICacheService + Memory/Redis
    └── Solution/                 ← Plantillas de solución (generadas 1 vez)
        ├── program.scriban         ← Program.cs con DI completo + health endpoints
        ├── appsettings.scriban
        ├── dbcontext.scriban
        ├── solution.scriban
        ├── api_csproj.scriban
        ├── infrastructure_csproj.scriban
        ├── auth_controller.scriban ← JWT: login/refresh/logout/me
        ├── menu_controller.scriban ← GET /api/menu + GET /api/menu/privilegios
        ├── menu_seed.scriban       ← CREATE TABLE Seguridad.* + MERGE 149 items
        ├── app_component.scriban   ← Angular AppComponent con menú dinámico
        ├── angular_menu_service.scriban ← MenuService Angular
        ├── angular_auth_service.scriban
        ├── angular_auth_guard.scriban
        ├── angular_login_component.scriban
        ├── angular_environment.scriban
        ├── angular_error_interceptor.scriban
        ├── angular_auth_interceptor.scriban
        ├── angular_app_config.scriban
        ├── angular_app_routes.scriban
        ├── solution_microservice.scriban
        ├── dockerfile.scriban
        ├── docker_compose.scriban
        ├── gateway_program.scriban
        ├── gateway_yarp.scriban
        ├── gateway_csproj.scriban
        ├── architecture.scriban
        └── deployment.scriban
```

### 2.3 Aigen.CLI

CLI construida con Spectre.Console:

```
Aigen.CLI/
├── Commands/
│   ├── GenerateCommand.cs   ← generate --config --no-interactive --dry-run --verbose
│   ├── ReadSchemaCommand.cs ← read-schema --config
│   └── NewCommand.cs        ← new --name --db --orm --auth
├── UI/
│   └── Banner.cs            ← Markup.Escape() para Spectre.Console
└── Program.cs               ← Registro de comandos
```

---

## 3. Flujo de ejecución

```
aigen generate --config aigen.json --no-interactive
       │
       ▼
1. Deserializar GeneratorConfig (JSON → C#)
       │
       ▼
2. ISchemaReader.ReadTablesAsync()
   ┌── SqlServerSchemaReader  ← INFORMATION_SCHEMA + sys.* + ReadStoredProceduresAsync()
   └── PostgreSqlSchemaReader ← information_schema + NormalizePostgresName()
       │
       ▼
3. NamingConventionService
   ├── PascalCase (ClassName, EntityName)
   ├── camelCase (TsPropertyName)
   ├── kebab-case (Angular route)
   ├── Plural español (ClassNamePlural)
   └── Desambiguación (TH_Serie + TB_Serie → SerieHist)
       │
       ▼
4. FileGeneratorService
   ├── Por cada tabla:
   │   ├── new TemplateContext { Table, Db, Config }
   │   ├── GenerateBackendAsync()
   │   │   ├── entity.scriban       → Domain/Entities/
   │   │   ├── dto.scriban          → Application/{Entidad}/DTOs/
   │   │   ├── irepository.scriban  → Application/Interfaces/
   │   │   ├── service.scriban      → Application/{Entidad}/
   │   │   ├── controller.scriban   → API/Controllers/
   │   │   └── [SP o EF o Dapper según crudStrategy+SpTables]
   │   │       ├── repository_sp + sp_crud  (storedProcedures / mixed+marcada)
   │   │       ├── repository_ef + entity_configuration  (EF)
   │   │       └── repository_dapper  (Dapper)
   │   └── GenerateFrontendAsync()
   │       ├── model.ts, service.ts, list.component.ts, form.component.ts
   │       └── [skip si TA_/TH_/TAR_]
   └── GenerateSolutionAsync() — 1 vez
       ├── dbcontext.scriban
       ├── program.scriban (DI + Swagger + JWT + Audit + Cache + Health)
       ├── auth_controller.scriban
       ├── menu_controller.scriban
       ├── menu_seed.scriban → sql/menu_seed.sql
       ├── app_component.scriban (menú dinámico Angular)
       ├── angular_menu_service.scriban
       ├── architecture.scriban → ARCHITECTURE.md
       └── deployment.scriban → DEPLOYMENT.md
       │
       ▼
5. GenerationResult
   ├── Files: lista de archivos generados
   ├── Errors: errores de plantilla
   └── Warnings: plantillas no encontradas
```

---

## 4. TemplateContext — Contrato de datos

El `TemplateContext` es el puente entre el schema de BD y las plantillas Scriban. Se renombra automáticamente a `snake_case`:

```csharp
// Propiedades principales (snake_case en Scriban)
entity_name           → "Dependencia"
entity_name_plural    → "Dependencias"
kebab_name            → "dependencia"
kebab_name_plural     → "dependencias"
controller_name       → "DependenciasController"
root_namespace        → "Doc4us.SGDEA"
project_name          → "Doc4Us"

// Columnas
columns[]             → lista de columnas con is_primary_key, c_sharp_type, is_blob, etc.
form_columns[]        → columnas sin PK/identity/audit
foreign_keys_script[] → FKs con constraint_name, navigation_property_name, tipos C#

// Estrategia
crud_strategy         → "direct" | "storedProcedures" | "mixed"
use_sp_for_this_table → true si esta tabla usa SPs en modo mixed
sp_prefix             → "PA_"
sp_schema             → "API"

// Auditoría
audit_enabled         → true/false global
audit_enabled_for_table → true si esta tabla está habilitada
audit_table_filters[] → [{entity_name, fields, fields_quoted}]

// Seguridad
jwt_key, jwt_issuer, jwt_audience
use_ef_interceptor, use_audit_sp

// Caché
use_cache, use_redis, use_memory_cache

// AllTablesScript (para plantillas de solución)
all_tables_script[]   → [{repository_name, service_name, class_name, controller_name,
                           has_full_crud, kebab_name_plural, table_name, ...]}
```

---

## 5. Arquitectura del código generado

### 5.1 Clean Architecture — Monolito

```
{ProjectName}.sln
├── {ProjectName}.Domain
│   └── Entities/
│       └── {Entidad}.cs              ← Propiedades C# + [Key] + [MaxLength]
│
├── {ProjectName}.Application
│   ├── {Entidades}/
│   │   ├── DTOs/
│   │   │   ├── {Entidad}Dto.cs       ← DTO de lectura
│   │   │   ├── Create{Entidad}Request.cs
│   │   │   ├── Update{Entidad}Request.cs
│   │   │   └── {Entidad}PagedResponse.cs (record posicional)
│   │   └── {Entidad}Service.cs       ← Lógica de negocio
│   └── Interfaces/
│       └── I{Entidad}Repository.cs   ← Contrato: GetById, GetPaged, Create, Update, Delete,
│                                        ToggleEstado, Exists
│
├── {ProjectName}.Infrastructure
│   ├── Persistence/
│   │   ├── {ProjectName}DbContext.cs ← EF Core DbContext
│   │   ├── Repositories/
│   │   │   └── {Entidad}Repository.cs← EF Core + Dapper / SP según crudStrategy
│   │   ├── Configurations/           ← Fluent API por entidad (solo en modo EF)
│   │   │   └── {Entidad}Configuration.cs
│   │   └── Interceptors/
│   │       └── AuditSaveChangesInterceptor.cs ← ICurrentUserContext + diccionario campos
│   └── Cache/
│       └── CacheService.cs           ← ICacheService + Memory/Redis
│
└── {ProjectName}.API
    ├── Controllers/
    │   ├── {Entidad}sController.cs   ← REST CRUD: GET/POST/PUT/DELETE/PATCH toggle
    │   ├── AuthController.cs         ← /api/auth/login · refresh · logout · me
    │   └── MenuController.cs         ← /api/menu · /api/menu/privilegios
    ├── Program.cs                    ← DI + Swagger + JWT + CORS + Serilog + Health
    └── appsettings.json
```

### 5.2 Repositorio — Árbol de decisión

```
crudStrategy = "direct"
    orm = EFCoreWithDapper  → repository_ef   + entity_configuration
    orm = Dapper            → repository_dapper
    orm = EntityFrameworkCore → repository_ef + entity_configuration

crudStrategy = "storedProcedures"
    → repository_sp (Dapper CommandType.StoredProcedure)
    → sp_crud.sql (8 SPs por tabla)

crudStrategy = "mixed"
    tabla en SpTables[]  → repository_sp + sp_crud.sql
    tabla fuera SpTables → según orm (EF/Dapper)
```

### 5.3 Soft-delete — Árbol de decisión

```
tabla.Columns contiene "Eliminado" → soft delete: Eliminado = true
tabla.Columns contiene "Estado"    → soft delete: Estado = false
tabla no tiene ninguno             → delete físico
```

### 5.4 Microservicios YARP

```
GeneratedMicroservices/
├── BasicService/          ← TB_* tables
│   └── {Clean Architecture completa}
│       └── Program.cs     ← GET /health · GET /ready
├── DocumentService/        ← TM_* document tables
├── ParameterService/       ← TP_* parameter tables
├── RelationalService/      ← TBR_* + TR_* tables
└── Gateway/                ← YARP API Gateway
    ├── Program.cs
    └── yarp.json           ← Rutas → BasicService:8081, DocumentService:8082, ...
```

---

## 6. Convención de tablas — Línea base de almacenamiento

| Prefijo | Tipo | HasFullCrud | IsReadOnly | Descripción |
|---|---|---|---|---|
| `TM_` | Movement | ✅ | ❌ | Tablas de movimiento/transaccional |
| `TB_` | Basic | ✅ | ❌ | Tablas básicas de entidades |
| `TBR_` | BasicRelated | ✅ | ❌ | Básicas con relaciones complejas |
| `TP_` | Parameter | ✅ | ❌ | Tablas paramétricas / catálogos |
| `TR_` | Relational | ❌ | ❌ | Tablas relacionales N:M |
| `TC_` | Composition | ✅ | ❌ | Tablas de composición |
| `TS_` | System | ❌ | ✅ | Tablas de sistema (solo entidad) |
| `TA_` | Audit | ❌ | ✅ | Tablas de auditoría (solo entidad) |
| `TH_` | Historical | ❌ | ✅ | Tablas históricas (solo entidad) |

---

## 7. Stored Procedures — Estándar TiGlobal modernizado

### Nomenclatura
```
[{SpSchema}].[{SpPrefix}{TableName}{Operacion}]
Ejemplo: [API].[PA_TB_DependenciaAdd]
```

### 8 SPs por tabla

| SP | Descripción | Mejora vs histórico |
|---|---|---|
| `PA_TablaAdd` | INSERT + retorna registro | `SCOPE_IDENTITY()` en lugar de `@@identity` |
| `PA_TablaUpdate` | UPDATE parámetros opcionales | `ISNULL(@param, columna)` + `and Estado=1` |
| `PA_TablaDelete` | Soft-delete + `ValidarDependencias` | `CREATE OR ALTER PROCEDURE` (idempotente) |
| `PA_TablaGetAll` | SELECT sin BLOBs, activos | Orden DESC por defecto |
| `PA_TablaGetAllFull` | SELECT * incluyendo BLOBs | Para exportación completa |
| `PA_TablaGetById` | SELECT por PK | — |
| `PA_TablaGetByFilter` | Búsqueda dinámica paginada | `sp_executesql` + `OFFSET/FETCH` (sargability) |
| `PA_TablaToggleEstado` | `CASE WHEN Estado=1 THEN 0 ELSE 1` | Nuevo — no existía en histórico |

---

## 8. Modelo de menú — Esquema Seguridad

### Tablas generadas

```sql
[Seguridad].[TP_Menu]          -- árbol jerárquico (Padre, Orden, OrdenPadre)
[Seguridad].[TR_MenuRol]       -- menú × AspNetRoles (Identity compatible)
[Seguridad].[TP_Privilegio]    -- 11 privilegios SGDEA
[Seguridad].[TR_RolPrivilegio] -- rol × privilegio
```

### Flujo de carga del menú Angular

```
1. Usuario hace login → AuthService guarda JWT en localStorage
2. AppComponent.ngOnInit() → AuthService.isAuthenticated() = true
3. MenuService.getMenu() → GET /api/menu (Bearer token)
4. MenuController extrae roles[] del claim JWT
5. JOIN TP_Menu → TR_MenuRol WHERE AspNetRoles IN (roles)
6. BuildTree() arma árbol Padre/Orden recursivo
7. Angular mapToMenuItems() → MenuItem[] PrimeNG
8. p-panelmenu renderiza árbol jerárquico dinámico
```

### 11 Privilegios SGDEA

```
Ver · Crear · Editar · Eliminar · Exportar
Imprimir · Descargar · Cargar · Actualizar · Firmar · Aprobar
```

---

## 9. Auditoría — Arquitectura de capas

```
HTTP Request
     │
     ▼
Controller → Service → Repository → DbContext.SaveChangesAsync()
                                           │
                                           ▼
                              AuditSaveChangesInterceptor
                                    │
                                    ├── ICurrentUserContext.Username
                                    │   └── HttpCurrentUserContext
                                    │       └── JWT claim: preferred_username → name → Identity.Name
                                    │
                                    ├── _auditFields[entityName]
                                    │   ├── null → auditar todos los campos
                                    │   └── HashSet<string> → solo campos configurados
                                    │
                                    └── INSERT INTO TA_AuditoriaTransacciones
                                        (Entidad, Operacion, CampoModificado,
                                         ValorAnterior, ValorNuevo, IdRegistro,
                                         Usuario, IP, FechaHora)
```

### Configuración en `aigen.json`

```json
"audit": {
  "provider": "EFInterceptor",
  "globalEnabled": true,
  "auditTable": "TA_AuditoriaTransacciones",
  "tables": {
    "TB_Funcionario": { "enabled": true,  "fields": ["Nombre","Email","Cargo"] },
    "TM_Documento":   { "enabled": true,  "fields": ["*"] },
    "TP_Ciudad":      { "enabled": false }
  }
}
```

---

## 10. Health endpoints — Cloud-native

Todos los modos generan endpoints de salud en `Program.cs`:

```
GET /health  → 200 OK { status: "healthy", service: "...", time: "..." }
              Liveness: el proceso está vivo

GET /ready   → 200 OK { status: "ready", ... }
              Readiness: la BD responde (SELECT 1)
           → 503 Problem si la BD no responde
```

Usados por Docker Compose, Kubernetes y el Gateway YARP.

---

## 11. Configuración completa — `aigen.json`

```json
{
  "aigenVersion": "1.0",
  "project": {
    "projectName": "Doc4Us",
    "rootNamespace": "Doc4us.SGDEA",
    "author": "TiGlobal SAS",
    "version": "1.0.0"
  },
  "database": {
    "provider": "SqlServer",
    "connectionString": "Server=...;Database=Doc4UsAIGen;..."
  },
  "architecture": {
    "style": "Monolith",
    "separateSolutionPerService": false,
    "tablePrefixGrouping": {}
  },
  "backend": {
    "orm": "EFCoreWithDapper",
    "crudStrategy": "direct",
    "spPrefix": "PA_",
    "spSchema": "API",
    "spTables": [],
    "targetFramework": "net8.0"
  },
  "frontend": {
    "framework": "Angular",
    "apiBaseUrl": "http://localhost:5000",
    "apiBaseProdUrl": "https://api.midominio.com"
  },
  "security": {
    "authentication": "Jwt",
    "jwtSource": "DatabaseTable",
    "userTable": "TB_Usuarios",
    "jwtKey": "...",
    "jwtIssuer": "Doc4Us",
    "jwtAudience": "Doc4UsApp",
    "jwtExpiresMinutes": 60,
    "useRefreshToken": true,
    "refreshExpiresDays": 7
  },
  "features": {
    "auditing": true,
    "cache": "InMemory",
    "generatePagination": true,
    "softDelete": true
  },
  "audit": {
    "provider": "EFInterceptor",
    "globalEnabled": true,
    "auditTable": "TA_AuditoriaTransacciones",
    "tables": {}
  },
  "tableSelection": "Exclude",
  "excludedPrefixes": ["TH_"],
  "excludedTables": ["__EFMigrationsHistory", "sysdiagrams"],
  "output": {
    "path": "C:\\DevOps\\AIGEN\\AIGEN\\Generated",
    "overwriteExisting": true
  }
}
```

---

## 12. Scripts de automatización

### rebuild_and_generate.ps1 v4

```
Paso 1: Limpiar /src, /frontend, /docs, /sql del Output
Paso 2: dotnet restore + dotnet build aigen.sln
Paso 3: dotnet test aigen.sln (opcional -SkipTests)
Paso 4: aigen generate --config aigen.json --no-interactive
Paso 5: dotnet build {GeneratedSolution}.sln (valida 0 errores)
Paso 6: ng build (opcional -SkipFrontend)
Paso 7: CODE_SUMMARY.md (conteo archivos + líneas por capa)
Paso 8: dotnet run API (opcional -RunApi)
```

### Perfiles disponibles

| Perfil | Comando | Archivos generados |
|---|---|---|
| SqlServer monolito | `-Db SqlServer` | 2767 · 0 errores |
| PostgreSQL monolito | `-Db Postgres` | ~2000 · 0 errores |
| Stored Procedures | `-Db SP` | 1353 · 0 errores |
| Mixed (EF+SP) | `aigen_mixed.json` | 2081 · 0 errores |
| Microservicios + YARP | `-Db Microservices` | 1102 · 0 errores |

---

## 13. Roadmap tecnológico

| Semana | Feature | Estado |
|---|---|---|
| S1-S10 | Core: schema reader, clean arch, EF+Dapper, soft-delete, PostgreSQL | ✅ |
| S11 | Frontend Angular + JWT end-to-end | ✅ |
| S12 | Microservicios + API Gateway YARP | ✅ |
| S13 | Auditoría EF Interceptor + Caché | ✅ |
| S14 | Stored Procedures (estándar TiGlobal) | ✅ |
| S15 | Menús dinámicos + privilegios + deuda técnica | ✅ |
| S16 | SignalR + MassTransit + 2FA | ⏳ |
| S17 | Docker completo + Nginx | ⏳ |
| S18 | Testing completo (Unit/Integration/E2E/Load) | ⏳ |
| S19 | Seguridad OWASP + Rate limiting | ⏳ |
| S20 | CI/CD GitHub Actions + SonarCloud | ⏳ |
| S21 | IA: Assistant + Agente + MCP Server | ⏳ |
| S22 | Multi-frontend (React/Vue) + temas | ⏳ |
| S23 | Documentación + NuGet `dotnet tool install -g aigen` | ⏳ |
