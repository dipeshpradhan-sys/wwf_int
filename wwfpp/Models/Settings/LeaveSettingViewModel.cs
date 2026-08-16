using System.Runtime.InteropServices;

namespace wwfpp.Models.Settings
{
    public class LeaveSettingViewModel
    {
        public int indv_leave_id {  get; set; } // int pk
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? gender { get; set; }
        public string? unit { get; set; }
        public string? emp_status { get; set; }
        public string? fiscal_year_to {  get; set; }
        public double? annual_leave_hours_carry_forward { get; set; }
        public double? annual_leave { get; set; }
        public double? sick_leave_hours_carry_forward { get; set; }
        public double? sick_leave { get; set; }
        public double? casual_leave { get; set; }
        public double? other_leave { get; set; }
        public double? maternity { get; set; }
        public double? paternity { get; set; }
        public double? mourning { get; set; }
        public double? unpaid_study { get; set; }
    }

    public class LeaveSettingListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<LeaveSettingViewModel> Fields { get; set; }
    }

}
