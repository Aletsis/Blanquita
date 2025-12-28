# Refactorización de Historico.razor - Clean Architecture + DDD

## 📋 Análisis del Estado Actual

### ❌ Problemas Identificados

#### 1. **Autenticación Hardcodeada en la Capa de Presentación**

**Líneas 14-63, 199-242:**
```csharp
private bool logged = false;
private LoginModel model = new();

private async Task OnValidSubmit()
{
    if (model.Username == "Admin" && model.Password == "Blanquita123...")
    {
        logged = true;
    }
}

public class LoginModel
{
    [Required(ErrorMessage = "El usuario es requerido")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La contraseña es requerida")]
    public string Password { get; set; } = string.Empty;
}
```

**Problemas:**
- ❌ Credenciales hardcodeadas en el código fuente (GRAVE RIESGO DE SEGURIDAD)
- ❌ Lógica de autenticación en la capa de presentación
- ❌ No usa el sistema de autenticación de ASP.NET Core
- ❌ Contraseña en texto plano
- ❌ No hay gestión de sesiones
- ❌ Modelo `LoginModel` definido dentro del componente

---

#### 2. **Strings Mágicos para Sucursales**

**Líneas 76-84:**
```razor
<MudSelect @bind-Value="sucursalFiltro">
    <MudSelectItem Value="@("")">Todas</MudSelectItem>
    <MudSelectItem Value="@("Himno")">Himno</MudSelectItem>
    <MudSelectItem Value="@("Pozos")">Pozos</MudSelectItem>
    <MudSelectItem Value="@("Soledad")">Soledad</MudSelectItem>
    <MudSelectItem Value="@("Saucito")">Saucito</MudSelectItem>
    <MudSelectItem Value="@("Chapultepec")">Chapultepec</MudSelectItem>
</MudSelect>
```

**Problemas:**
- ❌ Sucursales hardcodeadas en la vista
- ❌ No hay un concepto de dominio para "Sucursal"
- ❌ Dificulta agregar/modificar sucursales
- ❌ Viola el principio DRY (se repite en múltiples páginas)

---

#### 3. **Servicio en la Capa Web en lugar de Application**

**Línea 6:**
```csharp
@inject IReporteService ReporteService
```

**Archivo:** `src/Blanquita.Web/Services/IReporteService.cs`

**Problemas:**
- ❌ El servicio está en la capa Web en lugar de Application
- ❌ Viola la Dependency Rule de Clean Architecture
- ❌ El modelo `Reporte` está en `Blanquita.Web.Models` en lugar de Domain

---

#### 4. **Modelo Anémico sin Lógica de Dominio**

**Archivo:** `src/Blanquita.Web/Models/Reporte.cs`
```csharp
public class Reporte
{
    public int Id { get; set; }
    public string Sucursal { get; set; } = "";
    public DateTime Fecha { get; set; }
    public decimal TotalSistema { get; set; }
    public decimal TotalCorteManual { get; set; }
    public decimal Diferencia { get; set; }
    public string Notas { get; set; } = "";
    public DateTime FechaGeneracion { get; set; }
    public List<ReportRow> Detalles { get; set; } = new();
}
```

**Problemas:**
- ❌ Modelo anémico (solo propiedades, sin comportamiento)
- ❌ `Sucursal` como string en lugar de Value Object
- ❌ `Diferencia` calculada pero no encapsulada
- ❌ No hay validaciones de negocio
- ❌ Está en la capa Web en lugar de Domain

---

#### 5. **Lógica de Presentación Mezclada con Lógica de Negocio**

**Líneas 292-297:**
```csharp
private Color ObtenerColorDiferencia(decimal diferencia)
{
    if (diferencia == 0) return Color.Success;
    if (diferencia > 0) return Color.Info;
    return Color.Warning;
}
```

**Problema:**
- ⚠️ Lógica de UI en el componente (aceptable, pero podría mejorarse)
- ⚠️ La lógica de "qué color usar" podría estar en un helper

---

#### 6. **Manejo de Errores Genérico**

