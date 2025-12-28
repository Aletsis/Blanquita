# DTOs de Búsqueda - Clean Architecture ✅ COMPLETADO

## 📋 Resumen Ejecutivo

Se han creado DTOs de búsqueda estandarizados para toda la aplicación, siguiendo los principios de **Clean Architecture** y **DDD**. Estos DTOs encapsulan la lógica de búsqueda, validación y paginación, facilitando el mantenimiento y la reutilización.

---

## ✅ DTOs Creados

### 1. **DTOs Base (2 archivos)**

#### 📄 `PagedSearchRequest.cs`
**Ubicación:** `src/Blanquita.Application/DTOs/PagedSearchRequest.cs`

DTO base para búsquedas paginadas con ordenamiento.

**Propiedades:**
- `Page` - Número de página (basado en 1)
- `PageSize` - Tamaño de página (1-100)
- `SortColumn` - Columna para ordenar
- `SortAscending` - Dirección del ordenamiento
- `SearchTerm` - Término de búsqueda general

**Métodos:**
- `Validate()` - Valida parámetros
- `GetSkip()` - Calcula elementos a saltar
- `HasSearchTerm()` - Verifica si hay término de búsqueda
- `HasSorting()` - Verifica si hay ordenamiento

**Ejemplo de uso:**
```csharp
var request = new PagedSearchRequest
{
    Page = 1,
    PageSize = 20,
    SortColumn = "Name",
    SortAscending = true,
    SearchTerm = "Juan"
};

request.Validate(); // Lanza excepción si hay errores
int skip = request.GetSkip(); // Retorna 0 para página 1
```

---

#### 📄 `DateRangeSearchRequest.cs`
**Ubicación:** `src/Blanquita.Application/DTOs/DateRangeSearchRequest.cs`

DTO base para búsquedas por rango de fechas.

**Propiedades:**
- `FechaInicio` - Fecha de inicio (opcional)
- `FechaFin` - Fecha de fin (opcional)

**Métodos:**
- `GetNormalizedDateRange()` - Normaliza fechas (incluye día completo)
- `Validate()` - Valida que el rango sea correcto
- `HasDateFilter()` - Verifica si hay filtro de fecha
- `GetDaysInRange()` - Obtiene días en el rango

**Ejemplo de uso:**
```csharp
var request = new DateRangeSearchRequest
{
    FechaInicio = new DateTime(2025, 1, 1),
    FechaFin = new DateTime(2025, 1, 31)
};

var (inicio, fin) = request.GetNormalizedDateRange();
// inicio: 2025-01-01 00:00:00
// fin: 2025-01-31 23:59:59.9999999

int dias = request.GetDaysInRange(); // Retorna 30
```

---

### 2. **DTOs Específicos (6 archivos)**

#### 📄 `SearchCashierRequest.cs`
**Hereda de:** `PagedSearchRequest`

Para búsqueda de cajeras con filtros específicos.

**Propiedades adicionales:**
- `BranchId` - ID de sucursal
- `IsActive` - Solo cajeras activas
- `EmployeeNumber` - Número de empleado

**Ejemplo de uso:**
```csharp
var request = new SearchCashierRequest
{
    Page = 1,
    PageSize = 10,
    BranchId = 5,
    IsActive = true,
    SearchTerm = "María",
    SortColumn = "EmployeeNumber",
    SortAscending = true
};

request.Validate();

// En el servicio
var result = await _cashierService.GetPagedAsync(request);
```

---

#### 📄 `SearchSupervisorRequest.cs`
**Hereda de:** `PagedSearchRequest`

Para búsqueda de supervisores.

**Propiedades adicionales:**
- `BranchId` - ID de sucursal
- `IsActive` - Solo supervisores activos

**Ejemplo de uso:**
```csharp
var request = new SearchSupervisorRequest
{
    Page = 1,
    PageSize = 10,
    BranchId = 3,
    IsActive = true,
    SearchTerm = "Carlos"
};
```

---

#### 📄 `SearchCashRegisterRequest.cs`
**Hereda de:** `PagedSearchRequest`

Para búsqueda de cajas registradoras.

**Propiedades adicionales:**
- `Sucursal` - Value Object Sucursal
- `IsActive` - Solo cajas activas
- `CashRegisterName` - Nombre de caja

**Ejemplo de uso:**
```csharp
var request = new SearchCashRegisterRequest
{
    Page = 1,
    PageSize = 20,
    Sucursal = Sucursal.Himno,
    IsActive = true,
    SearchTerm = "Caja"
};
```

