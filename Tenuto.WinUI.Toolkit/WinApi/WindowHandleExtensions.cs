using System;
using Windows.Graphics;

namespace Tenuto.WinUI.Toolkit.WinApi;

internal static class WindowHandleExtensions
{
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



    public static void CenterWindowOnScreen(this IntPtr hWnd, SizeInt32 size)
    {
        var info = Interop.GetMonitorInfoOrNearest(hWnd);

        var centerX = (info.rcMonitor.left + info.rcMonitor.right) / 2;
        var centerY = (info.rcMonitor.bottom + info.rcMonitor.top) / 2;
        var left = centerX - size.Width / 2;
        var top = centerY - size.Height / 2;

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static void SetWindowPosition(this IntPtr hWnd, double xPos, double yPos)
    {
        (var x, var y) = hWnd.ToPosPixels(xPos, yPos);

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            x, y, 0, 0,
                            Interop.SWP_NOSIZE | Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static double ScaleToPix(this IntPtr hwnd) => Interop.GetDpiForWindow(hwnd) / 96d;

    public static double ScaleToDip(this IntPtr hwnd) => 96d / Interop.GetDpiForWindow(hwnd);

    public static void BottomRighAlignWindowOnScreen(this IntPtr hwnd, SizeInt32 size)
    {
        var info = Interop.GetMonitorInfoOrNearest(hwnd);
     
        var left = info.rcWork.right - size.Width;
        if (left < 0)
            left = 0;
        var top = info.rcWork.bottom - size.Height;
        if (top < 0)
            top = 0;

        var success = Interop.SetWindowPos(hwnd, Interop.HWND_TOP,
                            left, top, size.Width, size.Height,
                            Interop.SWP_NOZORDER | Interop.SWP_NOACTIVATE);
    }


    public static SizeInt32 ToPixelSizeInt32(this IntPtr hwnd, double dipWidth, double dipHeight)
    {
        var dpi = Interop.GetDpiForWindow(hwnd);
        var scalingFactor = dpi / 96d;
        return new SizeInt32((int)(dipWidth * scalingFactor), (int)(dipHeight * scalingFactor));
    }

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
