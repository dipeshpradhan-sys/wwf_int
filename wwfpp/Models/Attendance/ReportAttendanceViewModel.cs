namespace wwfpp.Models.Attendance
{
    public class ReportAttendanceViewModel
    {
        public string? report_mode { get; set; }
        public string? report_name { get; set; }
        public string? duty_station_id { get; set; }
        public string? employee_type { get; set; }
        public string? in_out_date { get; set; }
        public string? report_type { get; set; }
        public string? emp_status { get; set; }
        public string? start_date { get; set; }
        public string? end_date { get; set; }
        public string? absent_remark_lto { get; set; }
        public int? emp_id { get; set; }
    }
}
