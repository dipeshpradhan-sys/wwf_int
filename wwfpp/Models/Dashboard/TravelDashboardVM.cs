using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class TravelDashboardVM
    {
        public int EmpId { get; set; }
        public int EmpTravelId { get; set; }
        public string TravelType { get; set; }
        public string Destinations { get; set; }
        public DateTime SubmitDate { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string IAppStatus { get; set; }
        public int? IAppBy { get; set; }
        public string AppStatus { get; set; }
        public int? AppBy { get; set; }
        public int? CanBy { get; set; }

        // Extra fields for UI rendering
        public int? ToEmpId { get; set; }      // who the request is pending with
        //public string ApproveCode { get; set; } = string.Empty;
        //public string DeclineCode { get; set; } = string.Empty;
        //public string ApproveLabel { get; set; } = string.Empty;
        //public string DeclineLabel { get; set; } = string.Empty;
        public string ActionLinks { get; set; } = string.Empty;
    }

    // Group wrapper for employee-level grouping
    public class TravelDashboardGroup
    {
        public string EmployeeName { get; set; } = string.Empty;
        public List<TravelDashboardVM> Travels { get; set; } = new();
    }

}
