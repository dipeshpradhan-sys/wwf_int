using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wwfpp.Data
{
    //public DbSet<tbl_fiscal_year> tbl_fiscal_year { get; set; }
    public class tbl_fiscal_year
    {
        [Key]
        public string fiscal_year { get; set; } // [nvarchar](9) NOT NULL,
        public DateTime? date_from { get; set; }    //[datetime] NULL,
        public DateTime? date_to { get; set; }      //[datetime] NULL,
        public string? is_active { get; set; }      //[nvarchar] (1) NULL,
        public string? fiscal_year_abb { get; set; } //[varchar](50) NULL,
        public int yearly_working_hrs { get; set; } // int null

        //ICollection
    }
    //public DbSet<tbl_calendar_year> tbl_calendar_year { get; set; }
    public class tbl_calendar_year
    {
        [Key]
        public string calendar_year { get; set; }           //[nvarchar](9) NOT NULL,PRIMARY KEY
        public DateTime? calendar_date_from { get; set; }   //[datetime] NULL,
        public DateTime? calendar_date_to { get; set; }     //[datetime] NULL,
        public string? calendar_is_active { get; set; }     // [nvarchar](1) NULL,
        public string? calendar_year_abb { get; set; }       //[varchar](50) NULL,
    }
    //public DbSet<tbl_setting_timesheet_type> tbl_setting_timesheet_type { get; set; }
    public class tbl_setting_timesheet_type
    {
        [Key]
        public int type_id { get; set; }                    //[int] NOT NULL
        public string? timesheet_type { get; set; }          //[nvarchar](50) NULL,
         public short? first_day_of_week { get; set; }       //[smallint] NULL,
        /*
         * This need to deprecate as the values are 
         * saved on option table and have from there
         */

    }
    //public DbSet<tbl_calendar_setting> tbl_calendar_setting { get; set; }
    public class tbl_calendar_setting
    {
        [Key]
        public int cal_id { get; set; } //[int] NOT NULL,
        public byte cal_month { get; set; } //[tinyint] NULL,
        public short cal_year { get; set; } //[smallint] NULL,
        public string? d1 { get; set; }//[nvarchar](5) NULL,
        public string? d2 { get; set; }//[nvarchar](5) NULL,
        public string? d3 { get; set; }//[nvarchar](5) NULL,
        public string? d4 { get; set; }//[nvarchar](5) NULL,
        public string? d5 { get; set; }//[nvarchar](5) NULL,
        public string? d6 { get; set; }//[nvarchar](5) NULL,
        public string? d7 { get; set; }//[nvarchar](5) NULL,
        public string? d8 { get; set; }//[nvarchar](5) NULL,
        public string? d9 { get; set; }//[nvarchar](5) NULL,
        public string? d10 { get; set; }//[nvarchar](5) NULL,
        public string? d11 { get; set; }//[nvarchar](5) NULL,
        public string? d12 { get; set; }//[nvarchar](5) NULL,
        public string? d13 { get; set; }//[nvarchar](5) NULL,
        public string? d14 { get; set; }//[nvarchar](5) NULL,
        public string? d15 { get; set; }//[nvarchar](5) NULL,
        public string? d16 { get; set; }//[nvarchar](5) NULL,
        public string? d17 { get; set; }//[nvarchar](5) NULL,
        public string? d18 { get; set; }//[nvarchar](5) NULL,
        public string? d19 { get; set; }//[nvarchar](5) NULL,
        public string? d20 { get; set; }//[nvarchar](5) NULL,
        public string? d21 { get; set; }//[nvarchar](5) NULL,
        public string? d22 { get; set; }//[nvarchar](5) NULL,
        public string? d23 { get; set; }//[nvarchar](5) NULL,
        public string? d24 { get; set; }//[nvarchar](5) NULL,
        public string? d25 { get; set; }//[nvarchar](5) NULL,
        public string? d26 { get; set; }//[nvarchar](5) NULL,
        public string? d27 { get; set; }//[nvarchar](5) NULL,
        public string? d28 { get; set; }//[nvarchar](5) NULL,
        public string? d29 { get; set; }//[nvarchar](5) NULL,
        public string? d30 { get; set; }//[nvarchar](5) NULL,
        public string? d31 { get; set; }//[nvarchar](5) NULL,
    }
    //public DbSet<tbl_calendar_setting_biweekly> tbl_calendar_setting_biweekly { get; set; }
    public class tbl_calendar_setting_biweekly
    {
        [Key]
        public int cal_id { get; set; }                 //[int] NOT NULL,
        public string? fiscal_year { get; set; }        //[nvarchar](10) NULL,
        public DateTime? period_start_date { get; set; } //[datetime] NULL,
        public DateTime? period_end_date { get; set; }   //[datetime] NULL,
        public int week_name { get; set; }              //[int] NULL,    
        public string? d1 { get; set; }                 //[nvarchar](5) NULL
        public string? d2 { get; set; }                 //[nvarchar](5) NULL
        public string? d3 { get; set; }                 //[nvarchar](5) NULL
        public string? d4 { get; set; }                 //[nvarchar](5) NULL
        public string? d5 { get; set; }                 //[nvarchar](5) NULL
        public string? d6 { get; set; }                 //[nvarchar](5) NULL
        public string? d7 { get; set; }                 //[nvarchar](5) NULL
        public string? d8 { get; set; }                 //[nvarchar](5) NULL    
        public string? d9 { get; set; }                 //[nvarchar](5) NULL
        public string? d10 { get; set; }                //[nvarchar](5) NULL
        public string? d11 { get; set; }                //[nvarchar](5) NULL
        public string? d12 { get; set; }                //[nvarchar](5) NULL    
        public string? d13 { get; set; }                //[nvarchar](5) NULL
        public string? d14 { get; set; }                //[nvarchar](5) NULL
    }
    //public DbSet<tbl_calendar_setting_weekly> tbl_calendar_setting_weekly { get; set; }
    public class tbl_calendar_setting_weekly
    {   
        [Key]
        public int cal_id { get; set; }     // [int] NOT NULL,
        public string? fiscal_year { get; set; }    // [nvarchar](10) NULL,
        public DateTime? period_start_date { get; set; } //[datetime] NULL,
        public DateTime? period_end_date { get; set; }//[datetime] NULL,
        public int week_name { get; set; }   //[int] NULL,
        public string? d1 { get; set; } //[nvarchar](5) NULL,
        public string? d2 { get; set; } //[nvarchar](5) NULL,
        public string? d3 { get; set; } //[nvarchar](5) NULL,
        public string? d4 { get; set; } //[nvarchar](5) NULL,
        public string? d5 { get; set; } //[nvarchar](5) NULL,
        public string? d6 { get; set; } //[nvarchar](5) NULL,
        public string? d7 { get; set; } //[nvarchar](5) NULL,
    }
    //public DbSet<tbl_general_setting> tbl_general_setting { get; set; }
    public class tbl_general_setting
    {
        /*This is not in use. is this deprecated or else??? */
        [Key]
        public int setting_id { get; set; }  //[int] NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public double? yearly_working_hrs { get; set; }  //[float] NULL,
        public double? yearly_ins_amt_deduction { get; set; }  //[float] NULL,
    }
    //public DbSet<tbl_setting_holidays> tbl_setting_holidays { get; set; }
    public class tbl_setting_holidays
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public DateTime? holiday_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
    }
    //public DbSet<tbl_setting_language> tbl_setting_language { get; set; }
    public class tbl_setting_language
    {
        [Key]
        public int language_id { get; set; }  //[int] NOT NULL,
        public string? language { get; set; }  //[nvarchar](50) NULL,
        public DateTime? date { get; set; }  //[datetime] NULL,
        /*This also need to depricate as the language value taken from option table*/
    }
    //public DbSet<tbl_setting_limit_hrs> tbl_setting_limit_hrs { get; set; }
    public class tbl_setting_limit_hrs
    {
        /*Working hour menu*/
        [Key]
        public string hrs_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? normal_working_hrs { get; set; }  //[int] NULL,
        public int? overtime_normal_working_hrs { get; set; }  //[int] NULL,
        public int? overtime_hol_wek_working_hrs { get; set; }  //[int] NULL,
        public int? working_hours_per_pay_period { get; set; }  //[int] NULL,
        public string? populate_hrs_in_timesheet_for_holiday { get; set; }  //[nvarchar](1) NULL,
        public string? populate_hrs_in_timesheet_for_weekend { get; set; }  //[nvarchar](1) NULL,
    }
    //public DbSet<tbl_setting_rate> tbl_setting_rate { get; set; }
    public class tbl_setting_rate
    {
        [Key]
        public string setting_rate_id { get; set; }  //[nvarchar](50) NOT NULL,PRIMARY KEY
        public DateTime? setting_rate_date { get; set; }  //[datetime] NULL,
        public double? setting_rate { get; set; }  //[float] NULL,
        public int? setting_rate_period_name { get; set; }  //[int] NULL,
        public int? setting_rate_year { get; set; }  //[int] NULL,
        public string? setting_rate_status { get; set; }  //[nvarchar](1) NULL,
        public string? setting_rate_desc { get; set; }  //[ntext] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](20) NULL,
    }
    //public DbSet<tbl_settings_gl_codes> tbl_settings_gl_codes { get; set; }
    public class tbl_settings_gl_codes
    {
       [Key]
        public int id { get; set; }  //[int] NOT NULL,
        public string? gl_code { get; set; }  //[nvarchar](50) NULL,
        public string? gl_type { get; set; }  //[nvarchar](1) NULL,
        public string? staff_type { get; set; }  //[nvarchar](1) NULL,
    }
    //public DbSet<tbl_tax_setting> tbl_tax_setting { get; set; }
    public class tbl_tax_setting
    {
        [Key]
        public short Id { get; set; }      //smallint 
        public decimal? single_amt { get; set; }  //[money] NULL,
        public decimal? married_amt { get; set; }  //[money] NULL,
        public double? first_tax_percent { get; set; }  //[float] NULL,
        public double? second_tax_percent { get; set; }  //[float] NULL,
        public bool is_used_initial_tax_percent { get; set; }  //[bit] NOT NULL,
        public decimal? initial_tax_percent { get; set; }  //[money] NULL,
        public double? first_tax_amount { get; set; }  //[float] NULL,
        public decimal? second_tax_amount { get; set; }  //[money] NULL,
        public decimal? third_tax_amount_single { get; set; }  //[money] NULL,
        public decimal? third_tax_amount_married { get; set; }  //[money] NULL,
        public double? third_tax_percent { get; set; }  //[float] NULL,
        public double? fourth_tax_percent { get; set; }  //[float] NULL,
        public double? single_female_ded_per { get; set; }  //[float] NULL,
        public double? max_medical_expenses_reimbursed { get; set; }  //[float] NULL,
        public double? max_medical_tax_credit_amount { get; set; }  //[float] NULL,
        public double? max_medical_tax_credit_per { get; set; }  //[float] NULL,
        public decimal? ins_amt { get; set; }  //[money] NULL,
        public decimal? ins_amt_non_life { get; set; }  //[money] NULL,
        public decimal? fourth_tax_amount { get; set; }  //[money] NULL,
        public double? fifth_tax_percent { get; set; }  //[float] NULL
    }
    //public DbSet<tbl_yearly_ins_amt> tbl_yearly_ins_amt { get; set; }
    public class tbl_yearly_ins_amt
    {
        //may be not in use/ depricate later
        public decimal? ins_amt { get; set; }  //[money] NULL,
        public decimal? max_ins_amt_rembursh { get; set; }  //[money] NULL
    }
    //public DbSet<tbl_yearly_working_hrs> tbl_yearly_working_hrs { get; set; }
    public class tbl_yearly_working_hrs
    {
        public double? hrs { get; set; }  //[float] NULL
    }
    //public DbSet<tbl_setting_dependent_children_details> tbl_setting_dependent_children_details { get; set; }
    public class tbl_setting_dependent_children_details
    {
        [Key]
        public int id { get; set; }                             //int not null primary key
        public int max_nos_dep_child_eligible_paid { get; set; }  //[int] NOT NULL,
        public decimal? max_amt_first_age_range { get; set; }  //[money] NULL,
        public decimal? max_amt_second_age_range { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
        public double? child_pro_rata_age { get; set; }  //[float] NULL,
        public double? emp_pro_rata_age { get; set; }  //[float] NULL

    }
    //public DbSet<tbl_setting_paycode_category> tbl_setting_paycode_category { get; set; }
    public class tbl_setting_paycode_category
    {
        [Key] 
        public string category_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? category_name { get; set; }  //[nvarchar](250) NULL,
        public string? category_name_abbr { get; set; }  //[nvarchar](250) NULL,
    }
    //public DbSet<tbl_setting_paycode_sub_category> tbl_setting_paycode_sub_category { get; set; }
    public class tbl_setting_paycode_sub_category
    {
        [Key]
        public string sub_category_id { get; set; }  //[nvarchar](50) NOT NULL,
        
        [ForeignKey(nameof(TblSettingPaycodeCategory))]
        public string? category_id { get; set; }  //[nvarchar](50) NULL,
        public tbl_setting_paycode_category TblSettingPaycodeCategory {get; set;} = null!;
        
        public string? sub_category_name { get; set; }  //[nvarchar](250) NULL,
        public string? sub_category_name_abbr { get; set; }  //[nvarchar](250) NULL,
        public string? sub_category_code { get; set; }  //[nvarchar](20) NULL,
        public string? sub_category_type { get; set; }  //[nvarchar](250) NULL,
        public string? staff_type { get; set; }  //[nvarchar](250) NULL,
        public string? amt_type { get; set; }  //[nvarchar](250) NULL,
        public string? p_category_id { get; set; }  //[nvarchar](50) NULL,
        public int? sort { get; set; }  //[int] NULL,
    }
    
    
    
    /*
     * DEPRICATED
     * 
     */
    /* 
    //public DbSet<tbl_pension_setting> tbl_pension_setting { get; set; }
    public class tbl_pension_setting
    {
        public short min_months_for_pension { get; set; }  //[smallint] NULL,
        public string? percent { get; set; }  //[float] NULL
    }
    */

}
