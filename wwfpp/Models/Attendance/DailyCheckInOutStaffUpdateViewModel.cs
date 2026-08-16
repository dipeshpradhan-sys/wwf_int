using wwfpp.Models.Settings;

namespace wwfpp.Models.Attendance
{
    public class DailyCheckInOutStaffUpdateViewModel
    {
        public string main_id { get; set; }
        public int? emp_id { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? employee { get; set; }   // firstname midlename lastname (code)
        public string? emp_code { get; set; }
        public string? emp_status { get; set; }
        public string? employee_type { get; set; }
        public DateTime? in_out_date { get; set; }
        public string? office_in { get; set; }
        public string? office_in_at { get; set; }
        public string? check_in { get; set; }
        public string? check_out { get; set; }
        public string? duty_station_id { get; set; }
        public string? office_out_at { get; set; }
        public string? office_out { get; set; }
        public string? day_type { get; set; }
        public string? remarks { get; set; }
        public string? narration { get; set; }
        public string? status { get; set; }
        public int? RemarksOrder { get; set; }
        public string? reason { get; set; }
    }

    public class DailyCheckInOutStaffUpdateListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<DailyCheckInOutStaffUpdateViewModel> Fields { get; set; }
    }

    public class DailyCheckInOutStaffUpdateSubViewModel
    {
        public string sub_id { get; set; }
        public int? emp_id { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? emp_code { get; set; }
        public string? emp_status { get; set; }
        public string? employee_type { get; set; }
        public DateTime? in_out_date { get; set; }
        public string? check_in { get; set; }
        public string? check_out { get; set; }
        public string? duty_station_id { get; set; }
        public string? remarks { get; set; }
    }
    /*
    public class DailyCheckInOutStaffUpdateSubListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<DailyCheckInOutStaffUpdateSubViewModel> Fields { get; set; }
    }
    */
    public class AttendanceUpdateSubRequest
    {
        public string? id { get; set; }
        public string? mode { get; set; }
        public string? emp_id { get; set; }     //convert after getting value
        public string? employee_type { get; set; }
        public string? duty_station_id { get; set; } = string.Empty;
        public string? in_out_date { get; set; }    //convert after getting value
        public string? check_in { get; set; }
        public string? check_out { get; set; }
        public string? reason { get; set; }
    }

}
