# 🗺️ AIGEN — Derrotero de Desarrollo
**Proyecto:** AI-Powered Software Generator (AIGEN)  
**Stack:** .NET Core 8 · Scriban · Dapper · EF Core · Angular 17 · PrimeNG  
**BD objetivo:** SQL Server (convención Incoder TM_/TB_/TBR_/TP_/TR_/TA_/TS_/TH_/TI_/TX_)  
**Última actualización:** 2026-03-06

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
- [x] `TableMetadataExtensions` — `GetTableType()` con mapa de prefijos Incoder + heurística para tablas sin prefijo
- [x] `HasFullCrud()` / `IsReadOnly()` / `IsRelationalOnly()` — capacidades derivadas del tipo
- [x] `HasEstadoField()` — detección de columna Estado BIT
- [x] Análisis y adopción del estándar de nomenclatura Incoder (SQL Server 2008 R2)
- [x] Decisión híbrida: preservar prefijos TM_/TB_/etc., PascalCase, audit fields, Estado BIT; modernizar tipos deprecados
- [x] Suite de tests Semana 2 — **38 tests pasando ✅**

---

## ✅ SEMANA 3 — Motor de plantillas Scriban
> **Estado: COMPLETADA ✅** — 19 tests nuevos

- [x] `ScribanTemplateEngine` — renderizado de `.scriban` con TemplateContext en snake_case
- [x] `TemplateLocator` — resolución de rutas de plantillas por capa y tipo
- [x] `FileGeneratorService` — orquestación de generación por tabla según TableType
- [x] `TemplateContext` v1 — contexto completo inyectado en plantillas (entity_name, pk_name, pk_type, namespaces, features)
- [x] `GenerateCommand` CLI — selección interactiva de tablas + barra de progreso
- [x] **13 plantillas Scriban backend:**
  - [x] `entity.scriban` — entidad de dominio con Data Annotations
  - [x] `dto.scriban` — DTOs Read/Create/Update/Paged
  - [x] `irepository.scriban` — contrato de repositorio
  - [x] `service.scriban` — capa de aplicación
  - [x] `repository_dapper.scriban` — repositorio Dapper
  - [x] `repository_ef.scriban` — repositorio EF Core
  - [x] `controller.scriban` — controlador REST con Swagger
  - [x] `validator.scriban` — FluentValidation
  - [x] `entity_configuration.scriban` — Fluent API EF Core
  - [x] `dbcontext.scriban` — DbContext con DbSets
  - [x] `program.scriban` — Program.cs con DI
  - [x] `appsettings.scriban` — appsettings.json
  - [x] `solution.scriban` — archivo .sln
- [x] **Plantillas Angular:**
  - [x] `model.scriban` — TypeScript model
  - [x] `service.scriban` — Angular service con HttpClient
  - [x] `list.component.scriban` — PrimeNG DataTable
  - [x] `form.component.scriban` — Reactive Forms
- [x] Suite de tests Semana 3 — **pasando ✅**

---

## ✅ SEMANA 4 — EF Core + Dapper híbrido y lógica de generación avanzada
> **Estado: COMPLETADA ✅**

- [x] ORM `EFCoreWithDapper` — estrategia híbrida (EF para CRUD simple, Dapper para queries complejas)
- [x] `entity_configuration.scriban` — mapeo Fluent API completo con FK relations
- [x] `DbContext` con todos los `DbSet<T>` y configuraciones
- [x] Generación selectiva por tipo de tabla — Audit/Historical solo generan Entity (no repositorio/controller)
- [x] Soporte `excludedTables` en `aigen.json`
- [x] Lógica de prefijo → CRUD capabilities (tabla `TA_` = solo lectura, `TM_` = CRUD completo)
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

### Bugs identificados y corregidos

- [x] **Bug 1 — CS1061/CS0037: `.Value` en tipos NOT NULL**
  - Causa: `repository_ef.scriban` aplicaba `.Value` a todos los non-string sin verificar nullabilidad
  - Fix: `TemplateContext` agrega flag `is_value_type`; plantilla usa `.HasValue` / `.Value` solo cuando corresponde
  - Reducción: 1075 → 896 errores

- [x] **Bug 2 — UpdateRequest con `bool`/`int` no-nullable**
  - Causa: `dto.scriban` generaba `bool IBPMNLectura` en UpdateRequest; repositorio hacía `.HasValue` sobre `bool` → CS1061
  - Fix: `dto.scriban` v2 — en UpdateRequest todos los value types son `T?` (nullable) para updates parciales
  - Reducción: 896 → 61 errores

