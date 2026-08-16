namespace wwfpp.Models.Settings
{
    public class LeavePaidClearedViewModel
    {
        public int id { get; set; }
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public string? gender { get; set; }
        public string? fiscal_year { get; set; }
        public DateTime? date_from { get; set; }
        public DateTime? date_upto { get; set; }
        public int submit_counter { get; set; }
        public string? remarks { get; set; }
    }
    public class LeavePaidClearedViewModelNew
    {

        public int indv_leave_id { get; set; }
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? fiscal_year { get; set; }

        public double? annual_leave_caf { get; set; }
        public double? annual_leave_caf_t { get; set; }
        public double? annual_leave_caf_paid { get; set; }
        public double? annual_leave_caf_laps { get; set; }
        public double? annual_leave_caf_n { get; set; }

        public double? annual_leave { get; set; }
        public double? annual_leave_t { get; set; }
        public double? annual_leave_paid { get; set; }
        public double? annual_leave_laps { get; set; }
        public double? annual_leave_n { get; set; }

        public double? sick_leave_caf { get; set; }
        public double? sick_leave_caf_t { get; set; }
        public double? sick_leave_caf_paid { get; set; }
        public double? sick_leave_caf_laps { get; set; }
        public double? sick_leave_caf_n { get; set; }

        public double? sick_leave { get; set; }
        public double? sick_leave_t { get; set; }
        public double? sick_leave_paid { get; set; }
        public double? sick_leave_laps { get; set; }
        public double? sick_leave_n { get; set; }

        public double? casual_leave { get; set; }
        public double? casual_leave_t { get; set; }
        public double? casual_leave_paid { get; set; }
        public double? casual_leave_laps { get; set; }
        public double? casual_leave_n { get; set; }

        public double? other_leave { get; set; }
        public double? other_leave_t { get; set; }
        public double? other_leave_paid { get; set; }
        public double? other_leave_laps { get; set; }
        public double? other_leave_n { get; set; }

        public double? maternity { get; set; }
        public double? maternity_t { get; set; }
        public double? maternity_paid { get; set; }
        public double? maternity_laps { get; set; }
        public double? maternity_n { get; set; }

        public double? paternity { get; set; }
        public double? paternity_t { get; set; }
        public double? paternity_paid { get; set; }
        public double? paternity_laps { get; set; }
        public double? paternity_n { get; set; }

        public double? mourning { get; set; }
        public double? mourning_t { get; set; }
        public double? mourning_paid { get; set; }
        public double? mourning_laps { get; set; }
        public double? mourning_n { get; set; }


        public double? unpaid_study { get; set; }
        public double? unpaid_study_t { get; set; }
        public double? unpaid_study_paid { get; set; }
        public double? unpaid_study_laps { get; set; }
        public double? unpaid_study_n { get; set; }

        public DateTime? date_from { get; set; }
        public DateTime? date_upto { get; set; }
        public int? submit_counter { get; set; }
        public string? remarks { get; set; }
        public string? calculate { get; set; }
        public double total_annual_leave { get; set; }
        public double total_sick_leave { get; set; }

        public bool chkSave { get; set; }
    }
}
