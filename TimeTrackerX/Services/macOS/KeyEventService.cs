#if MACOS
using AppKit;
using System;

namespace TimeTrackerX.Platforms.MacOS
{
    public class KeyEventService : Services.IKeyEventService
    {
        private int _keyCount;
        private IntPtr _eventHandler;

        public event EventHandler<Services.KeyEventArgs> OnKeyDown;

        public void StartMonitoring()
        {
            if (_eventHandler == IntPtr.Zero)
            {
                _eventHandler = NSEvent.AddGlobalMonitorForEventsMatchingMask(
                    NSEventMask.KeyDown,
                    (evt) =>
                    {
                        _keyCount++;
                        OnKeyDown?.Invoke(
                            this,
                            new Services.KeyEventArgs(
                                evt.CharactersIgnoringModifiers ?? evt.KeyCode.ToString()
                            )
                        );
                    }
                );
            }
        }

        public int GetKeyCount() => _keyCount;

        public void StopMonitoring()
        {
            if (_eventHandler != IntPtr.Zero)
            {
                NSEvent.RemoveMonitor(_eventHandler);
                _eventHandler = IntPtr.Zero;
            }
        }
    }
}
#endif
