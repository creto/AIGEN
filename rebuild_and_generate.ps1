# ============================================================
#  AIGEN - Rebuild completo y regeneracion
#  Version: 3.0 â€” Semana 10
#  Uso: .\rebuild_and_generate.ps1 [-Db SqlServer|Postgres|Microservices] [-SkipTests] [-SkipFrontend] [-RunApi]
#
#  Ejemplos:
#  .\rebuild_and_generate.ps1                              # SqlServer (default) + completo
#  .\rebuild_and_generate.ps1 -Db Postgres                # PostgreSQL
#  .\rebuild_and_generate.ps1 -Db SqlServer -SkipTests -RunApi
#  .\rebuild_and_generate.ps1 -Db Postgres  -SkipTests -SkipFrontend
# ============================================================
param(
    [ValidateSet("SqlServer", "Postgres", "Microservices")]
    [string]$Db = "SqlServer",
    [switch]$SkipTests,
    [switch]$SkipFrontend,
    [switch]$RunApi
)

cls

# â”€â”€ Configuracion por BD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$configs = @{
    SqlServer = @{
        ConfigFile   = "C:\DevOps\AIGEN\AIGEN\Generated\aigen.json"
        OutputPath   = "C:\DevOps\AIGEN\AIGEN\Generated"
        SolutionName = "Doc4Us.sln"
        ProjectName  = "Doc4Us"
        ApiFolder    = "Doc4Us.API"
        DbLabel      = "Doc4UsAIGen (SQL Server)"
        Color        = "Cyan"
    }
    Postgres  = @{
        ConfigFile   = "C:\DevOps\AIGEN\AIGEN\aigen\configs\aigen_postgres.json"
        OutputPath   = "C:\DevOps\AIGEN\AIGEN\GeneratedPostgres"
        SolutionName = "AigenTest.sln"
        ProjectName  = "AigenTest"
        ApiFolder    = "AigenTest.API"
        DbLabel      = "aigen_test (PostgreSQL 18)"
        Color        = "Magenta"
    }
    Microservices = @{
        ConfigFile   = "C:\DevOps\AIGEN\AIGEN\aigen\configs\aigen_microservices.json"
        OutputPath   = "C:\DevOps\AIGEN\AIGEN\GeneratedMicroservices"
        SolutionName = ""
        ProjectName  = "Doc4Us.Microservices"
        ApiFolder    = ""
        DbLabel      = "Doc4UsAIGen (Microservicios)"
        Color        = "Green"
    }
}

$cfg = $configs[$Db]

# â”€â”€ Header â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host "================================================"  -ForegroundColor $cfg.Color
Write-Host "  AIGEN - Rebuild + Generate + Compile + Docs"   -ForegroundColor $cfg.Color
Write-Host "  Version 3.0 | Semana 11"                        -ForegroundColor $cfg.Color
Write-Host "  Base de datos : $($cfg.DbLabel)"               -ForegroundColor $cfg.Color
Write-Host "  Config        : $($cfg.ConfigFile)"            -ForegroundColor $cfg.Color
Write-Host "  Output        : $($cfg.OutputPath)"            -ForegroundColor $cfg.Color
Write-Host "================================================"  -ForegroundColor $cfg.Color

$ErrorCount   = 0
$WarningCount = 0

# â”€â”€ 1. Limpiar generados previos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "[1/8] Limpiando archivos generados previos..." -ForegroundColor Yellow
Remove-Item "$($cfg.OutputPath)\src"      -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$($cfg.OutputPath)\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$($cfg.OutputPath)\docs"     -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK - Carpetas limpiadas" -ForegroundColor Green

# â”€â”€ 2. Rebuild AIGEN â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "[2/8] Rebuilding AIGEN..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\aigen"
dotnet restore --verbosity quiet
$buildResult = dotnet build 2>&1
$buildResult | Select-Object -Last 3
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "  ERROR: AIGEN no compilo. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - AIGEN compilado" -ForegroundColor Green

