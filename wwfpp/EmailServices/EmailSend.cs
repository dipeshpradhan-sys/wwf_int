using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace wwfpp.EmailServices
{
    //Interface
    public interface ISendEmails
    {
        Task<string> SendEmailAsync(string to, string subject, string body,
                            string attachmentPath = null,
                            string cc = null, string bcc = null,
                            string fromName = null, string fromEmail = null);
    }
    public class EmailSend : ISendEmails
    {
        private readonly SmtpClient _smtpClient; //making object of smtpclient
        private readonly string sFromName = "";  //string for fromName
        private readonly string sFromEmail = "";  //string for fromEmail
        public EmailSend(IOptions<SmtpSettings> options)
        {
            var settings = options.Value;
            sFromName = settings.FromName;
            sFromEmail = settings.FromEmail;

            _smtpClient = new SmtpClient(settings.SmtpServer, settings.SmtpPort)
            {
                EnableSsl = settings.UseSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = settings.DefaultCredentials,
                Credentials = new NetworkCredential(settings.FromEmail, settings.PassPhrase)
            };
        }

        public async Task<string> SendEmailAsync(string to,string subject,string body,string attachmentPath = null,string cc = null,string bcc = null,string fromName = null,string fromEmail = null)
        {
            var _fromName = string.IsNullOrWhiteSpace(fromName) ? sFromName : fromName;
            var _fromEmail = string.IsNullOrWhiteSpace(fromEmail) ? sFromEmail : fromEmail;
            var _subject = string.IsNullOrWhiteSpace(subject) ? Lang.lbl_empty_subject : subject;

            string result;

            using (var mail = new MailMessage())
            {
                mail.IsBodyHtml = true;
                mail.From = new MailAddress(_fromEmail, _fromName);
                mail.To.Add(to.Trim());

                if (!string.IsNullOrWhiteSpace(cc))
                {
                    foreach (var ccAddr in cc.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                        mail.CC.Add(ccAddr.Trim());
                }

                if (!string.IsNullOrWhiteSpace(bcc))
                {
                    foreach (var bccAddr in bcc.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                        mail.Bcc.Add(bccAddr.Trim());
                }

                if (!string.IsNullOrWhiteSpace(attachmentPath))
                {
                    foreach (var atth in attachmentPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // Attach file – MailMessage will dispose attachments when disposed
                        mail.Attachments.Add(new Attachment(atth));
                    }
                }

                mail.Subject = _subject.Trim();
                mail.Body = body.Trim() + Lang.EMAIL_ATTN_DO_NO_REPLY;

                try
                {
                    await _smtpClient.SendMailAsync(mail).ConfigureAwait(false);
                    result = "true";
                }
                catch (Exception ex)
                {
                    result = $"Failed to send email: {ex.Message}";
                }
            } // MailMessage disposed here, attachments released

            // ✅ Delete attachments AFTER disposal
            if (!string.IsNullOrWhiteSpace(attachmentPath))
            {
                foreach (var atth in attachmentPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (File.Exists(atth))
                    {
                        try
                        {
                            File.Delete(atth);
                        }
                        catch (IOException ioEx)
                        {
                            result = $"Email sent but failed to delete file: {ioEx.Message}";
                        }
                    }
                }
            }

            return result;
        }


    }
}
