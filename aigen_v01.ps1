# ============================================================
#  AIGEN — Launcher Interactivo v1.0
#  Invoca rebuild_and_generate.ps1 con los parámetros correctos
#  según la base de datos y modo de ejecución deseado.
#
#  Uso directo (sin menú):
#    .\aigen.ps1 -Mode full                        # SqlServer completo
#    .\aigen.ps1 -Mode fast                        # SqlServer rápido
#    .\aigen.ps1 -Db Postgres -Mode full           # PostgreSQL completo
#    .\aigen.ps1 -Db Postgres -Mode nofront        # PostgreSQL sin frontend
#    .\aigen.ps1 -Db SqlServer -Mode api           # SqlServer + RunApi
#    .\aigen.ps1 -Mode menu                        # Menú interactivo (default)
# ============================================================

param(
    [ValidateSet("SqlServer", "Postgres")]
    [string]$Db   = "SqlServer",

    [ValidateSet("full", "fast", "nofront", "api", "menu")]
    [string]$Mode = "menu"
)

# ── Colores y helpers ────────────────────────────────────────────────────────

function Header {
    Clear-Host
    Write-Host ""
    Write-Host "  ╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "  ║          AIGEN — Code Generator Launcher            ║" -ForegroundColor Cyan
    Write-Host "  ║          .NET 8 · Angular · SQL Server · PostgreSQL ║" -ForegroundColor Cyan
    Write-Host "  ╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Section([string]$title) {
    Write-Host "  ── $title " -ForegroundColor DarkCyan -NoNewline
    Write-Host ("─" * [Math]::Max(0, 50 - $title.Length)) -ForegroundColor DarkCyan
}

function Ok([string]$msg)   { Write-Host "  ✅  $msg" -ForegroundColor Green  }
function Warn([string]$msg) { Write-Host "  ⚠️   $msg" -ForegroundColor Yellow }
function Err([string]$msg)  { Write-Host "  ❌  $msg" -ForegroundColor Red    }
function Info([string]$msg) { Write-Host "  ℹ️   $msg" -ForegroundColor Gray   }

# ── Definición de perfiles ───────────────────────────────────────────────────
# Cada perfil describe los flags que se pasan a rebuild_and_generate.ps1

$profiles = [ordered]@{
    "1" = @{
        Label   = "Completo con tests"
        Desc    = "Build · Tests · Generate · Compile · Frontend"
        Flags   = @()
        Emoji   = "🔵"
    }
    "2" = @{
        Label   = "Rápido sin tests"
        Desc    = "Build · Generate · Compile · Frontend  (sin tests)"
        Flags   = @("-SkipTests")
        Emoji   = "⚡"
    }
    "3" = @{
        Label   = "Solo backend"
        Desc    = "Build · Generate · Compile  (sin tests, sin frontend)"
        Flags   = @("-SkipTests", "-SkipFrontend")
        Emoji   = "🔧"
    }
    "4" = @{
        Label   = "Backend + levantar API"
        Desc    = "Build · Generate · Compile · dotnet run"
        Flags   = @("-SkipTests", "-SkipFrontend", "-RunApi")
        Emoji   = "🚀"
    }
    "5" = @{
        Label   = "Solo compilar (sin generar)"
        Desc    = "Build · Compile generado  (sin regenerar archivos)"
        Flags   = @("-SkipTests", "-SkipFrontend", "-SkipGenerate")
        Emoji   = "🔨"
    }
}

$databases = [ordered]@{
    "1" = @{ Label = "SQL Server";  Value = "SqlServer"; Emoji = "🗄️ " }
    "2" = @{ Label = "PostgreSQL";  Value = "Postgres";  Emoji = "🐘" }
    "3" = @{ Label = "Ambos (SqlServer primero, luego Postgres)"; Value = "Both"; Emoji = "🔄" }
}

# ── Ejecutar rebuild ─────────────────────────────────────────────────────────

function Run-Build([string]$dbValue, [string[]]$extraFlags) {
    $script = Join-Path $PSScriptRoot "rebuild_and_generate.ps1"

    if (-not (Test-Path $script)) {
        Err "No se encontró rebuild_and_generate.ps1 en: $PSScriptRoot"
        Err "Asegúrate de que aigen.ps1 está en la misma carpeta que rebuild_and_generate.ps1"
        return $false
    }

    $allFlags = @("-Db", $dbValue) + $extraFlags
    $flagStr  = $allFlags -join " "

    Write-Host ""
    Section "Ejecutando"
    Info "Comando: .\rebuild_and_generate.ps1 $flagStr"
    Write-Host ""

    $sw = [System.Diagnostics.Stopwatch]::StartNew()

    & $script @allFlags
    $exitCode = $LASTEXITCODE

    $sw.Stop()
    $elapsed = $sw.Elapsed.ToString("mm\:ss")

    Write-Host ""
    if ($exitCode -eq 0) {
        Ok "Completado en $elapsed"
    } else {
        Warn "Completado con código de salida $exitCode en $elapsed"
    }

    return $exitCode -eq 0
}

