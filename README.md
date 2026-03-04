# AIGEN - AI-Powered Software Generator

> From schema to production. Intelligently.

Generador de software empresarial que lee el schema de una base de datos SQL Server
y produce proyectos completos con Clean Architecture, microservicios, Angular 18 y .NET 8.
Incluye soporte para multiples motores de BD, ORM configurable, API Gateway, y capas de IA.

---

## Estado actual

| Semana | Descripcion | Estado |
|--------|-------------|--------|
| Semana 1 | CLI + GeneratorConfig + SqlServerSchemaReader + Metadata + Tests | Completada |
| Semana 2 | ConfigValidator + SchemaFilterService + NamingConventionService | Pendiente |
| Semana 3 | Motor de plantillas Scriban (Domain, Application, Infrastructure, API) | Pendiente |
| Semana 4 | Frontend Angular 18 con PrimeNG | Pendiente |
| Fase 3   | Integracion Claude API y Ollama | Pendiente |

---

## Inicio rapido

```bash
# 1. Clonar el repositorio
git clone https://github.com/creto/AIGEN.git
cd AIGEN/aigen

# 2. Restaurar dependencias y compilar
dotnet restore
dotnet build

# 3. Editar configuracion con tu connection string
#    Archivo: samples/proyecto-ejemplo.json

# 4. Ejecutar el generador
cd src/Aigen.CLI
dotnet run -- new --config ../../samples/proyecto-ejemplo.json
```

---

## Estructura del proyecto

```
aigen/
+-- aigen.sln                              <- Solucion con 5 proyectos
+-- git-manager.bat                        <- Gestor Git (22 operaciones)
+-- README.md
+-- .gitignore
|
+-- samples/
|   +-- proyecto-ejemplo.json             <- Config lista para editar
|
+-- src/
|   |
|   +-- Aigen.CLI/                        <- Proyecto ejecutable
|   |   +-- Aigen.CLI.csproj
|   |   +-- Program.cs                    <- Entry point + DI + TypeRegistrar
|   |   +-- Commands/
|   |   |   +-- NewCommand.cs             <- aigen new --config
|   |   +-- UI/
|   |       +-- Banner.cs                 <- UI colorida Spectre.Console
|   |
|   +-- Aigen.Core/                       <- Motor del generador
|   |   +-- Aigen.Core.csproj
|   |   +-- Config/
|   |   |   +-- GeneratorConfig.cs        <- Raiz del JSON de configuracion
|   |   |   +-- ProjectConfig.cs          <- Datos del proyecto
|   |   |   +-- DatabaseConfig.cs         <- Motor BD, connection string, schema
|   |   |   +-- ArchitectureConfig.cs     <- Microservices, Gateway YARP/Ocelot
|   |   |   +-- ConfigModels.cs           <- Backend, Frontend, Security, Features, AI, Output
|   |   |   +-- Enums/
|   |   |       +-- DatabaseEngine.cs     <- SqlServer, PostgreSQL, Oracle, MySQL, SQLite
|   |   |       +-- ArchitecturePattern.cs <- CleanArchitecture, LayeredNTier, VerticalSlice
|   |   |       +-- OrmType.cs            <- EFCore, Dapper, EFCoreWithDapper
|   |   |       +-- FrontendFramework.cs  <- Angular, React, Vue, Blazor
|   |   |       +-- AuthenticationType.cs <- Jwt, Identity, Keycloak, AzureAD
|   |   |       +-- FeatureEnums.cs       <- Logging, Validation, Cache, CICD, AI...
|   |   +-- Schema/
|   |   |   +-- ISchemaReader.cs          <- Interfaz: ReadAsync + TestConnectionAsync
|   |   |   +-- SqlServerSchemaReader.cs  <- Lee INFORMATION_SCHEMA + sys.*
|   |   +-- Metadata/
|   |       +-- DatabaseMetadata.cs       <- Raiz con estadisticas totales
|   |       +-- TableMetadata.cs          <- Clase, nombre, rutas API, helpers Scriban
|   |       +-- ColumnMetadata.cs         <- Tipos SQL/C#/TS, audit, PK, identity
|   |       +-- ForeignKeyMetadata.cs     <- FKs + IndexMetadata
|   |
|   +-- Aigen.Templates/                  <- Motor Scriban (Semana 3)
|   |   +-- Aigen.Templates.csproj
|   |   +-- Placeholder.cs
|   |
|   +-- Aigen.AI/                         <- Claude API + Ollama (Fase 3)
|       +-- Aigen.AI.csproj
|       +-- Placeholder.cs
|
+-- tests/
    +-- Aigen.Tests/
        +-- Aigen.Tests.csproj
        +-- Schema/
            +-- SqlServerSchemaReaderTests.cs   <- 7 tests (3 sin BD + 4 con BD real)
```

