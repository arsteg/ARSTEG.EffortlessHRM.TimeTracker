using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SystemWindows = System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using TimeTracker.Trace;
using TimeTracker.ActivityTracker;
using BrowserHistoryGatherer.Data;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Diagnostics;
using TimeTracker.AppUsedTracker;
using System.Management;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading;
using TimeTracker.Views;
using System.Timers;
using System.Dynamic;
using System.Windows.Input;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using GalaSoft.MvvmLight.Messaging;
using System.Net;

namespace TimeTracker.ViewModels
{
    public class TimeTrackerViewModel : ViewModelBase
    {
        #region  private members        
        private IConfiguration configuration;
        private bool trackerIsOn = false;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private DispatcherTimer idlTimeDetectionTimer = new DispatcherTimer();
        private DispatcherTimer saveDispatcherTimer = new DispatcherTimer();
        private DispatcherTimer deleteImagePath = new DispatcherTimer();
        private DispatcherTimer usedAppDetector = new DispatcherTimer();

        //private DispatcherTimer shareLiveScreen = new DispatcherTimer();
        private DispatcherTimer checkForLiveScreen = new DispatcherTimer();
        private System.Threading.Timer sendImageRegularly;
        private double frequencyOfLiveImage = 1000;
        private bool isLiveImageRunning = false;

        private DateTime trackingStartedAt;
        private DateTime trackingStopedAt;
        private TimeSpan timeTrackedSaved;
        private List<TimeLog> timeLogs = new List<TimeLog>();
        private List<TimeLog> unsavedTimeLogs = new List<TimeLog>();
        private List<ApplicationLog> unsavedApplicationLog = new List<ApplicationLog>();

        private bool userIsInactive = false;
        Random rand = new Random();
        private double minutesTracked = 0;
        private int totalMouseClick = 0;
        private int totalKeysPressed = 0;
        private int totalMouseScrolls = 0;
        private string machineId = string.Empty;
        private TimeLog timeLogForSwitchingMachine = new TimeLog();

        MouseHook mh;
        public event EventHandler RequestClose;
        #endregion

        #region constructor
        public TimeTrackerViewModel()
        {
			LogManager.Logger.Info($"timetracker constructor starts");
			CloseCommand = new RelayCommand<CancelEventArgs>(CloseCommandExecute);
            StartStopCommand = new RelayCommand(StartStopCommandExecute);            
            LogoutCommand = new RelayCommand(LogoutCommandExecute);
            RefreshCommand = new RelayCommand(RefreshCommandExecute);
            LogCommand = new RelayCommand(LogCommandExecute);            
            DeleteScreenshotCommand = new RelayCommand(DeleteScreenshotCommandExecute);
            SaveScreenshotCommand = new RelayCommand(SaveScreenshotCommandExecute);
            OpenDashboardCommand = new RelayCommand(OpenDashboardCommandExecute);
            ProductivityApplicationCommand = new RelayCommand(ProductivityApplicationCommandExecute);
            TaskCompleteCommand = new RelayCommand(TaskCompleteCommandExecute);
            CreateNewTaskCommand = new RelayCommand(CreateNewTaskCommandExecute);
			TaskOpenCommand = new RelayCommand(TaskOpenCommandExecute);
            SwitchTrackerNoCommand = new RelayCommand(SwitchTrackerNoCommandExecute);
            SwitchTrackerYesCommand = new RelayCommand(SwitchTrackerYesCommandExecute);

            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            idlTimeDetectionTimer.Tick += IdlTimeDetectionTimer_Tick;
            idlTimeDetectionTimer.Interval = new TimeSpan(00, 2, 00);

            dispatcherTimer.Interval = TimeSpan.FromMinutes(9);
            UserName = GlobalSetting.Instance.LoginResult.data.user.email;
            UserId = GlobalSetting.Instance.LoginResult.data.user.id;

            usedAppDetector.Tick += new EventHandler(UsedAppDetector_Tick);
            usedAppDetector.Interval = TimeSpan.FromMinutes(10);

            #region "live screen"
            //shareLiveScreen.Tick += new EventHandler(ShareLiveScreen_Tick);
            //shareLiveScreen.Interval = TimeSpan.FromMilliseconds(1000);
            //checkForLiveScreen.Tick += new EventHandler(CheckForLiveScreen_Tick);
            //checkForLiveScreen.Interval = new TimeSpan(00, 00, 30);
            //checkForLiveScreen.Start();
            #endregion

            minutesTracked = 0;
            CreateMachineId();

            mh = new MouseHook();
            mh.SetHook();

            mh.MouseClickEvent += mh_MouseClickEvent;
            mh.MouseDownEvent += Mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.MouseWheelEvent += mh_MouseWheelEvent;
            InterceptKeys.OnKeyDown -= InterceptKeys_OnKeyDown;
            InterceptKeys.OnKeyDown += InterceptKeys_OnKeyDown;
            InterceptKeys.Start();
            LogManager.Logger.Info("before initializeUI method");
            initializeUI();
			LogManager.Logger.Info("after initializeUI method");
			LogManager.Logger.Info($"timetracker constructor ends");
		}

