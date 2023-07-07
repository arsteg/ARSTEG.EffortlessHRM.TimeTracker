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
            Bitmap bitmap;
            string result = null;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder =
                 System.Drawing.Imaging.Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bitmap = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                var createdImage = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                var folderPath = @$"{Environment.CurrentDirectory}\{DateTime.Now.ToString("yyyy-MM-dd")}";
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }
                var fileName = $@"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
                result = @$"{folderPath}\{fileName}";
                bitmap.Save(result, jpgEncoder, myEncoderParameters);
            }
            catch (Exception)
            {
            }
            finally
            {
                DeleteObject(handle);
            }
            return result;
        }

        public static string CaptureMyScreen()
        {
            Bitmap bitmap;
            string result = null;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            // Determine the combined width and height of both monitors
            int width = 0;
            int height = 0;
            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                width += screen.Bounds.Width;
                height = Math.Max(height, screen.Bounds.Height);
            }

            bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                int x = 0;
                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    g.CopyFromScreen(screen.Bounds.Left, screen.Bounds.Top, x, 0, screen.Bounds.Size);
                    x += screen.Bounds.Width;
                }
            }

            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                var createdImage = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                var folderPath = Path.Combine(Environment.CurrentDirectory, DateTime.Now.ToString("yyyy-MM-dd"));
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var fileName = $@"{DateTime.UtcNow.ToString("HH-mm")}.jpg";
                result = Path.Combine(folderPath, fileName);
                bitmap.Save(result, jpgEncoder, myEncoderParameters);
            }
            catch (Exception)
            {
            }
            finally
            {
                DeleteObject(handle);
            }
            return result;
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
    }
}