---

## Comandos CLI disponibles

```bash
# Ver ayuda general
dotnet run -- --help

# Ver ayuda del comando new
dotnet run -- new --help

# Generar proyecto desde JSON de configuracion
dotnet run -- new --config samples/proyecto-ejemplo.json
dotnet run -- new -c samples/proyecto-ejemplo.json

# Comandos proximas versiones
dotnet run -- generate    # Semana 3 - genera capas de codigo
dotnet run -- preview     # Semana 3 - previsualiza sin escribir archivos
dotnet run -- assist      # Fase 3   - asistente con IA
```

---

## Flujo del comando aigen new

```
[1] Lee y deserializa GeneratorConfig desde el JSON
[2] Muestra tabla resumen de configuracion (Spectre.Console)
[3] Prueba la conexion a SQL Server (TestConnectionAsync)
[4] Lee el schema completo de la BD:
    - INFORMATION_SCHEMA.TABLES     -> lista de tablas
    - INFORMATION_SCHEMA.COLUMNS    -> columnas con tipos
    - KEY_COLUMN_USAGE              -> primary keys
    - sys.foreign_keys              -> foreign keys con navegacion
    - sys.indexes                   -> indices
[5] Filtra las tablas en ExcludedTables
[6] Selector interactivo MultiSelectionPrompt (SPACE + ENTER)
[7] Muestra resumen DatabaseMetadata
[8] Confirmacion del usuario
[9] [Semana 3] Genera codigo via plantillas Scriban
```

---

## Configuracion completa (proyecto-ejemplo.json)

```json
{
  "aigenVersion": "1.0",
  "project": {
    "projectName": "SistemaVentas",
    "rootNamespace": "Com.MiEmpresa.SistemaVentas",
    "version": "1.0.0",
    "author": "Oscar Cortes",
    "language": "Spanish"
  },
  "database": {
    "engine": "SqlServer",
    "connectionString": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;",
    "schema": "dbo",
    "excludedTables": ["__EFMigrationsHistory", "sysdiagrams"],
    "efStrategy": "DatabaseFirst"
  },
  "architecture": {
    "style": "Microservices",
    "pattern": "CleanArchitecture",
    "groupingStrategy": "Manual",
    "microservices": [
      { "name": "Clientes", "port": 5001, "tables": ["TM_Cliente"] },
      { "name": "Ventas",   "port": 5002, "tables": ["TM_Pedido", "TM_Factura"] }
    ],
    "gateway": { "technology": "YARP", "port": 5000 }
  },
  "backend":  { "orm": "EFCoreWithDapper", "repositoryPattern": "RepositoryUnitOfWork" },
  "frontend": { "framework": "Angular", "frameworkVersion": "18", "uiLibrary": "PrimeNG", "stateManagement": "Signals" },
  "security": { "authentication": "Jwt", "authorization": "Roles", "corsOrigins": "http://localhost:4200" },
  "features": { "logging": "Serilog", "apiDoc": "Swagger", "validation": "FluentValidation", "mapping": "AutoMapper",
                "generatePagination": true, "softDelete": true, "auditing": true, "generateDockerfile": true },
  "ai":       { "provider": "None", "model": "claude-sonnet-4-5" },
  "output":   { "type": "LocalPath", "path": "C:\\Proyectos\\{ProjectName}", "createGitRepo": true }
}
```

