using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using TimeTracker.Models.Communication;
using TimeTracker.ViewModels.Communication;
using TimeTracker.Trace;

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

            // Subscribe to messages collection changes for auto-scroll
            if (viewModel.Messages is INotifyCollectionChanged messagesCollection)
            {
                messagesCollection.CollectionChanged += Messages_CollectionChanged;
            }
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
            // Unsubscribe from events
            if (DataContext is CommunicationViewModel viewModel)
            {
                if (viewModel.Messages is INotifyCollectionChanged messagesCollection)
                {
                    messagesCollection.CollectionChanged -= Messages_CollectionChanged;
                }
                viewModel.Dispose();
            }
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Auto-scroll to bottom when new messages are added
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    MessagesScrollViewer?.ScrollToEnd();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private async void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox listBox && listBox.SelectedItem is ChatUser user)
                {
                    LogManager.Logger.Info($"User selected: {user.FullName} (ID: {user.Id})");

                    // Clear selection immediately to allow re-selection
                    listBox.SelectedItem = null;

                    if (string.IsNullOrEmpty(user.Id))
                    {
                        MessageBox.Show("User ID is empty - cannot create conversation", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (DataContext is CommunicationViewModel viewModel)
                    {
                        LogManager.Logger.Info("Calling StartConversationWithUserAsync...");
                        await viewModel.StartConversationWithUserAsync(user);
                        LogManager.Logger.Info("StartConversationWithUserAsync completed");
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.Logger.Error($"Error in UsersListBox_SelectionChanged: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Keep the old handler as fallback
        private async void UserItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ChatUser user)
            {
                LogManager.Logger.Info($"User clicked (MouseLeftButtonUp): {user.FullName}");
                if (DataContext is CommunicationViewModel viewModel)
                {
                    await viewModel.StartConversationWithUserAsync(user);
                }
            }
        }
    }
}
