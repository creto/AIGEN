@echo off
REM ================================================
REM AIGEN - Git Manager
REM Gestiona tu repositorio GitHub desde consola
REM ================================================
chcp 65001 > nul
color 0B

:MENU
cls
echo.
echo  ╔══════════════════════════════════════════╗
echo  ║         AIGEN - Git Manager v1.0         ║
echo  ║     https://github.com/creto/AIGEN       ║
echo  ╚══════════════════════════════════════════╝
echo.
echo  --- SUBIR CAMBIOS ----------------------------
echo  [1] Commit + Push  (guardar y subir)
echo  [2] Push           (subir commits locales)
echo  [3] Stash          (guardar cambios sin commit)
echo  [4] Stash Pop      (recuperar stash)
echo.
echo  --- TRAER CAMBIOS ----------------------------
echo  [5] Pull           (traer y mergear)
echo  [6] Fetch          (traer sin mergear)
echo  [7] Pull --rebase  (traer rebasando)
echo.
echo  --- RAMAS ------------------------------------
echo  [8] Ver ramas
echo  [9] Crear rama
echo  [10] Cambiar rama
echo  [11] Merge rama a main
echo  [12] Eliminar rama
echo.
echo  --- ESTADO E HISTORIAL -----------------------
echo  [13] Status
echo  [14] Log compacto
echo  [15] Log detallado
echo  [16] Diff cambios actuales
echo.
echo  --- DESHACER ---------------------------------
echo  [17] Deshacer ultimo commit (mantiene cambios)
echo  [18] Deshacer cambios de un archivo
echo  [19] Reset hard al ultimo commit
echo.
echo  --- TAGS -------------------------------------
echo  [20] Ver tags
echo  [21] Crear tag de version
echo  [22] Push tags a GitHub
echo.
echo  [0] Salir
echo.
set /p opcion="  Selecciona una opcion: "

if "%opcion%"=="1"  goto COMMIT_PUSH
if "%opcion%"=="2"  goto PUSH
if "%opcion%"=="3"  goto STASH
if "%opcion%"=="4"  goto STASH_POP
if "%opcion%"=="5"  goto PULL
if "%opcion%"=="6"  goto FETCH
if "%opcion%"=="7"  goto PULL_REBASE
if "%opcion%"=="8"  goto VER_RAMAS
if "%opcion%"=="9"  goto CREAR_RAMA
if "%opcion%"=="10" goto CAMBIAR_RAMA
if "%opcion%"=="11" goto MERGE_RAMA
if "%opcion%"=="12" goto ELIMINAR_RAMA
if "%opcion%"=="13" goto STATUS
if "%opcion%"=="14" goto LOG_CORTO
if "%opcion%"=="15" goto LOG_LARGO
if "%opcion%"=="16" goto DIFF
if "%opcion%"=="17" goto UNDO_COMMIT
if "%opcion%"=="18" goto UNDO_ARCHIVO
if "%opcion%"=="19" goto RESET_HARD
if "%opcion%"=="20" goto VER_TAGS
if "%opcion%"=="21" goto CREAR_TAG
if "%opcion%"=="22" goto PUSH_TAGS
if "%opcion%"=="0"  goto FIN
goto MENU

REM ================================================
REM [1] COMMIT + PUSH
REM ================================================
:COMMIT_PUSH
cls
echo.
echo  COMMIT + PUSH
echo  ─────────────────────────────────────────────
echo.
git status --short
echo.
set /p msg="  Mensaje del commit: "
if "%msg%"=="" (
    echo  ERROR: El mensaje no puede estar vacio.
    goto PAUSA
)
git add .
git commit -m "%msg%"
if %ERRORLEVEL% NEQ 0 (
    echo  No hay cambios para commitear.
    goto PAUSA
)
echo.
echo  Subiendo a GitHub...
git push origin main
if %ERRORLEVEL% EQU 0 (
    echo.
    echo  OK - Cambios en https://github.com/creto/AIGEN
) else (
    echo  ERROR al hacer push. Verifica conexion y autenticacion.
)
goto PAUSA

REM ================================================
REM [2] PUSH
REM ================================================
:PUSH
cls
echo.
echo  PUSH — Subiendo commits locales...
echo  ─────────────────────────────────────────────
git push origin main
if %ERRORLEVEL% EQU 0 (
    echo  OK - Push exitoso.
) else (
    echo  ERROR al hacer push.
)
goto PAUSA

REM ================================================
REM [3] STASH
REM ================================================
:STASH
cls
echo.
echo  STASH — Guardando cambios temporalmente...
echo  ─────────────────────────────────────────────
set /p smsg="  Descripcion del stash (opcional): "
if "%smsg%"=="" (
    git stash
) else (
    git stash push -m "%smsg%"
)
echo.
echo  Stashes guardados:
git stash list
goto PAUSA

