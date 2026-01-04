---
description: Publicar la aplicación para producción
---

# Workflow: Publicar Blanquita para Producción

Este workflow te guía a través del proceso completo de publicación de la aplicación Blanquita en producción.

## Pre-requisitos

Antes de comenzar, asegúrate de tener:
- Acceso al servidor de producción
- Credenciales de la base de datos de producción
- Permisos de administrador en el servidor
- .NET 9.0 Hosting Bundle instalado en el servidor

## Pasos

### 1. Verificar el estado del código

```powershell
git status
git branch
```

Asegúrate de estar en la rama `main` o `master` y que no haya cambios sin commitear.

### 2. Ejecutar tests

// turbo
```powershell
dotnet test
```

Verifica que todos los tests pasen antes de continuar.

### 3. Actualizar configuración de producción

Edita el archivo `src/Blanquita.Web/appsettings.Production.json` y actualiza:
- Cadena de conexión a la base de datos de producción
- Rutas de FoxPro (si aplica)
- Cualquier otra configuración específica de producción

**⚠️ IMPORTANTE:** No commitees contraseñas reales en el repositorio.

### 4. Ejecutar el script de publicación

Opción A - Publicación básica:
```powershell
.\publish-production.ps1
```

Opción B - Publicación con backup (recomendado):
```powershell
.\publish-production.ps1 -CreateBackup
```

Opción C - Publicación a ruta personalizada:
```powershell
.\publish-production.ps1 -OutputPath "D:\WebApps\Blanquita" -CreateBackup
```

### 5. Verificar archivos publicados

Navega a la carpeta de publicación y verifica que existan:
- `Blanquita.Web.dll`
- `Blanquita.Web.exe`
- `appsettings.json`
- `appsettings.Production.json`
- `web.config`
- Carpeta `wwwroot`
- Carpeta `logs`

### 6. Configurar IIS (si es primera vez)

Si es la primera vez que publicas, necesitas configurar IIS:

```powershell
# Crear Application Pool
Import-Module WebAdministration
New-WebAppPool -Name "BlanquitaAppPool"
Set-ItemProperty IIS:\AppPools\BlanquitaAppPool -Name managedRuntimeVersion -Value ""

# Crear sitio web
New-Website -Name "BlanquitaWeb" `
    -ApplicationPool "BlanquitaAppPool" `
    -PhysicalPath "C:\inetpub\wwwroot\Blanquita" `
    -Port 80
```

O hazlo manualmente desde el Administrador de IIS (`inetmgr`).

### 7. Configurar permisos

```powershell
$path = "C:\inetpub\wwwroot\Blanquita"
$acl = Get-Acl $path
$permission = "IIS AppPool\BlanquitaAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $path $acl
```

### 8. Aplicar migraciones de base de datos

Desde la carpeta del proyecto:

```powershell
cd src\Blanquita.Web
dotnet ef database update --connection "Server=TU_SERVIDOR;Database=BlanquitaDb;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True"
```

### 9. Reiniciar IIS

```powershell
iisreset
```

### 10. Probar la aplicación

1. Abre un navegador
2. Navega a `http://localhost` (o el dominio configurado)
3. Verifica que:
   - La aplicación carga correctamente
   - Puedes iniciar sesión
   - Las funcionalidades principales funcionan
   - No hay errores en la consola del navegador

### 11. Revisar logs

```powershell
# Ver logs recientes
Get-Content C:\inetpub\wwwroot\Blanquita\logs\blanquita-*.log -Tail 50

# Ver logs de errores
Get-Content C:\inetpub\wwwroot\Blanquita\logs\errors\blanquita-errors-*.log -Tail 50
```

### 12. Verificar Event Viewer (si hay problemas)

1. Abrir Visor de eventos (`eventvwr`)
2. Ir a Registros de Windows → Aplicación
3. Buscar errores de "IIS AspNetCore Module V2"

## Rollback (en caso de problemas)

Si algo sale mal y creaste un backup:

```powershell
# Detener el sitio
Stop-Website -Name "BlanquitaWeb"

# Restaurar backup (ajusta la fecha según tu backup)
$BackupPath = "C:\inetpub\wwwroot\Blanquita-backup-YYYYMMDD-HHMMSS"
Remove-Item "C:\inetpub\wwwroot\Blanquita" -Recurse -Force
Copy-Item $BackupPath "C:\inetpub\wwwroot\Blanquita" -Recurse

# Reiniciar IIS
iisreset

# Iniciar el sitio
Start-Website -Name "BlanquitaWeb"
```

## Checklist Final

- [ ] Aplicación accesible desde el navegador
- [ ] Login funciona correctamente
- [ ] Funcionalidades principales probadas
- [ ] No hay errores en los logs
- [ ] Base de datos conectada correctamente
- [ ] Rutas de FoxPro configuradas (si aplica)
- [ ] Monitoreo configurado

## Documentación Adicional

Para más detalles, consulta:
- [Guía de Publicación Completa](../Docs/PUBLICACION_PRODUCCION.md)
- [Guía de Despliegue en IIS](../Docs/DEPLOY_IIS.md)

## Notas

- Siempre crea un backup antes de publicar en producción
- Prueba la aplicación localmente antes de publicar
- Revisa los logs después de cada publicación
- Mantén un registro de las versiones publicadas
