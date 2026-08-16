namespace wwfpp.wwwroot.js
{
    public class EmployeeOvertimeViewModel
    {
        public int ot_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }
        public int? sal_year { get; set; }  //[int] NULL,
        public int? sal_month { get; set; }  //[int] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public double? times { get; set; }  //[float] NULL,
        public decimal? rate { get; set; }  //[money] NULL,
        public double? hrs { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[nvarchar](100) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public int? submit_by { get; set; }
        public decimal? ot_diff { get; set; }  //[money] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public int? pay_period_total_working_hrs { get; set; }  //[int] NULL,
    }

}