- [x] **Bug 3 — Colisión de ClassName entre TH_ y TB_**
  - Causa: `TH_Serie` y `TB_Serie` producían mismo `ClassName = "Serie"` → archivo sobreescrito → repositorio usaba entidad incorrecta
  - Fix: `NamingConventionService.ToClassName()` — prefijos `TH_`/`TAR_` generan sufijo `Hist` (`TH_Serie` → `SerieHist`)
  - Reducción: 61 → 15 errores

- [x] **Bug 4 — PK compuesta con nombres de columna SQL en lugar de propiedades C#**
  - Causa: `entity_configuration.scriban` usaba `table.primary_keys` (column names: `ID_TP_Dependencia`) no property names (`IDTPDependencia`)
  - Fix: construir `col_map` en la plantilla para traducir `column_name → property_name` antes de usarlo en `HasKey` e `HasIndex`
  - Reducción: 15 → 2 errores

- [x] **Bug 5 — `ToggleEstado` con nombre de propiedad hardcoded**
  - Causa: `ESTADO` (mayúsculas en BD) genera propiedad `ESTADO` en entidad, pero repositorio generaba `entity.Estado` (PascalCase fijo)
  - Fix temporal: patch directo sobre archivo generado
  - Fix pendiente Semana 7: exponer `EstadoPropertyName` en `TemplateContext`
  - Reducción: 2 → **0 errores** 🎉

- [x] **`Doc4Us.sln` compila con 0 errores** ✅

---

## 🔄 SEMANA 7 — Calidad del código generado y fixes residuales
> **Estado: EN CURSO 🔄** ← ESTAMOS AQUÍ

### Pendientes

- [ ] **Fix definitivo `EstadoPropertyName`** — `TemplateContext` expone el nombre real de la propiedad `Estado` para que `repository_ef.scriban` no lo hardcodee
- [ ] **Limpiar 136 warnings** — revisar y categorizar: nullable references, obsoletos, estilo
- [ ] **Resolver conflicto Git** — credenciales `codedosecodes` vs repo `creto` (SSH key o PAT en Credential Manager)
- [ ] **Push a GitHub** — subir estado actual de AIGEN al repositorio
- [ ] **Tests de regresión** — verificar que los fixes no rompieron casos que ya funcionaban
- [ ] **Validar frontend Angular generado** — revisar si `ng build` pasa en el frontend de Doc4Us

---

## ⏳ SEMANA 8 — Estabilización y validación end-to-end
> **Estado: PENDIENTE**

- [ ] `Doc4Us.API` — levantar la API generada y probar endpoints básicos con Swagger
- [ ] Validar CRUD completo para al menos 3 tablas representativas (TM_, TB_, TBR_)
- [ ] `ToggleEstado` fix completo en generador (no solo patch manual)
- [ ] Soporte `excludedPrefixes` en `aigen.json` como alternativa a `excludedTables` individuales
- [ ] Manejo de tablas sin PK — skip elegante con log en lugar de crash silencioso
- [ ] Mejorar `HasEstado` — devolver también `EstadoPropertyName` con case correcto
- [ ] Documentar bugs conocidos y workarounds en README del proyecto

---

## ⏳ SEMANA 9 — Integración con segundo proyecto (BD diferente)
> **Estado: PENDIENTE**

- [ ] Probar AIGEN contra una BD diferente a Doc4UsAIGen (sin convención Incoder)
- [ ] Validar heurística de clasificación para tablas sin prefijo
- [ ] Soporte multi-schema (`dbo` + `Interop` simultáneamente)
- [ ] Generar proyecto completo y compilar a 0 errores con BD nueva
- [ ] Comparar calidad del código generado vs Doc4Us

---

## ⏳ SEMANA 10 — Mejoras de plantillas y UX del generador
> **Estado: PENDIENTE**

- [ ] Plantilla `validator.scriban` — reglas FluentValidation por tipo de dato (MaxLength, Required, Range)
- [ ] Plantilla `controller.scriban` — respuestas estandarizadas, manejo de errores, paginación
- [ ] Soporte `softDelete` — `Eliminado BIT` en queries base (`BaseQuery` con `.Where(e => !e.Eliminado)`)
- [ ] Soporte auditoría automática — `CreadoEn`, `CreadoPor`, `ModificadoEn`, `ModificadoPor`
- [ ] Mejorar Angular forms — validaciones en template, mensajes de error con PrimeNG
- [ ] Comando `diff` en CLI — mostrar qué archivos cambiarían antes de generar