**Líneas 252-256, 275-278:**
```csharp
catch (Exception ex)
{
    Snackbar.Add($"Error al cargar reportes: {ex.Message}", Severity.Error);
    reportesFiltrados = new List<Reporte>();
}
```

**Problemas:**
- ❌ No hay logging estructurado
- ❌ Captura genérica de `Exception`
- ❌ No se distinguen tipos de errores

---

#### 7. **Dependencia Directa de JSRuntime**

**Líneas 369-373:**
```csharp
private async Task DescargarArchivo(byte[] contenido, string nombreArchivo, string contentType)
{
    var base64 = Convert.ToBase64String(contenido);
    await JS.InvokeVoidAsync("fileDownloadHelper.downloadFile", nombreArchivo, contentType, base64);
}
```

**Problema:**
- ⚠️ Lógica de descarga de archivos en el componente
- ⚠️ Podría abstraerse en un servicio

---

## ✅ Soluciones Propuestas

### 1. **Implementar Autenticación Correcta**

#### a) Crear Value Object para Credenciales (Domain)
```csharp
// src/Blanquita.Domain/ValueObjects/Credenciales.cs
namespace Blanquita.Domain.ValueObjects;

public sealed class Credenciales
{
    public string Usuario { get; }
    public string PasswordHash { get; }

    private Credenciales(string usuario, string passwordHash)
    {
        Usuario = usuario;
        PasswordHash = passwordHash;
    }

    public static Credenciales Crear(string usuario, string password)
    {
        if (string.IsNullOrWhiteSpace(usuario))
            throw new ArgumentException("El usuario no puede estar vacío", nameof(usuario));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return new Credenciales(usuario, passwordHash);
    }

    public bool VerificarPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}
```

#### b) Crear Servicio de Autenticación (Application)
```csharp
// src/Blanquita.Application/Interfaces/IAutenticacionService.cs
namespace Blanquita.Application.Interfaces;

public interface IAutenticacionService
{
    Task<bool> AutenticarAsync(string usuario, string password);
    Task<bool> EstaAutenticadoAsync();
    Task CerrarSesionAsync();
}
```

#### c) Usar AuthenticationStateProvider de Blazor
```csharp
// src/Blanquita.Web/Authentication/CustomAuthStateProvider.cs
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    // Implementación correcta con claims
}
```

#### d) Proteger la página con [Authorize]
```razor
@page "/historico"
@attribute [Authorize]
```

---

### 2. **Crear Enum/Value Object para Sucursales**

#### a) Crear Enum en Domain
```csharp
// src/Blanquita.Domain/Enums/Sucursal.cs
namespace Blanquita.Domain.Enums;

public enum Sucursal
{
    Todas = 0,
    Himno = 1,
    Pozos = 2,
    Soledad = 3,
    Saucito = 4,
    Chapultepec = 5
}
```

#### b) O mejor aún, crear Value Object
```csharp
// src/Blanquita.Domain/ValueObjects/Sucursal.cs
namespace Blanquita.Domain.ValueObjects;

public sealed class Sucursal : IEquatable<Sucursal>
{
    public string Nombre { get; }
    public string Codigo { get; }

    private Sucursal(string codigo, string nombre)
    {
        Codigo = codigo;
        Nombre = nombre;
    }

    public static readonly Sucursal Himno = new("HIM", "Himno");
    public static readonly Sucursal Pozos = new("POZ", "Pozos");
    public static readonly Sucursal Soledad = new("SOL", "Soledad");
    public static readonly Sucursal Saucito = new("SAU", "Saucito");
    public static readonly Sucursal Chapultepec = new("CHA", "Chapultepec");

    public static IEnumerable<Sucursal> ObtenerTodas() => new[]
    {
        Himno, Pozos, Soledad, Saucito, Chapultepec
    };

    public bool Equals(Sucursal? other) => 
        other is not null && Codigo == other.Codigo;

    public override bool Equals(object? obj) => 
        Equals(obj as Sucursal);

    public override int GetHashCode() => Codigo.GetHashCode();
}
```

---

### 3. **Mover Servicio y Modelo a las Capas Correctas**

