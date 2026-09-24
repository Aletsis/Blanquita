# 📚 Centro de Documentación - Sistema Blanquita

Bienvenido a la documentación oficial del sistema **Blanquita**. Esta carpeta contiene los manuales técnicos, guías operativas, instrucciones de despliegue y documentación de soporte.

---

## 📑 Índice de Manuales y Guías

| Documento | Audiencia | Descripción |
| :--- | :--- | :--- |
| **[📘 Manual Técnico](MANUAL_TECNICO.md)** | Desarrolladores, Arquitectos de Software | Detalle de Clean Architecture, entidades de dominio, esquema de PostgreSQL, sistema de migraciones, motor FoxPro DBF por streaming, microservicio de WhatsApp, Hangfire y logging con Serilog. |
| **[📋 Manual de Operaciones](MANUAL_OPERACIONES.md)** | Administradores, Supervisores, Operadores | Flujos operativos de recolección de efectivo, cortes de caja, conciliaciones, consulta de ventas FoxPro, facturación electrónica, envío por WhatsApp, diseño de etiquetas y respaldos. |
| **[🚀 Guía de Despliegue en Producción](GUIA_DESPLIEGUE.md)** | Administradores de Sistemas, DevOps | Requisitos de servidor, configuración de IIS (Application Pool, WebSockets, URL Rewrite), publicación automatizada (`publish-production.ps1`), variables de entorno y microservicio Node.js. |
| **[🧰 Guía de Soporte y Troubleshooting](GUIA_SOPORTE_TROUBLESHOOTING.md)** | Soporte Técnico, Mesa de Ayuda, TI | Matriz de diagnóstico y solución para errores de IIS (500.xx), concurrencia de `DbContext`, problemas con archivos DBF, desvinculación de WhatsApp, fallas en impresoras térmicas y análisis de logs. |

---

## 📌 Enlaces Rápidos y Arquitectura del Proyecto

- **Solución**: `Blanquita.sln` (.NET 8)
- **Capa Web (UI)**: `src/Blanquita.Web` (Blazor Server + MudBlazor)
- **Capa Aplicación**: `src/Blanquita.Application`
- **Capa Dominio**: `src/Blanquita.Domain`
- **Capa Infraestructura**: `src/Blanquita.Infrastructure` (EF Core + PostgreSQL + FoxPro DBF)
- **Microservicio WhatsApp**: `src/Blanquita.WhatsAppService` (Node.js + Baileys)
