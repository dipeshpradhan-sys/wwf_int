
namespace wwfpp.Models.Payroll
{
    public class PaySlipBlockViewModel
    {
        public string? id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }
        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? emp_code { get; set; }
        public string? employee { get; set; }  //[nvarchar](10) NULL,
        public string? gender { get; set; }  //[nvarchar](10) NULL,
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }
        public string? emp_status { get; set; }
        public string? block_status { get; set; }
    }
    public class PaySlipBlockListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<PaySlipBlockViewModel> Fields { get; set; }
    }
}
