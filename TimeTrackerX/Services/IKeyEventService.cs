using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Services
{
    public interface IKeyEventService
    {
        void StartMonitoring();
        void StopMonitoring();
        int GetKeyCount();
        event EventHandler<KeyEventArgs> OnKeyDown;
    }

    public class KeyEventArgs : EventArgs
    {
        public string Key { get; }

        public KeyEventArgs(string key)
        {
            Key = key;
        }
    }
}
