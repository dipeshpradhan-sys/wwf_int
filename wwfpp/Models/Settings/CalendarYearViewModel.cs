namespace wwfpp.Models.Settings
{
    public class CalendarYearViewModel
    {
        public string calendar_year { get; set; }           //[nvarchar](9) NOT NULL,PRIMARY KEY
        public DateTime? calendar_date_from { get; set; }   //[datetime] NULL,
        public DateTime? calendar_date_to { get; set; }     //[datetime] NULL,
        public string? calendar_is_active { get; set; }     // [nvarchar](1) NULL,
        public string? calendar_year_abb { get; set; }       //[varchar](50) NULL,
    }
}
