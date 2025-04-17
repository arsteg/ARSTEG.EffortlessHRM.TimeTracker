using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TimeTrackerX.ViewModels;

namespace TimeTrackerX;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}
