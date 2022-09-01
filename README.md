# Tenuto.WinUI.Toolkit

> A toolkit created by Wim Bokkers

Tenuto.WinUI.Toolkit is a Toolkit that can be used for Windows Desktop projects targeting WinUI 3 (as part of Windows App SDK).

Current status: under construction

## Windowing
The Toolkit demonstrates how to work with windows and dialogs in WinUI in a Builder-like way from code. I prefer this approach over using XAML with databinding, just to create a window or dialog hosting your views. Of course you can still use MVVM in your own controls and pages that are hosted in a window or dialog.

To create a modal dialog:

```csharp
// Get the hWnd of the main window that is the owner of the dialog. 
// When in the main window, we can do: 
var hWndMain = WinRT.Interop.WindowNative.GetWindowHandle(this);

// create your control (preferably a UserControl or a Page)
var myControl = new TextBlock { Text = "Hello Modal Dialog"});

// Show the modal dialog and wait for it to close
await TnWindow.CreateModalDialog(hWndMain)
    .WithTitle("Modal Dialog Window")
    .CenteredOnOwnerWindow(360, 360)
    .ShowAsync(myControl);
```

As you can see, you can use the ShowAsync to wait for the dialog to close.

To close a window hosting a control:

``` csharp
TnWindow.CloseWindowWithContent(myControl); 
```
See the examples on how this can be used to close the window from the control that is hosted by the window.


