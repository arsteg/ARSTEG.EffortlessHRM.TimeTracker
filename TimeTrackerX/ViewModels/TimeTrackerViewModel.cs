using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using SharpHook;
using SharpHook.Native;
using TimeTrackerX.Models;
using TimeTrackerX.Services;
using TimeTrackerX.Services.Interfaces;
using TimeTrackerX.Utilities;
using Timer = System.Timers.Timer;
using SharpHook.Data;
using NLog;

namespace TimeTrackerX.ViewModels
{
    public partial class TimeTrackerViewModel : ViewModelBase
    {
        #region Private Members
        private readonly IConfiguration _configuration;
        private readonly IScreenshotService _screenshotService;
        private readonly IMouseEventService _mouseEventService;
        private readonly IKeyEventService _keyEventService;
        private readonly REST _restService;
        private readonly INotificationService _notificationService;

        private bool _trackerIsOn;

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private DispatcherTimer idlTimeDetectionTimer = new DispatcherTimer();
        private DispatcherTimer saveDispatcherTimer = new DispatcherTimer();
        private DispatcherTimer deleteImagePath = new DispatcherTimer();
        private DispatcherTimer usedAppDetector = new DispatcherTimer();

        private DispatcherTimer shareLiveScreen = new DispatcherTimer();
        private DispatcherTimer checkForLiveScreen = new DispatcherTimer();

        private System.Threading.Timer _sendImageRegularly;
        private double _frequencyOfLiveImage = 1000;
        private bool _isLiveImageRunning;

        private DateTime _trackingStartedAt;
        private DateTime _trackingStoppedAt;
        private TimeSpan _timeTrackedSaved;
        private readonly List<TimeLog> _timeLogs = new List<TimeLog>();
        private readonly List<TimeLog> _unsavedTimeLogs = new List<TimeLog>();
        private readonly List<ApplicationLog> _unsavedApplicationLogs = new List<ApplicationLog>();

        private bool _userIsInactive;
        private readonly Random _rand = new Random();
        private double _minutesTracked;
        private int _totalMouseClicks;
        private int _totalKeysPressed;
        private int _totalMouseScrolls;
        private DateTime _lastInputTime;
        private string _machineId = string.Empty;
        private TimeLog _timeLogForSwitchingMachine = new TimeLog();
        SimpleGlobalHook hook = new SimpleGlobalHook();
        private static Logger logger;
        #endregion

        #region Constructor
        public TimeTrackerViewModel(
            //IConfiguration configuration,
            IScreenshotService screenshotService
        //IMouseEventService mouseEventService,
        //IKeyEventService keyEventService,
        //REST restService,
        //INotificationService notificationService
        )
        {
            //_configuration = configuration;
            _screenshotService = screenshotService;
            //_mouseEventService = mouseEventService;
            //_keyEventService = keyEventService;
            //_restService = restService;
            //_notificationService = notificationService;
            //UserName = GlobalSetting.Instance.LoginResult.data.user.email;
            UserId = GlobalSetting.Instance.LoginResult.data.user.id;
            logger = LogManager.GetCurrentClassLogger();
            this._machineId = new Machine().CreateMachineId();
            logger.Info($"TimeTrackerViewModel initialised with user Id: {UserId} and machine Id: {this._machineId}");
            _restService = new REST(new HttpProviders());
            InitializeCommands();
            InitializeTimers();
            InitializeInputHooks();
            InitializeUI();
            
            _tasks = new ObservableCollection<ProjectTask>();
        }

        private void Hook_HookDisabled(object? sender, HookEventArgs e)
        {
        }

        private void Hook_HookEnabled(object? sender, HookEventArgs e)
        {
        }

        private void Hook_MouseWheel(object? sender, MouseWheelHookEventArgs e)
        {
            IdleTimeDetector.UpdateLastInputTime();
            _totalMouseScrolls++;
        }

        private void Hook_MouseClicked(object? sender, MouseHookEventArgs e)
        {
            IdleTimeDetector.UpdateLastInputTime();
            
            _totalMouseClicks++;
        }

        private void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            IdleTimeDetector.UpdateLastInputTime();
            _totalKeysPressed++;
            // Check if the key is not a control key (like Shift, Alt, Ctrl)
            if (e.Data.KeyCode != KeyCode.VcLeftAlt && e.Data.KeyCode != KeyCode.VcRightAlt && e.Data.KeyCode != KeyCode.VcLeftShift && e.Data.KeyCode != KeyCode.VcRightShift)
            { // Use the KeyCode to get the actual readable key
                string keyText = e.Data.KeyCode.ToString().Substring(2);

                // Handle special cases (e.g., Space, Enter)
                if (keyText.Length == 1)
                {
                    CurrentInput += keyText;
                }
                else if (e.Data.KeyCode == KeyCode.VcSpace)
                {
                    CurrentInput += " ";
                }
            }
        }


        private async void IdlTimeDetectionTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                logger.Trace("Idle time detection timer tick");

                if (_trackerIsOn || _userIsInactive)
                {
                    TimeSpan idleTime = IdleTimeDetector.GetIdleTime();
                    logger.Debug($"Current idle time: {idleTime.TotalMinutes} minutes");

                    if (idleTime.TotalMinutes >= 4)
                    {
                        logger.Info("User inactive for more than 4 minutes, stopping tracker");
                        await SetTrackerStatus();
                        CanSendReport = true;
                        _userIsInactive = true;
                        dispatcherTimer.IsEnabled = false;
                        logger.Info("Tracker stopped due to inactivity");
                    }
                    else
                    {
                        logger.Debug("User activity detected");
                        if (!_trackerIsOn)
                        {
                            logger.Info("User became active, starting tracker");
                            await SetTrackerStatus();
                            _userIsInactive = false;
                            dispatcherTimer.IsEnabled = true;
                            CanSendReport = false;
                            await GetCurrentSavedTime();
                            logger.Info("Tracker started after inactivity");
                        }
                        else if (dispatcherTimer.IsEnabled == false)
                        {
                            logger.Debug("Resuming tracker after activity");
                            CanSendReport = false;
                            dispatcherTimer.IsEnabled = true;
                            await GetCurrentSavedTime();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in idle time detection timer");
            }
        }


        private async void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                logger.Trace("Dispatcher timer tick");
                if (_trackerIsOn)
                {
                    var lastInterval = dispatcherTimer.Interval;
                    var currentMinutes = DateTime.UtcNow.Minute;
                    _minutesTracked += 10;
                    logger.Debug($"Minutes tracked updated to: {_minutesTracked}");

                    var randomTime = _rand.Next(2, 9);
                    double forTimerInterval =
                        ((currentMinutes - (currentMinutes % 10)) + 10 + randomTime)
                        - currentMinutes;
                    dispatcherTimer.Interval = TimeSpan.FromMinutes(forTimerInterval);
                    logger.Debug($"Next timer interval set to: {forTimerInterval} minutes");

                    var filepath = await CaptureScreen();
                    ScreenshotImage = new Bitmap(filepath);
                    logger.Info($"Screenshot captured and loaded: {filepath}");

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CanShowScreenshot = true;
                    });