REM ================================================
REM [4] STASH POP
REM ================================================
:STASH_POP
cls
echo.
echo  STASH POP — Recuperando cambios...
echo  ─────────────────────────────────────────────
echo  Stashes disponibles:
git stash list
echo.
git stash pop
goto PAUSA

REM ================================================
REM [5] PULL
REM ================================================
:PULL
cls
echo.
echo  PULL — Trayendo cambios de GitHub...
echo  ─────────────────────────────────────────────
git pull origin main
if %ERRORLEVEL% EQU 0 (
    echo  OK - Repositorio actualizado.
) else (
    echo  ERROR al hacer pull. Puede haber conflictos.
)
goto PAUSA

REM ================================================
REM [6] FETCH
REM ================================================
:FETCH
cls
echo.
echo  FETCH — Verificando cambios remotos sin aplicar...
echo  ─────────────────────────────────────────────
git fetch origin
echo.
echo  Diferencias entre local y remoto:
git log HEAD..origin/main --oneline
if %ERRORLEVEL% NEQ 0 (
    echo  Sin diferencias o rama no encontrada.
)
goto PAUSA

REM ================================================
REM [7] PULL REBASE
REM ================================================
:PULL_REBASE
cls
echo.
echo  PULL REBASE — Trayendo y rebasando...
echo  ─────────────────────────────────────────────
git pull --rebase origin main
if %ERRORLEVEL% EQU 0 (
    echo  OK - Rebase exitoso.
) else (
    echo  ERROR. Puede haber conflictos. Resuelve y ejecuta: git rebase --continue
)
goto PAUSA

REM ================================================
REM [8] VER RAMAS
REM ================================================
:VER_RAMAS
cls
echo.
echo  RAMAS — Locales y remotas
echo  ─────────────────────────────────────────────
echo  Locales:
git branch
echo.
echo  Remotas:
git branch -r
echo.
echo  Todas:
git branch -a
goto PAUSA

REM ================================================
REM [9] CREAR RAMA
REM ================================================
:CREAR_RAMA
cls
echo.
echo  CREAR RAMA
echo  ─────────────────────────────────────────────
set /p rama="  Nombre de la nueva rama: "
if "%rama%"=="" goto MENU
git checkout -b "%rama%"
if %ERRORLEVEL% EQU 0 (
    echo.
    set /p pushrama="  Subir rama a GitHub? (s/n): "
    if /i "%pushrama%"=="s" git push -u origin "%rama%"
) else (
    echo  ERROR al crear la rama.
)
goto PAUSA

REM ================================================
REM [10] CAMBIAR RAMA
REM ================================================
:CAMBIAR_RAMA
cls
echo.
echo  CAMBIAR RAMA
echo  ─────────────────────────────────────────────
echo  Ramas disponibles:
git branch
echo.
set /p rama="  Nombre de la rama destino: "
if "%rama%"=="" goto MENU
git checkout "%rama%"
goto PAUSA

REM ================================================
REM [11] MERGE RAMA A MAIN
REM ================================================
:MERGE_RAMA
cls
echo.
echo  MERGE — Fusionar rama a main
echo  ─────────────────────────────────────────────
echo  Ramas disponibles:
git branch
echo.
set /p rama="  Nombre de la rama a fusionar: "
if "%rama%"=="" goto MENU
git checkout main
git merge "%rama%"
if %ERRORLEVEL% EQU 0 (
    echo.
    set /p pushmerge="  Subir merge a GitHub? (s/n): "
    if /i "%pushmerge%"=="s" git push origin main
) else (
    echo  ERROR en merge. Resuelve conflictos manualmente.
)
goto PAUSA

REM ================================================
REM [12] ELIMINAR RAMA
REM ================================================
:ELIMINAR_RAMA
cls
echo.
echo  ELIMINAR RAMA
echo  ─────────────────────────────────────────────
echo  Ramas locales:
git branch
echo.
set /p rama="  Nombre de la rama a eliminar: "
if "%rama%"=="" goto MENU
if /i "%rama%"=="main" (
    echo  ERROR: No puedes eliminar la rama main.
    goto PAUSA
)
set /p confirm="  Seguro? Esto elimina la rama local (s/n): "
if /i "%confirm%"=="s" (
    git branch -d "%rama%"
    set /p delremote="  Eliminar tambien en GitHub? (s/n): "
    if /i "%delremote%"=="s" git push origin --delete "%rama%"
)
goto PAUSA

