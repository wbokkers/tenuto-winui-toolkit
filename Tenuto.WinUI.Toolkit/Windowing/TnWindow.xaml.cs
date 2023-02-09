using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Tenuto.WinUI.Toolkit.WinApi;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace Tenuto.WinUI.Toolkit.Windowing;

public enum TnWindowPosition
{
    Default,
    CenteredOnScreen,
    CenteredOnOwnerWindow,
    BottomRightOfScreen,
    BottomCenterOfScreen
}

public partial class TnWindow : Window
{
    private static readonly List<TnWindow> _AllWindows = new();

    private static readonly object _LockObject = new();

    private readonly AppWindow _appWindow;
    private readonly TaskCompletionSource _closeCompletion;
    private readonly IntPtr _hWnd;
    private readonly TnWindowOptions _options;
    private readonly AppWindowPresenter _presenter;
    private double _height;
    private double _width;
    private TnWindow(TnWindowOptions options, FrameworkElement content)
    {
        _options = options;

        InitializeComponent();

        // see https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        _closeCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _hWnd = WindowNative.GetWindowHandle(this);

        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        // Show or remove the icon
        if(!string.IsNullOrEmpty(options.IconFileName))
            _appWindow.SetIcon(options.IconFileName);
        else if (AppWindowTitleBar.IsCustomizationSupported())
             _appWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
      
        if (options.IsCompact)
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
        Content = content;
    }

    public string DragDropId => "WIN_" + _appWindow.Id.Value.ToString();
    public Task WindowClosedTask => _closeCompletion?.Task ?? Task.CompletedTask;
    public static void CloseAllOpenWindows()
    {
        lock (_LockObject)
        {
            var cachedWindows = _AllWindows.ToArray();
            foreach (var window in cachedWindows)
            {
                window.Close();
            }
        }
    }

