namespace wwfpp.Models.Payroll
{
    public class AdvanceViewModel
    {
        public string? adv_id { get; set; }  //[nvarchar](50) NOT NULL,
        public decimal? adv_personnel { get; set; }  //[money] NULL,
        public decimal? adv_program { get; set; }  //[money] NULL,
        public decimal? adv_travel { get; set; }  //[money] NULL,
        public decimal? adv_field_drawing { get; set; }  //[money] NULL,
        public decimal? adv_welfare { get; set; }  //[money] NULL,
        public int? emp_id { get; set; }
        public short? adv_year { get; set; }  //[smallint] NULL,
        public short? adv_month { get; set; }  //[smallint] NULL,
        public decimal? adv_pf_loan { get; set; }  //[money] NULL,
        public decimal? adv_cit_loan { get; set; }  //[money] NULL,
        public string? adv_fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? adv_emp_week { get; set; }  //[tinyint] NULL,
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? emp_code { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }

    }
    public class AdvanceListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<AdvanceViewModel> Fields { get; set; }
    }
}
