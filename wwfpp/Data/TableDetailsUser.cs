using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
namespace wwfpp.Data
{
    /* 
     * User Administration 
     * Most of the tables related to users will be defined in this page
     * Note : here we have not defined the Primery keys. using EF Core 
     * (which is the modern, cross-platform version), you need to use 
     * the Fluent API approach.so check AppDbContext > OnModelCreating
     * for defined keys
     */

    //public DbSet<tbl_user_module> tbl_user_module { get; set; }
    public class tbl_user_module
    {
        [Key]
        public int module_id { get; set; }      //INT NOT NULL PRIMARY KEY,
        public string? module_code { get; set; }    //VARCHAR(50) NULL, UNIQUE KEY
        public string? module_name { get; set; }    //VARCHAR(50) NULL,
        public string? module_label { get; set; }   //NVARCHAR(250) NULL,
        public string? module_folder { get; set; } //VARCHAR(250) NULL,
        public int? module_sort { get; set; }       //INT NULL,    
        public string? module_status { get; set; }  //VARCHAR(1) /* A = Active P = Passive*/

        //This table's primery key is used in below tables as foreign key
        //public ICollection<tbl_user_menu> TblUserMenu { get; set; } = new List<tbl_user_menu>();
        //public ICollection<tbl_user_level_module> TblUserLevelModule { get; set; } = new List<tbl_user_level_module>();
        //public ICollection<tbl_user_user_module> TblUserUserModule { get; set; } = new List<tbl_user_user_module>();
    }
    public class UserModuleConfig : IEntityTypeConfiguration<tbl_user_module>
    {
        public void Configure(EntityTypeBuilder<tbl_user_module> builder)
        {
            builder.HasIndex(m => m.module_code).IsUnique(); // Module Unique Key
        }
    }
    //public DbSet<tbl_user_menu> tbl_user_menu { get; set; }
    public class tbl_user_menu
    {
        [Key]
        public required string menu_id { get; set; }  //VARCHAR(50) NOT NULL PRIMARY KEY,
        public required string menu_code { get; set; }        //VARCHAR (10)	NOT NULL,   UNIQUE KEY
        public string? menu_name { get; set; }        //VARCHAR(250) NULL,
        public string? menu_label { get; set; }      //NVARCHAR(250) NULL,   
        public string? menu_page { get; set; }      //VARCHAR(250) NULL,
        public int? menu_sort { get; set; }         //INT NULL,
        public string? menu_status { get; set; }    //VARCHAR(1), /* A = Active P = Passive*/
        [ForeignKey(nameof(TblUserModule))]
        public int? module_id { get; set; }
        public tbl_user_module TblUserModule { get; set; } = null!;

        //This table's primary key is used in below tables as foreign key
        //public ICollection<tbl_user_level_menu> tblUserLevelMenu { get; set; } = new List<tbl_user_level_menu>();
        //public ICollection<tbl_user_user_menu> tblUserUserMenu { get; set; } = new List<tbl_user_user_menu>();
    }
    public class UserMenuConfig : IEntityTypeConfiguration<tbl_user_menu>
    {
        public void Configure(EntityTypeBuilder<tbl_user_menu> builder)
        {
            builder.HasIndex(m => m.menu_code).IsUnique(); // Module Unique Key
        }
    }
    //public DbSet<tbl_user_level> tbl_user_level { get; set; }
    public class tbl_user_level
    {
        [Key]
        public string level_id { get; set; }   //VARCHAR(50) NOT NULL PRIMARY KEY,
        public string? level_name { get; set; } //VARCHAR(50) NOT NULL,
        public int? level_type { get; set; }    //INT NULL,
        public int? level_sort { get; set; }    //INT NULL,

        //This table's primery key is used in below tables as foreign key
        //public ICollection<tbl_user_level_module> tbl_user_level_module { get; set; } = new List<tbl_user_level_module>();
        //public ICollection<tbl_user_level_menu> tbl_user_level_menu { get; set; } = new List<tbl_user_level_menu>();
        //public ICollection<tbl_user_user_module> tbl_user_user_module { get; set; } = new List<tbl_user_user_module>();
        //public ICollection<tbl_user_user_menu> tbl_user_user_menu { get; set; } = new List<tbl_user_user_menu>();
        //public ICollection<tbl_user> tbl_user { get; set; } = new List<tbl_user>();
    }
    //public DbSet<tbl_user_level_module> tbl_user_level_module { get; set; }
    public class tbl_user_level_module
    {
        [Key]
        public required string Id { get; set; }          //VARCHAR(50) NOT NULL PRIMARY KEY,