                    saveDispatcherTimer = new DispatcherTimer();
                    saveDispatcherTimer.Interval = TimeSpan.FromSeconds(10);
                    saveDispatcherTimer.Tick += SaveDispatcherTimer_Tick;
                    saveDispatcherTimer.Start();
                    logger.Debug("Save dispatcher timer started");

                    await saveBrowserHistory(DateTime.Now.Subtract(lastInterval), DateTime.Now);

                    #region "Check weekly/monthly time limit"
                    if (WeeklyHoursLimit > 0)
                    {
                        if (CurrentWeekCompletedHours >= WeeklyHoursLimit)
                        {
                            IsWeeklyHoursCompleted = true;
                            StartStopCommandExecute();
                        }
                        else
                        {
                            IsWeeklyHoursCompleted = false;
                        }
                    }

                    if (!IsWeeklyHoursCompleted && MonthlyHoursLimit > 0)
                    {

                        if (CurrentMonthCompletedHours >= MonthlyHoursLimit)
                        {
                            IsMonthlyHoursCompleted = true;
                            StartStopCommandExecute();
                        }
                        else
                        {
                            IsMonthlyHoursCompleted = false;
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in dispatcher timer");
                TempLog($"DispatcherTimer error: {ex.Message}");
            }
        }

        private async void SaveDispatcherTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                logger.Trace("Save dispatcher timer tick");
                CanShowScreenshot = false;
                logger.Debug("Hiding screenshot preview");

