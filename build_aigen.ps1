# elimina elementos generados previamente
# Limpiar src + frontend + docs (todo excepto aigen.json y el .sln)
cls
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\src" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\frontend" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "C:\DevOps\AIGEN\AIGEN\Generated\docs" -Recurse -Force -ErrorAction SilentlyContinue


# 1. Rebuild AIGEN
cd C:\DevOps\AIGEN\AIGEN\aigen
dotnet restore
dotnet build 2>&1 | Select-Object -Last 5
dotnet test

# 4. Regenerar Doc4Us
dotnet run --project src/Aigen.CLI -- generate --config "..\Generated\aigen.json"

# 5. Compilar Doc4Us
cd C:\DevOps\AIGEN\AIGEN\Generated
dotnet build Doc4Us.sln 2>&1 | Select-Object -Last 10