using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using wwfpp.Data;

namespace wwfpp.Models.Users
{
    public class UsersViewModel
    {
        public int? user_id { get; set; }        //[int] NOT NULL,     
        public string? username { get; set; }   //[nvarchar](20) NULL,
        public string? level_id { get; set; }   //[varchar](50) NULL,
        public string? level_name { get; set; }   //[varchar](50) NULL,
        public int? emp_id { get; set; }        //[int] NULL,
        public string? emp_code { get; set; }
        public string? firstname{ get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? is_active { get; set; }  //[nvarchar](1) NULL,

        public int? sign_in_type { get; set; }          //[int] NOT NULL,

        public string? EmployeeList { get; set; }
        public string? UserLevel { get; set; }
    }
}