#### a) Crear Entidad de Dominio
```csharp
// src/Blanquita.Domain/Entities/ReporteHistorico.cs
namespace Blanquita.Domain.Entities;

public class ReporteHistorico
{
    public int Id { get; private set; }
    public Sucursal Sucursal { get; private set; }
    public DateTime Fecha { get; private set; }
    public decimal TotalSistema { get; private set; }
    public decimal TotalCorteManual { get; private set; }
    public string Notas { get; private set; }
    public DateTime FechaGeneracion { get; private set; }
    public IReadOnlyList<DetalleReporte> Detalles { get; private set; }

    // Propiedad calculada
    public decimal Diferencia => TotalCorteManual - TotalSistema;

    private ReporteHistorico() { } // Para EF Core

    public static ReporteHistorico Crear(
        Sucursal sucursal,
        DateTime fecha,
        decimal totalSistema,
        decimal totalCorteManual,
        List<DetalleReporte> detalles)
    {
        if (totalSistema < 0)
            throw new ArgumentException("El total del sistema no puede ser negativo");
        
        if (totalCorteManual < 0)
            throw new ArgumentException("El total del corte manual no puede ser negativo");

        return new ReporteHistorico
        {
            Sucursal = sucursal,
            Fecha = fecha,
            TotalSistema = totalSistema,
            TotalCorteManual = totalCorteManual,
            FechaGeneracion = DateTime.Now,
            Detalles = detalles,
            Notas = string.Empty
        };
    }

    public void ActualizarNotas(string notas)
    {
        if (notas == null)
            throw new ArgumentNullException(nameof(notas));
        
        Notas = notas;
    }

    public bool TieneDiferencia() => Diferencia != 0;
    
    public bool TieneSuperavit() => Diferencia > 0;
    
    public bool TieneDeficit() => Diferencia < 0;
}
```

#### b) Crear Interfaz en Application
```csharp
// src/Blanquita.Application/Interfaces/IReporteHistoricoService.cs
namespace Blanquita.Application.Interfaces;

public interface IReporteHistoricoService
{
    Task GuardarReporteAsync(ReporteHistorico reporte);
    Task<List<ReporteHistorico>> ObtenerReportesAsync();
    Task<ReporteHistorico?> ObtenerReportePorIdAsync(int id);
    Task EliminarReporteAsync(int id);
    Task<List<ReporteHistorico>> BuscarReportesAsync(
        Sucursal? sucursal = null, 
        DateTime? fechaInicio = null, 
        DateTime? fechaFin = null);
    Task ActualizarReporteAsync(ReporteHistorico reporte);
}
```

#### c) Crear DTO para Búsqueda
```csharp
// src/Blanquita.Application/DTOs/BuscarReportesRequest.cs
namespace Blanquita.Application.DTOs;

public sealed class BuscarReportesRequest
{
    public Sucursal? Sucursal { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }

    public (DateTime inicio, DateTime fin) GetNormalizedDateRange()
    {
        var inicio = FechaInicio ?? DateTime.MinValue;
        var fin = FechaFin ?? DateTime.MaxValue;
        
        // Si la fecha fin no tiene hora, incluir todo el día
        if (fin.TimeOfDay == TimeSpan.Zero)
            fin = fin.AddDays(1).AddTicks(-1);
        
        return (inicio, fin);
    }
}
```

---

### 4. **Agregar Logging Estructurado**

```csharp
@inject ILogger<Historico> Logger

private async Task CargarReportes()
{
    cargando = true;
    try
    {
        Logger.LogInformation("Iniciando carga de reportes históricos");
        var todosLosReportes = await ReporteService.ObtenerReportesAsync();
        reportesFiltrados = todosLosReportes;
        Logger.LogInformation("Se cargaron {Count} reportes exitosamente", todosLosReportes.Count);
    }
    catch (OperationCanceledException)
    {
        Logger.LogWarning("Carga de reportes cancelada por el usuario");
        Snackbar.Add("Carga cancelada", Severity.Warning);
        reportesFiltrados = new List<ReporteHistorico>();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error al cargar reportes históricos");
        Snackbar.Add($"Error al cargar reportes: {ex.Message}", Severity.Error);
        reportesFiltrados = new List<ReporteHistorico>();
    }
    finally
    {
        cargando = false;
    }
}
```

