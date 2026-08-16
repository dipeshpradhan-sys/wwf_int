namespace wwfpp.Models.Settings
{
    public class GeneralSettingViewModel
    {
        public int id { get; set; }  //[int] NOT NULL,
        public string? description { get; set; }  //[nvarchar](9) NULL,
        public double? max_leave_hours { get; set; }  //[float] NULL,
        public double? max_leave_days { get; set; }  //[float] NULL,
    }
}
