# 🗺️ AIGEN — Derrotero de Desarrollo v2.0
**Proyecto:** AI-Powered Software Generator (AIGEN)  
**Stack:** .NET Core 8 · Scriban · EF Core · Dapper · Angular 17 · PrimeNG  
**BD Objetivo:** SQL Server · PostgreSQL · Oracle (roadmap)  
**Convención BD:** Línea base de almacenamiento (TM_ / TB_ / TBR_ / TP_ / TR_ / TA_ / TS_ / TH_)  
**Repositorio:** github.com/creto/AIGEN  
**Última actualización:** Marzo 2026 — Semana 8 completada

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
Semana 9  ██░░░░░░░░░░░░░░░░░░  10% 🔄  Endpoints CRUD + validación BD real
Semana 10 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Multi-schema + PostgreSQL + Oracle
Semana 11 ░░░░░░░░░░░░░░░░░░░░   0% ⏳  Parámetros de proyecto + UI configuración
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
- [x] Script `rebuild_and_generate.ps1` — 6 pasos: clean, build, test, generate, compile, CODE_SUMMARY.md
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

## 🔄 SEMANA 9 — Validación End-to-End (EN CURSO)
> **Estado: EN CURSO 🔄**

- [ ] Probar endpoints GET / POST / PUT / DELETE desde Swagger contra BD real `Doc4UsAIGen`
- [ ] Validar CRUD completo para al menos 3 tablas representativas (TM_, TB_, TP_)
- [ ] Probar `ng build` del frontend Angular generado — identificar errores TypeScript
- [ ] Fix `excludedPrefixes` en `aigen.json` como alternativa a listar tablas individualmente
- [ ] Fix servicios duplicados cuando `ClassNamePlural` coincide entre tablas
- [ ] Documentar bugs de runtime encontrados

---

## ⏳ SEMANA 10 — Multi-Schema + PostgreSQL + Oracle
> **Estado: PENDIENTE**

### Feature 1 — Selección de Schemas de BD
- [ ] Propiedad `schemas: ["dbo", "Interop"]` en `aigen.json`
- [ ] `SqlServerSchemaReader` — filtro por schema en `INFORMATION_SCHEMA`
- [ ] CLI: selector interactivo de schemas disponibles antes de generar
- [ ] Namespace con schema prefix cuando hay múltiples schemas
- [ ] Resolver colisiones de nombre entre schemas

### Feature 4 — Soporte PostgreSQL y Oracle
- [ ] `PostgresSchemaReader` — tipos pg: text, uuid, bytea, jsonb, timestamp
- [ ] `OracleSchemaReader` — tipos Oracle: NUMBER, VARCHAR2, CLOB, DATE, BLOB
- [ ] `SchemaReaderFactory` — factory por `DatabaseEngine` enum
- [ ] `GeneratorConfig` — nueva propiedad `DatabaseEngine: SqlServer | PostgreSQL | Oracle`
- [ ] Tests de integración con Docker (postgres + oracle-xe)

**Archivos impactados:** `SqlServerSchemaReader.cs`, `GeneratorConfig.cs`, `GenerateCommand.cs`, `NamingConventionService.cs`  
**Nuevos archivos:** `PostgresSchemaReader.cs`, `OracleSchemaReader.cs`, `SchemaReaderFactory.cs`  
**Esfuerzo estimado:** 6-8 días

---

## ⏳ SEMANA 11 — Parámetros Configurables de Proyecto
> **Estado: PENDIENTE**

### Feature 2 — Wizard Interactivo de Configuración
- [ ] Comando `aigen new-project` con wizard paso a paso
- [ ] Parámetros solicitados: nombre de app, namespace raíz, autor, empresa, versión, año, descripción
- [ ] Propiedades en `GeneratorConfig`: `Author`, `Company`, `Version`, `Year`, `Description`
- [ ] Validación de namespace (sin espacios, sin caracteres especiales)
- [ ] Generación automática de `aigen.json` desde el wizard
- [ ] Todas las plantillas .scriban usan `{{ author }}`, `{{ company }}`, `{{ version }}`, `{{ year }}`
- [ ] `appsettings.scriban` con connection string interactivo
- [ ] Validaciones: namespace válido, versión semver, colores hex

