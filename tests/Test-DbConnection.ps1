# Script to test SQL Server Connection and list instances on failure
$ErrorActionPreference = "Continue"

$scriptPath = $PSScriptRoot
if (-not $scriptPath) { $scriptPath = Get-Location }

Write-Host "Script running from: $scriptPath" -ForegroundColor Gray

function Get-ConnectionString {
    param($Path)
    if (Test-Path $Path) {
        try {
            $content = Get-Content $Path -Raw
            if ($content) {
                $json = $content | ConvertFrom-Json
                return $json.ConnectionStrings.DefaultConnection
            }
        }
        catch {
            Write-Warning "Could not parse appsettings.json at $Path"
        }
    }
    return $null
}

$pathsToCheck = @(
    "$scriptPath\src\Blanquita.Web\appsettings.json",
    "$scriptPath\Blanquita.Web\appsettings.json",
    "src\Blanquita.Web\appsettings.json",
    "appsettings.json"
)

$appParamsPath = $null
foreach ($path in $pathsToCheck) {
    if (Test-Path $path) {
        $appParamsPath = $path
        break
    }
}

$connStr = $null
if ($appParamsPath) {
    Write-Host "Reading connection string from: $appParamsPath" -ForegroundColor Cyan
    $connStr = Get-ConnectionString -Path $appParamsPath
}

if (-not $connStr) {
    $connStr = "Server=localhost;Database=BlanquitaDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    Write-Host "Using default fallback string: $connStr" -ForegroundColor Yellow
}

Write-Host "Testing connection to: $connStr" -ForegroundColor Cyan

$success = $false
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = $connStr
    $conn.Open()
    Write-Host "✅ Connection Successful!" -ForegroundColor Green
    Write-Host "Server Version: $($conn.ServerVersion)" -ForegroundColor Green
    $conn.Close()
    $success = $true
}
catch {
    Write-Host "❌ Connection Failed to Target Database!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red

    # Probe master to see if it's just the DB missing or Auth failure
    Write-Host "`nProbing 'master' database to diagnose..." -ForegroundColor Cyan
    try {
        $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
        $builder.ConnectionString = $connStr
        $builder.InitialCatalog = "master"
        
        $connMaster = New-Object System.Data.SqlClient.SqlConnection
        $connMaster.ConnectionString = $builder.ConnectionString
        $connMaster.Open()
        Write-Host "✅ Connection to 'master' DB Successful!" -ForegroundColor Green
        Write-Host "Diagnosis: The SQL Server is accepting your credentials, but the database 'BlanquitaDb' probably does not exist." -ForegroundColor Yellow
        $connMaster.Close()
    }
    catch {
        Write-Host "❌ Connection to 'master' also failed." -ForegroundColor Red
        Write-Host "Diagnosis: Authentication failure or Server unreachable." -ForegroundColor Red
    }
}

if (-not $success) {
    Write-Host "`n--------------------------------------------------"
    Write-Host "Searching for available SQL Server instances..." -ForegroundColor Cyan
    Write-Host "--------------------------------------------------"
    
    # Method 1: Get-Service (Local only, works on Core)
    Write-Host "Checking local services..." -ForegroundColor Cyan
    $services = Get-Service *sql* -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -like "*SQL Server*" }
    if ($services) {
        $services | Select-Object Name, DisplayName, Status | Format-Table -AutoSize
    }
    else {
        Write-Host "No local SQL Server services found via Get-Service." -ForegroundColor Yellow
    }

    # Method 2: sqlcmd -L (If available)
    if (Get-Command sqlcmd -ErrorAction SilentlyContinue) {
        Write-Host "`nAttempting to list servers using sqlcmd -L..." -ForegroundColor Cyan
        try {
            # Run sqlcmd with a timeout or just run it. -L lists servers.
            # Warning: sqlcmd -L might take a while if network is slow.
            $servers = sqlcmd -L
            # Filter output
            $serverList = $servers | Where-Object { 
                $_ -and 
                -not $_.Contains("Servers:") -and 
                -not ([string]::IsNullOrWhiteSpace($_)) 
            } | ForEach-Object { $_.Trim() }
            
            if ($serverList) {
                Write-Host "Found the following servers via sqlcmd:" -ForegroundColor Green
                $serverList | ForEach-Object { Write-Host "  - $_" }
            }
            else {
                Write-Host "sqlcmd -L returned no servers." -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "sqlcmd failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Method 3: SqlDataSourceEnumerator (Network, fails on Core)
    if ($PSVersionTable.PSEdition -ne 'Core') {
        Write-Host "`nAttempting to enumerate network instances (Legacy)..." -ForegroundColor Cyan
        try {
            $sqlSources = [System.Data.Sql.SqlDataSourceEnumerator]::Instance
            $table = $sqlSources.GetDataSources()
            if ($table -and $table.Rows.Count -gt 0) {
                $table | Format-Table -AutoSize
            }
            else {
                Write-Host "No network instances found via Enumerator." -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "Network enumeration failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}