---

### 5. **Separar Lógica de Descarga de Archivos**

#### a) Crear Servicio
```csharp
// src/Blanquita.Application/Interfaces/IFileDownloadService.cs
namespace Blanquita.Application.Interfaces;

public interface IFileDownloadService
{
    Task DescargarArchivoAsync(byte[] contenido, string nombreArchivo, string contentType);
}
```

#### b) Implementación en Web
```csharp
// src/Blanquita.Web/Services/FileDownloadService.cs
public class FileDownloadService : IFileDownloadService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<FileDownloadService> _logger;

    public FileDownloadService(IJSRuntime js, ILogger<FileDownloadService> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async Task DescargarArchivoAsync(byte[] contenido, string nombreArchivo, string contentType)
    {
        try
        {
            _logger.LogInformation("Iniciando descarga de archivo: {FileName}", nombreArchivo);
            var base64 = Convert.ToBase64String(contenido);
            await _js.InvokeVoidAsync("fileDownloadHelper.downloadFile", nombreArchivo, contentType, base64);
            _logger.LogInformation("Descarga completada: {FileName}", nombreArchivo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar archivo: {FileName}", nombreArchivo);
            throw;
        }
    }
}
```

---

### 6. **Crear Helper para Colores de UI**

```csharp
// src/Blanquita.Web/Helpers/ReporteUIHelper.cs
namespace Blanquita.Web.Helpers;

public static class ReporteUIHelper
{
    public static Color ObtenerColorDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => Color.Success,
            > 0 => Color.Info,
            < 0 => Color.Warning
        };
    }

    public static string ObtenerIconoDiferencia(decimal diferencia)
    {
        return diferencia switch
        {
            0 => Icons.Material.Filled.CheckCircle,
            > 0 => Icons.Material.Filled.TrendingUp,
            < 0 => Icons.Material.Filled.TrendingDown
        };
    }
}
```

---

## 📁 Estructura de Archivos Propuesta

### Archivos a Crear

```
src/
├── Blanquita.Domain/
│   ├── Entities/
│   │   └── ReporteHistorico.cs ✨ NUEVO
│   ├── ValueObjects/
│   │   ├── Sucursal.cs ✨ NUEVO
│   │   └── Credenciales.cs ✨ NUEVO (si se implementa auth)
│   └── Enums/
│       └── Sucursal.cs ✨ ALTERNATIVA (si se prefiere enum)
│
├── Blanquita.Application/
│   ├── Interfaces/
│   │   ├── IReporteHistoricoService.cs ✨ NUEVO
│   │   ├── IAutenticacionService.cs ✨ NUEVO
│   │   └── IFileDownloadService.cs ✨ NUEVO
│   └── DTOs/
│       └── BuscarReportesRequest.cs ✨ NUEVO
│
└── Blanquita.Web/
    ├── Components/Pages/Reportes/
    │   └── Historico.razor 🔄 REFACTORIZAR
    ├── Helpers/
    │   └── ReporteUIHelper.cs ✨ NUEVO
    ├── Services/
    │   └── FileDownloadService.cs ✨ NUEVO
    └── Authentication/
        └── CustomAuthStateProvider.cs ✨ NUEVO (si se implementa auth)
```

### Archivos a Eliminar/Mover

```
❌ ELIMINAR:
src/Blanquita.Web/Services/IReporteService.cs
src/Blanquita.Web/Models/Reporte.cs

✅ MOVER A:
src/Blanquita.Application/Interfaces/IReporteHistoricoService.cs
src/Blanquita.Domain/Entities/ReporteHistorico.cs
```

---

## 🏗️ Principios de Clean Architecture a Aplicar

### ✅ Dependency Rule
- **Domain** no depende de nadie
- **Application** depende solo de Domain
- **Infrastructure** depende de Application y Domain
- **Web** depende de Application (no de Infrastructure directamente)

### ✅ Separation of Concerns
- **Domain**: Entidades, Value Objects, Enums
- **Application**: Interfaces de servicios, DTOs, Use Cases
- **Infrastructure**: Implementación de repositorios
- **Web**: Componentes Razor, solo lógica de presentación

