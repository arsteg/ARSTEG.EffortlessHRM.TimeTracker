using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using SkiaSharp;
using TimeTrackerX.Services.Interfaces;
using TimeTrackerX.Utilities;

namespace TimeTrackerX.Services.Implementation
{
    public class ScreenshotService : IScreenshotService
    {
        public async Task<string> CaptureScreenAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await CaptureScreenWindowsAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return (await CaptureScreenMacOSAsync());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Convert.ToBase64String(await CaptureScreenLinuxAsync());
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "Screenshot capture is not supported on this platform."
                );
            }
        }

        private async Task<string> CaptureScreenWindowsAsync()
        {
            try
            {
                var bytes = await ScreenshotUtility.CaptureMyScreen();

                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on Windows.", ex);
            }
        }

        private async Task<string> CaptureScreenMacOSAsync()
{
    try
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");

        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "screencapture",
                Arguments = $"-x \"{tempFile}\"", // -x for silent capture
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        if (!File.Exists(tempFile))
            throw new Exception($"Screenshot failed. File not found: {tempFile}");

        return tempFile;
    }
    catch (Exception ex)
    {
        throw new Exception("Failed to capture screenshot on macOS.", ex);
    }
}

        private async Task<byte[]> CaptureScreenLinuxAsync()
        {
            // Use `scrot` or `gnome-screenshot` for Linux
            try
            {
                var tempFile = Path.GetTempFileName() + ".png";
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "scrot",
                        Arguments = $"{tempFile}", // Save to temp file
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                if (!File.Exists(tempFile))
                {
                    throw new Exception("Failed to capture screenshot on Linux.");
                }

                var bytes = File.ReadAllBytes(tempFile);
                File.Delete(tempFile); // Clean up
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on Linux.", ex);
            }
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
