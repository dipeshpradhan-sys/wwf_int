using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace wwfpp.Data
{
    public class tbl_employee
    {
        [Key]
        public int emp_id { get; set; }  //[int] NOT NULL,
        public string? emp_code { get; set; }  //[nvarchar](6) NULL,
        public string? title { get; set; }  //[nvarchar](20) NULL,
        public string? firstname { get; set; }  //[nvarchar](30) NULL,
        public string? middlename { get; set; }  //[nvarchar](30) NULL,
        public string? lastname { get; set; }  //[nvarchar](30) NULL,
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public string? address1 { get; set; }  //[nvarchar](255) NULL,
        public string? address2 { get; set; }  //[nvarchar](255) NULL,
        public string? city { get; set; }  //[nvarchar](50) NULL,
        public string? state { get; set; }  //[nvarchar](50) NULL,
        public string? nationality { get; set; }  //[nvarchar](50) NULL,
        public string? postalcode { get; set; }  //[nvarchar](20) NULL,
        public string? phone1 { get; set; }  //[nvarchar](15) NULL,
        public string? phone2 { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? e_mail { get; set; }  //[nvarchar](50) NULL,
        public string? personal_email { get; set; }  //[nvarchar](50) NULL,
        public string? citizenship_number { get; set; }  //[nvarchar](20) NULL,
        public string? citizenship_copy { get; set; }  //[nvarchar](50) NULL,
        public string? passport_number { get; set; }  //[nvarchar](20) NULL,
        public string? passport_copy { get; set; }  //[nvarchar](50) NULL,
        public string? marital_status { get; set; }  //[nvarchar](1) NULL,
        public int? no_of_children { get; set; }  //[int] NULL,
        public string? dependent_details { get; set; }  //[ntext] NULL,
        public string? blood_group { get; set; }  //[nvarchar](5) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? employee_type { get; set; }  //[nvarchar](15) NULL,
        public string? department { get; set; }  //[nvarchar](50) NULL,
        public string? post { get; set; }  //[nvarchar](50) NULL,
        public decimal? salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? child_edu_all { get; set; }  //[money] NULL,
        public string? account_no { get; set; }  //[nvarchar](15) NULL,
        public string? pf_no { get; set; }  //[nvarchar](15) NULL,
        public string? cit_no { get; set; }  //[nvarchar](15) NULL,

        public int? manager_id { get; set; }  //[int] NULL,
        //[ForeignKey(nameof(manager_id))]
        //public tbl_employee? TblEmployeeManager { get; set; }   // navigation to manager

        public string? emp_status { get; set; }  //[nvarchar](1) NULL, /* A = Active | D = Passive*/
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public DateTime? dob { get; set; }  //[datetime] NULL,
        public decimal? remote_area_allow { get; set; }  //[money] NULL,
        public string? pan_no { get; set; }  //[nvarchar](20) NULL,
        public decimal? yearly_remote_exem { get; set; }  //[money] NULL,
        public string? marital_status_info { get; set; }  //[nvarchar](1) NULL,
        public string? emp_pay_status { get; set; }  //[nvarchar](1) NULL,
        public string? emp_level { get; set; }  //[nvarchar](255) NULL,
        public string? job_family { get; set; }  //[nvarchar](255) NULL,

        public int? line_manager_id { get; set; }  //[int] NULL,
        //[ForeignKey(nameof(line_manager_id))]
        //public tbl_employee? TblEmployeeLineManager { get; set; }   // navigation to manager

        public int? alt_manager_id { get; set; }  //[int] NULL,
        //[ForeignKey(nameof(line_manager_id))]
        //public tbl_employee? TblEmployeeAltManager { get; set; }   // navigation to manager

        public int? alt_line_manager_id { get; set; }  //[int] NULL,
        //[ForeignKey(nameof(alt_line_manager_id))]
        //public tbl_employee? TblEmployeeAltLineManager { get; set; }   // navigation to manager

        public string? employee_type_sub { get; set; }  //[nvarchar](20) NULL,
        public string? ethnicity { get; set; }  //[nvarchar](250) NULL,
        public double? work_percent { get; set; }  //[float] NULL,
        public string? nin_no { get; set; }  //[nvarchar](20) NULL,
        public string? pan_copy { get; set; }  //[nvarchar](50) NULL,
        public string? nin_copy { get; set; }  //[nvarchar](50) NULL,

        /******************************************************************************
        * ICollection
        * if not defined => works fine, but you’ll always query child tables manually.
        * if defined => cleaner navigation, easier.Include() queries, better readability.
        **********************************************************************************/
        // Reverse navigation: employees managed by this employee
        //public ICollection<tbl_employee> Manager { get; set; } = new List<tbl_employee>();
        //public ICollection<tbl_employee> LineManger { get; set; } = new List<tbl_employee>();
        //public ICollection<tbl_employee> AltManager { get; set; } = new List<tbl_employee>();
        //public ICollection<tbl_employee> AltLineManager { get; set; } = new List<tbl_employee>();


        //One to Many
        public ICollection<tbl_employee_photo> tbl_employee_photo { get; set; } = new List<tbl_employee_photo>();
        public ICollection<tbl_employee_contract> tbl_employee_contract { get; set; } = new List<tbl_employee_contract>();//for Foreign key maintain
        public ICollection<tbl_user> tbl_user { get; set; } = new List<tbl_user>();//for Foreign key maintain
        public ICollection<tbl_employee_overtime_request> TblEmployeeOvertimeRequest { get; set; } = new List<tbl_employee_overtime_request>();

        // Icollection for One-to-one
        public tbl_employee_salary_extra_settings? tblEmployeeSalaryExtraSettings { get; set; }
        public tbl_employee_overtime_settings? TblEmployeeOvertimeSettings { get; set; }
    }

    //public DbSet<tbl_employee_photo> tbl_employee_photo { get; set; }
    public class tbl_employee_photo
    {
        [Key]
        public string id { get; set; }          //[varchar] (50) NOT NULL PRIMARY KEY,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }                     //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;  

        public string? photo { get; set; }          //[varchar] (50) NULL
    }
    //public DbSet<tbl_employee_contract> tbl_employee_contract { get; set; }
    public class tbl_employee_contract
    {
        [Key]
        public int emp_contract_id { get; set; }              // [int] NOT NULL (PK)

        [ForeignKey(nameof(TblContractDocumentTemplate))]
        public int? contract_document_id { get; set; }        // [int] NULL
        public tbl_contract_document_template TblContractDocumentTemplate { get; set; } = null!;

        public string? contract_desc { get; set; }           // [ntext] NULL
        public DateTime? issue_date { get; set; }            // [datetime] NULL
        public DateTime? end_date { get; set; }              // [datetime] NULL

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }                     // [int] NULL
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? contract_status { get; set; }        //nvarchar 1
    }
    //public DbSet<tbl_employee_signed_contract> tbl_employee_signed_contract { get; set; }
    public class tbl_employee_signed_contract
    {
        [Key]
        public int emp_signed_contract_id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblContractDocumentTemplate))]
        public int? contract_document_id { get; set; }  //[int] NULL,
        public tbl_contract_document_template TblContractDocumentTemplate { get; set; } = null!;

        public string? signed_contract { get; set; }  //[nvarchar](50) NULL,
        public DateTime? upload_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;
    }
    //public DbSet<tbl_employee_address> tbl_employee_address { get; set; }
    public class tbl_employee_address
    {
        [Key]
        public int emp_id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(emp_id))]
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? address1 { get; set; }  //[nvarchar](255) NULL,
        public string? address2 { get; set; }  //[nvarchar](255) NULL,
        public string? city { get; set; }  //[nvarchar](50) NULL,
        public string? state { get; set; }  //[nvarchar](50) NULL,
        public string? country { get; set; }  //[nvarchar](50) NULL,
        public string? postalcode { get; set; }  //[nvarchar](20) NULL,
        public string? phone1 { get; set; }  //[nvarchar](15) NULL,
        public string? phone2 { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? personal_email { get; set; }  //[nvarchar](50) NULL,
        public string? skype { get; set; }  //[nvarchar](250) NULL,
    }
    //public DbSet<tbl_employee_document> tbl_employee_document { get; set; }
    public class tbl_employee_document
    {
        [Key]
        public string document_id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }  //[int] NOT NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        [ForeignKey(nameof(TblDocumentType))]
        public int? document_type_id { get; set; }  //[int] NOT NULL,
        public tbl_document_type TblDocumentType { get; set; } = null!;

        public string? document_number { get; set; }  //[nvarchar](50) NULL,
        public string? document_copy { get; set; }  //[nvarchar](50) NULL,
    }
    //public DbSet<tbl_employee_education> tbl_employee_education { get; set; }
    public class tbl_employee_education
        {
            [Key]
            public int emp_edu_id { get; set; }  //[int] NOT NULL,
            public string? slc_board { get; set; }  //[nvarchar](100) NULL,
            public string? slc_passed_year { get; set; }  //[nvarchar](4) NULL,
            public string? slc_division { get; set; }  //[nvarchar](20) NULL,
            public string? slc_major { get; set; }  //[nvarchar](50) NULL,
            public string? bch_board { get; set; }  //[nvarchar](100) NULL,
            public string? bch_passed_year { get; set; }  //[nvarchar](4) NULL,
            public string? bch_division { get; set; }  //[nvarchar](20) NULL,
            public string? bch_major { get; set; }  //[nvarchar](50) NULL,
            public string? hgt_board { get; set; }  //[nvarchar](100) NULL,
            public string? hgt_passed_year { get; set; }  //[nvarchar](4) NULL,
            public string? hgt_division { get; set; }  //[nvarchar](20) NULL,
            public string? hgt_major { get; set; }  //[nvarchar](50) NULL,
            public string? remarks { get; set; }  //[ntext] NULL,
            
            [ForeignKey(nameof(TblEmployee))]
            public int? emp_id { get; set; }  //[int] NULL,
            public tbl_employee TblEmployee { get; set; } = null!;

        }
    //public DbSet<tbl_employee_fund_source> tbl_employee_fund_source { get; set; }
    public class tbl_employee_fund_source
    {
        [Key]
        public int emp_fund_id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblFundSource))]
        public int fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get; set; } = null!;

        public double? annual_hrs { get; set; }  //[float] NULL,
        public DateTime? start_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
    }
    //public DbSet<tbl_employee_fund_source_hash> tbl_employee_fund_source_hash { get; set; }
    public class tbl_employee_fund_source_hash
    {
        [Key]
        public int id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }  //[int] NULL,
        public tbl_user TblUser { get;  set; } = null!;

        public string? emp_code { get; set; }  //[nvarchar](6) NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        [ForeignKey(nameof(TblFundSource))]
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get; set; } = null!;

        public string? fund_source { get; set; }  //[nvarchar](50) NULL,
        public double? annual_hrs { get; set; }  //[float] NULL,
        public DateTime? start_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
    }
    //public DbSet<tbl_employee_history> tbl_employee_history { get; set; }
    public class tbl_employee_history
    {
        [Key]
        public int id { get; set; }  //[int] NOT NULL //identity,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }  //[int] NOT NULL,
        public tbl_employee TblEmployee { get; set; }=null!;

        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? employee_type { get; set; }  //[nvarchar](15) NULL,
        public string? department { get; set; }  //[nvarchar](50) NULL,
        public string? post { get; set; }  //[nvarchar](50) NULL,
        public decimal? salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? child_edu_all { get; set; }  //[money] NULL,
        public string? emp_status { get; set; }  //[nvarchar](1) NULL, /*A = Active, D = Inactive*/
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public decimal? remote_area_allow { get; set; }  //[money] NULL,
        public decimal? yearly_remote_exem { get; set; }  //[money] NULL,

        [ForeignKey(nameof(TblEmployeeSaveBy))]
        public int? by_emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeSaveBy { get; set; } = null!;

        public string? job_family { get; set; }  //[nvarchar](255) NULL,
        public string? emp_level { get; set; }  //[nvarchar](255) NULL,

        [ForeignKey(nameof(TblEmployeeImm))]
        public int? manager_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeImm { get; set; } = null!;

        [ForeignKey(nameof(TblEmployeeLine))]
        public int? line_manager_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeLine { get; set; } = null!;

        public string? marital_status { get; set; }  //[nvarchar](1) NULL,
        public int? no_of_children { get; set; }  //[int] NULL
    }
    //public DbSet<tbl_employee_insurance> tbl_employee_insurance { get; set; }
    public class tbl_employee_insurance
    {
        [Key]
        public int emp_ins_id { get; set; }  //[int] NOT NULL,
        public string? ins_company { get; set; }  //[nvarchar](100) NULL,
        public string? ins_type { get; set; }  //[nvarchar](25) NULL,
        public DateTime? ins_valid_date { get; set; }  //[datetime] NULL,
        public string? policy_no { get; set; }  //[nvarchar](20) NULL,
        public decimal? ins_amount { get; set; }  //[money] NULL,
        public decimal? premium_amount { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;
    }
    //public DbSet<tbl_employee_signature> tbl_employee_signature { get; set; }
    public class tbl_employee_signature
    {
        [Key]
        public int emp_sign_id { get; set; }  //[int] NOT NULL,
        public string? signature { get; set; }  //[nvarchar](250) NULL,
        public DateTime? upload_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;
    }

    public class GetEmployeeFundSourceDetail
    {
        [Key]
        public int emp_id { get; set; }                 // int
        public string? firstnmae { get; set; } // nvarchar
        public string? middlename{ get; set; } // nvarchar
        public string? lastname { get; set; } // nvarchar
        public string? emp_code { get; set; } // nvarchar
        public string? fiscal_year { get; set; }          // varchar
        public DateTime? FiscalStartDate { get; set; }   // datetime, nullable
        public DateTime? FiscalEndDate { get; set; }     // datetime, nullable

        public int emp_fund_id { get; set; }             // int
        public int fund_id { get; set; }                 // int

        // float → double?, nullable
        public double? AssignedAnnualHoursForFund { get; set; }
        public double? AssignedAnnualDaysForFund { get; set; }
        public double? AssignedAnnualPercentageForFund { get; set; }

        public DateTime? start_date { get; set; }        // datetime, nullable
        public DateTime? end_date { get; set; }          // datetime, nullable
        public string? fund_source { get; set; }          // nvarchar
        public string? fund_desc { get; set; }            // nvarchar
        public DateTime? expiry_date { get; set; }       // datetime, nullable

        // float → double?, nullable
        public double? NormalTimeUsedAnnualHours { get; set; }
        public double? NormalTimeUsedAnnualDays { get; set; }
        public double? NormalTimePendingHours { get; set; }
        public double? NormalTimePendingDays { get; set; }
        public double? OverTimeUsedAnnualHours { get; set; }
        public double? OverTimeUsedAnnualDays { get; set; }
        public double? OverTimePendingHours { get; set; }
        public double? OverTimePendingDays { get; set; }
    }

    //Dependent

    //public DbSet<tbl_employee_dependent_children_details> tbl_employee_dependent_children_details { get; set; }
    public class tbl_employee_dependent_children_details
    {
        [Key]
        public int emp_dep_id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }  //[int] NOT NULL,
        public tbl_employee TblEmployee { get; set;} = null!;

        public string? c_name { get; set; }  //[nvarchar](255) NULL,
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public DateTime? date_of_birth { get; set; }  //[datetime] NULL,
        public string? dob_file_name { get; set; }  //[nvarchar](255) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
        public string? eligibility { get; set; }  //[nvarchar](1) NULL,
        public string? remarks { get; set; }  //[nvarchar](255) NULL,
    }

    //public DbSet<tbl_employee_dependent_children_details_allowance_final> tbl_employee_dependent_children_details_allowance_final { get; set; }
    public class tbl_employee_dependent_children_details_allowance_final
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? fiscal_year { get; set; }  //[nvarchar](20) NULL,

        [ForeignKey(nameof(TblEmployeeDependentChildrenDetails))]
        public int? emp_dep_id { get; set; }  //[int] NULL,
        public tbl_employee_dependent_children_details TblEmployeeDependentChildrenDetails { get; set; } = null!;
        
        public decimal? amount_actual { get; set; }  //[money] NULL,
        public decimal? amount_paid { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
        public double? dependant_age { get; set; }  //[float] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_dependent_children_details_sub> tbl_employee_dependent_children_details_sub { get; set; }
    public class tbl_employee_dependent_children_details_sub
    {
        [Key]
        public int emp_dep_sub_id { get; set; }  //[int] NOT NULL,

        [ForeignKey(nameof(TblEmployeeDependentChildrenDetails))]
        public int emp_dep_id { get; set; }  //[int] NOT NULL,
        public tbl_employee_dependent_children_details TblEmployeeDependentChildrenDetails { get; set; } = null!; 
        
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public string? file_name { get; set; }  //[nvarchar](255) NULL,
        public string? status { get; set; }  //[nvarchar](1) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
    }
    //public DbSet<tbl_dependent_children_details_allowance> tbl_dependent_children_details_allowance { get; set; }
    public class tbl_dependent_children_details_allowance
    {
        [Key]
        public string dep_allow_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        
        [ForeignKey(nameof(TblEmployeeDependentChildrenDetails))]
        public int? emp_dep_id { get; set; }  //[int] NULL,
        public tbl_employee_dependent_children_details TblEmployeeDependentChildrenDetails { get; set; } = null!; 

        public decimal? amount_actual { get; set; }  //[money] NULL,
        public decimal? amount_paid { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
    }
    //public DbSet<tbl_dependent_children_details_allowance_emp_wise> tbl_dependent_children_details_allowance_emp_wise { get; set; }
    public class tbl_dependent_children_details_allowance_emp_wise
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public decimal? amount_paid { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_dependent_children_details_allowance_fund_wise> tbl_dependent_children_details_allowance_fund_wise { get; set; }
    public class tbl_dependent_children_details_allowance_fund_wise
    {
        [Key]
        public string id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        [ForeignKey(nameof(TblFundSource))]
        public int? fund_id { get; set; }  //[int] NULL,
        public tbl_fund_source TblFundSource { get; set; } = null!;

        public double? hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }
    //public DbSet<tbl_employee_outside> tbl_employee_outside { get; set; }
    public class tbl_employee_outside
    {
        [Key]
        public int emp_id { get; set; }  //[int] NOT NULL,
        public string? emp_code { get; set; }  //[nvarchar](6) NULL,
        public string? title { get; set; }  //[nvarchar](20) NULL,
        public string? firstname { get; set; }  //[nvarchar](30) NULL,
        public string? middlename { get; set; }  //[nvarchar](30) NULL,
        public string? lastname { get; set; }  //[nvarchar](30) NULL,
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public DateTime? dob { get; set; }  //[datetime] NULL,
        public string? address { get; set; }  //[nvarchar](255) NULL,
        public string? phone { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? e_mail { get; set; }  //[nvarchar](50) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? emp_status { get; set; }  //[nvarchar](1) NULL,
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public string? pan_no { get; set; }  //[nvarchar](20) NULL,

        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation { get; set; } = null!;

        public string? photo { get; set; }  //[nvarchar](200) NULL,
    }











    /*
     * DEPRICATED
     * 
     */
    /* 
    //public DbSet<tbl_employee_pension> tbl_employee_pension { get; set; }
    public class tbl_employee_pension
    {
        [Key]
        public int pension_id {get;set;}  //[int] NOT NULL,
        public short? pen_year {get;set;}  //[smallint] NULL,
        public short? pen_month {get;set;}  //[smallint] NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id {get;set;}  //[int] NULL,
        public tbl_employee tbl_employee {get; set;} = null!;

        public decimal? amt_nrs {get;set;}  //[money] NULL,
        public decimal? rate_usd {get;set;}  //[money] NULL,
        public decimal? amt_usd {get;set;}  //[money] NULL,
        public DateTime? submit_date {get;set;}  //[datetime] NULL,

        [ForeignKey(nameof(TblEmployee))]    
        public int? submit_by {get;set;}  //[int] NULL,
        public tbl_employee tbl_employee_sb {get; set;} = null!;

        public string? pen_fiscal_year {get;set;}  //[nvarchar](10) NULL,
        public byte? pen_emp_week {get;set;}  //[tinyint] NULL,
    } 


    */

}
