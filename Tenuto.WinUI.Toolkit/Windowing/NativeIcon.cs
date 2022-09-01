using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tenuto.WinUI.Toolkit.Windowing;

public class NativeIcon : IDisposable
{
    private readonly IntPtr _handle;

    private NativeIcon(IntPtr iconHandle)
    {
        _handle = iconHandle;
    }

    /// <inheritdoc />
    ~NativeIcon()
    {
        Dispose(false);
    }

    internal IntPtr Handle => _handle;

    /// <summary>
    /// Loads an icon from an .ico file.
    /// </summary>
    public static NativeIcon FromFile(string filename, int desiredSize = 32)
    {
        const uint LR_LOADFROMFILE = 0x00000010;
        var handle = Interop.LoadImage(IntPtr.Zero, filename, 1, desiredSize, desiredSize, LR_LOADFROMFILE);
        ThrowIfInvalid(handle);
        return new NativeIcon(handle);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interop.DestroyIcon(Handle);
        }
    }

    private static void ThrowIfInvalid(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            var ex = new Win32Exception(Marshal.GetLastWin32Error());
            throw ex;
        }
    }
}
