@echo off
setlocal enabledelayedexpansion

git remote set-url origin https://github.com/creto/AIGEN.git
echo git remote set-url origin git@github-creto:creto/AIGEN.git
echo 

:MENU
cls
echo.
echo  ========================================
echo   AIGEN - Git Manager v1.0
echo   https://github.com/creto/AIGEN
echo  ========================================
echo.
echo  -- SUBIR CAMBIOS --
echo  [1] Commit + Push  (guardar y subir)
echo  [2] Push           (subir commits locales)
echo  [3] Stash          (guardar sin commit)
echo  [4] Stash Pop      (recuperar stash)
echo.
echo  -- TRAER CAMBIOS --
echo  [5] Pull           (traer y mergear)
echo  [6] Fetch          (ver cambios sin aplicar)
echo  [7] Pull Rebase    (traer rebasando)
echo.
echo  -- RAMAS --
echo  [8]  Ver ramas
echo  [9]  Crear rama
echo  [10] Cambiar rama
echo  [11] Merge a main
echo  [12] Eliminar rama
echo.
echo  -- ESTADO E HISTORIAL --
echo  [13] Status
echo  [14] Log compacto
echo  [15] Log detallado
echo  [16] Diff cambios
echo.
echo  -- DESHACER --
echo  [17] Undo ultimo commit (conserva cambios)
echo  [18] Undo archivo
echo  [19] Reset hard (CUIDADO)
echo.
echo  -- TAGS --
echo  [20] Ver tags
echo  [21] Crear tag
echo  [22] Push tags
echo.
echo  [0] Salir
echo.
set /p op="  Selecciona una opcion: "

if "%op%"=="1"  goto OP1
if "%op%"=="2"  goto OP2
if "%op%"=="3"  goto OP3
if "%op%"=="4"  goto OP4
if "%op%"=="5"  goto OP5
if "%op%"=="6"  goto OP6
if "%op%"=="7"  goto OP7
if "%op%"=="8"  goto OP8
if "%op%"=="9"  goto OP9
if "%op%"=="10" goto OP10
if "%op%"=="11" goto OP11
if "%op%"=="12" goto OP12
if "%op%"=="13" goto OP13
if "%op%"=="14" goto OP14
if "%op%"=="15" goto OP15
if "%op%"=="16" goto OP16
if "%op%"=="17" goto OP17
if "%op%"=="18" goto OP18
if "%op%"=="19" goto OP19
if "%op%"=="20" goto OP20
if "%op%"=="21" goto OP21
if "%op%"=="22" goto OP22
if "%op%"=="0"  goto FIN
goto MENU

:OP1
cls
echo.
echo  COMMIT + PUSH
echo  ----------------------------------------
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
    echo  No hay cambios nuevos para commitear.
    goto PAUSA
)
echo.
echo  Subiendo a GitHub...
git push origin main
if %ERRORLEVEL% EQU 0 (
    echo  OK - Cambios en https://github.com/creto/AIGEN
) else (
    echo  ERROR al hacer push.
)
goto PAUSA

:OP2
cls
echo.
echo  PUSH - Subiendo commits locales
echo  ----------------------------------------
git push origin main
if %ERRORLEVEL% EQU 0 (echo  OK - Push exitoso.) else (echo  ERROR.)
goto PAUSA

:OP3
cls
echo.
echo  STASH - Guardar cambios temporalmente
echo  ----------------------------------------
set /p smsg="  Descripcion del stash (ENTER para omitir): "
if "%smsg%"=="" (
    git stash
) else (
    git stash push -m "%smsg%"
)
echo.
echo  Stashes guardados:
git stash list
goto PAUSA

:OP4
cls
echo.
echo  STASH POP - Recuperar ultimo stash
echo  ----------------------------------------
echo  Stashes disponibles:
git stash list
echo.
git stash pop
goto PAUSA

:OP5
cls
echo.
echo  PULL - Traer cambios de GitHub
echo  ----------------------------------------
git pull origin main
if %ERRORLEVEL% EQU 0 (echo  OK - Actualizado.) else (echo  ERROR - puede haber conflictos.)
goto PAUSA

:OP6
cls
echo.
echo  FETCH - Ver cambios remotos sin aplicar
echo  ----------------------------------------
git fetch origin
echo.
echo  Commits en GitHub pendientes:
git log HEAD..origin/main --oneline
goto PAUSA

:OP7
cls
echo.
echo  PULL REBASE
echo  ----------------------------------------
git pull --rebase origin main
if %ERRORLEVEL% EQU 0 (echo  OK.) else (echo  ERROR. Ejecuta: git rebase --continue)
goto PAUSA

:OP8
cls
echo.
echo  RAMAS LOCALES:
echo  ----------------------------------------
git branch
echo.
echo  RAMAS REMOTAS:
git branch -r
goto PAUSA

:OP9
cls
echo.
echo  CREAR RAMA
echo  ----------------------------------------
set /p rama="  Nombre de la nueva rama (ej: feature/semana2): "
if "%rama%"=="" goto MENU
git checkout -b "%rama%"
if %ERRORLEVEL% EQU 0 (
    echo  Rama creada: %rama%
    set /p pushrama="  Subir rama a GitHub? (s/n): "
    if /i "%pushrama%"=="s" git push -u origin "%rama%"
) else (
    echo  ERROR al crear la rama.
)
goto PAUSA

