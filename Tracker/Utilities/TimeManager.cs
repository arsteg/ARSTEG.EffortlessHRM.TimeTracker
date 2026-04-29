using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TimeTracker.Utilities
{
    public partial class TimeManager
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr onj);
        public static string CaptureMyScreenOld()
        {
            Bitmap bitmap = null;
            string result = null;
            IntPtr handle = IntPtr.Zero;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            using (var myEncoderParameters = new EncoderParameters(1))
            {
                myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, 75L);

                bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }

                try
                {
                    handle = bitmap.GetHbitmap();
                    var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var fileName = $@"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
                    result = Path.Combine(folderPath, fileName);
                    bitmap.Save(result, jpgEncoder, myEncoderParameters);
                }
                catch (Exception ex)
                {
                    TimeTracker.Trace.LogManager.Logger.Error($"Error capturing screen (old method): {ex.Message}");
                }
                finally
                {
                    DeleteObject(handle);
                    bitmap?.Dispose();
                }
            }
            return result;
        }

        public static string CaptureMyScreen(bool isBlured = false)
        {
            Bitmap bitmap = null;
            Bitmap blurredBitmap = null;
            string result = null;
            IntPtr handle = IntPtr.Zero;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            using (var myEncoderParameters = new EncoderParameters(1))
            {
                myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, 75L);

                // Get the total dimensions of all screens combined
                int screenLeft = SystemInformation.VirtualScreen.Left;
                int screenTop = SystemInformation.VirtualScreen.Top;
                int screenWidth = SystemInformation.VirtualScreen.Width;
                int screenHeight = SystemInformation.VirtualScreen.Height;

                // Create a bitmap of the appropriate size to receive the full-screen screenshot.
                bitmap = new Bitmap(screenWidth, screenHeight);

                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                }

                Bitmap bitmapToSave = bitmap;
                if (isBlured)
                {
                    blurredBitmap = ApplyBlur(bitmap);
                    bitmapToSave = blurredBitmap;
                }

                try
                {
                    handle = bitmapToSave.GetHbitmap();
                    var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots", DateTime.Now.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var fileName = $@"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
                    result = Path.Combine(folderPath, fileName);
                    bitmapToSave.Save(result, jpgEncoder, myEncoderParameters);
                }
                catch (Exception ex)
                {
                    TimeTracker.Trace.LogManager.Logger.Error($"Error capturing screen: {ex.Message}");
                }
                finally
                {
                    DeleteObject(handle);
                    // Dispose bitmaps properly
                    bitmap?.Dispose();
                    blurredBitmap?.Dispose();
                }
            }
            return result;
        }

        public static Bitmap ApplyBlur(Bitmap image, int blurRadius = 5)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);
            int bytesPerPixel = 3;

            BitmapData srcData = null;
            BitmapData dstData = null;
            try
            {
                srcData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                dstData = blurred.LockBits(new Rectangle(0, 0, blurred.Width, blurred.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int stride = srcData.Stride;
                byte[] srcPixels = new byte[srcData.Stride * srcData.Height];
                byte[] tempPixels = new byte[srcData.Stride * srcData.Height];
                byte[] dstPixels = new byte[dstData.Stride * dstData.Height];

                System.Runtime.InteropServices.Marshal.Copy(srcData.Scan0, srcPixels, 0, srcPixels.Length);

                // Horizontal pass
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        int red = 0, green = 0, blue = 0, count = 0;
                        for (int kx = -blurRadius; kx <= blurRadius; kx++)
                        {
                            int nx = x + kx;
                            if (nx >= 0 && nx < image.Width)
                            {
                                int pixelOffset = y * stride + nx * bytesPerPixel;
                                blue += srcPixels[pixelOffset];
                                green += srcPixels[pixelOffset + 1];
                                red += srcPixels[pixelOffset + 2];
                                count++;
                            }
                        }
                        int dstOffset = y * stride + x * bytesPerPixel;
                        tempPixels[dstOffset] = (byte)(blue / count);
                        tempPixels[dstOffset + 1] = (byte)(green / count);
                        tempPixels[dstOffset + 2] = (byte)(red / count);
                    }
                }

                // Vertical pass
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        int red = 0, green = 0, blue = 0, count = 0;
                        for (int ky = -blurRadius; ky <= blurRadius; ky++)
                        {
                            int ny = y + ky;
                            if (ny >= 0 && ny < image.Height)
                            {
                                int pixelOffset = ny * stride + x * bytesPerPixel;
                                blue += tempPixels[pixelOffset];
                                green += tempPixels[pixelOffset + 1];
                                red += tempPixels[pixelOffset + 2];
                                count++;
                            }
                        }
                        int dstOffset = y * stride + x * bytesPerPixel;
                        dstPixels[dstOffset] = (byte)(blue / count);
                        dstPixels[dstOffset + 1] = (byte)(green / count);
                        dstPixels[dstOffset + 2] = (byte)(red / count);
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(dstPixels, 0, dstData.Scan0, dstPixels.Length);
            }
            finally
            {
                if (srcData != null) image.UnlockBits(srcData);
                if (dstData != null) blurred.UnlockBits(dstData);
            }
            return blurred;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void ClearScreenshotsFolder()
        {
            var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
            if (Directory.Exists(folderPath))
            {
                var directory = new DirectoryInfo(folderPath);
                directory.Delete(true);
            }
        }
    }
}
