namespace wwfpp.EmailServices
{
    public class SmtpSettings
    {
        public string EmailId { get; set; } = "";
        public string SmtpServer { get; set; } = "";
        public bool DefaultCredentials { get; set; } = false;
        public int SmtpPort { get; set; } = 587;
        public string FromName { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string PassPhrase { get; set; } = "";
        public bool UseSSL { get; set; } = true;
    }
}