# ── Modo directo (sin menú) ──────────────────────────────────────────────────

function Run-DirectMode {
    $modeMap = @{
        "full"    = @()
        "fast"    = @("-SkipTests")
        "nofront" = @("-SkipTests", "-SkipFrontend")
        "api"     = @("-SkipTests", "-SkipFrontend", "-RunApi")
    }

    $flags = $modeMap[$Mode]

    Header
    Section "Modo directo"
    Info "DB   : $Db"
    Info "Mode : $Mode  →  flags: $($flags -join ' ')"
    Write-Host ""

    if ($Db -eq "SqlServer" -or $Db -eq "Postgres") {
        Run-Build -dbValue $Db -extraFlags $flags | Out-Null
    }
}

# ── Menú interactivo ─────────────────────────────────────────────────────────

function Show-Menu {
    Header

    # Selección de BD
    Section "Base de datos"
    foreach ($k in $databases.Keys) {
        $d = $databases[$k]
        Write-Host "  $($d.Emoji)  [$k]  $($d.Label)" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "  Selecciona BD [1-3]: " -ForegroundColor Yellow -NoNewline
    $dbChoice = Read-Host

    if (-not $databases.ContainsKey($dbChoice)) {
        Err "Opción inválida: $dbChoice"
        return
    }

    $selectedDb = $databases[$dbChoice]

    # Selección de perfil
    Write-Host ""
    Section "Modo de ejecución"
    foreach ($k in $profiles.Keys) {
        $p = $profiles[$k]
        Write-Host "  $($p.Emoji)  [$k]  $($p.Label)" -ForegroundColor White
        Write-Host "       $($p.Desc)" -ForegroundColor DarkGray
    }
    Write-Host ""
    Write-Host "  Selecciona modo [1-5]: " -ForegroundColor Yellow -NoNewline
    $modeChoice = Read-Host

    if (-not $profiles.ContainsKey($modeChoice)) {
        Err "Opción inválida: $modeChoice"
        return
    }

    $selectedProfile = $profiles[$modeChoice]

    # Confirmación
    Write-Host ""
    Section "Confirmación"
    Write-Host "  BD    : $($selectedDb.Emoji) $($selectedDb.Label)" -ForegroundColor White
    Write-Host "  Modo  : $($selectedProfile.Emoji) $($selectedProfile.Label)" -ForegroundColor White
    Write-Host "  Flags : $($selectedProfile.Flags -join ' ')" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "  ¿Continuar? [S/n]: " -ForegroundColor Yellow -NoNewline
    $confirm = Read-Host

    if ($confirm -eq "n" -or $confirm -eq "N") {
        Info "Cancelado."
        return
    }

    # Ejecutar — si eligió "Ambos", corre SqlServer primero luego Postgres
    if ($selectedDb.Value -eq "Both") {
        Write-Host ""
        Section "Ejecutando SqlServer"
        $ok1 = Run-Build -dbValue "SqlServer" -extraFlags $selectedProfile.Flags

        Write-Host ""
        Section "Ejecutando PostgreSQL"
        $ok2 = Run-Build -dbValue "Postgres"  -extraFlags $selectedProfile.Flags

        Write-Host ""
        if ($ok1 -and $ok2) { Ok  "Ambas BDs completadas exitosamente" }
        elseif ($ok1)        { Warn "SqlServer OK — PostgreSQL con errores" }
        elseif ($ok2)        { Warn "PostgreSQL OK — SqlServer con errores" }
        else                 { Err  "Ambas BDs fallaron" }
    }
    else {
        Run-Build -dbValue $selectedDb.Value -extraFlags $selectedProfile.Flags | Out-Null
    }

    # Pausa final para ver resultado antes de cerrar
    Write-Host ""
    Write-Host "  Presiona Enter para salir..." -ForegroundColor DarkGray
    Read-Host | Out-Null
}

# ── Entry point ──────────────────────────────────────────────────────────────

if ($Mode -eq "menu") {
    Show-Menu
} else {
    Run-DirectMode
}
