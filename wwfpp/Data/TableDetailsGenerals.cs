using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace wwfpp.Data
{
    //public DbSet<tbl_contract_document_template> tbl_contract_document_template { get; set; }
    public class tbl_contract_document_template
    {
        [Key]
        public int contract_document_id { get; set; }
        public string document_subject { get; set; } //nvarchar 255
        public string document_desc { get; set; } //ntext

        //ICollection from child tables 

    }
    //public DbSet<tbl_document_templates> tbl_document_templates { get; set; }
    public class tbl_document_templates
    {
        [Key]
        public string id { get; set; }     //nvarchar 50 NOT NULL
        public string? document_title { get; set; } // nvarchar 250
        public string? document_version { get; set; } //nvarchar 250 
        public string? document_desc { get; set; } //ntext
        public string? upload_file { get; set; }    //nvarchar 250
        public DateTime? upload_date { get; set; }  //
        public string? status { get; set; }// varchar(1)

        //ICollection from child tables?
    }
    //public DbSet<tbl_fund_source> tbl_fund_source { get; set; }
    public class tbl_fund_source
    {
        [Key]
        public int fund_id { get; set; }            //[int] NOT NULL,
        public string? fund_source { get; set; }     //[nvarchar] (50) NULL,
        public string? fund_desc { get; set; }       //[nvarchar] (255) NULL,
        public string? fund_status { get; set; }     //[nvarchar] (1) NULL,
        public DateTime? expiry_date { get; set; }   //[datetime] NULL,
        public string? default_for_holiday { get; set; } //[nvarchar] (1) NULL,

        //ICollection from child tables
    }
    //public DbSet<tbl_duty_station> tbl_duty_station { get; set; }
    public class tbl_duty_station
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public string duty_station { get; set; }  //[varchar{50) NOT NULL,
        public string remarks { get; set; }  //[varchar{100) NOT NULL,
        public string? is_active { get; set; }  //[varchar{1) NULL,

        //ICollection from child tables?
    }
    //public DbSet<tbl_alert_execute_date> tbl_alert_execute_date { get; set; }
    public class tbl_alert_execute_date
    {
        [Key]
        public string id { get; set; } //nvarchar(50) NOT NULL,
        public DateTime last_alert_execute_date { get; set; }
        public DateTime last_alert_settlement_date { get; set; }
        public DateTime last_alert_timesheet_date { get; set; }
        public DateTime last_alert_birthday_date { get; set; }
    }
    //public DbSet<tbl_conflict_fraud_format> tbl_conflict_fraud_format { get; set; }
    public class tbl_conflict_fraud_format
    {
        //format will taken fiscal_year+format_type
        [Key]
        public int format_id { get; set; }   //[int] NOT NULL

        [ForeignKey(nameof(TblFiscalYear))]
        public string fiscal_year { get; set; } //[nvarchar] (9) NULL
        public tbl_fiscal_year TblFiscalYear { get; set; } = null!;

        public string lang { get; set; }//[nvarchar] (2) NULL
        public string format_type { get; set; }//[nvarchar] (1) NULL
        public string format_desc { get; set; } //[ntext] NULL

        //ICollection from child tables?
    }
    //public DbSet<tbl_fraud_corruption_sign> tbl_fraud_corruption_sign { get; set; }
    public class tbl_fraud_corruption_sign
    {
        [Key]
        public string sign_id { get; set; }      // [nvarchar] (50) NOT NULL,

        [ForeignKey(nameof(TblFiscalYear))]
        public string fiscal_year { get; set; }     //[nvarchar] (9) NULL,
        public tbl_fiscal_year TblFiscalYear { get; set; } = null!;

        [ForeignKey(nameof(TblEmployee))]
        public int emp_id { get; set; }     //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public string? position_title { get; set; }   //[nvarchar] (250) NULL,
        public DateTime? submit_date { get; set; }   //[datetime] NULL,
        public string? dept { get; set; }            //[nvarchar] (250) NULL,
    }
    //public DbSet<tbl_conflict_sign> tbl_conflict_sign { get; set; }
    public class tbl_conflict_sign
    {
        //format will taken fiscal_year+format_type
        [Key] 
        public string sign_id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblFiscalYear))]
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public tbl_fiscal_year TblFiscalYear { get; set; } = null!;

        public string no_conflict { get; set; }  //[nvarchar](1) NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; }=null!;
        public string? position_title { get; set; }  //[nvarchar](250) NULL,
        public string? submit_date { get; set; }  //[datetime] NULL,
        public string? dept { get; set; }  //[nvarchar](250) NULL,
    }
    //public DbSet<tbl_conflict_sign_sub> tbl_conflict_sign_sub { get; set; }
    public class tbl_conflict_sign_sub
    {
        [Key]
        public string sub_sign_id { get; set; }  //[nvarchar](50) NOT NULL,

        [ForeignKey(nameof(TblConflictSign))]
        public string sign_id { get; set; }  //[nvarchar](50) NULL,
        public tbl_conflict_sign TblConflictSign { get; set; } = null!;

        public int SN { get; set; }  //[int] NULL,
        public string? conflict { get; set; }  //[nvarchar](250) NULL,
        public string? reason { get; set; }  //[nvarchar](250) NULL,
    }
    //public DbSet<tbl_education_level> tbl_education_level { get; set; }
    public class tbl_education_level
    {
        [Key]
        public int education_level_id { get; set; }  //[int] NOT NULL,
        public string? education_level_title { get; set; }  //[nvarchar](100) NULL,

        //ICollection
    }
    //public DbSet<tbl_document_type> tbl_document_type { get; set; }
    public class tbl_document_type
    {
        [Key]
        public int document_type_id { get; set; }  //[int] NOT NULL,
        public string? document_title { get; set; }  //[nvarchar](100) NULL,

        //ICollection
    }
    //public DbSet<tbl_expenditure_category> tbl_expenditure_category { get; set; }
    public class tbl_expenditure_category
    {
        [Key]
        public int expd_id { get; set; }  //[int] NOT NULL,
        public string? expd_category { get; set; }  //[nvarchar](100) NULL,

        //ICollection
    }
    //public DbSet<tbl_expenditure_type> tbl_expenditure_type { get; set; }
    public class tbl_expenditure_type
    {
        [Key]
        public int expd_type_id { get; set; }  //[int] NOT NULL,
        public string expd_type { get; set; }  //[nvarchar](100) NULL,
        public string expd_description { get; set; }  //[nvarchar](250) NULL,

        [ForeignKey(nameof(TblExpenditureCategory))]
        public int expd_id { get; set; }  //[int] NULL,
        public tbl_expenditure_category TblExpenditureCategory { get; set; } = null!;

        public string measure_unit { get; set; }  //[nvarchar](50) NULL,
        public string gl_code { get; set; }  //[nvarchar](10) NULL,
        public string interco { get; set; }  //[nvarchar](10) NULL,
    }
    //public DbSet<tbl_task> tbl_task { get; set; }
    public class tbl_task
    {
        [Key]
        public int task_id { get; set; }  //[int] NOT NULL,
        public string? task_number { get; set; }  //[nvarchar](10) NULL,
        public string? task_description { get; set; }  //[nvarchar](250) NULL,
    }
    //public DbSet<tbl_currency> tbl_currency { get; set; }
    public class tbl_currency
    {
        [Key]
        public byte cur_id { get; set; }  //[tinyint] NOT NULL,
        public string? cur_abbr { get; set; }  //[nvarchar](20) NULL,
        public string? cur_name { get; set; }  //[nvarchar](50) NULL,

    }
}
