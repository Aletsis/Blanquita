namespace Blanquita.Domain.ValueObjects;

/// <summary>
/// Value Object que representa las series de documentos FoxPro asociadas a una sucursal.
/// Encapsula la lógica de negocio de qué series se usan para cada tipo de documento en cada sucursal.
/// </summary>
public sealed class SeriesDocumentoSucursal : IEquatable<SeriesDocumentoSucursal>
{
    public string SerieCliente { get; }
    public string SerieGlobal { get; }
    public string SerieDevolucion { get; }

    private SeriesDocumentoSucursal(string serieCliente, string serieGlobal, string serieDevolucion)
    {
        if (string.IsNullOrWhiteSpace(serieCliente))
            throw new ArgumentException("La serie de cliente no puede estar vacía", nameof(serieCliente));
        
        if (string.IsNullOrWhiteSpace(serieGlobal))
            throw new ArgumentException("La serie global no puede estar vacía", nameof(serieGlobal));
        
        if (string.IsNullOrWhiteSpace(serieDevolucion))
            throw new ArgumentException("La serie de devolución no puede estar vacía", nameof(serieDevolucion));

        SerieCliente = serieCliente;
        SerieGlobal = serieGlobal;
        SerieDevolucion = serieDevolucion;
    }

    /// <summary>
    /// Obtiene las series de documentos para una sucursal específica.
    /// </summary>
    public static SeriesDocumentoSucursal ObtenerPorSucursal(Sucursal sucursal)
    {
        if (sucursal == null)
            throw new ArgumentNullException(nameof(sucursal));

        if (sucursal == Sucursal.Himno)
            return new SeriesDocumentoSucursal("COH", "FGIH", "DFCH");
        
        if (sucursal == Sucursal.Pozos)
            return new SeriesDocumentoSucursal("COP", "FGIP", "DFCP");
        
        if (sucursal == Sucursal.Soledad)
            return new SeriesDocumentoSucursal("COS", "FGIS", "DFCS");
        
        if (sucursal == Sucursal.Saucito)
            return new SeriesDocumentoSucursal("COFS", "FGIFS", "DFCFS");
        
        if (sucursal == Sucursal.Chapultepec)
            return new SeriesDocumentoSucursal("COX", "FXIS", "DFCX");

        throw new InvalidOperationException($"No se encontraron series de documentos para la sucursal: {sucursal.Nombre}");
    }

    /// <summary>
    /// Obtiene las series de documentos por nombre de sucursal (para compatibilidad con código legacy).
    /// </summary>
    public static SeriesDocumentoSucursal ObtenerPorNombre(string nombreSucursal)
    {
        var sucursal = Sucursal.FromNombre(nombreSucursal);
        if (sucursal == null)
            throw new ArgumentException($"Sucursal no encontrada: {nombreSucursal}", nameof(nombreSucursal));

        return ObtenerPorSucursal(sucursal);
    }

    public bool Equals(SeriesDocumentoSucursal? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return SerieCliente == other.SerieCliente &&
               SerieGlobal == other.SerieGlobal &&
               SerieDevolucion == other.SerieDevolucion;
    }

    public override bool Equals(object? obj) => Equals(obj as SeriesDocumentoSucursal);

    public override int GetHashCode() => HashCode.Combine(SerieCliente, SerieGlobal, SerieDevolucion);

    public override string ToString() => $"Cliente: {SerieCliente}, Global: {SerieGlobal}, Devolución: {SerieDevolucion}";

    public static bool operator ==(SeriesDocumentoSucursal? left, SeriesDocumentoSucursal? right) =>
        Equals(left, right);

    public static bool operator !=(SeriesDocumentoSucursal? left, SeriesDocumentoSucursal? right) =>
        !Equals(left, right);
}