# â”€â”€ 3. Tests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
if ($SkipTests) {
    Write-Host "[3/8] Tests omitidos (flag -SkipTests)" -ForegroundColor DarkGray
} else {
    Write-Host "[3/8] Ejecutando tests..." -ForegroundColor Yellow
    $testResult = dotnet test 2>&1
    $testResult | Select-Object -Last 5
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  WARN: Algunos tests fallaron" -ForegroundColor Yellow
    } else {
        Write-Host "  OK - Tests ejecutados" -ForegroundColor Green
    }
}

# â”€â”€ 4. Regenerar proyecto â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "[4/8] Generando $($cfg.ProjectName) desde $Db..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\aigen"
dotnet run --project src/Aigen.CLI -- generate --config "$($cfg.ConfigFile)"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Generacion fallida. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Generacion completada" -ForegroundColor Green

# â”€â”€ 5. Compilar solucion Backend â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "[5/8] Compilando Backend..." -ForegroundColor Yellow
Set-Location $cfg.OutputPath

if ($Db -eq "Microservices") {
    $slns = Get-ChildItem $cfg.OutputPath -Filter "*.sln" -Recurse
    if ($slns.Count -eq 0) {
        Write-Host "  ERROR: No se encontraron .sln en $($cfg.OutputPath)" -ForegroundColor Red
        exit 1
    }
    $totalErrors = 0
    foreach ($sln in $slns) {
        Write-Host "  Compilando $($sln.Name)..." -ForegroundColor DarkCyan
        $slnResult = dotnet build $sln.FullName 2>&1
        $errLine   = $slnResult | Select-String "Error\(s\)" | Select-Object -Last 1
        $ec        = if ($errLine -match "(\d+) Error") { [int]$Matches[1] } else { 0 }
        if ($ec -gt 0) {
            Write-Host "  ERROR en $($sln.Name): $ec errores" -ForegroundColor Red
            $slnResult | Select-Object -Last 5
            $totalErrors += $ec
        } else {
            Write-Host "  OK - $($sln.Name)" -ForegroundColor Green
        }
    }
    $gwCsproj = Get-ChildItem "$($cfg.OutputPath)\Gateway" -Filter "*.csproj" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($gwCsproj) {
        Write-Host "  Compilando Gateway..." -ForegroundColor DarkCyan
        $gwResult = dotnet build $gwCsproj.FullName 2>&1
        $gwErr    = $gwResult | Select-String "Error\(s\)" | Select-Object -Last 1
        $gwEc     = if ($gwErr -match "(\d+) Error") { [int]$Matches[1] } else { 0 }
        if ($gwEc -gt 0) { $totalErrors += $gwEc }
        else { Write-Host "  OK - Gateway" -ForegroundColor Green }
    }
    $ErrorCount   = $totalErrors.ToString()
    $WarningCount = "0"
    if ($totalErrors -gt 0) {
        Write-Host "  ERROR: $totalErrors errores en microservicios. Abortando." -ForegroundColor Red
        exit 1
    }
} else {
    if (-not (Test-Path $cfg.SolutionName)) {
        Write-Host "  ERROR: No se encontro $($cfg.SolutionName)" -ForegroundColor Red
        exit 1
    }
    $slnResult    = dotnet build $cfg.SolutionName 2>&1
    $slnResult | Select-Object -Last 5
    $errLine      = $slnResult | Select-String "Error\(s\)"   | Select-Object -Last 1
    $warnLine     = $slnResult | Select-String "Warning\(s\)" | Select-Object -Last 1
    $ErrorCount   = if ($errLine  -match "(\d+) Error")   { $Matches[1] } else { "?" }
    $WarningCount = if ($warnLine -match "(\d+) Warning") { $Matches[1] } else { "?" }
    if ($ErrorCount -ne "0") {
        Write-Host "  ERROR: $ErrorCount errores backend. Abortando." -ForegroundColor Red
        exit 1
    }
}
Write-Host "  OK - Backend: $ErrorCount errores | $WarningCount warnings" -ForegroundColor Green
Write-Host "  OK - Backend: $ErrorCount errores | $WarningCount warnings" -ForegroundColor Green

