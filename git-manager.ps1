# AIGEN - Git Manager v1.0 (PowerShell)
# Uso: .\git-manager.ps1
# Primera vez: Set-ExecutionPolicy RemoteSigned -Scope CurrentUser

$repoUrl = 'https://github.com/creto/AIGEN'


function Show-Header {
    Clear-Host
    Write-Host ''
    Write-Host '  +==========================================+' -ForegroundColor Cyan
    Write-Host '  |       AIGEN - Git Manager v1.00         |' -ForegroundColor Cyan
    Write-Host '  |   https://github.com/creto/AIGEN        |' -ForegroundColor DarkGray
    Write-Host '  +==========================================+' -ForegroundColor Cyan
    Write-Host ''
    $branch = git branch --show-current 2>$null
    Write-Host '  Rama actual: ' -NoNewline
    Write-Host $branch -ForegroundColor Yellow
    Write-Host ''
}

function Show-Menu {
    Show-Header
    Write-Host '  --- SUBIR CAMBIOS ---' -ForegroundColor Green
    Write-Host '  [1]  Commit + Push   (guardar y subir cambios)'
    Write-Host '  [2]  Push            (subir commits pendientes)'
    Write-Host '  [3]  Stash           (guardar sin commit)'
    Write-Host '  [4]  Stash Pop       (recuperar ultimo stash)'
    Write-Host ''
    Write-Host '  --- TRAER CAMBIOS ---' -ForegroundColor Blue
    Write-Host '  [5]  Pull            (traer y mergear)'
    Write-Host '  [6]  Fetch           (verificar sin aplicar)'
    Write-Host '  [7]  Pull Rebase     (traer rebasando)'
    Write-Host ''
    Write-Host '  --- RAMAS ---' -ForegroundColor Magenta
    Write-Host '  [8]  Ver ramas'
    Write-Host '  [9]  Crear rama'
    Write-Host '  [10] Cambiar rama'
    Write-Host '  [11] Merge a main'
    Write-Host '  [12] Eliminar rama'
    Write-Host ''
    Write-Host '  --- ESTADO E HISTORIAL ---' -ForegroundColor Yellow
    Write-Host '  [13] Status'
    Write-Host '  [14] Log compacto (grafico)'
    Write-Host '  [15] Log detallado'
    Write-Host '  [16] Diff cambios'
    Write-Host ''
    Write-Host '  --- DESHACER ---' -ForegroundColor Red
    Write-Host '  [17] Undo ultimo commit (conserva archivos)'
    Write-Host '  [18] Undo archivo'
    Write-Host '  [19] Reset hard (CUIDADO - descarta todo)'
    Write-Host ''
    Write-Host '  --- TAGS ---' -ForegroundColor DarkCyan
    Write-Host '  [20] Ver tags'
    Write-Host '  [21] Crear tag'
    Write-Host '  [22] Push tags'
    Write-Host ''
    Write-Host '  [0]  Salir' -ForegroundColor DarkGray
    Write-Host ''
}

function Wait-Key {
    Write-Host ''
    Write-Host '  Presiona ENTER para continuar...' -ForegroundColor DarkGray
    Read-Host | Out-Null
}

function Invoke-CommitPush {
    Show-Header
    Write-Host '  COMMIT + PUSH' -ForegroundColor Green
    Write-Host '  Archivos modificados:' -ForegroundColor Yellow
    git status --short
    Write-Host ''
    $msg = Read-Host '  Mensaje del commit'
    if ([string]::IsNullOrWhiteSpace($msg)) {
        Write-Host '  ERROR: El mensaje no puede estar vacio.' -ForegroundColor Red
        Wait-Key; return
    }
    git add .
    git commit -m $msg
    if ($LASTEXITCODE -ne 0) {
        Write-Host '  No hay cambios nuevos.' -ForegroundColor Yellow
        Wait-Key; return
    }
    Write-Host '  Subiendo a GitHub...' -ForegroundColor Cyan
    git push origin main
    if ($LASTEXITCODE -eq 0) { Write-Host "  OK - $repoUrl" -ForegroundColor Green }
    else { Write-Host '  ERROR al hacer push.' -ForegroundColor Red }
    Wait-Key
}

function Invoke-Push {
    Show-Header
    Write-Host '  PUSH' -ForegroundColor Green
    git push origin main
    if ($LASTEXITCODE -eq 0) { Write-Host '  OK' -ForegroundColor Green }
    else { Write-Host '  ERROR' -ForegroundColor Red }
    Wait-Key
}

