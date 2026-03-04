# Línea Base — Estándar de Modelado de Bases de Datos

**Versión:** 1.0  
**Fecha:** 2025  
**Aplica a:** Modelos nuevos únicamente. Los modelos existentes no se migran.  
**Motores soportados:** SQL Server 2019+, PostgreSQL 14+, MySQL 8+, SQLite 3.35+

---

## 1. Principio General

> Todo modelo de datos nuevo debe ser comprensible sin documentación adicional.  
> El nombre de cada objeto debe revelar su propósito, tipo y alcance.

---

## 2. Nomenclatura de Tablas

### 2.1 Prefijos obligatorios

Cada tabla lleva un prefijo que comunica su naturaleza funcional. AIGEN usa estos prefijos para decidir qué operaciones CRUD generar automáticamente.

| Prefijo | Tipo | Descripción | CRUD generado |
|---------|------|-------------|---------------|
| `TM_` | Movimiento | Entidades transaccionales. Registros con ciclo de vida activo. | Completo |
| `TB_` | Básica | Catálogos sin FK padre. Solo ID, Nombre, Descripción, Estado. | Completo |
| `TBR_` | Básica relacionada | Catálogos con FK a tabla padre. | Completo |
| `TP_` | Parametrización | Configuraciones complejas con múltiples relaciones. | Completo + Toggle Estado |
| `TR_` | Relacional | Tablas N:M sin atributos propios. PK compuesta. | Insert / Delete / Select |
| `TC_` | Composición | Relación padre-hijo donde el hijo no existe sin el padre. | Completo |
| `TA_` | Auditoría | Registros de cambios. Solo lectura desde la aplicación. | Solo lectura |
| `TS_` | Sistema | Datos globales compartidos entre módulos. | Solo lectura en UI |
| `TH_` | Histórico | Snapshots de estado en un punto del tiempo. | Solo lectura |
| `TI_` | Imagen / Binario | Almacenamiento de archivos 1:1 con entidad padre. | Gestión binarios |
| `TX_` | Diccionario | Tablas de búsqueda fulltext. | Solo lectura |

### 2.2 Reglas de nombre

```
{PREFIJO}_{NombreNegocio}

✓ TM_OrdenCompra
✓ TB_TipoDocumento
✓ TBR_MunicipioDepto
✗ Orden           ← sin prefijo
✗ TM_ord_compra   ← snake_case en el nombre
✗ TM_ORDENCOMPRA  ← todo mayúsculas
```

- **PascalCase** después del prefijo, sin guiones bajos adicionales.
- **Español** para proyectos internos, **Inglés** para proyectos con equipos internacionales.
- **Singular** siempre: `TM_Factura`, no `TM_Facturas`.
- Máximo **40 caracteres** en nombre completo.

---

## 3. Estructura Base Obligatoria

Toda tabla nueva **debe** tener estas columnas en este orden:

```sql
-- SQL Server
CREATE TABLE TM_Ejemplo (
    ID              INT             IDENTITY(1,1) NOT NULL,
    -- columnas de negocio --
    Estado          BIT             NOT NULL DEFAULT 1,
    Eliminado       BIT             NOT NULL DEFAULT 0,
    CreadoEn        DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor       VARCHAR(100)    NOT NULL DEFAULT SYSTEM_USER,
    ModificadoEn    DATETIME2(0)    NULL,
    ModificadoPor   VARCHAR(100)    NULL,

    CONSTRAINT PK_TM_Ejemplo PRIMARY KEY CLUSTERED (ID)
);
```

```sql
-- PostgreSQL
CREATE TABLE tm_ejemplo (
    id              SERIAL          NOT NULL,
    -- columnas de negocio --
    estado          BOOLEAN         NOT NULL DEFAULT TRUE,
    eliminado       BOOLEAN         NOT NULL DEFAULT FALSE,
    creado_en       TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    creado_por      VARCHAR(100)    NOT NULL DEFAULT CURRENT_USER,
    modificado_en   TIMESTAMPTZ     NULL,
    modificado_por  VARCHAR(100)    NULL,

    CONSTRAINT pk_tm_ejemplo PRIMARY KEY (id)
);
```

### 3.1 Descripción de columnas estructurales

