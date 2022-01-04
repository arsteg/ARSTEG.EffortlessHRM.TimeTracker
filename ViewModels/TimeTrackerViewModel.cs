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

        #endregion

        #region constructor
        public TimeTrackerViewModel() {
            CloseCommand = new RelayCommand(CloseCommandExecute);
            StartStopCommand = new RelayCommand(StartStopCommandExecute, CanStartStopCommandExecute);
            EODReportsCommand = new RelayCommand(EODReportsCommandExecute);
            LogoutCommand = new RelayCommand(LogoutCommandExecute);


            configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            idlTimeDetectionTimer.Tick += IdlTimeDetectionTimer_Tick;
            idlTimeDetectionTimer.Interval = new TimeSpan(00,1,00);
            var screenCaptureTimeInterval = configuration.GetSection("AppSettings:ScreenCaptureTimeInterval");
            dispatcherTimer.Interval = TimeSpan.Parse(screenCaptureTimeInterval.Value);
            UserName = GlobalSetting.Instance.LoginResult.data.user.email;            
        }        
        #endregion

        #region Public Properties

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

        #endregion

        #region commands
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand StartStopCommand { get; set; }
        public RelayCommand EODReportsCommand { get; set; }
        public RelayCommand LogoutCommand { get; set; }

        #endregion 
        
        #region public methods
        public void CloseCommandExecute()
        {           
            Application.Current.Shutdown();
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
            timeTrackedSaved = await GetCurrrentdatTimeTracked();
            ShowTimeTracked(true);
        }
        public void EODReportsCommandExecute()
        {
            try
            {
                Task.Run(() =>
                {
                    EmailService emailService = new EmailService();
                    var sendGridKey = configuration.GetSection("AppSettings:SendGridKey").Value;
                    var senderEmail = configuration.GetSection("AppSettings:SenderEmail").Value;
                    var subject = $"Daily Report - {GlobalSetting.Instance.LoginResult.data.user.email}";
                    var msgbody = GetEMailBody();
                    var result = emailService.SendEmailAsyncToRecipientUsingSendGrid(sendGridKey, senderEmail, "info@arsteg.com", "", "", subject, msgbody).GetAwaiter().GetResult();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private string GetEMailBody()
        {
            var totalTimeTracked = GetCurrrentdatTimeTracked().Result;
            
            var imageshtml = "";
            
            imageshtml += $"<div><hr></div>";

            imageshtml += $"<div>Total time tracked {totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes} minutes</div>";

            imageshtml += $"<div><hr></div>";

            imageshtml += "<div><strong> Screenshots Taken</strong></div>";

            imageshtml += $"<div><hr></div>";

            var folderPath = @$"{Environment.CurrentDirectory}\{DateTime.Now.ToString("yyyy-MM-dd")}";
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
                trackingStopedAt = DateTime.Now;
                var rest = new REST(new HttpProviders());

                var result = await rest.AddTimeLog(new TimeLog()
                {
                    user = UserName,
                    date = DateTime.Today,
                    task = Taskname,
                    startTime = trackingStartedAt,
                    endTime = trackingStopedAt
                });

                var results = await rest.GetTimeLogs(new TimeLog()
                {
                    user = UserName,
                    date = DateTime.Today,
                    task = Taskname,
                    startTime = trackingStartedAt,
                    endTime = trackingStopedAt
                });
            }
            else
            {
                StartStopButtontext = "Stop";
                dispatcherTimer.Start();
                trackingStartedAt = DateTime.Now;
            }
            trackerIsOn = !trackerIsOn;
            return true;
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ShowTimeTracked(false);
            captureScreen();
        }
        private void captureScreen()
        {
            CurrentImagePath = Utilities.TimeManager.CaptureMyScreen();
        }
        private void IdlTimeDetectionTimer_Tick(object sender, EventArgs e)
        {
            if (trackerIsOn)
            {
                var idleTime = IdleTimeDetector.GetIdleTimeInfo();

                if (idleTime.IdleTime.TotalMinutes >= 10)
                {
                    SetTrackerStatus();
                }
                else
                {
                    if (!trackerIsOn)
                    {
                        SetTrackerStatus();
                    }
                }
            }
        }
        private async Task<TimeSpan> GetCurrrentdatTimeTracked()
        {
            var totalTime = new TimeSpan();
            var rest = new REST(new HttpProviders());
            var result = await rest.GetTimeLogs(new TimeLog()
            {
                user = UserName,
                date = DateTime.Today,
                task = Taskname,
                startTime = trackingStartedAt,
                endTime = trackingStopedAt
            });
            
            foreach (var timeLog in result.data.timeLogs) {
                var trackedTime = timeLog.endTime.Subtract(timeLog.startTime);
                totalTime+= new TimeSpan(trackedTime.Hours, trackedTime.Minutes, trackedTime.Seconds);
            }
            return totalTime;
        }
        private bool CanStartStopCommandExecute() {
            return !string.IsNullOrEmpty(taskName) && taskName.Length > 0; 
        }        
        private void ShowTimeTracked(bool currentSessionSaved= false )
        {
            var sessionTimeTracked = DateTime.Now.Subtract(trackingStartedAt);
            CurrentSessionTimeTracked = $"{sessionTimeTracked.Hours} hrs {sessionTimeTracked.Minutes.ToString("00")} m";
            if (currentSessionSaved) {
                TimeSpan totalTimeTracked = timeTrackedSaved ;
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }
            else 
            {
                TimeSpan totalTimeTracked = (timeTrackedSaved + sessionTimeTracked);
                CurrentDayTimeTracked = $"{totalTimeTracked.Hours} hrs {totalTimeTracked.Minutes.ToString("00")} m";
            }            
        }
        #endregion
    }
}
