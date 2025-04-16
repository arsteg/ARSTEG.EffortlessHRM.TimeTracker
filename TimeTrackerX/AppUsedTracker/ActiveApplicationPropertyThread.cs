using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using TimeTrackerX.ActivityTracker;

namespace TimeTrackerX.AppUsedTracker
{
    public class ActiveApplicationPropertyThread
    {
        private readonly MouseHook _mh;
        private int _appWiseTotalMouseClick = 0;
        private int _appWiseTotalKeysPressed = 0;
        private int _appWiseTotalMouseScrolls = 0;
        private double _appTotalIdletime = 0;
        private DateTime _lastInputTime = new DateTime();
        private double _lastInputIdletime = 0;
        private readonly WinEventDelegate _dele;
        private delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        );
        private readonly DispatcherTimer _idleTimeDetect = new DispatcherTimer();

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags
        );

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private bool _stopThread = false;
        private string _appName = string.Empty;
        private string _appTitle = string.Empty;

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public ActiveApplicationInfomationCollector activeApplicationInfomationCollector;
        private string _justSentApplicationName = "";

        public ActiveApplicationPropertyThread()
        {
            activeApplicationInfomationCollector = new ActiveApplicationInfomationCollector();
            _dele = WinEventProc;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                IntPtr m_hhook = SetWinEventHook(
                    EVENT_SYSTEM_FOREGROUND,
                    EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero,
                    _dele,
                    0,
                    0,
                    WINEVENT_OUTOFCONTEXT
                );
            }
            else
            {
                Console.WriteLine("WinEventHook not supported on this platform.");
            }

            _mh = new MouseHook();
            _mh.Start();

            _mh.MouseClickEvent += Mh_MouseClickEventApp;
            _mh.MouseDownEvent += Mh_MouseDownEventApp;
            _mh.MouseUpEvent += Mh_MouseUpEventApp;
            _mh.MouseWheelEvent += Mh_MouseWheelEventApp;

            // Uncomment when InterceptKeys is ready
            // InterceptKeys.OnKeyDown += InterceptKey_OnKeyDown;
            // InterceptKeys.Start();

            _idleTimeDetect.Tick += IdleTimeDetect_Tick;
            _idleTimeDetect.Interval = TimeSpan.FromSeconds(30);
        }

        private void IdleTimeDetect_Tick(object sender, EventArgs e)
        {
            CalculateIdleTimeAppWise();
        }

        private void Mh_MouseDownEventApp(object sender, MouseHook.MouseEventArgs e)
        {
            if (e.Button != MouseButton.None)
                _appWiseTotalMouseClick++;
        }

        private void Mh_MouseUpEventApp(object sender, MouseHook.MouseEventArgs e)
        {
            // No-op
        }

        private void Mh_MouseClickEventApp(object sender, MouseHook.MouseEventArgs e)
        {
            // No-op
        }

        private void Mh_MouseWheelEventApp(object sender, MouseHook.MouseEventArgs e)
        {
            _appWiseTotalMouseScrolls++;
        }

        private void InterceptKey_OnKeyDown(object sender, InterceptKeys.KeyEventArgs e)
        {
            if (e.Key != Key.None)
                _appWiseTotalKeysPressed++;
        }

        public void WinEventProc(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        )
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
                _idleTimeDetect.Start();
            }
            else
            {
                CalculateIdleTimeAppWise();
                activeApplicationInfomationCollector.ApplicationFocusEnd(
                    _justSentApplicationName,
                    _appWiseTotalMouseClick,
                    _appWiseTotalKeysPressed,
                    _appWiseTotalMouseScrolls,
                    _appTotalIdletime
                );
                ResetTrackerCount();
                _idleTimeDetect.Stop();
            }
            _stopThread = startThread;
        }

        public void StartThread()
        {
            if (_stopThread)
            {
                StartEndFocushedApplication();
            }
        }

        public string AppInfo
        {
            get => _appName;
        }

        private string GetActiveWindowTitle()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return string.Empty;

            const int nChars = 256;
            IntPtr handle = GetForegroundWindow();
            StringBuilder buff = new StringBuilder(nChars);

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return string.Empty;
        }

        private void GetActiveProcess()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _appName = string.Empty;
                _appTitle = string.Empty;
                return;
            }

            try
            {
                string title = GetActiveWindowTitle();
                if (!string.IsNullOrEmpty(title))
                {
                    try
                    {
                        Process[] allProcess = Process.GetProcesses();
                        var foregroundWindows = allProcess.Where(p => p.MainWindowTitle == title);
                        if (foregroundWindows.Any())
                        {
                            var foregroundWindow = foregroundWindows.FirstOrDefault();
                            if (foregroundWindow != null)
                            {
                                _appTitle = title;
                                _appName = foregroundWindow.MainModule?.ModuleName ?? title;
                            }
                        }
                        else
                        {
                            _appTitle = title;
                            _appName = title;
                        }
                    }
                    catch (Exception ex)
                    {
                        _appTitle = title;
                        _appName = title;
                    }
                }
            }
            catch (Exception ex)
            {
                TempLog($"GetActiveProcess error: {ex.Message}");
            }
        }

        private void GetActiveWindowTitleV1()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _appName = null;
                _appTitle = null;
                return;
            }

            IntPtr foregroundWindowHandle = GetForegroundWindow();
            if (foregroundWindowHandle == IntPtr.Zero)
            {
                TempLog("No foreground window found.");
                return;
            }

            GetWindowThreadProcessId(foregroundWindowHandle, out uint processId);
            try
            {
                Process process = Process.GetProcessById((int)processId);
                if (process != null)
                {
                    _appTitle = string.IsNullOrEmpty(process.MainWindowTitle)
                        ? null
                        : process.MainWindowTitle;
                    _appName = string.IsNullOrEmpty(process.MainModule?.ModuleName)
                        ? null
                        : process.MainModule?.ModuleName;
                }
            }
            catch (Exception ex)
            {
                TempLog($"GetActiveWindowTitleV1 error: {ex.Message}");
            }
        }

        private void GetActiveProcessV1()
        {
            GetActiveWindowTitleV1();
        }

        private void StartEndFocushedApplication()
        {
            GetActiveProcessV1();
            if (!string.IsNullOrEmpty(_appName))
            {
                if (_justSentApplicationName == "")
                {
                    _justSentApplicationName = _appName;
                    ResetTrackerCount();
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                }
                else if (_justSentApplicationName != _appName)
                {
                    activeApplicationInfomationCollector.ApplicationFocusEnd(
                        _justSentApplicationName,
                        _appWiseTotalMouseClick,
                        _appWiseTotalKeysPressed,
                        _appWiseTotalMouseScrolls,
                        _appTotalIdletime
                    );
                    ResetTrackerCount();
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                    _justSentApplicationName = _appName;
                }
            }
        }

        private void ResetTrackerCount()
        {
            _lastInputTime = new DateTime();
            _lastInputIdletime = 0;
            _appTotalIdletime = 0;
            _appWiseTotalKeysPressed = 0;
            _appWiseTotalMouseClick = 0;
            _appWiseTotalMouseScrolls = 0;
        }

        private void CalculateIdleTimeAppWise()
        {
            // Placeholder for IdleTimeDetector
            var idleTime = GetIdleTimeInfo();
            if (idleTime.IdleTime.TotalSeconds > 0)
            {
                if (
                    idleTime.LastInputTime.ToString("ddMMyyyyHHmmss")
                    == _lastInputTime.ToString("ddMMyyyyHHmmss")
                )
                {
                    _appTotalIdletime = _appTotalIdletime - _lastInputIdletime;
                }
                _appTotalIdletime += idleTime.IdleTime.TotalMilliseconds;
                _lastInputIdletime = idleTime.IdleTime.TotalMilliseconds;
                _lastInputTime = idleTime.LastInputTime;
            }
        }

        // Placeholder for IdleTimeDetector
        private (TimeSpan IdleTime, DateTime LastInputTime) GetIdleTimeInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: Implement Windows idle detection (e.g., GetLastInputInfo)
                return (TimeSpan.Zero, DateTime.Now);
            }
            else
            {
                TempLog("Idle time detection not implemented for this platform.");
                return (TimeSpan.Zero, DateTime.Now);
            }
        }

        #region Save log into Temp
        private void TempLog(string message)
        {
            string tempPath = Path.GetTempPath();
            string path = Path.Combine(tempPath, $"trackerlogApp_{DateTime.Now:ddMMyyyy}.log");
            try
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine($"\n{DateTime.Now} Info: {message}");
                }
            }
            catch (Exception)
            {
                // Silent fail as in original
            }
        }

        private string ReplaceWithZWSP(string input)
        {
            string zwsp = "\u200B";
            return Regex.Replace(input, "\\?", zwsp);
        }
        #endregion
    }
}
