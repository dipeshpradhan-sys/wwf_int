namespace wwfpp.Models.Payroll
{
    public class WelfarePaidoutViewModel
    {
        public string? id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public short? wl_year { get; set; }  //[smallint] NULL,
        public short? wl_month { get; set; }  //[smallint] NULL,
        public double? wl_amount { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? wl_fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? wl_emp_week { get; set; }  //[tinyint] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public string? emp_status { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? employee { get; set; }
    }

}
