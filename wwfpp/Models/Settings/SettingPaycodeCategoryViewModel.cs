namespace wwfpp.Models.Settings
{
    public class SettingPaycodeCategoryViewModel
    {
        public string category_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? category_name { get; set; }  //[nvarchar](250) NULL,
        public string? category_name_abbr { get; set; }  //[nvarchar](250) NULL,
    }

}
