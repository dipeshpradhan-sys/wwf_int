using System.Runtime.InteropServices;

namespace wwfpp.Models.Settings
{
    public class SalarySettingsExtraViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public string? duty_station_id { get; set; }
        public string? staff_type { get; set; }
        public string? is_field_staff { get; set; }
        public string? is_field_salary { get; set; }
        public string? is_get_dashain { get; set; }
        public double? welfare_con_percent { get; set; }
        public string? get_leave_accrual { get; set; }
        public string? get_gratuity_accrual { get; set; }
        public DateTime? gratuity_date { get; set; }
    }

    public class SalarySettingsExtraListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<SalarySettingsExtraViewModel> Fields { get; set; }
    }

}
