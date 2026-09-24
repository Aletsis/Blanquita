# 🧰 Guía de Soporte, Diagnóstico y Resolución de Problemas - Blanquita

Guía técnica de troubleshooting y mantenimiento para el equipo de soporte y TI del sistema Blanquita.

---

## 1. 🚨 Matriz de Errores Frecuentes y Soluciones

### 1.1 Errores de Inicio en IIS (HTTP 500.xx)

| Código de Error | Causa Probable | Solución |
| :--- | :--- | :--- |
| **HTTP 500.19** | Error de configuración en `web.config` o falta de módulos requeridos en IIS. | 1. Verificar que el **.NET 8 Hosting Bundle** y el módulo **URL Rewrite** estén instalados.<br>2. Comprobar que el usuario `IIS AppPool\BlanquitaAppPool` tenga permisos de lectura sobre `web.config`. |
| **HTTP 500.30** (*ANCM In-Process Start Failure*) | La aplicación .NET crasheó durante la inicialización (`Program.cs`). | 1. Abrir consola en el directorio de publicación y ejecutar directamente `dotnet Blanquita.Web.dll` para ver la excepción en pantalla.<br>2. Revisar el Visor de Eventos de Windows (`eventvwr.msc` -> Registros de Windows -> Aplicación -> Origen: *IIS AspNetCore Module V2*).<br>3. Verificar la cadena de conexión a PostgreSQL en `appsettings.Production.json`. |
| **HTTP 502.5** (*Process Failure*) | Incompatibilidad de versión de runtime o arquitectura x86/x64. | Confirmar que el Application Pool tenga la opción *Habilitar aplicaciones de 32 bits* configurada en `False` si el Hosting Bundle es de 64 bits. |

---

### 1.2 Problemas de Concurrencia de DbContext en Blazor Server

**Síntoma:**
```
System.InvalidOperationException: A second operation was started on this context instance before a previous operation completed.
This is usually caused by different threads concurrently using the same instance of DbContext.
```

**Causa:**
En Blazor Server, componentes que ejecutan múltiples llamadas asíncronas en paralelo (ej. `OnInitializedAsync` y un `MudTable` con `ServerData`) intentan usar la misma instancia `Scoped` de `BlanquitaDbContext`.

**Solución Implementada & Buenas Prácticas:**
1. Usar siempre el flag `_isInitialized` en el componente para condicionar el renderizado de tablas:
   ```razor
   @if (_isInitialized)
   {
       <MudTable ServerData="ServerReload" ... />
   }
   ```
2. Para tareas en segundo plano o eventos independientes en la misma página, inyectar `IDbContextFactory<BlanquitaDbContext>` y crear un contexto con ámbito aislado (`using var context = dbFactory.CreateDbContext()`).

---

### 1.3 Problemas con Archivos FoxPro (DBF)

| Problema | Causa | Solución |
| :--- | :--- | :--- |
| **`FileNotFoundException` o Ruta no accesible** | Rutas compartidas de red (UNC) no alcanzables desde la cuenta del servicio IIS. | 1. Asegurarse de que el usuario del Application Pool o el usuario configurado en IIS tenga permisos de red sobre la carpeta compartida.<br>2. Configurar la ruta física en `appsettings.json` o en `Configuraciones -> Parámetros`. |
| **`IOException: The process cannot access the file because it is being used by another process`** | Bloqueo exclusivo por parte del sistema POS legacy. | El lector de DBF de Blanquita está configurado con `FileShare.ReadWrite`. Si el bloqueo persiste, verificar que no existan bloqueos a nivel de sesión SMB en el servidor de archivos. |
| **Búsqueda lenta o alto consumo de memoria** | Consulta sobre archivos DBF masivos (>500 MB). | Utilizar las funciones de búsqueda con **Streaming** (`SearchInDbfFileStreamAsync`) y límite de memoria configurado (`maxMemoryMB`). |

---

### 1.4 Problemas con el Servicio de WhatsApp

