using System.Runtime.InteropServices;

namespace wwfpp.Models.Settings
{
    public class TimesheetSettingViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public string? timesheet_acceptance { get; set; }
        public int? emp_year { get; set; }
        public int? emp_month { get; set; }
    }

    public class TimesheetSettingListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<TimesheetSettingViewModel> Fields { get; set; }
    }

}
