namespace wwfpp.Models.Employee
{
    public class EmployeeContractViewModel
    {
        public int emp_contract_id { get; set; }              // [int] NOT NULL (PK)
        public int? contract_document_id { get; set; }        // [int] NULL
        public string? contract_desc { get; set; }           // [ntext] NULL
        public DateTime? issue_date { get; set; }            // [datetime] NULL
        public DateTime? end_date { get; set; }              // [datetime] NULL
        public string? contract_status { get; set; }        //nvarchar 1
        public string? document_subject { get; set; }
        public int? emp_id { get; set; }                     // [int] NULL
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
