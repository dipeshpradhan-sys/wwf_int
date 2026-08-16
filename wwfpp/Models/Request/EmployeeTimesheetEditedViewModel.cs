namespace wwfpp.wwwroot.js
{
    public class EmployeeTimesheetEditedViewModel
    {
        //CompositPK
        public int? emp_id { get; set; }
        public int? emp_year { get; set; }  //[int] NULL,
        public int? emp_month { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? emp_week { get; set; }  //[int] NULL,
        public int submit_counter { get; set; }  //[int] NOT NULL,
        public string? view_status { get; set; }  //[nvarchar](1) NULL,
        public int account_emp_id { get; set; }  //[int] NOT NULL,
        public DateTime? updated_date { get; set; }  //[datetime] NOT NULL
    }

}
