using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Admin
{
    public class MenusViewModel
    {
        [Key]
        public string menu_id { get; set; } = string.Empty;   // PK
        public string menu_code { get; set; } = string.Empty; // Unique, required
        public string? menu_name { get; set; }
        public string? menu_label { get; set; }
        public string? menu_page { get; set; }
        public int? menu_sort { get; set; }
        public string? menu_status { get; set; } // A = Active, P = Passive
        public int? module_id { get; set; }      // FK to tbl_user_module
        public string module_label { get; set; } = string.Empty;
    }

}