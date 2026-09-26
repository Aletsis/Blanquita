# Script de Despliegue de Artefactos de Release - Blanquita
# Este script toma el archivo .zip generado por GitHub Actions y lo despliega en el servidor IIS

param(
    [Parameter(Mandatory=$true)]
    [string]$ZipPackage,

    [string]$Destination = "C:\inetpub\wwwroot\Blanquita",
    [string]$AppPoolName = "BlanquitaAppPool",
    [switch]$CreateBackup = $true,
    [switch]$SkipAppPoolRestart = $false
)

function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

Write-Info "========================================="
Write-Info "  Despliegue de Artefacto CI/CD - Blanquita"
Write-Info "========================================="
Write-Info ""

# 1. Validar archivo Zip
if (-not (Test-Path $ZipPackage)) {
    Write-Error "El archivo de paquete no existe: $ZipPackage"
    exit 1
}

Write-Info "Paquete a desplegar: $ZipPackage"
Write-Info "Directorio destino:  $Destination"
Write-Info "Application Pool:    $AppPoolName"
Write-Info ""

# 2. Crear backup si existe el directorio de destino
if ($CreateBackup -and (Test-Path $Destination)) {
    $BackupPath = "$Destination-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Info "Creando backup previo en: $BackupPath..."
    try {
        Copy-Item -Path $Destination -Destination $BackupPath -Recurse -Force
        Write-Success "✓ Backup creado exitosamente"
    } catch {
        Write-Error "Fallo al crear backup: $_"
        exit 1
    }
}

# 3. Detener Application Pool en IIS
if (-not $SkipAppPoolRestart) {
    Write-Info "Deteniendo Application Pool ($AppPoolName)..."
    try {
        Import-Module WebAdministration -ErrorAction SilentlyContinue
        if (Get-WebAppPoolState -Name $AppPoolName -ErrorAction SilentlyContinue) {
            Stop-WebAppPool -Name $AppPoolName
            Start-Sleep -Seconds 2
            Write-Success "✓ Application Pool detenido"
        } else {
            Write-Warning "El AppPool '$AppPoolName' no se encontro o no se pudo consultar. Continuando..."
        }
    } catch {
        Write-Warning "No se pudo detener el AppPool automaticamente via WebAdministration. Si hay archivos bloqueados, ejecuta 'iisreset /stop'."
    }
}

# 4. Asegurar existencia de directorio destino
if (-not (Test-Path $Destination)) {
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
}

# 5. Descomprimir nuevo paquete
Write-Info "Descomprimiendo paquete en $Destination..."
try {
    Expand-Archive -Path $ZipPackage -DestinationPath $Destination -Force
    Write-Success "✓ Archivos extraidos correctamente"
} catch {
    Write-Error "Error al descomprimir archivos: $_"
    exit 1
}

# 6. Asegurar carpetas de logs
$LogsPath = Join-Path $Destination "logs"
$ErrorLogsPath = Join-Path $LogsPath "errors"
if (-not (Test-Path $LogsPath)) { New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null }
if (-not (Test-Path $ErrorLogsPath)) { New-Item -ItemType Directory -Path $ErrorLogsPath -Force | Out-Null }

# 7. Configurar permisos IIS
Write-Info "Asegurando permisos para IIS AppPool\$AppPoolName..."
try {
    $acl = Get-Acl $Destination
    $permission = "IIS AppPool\$AppPoolName", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $Destination $acl
    Write-Success "✓ Permisos de AppPool configurados"
} catch {
    Write-Warning "Aviso sobre permisos: $_"
}

# 8. Iniciar Application Pool en IIS
if (-not $SkipAppPoolRestart) {
    Write-Info "Iniciando Application Pool ($AppPoolName)..."
    try {
        Start-WebAppPool -Name $AppPoolName
        Write-Success "✓ Application Pool iniciado"
    } catch {
        Write-Warning "Inicia el AppPool manualmente o ejecuta 'iisreset'."
    }
}

Write-Info ""
Write-Success "========================================="
Write-Success "  ✓ DESPLIEGUE FINALIZADO EXITOSAMENTE"
Write-Success "========================================="
