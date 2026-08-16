namespace wwfpp.Models.Attendance
{
    public class EmployeeCheckInOutChangeLogViewModel
    {
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? old_value { get; set; }  //[varchar{200) NULL,
        public string? new_value { get; set; }  //[varchar{200) NULL,
        public int? by_emp_id { get; set; }  //[int] NULL,
        public DateTime? change_date { get; set; }  //[datetime] NULL,
        public string? change_on { get; set; }  //[varchar{5) NULL,
        public string? change_type { get; set; }  //[varchar{20) NULL,
        public string? reason { get; set; }  //[ntext] NULL,
    }

}
