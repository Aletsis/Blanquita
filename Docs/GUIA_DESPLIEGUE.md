# 🚀 Guía de Despliegue e Instalación en Producción - Blanquita

Guía paso a paso para la publicación, configuración y puesta en marcha del sistema Blanquita en entornos de producción (Windows Server con IIS / Entornos compatibles).

---

## 1. 📋 Requisitos Previos del Servidor

### 1.1 Servidor Web IIS y Módulos
En **Windows Server** (Administrador del Servidor -> Roles y Características) o **Windows 10/11** (Características de Windows):
- **Internet Information Services (IIS)**:
  - *World Wide Web Services* -> *Application Development Features* -> **WebSocket Protocol** (⚠️ Obligatorio para Blazor Server / SignalR).
- **.NET 8.0 Hosting Bundle**:
  - Descargar e instalar [.NET 8.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/8.0) (incluye ASP.NET Core Runtime y IIS AspNetCore Module V2).
  - Ejecutar `iisreset` tras la instalación.
- **IIS URL Rewrite Module**:
  - Instalar desde el sitio oficial de IIS para soportar redirección HTTPS y reescritura de URLs.

### 1.2 Base de Datos
- **PostgreSQL 14.x o superior** instalado y accesible desde el servidor web.
- Base de datos creada (ej. `BlanquitaDB`) con usuario y contraseña con permisos de lectura, escritura y creación de tablas/índices.

### 1.3 Entorno Node.js para WhatsApp Service
- **Node.js LTS (v18 o v20)** instalado en el servidor.
- Gestor de procesos: **PM2**, **NSSM** (como servicio de Windows) o **iisnode**.

---

## 2. 📦 Publicación de la Aplicación

### Opción A: Publicación Automatizada con Script PowerShell (Recomendada)
La raíz del repositorio incluye el script `publish-production.ps1` que realiza verificación de Git, backup automático, compilación, ejecución de tests y publicación de archivos:

```powershell
# Ejecutar en PowerShell como Administrador desde la raíz del proyecto
.\publish-production.ps1 -OutputPath "C:\inetpub\wwwroot\Blanquita" -Configuration Release -CreateBackup
```

### Opción B: Publicación Manual por CLI
```powershell
# Publicar el proyecto Web
dotnet publish src\Blanquita.Web\Blanquita.Web.csproj `
    -c Release `
    -o "C:\inetpub\wwwroot\Blanquita" `
    /p:EnvironmentName=Production

# Compilar el microservicio de WhatsApp
cd src\Blanquita.WhatsAppService
npm install
npm run build
```

---

## 3. ⚙️ Configuración en IIS

### 3.1 Crear el Application Pool (Grupo de Aplicaciones)
1. Abrir **Administrador de IIS (`inetmgr`)**.
2. Ir a **Grupos de aplicaciones** -> **Agregar grupo de aplicaciones...**
   - **Nombre**: `BlanquitaAppPool`
   - **Versión de .NET CLR**: **Sin código administrado (No Managed Code)** *(Crucial para .NET 8+)*
   - **Modo de canalización administrada**: **Integrada**
3. En la configuración avanzada del grupo:
   - Establecer **Modelo de proceso** -> **Cargar perfil de usuario** en `True`.

### 3.2 Crear el Sitio Web
1. En IIS -> Clic derecho en **Sitios** -> **Agregar sitio web...**
   - **Nombre del sitio**: `Blanquita`
   - **Grupo de aplicaciones**: `BlanquitaAppPool`
   - **Ruta de acceso física**: `C:\inetpub\wwwroot\Blanquita`
   - **Enlace (Binding)**: `http` puerto `80` (o `https` puerto `443` con certificado SSL).

### 3.3 Asignar Permisos de Sistema de Archivos
El usuario del Application Pool requiere control total sobre el directorio de publicación (especialmente carpetas de logs y temporales):

```powershell
# Ejecutar en PowerShell como Administrador
$path = "C:\inetpub\wwwroot\Blanquita"
$acl = Get-Acl $path
$permission = "IIS AppPool\BlanquitaAppPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl $path $acl
```

---

## 4. 🔑 Configuración de Variables y Archivos de Entorno

### 4.1 Archivo `appsettings.Production.json`
Ubicar en la raíz del directorio de publicación:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=BlanquitaDB;Username=postgres;Password=TuPasswordSeguro;TrustServerCertificate=true;"
  },
  "FoxPro": {
    "Pos10041Path": "\\\\servidor\\dbf\\pos10041.dbf",
    "Pos10042Path": "\\\\servidor\\dbf\\pos10042.dbf",
    "Mgw10008Path": "\\\\servidor\\dbf\\mgw10008.dbf",
    "Mgw10005Path": "\\\\servidor\\dbf\\mgw10005.dbf"
  },
  "WhatsAppService": {
    "BaseUrl": "http://localhost:3001",
    "ApiKey": "TU_WHATSAPP_API_KEY_SECRETA"
  },
  "AllowedHosts": "*"
}
```

### 4.2 Configuración del Microservicio de WhatsApp (`Blanquita.WhatsAppService`)
1. Crear un archivo `.env` en la carpeta del servicio:
   ```env
   PORT=3001
   NODE_ENV=production
   WHATSAPP_API_KEY=TU_WHATSAPP_API_KEY_SECRETA
   ```
2. Iniciar el servicio con **PM2** o **NSSM**:
   ```bash
   # Opción PM2:
   npm install -g pm2
   pm2 start dist/index.js --name "blanquita-whatsapp"
   pm2 save
   pm2 startup
   ```

---

## 5. 🔍 Lista de Verificación Post-Despliegue (Checklist)

- [ ] Base de datos PostgreSQL operativa y accesible con la cadena de conexión.
- [ ] Tablas e índices creados automáticamente por `DatabaseMigrationService` en el primer inicio.
- [ ] Protocolo **WebSocket** habilitado en IIS.
- [ ] Permisos de lectura/escritura concedidos a `IIS AppPool\BlanquitaAppPool`.
- [ ] Carpetas `logs/` y `logs/errors/` creadas y registrando eventos de Serilog.
- [ ] Microservicio de WhatsApp en ejecución en el puerto configurado (ej. `3001`).
- [ ] Dispositivo WhatsApp vinculado escaneando el código QR en `/configuraciones/whatsapp`.
- [ ] Acceso exitoso al panel de Hangfire en `https://<servidor>/hangfire` con usuario `Admin`.
- [ ] Prueba de lectura de archivos DBF desde el módulo de Documentos.
- [ ] Prueba de impresión de ticket en impresora de red (ESC/POS).
