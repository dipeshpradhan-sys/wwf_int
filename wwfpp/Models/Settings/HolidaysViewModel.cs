namespace wwfpp.Models.Settings
{
    public class HolidaysViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public DateTime? holiday_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public string? fiscal_year_abb { get; set; } = string.Empty;
    }
}