| Problema | Causa | Solución |
| :--- | :--- | :--- |
| **Estado `DISCONNECTED` constante** | La sesión en el teléfono fue cerrada o expiró la autenticación de Baileys. | 1. Ir a `Configuraciones` -> `WhatsApp` en la aplicación.<br>2. Escanear nuevamente el código QR desde la app móvil de WhatsApp.<br>3. Si el QR no carga, reiniciar el proceso de Node.js (`pm2 restart blanquita-whatsapp`). |
| **Error `401 Unauthorized` al enviar facturas** | Discrepancia entre la clave en `appsettings.json` (`WhatsAppService:ApiKey`) y `.env` (`WHATSAPP_API_KEY`). | Igualar el valor de la clave API en ambos archivos y reiniciar el servicio. |
| **Falla al adjuntar archivo PDF** | Archivo PDF bloqueado, temporal no encontrado o URL no accesible. | Verificar que el directorio temporal de almacenamiento de PDFs tenga permisos de escritura. |

---

### 1.5 Problemas con Impresoras Térmicas (ESC/POS)

| Problema | Causa | Solución |
| :--- | :--- | :--- |
| **Timeout de Conexión (`SocketException`)** | Impresora apagada, cable de red desconectado o IP modificada. | 1. Realizar `ping <IP_IMPRESORA>` desde el servidor.<br>2. Confirmar que el puerto `9100` (puerto RAW estándar) esté abierto.<br>3. Verificar en `Cajas -> Configuración` que la IP registrada coincida con la asignada en el router. |
| **Caracteres extraños o cortes a mitad de ticket** | Juego de caracteres (CodePage) incompatible en la impresora. | Verificar que la impresora térmica soporte emulación ESC/POS y página de códigos `PC850` o `CP437`. |

---

### 1.6 Desconexión de Señal en Blazor Server (SignalR)

**Síntoma:** Aparece el mensaje *"Intentando reconectar con el servidor..."* en el navegador.
1. **Verificar WebSockets en IIS**: Si WebSockets no está instalado, SignalR caerá en *Server-Sent Events* o *Long Polling*, causando inestabilidad y saturación de conexiones.
2. **Revisar Timeouts de Sesión**: La cookie de autenticación expira tras 5 minutos de inactividad por diseño de seguridad.

---

## 2. 📂 Ubicación y Análisis de Logs

### 2.1 Estructura de Carpetas de Logs

```
<Ruta_Publicación>/
├── logs/
│   ├── blanquita-20260923.log          # Eventos generales informativos y de negocio
│   └── errors/
│       └── blanquita-errors-20260923.log  # Excepciones, errores críticos (HTTP 500, DB)
└── Blanquita.WhatsAppService/
    └── auth_info/                      # Tokens y credenciales de sesión de WhatsApp
```

### 2.2 Patrones de Búsqueda Útiles en Logs

- **Filtrar errores en PowerShell**:
  ```powershell
  Get-Content .\logs\errors\blanquita-errors-*.log -Tail 100
  ```
- **Monitorear en tiempo real (Tail)**:
  ```powershell
  Get-Content .\logs\blanquita-20260923.log -Wait -Tail 20
  ```
- **Buscar errores específicos de DBF**:
  ```powershell
  Select-String -Path .\logs\*.log -Pattern "FoxPro" -CaseSensitive:$false
  ```

---

## 3. 📞 Niveles de Escalamiento de Soporte

1. **Nivel 1 (Operación / Piso de Venta)**:
   - Verificación de conexión de red e impresoras térmicas.
   - Reintento de corte de caja o recolección.
   - Re-escaneo de código QR de WhatsApp.
2. **Nivel 2 (Administrador de Sistemas / TI)**:
   - Revisión de logs en `logs/errors/`.
   - Inspección y reintento de tareas fallidas en `/hangfire`.
   - Verificación de rutas de archivos DBF y conectividad con PostgreSQL.
   - Reinicio de Application Pool en IIS (`Restart-WebAppPool BlanquitaAppPool`).
3. **Nivel 3 (Equipo de Desarrollo)**:
   - Ajustes en modelos de dominio, migraciones de base de datos o lógica de negocio en Clean Architecture.
