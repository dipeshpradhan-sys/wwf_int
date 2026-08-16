namespace wwfpp.Models.Settings
{
    public class FiscalYearViewModel
    {
        public string fiscal_year { get; set; } // [nvarchar](9) NOT NULL,
        public DateTime? date_from { get; set; }    //[datetime] NULL,
        public DateTime? date_to { get; set; }      //[datetime] NULL,
        public string? is_active { get; set; }      //[nvarchar] (1) NULL,
        public string? fiscal_year_abb { get; set; }       //[varchar](50) NULL,
        public int yearly_working_hrs { get; set; } // int null
    }

}