function Invoke-Stash {
    Show-Header
    Write-Host '  STASH' -ForegroundColor Green
    $smsg = Read-Host '  Descripcion (ENTER para omitir)'
    if ([string]::IsNullOrWhiteSpace($smsg)) { git stash }
    else { git stash push -m $smsg }
    git stash list
    Wait-Key
}

function Invoke-StashPop {
    Show-Header
    Write-Host '  STASH POP' -ForegroundColor Green
    git stash list
    Write-Host ''
    git stash pop
    Wait-Key
}

function Invoke-Pull {
    Show-Header
    Write-Host '  PULL' -ForegroundColor Blue
    git pull origin main
    if ($LASTEXITCODE -eq 0) { Write-Host '  OK' -ForegroundColor Green }
    else { Write-Host '  ERROR - posibles conflictos' -ForegroundColor Red }
    Wait-Key
}

function Invoke-Fetch {
    Show-Header
    Write-Host '  FETCH' -ForegroundColor Blue
    git fetch origin
    Write-Host '  Commits en GitHub pendientes:' -ForegroundColor Yellow
    git log HEAD..origin/main --oneline
    Wait-Key
}

function Invoke-PullRebase {
    Show-Header
    Write-Host '  PULL REBASE' -ForegroundColor Blue
    git pull --rebase origin main
    if ($LASTEXITCODE -eq 0) { Write-Host '  OK' -ForegroundColor Green }
    else { Write-Host '  ERROR - ejecuta: git rebase --continue' -ForegroundColor Red }
    Wait-Key
}

function Show-Branches {
    Show-Header
    Write-Host '  LOCALES:' -ForegroundColor Magenta
    git branch
    Write-Host ''
    Write-Host '  REMOTAS:' -ForegroundColor Magenta
    git branch -r
    Wait-Key
}

function New-Branch {
    Show-Header
    Write-Host '  CREAR RAMA' -ForegroundColor Magenta
    $rama = Read-Host '  Nombre (ej: feature/semana2)'
    if ([string]::IsNullOrWhiteSpace($rama)) { return }
    git checkout -b $rama
    if ($LASTEXITCODE -eq 0) {
        $push = Read-Host '  Subir a GitHub? (s/n)'
        if ($push -eq 's') { git push -u origin $rama }
    }
    Wait-Key
}

function Switch-Branch {
    Show-Header
    Write-Host '  CAMBIAR RAMA' -ForegroundColor Magenta
    git branch
    Write-Host ''
    $rama = Read-Host '  Rama destino'
    if ([string]::IsNullOrWhiteSpace($rama)) { return }
    git checkout $rama
    Wait-Key
}

function Invoke-Merge {
    Show-Header
    Write-Host '  MERGE A MAIN' -ForegroundColor Magenta
    git branch
    Write-Host ''
    $rama = Read-Host '  Rama a fusionar'
    if ([string]::IsNullOrWhiteSpace($rama)) { return }
    git checkout main
    git merge $rama
    if ($LASTEXITCODE -eq 0) {
        $push = Read-Host '  Subir a GitHub? (s/n)'
        if ($push -eq 's') { git push origin main }
    } else { Write-Host '  ERROR en merge.' -ForegroundColor Red }
    Wait-Key
}

function Remove-Branch {
    Show-Header
    Write-Host '  ELIMINAR RAMA' -ForegroundColor Magenta
    git branch
    Write-Host ''
    $rama = Read-Host '  Rama a eliminar'
    if ([string]::IsNullOrWhiteSpace($rama)) { return }
    if ($rama -eq 'main') { Write-Host '  No se puede eliminar main.' -ForegroundColor Red; Wait-Key; return }
    $confirm = Read-Host "  Confirmas eliminar '$rama'? (s/n)"
    if ($confirm -eq 's') {
        git branch -d $rama
        $del = Read-Host '  Eliminar en GitHub tambien? (s/n)'
        if ($del -eq 's') { git push origin --delete $rama }
    }
    Wait-Key
}

function Show-Status {
    Show-Header
    Write-Host '  STATUS' -ForegroundColor Yellow
    git status
    Wait-Key
}

function Show-LogShort {
    Show-Header
    Write-Host '  LOG COMPACTO' -ForegroundColor Yellow
    git log --oneline --graph --decorate -20
    Wait-Key
}

