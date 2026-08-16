using System.Runtime.InteropServices;

namespace wwfpp.Models.Settings
{
    public class OvertimeEmployeeSettingViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public string? gender { get; set; }
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? is_get_overtime { get; set; }
        public int? approval_person { get; set; }
    }

    public class OvertimeEmployeeSettingListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<OvertimeEmployeeSettingViewModel> Fields { get; set; }
    }

}
