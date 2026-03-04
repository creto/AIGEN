// ============================================================
//  AIGEN — SolutionGeneratorService.cs
//  Genera .sln y .csproj como parte del pipeline de generate
//  Se integra en FileGeneratorService.GenerateAsync()
// ============================================================
using System.Text;
using Aigen.Core.Config;

namespace Aigen.Templates.Services;

/// <summary>
/// Genera la estructura de solución .NET:
///   - {ProjectName}.sln
///   - src/{ProjectName}.Domain/{ProjectName}.Domain.csproj
///   - src/{ProjectName}.Application/{ProjectName}.Application.csproj
///   - src/{ProjectName}.Infrastructure/{ProjectName}.Infrastructure.csproj
///   - src/{ProjectName}.API/{ProjectName}.API.csproj
/// </summary>
public class SolutionGeneratorService
{
    public void Generate(AigenConfig config, string outputPath)
    {
        var p   = config.Project;
        var be  = config.Backend;
        var ns  = be.RootNamespace;
        var tf  = be.TargetFramework; // "net8.0"

        // GUIDs deterministas basados en el nombre del proyecto
        // (mismo proyecto → mismos GUIDs → no dirty el .sln en git)
        var domainGuid = DeterministicGuid($"{p.Name}.Domain");
        var appGuid    = DeterministicGuid($"{p.Name}.Application");
        var infraGuid  = DeterministicGuid($"{p.Name}.Infrastructure");
        var apiGuid    = DeterministicGuid($"{p.Name}.API");

        // ── 1. Solution ────────────────────────────────────────
        var sln = BuildSolution(p.Name, domainGuid, appGuid, infraGuid, apiGuid);
        WriteFile(Path.Combine(outputPath, $"{p.Name}.sln"), sln);

        // ── 2. Domain.csproj ───────────────────────────────────
        var domainCsproj = BuildDomainCsproj(ns, tf, p.Version);
        var domainDir = Path.Combine(outputPath, "src", $"{p.Name}.Domain");
        Directory.CreateDirectory(domainDir);
        WriteFile(Path.Combine(domainDir, $"{p.Name}.Domain.csproj"), domainCsproj);

        // ── 3. Application.csproj ──────────────────────────────
        var appCsproj = BuildApplicationCsproj(ns, tf, p.Name, p.Version);
        var appDir = Path.Combine(outputPath, "src", $"{p.Name}.Application");
        Directory.CreateDirectory(appDir);
        WriteFile(Path.Combine(appDir, $"{p.Name}.Application.csproj"), appCsproj);

        // ── 4. Infrastructure.csproj ───────────────────────────
        var infraCsproj = BuildInfrastructureCsproj(ns, tf, p.Name, be.Orm.ToString(), p.Version);
        var infraDir = Path.Combine(outputPath, "src", $"{p.Name}.Infrastructure");
        Directory.CreateDirectory(infraDir);
        WriteFile(Path.Combine(infraDir, $"{p.Name}.Infrastructure.csproj"), infraCsproj);

        // ── 5. API.csproj ──────────────────────────────────────
        var apiCsproj = BuildApiCsproj(ns, tf, p.Name, p.Version);
        var apiDir = Path.Combine(outputPath, "src", $"{p.Name}.API");
        Directory.CreateDirectory(apiDir);
        WriteFile(Path.Combine(apiDir, $"{p.Name}.API.csproj"), apiCsproj);

        Console.WriteLine($"  OK  {p.Name}.sln");
        Console.WriteLine($"  OK  {p.Name}.Domain.csproj");
        Console.WriteLine($"  OK  {p.Name}.Application.csproj");
        Console.WriteLine($"  OK  {p.Name}.Infrastructure.csproj");
        Console.WriteLine($"  OK  {p.Name}.API.csproj");
    }

    // ── Builders ──────────────────────────────────────────────