        [ForeignKey(nameof(TblUserLevel))]
        public string? level_id { get; set; }   //VARCHAR(50) NOT NULL,
        public tbl_user_level TblUserLevel { get; set; } = null!;
        [ForeignKey(nameof(TblUserModule))]
        public int? module_id { get; set; } //INT NOT NULL
        public tbl_user_module TblUserModule { get; set; } = null!;
    }
    //public DbSet<tbl_user_level_menu> tbl_user_level_menu { get; set; }
    public class tbl_user_level_menu
    {
        [Key]
        public required string Id { get; set; }  //VARCHAR(50) NOT NULL PRIMARY KEY,

        [ForeignKey(nameof(TblUserLevel))]
        public string level_id { get; set; }       //VARCHAR(50) NOT NULL,
        public tbl_user_level TblUserLevel { get; set; } = null!;

        [ForeignKey(nameof(TblUserMenu))]
        public required string menu_id { get; set; }    //VARCHAR(50) NOT NULL,
        public tbl_user_menu TblUserMenu { get; set; } = null!;

        public string? is_vw { get; set; }  //VARCHAR(1) NOT NULL,
        public string? is_ad { get; set; }  //VARCHAR(1) NOT NULL,
        public string? is_ed { get; set; }  //VARCHAR(1) NOT NULL,
        public string? is_de { get; set; }  //VARCHAR(1) NOT NULL,
    }
    //public DbSet<tbl_user> tbl_user { get; set; }
    public class tbl_user
    {
        [Key]
        public int user_id { get; set; }        //[int] NOT NULL,
        public string? username { get; set; }   //[nvarchar](20) NULL,
        public string? pwd { get; set; }        //[nvarchar](255) NULL,

        [ForeignKey(nameof(TblUserLevel))]
        public string? level_id { get; set; }   //[varchar](50) NULL,
        public tbl_user_level TblUserLevel { get; set; } = null!; //primery table
        [ForeignKey(nameof(TblEmployee))]
        public int? emp_id { get; set; }        //[int] NULL,
        public tbl_employee TblEmployee { get; set; } = null!; // primary table

        public string? is_active { get; set; }          //[nvarchar](1) NULL,
        public string? pin { get; set; }                //[varchar](250) NULL,
        public int sign_in_type { get; set; }          //[int] NOT NULL,
        public string? activation_key { get; set; }     //[varchar](50) NULL,
        public DateTime? submit_date { get; set; }      //[datetime] NULL,

        //This table's primery key is used in below tables as foreign key
        //public ICollection<tbl_user_user_module> tbl_user_user_module { get; set; } = new List<tbl_user_user_module>(); //this from foreign table
        //public ICollection<tbl_user_user_menu> tbl_user_user_menu { get; set; } = new List<tbl_user_user_menu>();
        //public ICollection<tbl_user_login_log> tbl_user_login_log { get; set; } = new List<tbl_user_login_log>();
        //public ICollection<tbl_user_pwd_history> tbl_user_pwd_history { get; set; } = new List<tbl_user_pwd_history>();
        //public ICollection<tbl_user_reset_token> tbl_user_reset_token { get; set; } = new List<tbl_user_reset_token>();
    }
    //public DbSet<tbl_user_user_module> tbl_user_user_module { get; set; }
    public class tbl_user_user_module
    {
        [Key]
        public required string Id { get; set; }  //VARCHAR(50) NOT NULL PRIMARY KEY,

        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }   //INT NOT NULL,
        public tbl_user? TblUser { get; set; } = null!;

