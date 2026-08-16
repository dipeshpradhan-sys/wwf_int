namespace wwfpp.Models.Employee
{
    public class EmployeeInsuranceViewModel
    {
        public int emp_ins_id { get; set; }  //[int] NOT NULL,
        public string? ins_company { get; set; }  //[nvarchar](100) NULL,
        public string? ins_type { get; set; }  //[nvarchar](25) NULL,
        public DateTime? ins_valid_date { get; set; }  //[datetime] NULL,
        public string? policy_no { get; set; }  //[nvarchar](20) NULL,
        public decimal? ins_amount { get; set; }  //[money] NULL,
        public decimal? premium_amount { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