function Show-LogLong {
    Show-Header
    Write-Host '  LOG DETALLADO' -ForegroundColor Yellow
    git log --format="%h | %ad | %an | %s" --date=short -10
    Wait-Key
}

function Show-Diff {
    Show-Header
    Write-Host '  DIFF' -ForegroundColor Yellow
    git diff --stat
    Write-Host ''
    $full = Read-Host '  Ver diff completo? (s/n)'
    if ($full -eq 's') { git diff }
    Wait-Key
}

function Undo-LastCommit {
    Show-Header
    Write-Host '  UNDO ULTIMO COMMIT' -ForegroundColor Red
    git log --oneline -1
    Write-Host ''
    $confirm = Read-Host '  Confirmas? Los archivos se conservan (s/n)'
    if ($confirm -eq 's') {
        git reset --soft HEAD~1
        Write-Host '  OK - Commit deshecho.' -ForegroundColor Green
    }
    Wait-Key
}

function Undo-File {
    Show-Header
    Write-Host '  UNDO ARCHIVO' -ForegroundColor Red
    git status --short
    Write-Host ''
    $archivo = Read-Host '  Ruta del archivo'
    if ([string]::IsNullOrWhiteSpace($archivo)) { return }
    $confirm = Read-Host '  Seguro? Se perderan los cambios (s/n)'
    if ($confirm -eq 's') {
        git checkout -- $archivo
        Write-Host '  OK' -ForegroundColor Green
    }
    Wait-Key
}

function Invoke-ResetHard {
    Show-Header
    Write-Host '  RESET HARD - IRREVERSIBLE' -ForegroundColor Red
    git status --short
    Write-Host ''
    $confirm = Read-Host '  Escribe SI para confirmar'
    if ($confirm -ceq 'SI') {
        git reset --hard HEAD
        git clean -fd
        Write-Host '  OK - Repositorio limpio.' -ForegroundColor Green
    } else { Write-Host '  Cancelado.' -ForegroundColor Yellow }
    Wait-Key
}

function Show-Tags {
    Show-Header
    Write-Host '  TAGS' -ForegroundColor DarkCyan
    git tag -l --sort=-version:refname
    Wait-Key
}

function New-Tag {
    Show-Header
    Write-Host '  CREAR TAG' -ForegroundColor DarkCyan
    $vtag = Read-Host '  Version (ej: v1.0.0)'
    if ([string]::IsNullOrWhiteSpace($vtag)) { return }
    $mtag = Read-Host '  Descripcion'
    git tag -a $vtag -m $mtag
    if ($LASTEXITCODE -eq 0) {
        $push = Read-Host '  Subir a GitHub? (s/n)'
        if ($push -eq 's') { git push origin $vtag }
    } else { Write-Host '  ERROR.' -ForegroundColor Red }
    Wait-Key
}

function Push-Tags {
    Show-Header
    Write-Host '  PUSH TAGS' -ForegroundColor DarkCyan
    git push origin --tags
    if ($LASTEXITCODE -eq 0) { Write-Host '  OK' -ForegroundColor Green }
    else { Write-Host '  ERROR' -ForegroundColor Red }
    Wait-Key
}

# LOOP PRINCIPAL
while ($true) {
    Show-Menu
    $opcion = Read-Host '  Selecciona una opcion'
    switch ($opcion) {
        '1'  { Invoke-CommitPush }
        '2'  { Invoke-Push }
        '3'  { Invoke-Stash }
        '4'  { Invoke-StashPop }
        '5'  { Invoke-Pull }
        '6'  { Invoke-Fetch }
        '7'  { Invoke-PullRebase }
        '8'  { Show-Branches }
        '9'  { New-Branch }
        '10' { Switch-Branch }
        '11' { Invoke-Merge }
        '12' { Remove-Branch }
        '13' { Show-Status }
        '14' { Show-LogShort }
        '15' { Show-LogLong }
        '16' { Show-Diff }
        '17' { Undo-LastCommit }
        '18' { Undo-File }
        '19' { Invoke-ResetHard }
        '20' { Show-Tags }
        '21' { New-Tag }
        '22' { Push-Tags }
        '0'  { Clear-Host; Write-Host '  Hasta luego!' -ForegroundColor Cyan; exit }
        default { Write-Host '  Opcion no valida.' -ForegroundColor Red; Start-Sleep -Seconds 1 }
    }
}