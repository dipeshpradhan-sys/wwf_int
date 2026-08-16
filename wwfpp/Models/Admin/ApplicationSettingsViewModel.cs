using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Admin
{

    public class ApplicationSettingsViewModel
    {
        [Key]
        public int option_id { get; set; }

        [StringLength(100)]
        public string option_name { get; set; } = string.Empty;

        [StringLength(500)]
        public string option_value { get; set; } = string.Empty;

        [StringLength(1)]
        public string? autoload { get; set; }

        [StringLength(500)]
        public string? option_note { get; set; }

    }
    /*
    public class ApplicationSettingsViewModel
    {
        public List<PPOptions> PPOptionsList { get; set; } = new();
        public ApplicationSettingsViewModel(List<PPOptions> options)
        {
            PPOptionsList = options;
        }
    }
    */
}