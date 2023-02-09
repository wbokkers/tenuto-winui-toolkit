using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Tenuto.WinUI.Toolkit.Windowing;

public enum TnContentDialogResult
{
    None,
    PrimaryButtonClicked,
    SecondaryButtonClicked
}

public sealed partial class TnContentDialog : UserControl
{
    private TnContentDialogResult _result;
    private TnWindowOptions? _options;
    private TnWindow? _window;

    public TnContentDialog(Window ownerWindow, bool isModal = true)
    {
        this.InitializeComponent();

        _options = isModal
            ? TnWindow.CreateModalDialog(ownerWindow)
            : TnWindow.CreateNonModalDialog(ownerWindow);

        _options.WithoutIcon()
            .WithSizeToFitContent()
            .WithPosition(TnWindowPosition.CenteredOnOwnerWindow);

        _result = TnContentDialogResult.None;

        MaxDialogHeight = 500;
        MaxDialogWidth = 800;
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _options = null;
    }

    public async Task<TnContentDialogResult> ShowAsync()
    {
        if (_options == null)
            return _result;

        _window = _options.Show(this);
        await _window.WindowClosedTask;

        return _result;
    }

    public double MaxDialogWidth { get; set; }

    public double MaxDialogHeight { get; set; }
    public double ButtonSpacing
    {
        get => ButtonPanel.Spacing;
        set => ButtonPanel.Spacing = value;
    }

    public string PrimaryButtonText
    {
        get => PrimaryButton.Content as string ?? "";
        set
        {
            PrimaryButton.Content = value;
            if (!string.IsNullOrEmpty(value))
            {
                PrimaryButton.Visibility = Visibility.Visible;
            }
        }
    }

    public string SecondaryButtonText
    {
        get => SecondaryButton.Content as string ?? "";
        set
        {
            SecondaryButton.Content = value;
            if (!string.IsNullOrEmpty(value))
            {
                SecondaryButton.Visibility = Visibility.Visible;
            }
        }
    }


    /// <summary>
    /// Override behavior of the content property
    /// </summary>
    public new UIElement? Content
    {
        get => DialogContent.Content as UIElement;
        set => DialogContent.Content = value;
    }
    public string Title
    {
        get => _window != null ? _window.Title
               : _options != null ? _options.Title
               : "";

        set
        {
            if (_window != null)
                _window.Title = value;
            else if (_options != null)
                _options.WithTitle(value);
        }
    }

    private void PrimaryButtonClicked(object sender, RoutedEventArgs e)
    {
        _result = TnContentDialogResult.PrimaryButtonClicked;
        _window?.Close();
    }

    private void SecondaryButtonClicked(object sender, RoutedEventArgs e)
    {
        _result = TnContentDialogResult.SecondaryButtonClicked;
        _window?.Close();
    }
}
