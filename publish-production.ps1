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
$SolutionPath = $ScriptPath  # El script está en la raíz del repositorio
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

# Paso 1: Verificar que estamos en la rama correcta y al día
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

try {
    Write-Info "Comprobando si hay cambios nuevos en GitHub..."
    git fetch origin $CurrentBranch 2>$null
    $LocalHash = git rev-parse HEAD
    $RemoteHash = git rev-parse "origin/$CurrentBranch"
    if ($LocalHash -ne $RemoteHash) {
        Write-Warning "¡ATENCIÓN! Tu copia local no coincide con origin/$CurrentBranch."
        Write-Warning "Local:  $LocalHash"
        Write-Warning "Remoto: $RemoteHash"
        $pull = Read-Host "¿Deseas ejecutar 'git pull' antes de publicar? (s/n)"
        if ($pull -eq "s") {
            git pull origin $CurrentBranch
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Error al hacer git pull."
                exit 1
            }
            Write-Success "✓ Repositorio actualizado con éxito."
        }
    } else {
        Write-Success "✓ El repositorio local está al día con GitHub."
    }
} catch {
    Write-Warning "No se pudo comprobar el estado remoto con GitHub (¿sin internet?). Continuando..."
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

# Bloquear temporalmente la aplicación en IIS con app_offline.htm para que w3wp libere Blanquita.Web.dll
$OfflineFile = Join-Path $OutputPath "app_offline.htm"
$CreatedOffline = $false
try {
    if (Test-Path $OutputPath) {
        Write-Info "Liberando DLLs bloqueadas por IIS (creando app_offline.htm)..."
        "<!DOCTYPE html><html><head><meta charset='utf-8'><title>Actualizando</title></head><body><h2>Actualizando aplicacion Blanquita... Por favor espere.</h2></body></html>" | Out-File -FilePath $OfflineFile -Encoding utf8
        $CreatedOffline = $true
        Start-Sleep -Seconds 2
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
}
finally {
    if ($CreatedOffline -and (Test-Path $OfflineFile)) {
        Remove-Item $OfflineFile -Force -ErrorAction SilentlyContinue
        Write-Success "✓ Aplicación reactivada (app_offline.htm eliminado)"
    }
}

# Paso 8: Gestionar archivo de variables de entorno (.env)
Write-Info ""
Write-Info "Paso 8: Verificando archivo de entorno (.env)..."
$DestEnvFile = Join-Path $OutputPath ".env"
$SourceEnvCandidates = @(
    (Join-Path $SolutionPath ".env"),
    (Join-Path $ProjectPath ".env")
)

if (-not (Test-Path $DestEnvFile)) {
    $foundSource = $null
    foreach ($candidate in $SourceEnvCandidates) {
        if (Test-Path $candidate) {
            $foundSource = $candidate
            break
        }
    }

    if ($foundSource) {
        Copy-Item -Path $foundSource -Destination $DestEnvFile -Force
        Write-Success "✓ Se copió .env desde '$foundSource' a '$DestEnvFile'"
    } else {
        Write-Warning "⚠️ ADVERTENCIA CRÍTICA: No existe '$DestEnvFile' ni se encontró .env en el código fuente."
        Write-Warning "   La aplicación no podrá iniciar a menos que cree '$DestEnvFile' o configure la variable en IIS."
        $exampleEnv = Join-Path $SolutionPath ".env.example"
        if (Test-Path $exampleEnv) {
            Copy-Item -Path $exampleEnv -Destination $DestEnvFile -Force
            Write-Warning "   Se copió .env.example como '$DestEnvFile'. ¡EDITA ESTE ARCHIVO con tus credenciales reales!"
        }
    }
} else {
    Write-Success "✓ Archivo .env existente en destino: $DestEnvFile"
}

# Validar que .env en destino tenga cadena de conexión
if (Test-Path $DestEnvFile) {
    $envContent = Get-Content $DestEnvFile -Raw
    if ($envContent -match "ConnectionStrings__DefaultConnection|DefaultConnection") {
        Write-Success "✓ .env contiene configuración para cadena de conexión."
    } else {
        Write-Warning "⚠️ El archivo '$DestEnvFile' NO parece contener 'ConnectionStrings__DefaultConnection'."
    }
}

# Paso 9: Crear carpeta de logs si no existe
Write-Info ""
Write-Info "Paso 9: Configurando carpetas de logs..."
$LogsPath = Join-Path $OutputPath "logs"
$ErrorLogsPath = Join-Path $LogsPath "errors"

if (-not (Test-Path $LogsPath)) {
    New-Item -ItemType Directory -Path $LogsPath -Force | Out-Null
}
if (-not (Test-Path $ErrorLogsPath)) {
    New-Item -ItemType Directory -Path $ErrorLogsPath -Force | Out-Null
}
Write-Success "✓ Carpetas de logs configuradas"

# Paso 10: Configurar permisos (solo si es IIS)
Write-Info ""
Write-Info "Paso 10: Configurando permisos para IIS..."
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

# Paso 11: Verificar archivos críticos
Write-Info ""
Write-Info "Paso 11: Verificando archivos críticos..."
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

# Paso 12: Mostrar información de versión y reiniciar IIS
Write-Info ""
Write-Info "Paso 12: Información de la publicación..."
$AssemblyPath = Join-Path $OutputPath "Blanquita.Web.dll"
$AssemblyInfo = [System.Reflection.Assembly]::LoadFile($AssemblyPath)
$Version = $AssemblyInfo.GetName().Version
Write-Info "Versión: $Version"
Write-Info "Fecha de publicación: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# Intentar reciclar el Application Pool automáticamente
Write-Info ""
Write-Info "Paso 13: Reciclando Application Pool en IIS..."
$AppCmd = "$env:windir\system32\inetsrv\appcmd.exe"
if (Test-Path $AppCmd) {
    try {
        & $AppCmd recycle apppool /apppool.name:"BlanquitaAppPool" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "✓ Application Pool 'BlanquitaAppPool' reciclado correctamente."
        } else {
            Write-Warning "No se pudo reciclar 'BlanquitaAppPool' con appcmd (posiblemente requiere elevación como Administrador)."
        }
    } catch {
        Write-Warning "Error al invocar appcmd: $_"
    }
}

# Resumen final
Write-Info ""
Write-Success "========================================="
Write-Success "  ✓ PUBLICACIÓN COMPLETADA EXITOSAMENTE"
Write-Success "========================================="
Write-Info ""
Write-Info "Próximos pasos:"
Write-Info "1. Asegúrate de que el archivo .env en '$OutputPath' tenga la cadena correcta"
Write-Info "2. Si no se recicló el pool automáticamente, ejecuta 'iisreset' en consola de Administrador"
Write-Info "3. Navega a tu sitio web para verificar que funciona correctamente"
Write-Info "4. Revisa los logs en: $LogsPath"
Write-Info ""
Write-Info "Ruta de publicación: $OutputPath"
Write-Info ""
