namespace wwfpp.wwwroot.js
{
    public class EmployeeLeaveIndvPaidClearedViewModel
    {
        public int indv_leave_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year { get; set; }  //[nvarchar](15) NULL,
        public double? annual_leave { get; set; }  //[float] NULL,
        public double? casual_leave { get; set; }  //[float] NULL,
        public double? sick_leave { get; set; }  //[float] NULL,
        public double? other_leave { get; set; }  //[float] NULL,
        public double? maternity { get; set; }  //[float] NULL,
        public double? paternity { get; set; }  //[float] NULL,
        public double? mourning { get; set; }  //[float] NULL,
        public double? unpaid_study { get; set; }  //[float] NULL,
        public double? annual_leave_hours_carry_forward { get; set; }  //[float] NULL,
        public double? sick_leave_hours_carry_forward { get; set; }  //[float] NULL,
        public double? annual_leave_paid { get; set; }  //[float] NULL,
        public double? annual_leave_laps { get; set; }  //[float] NULL,
        public double? casual_leave_laps { get; set; }  //[float] NULL,
        public double? sick_leave_laps { get; set; }  //[float] NULL,
        public double? other_leave_laps { get; set; }  //[float] NULL,
        public double? maternity_laps { get; set; }  //[float] NULL,
        public double? paternity_laps { get; set; }  //[float] NULL,
        public double? mourning_laps { get; set; }  //[float] NULL,
        public double? unpaid_study_laps { get; set; }  //[float] NULL,
        public double? annual_leave_hours_carry_forward_laps { get; set; }  //[float] NULL,
        public double? sick_leave_hours_carry_forward_laps { get; set; }  //[float] NULL,
        public DateTime? date_from { get; set; }  //[datetime] NULL,
        public DateTime? date_upto { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
    }

}
