namespace wwfpp.Models.Settings
{
    public class SalarySettingsViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public decimal? child_edu_all { get; set; }
        public string? gender { get; set; }
        public DateTime? join_date { get; set; } //Join date
        public DateTime? end_date { get; set; } //expiry date
        public string? marital_status { get; set; }
        public decimal? salary { get; set; }
        public decimal? remote_area_allow { get; set; }
        public decimal? yearly_remote_exem { get; set; }
        public string? emp_pay_status { get; set; }
        public string? account_no { get; set; }
        public string? pan_no { get; set; }
    }
    public class SalarySettingsListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<SalarySettingsViewModel> Fields { get; set; }
    }
}
