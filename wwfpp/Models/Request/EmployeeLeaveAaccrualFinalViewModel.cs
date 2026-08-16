namespace wwfpp.Models.Request
{
    public class EmployeeLeaveAaccrualFinalViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public double? carry_forward_leave { get; set; }  //[float] NULL,
        public double? annual_leave { get; set; }  //[float] NULL,
        public double? leave_taken { get; set; }  //[float] NULL,
        public double? leave_balance { get; set; }  //[float] NULL,
        public double? leave_accrual { get; set; }  //[float] NULL,
        public decimal? leave_encash { get; set; }  //[money] NULL,
        public decimal? pre_encash { get; set; }  //[money] NULL,
        public decimal? net_encash { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
        public double? an_paid_cleared { get; set; }  //[float] NULL,
        public double? si_carry_forward { get; set; }  //[float] NULL,
        public double? si_current { get; set; }  //[float] NULL,
        public double? si_taken { get; set; }  //[float] NULL,
        public double? si_paid_cleared { get; set; }  //[float] NULL,
        public double? si_balance { get; set; }  //[float] NULL,
        public double? si_accrual { get; set; }  //[float] NULL,
        public double? si_encash { get; set; }  //[float] NULL,
        public double? eli_day { get; set; }  //[float] NULL,
        public decimal? eli_amt { get; set; }  //[money] NULL,
    }

}
