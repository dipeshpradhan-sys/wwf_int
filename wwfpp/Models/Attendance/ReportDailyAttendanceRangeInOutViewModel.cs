namespace wwfpp.Models.Attendance
{
    public class ReportDailyAttendanceRangeInOutViewModel
    {
        public int EmpId { get; set; }
        public string Employee { get; set; } = string.Empty;
        public Dictionary<DateTime, (string In, string Out, string Remarks)> DateInOutRemarks { get; set; }
    }

}
