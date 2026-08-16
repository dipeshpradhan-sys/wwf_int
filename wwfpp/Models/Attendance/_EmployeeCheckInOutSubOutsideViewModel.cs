namespace wwfpp.Models.Attendance
{
    public class EmployeeCheckInOutSubOutsideViewModel
    {
        public string? id { get; set; }  //[varchar{50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,
        public int? in_guard_user_id { get; set; }  //[int] NULL,
        public int? out_guard_user_id { get; set; }  //[int] NULL,
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public string? remarks { get; set; }  //[varchar{100) NULL,
    }

}
