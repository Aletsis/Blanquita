# Refactorización de FoxProReportService - Clean Architecture + DDD

## Problema Identificado

El servicio `FoxProReportService` tenía lógica de dominio hardcodeada que violaba los principios de Clean Architecture y Domain-Driven Design:

```csharp
// ❌ ANTES: Lógica de dominio hardcodeada en Infrastructure
public BranchSeriesDto GetBranchSeries(string branchName)
{
    return branchName switch
    {
        "Himno" => new BranchSeriesDto { Cliente = "COH", Global = "FGIH", Devolucion = "DFCH" },
        "Pozos" => new BranchSeriesDto { Cliente = "COP", Global = "FGIP", Devolucion = "DFCP" },
        // ... más casos hardcodeados
        _ => new BranchSeriesDto()
    };
}
```

### Violaciones Detectadas:

1. **❌ Violación de SRP (Single Responsibility Principle)**
   - El servicio mezclaba responsabilidades de acceso a datos con lógica de negocio

2. **❌ Lógica de Dominio en Infrastructure**
   - Las reglas de negocio sobre series de documentos por sucursal estaban hardcodeadas en la capa de infraestructura

3. **❌ Falta de Encapsulación**
   - No había un concepto de dominio que representara las series de documentos

4. **❌ Difícil de Testear**
   - La lógica hardcodeada no podía ser testeada independientemente

## Solución Implementada

### 1. Creación de Value Object en el Dominio

Se creó `SeriesDocumentoSucursal` como un Value Object que encapsula la lógica de negocio:

```csharp
// ✅ DESPUÉS: Value Object en Domain Layer
public sealed class SeriesDocumentoSucursal : IEquatable<SeriesDocumentoSucursal>
{
    public string SerieCliente { get; }
    public string SerieGlobal { get; }
    public string SerieDevolucion { get; }

    public static SeriesDocumentoSucursal ObtenerPorSucursal(Sucursal sucursal)
    {
        if (sucursal == Sucursal.Himno)
            return new SeriesDocumentoSucursal("COH", "FGIH", "DFCH");
        
        if (sucursal == Sucursal.Pozos)
            return new SeriesDocumentoSucursal("COP", "FGIP", "DFCP");
        
        // ... lógica de dominio centralizada
        
        throw new InvalidOperationException($"No se encontraron series para: {sucursal.Nombre}");
    }

    public static SeriesDocumentoSucursal ObtenerPorNombre(string nombreSucursal)
    {
        var sucursal = Sucursal.FromNombre(nombreSucursal);
        if (sucursal == null)
            throw new ArgumentException($"Sucursal no encontrada: {nombreSucursal}");

        return ObtenerPorSucursal(sucursal);
    }
}
```

### 2. Refactorización del Servicio de Infrastructure

El servicio ahora delega la lógica de dominio al Value Object:

```csharp
// ✅ DESPUÉS: Infrastructure delega al Domain
public BranchSeriesDto GetBranchSeries(string branchName)
{
    try
    {
        var series = Domain.ValueObjects.SeriesDocumentoSucursal.ObtenerPorNombre(branchName);
        return new BranchSeriesDto
        {
            Cliente = series.SerieCliente,
            Global = series.SerieGlobal,
            Devolucion = series.SerieDevolucion
        };
    }
    catch (ArgumentException)
    {
        _logger.LogWarning("Branch series not found for branch: {BranchName}. Returning empty series.", branchName);
        return new BranchSeriesDto();
    }
}
```

### 3. Tests Unitarios para el Value Object

Se crearon 11 tests para validar el comportamiento del Value Object:

```csharp
[Fact]
public void ObtenerPorSucursal_ShouldReturnCorrectSeries_ForHimno()
{
    var series = SeriesDocumentoSucursal.ObtenerPorSucursal(Sucursal.Himno);

    Assert.Equal("COH", series.SerieCliente);
    Assert.Equal("FGIH", series.SerieGlobal);
    Assert.Equal("DFCH", series.SerieDevolucion);
}

[Fact]
public void ObtenerPorNombre_ShouldThrow_WhenNameNotFound()
{
    Assert.Throws<ArgumentException>(() => 
        SeriesDocumentoSucursal.ObtenerPorNombre("NonExistent"));
}
```

## Beneficios de la Refactorización

### ✅ Cumplimiento de Clean Architecture

1. **Separación de Responsabilidades**
   - Domain: Contiene la lógica de negocio (SeriesDocumentoSucursal)
   - Infrastructure: Solo se encarga de acceso a datos y mapeo

2. **Inversión de Dependencias**
   - Infrastructure depende de Domain (correcto)
   - Domain no depende de nada (correcto)

3. **Regla de Dependencia**
   - Las capas externas dependen de las internas
   - El flujo de dependencias apunta hacia el dominio

### ✅ Cumplimiento de DDD

1. **Value Object Bien Definido**
   - Inmutable (propiedades solo get)
   - Igualdad por valor (implementa IEquatable)
   - Sin identidad propia
   - Encapsula lógica de dominio

2. **Lenguaje Ubicuo**
   - `SeriesDocumentoSucursal` es un concepto del dominio
   - Los nombres reflejan el lenguaje del negocio

3. **Validación en el Dominio**
   - Las reglas de negocio están en el lugar correcto
   - Excepciones de dominio claras

### ✅ Mejoras Técnicas

1. **Testeable**
   - La lógica de dominio puede testearse independientemente
   - 11 tests unitarios añadidos

2. **Mantenible**
   - Cambios en las series solo requieren modificar el Value Object
   - Un solo lugar para la lógica de negocio

3. **Extensible**
   - Fácil añadir nuevas sucursales
   - Fácil añadir nuevos métodos de consulta

4. **Type-Safe**
   - Usa el Value Object `Sucursal` en lugar de strings
   - Reduce errores en tiempo de ejecución

## Estadísticas

- **Archivos Creados**: 2
  - `SeriesDocumentoSucursal.cs` (Domain)
  - `SeriesDocumentoSucursalTests.cs` (Tests)

- **Archivos Modificados**: 1
  - `FoxProReportService.cs` (Infrastructure)

- **Tests Añadidos**: 11
  - Total de tests del proyecto: **220 tests** ✅

- **Líneas de Código**:
  - Eliminadas: ~12 (lógica hardcodeada)
  - Añadidas: ~95 (Value Object + Tests)

## Conclusión

La refactorización ha movido exitosamente la lógica de negocio desde la capa de Infrastructure hacia la capa de Domain, respetando los principios de Clean Architecture y DDD. El código ahora es más mantenible, testeable y alineado con las mejores prácticas de diseño de software.
