using Microsoft.UI.Xaml.Controls;
using Tenuto.WinUI.Toolkit.Windowing;

namespace Tenuto.WinUI.ToolkitApp;

public sealed partial class MyUserControl : UserControl
{
    public MyUserControl(string message)
    {
        this.InitializeComponent();
        MessageTb.Text = message;
    }

    private void CloseWindow(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Use a utility method on TnWindow to close the window containing this content
        TnWindow.CloseWindowWithContent(this);
    }
}
