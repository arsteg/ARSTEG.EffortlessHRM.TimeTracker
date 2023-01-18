using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApplicationUsedLibrary
{
    public class ActiveApplicationPropertyThread
    {
        WinEventDelegate dele = null;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

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
            }
            else
            {
                activeApplicationInfomationCollector.ApplicationFocusEnd(_justSentApplicationName);
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
            Process[] AllProcess = Process.GetProcesses();
            string title = GetActiveWindowTitle();

            if(!string.IsNullOrEmpty(title))
            {
                var forgroundWindows = AllProcess.Where(p => p.MainWindowTitle == title).FirstOrDefault();
                if (forgroundWindows != null)
                {
                    _appTitle = title;
                    _appName = forgroundWindows.MainModule?.ModuleName;
                }
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
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                }
                if (_justSentApplicationName != _appName)
                {
                    activeApplicationInfomationCollector.ApplicationFocusEnd(_justSentApplicationName);
                    activeApplicationInfomationCollector.ApplicationFocusStart(_appName, _appTitle);
                    _justSentApplicationName = _appName;
                }
            }
        }
    }
}
