using Aigen.Core.Config;
using Aigen.Core.Config.Enums;
using Aigen.Core.Validation;
using FluentAssertions;
using Xunit;

namespace Aigen.Tests.Validation;

public class ConfigValidatorTests
{
    private readonly ConfigValidator _validator = new();

    private static GeneratorConfig ValidConfig() => new()
    {
        Project  = new ProjectConfig  { ProjectName = "TestApp", RootNamespace = "Com.Test.App" },
        Database = new DatabaseConfig
        {
            Engine           = DatabaseEngine.SqlServer,
            ConnectionString = "Server=localhost;Database=TestDB;User Id=sa;Password=pass;TrustServerCertificate=True;",
            Schema           = "dbo",
            TableSelection   = "All"
        },
        Architecture = new ArchitectureConfig { Style = OutputStyle.MonolithModular },
        Output       = new OutputConfig { Path = "C:/output/{ProjectName}" }
    };

    [Fact]
    public void ValidConfig_IsValid()
    {
        var result = _validator.Validate(ValidConfig());
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void MissingProjectName_AddsError()
    {
        var config = ValidConfig();
        config.Project.ProjectName = "";
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "project.projectName");
    }

    [Fact]
    public void TemplateConnectionString_AddsError()
    {
        var config = ValidConfig();
        config.Database.ConnectionString = "Server=TU_SERVIDOR;Database=TU_BD;";
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "database.connectionString");
    }

    [Fact]
    public void EmptyConnectionString_AddsError()
    {
        var config = ValidConfig();
        config.Database.ConnectionString = "";
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void InvalidTableSelection_AddsError()
    {
        var config = ValidConfig();
        config.Database.TableSelection = "Wrong";
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "database.tableSelection");
    }

    [Fact]
    public void IncludeSelectionWithEmptyList_AddsError()
    {
        var config = ValidConfig();
        config.Database.TableSelection = "Include";
        config.Database.IncludedTables = new List<string>();
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void MissingOutputPath_AddsError()
    {
        var config = ValidConfig();
        config.Output.Path = "";
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void DuplicateMicroservicePorts_AddsError()
    {
        var config = ValidConfig();
        config.Architecture.Style = OutputStyle.Microservices;
        config.Architecture.Microservices = new List<MicroserviceDefinition>
        {
            new() { Name = "MS1", Port = 5001, Tables = new() { "Tabla1" } },
            new() { Name = "MS2", Port = 5001, Tables = new() { "Tabla2" } }
        };
        config.Architecture.Gateway.GenerateGateway = false;
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Field == "architecture");
    }
}
