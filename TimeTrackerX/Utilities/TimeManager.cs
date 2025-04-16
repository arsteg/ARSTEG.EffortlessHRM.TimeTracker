using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using SkiaSharp;

namespace TimeTrackerX.Utilities
{
    public partial class TimeManager
    {
        // Windows-specific imports for screen capture
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateDC(
            string lpszDriver,
            string lpszDevice,
            string lpszOutput,
            IntPtr lpInitData
        );

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

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

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static string CaptureMyScreenOld()
        {
            string result = null;
            //try
            //{
            //    // Get primary screen bounds (first screen in Avalonia)
            //    var screen = AvaloniaLocator
            //        .Current.GetService<IScreenImpl>()
            //        .AllScreens.FirstOrDefault();
            //    if (screen == null)
            //        return null;

            //    var bounds = screen.Bounds;
            //    int width = bounds.Width;
            //    int height = bounds.Height;

            //    // Capture screen
            //    using var bitmap = CaptureScreen(0, 0, width, height);
            //    if (bitmap == null)
            //        return null;

            //    // Save to JPEG
            //    var folderPath = Path.Combine(
            //        Environment.CurrentDirectory,
            //        DateTime.Now.ToString("yyyy-MM-dd")
            //    );
            //    if (!Directory.Exists(folderPath))
            //    {
            //        Directory.CreateDirectory(folderPath);
            //    }
            //    var fileName = $"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
            //    result = Path.Combine(folderPath, fileName);

            //    SaveJpeg(bitmap, result, 75);
            //}
            //catch (Exception ex)
            //{
            //    // Log error (use TimeTrackerViewModel.TempLog if available)
            //    Console.WriteLine($"CaptureMyScreenOld error: {ex.Message}");
            //}
            return result;
        }

        public static string CaptureMyScreen()
        {
            string result = null;
            try
            {
                // Get virtual screen bounds (all screens combined)
                //var screens = AvaloniaLocator.Current.GetService<IScreenImpl>().AllScreens;
                //if (screens == null || !screens.Any())
                //    return null;

                //// Calculate total bounds
                //int left = screens.Min(s => s.Bounds.X);
                //int top = screens.Min(s => s.Bounds.Y);
                //int right = screens.Max(s => s.Bounds.X + s.Bounds.Width);
                //int bottom = screens.Max(s => s.Bounds.Y + s.Bounds.Height);
                //int width = right - left;
                //int height = bottom - top;

                //// Capture screen
                //using var bitmap = CaptureScreen(left, top, width, height);
                //if (bitmap == null)
                //    return null;

                //// Save to JPEG
                //var folderPath = Path.Combine(Environment.CurrentDirectory, "Screenshots", DateTime.Now.ToString("yyyy-MM-dd"));
                //if (!Directory.Exists(folderPath))
                //{
                //    Directory.CreateDirectory(folderPath);
                //}
                //var fileName = $"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
                //result = Path.Combine(folderPath, fileName);

                //SaveJpeg(bitmap, result, 75);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"CaptureMyScreen error: {ex.Message}");
            }
            return result;
        }

        private static SKBitmap CaptureScreen(int x, int y, int width, int height)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: Use GDI+ BitBlt (similar to original)
                IntPtr hdcScreen = GetDC(IntPtr.Zero);
                IntPtr hdc = CreateCompatibleDC(hdcScreen);
                IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, width, height);
                IntPtr hOld = SelectObject(hdc, hBitmap);

                try
                {
                    //BitBlt(hdc, 0, 0, width, height, hdcScreen, x, y, 0x00CC0020 /* SRCCOPY */);
                    //using var bitmap = new System.Drawing.Bitmap(width, height);
                    //using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    //{
                    //    IntPtr hdcBitmap = g.GetHdc();
                    //    BitBlt(hdcBitmap, 0, 0, width, height, hdc, 0, 0, 0x00CC0020);
                    //    g.ReleaseHdc(hdcBitmap);
                    //}
                    //// Convert System.Drawing.Bitmap to SKBitmap
                    //using var ms = new MemoryStream();
                    ////bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    //ms.Position = 0;
                    //return SKBitmap.Decode(ms);
                }
                finally
                {
                    SelectObject(hdc, hOld);
                    DeleteObject(hBitmap);
                    DeleteDC(hdc);
                    ReleaseDC(IntPtr.Zero, hdcScreen);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS: Use CGWindow (simplified, requires native code or library)
                // For simplicity, return null (implement native capture if needed)
                return null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux: Use X11/Gdk (requires additional dependencies)
                // For simplicity, return null (implement native capture if needed)
                return null;
            }
            return null;
        }

        private static void SaveJpeg(SKBitmap bitmap, string path, int quality)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            using var stream = File.OpenWrite(path);
            data.SaveTo(stream);
        }

        public static void ClearScreenshotsFolder()
        {
            var folderPath = Path.Combine(Environment.CurrentDirectory, "Screenshots");
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        // Windows-specific helper
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
    }
}
