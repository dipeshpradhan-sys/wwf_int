using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class LeaveFutureDashboardVM
    {
        public List<LeaveFutureDashboardGroup> Groups { get; set; } = new();
    }

    public class LeaveFutureDashboardGroup
    {
        public string EmployeeName { get; set; }
        public List<LeaveFutureDashboardRow> Leaves { get; set; } = new();
    }

    public class LeaveFutureDashboardRow
    {
        public string LeaveType { get; set; }
        public string FiscalYear { get; set; }
        public string SubmitDate { get; set; }
        public string StartDate { get; set; }
        public string HoursDays { get; set; }
        public string StatusClass { get; set; }
        public string ActionLinks { get; set; }
    }

}
