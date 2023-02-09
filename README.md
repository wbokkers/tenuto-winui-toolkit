# Tenuto.WinUI.Toolkit

> A toolkit created by Wim Bokkers

Tenuto.WinUI.Toolkit is a Toolkit that can be used for Windows Desktop projects targeting WinUI 3 (as part of Windows App SDK).

Current status: under construction

## Windowing
The Toolkit demonstrates how to work with windows and dialogs in WinUI using a fluent API. I prefer this approach over using XAML with databinding, just to create a window or dialog hosting your views. Of course you can still use MVVM in your own controls and pages that are hosted in a window or dialog.

To create a (modal) dialog window, you can use the TnContentDialog class. This mimics the behavior of the ContentDialog.  
Here's an example on how to use this:

```csharp
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
```

TnContentDialog uses a TnWindow under the hood to show the dialog.

TnWindow extends the Windows class with some nice features.
Here's an example on how to use TnWindow:  

```csharp
TnWindow.Create()
        .WithTitle("Window With Icon")
        .WithIcon("Icons/TestIcon.ico")
        .WithAlwaysOnTopBehavior(true)
        .WithSize(600,540)
        .WithPosition(TnWindowPosition.CenteredOnScreen)
        .Show(new MyUserControl("Hello Window With Icon"));
```

You can show TnWindow with a Show() or ShowAsync method. Awaiting the async method will return after the window closes.

Inn order to close a window hosting a control from code:

``` csharp
TnWindow.CloseWindowWithContent(myControl); 
```
See the examples on how this can be used to close the window from the control that is hosted by the window.


