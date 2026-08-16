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
        public async Task<string> SendEmailAsync(string to, string subject, string body, string attachmentPath = null,
        string cc = null, string bcc = null, string fromName = null, string fromEmail = null)
        {
            var _fromName = string.IsNullOrWhiteSpace(fromName) ? sFromName : fromName;
            var _fromEmail = string.IsNullOrWhiteSpace(fromEmail) ? sFromEmail : fromEmail;
            var _subject = string.IsNullOrWhiteSpace(subject) ? Lang.lbl_empty_subject : subject;

            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress(_fromEmail, _fromName);

            mail.To.Add(to.Trim());

            // Multiple CCs supported with ; or ,
            if (!string.IsNullOrWhiteSpace(cc))
            {
                foreach (var ccAddr in cc.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.CC.Add(ccAddr.Trim());
                }
            }

            // Multiple BCCs supported with ; or ,
            if (!string.IsNullOrWhiteSpace(bcc))
            {
                foreach (var bccAddr in bcc.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.Bcc.Add(bccAddr.Trim());
                }
            }

            // Multiple Attachements supported with ;
            if (!string.IsNullOrWhiteSpace(attachmentPath))
            {
                foreach (var atth in attachmentPath.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.Attachments.Add(new Attachment(atth));
                }
            }
            mail.Subject = _subject.Trim();
            mail.Body = body.Trim() + Lang.EMAIL_ATTN_DO_NO_REPLY;
            try
            {
                await _smtpClient.SendMailAsync(mail).ConfigureAwait(false);
                return "true"; // success
            }
            catch (Exception ex)
            {
                // log ex.Message if needed
                return $"Failed to send email: {ex.Message}"; // failure
            }
        }

    }
}
