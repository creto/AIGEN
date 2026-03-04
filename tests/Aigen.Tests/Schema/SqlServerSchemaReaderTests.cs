using Aigen.Core.Metadata;
using Aigen.Core.Schema;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aigen.Tests.Schema;

public class SqlServerSchemaReaderTests
{
    private const string TestConnectionString =
        "Server=TU_SERVIDOR;Database=TU_BD;User Id=sa;Password=TU_PASS;TrustServerCertificate=True;";

    [Fact]
    public void SqlServerSchemaReader_Implements_ISchemaReader()
    {
        new SqlServerSchemaReader().Should().BeAssignableTo<ISchemaReader>();
    }

    [Fact]
    public async Task TestConnectionAsync_WithInvalidConnection_ReturnsFalse()
    {
        var result = await new SqlServerSchemaReader().TestConnectionAsync(
            "Server=NOPE;Database=X;User Id=sa;Password=x;TrustServerCertificate=True;Connect Timeout=2;");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WithEmptyString_ReturnsFalse()
    {
        var result = await new SqlServerSchemaReader().TestConnectionAsync(string.Empty);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ISchemaReader_Mock_ReturnsExpectedData()
    {
        var mock = new Mock<ISchemaReader>();

        mock.Setup(r => r.TestConnectionAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        mock.Setup(r => r.ReadAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DatabaseMetadata
            {
                DatabaseName = "TestDB",
                ServerName   = "TestServer",
                Tables = new List<TableMetadata>
                {
                    new() { TableName = "TM_Cliente", ClassName = "Cliente" }
                }
            });

        var connected = await mock.Object.TestConnectionAsync("x");
        connected.Should().BeTrue();

        var metadata = await mock.Object.ReadAsync("x");
        metadata.Tables.Should().HaveCount(1);
        metadata.Tables[0].ClassName.Should().Be("Cliente");
    }

    [Fact(Skip = "Requiere SQL Server real")]
    public async Task TestConnectionAsync_WithValidConnection_ReturnsTrue()
    {
        var result = await new SqlServerSchemaReader()
            .TestConnectionAsync(TestConnectionString);
        result.Should().BeTrue();
    }

    [Fact(Skip = "Requiere SQL Server real")]
    public async Task ReadAsync_ReturnsMetadataWithTables()
    {
        var metadata = await new SqlServerSchemaReader()
            .ReadAsync(TestConnectionString);
        metadata.Tables.Should().NotBeEmpty();
        metadata.Engine.Should().Be("SqlServer");
    }

    [Fact(Skip = "Requiere SQL Server real")]
    public async Task ReadAsync_TM_Tables_HaveCleanClassName()
    {
        var metadata = await new SqlServerSchemaReader()
            .ReadAsync(TestConnectionString);
        foreach (var t in metadata.Tables.Where(t => t.TableName.StartsWith("TM_", StringComparison.OrdinalIgnoreCase)))
            t.ClassName.Should().NotStartWith("Tm");
    }
}
