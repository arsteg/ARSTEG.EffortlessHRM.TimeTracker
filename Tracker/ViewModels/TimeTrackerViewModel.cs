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

        private DateTime trackingStartedAt;
        private DateTime trackingStopedAt;
        private TimeSpan timeTrackedSaved;
        private List<TimeLog> timeLogs = new List<TimeLog>();
        private List<TimeLog> unsavedTimeLogs = new List<TimeLog>();

        private bool userIsInactive = false;
        Random rand = new Random();
        private double minutesTracked = 0;
        private int totalMouseClick = 0;
        private int totalKeysPressed = 0;
        private int totalMouseScrolls = 0;
        private string machineId = string.Empty;
        MouseHook mh;
        #endregion

        #region constructor
        public TimeTrackerViewModel()
        {
            CloseCommand = new RelayCommand<CancelEventArgs>(CloseCommandExecute);
            StartStopCommand = new RelayCommand(StartStopCommandExecute, CanStartStopCommandExecute);
            EODReportsCommand = new RelayCommand(EODReportsCommandExecute);
            LogoutCommand = new RelayCommand(LogoutCommandExecute);
            RefreshCommand = new RelayCommand(RefreshCommandExecute);
            LogCommand = new RelayCommand(LogCommandExecute);
            ScreenshotCaptureSoundCommand = new RelayCommand(ScreenshotCaptureSoundCommandExecute);
            DeleteScreenshotCommand = new RelayCommand(DeleteScreenshotCommandExecute);
            SaveScreenshotCommand = new RelayCommand(SaveScreenshotCommandExecute);
            OpenDashboardCommand = new RelayCommand(OpenDashboardCommandExecute);


            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            idlTimeDetectionTimer.Tick += IdlTimeDetectionTimer_Tick;
            idlTimeDetectionTimer.Interval = new TimeSpan(00, 2, 00);

            dispatcherTimer.Interval = TimeSpan.FromMinutes(9);
            UserName = GlobalSetting.Instance.LoginResult.data.user.email;

            usedAppDetector.Tick += new EventHandler(UsedAppDetector_Tick);
            usedAppDetector.Interval = TimeSpan.FromMinutes(10);

            minutesTracked = 0;
            CreateMachineId();

            mh = new MouseHook();
            mh.SetHook();

            mh.MouseClickEvent += mh_MouseClickEvent;
            mh.MouseDownEvent += Mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.MouseWheelEvent += mh_MouseWheelEvent;

            InterceptKeys.OnKeyDown += InterceptKeys_OnKeyDown;
            InterceptKeys.Start();

            BindProjectList();
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

        private string screenshotSoundtext = Properties.Settings.Default.playScreenCaptureSound
            ? "Turn off sound on screenshot capture"
            : "Turn on sound on screenshot capture";
        public string ScreenshotSoundtext
        {
            get { return screenshotSoundtext; }
            set
            {
                screenshotSoundtext = value;
                OnPropertyChanged(nameof(ScreenshotSoundtext));
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

        private bool canShowRefresh = true;
        public bool CanShowRefresh
        {
            get { return canShowRefresh; }
            set
            {
                canShowRefresh = value;
                OnPropertyChanged(nameof(CanShowRefresh));
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
        public RelayCommand ScreenshotCaptureSoundCommand { get; set; }
        public RelayCommand DeleteScreenshotCommand { get; set; }
        public RelayCommand SaveScreenshotCommand { get; set; }
        public RelayCommand OpenDashboardCommand { get; set; }

        ActiveApplicationPropertyThread activeWorker = new ActiveApplicationPropertyThread();

        #endregion

        #region public methods
        public async void CloseCommandExecute(CancelEventArgs args)
        {
            if (args == null)
            {
                if (trackerIsOn)
                {
                    MessageBox.Show("Please stop the tracker before closing the application.");
                    return;
                }
                else
                {
                    checkForUnsavedLog();
                }
            }
            else
            {
                if (trackerIsOn)
                {
                    MessageBox.Show("Please stop the tracker before closing the application.");
                    args.Cancel = true;
                }
                else
                {
                    checkForUnsavedLog();
                }
            }
        }
        public void LogoutCommandExecute()
        {
            if (trackerIsOn)
            {
                StartStopCommandExecute();
            }
            var login = new TimeTracker.Views.Login();
            login.Show();
            GlobalSetting.Instance.TimeTracker.Close();
        }
        public async void StartStopCommandExecute()
        {
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
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }
        public async void EODReportsCommandExecute()
        {
            ProgressWidthReport = 30;
            try
            {
                AddErrorLog("Info", "Sending the report");
                await Task.Run(() =>
                {
                    var historyEntries = BrowserHistory.GetHistoryEntries();
                    EmailService emailService = new EmailService();
                    var sendGridKey = configuration.GetSection("AppSettings:SendGridKey").Value;
                    var senderEmail = configuration.GetSection("AppSettings:SenderEmail").Value;
                    var emailReceiver = GlobalSetting.EmailReceiver;
                    var subject = $"Daily Report - {GlobalSetting.Instance.LoginResult.data.user.email}";
                    var msgbody = GetEMailBody(historyEntries);
                    if (msgbody != "" && msgbody != null)
                    {
                        var receivers = emailReceiver.Split(",");
                        foreach (var receiver in receivers)
                        {
                            var result = emailService.SendEmailAsyncToRecipientUsingSendGrid(sendGridKey, senderEmail, receiver, "", "", subject, msgbody).GetAwaiter().GetResult();
                        }
                        MessageBox.Show("Report has been emailed.");
                    }
                });
            }
            catch (Exception ex)
            {
                AddErrorLog("Error", $"Message: {ex?.Message} ex.StackTrace:{ex?.StackTrace} InnerException: {ex?.InnerException?.InnerException}");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ProgressWidthReport = 0;
            }
        }
        private string GetEMailBody(List<HistoryEntry> historyEntries)
        {
            try
            {
                var totalTimeTracked = GetCurrrentdateTimeTracked().Result;

                var imageshtml = "";

                imageshtml += $"<div><hr></div>";

                imageshtml += $"<div>Total time tracked {totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes} minutes</div>";

                imageshtml += $"<div><hr></div>";

                imageshtml += "<div><strong> Screenshots Taken</strong></div>";

                imageshtml += $"<div><hr></div>";

                var folderPath = @$"{Environment.CurrentDirectory}\{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var files = System.IO.Directory.GetFiles(folderPath).ToList();
                var html = imageshtml;
                html += @"<table border=1>";
                for (int i = 0; i < 24; i++)
                {
                    html += "<tr>";
                    html += $"<td>{i.ToString("00")}:{00}</td>";
                    for (int j = 0; j < 6; j++)
                    {
                        var file = getFileName(i, j, files);
                        if (file != "")
                        {
                            var filename = Path.GetFileName(file);
                            var imageContent = File.ReadAllBytes(file);
                            var htmlImg = @$"<div><img src='cid:{filename}' alt='Captured image file' height='200' width='200'/></div>";
                            html += $"<td style='min- idth:50px'> {htmlImg} </td>";
                        }
                        else
                        {
                            html += $"<td style='min-width:150px'></td>";
                        }
                    }
                    html += "</tr>";
                }

                html += @"<h3>Browsing History</h3>";

                html += @"</table>";

                html += @"<table border=1>";

                html += "<tr>";

                html += $"<th>Title</th>";

                html += $"<th>Uri</th>";

                html += $"<th>Visit Count </th>";

                html += "</tr>";


                foreach (var historyEntry in historyEntries)
                {
                    html += "<tr>";

                    html += $"<td> {historyEntry.Title} </td>";

                    html += $"<td> {historyEntry.Uri} </td>";

                    html += $"<td> {historyEntry.VisitCount} </td>";

                    html += "</tr>";
                }
                html += @"</table>";


                return $"<html><body>{html}</body></html>";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return "";
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
            BindProjectList();
            Tasks = null;
            taskName = string.Empty;
            SelectedTask = null;
        }
        public async void LogCommandExecute()
        {
            var rest = new REST(new HttpProviders());
            var errorLogList = await rest.GetErrorLogs(GlobalSetting.Instance.LoginResult.data.user.id);
            if (errorLogList != null && errorLogList?.status.ToUpper() == "SUCCESS" && errorLogList?.data?.errorLogList?.Count > 0)
            {
                if (errorLogList.data.errorLogList.Any(e => e.createdOn.Date == DateTime.Now.Date))
                {
                    var errors = errorLogList.data.errorLogList.Where(e => e.createdOn.Date == DateTime.Now.Date);
                    ShowLog(errors);
                }
                else
                {
                    MessageBox.Show("Log not found");
                }
            }
            else
            {
                MessageBox.Show("Log not found");
            }
        }
        public void ScreenshotCaptureSoundCommandExecute()
        {
            var playScreenCaptureSound = Properties.Settings.Default.playScreenCaptureSound;
            if (playScreenCaptureSound)
            {
                ScreenshotSoundtext = "Turn on sound on screenshot capture";
            }
            else
            {
                ScreenshotSoundtext = "Turn off sound on screenshot capture";
            }
            Properties.Settings.Default.playScreenCaptureSound = !playScreenCaptureSound;
            Properties.Settings.Default.Save();
        }
        public void DeleteScreenshotCommandExecute()
        {
            DeleteImagePath = CurrentImagePath;
            CurrentImagePath = null;
            saveDispatcherTimer.Stop();
            CanShowScreenshot = false;

            deleteImagePath = new DispatcherTimer();
            deleteImagePath.Tick += new EventHandler(deleteImagePath_Tick);
            deleteImagePath.Interval = new TimeSpan(0, 1, 0);
            deleteImagePath.Start();
        }
        public void SaveScreenshotCommandExecute()
        {
            CanShowScreenshot = false;
        }
        public void OpenDashboardCommandExecute()
        {
            string url = configuration.GetSection("ApplicationBaseUrl").Value + "#/screenshots";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }
        #endregion

        #region Private Methods

        private async Task<bool> SetTrackerStatus()
        {
            if (trackerIsOn)
            {
                StartStopButtontext = "Start";
                AddErrorLog("Info", $"stopped at {DateTime.Now}");
                trackingStopedAt = DateTime.Now;
                usedAppDetector.Stop();
            }
            else
            {
                AddErrorLog("Info", $"started at {DateTime.Now}");
                StartStopButtontext = "Stop";
                dispatcherTimer.Start();
                trackingStartedAt = DateTime.Now;
                minutesTracked = 0;
                if (SelectedTask == null && taskName.Length > 0)
                {
                    CreateNewTask();
                }
                StartApplicationTracker();
                usedAppDetector.Start();
            }
            trackerIsOn = !trackerIsOn;
            CanShowRefresh = !trackerIsOn;
            return true;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (trackerIsOn)
            {
                var currentMinutes = DateTime.Now.Minute;
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
            }
        }
        private void saveTimeSlot_Tick(object sender, EventArgs e)
        {
            CanShowScreenshot = false;
            var task = Task.Run(async () => await SaveTimeSlot(CurrentImagePath));
            task.Wait();
            totalKeysPressed = 0;
            totalMouseClick = 0;
            totalMouseScrolls = 0;
            ShowTimeTracked(false);
            ShowCurrentTimeTracked();
            saveDispatcherTimer.Stop();
        }
        private string CaptureScreen()
        {
            AddErrorLog("Info", $"screen captured at: {DateTime.Now}");
            currentImagePath = Utilities.TimeManager.CaptureMyScreen();
            if (Properties.Settings.Default.playScreenCaptureSound)
            {
                PlayMedia.PlayScreenCaptureSound();
            }
            CanShowScreenshot = true;
            Countdown(10, TimeSpan.FromSeconds(1), cur => CountdownTimer = "(" + cur + ")".ToString());
            return currentImagePath;
        }
        private void IdlTimeDetectionTimer_Tick(object sender, EventArgs e)
        {
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
                    }
                }
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
                    user = UserName,
                    date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)
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
                return totalTime;
            }
        }

        private async Task<TimeSpan?> GetCurrrentWeekTimeTracked()
        {
            try
            {
                var dayOfWeekToday = (int)DateTime.Today.DayOfWeek;
                var startDateOfWeek = DateTime.Today.AddDays(-1 * (dayOfWeekToday - 1));
                var totalTime = new TimeSpan();
                var rest = new REST(new HttpProviders());
                var result = await rest.GetCurrentWeekTotalTime(new CurrentWeekTotalTime()
                {
                    user = UserName,
                    startDate = new DateTime(startDateOfWeek.Date.Year, startDateOfWeek.Date.Month, startDateOfWeek.Date.Day),
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day),
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
                    user = UserName,
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Date.Month, 1),
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Date.Month, DateTime.Now.Date.Day),
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

        private async Task<TimeLog> SaveTimeSlot(string filePath)
        {
            Byte[] bytes = File.ReadAllBytes(filePath);
            String file = Convert.ToBase64String(bytes);
            var fileName = Path.GetFileName(filePath);
            var currentDate = DateTime.Now;
            var timeLog = new TimeLog()
            {
                user = UserName,
                date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day),
                task = SelectedTask._id,
                startTime = DateTime.Now.AddMinutes(-10),
                endTime = DateTime.Now,
                fileString = file,
                filePath = fileName,
                keysPressed = totalKeysPressed,
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
                    AddErrorLog("Info", $"Please check your internet connectivity.");
                    unsavedTimeLogs.Add(timeLog);
                    Task.Run(() =>
                    {
                        MessageBox.Show("Please check your internet connectivity.");
                    });
                    return null;
                }
                var rest = new REST(new HttpProviders());
                var result = rest.AddTimeLog(timeLog).GetAwaiter().GetResult();
                AddErrorLog("Info", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                if (!string.IsNullOrEmpty(result.data.message))
                {
                    if (result.data.message.Contains("User is logged in on another device, Do you want to make it active?"))
                    {
                        var isConfirm = MessageBox.Show(new Form() { TopMost = true }, $"{result.data.message}", "Confirmation", MessageBoxButtons.YesNo);
                        if (isConfirm == DialogResult.Yes)
                        {
                            timeLog.makeThisDeviceActive = true;
                            result = await rest.AddTimeLog(timeLog);
                        }
                        else
                        {
                            if (trackerIsOn)
                            {
                                StartStopCommandExecute();
                            }
                        }
                    }
                }
                else
                {
                    var tempUnsavedTimeLogs = unsavedTimeLogs;
                    foreach (var unsavedTimeLog in tempUnsavedTimeLogs.ToArray())
                    {
                        AddErrorLog("Info", $"Save unsaved log from SaveTimeSlot");
                        var unsavedLogResult = await rest.AddTimeLog(unsavedTimeLog);
                        unsavedTimeLogs.Remove(unsavedTimeLog);
                    }
                    tempUnsavedTimeLogs = null;
                }
                return result.data;
            }
            catch (Exception ex)
            {
                AddErrorLog("SaveTimeSlot Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                return null;
            }
        }

        private async void getTaskList()
        {
            Tasks = null;
            if (SelectedProject != null && SelectedProject._id.Length > 0)
            {
                var rest = new REST(new HttpProviders());
                var taskList = await rest.GetTaskListByProject(SelectedProject._id);

                if (taskList.status == "success" && taskList.data != null)
                {
                    Tasks = taskList.data.taskList;
                }
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
                var rest = new REST(new HttpProviders());
                rest.AddErrorLogs(new ErrorLog()
                {
                    error = error,
                    details = details
                });
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

        private async void CreateNewTask()
        {
            var taskUser = new List<TaskUser>();
            taskUser.Add(new TaskUser()
            {
                user = GlobalSetting.Instance.LoginResult.data.user.id
            });
            var rest = new REST(new HttpProviders());
            var newTaskResult = await rest.AddNewTask(new CreateTaskRequest
            {
                taskName = taskName,
                comment = "Created from tracker",
                project = SelectedProject._id,
                taskUsers = taskUser
            });

            if (newTaskResult.status.ToUpper() == "SUCCESS")
            {
                SelectedTask = newTaskResult.data.newTask;
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
                    sw.WriteLine("{0}", $"{errorLog.createdOn} {errorLog.error} {errorLog.details}");
                }
                sw.Flush();
                sw.Close();

                Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AddErrorLog("Error", $"Message: {ex?.Message} StackTrace: {ex?.StackTrace} innerException: {ex?.InnerException?.InnerException}");
                MessageBox.Show(ex.Message);
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
            activeWorker.StopThread(false);
            var focusedApplication = activeWorker.activeApplicationInfomationCollector._focusedApplication;
            TempStoreApplicationUsed(focusedApplication);
            if (focusedApplication != null)
            {
                foreach (var key in focusedApplication?.Keys)
                {
                    await AddUsedApplicationLog(new ApplicationLog
                    {
                        appWebsite = key,
                        type = "App",
                        ApplicationTitle = focusedApplication[key].AppTitle,
                        projectReference = SelectedProject._id,
                        userReference = GlobalSetting.Instance.LoginResult.data.user.id,
                        date = DateTime.Now,
                        inactive = focusedApplication[key].TotalIdletime,
                        keyboardStrokes = focusedApplication[key].TotalKeysPressed,
                        mouseClicks = focusedApplication[key].TotalMouseClick,
                        scrollingNumber = focusedApplication[key].TotalMouseScrolls,
                        ModuleName = SelectedProject.projectName,
                        TimeSpent = focusedApplication[key].Duration,
                        total = focusedApplication[key].Duration
                    });
                }
            }
        }

        private async Task AddUsedApplicationLog(ApplicationLog applicationLog)
        {
            try
            {
                var rest = new REST(new HttpProviders());
                var usedApp = await rest.AddUsedApplicationLog(applicationLog);
            }
            catch (Exception ex)
            {
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
            if (unsavedTimeLogs?.Count > 0)
            {
                var isConfirm = MessageBox.Show($"There are some logs saved locally. Do you want to save this to the server before closing the application? Otherwise, the data will be lost.", "Confirmation", MessageBoxButtons.YesNo);
                if (isConfirm == DialogResult.Yes)
                {
                    if (!CheckInternetConnectivity.IsConnectedToInternet())
                    {
                        MessageBox.Show("This needs an active internet connection");
                        
                    }
                    var rest = new REST(new HttpProviders());

                    var tempUnsavedTimeLogs = unsavedTimeLogs;
                    foreach (var unsavedTimeLog in tempUnsavedTimeLogs.ToArray())
                    {
                        var result = await rest.AddTimeLog(unsavedTimeLog);
                        AddErrorLog("Info saved from checkForUnsavedLog()", $"machineId {machineId} Message {result?.data?.message ?? ""}");
                        if (!string.IsNullOrEmpty(result?.data?.message))
                        {
                            if (result.data.message.Contains("User is logged in on another device, Do you want to make it active?"))
                            {
                                if (MessageBox.Show(new Form() { TopMost = true }, $"{result.data.message}", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    unsavedTimeLog.makeThisDeviceActive = true;
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
            else
            {
                SystemWindows.Application.Current.Shutdown();
                
            }
        }
        #endregion
    }
}
