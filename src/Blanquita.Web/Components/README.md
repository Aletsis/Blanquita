# Estructura de Componentes

Esta carpeta contiene todos los componentes de la aplicación Blazor, organizados de manera lógica para facilitar el mantenimiento y la escalabilidad.

## 📁 Estructura de Carpetas

### `/Dialogs`
Contiene todos los componentes de diálogo (modales) de la aplicación:
- `DialogoConfirmarGuardado.razor` - Diálogo de confirmación para guardar cambios
- `DialogoDetalleCorte.razor` - Muestra detalles de un corte de caja
- `DialogoDetalleRecoleccion.razor` - Muestra detalles de una recolección
- `DialogoDetalleReporte.razor` - Muestra detalles de un reporte
- `DialogoEditarNotas.razor` - Permite editar notas
- `DialogoImpresora.razor` - Configuración de impresoras
- `DialogoSeleccionArchivo.razor` - Selector de archivos
- `PrintDialog.razor` - Diálogo de impresión
- `ProductDialog.razor` - Diálogo para productos

### `/Layout`
Contiene los componentes de diseño y navegación:
- `MainLayout.razor` - Layout principal de la aplicación
- `MainLayout.razor.css` - Estilos del layout principal
- `NavMenu.razor` - Menú de navegación
- `NavMenu.razor.css` - Estilos del menú de navegación
- `RedirectToLogin.razor` - Componente de redirección al login

### `/Shared`
Contiene componentes compartidos y reutilizables:
- `ComponenteDiagnostico.razor` - Componente para diagnósticos del sistema

### `/Pages`
Contiene las páginas de la aplicación organizadas por módulos:
- `/Abarrotes` - Páginas relacionadas con el módulo de abarrotes
- `/Configuraciones` - Páginas de configuración del sistema
- `/Reportes` - Páginas de reportes y consultas

## 🔧 Uso de Componentes

Todos los componentes están disponibles globalmente gracias a las directivas `@using` en `_Imports.razor`:

```razor
@using Blanquita.Web.Components.Layout
@using Blanquita.Web.Components.Dialogs
@using Blanquita.Web.Components.Shared
```

Esto significa que puedes usar cualquier componente directamente sin necesidad de importarlo en cada página.

## 📝 Convenciones

- **Diálogos**: Todos los componentes de diálogo deben comenzar con "Dialogo" o terminar con "Dialog"
- **Layouts**: Componentes de diseño estructural de la aplicación
- **Shared**: Componentes reutilizables que no son diálogos ni layouts
- **Pages**: Páginas completas organizadas por módulo funcional

## 🚀 Mejores Prácticas

1. **Separación de responsabilidades**: Cada componente debe tener una única responsabilidad
2. **Reutilización**: Antes de crear un nuevo componente, verifica si existe uno similar en `/Shared`
3. **Nomenclatura**: Usa nombres descriptivos en español para mantener la consistencia
4. **Organización**: Coloca nuevos componentes en la carpeta apropiada según su función
