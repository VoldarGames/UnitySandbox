using System.Collections.Generic;
/// <summary>
/// Class that provides an enumeration of different shield locations
/// </summary>
public sealed class ShieldLocation
{
    public static readonly ShieldLocation Banner = new ShieldLocation("banner");
    public static readonly ShieldLocation Frame = new ShieldLocation("frame");
    public static readonly ShieldLocation FrameSecond = new ShieldLocation("framesecond");
    public static readonly ShieldLocation FrameThird = new ShieldLocation("framethird");
    public static readonly ShieldLocation Shape = new ShieldLocation("shape");
    public static readonly ShieldLocation Symbol = new ShieldLocation("symbol");
    public static readonly ShieldLocation Top = new ShieldLocation("top");
    public static readonly ShieldLocation Wings = new ShieldLocation("wings");

    public static readonly List<ShieldLocation> AllShieldLocations = new List<ShieldLocation>
    {
        Banner,
        Frame,
        FrameSecond,
        FrameThird,
        Shape,
        Symbol,
        Top,
        Wings
    };

    private ShieldLocation(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }
}
