# ============================================================
#  AIGEN — Launcher Interactivo v2.0
#  Invoca rebuild_and_generate.ps1 con los parametros correctos
#  segun la base de datos y modo de ejecucion deseado.
#
#  Uso directo (sin menu):
#    .\aigen.ps1                                   # Menu interactivo (default)
#    .\aigen.ps1 -Db SqlServer  -Mode fast         # SqlServer rapido
#    .\aigen.ps1 -Db Postgres   -Mode nofront      # PostgreSQL sin frontend
#    .\aigen.ps1 -Db SqlServer  -Mode api          # SqlServer + RunApi
#    .\aigen.ps1 -Db SP         -Mode backend      # Stored Procedures solo backend
#    .\aigen.ps1 -Db Microservices -Mode backend   # Microservicios solo backend
#    .\aigen.ps1 -Mode menu                        # Menu interactivo explicito
# ============================================================

param(
    [ValidateSet("SqlServer", "Postgres", "Microservices", "SP")]
    [string]$Db   = "SqlServer",

    [ValidateSet("full", "fast", "nofront", "backend", "api", "compile", "menu")]
    [string]$Mode = "menu"
)

# -- Colores y helpers -------------------------------------------------------

function Header {
    Clear-Host
    Write-Host ""
    Write-Host "  +=====================================================+" -ForegroundColor Cyan
    Write-Host "  |         AIGEN - Code Generator Launcher           |" -ForegroundColor Cyan
    Write-Host "  |   .NET 8 * Angular * SQL Server * PostgreSQL      |" -ForegroundColor Cyan
    Write-Host "  |   Microservicios YARP * Stored Procedures         |" -ForegroundColor Cyan
    Write-Host "  +=====================================================+" -ForegroundColor Cyan
    Write-Host "  Version 2.0 | Semana 14" -ForegroundColor DarkCyan
    Write-Host ""
}

function Section([string]$title) {
    Write-Host "  -- $title " -ForegroundColor DarkCyan -NoNewline
    Write-Host ("-" * [Math]::Max(0, 48 - $title.Length)) -ForegroundColor DarkCyan
}

function Ok([string]$msg)   { Write-Host "  [OK]  $msg" -ForegroundColor Green  }
function Warn([string]$msg) { Write-Host "  [!!]  $msg" -ForegroundColor Yellow }
function Err([string]$msg)  { Write-Host "  [XX]  $msg" -ForegroundColor Red    }
function Info([string]$msg) { Write-Host "  [>>]  $msg" -ForegroundColor Gray   }

# -- Definicion de perfiles --------------------------------------------------
# Cada perfil describe los flags que se pasan a rebuild_and_generate.ps1

$profiles = [ordered]@{
    "1" = @{
        Label = "Completo con tests"
        Desc  = "Build * Tests * Generate * Compile * Frontend"
        Flags = @()
        Key   = "full"
    }
    "2" = @{
        Label = "Rapido sin tests"
        Desc  = "Build * Generate * Compile * Frontend  (sin tests)"
        Flags = @("-SkipTests")
        Key   = "fast"
    }
    "3" = @{
        Label = "Solo backend"
        Desc  = "Build * Generate * Compile  (sin tests, sin frontend)"
        Flags = @("-SkipTests", "-SkipFrontend")
        Key   = "backend"
    }
    "4" = @{
        Label = "Backend + levantar API"
        Desc  = "Build * Generate * Compile * dotnet run"
        Flags = @("-SkipTests", "-SkipFrontend", "-RunApi")
        Key   = "api"
    }
    "5" = @{
        Label = "Sin frontend (con tests)"
        Desc  = "Build * Tests * Generate * Compile  (sin frontend)"
        Flags = @("-SkipFrontend")
        Key   = "nofront"
    }
    "6" = @{
        Label = "Solo compilar (sin regenerar)"
        Desc  = "Build AIGEN * Compile generado  (no regenera archivos)"
        Flags = @("-SkipTests", "-SkipFrontend", "-SkipGenerate")
        Key   = "compile"
    }
}

$databases = [ordered]@{
    "1" = @{
        Label  = "SQL Server — Monolito EF Core + Dapper"
        Value  = "SqlServer"
        Desc   = "Doc4UsAIGen | crudStrategy: direct | Frontend Angular"
    }
    "2" = @{
        Label  = "PostgreSQL — Monolito EF Core + Dapper"
        Value  = "Postgres"
        Desc   = "aigen_test | crudStrategy: direct | Sin frontend"
    }
    "3" = @{
        Label  = "SQL Server — Stored Procedures"
        Value  = "SP"
        Desc   = "Doc4UsAIGen | crudStrategy: storedProcedures | Schema [API] | Prefijo PA_"
    }
    "4" = @{
        Label  = "SQL Server — Microservicios + Gateway YARP"
        Value  = "Microservices"
        Desc   = "Doc4UsAIGen | 4 servicios: Basic/Document/Parameter/Relational + Gateway"
    }
    "5" = @{
        Label  = "Ambos: SqlServer + PostgreSQL"
        Value  = "Both"
        Desc   = "Ejecuta SqlServer primero, luego Postgres — mismo modo"
    }
}

# -- Ejecutar rebuild --------------------------------------------------------

