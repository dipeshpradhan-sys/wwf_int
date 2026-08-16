
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Models.Account;
using wwfpp.Services;
namespace wwfpp.EmailServices
{
    public class EmailService
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly ISendEmails _sendEmails;
        //private readonly SmtpClient _smtpClient; //making object of smtpclient
        private readonly string sFromName = "";  //string for fromName
        private readonly string sFromEmail = "";  //string for fromEmail
        public EmailService(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            GlobalOptionServices globalOptionServices,
            IOptions<SmtpSettings> options,
            ISendEmails sendEmails
            )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _globalOptionServices = globalOptionServices;
            _sendEmails = sendEmails;

            var settings = options.Value;
            sFromName = settings.FromName;
            sFromEmail = settings.FromEmail;
        }
        public string SendEmail(string category, string to, string subject, string body, string attachmentPath = null,
        string cc = null, string bcc = null, string fromName = null, string fromEmail = null)
        {
            //insert to database 
            string Id = GblUtilities.UniqueID();
            string To = string.IsNullOrWhiteSpace(to) ? "" : to;
            string Subject = string.IsNullOrWhiteSpace(subject) ? "" : subject;
            string Body = string.IsNullOrWhiteSpace(body) ? "" : body;
            string AttachmentPath = string.IsNullOrWhiteSpace(attachmentPath) ? "" : attachmentPath;
            string Cc = string.IsNullOrWhiteSpace(cc) ? "" : cc;
            string Bcc = string.IsNullOrWhiteSpace(bcc) ? "" : bcc;
            string FromName = string.IsNullOrWhiteSpace(fromName) ? sFromName : fromName;
            string FromEmail = string.IsNullOrWhiteSpace(fromEmail) ? sFromEmail : fromEmail;

            Subject = Subject
                .Replace("<[SITE-TITLE]>", _appSettings.SITE_TITLE, StringComparison.OrdinalIgnoreCase)
                .Replace("<[ORG-NAME]>", _globalOptionServices.OptionServices["op_org_name"], StringComparison.OrdinalIgnoreCase);

            Body = Body
                .Replace("<[SITE-TITLE]>", _appSettings.SITE_TITLE, StringComparison.OrdinalIgnoreCase)
                .Replace("<[ORG-NAME]>", _globalOptionServices.OptionServices["op_org_name"], StringComparison.OrdinalIgnoreCase)
                .Replace("<[SITE-ADMIN-NAME]>", Lang.SITE_ADMIN_NAME, StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(To) && string.IsNullOrWhiteSpace(Cc) && string.IsNullOrWhiteSpace(Bcc))
            {
                return Lang.msg_insufficient_info;
            }
            // Save in DB 
            _ = _context.tbl_email_list.Add(new tbl_email_list
            {
                id = Id,
                from_add = $"{FromName} <{FromEmail}>",
                to_add = To,
                subject = Subject,
                e_message =Body ,
                submit_date = DateTime.Now,
                status ="N",
                category = category,
                cc_add =Cc,
                bcc_add = Bcc
            });
            _ = _context.SaveChanges();
            _context.ChangeTracker.Clear();

            //AttachmentPath List save
            if (!string.IsNullOrWhiteSpace(AttachmentPath))
            {
                _ = _context.tbl_email_list_attachment.Add(new tbl_email_list_attachment
                {
                    id = GblUtilities.UniqueID(),
                    attachment = AttachmentPath,
                    eid = Id
                });
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();
            }
            //email
            try
            {
                _ = _sendEmails.SendEmailAsync(To, Subject, Body, AttachmentPath, Cc, Bcc, FromName, FromEmail);
                SendEmailUpate(Id);
                return "true";
            }
            catch (Exception ex)
            {
                SendEmailFailLog(Id, ex.Message);
                return ex.Message;
            }
        }
        public void SendEmailDb()
        {
            //top 1 from database 
            var emails = _context.tbl_email_list
                    .Where(p => p.status == "N")
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault(); 
            if (emails != null)
            {
                string Id = emails.id;
                string To = string.IsNullOrWhiteSpace(emails.to_add) ? "" : emails.to_add;
                string Subject = string.IsNullOrWhiteSpace(emails.subject) ? "" : emails.subject;
                string Body = string.IsNullOrWhiteSpace(emails.e_message) ? "" : emails.e_message;
                string Cc = string.IsNullOrWhiteSpace(emails.cc_add) ? "" : emails.cc_add;
                string Bcc = string.IsNullOrWhiteSpace(emails.bcc_add) ? "" : emails.bcc_add;
                string FromAdd = string.IsNullOrWhiteSpace(emails.from_add) ? "" : emails.from_add;
                string FromName = "";
                string FromEmail = "";
                if (!string.IsNullOrWhiteSpace(FromAdd))
                {
                    int start = FromAdd.IndexOf('<');
                    int end = FromAdd.IndexOf('>');

                    FromName = FromAdd.Substring(0, start).Trim();
                    FromEmail = FromAdd.Substring(start + 1, end - start - 1).Trim();
                }
                //attachments
                string AttachmentPath = "";
                var atth = _context.tbl_email_list_attachment
                    .Where(p => p.eid == Id)
                    .OrderByDescending(p => p.id)
                    .FirstOrDefault(); 
                if (atth != null)
                {
                    AttachmentPath = string.IsNullOrWhiteSpace(atth.attachment) ? "" : atth.attachment;
                }
                //email
                try
                {
                    _ = _sendEmails.SendEmailAsync(To, Subject, Body, AttachmentPath, Cc, Bcc, FromName, FromEmail);
                    SendEmailUpate(Id);
                }
                catch (Exception ex)
                {
                    SendEmailFailLog(Id, ex.Message);
                }
            }
        }
        public void SendEmailUpate(string Id)
        {
            _ = _context.tbl_email_list
            .Where(x => x.id == Id)
            .ExecuteUpdate(s => s
                .SetProperty(x => x.status, "Y")
                .SetProperty(x => x.sent_date, DateTime.Now)
            );
            _ = _context.SaveChanges();
            //CLOSE THE EXISTING INSTANCE
            _context.ChangeTracker.Clear();
            //END CLOSE THE EXISTING INSTANCE
        }
        public void SendEmailFailLog(string Id, string ErrorMsg)
        {
            _ = _context.tbl_email_list_sub.Add(new tbl_email_list_sub
            {
                id = GblUtilities.UniqueID(),
                message = ErrorMsg,
                eid = Id,
                log_date = DateTime.Now,
            });
            _ = _context.SaveChanges();
            //CLOSE THE EXISTING INSTANCE
            _context.ChangeTracker.Clear();
            //END CLOSE THE EXISTING INSTANCE  
        }
    }
}
