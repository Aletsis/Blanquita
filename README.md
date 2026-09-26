# 🥩 Blanquita - Sistema de Gestión y Punto de Venta

Sistema integral para la administración operativa, control de efectivo, integración con sistemas legacy FoxPro, facturación y notificaciones vía WhatsApp para la carnicería **Blanquita**.

Desarrollado con **.NET 8** y **Blazor Server** bajo los patrones de **Clean Architecture** y **Domain-Driven Design (DDD)**.

---

## 📋 Tabla de Contenidos
- [Características Principales](#-características-principales)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Requisitos Previos](#-requisitos-previos)
- [Configuración y Ejecución Local](#-configuración-y-ejecución-local)
- [Microservicio de WhatsApp](#-microservicio-de-whatsapp)
- [Publicación y Despliegue](#-publicación-y-despliegue)
- [📚 Documentación Oficial](#-documentación-oficial)

---

## ✨ Características Principales

- **Control de Efectivo y Cajas**: Registro de recolecciones de valores por denominación, cálculo de cortes de caja por turno y conciliación automática de diferencias.
- **Integración FoxPro DBF de Alto Rendimiento**: Lectura optimizada con streaming asíncrono (`IAsyncEnumerable`), cancelación en vivo y control de memoria para archivos DBF de gran tamaño.
- **Facturación y Despacho WhatsApp**: Búsqueda de documentos comerciales y envío automatizado de tickets y facturas PDF a clientes por WhatsApp.
- **Impresión Térmica en Red**: Emisión de comprobantes y tickets directamente a impresoras térmicas ESC/POS mediante sockets TCP/IP.
- **Diseño de Etiquetas**: Editor e impresión de etiquetas con código de barras para productos pesables y empaquetados.
- **Tareas Programadas (Hangfire)**: Procesamiento en segundo plano de sincronización de datos y mantenimiento periódico con panel web en `/hangfire`.
- **Auditoría y Logging Estructurado**: Registro detallado de eventos y errores con Serilog (consola y archivos rotativos diarios).

---

## 🛠 Tecnologías Utilizadas

- **Backend / Web**: .NET 8, C#, Blazor Server (Interactive Server Components), MudBlazor, MediatR, FluentValidation.
- **Base de Datos**: PostgreSQL 14+ con Entity Framework Core 8 (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- **Background Jobs**: Hangfire con almacenamiento en PostgreSQL (`Hangfire.PostgreSql`).
- **Logging**: Serilog (Console, File sinks con enriquecimiento de contexto).
- **Microservicio WhatsApp**: Node.js, TypeScript, Express, `@whiskeysockets/baileys`.
- **Integración Legacy**: NDbfReader / DbfDataReader con streaming personalizado.

---

## 🏗 Estructura del Proyecto

```
Blanquita/
├── src/
│   ├── Blanquita.Domain/            # Entidades, Value Objects, Interfaces de Repositorio y Eventos
│   ├── Blanquita.Application/       # Casos de uso (Commands/Queries), DTOs, Validadores y Servicios
│   ├── Blanquita.Infrastructure/    # EF Core, PostgreSQL, Repositorios, Servicios DBF, Hangfire e Impresión
│   ├── Blanquita.Web/               # UI Blazor Server, Componentes MudBlazor, Controladores y Middleware
│   └── Blanquita.WhatsAppService/   # Microservicio Node.js/TypeScript para envío por WhatsApp (Baileys)
├── tests/                           # Pruebas unitarias y de integración
├── Docs/                            # Documentación técnica, operativa y de despliegue
├── publish-production.ps1           # Script automatizado de publicación para producción
└── README.md
```

---

## 💻 Requisitos Previos

1. **.NET 8.0 SDK** (v8.0.x o superior).
2. **PostgreSQL 14.x o superior**.
3. **Node.js LTS (v18 o v20)** y **npm** (para el microservicio de WhatsApp).
4. **Visual Studio 2022** (v17.8+), **VS Code** o **Rider**.

---

## 🚀 Configuración y Ejecución Local

### 1. Clonar el repositorio
```bash
git clone https://github.com/Aletsis/Blanquita.git
cd Blanquita
```

### 2. Configurar la Base de Datos y Rutas
Edite `src/Blanquita.Web/appsettings.json` o configure variables de entorno (`.env`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=BlanquitaDB;Username=postgres;Password=tu_password"
  },
  "FoxPro": {
    "Pos10041Path": "C:\\datos\\pos10041.dbf",
    "Pos10042Path": "C:\\datos\\pos10042.dbf",
    "Mgw10008Path": "C:\\datos\\mgw10008.dbf",
    "Mgw10005Path": "C:\\datos\\mgw10005.dbf"
  }
}
```

> **Nota:** La base de datos y sus tablas/índices se inicializan y migran **automáticamente** al arrancar la aplicación gracias al servicio `DatabaseMigrationService`.

### 3. Restaurar y Ejecutar la Aplicación Web
```bash
dotnet restore
dotnet run --project src/Blanquita.Web
```

La aplicación estará disponible por defecto en:
- `https://localhost:7001` o `http://localhost:5000`
- Dashboard de Hangfire: `https://localhost:7001/hangfire` (Requiere rol `Admin`)

---

## 📱 Microservicio de WhatsApp

Para habilitar el envío de facturas y notificaciones por WhatsApp:

```bash
cd src/Blanquita.WhatsAppService
npm install
npm run dev
```

El servicio se iniciará en `http://localhost:3001`. En la interfaz web (`Configuraciones -> WhatsApp`) podrá escanear el código QR para vincular la sesión.

---

## 🌐 CI/CD y Despliegue

La solución cuenta con canalizaciones automatizadas de **Integración Continua (CI)** y **Entrega Continua (CD)** mediante GitHub Actions, así como guías y scripts de despliegue para servidores Windows con **IIS**:

- 👉 **[Guía de CI/CD con GitHub Actions](Docs/CI_CD_GUIDE.md)**: Flujos de compilación, ejecución de tests y empaquetado automático de releases (.zip).
- 👉 **[Script de Despliegue Automático](deploy-release.ps1)**: Utilidad de PowerShell para desplegar el paquete descargado en IIS con backup automático y reinicio de AppPool.
- 👉 **[Guía de Despliegue en IIS](Docs/GUIA_DESPLIEGUE.md)**: Configuración del servidor IIS, Application Pool "Sin código administrado", WebSockets y permisos.

Para publicar manualmente desde PowerShell:

```powershell
# Ejecutar script automatizado con backup
.\publish-production.ps1 -OutputPath "C:\inetpub\wwwroot\Blanquita" -Configuration Release -CreateBackup
```

---

## 📚 Documentación Oficial

Toda la documentación técnica y operativa se encuentra organizada en la carpeta `Docs/`:

| Manual / Guía | Enlace |
| :--- | :--- |
| **Guía de CI/CD (GitHub Actions)** | [Docs/CI_CD_GUIDE.md](Docs/CI_CD_GUIDE.md) |
| **Manual Técnico** | [Docs/MANUAL_TECNICO.md](Docs/MANUAL_TECNICO.md) |
| **Manual de Operaciones** | [Docs/MANUAL_OPERACIONES.md](Docs/MANUAL_OPERACIONES.md) |
| **Guía de Despliegue en Producción** | [Docs/GUIA_DESPLIEGUE.md](Docs/GUIA_DESPLIEGUE.md) |
| **Guía de Soporte y Troubleshooting** | [Docs/GUIA_SOPORTE_TROUBLESHOOTING.md](Docs/GUIA_SOPORTE_TROUBLESHOOTING.md) |
| **Índice General de Documentación** | [Docs/README.md](Docs/README.md) |
