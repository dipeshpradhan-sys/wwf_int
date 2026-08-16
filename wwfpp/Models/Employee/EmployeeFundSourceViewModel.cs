namespace wwfpp.Models.Employee
{
    public class EmployeeFundSourceViewModel
    {
        public int emp_fund_id { get; set; }  //[int] NOT NULL,
        public int fund_id { get; set; }  //[int] NULL,
        public string? fund_source { get; set; } = string.Empty;
        public DateTime? expiry_date { get; set; }
        public double? annual_hrs { get; set; }  //[float] NULL,
        public DateTime? start_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? emp_code { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public string? fiscal_year_abb { get; set; } = string.Empty;
    }
    public class FundSourceDto
    {
        public int FundId { get; set; }
        public string? FundSource { get; set; }   // new property
        public decimal? AnnualHours { get; set; }
    }

}
