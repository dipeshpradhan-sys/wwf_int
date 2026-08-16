namespace wwfpp.Models.Employee
{
    public class DependentChildrenDetailsAllowanceFundWiseViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? fund_id { get; set; }  //[int] NULL,
        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
