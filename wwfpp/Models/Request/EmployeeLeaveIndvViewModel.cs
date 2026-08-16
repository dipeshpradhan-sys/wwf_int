namespace wwfpp.wwwroot.js
{
    public class EmployeeLeaveIndvViewModel
    {
        public int indv_leave_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }
        public double? annual_leave { get; set; }  //[float] NULL,
        public double? casual_leave { get; set; }  //[float] NULL,
        public double? sick_leave { get; set; }  //[float] NULL,
        public double? annual_leave_hours_carry_forward { get; set; }  //[float] NULL,
        public double? maternity { get; set; }  //[float] NULL,
        public double? paternity { get; set; }  //[float] NULL,
        public double? mourning { get; set; }  //[float] NULL,
        public double? unpaid_study { get; set; }  //[float] NULL,
        public string? fiscal_year_to { get; set; }  //[nvarchar](15) NULL,
        public double? other_leave { get; set; }  //[float] NULL,
        public double? sick_leave_hours_carry_forward { get; set; }  //[float] NULL,
    }

}
