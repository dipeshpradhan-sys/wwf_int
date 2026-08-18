using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class EmployeeNameViewModel
    {
        //This is for the employee name
        ///public string emp_name_code { get; set; }
    }
    public class EmployeeDropDownViewModel
    {
        //This is for the dropdown of the employees
        public int emp_id { get; set; }
        public string emp_name_code { get; set; }
    }
    public class DataFilterRequest
    {
        public string Status { get; set; } // Custom filter
        public int? empFilter { get; set; } // Custom filter
        public string? FiscalYearFilter { get; set; } // Custom filter
        public string? EmployeeFilter { get; set; } // Custom filter
        public string? EmployeeStatusFilter { get; set; } // Custom filter
        public string? ApprovalStatusFilter { get; set; } // Custom filter
        public string? emailTo { get; set; } // Custom filter
        public string? emailSubject { get; set; } // Custom filter
        public DateTime? submit_date { get; set; } // Custom filter
        public DateTime? sent_date { get; set; } // Custom filter
        public string? leaveStatusFilter { get; set; } // Custom filter

        public int? Year { get; set; } // Custom filter

        public int? Month { get; set; } // Custom filter
        public string? PeriodFilter { get; set; } // Custom filter
        
    }
    public class CostumFilterRequest
    {
        public string FilterValue { get; set; }
    }
    public class DeleteRequest
    {
        public List<string> SelectedIds { get; set; }
    }
    public class bulkStatusUpdateRequest
    {
        public List<string> SelectedIds { get; set; }
        public string? mode { get; set; }
        public string? hStatus { get; set; }
    }

    public class BulkUpdateRequest
    {
        public string? mode { get; set; }
        public List<string> SelectedIds { get; set; } = new();
        public List<FieldRequest> Fields { get; set; } = new();
    }

    public class FieldRequest
    {
        public string? Field1 { get; set; }
        public string? Field2 { get; set; }
        public string? Field3 { get; set; }
        public string? Field4 { get; set; }
        public string? Field5 { get; set; }
    }

    public class modeRequest
    {
        public string mode { get; set; }
    }

    public class MultipleCostumFilterRequest
    {
        public required string FilterValue1 { get; set; }
        public string? FilterValue2 { get; set; }
        public string? FilterValue3 { get; set; }
        public string? FilterValue4 { get; set; }
        public string? FilterValue5 { get; set; }
        public string? FilterValue6 { get; set; }
        public string? FilterValue7 { get; set; }
        public string? FilterValue8 { get; set; }
        public string? FilterValue9 { get; set; }

    }
    public class FundSourceDropDownViewModel
    {
        //This is for the dropdown of the Fund Sources
        public int fund_id { get; set; }
        public string fund_source { get; set; }
    }
}