# â”€â”€ 6. Build Frontend Angular â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
if ($SkipFrontend) {
    Write-Host "[6/8] Frontend omitido (flag -SkipFrontend)" -ForegroundColor DarkGray
} elseif ($Db -eq "Postgres") {
    Write-Host "[6/8] Frontend omitido (PostgreSQL config no genera frontend)" -ForegroundColor DarkGray
} else {
    Write-Host "[6/8] Compilando Frontend Angular (ng build)..." -ForegroundColor Yellow
    $frontendPath = "$($cfg.OutputPath)\frontend"

    if (-not (Test-Path $frontendPath)) {
        Write-Host "  WARN: Carpeta frontend no encontrada en $frontendPath" -ForegroundColor Yellow
    } else {
        Set-Location $frontendPath

        if (-not (Test-Path "node_modules")) {
            Write-Host "  Instalando dependencias npm..." -ForegroundColor DarkGray
            npm install --silent 2>&1 | Select-Object -Last 3
        }

        $ngResult = ng build 2>&1
        $ngErrors = $ngResult | Select-String "\[ERROR\]" | Measure-Object | Select-Object -ExpandProperty Count

        if ($ngErrors -gt 0) {
            Write-Host "  ERROR: $ngErrors errores en ng build:" -ForegroundColor Red
            $ngResult | Select-String "\[ERROR\]" | Select-Object -First 5
            exit 1
        }

        $ngResult | Select-Object -Last 5
        Write-Host "  OK - Frontend compilado sin errores" -ForegroundColor Green
    }
}

# â”€â”€ 7. Generar CODE_SUMMARY.md â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "[7/8] Generando CODE_SUMMARY.md..." -ForegroundColor Yellow

$generatedRoot = $cfg.OutputPath
$outputFile    = "$generatedRoot\CODE_SUMMARY.md"
$fecha         = Get-Date -Format "yyyy-MM-dd HH:mm"
$projectName   = $cfg.ProjectName

$codeExtensions = @("*.cs", "*.ts", "*.html", "*.scss", "*.css", "*.json", "*.scriban")

$folders = @(
    @{ Path = "src\$projectName.Domain";         Label = "Domain (Entidades C#)" },
    @{ Path = "src\$projectName.Application";    Label = "Application (DTOs + Interfaces)" },
    @{ Path = "src\$projectName.Infrastructure"; Label = "Infrastructure (Repos + Config)" },
    @{ Path = "src\$projectName.API";            Label = "API (Controllers + Program)" }
)

if ($Db -eq "SqlServer") {
    $folders += @{ Path = "frontend\src\app\features"; Label = "Angular Features (Components)" }
    $folders += @{ Path = "frontend\src\app";          Label = "Angular Core (App + Routes)" }
}

$summaryLines = @()
$summaryLines += "# Resumen de Lineas de Codigo - $projectName"
$summaryLines += ""
$summaryLines += "> Generado automaticamente por AIGEN el $fecha"
$summaryLines += "> Base de datos: $($cfg.DbLabel)"
$summaryLines += ""
$summaryLines += "---"
$summaryLines += ""
$summaryLines += "## Detalle por Capa"
$summaryLines += ""
$summaryLines += "| Capa | Archivos | Lineas de codigo |"
$summaryLines += "|------|----------|-----------------|"

$totalFiles = 0
$totalLines = 0

