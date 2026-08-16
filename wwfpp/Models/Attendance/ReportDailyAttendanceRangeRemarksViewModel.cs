namespace wwfpp.Models.Attendance
{
    public class ReportDailyAttendanceRangeRemarksViewModel
    {
        public int EmpId { get; set; }
        public string Employee { get; set; }
        public Dictionary<DateTime, (string Remarks, string Flag)> DateRemarks { get; set; }
    }
}
