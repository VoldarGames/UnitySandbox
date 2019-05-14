/// <summary>
/// Class that provides an enumeration of different shield locations
/// </summary>
public sealed class ShieldLocation
{
    public static readonly ShieldLocation Shape = new ShieldLocation("shape");
    public static readonly ShieldLocation Structure = new ShieldLocation("structure");
    public static readonly ShieldLocation Ornament = new ShieldLocation("ornament");
    public static readonly ShieldLocation Bottom = new ShieldLocation("bottom");

    private ShieldLocation(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }
}
