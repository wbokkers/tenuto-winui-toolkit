using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tenuto.WinUI.Toolkit.WinApi;
using WinRT.Interop;

namespace Tenuto.WinUI.Toolkit.Windowing;

public enum TnWindowPlacement
{
    Default,
    CenteredOnScreen,
    CenteredOnOwnerWindow,
    BottomRightOfScreen,
}

public partial class TnWindow : Window
{
    private static readonly object _LockObject = new();
    private static readonly List<TnWindow> _ActiveWindows = new();
    private readonly TaskCompletionSource _closeCompletion;

    private readonly IntPtr _hWnd;
    private readonly AppWindow _appWindow;
    private readonly AppWindowPresenter _presenter;
    private IntPtr _hWndOwner;
    private double _width = 0;
    private double _height = 0;
    private NativeIcon? _icon;
    private bool _isModal = false;
    private TnWindowPlacement _placement;

    private bool _sizeToFitContent;

    private TnWindow(bool isCompact)
    {
        InitializeComponent();

        _hWndOwner = IntPtr.Zero;

        // see https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        _closeCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _hWnd = WindowNative.GetWindowHandle(this);

        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (isCompact)
        {
            var presenter = CompactOverlayPresenter.Create();
            _appWindow.SetPresenter(presenter);
            _presenter = presenter;
        }
        else
        {
            var presenter = OverlappedPresenter.Create();
            presenter.IsMinimizable = true;
            presenter.IsMaximizable = true;
            presenter.IsResizable = true;
            presenter.IsAlwaysOnTop = false;
            _appWindow.SetPresenter(presenter);
            _presenter = presenter;
        }

        Title = "";

        Closed += OnClosed;
    }

    public Task WindowClosedTask
    {
        get
        {
            return _closeCompletion?.Task ?? Task.CompletedTask;
        }
    }

    public bool HasOwner => _hWndOwner != IntPtr.Zero;

    public static TnWindow CreateCompact() => new(isCompact: true);

    public static TnWindow Create() => new(isCompact: false);

    public static TnWindow CreateModalDialog(Window ownerWindow) => CreateModalDialog(WindowNative.GetWindowHandle(ownerWindow));

    public static TnWindow CreateModalDialog(IntPtr hWndOwner)
    {
        return Create()
            .WithOwner(hWndOwner)
            .WithModalBehavior()
            .WithAlwaysOnTopBehavior();
    }

    public static TnWindow CreateNonModalDialog(IntPtr hWndOwner)
    {
        return Create()
            .WithOwner(hWndOwner)
            .WithAlwaysOnTopBehavior();
    }

    public static void CloseAllOpenWindows()
    {
        lock (_LockObject)
        {
            var cachedActive = _ActiveWindows.ToArray();
            foreach (var window in cachedActive)
            {
                window.Close();
            }
        }
    }

    public static bool CloseWindowWithContent(FrameworkElement content)
    {
        lock (_LockObject)
        {
            foreach (var window in _ActiveWindows)
            {
                if (window.Content == content)
                {
                    window.Close();
                    return true;
                }
            }

            return false;
        }
    }

    public static TnWindow? GetWindowWithContentType(Type type)
    {
        lock (_LockObject)
        {
            foreach (var window in _ActiveWindows)
            {
                if (window.Content != null && window.Content.GetType() == type)
                {
                    return window;
                }
            }

            return null;
        }
    }

    public TnWindow WithOwner(Window ownerWindow) => WithOwner(WindowNative.GetWindowHandle(ownerWindow));

    public TnWindow WithOwner(IntPtr hWndOwner)
    {
        _hWndOwner = hWndOwner;
        // HACK to set the OWNER of the window,
        // (needed for IsModal = true, otherwise it will crash!)
        // see https://docs.microsoft.com/en-us/answers/questions/910658/winui3how-to-show-a-model-window.html
        Interop.SetWindowLongPtr(_hWnd, Interop.GWLP_HWNDPARENT, hWndOwner);
        return this;
    }

    public TnWindow WithTitle(string? title)
    {
        Title = title ?? "";
        return this;
    }

    public TnWindow WithModalBehavior(bool isModal = true)
    {
        _isModal = isModal;

        // Force a modal to have an owner
        if (!HasOwner)
            throw new InvalidOperationException("WithModalBehavior must be preceded by the OwnedBy method");

        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsMinimizable = !isModal;
            // https://docs.microsoft.com/en-us/answers/questions/910658/winui3how-to-show-a-model-window.html
            presenter.IsModal = isModal; // crashes if the OWNER not set
            // NOTE: Setting IsModal = true will flash the window when the owner window is pressed
        }

