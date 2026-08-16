namespace wwfpp.Models.Settings
{
    public class SettingLimitHrsViewModel
    {
        public string hrs_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? normal_working_hrs { get; set; }  //[int] NULL,
        public int? overtime_normal_working_hrs { get; set; }  //[int] NULL,
        public int? overtime_hol_wek_working_hrs { get; set; }  //[int] NULL,
        public int? working_hours_per_pay_period { get; set; }  //[int] NULL,
        public string? populate_hrs_in_timesheet_for_holiday { get; set; }  //[nvarchar](1) NULL,
        public string? populate_hrs_in_timesheet_for_weekend { get; set; }  //[nvarchar](1) NULL,
    }
}
