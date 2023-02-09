using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.UI;

namespace Tenuto.WinUI.Toolkit.Windowing;

internal static class TitleBarCustomization
{
    public static void SetTitleBarThemeColors(AppWindow appWindow, bool isDark, Color? backgroundOverride = null)
    {
        // Title bar customization only works on Windows 11
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = appWindow.TitleBar;

            var background = isDark ? Color.FromArgb(255, 0x21, 0x21, 0x21) : Color.FromArgb(255, 0xF4, 0xF6, 0xF7);
            if (backgroundOverride != null)
                background = (Color)backgroundOverride;

            var hoverBackground = isDark ? Color.FromArgb(255, 0x31, 0x31, 0x31) : Color.FromArgb(255, 0xE4, 0xE6, 0xE7);

            var foreground = isDark ? Colors.White : Colors.Black;
            var inactiveForeground = Colors.Gray;

            titleBar.ForegroundColor = foreground;
            titleBar.BackgroundColor = background;
            titleBar.InactiveForegroundColor = inactiveForeground;
            titleBar.InactiveBackgroundColor = background;

            titleBar.ButtonForegroundColor = foreground;
            titleBar.ButtonBackgroundColor = background;
            titleBar.ButtonInactiveForegroundColor = inactiveForeground;
            titleBar.ButtonInactiveBackgroundColor = background;

            titleBar.ButtonHoverForegroundColor = foreground;
            titleBar.ButtonHoverBackgroundColor = hoverBackground;
            titleBar.ButtonPressedForegroundColor = foreground;
            titleBar.ButtonPressedBackgroundColor = inactiveForeground;
        }
    }
}