---

#### 📄 `SearchCashCollectionRequest.cs`
**Hereda de:** `DateRangeSearchRequest`

Para búsqueda de recolecciones de efectivo.

**Propiedades adicionales:**
- `Sucursal` - Value Object Sucursal
- `CashRegisterName` - Nombre de caja
- `IsCut` - Estado de corte
- `Page` / `PageSize` - Paginación opcional

**Ejemplo de uso:**
```csharp
var request = new SearchCashCollectionRequest
{
    FechaInicio = DateTime.Today.AddDays(-7),
    FechaFin = DateTime.Today,
    Sucursal = Sucursal.Pozos,
    IsCut = false, // Solo recolecciones sin cortar
    Page = 1,
    PageSize = 50
};

request.Validate();
var (inicio, fin) = request.GetNormalizedDateRange();
```

---

#### 📄 `SearchCashCutRequest.cs`
**Hereda de:** `DateRangeSearchRequest`

Para búsqueda de cortes de caja (el más completo).

**Propiedades adicionales:**
- `Sucursal` - Value Object Sucursal
- `CashRegisterName` - Nombre de caja
- `CashierName` - Nombre de cajera
- `SupervisorName` - Nombre de supervisor
- `MinAmount` / `MaxAmount` - Rango de montos
- `Page` / `PageSize` - Paginación
- `SortColumn` / `SortAscending` - Ordenamiento

**Ejemplo de uso:**
```csharp
var request = new SearchCashCutRequest
{
    FechaInicio = DateTime.Today.AddMonths(-1),
    FechaFin = DateTime.Today,
    Sucursal = Sucursal.Saucito,
    CashierName = "Ana",
    MinAmount = 1000m,
    MaxAmount = 50000m,
    Page = 1,
    PageSize = 25,
    SortColumn = "CutDate",
    SortAscending = false
};

request.Validate();
var (min, max) = request.GetAmountRange();
```

---

#### 📄 `BuscarReportesRequest.cs`
**Ya existía** - Para búsqueda de reportes históricos.

**Propiedades:**
- `Sucursal` - Value Object Sucursal
- `FechaInicio` / `FechaFin` - Rango de fechas

---

### 3. **DTO de Resultado Mejorado**

#### 📄 `PagedResult<T>.cs` (Mejorado)
**Ubicación:** `src/Blanquita.Application/DTOs/PagedResult.cs`

Resultado paginado genérico con propiedades calculadas.

**Propiedades:**
- `Items` - Colección de elementos
- `TotalCount` - Total de elementos
- `PageNumber` - Número de página actual
- `PageSize` - Tamaño de página

**Propiedades calculadas:**
- `TotalPages` - Total de páginas
- `HasPreviousPage` - Hay página anterior
- `HasNextPage` - Hay página siguiente
- `IsFirstPage` / `IsLastPage` - Indicadores
- `FirstItemNumber` / `LastItemNumber` - Números de elementos
- `HasItems` / `IsEmpty` - Estado del resultado

**Métodos estáticos:**
- `Empty()` - Crea resultado vacío
- `Create()` - Crea resultado con datos

**Métodos de instancia:**
- `Map<TResult>()` - Transforma elementos

**Ejemplo de uso:**
```csharp
// Crear resultado
var result = PagedResult<CashierDto>.Create(
    items: cashiers,
    totalCount: 150,
    pageNumber: 1,
    pageSize: 10
);

// Usar propiedades calculadas
Console.WriteLine($"Página {result.PageNumber} de {result.TotalPages}");
Console.WriteLine($"Mostrando {result.FirstItemNumber}-{result.LastItemNumber} de {result.TotalCount}");

if (result.HasNextPage)
{
    // Mostrar botón "Siguiente"
}

// Mapear a otro tipo
var viewModels = result.Map(dto => new CashierViewModel(dto));

// Crear resultado vacío
var empty = PagedResult<CashierDto>.Empty(page: 1, pageSize: 10);
```

---

## 🏗️ Jerarquía de DTOs

```
PagedSearchRequest (Base para paginación)
├── SearchCashierRequest
├── SearchSupervisorRequest
└── SearchCashRegisterRequest

DateRangeSearchRequest (Base para fechas)
├── SearchCashCollectionRequest
├── SearchCashCutRequest
└── BuscarReportesRequest
```

---

## 📊 Comparación: Antes vs Después

### Antes ❌

