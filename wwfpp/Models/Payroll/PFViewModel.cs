namespace wwfpp.Models.Payroll
{
    public class PFViewModel
    {
        public int emp_pf_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }   //[int] NULL,
        public string? pf_no { get; set; }
        public string? h_pf_no { get; set; }
        public string? pf_group { get; set; }
        public string? pf_type { get; set; }
        public double? add_percent_amount { get; set; }
        public double? ded_percent_amount { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? emp_code { get; set; }
        public string? employee { get; set; }
        public string? gender { get; set; }
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? emp_status { get; set; }
        public decimal? salary { get; set; }

    }
    public class PFListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<PFViewModel> Fields { get; set; }
    }
}
