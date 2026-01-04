# Script de Publicación para Producción - Blanquita
# Este script automatiza el proceso de publicación de la aplicación Blanquita

param(
    [string]$OutputPath = "C:\inetpub\wwwroot\Blanquita",
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$CreateBackup
)

# Colores para mensajes
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }
function Write-Error { Write-Host $args -ForegroundColor Red }

Write-Info "========================================="
Write-Info "  Publicación de Blanquita - Producción"
Write-Info "========================================="
Write-Info ""

# Obtener la ruta del script
$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionPath = Split-Path -Parent $ScriptPath
$ProjectPath = Join-Path $SolutionPath "src\Blanquita.Web"
$SolutionFile = Get-ChildItem -Path $SolutionPath -Filter "*.sln" | Select-Object -First 1

if (-not $SolutionFile) {
    Write-Error "No se encontró archivo .sln en $SolutionPath"
    exit 1
}

Write-Info "Ruta de la solución: $SolutionPath"
Write-Info "Archivo de solución: $($SolutionFile.Name)"
Write-Info "Ruta del proyecto: $ProjectPath"
Write-Info "Ruta de salida: $OutputPath"
Write-Info ""

# Paso 1: Verificar que estamos en la rama correcta
Write-Info "Paso 1: Verificando rama de Git..."
$CurrentBranch = git rev-parse --abbrev-ref HEAD
Write-Info "Rama actual: $CurrentBranch"

if ($CurrentBranch -ne "main" -and $CurrentBranch -ne "master") {
    Write-Warning "ADVERTENCIA: No estás en la rama main/master. Rama actual: $CurrentBranch"
    $continue = Read-Host "¿Deseas continuar? (s/n)"
    if ($continue -ne "s") {
        Write-Error "Publicación cancelada por el usuario."
        exit 1
    }
}

# Paso 2: Verificar estado de Git
Write-Info ""
Write-Info "Paso 2: Verificando estado de Git..."
$GitStatus = git status --porcelain
if ($GitStatus) {
    Write-Warning "ADVERTENCIA: Hay cambios sin commitear:"
    git status --short
    $continue = Read-Host "¿Deseas continuar? (s/n)"
    if ($continue -ne "s") {
        Write-Error "Publicación cancelada. Por favor, commitea tus cambios primero."
        exit 1
    }
}

# Paso 3: Crear backup si se solicita
if ($CreateBackup -and (Test-Path $OutputPath)) {
    Write-Info ""
    Write-Info "Paso 3: Creando backup de la versión actual..."
    $BackupPath = "$OutputPath-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    try {
        Copy-Item -Path $OutputPath -Destination $BackupPath -Recurse -Force
        Write-Success "✓ Backup creado en: $BackupPath"
    }
    catch {
        Write-Error "Error al crear backup: $_"
        exit 1
    }
}

# Paso 4: Limpiar solución
Write-Info ""
Write-Info "Paso 4: Limpiando solución..."
Set-Location $SolutionPath
dotnet clean $SolutionFile.FullName -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al limpiar la solución."
    exit 1
}
Write-Success "✓ Solución limpiada"

# Paso 5: Restaurar dependencias
Write-Info ""
Write-Info "Paso 5: Restaurando dependencias..."
dotnet restore $SolutionFile.FullName
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al restaurar dependencias."
    exit 1
}
Write-Success "✓ Dependencias restauradas"

# Paso 6: Ejecutar tests (opcional)
if (-not $SkipTests) {
    Write-Info ""
    Write-Info "Paso 6: Ejecutando tests..."
    dotnet test $SolutionFile.FullName -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Los tests fallaron. Publicación cancelada."
        exit 1
    }
    Write-Success "✓ Todos los tests pasaron"
}
else {
    Write-Warning "Paso 6: Tests omitidos (flag -SkipTests activado)"
}

# Paso 7: Publicar aplicación
Write-Info ""
Write-Info "Paso 7: Publicando aplicación..."
Write-Info "Configuración: $Configuration"
Write-Info "Destino: $OutputPath"

# Crear directorio de salida si no existe
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Publicar
dotnet publish "$ProjectPath\Blanquita.Web.csproj" `
    -c $Configuration `
    -o $OutputPath `
    --no-restore `
    /p:EnvironmentName=Production

if ($LASTEXITCODE -ne 0) {
    Write-Error "Error al publicar la aplicación."
    exit 1
}
Write-Success "✓ Aplicación publicada exitosamente"

# Paso 8: Crear carpeta de logs si no existe
Write-Info ""
Write-Info "Paso 8: Configurando carpetas de logs..."
$LogsPath = Join-Path $OutputPath "logs"
$ErrorLogsPath = Join-Path $LogsPath "errors"

if (-not (Test-Path $LogsPath)) {
    New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null
}
if (-not (Test-Path $ErrorLogsPath)) {
    New-Item -ItemType Directory -Path $ErrorLogsPath -Force | Out-Null
}
Write-Success "✓ Carpetas de logs configuradas"

# Paso 9: Configurar permisos (solo si es IIS)
Write-Info ""
Write-Info "Paso 9: Configurando permisos para IIS..."
try {
    # Dar permisos al usuario de IIS AppPool
    $acl = Get-Acl $OutputPath
    $permission = "IIS AppPool\BlanquitaAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
    $acl.SetAccessRule($accessRule)
    Set-Acl $OutputPath $acl
    Write-Success "✓ Permisos configurados para IIS AppPool\BlanquitaAppPool"
}
catch {
    Write-Warning "No se pudieron configurar permisos automáticamente. Configúralos manualmente si es necesario."
    Write-Warning "Error: $_"
}

# Paso 10: Verificar archivos críticos
Write-Info ""
Write-Info "Paso 10: Verificando archivos críticos..."
$CriticalFiles = @(
    "Blanquita.Web.dll",
    "appsettings.json",
    "appsettings.Production.json",
    "web.config"
)

$AllFilesExist = $true
foreach ($file in $CriticalFiles) {
    $filePath = Join-Path $OutputPath $file
    if (Test-Path $filePath) {
        Write-Success "  ✓ $file"
    }
    else {
        Write-Error "  ✗ $file - NO ENCONTRADO"
        $AllFilesExist = $false
    }
}

if (-not $AllFilesExist) {
    Write-Error "Faltan archivos críticos. Verifica la publicación."
    exit 1
}

# Paso 11: Mostrar información de versión
Write-Info ""
Write-Info "Paso 11: Información de la publicación..."
$AssemblyPath = Join-Path $OutputPath "Blanquita.Web.dll"
$AssemblyInfo = [System.Reflection.Assembly]::LoadFile($AssemblyPath)
$Version = $AssemblyInfo.GetName().Version
Write-Info "Versión: $Version"
Write-Info "Fecha de publicación: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# Resumen final
Write-Info ""
Write-Success "========================================="
Write-Success "  ✓ PUBLICACIÓN COMPLETADA EXITOSAMENTE"
Write-Success "========================================="
Write-Info ""
Write-Info "Próximos pasos:"
Write-Info "1. Verifica el archivo appsettings.Production.json y actualiza la cadena de conexión"
Write-Info "2. Si usas IIS, ejecuta 'iisreset' en una consola administrativa"
Write-Info "3. Navega a tu sitio web para verificar que funciona correctamente"
Write-Info "4. Revisa los logs en: $LogsPath"
Write-Info ""
Write-Info "Ruta de publicación: $OutputPath"
Write-Info ""