        return this;
    }

    public TnWindow WithAlwaysOnTopBehavior(bool isAlwaysOnTop = true)
    {
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = isAlwaysOnTop;
        }

        return this;
    }

    public TnWindow WithBorderAndTitleBar(bool hasBorder = true, bool hasTitleBar = true)
    {
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(hasBorder, hasTitleBar);
        }

        return this;
    }

    public TnWindow WithSizeToFitContent(bool fitsContent = true)
    {
        _sizeToFitContent = fitsContent;
        return this;
    }

    public TnWindow WithPlacement(TnWindowPlacement placement)
    {
        _placement = placement;
        return this;
    }

    public TnWindow WithSize(double width, double height)
    {
        _width = width;
        _height = height;
        return this;
    }

    public TnWindow Show(FrameworkElement content)
    {
        Content = content;

        if (_sizeToFitContent)
            SetWidthAndHeightToFitContent(3000, 3000);

        if (_placement == TnWindowPlacement.Default 
            &&_width > 0 && _height > 0)
        {
            _hWnd.SetWindowSize(_width, _height);
        }
        else
        {
            // Placements that need a width and a height
            if (_width == 0 || _height == 0)
            {
                if (Bounds.Width > 0 && Bounds.Height > 0)
                {
                    _width = Bounds.Width;
                    _height = Bounds.Height;
                }
                else
                {
                    SetWidthAndHeightToFitContent(3000, 3000);
                }
            }

            if (_placement == TnWindowPlacement.BottomRightOfScreen)
                _hWnd.BottomRighAlignWindowOnScreen(_width, _height);
            else if (_placement == TnWindowPlacement.CenteredOnOwnerWindow && HasOwner)
                DoCenterOnOwnerWindow();
            else if (_placement == TnWindowPlacement.CenteredOnScreen)
                _hWnd.CenterWindowOnScreen(_width, _height);
            else
                _hWnd.SetWindowSize(_width, _height);
        }

        if (_isModal)
        {
            // disable the main window, making it impossible to open other dialogs.
            EnableOwnerWindow(false);
        }

        Activate(); // This will bring the window to the foreground

        _ActiveWindows.Add(this);

        return this;
    }

    /// <summary>
    /// Show the window and wait for it to be closed
    /// </summary>
    /// <returns></returns>
    public async Task<TnWindow> ShowAsync(FrameworkElement content)
    {
        Show(content);

        // Wait for the window to close
        await WindowClosedTask;

        return this;
    }

    public void SetAsForeground()
    {
        _ = Interop.SetForegroundWindow(_hWnd); // activation is not enough
    }

    public TnWindow WithoutIcon()
    {
        try
        {
            // Change the extended window style to not show a window icon
            var currentStyle = Interop.GetWindowLongPtr(_hWnd, Interop.GWL_EXSTYLE);
            var newStyle = (int)currentStyle | Interop.WS_EX_DLGMODALFRAME;
            Interop.SetWindowLongPtr(_hWnd, Interop.GWL_EXSTYLE, (IntPtr)newStyle);
            Interop.SendMessage(_hWnd, Interop.WM_SETICON, 0, IntPtr.Zero);
        }
        catch { }
        return this;
    }

    public TnWindow WithIcon(string iconFileName, int desiredSize = 32)
    {
        try
        {
            _icon ??= NativeIcon.FromFile(iconFileName, desiredSize);
            Interop.SendMessage(_hWnd, Interop.WM_SETICON, 0, _icon.Handle);
        }
        catch
        {
        }

        return this;
    }

    public void BringToTop()
    {
        Interop.BringWindowToTop(_hWnd);
    }

    private void SetWidthAndHeightToFitContent(double maxWidth, double maxHeight)
    {
        var width = maxWidth;
        var height = maxHeight;

        // Determine the desired size
        if (Content != null)
        {
            Content.Measure(new Windows.Foundation.Size(maxWidth, maxHeight));
            width = Math.Min(maxWidth, Content.DesiredSize.Width + 16);
            height = Math.Min(maxHeight, Content.DesiredSize.Height + 40);
        }

        _width = width;
        _height = height;
    }

    private void DoCenterOnOwnerWindow()
    {
        if (HasOwner)
        {
            if (Interop.GetWindowRect(_hWndOwner, out var rect))
            {
                _hWnd.CenterWindowInRect(_width, _height, rect);
            }
        }
    }

    /// <summary>
    /// On closing the window,
    /// enable the owner window (if needed),
    /// cleanup the view,
    /// remove the content
    /// </summary>
    private void OnClosed(object sender, WindowEventArgs args)
    {
        lock (_LockObject)
        {
            Closed -= OnClosed;

            if (_isModal)
            {
                EnableOwnerWindow(true);
            }

            _ActiveWindows.Remove(this);

            _closeCompletion.SetResult(); // trigger completion

            Content = null;
        }
    }

    private void EnableOwnerWindow(bool enable)
    {
        if (HasOwner)
        {
            if (_hWndOwner != IntPtr.Zero)
                Interop.EnableWindow(_hWndOwner, enable);
        }
    }
}