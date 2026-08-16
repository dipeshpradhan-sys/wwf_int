namespace wwfpp.Models.Settings
{
    public class SettingPaycodeSubCategoryViewModel
    {
        public string sub_category_id { get; set; }  //[nvarchar](50) NOT NULL,

        public string? category_id { get; set; }  //[nvarchar](50) NULL,

        public string? sub_category_name { get; set; }  //[nvarchar](250) NULL,
        public string? sub_category_name_abbr { get; set; }  //[nvarchar](250) NULL,
        public string? sub_category_code { get; set; }  //[nvarchar](20) NULL,
        public string? sub_category_type { get; set; }  //[nvarchar](250) NULL,
        public string? staff_type { get; set; }  //[nvarchar](250) NULL,
        public string? amt_type { get; set; }  //[nvarchar](250) NULL,
        public string? p_category_id { get; set; }  //[nvarchar](50) NULL,
        public int? sort { get; set; }  //[int] NULL,
    }
}
