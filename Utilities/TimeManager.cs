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



        
        //        private double DurationMinutes(System.TimeSpan duration)
        //        {
        //            double rc = 0;
        //            try
        //            {

        //                int hours, minutes;
        //                double seconds;

        //                hours = Int32.Parse(duration.Hours.ToString());
        //                minutes = Int32.Parse(duration.Minutes.ToString());
        //                seconds = Int32.Parse(duration.Seconds.ToString());
        //                rc = hours * 60 + minutes + seconds / 60.00;
        //                return rc;
        //            }
        //            catch (Exception ex)
        //            {
        //                return rc;
        //            }
        //        }
        


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
                if(!System.IO.Directory.Exists(folderPath))
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

        public static void  SendEMail() 
        {
            try
            {
                var fromAddress = new MailAddress("apptesting157@gmail.com", "webdir123R");
                var toAddress = new MailAddress("info@arsteg.com");
                const string fromPassword = "Notification@2019";


                MailMessage message = new MailMessage();
                message.To.Add("info@arsteg.com");
                message.Subject = "Daily update from XXX";
                message.From = new System.Net.Mail.MailAddress("no-reply@example.com");
                message.IsBodyHtml = true;
                message.AlternateViews.Add(Mail_Body());
                var SmtpMail = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };                
                SmtpMail.DeliveryMethod = SmtpDeliveryMethod.Network;
                SmtpMail.EnableSsl = true;                
                SmtpMail.Send(message); //Smtpclient to send the mail message                  
            }
            catch (Exception ex)
            {
                
            }
        }
        private static AlternateView Mail_Body()
        {
            string path = @"C:\Work\EffortlessHR\TimeTracker\bin\Debug\netcoreapp3.1\2021-12\05-28-11.jpg";
            LinkedResource Img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
            Img.ContentId = "MyImage";
            string str = @"  
            <table>  
                <tr>  
                    <td> '" + "Test" + @"'  
                    </td>  
                </tr>  
                <tr>  
                    <td>  
                      <img src=cid:MyImage  id='img' alt='' width='100px' height='100px'/>   
                    </td>  
                </tr></table>  
            ";
            AlternateView AV =
            AlternateView.CreateAlternateViewFromString(str, null, MediaTypeNames.Text.Html);
            AV.LinkedResources.Add(Img);
            return AV;
        }
    }
    }