REM ================================================
REM [13] STATUS
REM ================================================
:STATUS
cls
echo.
echo  STATUS — Estado actual del repositorio
echo  ─────────────────────────────────────────────
echo  Rama actual:
git branch --show-current
echo.
git status
goto PAUSA

REM ================================================
REM [14] LOG COMPACTO
REM ================================================
:LOG_CORTO
cls
echo.
echo  LOG — Ultimos 20 commits
echo  ─────────────────────────────────────────────
git log --oneline --graph --decorate -20
goto PAUSA

REM ================================================
REM [15] LOG DETALLADO
REM ================================================
:LOG_LARGO
cls
echo.
echo  LOG DETALLADO — Ultimos 10 commits
echo  ─────────────────────────────────────────────
git log --format="%%h | %%ad | %%an | %%s" --date=short -10
goto PAUSA

REM ================================================
REM [16] DIFF
REM ================================================
:DIFF
cls
echo.
echo  DIFF — Cambios pendientes
echo  ─────────────────────────────────────────────
git diff --stat
echo.
set /p verdiff="  Ver diff completo? (s/n): "
if /i "%verdiff%"=="s" git diff
goto PAUSA

REM ================================================
REM [17] UNDO ULTIMO COMMIT
REM ================================================
:UNDO_COMMIT
cls
echo.
echo  UNDO COMMIT — Deshace el ultimo commit
echo  Los cambios se mantienen en tus archivos
echo  ─────────────────────────────────────────────
echo  Ultimo commit:
git log --oneline -1
echo.
set /p confirm="  Confirmas deshacer este commit? (s/n): "
if /i "%confirm%"=="s" (
    git reset --soft HEAD~1
    echo  OK - Commit deshecho. Cambios conservados en staging.
)
goto PAUSA

REM ================================================
REM [18] UNDO ARCHIVO
REM ================================================
:UNDO_ARCHIVO
cls
echo.
echo  UNDO ARCHIVO — Descarta cambios de un archivo
echo  ─────────────────────────────────────────────
git status --short
echo.
set /p archivo="  Nombre del archivo (ej: src/Aigen.Core/Config/GeneratorConfig.cs): "
if "%archivo%"=="" goto MENU
set /p confirm="  Seguro? Los cambios locales se perderan (s/n): "
if /i "%confirm%"=="s" (
    git checkout -- "%archivo%"
    echo  OK - Archivo restaurado al ultimo commit.
)
goto PAUSA

REM ================================================
REM [19] RESET HARD
REM ================================================
:RESET_HARD
cls
echo.
echo  RESET HARD — Descarta TODOS los cambios locales
echo  ─────────────────────────────────────────────
echo  ADVERTENCIA: Esta accion es irreversible.
echo  Todos los cambios no commiteados se perderan.
echo.
git status --short
echo.
set /p confirm="  Estas seguro? Escribe SI para confirmar: "
if "%confirm%"=="SI" (
    git reset --hard HEAD
    git clean -fd
    echo  OK - Repositorio restaurado al ultimo commit.
) else (
    echo  Operacion cancelada.
)
goto PAUSA

REM ================================================
REM [20] VER TAGS
REM ================================================
:VER_TAGS
cls
echo.
echo  TAGS — Versiones etiquetadas
echo  ─────────────────────────────────────────────
git tag -l --sort=-version:refname
goto PAUSA

REM ================================================
REM [21] CREAR TAG
REM ================================================
:CREAR_TAG
cls
echo.
echo  CREAR TAG — Etiquetar version
echo  ─────────────────────────────────────────────
set /p vtag="  Version (ej: v1.0.0): "
if "%vtag%"=="" goto MENU
set /p mtag="  Descripcion del tag: "
git tag -a "%vtag%" -m "%mtag%"
if %ERRORLEVEL% EQU 0 (
    echo  OK - Tag %vtag% creado.
    set /p pushtag="  Subir tag a GitHub? (s/n): "
    if /i "%pushtag%"=="s" git push origin "%vtag%"
) else (
    echo  ERROR al crear tag. Puede que ya exista.
)
goto PAUSA

REM ================================================
REM [22] PUSH TAGS
REM ================================================
:PUSH_TAGS
cls
echo.
echo  PUSH TAGS — Subiendo todos los tags a GitHub...
echo  ─────────────────────────────────────────────
git push origin --tags
if %ERRORLEVEL% EQU 0 (
    echo  OK - Tags subidos correctamente.
) else (
    echo  ERROR al subir tags.
)
goto PAUSA

REM ================================================
REM PAUSA Y VOLVER AL MENU
REM ================================================
:PAUSA
echo.
pause
goto MENU

REM ================================================
REM FIN
REM ================================================
:FIN
cls
echo.
echo  AIGEN Git Manager - Hasta luego!
echo.
exit /b 0
