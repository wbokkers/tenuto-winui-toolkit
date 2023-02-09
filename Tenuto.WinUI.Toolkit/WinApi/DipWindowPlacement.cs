namespace Tenuto.WinUI.Toolkit.WinApi;

/// <summary>
/// Placement in Device Independent Pixels.
/// Used to serialize a window placement in DIP. 
/// </summary>
internal struct DipWindowPlacement
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    /// <summary>
    /// Get the window placement in pixels
    /// </summary>
    public (int x, int y, int w, int h) ToPix(int dpi)
    {
        var scaleFactor = dpi / 96.0;

        return ((int)(X * scaleFactor),
                (int)(Y * scaleFactor),
                (int)(Width * scaleFactor),
                (int)(Height * scaleFactor));
    }

    /// <summary>
    /// Create a window placement from pixels
    /// </summary>
    public static DipWindowPlacement FromPix(int dpi, int x, int y, int w, int h)
    {
        var scaleFactor = 96.0 / dpi;
        return new DipWindowPlacement
        {
            X = x * scaleFactor,
            Y = y * scaleFactor,
            Width = w * scaleFactor,
            Height = h * scaleFactor
        };
    }
}
