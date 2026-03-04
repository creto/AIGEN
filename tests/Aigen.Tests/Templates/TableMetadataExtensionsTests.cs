using Aigen.Core.Metadata;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Templates;

public class TableMetadataExtensionsTests
{
    private static TableMetadata T(string name) =>
        new() { TableName = name, ClassName = name };

    // ── Prefijos Incoder ──────────────────────────────────────
    [Theory]
    [InlineData("TM_Factura",  TableType.Movement)]
    [InlineData("TB_Ciudad",   TableType.Basic)]
    [InlineData("TBR_Detalle", TableType.BasicRelated)]
    [InlineData("TP_Config",   TableType.Parameter)]
    [InlineData("TR_Relacion", TableType.Relational)]
    [InlineData("TA_Audit",    TableType.Audit)]
    [InlineData("TS_Sistema",  TableType.System)]
    [InlineData("TH_Historic", TableType.Historical)]
    // "SinPrefijo" ya no retorna Unknown — ahora usa heurística
    // Una tabla "SinPrefijo" con 0 columnas cae en Basic (fkCount=0, colCount=0 ≤ 6)
    // Ese comportamiento ES el correcto — Unknown solo existe como fallback en HasFullCrud
    public void GetTableType_DetectsPrefixCorrectly(string name, TableType expected)
        => T(name).GetTableType().Should().Be(expected);

    [Theory]
    [InlineData("TM_Factura",  true)]
    [InlineData("TB_Ciudad",   true)]
    [InlineData("TP_Config",   true)]
    [InlineData("TA_Audit",    false)]
    [InlineData("TS_Sistema",  false)]
    [InlineData("TH_Historic", false)]
    public void HasFullCrud_CorrectByType(string name, bool expected)
        => T(name).HasFullCrud().Should().Be(expected);

    [Theory]
    [InlineData("TA_Audit",    true)]
    [InlineData("TS_Sistema",  true)]
    [InlineData("TH_Historic", true)]
    [InlineData("TM_Factura",  false)]
    [InlineData("TB_Ciudad",   false)]
    public void IsReadOnly_CorrectByType(string name, bool expected)
        => T(name).IsReadOnly().Should().Be(expected);

    [Fact]
    public void HasEstadoField_TrueWhenColumnExists()
    {
        var t = T("TM_Factura");
        t.Columns.Add(new() { ColumnName = "Estado" });
        t.HasEstadoField().Should().BeTrue();
    }

    [Fact]
    public void HasEstadoField_FalseWhenNoColumn()
        => T("TM_Factura").HasEstadoField().Should().BeFalse();
}
