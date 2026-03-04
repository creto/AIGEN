using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Metadata;
using Aigen.Templates.Engine;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Templates;

public class DbContextTemplateTests
{
    private readonly ScribanTemplateEngine _engine = new();

    private static TemplateContext BuildCtx() => new()
    {
        Table  = new TableMetadata
        {
            TableName       = "TM_Factura",
            ClassName       = "Factura",
            ClassNamePlural = "Facturas",
            PrimaryKeys     = new() { "ID" },
            Columns         = new() { new() { ColumnName="ID", PropertyName="Id",
                                              CSharpType="int", IsPrimaryKey=true } }
        },
        Db     = new DatabaseMetadata
        {
            DatabaseName = "VentasDB",
            Schema       = "dbo",
            Tables       = new()
            {
                new() { TableName="TM_Factura", ClassName="Factura",
                        ClassNamePlural="Facturas", PrimaryKeys=new(){"ID"} },
                new() { TableName="TB_Ciudad",  ClassName="Ciudad",
                        ClassNamePlural="Ciudades", PrimaryKeys=new(){"ID"} },
            }
        },
        Config = new GeneratorConfig
        {
            Project  = new ProjectConfig
                { ProjectName = "Ventas", RootNamespace = "Com.Empresa.Ventas" },
            Database = new DatabaseConfig { Schema = "dbo" },
            Features = new FeaturesConfig { Auditing = true, SoftDelete = true },
            Backend  = new BackendConfig  { Orm = OrmType.EntityFrameworkCore }
        }
    };

    private static string LoadTemplate(string name)
    {
        var paths = new[]
        {
            Path.Combine("Templates", "Solution", name),
            Path.Combine("..", "..", "..", "..", "src",
                "Aigen.Templates", "Templates", "Solution", name),
        };
        foreach (var p in paths)
            if (File.Exists(p)) return File.ReadAllText(p);
        return $"// Template {name} not found";
    }

    [Fact]
    public async Task DbContext_ContainsProjectName()
    {
        var tpl    = LoadTemplate("dbcontext.scriban");
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("VentasDbContext");
    }

    [Fact]
    public async Task DbContext_ContainsApplyConfigurationsFromAssembly()
    {
        var tpl    = LoadTemplate("dbcontext.scriban");
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("ApplyConfigurationsFromAssembly");
    }

    [Fact]
    public async Task DbContext_WithAuditing_ContainsSaveChangesAsync()
    {
        var tpl    = LoadTemplate("dbcontext.scriban");
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("SaveChangesAsync")
              .And.Contain("ApplyAuditInfo");
    }

    [Fact]
    public async Task DbContext_ContainsDefaultSchema()
    {
        var tpl    = LoadTemplate("dbcontext.scriban");
        var result = await _engine.RenderStringAsync(tpl, BuildCtx());
        result.Should().Contain("HasDefaultSchema").And.Contain("dbo");
    }
}
