namespace wwfpp.Models.Settings
{
    public class CalendarSettingViewModel
    {
        public int cal_id { get; set; } //[int] NOT NULL,
        public byte cal_month { get; set; } //[tinyint] NULL,
        public short cal_year { get; set; } //[smallint] NULL,
        public string? d1 { get; set; }//[nvarchar](5) NULL,
        public string? d2 { get; set; }//[nvarchar](5) NULL,
        public string? d3 { get; set; }//[nvarchar](5) NULL,
        public string? d4 { get; set; }//[nvarchar](5) NULL,
        public string? d5 { get; set; }//[nvarchar](5) NULL,
        public string? d6 { get; set; }//[nvarchar](5) NULL,
        public string? d7 { get; set; }//[nvarchar](5) NULL,
        public string? d8 { get; set; }//[nvarchar](5) NULL,
        public string? d9 { get; set; }//[nvarchar](5) NULL,
        public string? d10 { get; set; }//[nvarchar](5) NULL,
        public string? d11 { get; set; }//[nvarchar](5) NULL,
        public string? d12 { get; set; }//[nvarchar](5) NULL,
        public string? d13 { get; set; }//[nvarchar](5) NULL,
        public string? d14 { get; set; }//[nvarchar](5) NULL,
        public string? d15 { get; set; }//[nvarchar](5) NULL,
        public string? d16 { get; set; }//[nvarchar](5) NULL,
        public string? d17 { get; set; }//[nvarchar](5) NULL,
        public string? d18 { get; set; }//[nvarchar](5) NULL,
        public string? d19 { get; set; }//[nvarchar](5) NULL,
        public string? d20 { get; set; }//[nvarchar](5) NULL,
        public string? d21 { get; set; }//[nvarchar](5) NULL,
        public string? d22 { get; set; }//[nvarchar](5) NULL,
        public string? d23 { get; set; }//[nvarchar](5) NULL,
        public string? d24 { get; set; }//[nvarchar](5) NULL,
        public string? d25 { get; set; }//[nvarchar](5) NULL,
        public string? d26 { get; set; }//[nvarchar](5) NULL,
        public string? d27 { get; set; }//[nvarchar](5) NULL,
        public string? d28 { get; set; }//[nvarchar](5) NULL,
        public string? d29 { get; set; }//[nvarchar](5) NULL,
        public string? d30 { get; set; }//[nvarchar](5) NULL,
        public string? d31 { get; set; }//[nvarchar](5) NULL,
    }

    public class CalendarSettingBiweeklyViewModel
    {
        public int cal_id { get; set; }                 //[int] NOT NULL,
        public string? fiscal_year { get; set; }        //[nvarchar](10) NULL,
        public DateTime? period_start_date { get; set; } //[datetime] NULL,
        public DateTime? period_end_date { get; set; }   //[datetime] NULL,
        public int week_name { get; set; }              //[int] NULL,    
        public string? d1 { get; set; }                 //[nvarchar](5) NULL
        public string? d2 { get; set; }                 //[nvarchar](5) NULL
        public string? d3 { get; set; }                 //[nvarchar](5) NULL
        public string? d4 { get; set; }                 //[nvarchar](5) NULL
        public string? d5 { get; set; }                 //[nvarchar](5) NULL
        public string? d6 { get; set; }                 //[nvarchar](5) NULL
        public string? d7 { get; set; }                 //[nvarchar](5) NULL
        public string? d8 { get; set; }                 //[nvarchar](5) NULL    
        public string? d9 { get; set; }                 //[nvarchar](5) NULL
        public string? d10 { get; set; }                //[nvarchar](5) NULL
        public string? d11 { get; set; }                //[nvarchar](5) NULL
        public string? d12 { get; set; }                //[nvarchar](5) NULL    
        public string? d13 { get; set; }                //[nvarchar](5) NULL
        public string? d14 { get; set; }                //[nvarchar](5) NULL
    }

    public class CalendarSettingWeeklyViewModel
    {
        public int cal_id { get; set; }     // [int] NOT NULL,
        public string? fiscal_year { get; set; }    // [nvarchar](10) NULL,
        public DateTime? period_start_date { get; set; } //[datetime] NULL,
        public DateTime? period_end_date { get; set; }//[datetime] NULL,
        public int week_name { get; set; }   //[int] NULL,
        public string? d1 { get; set; } //[nvarchar](5) NULL,
        public string? d2 { get; set; } //[nvarchar](5) NULL,
        public string? d3 { get; set; } //[nvarchar](5) NULL,
        public string? d4 { get; set; } //[nvarchar](5) NULL,
        public string? d5 { get; set; } //[nvarchar](5) NULL,
        public string? d6 { get; set; } //[nvarchar](5) NULL,
        public string? d7 { get; set; } //[nvarchar](5) NULL,
    }


}
