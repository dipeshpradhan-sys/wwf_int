namespace wwfpp.Models.Request
{
    public class EmployeeLeaveAccrualNewFundWiseViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? fund_id { get; set; }  //[int] NULL,
        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public int? counter { get; set; }  //[int] NULL,
    }

}
