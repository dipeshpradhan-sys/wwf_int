namespace wwfpp.Models
{
    public class AppSettings
    {
        //values are defined on appsettings.jason file
        public required string BaseUrl { get; set; }
        public required string DATE_FORMAT { get; set; }
        public required string DATE_FORMAT_JS { get; set; }
        public required string DATE_SEP_CHAR { get; set; }
        public required string FISCAL_YEAR_PATTERN { get; set; }
        public required string FISCAL_YEAR_START { get; set; }
        public required string FY_LCFY_IS_SAME { get; set; }
        public required string IS_FLXIBLE { get; set; }
        public required string LCF_YEAR_PATTERN { get; set; }
        public required string LEAVE_YEAR_START { get; set; }
        public required string ShowJsErr { get; set; }
        public required string SITE_SESSION { get; set; }
        public required string SITE_TITLE { get; set; }
        public required string SKIP_BEHALF_APPROVAL { get; set; }
        public required string Version { get; set; }
        public required string YEAR_START { get; set; }







    }
}
