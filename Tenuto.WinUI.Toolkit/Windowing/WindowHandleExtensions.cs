using System;
namespace Tenuto.WinUI.Toolkit.Windowing;

public static class WindowHandleExtensions
{
    public static void CenterWindowInRect(this IntPtr hWnd, double width, double height, Interop.RECT rect)
    {
        var dpi = Interop.GetDpiForWindow(hWnd);
        var scalingFactor = dpi / 96d;
        var w = (int)(width * scalingFactor);
        var h = (int)(height * scalingFactor);
        var centerX = (rect.left + rect.right) / 2;
        var centerY = (rect.bottom + rect.top) / 2;
        var left = Math.Max(0, centerX - (w / 2)); // do not allow to go off screen
        var top = Math.Max(0, centerY - (h / 2));

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, w, h,
                            Interop.SWP_SHOWWINDOW | Interop.SWP_NOZORDER);
    }



    public static void CenterWindowOnScreen(this IntPtr hWnd, double width, double height)
    {
        (var w, var h) = hWnd.ToSizePixels(width, height);

        var info = Interop.GetMonitorInfoOrNearest(hWnd);

        var centerX = (info.rcMonitor.left + info.rcMonitor.right) / 2;
        var centerY = (info.rcMonitor.bottom + info.rcMonitor.top) / 2;
        var left = centerX - (w / 2);
        var top = centerY - (h / 2);

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            left, top, w, h,
                            Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static void SetWindowSize(this IntPtr hWnd, double width, double height)
    {
        (var w, var h) = hWnd.ToSizePixels(width, height);

        Interop.SetWindowPos(hWnd, Interop.HWND_TOP,
                            0, 0, w, h,
                            Interop.SWP_NOMOVE | Interop.SWP_NOACTIVATE | Interop.SWP_NOZORDER);
    }

    public static double ScaleToPix(this IntPtr hwnd) => Interop.GetDpiForWindow(hwnd) / 96d;

    public static double ScaleToDip(this IntPtr hwnd) => 96d / Interop.GetDpiForWindow(hwnd);

    public static void BottomRighAlignWindowOnScreen(this IntPtr hwnd, double width, double minHeight)
    {
        var info = Interop.GetMonitorInfoOrNearest(hwnd);

        (var w, var h) = hwnd.ToSizePixels(width, minHeight);

        var left = info.rcWork.right - w;
        if (left < 0)
            left = 0;
        var top = info.rcWork.bottom - h;
        if (top < 0)
            top = 0;

        var success = Interop.SetWindowPos(hwnd, Interop.HWND_TOP,
                            left, top, w, h,
                            Interop.SWP_NOZORDER | Interop.SWP_NOACTIVATE);
    }

    /// <summary>
    /// Convert the specified Device Independent size to a size in pixels
    /// </summary>
    private static (int w, int h) ToSizePixels(this IntPtr hwnd, double dipWidth, double dipHeight)
    {
        var dpi = Interop.GetDpiForWindow(hwnd);
        var scalingFactor = dpi / 96d;
        return ((int)(dipWidth * scalingFactor), (int)(dipHeight * scalingFactor));
    }
}
