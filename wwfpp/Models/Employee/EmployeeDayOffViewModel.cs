namespace wwfpp.Models.Employee
{
    public class EmployeeDayOffViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public DateTime? dayoff_date { get; set; }  //[datetime] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public string? fiscal_year_abb { get; set; } = string.Empty;
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? emp_code { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; } = string.Empty;
    }
}
