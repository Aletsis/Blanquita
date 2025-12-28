# Migración de Capa de Presentación - Estado Actual

## 📋 Estado: EN PROGRESO

---

## ✅ Completado

### 1. **Program.cs Actualizado**
- ✅ Configurado para usar Clean Architecture
- ✅ Integración con `AddInfrastructure()` de DI
- ✅ Serilog configurado
- ✅ MudBlazor agregado
- ✅ DotNetEnv para variables de entorno

### 2. **appsettings.json Migrado**
- ✅ ConnectionStrings
- ✅ Configuración de FoxPro
- ✅ Configuración de Serilog completa

### 3. **Paquetes NuGet Instalados**
- ✅ MudBlazor
- ✅ Serilog.AspNetCore
- ✅ DotNetEnv

### 4. **Archivos Copiados**
- ✅ `App.razor`
- ✅ `Routes.razor`
- ✅ `_Imports.razor`
- ✅ Todos los archivos de `Layout/`
- ✅ Todos los archivos de `Pages/`
- ✅ Todos los archivos de `wwwroot/`

---

## ⚠️ Errores de Compilación (292 errores)

### Causas Principales

1. **Referencias a Servicios Antiguos**
   - Los componentes usan interfaces del proyecto antiguo
   - Ejemplo: `ICajeraService`, `ICajaService`, `ICorteService`, etc.
   - Necesitan actualizarse a: `ICashierService`, `ICashRegisterService`, `ICashCutService`

2. **Referencias a Modelos Antiguos**
   - Los componentes usan modelos del proyecto antiguo
   - Ejemplo: `Cajeras`, `Cajas`, `Cortes`, `Recolecciones`
   - Necesitan actualizarse a DTOs: `CashierDto`, `CashRegisterDto`, `CashCutDto`, `CashCollectionDto`

3. **Namespaces Incorrectos**
   - `using Blanquita.Services;` → `using Blanquita.Application.Interfaces;`
   - `using Blanquita.Models;` → `using Blanquita.Application.DTOs;`
   - `using Blanquita.Interfaces;` → `using Blanquita.Application.Interfaces;`

4. **Servicios No Migrados**
   - Algunos servicios específicos de UI aún no están migrados
   - Ejemplo: `IConfigurationManager`, servicios específicos de páginas

---

## 🔄 Estrategia de Migración

### Opción 1: Migración Incremental (Recomendada)
1. Crear adaptadores/wrappers temporales para servicios antiguos
2. Migrar página por página
3. Actualizar referencias gradualmente
4. Eliminar código antiguo al final

### Opción 2: Migración Completa
1. Actualizar todos los using statements
2. Reemplazar todas las referencias a servicios
3. Actualizar todos los modelos a DTOs
4. Corregir todos los errores de una vez

---

## 📊 Componentes a Migrar

### Páginas Principales
- `Home.razor`
- `Login.razor`
- `Error.razor`
- `BarcodeScanner.razor`

### Páginas de Configuración
- `Configuraciones/Cajas.razor`
- `Configuraciones/Cajeras.razor`
- `Configuraciones/Encargadas.razor`
- `Configuraciones/Impresoras.razor`
- Otras páginas de configuración...

### Páginas de Cajas
- `Cajas/Corte.razor`
- `Cajas/Recoleccion.razor`

### Páginas de Reportes
- `Reportes/ReporteFacturacion.razor`
- Otros reportes...

### Componentes de Layout
- `MainLayout.razor`
- `NavMenu.razor`
- Diálogos varios...

---

## 🎯 Próximos Pasos Inmediatos

### 1. Actualizar _Imports.razor
Agregar los namespaces correctos:
```razor
@using Blanquita.Application.Interfaces
@using Blanquita.Application.DTOs
@using Blanquita.Domain.ValueObjects
```

### 2. Crear Servicios de UI Faltantes
Algunos servicios específicos de UI necesitan ser recreados:
- ConfigurationManager
- Servicios de navegación
- Servicios de estado

### 3. Actualizar Componentes Críticos
Comenzar con los componentes más importantes:
1. MainLayout
2. NavMenu
3. Home
4. Login

### 4. Migrar Servicios Específicos de Páginas
Cada página puede tener lógica específica que necesita adaptarse.

---

## 📝 Notas Importantes

### Cambios de Nomenclatura

| Antiguo | Nuevo |
|---------|-------|
| `Cajeras` | `CashierDto` |
| `Cajas` | `CashRegisterDto` |
| `Encargadas` | `SupervisorDto` |
| `Cortes` | `CashCutDto` |
| `Recolecciones` | `CashCollectionDto` |
| `ICajeraService` | `ICashierService` |
| `ICajaService` | `ICashRegisterService` |
| `IEncargadaService` | `ISupervisorService` |
| `ICorteService` | `ICashCutService` |
| `IRecoService` | `ICashCollectionService` |

### Servicios Disponibles

**CRUD Services:**
- ✅ `ICashierService`
- ✅ `ICashRegisterService`
- ✅ `ISupervisorService`
- ✅ `ICashCutService`
- ✅ `ICashCollectionService`

**External Services:**
- ✅ `IFoxProReportService`
- ✅ `IPrintingService`
- ✅ `IExportService`

---

## 🚀 Recomendación

Dado el número de errores (292), recomiendo:

1. **Crear un documento de mapeo** de servicios antiguos → nuevos
2. **Actualizar _Imports.razor** primero
3. **Migrar componentes de forma incremental**, empezando por los más simples
4. **Crear adaptadores temporales** si es necesario para mantener funcionalidad
5. **Probar cada componente** después de migrarlo

---

**Fecha:** 26 de diciembre de 2025
**Estado:** Archivos copiados, necesita actualización de referencias y servicios
