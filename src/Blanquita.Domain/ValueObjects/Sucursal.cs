namespace Blanquita.Domain.ValueObjects;

/// <summary>
/// Value Object que representa una sucursal del negocio.
/// Encapsula el concepto de dominio de "Sucursal" con todas sus sucursales disponibles.
/// </summary>
public sealed class Sucursal : IEquatable<Sucursal>
{
    public string Codigo { get; }
    public string Nombre { get; }

    private Sucursal(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código de sucursal no puede estar vacío", nameof(codigo));
        
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de sucursal no puede estar vacío", nameof(nombre));

        Codigo = codigo;
        Nombre = nombre;
    }

    // Sucursales predefinidas
    public static readonly Sucursal Himno = new("HIM", "Himno");
    public static readonly Sucursal Pozos = new("POZ", "Pozos");
    public static readonly Sucursal Soledad = new("SOL", "Soledad");
    public static readonly Sucursal Saucito = new("SAU", "Saucito");
    public static readonly Sucursal Chapultepec = new("CHA", "Chapultepec");

    /// <summary>
    /// Obtiene todas las sucursales disponibles en el sistema.
    /// </summary>
    public static IReadOnlyList<Sucursal> ObtenerTodas() => new[]
    {
        Himno, Pozos, Soledad, Saucito, Chapultepec
    };

    /// <summary>
    /// Crea una instancia de Sucursal a partir de su nombre.
    /// </summary>
    public static Sucursal? FromNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return null;

        return ObtenerTodas().FirstOrDefault(s => 
            s.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Crea una instancia de Sucursal con código y nombre personalizados.
    /// </summary>
    public static Sucursal Create(string codigo, string nombre)
    {
        return new Sucursal(codigo, nombre);
    }

    /// <summary>
    /// Crea una instancia de Sucursal a partir de su código.
    /// </summary>
    public static Sucursal? FromCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return null;

        return ObtenerTodas().FirstOrDefault(s => 
            s.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
    }

    public bool Equals(Sucursal? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Codigo == other.Codigo;
    }

    public override bool Equals(object? obj) => Equals(obj as Sucursal);

    public override int GetHashCode() => Codigo.GetHashCode();

    public override string ToString() => Nombre;

    public static bool operator ==(Sucursal? left, Sucursal? right) => 
        Equals(left, right);

    public static bool operator !=(Sucursal? left, Sucursal? right) => 
        !Equals(left, right);
}
