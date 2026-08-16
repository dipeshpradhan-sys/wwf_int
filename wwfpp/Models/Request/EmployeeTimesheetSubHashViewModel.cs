namespace wwfpp.wwwroot.js
{
    public class EmployeeTimesheetSubHashViewModel
    {
        public string id { get; set; }            //[varchar] (50) NOT NULL
        public int? emp_id { get; set; }
        public short? emp_year { get; set; }  //[smallint] NULL,
        public byte? emp_month { get; set; }  //[tinyint] NULL,
        public byte? emp_day { get; set; }  //[tinyint] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL,
        public int? fund_id { get; set; }  //[int] NULL,
        public double? time_hours { get; set; }  //[float] NULL,
        public double? overtime_hours { get; set; }  //[float] NULL
    }

}
