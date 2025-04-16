#if LINUX
namespace TimeTrackerX.Platforms.Linux
{
    public class KeyEventService : Services.IKeyEventService
    {
        public event EventHandler<Services.KeyEventArgs> OnKeyDown;

        public void StartMonitoring()
        {
            // TODO: Implement Linux keyboard capture (X11 or libinput)
            System.Diagnostics.Debug.WriteLine("Linux keyboard monitoring not implemented.");
        }

        public void StopMonitoring()
        {
            // TODO: Cleanup Linux keyboard capture
        }

        public int GetKeyCount() => 0;
    }
}
#endif
