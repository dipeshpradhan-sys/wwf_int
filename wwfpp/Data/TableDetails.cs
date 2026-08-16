using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
namespace wwfpp.Data
{
    public class tbl_pp_options
    {
        [Key]
        public int option_id { get; set; }          //int NOT NULL PRIMARY KEY,
        public string? option_name { get; set; }    //nvarchar(100) NOT NULL, 
        public string? option_value { get; set; }   //nvarchar(500) NOT NULL,
        public string? autoload { get; set; }       //varchar(1) NOT NULL,
        public string? option_note { get; set; }    //nvarchar(500) NULL
    }
    public class PpOptionsConfig : IEntityTypeConfiguration<tbl_pp_options>
    {
        public void Configure(EntityTypeBuilder<tbl_pp_options> builder)
        {
            builder.HasIndex(m => m.option_name).IsUnique(); // Option name Unique Key
        }
    }
    public class tbl_email_list
    {
        [Key]
        public required string id { get; set; }     //nvarchar(50) NOT NULL PRIMARY KEY,
        public string? from_add { get; set; }       //nvarchar(200) NULL,
        public string? to_add { get; set; }         //nvarchar(1000) NULL,
        public string? subject { get; set; }        //nvarchar(1000) NULL,
        public string? e_message { get; set; }      //ntext NULL,
        public DateTime? submit_date { get; set; }  //datetime NULL,
        public string? status { get; set; }         //char(1),
        public DateTime? sent_date { get; set; }    //datetime NULL,
        public string? category { get; set; }       //varchar(50) NULL,
        public string? cc_add { get; set; }         //[nvarchar] (1000) NULL
        public string? bcc_add { get; set; }        //[nvarchar] (1000) NULL

        //This table's primery key is used in below tables as foreign key
        public ICollection<tbl_email_list_attachment> tbl_email_list_attachment { get; set; } = new List<tbl_email_list_attachment>();
        public ICollection<tbl_email_list_sub> tbl_email_list_sub { get; set; } = new List<tbl_email_list_sub>();

    }
    public class tbl_email_list_attachment
    {
        [Key]
        public required string id { get; set; } //NOT NULL PRIMARY KEY,
        public string? attachment { get; set; }    // nvarchar(500) NULL,
        
        [ForeignKey(nameof(TblEmailList))]      
        public string eid { get; set; }        //nvarchar(50) NOT NULL,
        public tbl_email_list TblEmailList { get; set; } = null!;
    }
    public class tbl_email_list_sub
    {
        [Key]
        public required string id { get; set; } //nvarchar(50) NOT NULL PRIMARY KEY,
        public string? message { get; set; }    //nvarchar(250) NULL,
        [ForeignKey(nameof(TblEmailList))]
        public string eid { get; set; }        //nvarchar(50) NOT NULL,
        public tbl_email_list TblEmailList { get; set; } = null!;
        public DateTime? log_date { get; set; }     // datetime NULL
    }
}
