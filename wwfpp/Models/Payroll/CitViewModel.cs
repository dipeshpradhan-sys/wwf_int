namespace wwfpp.Models.Payroll
{
    public class CitViewModel
    {
        public int? emp_cit_id { get; set; }
        public int? emp_id { get; set; }
        public string? cit_no { get; set; }
        public string? h_cit_no { get; set; }
        public string? cit_type { get; set; }
        public double? percent_amount { get; set; }
        public string? remarks { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? emp_code { get; set; }
        public string? employee { get; set; }
        public string? gender { get; set; }
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? emp_status { get; set; }
    }
    public class CitListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<CitViewModel> Fields { get; set; }
    }
}
