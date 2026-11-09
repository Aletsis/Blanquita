# Pendientes de la auditoría

Estado al 3 de noviembre de 2026, rama `fix/auditoria-parte2` (aún sin fusionar a `master` ni desplegar).

Lo que ya está hecho vive en el historial de commits (`git log master..fix/auditoria-parte2`).
Este archivo solo lista lo que falta. Bórralo cuando todo esté cerrado.

## Estado actual

- Compila con 0 advertencias y 0 errores.
- Pruebas: Domain 112/112, Application 117/117, Infrastructure 119 aprobadas y 1 omitida (diagnóstico manual con un respaldo real de `C:\`).
- `dotnet list package --vulnerable` no reporta nada.
- `Blanquita.Web.Tests` existe pero no tiene pruebas (opcional: agregar pruebas de componentes con bUnit).

## 1. Decisiones pendientes

- **Licencia de MediatR (14.2):** desde la versión 13 es comercial y los logs muestran el aviso de licencia faltante. Conseguir la llave (hay edición Community gratuita según ingresos de la empresa) y configurarla con `cfg.LicenseKey` en `AddMediatR` (`src/Blanquita.Application/DependencyInjection.cs`), leyéndola de la configuración o del `.env`, no escrita en el código.
- **Licencia de QuestPDF:** está en `LicenseType.Community`, gratuita solo por debajo de cierto ingreso anual. Confirmar que la empresa califica o conseguir la licencia correspondiente.
- **Filtro por sucursal en recolecciones:** se dejó pendiente por decisión del usuario.

## 2. Documentación final (actualizar `Docs/` al cerrar la auditoría)

- `.env` de la aplicación web con `ConnectionStrings__DefaultConnection` y `Encryption__Key`.
- `WHATSAPP_API_KEY` ahora es obligatoria en el `.env` del microservicio; la misma llave se captura en la pantalla de Configuración de la web.
- Después de definir `Encryption__Key`, volver a capturar la contraseña SMTP en Configuración.
- Migraciones nuevas:
  - `AddUniqueIndexToSentInvoiceLogs`: índice único en `SentInvoiceLogs`; si ya hay duplicados en la base, hay que limpiarlos antes de aplicarla.
  - `AddConciliacionCorteRowVersion`: usa la columna `xmin` de PostgreSQL, no genera SQL.
- La restauración de respaldos requiere que el usuario de PostgreSQL tenga permisos `CREATE`/`ALTER DATABASE`.
- Nueva política de contraseñas: mínimo 8 caracteres, con al menos una mayúscula, una minúscula y un número. Los mensajes de Identity están en español.
- Cerrar sesión ahora es por POST con token antiforgery (ya no sirve un enlace GET a `/account/logout`).
- `web.config` del microservicio de WhatsApp: `hiddenSegments` impide servir `auth_info`, `.env`, `node_modules`, logs de iisnode, etc.; errores detallados desactivados.
- Microservicio de WhatsApp: `node_modules` ya no está en git; en el servidor correr `npm ci` (usa `package-lock.json`). `dist/index.js` sí se versiona.
- Usuarios sin sucursal asignada que no son administradores ya no ven cajas de todas las sucursales; deben tener sucursal asignada.

## 3. Antes y durante el despliegue

- **Rotar la contraseña de PostgreSQL:** la anterior quedó en el historial de GitHub.
- Crear los archivos `.env` (web y microservicio de WhatsApp) en el servidor.
- Aplicar las migraciones pendientes.
- `npm ci` en `src/Blanquita.WhatsAppService`.

## 4. Validar en el ambiente real

- Restaurar un respaldo de la base.
- Editar la misma conciliación desde dos sesiones: la segunda debe recibir el aviso de conflicto de versión.
- Endpoint de productos por ID de la API Comercial con productos inactivos.
- Alertas de descuadre (correo y WhatsApp) encoladas y ejecutadas en Hangfire.
- Cerrar sesión desde el menú y por expiración de sesión.
- Que `/auth_info/creds.json` del microservicio responda 404.
- Buscar "second operation" en los logs de producción antes de decidir si se migra a `IDbContextFactory` en Blazor Server.