---

## Secciones de configuracion

| Seccion | Descripcion | Opciones clave |
|---------|-------------|----------------|
| project | Datos del proyecto | nombre, namespace, version, autor |
| database | Conexion y schema | SqlServer, PostgreSQL, Oracle, MySQL, SQLite |
| architecture | Patron arquitectural | Microservices, MonolithModular, CleanArchitecture |
| backend | ORM y patrones | EFCore, Dapper, EFCoreWithDapper, CQRS |
| frontend | Framework UI | Angular 18, React, Vue, Blazor |
| security | Autenticacion | Jwt, Keycloak, AzureAD, Identity |
| features | Caracteristicas | Serilog, Swagger, FluentValidation, Redis, Docker |
| ai | Proveedor IA | Claude, OpenAI, Ollama, None |
| output | Destino del codigo | LocalPath, GitHub, AzureDevOps |

---

## SqlServerSchemaReader

Lee el schema completo de SQL Server usando queries directas contra:

| Query | Informacion obtenida |
|-------|---------------------|
| INFORMATION_SCHEMA.TABLES | Lista de tablas del schema |
| INFORMATION_SCHEMA.COLUMNS | Columnas, tipos, nullable, identity |
| KEY_COLUMN_USAGE + TABLE_CONSTRAINTS | Primary keys |
| sys.foreign_keys + sys.foreign_key_columns | Foreign keys con navegacion |
| sys.indexes + sys.index_columns | Indices y columnas indexadas |

**Limpieza automatica de prefijos:** TM_, TC_, TS_, TR_, TD_, TB_, TG_

Ejemplo: TM_Factura -> clase Factura, servicio FacturaService, ruta /api/facturas

**Campos de auditoria marcados automaticamente (IsAuditField = true):**
_ippublica, _nombremaquina, _usuario, _browser, _sessionid, _xmlauditoria,
createdAt, createdBy, updatedAt, updatedBy, eliminado, deletedAt

**Mapeo de tipos:**

| SQL Server | C# | TypeScript |
|---|---|---|
| int / bigint / smallint | int / long / short | number |
| bit | bool | boolean |
| decimal / money | decimal | number |
| datetime / datetime2 | DateTime | Date |
| date | DateOnly | Date |
| varchar / nvarchar | string | string |
| uniqueidentifier | Guid | string |
| varbinary | byte[] | string |

---

## Stack tecnologico

| Capa | Tecnologia | Version |
|------|------------|---------|
| CLI | Spectre.Console | 0.49.1 |
| Backend | .NET + C# | 8.0 |
| Patron | Clean Architecture | - |
| ORM | EF Core + Dapper | hibrido |
| Frontend | Angular + PrimeNG + Signals | 18 |
| Auth | JWT / Keycloak | - |
| Logging | Serilog | - |
| Validacion | FluentValidation | - |
| Mapeo | AutoMapper | - |
| Templates | Scriban | Semana 3 |
| IA | Claude API / Ollama | Fase 3 |
| Gateway | YARP / Ocelot | - |
| Tests | xUnit + Moq + FluentAssertions | - |
| BD soportadas | SqlServer, PostgreSQL, Oracle, MySQL, SQLite | - |

---

## Paquetes NuGet

**Aigen.CLI**
```
Spectre.Console                          0.49.1
Spectre.Console.Cli                      0.49.1
Microsoft.Extensions.DependencyInjection 8.0.1
Microsoft.Extensions.Configuration.Json 8.0.1
```

**Aigen.Core**
```
Microsoft.Data.SqlClient                 5.2.2
```

**Aigen.Tests**
```
xunit                                    2.9.0
xunit.runner.visualstudio                2.8.2
FluentAssertions                         6.12.0
Moq                                      4.20.72
Microsoft.NET.Test.Sdk                   17.11.1
```

---

## Tests unitarios

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar con detalle
dotnet test --verbosity normal

