namespace wwfpp.Models.Settings
{
    public class SettingRateViewModel
    {
        public string setting_rate_id { get; set; }  //[nvarchar](50) NOT NULL,PRIMARY KEY
        public DateTime? setting_rate_date { get; set; }  //[datetime] NULL,
        public double? setting_rate { get; set; }  //[float] NULL,
        public int? setting_rate_period_name { get; set; }  //[int] NULL,
        public int? setting_rate_year { get; set; }  //[int] NULL,
        public string? setting_rate_status { get; set; }  //[nvarchar](1) NULL,
        public string? setting_rate_desc { get; set; }  //[ntext] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](20) NULL,
    }
}
