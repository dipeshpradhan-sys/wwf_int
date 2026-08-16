using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.General
{
    public class FundSourceViewModel
    {
        public int fund_id { get; set; }            //[int] NOT NULL,
        public string? fund_source { get; set; }     //[nvarchar] (50) NULL,
        public string? fund_desc { get; set; }       //[nvarchar] (255) NULL,
        public string? fund_status { get; set; }     //[nvarchar] (1) NULL,
        public DateTime? expiry_date { get; set; }   //[datetime] NULL,
        public string? default_for_holiday { get; set; } //[nvarchar] (1) NULL,
    }
}
