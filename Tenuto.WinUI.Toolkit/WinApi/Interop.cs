using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Tenuto.WinUI.Toolkit;

public class Interop
{
    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, [In, Out] MONITORINFOEX info);




    //        public const uint NIF_INFO = 0x010;
    public const uint NIF_MESSAGE = 0x01;

    //public const uint NIF_ICON = 0x02;
    //public const uint NIF_TIP = 0x04;
    //public const uint NIF_SHOWTIP = 0x00000080;
    //public const int GCL_HCURSOR = -12;
    //public const int IDC_WAIT = 32514;
    //public const int IDC_ARROW = 32512;
    //public const uint NIM_ADD = 0x00;
    //public const uint NIM_MODIFY = 0x01;
    //public const uint NIM_DELETE = 0x02;
    //public const int IMAGE_BITMAP = 0;
    //public const int IMAGE_ICON = 1;
    //public const int IMAGE_CURSOR = 2;
    //public const int IMAGE_ENHMETAFILE = 3;



    public const uint WM_CLOSE = 0x0010;
    public const uint WM_COMMAND = 0x0111;
    public const uint WM_LBUTTONDOWN = 0x0201;
    public const uint WM_APP = 0x8000;
    public const uint WM_USER = 0x0400;
    public const uint WM_HOTKEY = 0x0312;
    public const uint WM_DESTROY = 0x0002;
    public const uint WM_LBUTTONDBLCLK = 0x0203;
    public const uint WM_CONTEXTMENU = 0x007B;
    public const uint WM_RBUTTONDOWN = 0x0204;
    public const uint WM_SETCURSOR = 0x0020;
    public const uint WM_SETFOCUS = 0x0007;
    public const uint WM_KILLFOCUS = 0x0008;
    public const uint WM_ACTIVATE = 0x0006;
    public const int WM_SETICON = 0x0080;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public const uint MF_BYPOSITION = 0x00000400;
    public const uint TPM_CENTERALIGN = 0x0004;
    public const uint TPM_LEFTALIGN = 0x000;
    public const uint TPM_RIGHTALIGN = 0x008;
    public const uint TPM_BOTTOMALIGN = 0x0020;
    public const uint TPM_TOPALIGN = 0x000;
    public const uint TPM_VCENTERALIGN = 0x0010;
    public const int SWP_NOSIZE = 0x0001,
        SWP_NOMOVE = 0x0002,
        SWP_NOZORDER = 0x0004,
        SWP_NOREDRAW = 0x0008,
        SWP_NOACTIVATE = 0x0010,
        SWP_SHOWWINDOW = 0x0040;
    public const int MONITOR_DEFAULTTONULL = 0;
    public const int MONITOR_DEFAULTTOPRIMARY = 1;
    public const int MONITOR_DEFAULTTONEAREST = 2;
    public const uint MB_OK = 0x0;
    public const uint MB_ICONEXCLAMATION = 0x00000030;
    public const int GWLP_HWNDPARENT = -8;
    public const int GWLP_WNDPROC = -4;
    public const int GWLP_STYLE = -16;
    public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
    public static readonly IntPtr HWND_NOTOPMOST = (IntPtr)(-2);
    public static readonly IntPtr HWND_TOP = (IntPtr)0;
    public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    // This static method is required because legacy OSes do not support
    // SetWindowLongPtr
    public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        else
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    [DllImport("user32.dll")]
    public static extern int GetDpiForWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnableWindow(IntPtr hwnd, bool bEnable);


    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    public static MONITORINFOEX GetMonitorInfoOrNearest(IntPtr hwnd)
    {
        var hwndDesktop = Interop.MonitorFromWindow(hwnd, Interop.MONITOR_DEFAULTTONEAREST);
        var info = new Interop.MONITORINFOEX();
        info.cbSize = 40;
        Interop.GetMonitorInfo(hwndDesktop, info);

        return info;
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);


    [DllImport("Advapi32.dll")]
    private static extern bool GetUserName(StringBuilder lpBuffer, ref int nSize);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


    //[StructLayout(LayoutKind.Sequential)]
    //public struct NOTIFYICONDATA
    //{
    //    public uint Size; // DWORD
    //    public IntPtr HWnd; // HWND
    //    public uint ID; // UINT
    //    public uint Flags; // UINT
    //    public uint CallbackMessage; // UINT
    //    public IntPtr HIcon; // HICON

    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    //    public string Tip; // char[128]

    //    public uint State; // DWORD
    //    public uint StateMask; // DWORD

    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    //    public string Info; // char[256]

    //    public uint TimeoutOrVersion; // UINT

    //    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    //    public string InfoTitle; // char[64]

    //    public uint InfoFlags; // DWORD
    //                           //GUID guidItem; > IE 6
    //}

    //[StructLayout(LayoutKind.Sequential)]
    //public struct POINTSTRUCT
    //{
    //    public int x;
    //    public int y;

    //    public POINTSTRUCT(int x, int y)
    //    {
    //        this.x = x;
    //        this.y = y;
    //    }
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

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