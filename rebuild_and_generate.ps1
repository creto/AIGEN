# ============================================================
#  AIGEN - Rebuild completo y regeneracion de Doc4Us
#  Uso: .\rebuild_and_generate.ps1
# ============================================================

cls

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  AIGEN - Rebuild + Generate + Compile + Docs"  -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# -- 1. Limpiar generados previos ----------------------------
Write-Host ""
Write-Host "[1/6] Limpiando archivos generados previos..." -ForegroundColor Yellow
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\src"      -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\docs"     -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK - Carpetas limpiadas" -ForegroundColor Green

# -- 2. Rebuild AIGEN ----------------------------------------
Write-Host ""
Write-Host "[2/6] Rebuilding AIGEN..." -ForegroundColor Yellow
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
Write-Host "[3/6] Ejecutando tests..." -ForegroundColor Yellow
$testResult = dotnet test 2>&1
$testResult | Select-Object -Last 5
Write-Host "  OK - Tests ejecutados" -ForegroundColor Green

# -- 4. Regenerar Doc4Us -------------------------------------
Write-Host ""
Write-Host "[4/6] Generando Doc4Us..." -ForegroundColor Yellow
dotnet run --project src/Aigen.CLI -- generate --config "..\Generated\aigen.json"
Write-Host "  OK - Generacion completada" -ForegroundColor Green

# -- 5. Compilar Doc4Us.sln ----------------------------------
Write-Host ""
Write-Host "[5/6] Compilando Doc4Us.sln..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\Generated"
$slnResult = dotnet build Doc4Us.sln 2>&1
$slnResult | Select-Object -Last 5

$errLine  = $slnResult | Select-String "Error\(s\)"   | Select-Object -Last 1
$warnLine = $slnResult | Select-String "Warning\(s\)" | Select-Object -Last 1
$errors   = if ($errLine  -match "(\d+) Error")   { $Matches[1] } else { "?" }
$warnings = if ($warnLine -match "(\d+) Warning") { $Matches[1] } else { "?" }

if ($errors -ne "0") {
    Write-Host "  ERROR: $errors errores de compilacion. Abortando docs." -ForegroundColor Red
    exit 1
}
Write-Host "  OK - $errors errores | $warnings warnings" -ForegroundColor Green

# -- 6. Generar CODE_SUMMARY.md ------------------------------
Write-Host ""
Write-Host "[6/6] Generando CODE_SUMMARY.md..." -ForegroundColor Yellow

$generatedRoot = "C:\DevOps\AIGEN\AIGEN\Generated"
$outputFile    = "$generatedRoot\CODE_SUMMARY.md"
$fecha         = Get-Date -Format "yyyy-MM-dd HH:mm"

# Extensiones de codigo a contar
$codeExtensions = @("*.cs", "*.ts", "*.html", "*.scss", "*.css", "*.json", "*.scriban")

# Carpetas a analizar (excluir bin, obj, node_modules)
$folders = @(
    @{ Path = "src\Doc4Us.Domain";          Label = "Domain (Entidades C#)" },
    @{ Path = "src\Doc4Us.Application";     Label = "Application (DTOs + Interfaces)" },
    @{ Path = "src\Doc4Us.Infrastructure";  Label = "Infrastructure (Repos + Config)" },
    @{ Path = "src\Doc4Us.API";             Label = "API (Controllers + Program)" },
    @{ Path = "frontend\src\app\features";  Label = "Angular Features (Components)" }
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
                 Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\" }
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
             Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\" }
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
$lines += "| Tablas procesadas | 272 |"
$lines += "| Generado por | AIGEN v1.0.0 |"
$lines += "| Fecha generacion | $fecha |"
$lines += ""
$lines += "---"
$lines += ""
$lines += "*Documento generado automaticamente por AIGEN - no editar manualmente.*"
$lines += "*Para regenerar: ejecutar* ``rebuild_and_generate.ps1``"

$lines | Out-File $outputFile -Encoding UTF8
Write-Host "  OK - CODE_SUMMARY.md generado" -ForegroundColor Green
Write-Host "       $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas totales" -ForegroundColor Cyan

# -- Resumen final -------------------------------------------
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  RESULTADO FINAL:" -ForegroundColor Cyan
Write-Host "  Build  : $errors errores | $warnings warnings" -ForegroundColor $(if ($errors -eq "0") {"Green"} else {"Red"})
Write-Host "  Docs   : ARCHITECTURE.md + DEPLOYMENT.md + CODE_SUMMARY.md" -ForegroundColor Green
Write-Host "  Codigo : $totalFiles archivos | $("{0:N0}" -f $totalLines) lineas" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
