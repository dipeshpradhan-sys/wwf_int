using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace wwfpp.Models
{
    public class EmployeeOvertimeAddEditViewModel
    {
        public string OtReqId { get; set; }
        public string FiscalYear { get; set; }
        public DateTime? OtDate { get; set; }
        public double TotalHours { get; set; }
        public string OtDesc { get; set; }
        public int? RequestedBy { get; set; }
        public string? emp_status { get; set; }
        public string ? id { get; set; }
        public int emp_id { get; set; }
        public List<OvertimeSessionViewModel> Sessions { get; set; } = new();


    }

    public class OvertimeSessionViewModel
    {
        public int Sno { get; set; }
        public int? StartHour { get; set; }
        public int? StartMinute { get; set; }
        public int? EndHour { get; set; }
        public int? EndMinute { get; set; }
        public double Hours { get; set; }
        // 🔑 New helper property: tells Razor whether to show Remove button
        public bool CanRemove { get; set; } = false;
    }
}

