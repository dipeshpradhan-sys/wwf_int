namespace wwfpp.Models.General
{
    public class CurrencyViewModel
    {
        public byte cur_id { get; set; }  //[tinyint] NOT NULL,
        public string cur_abbr { get; set; }  //[nvarchar](20) NULL,
        public string cur_name { get; set; }  //[nvarchar](50) NULL,
    }
}
