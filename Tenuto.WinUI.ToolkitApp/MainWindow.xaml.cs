using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using System.Threading.Tasks;
using Tenuto.WinUI.Toolkit.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tenuto.WinUI.ToolkitApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void OpenModalDialogClick(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            LayoutRoot.Background = new SolidColorBrush(Colors.LightGray);

            // Create the control to show in the dialog
            var myControl = new MyUserControl("Hello Modal Dialog");

            // Show the modal dialog and wait for it to close
            await TnWindow.CreateModalDialog(hWnd)
                .WithTitle("Modal Dialog Window")
                .CenteredOnOwnerWindow(360, 360)
                .ShowAsync(myControl);

            LayoutRoot.Background = new SolidColorBrush(Colors.White);

            // Get the input from the dialog
            var nameEntered = myControl.NameEntered;

            Debug.WriteLine("You entered: " + nameEntered);

        }

        private void OpenCompactWindowClick(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
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
                .WithSize(400,400)
                .Show(new MyUserControl("Hello Default Window"));

            window.BringToTop();
        }

        private void OpenWindowWithOwnerClick(object sender, RoutedEventArgs e)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Open a default window. Do not wait for it to close
            var window = TnWindow.Create()
                .OwnedBy(hWnd)
                .WithTitle("Owned Window")
                .WithSize(400, 400)
                .Show(new MyUserControl("Hello Owned Window"));
        }
    }
}
