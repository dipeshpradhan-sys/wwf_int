namespace wwfpp.Models.Settings
{
    public class SettingLanguageViewModel
    {
        public int language_id { get; set; }  //[int] NOT NULL,
        public string? language { get; set; }  //[nvarchar](50) NULL,
        public DateTime? date { get; set; }  //[datetime] NULL,
    }
}
