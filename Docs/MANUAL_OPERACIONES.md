# 📋 Manual de Operaciones y Administración - Sistema Blanquita

Guía operativa y administrativa para el uso diario, control de caja, gestión comercial y mantenimiento del sistema Blanquita.

---

## 1. 👥 Roles de Usuario y Acceso

El sistema implementa control de acceso basado en roles (RBAC) mediante ASP.NET Core Identity:

| Rol | Permisos y Funciones Principales |
| :--- | :--- |
| **Administrador (`Admin`)** | Control total del sistema: gestión de usuarios y roles, configuración de sucursales e impresoras, diseño de etiquetas, dashboard de Hangfire, auditoría y respaldos. |
| **Supervisor (`Supervisor` / `Encargada`)** | Autorización y registro de recolecciones de valores, ejecución y cierre de cortes de caja, conciliaciones, consulta de reportes históricos y devoluciones. |
| **Cajero (`Cajero` / `Cajera`)** | Consulta de precios/productos, verificación de documentos y generación de tickets para clientes. |

---

## 2. 💰 Gestión de Efectivo y Cajas

### 2.1 Módulo de Recolecciones de Valores
Permite registrar retiros parciales de efectivo durante el turno para mitigar riesgos en piso de venta.

1. **Ruta**: `Cajas` -> `Recolecciones`.
2. **Procedimiento**:
   - Seleccionar la **Caja** y la **Cajera** en turno.
   - Indicar la **Encargada / Supervisora** que autoriza el retiro.
   - Capturar la cantidad de billetes por denominación ($1000, $500, $200, $100, $50, $20).
   - El sistema calcula automáticamente el total acumulado en tiempo real.
   - Hacer clic en **Guardar e Imprimir**: se generará el folio único y se emitirá el comprobante por la impresora térmica asignada a la caja.

### 2.2 Módulo de Cortes de Caja
1. **Ruta**: `Cajas` -> `Cortes`.
2. **Procedimiento**:
   - Seleccionar sucursal, caja, cajera y turno (Matutino / Vespertino).
   - El sistema totaliza automáticamente las recolecciones realizadas durante el periodo.
   - Capturar el conteo final de efectivo, vouchers de tarjeta y vales.
   - Se calcula la **diferencia** (Sobrante / Faltante).
   - Confirmar el corte para archivar el registro e imprimir el ticket resumen.

### 2.3 Conciliaciones de Corte
1. **Ruta**: `Cajas` -> `Conciliaciones`.
2. **Propósito**: Comparar los registros capturados en el sistema contra las ventas reportadas en los archivos DBF del sistema punto de venta.
3. Permite validar diferencias monetarias y autorizar ajustes contables con trazabilidad.

---

## 3. 🛍️ Operaciones Comerciales e Integración FoxPro

### 3.1 Consulta de Documentos y Ventas
- **Ruta**: `Comercial` -> `Documentos`.
- Permite buscar ventas, tickets y facturas directamente sobre los archivos DBF de FoxPro (`pos10041.dbf` / `pos10042.dbf`).
- **Filtros disponibles**: Rango de fechas, cliente, serie, folio o monto.
- Soporta cancelación de búsquedas pesadas en cualquier momento sin bloquear la interfaz.

### 3.2 Catálogo de Clientes y Productos
- **Clientes**: Búsqueda por RFC, código o nombre; consulta de saldo y condiciones crediticias.
- **Productos**: Verificación de existencias, listas de precios y consulta rápida para básculas.
- **Abarrotes y Pedidos**: Módulos para rastreo de pedidos especiales y productos complementarios.

### 3.3 Devoluciones de Mercancía
- **Ruta**: `Devoluciones`.
- Registro y consulta de notas de devolución vinculadas a documentos originales de venta.

---

## 4. 📱 Facturación y Envío por WhatsApp

### 4.1 Envío Automático de Facturas
1. Desde la pantalla de `Comercial` -> `Facturación` o búsqueda de documentos.
2. Localizar el documento timbrado y presionar el botón **Enviar WhatsApp**.
3. El sistema solicitará o precargará el número de teléfono móvil del cliente.
4. Al confirmar, el microservicio de WhatsApp generará y enviará el mensaje junto con el archivo PDF/XML adjunto.
5. El resultado de la entrega queda registrado en el historial de `SentInvoiceLogs`.

### 4.2 Verificación y Vinculación del Dispositivo WhatsApp
1. Acceder a `Configuraciones` -> `WhatsApp`.
2. Si el estado es `DISCONNECTED`:
   - Se mostrará el código QR en pantalla.
   - Abrir WhatsApp en el teléfono asignado a la carnicería -> **Dispositivos vinculados** -> **Vincular un dispositivo**.
   - Escanear el código QR.
   - El estado cambiará a `CONNECTED`.

---

## 5. 🏷️ Diseño e Impresión de Etiquetas

1. **Ruta**: `Configuraciones` -> `Diseño de Etiquetas`.
2. **Funcionalidades**:
   - Crear y editar plantillas de etiquetas para productos pesables y empaquetados.
   - Configurar elementos dinámicos: Nombre de producto, Código de barras (EAN-13 / Code128), Precio por kilo, Peso, Importe total, Fecha de caducidad e Información nutrimental.
   - Asignar la impresora térmica de destino por sucursal o caja.

---

## 6. 🛠️ Operaciones de Administración y TI

### 6.1 Monitoreo de Trabajos en Hangfire
- **URL**: `https://<servidor>/hangfire`
- **Requisito**: Haber iniciado sesión con una cuenta de rol `Admin`.
- Permite monitorear:
  - Trabajos encolados (*Enqueued*), en proceso (*Processing*) y fallidos (*Failed*).
  - Tareas recurrentes (*Recurring Jobs*) como sincronización de datos y limpieza de logs.
  - Reintentar manualmente tareas fallidas.

### 6.2 Parámetros del Sistema y Rutas FoxPro
- **Ruta**: `Configuraciones` -> `Parámetros`.
- Permite ajustar las rutas absolutas o de red compartida (UNC) hacia los archivos DBF:
  - Ruta POS Documentos (`pos10041.dbf`, `pos10042.dbf`)
  - Ruta MGW Catálogos (`mgw10008.dbf`, `mgw10005.dbf`)
- Toda modificación queda auditada en la tabla `SystemConfigurationAuditLogs`.

### 6.3 Respaldos de Base de Datos
- **Frecuencia Recomendada**: Diaria (automatizada) y previa a actualizaciones mayores.
- **Procedimiento Manual**:
  ```bash
  pg_dump -U postgres -d BlanquitaDB -F c -b -v -f "C:\Backups\BlanquitaDB_$(date +%Y%m%d_%H%M%S).backup"
  ```
- **Procedimiento de Restauración**:
  ```bash
  pg_restore -U postgres -d BlanquitaDB -v "C:\Backups\BlanquitaDB_archivo.backup"
  ```

### 6.4 Auditoría y Revisión de Logs
- Los logs operativos y de error se localizan en:
  - Eventos generales: `logs/blanquita-YYYYMMDD.log`
  - Errores críticos: `logs/errors/blanquita-errors-YYYYMMDD.log`
- Consulte estos archivos ante cualquier anomalía o reporte de usuario.
