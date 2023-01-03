using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        MouseHook mh;
        #endregion

        #region constructor
        public TimeTrackerViewModel()
        {
            var projects = new Project();
            Projects = projects.getProjects();

            CloseCommand = new RelayCommand<CancelEventArgs>(CloseCommandExecute);
            StartStopCommand = new RelayCommand(StartStopCommandExecute, CanStartStopCommandExecute);
            EODReportsCommand = new RelayCommand(EODReportsCommandExecute);
            LogoutCommand = new RelayCommand(LogoutCommandExecute);
            RefreshCommand = new RelayCommand(RefreshCommandExecute);
            LogCommand = new RelayCommand(LogCommandExecute);
            ScreenshotCaptureSoundCommand = new RelayCommand(ScreenshotCaptureSoundCommandExecute);
            DeleteScreenshotCommand = new RelayCommand(DeleteScreenshotCommandExecute);
            SaveScreenshotCommand = new RelayCommand(SaveScreenshotCommandExecute);


            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            idlTimeDetectionTimer.Tick += IdlTimeDetectionTimer_Tick;
            idlTimeDetectionTimer.Interval = new TimeSpan(00,2,00);

            dispatcherTimer.Interval = TimeSpan.FromMinutes(9);
            UserName = GlobalSetting.Instance.LoginResult.data.user.email;
            minutesTracked = 0;

            mh = new MouseHook();
            mh.SetHook();

            mh.MouseClickEvent += mh_MouseClickEvent;
            mh.MouseDownEvent += Mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;

            InterceptKeys.OnKeyDown += InterceptKeys_OnKeyDown;
            InterceptKeys.Start();
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

        private bool canSendReport=true;

        public bool CanSendReport
        {
            get { return canSendReport; }
            set {
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

        private Project _sproject;
        public Project SProject
        {
            get { return _sproject; }
            set
            {
                _sproject = value;
                OnPropertyChanged("SProject");
                OnPropertyChanged("AllowTaskSelection");
                if (SProject != null)
                {
                    getTaskList();
                }
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

        private ProjectTask _stask;
        public ProjectTask STask
        {
            get { return _stask; }
            set
            {
                _stask = value;
                OnPropertyChanged("STask");
            }
        }
        public bool AllowTaskSelection
        {
            get { return (SProject != null); }
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
                    Application.Current.Shutdown();
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
                    Application.Current.Shutdown();
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
                LogManager.Logger.Info("Sending the report");
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
                LogManager.Logger.Error(ex);
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
            Project projects = new Project();
            Projects = projects.getProjectsv1();
        }
        public void LogCommandExecute()
        {
            var file = @$"{Environment.CurrentDirectory}\logs\{DateTime.Today.ToString("yyyyMMdd")}.log";
            Process.Start(new ProcessStartInfo { FileName = file, UseShellExecute = true });
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
            CanShowScreenshot= false;

            deleteImagePath = new DispatcherTimer();
            deleteImagePath.Tick += new EventHandler(deleteImagePath_Tick);
            deleteImagePath.Interval = new TimeSpan(0, 1, 0);
            deleteImagePath.Start();
        }
        public void SaveScreenshotCommandExecute()
        {
            CanShowScreenshot = false;
        }
        #endregion

        #region Private Methods

        private async Task<bool> SetTrackerStatus()
        {
            if (trackerIsOn)
            {
                StartStopButtontext = "Start";
                Logger.Info($"stopped at {DateTime.Now}");
                trackingStopedAt = DateTime.Now;
            }
            else
            {
                Logger.Info($"started at {DateTime.Now}");
                StartStopButtontext = "Stop";
                dispatcherTimer.Start();
                trackingStartedAt = DateTime.Now;
                minutesTracked = 0;
            }
            trackerIsOn = !trackerIsOn;
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
            ShowTimeTracked(false);
            ShowCurrentTimeTracked();
            saveDispatcherTimer.Stop();
        }

        private string CaptureScreen()
        {
            Logger.Info($"screen captured at: {DateTime.Now}");
            currentImagePath = Utilities.TimeManager.CaptureMyScreen();
            if (Properties.Settings.Default.playScreenCaptureSound)
            {
                PlayMedia.PlayScreenCaptureSound();
            }
            CanShowScreenshot = true;
            Countdown(10, TimeSpan.FromSeconds(1), cur => CountdownTimer = "(" + cur + ")".ToString());
            var screen = SystemParameters.WorkArea;
            VerticalOffset = (screen.Height - 200);
            HorizontalOffset = (screen.Width);
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
        public async void getCurrentSavedTime()
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

        private async Task<TimeSpan> GetCurrrentWeekTimeTracked()
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

        private async Task<TimeSpan> GetCurrrentMonthTimeTracked()
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

        private bool CanStartStopCommandExecute()
        {
            return !string.IsNullOrEmpty(taskName) && taskName.Length > 0;
            //return STask?.Id > 0;
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

            var sessionTimeTracked = TimeSpan.FromMinutes(minutesTracked);
            CurrentSessionTimeTracked = $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes.ToString("00")} m";
            TimeSpan totalTimeTracked = timeTrackedSaved;
            CurrentWeekTimeTracked = $" {(currentWeekTimeTracked.Days * 24) + currentWeekTimeTracked.Hours} hrs {currentWeekTimeTracked.Minutes.ToString("00")} m";
            var currentMonthTimeTracked = await GetCurrrentMonthTimeTracked();
            CurrentMonthTimeTracked = $"{(currentMonthTimeTracked.Days * 24) + currentMonthTimeTracked.Hours} hrs {currentMonthTimeTracked.Minutes.ToString("00")} m";
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
                task = Taskname,
                startTime = DateTime.Now.AddMinutes(-10),
                endTime = DateTime.Now,
                fileString = file,
                filePath = fileName,
                keysPressed = totalKeysPressed,
                clicks = totalMouseClick
            };
            try
            {
                var rest = new REST(new HttpProviders());
                var result = await rest.AddTimeLog(timeLog);
                return result;
            }
            catch (Exception ex)
            {
                unsavedTimeLogs.Add(timeLog);

                return null;
            }
        }

        private void getTaskList()
        {
            ProjectTask projectTask = new ProjectTask();
            Tasks = projectTask.getTaskByProjectId(SProject.Id);
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
        #endregion
    }
}
