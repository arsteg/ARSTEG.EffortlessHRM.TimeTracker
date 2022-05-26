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

namespace TimeTracker.ViewModels
{
    public class TimeTrackerViewModel: ViewModelBase
    {
        #region  private members        
        private IConfiguration configuration;
        private bool trackerIsOn = false;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private DispatcherTimer idlTimeDetectionTimer = new DispatcherTimer();

        private DateTime  trackingStartedAt;
        private DateTime  trackingStopedAt;
        private TimeSpan timeTrackedSaved;
        private List<TimeLog> timeLogs = new List<TimeLog>();
        private List<TimeLog> unsavedTimeLogs = new List<TimeLog>();

        private bool userIsInactive = false;
        Random rand = new Random();
        private double minutesTracked = 0;        
        private double totalMouseClick= 0;
        private double totalKeysPressed= 0;
        MouseHook mh;       
        #endregion

        #region constructor
        public TimeTrackerViewModel() {
            CloseCommand = new RelayCommand<CancelEventArgs>(CloseCommandExecute);
            StartStopCommand = new RelayCommand(StartStopCommandExecute, CanStartStopCommandExecute);
            EODReportsCommand = new RelayCommand(EODReportsCommandExecute);
            LogoutCommand = new RelayCommand(LogoutCommandExecute);


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
            Title = "Effortless-HRM";
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


        private string startStopButtontext="Start";        
        public string StartStopButtontext
        {
            get { return startStopButtontext; }
            set { 
                startStopButtontext = value;
                OnPropertyChanged(nameof(StartStopButtontext));
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

        private String currentSessionTimeTracked;
        public string CurrentSessionTimeTracked
        {
            get { return currentSessionTimeTracked;}
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

        #endregion

        #region commands
        public RelayCommand<CancelEventArgs> CloseCommand { get; set; }
        public RelayCommand StartStopCommand { get; set; }
        public RelayCommand EODReportsCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        #endregion 
        
        #region public methods
        public async void CloseCommandExecute(CancelEventArgs args)
        {
           if(args==null)
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
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                ProgressWidthStart = 0;
            }
        }
        public async  void EODReportsCommandExecute()
        {
            ProgressWidthReport = 30;
            try
            {
                LogManager.Logger.Info("Sending the report");
                await Task.Run(() =>
                {
                    EmailService emailService = new EmailService();
                    var sendGridKey = configuration.GetSection("AppSettings:SendGridKey").Value;
                    var senderEmail = configuration.GetSection("AppSettings:SenderEmail").Value;
                    var emailReceiver = configuration.GetSection("EmailReceiver").Value;
                    var subject = $"Daily Report - {GlobalSetting.Instance.LoginResult.data.user.email}";
                    var msgbody = GetEMailBody();
                    if (msgbody!="" && msgbody != null)
                    {
                        var receivers = emailReceiver.Split(",");
                        foreach(var receiver in receivers)
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
            finally{
                ProgressWidthReport = 0;
            }
        }

        private string GetEMailBody()
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
                html += @"</table>";
                return $"<html><body>{html}</body></html>";
            }
            catch(Exception ex)
            {                
                MessageBox.Show(ex.Message);
                return "";
            }
        }

        private static string getFileName(int r, int c, List<string> files)
        {
            var result = "";
            foreach(var file in files)
            {               
                var fileName = Path.GetFileNameWithoutExtension(file);
                var hh = int.Parse(fileName.Split('-').FirstOrDefault());
                var mm = int.Parse(fileName.Split('-').LastOrDefault());
                if (hh==r && mm <= (c * 10 + 9) && mm>=(c*10))
                {
                    result = file;
                }
            }
            return result;
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
            }
            trackerIsOn = !trackerIsOn;
            return true;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var currentMinutes = DateTime.Now.Minute;
            minutesTracked += 10;                        
            var randonTime = (rand.Next(2, 9));            
            double forTimerInterval = ((currentMinutes - (currentMinutes % 10)) + 10 + randonTime) - currentMinutes; 
            dispatcherTimer.Interval = TimeSpan.FromMinutes(forTimerInterval);
            
            var task = Task.Run(async () => await SaveTimeSlot());
            task.Wait();
                        
            ShowTimeTracked(false);            
            captureScreen();
        }
        private void captureScreen()
        {
            Logger.Info($"screen captured at: {DateTime.Now}");
            CurrentImagePath = Utilities.TimeManager.CaptureMyScreen();
            bool playScreenCaptureSound = false;
            bool.TryParse(configuration.GetSection("PlayScreenCaptureSound").Value, out playScreenCaptureSound);
            if (playScreenCaptureSound)
            {
                PlayMedia.PlayScreenCaptureSound();
            }
        }
        private void IdlTimeDetectionTimer_Tick(object sender, EventArgs e)
        {
            if (trackerIsOn || userIsInactive)
            {
                var idleTime = IdleTimeDetector.GetIdleTimeInfo();

                if (idleTime.IdleTime.TotalMinutes >= 2)
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
                    }
                }
            }
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
                    date = DateTime.Today,
                    task = Taskname,
                    startTime = trackingStartedAt,
                    endTime = trackingStopedAt
                });

                foreach (var timeLog in result.data.timeLogs)
                {
                    var trackedTime = timeLog.endTime.Subtract(timeLog.startTime);
                    totalTime += new TimeSpan(trackedTime.Hours, trackedTime.Minutes, trackedTime.Seconds);
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
            var day = (int)DateTime.Today.DayOfWeek;

            var startDate = DateTime.Now.AddDays(-1 * day);

            var totalTime = new TimeSpan();
            try
            {
                var rest = new REST(new HttpProviders());
                var result = await rest.GetTimeLogs(new TimeLog()
                {
                    user = UserName,
                    date = DateTime.Today,
                    task = Taskname,
                    startTime = trackingStartedAt,
                    endTime = trackingStopedAt
                });

                foreach (var timeLog in result.data.timeLogs)
                {
                    var trackedTime = timeLog.endTime.Subtract(timeLog.startTime);
                    totalTime += new TimeSpan(trackedTime.Hours, trackedTime.Minutes, trackedTime.Seconds);
                }
                return totalTime;
            }
            catch (Exception ex)
            {
                return totalTime;
            }
        }
        private bool CanStartStopCommandExecute() {
            return !string.IsNullOrEmpty(taskName) && taskName.Length > 0; 
        }        
        private void ShowTimeTracked(bool currentSessionSaved= false )
        {
            var sessionTimeTracked = TimeSpan.FromMinutes(minutesTracked);
            CurrentSessionTimeTracked = $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes.ToString("00")} m";
            if (currentSessionSaved) {
                TimeSpan totalTimeTracked = timeTrackedSaved ;
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
            else 
            {
                TimeSpan totalTimeTracked = timeTrackedSaved + TimeSpan.FromMinutes(minutesTracked);// (timeTrackedSaved + sessionTimeTracked);
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }            
        }
        
        private async Task<TimeLog> SaveTimeSlot()
        {
            var timeLog = new TimeLog()
            {
                user = UserName,
                date = DateTime.Today,
                task = Taskname,
                startTime = DateTime.Now.AddMinutes(-10),
                endTime = DateTime.Now
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
        #endregion
    }
}