:OP10
cls
echo.
echo  CAMBIAR RAMA
echo  ----------------------------------------
echo  Ramas disponibles:
git branch
echo.
set /p rama="  Nombre de la rama destino: "
if "%rama%"=="" goto MENU
git checkout "%rama%"
goto PAUSA

:OP11
cls
echo.
echo  MERGE A MAIN
echo  ----------------------------------------
echo  Ramas disponibles:
git branch
echo.
set /p rama="  Rama a fusionar en main: "
if "%rama%"=="" goto MENU
git checkout main
git merge "%rama%"
if %ERRORLEVEL% EQU 0 (
    set /p pushmerge="  Subir a GitHub? (s/n): "
    if /i "%pushmerge%"=="s" git push origin main
) else (
    echo  ERROR en merge. Resuelve conflictos manualmente.
)
goto PAUSA

:OP12
cls
echo.
echo  ELIMINAR RAMA
echo  ----------------------------------------
echo  Ramas locales:
git branch
echo.
set /p rama="  Nombre de la rama a eliminar: "
if "%rama%"=="" goto MENU
if /i "%rama%"=="main" (
    echo  ERROR: No se puede eliminar main.
    goto PAUSA
)
set /p conf="  Confirmas eliminar %rama%? (s/n): "
if /i "%conf%"=="s" (
    git branch -d "%rama%"
    set /p delr="  Eliminar en GitHub tambien? (s/n): "
    if /i "%delr%"=="s" git push origin --delete "%rama%"
)
goto PAUSA

:OP13
cls
echo.
echo  STATUS
echo  ----------------------------------------
git status
goto PAUSA

:OP14
cls
echo.
echo  LOG COMPACTO - Ultimos 20 commits
echo  ----------------------------------------
git log --oneline --graph --decorate -20
goto PAUSA

:OP15
cls
echo.
echo  LOG DETALLADO - Ultimos 10 commits
echo  ----------------------------------------
git log --format="%%h | %%ad | %%an | %%s" --date=short -10
goto PAUSA

:OP16
cls
echo.
echo  DIFF - Cambios pendientes
echo  ----------------------------------------
git diff --stat
echo.
set /p vd="  Ver diff completo? (s/n): "
if /i "%vd%"=="s" git diff
goto PAUSA

:OP17
cls
echo.
echo  UNDO ULTIMO COMMIT (soft reset)
echo  Los cambios quedan en tus archivos.
echo  ----------------------------------------
echo  Ultimo commit:
git log --oneline -1
echo.
set /p conf="  Confirmas deshacer? (s/n): "
if /i "%conf%"=="s" (
    git reset --soft HEAD~1
    echo  OK - Commit deshecho. Cambios conservados.
) else (
    echo  Cancelado.
)
goto PAUSA

:OP18
cls
echo.
echo  UNDO ARCHIVO - Restaurar al ultimo commit
echo  ----------------------------------------
git status --short
echo.
set /p arch="  Ruta del archivo: "
if "%arch%"=="" goto MENU
set /p conf="  Seguro? Los cambios locales se perderan (s/n): "
if /i "%conf%"=="s" (
    git checkout -- "%arch%"
    echo  OK - Archivo restaurado.
)
goto PAUSA

:OP19
cls
echo.
echo  RESET HARD - ADVERTENCIA: IRREVERSIBLE
echo  Descarta TODOS los cambios no commiteados.
echo  ----------------------------------------
git status --short
echo.
set /p conf="  Escribe SI para confirmar: "
if "%conf%"=="SI" (
    git reset --hard HEAD
    git clean -fd
    echo  OK - Repositorio limpio al ultimo commit.
) else (
    echo  Cancelado.
)
goto PAUSA

:OP20
cls
echo.
echo  TAGS
echo  ----------------------------------------
git tag -l --sort=-version:refname
goto PAUSA

:OP21
cls
echo.
echo  CREAR TAG
echo  ----------------------------------------
set /p vtag="  Version (ej: v1.0.0): "
if "%vtag%"=="" goto MENU
set /p mtag="  Descripcion del tag: "
git tag -a "%vtag%" -m "%mtag%"
if %ERRORLEVEL% EQU 0 (
    echo  OK - Tag %vtag% creado.
    set /p pt="  Subir a GitHub? (s/n): "
    if /i "%pt%"=="s" git push origin "%vtag%"
) else (
    echo  ERROR. Puede que el tag ya exista.
)
goto PAUSA

:OP22
cls
echo.
echo  PUSH TAGS - Subiendo todos los tags
echo  ----------------------------------------
git push origin --tags
if %ERRORLEVEL% EQU 0 (echo  OK - Tags subidos.) else (echo  ERROR.)
goto PAUSA

:PAUSA
echo.
pause
goto MENU

:FIN
cls
echo.
echo  AIGEN Git Manager - Hasta luego!
echo.
exit /b 0