| Columna | Tipo SQL Server | Tipo PostgreSQL | Propósito |
|---------|----------------|-----------------|-----------|
| `ID` | `INT IDENTITY(1,1)` | `SERIAL` | PK autoincremental. Preferir sobre GUID. |
| `Estado` | `BIT NOT NULL DEFAULT 1` | `BOOLEAN NOT NULL DEFAULT TRUE` | Activo/Inactivo. `1 = activo`. |
| `Eliminado` | `BIT NOT NULL DEFAULT 0` | `BOOLEAN NOT NULL DEFAULT FALSE` | Soft delete. `1 = eliminado`. |
| `CreadoEn` | `DATETIME2(0)` | `TIMESTAMPTZ` | Timestamp UTC de creación. |
| `CreadoPor` | `VARCHAR(100)` | `VARCHAR(100)` | Usuario que creó el registro. |
| `ModificadoEn` | `DATETIME2(0) NULL` | `TIMESTAMPTZ NULL` | Timestamp UTC de última modificación. |
| `ModificadoPor` | `VARCHAR(100) NULL` | `VARCHAR(100) NULL` | Usuario que modificó por última vez. |

### 3.2 Excepciones permitidas

| Tabla | Columnas que puede omitir | Razón |
|-------|--------------------------|-------|
| `TR_` Relacional | `Estado`, `Eliminado` | La relación existe o no existe. |
| `TA_` Auditoría | `Estado`, `Eliminado`, `ModificadoEn/Por` | Es inmutable por diseño. |
| `TH_` Histórico | `Estado`, `Eliminado` | Los históricos no se modifican. |

---

## 4. Tipos de Dato

### 4.1 Catálogo de tipos aprobados

| Propósito | SQL Server | PostgreSQL | Evitar |
|-----------|-----------|------------|--------|
| PK entera | `INT IDENTITY` | `SERIAL` / `BIGSERIAL` | `UNIQUEIDENTIFIER` como PK |
| Entero pequeño | `TINYINT`, `SMALLINT` | `SMALLINT` | — |
| Entero estándar | `INT` | `INTEGER` | — |
| Entero grande | `BIGINT` | `BIGINT` | — |
| Dinero / Decimal | `DECIMAL(18,4)` | `NUMERIC(18,4)` | `MONEY`, `FLOAT`, `REAL` |
| Texto corto | `NVARCHAR(n)` | `VARCHAR(n)` | `TEXT` para campos cortos |
| Texto largo | `NVARCHAR(MAX)` | `TEXT` | `NTEXT` (deprecado) |
| Booleano | `BIT` | `BOOLEAN` | `INT` para flags |
| Fecha + hora | `DATETIME2(0)` | `TIMESTAMPTZ` | `DATETIME`, `SMALLDATETIME` |
| Solo fecha | `DATE` | `DATE` | — |
| Solo hora | `TIME(0)` | `TIME` | — |
| JSON | `NVARCHAR(MAX)` + check | `JSONB` | `XML` para JSON |
| Binario / Archivo | `VARBINARY(MAX)` | `BYTEA` | `IMAGE` (deprecado) |
| UUID / Sync | `UNIQUEIDENTIFIER` | `UUID` | Como PK principal |

### 4.2 Reglas de precisión

```sql
-- Dinero: SIEMPRE decimal con escala 4
Precio      DECIMAL(18, 4)  -- hasta 99,999,999,999,999.9999
Porcentaje  DECIMAL(7,  4)  -- hasta 999.9999

-- Fechas: SIEMPRE UTC en la BD, conversión timezone en la capa de aplicación
CreadoEn    DATETIME2(0)    DEFAULT GETUTCDATE()  -- sin microsegundos
EventoEn    DATETIME2(7)    -- cuando se requiere precisión máxima

-- Texto: definir longitud máxima realista, no usar MAX por defecto
Nombre      NVARCHAR(150)   -- no NVARCHAR(MAX)
Email       NVARCHAR(254)   -- RFC 5321
Descripcion NVARCHAR(500)   -- cuando es corta
Contenido   NVARCHAR(MAX)   -- solo cuando genuinamente ilimitado
```

---

## 5. Nomenclatura de Columnas

