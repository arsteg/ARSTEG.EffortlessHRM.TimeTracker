using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TimeTracker.ActivityTracker;
using TimeTracker.Utilities;

namespace TimeTracker.AppUsedTracker
{
    public class ActiveApplicationPropertyThread
    {
        MouseHook mh;
        private int appWiseTotalMouseClick = 0;
        private int appWiseTotalKeysPressed = 0;
        private int appWiseTotalMouseScrolls = 0;
        private double appTotalIdletime = 0;
        private DateTime lastInputTime = new DateTime();
        private double lastInputIdletime = 0;
        WinEventDelegate dele = null;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        private DispatcherTimer idleTimeDetect = new DispatcherTimer();

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);





        private bool stopThread = false;
        private string _appName = string.Empty;
        private string _appTitle = string.Empty;


        [DllImport("user32.dll")]
        private static extern UInt32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public ActiveApplicationInfomationCollector activeApplicationInfomationCollector;
        private string _justSentApplicationName = "";
        public ActiveApplicationPropertyThread()
        {
            activeApplicationInfomationCollector = new ActiveApplicationInfomationCollector();

            dele = new WinEventDelegate(WinEventProc);
            IntPtr m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

            mh = new MouseHook();
            mh.SetHook();

            mh.MouseClickEvent += mh_MouseClickEventApp;    
            mh.MouseDownEvent += Mh_MouseDownEventApp;
            mh.MouseUpEvent += mh_MouseUpEventApp;
            mh.MouseWheelEvent += mh_MouseWheelEventApp;

            InterceptKeys.OnKeyDown += InterceptKey_OnKeyDown;
            InterceptKeys.Start();

            idleTimeDetect.Tick += IdleTimeDetect_Tick;
            idleTimeDetect.Interval = new TimeSpan(00, 00, 30);

        }

        private void IdleTimeDetect_Tick(object sender, EventArgs e)
        {
            CalculateIdleTimeAppWise();
        }

        private void Mh_MouseDownEventApp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            appWiseTotalMouseClick++;
        }

        private void mh_MouseUpEventApp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }
        private void mh_MouseClickEventApp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }
        private void mh_MouseWheelEventApp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            appWiseTotalMouseScrolls++;
        }

        private void InterceptKey_OnKeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            appWiseTotalKeysPressed++;
        }

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            StartThread();
        }

        public void StopThread(bool startThread)
        {
            if (startThread)
            {
                activeApplicationInfomationCollector = new ActiveApplicationInfomationCollector();
                _justSentApplicationName = "";
                StartEndFocushedApplication();
                idleTimeDetect.Start();
            }
            else
            {
                CalculateIdleTimeAppWise();
                activeApplicationInfomationCollector.ApplicationFocusEnd(_justSentApplicationName, appWiseTotalMouseClick, appWiseTotalKeysPressed, appWiseTotalMouseScrolls, appTotalIdletime);
                ResetTrackerCount();
                idleTimeDetect.Stop();
            }
            stopThread = startThread;
        }

        public void StartThread()
        {
            if (stopThread)
            {
                StartEndFocushedApplication();
            }
        }

        public string AppInfo
        {
            get { return _appName; }
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return string.Empty;
        }
        private void GetActiveProcess()
        {
            try
            {
                Process[] AllProcess = Process.GetProcesses();
                string title = GetActiveWindowTitle();

                if (!string.IsNullOrEmpty(title))
                {
                    var forgroundWindows = AllProcess.Where(p => p.MainWindowTitle == title).FirstOrDefault();
                    if (forgroundWindows != null)
                    {
                        _appTitle = title;
                        _appName = forgroundWindows.MainModule?.ModuleName;
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }
        private void StartEndFocushedApplication()
        {
            GetActiveProcess();
            if (!string.IsNullOrEmpty(_appName))
            {
                if (_justSentApplicationName == "")
                {
                    _justSentApplicationName = _appName;
                    ResetTrackerCount();
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                }
                if (_justSentApplicationName != _appName)
                {
                    activeApplicationInfomationCollector.ApplicationFocusEnd(_justSentApplicationName, appWiseTotalMouseClick, appWiseTotalKeysPressed, appWiseTotalMouseScrolls, appTotalIdletime);
                    ResetTrackerCount();
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                    _justSentApplicationName = _appName;
                }
            }
        }

        private void ResetTrackerCount()
        {
            lastInputTime = new DateTime();
            lastInputIdletime = 0;
            appTotalIdletime = 0;
            appWiseTotalKeysPressed = 0;
            appWiseTotalMouseClick = 0;
            appWiseTotalMouseScrolls = 0;
        }

        private void CalculateIdleTimeAppWise()
        {
            var idleTime = IdleTimeDetector.GetIdleTimeInfo();
            if (idleTime.IdleTime.TotalSeconds > 0)
            {
                if (idleTime.LastInputTime.ToString("ddMMyyyyHHmmss") == lastInputTime.ToString("ddMMyyyyHHmmss"))
                {
                    appTotalIdletime = appTotalIdletime - lastInputIdletime;
                }
                appTotalIdletime += idleTime.IdleTime.TotalMilliseconds;
                lastInputIdletime = idleTime.IdleTime.TotalMilliseconds;
                lastInputTime = idleTime.LastInputTime;
            }
        }
    }
}
