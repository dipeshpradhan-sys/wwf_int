namespace wwfpp.Models.Settings
{
    public class SalaryTaxPercentViewModel
    {
        public int EmpId { get; set; }
        public string? Employee { get; set; }
        public string? Marital { get; set; }
        public string? Gender { get; set; }
        public string? EmpStatus { get; set; }
        public DateTime? StartDate{ get; set; } //Join date
        public DateTime? EndDate { get; set; } //expiry date
        public Decimal? Salary { get; set; }
        public string? percent_for_tax_add { get; set; }
    }

    public class SalaryTaxPercentListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<SalaryTaxPercentViewModel> Fields { get; set; }
    }

}
