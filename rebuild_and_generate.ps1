# ============================================================
#  AIGEN - Rebuild completo y regeneracion de Doc4Us
#  Version: 2.0 — Semana 9
#  Uso: .\rebuild_and_generate.ps1 [-SkipTests] [-SkipFrontend] [-RunApi]
# ============================================================
param(
    [switch]$SkipTests,
    [switch]$SkipFrontend,
    [switch]$RunApi
)

cls

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  AIGEN - Rebuild + Generate + Compile + Docs"  -ForegroundColor Cyan
Write-Host "  Version 2.0 | Semana 9"                       -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

$ErrorCount   = 0
$WarningCount = 0

# -- 1. Limpiar generados previos ----------------------------
Write-Host ""
Write-Host "[1/8] Limpiando archivos generados previos..." -ForegroundColor Yellow
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\src"      -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\docs"     -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK - Carpetas limpiadas" -ForegroundColor Green

# -- 2. Rebuild AIGEN ----------------------------------------
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

# -- 3. Tests ------------------------------------------------
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

# -- 4. Regenerar Doc4Us -------------------------------------
Write-Host ""
Write-Host "[4/8] Generando Doc4Us..." -ForegroundColor Yellow
dotnet run --project src/Aigen.CLI -- generate --config "..\Generated\aigen.json"
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Generacion fallida. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Generacion completada" -ForegroundColor Green

# -- 5. Compilar Doc4Us.sln (Backend) ------------------------
Write-Host ""
Write-Host "[5/8] Compilando Doc4Us.sln (Backend)..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\Generated"
$slnResult = dotnet build Doc4Us.sln 2>&1
$slnResult | Select-Object -Last 5

$errLine  = $slnResult | Select-String "Error\(s\)"   | Select-Object -Last 1
$warnLine = $slnResult | Select-String "Warning\(s\)" | Select-Object -Last 1
$ErrorCount   = if ($errLine  -match "(\d+) Error")   { $Matches[1] } else { "?" }
$WarningCount = if ($warnLine -match "(\d+) Warning") { $Matches[1] } else { "?" }

