namespace wwfpp.Models.Employee
{
    public class DependentChildrenDetailsAllowanceEmpWiseViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public decimal? amount_paid { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