```
PascalCase. Sin prefijos. Español o Inglés consistente con el proyecto.

✓ NombreCompleto
✓ FechaVencimiento
✓ IdCliente          ← FK a TM_Cliente
✗ nom_completo       ← snake_case
✗ strNombre          ← notación húngara
✗ NOMBRECOMPLETO     ← todo mayúsculas
```

### 5.1 Convención de FKs

```sql
-- El nombre de la columna FK = Id + NombreTablaReferenciada (sin prefijo TM_/TB_)
IdCliente   INT  -- FK a TM_Cliente
IdCiudad    INT  -- FK a TB_Ciudad
IdUsuario   INT  -- FK a TS_Usuario

-- Constraint: FK_TablaOrigen_TablaDestino
CONSTRAINT FK_TM_Orden_TM_Cliente
    FOREIGN KEY (IdCliente) REFERENCES TM_Cliente(ID)
```

### 5.2 Nombres reservados (usar exactamente así)

| Columna | Tipo | Significado |
|---------|------|-------------|
| `ID` | INT / BIGINT | PK de la tabla |
| `Estado` | BIT | Activo = 1, Inactivo = 0 |
| `Eliminado` | BIT | Soft delete = 1 |
| `Nombre` | NVARCHAR | Nombre corto del registro |
| `Descripcion` | NVARCHAR | Descripción larga |
| `Orden` | INT | Orden de presentación |
| `Valor` | DECIMAL | Valor numérico genérico |
| `Codigo` | NVARCHAR | Código único de negocio |
| `Observaciones` | NVARCHAR(MAX) | Texto libre de usuario |

---

## 6. Constraints y Claves

### 6.1 Nomenclatura de constraints

```sql
PK_NombreTabla                       -- Primary Key
FK_TablaOrigen_TablaDestino          -- Foreign Key
UQ_NombreTabla_Columna               -- Unique
IX_NombreTabla_Columna               -- Índice no único
IXU_NombreTabla_Columna              -- Índice único
CK_NombreTabla_Columna               -- Check constraint
DF_NombreTabla_Columna               -- Default
```

### 6.2 Reglas de PK

```sql
-- Preferir IDENTITY sobre GUID para PKs locales
ID INT IDENTITY(1,1) NOT NULL  -- ✓ rendimiento óptimo en clustered index

-- GUID solo para:
-- 1. Sincronización entre bases de datos
-- 2. APIs que exponen IDs al cliente (usar como columna adicional, no PK)
ID      INT              IDENTITY(1,1) NOT NULL,  -- PK interna
Codigo  UNIQUEIDENTIFIER DEFAULT NEWSEQUENTIALID() NOT NULL  -- ID público API
```

### 6.3 Reglas de FK

```sql
-- Siempre con nombre explícito
-- Siempre con ON DELETE RESTRICT (no CASCADE en tablas transaccionales)
CONSTRAINT FK_TM_OrdenDetalle_TM_Orden
    FOREIGN KEY (IdOrden)
    REFERENCES TM_Orden(ID)
    ON DELETE RESTRICT    -- nunca CASCADE en TM_
    ON UPDATE RESTRICT;

-- Excepción: tablas TC_ (composición) pueden usar CASCADE
CONSTRAINT FK_TC_DireccionCliente_TM_Cliente
    FOREIGN KEY (IdCliente)
    REFERENCES TM_Cliente(ID)
    ON DELETE CASCADE;    -- si se elimina el cliente, se eliminan sus direcciones
```

---

## 7. Índices

### 7.1 Índices obligatorios automáticos (AIGEN los genera)

```sql
-- 1. PK clustered (siempre sobre ID)
-- 2. FK columns (para evitar table scans en JOINs)
CREATE INDEX IX_TM_Orden_IdCliente ON TM_Orden (IdCliente);

-- 3. Estado + Eliminado (filtro más frecuente)
CREATE INDEX IX_TM_Orden_Estado_Eliminado
    ON TM_Orden (Estado, Eliminado);
```

### 7.2 Cuándo agregar índices adicionales

```
Agregar índice si la columna aparece en:
  ✓ WHERE frecuente (más del 20% de las queries)
  ✓ ORDER BY en consultas paginadas
  ✓ GROUP BY en reportes

No agregar índice en:
  ✗ Columnas BIT (baja cardinalidad)
  ✗ Columnas de auditoría (CreadoEn, ModificadoEn)
  ✗ Columnas que se actualizan muy frecuentemente
```

