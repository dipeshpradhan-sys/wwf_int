using System.Runtime.InteropServices;

namespace wwfpp.Models.Settings
{
    public class LeaveExcessEncashViewModel
    {
        public int indv_leave_id { get; set; } // int pk
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? gender { get; set; }
        public string? unit { get; set; }
        public string? emp_status { get; set; }
        public string? status { get; set; }
        public decimal? salary { get; set; }
        public string? fiscal_year { get; set; }
        public double? max_annual_leave_cafw { get; set; }
        public double? tot_annual_leave_paid { get; set; }
        public double? cur_annual_leave_laps { get; set; }
        public double? max_sick_leave_cafw { get; set; }
        public double? tot_sick_leave_paid { get; set; }
        public double? cur_sick_leave_laps { get; set; }
        public int? sumbit_counter { get; set; }
        public double? bacic_salary { get; set; }
        public double? tot_annual_leave_amt { get; set; }
        public double? tot_sick_leave_amt { get; set; }
        public int? paid_month { get; set; }
        public int? paid_year { get; set; }
        public double? tot_leave_paid { get; set; }
        public double? cur_leave_laps { get; set; }
        public double? tot_leave_amt { get; set; }
    }

    public class LeaveExcessEncashListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<LeaveExcessEncashViewModel> Fields { get; set; }
    }

}
