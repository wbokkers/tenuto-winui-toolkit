using Microsoft.UI.Windowing;
using System;
using System.Diagnostics;
using System.Text.Json;
using Tenuto.WinUI.Toolkit.SettingsStorage;
using Windows.Graphics;

namespace Tenuto.WinUI.Toolkit.WinApi;

internal static class WindowHandleExtensions
{
    private static readonly Random _Rnd = new();

    public static void CenterOnMonitor(this IntPtr hWnd, SizeInt32 size, Interop.MONITORINFOEX monitor, bool useRandomDisplacement = false, double minLeftDip = 0)
    {
        var rect = monitor.rcWork; // work area

        var minLeftPix = minLeftDip != 0
            ? (int)Math.Round(minLeftDip * hWnd.ScaleToPix())
            : 0;

        var centerX = (rect.left + rect.right) / 2;
        var centerY = (rect.bottom + rect.top) / 2;

        if (useRandomDisplacement)
        {
            var rndx = Math.Min(100, size.Width / 5);
            var rndy = Math.Min(100, size.Height / 5);
            centerX += _Rnd.Next(-rndx, rndx);
            centerY += _Rnd.Next(-rndy, rndy);
        }

        var left = Math.Max(rect.left + minLeftPix, centerX - (size.Width / 2));
        var top = Math.Max(rect.top, centerY - (size.Height / 2));

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static void CenterWindowInRect(this IntPtr hWnd, SizeInt32 size, Interop.RECT rect)
    {
        var centerX = (rect.left + rect.right) / 2;
        var centerY = (rect.bottom + rect.top) / 2;

        var left = Math.Max(0, centerX - size.Width / 2); // do not allow to go off screen
        var top = Math.Max(0, centerY - size.Height / 2);

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static void CenterOnRect(this IntPtr hWnd, SizeInt32 size, Interop.RECT rect)
    {
        var centerX = (rect.left + rect.right) / 2;
        var centerY = (rect.bottom + rect.top) / 2;
        var left = Math.Max(rect.left, centerX - (size.Width / 2)); // do not allow to go off screen
        var top = Math.Max(rect.top, centerY - (size.Height / 2));

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_SHOWWINDOW | Interop.SWP_NOZORDER);
    }



    /// <summary>
    /// Align to bottom-center based on screen pixels
    /// </summary>
    public static void PlaceBottomCenterOnMonitor(this IntPtr hwnd, SizeInt32 size, Interop.MONITORINFOEX monitor)
    {
        var workRect = monitor.rcWork;

        var top = workRect.bottom - size.Height;
        if (top < 0)
            top = 0;

        var centerX = (workRect.left + workRect.right) / 2;
        var left = centerX - (size.Width / 2);

        _ = Interop.SetWindowPos(hwnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOZORDER | Interop.SWP_NOACTIVATE);
    }

    /// <summary>
    /// Align to bottom-right based on size in effective pixels
    /// </summary>
    public static void PlaceBottomRighOnMonitor(this IntPtr hWnd, double width, double height, Interop.MONITORINFOEX monitor)
    {
        var size = hWnd.ToSizePixels(width, height);
        PlaceRightOnMonitor(hWnd, size, monitor);
    }

    public static void PlaceLeftOnMonitor(this IntPtr hwnd, double width, Interop.MONITORINFOEX monitor, double marginTop = 6, double marginBottom = 0, double marginLeft = 0)
    {
        var scaleToPix = ScaleToPix(hwnd);

        var marginTopPix = DipToPixels(marginTop, scaleToPix);
        var marginBottomPix = DipToPixels(marginBottom, scaleToPix);
        var marginLeftPix = DipToPixels(marginLeft, scaleToPix);
        var widthPix = DipToPixels(width, scaleToPix);

        var workRect = monitor.rcWork;

        var left = marginLeftPix + workRect.left;
        var top = marginTopPix + workRect.top;

        var height = (workRect.bottom - workRect.top) - (marginTopPix + marginBottomPix);

        _ = Interop.SetWindowPos(hwnd, Interop.HWND_TOP,
                            left, top, widthPix, height,
                            Interop.SWP_NOZORDER | Interop.SWP_NOACTIVATE);
    }

    /// <summary>
    /// Align to right based on screen pixels
    /// </summary>
    public static void PlaceRightOnMonitor(this IntPtr hwnd, SizeInt32 size, Interop.MONITORINFOEX monitor)
    {
        var left = monitor.rcWork.right - size.Width;
        if (left < 0)
            left = 0;
        var top = monitor.rcWork.bottom - size.Height;
        if (top < 0)
            top = 0;

        _ = Interop.SetWindowPos(hwnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOZORDER | Interop.SWP_NOACTIVATE);
    }

    public static bool RestoreSizeAndPosition(this IntPtr hWnd, string restoreSettingKey)
    {
        var pJson = LocalSettingsUtil.GetStringValue(null, restoreSettingKey);
        if (pJson == null)
            return false;

        if (JsonSerializer.Deserialize(pJson, typeof(DipWindowPlacement)) is DipWindowPlacement p)
        {
            // Convert from DPI to pixels
            var dpi = Interop.GetDpiForWindow(hWnd);

            (var x, var y, var w, var h) = p.ToPix(dpi);

            // Only restore when a monitor can display the rectangle
            if (Interop.IsRectangleOnAMonitor(x, y, w, h))
            {
                Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                                    x, y, w, h,
                                    Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public static void SaveSizeAndPosition(this IntPtr hWnd, AppWindow window, string? restoreSettingKey)
    {
        try
        {
            if (restoreSettingKey == null)
                return;

            // Do not save the position if the window is minimized or maximized
            if (Interop.IsIconic(hWnd) || Interop.IsZoomed(hWnd))
                return;

            // Convert window placement to DIP placement
            var dpi = Interop.GetDpiForWindow(hWnd);
            var pos = window.Position;
            var size = window.Size;
            var p = DipWindowPlacement.FromPix(dpi, pos.X, pos.Y, size.Width, size.Height);

            var pJson = JsonSerializer.Serialize(p, typeof(DipWindowPlacement));
            LocalSettingsUtil.SetStringValue(pJson, restoreSettingKey);
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error in SaveSizeAndPosition");
            Debug.WriteLine(ex.Message);
        }
    }

    public static double ScaleToDip(this IntPtr hwnd) => 96d / Interop.GetDpiForWindow(hwnd);

    public static double ScaleToPix(this IntPtr hwnd) => Interop.GetDpiForWindow(hwnd) / 96d;

    public static void SetWindowPosition(this IntPtr hWnd, double xPos, double yPos)
    {
        (var x, var y) = hWnd.ToPosPixels(xPos, yPos);

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            x, y, 0, 0,
                            Interop.SWP_NOSIZE | Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static void ShowWindowWithoutFocus(this IntPtr hWnd, bool bringToTop)
    {
        if (hWnd == IntPtr.Zero)
            return;

        // Show the window if not visible
        if (Interop.IsIconic(hWnd) || !Interop.IsWindowVisible(hWnd))
        {
            Interop.ShowWindow(hWnd, Interop.SW_SHOWNOACTIVATE);
        }

        // Bring the window to the top without making it active
        if (bringToTop)
        {
            Interop.SetWindowPos(hWnd, Interop.HWND_TOP, 0, 0, 0, 0, Interop.SWP_NOMOVE | Interop.SWP_NOSIZE | Interop.SWP_SHOWWINDOW | Interop.SWP_NOACTIVATE);
        }
    }

    public static SizeInt32 ToPixelSizeInt32(this IntPtr hwnd, double dipWidth, double dipHeight)
    {
        var dpi = Interop.GetDpiForWindow(hwnd);
        var scalingFactor = dpi / 96d;
        return new SizeInt32((int)(dipWidth * scalingFactor), (int)(dipHeight * scalingFactor));
    }

    public static SizeInt32 ToSizePixels(this IntPtr hwnd, double dipWidth, double dipHeight)
    {
        var dpi = Interop.GetDpiForWindow(hwnd);
        var scalingFactor = dpi / 96d;
        return new SizeInt32((int)(dipWidth * scalingFactor) + 1, (int)(dipHeight * scalingFactor) + 1);
    }

    private static int DipToPixels(double dip, double scaleToPix) => (int)Math.Round(dip * scaleToPix);

    /// <summary>
    /// Convert the specified Device Independent size to a size in pixels
    /// </summary>
    private static (int x, int y) ToPosPixels(this IntPtr hwnd, double dipX, double dipY)
    {
        var dpi = Interop.GetDpiForWindow(hwnd);
        var scalingFactor = dpi / 96d;
        return ((int)(dipX * scalingFactor), (int)(dipY * scalingFactor));
    }
}