using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Utilities;
using SystemWindows = System.Windows;

namespace TimeTracker.ViewModels
{
    public class ProductivityAppsSettingsViewModel : ViewModelBase
    {
        #region  private members        
        private IConfiguration configuration;                
        #endregion

        #region constructor
        public ProductivityAppsSettingsViewModel()
        {
            CloseCommand = new RelayCommand<CancelEventArgs>(CloseCommandExecute);
            DeleteApplication = new RelayCommand<string>(DeleteApplicationExecute);
            ReloadProcessesCommand = new RelayCommand(LoadRunningProcesses);
            AddApplication = new RelayCommand<string>(AddApplicationExecute);

            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            this.loadData();
        }
        

        #endregion

        #region Public Properties                

        private ObservableCollection<ProductivityModel> productivityApps = new ObservableCollection<ProductivityModel>();

        public ObservableCollection<ProductivityModel>  ProductivityApps
        {
            get { return productivityApps; }
            set { 
                productivityApps = value;
                OnPropertyChanged(nameof(ProductivityApps));
            }
        }


        private ObservableCollection<ProductivityModel> runningProcesses = new ObservableCollection<ProductivityModel>();

        public ObservableCollection<ProductivityModel> RunningProcesses
        {
            get { return runningProcesses; }
            set
            {
                runningProcesses = value;
                OnPropertyChanged(nameof(RunningProcesses));
            }
        }



        #endregion

        #region commands
        public RelayCommand<CancelEventArgs> CloseCommand { get; set; }
        public RelayCommand<string> DeleteApplication { get; set; }
        public RelayCommand ReloadProcessesCommand{ get; set; }
        public RelayCommand<string> AddApplication{ get; set; }

        #endregion

        #region public methods
        public async void CloseCommandExecute(CancelEventArgs args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // Get the process ID of the current application
                int currentProcessId = Process.GetCurrentProcess().Id;
                // Terminate the process
                Process.GetProcessById(currentProcessId)?.Kill();            }
            else
            {
                // Get the current window using the Application object
                Window window = SystemWindows.Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                // Close the window if found
                window?.Close();
            }
        }
        public async void DeleteApplicationExecute(string id)
        {
            deleteProductivityApp(id);
            getProductivityApps();
        }
        
        #endregion

        #region Private Methods
                

       


       
        private async Task<ObservableCollection<ProductivityModel>>  getProductivityApps()
        {
            var rest = new REST(new HttpProviders());
            var result = await rest.GetProductivityApps($"api/v1/appWebsite/productivity/apps/{GlobalSetting.Instance.LoginResult.data.user.id}");                        
            this.ProductivityApps = new ObservableCollection<ProductivityModel>(result.data);
            return this.ProductivityApps;
        }

        private async void deleteProductivityApp(string id)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)  // error is here
            {
                var rest = new REST(new HttpProviders());
                var result = await rest.DeleteProductivityApp($"/api/v1/appWebsite/productivity/{id}");
                if(result.data!=null)
                {                    
                    this.LoadRunningProcesses();
                }
            }                        
        }
        private async void LoadRunningProcesses()
        {
            this.RunningProcesses.Clear();
            var processList= new ObservableCollection<ProductivityModel>(this.GetRunningApplications());
            foreach (var process in processList)
            {
                var pExists = this.ProductivityApps.Any(x => x.key.ToLower() == process.key.ToLower());
                if (!pExists) {
                    this.RunningProcesses.Add(process);
                }
            }
        }
        private async void AddApplicationExecute(string key)
        {
            var model = this.ProductivityApps.Where(x => x.key == key).FirstOrDefault();
            if (model == null)
            {
                model = this.RunningProcesses.Where(x => x.key == key).FirstOrDefault();
                model.isApproved = false;
                model.isProductive = true;
                model.icon = model.key;
                var rest = new REST(new HttpProviders());
                var result = await rest.AddProductivityApps($"api/v1/appWebsite/productivity", model);
                if (result.data != null)
                {
                    this.LoadRunningProcesses();
                }
            }
            this.loadData();
        }               

        private async void loadData()
        {
            await this.getProductivityApps();            
            this.LoadRunningProcesses();
        }

        public  List<ProductivityModel> GetRunningApplications()
        {
            List<ProductivityModel> runningApplications = new List<ProductivityModel>();

            if (Commons.IsWindows())
            {
                runningApplications.AddRange(GetRunningApplicationsWindows());
            }
            else if (Commons.IsMacOS())
            {
                runningApplications.AddRange(GetRunningApplicationsMacOS());
            }
            else if(Commons.IsLinux())
            {
                throw new NotSupportedException("Unsupported operating system.");
            }
            return runningApplications;
        }

        private  List<ProductivityModel> GetRunningApplicationsWindows()
        {
            List<ProductivityModel> runningApplications = new List<ProductivityModel>();

            foreach (Process process in Process.GetProcesses())
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle) && HasUI(process))
                {
                    runningApplications.Add(new ProductivityModel { key = process.ProcessName, name= process.MainWindowTitle });
                }
            }

            return runningApplications;
        }

        private  List<ProductivityModel> GetRunningApplicationsMacOS()
        {
            List<ProductivityModel> runningApplications = new List<ProductivityModel>();

            Process process = new Process();
            process.StartInfo.FileName = "bash";
            process.StartInfo.Arguments = "-c \"osascript -e 'tell application \"System Events\" to get name of (processes where background only is false) end'\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    //runningApplications.Add(line);
                }
            }
            process.WaitForExit();

            return runningApplications;
        }

        private bool HasUI(Process process)
        {
            // Check if the process has a non-zero MainWindowHandle
            return process.MainWindowHandle != IntPtr.Zero;
        }
    }
    #endregion
}
