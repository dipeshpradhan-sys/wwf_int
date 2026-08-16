namespace wwfpp.wwwroot.js
{
    public class EmployeeTimesheetSubViewModel
    {
        //CompositPK
        public int? emp_id { get; set; }
        public short? emp_year { get; set; }  //[smallint] NULL,
        public short? emp_month { get; set; }  //[tinyint] NULL,
        public short? emp_day { get; set; }  //[tinyint] NULL,
        public int? fund_id { get; set; }  //[int] NULL,
        public double? time_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public double? overtime_hours { get; set; }  //[float] NULL,
        public string? is_active { get; set; }  //[nvarchar](5) NULL,
        public int? submit_counter { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL
    }

}
