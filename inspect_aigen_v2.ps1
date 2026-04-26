# ============================================================================
# inspect_aigen_v2.ps1 - Inspeccion COMPLETA de arquitectura AIGEN
# Para uso antes de disenar S16.2 MassTransit
# ASCII puro + BOM UTF-8
# ============================================================================

[CmdletBinding()]
param(
    [string]$AigenRoot = "C:\DevOps\AIGEN\AIGEN\aigen",
    [string]$OutputFile = ".\aigen_inspection_v2.txt"
)

try { [Console]::OutputEncoding = [System.Text.Encoding]::UTF8 } catch { }

$report = @()
function Add-Section { param($Title) 
    $script:report += ""
    $script:report += "=" * 78
    $script:report += "## $Title"
    $script:report += "=" * 78
}
function Add-Sub { param($Title)
    $script:report += ""
    $script:report += "### $Title"
    $script:report += "-" * 78
}

$report += "AIGEN - Inspeccion arquitectura para S16.2 MassTransit"
$report += "Generado: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$report += "Root: $AigenRoot"

# ============================================================================
# 1. Estructura general
# ============================================================================
Add-Section "1. ESTRUCTURA GENERAL src\"

$srcRoot = Join-Path $AigenRoot "src"
Get-ChildItem $srcRoot -Recurse -File -Filter "*.cs" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
    Sort-Object FullName |
    ForEach-Object {
        $rel = $_.FullName.Substring($srcRoot.Length + 1)
        $report += "  $rel ($($_.Length) bytes)"
    }

# ============================================================================
# 2. FileGeneratorService.cs COMPLETO (es el corazon de la integracion)
# ============================================================================
Add-Section "2. FileGeneratorService.cs (COMPLETO - max 600 lineas)"

$fgsPath = Join-Path $AigenRoot "src\Aigen.Templates\FileGeneratorService.cs"
if (Test-Path $fgsPath) {
    $report += "  Archivo: $fgsPath"
    $report += ""
    $i = 1
    Get-Content $fgsPath | ForEach-Object {
        $report += "  $('{0,4}' -f $i): $_"
        $i++
    }
} else {
    $report += "  [ERROR] No encontrado: $fgsPath"
}

# ============================================================================
# 3. ITemplateEngine
# ============================================================================
Add-Section "3. ITemplateEngine - interfaz de renderizado"

Get-ChildItem $srcRoot -Recurse -File -Filter "*TemplateEngine*.cs" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
    ForEach-Object {
        Add-Sub $_.FullName.Substring($srcRoot.Length + 1)
        $i = 1
        Get-Content $_.FullName | ForEach-Object {
            $report += "  $('{0,4}' -f $i): $_"
            $i++
        }
    }

# ============================================================================
# 4. TemplateLocator
# ============================================================================
Add-Section "4. TemplateLocator - resolucion de paths"

Get-ChildItem $srcRoot -Recurse -File -Filter "*TemplateLocator*.cs" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
    ForEach-Object {
        Add-Sub $_.FullName.Substring($srcRoot.Length + 1)
        $i = 1
        Get-Content $_.FullName | ForEach-Object {
            $report += "  $('{0,4}' -f $i): $_"
            $i++
        }
    }

# ============================================================================
# 5. ConfigModels.cs - GeneratorConfig + FeaturesConfig
# ============================================================================
Add-Section "5. ConfigModels.cs - estructura de configuracion"

$configPath = Join-Path $AigenRoot "src\Aigen.Core\Config\ConfigModels.cs"
if (Test-Path $configPath) {
    $report += "  Archivo: $configPath"
    $report += ""
    $i = 1
    Get-Content $configPath | ForEach-Object {
        $report += "  $('{0,4}' -f $i): $_"
        $i++
    }
} else {
    Add-Sub "Buscando ConfigModels.cs en otros lugares"
    Get-ChildItem $srcRoot -Recurse -File -Filter "ConfigModels*.cs" -ErrorAction SilentlyContinue |
        ForEach-Object { $report += "  Encontrado: $($_.FullName)" }
}

# ============================================================================
# 6. Plantillas backend existentes
# ============================================================================
Add-Section "6. PLANTILLAS Backend\ existentes"

$templatesPath = Join-Path $AigenRoot "src\Aigen.Templates\Templates"
if (Test-Path $templatesPath) {
    Get-ChildItem $templatesPath -Recurse -File -Filter "*.scriban" |
        Sort-Object FullName |
        ForEach-Object {
            $rel = $_.FullName.Substring($templatesPath.Length + 1)
            $report += "  $rel ($($_.Length) bytes)"
        }
}

