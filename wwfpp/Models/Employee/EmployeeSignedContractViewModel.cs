namespace wwfpp.Models.Employee
{
    public class EmployeeSignedContractViewModel
    {
        public int emp_signed_contract_id { get; set; }  //[int] NOT NULL,
        public int? contract_document_id { get; set; }  //[int] NULL,
        public string? document_subject { get; set; } = string.Empty;
        public string? signed_contract { get; set; }  //[nvarchar](50) NULL,
        public DateTime? upload_date { get; set; }  //[datetime] NULL,
        public int? emp_id { get; set; }                     // [int] NULL
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; } = string.Empty;
    }

}
