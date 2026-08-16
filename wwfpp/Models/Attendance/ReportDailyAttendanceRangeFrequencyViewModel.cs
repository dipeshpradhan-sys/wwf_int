namespace wwfpp.Models.Attendance
{
    public class ReportDailyAttendanceRangeFrequencyViewModel
    {
        public int EmpId { get; set; }
        public string Employee { get; set; } = string.Empty;
        public Dictionary<DateTime, (string checkIn, int LTO, int LTH)> DateFrequency { get; set; }
        public int LtoT { get; set; }
        public int LthT { get; set; }
    }
}
