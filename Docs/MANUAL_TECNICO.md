# 📘 Manual Técnico - Sistema Blanquita

Sistema Integral de Gestión Comercial, Cortes de Caja, Integración Legacy FoxPro y Facturación para Carnicería Blanquita.

---

## 1. 🏗️ Arquitectura General

El sistema Blanquita está diseñado bajo los principios de **Clean Architecture (Arquitectura Limpia)** y **Domain-Driven Design (DDD)**, garantizando desacoplamiento, alta testabilidad y mantenibilidad.

```
                      ┌─────────────────────────────────────────┐
                      │            Blanquita.Web                │
                      │  (Blazor Server, MudBlazor, Middleware) │
                      └────────────────────┬────────────────────┘
                                           │
                                           ▼
                      ┌─────────────────────────────────────────┐
                      │          Blanquita.Application          │
                      │ (Commands, Queries, DTOs, Validators)   │
                      └──────────────┬───────────────────┬──────┘
                                     │                   │
                                     ▼                   ▼
    ┌───────────────────────────────────┐    ┌──────────────────────────────────┐
    │          Blanquita.Domain         │    │      Blanquita.Infrastructure    │
    │ (Entities, ValueObjects, Enums,   │◄───┤ (EF Core, PostgreSQL, FoxPro DBF,│
    │  Events, Repository Interfaces)   │    │  Printing, Serilog, Hangfire)    │
    └───────────────────────────────────┘    └─────────────────┬────────────────┘
                                                               │
                                                               ▼
                                             ┌──────────────────────────────────┐
                                             │    Blanquita.WhatsAppService     │
                                             │ (Microservicio Node.js/Baileys)  │
                                             └──────────────────────────────────┘
```

### 1.1 Estructura de Capas

| Capa / Proyecto | Responsabilidad | Tecnologías / Componentes Clave |
| :--- | :--- | :--- |
| **`Blanquita.Domain`** | Núcleo del negocio independiente de infraestructura. Contiene entidades, objetos de valor, interfaces de repositorios, eventos de dominio y excepciones de negocio. | C# .NET 8, sin dependencias externas. |
| **`Blanquita.Application`** | Casos de uso de la aplicación, orquestación de flujos de negocio, DTOs, validaciones con FluentValidation, mapeos y contratos de servicios. | MediatR, FluentValidation, AutoMapper / Mapster. |
| **`Blanquita.Infrastructure`** | Implementación de acceso a datos, proveedores externos, integración con archivos DBF (Visual FoxPro), persistencia en PostgreSQL, background jobs y logging. | EF Core, Npgsql, Hangfire, NDbfReader / DbfDataReader, Serilog, ESC/POS Sockets. |
| **`Blanquita.Web`** | Interfaz de usuario interactiva en tiempo real y controladores API auxiliares. | Blazor Server (.NET 8), MudBlazor, SignalR, SweetAlert2. |
| **`Blanquita.WhatsAppService`** | Microservicio independiente para el envío automatizado de notificaciones y facturas PDF vía WhatsApp. | Node.js, TypeScript, Express, `@whiskeysockets/baileys`. |

---

## 2. 🗄️ Base de Datos y Persistencia

### 2.1 Motor de Base de Datos
- **Motor Principal**: **PostgreSQL 14+**
- **ORM**: Entity Framework Core 8 con proveedor `Npgsql.EntityFrameworkCore.PostgreSQL`.
- **Modo de Conexión**: Configurado a través de `ConnectionStrings:DefaultConnection`.

### 2.2 Sistema de Migración Automática (`DatabaseMigrationService`)
Al iniciar la aplicación (`Program.cs` mediante `app.MigrateDatabaseAsync()`), se ejecuta un mecanismo de verificación y auto-migración estructurado:

```csharp
// src/Blanquita.Infrastructure/Persistence/Migrations/DatabaseMigrationService.cs
```
1. **Verificación de Existencia**: Comprueba la base de datos y la crea si no existe.
2. **Esquema de Tablas**: Valida la existencia de todas las tablas requeridas.
3. **Validación de Columnas y Tipos**: Agrega dinámicamente columnas faltantes sin pérdida de datos.
4. **Columnas Computadas e Índices**: Garantiza índices únicos y columnas calculadas como `CantidadTotal` en recolecciones.

### 2.3 Tablas Principales del Esquema

| Tabla | Entidad de Dominio | Descripción |
| :--- | :--- | :--- |
| `Cajeras` | `Cashier` | Catálogo de personal de cajas (N° Nómina, Nombre, Sucursal, Estado). |
| `Cajas` | `CashRegister` | Cajas registradoras, IP de impresora térmica asignada y puerto. |
| `Encargadas` | `Supervisor` | Supervisoras autorizadas para corte y recolección. |
| `Recolecciones` | `CashCollection` | Retiros parciales de efectivo con desglose por denominación (1000, 500, 200, 100, 50, 20). |
| `Cortes` | `CashCut` | Cortes de caja por turno, totales acumulados y diferencias. |
| `ConciliacionesCorte` | `ConciliacionCorte` | Conciliaciones entre valores físicos y registros del sistema. |
| `Sucursales` | `Branch` | Sucursales de la empresa y configuración asociada. |
| `DisenosEtiqueta` | `LabelDesign` | Plantillas y configuraciones de impresión de etiquetas para productos. |
| `ConfiguracionesSistema` | `SystemConfiguration` | Parámetros globales y rutas de integración. |
| `SentInvoiceLogs` | `SentInvoiceLog` | Auditoría de facturas enviadas a clientes vía WhatsApp / Correo. |
| `ReportesHistoricos` | `ReporteHistorico` | Instantáneas de reportes consolidados y analítica. |