# ============================================================================
# 7. program.scriban COMPLETO
# ============================================================================
Add-Section "7. program.scriban (COMPLETO)"

$progPath = Get-ChildItem $templatesPath -Recurse -File -Filter "program.scriban" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($progPath) {
    $report += "  Archivo: $($progPath.FullName)"
    $report += ""
    $i = 1
    Get-Content $progPath.FullName | ForEach-Object {
        $report += "  $('{0,4}' -f $i): $_"
        $i++
    }
} else {
    $report += "  [ERROR] program.scriban no encontrado"
}

# ============================================================================
# 8. appsettings.scriban COMPLETO
# ============================================================================
Add-Section "8. appsettings.scriban (COMPLETO)"

$appPath = Get-ChildItem $templatesPath -Recurse -File -Filter "appsettings*.scriban" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($appPath) {
    $report += "  Archivo: $($appPath.FullName)"
    $report += ""
    $i = 1
    Get-Content $appPath.FullName | ForEach-Object {
        $report += "  $('{0,4}' -f $i): $_"
        $i++
    }
}

# ============================================================================
# 9. Plantillas csproj
# ============================================================================
Add-Section "9. Plantillas csproj"

Get-ChildItem $templatesPath -Recurse -File -Filter "*csproj*.scriban" -ErrorAction SilentlyContinue |
    ForEach-Object {
        Add-Sub $_.Name
        $i = 1
        Get-Content $_.FullName | ForEach-Object {
            $report += "  $('{0,4}' -f $i): $_"
            $i++
        }
    }

# ============================================================================
# 10. signalr_hub.scriban (referencia del patron S16.1)
# ============================================================================
Add-Section "10. signalr_hub.scriban (referencia S16.1)"

$hubPath = Get-ChildItem $templatesPath -Recurse -File -Filter "signalr*.scriban" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($hubPath) {
    $report += "  Archivo: $($hubPath.FullName)"
    $report += ""
    $i = 1
    Get-Content $hubPath.FullName | ForEach-Object {
        $report += "  $('{0,4}' -f $i): $_"
        $i++
    }
}

# ============================================================================
# 11. aigen.json actual
# ============================================================================
Add-Section "11. aigen.json - configuracion actual"

$jsonCandidates = @(
    (Join-Path $AigenRoot "configs\aigen.json"),
    (Join-Path $AigenRoot "samples\aigen.json"),
    (Join-Path $AigenRoot "aigen.json"),
    (Join-Path $AigenRoot "src\Aigen.CLI\aigen.json")
)
$jsonFound = $false
foreach ($p in $jsonCandidates) {
    if (Test-Path $p) {
        $report += "  Archivo: $p"
        $report += ""
        $i = 1
        Get-Content $p | ForEach-Object {
            $report += "  $('{0,4}' -f $i): $_"
            $i++
        }
        $jsonFound = $true
        break
    }
}
if (-not $jsonFound) {
    $report += "  Buscando en todo el proyecto..."
    Get-ChildItem $AigenRoot -Recurse -File -Filter "aigen.json" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -notmatch "\\(bin|obj|node_modules)\\" } |
        ForEach-Object { $report += "  Encontrado: $($_.FullName)" }
}

# ============================================================================
# 12. ConfigModels duplicado o ConfigModels en otros lugares
# ============================================================================
Add-Section "12. Otros archivos de configuracion"

Get-ChildItem $srcRoot -Recurse -File -Filter "*Config*.cs" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
    ForEach-Object {
        $rel = $_.FullName.Substring($srcRoot.Length + 1)
        $report += "  $rel"
    }

# ============================================================================
# 13. csproj reales (para entender referencias entre proyectos)
# ============================================================================
Add-Section "13. .csproj reales (referencias entre proyectos)"

Get-ChildItem $srcRoot -Recurse -File -Filter "*.csproj" -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" } |
    ForEach-Object {
        Add-Sub $_.Name
        $i = 1
        Get-Content $_.FullName | ForEach-Object {
            $report += "  $('{0,4}' -f $i): $_"
            $i++
        }
    }

# ============================================================================
# Escritura
# ============================================================================
$report -join "`r`n" | Out-File -FilePath $OutputFile -Encoding UTF8

Write-Host ""
Write-Host "[OK] Reporte generado: $OutputFile" -ForegroundColor Green
Write-Host "     Tamano: $((Get-Item $OutputFile).Length) bytes" -ForegroundColor Green
Write-Host "     Lineas: $($report.Count)" -ForegroundColor Green
Write-Host ""
Write-Host "Pega el contenido completo en el chat para Fase 2 (diseno)." -ForegroundColor Cyan
Write-Host ""
