using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for ProductiveApplication.xaml
    /// </summary>
    public partial class ProductiveApplicationSampleUICode : Window
    {
        ObservableCollection<Member> members = new ObservableCollection<Member>();
        public ProductiveApplicationSampleUICode()
        {
            InitializeComponent();

            //Create data grid items
            members.Add(new Member {Number=" 1", Character="", BgColor="",Name="",Position="", Email=""});

            membersDatagrid.ItemsSource = members;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton== MouseButton.Left)
            {
                this.DragMove();
            }

        }

        private bool IsMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximized) {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;
                    IsMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    this.Width = 1080;
                    this.Height = 720;
                    IsMaximized = true;

                }
            }

        }
    }

    public class Member    {
        public string Number { get; set; }
        public string Character { get; set; }
        public string BgColor { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
