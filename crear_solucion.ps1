# ============================================================
#  AIGEN - Crear estructura de solución .NET
#  Ejecutar desde: C:\DevOps\AIGEN\AIGEN\aigen\
#  Prerequisito: haber ejecutado aigen generate primero
# ============================================================

param(
    [string]$Generated = "C:\DevOps\AIGEN\AIGEN\Generated",
    [string]$ProjectName = "Doc4Us",
    [string]$RootNamespace = "Doc4us.SGDEA",
    [string]$DotnetVersion = "net8.0",
    [string]$Version = "1.0.0"
)

$src = "$Generated\src"

Write-Host "`nCreando estructura de solucion .NET para $ProjectName..." -ForegroundColor Cyan

# ── 1. Solution file ──────────────────────────────────────────
$domainGuid = [System.Guid]::NewGuid().ToString().ToUpper()
$appGuid    = [System.Guid]::NewGuid().ToString().ToUpper()
$infraGuid  = [System.Guid]::NewGuid().ToString().ToUpper()
$apiGuid    = [System.Guid]::NewGuid().ToString().ToUpper()

$sln = @"

Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 17
VisualStudioVersion = 17.0.31903.59
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$ProjectName.Domain", "src\$ProjectName.Domain\$ProjectName.Domain.csproj", "{$domainGuid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$ProjectName.Application", "src\$ProjectName.Application\$ProjectName.Application.csproj", "{$appGuid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$ProjectName.Infrastructure", "src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj", "{$infraGuid}"
EndProject
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "$ProjectName.API", "src\$ProjectName.API\$ProjectName.API.csproj", "{$apiGuid}"
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{$domainGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$domainGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$domainGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$domainGuid}.Release|Any CPU.Build.0 = Release|Any CPU
		{$appGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$appGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$appGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$appGuid}.Release|Any CPU.Build.0 = Release|Any CPU
		{$infraGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$infraGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$infraGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$infraGuid}.Release|Any CPU.Build.0 = Release|Any CPU
		{$apiGuid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{$apiGuid}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{$apiGuid}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{$apiGuid}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
"@
$sln | Set-Content "$Generated\$ProjectName.sln" -Encoding UTF8
Write-Host "  OK  $ProjectName.sln" -ForegroundColor Green

# ── 2. Domain.csproj ──────────────────────────────────────────
$domainCsproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$DotnetVersion</TargetFramework>
    <RootNamespace>$RootNamespace.Domain</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Version>$Version</Version>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>
"@
New-Item -ItemType Directory -Force -Path "$src\$ProjectName.Domain" | Out-Null
$domainCsproj | Set-Content "$src\$ProjectName.Domain\$ProjectName.Domain.csproj" -Encoding UTF8
Write-Host "  OK  $ProjectName.Domain.csproj" -ForegroundColor Green

# ── 3. Application.csproj ─────────────────────────────────────
$appCsproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$DotnetVersion</TargetFramework>
    <RootNamespace>$RootNamespace.Application</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Version>$Version</Version>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\$ProjectName.Domain\$ProjectName.Domain.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  </ItemGroup>
</Project>
"@
New-Item -ItemType Directory -Force -Path "$src\$ProjectName.Application" | Out-Null
$appCsproj | Set-Content "$src\$ProjectName.Application\$ProjectName.Application.csproj" -Encoding UTF8
Write-Host "  OK  $ProjectName.Application.csproj" -ForegroundColor Green

# ── 4. Infrastructure.csproj ──────────────────────────────────
$infraCsproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$DotnetVersion</TargetFramework>
    <RootNamespace>$RootNamespace.Infrastructure</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Version>$Version</Version>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\$ProjectName.Domain\$ProjectName.Domain.csproj" />
    <ProjectReference Include="..\$ProjectName.Application\$ProjectName.Application.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Dapper" Version="2.1.35" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.0" />
  </ItemGroup>
</Project>
"@
New-Item -ItemType Directory -Force -Path "$src\$ProjectName.Infrastructure" | Out-Null
$infraCsproj | Set-Content "$src\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj" -Encoding UTF8
Write-Host "  OK  $ProjectName.Infrastructure.csproj" -ForegroundColor Green

# ── 5. API.csproj ─────────────────────────────────────────────
$apiCsproj = @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>$DotnetVersion</TargetFramework>
    <RootNamespace>$RootNamespace.API</RootNamespace>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <Version>$Version</Version>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\$ProjectName.Application\$ProjectName.Application.csproj" />
    <ProjectReference Include="..\$ProjectName.Infrastructure\$ProjectName.Infrastructure.csproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
  </ItemGroup>
</Project>
"@
New-Item -ItemType Directory -Force -Path "$src\$ProjectName.API" | Out-Null
$apiCsproj | Set-Content "$src\$ProjectName.API\$ProjectName.API.csproj" -Encoding UTF8
Write-Host "  OK  $ProjectName.API.csproj" -ForegroundColor Green

# ── 6. Mover archivos .cs a las carpetas de proyecto ──────────
Write-Host "`nMoviendo archivos a carpetas de proyecto..." -ForegroundColor Cyan

# Domain: Entities
$domainEntities = "$src\$ProjectName.Domain\Entities"
if (Test-Path "$src\Doc4Us.Domain\Entities") {
    if (!(Test-Path $domainEntities)) {
        Move-Item "$src\Doc4Us.Domain\Entities" $domainEntities
    }
    Write-Host "  OK  Domain\Entities movido" -ForegroundColor Green
}

Write-Host "`n====================================" -ForegroundColor Cyan
Write-Host "  Solucion creada exitosamente!" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "`nPara compilar:" -ForegroundColor Yellow
Write-Host "  cd $Generated" -ForegroundColor White
Write-Host "  dotnet build $ProjectName.sln" -ForegroundColor White
