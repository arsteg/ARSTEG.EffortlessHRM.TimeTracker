using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Services.Interfaces
{
    public interface IScreenshotService
    {
        byte[] CaptureScreen();
    }

    public interface IKeyCounterService
    {
        void StartMonitoring();
        int GetKeyCount();
    }

    public interface IScrollCounterService
    {
        void StartMonitoring();
        int GetScrollCount();
    }

    public interface IClickCounterService
    {
        void StartMonitoring();
        int GetClickCount();
    }

    public interface IBrowserTrackerService
    {
        List<string> GetVisitedSites();
    }

    public interface IMouseEventService
    {
        List<string> GetVisitedSites();
    }

    public interface IPowerModeService
    {
        List<string> GetVisitedSites();
    }

    public interface INotificationService
    {
        List<string> GetVisitedSites();
    }
}