### 7.3 Índices con columnas incluidas (covering index)

```sql
-- Para queries de lista con filtro frecuente:
CREATE INDEX IX_TM_Orden_IdCliente_Fecha
    ON TM_Orden (IdCliente, FechaCreacion)
    INCLUDE (Total, Estado);   -- columnas del SELECT, no del WHERE
```

---

## 8. Patrones de Diseño

### 8.1 Tabla simple (TB_)

```sql
CREATE TABLE TB_TipoDocumento (
    ID          INT          IDENTITY(1,1) NOT NULL,
    Nombre      NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    Codigo      NVARCHAR(20)  NULL,
    Orden       INT           NOT NULL DEFAULT 0,
    Estado      BIT           NOT NULL DEFAULT 1,
    CreadoEn    DATETIME2(0)  NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor   VARCHAR(100)  NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT PK_TB_TipoDocumento PRIMARY KEY (ID),
    CONSTRAINT UQ_TB_TipoDocumento_Nombre UNIQUE (Nombre)
);
```

### 8.2 Maestro-Detalle (TM_ + TC_)

```sql
-- Maestro
CREATE TABLE TM_Orden (
    ID          INT           IDENTITY(1,1) NOT NULL,
    IdCliente   INT           NOT NULL,
    Numero      NVARCHAR(20)  NOT NULL,
    Fecha       DATE          NOT NULL,
    Total       DECIMAL(18,4) NOT NULL DEFAULT 0,
    Estado      BIT           NOT NULL DEFAULT 1,
    Eliminado   BIT           NOT NULL DEFAULT 0,
    CreadoEn    DATETIME2(0)  NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor   VARCHAR(100)  NOT NULL DEFAULT SYSTEM_USER,
    ModificadoEn  DATETIME2(0) NULL,
    ModificadoPor VARCHAR(100) NULL,
    CONSTRAINT PK_TM_Orden    PRIMARY KEY (ID),
    CONSTRAINT FK_TM_Orden_TM_Cliente
        FOREIGN KEY (IdCliente) REFERENCES TM_Cliente(ID)
);

-- Detalle (composición: muere con el padre)
CREATE TABLE TC_OrdenDetalle (
    ID          INT           IDENTITY(1,1) NOT NULL,
    IdOrden     INT           NOT NULL,
    IdProducto  INT           NOT NULL,
    Cantidad    DECIMAL(18,4) NOT NULL,
    PrecioUnitario DECIMAL(18,4) NOT NULL,
    Subtotal    DECIMAL(18,4) NOT NULL,
    Estado      BIT           NOT NULL DEFAULT 1,
    Eliminado   BIT           NOT NULL DEFAULT 0,
    CreadoEn    DATETIME2(0)  NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor   VARCHAR(100)  NOT NULL DEFAULT SYSTEM_USER,
    ModificadoEn  DATETIME2(0) NULL,
    ModificadoPor VARCHAR(100) NULL,
    CONSTRAINT PK_TC_OrdenDetalle PRIMARY KEY (ID),
    CONSTRAINT FK_TC_OrdenDetalle_TM_Orden
        FOREIGN KEY (IdOrden) REFERENCES TM_Orden(ID) ON DELETE CASCADE,
    CONSTRAINT FK_TC_OrdenDetalle_TM_Producto
        FOREIGN KEY (IdProducto) REFERENCES TM_Producto(ID)
);
```

### 8.3 Relacional N:M sin atributos (TR_)

```sql
CREATE TABLE TR_UsuarioRol (
    IdUsuario   INT NOT NULL,
    IdRol       INT NOT NULL,
    CreadoEn    DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor   VARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
    CONSTRAINT PK_TR_UsuarioRol PRIMARY KEY (IdUsuario, IdRol),
    CONSTRAINT FK_TR_UsuarioRol_TS_Usuario FOREIGN KEY (IdUsuario) REFERENCES TS_Usuario(ID),
    CONSTRAINT FK_TR_UsuarioRol_TB_Rol     FOREIGN KEY (IdRol)     REFERENCES TB_Rol(ID)
);
-- NOTA: TR_ no tiene Estado ni Eliminado. La relación existe o se elimina físicamente.
```

