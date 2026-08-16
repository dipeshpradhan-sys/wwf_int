namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelSettlementMainViewModel
    {
        public string trav_set_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_travel_id { get; set; }  //[int] NULL,
        public int? emp_id { get; set; }
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? travel_date { get; set; }  //[datetime] NULL,
        public DateTime? return_date { get; set; }  //[datetime] NULL,
        public double? usd_rate { get; set; }  //[float] NULL,
        public decimal? adv_cash_less { get; set; }  //[money] NULL,
        public string? charge_per_or_amt { get; set; }  //[nvarchar](1) NULL,
        public int? charge_fund_id_1 { get; set; }  //[int] NULL,   //currently FK not defined with fund_source
        public int? charge_fund_id_2 { get; set; }  //[int] NULL,   //currently FK not defined with fund_source
        public int? charge_fund_id_3 { get; set; }  //[int] NULL,   //currently FK not defined with fund_source
        public int? charge_fund_id_4 { get; set; }  //[int] NULL,   //currently FK not defined with fund_source
        public double? charge_fund_per_1 { get; set; }  //[float] NULL, 
        public double? charge_fund_per_2 { get; set; }  //[float] NULL, 
        public double? charge_fund_per_3 { get; set; }  //[float] NULL, 
        public double? charge_fund_per_4 { get; set; }  //[float] NULL, 
        public decimal? charge_fund_amt_1 { get; set; }  //[money] NULL,
        public decimal? charge_fund_amt_2 { get; set; }  //[money] NULL,
        public decimal? charge_fund_amt_3 { get; set; }  //[money] NULL,
        public decimal? charge_fund_amt_4 { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[nvarchar](255) NULL,
        public string? app_status { get; set; }  //[nvarchar](1) NULL,
        public int? app_by { get; set; }  //[int] NULL,
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public string? is_for_set { get; set; }  //[nvarchar](1) NULL,
    }

}
