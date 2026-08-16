namespace wwfpp.Models.Request
{
    public class EmployeeDayoffViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public DateTime? dayoff_date { get; set; }  //[datetime] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
    }
}