        private void initializeUI()
        {
            try
            {
                BindProjectList();
                //ConnectWebSocket();
                DeleteTempFolder();
                populateUserName();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
        }

        private void Mh_MouseDownEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            totalMouseClick++;
        }
        private void mh_MouseUpEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }
        private void mh_MouseClickEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }
        private void mh_MouseWheelEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            totalMouseScrolls++;
        }

        private void InterceptKeys_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            totalKeysPressed++;
            // Check if the key is not a control key (like Shift, Alt, Ctrl)
            if (!e.Control && !e.Alt && !e.Shift && !char.IsControl((char)e.KeyValue))
            {                // Use the KeyCode to get the actual readable key
                string keyText = e.KeyCode.ToString();

                // Handle special cases (e.g., Space, Enter)
                if (keyText.Length == 1)
                {
                    CurrentInput += keyText;
                }
                else if (e.KeyCode == Keys.Space)
                {
                    CurrentInput += " ";
                }
            }
        }       

        private void OnRequestClose()
        {
            RequestClose?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Properties                

        private string title = "";
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }


        private string startStopButtontext = "Start";
        public string StartStopButtontext
        {
            get { return startStopButtontext; }
            set
            {
                startStopButtontext = value;
                OnPropertyChanged(nameof(StartStopButtontext));
            }
        }

        private string currentInput = string.Empty;
        public string CurrentInput
        {
            get { return currentInput; }
            set
            {
                currentInput = value;
                OnPropertyChanged(nameof(CurrentInput));
            }
        }      

        private string currentImagePath;
        public string CurrentImagePath
        {
            get { return currentImagePath; }
            set
            {
                currentImagePath = value;
                OnPropertyChanged(nameof(CurrentImagePath));
            }
        }

        public string DeleteImagePath { get; set; }

        private String currentSessionTimeTracked;
        public string CurrentSessionTimeTracked
        {
            get { return currentSessionTimeTracked; }
            set
            {
                currentSessionTimeTracked = value;
                OnPropertyChanged(nameof(CurrentSessionTimeTracked));
            }
        }

        private String currentDayTimeTracked;
        public string CurrentDayTimeTracked
        {
            get { return currentDayTimeTracked; }
            set
            {
                currentDayTimeTracked = value;
                OnPropertyChanged(nameof(CurrentDayTimeTracked));
            }
        }
        private String currentWeekTimeTracked;
        public string CurrentWeekTimeTracked
        {
            get { return currentWeekTimeTracked; }
            set
            {
                currentWeekTimeTracked = value;
                OnPropertyChanged(nameof(CurrentWeekTimeTracked));
            }
        }
        private String currentMonthTimeTracked;
        public string CurrentMonthTimeTracked
        {
            get { return currentMonthTimeTracked; }
            set
            {
                currentMonthTimeTracked = value;
                OnPropertyChanged(nameof(CurrentMonthTimeTracked));
            }
        }

        private String videoImage;
        public string VideoImage
        {
            get { return videoImage; }
            set
            {
                videoImage = value;
                OnPropertyChanged(nameof(videoImage));
            }
        }

        private String userName;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private String userId;
        public string UserId
        {
            get { return userId; }
            set
            {
                userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        private String projectName;
        public string ProjectName
        {
            get { return projectName; }
            set
            {
                projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }

        private String taskName;
        public string Taskname
        {
            get { return taskName; }
            set
            {
                taskName = value;
                OnPropertyChanged(nameof(Taskname));
            }
        }

		private String taskDescription;
		public string TaskDescription
		{
			get { return taskDescription; }
			set
			{
				taskDescription = value;
				OnPropertyChanged(nameof(TaskDescription));
			}
		}

		private bool canSendReport = true;

        public bool CanSendReport
        {
            get { return canSendReport; }
            set
            {
                canSendReport = value;
                OnPropertyChanged(nameof(CanSendReport));
            }
        }
        private int progressWidthStart = 0;
        public int ProgressWidthStart
        {
            get { return progressWidthStart; }
            set
            {
                progressWidthStart = value;
                OnPropertyChanged(nameof(ProgressWidthStart));
            }
        }

        private int progressWidthReport = 0;
        public int ProgressWidthReport
        {
            get { return progressWidthReport; }
            set
            {
                progressWidthReport = value;
                OnPropertyChanged(nameof(ProgressWidthReport));
            }
        }

        private String countdownTimer;
        public string CountdownTimer
        {
            get { return countdownTimer; }
            set
            {
                countdownTimer = value;
                OnPropertyChanged(nameof(CountdownTimer));
            }
        }

        private bool canShowScreenshot = false;
        public bool CanShowScreenshot
        {
            get { return canShowScreenshot; }
            set
            {
                canShowScreenshot = value;
                OnPropertyChanged(nameof(CanShowScreenshot));
            }
        }

        private List<Project> _projects;
        public List<Project> Projects
        {
            get { return _projects; }
            set
            {
                _projects = value;
                OnPropertyChanged("Projects");
            }
        }
        private string _userFullName;
        public string UserFullName
        {
            get { return _userFullName; }
            set
            {
                _userFullName = value;
                OnPropertyChanged("UserFullName");
            }
        }

        private Project _selectedproject;
        public Project SelectedProject
        {
            get { return _selectedproject; }
            set
            {                
                _selectedproject = value;
                OnPropertyChanged("SelectedProject");
                OnPropertyChanged("AllowTaskSelection");
                if (SelectedProject != null)
                {
                    getTaskList();
                }
            }
        }

        private string _selectedprojectName;
        public string SelectedProjectName
        {
            get { return _selectedprojectName; }
            set
            {                
                _selectedprojectName = value;
                OnPropertyChanged("SelectedProjectName");
            }
        }

        private List<ProjectTask> _tasks;
        public List<ProjectTask> Tasks
        {
            get { return _tasks; }
            set
            {
                _tasks = value;
                OnPropertyChanged("Tasks");
            }
        }

        private ProjectTask _selectedtask;
        public ProjectTask SelectedTask
        {
            get { return _selectedtask; }
            set
            {
                _selectedtask = value;
                TaskDescription = _selectedtask?.description;
				OnPropertyChanged("SelectedTask");
            }
        }

        public bool AllowTaskSelection
        {
            get { return (SelectedProject != null); }
        }

        private double verticalOffset = 0;
        public double VerticalOffset
        {
            get { return verticalOffset; }
            set
            {
                verticalOffset = value;
                OnPropertyChanged(nameof(VerticalOffset));
            }
        }

        private double horizontalOffset = 0;
        public double HorizontalOffset
        {
            get { return horizontalOffset; }
            set
            {
                horizontalOffset = value;
                OnPropertyChanged(nameof(HorizontalOffset));
            }
        }

        private string canShowRefresh = "Visible";
        public string CanShowRefresh
        {
            get { return canShowRefresh; }
            set
            {
                canShowRefresh = value;
                OnPropertyChanged(nameof(CanShowRefresh));
            }
        }

        private string errorMessage = "";
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
        private string messageColor = "red";
        public string MessageColor
        {
            get { return messageColor; }
            set
            {
                messageColor = value;
                OnPropertyChanged(nameof(MessageColor));
            }
        }

        private bool popupForSwitchTracker = false;
        public bool PopupForSwitchTracker
        {
            get { return popupForSwitchTracker; }
            set
            {
                popupForSwitchTracker = value;
                OnPropertyChanged(nameof(PopupForSwitchTracker));
            }
        }

        #endregion

        #region commands
        public RelayCommand<CancelEventArgs> CloseCommand { get; set; }
        public RelayCommand StartStopCommand { get; set; }
        public RelayCommand EODReportsCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand LogCommand { get; set; }        
        public RelayCommand DeleteScreenshotCommand { get; set; }
        public RelayCommand SaveScreenshotCommand { get; set; }
        public RelayCommand OpenDashboardCommand { get; set; }
        public RelayCommand ProductivityApplicationCommand { get; set; }
        public RelayCommand TaskCompleteCommand { get; set; }
        public RelayCommand CreateNewTaskCommand { get; set; }
        public RelayCommand TaskOpenCommand { get; set; }
        public RelayCommand SwitchTrackerYesCommand { get; set; }
        public RelayCommand SwitchTrackerNoCommand { get ; set; }


        ActiveApplicationPropertyThread activeWorker = new ActiveApplicationPropertyThread();

        #endregion

        #region public methods
        public async void CloseCommandExecute(CancelEventArgs args)
        {
            try
            {
                if (args == null)
                {
                    if (trackerIsOn)
                    {
                        ShowErrorMessage("Please stop the tracker before closing the application.");
                        return;
                    }
                    else
                    {
                        checkForUnsavedLog();
                        SystemWindows.Application.Current.Shutdown();
                    }
                }
                else
                {
                    if (trackerIsOn)
                    {
                        ShowErrorMessage("Please stop the tracker before closing the application.");
                        args.Cancel = true;
                    }
                    else
                    {
                        checkForUnsavedLog();
                    }
                }
            }
            catch(Exception ex)
            {
				LogManager.Logger.Error(ex);
			}
        }
        public void LogoutCommandExecute()
        {
            try
            {
                if (trackerIsOn)
                {
                    StartStopCommandExecute();
                }
                if (GlobalSetting.Instance.TimeTracker != null)
                {
                    GlobalSetting.Instance.TimeTracker.Close();
                    //GlobalSetting.Instance.TimeTracker=null;
                }
                if (GlobalSetting.Instance.LoginView == null)
                {
                    GlobalSetting.Instance.LoginView = new TimeTracker.Views.Login(false);
                }                
                GlobalSetting.Instance.LoginView.Show();
                // Close the window.
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
		}
        public async void StartStopCommandExecute()
        {            
            if (string.IsNullOrEmpty(taskName) || taskName.Length == 0)
            {
                ShowErrorMessage("No task selected");
                return;
            }            
            ProgressWidthStart = 30;
            try
            {
                if (trackerIsOn)
                {
                    idlTimeDetectionTimer.Stop();
                    CanSendReport = true;
                }
                else
                {
                    idlTimeDetectionTimer.Start();
                    CanSendReport = false;
                }
                await SetTrackerStatus();
                timeTrackedSaved = await GetCurrrentdateTimeTracked();
                ShowTimeTracked(true);
                ShowCurrentTimeTracked();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
				LogManager.Logger.Error(ex);
		}
            finally
            {
                ProgressWidthStart = 0;
            }
        }        
        
        private static string getFileName(int r, int c, List<string> files)
        {
            var result = "";
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var hh = int.Parse(fileName.Split('-').FirstOrDefault());
                var mm = int.Parse(fileName.Split('-').LastOrDefault());
                if (hh == r && mm <= (c * 10 + 9) && mm >= (c * 10))
                {
                    result = file;
                }
            }
            return result;
        }
        public void RefreshCommandExecute()
        {
            try
            {               
                LogManager.Logger.Info($"refeshcommandexecute starts");
                populateUserName();
                BindProjectList();
                Tasks = null;
                taskName = string.Empty;
                TaskDescription = string.Empty;
                SelectedTask = null;
				LogManager.Logger.Info($"refreshcommand execution ends");
			}
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
		}
        public async void LogCommandExecute()
        {
            try
            {
                var rest = new REST(new HttpProviders());
                var errorLogList = await rest.GetErrorLogs(GlobalSetting.Instance.LoginResult.data.user.id);
                if (errorLogList != null && errorLogList?.status.ToUpper() == "SUCCESS" && errorLogList?.data?.errorLogList?.Count > 0)
                {
                    if (errorLogList.data.errorLogList.Any(e => e.createdOn.Date == DateTime.UtcNow.Date))
                    {
                        var errors = errorLogList.data.errorLogList.Where(e => e.createdOn.Date == DateTime.UtcNow.Date);
                        ShowLog(errors);
                    }
                    else
                    {
                        ShowErrorMessage("Log not found");
                    }
                }
                else
                {
                    ShowErrorMessage("Log not found");
                }
            }
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
		}        
        public void DeleteScreenshotCommandExecute()
        {
            try
            {
				LogManager.Logger.Info($"delete screenshot starts");
				DeleteImagePath = CurrentImagePath;
                CurrentImagePath = null;
                saveDispatcherTimer.Stop();
                CanShowScreenshot = false;

                deleteImagePath = new DispatcherTimer();
                deleteImagePath.Tick += new EventHandler(deleteImagePath_Tick);
                deleteImagePath.Interval = new TimeSpan(0, 1, 0);
                deleteImagePath.Start();
				LogManager.Logger.Info($"delete screenshot ends");
			}
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
		}
        public void SaveScreenshotCommandExecute()
        {
            CanShowScreenshot = false;
        }
        public void OpenDashboardCommandExecute()
        {
            try
            {
                string url = configuration.GetSection("ApplicationBaseUrl").Value + "#/screenshots";
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
		}
        public void ProductivityApplicationCommandExecute()
        {
            try
            {
                GlobalSetting.Instance.ProductivityAppsSettings = new TimeTracker.Views.ProductivityAppsSettings();
                GlobalSetting.Instance.ProductivityAppsSettings.Show();
            }
			catch (Exception ex)
			{
				LogManager.Logger.Error(ex);
			}
		} 
        public async void TaskCompleteCommandExecute()
        {

            if (string.IsNullOrEmpty(taskName) || taskName.Length == 0)
            {
                ShowErrorMessage("No task selected");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                var rest = new REST(new HttpProviders());

                dynamic task = new ExpandoObject();
                task.status = "Done";
                task.project = SelectedProject._id; 
				var result = await rest.CompleteATask(SelectedTask._id, task);
                if (result.data != null)
                {
                    ShowErrorMessage("Task has been marked as completed");
                    if (SelectedProject != null)
                    {                        
                        await getTaskList();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
				LogManager.Logger.Error(ex);			
		}
            finally
            {
                ProgressWidthStart = 0;
            }
        }
        public async void CreateNewTaskCommandExecute()
        {
            if (string.IsNullOrEmpty(taskName) || taskName.Length == 0)
            {
                ShowErrorMessage("Please specify task details");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                CreateNewTask();                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }
        public async void TaskOpenCommandExecute()
        {
            if (string.IsNullOrEmpty(taskName) || taskName.Length == 0)
            {
                ShowErrorMessage("No task is selected");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                var taskUrl = $"{GlobalSetting.portalBaseUrl}#/edit-task?taskId={this.SelectedTask._id}";
				Process.Start(new ProcessStartInfo(taskUrl) { UseShellExecute = true });

			}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }
        public void SwitchTrackerNoCommandExecute()
        {
            PopupForSwitchTracker = false;
            minutesTracked = 0;
            if (trackerIsOn)
            {
                StartStopCommandExecute();
            }
            totalKeysPressed = 0;
            totalMouseClick = 0;
            totalMouseScrolls = 0;
            saveDispatcherTimer.Stop();
            CurrentInput = string.Empty;
        }
        public async void SwitchTrackerYesCommandExecute()
        {
            try
            {
                PopupForSwitchTracker = false;
                minutesTracked = 10;
                timeLogForSwitchingMachine.makeThisDeviceActive = true;
                var rest = new REST(new HttpProviders());
                var result = await rest.AddTimeLog(timeLogForSwitchingMachine);
                totalKeysPressed = 0;
                totalMouseClick = 0;
                totalMouseScrolls = 0;
                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
            }
            catch (Exception ex)
            {
                TempLog($"Catch block for Save time logs (switching the machine) #Local Time {DateTime.Now} #UTC Time {DateTime.UtcNow} #time start {timeLogForSwitchingMachine.startTime} #time end {timeLogForSwitchingMachine.endTime}");
                unsavedTimeLogs.Add(timeLogForSwitchingMachine);
                AddErrorLog("SaveTimeSlot (switching the machine) Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                LogManager.Logger.Info(ex);
            }
            timeLogForSwitchingMachine = new TimeLog();
        }
        #endregion

        #region Private Methods

        private async Task<bool> SetTrackerStatus()
        {
            if (trackerIsOn)
            {
                StartStopButtontext = "Start";
                AddErrorLog("Info", $"stopped at {DateTime.UtcNow}");
                trackingStopedAt = DateTime.UtcNow;
                StopApplicationTracker();
                usedAppDetector.Stop();
            }
            else
            {
                AddErrorLog("Info", $"started at {DateTime.UtcNow}");
                StartStopButtontext = "Stop";
                dispatcherTimer.Start();
                trackingStartedAt = DateTime.UtcNow;
                minutesTracked = 0;
                if (taskName.Length > 0)
                {
                    CreateNewTask();
                }
                activeWorker.activeApplicationInfomationCollector._focusedApplication = new Dictionary<string, FocushedApplicationDetails>();
                StartApplicationTracker();
                usedAppDetector.Start();
            }
            trackerIsOn = !trackerIsOn;
            CanShowRefresh = trackerIsOn ? "Hidden" : "Visible";
            return true;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {                
                if (trackerIsOn)
                {
                    var lastInterval = dispatcherTimer.Interval;
                    var currentMinutes = DateTime.UtcNow.Minute;
                    minutesTracked += 10;
                    var randonTime = (rand.Next(2, 9));
                    double forTimerInterval = ((currentMinutes - (currentMinutes % 10)) + 10 + randonTime) - currentMinutes;
                    dispatcherTimer.Interval = TimeSpan.FromMinutes(forTimerInterval);
                    var filepath = CaptureScreen();
                    CurrentImagePath = filepath;

                    saveDispatcherTimer = new DispatcherTimer();
                    saveDispatcherTimer.Tick += new EventHandler(saveTimeSlot_Tick);
                    saveDispatcherTimer.Interval = new TimeSpan(0, 0, 10);
                    saveDispatcherTimer.Start();
                    var task = Task.Run(async () => await saveBrowserHistory(DateTime.Now.Subtract(lastInterval), DateTime.Now));
                    task.Wait();
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                LogManager.Logger.Error(ex);
            }
        }
        private async void  saveTimeSlot_Tick(object sender, EventArgs e)
        {
            try
            {
                CanShowScreenshot = false;                
                var task = Task.Run(async () => await SaveTimeSlot(CurrentImagePath));
                    task.Wait();
                var statusCode = task?.Result.statusCode; // Retrieve the result
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    StartStopCommand.Execute(null);
                    Messenger.Default.Send(new NotificationMessage("RestoreWindowMethod"));
                    await ShowErrorMessage("Unauthorized access detected. You will be logged out.");
                    LogoutCommand.Execute(null);                    
                }
                totalKeysPressed = 0;
                    totalMouseClick = 0;
                    totalMouseScrolls = 0;
                    ShowTimeTracked(false);
                    ShowCurrentTimeTracked();
                    saveDispatcherTimer.Stop();
                    CurrentInput = string.Empty;
            }                        
            catch (Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
        }
        private string CaptureScreen()
        {
            try
            {
                AddErrorLog("Info", $"screen captured at: {DateTime.UtcNow}");
                currentImagePath = Utilities.TimeManager.CaptureMyScreen();
                var playScreenCaptureSound = Task.Run(async () => await APIService.GetEnableBeepSoundSetting()).Result;
                
                if (playScreenCaptureSound)
                {
                    PlayMedia.PlayScreenCaptureSound();
                }
                CanShowScreenshot = true;
                Countdown(10, TimeSpan.FromSeconds(1), cur => CountdownTimer = "(" + cur + ")".ToString());
                return currentImagePath;
            }
            catch(Exception ex)
            {
				LogManager.Logger.Error(ex);
                throw ex;
			}
        }
        private void IdlTimeDetectionTimer_Tick(object sender, EventArgs e)
        {
            try
            {
				LogManager.Logger.Info($"IdlTimeDetectionTimer_Tick execution starts");
				if (trackerIsOn || userIsInactive)
                {
                    var idleTime = IdleTimeDetector.GetIdleTimeInfo();

                    if (idleTime.IdleTime.TotalMinutes >= 4)
                    {
                        SetTrackerStatus().Wait();
                        CanSendReport = true;
                        userIsInactive = true;
                        dispatcherTimer.IsEnabled = false;
                    }
                    else
                    {
                        if (!trackerIsOn)
                        {
                            SetTrackerStatus().Wait();
                            userIsInactive = false;
                            dispatcherTimer.IsEnabled = true;
                            CanSendReport = false;
                            getCurrentSavedTime();
                        }
                        else if (dispatcherTimer.IsEnabled == false)
                        {
                            CanSendReport = false;
                            dispatcherTimer.IsEnabled = true;
                            getCurrentSavedTime();
                        }
                    }
                }
				LogManager.Logger.Info($"IdlTimeDetectionTimer_Tick execution ends");
			}
            catch(Exception ex)
            {
                LogManager.Logger.Error(ex);
            }
        }
        private void UsedAppDetector_Tick(object sender, EventArgs e)
        {
            StopApplicationTracker();
            StartApplicationTracker();
        }
        private async void getCurrentSavedTime()
        {
            timeTrackedSaved = await GetCurrrentdateTimeTracked();
        }
        private async Task<TimeSpan> GetCurrrentdateTimeTracked()
        {
            var totalTime = new TimeSpan();
            try
            {
                var rest = new REST(new HttpProviders());
                var result = await rest.GetTimeLogs(new TimeLog()
                {
                    user = UserId,
                    date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
                });

                if (result.data != null)
                {
                    foreach (var timeLog in result.data)
                    {
                        var trackedTime = timeLog.endTime.Subtract(timeLog.startTime);
                        totalTime += new TimeSpan(trackedTime.Hours, trackedTime.Minutes, trackedTime.Seconds);
                    }
                }
                return totalTime;
            }
            catch (Exception ex)
            {
				LogManager.Logger.Error(ex);
				return totalTime;
            }
        }

        private async Task<TimeSpan?> GetCurrrentWeekTimeTracked()
        {
            try
            {
                var dayOfWeekToday = (int)DateTime.UtcNow.DayOfWeek;
                var startDateOfWeek = DateTime.Today.AddDays(-1 * (dayOfWeekToday - 1));
                var totalTime = new TimeSpan();
                var rest = new REST(new HttpProviders());
                var result = await rest.GetCurrentWeekTotalTime(new CurrentWeekTotalTime()
                {
                    user = UserId,
                    startDate = new DateTime(startDateOfWeek.Date.Year, startDateOfWeek.Date.Month, startDateOfWeek.Date.Day),
                    endDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Date.Month, DateTime.UtcNow.Date.Day),
                });
                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                return totalTime;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<TimeSpan?> GetCurrrentMonthTimeTracked()
        {
            try
            {
                var totalTime = new TimeSpan();
                var rest = new REST(new HttpProviders());
                var result = await rest.GetCurrentWeekTotalTime(new CurrentWeekTotalTime()
                {
                    user = UserId,
                    startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Date.Month, 1),
                    endDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Date.Month, DateTime.UtcNow.Date.Day),
                });
                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                return totalTime;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private bool CanStartStopCommandExecute()
        {
            return !string.IsNullOrEmpty(taskName) && taskName.Length > 0;
        }
        private void ShowTimeTracked(bool currentSessionSaved = false)
        {
            var sessionTimeTracked = TimeSpan.FromMinutes(minutesTracked);
            CurrentSessionTimeTracked = $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes.ToString("00")} m";
            if (currentSessionSaved)
            {
                TimeSpan totalTimeTracked = timeTrackedSaved;
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
            else
            {
                TimeSpan totalTimeTracked = timeTrackedSaved + TimeSpan.FromMinutes(minutesTracked);// (timeTrackedSaved + sessionTimeTracked);
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
        }

        private async void ShowCurrentTimeTracked()
        {
            var currentWeekTimeTracked = await GetCurrrentWeekTimeTracked();
            if (currentWeekTimeTracked != null)
            {
                CurrentWeekTimeTracked = $" {(currentWeekTimeTracked?.Days * 24) + currentWeekTimeTracked?.Hours} hrs {currentWeekTimeTracked?.Minutes.ToString("00")} m";
            }

            var currentMonthTimeTracked = await GetCurrrentMonthTimeTracked();
            if (currentMonthTimeTracked != null)
            {
                CurrentMonthTimeTracked = $"{(currentMonthTimeTracked?.Days * 24) + currentMonthTimeTracked?.Hours} hrs {currentMonthTimeTracked?.Minutes.ToString("00")} m";
            }
        }

        private async Task<(TimeLog timeLog, HttpStatusCode statusCode)> SaveTimeSlot(string filePath)
        {
            Byte[] bytes = File.ReadAllBytes(filePath);
            String file = Convert.ToBase64String(bytes);
            var fileName = Path.GetFileName(filePath);
            var currentDate = DateTime.UtcNow;
            var timeLog = new TimeLog()
            {
                user = UserId,
                date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day),
                task = SelectedTask._id,
                startTime = DateTime.UtcNow.AddMinutes(-10),
                endTime = DateTime.UtcNow,
                fileString = file,
                filePath = fileName,
                keysPressed = totalKeysPressed,
                allKeysPressed= CurrentInput,
                clicks = totalMouseClick,
                scrolls = totalMouseScrolls,
                project = SelectedProject._id,
                machineId = machineId,
                makeThisDeviceActive = false
            };
            try
            {
                if (!CheckInternetConnectivity.IsConnectedToInternet())
                {
                    LogManager.Logger.Info($"No internet #Local Time {DateTime.Now} #UTC {DateTime.UtcNow} #Start time {timeLog.startTime} #End Time {timeLog.endTime}");
                    TempLog($"No internet #Local Time {DateTime.Now} #UTC {DateTime.UtcNow} #Start time {timeLog.startTime} #End Time {timeLog.endTime}");
                    unsavedTimeLogs.Add(timeLog);
                    ShowErrorMessage("Please check your internet connectivity.");
                    return (null, HttpStatusCode.OK);
                }
                var rest = new REST(new HttpProviders());
                var result = rest.AddTimeLog(timeLog).GetAwaiter().GetResult();
                if (result.statusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (null, HttpStatusCode.Unauthorized);
                }
                AddErrorLog("Info", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                LogManager.Logger.Info($"machineId {machineId} Message {result?.data?.message ?? ""}");
                if (!string.IsNullOrEmpty(result.data.message))
                {
                    if (result.data.message.Contains("User is logged in on another device, Do you want to make it active?"))
                    {
                        PopupForSwitchTracker = true;
                        timeLogForSwitchingMachine = timeLog;
                        minutesTracked = 0;
                        return (null,HttpStatusCode.OK);

                        #region"not in use"
                        //var isConfirm = MessageBox.Show(new Form() { TopMost = true }, $"{result.data.message}", "Confirmation", MessageBoxButtons.YesNo);
                        //if (isConfirm == DialogResult.Yes)
                        //{
                        //    timeLog.makeThisDeviceActive = true;
                        //    result = await rest.AddTimeLog(timeLog);
                        //}
                        //else
                        //{
                        //    if (trackerIsOn)
                        //    {
                        //        StartStopCommandExecute();
                        //    }
                        //}
                        #endregion
                    }
                }
                else if(unsavedTimeLogs.Count > 0)
                {
                    timeLog = null;
                    var tempUnsavedTimeLogs = unsavedTimeLogs;
                    foreach (var unsavedTimeLog in tempUnsavedTimeLogs.ToArray())
                    {
                        TempLog($"Save unsaved time #logs start Time {unsavedTimeLog.startTime} #log end Time {unsavedTimeLog.endTime}");
                        AddErrorLog("Info", $"Save unsaved log from SaveTimeSlot");
                        LogManager.Logger.Info($"Save unsaved time #logs start Time {unsavedTimeLog.startTime} #log end Time {unsavedTimeLog.endTime}");
                        var unsavedLogResult = await rest.AddTimeLog(unsavedTimeLog);
                        unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                    tempUnsavedTimeLogs = null;
                }
                return  (result?.data, result?.statusCode ?? HttpStatusCode.InternalServerError); ;
            }            
            catch (Exception ex)
            {
                TempLog($"Catch block for Save time logs #Local Time {DateTime.Now} #UTC Time {DateTime.UtcNow} #time start {timeLog.startTime} #time end {timeLog.endTime}");
                if (timeLog != null)
                {
                    unsavedTimeLogs.Add(timeLog);
                }
                AddErrorLog("SaveTimeSlot Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
				LogManager.Logger.Error("SaveTimeSlot Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                throw;
            }
        }

        private async Task getTaskList()
        {
            ProgressWidthStart = 30;
            try
            {
                Tasks = null;
                if (SelectedProject != null && SelectedProject._id.Length > 0)
                {
                    var rest = new REST(new HttpProviders());
                    var taskList = await rest.GetTaskListByProject(new TaskRequest()
                    {
                        projectId = SelectedProject._id,
                        userId = GlobalSetting.Instance.LoginResult.data.user.id,
                        skip="",
                        next=""
                    });

                    if (taskList.status == "success" && taskList.taskList != null)
                    {
                        var projectTaskList = new List<ProjectTask>();
                        taskList.taskList.ForEach(t =>
                        {
                            if (t.status.ToLower() != "closed" && t.status.ToLower() != "done")
                                projectTaskList.Add(new ProjectTask()
                                {
                                    taskName = t.taskName,
                                    description = t.description,
                                    _id = t.id
                                });
                        });
                        Tasks = projectTaskList;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally {
                ProgressWidthStart = 0;
            }
        }

        private void Countdown(int count, TimeSpan interval, Action<int> ts)
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = interval;
            dispatcherTimer.Tick += (_, a) =>
            {
                if (count-- == 1)
                {
                    CanShowScreenshot = false;
                    dispatcherTimer.Stop();
                }
                else
                {
                    ts(count);
                }
            };
            ts(count);
            dispatcherTimer.Start();
        }

        private void deleteImagePath_Tick(object sender, EventArgs e)
        {
            if (File.Exists(DeleteImagePath))
            {
                try
                {
                    File.Delete(DeleteImagePath);
                    DeleteImagePath = "";
                    deleteImagePath.Stop();
                }
                catch
                {
                    deleteImagePath.Stop();
                }
            }
            else
            {
                deleteImagePath.Stop();
            }
        }

        private void AddErrorLog(string error, string details)
        {
            try
            {
                //var rest = new REST(new HttpProviders());
                //rest.AddErrorLogs(new ErrorLog()
                //{
                //    error = error,
                //    details = details
                //});
            }
            catch (Exception ex)
            {

            }
        }

        private async void BindProjectList()
        {
            Projects = null;
            var rest = new REST(new HttpProviders());
            var projectList = await rest.GetProjectListByUserId(new ProjectRequest
            {
                userId = GlobalSetting.Instance.LoginResult.data.user.id
            });

            if (projectList.status == "success" && projectList.data != null)
            {
                Projects = projectList.data.projectList;
            }
        }

        private async void populateUserName() {
            UserFullName = $"Welcome, {GlobalSetting.Instance.LoginResult.data.user.firstName} {GlobalSetting.Instance.LoginResult.data.user.lastName}.";             
        }

        private async void CreateNewTask()
        {
            try
            {
                var taskUsers = new string[] { GlobalSetting.Instance.LoginResult.data.user.id };
                var rest = new REST(new HttpProviders());
                var newTaskResult = await rest.AddNewTask(new CreateTaskRequest
                {
                    taskName = taskName,
                    comment = "Created by TimeTracker",
                    project = SelectedProject._id,
                    taskUsers = taskUsers,
                    user = GlobalSetting.Instance.LoginResult.data.user.id,
                    description = TaskDescription,
                    endDate = null,
                    priority = null,
                    startDate = DateTime.UtcNow,
                    startTime = DateTime.UtcNow,
                    taskAttachments = null,
                    title = "Task",
                    status = "In Progress"
                });
                if (newTaskResult?.status.ToUpper() == "SUCCESS")
                {
                    ShowInformationMessage("Task has been created");
                    SelectedTask = newTaskResult.data.newTask;
                }
            }
            catch (Exception ex) {
                LogManager.Logger.Error($"Error in CreateNewTask(): {ex}");
			}
        }

        private void ShowLog(IEnumerable<ErrorLog> errorLogList)
        {
            string tempPath = Path.GetTempPath();
            string path = Path.Combine(tempPath, $"{DateTime.Today.ToString("yyyyMMdd")}.log");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            try
            {
                StreamWriter sw;
                if (!File.Exists(path))
                {
                    sw = File.CreateText(path);
                }
                else
                {
                    sw = File.AppendText(path);
                }
                foreach (var errorLog in errorLogList)
                {
                    sw.WriteLine("{0}", $"{errorLog.createdOn.ToLocalTime()} {errorLog.error} {errorLog.details}");
                }
                sw.Flush();
                sw.Close();

                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AddErrorLog("Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                MessageBox.Show(ex.Message);
				LogManager.Logger.Error(ex);
			}
        }

        private void TempStoreApplicationUsed(Dictionary<string, FocushedApplicationDetails> ne)
        {
            string tempPath = Path.GetTempPath();
            string path = Path.Combine(tempPath, $"{DateTime.Now.ToString("ddMMyyyyHHmmss")}.log");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            try
            {
                StreamWriter sw;
                if (!File.Exists(path))
                {
                    sw = File.CreateText(path);
                }
                else
                {
                    sw = File.AppendText(path);
                }
                double totalMilisecons = 0;
                double totalIdleTime = 0;
                double totalmous = 0;
                double totalmousesc = 0;
                double totalKeybo = 0;
                foreach (var j in ne.Keys)
                {
                    totalMilisecons += ne[j].Duration;
                    totalIdleTime += ne[j].TotalIdletime;
                    totalmous += ne[j].TotalMouseClick;
                    totalmousesc += ne[j].TotalMouseScrolls;
                    totalKeybo += ne[j].TotalKeysPressed;
                    sw.WriteLine("{0}", $"{j} ## {ne[j].AppTitle} ## {ne[j].Duration} # Keycount # {ne[j].TotalKeysPressed} # Mouse Count # {ne[j].TotalMouseClick} # Scroll Count # {ne[j].TotalMouseScrolls} # total Inactive # {ne[j].TotalIdletime}");
                }
                sw.WriteLine("{0}", $" totalMilisecons {totalMilisecons} # totalIdleTime {totalIdleTime} # mouse {totalmous} # mousescro {totalmousesc} # keyboard {totalKeybo}");
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                AddErrorLog("Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                MessageBox.Show(ex.Message);
            }
        }

        private void StartApplicationTracker()
        {
            System.Threading.Thread activeThread = new System.Threading.Thread(activeWorker.StartThread);
            activeWorker.StopThread(true);
            activeThread.Start();
        }

        private async void StopApplicationTracker()
        {
            try {
                activeWorker.StopThread(false);
                var focusedApplication = activeWorker.activeApplicationInfomationCollector._focusedApplication;

                if (focusedApplication != null)
                {
                    // Create a snapshot of the keys to avoid modifying the collection during iteration
                    foreach (var key in focusedApplication.Keys)
                    {
                        //await AddUsedApplicationLog(new ApplicationLog
                        //{
                        //    appWebsite = key,
                        //    type = "App",
                        //    ApplicationTitle = focusedApplication[key].AppTitle,
                        //    projectReference = SelectedProject._id,
                        //    userReference = GlobalSetting.Instance.LoginResult.data.user.id,
                        //    date = DateTime.UtcNow,
                        //    inactive = focusedApplication[key].TotalIdletime,
                        //    keyboardStrokes = focusedApplication[key].TotalKeysPressed,
                        //    mouseClicks = focusedApplication[key].TotalMouseClick,
                        //    scrollingNumber = focusedApplication[key].TotalMouseScrolls,
                        //    ModuleName = SelectedProject.projectName,
                        //    TimeSpent = focusedApplication[key].Duration,
                        //    total = focusedApplication[key].Duration
                        //});
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }            
        }


        private async Task AddUsedApplicationLog(ApplicationLog applicationLog)
        {
            try
            {
                var rest = new REST(new HttpProviders());
                var usedApp = await rest.AddUsedApplicationLog(applicationLog);
                applicationLog = null;

                if (unsavedApplicationLog.Count > 0)
                {
                    var tempApplicationLog = unsavedApplicationLog;
                    foreach(var app in tempApplicationLog.ToArray())
                    {
                        TempLog($"Save unsaved app and website used # {app.ModuleName}");
                        AddErrorLog("Info", $"Save unsaved app and website used");
                        await rest.AddUsedApplicationLog(app);
                        unsavedApplicationLog.Remove(app);
                    }
                    tempApplicationLog = null;
                }
            }
            catch (Exception ex)
            {
                if (applicationLog != null)
                {
                    unsavedApplicationLog.Add(applicationLog);
                }
                AddErrorLog("Error AddUsedApplicationLog", $"Message: {ex?.Message} ex.StackTrace:{ex?.StackTrace} InnerException: {ex?.InnerException?.InnerException}");
            }
        }

        private void CreateMachineId()
        {
            machineId = string.Empty;
            var win32Processor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var win32BaseBoard = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            //var win32DiskDrive = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (var processor in win32Processor.Get())
            {
                if (!string.IsNullOrEmpty(Convert.ToString(processor["ProcessorId"])))
                {
                    machineId = processor["ProcessorId"].ToString();
                    break;
                }
            }

            foreach (var baseboard in win32BaseBoard.Get())
            {
                if (!string.IsNullOrEmpty(Convert.ToString(baseboard.GetPropertyValue("SerialNumber"))))
                {
                    machineId += baseboard.GetPropertyValue("SerialNumber").ToString();
                    break;
                }
            }
            //foreach (var baseboard in win32DiskDrive.Get())
            //{
            //    if (!string.IsNullOrEmpty(Convert.ToString(baseboard.GetPropertyValue("SerialNumber"))))
            //    {
            //        machineId += baseboard.GetPropertyValue("SerialNumber").ToString();
            //        break;
            //    }
            //}
        }

        private async Task checkForUnsavedLog()
        {
            LogManager.Logger.Info($"checkForUnsavedLog() called on close");
            if (unsavedTimeLogs?.Count > 0)
            {
                var isConfirm = MessageBox.Show($"There are some logs saved locally. Do you want to save this to the server before closing the application? Otherwise, the data will be lost.", "Confirmation", MessageBoxButtons.YesNo);
                if (isConfirm == DialogResult.Yes)
                {
                    if (!CheckInternetConnectivity.IsConnectedToInternet())
                    {
                        ShowErrorMessage("This needs an active internet connection");
                        return;
                    }
                    var rest = new REST(new HttpProviders());

                    var tempUnsavedTimeLogs = unsavedTimeLogs;
                    foreach (var unsavedTimeLog in tempUnsavedTimeLogs.ToArray())
                    {
                        var result = await rest.AddTimeLog(unsavedTimeLog);
                        AddErrorLog("Info saved from checkForUnsavedLog()", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                        LogManager.Logger.Info("Info saved from checkForUnsavedLog() after save time log", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                        if (!string.IsNullOrEmpty(result?.data?.message))
                        {
                            if (result.data.message.Contains("User is logged in on another device, Do you want to make it active?"))
                            {
                                if (MessageBox.Show(new Form() { TopMost = true }, $"{result.data.message}", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    unsavedTimeLog.makeThisDeviceActive = true;
                                    LogManager.Logger.Info("Info saved from checkForUnsavedLog() after save time log for other system", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                                    result = await rest.AddTimeLog(unsavedTimeLog);
                                }
                            }
                        }
                        unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                    SystemWindows.Application.Current.Shutdown();

                }
                else
                {
                    SystemWindows.Application.Current.Shutdown();

                }
            }
        }

        #region "Live screen functions"
        private async void ShareLiveScreen_Tick(object sender, EventArgs e)
        {
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenshot.Size);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                var rest = new REST(new HttpProviders());

                screenshot.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                await rest.sendLiveScreenDataV1(new LiveImageRequest
                {
                    fileString = Convert.ToBase64String(stream.ToArray())
                });
            }
        }

        private async Task SendLiveImage()
        {
            try
            {
                Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, screenshot.Size);
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    var rest = new REST(new HttpProviders());

                    screenshot.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                    await rest.sendLiveScreenDataV1(new LiveImageRequest
                    {
                        fileString = Convert.ToBase64String(stream.ToArray())
                    });
                }
            }
            catch (Exception ex)
            {
                TempLog($"live screen error SendLiveImage: {ex.Message}");
                LogManager.Logger.Error($"live screen error SendLiveImage : {ex.Message}");
            }
        }

        private async void CheckForLiveScreen_Tick(object sender, EventArgs e)
        {
            try
            {
                var rest = new REST(new HttpProviders());
                var response = await rest.checkLiveScreen(new TaskUser
                {
                    user = UserId
                });

                if (response != null)
                {
                    if (response.Success)
                    {
                        if (!isLiveImageRunning)
                        {
                            isLiveImageRunning = true;
                            sendImageRegularly = new System.Threading.Timer(async (_) => await SendLiveImage(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(frequencyOfLiveImage));

                            //isLiveImageRunning = true;
                            //System.Windows.Forms.Application.Current.Dispatcher.Invoke(async () =>
                            //{
                            //    sendImageRegularly = new System.Threading.Timer(async (_) => await SendLiveImage(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(frequencyOfLiveImage));
                            //});

                            //Dispatcher.CurrentDispatcher.Invoke(() =>
                            //{
                            //    sendImageRegularly = new System.Threading.Timer(async (_) =>
                            //    {
                            //        await Dispatcher.CurrentDispatcher.InvokeAsync(async () => await SendLiveImage());
                            //    }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(frequencyOfLiveImage));
                            //});
                        }
                    }
                    else
                    {
                        isLiveImageRunning = false;
                        if (sendImageRegularly != null)
                        {
                            sendImageRegularly.Dispose();
                        }
                    }
                }
                else
                {
                    isLiveImageRunning = false;
                    if (sendImageRegularly != null)
                    {
                        sendImageRegularly.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                TempLog($"live screen error CheckForLiveScreen_Tick : {ex.Message}");
                LogManager.Logger.Error($"live screen error CheckForLiveScreen_Tick : {ex.Message}");
            }
        }
        #endregion

        private void DeleteTempFolder()
        {
            TimeManager.ClearScreenshotsFolder();
        }

        private async Task saveBrowserHistory(DateTime startDate, DateTime endDate)
        {
            try
            {
                var browserHistoryList = BrowserHistory.GetHistoryEntries(startDate, endDate);
                if (browserHistoryList.Count > 0)
                {
                    var rest = new REST(new HttpProviders());
                    foreach (var browserHistory in browserHistoryList)
                    {
                        var result = await rest.AddBrowserHistory(browserHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error while saveBrowserHistory(): {ex}");
			}
        }
        #endregion

        #region "Web socket"

        private readonly ClientWebSocket webSocket = new ClientWebSocket();

        private async Task ConnectWebSocket()
        {
            try
            {
                //var url = $"wss://effortlesshrmapi.azurewebsites.net:4000/{userId}";
                //if (await Commons.CheckUrlAccessibility(url))
                //{
                //    //await webSocket.ConnectAsync(new Uri("ws://localhost:8081/63f846e32ff78af44d597cbc"), CancellationToken.None);
                //    //await webSocket.ConnectAsync(new Uri("ws://localhost:8081/62dfa8d13babb9ac2072863c"), CancellationToken.None);
                //    await webSocket.ConnectAsync(new Uri(url), CancellationToken.None);

                //    // Start listening for incoming WebSocket messages
                //    ListenForWebSocketMessages();
                //}
            }
            catch (Exception ex)
            {

            }
        }

        private async void ListenForWebSocketMessages()
        {
            try
            {
                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        //break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Parse incoming message to determine event to trigger
                        var eventData = JsonConvert.DeserializeObject<EventData>(Encoding.UTF8.GetString(buffer, 0, result.Count));

                        Thread thread = new Thread(() =>
                        {
                            //DispatcherTimerThread(shareLiveScreen);
                        });

                        if (eventData.EventName == "startlivepreview" && eventData.UserId == UserId)
                        {
                            thread.Start();
                            //shareLiveScreen.Start();
                        }
                        else
                        {
                            //Dispatcher.ExitAllFrames();
                            //shareLiveScreen.Stop();
                        }
                    }
                }
            }
            catch (Exception ex) {
				LogManager.Logger.Error(ex);
			}
        }

        private static void DispatcherTimerThread(DispatcherTimer shareLiveScreen)
        {
            // Start the timer
            shareLiveScreen.Start();

            // Keep the thread alive so that the timer can continue executing
            Dispatcher.Run();
        }        
        private async Task ShowErrorMessage(string errorMessage)
        {
            MessageColor = "red";
            await Task.Delay(TimeSpan.FromSeconds(1));
            // Show the error message in the label
            ErrorMessage = errorMessage;

            // Wait for 10 seconds using Task.Delay without blocking the UI thread
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Clear the error message after 10 seconds
            ErrorMessage = string.Empty;
        }

        private async void ShowInformationMessage(string errorMessage)
        {
            MessageColor = "green";
            await Task.Delay(TimeSpan.FromSeconds(1));
            // Show the error message in the label
            ErrorMessage = errorMessage;

            // Wait for 10 seconds using Task.Delay without blocking the UI thread
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Clear the error message after 10 seconds
            ErrorMessage = string.Empty;
        }

        #endregion
        #region "Save log into Temp"
        private void TempLog(string message)
        {
            string tempPath = Path.GetTempPath();
            //string path = Path.Combine(tempPath, $"trackerlog_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.log");
            string path = Path.Combine(tempPath, $"trackerlog_{DateTime.Now.ToString("ddMMyyyy")}.log");
            try
            {
                StreamWriter sw;
                if (!File.Exists(path))
                {
                    sw = File.CreateText(path);
                }
                else
                {
                    sw = File.AppendText(path);
                }

                sw.WriteLine($"{DateTime.Now.ToString()} Info : {message}");
                
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                AddErrorLog("Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                MessageBox.Show(ex.Message);
				LogManager.Logger.Error(ex);
			}
        }       
        #endregion
    }
}