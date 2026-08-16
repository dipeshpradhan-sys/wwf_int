using System.ComponentModel.DataAnnotations;

namespace wwfpp.Data
{
    public class vwAttendanceDailyStaffUpdate
    {
        public string id { get; set; }
        public int emp_id { get; set; }
        public string firstname { get; set; }
        public string? middlename { get; set; }
        public string lastname { get; set; }
        public string emp_code { get; set; }
        public string emp_status { get; set; }
        public string employee_type { get; set; }
        public DateTime? in_out_date { get; set; }
        public string? office_in { get; set; }
        public string? office_in_at { get; set; }
        public string? check_in { get; set; }
        public string? check_out { get; set; }
        public string duty_station_id { get; set; }
        public string? office_out_at { get; set; }
        public string? office_out { get; set; }
        public string remarks { get; set; }
        public string day_type { get; set; }
        public string? narration { get; set; }
        public int RemarksOrder { get; set; }
    }
    public class vwAttendanceDailyStaffUpdateSub
    {
        public string id { get; set; }
        public int emp_id { get; set; }
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
    public class vwAttendanceDailyStaffUpdateChangeLog
    {
        public string id { get; set; }
        public int? emp_id { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? employee_type { get; set; }
        public DateTime? in_out_date { get; set; }
        public string? old_value { get; set; }
        public string? new_value { get; set; }
        public int? by_emp_id { get; set; }
        public DateTime? change_date { get; set; }
        public string? change_on { get; set; }
        public string? change_type { get; set; }
        public string? reason { get; set; }
    }

    public class que_timesheet_sub
    {
        public int emp_id { get; set; }
        public short emp_year { get; set; }
        public byte emp_month { get; set; }
        public byte emp_day { get; set; }
        public int fund_id { get; set; }
        public double? time_hours { get; set; }
        public double? overtime_hours { get; set; }
        public DateTime? submit_date { get; set; }
        public string is_active { get; set; }
        public int submit_counter { get; set; }
        public string fiscal_year { get; set; }
        public short emp_week { get; set; }
        public DateTime? fiscal { get; set; }
    }

    public class que_employee_salary_previous
    {
        [Key]
        public int sal_id { get; set; }
        public int emp_id { get; set; }
        public short sal_month { get; set; }
        public short sal_year { get; set; }
        public decimal? t_basic_salary { get; set; }
        public decimal? t_pf { get; set; }
        public decimal? t_allow { get; set; }
        public decimal? t_lip_rem { get; set; }
        public decimal? t_raa { get; set; }
        public decimal? t_dashain { get; set; }
        public decimal? t_pf_d { get; set; }
        public decimal? t_cit_d { get; set; }
        public decimal? t_betalabi { get; set; }
        public decimal? t_tax_pre { get; set; }
        public decimal? t_tax { get; set; }
        public string? remarks { get; set; }
        public string? fiscal_year { get; set; }
        public byte? emp_week { get; set; }
        public DateTime? fiscal { get; set; }
        public string? month_fiscal { get; set; }
    }
    public class que_swf_payback
    {
        public int? emp_id { get; set; }
        public short? sal_year { get; set; }
        public short? sal_month { get; set; }
        public decimal? loan { get; set; }
        public string? fiscal_year { get; set; }
        public byte? emp_week { get; set; }
        public DateTime fiscal { get; set; }
    }
    

}
