# Instrucciones Finales de Despliegue - Blanquita

**Fecha de publicación:** 2026-01-03 19:03

## ✅ Estado Actual

La aplicación ha sido **publicada exitosamente** en:
```
C:\Users\B10 Caja 2\Desktop\Blanquita-Publish
```

## 📋 Próximos Pasos para Completar el Despliegue

### 1. Configurar la Cadena de Conexión de Producción

**IMPORTANTE:** Antes de mover los archivos al servidor, debes actualizar la configuración de producción.

Edita el archivo:
```
C:\Users\B10 Caja 2\Desktop\Blanquita-Publish\appsettings.Production.json
```

Actualiza la cadena de conexión con los datos de tu servidor de producción:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR_PRODUCCION;Database=BlanquitaDb;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=True"
  }
}
```

**⚠️ IMPORTANTE:** 
- Reemplaza `TU_SERVIDOR_PRODUCCION` con la dirección de tu servidor SQL
- Reemplaza `TU_USUARIO` y `TU_CONTRASEÑA` con las credenciales correctas
- Si usas autenticación de Windows, usa `Trusted_Connection=True` en lugar de User Id y Password

### 2. Configurar Rutas de FoxPro (si aplica)

Si tu aplicación necesita acceder a archivos FoxPro, actualiza también estas rutas en `appsettings.Production.json`:

```json
{
  "FoxPro": {
    "Pos10041Path": "RUTA_REAL_POS_10041",
    "Pos10042Path": "RUTA_REAL_POS_10042",
    "Mgw10008Path": "RUTA_REAL_MGW_10008",
    "Mgw10005Path": "RUTA_REAL_MGW_10005"
  }
}
```

### 3. Mover los Archivos al Servidor de Producción

Tienes dos opciones:

#### Opción A: Usar PowerShell como Administrador (Recomendado)

1. Abre PowerShell **como Administrador**
2. Ejecuta el siguiente comando:

```powershell
# Crear el directorio si no existe
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\Blanquita" -Force

# Copiar los archivos
Copy-Item -Path "$env:USERPROFILE\Desktop\Blanquita-Publish\*" -Destination "C:\inetpub\wwwroot\Blanquita" -Recurse -Force
```

#### Opción B: Copiar Manualmente

1. Abre el Explorador de Windows como Administrador
2. Navega a `C:\Users\B10 Caja 2\Desktop\Blanquita-Publish`
3. Copia todos los archivos
4. Pégalos en `C:\inetpub\wwwroot\Blanquita`

### 4. Configurar IIS

Si es la primera vez que despliegas, necesitas configurar IIS:

#### 4.1. Crear Application Pool

Abre PowerShell como Administrador y ejecuta:

```powershell
Import-Module WebAdministration

# Crear Application Pool
New-WebAppPool -Name "BlanquitaAppPool"
Set-ItemProperty IIS:\AppPools\BlanquitaAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\BlanquitaAppPool -Name managedPipelineMode -Value "Integrated"
```

O manualmente desde el Administrador de IIS:
1. Abrir `inetmgr`
2. Clic derecho en **Grupos de aplicaciones** → **Agregar grupo de aplicaciones**
3. Nombre: `BlanquitaAppPool`
4. Versión de .NET CLR: **Sin código administrado**
5. Clic en **Aceptar**

#### 4.2. Crear Sitio Web

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
6. Clic en **Aceptar**

#### 4.3. Configurar Permisos

```powershell
$path = "C:\inetpub\wwwroot\Blanquita"
$acl = Get-Acl $path
$permission = "IIS AppPool\BlanquitaAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $path $acl

# Permisos para la carpeta de logs
$logsPath = Join-Path $path "logs"
$acl = Get-Acl $logsPath
$acl.SetAccessRule($accessRule)
Set-Acl $logsPath $acl
```

### 5. Aplicar Migraciones de Base de Datos

Desde PowerShell como Administrador:

```powershell
cd C:\inetpub\wwwroot\Blanquita

# Establecer la variable de entorno
$env:ASPNETCORE_ENVIRONMENT="Production"

# Ejecutar migraciones (reemplaza la cadena de conexión con tus datos)
dotnet Blanquita.Web.dll -- ef database update
```

O desde la carpeta del proyecto:

```powershell
cd "C:\Users\B10 Caja 2\source\repos\Blanquita\src\Blanquita.Web"
dotnet ef database update --connection "Server=TU_SERVIDOR;Database=BlanquitaDb;User Id=TU_USUARIO;Password=TU_CONTRASEÑA;TrustServerCertificate=True"
```

### 6. Reiniciar IIS

```powershell
iisreset
```

### 7. Verificar el Despliegue

1. Abre un navegador
2. Navega a `http://localhost` (o el puerto/dominio configurado)
3. Verifica que:
   - [ ] La aplicación carga correctamente
   - [ ] Puedes iniciar sesión
   - [ ] Las funcionalidades principales funcionan
   - [ ] No hay errores en la consola del navegador

### 8. Revisar Logs

```powershell
# Ver logs recientes
Get-Content C:\inetpub\wwwroot\Blanquita\logs\blanquita-*.log -Tail 50

# Ver logs de errores
Get-Content C:\inetpub\wwwroot\Blanquita\logs\errors\blanquita-errors-*.log -Tail 50
```

Si hay errores, también revisa el **Visor de eventos**:
1. Abrir `eventvwr`
2. Ir a **Registros de Windows** → **Aplicación**
3. Buscar errores de **IIS AspNetCore Module V2**

## 🔧 Solución de Problemas

### Error: HTTP 500.30 - ANCM In-Process Start Failure

1. Habilitar logs de stdout en `web.config`:
```xml
<aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
```
2. Revisar los logs generados
3. Verificar la cadena de conexión
4. Verificar que el .NET 9.0 Hosting Bundle esté instalado

### Error: No se puede conectar a la base de datos

1. Verificar que SQL Server esté ejecutándose
2. Verificar la cadena de conexión en `appsettings.Production.json`
3. Verificar que el usuario tenga permisos en la base de datos
4. Probar la conexión con SQL Server Management Studio

### La aplicación se desconecta frecuentemente

1. Habilitar WebSocket Protocol en características de Windows
2. Verificar que el firewall permita WebSockets
3. Ajustar timeout del Application Pool en IIS

## 📊 Checklist Final

- [ ] Cadena de conexión actualizada en `appsettings.Production.json`
- [ ] Rutas de FoxPro configuradas (si aplica)
- [ ] Archivos copiados a `C:\inetpub\wwwroot\Blanquita`
- [ ] Application Pool creado en IIS
- [ ] Sitio web creado en IIS
- [ ] Permisos configurados
- [ ] Migraciones de base de datos ejecutadas
- [ ] IIS reiniciado
- [ ] Aplicación accesible desde el navegador
- [ ] Funcionalidades principales probadas
- [ ] Logs revisados (sin errores críticos)

## 📚 Documentación Adicional

- [Guía de Publicación Completa](PUBLICACION_PRODUCCION.md)
- [Guía de Despliegue en IIS](DEPLOY_IIS.md)
- [README del Proyecto](../README.md)

## 🆘 Soporte

Si encuentras problemas:
1. Revisa los logs en `C:\inetpub\wwwroot\Blanquita\logs`
2. Revisa el Visor de eventos de Windows
3. Consulta la documentación en la carpeta `Docs/`

---

**¡Buena suerte con el despliegue!** 🚀
