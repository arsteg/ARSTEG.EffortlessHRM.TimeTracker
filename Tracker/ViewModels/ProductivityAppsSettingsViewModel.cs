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
using TimeTracker.Services.Interfaces;
using TimeTracker.Utilities;
using SystemWindows = System.Windows;

namespace TimeTracker.ViewModels
{
    public class ProductivityAppsSettingsViewModel : ViewModelBase
    {
        #region  private members
        private readonly IRestService _restService;
        private IConfiguration configuration;
        #endregion

        #region constructor
        /// <summary>
        /// Creates a new ProductivityAppsSettingsViewModel with the specified REST service.
        /// </summary>
        /// <param name="restService">The REST service for API calls.</param>
        public ProductivityAppsSettingsViewModel(IRestService restService)
        {
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));

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
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Get the process ID of the current application
                    using (var currentProcess = Process.GetCurrentProcess())
                    {
                        int currentProcessId = currentProcess.Id;
                        // Terminate the process
                        Process.GetProcessById(currentProcessId)?.Kill();
                    }
                }
                else
                {
                    // Get the current window using the Application object
                    Window window = SystemWindows.Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                    // Close the window if found
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CloseCommandExecute error: {ex.Message}");
            }
        }
        public async void DeleteApplicationExecute(string id)
        {
            try
            {
                deleteProductivityApp(id);
                await getProductivityApps();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteApplicationExecute error: {ex.Message}");
            }
        }
        
        #endregion

        #region Private Methods
                

       


       
        private async Task<ObservableCollection<ProductivityModel>> getProductivityApps()
        {
            var result = await _restService.GetProductivityApps($"api/v1/appWebsite/productivity/apps/{GlobalSetting.Instance.LoginResult.data.user.id}");
            this.ProductivityApps = new ObservableCollection<ProductivityModel>(result.data);
            return this.ProductivityApps;
        }

        private async void deleteProductivityApp(string id)
        {
            try
            {
                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    var result = await _restService.DeleteProductivityApp($"/api/v1/appWebsite/productivity/{id}");
                    if (result?.data != null)
                    {
                        this.LoadRunningProcesses();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"deleteProductivityApp error: {ex.Message}");
            }
        }
        private async void LoadRunningProcesses()
        {
            try
            {
                this.RunningProcesses.Clear();
                var processList = new ObservableCollection<ProductivityModel>(this.GetRunningApplications());
                foreach (var process in processList)
                {
                    var pExists = this.ProductivityApps.Any(x => x.key.ToLower() == process.key.ToLower());
                    if (!pExists)
                    {
                        this.RunningProcesses.Add(process);
                    }
                }
                this.productivityApps.Clear();
                await this.getProductivityApps();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadRunningProcesses error: {ex.Message}");
            }
        }
        private async void AddApplicationExecute(string key)
        {
            try
            {
                var model = this.ProductivityApps.Where(x => x.key == key).FirstOrDefault();
                if (model == null)
                {
                    model = this.RunningProcesses.Where(x => x.key == key).FirstOrDefault();
                    if (model != null)
                    {
                        model.isApproved = false;
                        model.status = "pending";
                        model.icon = model.key;
                        var result = await _restService.AddProductivityApps($"api/v1/appWebsite/productivity", model);
                        if (result?.data != null)
                        {
                            this.LoadRunningProcesses();
                        }
                    }
                }
                this.loadData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddApplicationExecute error: {ex.Message}");
            }
        }               

        private async void loadData()
        {
            try
            {
                await this.getProductivityApps();
                this.LoadRunningProcesses();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"loadData error: {ex.Message}");
            }
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

        private List<ProductivityModel> GetRunningApplicationsWindows()
        {
            List<ProductivityModel> runningApplications = new List<ProductivityModel>();

            foreach (Process process in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrEmpty(process.MainWindowTitle) && HasUI(process))
                    {
                        runningApplications.Add(new ProductivityModel { key = process.ProcessName, name = process.MainWindowTitle });
                    }
                }
                finally
                {
                    process.Dispose();
                }
            }

            return runningApplications;
        }

        private List<ProductivityModel> GetRunningApplicationsMacOS()
        {
            List<ProductivityModel> runningApplications = new List<ProductivityModel>();

            using (var process = new Process())
            {
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
            }

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
