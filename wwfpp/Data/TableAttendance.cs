using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wwfpp.Data
{
    //public DbSet<tbl_employee_check_in_out_setting> tbl_employee_check_in_out_setting { get; set; }
    //Setting to hold information to send attendance list ot all staff for update
    public class tbl_employee_check_in_out_setting
    {
        [Key]
        public byte id { get; set; }  //[tinyint] NOT NULL,
        public string? send_staff_update { get; set; }  //[varchar{1) NULL,
        public string? send_hrs_min { get; set; }  //[varchar{5) NULL,
        public string? send_am_pm { get; set; }  //[varchar{2) NULL,
        
        //Upperjhata bida aailagyo bhane will be mentioned here so update will not be tiggered on those days
        public string? send_off_days { get; set; }  //[varchar{250) NULL, 
    }    
    //public DbSet<tbl_employee_check_in_out_staff_update> tbl_employee_check_in_out_staff_update { get; set; }
    public class tbl_employee_check_in_out_staff_update
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public DateTime? in_out_date { get; set; }  //[datetime] NULL,

        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation {get; set; } = null!;

        [ForeignKey(nameof(TblEmployee))]
        public int? by_emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee {  get; set; } = null!;

        public DateTime? submit_date_time { get; set; }  //[datetime] NULL,

    }
    //public DbSet<tbl_employee_check_in_out_main> tbl_employee_check_in_out_main { get; set; }
    public class tbl_employee_check_in_out_main
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,
        
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee {  get; set; }=null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? office_in { get; set; }  //[varchar{20) NOT NULL,
        public string? office_in_at { get; set; }  //[varchar{20) NOT NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,
        public string? office_out_at { get; set; }  //[varchar{20) NULL,
        public string? office_out { get; set; }  //[varchar{20) NULL,
        public string? remarks { get; set; }  //[varchar{100) NULL,
        
        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation {get; set; } = null!;

        public string? day_type { get; set; }  //[varchar{1) NULL,
        public string? narration { get; set; }  //[nvarchar](550) NULL,

    }
    //public DbSet<tbl_employee_check_in_out_sub> tbl_employee_check_in_out_sub { get; set; }
    public class tbl_employee_check_in_out_sub
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,

        [ForeignKey("TblUserGuardIn")]
        public int? in_guard_user_id { get; set; }  //[int] NULL,
        public tbl_user_guard TblUserGuardIn { get; set; } = null!;

        [ForeignKey("TblUserGuardOut")]
        public int? out_guard_user_id { get; set; }  //[int] NULL,
        public tbl_user_guard TblUserGuardOut { get; set; } = null!;

        [ForeignKey("TblDutyStation")]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation { get; set; } = null!;

        public string? remarks { get; set; }  //[varchar{100) NULL,

    }
    //public DbSet<tbl_employee_check_in_out_change_log> tbl_employee_check_in_out_change_log { get; set; }
    public class tbl_employee_check_in_out_change_log
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,

        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? old_value { get; set; }  //[varchar{200) NULL,
        public string? new_value { get; set; }  //[varchar{200) NULL,

        [ForeignKey(nameof(TblEmployeeChangeBy))]
        public int? by_emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeChangeBy { get; set; } = null!;

        public DateTime? change_date { get; set; }  //[datetime] NULL,
        public string? change_on { get; set; }  //[varchar{5) NULL,
        public string? change_type { get; set; }  //[varchar{20) NULL,
        public string? reason { get; set; }  //[ntext] NULL,
    }

    /*OUTSIDE EMPLOYEEES */

    //public DbSet<tbl_employee_check_in_out_main_outside> tbl_employee_check_in_out_main_outside { get; set; }
    public class tbl_employee_check_in_out_main_outside
    {
        [Key]
        public string id { get; set; }  //[varchar{50) NOT NULL,

        [ForeignKey(nameof(TblEmployeeOutside))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee_outside TblEmployeeOutside { get; set; } = null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? office_in { get; set; }  //[varchar{20) NOT NULL,
        public string? office_in_at { get; set; }  //[varchar{20) NOT NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,
        public string? office_out_at { get; set; }  //[varchar{20) NULL,
        public string? office_out { get; set; }  //[varchar{20) NULL,
        public string? remarks { get; set; }  //[varchar{100) NULL,

        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation {get; set; } = null!;

        public string? day_type { get; set; }  //[varchar{1) NULL,
        public string? narration { get; set; }  //[varchar{100) NULL,
    }
    //public DbSet<tbl_employee_check_in_out_sub_outside> tbl_employee_check_in_out_sub_outside { get; set; }
    public class tbl_employee_check_in_out_sub_outside
    {
        [Key]
        public string? id { get; set; }  //[varchar{50) NOT NULL,

        [ForeignKey(nameof(TblEmployeeOutside))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee_outside TblEmployeeOutside { get; set; } = null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? check_in { get; set; }  //[varchar{20) NOT NULL,
        public string? check_out { get; set; }  //[varchar{20) NULL,

        [ForeignKey("TblUserGuardIn")]
        public int? in_guard_user_id { get; set; }  //[int] NULL,
        public tbl_user_guard TblUserGuardIn { get; set; } = null!;

        [ForeignKey("TblUserGuardOut")]
        public int? out_guard_user_id { get; set; }  //[int] NULL,
        public tbl_user_guard TblUserGuardOut { get; set; } = null!;        

        [ForeignKey(nameof(TblDutyStation))]
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public tbl_duty_station TblDutyStation { get; set; } = null!;

        public string? remarks { get; set; }  //[varchar{100) NULL,
    }    
    //public DbSet<tbl_employee_check_in_out_change_log_outside> tbl_employee_check_in_out_change_log_outside { get; set; }
    public class tbl_employee_check_in_out_change_log_outside
    {
        public string id { get; set; }  //[varchar{50) NOT NULL,
        
        [ForeignKey(nameof(TblEmployeeOutside))]
        public int? emp_id { get; set; }  //[int] NULL,
        public tbl_employee_outside TblEmployeeOutside { get; set; } = null!;

        public DateTime? in_out_date { get; set; }  //[datetime] NULL,
        public string? old_value { get; set; }  //[varchar{200) NULL,
        public string? new_value { get; set; }  //[varchar{200) NULL,
        
        [ForeignKey(nameof(TblEmployeeChangeBy))]
        public int? by_emp_id { get; set; }  //[int] NULL,
        public tbl_employee TblEmployeeChangeBy { get; set; } = null!;

        public DateTime? change_date { get; set; }  //[datetime] NULL,
        public string? change_on { get; set; }  //[varchar{5) NULL,
        public string? change_type { get; set; }  //[varchar{20) NULL,
        public string? reason { get; set; }  //[ntext] NULL,

    }
      
    /*OWNER INFORMATION ON ATTENDANCE SOFTEATE */
    public class tbl_owner
        {
        [Key]
        public string pk_owner_id { get; set; }  //[varchar{20) NOT NULL,
        public string owner_name { get; set; }  //[varchar{250) NOT NULL,
        public string? address { get; set; }  //[varchar{250) NULL,
        public string? contact_person { get; set; }  //[varchar{100) NULL,
        public string? phone { get; set; }  //[varchar{50) NULL,
        public string? fax { get; set; }  //[varchar{50) NULL,
        public string? mobile { get; set; }  //[varchar{50) NULL,
        public string? e_mail { get; set; }  //[varchar{100) NULL,
        public string? website { get; set; }  //[varchar{250) NULL,
        public string? logo { get; set; }  //[varchar{250) NULL,
        public string? PAN { get; set; }  //[varchar{50) NULL,
        public string? created_date { get; set; }  //[datetime] NULL
    }
}
