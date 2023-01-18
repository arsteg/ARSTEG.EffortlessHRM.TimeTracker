using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationUsedLibrary
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
                    Duration = 0
                });
            }
        }
        public void ApplicationFocusEnd(string appName)
        {
            if (_focusedApplication.ContainsKey(appName))
            {
                //end the timer and update the seconds
                TimeSpan elapsed = DateTime.Now - _startTime;
                _focusedApplication[appName].Duration = Convert.ToDouble(_focusedApplication[appName].Duration + elapsed.TotalMilliseconds);
            }
        }
    }

    public class FocushedApplicationDetails
    {
        public string? AppTitle { get; set; }
        public double Duration { get; set; }
    }
}