### 8.4 Relacional N:M con atributos (TP_)

```sql
-- Cuando la relación tiene atributos propios, pasa a ser TP_ con ID propio
CREATE TABLE TP_UsuarioProyecto (
    ID          INT           IDENTITY(1,1) NOT NULL,
    IdUsuario   INT           NOT NULL,
    IdProyecto  INT           NOT NULL,
    Rol         NVARCHAR(50)  NOT NULL,  -- atributo propio
    FechaInicio DATE          NOT NULL,  -- atributo propio
    FechaFin    DATE          NULL,      -- atributo propio
    Estado      BIT           NOT NULL DEFAULT 1,
    Eliminado   BIT           NOT NULL DEFAULT 0,
    CreadoEn    DATETIME2(0)  NOT NULL DEFAULT GETUTCDATE(),
    CreadoPor   VARCHAR(100)  NOT NULL DEFAULT SYSTEM_USER,
    ModificadoEn  DATETIME2(0) NULL,
    ModificadoPor VARCHAR(100) NULL,
    CONSTRAINT PK_TP_UsuarioProyecto PRIMARY KEY (ID),
    CONSTRAINT UQ_TP_UsuarioProyecto UNIQUE (IdUsuario, IdProyecto)
);
```

### 8.5 Auditoría (TA_)

```sql
CREATE TABLE TA_TM_Orden (
    ID              BIGINT        IDENTITY(1,1) NOT NULL,
    IdRegistro      INT           NOT NULL,       -- ID de la fila auditada
    Operacion       CHAR(1)       NOT NULL,        -- I=Insert, U=Update, D=Delete
    DatosAnteriores NVARCHAR(MAX) NULL,            -- JSON con valores previos
    DatosNuevos     NVARCHAR(MAX) NULL,            -- JSON con valores nuevos
    Usuario         VARCHAR(100)  NOT NULL,
    Aplicacion      VARCHAR(100)  NULL,
    IpMaquina       VARCHAR(50)   NULL,
    FechaHora       DATETIME2(3)  NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_TA_TM_Orden PRIMARY KEY (ID),
    CONSTRAINT CK_TA_TM_Orden_Operacion CHECK (Operacion IN ('I','U','D'))
);
-- Particionado por FechaHora en producción (mensual)
-- NUNCA tiene Estado, Eliminado ni campos de modificación
```

---

## 9. Stored Procedures y Queries

### 9.1 Nomenclatura

```sql
PA_SEL_{Tabla}         -- Select por ID
PA_ALL_{Tabla}         -- Select paginado
PA_INS_{Tabla}         -- Insert
PA_UPD_{Tabla}         -- Update
PA_DEL_{Tabla}         -- Delete (soft o físico)
PA_ESTADO_{Tabla}      -- Toggle Estado
PA_QRY_{Nombre}        -- Query compleja de negocio
PA_RPT_{Nombre}        -- Reporte
```

### 9.2 Reglas obligatorias

```sql
-- ✓ Siempre con esquema explícito
SELECT * FROM dbo.TM_Orden            -- ✓
SELECT * FROM TM_Orden                -- ✗ sin schema

-- ✓ Nunca SELECT *
SELECT ID, Nombre, Estado FROM TB_Ciudad   -- ✓
SELECT * FROM TB_Ciudad                    -- ✗

-- ✓ Siempre filtrar Eliminado
WHERE Eliminado = 0 AND Estado = 1    -- ✓ en queries de UI
WHERE 1=1                             -- ✗ sin filtro de soft delete

-- ✓ Paginación con OFFSET/FETCH (no TOP)
ORDER BY ID
OFFSET (@Pagina - 1) * @TamanioPagina ROWS
FETCH NEXT @TamanioPagina ROWS ONLY;

-- ✓ Variables tipo tabla en lugar de tablas temporales
DECLARE @Resultado TABLE (ID INT, Nombre NVARCHAR(100));
-- ✗ CREATE TABLE #Temp (...)  <- bloqueos en tempdb

-- ✓ Parámetros en PascalCase con @
@IdCliente INT,
@FechaInicio DATE,
@Pagina INT = 1,
@TamanioPagina INT = 20
```

