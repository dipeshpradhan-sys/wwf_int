namespace wwfpp.Models.Payroll
{
    public class SwfLoanViewModel
    {
        public string id { get; set; }  //[nvarchar](255) NOT NULL,
        public int? emp_id { get; set; }//[int] NULL,
        public string? start_month { get; set; }  //[nvarchar](2) NULL,
        public string? start_year { get; set; }  //[nvarchar](4) NULL,
        public decimal? amount { get; set; }  //[money] NULL,
        public decimal? int_amount { get; set; }  //[money] NULL,
        public int? no_of_installment { get; set; }  //[int] NULL,
        public string? status { get; set; }  //[nvarchar](1) NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public decimal? total_loan { get; set; }  //[money] NULL,
        public decimal? month_installment { get; set; }  //[money] NULL,
        public decimal? s_amount { get; set; }
        public DateTime? s_date { get; set; }
        public string? s_remarks { get; set; }
        public decimal? paid_amount { get; set; }
        public string? emp_code { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? employee {  get; set; }
        public string? emp_status { get; set; }
        public List<SettlementRow> Settlements { get; set; } = new List<SettlementRow>();
        public List<HistoryRow> History { get; set; } = new List<HistoryRow>();
        public TotalsRow Totals { get; set; } = new TotalsRow();
        public SwfLoanTotalViewModel TloanInt { get; set; } = new SwfLoanTotalViewModel();

    }
    public class SettlementRow
    {
        public DateTime? s_date { get; set; }
        public string remarks { get; set; }
        public decimal? amount { get; set; }
    }

    public class HistoryRow
    {
        public int sal_year { get; set; }
        public int sal_month { get; set; }
        public decimal? loan { get; set; }
    }

    public class TotalsRow
    {
        public decimal? TotalDueLoan { get; set; }
        public decimal? TotalPaidLoan { get; set; }
    }
    public class SwfLoanTotalViewModel
    {
        public decimal? amount { get; set; }  //[money] NULL,
        public decimal? int_amount { get; set; }  //[money] NULL,
        public decimal? TLoanInt { get; set; }  //[money] NULL,
        public decimal? paid_amount { get; set; }  //[money] NULL,
    }

}
