using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Admin
{
    public class ModulesViewModel
    {
        [Key]
        public int module_id { get; set; }
        public string? module_code { get; set; }
        public string? module_name { get; set; }
        public string? module_label { get; set; }
        public string? module_folder { get; set; }
        public int? module_sort { get; set; }
        public string? module_status { get; set; } // A = Active, P = Passive

    }

}