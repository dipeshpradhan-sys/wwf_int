namespace wwfpp.Models.Settings
{
    public class SettingTimesheetTypeViewModel
    {
        public int type_id { get; set; }                    //[int] NOT NULL
        public string? timesheet_type { get; set; }          //[nvarchar](50) NULL,
        public short? first_day_of_week { get; set; }       //[smallint] NULL,
    }
}
