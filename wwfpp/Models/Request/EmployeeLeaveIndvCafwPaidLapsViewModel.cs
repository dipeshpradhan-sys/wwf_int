namespace wwfpp.wwwroot.js
{
    public class EmployeeLeaveIndvCafwPaidLapsViewModel
    {
        public int indv_leave_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](15) NULL,
        public double? max_annual_leave_cafw { get; set; }  //[float] NULL,
        public double? tot_annual_leave_paid { get; set; }  //[float] NULL,
        public double? cur_annual_leave_laps { get; set; }  //[float] NULL,
        public double? max_sick_leave_cafw { get; set; }  //[float] NULL,
        public double? tot_sick_leave_paid { get; set; }  //[float] NULL,
        public double? cur_sick_leave_laps { get; set; }  //[float] NULL,
        public int? sumbit_counter { get; set; }  //[int] NULL,
        public double? bacic_salary { get; set; }  //[float] NULL,
        public double? tot_annual_leave_amt { get; set; }  //[float] NULL,
        public double? tot_sick_leave_amt { get; set; }  //[float] NULL,
        public int? paid_month { get; set; }  //[int] NULL,
        public int? paid_year { get; set; }  //[int] NULL,
    }
}
