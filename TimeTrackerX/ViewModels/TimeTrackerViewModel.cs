using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using TimeTrackerX.Models;
using TimeTrackerX.Services;
using TimeTrackerX.Services.Interfaces;
using TimeTrackerX.Utilities;
using Timer = System.Timers.Timer;

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

        private Timer dispatcherTimer = new Timer();
        private Timer idlTimeDetectionTimer = new Timer();
        private Timer saveDispatcherTimer = new Timer();
        private Timer deleteImagePath = new Timer();
        private Timer usedAppDetector = new Timer();

        private Timer shareLiveScreen = new Timer();
        private Timer checkForLiveScreen = new Timer();

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
        private string _machineId = string.Empty;
        private TimeLog _timeLogForSwitchingMachine = new TimeLog();
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
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            var nineMinutes = TimeSpan.FromMinutes(4);
            dispatcherTimer.Interval = nineMinutes;
            UserName = GlobalSetting.Instance.LoginResult.data.user.email;
            UserId = GlobalSetting.Instance.LoginResult.data.user.id;
            this._machineId = new Machine().CreateMachineId();
            _restService = new REST(new HttpProviders());
            InitializeCommands();
            InitializeTimers();
            InitializeInputHooks();
            InitializeUI();

            _tasks = new ObservableCollection<ProjectTask>();
        }

        private async void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (_trackerIsOn)
                {
                    var lastInterval = dispatcherTimer.Interval;
                    var currentMinutes = DateTime.UtcNow.Minute;
                    _minutesTracked += 10;
                    var randomTime = _rand.Next(2, 9);
                    double forTimerInterval =
                        ((currentMinutes - (currentMinutes % 10)) + 10 + randomTime)
                        - currentMinutes;
                    dispatcherTimer.Interval = TimeSpan
                        .FromMinutes(forTimerInterval);
                        
                    var filepath = await CaptureScreen();
                    ScreenshotImage = new Bitmap(filepath);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        CanShowScreenshot = true;
                    });
                    saveDispatcherTimer = new Timer();
                    saveDispatcherTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds;
                    saveDispatcherTimer.Elapsed += SaveTimeSlot_Tick;
                    saveDispatcherTimer.Start();
                    await SaveBrowserHistory(DateTime.Now.Subtract(lastInterval), DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                TempLog($"DispatcherTimer error: {ex.Message}");
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
            //_dispatcherTimer.Interval = TimeSpan.FromMinutes(9).TotalMilliseconds;
            //_dispatcherTimer.Elapsed += DispatcherTimer_Tick;

            //_idleTimeDetectionTimer.Interval = TimeSpan.FromMinutes(2).TotalMilliseconds;
            //_idleTimeDetectionTimer.Elapsed += IdleTimeDetectionTimer_Tick;

            //_usedAppDetector.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            //_usedAppDetector.Elapsed += UsedAppDetector_Tick;

            //_shareLiveScreen.Interval = TimeSpan.FromSeconds(5).TotalMilliseconds;
            //_shareLiveScreen.Elapsed += ShareLiveScreen_Tick;

            //_checkForLiveScreen.Interval = TimeSpan.FromSeconds(30).TotalMilliseconds;
            //_checkForLiveScreen.Elapsed += CheckForLiveScreen_Tick;
            //_checkForLiveScreen.Start();
        }

        private void InitializeInputHooks()
        {
            //_mouseEventService.MouseClick += (s, e) => _totalMouseClicks++;
            //_mouseEventService.MouseWheel += (s, e) => _totalMouseScrolls++;
            //_keyEventService.KeyDown += (s, e) =>
            //{
            //    _totalKeysPressed++;
            //    if (!string.IsNullOrEmpty(e.Key) && e.Key.Length == 1)
            //    {
            //        CurrentInput += e.Key;
            //    }
            //    else if (e.Key == "Space")
            //    {
            //        CurrentInput += " ";
            //    }
            //};
            //_mouseEventService.Start();
            //_keyEventService.Start();
        }

        private void InitializeUI()
        {
            try
            {
                BindProjectList();
                DeleteTempFolder();
                PopulateUserName();
                RefreshCommandExecute();
            }
            catch (Exception ex)
            {
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
                if (_trackerIsOn)
                {
                    await ShowErrorMessage(
                        "Please stop the tracker before closing the application."
                    );
                    return;
                }
                await CheckForUnsavedLog();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                TempLog($"CloseCommand error: {ex.Message}");
            }
        }

        private async Task LogoutCommandExecute()
        {
            try
            {
                if (_trackerIsOn)
                {
                    await StartStopCommandExecute();
                }
                var loginWindow = new Login();
                //App.Current.MainWindow = loginWindow;
                //loginWindow.Show();
                await Task.Delay(100); // Ensure window transition
                // App.Current.MainWindow?.Close();
            }
            catch (Exception ex)
            {
                TempLog($"LogoutCommand error: {ex.Message}");
            }
        }

        private async Task StartStopCommandExecute()
        {
            if (string.IsNullOrEmpty(TaskName))
            {
                await ShowErrorMessage("No task selected");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                if (_trackerIsOn)
                {
                    //idleTimeDetectionTimer.Stop();
                    CanSendReport = true;
                }
                else
                {
                    //idleTimeDetectionTimer.Start();
                    CanSendReport = false;
                }
                await SetTrackerStatus();
                _timeTrackedSaved = await GetCurrentDateTimeTracked();
                ShowTimeTracked(true);
                ShowCurrentTimeTracked();
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private async Task RefreshCommandExecute()
        {
            ButtonEventInProgress = true;
            try
            {
                PopulateUserName();
                BindProjectList();
                Tasks = new ObservableCollection<ProjectTask>();
                TaskName = string.Empty;
                TaskDescription = string.Empty;
                SelectedTask = null;
            }
            catch (Exception ex)
            {
                TempLog($"RefreshCommand error: {ex.Message}");
            }
            finally
            {
                ButtonEventInProgress = false;
            }
        }

        private async Task LogCommandExecute()
        {
            try
            {
                string logsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsFolderPath))
                {
                    Directory.CreateDirectory(logsFolderPath);
                }
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
                TempLog($"LogCommand error: {ex.Message}");
            }
        }

        private async Task DeleteScreenshotCommandExecute()
        {
            try
            {
                DeleteImagePath = CurrentImagePath;
                CurrentImagePath = null;
                saveDispatcherTimer.Stop();
                CanShowScreenshot = false;

                //deleteImagePathTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
                //deleteImagePathTimer.Elapsed += DeleteImagePath_Tick;
                //deleteImagePathTimer.Start();
            }
            catch (Exception ex)
            {
                TempLog($"DeleteScreenshot error: {ex.Message}");
            }
        }

        private async Task SaveScreenshotCommandExecute()
        {
            try
            {
                CanShowScreenshot = false;
                if (!string.IsNullOrEmpty(CurrentImagePath))
                {
                    string savedPath = Path.Combine(Path.GetTempPath(), "saved_screenshot.png");
                    File.Copy(CurrentImagePath, savedPath, true);
                    //_notificationService.Show("Screenshot Saved", "Screenshot saved successfully.");
                }
            }
            catch (Exception ex)
            {
                TempLog($"SaveScreenshot error: {ex.Message}");
            }
        }

        private async Task OpenDashboardCommandExecute()
        {
            try
            {
                string url =
                    _configuration.GetSection("ApplicationBaseUrl").Value + "#/screenshots";
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
                TempLog($"OpenDashboard error: {ex.Message}");
            }
        }

        private async Task ProductivityApplicationCommandExecute()
        {
            await ShowErrorMessage("Productivity Applications not implemented.");
        }

        private async Task TaskCompleteCommandExecute()
        {
            ButtonEventInProgress = true;
            if (string.IsNullOrEmpty(TaskName))
            {
                await ShowErrorMessage("No task selected");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                dynamic task = new { status = "Done", project = SelectedProject._id };
                var result = await _restService.CompleteATask(SelectedTask._id, task);
                if (result.data != null)
                {
                    await ShowInformationMessage("Task has been marked as completed");
                    if (SelectedProject != null)
                    {
                        await GetTaskList();
                    }
                }
            }
            catch (Exception ex)
            {
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
            if (string.IsNullOrEmpty(TaskName))
            {
                await ShowErrorMessage("Please specify task details");
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                await CreateNewTask();
            }
            catch (Exception ex)
            {
                await ShowErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private async Task TaskOpenCommandExecute()
        {
            ButtonEventInProgress = true;
            if (string.IsNullOrEmpty(TaskName))
            {
                await ShowErrorMessage("No task is selected");
                ButtonEventInProgress = false;
                return;
            }
            ProgressWidthStart = 30;
            try
            {
                var taskUrl =
                    $"{GlobalSetting.portalBaseUrl}#/home/edit-task?taskId={SelectedTask._id}";
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
            PopupForSwitchTracker = false;
            _minutesTracked = 0;
            if (_trackerIsOn)
            {
                await StartStopCommandExecute();
            }
            _totalKeysPressed = 0;
            _totalMouseClicks = 0;
            _totalMouseScrolls = 0;
            saveDispatcherTimer.Stop();
            CurrentInput = string.Empty;
        }

        private async Task SwitchTrackerYesCommandExecute()
        {
            try
            {
                PopupForSwitchTracker = false;
                _minutesTracked = 10;
                _timeLogForSwitchingMachine.makeThisDeviceActive = true;
                var result = await _restService.AddTimeLog(_timeLogForSwitchingMachine);
                _totalKeysPressed = 0;
                _totalMouseClicks = 0;
                _totalMouseScrolls = 0;
                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
            }
            catch (Exception ex)
            {
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

            var onlineStatusResult = await _restService.UpdateOnlineStatus(
                UserId,
                _machineId,
                !_trackerIsOn,
                SelectedProject?._id,
                SelectedTask?._id
            );

            if (_trackerIsOn)
            {
                TempLog($"Stopped at {DateTime.UtcNow}");
                _trackingStoppedAt = DateTime.UtcNow;
                StopTracker();
                _trackerIsOn = false;
                StartStopButtonText = "Start";
                CanShowRefresh = "Visible";
            }
            else
            {
                TempLog($"Started at {now}");
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
                    await CreateNewTask();
                }
                _trackerIsOn = true;
                StartStopButtonText = "Stop";
                CanShowRefresh = "Hidden";
            }
            return true;
        }

        private void StartTracker()
        {
            dispatcherTimer?.Start();
            usedAppDetector?.Start();
        }

        private void StopTracker()
        {
            usedAppDetector?.Stop();
            dispatcherTimer?.Stop();
        }

        private async void SaveTimeSlot_Tick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                CanShowScreenshot = false;
                var (timeLog, statusCode) = await SaveTimeSlot(CurrentImagePath);
                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    await StartStopCommandExecute();
                    await ShowErrorMessage("Unauthorized access detected. You will be logged out.");
                    await LogoutCommandExecute();
                }
                _totalKeysPressed = 0;
                _totalMouseClicks = 0;
                _totalMouseScrolls = 0;
                ShowTimeTracked(false);
                ShowCurrentTimeTracked();
                saveDispatcherTimer.Stop();
                CurrentInput = string.Empty;
            }
            catch (Exception ex)
            {
                TempLog($"SaveTimeSlot error: {ex.Message}");
            }
        }

        private async Task<string> CaptureScreen()
        {
            try
            {
                TempLog($"Screen captured at: {DateTime.UtcNow}");
                var screenshot = await _screenshotService.CaptureScreenAsync();
                CurrentImagePath = screenshot;
                CanShowScreenshot = true;
                Countdown(10, TimeSpan.FromSeconds(1), cur => CountdownTimer = $"({cur})");
                return CurrentImagePath;
            }
            catch (Exception ex)
            {
                TempLog($"CaptureScreen error: {ex.Message}");
                throw;
            }
        }

        private async void IdleTimeDetectionTimer_Tick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (_trackerIsOn || _userIsInactive)
                {
                    //var idleTime = await _mouseEventService.GetIdleTimeAsync();
                    //if (idleTime.TotalMinutes >= 4)
                    //{
                    //    await SetTrackerStatus();
                    //    CanSendReport = true;
                    //    _userIsInactive = true;
                    //    _dispatcherTimer.Stop();
                    //}
                    //else
                    //{
                    //    if (!_trackerIsOn)
                    //    {
                    //        await SetTrackerStatus();
                    //        _userIsInactive = false;
                    //        _dispatcherTimer.Start();
                    //        CanSendReport = false;
                    //        await GetCurrentSavedTime();
                    //    }
                    //    else if (!_dispatcherTimer.Enabled)
                    //    {
                    //        CanSendReport = false;
                    //        _dispatcherTimer.Start();
                    //        await GetCurrentSavedTime();
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                TempLog($"IdleTimeDetection error: {ex.Message}");
            }
        }

        private void UsedAppDetector_Tick(object? sender, ElapsedEventArgs e)
        {
            // Placeholder: Application tracking not implemented
            TempLog("UsedAppDetector triggered (not implemented).");
        }

        private async Task GetCurrentSavedTime()
        {
            _timeTrackedSaved = await GetCurrentDateTimeTracked();
        }

        private async Task<TimeSpan> GetCurrentDateTimeTracked()
        {
            var totalTime = TimeSpan.Zero;
            try
            {
                var timeLog = new TimeLog
                {
                    user = UserId,
                    date = new DateTime(
                        DateTime.UtcNow.Year,
                        DateTime.UtcNow.Month,
                        DateTime.UtcNow.Day
                    )
                };
                var result = await _restService.GetTimeLogs(timeLog);
                if (result.data != null)
                {
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
                return totalTime;
            }
            catch (Exception ex)
            {
                TempLog($"GetCurrentDateTimeTracked error: {ex.Message}");
                return totalTime;
            }
        }

        private async Task<TimeSpan?> GetCurrentWeekTimeTracked()
        {
            try
            {
                var dayOfWeekToday = (int)DateTime.UtcNow.DayOfWeek;
                var startDateOfWeek = DateTime.Today.AddDays(-1 * (dayOfWeekToday - 1));
                var totalTime = TimeSpan.Zero;
                var result = await _restService.GetCurrentWeekTotalTime(
                    new CurrentWeekTotalTime
                    {
                        user = UserId,
                        startDate = startDateOfWeek,
                        endDate = DateTime.UtcNow
                    }
                );
                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                return totalTime;
            }
            catch (Exception ex)
            {
                TempLog($"GetCurrentWeekTimeTracked error: {ex.Message}");
                return null;
            }
        }

        private async Task<TimeSpan?> GetCurrentMonthTimeTracked()
        {
            try
            {
                var totalTime = TimeSpan.Zero;
                var result = await _restService.GetCurrentWeekTotalTime(
                    new CurrentWeekTotalTime
                    {
                        user = UserId,
                        startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
                        endDate = DateTime.UtcNow
                    }
                );
                totalTime = TimeSpan.FromMinutes(result.data.Count * 10);
                return totalTime;
            }
            catch (Exception ex)
            {
                TempLog($"GetCurrentMonthTimeTracked error: {ex.Message}");
                return null;
            }
        }

        private void ShowTimeTracked(bool currentSessionSaved)
        {
            var sessionTimeTracked = TimeSpan.FromMinutes(_minutesTracked);
            CurrentSessionTimeTracked =
                $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes:D2} m";
            TimeSpan totalTimeTracked = currentSessionSaved
                ? _timeTrackedSaved
                : _timeTrackedSaved + sessionTimeTracked;
            CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes:D2} m";
        }

        private async void ShowCurrentTimeTracked()
        {
            var currentWeekTimeTracked = await GetCurrentWeekTimeTracked();
            if (currentWeekTimeTracked.HasValue)
            {
                CurrentWeekTimeTracked =
                    $"{(currentWeekTimeTracked.Value.Days * 24) + currentWeekTimeTracked.Value.Hours} hrs {currentWeekTimeTracked.Value.Minutes:D2} m";
            }

            var currentMonthTimeTracked = await GetCurrentMonthTimeTracked();
            if (currentMonthTimeTracked.HasValue)
            {
                CurrentMonthTimeTracked =
                    $"{(currentMonthTimeTracked.Value.Days * 24) + currentMonthTimeTracked.Value.Hours} hrs {currentMonthTimeTracked.Value.Minutes:D2} m";
            }
        }

        private async Task<(TimeLog timeLog, HttpStatusCode statusCode)> SaveTimeSlot(
            string filePath
        )
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string file = Convert.ToBase64String(bytes);
                var fileName = Path.GetFileName(filePath);
                var currentDate = DateTime.UtcNow;
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

                if (!CheckInternetConnectivity.IsConnectedToInternet())
                {
                    TempLog($"No internet at {DateTime.UtcNow}");
                    _unsavedTimeLogs.Add(timeLog);
                    await ShowErrorMessage("Please check your internet connectivity.");
                    return (null, HttpStatusCode.OK);
                }

                var result = await _restService.AddTimeLog(timeLog);
                if (result.statusCode == HttpStatusCode.Unauthorized)
                {
                    return (null, HttpStatusCode.Unauthorized);
                }

                TempLog($"machineId {_machineId} Message {result?.data?.message ?? ""}");
                if (!string.IsNullOrEmpty(result.data.message))
                {
                    if (
                        result.data.message.Contains(
                            "The user is logged in on another device. Would you like to make this device active?"
                        )
                    )
                    {
                        PopupForSwitchTracker = true;
                        _timeLogForSwitchingMachine = timeLog;
                        _minutesTracked = 0;
                        return (null, HttpStatusCode.OK);
                    }
                }
                else if (_unsavedTimeLogs.Count > 0)
                {
                    foreach (var unsavedTimeLog in _unsavedTimeLogs.ToArray())
                    {
                        TempLog($"Save unsaved time log: {unsavedTimeLog.startTime}");
                        await _restService.AddTimeLog(unsavedTimeLog);
                        _unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                }
                return (result?.data, result?.statusCode ?? HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                TempLog($"SaveTimeSlot error: {ex.Message}");
                return (null, HttpStatusCode.InternalServerError);
            }
        }

        private async Task GetTaskList()
        {
            ProgressWidthStart = 30;
            try
            {
                Tasks.Clear();
                if (SelectedProject != null && !string.IsNullOrEmpty(SelectedProject._id))
                {
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
                    }
                }
            }
            catch (Exception ex)
            {
                TempLog($"GetTaskList error: {ex.Message}");
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }

        private void Countdown(int count, TimeSpan interval, Action<int> ts)
        {
            var timer = new Timer(interval.TotalMilliseconds);
            timer.Elapsed += (s, e) =>
            {
                if (count-- == 1)
                {
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
            if (File.Exists(DeleteImagePath))
            {
                try
                {
                    File.Delete(DeleteImagePath);
                    DeleteImagePath = "";
                    //deleteImagePathTimer.Stop();
                }
                catch
                {
                    //deleteImagePathTimer.Stop();
                }
            }
            else
            {
                //deleteImagePathTimer.Stop();
            }
        }

        private async void BindProjectList()
        {
            Projects = null;
            try
            {
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
                }
            }
            catch (Exception ex)
            {
                TempLog($"BindProjectList error: {ex.Message}");
            }
        }

        private void PopulateUserName()
        {
            UserFullName =
                $"Welcome, {GlobalSetting.Instance.LoginResult.data.user.firstName} {GlobalSetting.Instance.LoginResult.data.user.lastName}.";
        }

        private async Task CreateNewTask()
        {
            ButtonEventInProgress = true;
            try
            {
                var taskUsers = new[] { GlobalSetting.Instance.LoginResult.data.user.id };
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
                    await ShowInformationMessage("Task has been created");
                    SelectedTask = newTaskResult.data.newTask;
                }
                else
                {
                    await ShowErrorMessage(
                        newTaskResult?.message ?? "Something went wrong while creating the task."
                    );
                }
            }
            catch (Exception ex)
            {
                TempLog($"CreateNewTask error: {ex.Message}");
            }
            finally
            {
                ButtonEventInProgress = false;
            }
        }

        private async Task CheckForUnsavedLog()
        {
            if (_unsavedTimeLogs?.Count > 0)
            {
                //var result = await MessageBox
                //    .Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                //        new MessageBox.Avalonia.DTO.MessageBoxStandardParams
                //        {
                //            ContentTitle = "Confirmation",
                //            ContentMessage =
                //                "There are some logs saved locally. Do you want to save this to the server before closing the application? Otherwise, the data will be lost.",
                //            ButtonDefinitions = MessageBox.Avalonia.Enums.ButtonEnum.YesNo,
                //            Icon = MessageBox.Avalonia.Enums.Icon.Info
                //        }
                //    )
                //    .ShowAsync();

                if (
                    1 == 1 /*result== MessageBox.Avalonia.Enums.ButtonResult.Yes*/
                )
                {
                    if (!CheckInternetConnectivity.IsConnectedToInternet())
                    {
                        await ShowErrorMessage("This needs an active internet connection");
                        return;
                    }

                    foreach (var unsavedTimeLog in _unsavedTimeLogs.ToArray())
                    {
                        var saveResult = await _restService.AddTimeLog(unsavedTimeLog);
                        TempLog($"Saved unsaved log: {saveResult?.data?.message ?? ""}");
                        if (
                            !string.IsNullOrEmpty(saveResult?.data?.message)
                            && saveResult.data.message.Contains(
                                "User is logged in on another device"
                            )
                        )
                        {
                            //var confirm = await MessageBox
                            //    .Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                            //        new MessageBox.Avalonia.DTO.MessageBoxStandardParams
                            //        {
                            //            ContentTitle = "Confirmation",
                            //            ContentMessage = saveResult.data.message,
                            //            ButtonDefinitions = MessageBox
                            //                .Avalonia
                            //                .Enums
                            //                .ButtonEnum
                            //                .YesNo,
                            //            Icon = MessageBox.Avalonia.Enums.Icon.Info
                            //        }
                            //    )
                            //    .ShowAsync();

                            //if (confirm == MessageBox.Avalonia.Enums.ButtonResult.Yes)
                            //{
                            //    unsavedTimeLog.makeThisDeviceActive = true;
                            //    await _restService.AddTimeLog(unsavedTimeLog);
                            //}
                        }
                        _unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                }
            }
        }

        private async void ShareLiveScreen_Tick(object? sender, ElapsedEventArgs e)
        {
            //try
            //{
            //    var screenshot = await _screenshotService.CaptureScreen();
            //    using var stream = new MemoryStream();
            //    await File.WriteAllBytesAsync(screenshot.Path, stream.ToArray());
            //    await _restService.sendLiveScreenDataV1(
            //        new LiveImageRequest { fileString = Convert.ToBase64String(stream.ToArray()) }
            //    );
            //}
            //catch (Exception ex)
            //{
            //    TempLog($"ShareLiveScreen error: {ex.Message}");
            //}
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
                var tempFolder = Path.Combine(Path.GetTempPath(), "TimeTrackerScreenshots");
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
            catch (Exception ex)
            {
                TempLog($"DeleteTempFolder error: {ex.Message}");
            }
        }

        private async Task SaveBrowserHistory(DateTime startDate, DateTime endDate)
        {
            // Placeholder: Browser history not implemented
            TempLog($"SaveBrowserHistory called (not implemented) from {startDate} to {endDate}");
        }

        private async Task ShowErrorMessage(string errorMessage)
        {
            MessageColor = Brushes.Red;
            ErrorMessage = errorMessage;
            await Task.Delay(TimeSpan.FromSeconds(10));
            ErrorMessage = string.Empty;
        }

        private async Task ShowInformationMessage(string message)
        {
            MessageColor = Brushes.Green;
            ErrorMessage = message;
            await Task.Delay(TimeSpan.FromSeconds(20));
            ErrorMessage = string.Empty;
        }

        private void TempLog(string message)
        {
            try
            {
                //string tempPath = Path.GetTempPath();
                //string path = Path.Combine(tempPath, $"trackerlog_{DateTime.Now:ddMMyyyy}.log");
                //using var sw = File.AppendText(path);
                //sw.WriteLine($"{DateTime.Now} Info: {message}");
            }
            catch (Exception ex)
            {
                //_notificationService.Show("Logging Error", $"Failed to log: {ex.Message}");
            }
        }

        partial void OnSelectedProjectChanged(Project oldValue, Project newValue)
        {
            // Clear existing tasks and task selection
            Tasks.Clear();
            SelectedTask = null;
            TaskName = string.Empty;
            TaskDescription = string.Empty;

            // Fetch tasks for the new project
            if (newValue != null)
            {
                _ = GetTaskList(); // Run asynchronously
            }

            // Notify UI that AllowTaskSelection may have changed
            OnPropertyChanged(nameof(AllowTaskSelection));
        }

        partial void OnSelectedTaskChanged(ProjectTask oldValue, ProjectTask newValue)
        {
            if (newValue != null)
            {
                TaskName = newValue.taskName;
                TaskDescription = newValue.description;
            }
            else
            {
                TaskName = string.Empty;
                TaskDescription = string.Empty;
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
