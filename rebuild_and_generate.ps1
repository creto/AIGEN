# ============================================================
#  AIGEN - Rebuild completo y regeneracion de Doc4Us
#  Uso: .\rebuild_and_generate.ps1
# ============================================================

cls

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  AIGEN - Rebuild + Generate + Compile"         -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# -- 1. Limpiar generados previos ----------------------------
Write-Host ""
Write-Host "[1/5] Limpiando archivos generados previos..." -ForegroundColor Yellow
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\src"      -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\docs"     -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK - Carpetas limpiadas" -ForegroundColor Green

# -- 2. Rebuild AIGEN ----------------------------------------
Write-Host ""
Write-Host "[2/5] Rebuilding AIGEN..." -ForegroundColor Yellow
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
Write-Host "[3/5] Ejecutando tests..." -ForegroundColor Yellow
$testResult = dotnet test 2>&1
$testResult | Select-Object -Last 5
Write-Host "  OK - Tests ejecutados" -ForegroundColor Green

# -- 4. Regenerar Doc4Us -------------------------------------
Write-Host ""
Write-Host "[4/5] Generando Doc4Us..." -ForegroundColor Yellow
dotnet run --project src/Aigen.CLI -- generate --config "..\Generated\aigen.json"
Write-Host "  OK - Generacion completada" -ForegroundColor Green

# -- 5. Compilar Doc4Us.sln ----------------------------------
Write-Host ""
Write-Host "[5/5] Compilando Doc4Us.sln..." -ForegroundColor Yellow
Set-Location "C:\DevOps\AIGEN\AIGEN\Generated"
$slnResult = dotnet build Doc4Us.sln 2>&1
$slnResult | Select-Object -Last 10

# -- Resumen final -------------------------------------------
$errLine  = $slnResult | Select-String "Error\(s\)"   | Select-Object -Last 1
$warnLine = $slnResult | Select-String "Warning\(s\)" | Select-Object -Last 1

$errors   = if ($errLine  -match "(\d+) Error")   { $Matches[1] } else { "?" }
$warnings = if ($warnLine -match "(\d+) Warning") { $Matches[1] } else { "?" }

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
if ($errors -eq "0") {
    Write-Host "  RESULTADO: $errors errores | $warnings warnings - OK" -ForegroundColor Green
} else {
    Write-Host "  RESULTADO: $errors errores | $warnings warnings - FALLO" -ForegroundColor Red
}
Write-Host "================================================" -ForegroundColor Cyan
