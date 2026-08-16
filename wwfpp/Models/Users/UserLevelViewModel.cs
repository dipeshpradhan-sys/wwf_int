using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Users
{
    public class UserLevelViewModel
    {
        public string level_id { get; set; }   //VARCHAR(50) NOT NULL PRIMARY KEY,
        public string? level_name { get; set; } //VARCHAR(50) NOT NULL,
        public int? level_type { get; set; }    //INT NULL, 
        public int? level_sort { get; set; }    //INT NULL, 
    }

}