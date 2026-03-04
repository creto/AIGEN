using Aigen.Core.Metadata;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Templates;

public class TableMetadataExtensionsHeuristicTests
{
    // Helper: crea tabla con estructura controlada
    private static TableMetadata T(string name,
        int pkCount = 1, int fkCount = 0, int colCount = 5,
        bool hasOperacion = false, bool hasTimestamp = false)
    {
        var cols = Enumerable.Range(1, colCount).Select(i => new ColumnMetadata
        {
            ColumnName   = i == 1 ? "ID" :
                           hasOperacion && i == 2 ? "Operacion" :
                           hasTimestamp  && i == 3 ? "FechaHora" : $"Col{i}",
            CSharpType   = hasTimestamp && i == 3 ? "DateTime" : "string",
            IsPrimaryKey = i == 1
        }).ToList();

        return new TableMetadata
        {
            TableName   = name,
            ClassName   = name,
            PrimaryKeys = Enumerable.Range(1, pkCount).Select(i => $"Id{i}").ToList(),
            ForeignKeys = Enumerable.Range(1, fkCount)
                .Select(i => new ForeignKeyMetadata { ColumnName = $"IdFK{i}" }).ToList(),
            Columns     = cols
        };
    }

    // ── Prefijos Incoder no deben verse afectados por heurística ─
    [Theory]
    [InlineData("TM_Factura",  TableType.Movement)]
    [InlineData("TB_Ciudad",   TableType.Basic)]
    [InlineData("TBR_Detalle", TableType.BasicRelated)]
    [InlineData("TP_Config",   TableType.Parameter)]
    [InlineData("TR_UsuRol",   TableType.Relational)]
    [InlineData("TA_Audit",    TableType.Audit)]
    [InlineData("TS_Sistema",  TableType.System)]
    [InlineData("TH_Historic", TableType.Historical)]
    public void ConPrefijo_HeuristicaNoInterfiere(string name, TableType expected)
        => T(name).GetTableType().Should().Be(expected);

    // ── Heurística por nombre — Audit ─────────────────────────
    [Theory]
    [InlineData("EventLog")]
    [InlineData("AuditLog")]
    [InlineData("Auditoria")]
    [InlineData("LogSistema")]
    [InlineData("ChangeLog")]
    public void SinPrefijo_NombreAuditoria_DetectaAudit(string name)
        => T(name).GetTableType().Should().Be(TableType.Audit);

    // ── Heurística por nombre — Historical ───────────────────
    [Theory]
    [InlineData("Historico")]
    [InlineData("HistorialCliente")]  // "Historial" → Historical (no Audit)
    [InlineData("ArchivoDocumento")]
    [InlineData("Snapshot")]
    public void SinPrefijo_NombreHistorico_DetectaHistorical(string name)
        => T(name).GetTableType().Should().Be(TableType.Historical);

    // ── Heurística por nombre — System ───────────────────────
    [Theory]
    [InlineData("ConfiguracionSistema")]
    [InlineData("AppConfig")]
    [InlineData("GlobalSetting")]
    public void SinPrefijo_NombreConfig_DetectaSystem(string name)
        => T(name).GetTableType().Should().Be(TableType.System);

    // ── Heurística por estructura — Relacional ────────────────
    [Fact]
    public void SinPrefijo_PKCompuestaPor2FKs_3Cols_DetectaRelacional()
        => T("UsuarioRol", pkCount: 2, fkCount: 2, colCount: 3)
            .GetTableType().Should().Be(TableType.Relational);

    [Fact]
    public void SinPrefijo_PKCompuesta_5Cols_NoEsRelacional()
        // Con 5 columnas ya no es "tabla relacional pura" (umbral ≤4)
        => T("UsuarioRol", pkCount: 2, fkCount: 2, colCount: 5)
            .GetTableType().Should().NotBe(TableType.Relational);

    // ── Heurística por estructura — Audit ─────────────────────
    [Fact]
    public void SinPrefijo_ConOperacionYTimestamp_DetectaAudit()
        => T("RegistroCambios", hasOperacion: true, hasTimestamp: true)
            .GetTableType().Should().Be(TableType.Audit);

    // ── Heurística por estructura — Basic ─────────────────────
    [Fact]
    public void SinPrefijo_SinFKsPocosCols_DetectaBasic()
        // 0 FKs, 5 cols ≤ 6 → Basic
        => T("TipoDocumento", fkCount: 0, colCount: 5)
            .GetTableType().Should().Be(TableType.Basic);

    [Fact]
    public void SinPrefijo_MuchasCols_DetectaMovement()
        // 0 FKs pero 10 cols > 6 → Movement
        => T("Factura", pkCount: 1, fkCount: 2, colCount: 10)
            .GetTableType().Should().Be(TableType.Movement);

    // ── Producto: 5 cols, 0 FKs → Basic (catálogo simple) ────
    [Fact]
    public void SinPrefijo_Producto5Cols_EsBasic()
        => T("Producto", fkCount: 0, colCount: 5)
            .GetTableType().Should().Be(TableType.Basic);

    // ── HasFullCrud para tablas sin prefijo ───────────────────
    [Fact]
    public void SinPrefijo_TablaBasic_TieneFullCrud()
        => T("Producto").HasFullCrud().Should().BeTrue();

    [Fact]
    public void SinPrefijo_TablaAuditoria_NoTieneFullCrud()
        => T("EventLog").HasFullCrud().Should().BeFalse();

    [Fact]
    public void SinPrefijo_TablaHistorica_NoTieneFullCrud()
        => T("Historial").HasFullCrud().Should().BeFalse();

    // ── GetTableTypeLabel ─────────────────────────────────────
    [Fact]
    public void ConPrefijo_Label_NoMuestraInferido()
        => T("TM_Factura").GetTableTypeLabel().Should().NotContain("inferido");

    [Fact]
    public void SinPrefijo_Label_MuestraInferido()
        => T("Factura", colCount: 10).GetTableTypeLabel().Should().Contain("inferido");

    // ── Escenario híbrido (expectativas corregidas) ───────────
    [Fact]
    public void Hibrido_TablasConYSinPrefijo_ClasificanCorrectamente()
    {
        // Con prefijo → clasificación directa
        T("TM_Orden").GetTableType()    .Should().Be(TableType.Movement);
        T("TA_AudDB").GetTableType()    .Should().Be(TableType.Audit);
        T("TB_Ciudad").GetTableType()   .Should().Be(TableType.Basic);

        // Sin prefijo → heurística
        // Producto (5 cols, 0 FKs) → Basic (catálogo inferido)
        T("Producto", colCount: 5, fkCount: 0).GetTableType()
            .Should().Be(TableType.Basic);
        // EventLog → Audit por nombre
        T("EventLog").GetTableType()
            .Should().Be(TableType.Audit);
        // Factura (10 cols, 2 FKs) → Movement por estructura
        T("Factura", colCount: 10, fkCount: 2).GetTableType()
            .Should().Be(TableType.Movement);
    }
}
