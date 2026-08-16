namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelSettlementSubViewModel
    {
        //CompositPK // trev_set_id+sn
        public string trav_set_id { get; set; } //[nvarchar](50) NULL,
        public short? sn { get; set; }  //[smallint] NULL,
        public DateTime? bill_date { get; set; }  //[datetime] NULL,
        public string? location { get; set; }       //[nvarchar](255) NULL,
        public string? description { get; set; }    //[nvarchar](255) NULL,
        //ref is a reserved keyword in C#, so you can’t use it directly as a property name.                        
        public string? RefField { get; set; }             //[nvarchar](50) NULL,
        public string? int_cur_name { get; set; }  //[nvarchar](50) NULL,       
        public double? int_rate { get; set; }  //[float] NULL,
        public decimal? int_amount { get; set; }  //[money] NULL,
        public decimal? int_usd_amount { get; set; }  //[money] NULL,
        public decimal? nat_bill_amount { get; set; }  //[money] NULL,
        public decimal? nat_VAT { get; set; }  //[money] NULL,
        public decimal? nat_TDS { get; set; }  //[money] NULL,
        public decimal? nat_amount { get; set; }  //[money] NULL
    }

}
