using System.Windows;
using TimeTracker.ViewModels.Communication;

namespace TimeTracker.Views.Communication
{
    /// <summary>
    /// Interaction logic for CommunicationWindow.xaml
    /// </summary>
    public partial class CommunicationWindow : Window
    {
        public CommunicationWindow()
        {
            InitializeComponent();
            Closed += CommunicationWindow_Closed;
        }

        public CommunicationWindow(CommunicationViewModel viewModel) : this()
        {
            DataContext = viewModel;
            Loaded += CommunicationWindow_Loaded;
        }

        private async void CommunicationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CommunicationViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        private void CommunicationWindow_Closed(object sender, System.EventArgs e)
        {
            if (DataContext is CommunicationViewModel viewModel)
            {
                viewModel.Dispose();
            }
        }
    }
}
