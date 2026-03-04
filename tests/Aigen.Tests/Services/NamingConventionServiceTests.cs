using Aigen.Core.Services;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Services;

public class NamingConventionServiceTests
{
    private readonly NamingConventionService _svc = new();

    [Theory]
    [InlineData("TM_Factura",      "Factura")]
    [InlineData("TC_Ciudad",       "Ciudad")]
    [InlineData("TB_Bodega",       "Bodega")]
    [InlineData("TA_Ingreso",      "Ingreso")]
    [InlineData("TP_Departamento", "Departamento")]
    [InlineData("TG_Log",          "Log")]
    [InlineData("Parametro",       "Parametro")]
    [InlineData("factura_venta",   "FacturaVenta")]
    public void ToClassName_RemovesPrefixAndPascalCases(string input, string expected)
        => _svc.ToClassName(input).Should().Be(expected);

    [Theory]
    [InlineData("Factura",       "Facturas")]
    [InlineData("Bodega",        "Bodegas")]
    [InlineData("Configuracion", "Configuraciones")]
    [InlineData("Luz",           "Luces")]
    [InlineData("Crisis",        "Crisis")]
    public void ToClassNamePlural_PluralizesCorrectly(string input, string expected)
        => _svc.ToClassNamePlural(input).Should().Be(expected);

    [Theory]
    [InlineData("FacturaVenta",  "factura-venta")]
    [InlineData("Ciudad",        "ciudad")]
    [InlineData("TipoDocumento", "tipo-documento")]
    public void ToAngularFileName_ReturnsKebabCase(string input, string expected)
        => _svc.ToAngularFileName(input).Should().Be(expected);

    [Theory]
    [InlineData("createdAt", true)]
    [InlineData("updatedBy", true)]
    [InlineData("_usuario",  true)]
    [InlineData("eliminado", true)]
    [InlineData("nombre",    false)]
    [InlineData("precio",    false)]
    public void IsAuditField_DetectsAuditColumns(string col, bool expected)
        => _svc.IsAuditField(col).Should().Be(expected);

    [Fact]
    public void ToServiceName_AppendsService()
        => _svc.ToServiceName("Factura").Should().Be("FacturaService");

    [Fact]
    public void ToControllerName_AppendsController()
        => _svc.ToControllerName("Facturas").Should().Be("FacturasController");

    [Fact]
    public void ToApiRoute_ReturnsLowercasePluralRoute()
        => _svc.ToApiRoute("Facturas").Should().Be("/api/facturas");

    [Fact]
    public void ToObjectName_ReturnsCamelCase()
        => _svc.ToObjectName("FacturaVenta").Should().Be("facturaVenta");
}
