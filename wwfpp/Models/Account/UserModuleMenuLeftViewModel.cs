using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Account
{
    public class UserModuleMenuLeftViewModel
    {
        public required List<ModuleDto> UserLeftModuleMenu { get; set; }
    }

    // ViewModel (for display)
    public class ModuleDto
    {
        public required int module_id { get; set; }
        public string? module_code { get; set; }
        public string? module_name { get; set; }
        public string? module_label { get; set; }
        public string? module_folder { get; set; }
        //public required ICollection<MenuDto> tbl_user_menu { get; set; }
        public List<MenuDto> Menus { get; set; } = new List<MenuDto>();
    }

    public class MenuDto
    {
        public required string menu_id { get; set; }
        public string? menu_code { get; set; }
        public string? menu_name { get; set; }
        public string? menu_label { get; set; }
        public string? menu_page { get; set; }

    }

}
