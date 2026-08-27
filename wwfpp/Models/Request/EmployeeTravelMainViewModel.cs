using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Request
{
    public class EmployeeTravelMainViewModel
    {
        // Core travel info
        public int emp_id { get; set; }
        public int id { get; set; }
        public string? emp_status { get; set; }
        public string? fiscal_year { get; set; }
        public int? EmpTravelId { get; set; }
        public string? TravelType { get; set; }
        public string? TripPurpose { get; set; }
        public string? Destinations { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateFrom { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateTo { get; set; }
        public string? Denomination { get; set; }
        public string? Remarks { get; set; }

        // Expenses
        public List<ExpenseViewModel> Expenses { get; set; } = new();

        // Funding Sources
        public List<TravelFundSourceViewModel> TravelFundSources { get; set; } = new();

        // Totals per currency
        public decimal TotalNRS { get; set; }
        public decimal TotalIC { get; set; }
        public decimal TotalUSD { get; set; }
        public decimal TotalEuro { get; set; }
        public decimal TotalPound { get; set; }
        public decimal TotalCHF { get; set; }

        public string? can_remarks { get; set; }
        public string? can_desc { get; set; }
        public int? can_by { get; set; }
        public string? i_app_status { get; set; }
        public string? i_app_by_name { get; set; }
        public string? rec_remarks { get; set; }
        public DateTime? i_app_date { get; set; }
        public string? app_by_name { get; set; }
        public string? app_status { get; set; }
        public DateTime? app_date { get; set; }
        public string? app_remarks { get; set; }
        public int? ApproverId { get; set; }
        public string? ApproverEmail { get; set; }
    }

    public class ExpenseViewModel
    {
        public int par_id { get; set; }
        public string? Particular { get; set; }
        public string? Detail { get; set; }
        public string? Unit { get; set; }
        public byte Currency { get; set; }
        public double? Nos { get; set; }     // float → double?
        public decimal? Rate { get; set; } = 0;
        //public decimal? Amount => Nos * Rate;
        public decimal? Amount { get; set; }
    }

    public class TravelFundSourceViewModel
    {
        public int? FundId { get; set; }
    }

}
