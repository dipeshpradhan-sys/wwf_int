namespace wwfpp.Models.Attendance
{
    //for daily report
    public class ReportDailyAttendanceMainViewModel
    {
        public int m_emp_id { get; set; }
        public string? employee { get; set; } = string.Empty;
        public string in_out_date { get; set; } = string.Empty;
        public string? first_check_in { get; set; } = string.Empty;
        public string? last_check_out { get; set; } = string.Empty;
        public string? remarks { get; set; } = string.Empty;
        public string? narration { get; set; } = string.Empty;
        public string? late_to_office { get; set; } = string.Empty;
        public string? late_to_home { get; set; } = string.Empty;
        public string? status { get; set; } = string.Empty;
        public int RemarksOrder { get; set; }
        public List<ReportDailyAttendanceSubViewModel> SubDetails { get; set; } = new();
    }
    public class ReportDailyAttendanceSubViewModel
    {
        public int s_emp_id { get; set; }
        public string? in_out_date { get; set; } = string.Empty;
        public string? check_in { get; set; } = string.Empty;
        public string? check_out { get; set; } = string.Empty;
    }
}