    public static bool CloseWindowWithContent(FrameworkElement content)
    {
        lock (_LockObject)
        {
            foreach (var window in _AllWindows)
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

    public static TnWindowOptions Create() => TnWindowOptions.Create();

    public static TnWindowOptions CreateCompact() => TnWindowOptions.CreateCompact();

    public static TnWindowOptions CreateModalDialog(Window owner) => TnWindowOptions.CreateModalDialog(owner);

    public static TnWindowOptions CreateNonModalDialog(Window? owner = null) => TnWindowOptions.CreateNonModalDialog(owner);

    public static TnWindowOptions CreateNotification(TnWindowPosition position) => TnWindowOptions.CreateNotification(position);
    public static IEnumerable<TnWindow> GetAllWindowsWithContentType(Type type)
    {
        lock (_LockObject)
        {
            return _AllWindows.Where(w => w.Content.GetType() == type);
        }
    }

    public static TnWindow? GetWindowWithContentType(Type type)
    {
        lock (_LockObject)
        {
            foreach (var window in _AllWindows)
            {
                if (window.Content != null && window.Content.GetType() == type)
                {
                    return window;
                }
            }

            return null;
        }
    }
    public static TnWindow? GetWindowWithDragDropId(string dragDropId)
    {
        lock (_LockObject)
        {
            return _AllWindows.FirstOrDefault(w => w.DragDropId == dragDropId);
        }
    }

    public static TnWindow ShowWithOptions(TnWindowOptions options, FrameworkElement content)
    {
        var window = new TnWindow(options, content);
        window.DoShow();

        return window;
    }

    public static async Task ShowWithOptionsAsync(TnWindowOptions options, FrameworkElement content)
    {
        var window = new TnWindow(options, content);
        window.DoShow();

        // Wait for the window to close
        await window.WindowClosedTask;
    }

    public void BringToTop()
    {
        Interop.BringWindowToTop(_hWnd);
    }

    public void Hide()
    {
        _appWindow.Hide();
    }

    /// <summary>
    /// Make the window visible taking the initial WithNoFocus() setting
    /// into account
    /// </summary>
    public void MakeVisible(bool bringToTop = false)
    {
        if (_options.NoFocus)
        {
            MakeVisibleWithoutFocus(bringToTop);
        }
        else
        {
            Activate();
        }
    }

    /// <summary>
    /// Make the window visible without focus, even when the WithNoFocus options was not supplied.
    /// </summary>
    public void MakeVisibleWithoutFocus(bool bringToTop = false)
    {
        _hWnd.ShowWindowWithoutFocus(bringToTop);
    }

    /// <summary>
    /// (Re)apply the position and size settings.
    /// You can use this to recalculate the windows size (if WithSizeToFitContent was being used)
    /// and to reposition the window to the current position settings
    /// </summary>
    public void ResetPlacement() => SetPlacementSettings();

    /// <summary>
    /// Tries to restore the size and the position of this window from a previous usage of the restoreKey.
    /// The restoreKey will also be used to save the size and position on closing the window.
    /// </summary>
    public bool RestoreSizeAndPosition()
    {
        if (_options.RestoreSizeKey == null)
            return false;

        // TODO:
        // Make restore explicitly set the _width/_height/_posX/_posY

        var isRestored = _hWnd.RestoreSizeAndPosition(_options.RestoreSizeKey);

        if (isRestored)
        {
            // Reset all size and position related settings
            _width = _options.Width = 0;
            _height = _options.Height = 0;
            _options.Position = TnWindowPosition.Default;
            _options.SizeToFitContent = false;
        }

        return isRestored;
    }

    public void SetThemedTitleBarColors(bool isDark = false, Color? backgroundOverride = null)
    {
        TitleBarCustomization.SetTitleBarThemeColors(_appWindow, isDark, backgroundOverride);
    }

    public async Task WaitOnClose()
    {
        await WindowClosedTask;
    }
    internal void AdaptSizeToFitContent()
    {
        _options.WithSizeToFitContent();
        ResetPlacement();
    }

  
    private void DoCenterOnOwnerWindow(SizeInt32 size)
    {
        if (_options.HWndOwner != IntPtr.Zero)
        {
            if (Interop.GetWindowRect(_options.HWndOwner, out var rect))
            {
                _hWnd.CenterOnRect(size, rect);
            }
        }
    }

    private void DoShow()
    {
        if (!_AllWindows.Contains(this))
            _AllWindows.Add(this);

        if (!string.IsNullOrEmpty(_options.Title))
        {
            Title = _options.Title;
        }

        SetOwnerAndModalBehavior(_options.HWndOwner, _options.IsModal);
        SetAlwaysOnTopBehavior(_options.IsAlwaysOnTop);
        SetContentExtendedIntoTitleBar(_options.IsContentExtendedIntoTitleBar);
        if (_options.UseTitleBarTheme)
            SetThemedTitleBarColors(_options.TitleBarIsDark, _options.TitleBarBackgroundOverride);

        SetMinMaxButtons(!_options.NoMinMaxButtons);
        SetIsResizable(!_options.NoResizing);

        // Only override switcher visibility when explicitly set
        if (_options.IsShownInSwitchers.HasValue)
            SetSwitcherVisibility(_options.IsShownInSwitchers.Value);

        SetPlacementSettings();

        if (_options.CloseAction != null)
            Closed += (_, __) => _options.CloseAction();

        MakeVisible(bringToTop: true);

       

        if (_options.IsModal)
        {
            Content.KeyDown += OnModalWindowKeyDown;

            // disable the main window, making it impossible to open other dialogs.
            EnableOwnerWindow(false);
        }
    }

    private void EnableOwnerWindow(bool enable)
    {
        var ownerWindow = _options.Owner;

        if (ownerWindow != null && _options.IsModal)
        {
            if (ownerWindow.Content is FrameworkElement content)
            {
                content.IsHitTestVisible = enable;
                content.Opacity = enable ? 1.0 : 0.6;
            }

            // TODO: EnableWindow does not (yet) work well with WinUI
            //       some XAML UI may still receive messages when disabling
            // Interop.EnableWindow(_options.HWndOwner, enable);
        }
    }

    /// <summary>
    /// On closing the window,
    /// enable the owner window (if needed),
    /// remove the content
    /// </summary>
    private void OnClosed(object sender, WindowEventArgs args)
    {
        lock (_LockObject)
        {
            Closed -= OnClosed;

            //Interop.AnimateWindow(_hWnd, 1000, Interop.AW_SLIDE | Interop.AW_VER_POSITIVE);
            if (_options.RestoreSizeKey != null)
                _hWnd.SaveSizeAndPosition(_appWindow, _options.RestoreSizeKey);

            if (_options.IsModal)
            {
                EnableOwnerWindow(true);
            }

            _AllWindows.Remove(this);

            // Notify the close completion that the window is closed
            _closeCompletion.SetResult(); // trigger completion

            Content = null; // Remove content from window
        }
    }

    /// <summary>
    /// Apply the window options.
    /// Assume that the Content is already set!!
    /// </summary>
    private void OnModalWindowKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            e.Handled = true;
            Close();
        }
        else
        {
            e.Handled = false;
        }
    }

