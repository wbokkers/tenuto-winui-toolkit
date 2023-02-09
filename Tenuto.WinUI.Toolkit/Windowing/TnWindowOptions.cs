using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.UI;
using WinRT.Interop;

namespace Tenuto.WinUI.Toolkit.Windowing;

/// <summary>
/// Provides a fluent interface for setting up window properties and showing the window
/// </summary>
public class TnWindowOptions
{
    public string? RestoreSizeKey { get; set; } = null;
    public bool IsCompact { get; set; } = false;
    public bool UseTitleBarTheme { get; set; }
    public bool TitleBarIsDark { get; set; }
    public Color? TitleBarBackgroundOverride { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public TnWindowPosition Position { get; set; }
    public bool SizeToFitContent { get; set; }
    public bool NoResizing { get; set; }
    public bool NoMinMaxButtons { get; set; }

    public string? IconFileName { get; set; }
    public bool IsAlwaysOnTop { get; set; }
    public bool IsContentExtendedIntoTitleBar { get; set; }
    public bool IsModal { get; set; }
    public Window? Owner { get; set; } = null;
    public nint HWndOwner => Owner != null ? WindowNative.GetWindowHandle(Owner) : IntPtr.Zero;
 
    public string Title { get; private set; } = "";
    public bool? IsShownInSwitchers { get; private set; } = null;
    public bool NoFocus { get; private set; }
    public Action? CloseAction { get; private set; }

    public static TnWindowOptions CreateCompact()
    {
        return Create().WithCompactInterface();
    }

    public static TnWindowOptions CreateNotification(TnWindowPosition position)
    {
        return Create()
            .WithAlwaysOnTopBehavior()
            .WithoutFocus()
            .WithoutIcon()
            .WithoutResizing()
            .WithoutMinMaxButtons()
            .WithContentExtendedIntoTitleBar()
            .WithSwitcherVisibility(false)
            .WithPosition(position);
    }

    public static TnWindowOptions Create()
    {
        return new TnWindowOptions();
    }

    public static TnWindowOptions CreateModalDialog(Window owner)
    {
        return Create()
            .WithOwner(owner)
            .WithModalBehavior()
            .WithoutIcon()
            .WithAlwaysOnTopBehavior();
    }

    public static TnWindowOptions CreateNonModalDialog(Window? owner)
    {
    
        return Create()
            .WithOwner(owner)
            .WithoutIcon()
            .WithAlwaysOnTopBehavior();
    }


    public TnWindowOptions WithOwner(Window? owner)
    {
        Owner = owner;
        return this;
    }

    public TnWindowOptions WithRestoreSizeKey(string? restoreKey)
    {
        RestoreSizeKey = restoreKey;
        return this;
    }

    public TnWindowOptions WithTitle(string? title)
    {
        Title = title ?? "";
        return this;
    }

    public TnWindowOptions WithSwitcherVisibility(bool isShownInSwitchers = true)
    {
        IsShownInSwitchers = isShownInSwitchers;
        return this;
    }

    public TnWindowOptions WithoutFocus()
    {
        NoFocus = true;
        return this;
    }

    public TnWindowOptions WithoutMinMaxButtons()
    {
        NoMinMaxButtons = true;
        return this;
    }

    public TnWindowOptions WithCompactInterface()
    {
        IsCompact = true;
        return this;
    }

    public TnWindowOptions WithoutResizing()
    {
        NoResizing = true;
        return this;
    }

    public TnWindowOptions WithoutIcon()
    {
        IconFileName = null;

        return this;
    }

    public TnWindowOptions WithIcon(string iconFileName)
    {
        try
        {
            IconFileName = iconFileName;
        }
        catch
        {
        }
        return this;
    }

    public TnWindowOptions WithModalBehavior(bool isModal = true)
    {
        IsModal = isModal;
        return this;
    }

    /// <summary>
    /// Extend the content in the title bar.
    /// </summary>
    public TnWindowOptions WithContentExtendedIntoTitleBar(bool isContentExtended = true)
    {
        IsContentExtendedIntoTitleBar = isContentExtended;

        return this;
    }

    public TnWindowOptions WithAlwaysOnTopBehavior(bool isAlwaysOnTop = true)
    {
        IsAlwaysOnTop = isAlwaysOnTop;
        return this;
    }

    public TnWindowOptions WithSizeToFitContent(bool fitsContent = true)
    {
        SizeToFitContent = fitsContent;
        return this;
    }

    public TnWindowOptions WithPosition(TnWindowPosition position)
    {
        Position = position;
        return this;
    }

    public TnWindowOptions WithSize(double width, double height)
    {
        Width = width;
        Height = height;
        return this;
    }

    public TnWindowOptions WithCloseAction(Action? closeAction)
    {
        CloseAction = closeAction;
        return this;
    }

    public TnWindowOptions WithThemedTitleBarColors(bool isDark = false, Color? backgroundOverride = null)
    {
        UseTitleBarTheme = true;
        TitleBarIsDark = isDark;
        TitleBarBackgroundOverride = backgroundOverride;

        return this;
    }

    public TnWindow Show(FrameworkElement content)
    {
        return TnWindow.ShowWithOptions(this, content);
    }

    /// <summary>
    /// Show the window and wait for it to be closed
    /// </summary>
    public async Task ShowAsync(FrameworkElement content)
    {
        await TnWindow.ShowWithOptionsAsync(this, content);
    }
}
