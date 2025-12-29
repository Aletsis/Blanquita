# Guía de Despliegue en IIS (Internet Information Services)

Esta guía detalla los pasos necesarios para desplegar la aplicación Blanquita (Blazor Server) en un servidor Windows con IIS.

## 1. Requisitos Previos del Servidor

Antes de comenzar, asegúrese de que el servidor o la máquina destino tenga instalado lo siguiente:

### 1.1. IIS (Internet Information Services)
Debe estar habilitado en Windows.
- **Windows Server:** Administrador del servidor -> Agregar roles y características -> Servidor web (IIS).
- **Windows 10/11:** Habilitar o deshabilitar las características de Windows -> Internet Information Services.
  - Asegúrese de marcar **World Wide Web Services** -> **Application Development Features** -> **WebSocket Protocol** (Importante para Blazor Server).

### 1.2. .NET Core Hosting Bundle
Instale el **.NET Core Hosting Bundle** correspondiente a la versión de .NET que usa la aplicación (actualmente .NET 8.0).
- Descárguelo desde el sitio oficial de Microsoft: [Download .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) (busque "Hosting Bundle" en la sección de ASP.NET Core Runtime).
- **Importante:** Después de la instalación, es recomendable reiniciar el servidor o ejecutar `iisreset` en una consola administrativa.

### 1.3. URL Rewrite Module (Opcional pero Recomendado)
Si planea usar reglas de reescritura o redirecciones HTTPS, instale el [IIS URL Rewrite Module](https://www.iis.net/downloads/microsoft/url-rewrite).

## 2. Publicación de la Aplicación

Debe generar los archivos listos para producción. Puede hacerlo desde Visual Studio o línea de comandos.

### Opción A: Desde Visual Studio
1. Clic derecho en el proyecto **Blanquita.Web** -> **Publicar**.
2. Seleccione **Carpeta** como destino.
3. Elija una ruta (ej. `bin\Release\net8.0\publish`).
4. Clic en **Publicar**.

### Opción B: Desde Línea de Comandos (CLI)
Abra una terminal en la carpeta de la solución o del proyecto `src/Blanquita.Web` y ejecute:

```powershell
dotnet publish -c Release -o C:\inetpub\wwwroot\Blanquita
```
*Nota: Reemplace `C:\inetpub\wwwroot\Blanquita` con la ruta deseada en el servidor.*

## 3. Configuración en IIS

1. Abra el **Administrador de IIS** (`inetmgr`).

### 3.1. Crear un Application Pool (Grupo de Aplicaciones)
1. En el panel izquierdo, clic derecho en **Grupos de aplicaciones** -> **Agregar grupo de aplicaciones...**
2. **Nombre:** `BlanquitaAppPool` (o el nombre que prefiera).
3. **Versión de .NET CLR:** Seleccione **Sin código administrado** (No Managed Code). *Nota: .NET Core/5+ gestiona sus propios procesos, IIS actúa como proxy inverso.*
4. **Modo de canalización:** Integrada.
5. Clic en **Aceptar**.

### 3.2. Crear el Sitio Web
1. En el panel izquierdo, clic derecho en **Sitios** -> **Agregar sitio web...**
2. **Nombre del sitio:** `BlanquitaWeb`.
3. **Grupo de aplicaciones:** Seleccione el que creó en el paso anterior (`BlanquitaAppPool`).
4. **Ruta de acceso física:** Seleccione la carpeta donde publicó los archivos (ej. `C:\inetpub\wwwroot\Blanquita`).
5. **Enlace (Binding):**
   - **Tipo:** http (o https si tiene certificado).
   - **Puerto:** 80 (o uno diferente si el 80 está ocupado, ej. 8080).
   - **Nombre de host:** Déjelo en blanco para probar por IP o `localhost`, o ponga el dominio si tiene uno configurado.
6. Clic en **Aceptar**.

## 4. Configuración de Base de Datos y Permisos

### 4.1. Cadena de Conexión
Asegúrese de que el archivo `appsettings.json` (o `appsettings.Production.json`) en la carpeta publicada tenga la cadena de conexión correcta apuntando a su servidor SQL Server de producción.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SU_SERVIDOR;Database=BlanquitaDB;User Id=USUARIO_SQL;Password=CONTRASEÑA_SQL;TrustServerCertificate=True;"
}
```

> **Nota de Seguridad:** En producción, evite guardar contraseñas en texto plano si es posible. Considere usar variables de entorno del sistema o Azure Key Vault si aplica.

### 4.2. Permisos de Archivos (Logs y Base de datos local)
El usuario del Application Pool (`IIS AppPool\BlanquitaAppPool`) necesita permisos de lectura y ejecución sobre la carpeta física.
Si su aplicación escribe logs en una carpeta específica o usa bases de datos locales (como SQLite o LocalDB), asegúrese de dar permisos de **Escritura/Modificación** a ese usuario en esas carpetas específicas.

## 5. Pruebas y Solución de Problemas

1. Abra un navegador y navegue a `http://localhost` (o el puerto/dominio configurado).
2. Si ve la pantalla de carga de Blazor, ¡felicidades!

### Errores Comunes (HTTP 500.xx)

- **HTTP 500.19:** Generalmente problema de permisos en `web.config` o falta instalar el URL Rewrite Module.
- **HTTP 500.30 - ANCM In-Process Start Failure:** La aplicación falló al iniciar.
  - Revise el **Visor de Eventos (Event Viewer)** de Windows -> Registros de Windows -> Aplicación. Busque errores de "IIS AspNetCore Module V2".
  - Intente ejecutar el `.exe` de la aplicación (ej. `Blanquita.Web.exe`) directamente desde la carpeta de publicación usando la consola para ver si arroja errores de excepción (ej. conexión a BD fallida).
  - Habilite logs de stdout en `web.config`: cambie `stdoutLogEnabled="false"` a `true` y cree la carpeta `logs` dentro del directorio de publicación.

### WebSockets
Si la conexión se cae frecuentemente o ve errores de reconexión en Blazor Server, verifique que el protocolo **WebSocket** esté habilitado en las características de Windows del servidor (ver sección 1.1).