**Archivos impactados:** `GeneratorConfig.cs`, `TemplateContext.cs`, todas las plantillas `.scriban`  
**Nuevos archivos:** CLI comando `new-project`  
**Esfuerzo estimado:** 2-3 días

---

## ⏳ SEMANA 12 — Microservicios + API Gateway
> **Estado: PENDIENTE**

### Feature 3 — Arquitectura de Microservicios
- [ ] Nueva opción en `aigen.json`: `architecture: "monolith" | "microservices"`
- [ ] Agrupación de tablas por dominio según prefijo (TM_ → DocumentosService, TP_ → ParametrosService)
- [ ] Un `.sln` + Clean Architecture por microservicio
- [ ] API Gateway con YARP — configuración de rutas auto-generada
- [ ] `docker-compose.yml` multi-servicio auto-generado
- [ ] Health checks en cada microservicio
- [ ] Shared kernel: entidades y DTOs compartidos entre servicios

**Nuevas plantillas:** `solution_microservice.scriban`, `gateway_yarp.scriban`, `docker_compose.scriban`  
**Archivos impactados:** `FileGeneratorService.cs` (modo microservicios), `GeneratorConfig.cs`  
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

| # | Feature | Semana | Impacto | Archivos clave |
|---|---------|--------|---------|----------------|
| 1 | Multi-schema: selección de schemas de BD | S10 | Alto | SqlServerSchemaReader, GeneratorConfig |
| 2 | Parámetros configurables (wizard) | S11 | Alto | GeneratorConfig, TemplateContext, todas las plantillas |
| 3 | Microservicios + API Gateway (YARP) | S12 | Alto | FileGeneratorService, plantillas solution_microservice, docker_compose |
| 4 | Soporte PostgreSQL y Oracle | S10 | Alto | PostgresSchemaReader, OracleSchemaReader, SchemaReaderFactory |
| 5 | Stored Procedures + script SQL descargable | S13 | Medio | StoredProcedureMetadata, repository_sp.scriban, sp_crud.scriban |
| 6 | IA integrada como asistente de desarrollo | S14 | Alto | Aigen.AI.csproj, Claude API, Qdrant RAG |
| 7 | Multi-frontend: Angular, React, Vue, Blazor | S15 | Alto | TemplateLocator, nuevas carpetas de plantillas |
| 8 | Paleta de colores configurable | S15 | Medio | theme.scriban, styles.scss generado |
| 9 | Logo / imagen de marca en el frontend | S15 | Medio | logoPath en config, assets/ generado |
| 10 | Docs auto-actualizados en cada generación | S7-S16 | Medio | architecture.scriban, api_catalog.scriban, db_dictionary.scriban |
| 11 | Documentación completa auto-generada | S16 | Alto | aigen generate-docs, exportación MD/HTML/PDF |

---

## 🐛 Backlog Técnico

| # | Archivo | Descripción | Impacto | Estado |
|---|---------|-------------|---------|--------|
| 1 | `controller.scriban` | Constraint `{id:string}` inválido | Bajo | ✅ Resuelto S8 |
| 2 | `repository_ef.scriban` | `ToggleEstado` con `EstadoPropertyName` | Bajo | ✅ Resuelto S7 |
| 3 | `aigen.json` | No soporta `excludedPrefixes` — listar tablas individualmente | Medio | S9 |
| 4 | `entity.scriban` | Columnas audit aparecen en DTO de lectura | Bajo | S10 |
| 5 | Angular templates | `ng build` no verificado — posibles errores TypeScript | Desconocido | S9 |
| 6 | `FileGeneratorService` | Servicios duplicados cuando `ClassNamePlural` coincide | Bajo | S9 |
| 7 | `program.scriban` | Swagger solo en Development — falta config Staging/Production | Bajo | S10 |

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
| IA con modelo base configurable | Claude primario, OpenAI fallback, Ollama offline — sin vendor lock-in |
| RAG con Qdrant para contexto de IA | Semántica sobre el código generado para respuestas precisas del asistente |
| Microservicios agrupados por prefijo | TM_ → un dominio, TP_ → otro dominio — mapeo natural del estándar Línea base de almacenamiento |
