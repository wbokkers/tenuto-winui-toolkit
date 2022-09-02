using Microsoft.UI.Xaml.Controls;

namespace Tenuto.WinUI.ToolkitApp;

public sealed partial class EditNameView : UserControl
{
    public EditNameView()
    {
        this.InitializeComponent();
    }

    public string NameInput
    {
        get => TbName.Text;
        set => TbName.Text = value;
    }
}
