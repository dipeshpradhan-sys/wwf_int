using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Users
{
    public class GuardViewModel
    {
        [Key]
        public int pk_user_id { get; set; }     // [INT] NOT NULL ,
        public string? user_name { get; set; }  //[VARCHAR] (20)  NULL ,
        public string? user_pass { get; set; }  //[VARCHAR] (20)  NULL ,
        public string? full_name { get; set; }  //[VARCHAR] (200)  NULL ,
        public string? is_active { get; set; }  //[VARCHAR] (1) NULL ,
        public string? user_type { get; set; }  //[VARCHAR] (50)  NULL


    }

}