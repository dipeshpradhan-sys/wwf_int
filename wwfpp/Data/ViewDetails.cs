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
    public class vw_swf_payback
    {
        public int? emp_id { get; set; }
        public short? sal_year { get; set; }
        public short? sal_month { get; set; }
        public decimal? loan { get; set; }
        public string? fiscal_year { get; set; }
        public byte? emp_week { get; set; }
        public DateTime fiscal { get; set; }
    }
    public class vw_Employee
    {
        [Key]
        public int emp_id { get; set; }

        public string emp_code { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string firstname { get; set; } = string.Empty;
        public string middlename { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
        public string employeename { get; set; } = string.Empty;
        public string employeenameWithCode { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string address1 { get; set; } = string.Empty;
        public string address2 { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
        public string nationality { get; set; } = string.Empty;
        public string postalcode { get; set; } = string.Empty;
        public string phone1 { get; set; } = string.Empty;
        public string phone2 { get; set; } = string.Empty;
        public string mobile { get; set; } = string.Empty;
        public string e_mail { get; set; } = string.Empty;
        public string personal_email { get; set; } = string.Empty;
        public string citizenship_number { get; set; } = string.Empty;
        public string citizenship_copy { get; set; } = string.Empty;
        public string passport_number { get; set; } = string.Empty;
        public string passport_copy { get; set; } = string.Empty;
        public string marital_status { get; set; } = string.Empty;
        public int no_of_children { get; set; }
        public string dependent_details { get; set; } = string.Empty;
        public string blood_group { get; set; } = string.Empty;
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }
        public string employee_type { get; set; } = string.Empty;
        public string department { get; set; } = string.Empty;
        public string post { get; set; } = string.Empty;
        public decimal? salary { get; set; }
        public decimal? grade { get; set; }
        public decimal? child_edu_all { get; set; }
        public string account_no { get; set; } = string.Empty;
        public string pf_no { get; set; } = string.Empty;
        public string cit_no { get; set; } = string.Empty;
        public int? manager_id { get; set; }
        public string? emp_status { get; set; } = string.Empty;
        public DateTime? deactivated_date { get; set; }
        public string remarks { get; set; } = string.Empty;
        public DateTime? effective_date { get; set; }
        public DateTime? dob { get; set; }
        public decimal? remote_area_allow { get; set; }
        public string pan_no { get; set; } = string.Empty;
        public decimal? yearly_remote_exem { get; set; }
        public string marital_status_info { get; set; } = string.Empty;
        public string emp_pay_status { get; set; } = string.Empty;
        public string emp_level { get; set; } = string.Empty;
        public string job_family { get; set; } = string.Empty;
        public int line_manager_id { get; set; }
        public int alt_manager_id { get; set; }
        public int alt_line_manager_id { get; set; }
        public string ethnicity { get; set; } = string.Empty;
        public decimal? work_percent { get; set; }
        public string nin_no { get; set; } = string.Empty;
        public string pan_copy { get; set; } = string.Empty;
        public string nin_copy { get; set; } = string.Empty;
        public string employee_type_sub { get; set; } = string.Empty;
        public string immediateSupervisor { get; set; } = string.Empty;
        public string lineDirector { get; set; } = string.Empty;
        public string AltImmediateSupervisor { get; set; } = string.Empty;
        public string AltLineDirector { get; set; } = string.Empty;
        public string emp_photo { get; set; } = string.Empty;
        public string? employeeTypeWithSub { get; set; } = string.Empty;
        public string? username { get; set; } = string.Empty;
        public string? level_id { get; set; } = string.Empty;
        public int? user_id { get; set; }
        public string? is_active { get; set; } = string.Empty;
    }

    public class vw_EmployeeOvertime
    {
        [Key]
        public string OtReqId { get; set; }
        public int? EmpId { get; set; }
        public string? EmployeenameWithCode { get; set; }
        public string? EmployeeStatus { get; set; }
        public string? DayName { get; set; }
        public DateTime? OvertimeDate { get; set; }
        public DateTime? SubmitDate { get; set; }
        public double? TotalHours { get; set; }
        public string? RequestBy { get; set; }
        public string? Description { get; set; }
        public string? OvertimeStatus { get; set; }
        public string? OvertimePaidedStatus { get; set; }
        public string? app_status { get; set; }
        public DateTime? paid_date { get; set; }
    }


    public class vw_employee_leave_hash
    {
        [Key]
        public int emp_leave_id { get; set; }
        public string fiscal_year { get; set; }
        public byte? leave_type_id { get; set; }
        public DateTime? submit_date { get; set; }
        public DateTime? leave_from_date { get; set; }
        public DateTime? leave_to_date { get; set; }
        public string? leave_desc { get; set; }
        public string app_status { get; set; }
        public int? app_by { get; set; }
        public DateTime? app_date { get; set; }
        public int? emp_id { get; set; }
        public string? emp_status { get; set; }
        public double? leave_in_hrs { get; set; }
        public string app_remarks { get; set; }

        // Extra fields from joins
        public string employee_name { get; set; }
        public string leave_type_desc { get; set; }
    }

    public class vw_Employee_Medical_Insurance
    {
        [Key]
        public string id { get; set; }
        public string fiscal_year { get; set; }
        public int emp_id { get; set; }
        public string marital_status { get; set; }
        public string bill_no { get; set; }
        public DateTime? bill_date { get; set; }
        public double? self_amt { get; set; }
        public double? spouse_amt { get; set; }
        public double? other_dep_amt { get; set; }

        public double? total_amt { get; set; }
        public DateTime? submit_date { get; set; }
        public string remarks { get; set; }
        public string app_status { get; set; }
        public int? app_by { get; set; }
        public DateTime? app_date { get; set; }
        public int? sal_month { get; set; }
        public int? sal_year { get; set; }

        public string? period { get; set; }
        public string reim_type { get; set; }

        public string employeenameWithCode { get; set; }
        public string app_by_name { get; set; }
    }
    public class vw_employee_salary_extra_settings
    {
        [Key]
        public int emp_id { get; set; }   // FK to tbl_employee.emp_id
        [StringLength(1)]
        public string? is_field_staff { get; set; }   // never null (ISNULL -> '')
        [StringLength(1)]
        public string? is_get_dashain { get; set; }
        public double? welfare_con_percent { get; set; }   // ISNULL -> 0
        [StringLength(1)]
        public string? timesheet_acceptance { get; set; }
        [StringLength(1)]
        public string? is_field_salary { get; set; }
        [StringLength(1)]
        public string? staff_type { get; set; }
        [StringLength(1)]
        public string? get_leave_accrual { get; set; }
        [StringLength(1)]
        public string? get_gratuity_accrual { get; set; }
        public DateTime? gratuity_date { get; set; }   // still nullable, no ISNULL
        [StringLength(50)]
        public string? duty_station_id { get; set; }
        public int? emp_year { get; set; }   // ISNULL -> 0
        public int? emp_month { get; set; }
        [StringLength(10)]
        public string fiscal_year { get; set; }
        public short emp_week { get; set; }   // ISNULL -> 0
    }
    public class vw_employee_salary_previous
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

        public string remarks { get; set; }

        public DateTime? fiscal { get; set; }
        public string month_fiscal { get; set; }
    }
    public class que_year_salary
    {
        public float? salary_id { get; set; }
        public int? emp_id { get; set; }
        public short? sal_year { get; set; }
        public short? sal_month { get; set; }
        public decimal? basic_salary { get; set; }
        public decimal? grade { get; set; }
        public decimal? pf_a { get; set; }
        public decimal? children_edu_all { get; set; }
        public decimal? performance_all { get; set; }
        public decimal? remote_area_all { get; set; }
        public decimal? others { get; set; }
        public decimal? overtime { get; set; }
        public decimal? pf_d { get; set; }
        public decimal? incometax_d { get; set; }
        public decimal? insurance_d { get; set; }
        public decimal? cit_d { get; set; }
        public decimal? betalibi_d { get; set; }
        public string? is_dashain { get; set; }
        public decimal? dashain_a { get; set; }
        public decimal? tel_per_adv { get; set; }
        public decimal? travel_prog_adv { get; set; }
        public string? remarks { get; set; }
        public DateTime? submit_date { get; set; }
        public int? submit_by { get; set; }
        public string? percent_for_tax_add { get; set; }
        public decimal? medical_deduction_on_tax { get; set; }
        public decimal? welfare_fund { get; set; }
        public decimal? remote_exem { get; set; }
        public decimal? gratudi { get; set; }
        public decimal? act_basic_salary { get; set; }
        public decimal? act_pf_a { get; set; }
        public decimal? act_remote_area_all { get; set; }
        public decimal? act_pf_d { get; set; }
        public decimal? a_cit_d { get; set; }
        public string? cit_type { get; set; }
        public double? cit_percent_amonnt { get; set; }
        public decimal? marital_d { get; set; }
        public decimal? yearly_salary { get; set; }
        public decimal? yearly_tax { get; set; }
        public decimal? monthly_salary { get; set; }
        public decimal? month_amount { get; set; }
        public decimal? pr_adv { get; set; }
        public decimal? fd_adv { get; set; }
        public decimal? wl_adv { get; set; }
        public decimal? wl_per { get; set; }
        public decimal? net_in_hand { get; set; }
        public decimal? insurance { get; set; }
        public decimal? first_taxable_amount { get; set; }
        public double? initial_tax_percent { get; set; }
        public double? first_tax_percent { get; set; }
        public double? second_tax_percent { get; set; }
        public decimal? pre_access_tax { get; set; }
        public decimal? adv_PF_loan { get; set; }
        public decimal? adv_CIT_loan { get; set; }
        public decimal? d_3_amt { get; set; }
        public double? d_3_p { get; set; }
        public double? d_4_p { get; set; }
        public string ?fiscal_year { get; set; }
        public byte ?emp_week { get; set; }
        public decimal? gratuity { get; set; }
        public decimal? gratuity_ded { get; set; }
        public decimal? medical_expense_reimburse_eligible { get; set; }
        public decimal? medical_expense_reimburse_total { get; set; }
        public decimal? leave_encash { get; set; }
        public decimal? second_tax_amount { get; set; }
        public double? gender_ded_per { get; set; }
        public decimal? ssf { get; set; }
        public decimal? ssf_ded { get; set; }
        public decimal? insurance_d_nl { get; set; }
        public decimal? fourth_tax_amount { get; set; }
        public double? fifth_tax_percent { get; set; }

        public string ?fullname { get; set; }
        public string ?employee_type { get; set; }
        public string ?post { get; set; }
        public DateTime? fiscal { get; set; }
        public string ?actual_fiscal { get; set; }
    }
    public class vw_timesheet_sub
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

    public class que_year_salary_a_field
    {
        public string? salary_id { get; set; }
        public int? emp_id { get; set; }
        public short? sal_year { get; set; }
        public short? sal_month { get; set; }
        public decimal? act_basic_salary { get; set; }
        public decimal? act_pf_a { get; set; }
        public decimal? act_pf_d { get; set; }
        public decimal? a_cit_d { get; set; }
        public decimal? act_remote_area_all { get; set; }
        public decimal? basic_salary { get; set; }
        public decimal? grade { get; set; }
        public decimal? pf_a { get; set; }
        public decimal? children_edu_all { get; set; }
        public decimal? performance_all { get; set; }
        public decimal? remote_area_all { get; set; }
        public decimal? overtime { get; set; }
        public decimal? dashain_a { get; set; }
        public decimal? gratudi { get; set; }
        public decimal? insurance { get; set; }
        public decimal? others { get; set; }
        public decimal? pf_d { get; set; }
        public decimal? cit_d { get; set; }
        public decimal? pre_access_tax { get; set; }
        public decimal? incometax_d { get; set; }
        public decimal? betalibi_d { get; set; }
        public decimal? tel_per_adv { get; set; }
        public decimal? travel_prog_adv { get; set; }
        public decimal? pr_adv { get; set; }
        public decimal? fd_adv { get; set; }
        public decimal? welfare_fund { get; set; }
        public decimal? adv_PF_loan { get; set; }
        public decimal? adv_CIT_loan { get; set; }
        public decimal? wl_adv { get; set; }
        public decimal? net_in_hand { get; set; }
        public string? remarks { get; set; }
        public DateTime? submit_date { get; set; }
        public int? submit_by { get; set; }
        public string? fiscal_year { get; set; }
        public byte? emp_week { get; set; }
        public decimal? gratuity { get; set; }
        public decimal? gratuity_ded { get; set; }
        public decimal? medical_expense_reimburse_total { get; set; }
        public decimal? leave_encash { get; set; }
        public decimal? ssf { get; set; }
        public decimal? ssf_ded { get; set; }
        public decimal? annual_health_checkup_add { get; set; }
        public decimal? annual_health_checkup_ded { get; set; }

        // Extra columns from View
        public string? fullname { get; set; }
        public DateTime? fiscal { get; set; }
        public string ?employee_type { get; set; }
        public string ?post { get; set; }
        public string ?actual_fiscal { get; set; }
    }

    public class que_year_salary_sum_fiscalwise
    {
        public int? emp_id { get; set; }
        public string? fullname { get; set; }
        public string ?actual_fiscal { get; set; }
        public string ?fiscal_year { get; set; }

        public decimal? basic_salary { get; set; }
        public decimal? grade { get; set; }
        public decimal? pf_a { get; set; }
        public decimal? children_edu_all { get; set; }
        public decimal? performance_all { get; set; }
        public decimal? remote_area_all { get; set; }
        public decimal? others { get; set; }
        public decimal? overtime { get; set; }
        public decimal? pf_d { get; set; }
        public decimal? incometax_d { get; set; }
        public decimal? cit_d { get; set; }
        public decimal? betalibi_d { get; set; }
        public decimal? dashain_a { get; set; }
        public decimal? tel_per_adv { get; set; }
        public decimal? travel_prog_adv { get; set; }
        public decimal? welfare_fund { get; set; }
        public decimal? gratudi { get; set; }
        public decimal? pr_adv { get; set; }
        public decimal? fd_adv { get; set; }
        public decimal? wl_adv { get; set; }
        public decimal? net_in_hand { get; set; }
        public decimal? insurance { get; set; }
        public decimal? pre_access_tax { get; set; }
        public decimal? adv_PF_loan { get; set; }
        public decimal? adv_CIT_loan { get; set; }
        public decimal? gratuity { get; set; }
        public decimal? gratuity_ded { get; set; }
        public decimal? medical_expense_reimburse_total { get; set; }
        public decimal? leave_encash { get; set; }
        public decimal? ssf { get; set; }
        public decimal? ssf_ded { get; set; }
    }

    public class que_year_salary_sum_fiscalwise_all
    {
        public int? emp_id { get; set; }
        public string? fullname { get; set; }
        public string ?actual_fiscal { get; set; }
        public string ?fiscal_year { get; set; }

        public decimal? basic_salary { get; set; }
        public decimal? grade { get; set; }
        public decimal? pf_a { get; set; }
        public decimal? children_edu_all { get; set; }
        public decimal? performance_all { get; set; }
        public decimal? remote_area_all { get; set; }
        public decimal? others { get; set; }
        public decimal? overtime { get; set; }
        public decimal? pf_d { get; set; }
        public decimal? incometax_d { get; set; }
        public decimal? cit_d { get; set; }
        public decimal? betalibi_d { get; set; }
        public decimal? dashain_a { get; set; }
        public decimal? tel_per_adv { get; set; }
        public decimal? travel_prog_adv { get; set; }
        public decimal? welfare_fund { get; set; }
        public decimal? gratudi { get; set; }
        public decimal? pr_adv { get; set; }
        public decimal? fd_adv { get; set; }
        public decimal? wl_adv { get; set; }
        public decimal? net_in_hand { get; set; }
        public decimal? insurance { get; set; }
        public decimal? pre_access_tax { get; set; }
        public decimal? adv_PF_loan { get; set; }
        public decimal? adv_CIT_loan { get; set; }
        public decimal? gratuity { get; set; }
        public decimal? gratuity_ded { get; set; }
        public decimal? medical_expense_reimburse_total { get; set; }
        public decimal? leave_encash { get; set; }
        public decimal? ssf { get; set; }
        public decimal? ssf_ded { get; set; }
    }

    public class que_year_salary_custom
    {
        public int emp_id { get; set; }
        public string fullname { get; set; }
        public string employee_type { get; set; }
        public DateTime? fiscal { get; set; }          // depends on actual type in que_year_salary
        public decimal? pf_d { get; set; }
        public decimal? cit_d { get; set; }
        public decimal? incometax_d { get; set; }
        public decimal? welfare_fund { get; set; }
        public decimal? adv_pf_loan { get; set; }
        public decimal? adv_cit_loan { get; set; }
        public decimal? wl_adv { get; set; }
        public decimal? basic_salary { get; set; }
        public string grade { get; set; }
        public decimal? pf_a { get; set; }
        public decimal? children_edu_all { get; set; }
        public decimal? insurance { get; set; }
        public decimal? performance_all { get; set; }
        public decimal? remote_area_all { get; set; }
        public decimal? others { get; set; }
        public decimal? overtime { get; set; }
        public decimal? betalibi_d { get; set; }
        public decimal? dashain_a { get; set; }
        public decimal? tel_per_adv { get; set; }
        public decimal? pr_adv { get; set; }
        public decimal? travel_prog_adv { get; set; }
        public decimal? fd_adv { get; set; }
        public string actual_fiscal { get; set; }
        public string fiscal_year { get; set; }
        public int? emp_week { get; set; }
        public decimal? gratuity { get; set; }
        public decimal? gratuity_ded { get; set; }
        public decimal? medical_expense_reimburse_total { get; set; }
        public decimal? leave_encash { get; set; }
        public decimal? ssf { get; set; }
        public decimal? ssf_ded { get; set; }
        public decimal? annual_health_checkup_add { get; set; }
        public decimal? annual_health_checkup_ded { get; set; }
    }


}
