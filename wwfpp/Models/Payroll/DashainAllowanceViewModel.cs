using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Payroll
{
    public class DashainAllowanceViewModel
    {
        public int emp_id { get; set; }

        public int? sal_year { get; set; }

        public int? sal_month { get; set; }

        public string? FullName { get; set; }

        public string? emp_code { get; set; }

        [DataType(DataType.Currency)]
        public decimal? dashain_amount { get; set; }

        public double? total_hours { get; set; }   // float

        [StringLength(250)]
        public string? remarks { get; set; }

        public string? fiscal_year { get; set; }
    }

    public class DashainAllowancListeViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<DashainAllowanceViewModel> Fields { get; set; }
    }
}
