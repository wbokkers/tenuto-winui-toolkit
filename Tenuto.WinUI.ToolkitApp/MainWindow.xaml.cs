using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using Tenuto.WinUI.Toolkit.Windowing;

namespace Tenuto.WinUI.ToolkitApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        _savedName = "";
    }

    private async void OpenModalDialogClick(object sender, RoutedEventArgs e)
    {
        var ownerWindow = this;

        LayoutRoot.Background = new SolidColorBrush(Colors.LightGray);

        // Create the control to show in the dialog
        var myControl = new MyUserControl("Hello Modal Dialog");

        // Show the modal dialog and wait for it to close
        await TnWindow.CreateModalDialog(ownerWindow)
            .WithTitle("Modal Dialog Window")
            .CenteredOnOwnerWindow(360, 360)
            .ShowAsync(myControl);

        LayoutRoot.Background = new SolidColorBrush(Colors.White);
    }

    private string _savedName;
    private async void OpenContentDialogClick(object sender, RoutedEventArgs e)
    {
        var editView = new EditNameView();
        editView.NameInput = _savedName;

        var ownerWindow = this;

        var dialog = new TnContentDialog(ownerWindow)
        {
            Title = "Name input",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel",
            Content = editView
        };

        var res = await dialog.ShowAsync();

        if (res == TnContentDialogResult.PrimaryButtonClicked)
        {
            _savedName = editView.NameInput;
        }
    }

    private void OpenCompactWindowClick(object sender, RoutedEventArgs e)
    {
        // Open a compact window. Do not wait for it to close
        TnWindow.CreateCompact()
            .WithTitle("Compact Window")
            .BottomRightAlignedOnScreen(400, 300)
            .Show(new MyUserControl("Hello Compact Window"));
    }

    private void OpenDefaultWindowClick(object sender, RoutedEventArgs e)
    {
        // Open a default window. Do not wait for it to close
        var window = TnWindow.Create()
            .WithTitle("Default Window")
            .WithSize(400, 400)
            .Show(new MyUserControl("Hello Default Window"));

        window.BringToTop();
    }

    private void OpenWindowWithOwnerClick(object sender, RoutedEventArgs e)
    {
        var ownerWindow = this;

        // Open a default window. Do not wait for it to close
        TnWindow.Create()
            .OwnedBy(ownerWindow)
            .WithTitle("Owned Window")
            .WithSize(400, 400)
            .Show(new MyUserControl("Hello Owned Window"));
    }

}