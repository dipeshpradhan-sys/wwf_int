namespace wwfpp.Models.Payroll
{
    public class WelfareInterestViewModel
    {
        public string? id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public short? wl_year { get; set; }  //[smallint] NULL,
        public short? wl_month { get; set; }  //[smallint] NULL,
        public double? wl_amount { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? wl_fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? wl_emp_week { get; set; }  //[tinyint] NULL,
    }
    public class WelfareInterestListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<WelfareInterestViewModel> Fields { get; set; }
    }
}
