﻿using System;
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

            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                var createdImage = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                var folderPath = Path.Combine(Path.Combine(Environment.CurrentDirectory,"Screenshots"), DateTime.Now.ToString("yyyy-MM-dd"));
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

        public static void ClearScreenshotsFolder()
        {
            var folderPath = Path.Combine(Environment.CurrentDirectory, "Screenshots");
            if (Directory.Exists(folderPath))
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(folderPath);
                directory.Delete(true);
            }
        }
    }
}
