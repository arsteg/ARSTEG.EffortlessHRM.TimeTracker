using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.AppUsedTracker
{
    public class ActiveApplicationInfomationCollector
    {
        public DateTime _startTime;
        public Dictionary<string, FocushedApplicationDetails> _focusedApplication;
        public ActiveApplicationInfomationCollector()
        {
            _focusedApplication = new Dictionary<string, FocushedApplicationDetails>();
        }
        public void ApplicationFocusStart(string appName, string appTitle)
        {
            _startTime = DateTime.Now;
            if (!_focusedApplication.ContainsKey(appName))
            {
                _focusedApplication.Add(appName, new FocushedApplicationDetails
                {
                    AppTitle = appTitle,
                    Duration = 0,
                    TotalKeysPressed = 0,
                    TotalMouseClick = 0,
                    TotalMouseScrolls = 0,
                    TotalIdletime = 0,
                });
            }
        }
        public void ApplicationFocusEnd(string appName, int TotalMouseClick, int TotalKeysPressed, int TotalMouseScrolls, double TotalIdletime)
        {
            if (_focusedApplication.ContainsKey(appName))
            {
                //end the timer and update the seconds
                TimeSpan elapsed = DateTime.Now - _startTime;
                _focusedApplication[appName].Duration = Convert.ToDouble(_focusedApplication[appName].Duration + elapsed.TotalMilliseconds);
                _focusedApplication[appName].TotalMouseClick += TotalMouseClick;
                _focusedApplication[appName].TotalKeysPressed += TotalKeysPressed;
                _focusedApplication[appName].TotalMouseScrolls += TotalMouseScrolls;
                _focusedApplication[appName].TotalIdletime += TotalIdletime;
            }
        }
    }

    public class FocushedApplicationDetails
    {
        public string? AppTitle { get; set; }
        public double Duration { get; set; }
        public int TotalMouseClick { get; set; }
        public int TotalKeysPressed { get; set; }
        public int TotalMouseScrolls { get; set; }
        public double TotalIdletime { get; set; }
    }
}
