using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class ApprovalFromEmailVM
    {
        public int? EmpID { get; set; }
        public string? AppID { get; set; } = default!;

        public int? Month { get; set; }
        public int? Year { get; set; }
        public int? ToID { get; set; }
        public int? ToEmpID { get; set; }

        public string? St { get; set; }
        public int? Counter { get; set; }

        public string?Description { get; set; }
        public string? ApproveFor { get; set; }
        

    }
}