---

## 10. Schemas (Namespaces de BD)

```sql
-- Formato: abreviatura del módulo en mayúsculas
dbo          -- Datos compartidos / sin módulo asignado
VEN          -- Ventas
COM          -- Compras
INV          -- Inventario
CON          -- Contabilidad
RH           -- Recursos Humanos
SEG          -- Seguridad

-- Ejemplo
CREATE TABLE VEN.TM_Orden (...)
CREATE TABLE INV.TM_Producto (...)
CREATE TABLE SEG.TS_Usuario (...)
```

---

## 11. Checklist de Revisión

Antes de aprobar un modelo nuevo, verificar:

```
ESTRUCTURA
[ ] Prefijo de tabla correcto (TM_, TB_, TP_, TR_, TC_, TA_, TS_, TH_, TI_, TX_)
[ ] PascalCase en nombre de tabla y columnas
[ ] PK con nombre ID y tipo INT IDENTITY(1,1)
[ ] Columnas obligatorias: Estado, Eliminado, CreadoEn, CreadoPor, ModificadoEn, ModificadoPor
[ ] Excepciones de columnas justificadas (TR_, TA_, TH_)

TIPOS DE DATO
[ ] Sin MONEY, FLOAT, REAL → usar DECIMAL(18,4)
[ ] Sin DATETIME, SMALLDATETIME → usar DATETIME2(0) o DATE
[ ] Sin IMAGE, TEXT, NTEXT → usar VARBINARY(MAX), NVARCHAR(MAX)
[ ] Fechas en UTC (GETUTCDATE() / NOW())
[ ] NVARCHAR con longitud definida (no MAX por defecto)

CONSTRAINTS
[ ] PK nombrada: PK_NombreTabla
[ ] FKs nombradas: FK_TablaOrigen_TablaDestino
[ ] ON DELETE RESTRICT en TM_ (nunca CASCADE)
[ ] CASCADE solo en TC_ (composición)
[ ] Restricciones CHECK en columnas con valores fijos (Operacion IN ('I','U','D'))

ÍNDICES
[ ] Índice sobre cada columna FK
[ ] Índice sobre Estado + Eliminado en tablas TM_
[ ] Sin índices en columnas BIT de baja cardinalidad

DOCUMENTACIÓN
[ ] Comentario/descripción en columnas no obvias
[ ] Propiedades extendidas para tablas y columnas clave (SQL Server)
```

---

## 12. Lo que NO aplica a modelos existentes

Esta línea base aplica **únicamente a tablas nuevas**. Para tablas existentes:

- No renombrar columnas (rompe código legacy)
- No cambiar tipos de dato (riesgo de pérdida de datos)
- No agregar columnas obligatorias sin default (rompe inserts existentes)
- Sí se pueden agregar columnas nuevas **con DEFAULT**
- Sí se pueden agregar índices nuevos (no destructivo)
- Sí se pueden agregar FKs si los datos son consistentes

---

## 13. Integración con AIGEN

AIGEN detecta automáticamente el prefijo de cada tabla y adapta el código generado:

| Prefijo | Entity | Repository | Controller | Frontend |
|---------|--------|------------|------------|----------|
| `TM_` | ✓ | CRUD completo | 5 endpoints | Lista + Formulario |
| `TB_` | ✓ | CRUD completo | 5 endpoints | Lista + Formulario |
| `TBR_` | ✓ | CRUD completo | 5 endpoints | Lista + Formulario |
| `TP_` | ✓ | CRUD + Toggle Estado | 6 endpoints | Lista + Formulario |
| `TR_` | ✓ | Insert/Delete/Select | 3 endpoints | Solo asignación |
| `TC_` | ✓ | CRUD completo | 5 endpoints | Embebido en padre |
| `TA_` | ✓ | Solo lectura | 2 endpoints (GET) | Solo lectura |
| `TS_` | ✓ | Solo lectura | 2 endpoints (GET) | Solo lectura |
| `TH_` | ✓ | Solo lectura | 2 endpoints (GET) | Solo lectura |

---

*Documento generado para el proyecto AIGEN — Línea base modelos nuevos v1.0*
