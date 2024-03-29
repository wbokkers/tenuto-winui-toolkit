﻿using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Tenuto.WinUI.Toolkit.Windowing;
using WinRT.Interop;

namespace Tenuto.WinUI.ToolkitApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        _savedName = "";

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.SetIcon("Icons/TestIcon.ico");
    }

    private async void OpenModalDialogClick(object sender, RoutedEventArgs e)
    {
        var ownerWindow = this;
      
        // Create the control to show in the dialog
        var myControl = new MyUserControl("Hello Modal Dialog");

        // Show the modal dialog and wait for it to close
        await TnWindow.CreateModalDialog(ownerWindow)
            .WithTitle("Modal Dialog Window")
            .WithSize(360, 360)
            .WithPosition(TnWindowPosition.CenteredOnOwnerWindow)
            .ShowAsync(myControl);
    }

    private string _savedName;
    private async void OpenContentDialogClick(object sender, RoutedEventArgs e)
    {
        // Create a view to show in the dialog
        // In this example we use a view with a property NameInput
        var editView = new EditNameView();
        editView.NameInput = _savedName;

        // Select the owner window. A modal dialog needs an owner window.
        var ownerWindow = this;

        // Create the dialog containing a title, two buttons, and our view 
        // We want our dialog to have modal behavior
        var dialog = new TnContentDialog(ownerWindow, isModal: true)
        {
            Title = "Name input",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Cancel",
            Content = editView
        };

        // Show the dialog and wait for it to close
        var res = await dialog.ShowAsync();

        // When the 'Save' button was clicked, we save the input
        if (res == TnContentDialogResult.PrimaryButtonClicked)
        {
            _savedName = editView.NameInput;
        }
    }

    private void OpenCompactWindowClick(object sender, RoutedEventArgs e)
    {
        // Open a compact window,
        // with no icon and owned by this window.
        // Do not wait for it to close
        TnWindow.CreateCompact()
            .WithoutIcon()
            .WithOwner(this)
            .WithTitle("Compact Window")
            .WithSize(400, 300)
            .WithPosition(TnWindowPosition.BottomRightOfScreen)
            .Show(new MyUserControl("Hello Compact Window"));
    }



    private void OpenWindowWithOwnerClick(object sender, RoutedEventArgs e)
    {
        var ownerWindow = this;

        // Open a default window. Do not wait for it to close
        TnWindow.Create()
            .WithOwner(ownerWindow)
            .WithTitle("Owned Window")
            .WithSize(400, 400)
            .Show(new MyUserControl("Hello Owned Window"));
    }

    private void OpenWindowWithIconClick(object sender, RoutedEventArgs e)
    {
        TnWindow.Create()
            .WithTitle("Window With Icon")
            .WithIcon("Icons/TestIcon.ico")
            .WithAlwaysOnTopBehavior(true)
            .WithSize(600, 540)
            .WithPosition(TnWindowPosition.CenteredOnScreen)
            .Show(new MyUserControl("Hello Window With Icon"));
    }
}