using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;
using TimeTracker.ViewModels;
using System.Threading;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        Mutex mutex;

        public Login()
        {            

            bool aIsNewInstance = false;
            mutex = new Mutex(true, "TimeTracker", out aIsNewInstance);
            if (!aIsNewInstance)
            {
                MessageBox.Show("An instance is already running...");
                System.Windows.Application.Current.Shutdown();
            }
            InitializeComponent();
            this.Loaded += Login_Loaded;
        }

        private void Login_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.userName != string.Empty)
            {
                ((LoginViewModel)this.DataContext).UserName = Properties.Settings.Default.userName;                
            }
            if (Properties.Settings.Default.userPassword != string.Empty)
            {                
                this.txtPassword.Password = Properties.Settings.Default.userPassword;
                this.chkRememberMe.IsChecked = true;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((LoginViewModel)this.DataContext).Password = ((PasswordBox)sender).Password; }
        }
    }
}
