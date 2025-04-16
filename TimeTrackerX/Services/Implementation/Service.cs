using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Services.Implementation
{
    // Dummy implementation for IScreenshotService
    public class DummyScreenshotService : Interfaces.IScreenshotService
    {
        public byte[] CaptureScreen()
        {
            Console.WriteLine("DummyScreenshotService: Simulating screen capture.");
            // Return a dummy byte array (e.g., empty or small data)
            return new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Minimal JPEG header simulation
        }
    }

    // Dummy implementation for IKeyCounterService
    public class DummyKeyCounterService : Interfaces.IKeyCounterService
    {
        private int _keyCount = 0;

        public void StartMonitoring()
        {
            Console.WriteLine("DummyKeyCounterService: Started monitoring keystrokes.");
            // Simulate key presses by incrementing count
            _keyCount = 10; // Arbitrary value
        }

        public int GetKeyCount()
        {
            Console.WriteLine($"DummyKeyCounterService: Returning key count: {_keyCount}");
            return _keyCount;
        }
    }

    // Dummy implementation for IScrollCounterService
    public class DummyScrollCounterService : Interfaces.IScrollCounterService
    {
        private int _scrollCount = 0;

        public void StartMonitoring()
        {
            Console.WriteLine("DummyScrollCounterService: Started monitoring scrolls.");
            // Simulate scrolls
            _scrollCount = 5; // Arbitrary value
        }

        public int GetScrollCount()
        {
            Console.WriteLine($"DummyScrollCounterService: Returning scroll count: {_scrollCount}");
            return _scrollCount;
        }
    }

    // Dummy implementation for IClickCounterService
    public class DummyClickCounterService : Interfaces.IClickCounterService
    {
        private int _clickCount = 0;

        public void StartMonitoring()
        {
            Console.WriteLine("DummyClickCounterService: Started monitoring clicks.");
            // Simulate clicks
            _clickCount = 15; // Arbitrary value
        }

        public int GetClickCount()
        {
            Console.WriteLine($"DummyClickCounterService: Returning click count: {_clickCount}");
            return _clickCount;
        }
    }

    // Dummy implementation for IBrowserTrackerService
    public class DummyBrowserTrackerService : Interfaces.IBrowserTrackerService
    {
        public List<string> GetVisitedSites()
        {
            Console.WriteLine("DummyBrowserTrackerService: Retrieving visited sites.");
            // Return dummy URLs
            return new List<string>
            {
                "https://example.com",
                "https://dummy-site.org",
                "https://test.net"
            };
        }
    }

    // Dummy implementation for IMouseEventService
    public class DummyMouseEventService : Interfaces.IMouseEventService
    {
        public List<string> GetVisitedSites()
        {
            Console.WriteLine("DummyMouseEventService: Retrieving mouse event sites.");
            // Return dummy data (reusing "sites" as per interface)
            return new List<string> { "mouse://click-at-100,200", "mouse://move-to-300,400" };
        }
    }

    // Dummy implementation for IPowerModeService
    public class DummyPowerModeService : Interfaces.IPowerModeService
    {
        public List<string> GetVisitedSites()
        {
            Console.WriteLine("DummyPowerModeService: Retrieving power mode events.");
            // Return dummy data (reusing "sites" as per interface)
            return new List<string> { "power://battery-mode", "power://plugged-in" };
        }
    }

    // Dummy implementation for INotificationService
    public class DummyNotificationService : Interfaces.INotificationService
    {
        public List<string> GetVisitedSites()
        {
            Console.WriteLine("DummyNotificationService: Retrieving notification events.");
            // Return dummy data (reusing "sites" as per interface)
            return new List<string>
            {
                "notification://timer-ended",
                "notification://break-reminder"
            };
        }
    }
}
