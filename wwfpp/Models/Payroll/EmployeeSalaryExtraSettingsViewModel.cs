namespace wwfpp.Models.Payroll
{
    public class EmployeeSalaryExtraSettingsViewModel
    {
        //CompositPK
        public int emp_id { get; set; }
        public string? is_field_staff { get; set; }  //[nvarchar](1) NULL,
        public string? is_get_dashain { get; set; }  //[nvarchar](1) NULL,
        public byte? welfare_con_percent { get; set; }  //[tinyint] NULL,
        public string? timesheet_acceptance { get; set; }  //[nvarchar](1) NULL,
        public string? is_field_salary { get; set; }  //[nvarchar](1) NULL,
        public string? staff_type { get; set; }  //[nvarchar](1) NULL,
        public string? get_leave_accrual { get; set; }  //[nvarchar](1) NULL,
        public string? get_gratuity_accrual { get; set; }  //[nvarchar](1) NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public int? emp_year { get; set; }  //[int] NULL,
        public int? emp_month { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL
    }

}
