namespace wwfpp.Models.Personnel
{
    public class LeaveViewModel
    {
        public int id { get; set; }        //emp_leave_id
        public byte? leave_type_id { get; set; }
        public string? description { get; set; }
        public DateTime? submit_date { get; set; } = default!;
        public DateTime? leave_from_date { get; set; } = default!;
        public DateTime? leave_to_date { get; set; } = default!;
        public string? leave_desc { get; set; }
        public double? leave_in_hrs { get; set; }
        public double? leave_in_days { get; set; }

        public string? app_status { get; set; }
        public int? app_by { get; set; }
        public string? app_by_name { get; set; }
        public DateTime? app_date { get; set; } = default!;
        public string? app_remarks { get; set; }

        public int? emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public DateTime? can_submit_date { get; set; }
        public string? can_desc { get; set; }
        public int? can_by { get; set; }
        public string? can_by_name { get; set; }
        public DateTime? can_date { get; set; }
        public string? can_remarks { get; set; }
        public string? fiscal_year { get; set; }
        public string? fiscal_year_abb { get; set; }
        public DateTime? start_fiscal_date { get; set; }
        public DateTime? end_fiscal_date { get; set; }
        public double workingHoursDays { get; set; }
        public string? showBtnCan { get; set; }
    }
}