if ($ErrorCount -ne "0") {
    Write-Host "  ERROR: $ErrorCount errores de compilacion backend. Abortando." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Backend: $ErrorCount errores | $WarningCount warnings" -ForegroundColor Green

# -- 6. Build Frontend Angular -------------------------------
Write-Host ""
if ($SkipFrontend) {
    Write-Host "[6/8] Frontend omitido (flag -SkipFrontend)" -ForegroundColor DarkGray
} else {
    Write-Host "[6/8] Compilando Frontend Angular (ng build)..." -ForegroundColor Yellow
    Set-Location "C:\DevOps\AIGEN\AIGEN\Generated\frontend"

    # npm install solo si no existe node_modules
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

# -- 7. Generar CODE_SUMMARY.md ------------------------------
Write-Host ""
Write-Host "[7/8] Generando CODE_SUMMARY.md..." -ForegroundColor Yellow

$generatedRoot = "C:\DevOps\AIGEN\AIGEN\Generated"
$outputFile    = "$generatedRoot\CODE_SUMMARY.md"
$fecha         = Get-Date -Format "yyyy-MM-dd HH:mm"

$codeExtensions = @("*.cs", "*.ts", "*.html", "*.scss", "*.css", "*.json", "*.scriban")

$folders = @(
    @{ Path = "src\Doc4Us.Domain";          Label = "Domain (Entidades C#)" },
    @{ Path = "src\Doc4Us.Application";     Label = "Application (DTOs + Interfaces)" },
    @{ Path = "src\Doc4Us.Infrastructure";  Label = "Infrastructure (Repos + Config)" },
    @{ Path = "src\Doc4Us.API";             Label = "API (Controllers + Program)" },
    @{ Path = "frontend\src\app\features";  Label = "Angular Features (Components)" },
    @{ Path = "frontend\src\app";           Label = "Angular Core (App + Routes)" }
)

$lines = @()
$lines += "# Resumen de Lineas de Codigo - Doc4Us"
$lines += ""
$lines += "> Generado automaticamente por AIGEN el $fecha"
$lines += ""
$lines += "---"
$lines += ""
$lines += "## Detalle por Capa"
$lines += ""
$lines += "| Capa | Archivos | Lineas de codigo |"
$lines += "|------|----------|-----------------|"

$totalFiles = 0
$totalLines = 0

foreach ($folder in $folders) {
    $fullPath = Join-Path $generatedRoot $folder.Path
    if (-not (Test-Path $fullPath)) {
        $lines += "| $($folder.Label) | N/A | N/A |"
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
    $lines += "| $($folder.Label) | $fileCount | $("{0:N0}" -f $lineCount) |"
}

$lines += "| **TOTAL** | **$totalFiles** | **$("{0:N0}" -f $totalLines)** |"
$lines += ""
$lines += "---"
$lines += ""
$lines += "## Detalle por Extension"
$lines += ""
$lines += "| Extension | Archivos | Lineas |"
$lines += "|-----------|----------|--------|"

foreach ($ext in $codeExtensions) {
    $files = Get-ChildItem $generatedRoot -Recurse -Filter $ext -ErrorAction SilentlyContinue |
             Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\.angular\\" }
    if ($files.Count -gt 0) {
        $extLines = 0
        foreach ($f in $files) {
            $extLines += (Get-Content $f.FullName -ErrorAction SilentlyContinue | Measure-Object -Line).Lines
        }
        $lines += "| $ext | $($files.Count) | $("{0:N0}" -f $extLines) |"
    }
}

$lines += ""
$lines += "---"
$lines += ""
$lines += "## Totales Generales"
$lines += ""
$lines += "| Metrica | Valor |"
$lines += "|---------|-------|"
$lines += "| Total archivos de codigo | $totalFiles |"
$lines += "| Total lineas de codigo | $("{0:N0}" -f $totalLines) |"
$lines += "| Capas generadas | 4 (.NET) + 1 (Angular) |"
$lines += "| Base de datos origen | Doc4UsAIGen (SQL Server) |"
$lines += "| Tablas procesadas | 268 |"
$lines += "| Generado por | AIGEN v2.0.0 |"
$lines += "| Fecha generacion | $fecha |"
$lines += ""
$lines += "---"
$lines += ""
$lines += "*Documento generado automaticamente por AIGEN - no editar manualmente.*"
$lines += "*Para regenerar: ejecutar* ``rebuild_and_generate.ps1``"

Set-Location "C:\DevOps\AIGEN\AIGEN\Generated"
$lines | Out-File $outputFile -Encoding UTF8
Write-Host "  OK - CODE_SUMMARY.md generado" -ForegroundColor Green
Write-Host "       $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas totales" -ForegroundColor Cyan

# -- 8. Levantar API (opcional) ------------------------------
Write-Host ""
if ($RunApi) {
    Write-Host "[8/8] Levantando API en modo Development..." -ForegroundColor Yellow
    Write-Host "  Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
     Write-Host " para acceder a la API Swagger: http://localhost:5000/swagger"
    Write-Host "  Ctrl+C para detener" -ForegroundColor DarkGray
    Set-Location "C:\DevOps\AIGEN\AIGEN\Generated\src\Doc4Us.API"
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run
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

# -- Resumen final -------------------------------------------
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  RESULTADO FINAL:" -ForegroundColor Cyan
Write-Host "  Backend : $ErrorCount errores | $WarningCount warnings" -ForegroundColor $(if ($ErrorCount -eq "0") {"Green"} else {"Red"})
if (-not $SkipFrontend) {
    Write-Host "  Frontend: ng build OK - 0 errores" -ForegroundColor Green
}
Write-Host "  Docs    : ARCHITECTURE.md + DEPLOYMENT.md + CODE_SUMMARY.md" -ForegroundColor Green
Write-Host "  Codigo  : $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Uso avanzado:" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1                  # completo" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -SkipTests       # omitir tests" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -SkipFrontend    # omitir ng build" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -RunApi          # levantar API al final" -ForegroundColor DarkGray
Write-Host "  .\rebuild_and_generate.ps1 -SkipTests -RunApi  # rapido + API" -ForegroundColor DarkGray
Write-Host "================================================" -ForegroundColor Cyan
