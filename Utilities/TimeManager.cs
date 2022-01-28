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
        public static string CaptureMyScreen()
        {
            Bitmap bitmap;
            string result = null;

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
                var fileName = $@"{DateTime.Now.ToString("HH-mm")}.jpg";
                result = @$"{folderPath}\{fileName}";
                bitmap.Save(result);
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
    }
}