    private void SetAlwaysOnTopBehavior(bool isAlwaysOnTop = true)
    {
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = isAlwaysOnTop;
        }
    }

    /// <summary>
    /// Extend the content in the title bar.
    /// </summary>
    private void SetContentExtendedIntoTitleBar(bool isContentExtended = true)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            // Only adapt when setting is different.
            // Otherwise we get strange behavior
            if(_appWindow.TitleBar.ExtendsContentIntoTitleBar != isContentExtended)
                _appWindow.TitleBar.ExtendsContentIntoTitleBar = isContentExtended;
        }
    }

    private void SetIsResizable(bool isResizable)
    {
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = isResizable;
        }
    }

    private void SetMinMaxButtons(bool showMinMaxButtons)
    {
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsMinimizable = showMinMaxButtons;
            presenter.IsMaximizable = showMinMaxButtons;
        }
    }

    private void SetOwnerAndModalBehavior(IntPtr hWndOwner, bool isModal)
    {
        //
        // Force a modal to have an owner
        //
        if (isModal && hWndOwner == IntPtr.Zero)
            throw new ArgumentException("A modal dialog should have a window owner", "hWndOwner");

        //
        // Set the owner
        //
        if (hWndOwner != IntPtr.Zero)
        {
            // HACK to set the OWNER of the window,
            // (needed for IsModal = true, otherwise it will crash!)
            // see https://docs.microsoft.com/en-us/answers/questions/910658/winui3how-to-show-a-model-window.html
            Interop.SetWindowLongPtr(_hWnd, Interop.GWLP_HWNDPARENT, hWndOwner);
        }

        //
        // Set modal setting
        //
        if (_presenter is OverlappedPresenter presenter)
        {
            presenter.IsMinimizable = !isModal;
            // https://docs.microsoft.com/en-us/answers/questions/910658/winui3how-to-show-a-model-window.html
            presenter.IsModal = isModal; // crashes if the OWNER not set
            // NOTE: Setting IsModal = true will flash the window when the owner window is pressed
        }
    }

    private void SetPlacementSettings()
    {
        _width = _options.Width;
        _height = _options.Height;

        // If a restore size key exists, try to restore the size and the position
        if (_options.RestoreSizeKey != null && RestoreSizeAndPosition())
        {
            return;
        }

        // Override _width/_height when 'fit to content' was requested
        if (_options.SizeToFitContent)
            SetWidthAndHeightToFitContent(3000, 3000);

        // set _width and _height if not defined
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

        var size = _hWnd.ToSizePixels(_width, _height);
        _appWindow.ResizeClient(size);

        var monitor = Interop.GetMonitorInfoOrNearest(_hWnd);

        if (_options.Position == TnWindowPosition.BottomRightOfScreen)
        {
            _hWnd.PlaceRightOnMonitor(_appWindow.Size, monitor);
        }
        else if (_options.Position == TnWindowPosition.BottomCenterOfScreen)
        {
            _hWnd.PlaceBottomCenterOnMonitor(_appWindow.Size, monitor);
        }
        else if (_options.Position == TnWindowPosition.CenteredOnOwnerWindow)
        {
            // Does not really work if no owner
            DoCenterOnOwnerWindow(_appWindow.Size);
        }
        else if (_options.Position == TnWindowPosition.CenteredOnScreen)
        {
            _hWnd.CenterOnMonitor(_appWindow.Size, monitor);
        }
        else // default
        {
        }
    }
    private void SetSwitcherVisibility(bool isShownInSwitchers = true)
    {
        if (_appWindow != null)
            _appWindow.IsShownInSwitchers = isShownInSwitchers;
    }
    private void SetWidthAndHeightToFitContent(double maxWidth, double maxHeight)
    {
        var width = maxWidth;
        var height = maxHeight;

        var fitToContentHeightAdjustmentDip = _options?.IsContentExtendedIntoTitleBar == true
            ? -32 : 0;

        // Determine the desired size
        if (Content != null)
        {
            Content.Measure(new Size(maxWidth, maxHeight));

            width = Math.Min(maxWidth, Content.DesiredSize.Width);
            height = Math.Min(maxHeight, Content.DesiredSize.Height);

            height += fitToContentHeightAdjustmentDip;
        }

        _width = width;
        _height = height;
    }
}