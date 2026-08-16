namespace wwfpp.wwwroot.js
{
    public class EmployeeTimesheetAppViewModel
    {
        public string app_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public int? emp_year { get; set; }  //[int] NULL,
        public int? emp_month { get; set; }  //[int] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? app_dec { get; set; }  //[nvarchar](1) NULL,
        public int? app_by { get; set; }  //[int] NULL,
        public int? submit_counter { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL,
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public string? app_remarks { get; set; }  //[text] NULL,
    }

}