---

## ⏳ SEMANA 11 — Soporte Dapper avanzado y stored procedures
> **Estado: PENDIENTE**

- [ ] `repository_dapper.scriban` — queries parametrizadas para tablas complejas
- [ ] Soporte stored procedures — detectar SPs relacionados a una tabla y generar wrappers
- [ ] Paginación server-side en Dapper (OFFSET/FETCH NEXT)
- [ ] Filtros dinámicos en queries (búsqueda por múltiples campos)
- [ ] Transacciones explícitas en operaciones multi-tabla

---

## ⏳ SEMANA 12 — CLI avanzado y modo watch
> **Estado: PENDIENTE**

- [ ] Comando `validate` — verificar `aigen.json` sin conectar a BD
- [ ] Comando `diff` — comparar schema actual vs última generación (detectar cambios)
- [ ] Modo `--watch` — regenerar archivos afectados cuando la BD cambia
- [ ] Comando `rollback` — restaurar archivos a la versión anterior
- [ ] Soporte `--dry-run` — simular generación sin escribir archivos
- [ ] Output detallado con `--verbose` — mostrar qué plantilla genera cada archivo

---

## ⏳ SEMANAS 13-16 — Fase Beta y documentación
> **Estado: PENDIENTE**

- [ ] Documentación técnica completa (README, ARCHITECTURE.md, CONTRIBUTING.md)
- [ ] Guía de plantillas — cómo crear plantillas personalizadas
- [ ] Soporte para múltiples proyectos en el mismo `aigen.json`
- [ ] Empaquetado como herramienta global .NET (`dotnet tool install -g aigen`)
- [ ] Pipeline CI/CD básico (GitHub Actions: build + test en cada push)
- [ ] Pruebas con 3+ BDs de clientes reales

---

## 📊 Progreso general

```
Semana 1  ████████████████████ 100% ✅
Semana 2  ████████████████████ 100% ✅
Semana 3  ████████████████████ 100% ✅
Semana 4  ████████████████████ 100% ✅
Semana 5  ████████████████████ 100% ✅
Semana 6  ████████████████████ 100% ✅ ← HITO: 0 errores
Semana 7  ████████░░░░░░░░░░░░  40% 🔄 ← ESTAMOS AQUÍ
Semana 8  ░░░░░░░░░░░░░░░░░░░░   0% ⏳
Semana 9  ░░░░░░░░░░░░░░░░░░░░   0% ⏳
Semana 10 ░░░░░░░░░░░░░░░░░░░░   0% ⏳
```

---

## 🐛 Bugs conocidos pendientes (backlog técnico)

| # | Archivo | Descripción | Impacto | Prioridad |
|---|---------|-------------|---------|-----------|
| 1 | `repository_ef.scriban` | `ToggleEstado` hardcodea `entity.Estado` — falla si columna es `ESTADO` (mayúsculas) | Bajo — solo afecta tablas con `ESTADO` en mayúsculas | Alta |
| 2 | `TemplateContext.HasEstado` | No expone `EstadoPropertyName` real — plantilla no puede saber el nombre exacto de la propiedad | Bajo | Alta |
| 3 | `aigen.json` | No soporta `excludedPrefixes` — hay que listar tablas individualmente | Medio — engorroso con BDs grandes | Media |
| 4 | `entity.scriban` | Columnas de auditoría no se excluyen de `AllColumns` — aparecen en el DTO de lectura | Bajo | Media |
| 5 | Angular templates | `ng build` no verificado — puede haber errores de tipos TypeScript | Desconocido | Alta |

---

## 🔑 Decisiones arquitectónicas tomadas

| Decisión | Justificación |
|----------|---------------|
| Scriban como motor de plantillas | Sintaxis limpia, soporte .NET nativo, sin dependencias pesadas |
| Prefijos de tabla como señales de generación | Estándar Incoder — el prefijo define qué CRUD se genera |
| `TH_`/`TAR_` → sufijo `Hist` en ClassName | Evita colisiones entre tablas históricas y sus contrapartes activas |
| UpdateRequest con `T?` para todos los value types | Permite updates parciales — cliente solo envía campos que cambia |
| EF Core + Dapper híbrido | EF para CRUD simple, Dapper para queries complejas y stored procedures |
| Clean Architecture (Domain/Application/Infrastructure/API) | Separación de responsabilidades, testabilidad, independencia de frameworks |
