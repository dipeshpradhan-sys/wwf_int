namespace wwfpp.Models.Request
{
    public class EmployeeLeaveViewModel
    {
        public int emp_leave_id { get; set; }  //[int] NOT NULL,
        public byte? leave_type_id { get; set; }  //[tinyint] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? leave_from_date { get; set; }  //[datetime] NULL,
        public DateTime? leave_to_date { get; set; }  //[datetime] NULL,
        public string? leave_desc { get; set; }  //[ntext] NULL,
        public string? app_status { get; set; }  //[nvarchar](20) NULL,
        public int? app_by { get; set; }  //[int] NULL,
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public double? leave_in_hrs { get; set; }  //[float] NULL,
        public string? app_remarks { get; set; }  //[text] NULL,
        public DateTime? can_submit_date { get; set; }  //[datetime] NULL,
        public string? can_desc { get; set; }  //[ntext] NULL,
        public int? can_by { get; set; }  //[int] NULL,
        public DateTime? can_date { get; set; }  //[datetime] NULL,
        public string? can_remarks { get; set; }  //[ntext] NULL,
    }
}
