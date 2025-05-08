using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SkiaSharp;

namespace TimeTrackerX.Utilities
{
    public static class ScreenshotUtility
    {
        // Windows API imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            uint dwRop
        );

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern uint GetDIBits(
            IntPtr hdc,
            IntPtr hBitmap,
            uint startScan,
            uint scanLines,
            IntPtr bits,
            ref BITMAPINFO bmi,
            uint usage
        );

        private const int SM_XVIRTUALSCREEN = 76;
        private const int SM_YVIRTUALSCREEN = 77;
        private const int SM_CXVIRTUALSCREEN = 78;
        private const int SM_CYVIRTUALSCREEN = 79;
        private const int SRCCOPY = 0xCC0020;
        private const uint BI_RGB = 0;
        private const uint DIB_RGB_COLORS = 0;

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public uint[] bmiColors;
        }

        public static async Task<string> CaptureMyScreen()
        {
            string result = null;

            try
            {
                // Capture the screenshot and get the raw bytes
                byte[] screenshotBytes = await CaptureScreenAsync();

                // Load the screenshot into SkiaSharp for processing
                using var bitmap = SKBitmap.Decode(screenshotBytes);
                if (bitmap == null)
                {
                    throw new Exception("Failed to decode screenshot.");
                }

                // Set JPEG encoding parameters (75% quality)
                var jpgQuality = 75;
                using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, jpgQuality);

                // Create the output directory
                var folderPath = Path.Combine(
                    Environment.CurrentDirectory,
                    "Screenshots",
                    DateTime.Now.ToString("yyyy-MM-dd")
                );
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Clean up only our previously generated screenshots (*.jpg) in the folder
                var existingScreenshots = Directory.GetFiles(folderPath, "*.jpg");
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

                // Save the screenshot to the final destination
                var fileName = $"{DateTime.UtcNow:HH-mm}.jpg";
                result = Path.Combine(folderPath, fileName);
                await File.WriteAllBytesAsync(result, data.ToArray());
            }
            catch (Exception ex)
            {
                TempLog($"CaptureMyScreen error: {ex.Message}");
            }

            return result;
        }

        private static async Task<byte[]> CaptureScreenAsync()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await CaptureScreenWindowsAsync();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await CaptureScreenMacOSAsync();
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

        private static async Task<byte[]> CaptureScreenWindowsAsync()
        {
            try
            {
                // Get virtual screen bounds (all monitors)
                int left = GetSystemMetrics(SM_XVIRTUALSCREEN);
                int top = GetSystemMetrics(SM_YVIRTUALSCREEN);
                int width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
                int height = GetSystemMetrics(SM_CYVIRTUALSCREEN);

                // Capture screen using BitBlt
                IntPtr hdcSrc = GetDC(IntPtr.Zero);
                IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
                IntPtr hOld = SelectObject(hdcDest, hBitmap);
                bool success = BitBlt(hdcDest, 0, 0, width, height, hdcSrc, left, top, SRCCOPY);
                SelectObject(hdcDest, hOld);
                DeleteDC(hdcDest);
                ReleaseDC(IntPtr.Zero, hdcSrc);

                if (!success)
                {
                    DeleteObject(hBitmap);
                    throw new Exception("BitBlt failed to capture screenshot.");
                }

                // Get bitmap info and pixel data
                BITMAPINFO bmi = new BITMAPINFO
                {
                    bmiHeader = new BITMAPINFOHEADER
                    {
                        biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                        biWidth = width,
                        biHeight = -height, // Negative for top-down bitmap
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = BI_RGB,
                        biSizeImage = (uint)(width * height * 4)
                    },
                    bmiColors = new uint[1]
                };

                IntPtr bitsPtr;
                IntPtr hdcScreen = GetDC(IntPtr.Zero);
                uint result = GetDIBits(
                    hdcScreen,
                    hBitmap,
                    0,
                    (uint)height,
                    IntPtr.Zero,
                    ref bmi,
                    DIB_RGB_COLORS
                );
                if (result == 0)
                {
                    DeleteObject(hBitmap);
                    ReleaseDC(IntPtr.Zero, hdcScreen);
                    throw new Exception("GetDIBits failed to retrieve bitmap info.");
                }

                byte[] pixelData = new byte[bmi.bmiHeader.biSizeImage];
                bitsPtr = Marshal.AllocHGlobal(pixelData.Length);
                result = GetDIBits(
                    hdcScreen,
                    hBitmap,
                    0,
                    (uint)height,
                    bitsPtr,
                    ref bmi,
                    DIB_RGB_COLORS
                );
                if (result == 0)
                {
                    Marshal.FreeHGlobal(bitsPtr);
                    DeleteObject(hBitmap);
                    ReleaseDC(IntPtr.Zero, hdcScreen);
                    throw new Exception("GetDIBits failed to retrieve pixel data.");
                }

                Marshal.Copy(bitsPtr, pixelData, 0, pixelData.Length);
                Marshal.FreeHGlobal(bitsPtr);
                DeleteObject(hBitmap);
                ReleaseDC(IntPtr.Zero, hdcScreen);

                // Create SKBitmap from pixel data
                using var bitmap = new SKBitmap();
                var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                IntPtr pixelPtr = Marshal.AllocHGlobal(pixelData.Length);
                Marshal.Copy(pixelData, 0, pixelPtr, pixelData.Length);
                bitmap.InstallPixels(
                    info,
                    pixelPtr,
                    info.RowBytes,
                    (addr, ctx) => Marshal.FreeHGlobal(addr),
                    null
                );

                // Save to temporary file
                var tempFile = Path.GetTempFileName() + ".png";
                using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                {
                    await File.WriteAllBytesAsync(tempFile, data.ToArray());
                }

                // Read bytes and clean up
                var bytes = await File.ReadAllBytesAsync(tempFile);
                File.Delete(tempFile);
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on Windows.", ex);
            }
        }

        private static async Task<byte[]> CaptureScreenMacOSAsync()
        {
            try
            {
                var tempFile = Path.GetTempFileName() + ".png";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/sbin/screencapture",
                        Arguments = $"-x -D all {tempFile}", // -x for silent, -D all for all displays
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (!File.Exists(tempFile))
                {
                    throw new Exception("Failed to capture screenshot on macOS.");
                }

                var bytes = await File.ReadAllBytesAsync(tempFile);
                File.Delete(tempFile);
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on macOS.", ex);
            }
        }

        private static async Task<byte[]> CaptureScreenLinuxAsync()
        {
            try
            {
                var tempFile = Path.GetTempFileName() + ".png";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "scrot",
                        Arguments = $"--multidisp {tempFile}", // --multidisp for all monitors
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                if (!File.Exists(tempFile))
                {
                    // Try grim for Wayland
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "grim",
                            Arguments = $"{tempFile}",
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
                }

                var bytes = await File.ReadAllBytesAsync(tempFile);
                File.Delete(tempFile);
                return bytes;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to capture screenshot on Linux.", ex);
            }
        }

        public static void ClearScreenshotsFolder()
        {
            try
            {
                var folderPath = Path.Combine(Environment.CurrentDirectory, "Screenshots");
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }
            }
            catch (Exception ex)
            {
                TempLog($"ClearScreenshotsFolder error: {ex.Message}");
            }
        }

        // Placeholder for TimeTrackerViewModel's TempLog
        private static void TempLog(string message)
        {
            Console.WriteLine(message); // Replace with actual TempLog
        }
    }
}
