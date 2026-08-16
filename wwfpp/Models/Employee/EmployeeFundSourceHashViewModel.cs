namespace wwfpp.Models.Employee
{
    public class EmployeeFundSourceHashViewModel
    {
        public int id { get; set; }  //[int] NOT NULL,
        public int user_id { get; set; }  //[int] NULL,
        public string? emp_code { get; set; }  //[nvarchar](6) NULL,
        public int emp_id { get; set; }  //[int] NULL,
        public int? fund_id { get; set; }  //[int] NULL,
        public string? fund_source { get; set; }  //[nvarchar](50) NULL,
        public double? annual_hrs { get; set; }  //[float] NULL,
        public DateTime? start_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }
}
