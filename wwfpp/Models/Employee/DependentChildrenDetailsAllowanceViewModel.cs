namespace wwfpp.Models.Employee
{
    public class DependentChildrenDetailsAllowanceViewModel
    {
        public int emp_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? dep_allow_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public int? emp_dep_id { get; set; }  //[int] NULL,
        public decimal? amount_actual { get; set; }  //[money] NULL,
        public decimal? amount_paid { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
    }
    public class DependentChildrenDetailsListAllowanceViewModel
    {
        public string? mode { get; set; }
        public List<DependentChildrenDetailsAllowanceViewModel> Fields { get; set; }
    }
}
