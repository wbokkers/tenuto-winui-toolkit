using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
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
    private TnWindow? _window;

    public TnContentDialog(Window ownerWindow, bool isModal = true)
       : this(WindowNative.GetWindowHandle(ownerWindow), isModal)
    {
    }

    public TnContentDialog(IntPtr hWndOwner, bool isModal = true)
    {
        this.InitializeComponent();

        _window = isModal
            ? TnWindow.CreateModalDialog(hWndOwner)
            : TnWindow.CreateNonModalDialog(hWndOwner);
            
        _window.WithoutIcon()
            .WithSizeToFitContent()
            .WithPlacement(TnWindowPlacement.CenteredOnOwnerWindow);
     
        _window.Closed += OnWindowClosed;
        _result = TnContentDialogResult.None;

        MaxDialogHeight = 500;
        MaxDialogWidth = 800;
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _window = null;
    }

    public async Task<TnContentDialogResult> ShowAsync()
    {
        if (_window == null)
            return _result;

        await _window.ShowAsync(this);
        return _result;
    }

    //private void ResizeWindowForContent()
    //{
    //    var width = MaxDialogWidth;
    //    var height = MaxDialogHeight;

    //    // Determine the desired size
    //    if (_window != null && Content != null)
    //    {
    //        var bounds = _window.Bounds;
    //        RootGrid.Measure(new Windows.Foundation.Size(bounds.Width, bounds.Height));
    //        width = Math.Min(MaxDialogWidth, RootGrid.DesiredSize.Width + 16);
    //        height = Math.Min(MaxDialogHeight, RootGrid.DesiredSize.Height + 40);
    //    }
    //    else if (Content != null)
    //    {
    //        RootGrid.Measure(new Windows.Foundation.Size(MaxDialogWidth, MaxDialogHeight));
    //        width = RootGrid.DesiredSize.Width + 16;
    //        height = RootGrid.DesiredSize.Height + 40;
    //    }

    //    _window?.DoCenterOnOwnerWindow(width, height);
    //}

    public double MaxDialogWidth  { get;set; }
   
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
            if(!string.IsNullOrEmpty(value))
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
        get => _window != null ? _window.Title : "";
        set
        {
            if (_window != null)
                _window.Title = value;
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
