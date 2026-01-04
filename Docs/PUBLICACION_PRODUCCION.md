# Guía de Publicación para Producción - Blanquita

Esta guía proporciona instrucciones paso a paso para publicar la aplicación Blanquita en un entorno de producción.

## 📋 Tabla de Contenidos
- [Preparación Pre-Publicación](#preparación-pre-publicación)
- [Métodos de Publicación](#métodos-de-publicación)
- [Configuración Post-Publicación](#configuración-post-publicación)
- [Verificación y Pruebas](#verificación-y-pruebas)
- [Rollback en Caso de Problemas](#rollback-en-caso-de-problemas)

---

## 🔧 Preparación Pre-Publicación

### 1. Verificar el Estado del Código

Antes de publicar, asegúrate de que:

- [ ] Todos los cambios están commiteados en Git
- [ ] Estás en la rama `main` o `master`
- [ ] Todos los tests pasan correctamente
- [ ] No hay errores de compilación

```powershell
# Verificar estado de Git
git status

# Verificar rama actual
git branch

# Ejecutar tests
dotnet test
```

### 2. Actualizar Configuración de Producción

Edita el archivo `src/Blanquita.Web/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR_PRODUCCION;Database=BlanquitaDb;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=True"
  },
  "FoxPro": {
    "Pos10041Path": "RUTA_PRODUCCION_POS_10041",
    "Pos10042Path": "RUTA_PRODUCCION_POS_10042",
    "Mgw10008Path": "RUTA_PRODUCCION_MGW_10008",
    "Mgw10005Path": "RUTA_PRODUCCION_MGW_10005"
  }
}
```

**⚠️ IMPORTANTE:** 
- Nunca commitees contraseñas reales en el repositorio
- Considera usar variables de entorno o Azure Key Vault para secretos
- Asegúrate de que las rutas de FoxPro sean correctas para producción

### 3. Verificar Requisitos del Servidor

El servidor de producción debe tener:

- [ ] Windows Server 2016 o superior (o Windows 10/11 para pruebas)
- [ ] IIS instalado y configurado
- [ ] .NET 9.0 Hosting Bundle instalado
- [ ] SQL Server accesible desde el servidor
- [ ] WebSocket Protocol habilitado en IIS
- [ ] Permisos de escritura en carpetas de logs

---

## 🚀 Métodos de Publicación

### Método 1: Script Automatizado (Recomendado)

Hemos creado un script de PowerShell que automatiza todo el proceso:

```powershell
# Publicación básica
.\publish-production.ps1

# Publicación con backup de la versión anterior
.\publish-production.ps1 -CreateBackup

# Publicación sin ejecutar tests (no recomendado)
.\publish-production.ps1 -SkipTests

# Publicación a una ruta personalizada
.\publish-production.ps1 -OutputPath "D:\WebApps\Blanquita"

# Todas las opciones combinadas
.\publish-production.ps1 -OutputPath "C:\inetpub\wwwroot\Blanquita" -CreateBackup -Configuration Release
```

**Parámetros disponibles:**
- `-OutputPath`: Ruta donde se publicará la aplicación (default: `C:\inetpub\wwwroot\Blanquita`)
- `-Configuration`: Configuración de build (default: `Release`)
- `-SkipTests`: Omite la ejecución de tests
- `-CreateBackup`: Crea un backup de la versión actual antes de publicar

### Método 2: Publicación Manual con CLI

Si prefieres hacerlo manualmente:

```powershell
# 1. Navegar a la carpeta de la solución
cd "C:\Users\B10 Caja 2\source\repos\Blanquita"

# 2. Limpiar la solución
dotnet clean -c Release

# 3. Restaurar dependencias
dotnet restore

# 4. Ejecutar tests
dotnet test -c Release

# 5. Publicar
dotnet publish src\Blanquita.Web\Blanquita.Web.csproj `
    -c Release `
    -o C:\inetpub\wwwroot\Blanquita `
    /p:EnvironmentName=Production

# 6. Crear carpetas de logs
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\Blanquita\logs\errors" -Force
```

### Método 3: Desde Visual Studio

1. Clic derecho en el proyecto **Blanquita.Web** → **Publicar**
2. Seleccionar **Carpeta** como destino
3. Configurar la ruta: `C:\inetpub\wwwroot\Blanquita`
4. En **Configuración**:
   - Configuration: `Release`
   - Target Framework: `net9.0`
   - Deployment Mode: `Framework-dependent`
   - Target Runtime: `win-x64`
5. Clic en **Publicar**

---

## ⚙️ Configuración Post-Publicación

### 1. Configurar IIS

#### Crear Application Pool

```powershell
# Desde PowerShell como Administrador
Import-Module WebAdministration

# Crear Application Pool
New-WebAppPool -Name "BlanquitaAppPool"
Set-ItemProperty IIS:\AppPools\BlanquitaAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\BlanquitaAppPool -Name managedPipelineMode -Value "Integrated"
```

O manualmente:
1. Abrir **Administrador de IIS** (`inetmgr`)
2. Clic derecho en **Grupos de aplicaciones** → **Agregar grupo de aplicaciones**
3. Nombre: `BlanquitaAppPool`
4. Versión de .NET CLR: **Sin código administrado**
5. Modo de canalización: **Integrada**

#### Crear Sitio Web

```powershell
# Crear sitio web
New-Website -Name "BlanquitaWeb" `
    -ApplicationPool "BlanquitaAppPool" `
    -PhysicalPath "C:\inetpub\wwwroot\Blanquita" `
    -Port 80
```

O manualmente:
1. Clic derecho en **Sitios** → **Agregar sitio web**
2. Nombre: `BlanquitaWeb`
3. Application Pool: `BlanquitaAppPool`
4. Ruta física: `C:\inetpub\wwwroot\Blanquita`
5. Puerto: `80` (o el que prefieras)

### 2. Configurar Permisos

```powershell
# Dar permisos al Application Pool
$path = "C:\inetpub\wwwroot\Blanquita"
$acl = Get-Acl $path
$permission = "IIS AppPool\BlanquitaAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $path $acl
```

### 3. Configurar Base de Datos

```powershell
# Navegar a la carpeta de publicación
cd C:\inetpub\wwwroot\Blanquita

# Ejecutar migraciones (si es necesario)
dotnet Blanquita.Web.dll -- ef database update
```

O desde la carpeta del proyecto:

```powershell
cd "C:\Users\B10 Caja 2\source\repos\Blanquita\src\Blanquita.Web"
dotnet ef database update --connection "Server=TU_SERVIDOR;Database=BlanquitaDb;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True"
```

### 4. Reiniciar IIS

```powershell
# Reiniciar IIS
iisreset
```

---

## ✅ Verificación y Pruebas

### 1. Verificar Archivos Publicados

Asegúrate de que estos archivos existan en `C:\inetpub\wwwroot\Blanquita`:

- [ ] `Blanquita.Web.dll`
- [ ] `Blanquita.Web.exe`
- [ ] `appsettings.json`
- [ ] `appsettings.Production.json`
- [ ] `web.config`
- [ ] Carpeta `wwwroot`
- [ ] Carpeta `logs` (creada automáticamente)

### 2. Probar la Aplicación Localmente

Antes de configurar IIS, prueba que la aplicación funcione:

```powershell
cd C:\inetpub\wwwroot\Blanquita
$env:ASPNETCORE_ENVIRONMENT="Production"
.\Blanquita.Web.exe
```

Deberías ver algo como:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Navega a `http://localhost:5000` y verifica que la aplicación cargue.

### 3. Probar a través de IIS

1. Abre un navegador
2. Navega a `http://localhost` (o el puerto configurado)
3. Verifica que:
   - [ ] La aplicación carga correctamente
   - [ ] Puedes iniciar sesión
   - [ ] Las funcionalidades principales funcionan
   - [ ] No hay errores en la consola del navegador

### 4. Revisar Logs

```powershell
# Ver logs recientes
Get-Content C:\inetpub\wwwroot\Blanquita\logs\blanquita-*.log -Tail 50

# Ver logs de errores
Get-Content C:\inetpub\wwwroot\Blanquita\logs\errors\blanquita-errors-*.log -Tail 50
```

### 5. Verificar Event Viewer

Si hay problemas:

1. Abrir **Visor de eventos** (`eventvwr`)
2. Ir a **Registros de Windows** → **Aplicación**
3. Buscar errores de **IIS AspNetCore Module V2**

---

## 🔄 Rollback en Caso de Problemas

### Si usaste el script con `-CreateBackup`:

```powershell
# Detener el sitio
Stop-Website -Name "BlanquitaWeb"

# Restaurar backup (reemplaza la fecha con tu backup)
$BackupPath = "C:\inetpub\wwwroot\Blanquita-backup-20260103-184500"
Remove-Item "C:\inetpub\wwwroot\Blanquita" -Recurse -Force
Copy-Item $BackupPath "C:\inetpub\wwwroot\Blanquita" -Recurse

# Reiniciar IIS
iisreset

# Iniciar el sitio
Start-Website -Name "BlanquitaWeb"
```

### Si no tienes backup:

1. Revierte el código a la versión anterior en Git:
```powershell
git log --oneline  # Ver commits recientes
git checkout <commit-hash>  # Revertir a un commit específico
```

2. Vuelve a publicar usando el script o manualmente

---

## 📊 Checklist de Publicación

Usa esta lista para asegurarte de no olvidar nada:

### Pre-Publicación
- [ ] Código commiteado y pusheado
- [ ] Tests pasando
- [ ] `appsettings.Production.json` actualizado
- [ ] Backup de la versión actual creado
- [ ] Base de datos de producción lista

### Publicación
- [ ] Aplicación publicada exitosamente
- [ ] Archivos críticos verificados
- [ ] Carpetas de logs creadas

### Post-Publicación
- [ ] IIS configurado correctamente
- [ ] Permisos configurados
- [ ] Migraciones de base de datos ejecutadas
- [ ] IIS reiniciado
- [ ] Aplicación accesible desde el navegador
- [ ] Funcionalidades principales probadas
- [ ] Logs revisados (sin errores críticos)

### Monitoreo
- [ ] Configurar alertas de errores
- [ ] Revisar logs diariamente
- [ ] Monitorear rendimiento
- [ ] Verificar backups automáticos de BD

---

## 🆘 Solución de Problemas Comunes

### Error: HTTP 500.30 - ANCM In-Process Start Failure

**Causa:** La aplicación falló al iniciar.

**Solución:**
1. Habilitar logs de stdout en `web.config`:
```xml
<aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
```
2. Revisar los logs generados
3. Verificar la cadena de conexión
4. Verificar que el .NET Hosting Bundle esté instalado

### Error: HTTP 500.19 - Configuration Error

**Causa:** Problema con `web.config` o permisos.

**Solución:**
1. Verificar permisos del usuario IIS AppPool
2. Instalar URL Rewrite Module
3. Verificar que `web.config` sea válido

### La aplicación se desconecta frecuentemente

**Causa:** WebSockets no habilitado.

**Solución:**
1. Habilitar WebSocket Protocol en características de Windows
2. Verificar que el firewall permita WebSockets
3. Ajustar timeout del Application Pool

### No se pueden escribir logs

**Causa:** Permisos insuficientes.

**Solución:**
```powershell
$logsPath = "C:\inetpub\wwwroot\Blanquita\logs"
$acl = Get-Acl $logsPath
$permission = "IIS AppPool\BlanquitaAppPool", "Modify", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $logsPath $acl
```

---

## 📞 Soporte

Para más información, consulta:
- [Guía de Despliegue en IIS](DEPLOY_IIS.md)
- [Documentación de .NET](https://docs.microsoft.com/aspnet/core/host-and-deploy/iis/)
- Logs de la aplicación en `C:\inetpub\wwwroot\Blanquita\logs`

---

**Última actualización:** 2026-01-03
