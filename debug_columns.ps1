# Script para diagnosticar tipos de columnas en SQL Server
$connStr = "Server=200.31.22.7,1437;Database=Doc4UsAIGen;User Id=Oscar;Password=Dukakis2701;TrustServerCertificate=True;"
$table = "TA_TM_AnexoDocumento"

$query = @"
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    NUMERIC_PRECISION,
    NUMERIC_SCALE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '$table'
AND TABLE_SCHEMA = 'dbo'
ORDER BY ORDINAL_POSITION
"@

$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = New-Object System.Data.SqlClient.SqlCommand($query, $conn)
$reader = $cmd.ExecuteReader()

while ($reader.Read()) {
    Write-Host ("{0,-30} {1,-20} nullable={2}" -f $reader["COLUMN_NAME"], $reader["DATA_TYPE"], $reader["IS_NULLABLE"])
}
$conn.Close()