    private static string BuildSolution(string name,
        Guid domainGuid, Guid appGuid, Guid infraGuid, Guid apiGuid)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
        sb.AppendLine("# Visual Studio Version 17");
        sb.AppendLine("VisualStudioVersion = 17.0.31903.59");

        void AddProject(string label, Guid guid)
        {
            sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{name}.{label}\", \"src\\{name}.{label}\\{name}.{label}.csproj\", \"{{{guid.ToString().ToUpper()}}}\"");
            sb.AppendLine("EndProject");
        }

        AddProject("Domain",         domainGuid);
        AddProject("Application",    appGuid);
        AddProject("Infrastructure", infraGuid);
        AddProject("API",            apiGuid);

        sb.AppendLine("Global");
        sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
        sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
        sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
        sb.AppendLine("\tEndGlobalSection");
        sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");

        foreach (var (guid, _) in new[] { (domainGuid, "Domain"), (appGuid, "App"), (infraGuid, "Infra"), (apiGuid, "API") })
        {
            var g = $"{{{guid.ToString().ToUpper()}}}";
            sb.AppendLine($"\t\t{g}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            sb.AppendLine($"\t\t{g}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            sb.AppendLine($"\t\t{g}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            sb.AppendLine($"\t\t{g}.Release|Any CPU.Build.0 = Release|Any CPU");
        }

        sb.AppendLine("\tEndGlobalSection");
        sb.AppendLine("EndGlobal");
        return sb.ToString();
    }

    private static string BuildDomainCsproj(string ns, string tf, string version) => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{tf}</TargetFramework>
            <RootNamespace>{ns}.Domain</RootNamespace>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <Version>{version}</Version>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.EntityFrameworkCore.Abstractions" Version="8.0.0" />
          </ItemGroup>
        </Project>
        """;

    private static string BuildApplicationCsproj(string ns, string tf, string name, string version) => $"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>{tf}</TargetFramework>
            <RootNamespace>{ns}.Application</RootNamespace>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <Version>{version}</Version>
          </PropertyGroup>
          <ItemGroup>
            <ProjectReference Include="..\{name}.Domain\{name}.Domain.csproj" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="FluentValidation" Version="11.9.0" />
            <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
          </ItemGroup>
        </Project>
        """;

    private static string BuildInfrastructureCsproj(string ns, string tf, string name, string orm, string version)
    {
        var efPackages = orm == "EFCore" ? $"""
              <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
              <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
                <PrivateAssets>all</PrivateAssets>
                <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
              </PackageReference>
        """ : "";

        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>{tf}</TargetFramework>
                <RootNamespace>{ns}.Infrastructure</RootNamespace>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <Version>{version}</Version>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="..\{name}.Domain\{name}.Domain.csproj" />
                <ProjectReference Include="..\{name}.Application\{name}.Application.csproj" />
              </ItemGroup>
              <ItemGroup>
            {efPackages}
                <PackageReference Include="Dapper" Version="2.1.35" />
                <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />
              </ItemGroup>
            </Project>
            """;
    }

    private static string BuildApiCsproj(string ns, string tf, string name, string version) => $"""
        <Project Sdk="Microsoft.NET.Sdk.Web">
          <PropertyGroup>
            <TargetFramework>{tf}</TargetFramework>
            <RootNamespace>{ns}.API</RootNamespace>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <Version>{version}</Version>
          </PropertyGroup>
          <ItemGroup>
            <ProjectReference Include="..\{name}.Application\{name}.Application.csproj" />
            <ProjectReference Include="..\{name}.Infrastructure\{name}.Infrastructure.csproj" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
            <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
            <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
            <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
          </ItemGroup>
        </Project>
        """;

    // ── Helpers ───────────────────────────────────────────────

    /// <summary>
    /// GUID determinista: mismo nombre → mismo GUID siempre.
    /// Evita que el .sln aparezca como modificado en git en cada generate.
    /// </summary>
    private static Guid DeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    private static void WriteFile(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
