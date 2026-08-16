namespace wwfpp.Models.Request
{
    public class EmployeeLeaveAccrualNewViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public double? an_leave_balance { get; set; }  //[float] NULL,
        public double? an_leave_accrual { get; set; }  //[float] NULL,
        public double? si_leave_balance { get; set; }  //[float] NULL,
        public double? si_leave_accrual { get; set; }  //[float] NULL,
        public double? leave_accrual_days { get; set; }  //[float] NULL,
        public decimal? leave_payable { get; set; }  //[money] NULL,
        public decimal? pre_provisioned { get; set; }  //[money] NULL,
        public decimal? net_provision { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[int] NULL,
    }

}
