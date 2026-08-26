using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics.Contracts;
using wwfpp.Models;
using wwfpp.Models.Account;
using wwfpp.Models.Attendance;

namespace wwfpp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        //Application Administration
        public DbSet<tbl_pp_options> tbl_pp_options { get; set; }
        public DbSet<tbl_email_list> tbl_email_list { get; set; }
        public DbSet<tbl_email_list_attachment> tbl_email_list_attachment { get; set; }
        public DbSet<tbl_email_list_sub> tbl_email_list_sub { get; set; }

        //General Administration
        public DbSet<tbl_contract_document_template> tbl_contract_document_template { get; set; }
        public DbSet<tbl_document_templates> tbl_document_templates { get; set; }
        public DbSet<tbl_fund_source> tbl_fund_source { get; set; }
        public DbSet<tbl_duty_station> tbl_duty_station { get; set; }
        public DbSet<tbl_conflict_fraud_format> tbl_conflict_fraud_format { get; set; }
        public DbSet<tbl_conflict_sign> tbl_conflict_sign { get; set; }
        public DbSet<tbl_conflict_sign_sub> tbl_conflict_sign_sub { get; set; }
        public DbSet<tbl_education_level> tbl_education_level { get; set; }
        public DbSet<tbl_expenditure_category> tbl_expenditure_category { get; set; }
        public DbSet<tbl_expenditure_type> tbl_expenditure_type { get; set; }
        public DbSet<tbl_document_type> tbl_document_type { get; set; }
        public DbSet<tbl_currency> tbl_currency { get; set; }
        public DbSet<tbl_alert_execute_date> tbl_alert_execute_date { get; set; }
        public DbSet<tbl_task> tbl_task { get; set; }
        public DbSet<tbl_fraud_corruption_sign> tbl_fraud_corruption_sign { get; set; }

        //Employee Administration
        public DbSet<tbl_employee> tbl_employee { get; set; }
        public DbSet<tbl_employee_photo> tbl_employee_photo { get; set; }
        public DbSet<tbl_employee_contract> tbl_employee_contract { get; set; }
        public DbSet<tbl_employee_signed_contract> tbl_employee_signed_contract { get; set; }
        public DbSet<tbl_employee_address> tbl_employee_address { get; set; }
        public DbSet<tbl_employee_document> tbl_employee_document { get; set; }
        public DbSet<tbl_employee_education> tbl_employee_education { get; set; }
        public DbSet<tbl_employee_fund_source> tbl_employee_fund_source { get; set; }
        public DbSet<tbl_employee_fund_source_hash> tbl_employee_fund_source_hash { get; set; }
        public DbSet<tbl_employee_history> tbl_employee_history { get; set; }
        public DbSet<tbl_employee_insurance> tbl_employee_insurance { get; set; }
        public DbSet<tbl_employee_outside> tbl_employee_outside { get; set; }
        public DbSet<tbl_employee_signature> tbl_employee_signature { get; set; }
        public DbSet<GetEmployeeFundSourceDetail> GetEmployeeFundSourceDetail { get; set; }

        //Dependent
        public DbSet<tbl_employee_dependent_children_details> tbl_employee_dependent_children_details { get; set; }
        public DbSet<tbl_employee_dependent_children_details_sub> tbl_employee_dependent_children_details_sub { get; set; }
        public DbSet<tbl_dependent_children_details_allowance> tbl_dependent_children_details_allowance { get; set; }
        public DbSet<tbl_dependent_children_details_allowance_emp_wise> tbl_dependent_children_details_allowance_emp_wise { get; set; }
        public DbSet<tbl_dependent_children_details_allowance_fund_wise> tbl_dependent_children_details_allowance_fund_wise { get; set; }
        public DbSet<tbl_employee_dependent_children_details_allowance_final> tbl_employee_dependent_children_details_allowance_final { get; set; }


        //Request Administration
        public DbSet<tbl_employee_dayoff> tbl_employee_dayoff { get; set; }

        //Leave
        public DbSet<tbl_leave_heading> tbl_leave_heading { get; set; }
        public DbSet<tbl_employee_leave> tbl_employee_leave { get; set; }
        public DbSet<tbl_employee_excess_leave_encash_emp_wise> tbl_employee_excess_leave_encash_emp_wise { get; set; }
        public DbSet<tbl_employee_excess_leave_encash_fund_wise> tbl_employee_excess_leave_encash_fund_wise { get; set; }
        public DbSet<tbl_employee_leave_accrual> tbl_employee_leave_accrual { get; set; }
        public DbSet<tbl_employee_leave_accrual_final> tbl_employee_leave_accrual_final { get; set; }
        public DbSet<tbl_employee_leave_accrual_fund_wise> tbl_employee_leave_accrual_fund_wise { get; set; }
        public DbSet<tbl_employee_leave_accrual_fund_wise_final> tbl_employee_leave_accrual_fund_wise_final { get; set; }
        public DbSet<tbl_employee_leave_accrual_new> tbl_employee_leave_accrual_new { get; set; }
        public DbSet<tbl_employee_leave_accrual_new_fund_wise> tbl_employee_leave_accrual_new_fund_wise { get; set; }
        public DbSet<tbl_employee_leave_forward> tbl_employee_leave_forward { get; set; }
        public DbSet<tbl_employee_leave_hash> tbl_employee_leave_hash { get; set; }
        public DbSet<tbl_employee_leave_indv> tbl_employee_leave_indv { get; set; }
        public DbSet<tbl_employee_leave_indv_cafw_paid_laps> tbl_employee_leave_indv_cafw_paid_laps { get; set; }
        public DbSet<tbl_employee_leave_indv_paid_cleared> tbl_employee_leave_indv_paid_cleared { get; set; }
        public DbSet<tbl_employee_leave_indv_paid_cleared_new> tbl_employee_leave_indv_paid_cleared_new { get; set; }
        public DbSet<tbl_yearly_annual_leave_cf> tbl_yearly_annual_leave_cf { get; set; }
        public DbSet<tbl_yearly_sick_leave_cf> tbl_yearly_sick_leave_cf { get; set; }

        //Timesheet
        public DbSet<tbl_employee_timesheet_app> tbl_employee_timesheet_app { get; set; }
        public DbSet<tbl_employee_timesheet_edited> tbl_employee_timesheet_edited { get; set; }
        public DbSet<tbl_employee_timesheet_main> tbl_employee_timesheet_main { get; set; }
        public DbSet<tbl_employee_timesheet_sub> tbl_employee_timesheet_sub { get; set; }
        public DbSet<tbl_employee_timesheet_sub_hash> tbl_employee_timesheet_sub_hash { get; set; }

        //Travel
        public DbSet<tbl_travel_particulars> tbl_travel_particulars { get; set; }
        public DbSet<tbl_employee_administrator> tbl_employee_administrator { get; set; }
        public DbSet<tbl_employee_travel_codes> tbl_employee_travel_codes { get; set; }
        public DbSet<tbl_employee_travel_main> tbl_employee_travel_main { get; set; }
        public DbSet<tbl_employee_travel_printed> tbl_employee_travel_printed { get; set; }
        public DbSet<tbl_employee_travel_settlement_main> tbl_employee_travel_settlement_main { get; set; }
        public DbSet<tbl_employee_travel_settlement_sub> tbl_employee_travel_settlement_sub { get; set; }
        public DbSet<tbl_employee_travel_settlement_sub_doc> tbl_employee_travel_settlement_sub_doc { get; set; }
        public DbSet<tbl_employee_travel_sub> tbl_employee_travel_sub { get; set; }

        //Overtime
        public DbSet<tbl_employee_overtime> tbl_employee_overtime { get; set; }
        public DbSet<tbl_employee_overtime_final> tbl_employee_overtime_final { get; set; }
        public DbSet<tbl_employee_overtime_request> tbl_employee_overtime_request { get; set; }
        public DbSet<tbl_employee_overtime_request_sub> tbl_employee_overtime_request_sub { get; set; }
        public DbSet<tbl_employee_overtime_settings> tbl_employee_overtime_settings { get; set; }

        //Payroll Administration
        public DbSet<tbl_employee_advance> tbl_employee_advance { get; set; }
        public DbSet<tbl_employee_pf> tbl_employee_pf { get; set; }
        public DbSet<tbl_employee_cit> tbl_employee_cit { get; set; }
        public DbSet<tbl_salary_differential_month> tbl_salary_differential_month { get; set; }
        public DbSet<tbl_salary_differential_week> tbl_salary_differential_week { get; set; }
        public DbSet<tbl_employee_medical_reimburse> tbl_employee_medical_reimburse { get; set; }

        //SSF
        public DbSet<tbl_employee_ssf_info> tbl_employee_ssf_info { get; set; }
        public DbSet<tbl_employee_swf_loan> tbl_employee_swf_loan { get; set; }
        public DbSet<tbl_employee_swf_loan_direct_settle> tbl_employee_swf_loan_direct_settle { get; set; }

        //Welfare
        public DbSet<tbl_employee_welfare_interest> tbl_employee_welfare_interest { get; set; }
        public DbSet<tbl_employee_welfare_paidout> tbl_employee_welfare_paidout { get; set; }

        //Salary
        public DbSet<tbl_employee_salary> tbl_employee_salary { get; set; }
        public DbSet<tbl_employee_salary_a_field> tbl_employee_salary_a_field { get; set; }
        public DbSet<tbl_employee_salary_block> tbl_employee_salary_block { get; set; }
        public DbSet<tbl_employee_salary_diff> tbl_employee_salary_diff { get; set; }
        public DbSet<tbl_employee_salary_extra_settings> tbl_employee_salary_extra_settings { get; set; }
        public DbSet<tbl_employee_salary_final> tbl_employee_salary_final { get; set; }
        public DbSet<tbl_employee_salary_previous> tbl_employee_salary_previous { get; set; }
        public DbSet<tbl_employee_salary_tax_percent> tbl_employee_salary_tax_percent { get; set; }

        //Dashain Allowance
        public DbSet<tbl_employee_dashain_allowance> tbl_employee_dashain_allowance { get; set; }
        public DbSet<tbl_employee_dashain_allowance_emp_wise> tbl_employee_dashain_allowance_emp_wise { get; set; }
        public DbSet<tbl_employee_dashain_allowance_emp_wise_final> tbl_employee_dashain_allowance_emp_wise_final { get; set; }
        public DbSet<tbl_employee_dashain_allowance_fund_wise> tbl_employee_dashain_allowance_fund_wise { get; set; }
        public DbSet<tbl_employee_dashain_allowance_fund_wise_final> tbl_employee_dashain_allowance_fund_wise_final { get; set; }

        //Gratuaty
        public DbSet<tbl_employee_gratuity_accrual> tbl_employee_gratuity_accrual { get; set; }
        public DbSet<tbl_employee_gratuity_accrual_final> tbl_employee_gratuity_accrual_final { get; set; }
        public DbSet<tbl_employee_gratuity_accrual_fund_wise> tbl_employee_gratuity_accrual_fund_wise { get; set; }
        public DbSet<tbl_employee_gratuity_accrual_fund_wise_final> tbl_employee_gratuity_accrual_fund_wise_final { get; set; }
        public DbSet<tbl_employee_gratuity_info> tbl_employee_gratuity_info { get; set; }



        //Attendance
        public DbSet<tbl_employee_check_in_out_change_log> tbl_employee_check_in_out_change_log { get; set; }
        public DbSet<tbl_employee_check_in_out_change_log_outside> tbl_employee_check_in_out_change_log_outside { get; set; }
        public DbSet<tbl_employee_check_in_out_main> tbl_employee_check_in_out_main { get; set; }
        public DbSet<tbl_employee_check_in_out_main_outside> tbl_employee_check_in_out_main_outside { get; set; }
        public DbSet<tbl_employee_check_in_out_setting> tbl_employee_check_in_out_setting { get; set; }
        public DbSet<tbl_employee_check_in_out_staff_update> tbl_employee_check_in_out_staff_update { get; set; }
        public DbSet<tbl_employee_check_in_out_sub> tbl_employee_check_in_out_sub { get; set; }
        public DbSet<tbl_employee_check_in_out_sub_outside> tbl_employee_check_in_out_sub_outside { get; set; }
        /** Views */
        public DbSet<vwAttendanceDailyStaffUpdate> vwAttendanceDailyStaffUpdate { get; set; }
        public DbSet<vwAttendanceDailyStaffUpdateSub> vwAttendanceDailyStaffUpdateSub { get; set; }
        public DbSet<vwAttendanceDailyStaffUpdateChangeLog> vwAttendanceDailyStaffUpdateChangeLog { get; set; }
        public DbSet<vw_Employee> vw_Employee { get; set; } = null!;
        public DbSet<vw_EmployeeOvertime> vw_EmployeeOvertime { get; set; } = null!;
        public DbSet<vw_employee_leave_hash> vw_employee_leave_hash { get; set; } = null!;
        public DbSet<vw_Employee_Medical_Insurance> vw_Employee_Medical_Insurance { get; set; } = null!;
        public DbSet<vw_employee_salary_extra_settings> vw_employee_salary_extra_settings { get; set; } = null!;
        public DbSet<vw_employee_salary_previous> vw_employee_salary_previous { get; set; } = null!;
        public DbSet<vw_year_salary> vw_year_salary { get; set; } = null!;
        public DbSet<vw_timesheet_sub> vw_timesheet_sub { get; set; } = null!;


        //User Administration
        public DbSet<tbl_user_module> tbl_user_module { get; set; }
        public DbSet<tbl_user_menu> tbl_user_menu { get; set; }
        public DbSet<tbl_user_level> tbl_user_level { get; set; }
        public DbSet<tbl_user_level_module> tbl_user_level_module { get; set; }
        public DbSet<tbl_user_level_menu> tbl_user_level_menu { get; set; }
        public DbSet<tbl_user> tbl_user { get; set; }
        public DbSet<tbl_user_user_module> tbl_user_user_module { get; set; }
        public DbSet<tbl_user_user_menu> tbl_user_user_menu { get; set; }
        public DbSet<tbl_user_login_fail> tbl_user_login_fail { get; set; }
        public DbSet<tbl_user_login_log> tbl_user_login_log { get; set; }
        public DbSet<tbl_user_pwd_history> tbl_user_pwd_history { get; set; }
        public DbSet<tbl_user_reset_token> tbl_user_reset_token { get; set; }
        public DbSet<tbl_user_guard> tbl_user_guard { get; set; }

        ////Settings / Maintenance
        public DbSet<tbl_fiscal_year> tbl_fiscal_year { get; set; }
        public DbSet<tbl_calendar_year> tbl_calendar_year { get; set; }
        public DbSet<tbl_setting_timesheet_type> tbl_setting_timesheet_type { get; set; }
        public DbSet<tbl_calendar_setting> tbl_calendar_setting { get; set; }
        public DbSet<tbl_calendar_setting_biweekly> tbl_calendar_setting_biweekly { get; set; }
        public DbSet<tbl_calendar_setting_weekly> tbl_calendar_setting_weekly { get; set; }

        public DbSet<tbl_general_setting> tbl_general_setting { get; set; }
        public DbSet<tbl_setting_holidays> tbl_setting_holidays { get; set; }
        public DbSet<tbl_setting_language> tbl_setting_language { get; set; }
        public DbSet<tbl_setting_limit_hrs> tbl_setting_limit_hrs { get; set; }
        public DbSet<tbl_setting_rate> tbl_setting_rate { get; set; }
        public DbSet<tbl_settings_gl_codes> tbl_settings_gl_codes { get; set; }
        public DbSet<tbl_tax_setting> tbl_tax_setting { get; set; }
        public DbSet<tbl_yearly_ins_amt> tbl_yearly_ins_amt { get; set; }
        public DbSet<tbl_yearly_working_hrs> tbl_yearly_working_hrs { get; set; }
        public DbSet<tbl_setting_dependent_children_details> tbl_setting_dependent_children_details { get; set; }
        public DbSet<tbl_setting_paycode_category> tbl_setting_paycode_category { get; set; }
        public DbSet<tbl_setting_paycode_sub_category> tbl_setting_paycode_sub_category { get; set; }


        //MISC
        public DbSet<tbl_owner> tbl_owner { get; set; }

        //Deprecated
        //public DbSet<tbl_employee_pension> tbl_employee_pension { get; set; }
        //public DbSet<tbl_pension_setting> tbl_pension_setting { get; set; }

        //public DbSet<tbl_forms> tbl_forms { get; set; }
        //public DbSet<tbl_user_forms> tbl_user_forms { get; set; }
        //public DbSet<tbl_user_level_access> tbl_user_level_access { get; set; }



        /*VIEWS*/

        //User Administration
        public DbSet<que_user_log> que_user_log { get; set; }

        // public DbSet<que_fiscal_year> que_fiscal_year	 { get; set; }
        // public DbSet<que_calendar_setting_biweekly> que_calendar_setting_biweekly	 { get; set; }
        // public DbSet<que_calendar_setting_weekly> que_calendar_setting_weekly	 { get; set; }

        // public DbSet<que_employee_dependent_details_status_count> que_employee_dependent_details_status_count	 { get; set; }

        // public DbSet<que_employee_funds> que_employee_funds	 { get; set; }


        // public DbSet<que_employee_leave_hash> que_employee_leave_hash	 { get; set; }

        // public DbSet<que_employee_timesheet_app> que_employee_timesheet_app	 { get; set; }
        public DbSet<que_timesheet_sub> que_timesheet_sub { get; set; }

        // public DbSet<que_employee_travel_settlement_main> que_employee_travel_settlement_main	 { get; set; }


        // public DbSet<que_employee_overtime_request> que_employee_overtime_request	 { get; set; }
        // public DbSet<que_overtime> que_overtime	 { get; set; }


        // public DbSet<que_cost_center> que_cost_center	 { get; set; }
        // public DbSet<que_cost_center_active_only> que_cost_center_active_only	 { get; set; }
        // public DbSet<que_cost_center_net_in_hand_join> que_cost_center_net_in_hand_join	 { get; set; }
        // public DbSet<que_cost_center_salary> que_cost_center_salary	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit> que_cost_center_salary_benifit	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit_ded_join> que_cost_center_salary_benifit_ded_join	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit_join> que_cost_center_salary_benifit_join	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit_overall> que_cost_center_salary_benifit_overall	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit_overall_join> que_cost_center_salary_benifit_overall_join	 { get; set; }
        // public DbSet<que_cost_center_salary_benifit_per_adv_join> que_cost_center_salary_benifit_per_adv_join	 { get; set; }
        // public DbSet<que_cost_center_time> que_cost_center_time	 { get; set; }
        // public DbSet<que_cost_center_time_cost> que_cost_center_time_cost	 { get; set; }
         public DbSet<que_employee_salary_previous> que_employee_salary_previous { get; set; }
        // public DbSet<que_employee_welfare_interest> que_employee_welfare_interest	 { get; set; }
        // public DbSet<que_employee_welfare_paidout> que_employee_welfare_paidout	 { get; set; }
        // public DbSet<que_salary> que_salary	 { get; set; }
         public DbSet<vw_swf_payback> vw_swf_payback { get; set; }
        // public DbSet<que_year_salary> que_year_salary	 { get; set; }
        // public DbSet<que_year_salary_a_field> que_year_salary_a_field	 { get; set; }
        // public DbSet<que_year_salary_custom> que_year_salary_custom	 { get; set; }
        // public DbSet<que_year_salary_sum_fiscalwise> que_year_salary_sum_fiscalwise	 { get; set; }
        // public DbSet<que_year_salary_sum_fiscalwise_all> que_year_salary_sum_fiscalwise_all	 { get; set; }


        //Hataoooo
        // public DbSet<que_setting_holidays> que_setting_holidays	 { get; set; }
        // public DbSet<que_setting_language> que_setting_language	 { get; set; }
        // public DbSet<que_setting_limit_hrs> que_setting_limit_hrs	 { get; set; }
        // public DbSet<que_setting_paycode_sub_category> que_setting_paycode_sub_category	 { get; set; }
        // public DbSet<que_setting_rate> que_setting_rate	 { get; set; }
        // public DbSet<que_setting_timesheet_type> que_setting_timesheet_type	 { get; set; }
        // public DbSet<que_salary_differential_week> que_salary_differential_week	 { get; set; }
        // public DbSet<que_salary_differential_month> que_salary_differential_month	 { get; set; }
        // public DbSet<vw_owner> vw_owner	 { get; set; }



        //public DbSet<vw_user_module_menu> vw_user_module_menu { get; set; }

        /* PROCEDURES */
        public DbSet<GetEmployeeTimesheetPivot> GetEmployeeTimesheetPivot { get; set; } = null!;
        public DbSet<GetEmployeeLeave> GetEmployeeLeave { get; set; } = null!;

        /*
         * Define the primery (if no primery key defined on single field) / Unique keys below for the table defined above 
         * Also for UNIQUE KEY GENERATOR FOR Tables/VIEWs IF NO PRIMARY KEY IS EXIST
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<tbl_pp_options>().ToTable("tbl_pp_options"); // matches actual DB table

            _ = modelBuilder.Entity<vwAttendanceDailyStaffUpdate>().HasNoKey().ToView("vw_AttendanceDailyStaffUpdate");
            _ = modelBuilder.Entity<vwAttendanceDailyStaffUpdateSub>().HasNoKey().ToView("vw_AttendanceDailyStaffUpdateSub");
            _ = modelBuilder.Entity<vwAttendanceDailyStaffUpdateChangeLog>().HasNoKey().ToView("vw_AttendanceDailyStaffUpdateChangeLog");

            //refer Related module's table for Keys definitions
            // Automatically applies all IEntityTypeConfiguration<T> classes in this assembly
            _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


            //List of tables that has no Primary Key defined
            _ = modelBuilder.Entity<tbl_employee_overtime_request_sub>().HasNoKey();
            //modelBuilder.Entity<tbl_employee_salary_extra_settings>().HasNoKey();//kept now
            _ = modelBuilder.Entity<tbl_yearly_annual_leave_cf>().HasNoKey();
            _ = modelBuilder.Entity<tbl_yearly_sick_leave_cf>().HasNoKey();
            _ = modelBuilder.Entity<tbl_yearly_ins_amt>().HasNoKey();
            _ = modelBuilder.Entity<tbl_yearly_working_hrs>().HasNoKey();

            _ = modelBuilder.Entity<tbl_employee_timesheet_main>().HasKey(e => new { e.emp_id, e.emp_year, e.emp_month, e.emp_day, e.submit_counter });
            _ = modelBuilder.Entity<tbl_employee_timesheet_sub>().HasKey(e => new { e.emp_id, e.emp_year, e.emp_month, e.emp_day, e.fund_id, e.submit_counter });
            _ = modelBuilder.Entity<tbl_employee_timesheet_edited>().HasKey(e => new { e.emp_id, e.emp_year, e.emp_month, e.fiscal_year, e.emp_week, e.submit_counter });
            _ = modelBuilder.Entity<tbl_employee_travel_sub>().HasKey(e => new { e.emp_travel_id, e.par_id });
            _ = modelBuilder.Entity<tbl_employee_travel_codes>().HasKey(e => new { e.emp_travel_id, e.sn});
            _ = modelBuilder.Entity<tbl_employee_travel_settlement_sub>().HasKey(e => new { e.trav_set_id, e.sn });

            // //for reletionship between tables (One-to-many)
            _ = modelBuilder.Entity<tbl_user>()
                .HasOne(u => u.TblUserLevel)        //says each tbl_user has one related tbl_level.
                .WithMany()                         //says each tbl_level can have many tbl_user rows. // if there ICollection defined on tbl_level then we need to pass l => l.tbl_user as parameter
                .HasForeignKey(u => u.level_id);    //tells EF Core that the foreign key column is level_id in tbl_user.

            _ = modelBuilder.Entity<tbl_user_user_module>()
                .HasOne(m => m.TblUser)
                .WithMany()
                .HasForeignKey(m => m.user_id);

            _ = modelBuilder.Entity<tbl_user_user_module>()
                .HasOne(m => m.TblUserModule)
                .WithMany()
                .HasForeignKey(m => m.module_id);

            _ = modelBuilder.Entity<tbl_user_user_menu>()
                .HasOne(m => m.TblUser)
                .WithMany()
                .HasForeignKey(m => m.user_id);

            _ = modelBuilder.Entity<tbl_user_menu>()
                .HasOne(r => r.TblUserModule)
                .WithMany()
                .HasForeignKey(r => r.module_id);

            _ = modelBuilder.Entity<tbl_employee_overtime_request>()
                .HasOne(r => r.TblEmployee)
                .WithMany(e => e.TblEmployeeOvertimeRequest)
                .HasForeignKey(r => r.emp_id);

            //for reletionship between tables (shared PK = One to One)
            _ = modelBuilder.Entity<tbl_employee>()
                .HasOne(e => e.tblEmployeeSalaryExtraSettings)
                .WithOne(s => s.TblEmployee)
                .HasForeignKey<tbl_employee_salary_extra_settings>(s => s.emp_id);

            _ = modelBuilder.Entity<tbl_employee>()
                .HasOne(e => e.TblEmployeeOvertimeSettings)
                .WithOne(s => s.TblEmployee)
                .HasForeignKey<tbl_employee_overtime_settings>(s => s.emp_id);


            //index for tbl_emaitbl_employee_check_in_out_mainl_list
            _ = modelBuilder.Entity<tbl_employee_check_in_out_main>()
                .HasIndex(e => new { e.id, e.emp_id, e.check_in, e.remarks })
                .HasDatabaseName("index_employee_check_in_out_main")
                .IncludeProperties(e => new { e.in_out_date, e.check_out, e.day_type, e.narration });

            //index for tbl_email_list
            _ = modelBuilder.Entity<tbl_email_list>()
                .HasIndex(e => new { e.id, e.status, e.category })
                .HasDatabaseName("index_email_list_search")
                .IncludeProperties(e => new { e.to_add, e.subject, e.sent_date, e.submit_date, e.cc_add });


            _ = modelBuilder.Entity<vw_swf_payback>()
                            .HasKey(t => new { t.emp_id, t.sal_month, t.sal_year });



            // 🔑 Global delete behavior convention: Restrict by default
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Map the view with emp_id as the key
            _ = modelBuilder.Entity<vw_Employee>()
                            .ToView("vw_Employee")       // map to SQL view
                            .HasKey(e => e.emp_id);

            _ = modelBuilder.Entity<vw_EmployeeOvertime>()
                .ToView("vw_EmployeeOvertime")       // map to SQL view
                .HasKey(e => e.OtReqId);           // use OtReqId as unique key

            _ = modelBuilder.Entity<vw_employee_leave_hash>()
                .ToView("vw_employee_leave_hash")       // map to SQL view
                .HasKey(e => e.emp_leave_id);           // use emp_leave_id as unique key
            _ = modelBuilder.Entity<tbl_employee_overtime_request_sub>()
                .HasKey(s => new { s.ot_req_id, s.sno });
            _ = modelBuilder.Entity<vw_Employee_Medical_Insurance>()
                .ToView("vw_Employee_Medical_Insurance")       // map to SQL view
                .HasKey(e => e.id);           // use Id as unique key
            _ = modelBuilder.Entity<vw_employee_salary_previous>()
                .ToView("vw_employee_salary_previous")   // tells EF Core it's a view, not a table
                .HasKey(e => e.sal_id);                  // use sal_id as the primary key
            _ = modelBuilder.Entity<vw_year_salary>()
                .HasNoKey()
                .ToView("vw_year_salary");
            _ = modelBuilder.Entity<vw_timesheet_sub>(entity =>
            {
                entity.ToView("vw_timesheet_sub"); // exact SQL view name

                // Composite key: emp_id + emp_year + emp_month + emp_day + submit_counter
                entity.HasKey(e => new { e.emp_id, e.emp_year, e.emp_month, e.emp_day, e.submit_counter });

                entity.Property(e => e.emp_id).HasColumnName("emp_id");
                entity.Property(e => e.emp_year).HasColumnName("emp_year");
                entity.Property(e => e.emp_month).HasColumnName("emp_month");
                entity.Property(e => e.emp_day).HasColumnName("emp_day");
                entity.Property(e => e.fund_id).HasColumnName("fund_id");
                entity.Property(e => e.time_hours).HasColumnName("time_hours");
                entity.Property(e => e.overtime_hours).HasColumnName("overtime_hours");
                entity.Property(e => e.submit_date).HasColumnName("submit_date");
                entity.Property(e => e.is_active).HasColumnName("is_active");
                entity.Property(e => e.submit_counter).HasColumnName("submit_counter");
                entity.Property(e => e.fiscal_year).HasColumnName("fiscal_year");
                entity.Property(e => e.emp_week).HasColumnName("emp_week");
                entity.Property(e => e.fiscal).HasColumnName("fiscal");
            });
            _ = modelBuilder.Entity<que_timesheet_sub>(entity =>
            {
                _ = entity.ToView("que_timesheet_sub"); // exact SQL view name

                // Composite key: emp_id + emp_year + emp_month + emp_day + submit_counter
                _ = entity.HasKey(e => new { e.emp_id, e.emp_year, e.emp_month, e.emp_day, e.submit_counter });

                _ = entity.Property(e => e.emp_id).HasColumnName("emp_id");
                _ = entity.Property(e => e.emp_year).HasColumnName("emp_year");
                _ = entity.Property(e => e.emp_month).HasColumnName("emp_month");
                _ = entity.Property(e => e.emp_day).HasColumnName("emp_day");
                _ = entity.Property(e => e.fund_id).HasColumnName("fund_id");
                _ = entity.Property(e => e.time_hours).HasColumnName("time_hours");
                _ = entity.Property(e => e.overtime_hours).HasColumnName("overtime_hours");
                _ = entity.Property(e => e.submit_date).HasColumnName("submit_date");
                _ = entity.Property(e => e.is_active).HasColumnName("is_active");
                _ = entity.Property(e => e.submit_counter).HasColumnName("submit_counter");
                _ = entity.Property(e => e.fiscal_year).HasColumnName("fiscal_year");
                _ = entity.Property(e => e.emp_week).HasColumnName("emp_week");
                _ = entity.Property(e => e.fiscal).HasColumnName("fiscal");
            });
        }
    }
}