# Ejecutar solo tests sin BD (los 3 que pasan sin SQL Server)
dotnet test --filter "Category!=Integration"
```

| Test | Requiere BD | Descripcion |
|------|-------------|-------------|
| Implements_ISchemaReader | No | Verifica implementacion de interfaz |
| WithInvalidConnection_ReturnsFalse | No | Conexion invalida retorna false |
| WithEmpty_ReturnsFalse | No | Cadena vacia retorna false |
| ISchemaReader_Mock_Works | No | Mock funciona para unit tests |
| WithValidConnection_ReturnsTrue | Si | Conexion valida retorna true |
| ReadAsync_ReturnsMetadata | Si | Lee tablas y columnas correctamente |
| TM_Tables_HaveCleanClassName | Si | Prefijos TM_ se limpian del ClassName |

---

## Gestor Git (git-manager.bat)

Ejecutar desde la carpeta raiz del proyecto:

```cmd
git-manager.bat
```

| Opcion | Comando Git equivalente | Descripcion |
|--------|------------------------|-------------|
| [1] Commit + Push | git add . && git commit && git push | Guardar y subir |
| [2] Push | git push origin main | Subir commits pendientes |
| [3] Stash | git stash push | Guardar sin commitear |
| [4] Stash Pop | git stash pop | Recuperar stash |
| [5] Pull | git pull origin main | Traer y mergear |
| [6] Fetch | git fetch origin | Ver cambios sin aplicar |
| [7] Pull Rebase | git pull --rebase | Traer rebasando |
| [8] Ver ramas | git branch -a | Locales y remotas |
| [9] Crear rama | git checkout -b | Crea y opcionalmente sube |
| [10] Cambiar rama | git checkout | Cambiar de rama |
| [11] Merge a main | git merge | Fusionar rama a main |
| [12] Eliminar rama | git branch -d | Local y/o remota |
| [13] Status | git status | Estado actual |
| [14] Log compacto | git log --oneline --graph | Historial visual |
| [15] Log detallado | git log --format=... | Con fecha y autor |
| [16] Diff | git diff --stat | Cambios pendientes |
| [17] Undo ultimo commit | git reset --soft HEAD~1 | Conserva archivos |
| [18] Undo archivo | git checkout -- archivo | Restaurar un archivo |
| [19] Reset hard | git reset --hard HEAD | Descarta todo (pide SI) |
| [20] Ver tags | git tag -l | Lista de versiones |
| [21] Crear tag | git tag -a v1.0.0 | Tag con descripcion |
| [22] Push tags | git push origin --tags | Subir todos los tags |

---

## Compilar y ejecutar

```bash
# Desde la raiz del proyecto (carpeta aigen/)

# Restaurar paquetes NuGet
dotnet restore

# Compilar toda la solucion
dotnet build

# Compilar en Release
dotnet build -c Release

# Ejecutar tests
dotnet test

# Ejecutar el CLI
cd src/Aigen.CLI
dotnet run -- --help
dotnet run -- new --help
dotnet run -- new --config ../../samples/proyecto-ejemplo.json

# Publicar ejecutable standalone
dotnet publish src/Aigen.CLI -c Release -r win-x64 --self-contained
```

---

## Error conocido y solucion

**Error de compilacion en SqlServerSchemaReader.cs:**
```
error CS8864: Records may only inherit from object or another record
```

**Causa:** La clase interna RawColumn fue declarada como record en lugar de class.

**Solucion:** En src/Aigen.Core/Schema/SqlServerSchemaReader.cs, ultima seccion:
```csharp
// Cambiar esto:
private record RawColumn : ColumnMetadata { ... }