### ✅ Single Responsibility Principle
- Cada clase/método tiene una única responsabilidad
- Autenticación separada de reportes
- Descarga de archivos en servicio dedicado

### ✅ Don't Repeat Yourself (DRY)
- Sucursales definidas una sola vez
- Lógica de normalización de fechas en DTO
- Helpers para lógica de UI reutilizable

---

## 🎯 Principios de DDD a Aplicar

### ✅ Ubiquitous Language
- `ReporteHistorico` en lugar de `Reporte`
- `Sucursal` como concepto de dominio
- `Diferencia` como propiedad calculada

### ✅ Value Objects
- `Sucursal` como Value Object inmutable
- `Credenciales` para autenticación

### ✅ Rich Domain Model
- Métodos de negocio en la entidad: `TieneDiferencia()`, `TieneSuperavit()`
- Validaciones en el constructor
- Encapsulación de lógica

### ✅ Domain Primitives
- Reemplazo de strings por tipos del dominio
- Uso de Value Objects en lugar de primitivos

---

## 📊 Prioridades de Refactorización

### 🔴 CRÍTICO (Seguridad)
1. **Eliminar credenciales hardcodeadas**
   - Implementar autenticación correcta con ASP.NET Core Identity
   - O usar `[Authorize]` con el sistema existente

### 🟠 ALTO (Arquitectura)
2. **Mover servicio y modelo a capas correctas**
   - Crear `ReporteHistorico` en Domain
   - Crear `IReporteHistoricoService` en Application
   - Eliminar archivos de Web/Services y Web/Models

3. **Crear concepto de Sucursal**
   - Enum o Value Object en Domain
   - Usar en toda la aplicación

### 🟡 MEDIO (Calidad)
4. **Agregar logging estructurado**
   - Inyectar `ILogger<Historico>`
   - Logging en todos los métodos importantes

5. **Crear DTOs para búsquedas**
   - `BuscarReportesRequest` con lógica de normalización

### 🟢 BAJO (Mejoras)
6. **Separar lógica de descarga**
   - Crear `IFileDownloadService`

7. **Crear helpers de UI**
   - `ReporteUIHelper` para colores e iconos

---

## ✅ Checklist de Refactorización

### Fase 1: Seguridad (CRÍTICO)
- [ ] Eliminar credenciales hardcodeadas
- [ ] Implementar `[Authorize]` en la página
- [ ] O crear sistema de autenticación correcto

### Fase 2: Clean Architecture (ALTO)
- [ ] Crear `ReporteHistorico` en Domain
- [ ] Crear Value Object `Sucursal`
- [ ] Crear `IReporteHistoricoService` en Application
- [ ] Crear `BuscarReportesRequest` DTO
- [ ] Mover implementación a Infrastructure
- [ ] Actualizar inyección de dependencias

### Fase 3: Mejoras de Código (MEDIO)
- [ ] Agregar `ILogger<Historico>`
- [ ] Implementar logging estructurado
- [ ] Manejo específico de excepciones
- [ ] Crear `IFileDownloadService`

### Fase 4: Pulido (BAJO)
- [ ] Crear `ReporteUIHelper`
- [ ] Documentar código
- [ ] Agregar pruebas unitarias

---

## 🎓 Conclusión

El archivo `Historico.razor` presenta varios problemas arquitectónicos y de seguridad que deben ser abordados:

1. **CRÍTICO**: Credenciales hardcodeadas (riesgo de seguridad)
2. **ALTO**: Violación de Clean Architecture (servicios y modelos en capa Web)
3. **MEDIO**: Falta de logging y manejo robusto de errores
4. **BAJO**: Código que podría ser más limpio y mantenible

La refactorización propuesta seguirá los mismos principios aplicados exitosamente en `Reimpresiones.razor`, asegurando:
- ✅ Seguridad mejorada
- ✅ Arquitectura limpia y mantenible
- ✅ Código testeable
- ✅ Separación clara de responsabilidades
- ✅ Uso correcto de DDD y Clean Architecture
