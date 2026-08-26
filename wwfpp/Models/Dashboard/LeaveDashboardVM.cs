using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class LeaveDashboardVM
    {
        public List<LeaveDashboardGroup> Groups { get; set; } = new();
    }

    public class LeaveDashboardGroup
    {
        public string EmployeeName { get; set; }
        public List<LeaveDashboardRow> Leaves { get; set; } = new();
    }

    public class LeaveDashboardRow
    {
        public string LeaveType { get; set; }
        public string SubmitDate { get; set; }
        public string StartDate { get; set; }
        public string HoursDays { get; set; }
        public string StatusClass { get; set; }
        public string ActionLinks { get; set; }
    }

}
