namespace wwfpp.Models.Personnel
{
    public class TravelListViewModel
    {
        public int? EmpTravelId { get; set; }
        public string? TravelType { get; set; }
        public string? Destinations { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? SubmitDate { get; set; }
        public string? AppStatus { get; set; }
        public int? EmpId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Lastname { get; set; }
        public string? EmpStatus { get; set; }
        public string? Employee { get; set; }
        public string? ShowPrint { get; set; }
        public string? ShowBtnCan { get; set; }
    }
    public class TravelViewModel
    {
        public int? EmpTravelId { get; set; }
        public string? TravelType { get; set; }
        public string? TripPurpose { get; set; }
        public string? Destinations { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? SubmitDate { get; set; }
        public string? Denomination { get; set; }
        public string? Remarks { get; set; }


        public int? EmpId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Lastname { get; set; }
        public string? Employee { get; set; }
        public string? EmpStatus { get; set; }


        public string? IAppStatus { get; set; }
        public int? IAppBy { get; set; }
        public string? IAppByName { get; set; }
        public DateTime? IAppDate { get; set; }
        public string? IAppByPost { get; set; }
        public string? IAppRemarks { get; set; }        //actual field name id RecRemarks


        public string? AppStatus { get; set; }
        public int? AppBy { get; set; }
        public string? AppByName { get; set; }
        public DateTime? AppDate { get; set; }
        public string? AppByPost { get; set; }
        public string? AppRemarks { get; set; }


        public DateTime? CanSubmitDate { get; set; }
        public string? CanDesc { get; set; }

        public int? CanBy { get; set; }
        public string? CanByName { get; set; }
        public DateTime? CanDate { get; set; }
        public string? CanRemarks { get; set; }

        public string? FiscalYear { get; set; }
        public string? FiscalYearAbb { get; set; }
        public DateTime? StartFiscalDate { get; set; }
        public DateTime? EndFiscalDate { get; set; }
        // Expenses
        public List<TravelExpenseViewModel> TravelExpenses { get; set; }

        // Funding Sources
        public List<TravelFundSourceViewModel> TravelFundSources { get; set; }

        // Totals per currency
        public decimal TotalNRS { get; set; }
        public decimal TotalIC { get; set; }
        public decimal TotalUSD { get; set; }
        public decimal TotalEuro { get; set; }
        public decimal TotalPound { get; set; }
        public decimal TotalCHF { get; set; }

        //To use in travel print section
        public string? EmployeeSignature { get; set; }
        public string? RecommendedSignature { get; set; }
        public string? ApprovedSignature { get; set; }
    }

    public class TravelExpenseViewModel
    {
        public byte? ParId { get; set; }
        public string? Particular { get; set; }
        public string? Detail { get; set; }
        public string? Unit { get; set; }
        public byte? CurId { get; set; }
        public string? Currency { get; set; }
        public double? Nos { get; set; }     // float -> double
        public decimal? Rate { get; set; } = 0;
        public decimal? Amount { get; set; }    //public decimal? Amount => Nos * Rate;

        public DateTime? SubmitDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    public class TravelFundSourceViewModel
    {
        public byte? Sn { get; set; }
        public int? FundId { get; set; }
        public string? FundName { get; set; }
    }
}
