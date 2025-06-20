﻿using System;
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
        public async Task<string> CaptureScreenAsync(bool isBlur=false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await CaptureScreenWindowsAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await CaptureScreenMacOSAsync(isBlur);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await CaptureScreenLinuxAsync();
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
                var filePath = await ScreenshotUtility.CaptureMyScreen();

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on Windows.", ex);
            }
        }

        private async Task<string> CaptureScreenMacOSAsync(bool isBlur)
        {
            try
            {
                var fileName = $"{DateTime.UtcNow:HH-mm}.png";
                var tempFile = Path.Combine(Path.GetTempPath(), fileName);

                // Clean up only our previously generated screenshots
                var existingScreenshots = Directory.GetFiles(Path.GetTempPath(), "*.png");
                foreach (var file in existingScreenshots)
                {
                    try { File.Delete(file); } catch { }
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
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

                if (isBlur)
                {
                    using var inputStream = File.OpenRead(tempFile);
                    using var original = SKBitmap.Decode(inputStream);
                    using var surface = SKSurface.Create(new SKImageInfo(original.Width, original.Height));
                    var canvas = surface.Canvas;

                    var paint = new SKPaint
                    {
                        ImageFilter = SKImageFilter.CreateBlur(10, 10), // Adjust blur radius here
                        IsAntialias = true
                    };

                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(original, 0, 0, paint);

                    using var image = surface.Snapshot();
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    using var outputStream = File.OpenWrite(tempFile);
                    data.SaveTo(outputStream);
                }

                return tempFile;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on macOS.", ex);
            }
        }

        private async Task<string> CaptureScreenLinuxAsync()
        {
            try
            {
                // Generate unique temp file name with .png extension
                var tempFile = Path.Combine(Path.GetTempPath(), $"screenshot_{Guid.NewGuid()}.png");

                // Clean up only our previously generated screenshots
                var existingScreenshots = Directory.GetFiles(
                    Path.GetTempPath(),
                    "screenshot_*.png"
                );
                foreach (var file in existingScreenshots)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore errors deleting old files
                    }
                }

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "scrot",
                        Arguments = $"\"{tempFile}\"", // Save to temp file
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (!File.Exists(tempFile))
                {
                    throw new Exception("Failed to capture screenshot on Linux.");
                }

                return tempFile; // Return file path instead of deleting
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
