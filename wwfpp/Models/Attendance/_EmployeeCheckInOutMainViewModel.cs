namespace wwfpp.Models.Attendance
{
    public class EmployeeCheckInOutMainViewModel
    {
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? office_in { get; set; }  //[varchar{20) NOT NULL,
        public string? office_in_at { get; set; }  //[varchar{20) NOT NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,
        public string? office_out_at { get; set; }  //[varchar{20) NULL,
        public string? office_out { get; set; }  //[varchar{20) NULL,
        public string? remarks { get; set; }  //[varchar{100) NULL,
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public string? day_type { get; set; }  //[varchar{1) NULL,
        public string? narration { get; set; }  //[nvarchar](550) NULL,
    }

}