        [ForeignKey(nameof(TblUserModule))]
        public int module_id { get; set; } //INT NOT NULL
        public tbl_user_module? TblUserModule { get; set; } = null!;
    }
    //public DbSet<tbl_user_user_menu> tbl_user_user_menu { get; set; }
    public class tbl_user_user_menu
    {
        [Key]
        public required string Id { get; set; }     //VARCHAR(50) NOT NULL PRIMARY KEY,
        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }            //INT NOT NULL,
        public tbl_user TblUser { get; set; } = null!;
        [ForeignKey(nameof(TblUserMenu))]
        public string? menu_id { get; set; }            //VARCHAR(50) NOT NULL,
        public tbl_user_menu TblUserMenu { get; set; } = null!;
        public string? is_vw { get; set; } = string.Empty;  //VARCHAR(1) NOT NULL,
        public string? is_ad { get; set; } = string.Empty;  //VARCHAR(1) NOT NULL,
        public string? is_ed { get; set; } = string.Empty;  //VARCHAR(1) NOT NULL,
        public string? is_de { get; set; } = string.Empty;  //VARCHAR(1) NOT NULL,

        //public tbl_user TblUserRight { get; set; } = null!;
    }
    //public DbSet<tbl_user_login_fail> tbl_user_login_fail { get; set; }
    public class tbl_user_login_fail
    {
        [Key]
        public string Id { get; set; }      //VARCHAR(50) NOT NULL PRIMARY KEY,
        public required string username { get; set; }   //varchar(50) NOT NULL,
        public DateTime on_date { get; set; }  //datetime NOT NULL,
        public string ip { get; set; }     //varchar(255) NOT NULL,
        public string? user_agent { get; set; }     //varchar(255) NULL
    }
    //public DbSet<tbl_user_login_log> tbl_user_login_log { get; set; }
    public class tbl_user_login_log
    {
        [Key]
        public string ID { get; set; }      //[nvarchar](255) NOT NULL

        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }    //[int] NULL,			
        public tbl_user TblUser { get; set; } = null!; //point primary table
        public DateTime? in_date { get; set; }  //[datetime] NULL,
        public DateTime? out_date { get; set; } //[datetime] NULL,
        public string? ip { get; set; }     //[nvarchar](255) NULL,
        public string? user_agent { get; set; }     //varchar(255)  NULL
    }
    //public DbSet<tbl_user_pwd_history> tbl_user_pwd_history { get; set; }
    public class tbl_user_pwd_history
    {
        [Key]
        public string Id { get; set; }     //VARCHAR(50) NOT NULL PRIMARY KEY,

        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }       // INT NOT NULL,
        public tbl_user TblUser { get; set; } = null!;

        public string? pwd { get; set; }        //VARCHAR(255) NULL
        public DateTime? updated_date { get; set; }  //DateTime NULL,
        public string? is_current_one { get; set; } // VARCHAR(1) NULL
    }
    //public DbSet<tbl_user_reset_token> tbl_user_reset_token { get; set; }
    public class tbl_user_reset_token
    {
        [Key]
        public string Id { get; set; }      //[varchar] (50) NOT NULL PRIMARY KEY        
        [ForeignKey(nameof(TblUser))]
        public int user_id { get; set; }    //[int] NOT NULL
        public tbl_user TblUser { get; set; } = null!; // navigation        
        public string? token { get; set; }      //[varchar](100) NOT NULL
        public DateTime? expiry { get; set; }   //[datetime] NULL,
        public string? pwdorpin { get; set; }   //[varchar](5) NULL, /* PWD = password | PIN = pin*/
    }
    //public DbSet<tbl_user_guard> tbl_user_guard { get; set; }
    public class tbl_user_guard
    {
        [Key]
        public int pk_user_id { get; set; }         // [INT] NOT NULL ,
        public string? user_name { get; set; } = string.Empty;  //[VARCHAR] (20)  NULL ,
        public string? user_pass { get; set; } = string.Empty;  //[VARCHAR] (20)  NULL ,
        public string? full_name { get; set; } = string.Empty;  //[VARCHAR] (200)  NULL ,
        public string? is_active { get; set; } = string.Empty;  //[VARCHAR] (1) NULL ,
        public string? user_type { get; set; } = string.Empty;  //[VARCHAR] (50)  NULL

    }



    //public DbSet<que_user_log> que_user_log { get; set; }
    public class que_user_log
    {
        public string ID { get; set; }
        public DateTime? in_date { get; set; }
        public DateTime? out_date { get; set; }
        public string? ip { get; set; }
        public string? user_agent { get; set; }
        public string? username { get; set; }
        public string? fullname { get; set; }
        public string? level_name { get; set; }
    }
    /*
     * DEPRICATED
     * 
     */


    /* 
    //public DbSet<tbl_forms> tbl_forms { get; set; }
    public class tbl_forms
    {
        [Key]
        public int form_id { get; set; }  //[int] NOT NULL,
        public string? form_name { get; set; }  //[nvarchar](50) NULL,
        public string? page_name { get; set; }  //[nvarchar](50) NULL,
    }

    //public DbSet<tbl_user_forms> tbl_user_forms { get; set; }
    public class tbl_user_forms
    {
        public int? user_id {get;set;}  //[int] NULL,
        public int? form_id {get;set;}  //[int] NULL
    }
    //public DbSet<tbl_user_level_access> tbl_user_level_access { get; set; }
    public class tbl_user_level_access
    {
        public string? user_access_id {get;set;}        //[int] NULL,
        public string? user_menu_category {get;set;}    //[nvarchar](50) NULL,
        public string? user_menu_id {get;set;}          //[nvarchar](50) NULL,
        public string? user_menu {get;set;}             //[nvarchar](50) NULL,
        public string? user_group_A {get;set;}  //[nvarchar](1) NULL,
        public string? user_group_B {get;set;}  //[nvarchar](1) NULL,
        public string? user_group_C {get;set;}  //[nvarchar](1) NULL,
        public string? user_group_D {get;set;}  //[nvarchar](1) NULL,
        public string? user_group_E {get;set;}  //[nvarchar](1) NULL,
        public string? user_group_F {get;set;}  //[nvarchar](1) NULL
    }

    */


}
