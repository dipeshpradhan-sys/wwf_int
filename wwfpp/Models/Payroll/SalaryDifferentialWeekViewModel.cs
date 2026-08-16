namespace wwfpp.Models.Payroll
{
    public class SalaryDifferentialWeekViewModel
    {
        public string fiscal_year { get; set; }  //[nvarchar](20) NOT NULL,
        public string? timesheet_type { get; set; }  //[nvarchar](50) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL
    }

}