                var (timeLog, statusCode) = await SaveTimeSlot(CurrentImagePath);
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    logger.Warn("Unauthorized access detected, stopping tracker and logging out");
                    await StartStopCommandExecute();
                    await ShowErrorMessage("Unauthorized access detected. You will be logged out.");
                    await LogoutCommandExecute();
                    return;
                }

                _totalKeysPressed = 0;
                _totalMouseClicks = 0;
                _totalMouseScrolls = 0;
                logger.Debug("Reset input counters");

                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
                logger.Info("Time slot saved successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving time slot");
                TempLog($"SaveTimeSlot error: {ex.Message}");
            }
        }

        private void InitializeCommands()
        {
            CloseCommand = new AsyncRelayCommand(CloseCommandExecute);
            StartStopCommand = new AsyncRelayCommand(StartStopCommandExecute);
            LogoutCommand = new AsyncRelayCommand(LogoutCommandExecute);
            RefreshCommand = new AsyncRelayCommand(RefreshCommandExecute);
            LogCommand = new AsyncRelayCommand(LogCommandExecute);
            DeleteScreenshotCommand = new AsyncRelayCommand(DeleteScreenshotCommandExecute);
            SaveScreenshotCommand = new AsyncRelayCommand(SaveScreenshotCommandExecute);
            OpenDashboardCommand = new AsyncRelayCommand(OpenDashboardCommandExecute);
            ProductivityApplicationCommand = new AsyncRelayCommand(
                ProductivityApplicationCommandExecute
            );
            TaskCompleteCommand = new AsyncRelayCommand(TaskCompleteCommandExecute);
            CreateNewTaskCommand = new AsyncRelayCommand(CreateNewTaskCommandExecute);
            TaskOpenCommand = new AsyncRelayCommand(TaskOpenCommandExecute);
            SwitchTrackerNoCommand = new AsyncRelayCommand(SwitchTrackerNoCommandExecute);
            SwitchTrackerYesCommand = new AsyncRelayCommand(SwitchTrackerYesCommandExecute);
        }

        private void InitializeTimers()
        {
            logger.Debug("Initializing timers");
            idlTimeDetectionTimer.Tick += IdlTimeDetectionTimer_Tick;
            idlTimeDetectionTimer.Interval = new TimeSpan(00, 2, 00);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            var nineMinutes = TimeSpan.FromMinutes(9);
            dispatcherTimer.Interval = nineMinutes;
            logger.Debug("Timers initialized");
        }


        private void InitializeInputHooks()
        {
            logger.Debug("Initializing input hooks");
            hook.HookEnabled += Hook_HookEnabled;
            hook.HookDisabled += Hook_HookDisabled;
            hook.KeyPressed += Hook_KeyPressed;
            hook.MouseClicked += Hook_MouseClicked;
            hook.MouseWheel += Hook_MouseWheel;
            hook.RunAsync();
            logger.Info("Input hooks initialized and running");
        }

        private void InitializeUI()
        {
            try
            {
                logger.Debug("Initializing UI components");
                BindProjectList();
                DeleteTempFolder();
                PopulateUserName();
                RefreshCommandExecute();
                CheckWeeklyMonthlyTimeLimit();
                logger.Debug("UI initialization completed");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error initializing UI");
                TempLog($"InitializeUI error: {ex.Message}");
            }
        }
        #endregion

        #region Public Properties
        [ObservableProperty]
        private string _title = "Time Tracker";

        [ObservableProperty]
        private string _startStopButtonText = "Start";

        [ObservableProperty]
        private string _currentInput = string.Empty;

        [ObservableProperty]
        private string _currentImagePath;

        [ObservableProperty]
        private string _deleteImagePath;

        [ObservableProperty]
        private string _currentSessionTimeTracked;

        [ObservableProperty]
        private string _currentDayTimeTracked;

        [ObservableProperty]
        private string _currentWeekTimeTracked;

        [ObservableProperty]
        private string _currentMonthTimeTracked;

        [ObservableProperty]
        private string _videoImage;

        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userId;

        [ObservableProperty]
        private string _projectName;

        [ObservableProperty]
        private string _taskName;

        [ObservableProperty]
        private string _taskDescription;

        [ObservableProperty]
        private bool _canSendReport = true;

        [ObservableProperty]
        private int _progressWidthStart;

        [ObservableProperty]
        private int _progressWidthReport;

        [ObservableProperty]
        private string _countdownTimer;

        [ObservableProperty]
        private bool _canShowScreenshot;

        [ObservableProperty]
        private Bitmap _screenshotImage;

        [ObservableProperty]
        private List<Project> _projects;

        [ObservableProperty]
        private string _userFullName;

        [ObservableProperty]
        private Project _selectedProject;

        [ObservableProperty]
        private string _selectedProjectName;

        private ObservableCollection<ProjectTask> _tasks;
        public ObservableCollection<ProjectTask> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        [ObservableProperty]
        private ProjectTask _selectedTask;

        public bool AllowTaskSelection => SelectedProject != null;

        [ObservableProperty]
        private double _verticalOffset;

        [ObservableProperty]
        private double _horizontalOffset;

        [ObservableProperty]
        private string _canShowRefresh = "Visible";

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private IBrush _messageColor = Brushes.Red;

        [ObservableProperty]
        private bool _popupForSwitchTracker;

        [ObservableProperty]
        private bool _isLoggingEnabled;

        [ObservableProperty]
        private bool _buttonEventInProgress;

        private Int32 _WeeklyHoursLimit;
        public Int32 WeeklyHoursLimit
        {
            get { return _WeeklyHoursLimit; }
            set
            {
                _WeeklyHoursLimit = value;
                OnPropertyChanged(nameof(WeeklyHoursLimit));
            }
        }

        private Int32 _MonthlyHoursLimit;
        public Int32 MonthlyHoursLimit
        {
            get { return _MonthlyHoursLimit; }
            set
            {
                _MonthlyHoursLimit = value;
                OnPropertyChanged(nameof(MonthlyHoursLimit));
            }
        }

        private Int32 _CurrentWeekCompletedHours;
        public Int32 CurrentWeekCompletedHours
        {
            get { return _CurrentWeekCompletedHours; }
            set
            {
                _CurrentWeekCompletedHours = value;
                OnPropertyChanged(nameof(CurrentWeekCompletedHours));
            }
        }

        private bool _isWeeklyHoursCompleted;
        public bool IsWeeklyHoursCompleted
        {
            get { return _isWeeklyHoursCompleted; }
            set
            {
                _isWeeklyHoursCompleted = value;
                OnPropertyChanged(nameof(IsWeeklyHoursCompleted));
            }
        }

        private Int32 _CurrentMonthCompletedHours;
        public Int32 CurrentMonthCompletedHours
        {
            get { return _CurrentMonthCompletedHours; }
            set
            {
                _CurrentMonthCompletedHours = value;
                OnPropertyChanged(nameof(CurrentMonthCompletedHours));
            }
        }

        private bool _isMonthlyHoursCompleted;
        public bool IsMonthlyHoursCompleted
        {
            get { return _isMonthlyHoursCompleted; }
            set
            {
                _isMonthlyHoursCompleted = value;
                OnPropertyChanged(nameof(IsMonthlyHoursCompleted));
            }
        }
        #endregion

        #region Commands
        public AsyncRelayCommand CloseCommand { get; private set; }
        public AsyncRelayCommand StartStopCommand { get; private set; }
        public AsyncRelayCommand LogoutCommand { get; private set; }
        public AsyncRelayCommand RefreshCommand { get; private set; }
        public AsyncRelayCommand LogCommand { get; private set; }
        public AsyncRelayCommand DeleteScreenshotCommand { get; private set; }
        public AsyncRelayCommand SaveScreenshotCommand { get; private set; }
        public AsyncRelayCommand OpenDashboardCommand { get; private set; }
        public AsyncRelayCommand ProductivityApplicationCommand { get; private set; }
        public AsyncRelayCommand TaskCompleteCommand { get; private set; }
        public AsyncRelayCommand CreateNewTaskCommand { get; private set; }
        public AsyncRelayCommand TaskOpenCommand { get; private set; }
        public AsyncRelayCommand SwitchTrackerNoCommand { get; private set; }
        public AsyncRelayCommand SwitchTrackerYesCommand { get; private set; }
        #endregion

        #region Command Methods
        private async Task CloseCommandExecute()
        {
            try
            {
                logger.Info("Close command executed");
                if (_trackerIsOn)
                {
                    logger.Warn("Attempt to close while tracker is running");
                    await ShowErrorMessage(
                        "Please stop the tracker before closing the application."
                    );
                    return;
                }
                await CheckForUnsavedLog();
                logger.Info("Application closing");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in close command");
                TempLog($"CloseCommand error: {ex.Message}");
            }
        }

        private async Task LogoutCommandExecute()
        {
            try
            {
                logger.Info("Logout command executed");
                if (_trackerIsOn)
                {
                    logger.Info("Stopping tracker before logout");
                    await StartStopCommandExecute();
                }

                logger.Debug("Creating and showing login window");
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var loginWindow = new LoginView();
                    GlobalSetting.Instance.LoginView = loginWindow;
                    loginWindow.Show();
                    GlobalSetting.Instance.TimeTracker.Close();
                    GlobalSetting.Instance.TimeTracker = null;
                });
                logger.Info("Logout completed");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in logout command");
                TempLog($"LogoutCommand error: {ex.Message}");
            }
        }

        private async Task StartStopCommandExecute()
        {
            logger.Info("Start/Stop command executed");
            if (!_trackerIsOn)
            {
                if (IsWeeklyHoursCompleted || IsMonthlyHoursCompleted)
                {
                    var msg = IsWeeklyHoursCompleted
                        ? "Your weekly hours have been completed."
                        : "Your monthly hours have been completed.";
                    await ShowErrorMessage(msg);
                    return;
                }
            }
            if (string.IsNullOrEmpty(TaskName))
            {
                logger.Warn("No task selected when trying to start/stop tracker");
                await ShowErrorMessage("No task selected");
                return;
            }

            ProgressWidthStart = 30;
            try
            {
                if (_trackerIsOn)
                {
                    logger.Info("Stopping tracker");
                    idlTimeDetectionTimer.Stop();
                    CanSendReport = true;
                    if (IsWeeklyHoursCompleted || IsMonthlyHoursCompleted)
                    {
                        var msg = IsWeeklyHoursCompleted
                            ? "Your weekly hours have been completed."
                            : "Your monthly hours have been completed.";
                        await ShowErrorMessage(msg);
                    }
                }
                else
                {
                    logger.Info("Starting tracker");
                    IdleTimeDetector.Initialize();
                    idlTimeDetectionTimer.Start();
                    CanSendReport = false;
                }

                await SetTrackerStatus();
                _timeTrackedSaved = await GetCurrentDateTimeTracked();
                ShowTimeTracked(true);
                ShowCurrentTimeTracked();
                logger.Info($"Tracker {(_trackerIsOn ? "started" : "stopped")} successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in start/stop command");
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private async Task RefreshCommandExecute()
        {
            logger.Info("Refresh command executed");
            ButtonEventInProgress = true;
            try
            {
                PopulateUserName();
                BindProjectList();
                Tasks = new ObservableCollection<ProjectTask>();
                TaskName = string.Empty;
                TaskDescription = string.Empty;
                SelectedTask = null;
                logger.Info("UI refreshed successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in refresh command");
                TempLog($"RefreshCommand error: {ex.Message}");
            }
            finally
            {
                ButtonEventInProgress = false;
            }
        }

        private async Task LogCommandExecute()
        {
            logger.Info("Log command executed");
            try
            {
                string logsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsFolderPath))
                {
                    logger.Debug("Creating logs directory");
                    Directory.CreateDirectory(logsFolderPath);
                }

                logger.Info($"Opening logs folder: {logsFolderPath}");
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = logsFolderPath,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in log command");
                TempLog($"LogCommand error: {ex.Message}");
            }
        }

        private async Task DeleteScreenshotCommandExecute()
        {
            logger.Info("Delete screenshot command executed");
            try
            {
                DeleteImagePath = CurrentImagePath;
                CurrentImagePath = null;
                saveDispatcherTimer.Stop();
                CanShowScreenshot = false;
                logger.Info("Screenshot marked for deletion");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in delete screenshot command");
                TempLog($"DeleteScreenshot error: {ex.Message}");
            }
        }


        private async Task SaveScreenshotCommandExecute()
        {
            logger.Info("Save screenshot command executed");
            try
            {
                CanShowScreenshot = false;
                if (!string.IsNullOrEmpty(CurrentImagePath))
                {
                    string savedPath = Path.Combine(Path.GetTempPath(), "saved_screenshot.png");
                    File.Copy(CurrentImagePath, savedPath, true);
                    logger.Info($"Screenshot saved to: {savedPath}");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in save screenshot command");
                TempLog($"SaveScreenshot error: {ex.Message}");
            }
        }

        private async Task OpenDashboardCommandExecute()
        {
            logger.Info("Open dashboard command executed");
            try
            {
                string url =
                    _configuration.GetSection("ApplicationBaseUrl").Value + "#/screenshots";
                logger.Info($"Opening dashboard URL: {url}");
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in open dashboard command");
                TempLog($"OpenDashboard error: {ex.Message}");
            }
        }

        private async Task ProductivityApplicationCommandExecute()
        {
            logger.Info("Productivity application command executed");
            await ShowErrorMessage("Productivity Applications not implemented.");
        }

        private async Task TaskCompleteCommandExecute()
        {
            logger.Info("Task complete command executed");
            ButtonEventInProgress = true;
            if (string.IsNullOrEmpty(TaskName))
            {
                logger.Warn("No task selected when trying to complete task");
                await ShowErrorMessage("No task selected");
                return;
            }

            ProgressWidthStart = 30;
            try
            {
                dynamic task = new { status = "Done", project = SelectedProject._id };
                logger.Info($"Marking task as complete: {SelectedTask._id}");
                var result = await _restService.CompleteATask(SelectedTask._id, task);
                if (result.data != null)
                {
                    logger.Info("Task marked as completed successfully");
                    await ShowInformationMessage("Task has been marked as completed");
                    if (SelectedProject != null)
                    {
                        await GetTaskList();
                    }
                }
                else
                {
                    logger.Warn("Failed to mark task as complete");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in task complete command");
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
                ButtonEventInProgress = false;
            }
        }

        private async Task CreateNewTaskCommandExecute()
        {
            logger.Info("Create new task command executed");
            if (string.IsNullOrEmpty(TaskName))
            {
                logger.Warn("Task name not specified when trying to create task");
                await ShowErrorMessage("Please specify task details");
                return;
            }

            ProgressWidthStart = 30;
            try
            {
                logger.Info($"Creating new task: {TaskName}");
                await CreateNewTask();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in create new task command");
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private async Task TaskOpenCommandExecute()
        {
            logger.Info("Task open command executed");
            ButtonEventInProgress = true;
            if (string.IsNullOrEmpty(TaskName))
            {
                logger.Warn("No task selected when trying to open task");
                await ShowErrorMessage("No task is selected");
                ButtonEventInProgress = false;
                return;
            }

            ProgressWidthStart = 30;
            try
            {
                var taskUrl =
                    $"{GlobalSetting.portalBaseUrl}#/home/edit-task?taskId={SelectedTask._id}";
                logger.Info($"Opening task URL: {taskUrl}");
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = taskUrl,
                        UseShellExecute = true
                    }
                );
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in task open command");
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
                ButtonEventInProgress = false;
            }
        }

        private async Task SwitchTrackerNoCommandExecute()
        {
            logger.Info("User chose not to switch tracker device");
            PopupForSwitchTracker = false;
            _minutesTracked = 0;
            if (_trackerIsOn)
            {
                logger.Info("Stopping tracker after user chose not to switch");
                await StartStopCommandExecute();
            }
            _totalKeysPressed = 0;
            _totalMouseClicks = 0;
            _totalMouseScrolls = 0;
            saveDispatcherTimer.Stop();
            CurrentInput = string.Empty;
            logger.Info("Tracker state reset after device switch declined");
        }

        private async Task SwitchTrackerYesCommandExecute()
        {
            try
            {
                logger.Info("User chose to switch tracker device");
                PopupForSwitchTracker = false;
                _minutesTracked = 10;
                _timeLogForSwitchingMachine.makeThisDeviceActive = true;

                logger.Info("Sending time log with device switch flag");
                var result = await _restService.AddTimeLog(_timeLogForSwitchingMachine);

                _totalKeysPressed = 0;
                _totalMouseClicks = 0;
                _totalMouseScrolls = 0;
                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
                logger.Info("Device switch completed successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error switching tracker device");
                TempLog($"SwitchTrackerYes error: {ex.Message}");
                _unsavedTimeLogs.Add(_timeLogForSwitchingMachine);
            }
            _timeLogForSwitchingMachine = new TimeLog();
        }
        #endregion

        #region Private Methods
        private async Task<bool> SetTrackerStatus()
        {
            bool previousState = _trackerIsOn;
            DateTime now = DateTime.UtcNow;

            logger.Info($"Setting tracker status to: {!_trackerIsOn}");
            var onlineStatusResult = await _restService.UpdateOnlineStatus(
                UserId,
                _machineId,
                !_trackerIsOn,
                SelectedProject?._id,
                SelectedTask?._id
            );

            if (_trackerIsOn)
            {
                logger.Info($"Stopping tracker at {DateTime.UtcNow}");
                _trackingStoppedAt = DateTime.UtcNow;
                StopTracker();
                _trackerIsOn = false;
                StartStopButtonText = "Start";
                CanShowRefresh = "Visible";
                logger.Info("Tracker stopped successfully");
            }
            else
            {
                logger.Info($"Starting tracker at {now}");
                _trackingStartedAt = now;
                _minutesTracked = 0;
                StartTracker();

                if (
                    !string.IsNullOrEmpty(TaskName)
                    && !Tasks.Any(t =>
                        t.taskName.Equals(TaskName, StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    logger.Info("Creating new task as it doesn't exist");
                    await CreateNewTask();
                }
                _trackerIsOn = true;
                StartStopButtonText = "Stop";
                CanShowRefresh = "Hidden";
                logger.Info("Tracker started successfully");
            }
            return true;
        }

        private void StartTracker()
        {
            logger.Debug("Starting tracker timers");
            dispatcherTimer?.Start();
            usedAppDetector?.Start();
            logger.Info("Tracker timers started");
        }

        private void StopTracker()
        {
            logger.Debug("Stopping tracker timers");
            usedAppDetector?.Stop();
            dispatcherTimer?.Stop();
            logger.Info("Tracker timers stopped");
        }


        private async void SaveTimeSlot_Tick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                logger.Trace("Save time slot timer tick");
                CanShowScreenshot = false;
                logger.Debug("Hiding screenshot preview");

                var (timeLog, statusCode) = await SaveTimeSlot(CurrentImagePath);
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    logger.Warn("Unauthorized access detected, stopping tracker and logging out");
                    await StartStopCommandExecute();
                    await ShowErrorMessage("Unauthorized access detected. You will be logged out.");
                    await LogoutCommandExecute();
                    return;
                }

                _totalKeysPressed = 0;
                _totalMouseClicks = 0;
                _totalMouseScrolls = 0;
                logger.Debug("Reset input counters");

                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
                logger.Info("Time slot saved successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving time slot");
                TempLog($"SaveTimeSlot error: {ex.Message}");
            }
        }

        private void UpdateLastInputTime()
        {
            _lastInputTime = DateTime.UtcNow;
        }

        private int GetIdleTime()
        {
            return DateTime.UtcNow.Subtract(_lastInputTime).Minutes;
        }

        private async Task<string> CaptureScreen()
        {
            try
            {
                logger.Info("Capturing screen");
                var userSettings = Task.Run(
                    async () => await APIService.GetUserPreferencesSetting()
                ).Result;

                var screenshot = await _screenshotService.CaptureScreenAsync(userSettings.isBlurScreenshot);
                CurrentImagePath = screenshot;
                if (!userSettings.isBeepSoundEnabled)
                {
                    PlayMedia.PlayScreenCaptureSound();
                }
                if (!userSettings.isScreenshotNotificationEnabled)
                {
                    CanShowScreenshot = true;
                }
                WeeklyHoursLimit = userSettings.weeklyHoursLimit * 60;
                MonthlyHoursLimit = userSettings.monthlyHoursLimit * 60;

                Countdown(10, TimeSpan.FromSeconds(1), cur => CountdownTimer = $"({cur})");
                logger.Info($"Screen captured and saved to: {CurrentImagePath}");
                return CurrentImagePath;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error capturing screen");
                TempLog($"CaptureScreen error: {ex.Message}");
                throw;
            }
        }

        private async Task GetCurrentSavedTime()
        {
            logger.Debug("Getting current saved time");
            _timeTrackedSaved = await GetCurrentDateTimeTracked();
            logger.Info($"Current saved time: {_timeTrackedSaved}");
        }

        private async Task<TimeSpan> GetCurrentDateTimeTracked()
        {
            var totalTime = TimeSpan.Zero;
            try
            {
                logger.Debug("Getting current date time tracked");
                var timeLog = new TimeLog
                {
                    user = UserId,
                    date = new DateTime(
                        DateTime.UtcNow.Year,
                        DateTime.UtcNow.Month,
                        DateTime.UtcNow.Day
                    )
                };

                logger.Info($"Requesting time logs for user: {UserId}");
                var result = await _restService.GetTimeLogs(timeLog);
                if (result.data != null)
                {
                    logger.Debug($"Received {result.data.Count} time logs");
                    foreach (var log in result.data)
                    {
                        var trackedTime = log.endTime.Subtract(log.startTime);
                        totalTime += new TimeSpan(
                            trackedTime.Hours,
                            trackedTime.Minutes,
                            trackedTime.Seconds
                        );
                    }
                }
                logger.Info($"Total time tracked: {totalTime}");
                return totalTime;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting current date time tracked");
                TempLog($"GetCurrentDateTimeTracked error: {ex.Message}");
                return totalTime;
            }
        }


        private async Task<TimeSpan?> GetCurrentWeekTimeTracked()
        {
            try
            {
                logger.Debug("Getting current week time tracked");
                var dayOfWeekToday = (int)DateTime.UtcNow.DayOfWeek;
                var startDateOfWeek = DateTime.Today.AddDays(-1 * (dayOfWeekToday - 1));
                var totalTime = TimeSpan.Zero;

                logger.Info($"Requesting week time for period: {startDateOfWeek} to {DateTime.UtcNow}");
                var result = await _restService.GetCurrentWeekTotalTime(
                   new CurrentWeekTotalTime()
                   {
                       user = UserId,
                       startDate = new DateTime(
                           startDateOfWeek.Date.Year,
                           startDateOfWeek.Date.Month,
                           startDateOfWeek.Date.Day
                       ),
                       endDate = new DateTime(
                           DateTime.UtcNow.Year,
                           DateTime.UtcNow.Date.Month,
                           DateTime.UtcNow.Date.Day
                       ),
                   }
               );

                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                logger.Info($"Current week time tracked: {totalTime}");
                return totalTime;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting current week time tracked");
                TempLog($"GetCurrentWeekTimeTracked error: {ex.Message}");
                return null;
            }
        }

        private async Task<TimeSpan?> GetCurrentMonthTimeTracked()
        {
            try
            {
                logger.Debug("Getting current month time tracked");
                var totalTime = TimeSpan.Zero;

                logger.Info($"Requesting month time for current month");
                var result = await _restService.GetCurrentWeekTotalTime(
                   new CurrentWeekTotalTime()
                   {
                       user = UserId,
                       startDate = new DateTime(
                           DateTime.UtcNow.Year,
                           DateTime.UtcNow.Date.Month,
                           1
                       ),
                       endDate = new DateTime(
                           DateTime.UtcNow.Year,
                           DateTime.UtcNow.Date.Month,
                           DateTime.UtcNow.Date.Day
                       ),
                   }
               );

                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                logger.Info($"Current month time tracked: {totalTime}");
                return totalTime;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting current month time tracked");
                TempLog($"GetCurrentMonthTimeTracked error: {ex.Message}");
                return null;
            }
        }

        private void ShowTimeTracked(bool currentSessionSaved)
        {
            logger.Trace("Showing time tracked");
            var sessionTimeTracked = TimeSpan.FromMinutes(_minutesTracked);
            CurrentSessionTimeTracked =
                $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes.ToString("00")} m";
            if (currentSessionSaved)
            {
                TimeSpan totalTimeTracked = _timeTrackedSaved;
                CurrentDayTimeTracked =
                    $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
            else
            {
                TimeSpan totalTimeTracked = _timeTrackedSaved + TimeSpan.FromMinutes(_minutesTracked); // (timeTrackedSaved + sessionTimeTracked);
                CurrentDayTimeTracked =
                    $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
            logger.Debug($"Displayed time: Session={CurrentSessionTimeTracked}, Day={CurrentDayTimeTracked}");
        }

        private async void ShowCurrentTimeTracked()
        {
            logger.Trace("Showing current time tracked");
            var currentWeekTimeTracked = await GetCurrentWeekTimeTracked();
            if (currentWeekTimeTracked.HasValue)
            {
                CurrentWeekCompletedHours = (int)currentWeekTimeTracked.Value.TotalMinutes;
                CurrentWeekTimeTracked =
                    $"{(currentWeekTimeTracked.Value.Days * 24) + currentWeekTimeTracked.Value.Hours} hrs {currentWeekTimeTracked.Value.Minutes:D2} m";
                logger.Debug($"Week time displayed: {CurrentWeekTimeTracked}");
            }

            var currentMonthTimeTracked = await GetCurrentMonthTimeTracked();
            if (currentMonthTimeTracked.HasValue)
            {
                CurrentMonthCompletedHours = (int)currentMonthTimeTracked.Value.TotalMinutes;
                CurrentMonthTimeTracked =
                    $"{(currentMonthTimeTracked.Value.Days * 24) + currentMonthTimeTracked.Value.Hours} hrs {currentMonthTimeTracked.Value.Minutes:D2} m";
                logger.Debug($"Month time displayed: {CurrentMonthTimeTracked}");
            }
        }

        private async Task<(TimeLog timeLog, HttpStatusCode statusCode)> SaveTimeSlot(
            string filePath
        )
        {
            try
            {
                logger.Info("Saving time slot");
                byte[] bytes = File.ReadAllBytes(filePath);
                string file = Convert.ToBase64String(bytes);
                string fileName = Path.GetFileName(filePath);
                var currentDate = DateTime.UtcNow;

                logger.Debug($"Creating time log for task: {SelectedTask?._id}");
                var timeLog = new TimeLog
                {
                    user = UserId,
                    date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day),
                    task = SelectedTask._id,
                    startTime = DateTime.UtcNow.AddMinutes(-10),
                    endTime = DateTime.UtcNow,
                    fileString = file,
                    filePath = fileName,
                    keysPressed = _totalKeysPressed,
                    allKeysPressed = CurrentInput,
                    clicks = _totalMouseClicks,
                    scrolls = _totalMouseScrolls,
                    project = SelectedProject._id,
                    machineId = _machineId,
                    makeThisDeviceActive = false
                };

                if (!await CheckInternetConnectivity.IsConnectedToInternetAsync())
                {
                    logger.Warn("No internet connection, saving time log locally");
                    TempLog($"No internet at {DateTime.UtcNow}");
                    _unsavedTimeLogs.Add(timeLog);
                    await ShowErrorMessage("Please check your internet connectivity.");
                    return (null, HttpStatusCode.OK);
                }

                logger.Info("Sending time log to server");
                var result = await _restService.AddTimeLog(timeLog);
                if (result.statusCode == HttpStatusCode.Unauthorized)
                {
                    logger.Warn("Unauthorized response from server");
                    return (null, HttpStatusCode.Unauthorized);
                }
                else if (result.statusCode == System.Net.HttpStatusCode.NotAcceptable)
                {
                    //await ShowErrorMessage(result?.message);
                    return (null, HttpStatusCode.NotAcceptable);
                }

                logger.Info($"Server response: {result?.data?.message ?? "No message"}");
                if (!string.IsNullOrEmpty(result.data.message))
                {
                    if (
                        result.data.message.Contains(
                            "The user is logged in on another device. Would you like to make this device active?"
                        )
                    )
                    {
                        logger.Warn("User logged in on another device");
                        PopupForSwitchTracker = true;
                        _timeLogForSwitchingMachine = timeLog;
                        _minutesTracked = 0;
                        return (null, HttpStatusCode.OK);
                    }
                }
                else if (_unsavedTimeLogs.Count > 0)
                {
                    logger.Info("Saving unsaved time logs");
                    foreach (var unsavedTimeLog in _unsavedTimeLogs.ToArray())
                    {
                        logger.Info($"Saving unsaved time log: {unsavedTimeLog.startTime}");
                        await _restService.AddTimeLog(unsavedTimeLog);
                        _unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                }
                logger.Info("Time slot saved successfully");
                return (result?.data, result?.statusCode ?? HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving time slot");
                TempLog($"SaveTimeSlot error: {ex.Message}");
                return (null, HttpStatusCode.InternalServerError);
            }
        }

        private async Task GetTaskList()
        {
            logger.Info("Getting task list");
            ProgressWidthStart = 30;
            try
            {
                Tasks.Clear();
                if (SelectedProject != null && !string.IsNullOrEmpty(SelectedProject._id))
                {
                    logger.Debug($"Requesting tasks for project: {SelectedProject._id}");
                    var taskList = await _restService.GetTaskListByProject(
                        new TaskRequest
                        {
                            projectId = SelectedProject._id,
                            userId = GlobalSetting.Instance.LoginResult.data.user.id,
                            skip = "",
                            next = ""
                        }
                    );

                    if (taskList.status == "success" && taskList.taskList != null)
                    {
                        logger.Debug($"Received {taskList.taskList.Count} tasks");
                        foreach (var t in taskList.taskList)
                        {
                            if (t.status.ToLower() != "closed" && t.status.ToLower() != "done")
                            {
                                Tasks.Add(
                                    new ProjectTask
                                    {
                                        taskName = t.taskName,
                                        description = t.description,
                                        _id = t._id
                                    }
                                );
                            }
                        }
                        logger.Info($"Added {Tasks.Count} active tasks to list");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error getting task list");
                TempLog($"GetTaskList error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private void Countdown(int count, TimeSpan interval, Action<int> ts)
        {
            logger.Debug($"Starting countdown: {count} seconds");
            var timer = new Timer(interval.TotalMilliseconds);
            timer.Elapsed += (s, e) =>
            {
                if (count-- == 1)
                {
                    logger.Trace("Countdown completed");
                    CanShowScreenshot = false;
                    timer.Stop();
                }
                else
                {
                    ts(count);
                }
            };
            ts(count);
            timer.Start();
        }
        private void DeleteImagePath_Tick(object? sender, ElapsedEventArgs e)
        {
            logger.Trace("Delete image path timer tick");
            if (File.Exists(DeleteImagePath))
            {
                try
                {
                    logger.Info($"Deleting image: {DeleteImagePath}");
                    File.Delete(DeleteImagePath);
                    DeleteImagePath = "";
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error deleting image");
                }
            }
        }

        private async void BindProjectList()
        {
            logger.Info("Binding project list");
            Projects = null;
            try
            {
                logger.Debug($"Requesting projects for user: {GlobalSetting.Instance.LoginResult.data.user.id}");
                var projectList = await _restService.GetProjectListByUserId(
                    new ProjectRequest { userId = GlobalSetting.Instance.LoginResult.data.user.id }
                );

                if (
                    projectList != null
                    && projectList.status == "success"
                    && projectList.data != null
                )
                {
                    Projects = projectList.data.projectList;
                    var projectData = await APIService.GetUserPreferencesByKey();
                    if (SelectedProject == null)
                    {
                        if (projectData != null)
                        {
                            SelectedProject = projectData;
                            SelectedProjectName = SelectedProject.projectName;
                        }
                    }
                    logger.Info($"Loaded {Projects.Count} projects");
                }
                else
                {
                    logger.Warn("No projects received or request failed");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error binding project list");
                TempLog($"BindProjectList error: {ex.Message}");
            }
        }

        private void PopulateUserName()
        {
            logger.Trace("Populating user name");
            UserFullName =
                $"Welcome, {GlobalSetting.Instance.LoginResult.data.user.firstName} {GlobalSetting.Instance.LoginResult.data.user.lastName}.";
            logger.Debug($"User name set to: {UserFullName}");
        }

        private async Task CreateNewTask()
        {
            logger.Info("Creating new task");
            ButtonEventInProgress = true;
            try
            {
                var taskUsers = new[] { GlobalSetting.Instance.LoginResult.data.user.id };
                logger.Debug($"Creating task '{TaskName}' for project: {SelectedProject._id}");

                var newTaskResult = await _restService.AddNewTask(
                    new CreateTaskRequest
                    {
                        taskName = TaskName,
                        comment = "Created by TimeTracker",
                        project = SelectedProject._id,
                        taskUsers = taskUsers,
                        user = GlobalSetting.Instance.LoginResult.data.user.id,
                        description = TaskDescription,
                        startDate = DateTime.UtcNow,
                        startTime = DateTime.UtcNow,
                        status = "In Progress"
                    }
                );

                if (newTaskResult?.status.ToUpper() == "SUCCESS")
                {
                    logger.Info("Task created successfully");
                    await ShowInformationMessage("Task has been created");
                    SelectedTask = newTaskResult.data.newTask;
                }
                else
                {
                    logger.Warn($"Failed to create task: {newTaskResult?.message ?? "Unknown error"}");
                    await ShowErrorMessage(
                        newTaskResult?.message ?? "Something went wrong while creating the task."
                    );
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error creating new task");
                TempLog($"CreateNewTask error: {ex.Message}");
            }
            finally
            {
                ButtonEventInProgress = false;
            }
        }

        private async Task CheckForUnsavedLog()
        {
            logger.Info("Checking for unsaved logs");
            if (_unsavedTimeLogs?.Count > 0)
            {
                logger.Warn($"Found {_unsavedTimeLogs.Count} unsaved time logs");

                if (!await CheckInternetConnectivity.IsConnectedToInternetAsync())
                {
                    logger.Warn("No internet connection, cannot save unsaved logs");
                    await ShowErrorMessage("This needs an active internet connection");
                    return;
                }

                foreach (var unsavedTimeLog in _unsavedTimeLogs.ToArray())
                {
                    logger.Info($"Saving unsaved time log from: {unsavedTimeLog.startTime}");
                    var saveResult = await _restService.AddTimeLog(unsavedTimeLog);
                    logger.Info($"Save result: {saveResult?.data?.message ?? "No message"}");

                    if (
                        !string.IsNullOrEmpty(saveResult?.data?.message)
                        && saveResult.data.message.Contains(
                            "User is logged in on another device"
                        )
                    )
                    {
                        logger.Warn("User logged in on another device while saving unsaved logs");
                    }
                    _unsavedTimeLogs.Remove(unsavedTimeLog);
                }
                logger.Info("All unsaved logs processed");
            }
        }

        private async Task SendLiveImage()
        {
            //try
            //{
            //    var screenshot = await _screenshotService.CaptureScreenAsync();
            //    using var stream = new MemoryStream();
            //    await File.WriteAllBytesAsync(screenshot.Path, stream.ToArray());
            //    await _restService.sendLiveScreenDataV1(
            //        new LiveImageRequest { fileString = Convert.ToBase64String(stream.ToArray()) }
            //    );
            //}
            //catch (Exception ex)
            //{
            //    TempLog($"SendLiveImage error: {ex.Message}");
            //}
        }

        private async void CheckForLiveScreen_Tick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                var response = await _restService.checkLiveScreen(new TaskUser { user = UserId });
                if (response?.Success == true)
                {
                    if (!_isLiveImageRunning)
                    {
                        _isLiveImageRunning = true;
                        _sendImageRegularly = new System.Threading.Timer(
                            async (_) => await SendLiveImage(),
                            null,
                            TimeSpan.Zero,
                            TimeSpan.FromMilliseconds(_frequencyOfLiveImage)
                        );
                    }
                }
                else
                {
                    _isLiveImageRunning = false;
                    _sendImageRegularly?.Dispose();
                }
            }
            catch (Exception ex)
            {
                TempLog($"CheckForLiveScreen error: {ex.Message}");
            }
        }

        private void DeleteTempFolder()
        {
            try
            {
                logger.Info("Deleting temp folder");
                var tempFolder = Path.Combine(Path.GetTempPath(), "TimeTrackerScreenshots");
                if (Directory.Exists(tempFolder))
                {
                    logger.Debug($"Deleting directory: {tempFolder}");
                    Directory.Delete(tempFolder, true);
                    logger.Info("Temp folder deleted");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error deleting temp folder");
                TempLog($"DeleteTempFolder error: {ex.Message}");
            }
        }


        private async Task ShowErrorMessage(string errorMessage)
        {
            logger.Error($"Showing error message: {errorMessage}");
            MessageColor = Brushes.Red;
            ErrorMessage = errorMessage;
            await Task.Delay(TimeSpan.FromSeconds(10));
            ErrorMessage = string.Empty;
        }

        private async Task ShowInformationMessage(string message)
        {
            logger.Info($"Showing information message: {message}");
            MessageColor = Brushes.Green;
            ErrorMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(20));
            ErrorMessage = string.Empty;
        }

        private void TempLog(string message)
        {
            logger.Info(message);
        }

        partial void OnSelectedProjectChanged(Project oldValue, Project newValue)
        {
            logger.Debug($"Selected project changed from {oldValue?._id} to {newValue?._id}");

            // Clear existing tasks and task selection
            Tasks.Clear();
            SelectedTask = null;
            TaskName = string.Empty;
            TaskDescription = string.Empty;

            // Fetch tasks for the new project
            if (newValue != null)
            {
                logger.Info($"Loading tasks for new project: {newValue._id}");
                var userSettings = Task.Run(
                    async () => await APIService.SetUserPreferences(new CreateUserPreferenceRequest
                    {
                        userId = GlobalSetting.Instance.LoginResult.data.user.id,
                        preferenceKey = GlobalSetting.Instance.userPreferenceKey.TrackerSelectedProject,
                        preferenceValue = $"{newValue._id}#{newValue.projectName}"
                    })
                    ).Result;
                _ = GetTaskList(); // Run asynchronously
            }

            // Notify UI that AllowTaskSelection may have changed
            OnPropertyChanged(nameof(AllowTaskSelection));
        }

        partial void OnSelectedTaskChanged(ProjectTask oldValue, ProjectTask newValue)
        {
            logger.Debug($"Selected task changed from {oldValue?._id} to {newValue?._id}");
            if (newValue != null)
            {
                TaskName = newValue.taskName;
                TaskDescription = newValue.description;
                logger.Debug($"Set task name to: {TaskName}");
            }
            else
            {
                TaskName = string.Empty;
                TaskDescription = string.Empty;
                logger.Debug("Cleared task selection");
            }
        }

        private async Task saveBrowserHistory(DateTime startDate, DateTime endDate)
        {
            try
            {
                logger.Info($"Saving browser history from {startDate} to {endDate}");
                var browserHistoryList = BrowserHistory.GetHistoryEntries(startDate, endDate);
                if (browserHistoryList.Count > 0)
                {
                    logger.Debug($"Found {browserHistoryList.Count} browser history entries");
                    var rest = new REST(new HttpProviders());
                    foreach (var browserHistory in browserHistoryList)
                    {
                        
                        var result = await rest.AddBrowserHistory(browserHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error saving browser history");
                throw;
            }
        }

        private async Task CheckWeeklyMonthlyTimeLimit()
        {
            var userSettings = await APIService.GetUserPreferencesSetting();
            WeeklyHoursLimit = userSettings.weeklyHoursLimit * 60;
            MonthlyHoursLimit = userSettings.monthlyHoursLimit * 60;

            // Weekly check
            if (WeeklyHoursLimit > 0)
            {
                var weeklyTracked = await GetCurrentWeekTimeTracked();
                if (weeklyTracked != null)
                {
                    var weeklyMinutes = (int)weeklyTracked.Value.TotalMinutes;
                    CurrentWeekTimeTracked =
                        $"{(weeklyTracked.Value.Days * 24) + weeklyTracked.Value.Hours} hrs {weeklyTracked.Value.Minutes:00} m";

                    if (weeklyMinutes >= WeeklyHoursLimit)
                    {
                        IsWeeklyHoursCompleted = true;
                        await ShowErrorMessage("Your weekly hours have been completed.");
                        return;
                    }
                }
                IsWeeklyHoursCompleted = false;
            }

            // Monthly check
            if (MonthlyHoursLimit > 0)
            {
                var monthlyTracked = await GetCurrentMonthTimeTracked();
                if (monthlyTracked != null)
                {
                    var monthlyMinutes = (int)monthlyTracked.Value.TotalMinutes;
                    CurrentMonthTimeTracked =
                        $"{(monthlyTracked.Value.Days * 24) + monthlyTracked.Value.Hours} hrs {monthlyTracked.Value.Minutes:00} m";

                    if (monthlyMinutes >= MonthlyHoursLimit)
                    {
                        IsMonthlyHoursCompleted = true;
                        await ShowErrorMessage("Your monthly hours have been completed.");
                        return;
                    }
                }
                IsMonthlyHoursCompleted = false;
            }
        }

        #endregion

        #region WebSocket (Commented Out)
        /*
        private readonly ClientWebSocket _webSocket = new ClientWebSocket();

        private async Task ConnectWebSocket()
        {
            try
            {
                // var url = $"wss://effortlesshrmapi.azurewebsites.net:4000/{UserId}";
                // await _webSocket.ConnectAsync(new Uri(url), CancellationToken.None);
                // ListenForWebSocketMessages();
            }
            catch (Exception ex)
            {
                TempLog($"ConnectWebSocket error: {ex.Message}");
            }
        }

        private async void ListenForWebSocketMessages()
        {
            try
            {
                var buffer = new byte[1024 * 4];
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Handle WebSocket messages (not implemented)
                    }
                }
            }
            catch (Exception ex)
            {
                TempLog($"WebSocket error: {ex.Message}");
            }
        }
        */
        #endregion
    }
}