```csharp
// Servicio con múltiples parámetros
Task<PagedResult<CashierDto>> GetPagedAsync(
    string? searchTerm, 
    int page, 
    int pageSize, 
    string? sortColumn, 
    bool sortAscending = true);

// Llamada con muchos parámetros
var result = await service.GetPagedAsync(
    "Juan",    // searchTerm
    1,         // page
    10,        // pageSize
    "Name",    // sortColumn
    true       // sortAscending
);

// Búsqueda por fechas sin normalización
var start = fechaInicio ?? DateTime.MinValue;
var end = fechaFin ?? DateTime.MaxValue;
if (end.TimeOfDay == TimeSpan.Zero)
    end = end.AddDays(1).AddTicks(-1);
```

### Después ✅

```csharp
// Servicio con DTO único
Task<PagedResult<CashierDto>> GetPagedAsync(
    SearchCashierRequest request,
    CancellationToken cancellationToken = default);

// Llamada con objeto expresivo
var request = new SearchCashierRequest
{
    SearchTerm = "Juan",
    Page = 1,
    PageSize = 10,
    SortColumn = "Name",
    SortAscending = true,
    BranchId = 5,
    IsActive = true
};

request.Validate(); // Validación centralizada
var result = await service.GetPagedAsync(request);

// Normalización encapsulada
var (inicio, fin) = request.GetNormalizedDateRange();
```

---

## ✅ Beneficios Obtenidos

### 1. **Encapsulación de Lógica**
- ✅ Validación centralizada en los DTOs
- ✅ Normalización de fechas encapsulada
- ✅ Cálculos de paginación en un solo lugar

### 2. **Type Safety**
- ✅ Uso de Value Objects (`Sucursal`)
- ✅ Propiedades fuertemente tipadas
- ✅ Menos errores en tiempo de ejecución

### 3. **Mantenibilidad**
- ✅ Fácil agregar nuevos filtros
- ✅ Cambios en un solo lugar
- ✅ Código más expresivo

### 4. **Testabilidad**
- ✅ DTOs fáciles de instanciar en tests
- ✅ Validaciones testeables
- ✅ Lógica aislada

### 5. **Reutilización**
- ✅ DTOs base reutilizables
- ✅ Herencia para especialización
- ✅ Métodos helper compartidos

---

## 🎯 Patrones Aplicados

### 1. **Builder Pattern (Implícito)**
```csharp
var request = new SearchCashCutRequest
{
    Sucursal = Sucursal.Himno,
    FechaInicio = DateTime.Today,
    Page = 1,
    PageSize = 10
};
```

### 2. **Template Method Pattern**
```csharp
// Clase base define estructura
public class DateRangeSearchRequest
{
    public virtual void Validate() { /* validación base */ }
}

// Clase derivada extiende
public class SearchCashCutRequest : DateRangeSearchRequest
{
    public new void Validate()
    {
        base.Validate(); // Llama validación base
        // Validación específica
    }
}
```

### 3. **Factory Pattern**
```csharp
var empty = PagedResult<T>.Empty(page: 1, pageSize: 10);
var result = PagedResult<T>.Create(items, totalCount, page, pageSize);
```

### 4. **Fluent Interface**
```csharp
if (request.HasSearchTerm() && request.HasBranchFilter())
{
    // Lógica de búsqueda
}
```

---

## 📝 Guía de Uso

### Escenario 1: Búsqueda Simple de Cajeras

```csharp
// En el componente Blazor
private async Task BuscarCajeras()
{
    var request = new SearchCashierRequest
    {
        SearchTerm = searchTerm,
        Page = currentPage,
        PageSize = 10,
        IsActive = true
    };

    try
    {
        request.Validate();
        var result = await _cashierService.GetPagedAsync(request);
        
        cajeras = result.Items.ToList();
        totalPages = result.TotalPages;
        
        Logger.LogInformation(
            "Búsqueda completada: {Count} cajeras encontradas", 
            result.TotalCount);
    }
    catch (ArgumentException ex)
    {
        Logger.LogWarning("Parámetros de búsqueda inválidos: {Message}", ex.Message);
        Snackbar.Add(ex.Message, Severity.Warning);
    }
}
```

### Escenario 2: Búsqueda Avanzada de Cortes

