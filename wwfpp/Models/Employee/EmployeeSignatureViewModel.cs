namespace wwfpp.Models.Employee
{
    public class EmployeeSignatureViewModel
    {
        public int emp_sign_id { get; set; }  //[int] NOT NULL,
        public string? signature { get; set; }  //[nvarchar](250) NULL,
        public DateTime? upload_date { get; set; }  //[datetime] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
