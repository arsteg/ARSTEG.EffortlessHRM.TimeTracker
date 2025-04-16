using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TimeTrackerX.Utilities
{
    public class EmailService
    {
        public async Task<SendGrid.Response> SendEmailAsyncToRecipientUsingSendGrid(
            string key,
            string fromEmail,
            string toEmail,
            string ccEmail,
            string bccEmail,
            string subject,
            string body
        )
        {
            try
            {
                var client = new SendGridClient(key);
                var message = new SendGridMessage();

                message.SetFrom(new EmailAddress(fromEmail, "Effortless HRM"));
                message.AddTo(new EmailAddress(toEmail));
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    message.AddCc(new EmailAddress(ccEmail));
                }
                if (!string.IsNullOrEmpty(bccEmail))
                {
                    message.AddBcc(new EmailAddress(bccEmail));
                }

                var folderPath =
                    @$"{Environment.CurrentDirectory}\{DateTime.Now.ToString("yyyy-MM-dd")}";
                var files = System.IO.Directory.GetFiles(folderPath);

                foreach (var file in files)
                {
                    var contentid = Path.GetFileName(file);
                    message.AddAttachment(
                        file,
                        Convert.ToBase64String(File.ReadAllBytes(file)),
                        "image/jpeg",
                        "inline",
                        contentid
                    );
                }
                message.SetSubject(subject);
                //message.AddContent(MimeType.Html, body);
                message.HtmlContent = body;

                return await client.SendEmailAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
