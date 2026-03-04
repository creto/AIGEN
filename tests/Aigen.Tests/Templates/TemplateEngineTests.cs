using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Aigen.Templates;
using Aigen.Templates.Engine;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Templates;

public class ScribanTemplateEngineTests
{
    private readonly ScribanTemplateEngine _engine = new();

    private static TemplateContext BuildCtx(string tableName = "TM_Factura")
    {
        var table = new TableMetadata
        {
            TableName      = tableName,
            ClassName      = "Factura",
            ClassNamePlural= "Facturas",
            ObjectName     = "factura",
            ServiceName    = "FacturaService",
            RepositoryName = "FacturaRepository",
            ControllerName = "FacturasController",
            ApiRoute       = "/api/facturas",
            PrimaryKeys    = new() { "ID" },
            Columns = new()
            {
                new ColumnMetadata
                {
                    ColumnName    = "ID",
                    PropertyName  = "Id",
                    TsPropertyName= "id",
                    CSharpType    = "int",
                    TypeScriptType= "number",
                    IsPrimaryKey  = true,
                    IsIdentity    = true,
                    IsNullable    = false
                },
                new ColumnMetadata
                {
                    ColumnName    = "Nombre",
                    PropertyName  = "Nombre",
                    TsPropertyName= "nombre",
                    CSharpType    = "string",
                    TypeScriptType= "string",
                    IsNullable    = false,
                    MaxLength     = 100
                },
                new ColumnMetadata
                {
                    ColumnName    = "Valor",
                    PropertyName  = "Valor",
                    TsPropertyName= "valor",
                    CSharpType    = "decimal",
                    TypeScriptType= "number",
                    IsNullable    = false
                }
            }
        };

        return new TemplateContext
        {
            Table  = table,
            Db     = new DatabaseMetadata { DatabaseName = "TestDB", Schema = "dbo" },
            Config = new GeneratorConfig
            {
                Project  = new ProjectConfig
                    { ProjectName = "TestApp", RootNamespace = "Com.Test.App" },
                Database = new DatabaseConfig { Schema = "dbo" },
                Features = new FeaturesConfig
                {
                    GeneratePagination = true,
                    SoftDelete         = true,
                    Validation         = ValidationProvider.FluentValidation
                },
                Backend  = new BackendConfig { Orm = OrmType.Dapper }
            }
        };
    }

    [Fact]
    public async Task RenderString_SimpleTemplate_Works()
    {
        var tpl    = "Hello {{ entity_name }}!";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Be("Hello Factura!");
    }

    [Fact]
    public async Task RenderString_PluralHelper_Works()
    {
        var tpl    = "{{ entity_name_plural }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Be("Facturas");
    }

    [Fact]
    public async Task RenderString_PkType_IsInt()
    {
        var tpl    = "{{ pk_type }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Be("int");
    }

    [Fact]
    public async Task RenderString_ApiRoute_IsCorrect()
    {
        var tpl    = "{{ api_route }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Be("/api/facturas");
    }

    [Fact]
    public async Task RenderString_NsApplication_ContainsPlural()
    {
        var tpl    = "{{ ns_application }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("Facturas");
    }

    [Fact]
    public async Task RenderString_ForLoopColumns_IteratesAll()
    {
        var tpl    = "{{ for col in all_columns }}{{ col.property_name }};{{ end }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("Id;").And.Contain("Nombre;").And.Contain("Valor;");
    }

    [Fact]
    public async Task RenderString_KebabFunction_Works()
    {
        var tpl    = "{{ kebab entity_name }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Be("factura");
    }

    [Fact]
    public async Task RenderString_HasFullCrud_TrueForTM()
    {
        var tpl    = "{{ has_full_crud }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx("TM_Factura"));
        result.Should().Be("true");
    }

    [Fact]
    public async Task RenderString_IsReadOnly_TrueForTA()
    {
        var tpl    = "{{ is_read_only }}";
        var result = await _engine.RenderStringAsync(tpl, BuildCtx("TA_Auditoria"));
        result.Should().Be("true");
    }

    [Fact]
    public async Task RenderString_InvalidTemplate_ThrowsException()
    {
        var tpl = "{{ invalid syntax {{{{";
        var act = () => _engine.RenderStringAsync(tpl, BuildCtx());
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