---

## 3. 🔌 Integraciones y Servicios de Infraestructura

### 3.1 Motor de Integración FoxPro (Archivos DBF)
El sistema interactúa con bases de datos legacy de Visual FoxPro (`pos10041.dbf`, `pos10042.dbf`, `mgw10008.dbf`, `mgw10005.dbf`) aplicando técnicas avanzadas de optimización:

1. **Lectura por Streaming (`IAsyncEnumerable<DataRow>`)**: Permite procesar millones de registros sin saturar la memoria RAM del servidor.
2. **Límite de Memoria Configurable**: Detección temprana de tamaño en MB (`maxMemoryMB`) para evitar `OutOfMemoryException`.
3. **Cancelación Asíncrona (`CancellationToken`)**: Todos los lectores DBF respetan tokens de cancelación vinculados a la UI de Blazor.
4. **Caché en Memoria (`IMemoryCache`)**: Almacena temporalmente catálogos de lectura frecuente con políticas de desalojo (`SizeLimit: 1000`, `CompactionPercentage: 0.25`).

### 3.2 Microservicio de WhatsApp (`Blanquita.WhatsAppService`)
- **Tecnología**: Node.js + TypeScript con la librería **Baileys** (conexión directa con protocolo WebSocket de WhatsApp Web sin depender de Puppeteer/Chrome).
- **Seguridad**: Autenticación por cabecera HTTP `X-API-Key`.
- **Endpoints Principales**:
  - `GET /status`: Consulta el estado de conexión (`CONNECTED`, `CONNECTING`, `DISCONNECTED`).
  - `GET /qr`: Retorna el código QR en base64 para sincronización.
  - `POST /send-message`: Envío de mensajes de texto directo.
  - `POST /send-invoice`: Envío de facturas y archivos PDF adjuntos.
- **Persistencia de Sesión**: La carpeta `auth_info/` almacena las credenciales y tokens de la sesión activa de WhatsApp.

### 3.3 Impresión Térmica ESC/POS (`IPrintingService` / `PrinterService`)
- **Protocolo**: Conexión directa TCP/IP mediante Sockets en el puerto configurado (ej. `9100`).
- **Formatos**: Generación de bytes ESC/POS para tickets de corte, recibos de recolección y etiquetas de pesaje.
- **Resiliencia**: Timeouts configurables para evitar bloqueos en caso de impresoras apagadas o desconectadas.

### 3.4 Tareas Programadas con Hangfire
- **Almacenamiento**: Persistencia de trabajos en PostgreSQL mediante `Hangfire.PostgreSql`.
- **Dashboard**: Accesible en la ruta `/hangfire`, protegido mediante `HangfireDashboardAuthorizationFilter` (acceso restringido a administradores).
- **Trabajos Recurrentes**: Sincronización periódica de catálogos, depuración de logs antiguos y generación de reportes automáticos.

### 3.5 Sistema de Logging con Serilog
- **Configuración Dual**: `Program.cs` y `appsettings.json`.
- **Sinks**:
  - `Console`: Salida formateada con colores para diagnóstico en vivo.
  - `File (General)`: `logs/blanquita-.log` con rotación diaria y retención de 30 días.
  - `File (Errores)`: `logs/errors/blanquita-errors-.log` con nivel `Error` o superior y retención de 90 días.
- **Enriquecedores**: Contexto de ejecución, Environment, MachineName, ProcessId y ThreadId.

---

## 4. 🎨 Frontend y Concurrencia en Blazor Server

### 4.1 Ciclo de Vida y Seguridad en DbContext
En Blazor Server, el contenedor de inyección de dependencias comparte la instancia `Scoped` de `BlanquitaDbContext` dentro de un mismo circuito SignalR. Para prevenir errores del tipo:
```
System.InvalidOperationException: A second operation was started on this context instance before a previous operation completed.
```

**Patrón Implementado**:
1. Uso de la bandera de estado `_isInitialized` en componentes complejos.
2. Renderizado condicional de componentes que cargan datos de servidor (ej. `MudTable` con `ServerData`):
   ```razor
   @if (_isInitialized)
   {
       <MudTable ServerData="ServerReload" ... />
   }
   else
   {
       <MudProgressCircular Indeterminate="true" />
   }
   ```
3. Separación de llamadas concurrentes mediante `IDbContextFactory<BlanquitaDbContext>` en tareas en segundo plano.

---

## 5. 🔐 Seguridad y Autenticación

- **ASP.NET Core Identity**: Adaptado para PostgreSQL con `ApplicationUser` y roles (`Admin`, `Supervisor`, `Cajero`).
- **Políticas de Sesión**:
  - Tiempo de inactividad de sesión: 5 minutos con `SlidingExpiration = true`.
  - Longitud mínima de contraseña: 4 caracteres (optimizada para uso ágil en punto de venta).
  - Claims personalizados mediante `CustomUserClaimsPrincipalFactory`.

---

## 6. 🧪 Pruebas Automatizadas

La solución cuenta con proyectos de prueba en la carpeta `tests/`:
- **Pruebas Unitarias**: Verificación de reglas de dominio, validadores y mapeos.
- **Pruebas de Integración**: Lectura de archivos DBF, repositorios EF Core y serialización.
- **Comando de Ejecución**:
  ```bash
  dotnet test --configuration Release
  ```
