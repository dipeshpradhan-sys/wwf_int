namespace wwfpp.Models
{
    public class LeaveAppVM
    {
        public int AppId { get; set; }
        public int EmpId { get; set; }
        public int LeaveTypeID { get; set; }
        public string? LeaveType { get; set; }
        public DateTime? SubmitDate { get; set; }
        public DateTime? StartDate { get; set; }
        public string? LeaveInHours { get; set; }
        public int ToEmpId { get; set; }
        public int ToId { get; set; }

        public string? FiscalYear { get; set; }

    }

}
