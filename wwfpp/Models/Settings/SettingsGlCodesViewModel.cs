namespace wwfpp.Models.Settings
{
    public class SettingsGlCodesViewModel
    {
        public int id { get; set; }  //[int] NOT NULL,
        public string? gl_code { get; set; }  //[nvarchar](50) NULL,
        public string? gl_type { get; set; }  //[nvarchar](1) NULL,
        public string? staff_type { get; set; }  //[nvarchar](1) NULL,
    }
}
