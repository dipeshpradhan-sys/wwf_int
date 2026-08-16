namespace wwfpp.Models.Employee
{
    public class EmployeeDependentChildrenDetailsAllowanceFinalViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](20) NULL,
        public int? emp_dep_id { get; set; }  //[int] NULL,
        public decimal? amount_actual { get; set; }  //[money] NULL,
        public decimal? amount_paid { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
        public double? dependant_age { get; set; }  //[float] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
