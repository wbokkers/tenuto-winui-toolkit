using System;
using System.Runtime.InteropServices;

namespace Tenuto.WinUI.Toolkit;

internal class Interop
{
    public const int GWL_EXSTYLE = -20;
    public const int GWLP_HWNDPARENT = -8;
    public const int GWLP_STYLE = -16;
    public const int GWLP_WNDPROC = -4;
    public const uint MB_ICONEXCLAMATION = 0x00000030;
    public const uint MB_OK = 0x0;
    public const uint MF_BYPOSITION = 0x00000400;
    public const int MONITOR_DEFAULTTONEAREST = 2;
    public const int MONITOR_DEFAULTTONULL = 0;
    public const int MONITOR_DEFAULTTOPRIMARY = 1;
    public const uint NIF_MESSAGE = 0x01;
    public const int SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_SHOWWINDOW = 0x0040;

    public const int SW_SHOWNORMAL = 1;
    public const int SW_RESTORE = 9;
    public const int SW_MINIMIZE = 6;
    public const int SW_MAXIMIZE = 3;
    public const int SW_SHOWNOACTIVATE = 4;
    public const int SW_SHOW = 5;
    public const int SW_SHOWMINNOACTIVE = 7;
    public const int SW_FORCEMINIMIZE = 11;
    public const int SW_SHOWDEFAULT = 10;
    public const int SW_HIDE = 0;


    public const uint TPM_BOTTOMALIGN = 0x0020;
    public const uint TPM_CENTERALIGN = 0x0004;
    public const uint TPM_LEFTALIGN = 0x000;
    public const uint TPM_RIGHTALIGN = 0x008;
    public const uint TPM_TOPALIGN = 0x000;
    public const uint TPM_VCENTERALIGN = 0x0010;
    public const uint WM_ACTIVATE = 0x0006;
    public const uint WM_APP = 0x8000;
    public const uint WM_CLOSE = 0x0010;
    public const uint WM_COMMAND = 0x0111;
    public const uint WM_CONTEXTMENU = 0x007B;
    public const uint WM_DESTROY = 0x0002;
    public const uint WM_HOTKEY = 0x0312;
    public const uint WM_KILLFOCUS = 0x0008;
    public const uint WM_LBUTTONDBLCLK = 0x0203;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_RBUTTONDOWN = 0x0204;
    public const uint WM_SETCURSOR = 0x0020;
    public const uint WM_SETFOCUS = 0x0007;
    public const int WM_SETICON = 0x0080;
    public const uint WM_USER = 0x0400;
    public const int WS_EX_DLGMODALFRAME = 0x0001;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
    public static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);
    public static readonly IntPtr HWND_TOP = (IntPtr)0;
    public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

    [DllImport("user32.dll")]
    public static extern int GetDpiForWindow(IntPtr hWnd);

    public static MONITORINFOEX GetMonitorInfoOrNearest(IntPtr hwnd)
    {
        var hwndDesktop = Interop.MonitorFromWindow(hwnd, Interop.MONITOR_DEFAULTTONEAREST);
        var info = new Interop.MONITORINFOEX
        {
            cbSize = 40
        };
        Interop.GetMonitorInfo(hwndDesktop, info);

        return info;
    }

    public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
            return GetWindowLongPtr64(hWnd, nIndex);
        else
            return GetWindowLongPtr32(hWnd, nIndex);
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    public static bool IsRectangleOnAMonitor(int pixX, int pixY, int pixWidth, int pixHeight)
    {
        var rect = new RECT
        {
            left = pixX,
            top = pixY,
            right = pixX + pixWidth,
            bottom = pixY + pixHeight
        };

        var hwndDesktop = Interop.MonitorFromRect(ref rect, Interop.MONITOR_DEFAULTTONULL);
        return hwndDesktop != IntPtr.Zero;
    }

    /// <summary>
    /// Determines whether a window is maximized.
    /// </summary>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsZoomed(IntPtr hWnd);


    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8) // 64 bit
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    /// <summary>
    /// Determines whether the specified window is minimized (iconic).
    /// </summary>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);


    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFOEX info);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator System.Drawing.Point(POINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator POINT(System.Drawing.Point p)
        {
            return new POINT(p.X, p.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MONITORINFOEX
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
    }
}