// Por esto:
private class RawColumn : ColumnMetadata { ... }
```

---

## Roadmap

**Semana 1 - Completada**
- CLI con Spectre.Console (aigen new, generate, preview, assist)
- GeneratorConfig con 10 secciones configurables via JSON
- 30+ enums tipados para todos los parametros
- SqlServerSchemaReader (INFORMATION_SCHEMA + sys.*)
- DatabaseMetadata, TableMetadata, ColumnMetadata, ForeignKeyMetadata
- Tests unitarios con xUnit + Moq + FluentAssertions
- Selector interactivo de tablas
- git-manager.bat con 22 operaciones Git

**Semana 2 - Pendiente**
- ConfigValidator.cs (validar JSON obligatorios y coherencia)
- SchemaFilterService.cs (aplicar IncludedTables / ExcludedTables)
- NamingConventionService.cs (PascalCase, camelCase, plurales ES/EN)
- PostgreSqlSchemaReader.cs
- Tests de integracion con BD de prueba

**Semana 3 - Pendiente**
- Motor de plantillas Scriban
- Plantillas: Entity, DbContext, Repository, Service, Controller, DTO
- Plantillas frontend Angular: service, component, model
- TemplateEngine con contexto DatabaseMetadata
- FileGeneratorService (escribe archivos en disco)

**Semana 4 - Pendiente**
- Frontend Angular 18 con PrimeNG
- Wizard de configuracion paso a paso
- Preview en tiempo real del codigo generado
- Descarga del proyecto generado como ZIP

**Fase 3 - Pendiente**
- Integracion Claude API (claude-sonnet-4-5)
- Integracion Ollama (modelos locales)
- EnhanceEntities: mejora descripciones y propiedades
- InferBusinessRules: infiere validaciones desde el schema
- GenerateTests: genera tests unitarios automaticamente

---

## Repositorio

https://github.com/creto/AIGEN

---



## SEMANA 1

# AIGEN - AI-Powered Software Generator

> From schema to production. Intelligently.

Generador de software empresarial que lee el schema de SQL Server y produce
proyectos completos con Clean Architecture, microservicios, Angular 18 y .NET 8.

## Inicio rapido

    dotnet restore
    dotnet build
    cd src/Aigen.CLI
    dotnet run -- new --config ../../samples/proyecto-ejemplo.json

## Estructura

    aigen/
    ├── aigen.sln
    ├── samples/proyecto-ejemplo.json
    ├── src/
    │   ├── Aigen.CLI/
    │   ├── Aigen.Core/
    │   ├── Aigen.Templates/
    │   └── Aigen.AI/
    └── tests/Aigen.Tests/

## Stack tecnologico

| Capa       | Tecnologia                      |
|------------|---------------------------------|
| CLI        | Spectre.Console 0.49            |
| Backend    | .NET 8 + Clean Architecture     |
| ORM        | EF Core + Dapper                |
| Frontend   | Angular 18 + PrimeNG + Signals  |
| Auth       | JWT / Keycloak                  |
| Logging    | Serilog                         |
| Templates  | Scriban (Semana 3)              |
| IA         | Claude API / Ollama (Fase 3)    |
| Tests      | xUnit + Moq + FluentAssertions  |
| Gateway    | YARP                            |

## Comandos

| Comando                              | Estado     |
|--------------------------------------|------------|
| aigen new --config archivo.json      | Semana 1   |
| aigen generate                       | Semana 3   |
| aigen preview                        | Semana 3   |
| aigen assist                         | Fase 3     |

## Roadmap

- [x] Semana 1 - CLI + GeneratorConfig + SqlServerSchemaReader
- [ ] Semana 2 - Validaciones + tests de integracion
- [ ] Semana 3 - Plantillas Scriban
- [ ] Semana 4 - Frontend Angular 18
- [ ] Fase 3   - Integracion Claude API y Ollama

## Metas logradas semana 1
Mira lo que logró el generador en Semana 1:

✅ Conectó a SQL Server en 200.31.22.7,1437
✅ Leyó 287 tablas, 3018 columnas, 311 relaciones FK
✅ Filtró y mostró 7 tablas seleccionadas
✅ Limpió prefijos automáticamente: TB_Bodega → Bodega, TM_Documento → Documento, TP_Ciudad → TpCiudad
✅ Detectó PKs y FKs correctamente
✅ Pregunta confirmación antes de generar

## metas pasadsa semana 2
..
....




## Autor

Oscar Mauricio Cortes Pinzon
AIGEN v1.0.0 - 2025