function Run-Build([string]$dbValue, [string[]]$extraFlags) {
    $script = Join-Path $PSScriptRoot "rebuild_and_generate.ps1"

    if (-not (Test-Path $script)) {
        Err "No se encontro rebuild_and_generate.ps1 en: $PSScriptRoot"
        Err "Asegurate de que aigen.ps1 este en la misma carpeta que rebuild_and_generate.ps1"
        return $false
    }

    $flagStr  = "-Db $dbValue " + ($extraFlags -join " ")

    Write-Host ""
    Section "Ejecutando"
    Info "Comando: .\rebuild_and_generate.ps1 $flagStr"
    Write-Host ""

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    & $script -Db $dbValue @extraFlags
    $exitCode = $LASTEXITCODE
    $sw.Stop()
    $elapsed = $sw.Elapsed.ToString("mm\:ss")

    Write-Host ""
    if ($exitCode -eq 0) {
        Ok "Completado en $elapsed"
    } else {
        Warn "Completado con codigo de salida $exitCode en $elapsed"
    }

    return $exitCode -eq 0
}

# -- Modo directo (sin menu) -------------------------------------------------

function Run-DirectMode {
    # Mapa de modes a flags
    $modeMap = @{
        "full"    = @()
        "fast"    = @("-SkipTests")
        "nofront" = @("-SkipFrontend")
        "backend" = @("-SkipTests", "-SkipFrontend")
        "api"     = @("-SkipTests", "-SkipFrontend", "-RunApi")
        "compile" = @("-SkipTests", "-SkipFrontend", "-SkipGenerate")
    }

    $flags = $modeMap[$Mode]

    Header
    Section "Modo directo"
    Info "DB   : $Db"
    Info "Mode : $Mode  ->  flags: $($flags -join ' ')"
    Write-Host ""

    Run-Build -dbValue $Db -extraFlags $flags | Out-Null
}

# -- Menu interactivo --------------------------------------------------------

function Show-Menu {
    Header

    # Seleccion de BD
    Section "Base de datos"
    foreach ($k in $databases.Keys) {
        $d = $databases[$k]
        Write-Host "  [$k]  $($d.Label)" -ForegroundColor White
        Write-Host "       $($d.Desc)" -ForegroundColor DarkGray
        Write-Host ""
    }
    Write-Host "  Selecciona BD [1-5]: " -ForegroundColor Yellow -NoNewline
    $dbChoice = Read-Host

    if (-not $databases.Contains($dbChoice)) {
        Err "Opcion invalida: $dbChoice"
        pause
        return
    }

    $selectedDb = $databases[$dbChoice]

    # Seleccion de perfil
    Write-Host ""
    Section "Modo de ejecucion"
    foreach ($k in $profiles.Keys) {
        $p = $profiles[$k]
        Write-Host "  [$k]  $($p.Label)" -ForegroundColor White
        Write-Host "       $($p.Desc)" -ForegroundColor DarkGray
        Write-Host ""
    }
    Write-Host "  Selecciona modo [1-6]: " -ForegroundColor Yellow -NoNewline
    $modeChoice = Read-Host

    if (-not $profiles.Contains($modeChoice)) {
        Err "Opcion invalida: $modeChoice"
        pause
        return
    }

    $selectedProfile = $profiles[$modeChoice]

    # Advertencias especificas por combinacion BD + modo
    Write-Host ""
    if ($selectedDb.Value -eq "Microservices" -and $selectedProfile.Flags -contains "-RunApi") {
        Warn "RunApi no aplica para Microservices — se ignorara (usar docker-compose)"
        $selectedProfile.Flags = $selectedProfile.Flags | Where-Object { $_ -ne "-RunApi" }
    }
    if ($selectedDb.Value -eq "SP" -and $selectedProfile.Flags -notcontains "-SkipFrontend") {
        Info "Modo SP no genera frontend — se omitira automaticamente"
    }

    # Confirmacion
    Section "Confirmacion"
    Write-Host "  BD    : $($selectedDb.Label)"                       -ForegroundColor White
    Write-Host "  Modo  : $($selectedProfile.Label)"                  -ForegroundColor White
    Write-Host "  Flags : $($selectedProfile.Flags -join ' ')"        -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  Continuar? [S/n]: " -ForegroundColor Yellow -NoNewline
    $confirm = Read-Host

    if ($confirm -eq "n" -or $confirm -eq "N") {
        Info "Cancelado."
        return
    }

    # Ejecutar — si eligio "Ambos", corre SqlServer primero luego Postgres
    if ($selectedDb.Value -eq "Both") {
        Write-Host ""
        Section "Ejecutando SqlServer"
        $ok1 = Run-Build -dbValue "SqlServer" -extraFlags $selectedProfile.Flags

        Write-Host ""
        Section "Ejecutando PostgreSQL"
        $ok2 = Run-Build -dbValue "Postgres" -extraFlags $selectedProfile.Flags

        Write-Host ""
        if ($ok1 -and $ok2)  { Ok   "Ambas BDs completadas exitosamente" }
        elseif ($ok1)         { Warn "SqlServer OK — PostgreSQL con errores" }
        elseif ($ok2)         { Warn "PostgreSQL OK — SqlServer con errores" }
        else                  { Err  "Ambas BDs fallaron" }
    }
    else {
        Run-Build -dbValue $selectedDb.Value -extraFlags $selectedProfile.Flags | Out-Null
    }

    # Pausa final
    Write-Host ""
    Write-Host "  Presiona Enter para salir..." -ForegroundColor DarkGray
    Read-Host | Out-Null
}

# -- Entry point -------------------------------------------------------------

if ($Mode -eq "menu") {
    Show-Menu
} else {
    Run-DirectMode
}
