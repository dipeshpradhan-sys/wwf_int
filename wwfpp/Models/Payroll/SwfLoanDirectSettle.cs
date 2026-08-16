namespace wwfpp.Models.Payroll
{
    public class SwfLoanDirectSettle
    {
        public string id { get; set; }  //[nvarchar](255) NOT NULL,
        public decimal? amount { get; set; }  //[money] NULL,
        public DateTime? s_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](255) NULL,
        public string? swf_loan_id { get; set; }  //[nvarchar](255) NULL,
    }
}
