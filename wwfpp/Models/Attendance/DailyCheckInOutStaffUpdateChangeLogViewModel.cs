namespace wwfpp.Models.Attendance
{
    public class DailyCheckInOutStaffUpdateChangeLogViewModel
    {
        public string log_id { get; set; }  //id = pk
        public int? emp_id { get; set; }
        public DateTime? in_out_date { get; set; }  //composit : emp_id && in_out_date 
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string lastname { get; set; }
        public string? employee_type { get; set; }
        public string? old_value { get; set; }
        public string? new_value { get; set; }
        public int? by_emp_id { get; set; }
        public DateTime? change_date { get; set; }
        public string? change_on { get; set; }
        public string? change_type { get; set; }
        public string? reason { get; set; }

    }

}
