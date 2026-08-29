using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;

namespace wwfpp.Data
{
        //Request Administration
        //public DbSet<tbl_employee_dayoff> tbl_employee_dayoff { get; set; }
        public class tbl_employee_dayoff
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;
            
            public DateTime? dayoff_date { get; set; }  //[datetime] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        }

        //Leave
        //public DbSet<tbl_leave_heading> tbl_leave_heading { get; set; }
        public class tbl_leave_heading
        {
            [Key]
            public byte leave_type_id { get; set; }  //[tinyint] NOT NULL,
            public string? abbr { get; set; }  //[nvarchar](5) NULL,
            public string? description { get; set; }  //[nvarchar](25) NULL,
            public string? category { get; set; }  //[nvarchar](1) NULL,
            public double? max_leave_hours { get; set; }  //[float] NULL,
        }
        //public DbSet<tbl_employee_leave> tbl_employee_leave { get; set; }
        public class tbl_employee_leave
        {
            [Key]
            public int emp_leave_id { get; set; }  //[int] NOT NULL,

            [ForeignKey(nameof(TblLeaveHeading))]
            public byte? leave_type_id { get; set; }  //[tinyint] NULL,
            public tbl_leave_heading TblLeaveHeading { get; set; } = null!;

            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public DateTime? leave_from_date { get; set; }  //[datetime] NULL,
            public DateTime? leave_to_date { get; set; }  //[datetime] NULL,
            public string? leave_desc { get; set; }  //[ntext] NULL,
            public string? app_status { get; set; }  //[nvarchar](20) NULL,
            
            [ForeignKey(nameof(TblEmployeeAppBy))]
            public int? app_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeAppBy { get; set; } = null!;
            
            public DateTime? app_date { get; set; }  //[datetime] NULL,
            
            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;
            
            public double? leave_in_hrs { get; set; }  //[float] NULL,
            public string? app_remarks { get; set; }  //[text] NULL,
            public DateTime? can_submit_date { get; set; }  //[datetime] NULL,
            public string? can_desc { get; set; }  //[ntext] NULL,
            
            [ForeignKey(nameof(TblEmployeeCanBy))]
            public int? can_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeCanBy { get; set; } = null!;

            public DateTime? can_date { get; set; }  //[datetime] NULL,
            public string? can_remarks { get; set; }  //[ntext] NULL,
        }
        //public DbSet<tbl_employee_excess_leave_encash_emp_wise> tbl_employee_excess_leave_encash_emp_wise { get; set; }
        public class tbl_employee_excess_leave_encash_emp_wise
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;
            
