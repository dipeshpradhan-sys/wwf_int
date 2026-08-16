namespace wwfpp.Models.Attendance
{
    public class ReportDailyAttendanceRangeHoursViewModel
    {
        public int EmpId { get; set; }
        public string Employee { get; set; } = string.Empty;
        public Dictionary<DateTime, (double LTO, string LTOF, double LTH, string LTHF)> DateHours { get; set; }
        public string LtoT { get; set; } = string.Empty;
        public string LthT { get; set; } = string.Empty;
    }

}
