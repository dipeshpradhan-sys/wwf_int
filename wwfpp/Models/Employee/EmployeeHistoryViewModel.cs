namespace wwfpp.Models.Employee
{
    public class EmployeeHistoryViewModel
    {
        //CompositPK
        public int emp_id { get; set; }  //[int] NOT NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? employee_type { get; set; }  //[nvarchar](15) NULL,
        public string? department { get; set; }  //[nvarchar](50) NULL,
        public string? post { get; set; }  //[nvarchar](50) NULL,
        public decimal? salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? child_edu_all { get; set; }  //[money] NULL,
        public string? emp_status_for { get; set; }  //[nvarchar](1) NULL, /*A = Active, D = Inactive*/
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public decimal? remote_area_allow { get; set; }  //[money] NULL,
        public decimal? yearly_remote_exem { get; set; }  //[money] NULL,
        public int? by_emp_id { get; set; }  //[int] NULL,
        public string? job_family { get; set; }  //[nvarchar](255) NULL,
        public string? emp_level { get; set; }  //[nvarchar](255) NULL,
        public int? manager_id { get; set; }  //[int] NULL,
        public int? line_manager_id { get; set; }  //[int] NULL,
        public string? marital_status { get; set; }  //[nvarchar](1) NULL,
        public int? no_of_children { get; set; }  //[int] NULL
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }
}
