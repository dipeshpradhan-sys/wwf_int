namespace wwfpp.wwwroot.js
{
    public class EmployeeTimesheetMainViewModel
    {
        //CompositPK
        public int? emp_id { get; set; }
        public short? emp_year { get; set; }  //[smallint] NULL,
        public byte? emp_month { get; set; }  //[tinyint] NULL,
        public byte? emp_day { get; set; }  //[tinyint] NULL,
        public byte? leave_type_id { get; set; }  //[tinyint] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public int? submit_counter { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL
    }

}