```csharp
private async Task BuscarCortes()
{
    var request = new SearchCashCutRequest
    {
        FechaInicio = fechaInicio,
        FechaFin = fechaFin,
        Sucursal = sucursalSeleccionada,
        CashierName = cajera,
        MinAmount = montoMinimo,
        MaxAmount = montoMaximo,
        Page = currentPage,
        PageSize = 25,
        SortColumn = "CutDate",
        SortAscending = false
    };

    try
    {
        request.Validate();
        
        Logger.LogInformation(
            "Buscando cortes - Sucursal: {Sucursal}, Rango: {Inicio} - {Fin}",
            request.Sucursal?.Nombre ?? "Todas",
            request.FechaInicio,
            request.FechaFin);

        var result = await _cashCutService.SearchAsync(request);
        
        cortes = result.Items.ToList();
        
        Snackbar.Add(
            $"Se encontraron {result.TotalCount} cortes ({result.FirstItemNumber}-{result.LastItemNumber})",
            Severity.Success);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error al buscar cortes");
        Snackbar.Add("Error al buscar cortes", Severity.Error);
    }
}
```

### Escenario 3: Búsqueda de Recolecciones por Fecha

```csharp
private async Task BuscarRecolecciones()
{
    var request = new SearchCashCollectionRequest
    {
        FechaInicio = DateTime.Today.AddDays(-7),
        FechaFin = DateTime.Today,
        Sucursal = Sucursal.Pozos,
        IsCut = false, // Solo sin cortar
        Page = 1,
        PageSize = 50
    };

    request.Validate();
    
    var (inicio, fin) = request.GetNormalizedDateRange();
    
    Logger.LogInformation(
        "Buscando recolecciones sin cortar del {Inicio:d} al {Fin:d}",
        inicio,
        fin);

    var recolecciones = await _cashCollectionService.SearchAsync(request);
    
    if (recolecciones.IsEmpty)
    {
        Snackbar.Add("No hay recolecciones pendientes", Severity.Info);
    }
}
```

---

## 🔄 Actualización de Interfaces de Servicios

### Antes ❌

```csharp
public interface ICashierService
{
    Task<PagedResult<CashierDto>> GetPagedAsync(
        string? searchTerm, 
        int page, 
        int pageSize, 
        string? sortColumn, 
        bool sortAscending = true);
}
```

### Después ✅

```csharp
public interface ICashierService
{
    Task<PagedResult<CashierDto>> GetPagedAsync(
        SearchCashierRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## 📚 Próximos Pasos Recomendados

### 🟡 MEDIO - Actualizar Servicios
- [ ] Actualizar `ICashierService` para usar `SearchCashierRequest`
- [ ] Actualizar `ISupervisorService` para usar `SearchSupervisorRequest`
- [ ] Actualizar `ICashRegisterService` para usar `SearchCashRegisterRequest`
- [ ] Actualizar `ICashCollectionService` para usar `SearchCashCollectionRequest`
- [ ] Actualizar `ICashCutService` para usar `SearchCashCutRequest`

### 🟢 BAJO - Mejoras Adicionales
- [ ] Agregar FluentValidation para validaciones más complejas
- [ ] Crear extensiones para IQueryable que usen los DTOs
- [ ] Agregar soporte para filtros dinámicos
- [ ] Crear DTOs para exportación (Excel, PDF)

---

## ✅ Verificación

### Compilación
```bash
dotnet build src/Blanquita.Application/Blanquita.Application.csproj
```
**Resultado:** ✅ Compilación exitosa

---

## 📁 Archivos Creados

### DTOs Base (2 archivos)
1. ✅ `PagedSearchRequest.cs` - Base para búsquedas paginadas
2. ✅ `DateRangeSearchRequest.cs` - Base para búsquedas por fecha

### DTOs Específicos (5 archivos)
3. ✅ `SearchCashierRequest.cs` - Búsqueda de cajeras
4. ✅ `SearchSupervisorRequest.cs` - Búsqueda de supervisores
5. ✅ `SearchCashRegisterRequest.cs` - Búsqueda de cajas
6. ✅ `SearchCashCollectionRequest.cs` - Búsqueda de recolecciones
7. ✅ `SearchCashCutRequest.cs` - Búsqueda de cortes

### DTOs Mejorados (1 archivo)
8. ✅ `PagedResult.cs` - Resultado paginado mejorado

**Total:** 8 archivos creados/mejorados

---

## 🎓 Conclusión

Se han creado DTOs de búsqueda estandarizados que:

✅ Encapsulan lógica de validación y normalización
✅ Siguen principios de Clean Architecture
✅ Son reutilizables mediante herencia
✅ Mejoran la mantenibilidad del código
✅ Facilitan el testing
✅ Hacen el código más expresivo
✅ Reducen errores en tiempo de ejecución

**Estado:** ✅ **COMPLETADO Y COMPILANDO**