            public decimal? amount { get; set; }  //[money] NULL,
            public double? total_hours { get; set; }  //[float] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_excess_leave_encash_fund_wise> tbl_employee_excess_leave_encash_fund_wise { get; set; }
        public class tbl_employee_excess_leave_encash_fund_wise
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; }= null!;

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            
            [ForeignKey(nameof(TblFundSource))]
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource { get; set; } = null!;

            public string? hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual> tbl_employee_leave_accrual { get; set; }
        public class tbl_employee_leave_accrual
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            public decimal? basic_salary { get; set; }  //[money] NULL,
            public double? leave_balance { get; set; }  //[float] NULL,
            public double? leave_accrual { get; set; }  //[float] NULL,
            public decimal? leave_encash { get; set; }  //[money] NULL,
            public decimal? pre_encash { get; set; }  //[money] NULL,
            public decimal? net_encash { get; set; }  //[money] NULL,
            public double? total_hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual_final> tbl_employee_leave_accrual_final { get; set; }
        public class tbl_employee_leave_accrual_final
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            public decimal? basic_salary { get; set; }  //[money] NULL,
            public double? carry_forward_leave { get; set; }  //[float] NULL,
            public double? annual_leave { get; set; }  //[float] NULL,
            public double? leave_taken { get; set; }  //[float] NULL,
            public double? leave_balance { get; set; }  //[float] NULL,
            public double? leave_accrual { get; set; }  //[float] NULL,
            public decimal? leave_encash { get; set; }  //[money] NULL,
            public decimal? pre_encash { get; set; }  //[money] NULL,
            public decimal? net_encash { get; set; }  //[money] NULL,
            public double? total_hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
            public short? counter { get; set; }  //[smallint] NULL,
            public double? an_paid_cleared { get; set; }  //[float] NULL,
            public double? si_carry_forward { get; set; }  //[float] NULL,
            public double? si_current { get; set; }  //[float] NULL,
            public double? si_taken { get; set; }  //[float] NULL,
            public double? si_paid_cleared { get; set; }  //[float] NULL,
            public double? si_balance { get; set; }  //[float] NULL,
            public double? si_accrual { get; set; }  //[float] NULL,
            public double? si_encash { get; set; }  //[float] NULL,
            public double? eli_day { get; set; }  //[float] NULL,
            public decimal? eli_amt { get; set; }  //[money] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual_fund_wise> tbl_employee_leave_accrual_fund_wise { get; set; }
        public class tbl_employee_leave_accrual_fund_wise
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; } //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!; 

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            
            [ForeignKey(nameof(TblFundSource))] 
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource {get;set;} = null!;

            public double? hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual_fund_wise_final> tbl_employee_leave_accrual_fund_wise_final { get; set; }
        public class tbl_employee_leave_accrual_fund_wise_final
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }//[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

            [ForeignKey(nameof(TblFundSource))] 
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource {get;set;} = null!;

            public double? hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual_new> tbl_employee_leave_accrual_new { get; set; }
        public class tbl_employee_leave_accrual_new
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            public decimal? basic_salary { get; set; }  //[money] NULL,
            public double? an_leave_balance { get; set; }  //[float] NULL,
            public double? an_leave_accrual { get; set; }  //[float] NULL,
            public double? si_leave_balance { get; set; }  //[float] NULL,
            public double? si_leave_accrual { get; set; }  //[float] NULL,
            public double? leave_accrual_days { get; set; }  //[float] NULL,
            public decimal? leave_payable { get; set; }  //[money] NULL,
            public decimal? pre_provisioned { get; set; }  //[money] NULL,
            public decimal? net_provision { get; set; }  //[money] NULL,
            public double? total_hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
            public int? counter { get; set; }  //[int] NULL,
        }
        //public DbSet<tbl_employee_leave_accrual_new_fund_wise> tbl_employee_leave_accrual_new_fund_wise { get; set; }
        public class tbl_employee_leave_accrual_new_fund_wise
        {
            [Key]
            public string id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

            [ForeignKey(nameof(TblFundSource))] 
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource {get;set;} = null!;

            public double? hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public int? counter { get; set; }  //[int] NULL,
        }

        //public DbSet<tbl_employee_leave_forward> tbl_employee_leave_forward { get; set; }
        public class tbl_employee_leave_forward
        {
            [Key]
            public int carry_forward_id { get; set; }  //[int] NOT NULL,
            public double? hours { get; set; }  //[float] NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year_to { get; set; }  //[nvarchar](9) NULL,
        }
        //public DbSet<tbl_employee_leave_hash> tbl_employee_leave_hash { get; set; }
        public class tbl_employee_leave_hash
        {
            //This leave Future
            [Key]
            public int emp_leave_id { get; set; }  //[int] NOT NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](18) NULL,

            [ForeignKey(nameof(TblLeaveHeading))]
            public byte? leave_type_id { get; set; }  //[tinyint] NULL,
            public tbl_leave_heading TblLeaveHeading { get; set; } = null!;

            public DateTime? submit_date { get; set; }  //[smalldatetime] NULL,
            public DateTime? leave_from_date { get; set; }  //[smalldatetime] NULL,
            public DateTime? leave_to_date { get; set; }  //[smalldatetime] NULL,
            public string? leave_desc { get; set; }  //[ntext] NULL,
            public string? app_status { get; set; }  //[nvarchar](20) NULL,

            [ForeignKey(nameof(TblEmployeeAppBy))]
            public int? app_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeAppBy { get; set; } = null!;

            public DateTime? app_date { get; set; }  //[smalldatetime] NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public double? leave_in_hrs { get; set; }  //[float] NULL,
            public string? app_remarks { get; set; }  //[text] NULL,
        }
        //public DbSet<tbl_employee_leave_indv> tbl_employee_leave_indv { get; set; }
        public class tbl_employee_leave_indv
        {
            [Key]
            public int indv_leave_id { get; set; }  //[int] NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public double? annual_leave { get; set; }  //[float] NULL,
            public double? casual_leave { get; set; }  //[float] NULL,
            public double? sick_leave { get; set; }  //[float] NULL,
            public double? annual_leave_hours_carry_forward { get; set; }  //[float] NULL,
            public double? maternity { get; set; }  //[float] NULL,
            public double? paternity { get; set; }  //[float] NULL,
            public double? mourning { get; set; }  //[float] NULL,
            public double? unpaid_study { get; set; }  //[float] NULL,
            public string? fiscal_year_to { get; set; }  //[nvarchar](15) NULL,
            public double? other_leave { get; set; }  //[float] NULL,
            public double? sick_leave_hours_carry_forward { get; set; }  //[float] NULL,
        }
        //public DbSet<tbl_employee_leave_indv_cafw_paid_laps> tbl_employee_leave_indv_cafw_paid_laps { get; set; }
        public class tbl_employee_leave_indv_cafw_paid_laps
        {
            [Key]
            public int indv_leave_id { get; set; }  //[int] NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year { get; set; }  //[nvarchar](15) NULL,
            public double? max_annual_leave_cafw { get; set; }  //[float] NULL,
            public double? tot_annual_leave_paid { get; set; }  //[float] NULL,
            public double? cur_annual_leave_laps { get; set; }  //[float] NULL,
            public double? max_sick_leave_cafw { get; set; }  //[float] NULL,
            public double? tot_sick_leave_paid { get; set; }  //[float] NULL,
            public double? cur_sick_leave_laps { get; set; }  //[float] NULL,
            public int? sumbit_counter { get; set; }  //[int] NULL,
            public double? bacic_salary { get; set; }  //[float] NULL,
            public double? tot_annual_leave_amt { get; set; }  //[float] NULL,
            public double? tot_sick_leave_amt { get; set; }  //[float] NULL,
            public int? paid_month { get; set; }  //[int] NULL,
            public int? paid_year { get; set; }  //[int] NULL,
        }
        //public DbSet<tbl_employee_leave_indv_paid_cleared> tbl_employee_leave_indv_paid_cleared { get; set; }
        public class tbl_employee_leave_indv_paid_cleared
        {
            [Key]
            public int indv_leave_id { get; set; }  //[int] NOT NULL,
            
            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year { get; set; }  //[nvarchar](15) NULL,
            public double? annual_leave { get; set; }  //[float] NULL,
            public double? casual_leave { get; set; }  //[float] NULL,
            public double? sick_leave { get; set; }  //[float] NULL,
            public double? other_leave { get; set; }  //[float] NULL,
            public double? maternity { get; set; }  //[float] NULL,
            public double? paternity { get; set; }  //[float] NULL,
            public double? mourning { get; set; }  //[float] NULL,
            public double? unpaid_study { get; set; }  //[float] NULL,
            public double? annual_leave_hours_carry_forward { get; set; }  //[float] NULL,
            public double? sick_leave_hours_carry_forward { get; set; }  //[float] NULL,
            public double? annual_leave_paid { get; set; }  //[float] NULL,
            public double? annual_leave_laps { get; set; }  //[float] NULL,
            public double? casual_leave_laps { get; set; }  //[float] NULL,
            public double? sick_leave_laps { get; set; }  //[float] NULL,
            public double? other_leave_laps { get; set; }  //[float] NULL,
            public double? maternity_laps { get; set; }  //[float] NULL,
            public double? paternity_laps { get; set; }  //[float] NULL,
            public double? mourning_laps { get; set; }  //[float] NULL,
            public double? unpaid_study_laps { get; set; }  //[float] NULL,
            public double? annual_leave_hours_carry_forward_laps { get; set; }  //[float] NULL,
            public double? sick_leave_hours_carry_forward_laps { get; set; }  //[float] NULL,
            public DateTime? date_from { get; set; }  //[datetime] NULL,
            public DateTime? date_upto { get; set; }  //[datetime] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
        }
        //public DbSet<tbl_employee_leave_indv_paid_cleared_new> tbl_employee_leave_indv_paid_cleared_new { get; set; }
        public class tbl_employee_leave_indv_paid_cleared_new
        {
            [Key]
            public int indv_leave_id { get; set; }  //[int] NOT NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? fiscal_year { get; set; }  //[nvarchar](15) NULL,
            public double? annual_leave_caf { get; set; }  //[float] NULL,
            public double? sick_leave_caf { get; set; }  //[float] NULL,
            public double? annual_leave { get; set; }  //[float] NULL,
            public double? casual_leave { get; set; }  //[float] NULL,
            public double? sick_leave { get; set; }  //[float] NULL,
            public double? other_leave { get; set; }  //[float] NULL,
            public double? maternity { get; set; }  //[float] NULL,
            public double? paternity { get; set; }  //[float] NULL,
            public double? mourning { get; set; }  //[float] NULL,
            public double? unpaid_study { get; set; }  //[float] NULL,
            public double? annual_leave_caf_paid { get; set; }  //[float] NULL,
            public double? sick_leave_caf_paid { get; set; }  //[float] NULL,
            public double? annual_leave_paid { get; set; }  //[float] NULL,
            public double? casual_leave_paid { get; set; }  //[float] NULL,
            public double? sick_leave_paid { get; set; }  //[float] NULL,
            public double? other_leave_paid { get; set; }  //[float] NULL,
            public double? maternity_paid { get; set; }  //[float] NULL,
            public double? paternity_paid { get; set; }  //[float] NULL,
            public double? mourning_paid { get; set; }  //[float] NULL,
            public double? unpaid_study_paid { get; set; }  //[float] NULL,
            public double? annual_leave_caf_laps { get; set; }  //[float] NULL,
            public double? sick_leave_caf_laps { get; set; }  //[float] NULL,
            public double? annual_leave_laps { get; set; }  //[float] NULL,
            public double? casual_leave_laps { get; set; }  //[float] NULL,
            public double? sick_leave_laps { get; set; }  //[float] NULL,
            public double? other_leave_laps { get; set; }  //[float] NULL,
            public double? maternity_laps { get; set; }  //[float] NULL,
            public double? paternity_laps { get; set; }  //[float] NULL,
            public double? mourning_laps { get; set; }  //[float] NULL,
            public double? unpaid_study_laps { get; set; }  //[float] NULL,
            public DateTime? date_from { get; set; }  //[datetime] NULL,
            public DateTime? date_upto { get; set; }  //[datetime] NULL,
            public int? submit_counter { get; set; }  //[int] NULL,
            public string? remarks { get; set; }  //[nvarchar](250) NULL,
        }
        //public DbSet<tbl_yearly_annual_leave_cf> tbl_yearly_annual_leave_cf { get; set; }
        public class tbl_yearly_annual_leave_cf
        {
            //CompositPK
            public double? hrs { get; set; }  //[float] NULL
        }
        //public DbSet<tbl_yearly_sick_leave_cf> tbl_yearly_sick_leave_cf { get; set; }
        public class tbl_yearly_sick_leave_cf
        {
            //CompositPK
            public double? hrs { get; set; }  //[float] NULL
        }

        /*****TIMESHEET******/
        //public DbSet<tbl_employee_timesheet_main> tbl_employee_timesheet_main { get; set; }
        public class tbl_employee_timesheet_main
        {
            //CompositPK
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public short? emp_year { get; set; }  //[smallint] NULL,
            public byte? emp_month { get; set; }  //[tinyint] NULL,
            public byte? emp_day { get; set; }  //[tinyint] NULL,

            [ForeignKey(nameof(TblLeaveHeading))]
            public byte? leave_type_id { get; set; }  //[tinyint] NULL,
            public tbl_leave_heading TblLeaveHeading { get; set; } = null!;

            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public int? submit_counter { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public short? emp_week { get; set; }  //[smallint] NULL
        }
        //public DbSet<tbl_employee_timesheet_sub> tbl_employee_timesheet_sub { get; set; }
        public class tbl_employee_timesheet_sub
        {
            //CompositPK
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public short? emp_year { get; set; }  //[smallint] NULL,
            public byte? emp_month { get; set; }  //[tinyint] NULL,
            public byte? emp_day { get; set; }  //[tinyint] NULL,

            [ForeignKey(nameof(TblFundSource))] 
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource {get;set;} = null!;

            public double? time_hours { get; set; }  //[float] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public double? overtime_hours { get; set; }  //[float] NULL,
            public string? is_active { get; set; }  //[nvarchar](5) NULL,
            public int? submit_counter { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public short? emp_week { get; set; }  //[smallint] NULL
        }
        //public DbSet<tbl_employee_timesheet_app> tbl_employee_timesheet_app { get; set; }
        public class tbl_employee_timesheet_app
        {
            [Key]
            public string app_id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public int? emp_year { get; set; }  //[int] NULL,
            public int? emp_month { get; set; }  //[int] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? app_dec { get; set; }  //[nvarchar](1) NULL,

            [ForeignKey(nameof(TblEmployeeAppBy))]
            public int? app_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeAppBy { get; set; } = null!;  //[int] NULL,

            public int? submit_counter { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public short? emp_week { get; set; }  //[smallint] NULL,
            public DateTime? app_date { get; set; }  //[datetime] NULL,
            public string? app_remarks { get; set; }  //[text] NULL,
        }
        //public DbSet<tbl_employee_timesheet_edited> tbl_employee_timesheet_edited { get; set; }
        public class tbl_employee_timesheet_edited
        {
            //CompositPK
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public int? emp_year { get; set; }  //[int] NULL,
            public int? emp_month { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
            public int? emp_week { get; set; }  //[int] NULL,
            public int submit_counter { get; set; }  //[int] NOT NULL,
            public string? view_status { get; set; }  //[nvarchar](1) NULL,

            [ForeignKey(nameof(TblEmployeeAccBy))] 
            public int account_emp_id { get; set; }  //[int] NOT NULL,
            public tbl_employee TblEmployeeAccBy { get; set; } = null!;  //[int] NULL,

            public DateTime? updated_date { get; set; }  //[datetime] NOT NULL
        }
        //public DbSet<tbl_employee_timesheet_sub_hash> tbl_employee_timesheet_sub_hash { get; set; }
        public class tbl_employee_timesheet_sub_hash
        {
            [Key]
            public string id { get; set; }            //[varchar] (50) NOT NULL
            
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public short? emp_year { get; set; }  //[smallint] NULL,
            public byte? emp_month { get; set; }  //[tinyint] NULL,
            public byte? emp_day { get; set; }  //[tinyint] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public short? emp_week { get; set; }  //[smallint] NULL,
            
            [ForeignKey(nameof(TblFundSource))] 
            public int? fund_id { get; set; }  //[int] NULL,
            public tbl_fund_source TblFundSource {get;set;} = null!;
            
            public double? time_hours { get; set; }  //[float] NULL,
            public double? overtime_hours { get; set; }  //[float] NULL
        }

        /*********************TRAVEL****************/
        //public DbSet<tbl_travel_particulars> tbl_travel_particulars { get; set; }
        public class tbl_travel_particulars
        {
            [Key]
            public byte par_id { get; set; }                //[tinyint] NOT NULL,
            public string? particular { get; set; }           // ntext NULL
        }
        //public DbSet<tbl_employee_administrator> tbl_employee_administrator { get; set; }

        public class tbl_employee_administrator
        {
            //holds emp_id on all field.//so many so not idea wheather to define foreign key or not 
            [Key]
            public short id { get; set; }  //[smallint] NOT NULL,
            public int? cra { get; set; }  //[int] NULL,
            public int? doo { get; set; }  //[int] NULL,
            public int? faa { get; set; }  //[int] NULL,
            public int? aca { get; set; }  //[int] NULL,
            public int? hra { get; set; }  //[int] NULL,
            public int? rca { get; set; }  //[int] NULL,
            public int? t_t_a_1 { get; set; }  //[int] NULL,
            public int? t_t_a_2 { get; set; }  //[int] NULL,
            public int? t_a_s_1 { get; set; }  //[int] NULL,
            public int? t_a_s_2 { get; set; }  //[int] NULL,
            public int? t_a_s_3 { get; set; }  //[int] NULL,
            public int? t_a_s_4 { get; set; }  //[int] NULL,
            public int? acr { get; set; }  //[int] NULL,
            public int? t_t_a_3 { get; set; }  //[int] NOT NULL,
            public int? t_t_a_4 { get; set; }  //[int] NOT NULL,
            public int? t_t_a_5 { get; set; }  //[int] NOT NULL,
            public int? t_a_s_5 { get; set; }  //[int] NOT NULL,
            public int? ahr { get; set; }  //[int] NULL,
        }
        //public DbSet<tbl_employee_travel_main> tbl_employee_travel_main { get; set; }
        public class tbl_employee_travel_main
        {
            //CompositPK
            [Key]
            public int emp_travel_id { get; set; }  //[int] NOT NULL,

            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,

            public string? trip_purpose { get; set; }  //[ntext] NULL,
            public string? destinations { get; set; }  //[nvarchar](255) NULL,
            public DateTime? date_from { get; set; }  //[datetime] NULL,
            public DateTime? date_to { get; set; }  //[datetime] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? app_status { get; set; }  //[nvarchar](20) NULL,

            [ForeignKey(nameof(TblEmployeeAppBy))]
            public int? app_by { get; set; }
            public tbl_employee TblEmployeeAppBy { get; set; } = null!;  //[int] NULL,

            public DateTime? app_date { get; set; }  //[datetime] NULL,
            public string? denomination { get; set; }  //[ntext] NULL,
            public string? remarks { get; set; }  //[ntext] NULL,
            public string? travel_type { get; set; }  //[nvarchar](20) NULL,
            public string? i_app_status { get; set; }  //[nvarchar](20) NULL,

            [ForeignKey(nameof(TblEmployeeIAppBy))]
            public int? i_app_by { get; set; }
            public tbl_employee TblEmployeeIAppBy { get; set; } = null!;  //[int] NULL,

            public DateTime? i_app_date { get; set; }  //[datetime] NULL,
            public string? i_app_by_post { get; set; }  //[nvarchar](100) NULL,
            public string? app_by_post { get; set; }  //[nvarchar](100) NULL,
            public string? rec_remarks { get; set; }  //[text] NULL,
            public string? app_remarks { get; set; }  //[text] NULL,
            public DateTime? can_submit_date { get; set; }  //[datetime] NULL,
            public string? can_desc { get; set; }  //[ntext] NULL,

            [ForeignKey(nameof(TblEmployeeCanBy))]
            public int? can_by { get; set; }
            public tbl_employee TblEmployeeCanBy { get; set; } = null!;  //[int] NULL,

            public DateTime? can_date { get; set; }  //[datetime] NULL,
            public string? can_remarks { get; set; }  //[ntext] NULL,
            public string? employeenameWithCode { get; set; } // from vw_Employee
    }
        //public DbSet<tbl_employee_travel_sub> tbl_employee_travel_sub { get; set; }
        public class tbl_employee_travel_sub
        {
            //no primary key | so define later
            //currently : CompositPK emp_travel_id+par_id

            [ForeignKey(nameof(TblEmployeetravelMain))]
            public int emp_travel_id { get; set; }  //[int] NULL,
            public tbl_employee_travel_main TblEmployeetravelMain { get; set; } = null!;

            public byte? par_id { get; set; }  //[tinyint] NULL,
            public string? detail { get; set; }  //[nvarchar](255) NULL,
            public string? unit { get; set; }  //[nvarchar](20) NULL,
            public byte? cur_id { get; set; }  //[tinyint] NULL,
            public double? nos { get; set; }  //[float] NULL,
            public decimal? rate { get; set; }  //[money] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public DateTime? update_date { get; set; }  //[datetime] NULL
        }
        //public DbSet<tbl_employee_travel_codes> tbl_employee_travel_codes { get; set; }
        public class tbl_employee_travel_codes
        {
            //CompositPK
            [ForeignKey(nameof(TbEemployeeTravelMain))]
            public int emp_travel_id { get; set; }  //[int] NOT NULL,
            public tbl_employee_travel_main TbEemployeeTravelMain {get; set; } = null!;

            public byte? sn { get; set; }  //[tinyint] NULL,

            [ForeignKey(nameof(TblFundSource))]
            public int? fund_id { get; set; }  //[int] NULL
            public tbl_fund_source TblFundSource {get; set; } = null!;
        }
        //public DbSet<tbl_employee_travel_printed> tbl_employee_travel_printed { get; set; }
        public class tbl_employee_travel_printed
        {
            //CompositPK or one to one
            [Key]
            public int emp_travel_id { get; set; }  //[int] NOT NULL,
            [ForeignKey(nameof(emp_travel_id))]
            public tbl_employee_travel_main TbEmployeeTravelMain {get; set; } = null!;

            [ForeignKey(nameof(TblEmployeeAccAppBy))]
            public int? acc_app_by { get; set; }
            public tbl_employee TblEmployeeAccAppBy { get; set; } = null!;  //[int] NULL,

            [ForeignKey(nameof(TblEmployeeAdvAppBy))]
            public int? adv_app_by { get; set; }
            public tbl_employee TblEmployeeAdvAppBy { get; set; } = null!;  //[int] NULL,

            public string? acc_app_by_post { get; set; }  //[nvarchar](100) NULL,
            public string? adv_app_by_post { get; set; }  //[nvarchar](100) NULL,
        }

        /***********TRAVEL SETTLEMENT ***************/
        //public DbSet<tbl_employee_travel_settlement_main> tbl_employee_travel_settlement_main { get; set; }
        public class tbl_employee_travel_settlement_main
        {
            [Key]
            public string trav_set_id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployeeTravelMain))]
            public int? emp_travel_id { get; set; }  //[int] NULL,
            public tbl_employee_travel_main TblEmployeeTravelMain { get; set; } = null!;
            
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,
            
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public DateTime? travel_date { get; set; }  //[datetime] NULL,
            public DateTime? return_date { get; set; }  //[datetime] NULL,
            public double? usd_rate { get; set; }  //[float] NULL,
            public decimal? adv_cash_less { get; set; }  //[money] NULL,
            public string? charge_per_or_amt { get; set; }  //[nvarchar](1) NULL,
            public int? charge_fund_id_1 { get; set; }  //[int] NULL,   //currently FK not defined with tbl_fund_source
            public int? charge_fund_id_2 { get; set; }  //[int] NULL,   //currently FK not defined with tbl_fund_source
            public int? charge_fund_id_3 { get; set; }  //[int] NULL,   //currently FK not defined with tbl_fund_source
            public int? charge_fund_id_4 { get; set; }  //[int] NULL,   //currently FK not defined with tbl_fund_source
            public double? charge_fund_per_1 { get; set; }  //[float] NULL, 
            public double? charge_fund_per_2 { get; set; }  //[float] NULL, 
            public double? charge_fund_per_3 { get; set; }  //[float] NULL, 
            public double? charge_fund_per_4 { get; set; }  //[float] NULL, 
            public decimal? charge_fund_amt_1 { get; set; }  //[money] NULL,
            public decimal? charge_fund_amt_2 { get; set; }  //[money] NULL,
            public decimal? charge_fund_amt_3 { get; set; }  //[money] NULL,
            public decimal? charge_fund_amt_4 { get; set; }  //[money] NULL,
            public string? remarks { get; set; }  //[nvarchar](255) NULL,
            public string? app_status { get; set; }  //[nvarchar](1) NULL,

            [ForeignKey(nameof(TblEmployeeAppby))]
            public int? app_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeAppby {get; set;} =null!; 

            public DateTime? app_date { get; set; }  //[datetime] NULL,
            public string? is_for_set { get; set; }  //[nvarchar](1) NULL,
    }
        //public DbSet<tbl_employee_travel_settlement_sub> tbl_employee_travel_settlement_sub { get; set; }
        public class tbl_employee_travel_settlement_sub
        {
            //CompositPK
            //[Key] trev_set_id+sn

            [ForeignKey(nameof(TblEmployeeTravelSettlementMain))]
            public string trav_set_id { get; set; } //[nvarchar](50) NULL,
            public tbl_employee_travel_settlement_main TblEmployeeTravelSettlementMain { get; set; } = null!;  //[int] NULL,
            
            public short? sn { get; set; }  //[smallint] NULL,
            public DateTime? bill_date { get; set; }  //[datetime] NULL,
            public string? location { get; set; }       //[nvarchar](255) NULL,
            public string? description { get; set; }    //[nvarchar](255) NULL,

            //ref is a reserved keyword in C#, so you can’t use it directly as a property name.                        
            [Column("ref")] 
            public string? RefField { get;set;}             //[nvarchar](50) NULL,
        
            public string? int_cur_name { get; set; }  //[nvarchar](50) NULL,       
            public double? int_rate { get; set; }  //[float] NULL,
            public decimal? int_amount { get; set; }  //[money] NULL,
            public decimal? int_usd_amount { get; set; }  //[money] NULL,
            public decimal? nat_bill_amount { get; set; }  //[money] NULL,
            public decimal? nat_VAT { get; set; }  //[money] NULL,
            public decimal? nat_TDS { get; set; }  //[money] NULL,
            public decimal? nat_amount { get; set; }  //[money] NULL
        }
        //public DbSet<tbl_employee_travel_settlement_sub_doc> tbl_employee_travel_settlement_sub_doc { get; set; }
        public class tbl_employee_travel_settlement_sub_doc
        {
            [Key]
            public string trav_set_doc_id { get; set; }  //[nvarchar](50) NOT NULL,
            public string? doc_name { get; set; }  //[nvarchar](250) NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,

            [ForeignKey(nameof(TblEmployeeTravelSettlementMain))]
            public string trav_set_id { get; set; } //[nvarchar](50) NULL,
            public tbl_employee_travel_settlement_main TblEmployeeTravelSettlementMain { get; set; } = null!;  

        }
        
        /******************OVERTIME**********************/
        //public DbSet<tbl_employee_overtime> tbl_employee_overtime { get; set; }
        public class tbl_employee_overtime
        {
            [Key]
            public int ot_id { get; set; }  //[int] NOT NULL,
            
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,
            
            public int? sal_year { get; set; }  //[int] NULL,
            public int? sal_month { get; set; }  //[int] NULL,
            public decimal? basic_salary { get; set; }  //[money] NULL,
            public double? times { get; set; }  //[float] NULL,
            public decimal? rate { get; set; }  //[money] NULL,
            public double? hrs { get; set; }  //[float] NULL,
            public string? remarks { get; set; }  //[nvarchar](100) NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,

            [ForeignKey(nameof(TblEmployeeSubmitBy))]
            public int? submit_by { get; set; }
            public tbl_employee TblEmployeeSubmitBy { get; set; } = null!;  

            public decimal? ot_diff { get; set; }  //[money] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public byte? emp_week { get; set; }  //[tinyint] NULL,
            public int? pay_period_total_working_hrs { get; set; }  //[int] NULL,
        }
        //public DbSet<tbl_employee_overtime_final> tbl_employee_overtime_final { get; set; }
        public class tbl_employee_overtime_final
        {
            [Key]
            public int ot_id { get; set; }  //[int] NOT NULL,
            
            [ForeignKey(nameof(TblEmployee))] 
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;

            public int? sal_year { get; set; }  //[int] NULL,
            public int? sal_month { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public byte? emp_week { get; set; }  //[tinyint] NULL,
            public decimal? basic_salary { get; set; }  //[money] NULL,
            public double? times { get; set; }  //[float] NULL,
            public decimal? rate { get; set; }  //[money] NULL,
            public double? hrs { get; set; }  //[float] NULL,
            public string? remarks { get; set; }  //[nvarchar](100) NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            
            [ForeignKey(nameof(TblEmployeeSubmitBy))] 
            public int? submit_by { get; set; }  //[int] NULL,
            public tbl_employee TblEmployeeSubmitBy { get; set; } = null!;

            public int? pay_period_total_working_hrs { get; set; }  //[int] NULL,
            public short? counter { get; set; }  //[smallint] NULL,
        }
        //public DbSet<tbl_employee_overtime_request> tbl_employee_overtime_request { get; set; }
        public class tbl_employee_overtime_request
        {
            [Key]
            public string ot_req_id { get; set; }  //[nvarchar](50) NOT NULL,

            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }
            public tbl_employee TblEmployee { get; set; } = null!;  //[int] NULL,
            
            public DateTime? ot_date { get; set; }  //[datetime] NULL,
            public double? total_hours { get; set; }  //[float] NULL,
            public string? ot_desc { get; set; }  //[nvarchar](255) NULL,
            
            //[ForeignKey(nameof(TblEmployeeReqBy))]
            public int? requested_by { get; set; }
            //public tbl_employee TblEmployeeReqBy { get; set; } = null!;
            public string? req_status { get; set; }  //[nvarchar](1) NULL,
            public DateTime? req_date { get; set; }  //[datetime] NULL,
            public string? app_status { get; set; }  //[nvarchar](1) NULL,

            //[ForeignKey(nameof(TblEmployeeAppBy))]
            public int? app_by { get; set; }
            //public tbl_employee TblEmployeeAppBy { get; set; } = null!;  //[int] NULL,

            public DateTime? app_date { get; set; }  //[datetime] NULL,
            public DateTime? submit_date { get; set; }  //[datetime] NULL,
            public string? is_paid { get; set; }  //[nvarchar](1) NULL,
            public int? paid_month { get; set; }  //[int] NULL,
            public int? paid_year { get; set; }  //[int] NULL,
            public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
            public byte? emp_week { get; set; }  //[tinyint] NULL,
            public int? paid_day { get; set; }  //[int] NULL,
            public string? req_remarks { get; set; }  //[text] NULL,
            public string? app_remarks { get; set; }  //[text] NULL,
        }
        //public DbSet<tbl_employee_overtime_request_sub> tbl_employee_overtime_request_sub { get; set; }
        public class tbl_employee_overtime_request_sub
        {
            //CompositPK
            //[Key] ot_req_id+sno
            [ForeignKey(nameof(TblEmployeeOvertimeRequest))]
            public string? ot_req_id { get; set; }  //[nvarchar](50) NULL,
            public tbl_employee_overtime_request TblEmployeeOvertimeRequest { get; set; } = null!; 
            
            public short? sno { get; set; }  //[smallint] NULL,
            public string? start_time { get; set; }  //[nvarchar](11) NULL,
            public string? end_time { get; set; }  //[nvarchar](11) NULL
        }
        //public DbSet<tbl_employee_overtime_settings> tbl_employee_overtime_settings { get; set; }
        public class tbl_employee_overtime_settings
        {
            // one to one relationship
            [Key]
            public int emp_id { get; set; }

            public tbl_employee? TblEmployee { get; set; }  //[int] NULL,
            
            public string? is_get_overtime { get; set; }  //[nvarchar](1) NULL,
            public int? approval_person { get; set; }  //[int] NULL
        }


}
