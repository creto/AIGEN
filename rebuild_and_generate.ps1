# ============================================================
#  AIGEN - Rebuild completo y regeneracion
#  Version: 3.0 — Semana 10
#  Uso: .\rebuild_and_generate.ps1 [-Db SqlServer|Postgres] [-SkipTests] [-SkipFrontend] [-RunApi]
#
#  Ejemplos:
#  .\rebuild_and_generate.ps1                              # SqlServer (default) + completo
#  .\rebuild_and_generate.ps1 -Db Postgres                # PostgreSQL
#  .\rebuild_and_generate.ps1 -Db SqlServer -SkipTests -RunApi
#  .\rebuild_and_generate.ps1 -Db Postgres  -SkipTests -SkipFrontend
# ============================================================
param(
    [ValidateSet("SqlServer", "Postgres")]
    [string]$Db = "SqlServer",
    [switch]$SkipTests,
    [switch]$SkipFrontend,
    [switch]$RunApi
)

cls

# ── Configuracion por BD ─────────────────────────────────────────────────────
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
}

$cfg = $configs[$Db]

# ── Header ───────────────────────────────────────────────────────────────────
Write-Host "================================================"  -ForegroundColor $cfg.Color
Write-Host "  AIGEN - Rebuild + Generate + Compile + Docs"   -ForegroundColor $cfg.Color
Write-Host "  Version 3.0 | Semana 10"                        -ForegroundColor $cfg.Color
Write-Host "  Base de datos : $($cfg.DbLabel)"               -ForegroundColor $cfg.Color
Write-Host "  Config        : $($cfg.ConfigFile)"            -ForegroundColor $cfg.Color
Write-Host "  Output        : $($cfg.OutputPath)"            -ForegroundColor $cfg.Color
Write-Host "================================================"  -ForegroundColor $cfg.Color

$ErrorCount   = 0
$WarningCount = 0

# ── 1. Limpiar generados previos ─────────────────────────────────────────────
Write-Host ""
Write-Host "[1/8] Limpiando archivos generados previos..." -ForegroundColor Yellow
Remove-Item "$($cfg.OutputPath)\src"      -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$($cfg.OutputPath)\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$($cfg.OutputPath)\docs"     -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK - Carpetas limpiadas" -ForegroundColor Green

# ── 2. Rebuild AIGEN ─────────────────────────────────────────────────────────
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

# ── 3. Tests ─────────────────────────────────────────────────────────────────
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

# ── 4. Regenerar proyecto ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "[4/8] Generando $($cfg.ProjectName) desde $Db..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\aigen"
dotnet run --project src/Aigen.CLI -- generate --config "$($cfg.ConfigFile)"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Generacion fallida. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Generacion completada" -ForegroundColor Green

# ── 5. Compilar solucion Backend ─────────────────────────────────────────────
Write-Host ""
Write-Host "[5/8] Compilando $($cfg.SolutionName) (Backend)..." -ForegroundColor Yellow
Set-Location $cfg.OutputPath

if (-not (Test-Path $cfg.SolutionName)) {
    Write-Host "  ERROR: No se encontro $($cfg.SolutionName) en $($cfg.OutputPath)" -ForegroundColor Red
    exit 1
}

$slnResult    = dotnet build $cfg.SolutionName 2>&1
$slnResult | Select-Object -Last 5

$errLine      = $slnResult | Select-String "Error\(s\)"   | Select-Object -Last 1
$warnLine     = $slnResult | Select-String "Warning\(s\)" | Select-Object -Last 1
$ErrorCount   = if ($errLine  -match "(\d+) Error")   { $Matches[1] } else { "?" }
$WarningCount = if ($warnLine -match "(\d+) Warning") { $Matches[1] } else { "?" }

if ($ErrorCount -ne "0") {
    Write-Host "  ERROR: $ErrorCount errores de compilacion backend. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Backend: $ErrorCount errores | $WarningCount warnings" -ForegroundColor Green

# ── 6. Build Frontend Angular ────────────────────────────────────────────────
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

# ── 7. Generar CODE_SUMMARY.md ───────────────────────────────────────────────
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

# ── 8. Levantar API (opcional) ───────────────────────────────────────────────
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

# ── Resumen final ─────────────────────────────────────────────────────────────
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
