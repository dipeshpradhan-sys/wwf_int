namespace wwfpp.Models.Payroll
{
    public class SSFViewModel
    {
        public int id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }   //[int] NULL,
        public string? ssf_number { get; set; }  //[nvarchar](20) NULL,
        public double? add_percent { get; set; }  //[float] NULL,
        public double? ded_percent { get; set; }  //[float] NULL,
        public double? add_percent_amount { get; set; }  //[float] NULL,
        public double? ded_percent_amount { get; set; }  //[float] NULL,
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
    public class SSFListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<SSFViewModel> Fields { get; set; }
    }
}
