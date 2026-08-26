using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class OvertimeDashboardVM
    {
        public string OtReqId { get; set; }
        public int EmpId { get; set; }
        public DateTime OtDate { get; set; }
        public DateTime SubmitDate { get; set; }
        public double TotalHours { get; set; }
        public string OtDesc { get; set; }
        public int? RequestedBy { get; set; }

        // Extra fields for UI rendering
        public int? ToEmpId { get; set; }      // who the request is pending with
        public string ActionLinks { get; set; } = string.Empty;
    }

    // Group wrapper for employee-level grouping
    public class OvertimeDashboardGroup
    {
        public string EmployeeName { get; set; } = string.Empty;
        public List<OvertimeDashboardVM> Overtimes { get; set; } = new();
    }

}
