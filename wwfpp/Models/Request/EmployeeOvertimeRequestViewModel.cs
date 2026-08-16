namespace wwfpp.Models.Request
{
    public class EmployeeOvertimeRequestViewModel
    {
        public string ot_req_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public DateTime? ot_date { get; set; }  //[datetime] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public string? ot_desc { get; set; }  //[nvarchar](255) NULL,
        public int? requested_by { get; set; }
        public string? req_status { get; set; }  //[nvarchar](1) NULL,
        public DateTime? req_date { get; set; }  //[datetime] NULL,
        public string? app_status { get; set; }  //[nvarchar](1) NULL,
        public int? app_by { get; set; }
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? is_paid { get; set; }  //[nvarchar](1) NULL,
        public int? paid_month { get; set; }  //[int] NULL,
        public int? paid_year { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public int? paid_day { get; set; }  //[int] NULL,
        public string? req_remarks { get; set; }  //[text] NULL,
        public string? app_remarks { get; set; }  //[text] NULL,
    }

}
