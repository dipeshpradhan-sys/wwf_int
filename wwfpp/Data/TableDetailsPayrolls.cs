using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace wwfpp.Data
{
    /***************PAYROLL ADMINISTRATION************************/
    //public DbSet<tbl_employee_advance> tbl_employee_advance { get; set; }
    public class tbl_employee_advance
    {
        [Key]
        public string adv_id {get;set;}  //[nvarchar](50) NOT NULL,
        public decimal? adv_personnel {get;set;}  //[money] NULL,
        public decimal? adv_program {get;set;}  //[money] NULL,
        public decimal? adv_travel {get;set;}  //[money] NULL,
        public decimal? adv_field_drawing {get;set;}  //[money] NULL,
        public decimal? adv_welfare {get;set;}  //[money] NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;} 
        public tbl_employee TblEmployee { get; set; }= null!;  //[int] NULL,

        public short? adv_year {get;set;}  //[smallint] NULL,
        public short? adv_month {get;set;}  //[smallint] NULL,
        public decimal? adv_PF_loan {get;set;}  //[money] NULL,
        public decimal? adv_CIT_loan {get;set;}  //[money] NULL,
        public string? adv_fiscal_year {get;set;}  //[nvarchar](10) NULL,
        public byte? adv_emp_week {get;set;}  //[tinyint] NULL,
    }
    //public DbSet<tbl_employee_pf> tbl_employee_pf { get; set; }
    public class tbl_employee_pf
    {
        [Key]
        public int emp_pf_id {get;set;}  //[int] NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;}   //[int] NULL,
        public tbl_employee TblEmployee { get; set; }= null!; 

        public string? pf_group {get;set;}  //[nvarchar](1) NULL,
        public string? pf_type {get;set;}  //[nvarchar](1) NULL,
        public double? add_percent_amount {get;set;}  //[float] NULL,
        public double? ded_percent_amount {get;set;}  //[float] NULL,
    } 
    //public DbSet<tbl_employee_cit> tbl_employee_cit { get; set; }
    public class tbl_employee_cit
    {
        [Key]
        public int emp_cit_id {get;set;}  //[int] NOT NULL,
        public string? cit_type {get;set;}  //[nvarchar](1) NULL,
        public double? percent_amount {get;set;}  //[float] NULL,
        public string? remarks {get;set;}  //[ntext] NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;} //[int] NULL,
        public tbl_employee TblEmployee { get; set; }= null!;  
    }
    //public DbSet<tbl_salary_differential_month> tbl_salary_differential_month { get; set; }
    public class tbl_salary_differential_month
    {
        [Key]
        public string fiscal_year {get;set;}  //[nvarchar](10) NOT NULL,
        public short? sal_year {get;set;}  //[smallint] NULL,
        public byte? sal_month {get;set;}  //[tinyint] NULL,
    } 
    //public DbSet<tbl_salary_differential_week> tbl_salary_differential_week { get; set; }
    public class tbl_salary_differential_week
    {
        [Key]
        public string fiscal_year {get;set;}  //[nvarchar](20) NOT NULL,
        public string? timesheet_type {get;set;}  //[nvarchar](50) NULL,
        public short? emp_week {get;set;}  //[smallint] NULL
    }
    //public DbSet<tbl_employee_medical_reimburse> tbl_employee_medical_reimburse { get; set; }
    public class tbl_employee_medical_reimburse
    {
        [Key]
        public string id {get;set;}  //[varchar{50) NOT NULL,
        public string? fiscal_year {get;set;}  //[varchar{10) NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;}  //[int] NULL,
        public tbl_employee TblEmployee { get; set; }= null!; 

        public string? marital_status {get;set;}  //[nvarchar](1) NULL,
        public string? bill_no {get;set;}  //[nvarchar](20) NULL,
        public DateTime? bill_date {get;set;}  //[datetime] NULL,
        public double? self_amt {get;set;}  //[float] NULL,
        public double? spouse_amt {get;set;}  //[float] NULL,
        public double? other_dep_amt {get;set;}  //[float] NULL,
        public DateTime? submit_date {get;set;}  //[datetime] NULL,
        public string? remarks {get;set;}  //[varchar{250) NULL,
        public string? app_status {get;set;}  //[varchar{20) NULL,

        [ForeignKey(nameof(TblEmployeeAppBy))]
        public int? app_by {get;set;}  //[int] NULL,
        public tbl_employee TblEmployeeAppBy { get; set; }= null!; 
        public DateTime? app_date {get;set;}  //[datetime] NULL,
        public int? sal_month {get;set;}  //[int] NULL,
        public int? sal_year {get;set;}  //[int] NULL,
        public string? reim_type {get;set;}  //[nvarchar](50) NULL,
    }

    //SSF
    //public DbSet<tbl_employee_ssf_info> tbl_employee_ssf_info { get; set; }
    public class tbl_employee_ssf_info{
        [Key]
        public int id {get;set;}  //[int] NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;}   //[int] NULL,
        public tbl_employee TblEmployee { get; set; }= null!;

        public string? ssf_number {get;set;}  //[nvarchar](20) NULL,
        public double? add_percent {get;set;}  //[float] NULL,
        public double? ded_percent {get;set;}  //[float] NULL,
        public double? add_percent_amount {get;set;}  //[float] NULL,
        public double? ded_percent_amount {get;set;}  //[float] NULL,
        }
    //public DbSet<tbl_employee_swf_loan> tbl_employee_swf_loan { get; set; }
    public class tbl_employee_swf_loan
    {
        [Key]
        public string id { get; set; }  //[nvarchar](255) NOT NULL,
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  
        public string? start_month { get; set; }  //[nvarchar](2) NULL,
        public string? start_year { get; set; }  //[nvarchar](4) NULL,
        public decimal? amount { get; set; }  //[money] NULL,
        public decimal? int_amount { get; set; }  //[money] NULL,
        public int? no_of_installment { get; set; }  //[int] NULL,
        public string? status { get; set; }  //[nvarchar](1) NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
    }
    //public DbSet<tbl_employee_swf_loan_direct_settle> tbl_employee_swf_loan_direct_settle { get; set; }
    public class tbl_employee_swf_loan_direct_settle
    {
        [Key]
        public string id { get; set; }  //[nvarchar](255) NOT NULL,
        public decimal? amount { get; set; }  //[money] NULL,
        public DateTime? s_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](255) NULL,

        [ForeignKey(nameof(TblEmployeeSwfLoan))]
        public string? swf_loan_id { get; set; }  //[nvarchar](255) NULL,
        public tbl_employee_swf_loan TblEmployeeSwfLoan { get; set; } = null!;
    }

    //Welfare
    //public DbSet<tbl_employee_welfare_interest> tbl_employee_welfare_interest { get; set; }
    public class tbl_employee_welfare_interest
    {
        [Key] 
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }
        public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

        public short? wl_year { get; set; }  //[smallint] NULL,
        public short? wl_month { get; set; }  //[smallint] NULL,
        public double? wl_amount { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? wl_fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? wl_emp_week { get; set; }  //[tinyint] NULL,
    }
    //public DbSet<tbl_employee_welfare_paidout> tbl_employee_welfare_paidout { get; set; }
    public class tbl_employee_welfare_paidout
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public short? wl_year { get; set; }  //[smallint] NULL,
        public short? wl_month { get; set; }  //[smallint] NULL,
        public double? wl_amount { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? wl_fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? wl_emp_week { get; set; }  //[tinyint] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
    }

    //Salary
    //public DbSet<tbl_employee_salary> tbl_employee_salary { get; set; }
    public class tbl_employee_salary
    {
        [Key]
        public float salary_id { get; set; }  //[real] NOT NULL, [NOTE: change on database to int]

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }
        public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? pf_a { get; set; }  //[money] NULL,
        public decimal? children_edu_all { get; set; }  //[money] NULL,
        public decimal? performance_all { get; set; }  //[money] NULL,
        public decimal? remote_area_all { get; set; }  //[money] NULL,
        public decimal? others { get; set; }  //[money] NULL,
        public decimal? overtime { get; set; }  //[money] NULL,
        public decimal? pf_d { get; set; }  //[money] NULL,
        public decimal? incometax_d { get; set; }  //[money] NULL,
        public decimal? insurance_d { get; set; }  //[money] NULL,
        public decimal? cit_d { get; set; }  //[money] NULL,
        public decimal? betalibi_d { get; set; }  //[money] NULL,
        public string? is_dashain { get; set; }  //[nvarchar](1) NULL,
        public decimal? dashain_a { get; set; }  //[money] NULL,
        public decimal? tel_per_adv { get; set; }  //[money] NULL,
        public decimal? travel_prog_adv { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[nvarchar](100) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public int? submit_by { get; set; }  //[int] NULL,
        public string? percent_for_tax_add { get; set; }  //[nvarchar](1) NULL,
        public decimal? medical_deduction_on_tax { get; set; }  //[money] NULL,
        public decimal? welfare_fund { get; set; }  //[money] NULL,
        public decimal? remote_exem { get; set; }  //[money] NULL,
        public decimal? gratudi { get; set; }  //[money] NULL,
        public decimal? act_basic_salary { get; set; }  //[money] NULL,
        public decimal? act_pf_a { get; set; }  //[money] NULL,
        public decimal? act_remote_area_all { get; set; }  //[money] NULL,
        public decimal? act_pf_d { get; set; }  //[money] NULL,
        public decimal? a_cit_d { get; set; }  //[money] NULL,
        public string? cit_type { get; set; }  //[nvarchar](1) NULL,
        public double? cit_percent_amonnt { get; set; }  //[float] NULL,
        public decimal? marital_d { get; set; }  //[money] NULL,
        public decimal? yearly_salary { get; set; }  //[money] NULL,
        public decimal? yearly_tax { get; set; }  //[money] NULL,
        public decimal? monthly_salary { get; set; }  //[money] NULL,
        public decimal? month_amount { get; set; }  //[money] NULL,
        public decimal? pr_adv { get; set; }  //[money] NULL,
        public decimal? fd_adv { get; set; }  //[money] NULL,
        public decimal? wl_adv { get; set; }  //[money] NULL,
        public decimal? wl_per { get; set; }  //[money] NULL,
        public decimal? net_in_hand { get; set; }  //[money] NULL,
        public decimal? insurance { get; set; }  //[money] NULL,
        public decimal? first_taxable_amount { get; set; }  //[money] NULL,
        public double? initial_tax_percent { get; set; }  //[float] NULL,
        public double? first_tax_percent { get; set; }  //[float] NULL,
        public double? second_tax_percent { get; set; }  //[float] NULL,
        public decimal? pre_access_tax { get; set; }  //[money] NULL,
        public decimal? adv_PF_loan { get; set; }  //[money] NULL,
        public decimal? adv_CIT_loan { get; set; }  //[money] NULL,
        public decimal? d_3_amt { get; set; }  //[money] NULL,
        public double? d_3_p { get; set; }  //[float] NULL,
        public double? d_4_p { get; set; }  //[float] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public decimal? gratuity { get; set; }  //[money] NULL,
        public decimal? gratuity_ded { get; set; }  //[money] NULL,
        public decimal? medical_expense_reimburse_eligible { get; set; }  //[money] NULL,
        public decimal? medical_expense_reimburse_total { get; set; }  //[money] NULL,
        public decimal? leave_encash { get; set; }  //[money] NULL,
        public decimal? second_tax_amount { get; set; }  //[money] NULL,
        public double? gender_ded_per { get; set; }  //[float] NULL,
        public decimal? ssf { get; set; }  //[money] NULL,
        public decimal? ssf_ded { get; set; }  //[money] NULL,
        public decimal? insurance_d_nl { get; set; }  //[money] NULL,
        public decimal? fourth_tax_amount { get; set; }  //[money] NULL,
        public double? fifth_tax_percent { get; set; }  //[float] NULL,
        public decimal? annual_health_checkup_add { get; set; }  //[money] NULL,
        public decimal? annual_health_checkup_ded { get; set; }  //[money] NULL,

    }
    //public DbSet<tbl_employee_salary_a_field> tbl_employee_salary_a_field { get; set; }
    public class tbl_employee_salary_a_field
    {
        [Key]
        public string salary_id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  

        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public decimal? act_basic_salary { get; set; }  //[money] NULL,
        public decimal? act_pf_a { get; set; }  //[money] NULL,
        public decimal? act_pf_d { get; set; }  //[money] NULL,
        public decimal? a_cit_d { get; set; }  //[money] NULL,
        public decimal? act_remote_area_all { get; set; }  //[money] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? pf_a { get; set; }  //[money] NULL,
        public decimal? children_edu_all { get; set; }  //[money] NULL,
        public decimal? performance_all { get; set; }  //[money] NULL,
        public decimal? remote_area_all { get; set; }  //[money] NULL,
        public decimal? overtime { get; set; }  //[money] NULL,
        public decimal? dashain_a { get; set; }  //[money] NULL,
        public decimal? gratudi { get; set; }  //[money] NULL,
        public decimal? insurance { get; set; }  //[money] NULL,
        public decimal? others { get; set; }  //[money] NULL,
        public decimal? pf_d { get; set; }  //[money] NULL,
        public decimal? cit_d { get; set; }  //[money] NULL,
        public decimal? pre_access_tax { get; set; }  //[money] NULL,
        public decimal? incometax_d { get; set; }  //[money] NULL,
        public decimal? betalibi_d { get; set; }  //[money] NULL,
        public decimal? tel_per_adv { get; set; }  //[money] NULL,
        public decimal? travel_prog_adv { get; set; }  //[money] NULL,
        public decimal? pr_adv { get; set; }  //[money] NULL,
        public decimal? fd_adv { get; set; }  //[money] NULL,
        public decimal? welfare_fund { get; set; }  //[money] NULL,
        public decimal? adv_PF_loan { get; set; }  //[money] NULL,
        public decimal? adv_CIT_loan { get; set; }  //[money] NULL,
        public decimal? wl_adv { get; set; }  //[money] NULL,
        public decimal? net_in_hand { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[nvarchar](100) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblEmployeeSubmitBy))]
        public int? submit_by { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeSubmitBy { get; set; } = null!;

        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public decimal? gratuity { get; set; }  //[money] NULL,
        public decimal? gratuity_ded { get; set; }  //[money] NULL,
        public decimal? medical_expense_reimburse_total { get; set; }  //[money] NULL,
        public decimal? leave_encash { get; set; }  //[money] NULL,
        public decimal? ssf { get; set; }  //[money] NULL,
        public decimal? ssf_ded { get; set; }  //[money] NULL,
        public decimal? annual_health_checkup_add { get; set; }  //[money] NULL,
        public decimal? annual_health_checkup_ded { get; set; }  //[money] NULL,
    }
    //public DbSet<tbl_employee_salary_block> tbl_employee_salary_block { get; set; }
    public class tbl_employee_salary_block
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }
        public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
    }
    //public DbSet<tbl_employee_salary_diff> tbl_employee_salary_diff { get; set; }
    public class tbl_employee_salary_diff
    {
        [Key]
        public int id { get; set; }  //[int] IDENTITY(1,1) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }
        public tbl_employee TblEmployee { get; set; } = null!;  //[int] NOT NULL,

        public short emp_year { get; set; }  //[smallint] NOT NULL,
        public byte emp_month { get; set; }  //[tinyint] NOT NULL,
        public decimal basic_salary { get; set; }  //[money] NOT NULL,
        public decimal pf_a { get; set; }  //[money] NOT NULL,
        public decimal gratuity_a { get; set; }  //[money] NOT NULL,
        public decimal ssf_a { get; set; }  //[money] NOT NULL,
        public decimal pf_d { get; set; }  //[money] NOT NULL,
        public decimal gratuity_d { get; set; }  //[money] NOT NULL,
        public decimal ssf_d { get; set; }  //[money] NOT NULL,
        public string emp_code { get; set; }  //[nvarchar](6) NOT NULL,
        public string fiscal_year { get; set; } //[nvarchar] (10) NULL
    }
    //public DbSet<tbl_employee_salary_extra_settings> tbl_employee_salary_extra_settings { get; set; }
    public class tbl_employee_salary_extra_settings
    {
        [Key]
        public int emp_id { get; set; }//One to One
        public tbl_employee? TblEmployee { get; set; }  //[int] NOT NULL,
        public string? is_field_staff { get; set; }  //[nvarchar](1) NULL,
        public string? is_get_dashain { get; set; }  //[nvarchar](1) NULL,
        public double? welfare_con_percent { get; set; }  //[tinyint] NULL,
        public string? timesheet_acceptance { get; set; }  //[nvarchar](1) NULL,
        public string? is_field_salary { get; set; }  //[nvarchar](1) NULL,
        public string? staff_type { get; set; }  //[nvarchar](1) NULL,
        public string? get_leave_accrual { get; set; }  //[nvarchar](1) NULL,
        public string? get_gratuity_accrual { get; set; }  //[nvarchar](1) NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station? TblDutyStation { get; set; }  //[int] NULL,

        public int? emp_year { get; set; }  //[int] NULL,
        public int? emp_month { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public short? emp_week { get; set; }  //[smallint] NULL
    }
    //public DbSet<tbl_employee_salary_final> tbl_employee_salary_final { get; set; }
    public class tbl_employee_salary_final
    {
        [Key]
        public int final_salary_id { get; set; }  //[numeric{18, 0) NOT NULL,
        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  

        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public DateTime? sal_start_date { get; set; }  //[datetime] NULL,
        public DateTime? sal_end_date { get; set; }  //[datetime] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        [ForeignKey(nameof(TblEmployeeSubmitBy))]
        public int? submit_by { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeSubmitBy { get; set; } = null!;
        public decimal? act_basic_salary { get; set; }  //[money] NULL,
        public decimal? act_pf_a { get; set; }  //[money] NULL,
        public decimal? act_remote_area_all { get; set; }  //[money] NULL,
        public decimal? less_remote_exemption { get; set; }  //[money] NULL,
        public decimal? welfare_fund_contribution { get; set; }  //[money] NULL,
        public decimal? welfare_fund_interest { get; set; }  //[money] NULL,
        public decimal? welfare_fund_paid { get; set; }  //[money] NULL,
        public decimal? welfare_fund_payable { get; set; }  //[money] NULL,
        public decimal? less_adv_personal { get; set; }  //[money] NULL,
        public decimal? less_adv_program { get; set; }  //[money] NULL,
        public decimal? less_adv_travel { get; set; }  //[money] NULL,
        public decimal? less_adv_field { get; set; }  //[money] NULL,
        public decimal? less_loan_welfare { get; set; }  //[money] NULL,
        public decimal? less_loan_pf { get; set; }  //[money] NULL,
        public decimal? less_loan_cit { get; set; }  //[money] NULL,
        public string? leave_accrual_payble_included { get; set; }  //[varchar{1) NULL,
        public string? leave_accrual_payble_tax_percent { get; set; }  //[float] NULL,
        public decimal? leave_accrual_payble_tax_amount { get; set; }  //[money] NULL,
        public decimal? leave_accrual_payble_amount { get; set; }  //[money] NULL,
        public string? gratuity_accrual_payble_included { get; set; }  //[varchar{1) NULL,
        public double? gratuity_accrual_payble_tax_percent { get; set; }  //[float] NULL,
        public decimal? gratuity_accrual_payble_tax_amount { get; set; }  //[money] NULL,
        public decimal? gratuity_accrual_payble_amount { get; set; }  //[money] NULL,
        public string? percent_for_tax_add { get; set; }  //[nvarchar](1) NULL,
        public decimal? salary_paid { get; set; }  //[money] NULL,
        public decimal? salary_payable { get; set; }  //[money] NULL,
        public decimal? pf_a_paid { get; set; }  //[money] NULL,
        public decimal? pf_a_payable { get; set; }  //[money] NULL,
        public decimal? children_edu_all_paid { get; set; }  //[money] NULL,
        public decimal? children_edu_all_payable { get; set; }  //[money] NULL,
        public decimal? overtime_paid { get; set; }  //[money] NULL,
        public decimal? overtime_payable { get; set; }  //[money] NULL,
        public decimal? performance_all_paid { get; set; }  //[money] NULL,
        public decimal? performance_all_payable { get; set; }  //[money] NULL,
        public decimal? insurance_paid { get; set; }  //[money] NULL,
        public decimal? insurance_payable { get; set; }  //[money] NULL,
        public decimal? remote_area_all_paid { get; set; }  //[money] NULL,
        public decimal? remote_area_all_payable { get; set; }  //[money] NULL,
        public decimal? others_paid { get; set; }  //[money] NULL,
        public decimal? others_payable { get; set; }  //[money] NULL,
        public string? others_remarks { get; set; }  //[nvarchar](250) NULL,
        public decimal? dashain_a_paid { get; set; }  //[money] NULL,
        public decimal? dashain_a_payable { get; set; }  //[money] NULL,
        public string? is_dashain { get; set; }  //[nvarchar](1) NULL,
        public decimal? pf_d_deducted { get; set; }  //[money] NULL,
        public decimal? pf_d_deductable { get; set; }  //[money] NULL,
        public decimal? cit_d_deducted { get; set; }  //[money] NULL,
        public decimal? cit_d_deductable { get; set; }  //[money] NULL,
        public decimal? betalibi_d_deducted { get; set; }  //[money] NULL,
        public decimal? betalibi_d_deductable { get; set; }  //[money] NULL,
        public decimal? taxable_salary { get; set; }  //[money] NULL,
        public decimal? payable_salary { get; set; }  //[money] NULL,
        public decimal? gross_tax { get; set; }  //[money] NULL,
        public decimal? medical_d_on_tax_deducted { get; set; }  //[money] NULL,
        public decimal? medical_d_on_tax_deductable { get; set; }  //[money] NULL,
        public decimal? pre_access_tax { get; set; }  //[money] NULL,
        public decimal? less_tax_deducted { get; set; }  //[money] NULL,
        public decimal? less_tax_deductable { get; set; }  //[money] NULL,
        public decimal? net_payble_salary { get; set; }  //[money] NULL,
        public decimal? net_in_hand { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public decimal? marital_basic_d { get; set; }  //[money] NULL,
        public decimal? less_insurance_d { get; set; }  //[money] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
        public decimal? leave_encashment_paid { get; set; }  //[money] NULL,
        public decimal? leave_encashment_payble { get; set; }  //[money] NULL,
        public decimal? gratuity_paid { get; set; }  //[money] NULL,
        public decimal? gratuity_payble { get; set; }  //[money] NULL,
        public decimal? medical_reim_paid { get; set; }  //[money] NULL,
        public decimal? medical_reim_payble { get; set; }  //[money] NULL,
        public decimal? medical_eli_paid { get; set; }  //[money] NULL,
        public decimal? medical_eli_payble { get; set; }  //[money] NULL,
    }
    //public DbSet<tbl_employee_salary_previous> tbl_employee_salary_previous { get; set; }
    public class tbl_employee_salary_previous
    {
        [Key]
        public int sal_id { get; set; }  //[int] NOT NULL,
        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 
        public short? sal_month { get; set; }  //[smallint] NULL,
        public short? sal_year { get; set; }  //[smallint] NULL,
        public double? t_basic_salary { get; set; }  //[float] NULL,
        public double? t_pf { get; set; }  //[float] NULL,
        public double? t_allow { get; set; }  //[float] NULL,
        public double? t_raa { get; set; }  //[float] NULL,
        public double? t_lip_rem { get; set; }  //[float] NULL,
        public double? t_dashain { get; set; }  //[float] NULL,
        public double? t_betalabi { get; set; }  //[float] NULL,
        public double? t_pf_d { get; set; }  //[float] NULL,
        public double? t_cit_d { get; set; }  //[float] NULL,
        public double? t_tax_pre { get; set; }  //[float] NULL,
        public double? t_tax { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
    }
    //public DbSet<tbl_employee_salary_tax_percent> tbl_employee_salary_tax_percent { get; set; }
    public class tbl_employee_salary_tax_percent
    {
        //One to One    
        [Key]
        public int emp_id { get; set; } //[int] NULL,
        [ForeignKey(nameof(emp_id))]
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? percent_for_tax_add { get; set; }  //[nvarchar](1) NULL
    }

    //Dashain Allowance
    //public DbSet<tbl_employee_dashain_allowance> tbl_employee_dashain_allowance { get; set; }
    public class tbl_employee_dashain_allowance
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? sal_year { get; set; }  //[int] NULL,
        public int? sal_month { get; set; }  //[int] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_dashain_allowance_emp_wise> tbl_employee_dashain_allowance_emp_wise { get; set; }
    public class tbl_employee_dashain_allowance_emp_wise
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public decimal? dashain_amount { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public byte? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_dashain_allowance_emp_wise_final> tbl_employee_dashain_allowance_emp_wise_final { get; set; }
    public class tbl_employee_dashain_allowance_emp_wise_final
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public int? sal_year { get; set; }  //[int] NULL,
        public int? sal_month { get; set; }  //[int] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  

        public decimal? dashain_amount { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_dashain_allowance_fund_wise> tbl_employee_dashain_allowance_fund_wise { get; set; }
    public class tbl_employee_dashain_allowance_fund_wise
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; } //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblFundSource))] 
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get;set;} = null!;

        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_dashain_allowance_fund_wise_final> tbl_employee_dashain_allowance_fund_wise_final { get; set; }
    public class tbl_employee_dashain_allowance_fund_wise_final
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; } //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblFundSource))] 
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get;set;} = null!;

        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }

    //Gratuaty
    //public DbSet<tbl_employee_gratuity_accrual> tbl_employee_gratuity_accrual { get; set; }
    public class tbl_employee_gratuity_accrual
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,
        public DateTime? fy_end_date { get; set; }  //[datetime] NULL,
        public double? service_year { get; set; }  //[float] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? gratuity_encash { get; set; }  //[money] NULL,
        public decimal? pre_encash { get; set; }  //[money] NULL,
        public decimal? net_encash { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_gratuity_accrual_final> tbl_employee_gratuity_accrual_final { get; set; }
    public class tbl_employee_gratuity_accrual_final
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; } //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,
        public DateTime? fy_end_date { get; set; }  //[datetime] NULL,
        public double? service_year { get; set; }  //[float] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? gratuity_encash { get; set; }  //[money] NULL,
        public decimal? pre_encash { get; set; }  //[money] NULL,
        public decimal? net_encash { get; set; }  //[money] NULL,
        public decimal? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_gratuity_accrual_fund_wise> tbl_employee_gratuity_accrual_fund_wise { get; set; }
    public class tbl_employee_gratuity_accrual_fund_wise
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; } //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblFundSource))] 
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get;set;} = null!;

        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_gratuity_accrual_fund_wise_final> tbl_employee_gratuity_accrual_fund_wise_final { get; set; }
    public class tbl_employee_gratuity_accrual_fund_wise_final
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; }//[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblFundSource))] 
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get;set;} = null!;

        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_gratuity_info> tbl_employee_gratuity_info { get; set; }
    public class tbl_employee_gratuity_info
    {
        [Key]
        public int id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblEmployee))] 
        public int? emp_id { get; set; } //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; 

        public string? gr_number { get; set; }  //[nvarchar](20) NULL,
        public string? gr_group { get; set; }  //[nvarchar](1) NULL,
        public string? gr_type { get; set; }  //[nvarchar](1) NULL,
        public double? add_percent_amount { get; set; }  //[float] NULL,
        public double? ded_percent_amount { get; set; }  //[float] NULL,
        public double? opening_balance { get; set; }  //[float] NULL,
        public double? opening_interest { get; set; }  //[float] NULL,
    }
}
