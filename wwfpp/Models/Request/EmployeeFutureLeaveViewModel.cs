namespace wwfpp.Models
{
    public class EmployeeFutureLeaveViewModel
    {
        public int id { get; set; }                     // emp_leave_id
        public int emp_leave_id { get; set; }                     // emp_leave_id
        
        public int? emp_id { get; set; }                 // emp_id
        public string? EmployeeName { get; set; }       // joined from tbl_employee

        // Dropdowns
        public string? FiscalYear { get; set; }         // fiscal_year
        public string? FutureFiscalYear { get; set; }         // fiscal_year
        public byte? LeaveTypeId { get; set; }          // leave_type_id
        public string? LeaveType { get; set; }          // description from tbl_leave_heading

        // Dates
        public DateTime? SubmitDate { get; set; }
        public DateTime? LeaveFromDate { get; set; }
        public DateTime? LeaveToDate { get; set; }

        // Hours/Days
        public double? LeaveInHours { get; set; }       // leave_in_hrs
        public double? LeaveInDays { get; set; }        // computed

        // Status
        public string? Status { get; set; }             // app_status
        public string? Remarks { get; set; }            // app_remarks
        public int? AppBy { get; set; }                 // app_by
        public DateTime? AppDate { get; set; }          // app_date
        public string? emp_status { get; set; }
    }
}