foreach ($folder in $folders) {
    $fullPath = Join-Path $generatedRoot $folder.Path
    if (-not (Test-Path $fullPath)) {
        $summaryLines += "| $($folder.Label) | N/A | N/A |"
        continue
    }

    $fileCount = 0
    $lineCount = 0

    foreach ($ext in $codeExtensions) {
        $files = Get-ChildItem $fullPath -Recurse -Filter $ext -ErrorAction SilentlyContinue |
                 Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\.angular\\" }
        foreach ($file in $files) {
            $fileCount++
            $lineCount += (Get-Content $file.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines
        }
    }

    $totalFiles += $fileCount
    $totalLines += $lineCount
    $summaryLines += "| $($folder.Label) | $fileCount | $("{0:N0}" -f $lineCount) |"
}

$summaryLines += "| **TOTAL** | **$totalFiles** | **$("{0:N0}" -f $totalLines)** |"
$summaryLines += ""
$summaryLines += "---"
$summaryLines += ""
$summaryLines += "## Totales Generales"
$summaryLines += ""
$summaryLines += "| Metrica | Valor |"
$summaryLines += "|---------|-------|"
$summaryLines += "| Total archivos de codigo | $totalFiles |"
$summaryLines += "| Total lineas de codigo | $("{0:N0}" -f $totalLines) |"
$summaryLines += "| Base de datos origen | $($cfg.DbLabel) |"
$summaryLines += "| Motor BD | $Db |"
$summaryLines += "| Generado por | AIGEN v3.0.0 |"
$summaryLines += "| Fecha generacion | $fecha |"
$summaryLines += ""
$summaryLines += "---"
$summaryLines += ""
$summaryLines += "*Documento generado automaticamente por AIGEN - no editar manualmente.*"
$summaryLines += "*Para regenerar: ejecutar* ``rebuild_and_generate.ps1 -Db $Db``"

$summaryLines | Out-File $outputFile -Encoding UTF8
Write-Host "  OK - CODE_SUMMARY.md generado" -ForegroundColor Green
Write-Host "       $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas totales" -ForegroundColor $cfg.Color

# â”€â”€ 8. Levantar API (opcional) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
if ($RunApi) {
    $apiPath = "$($cfg.OutputPath)\src\$($cfg.ApiFolder)"
    if (-not (Test-Path $apiPath)) {
        Write-Host "[8/8] WARN: Carpeta API no encontrada: $apiPath" -ForegroundColor Yellow
    } else {
        Write-Host "[8/8] Levantando API ($Db) en modo Development..." -ForegroundColor Yellow
        Write-Host "  Swagger : http://localhost:5000/swagger" -ForegroundColor $cfg.Color
        Write-Host "  Ctrl+C  : para detener" -ForegroundColor DarkGray
        Set-Location $apiPath
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        dotnet run
    }
} else {
    Write-Host "[8/8] API no levantada (usa -RunApi para iniciarla)" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  Para levantar manualmente:" -ForegroundColor DarkGray
    Write-Host "  cd C:\DevOps\AIGEN\AIGEN\Generated\src\Doc4Us.API" -ForegroundColor DarkGray
    Write-Host "  `$env:ASPNETCORE_ENVIRONMENT = 'Development'" -ForegroundColor DarkGray
    Write-Host "  dotnet run" -ForegroundColor DarkGray
    Set-Location "C:\DevOps\AIGEN\AIGEN\Generated\src\Doc4Us.API"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run
}

# â”€â”€ Resumen final â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host ""
Write-Host "================================================" -ForegroundColor $cfg.Color
Write-Host "  RESULTADO FINAL:" -ForegroundColor $cfg.Color
Write-Host "  BD       : $($cfg.DbLabel)" -ForegroundColor $cfg.Color
Write-Host "  Backend  : $ErrorCount errores | $WarningCount warnings" -ForegroundColor $(if ($ErrorCount -eq "0") {"Green"} else {"Red"})
if (-not $SkipFrontend -and $Db -eq "SqlServer") {
    Write-Host "  Frontend : ng build OK - 0 errores" -ForegroundColor Green
}
Write-Host "  Docs     : CODE_SUMMARY.md generado" -ForegroundColor Green
Write-Host "  Codigo   : $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor $cfg.Color
Write-Host ""
Write-Host "  Uso:" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1                                    # SqlServer completo" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -Db Postgres                       # PostgreSQL completo" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -Db SqlServer -SkipTests -RunApi   # rapido + API" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -Db Postgres  -SkipTests -SkipFrontend" -ForegroundColor DarkGray
Write-Host "================================================" -ForegroundColor $cfg.Color
