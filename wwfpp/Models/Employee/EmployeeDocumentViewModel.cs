namespace wwfpp.Models.Employee
{
    public class EmployeeDocumentViewModel
    {
        public string document_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NOT NULL,
        public int? document_type_id { get; set; }  //[int] NOT NULL,
        public string? document_number { get; set; }  //[nvarchar](50) NULL,
        public string? document_copy { get; set; }  //[nvarchar](50) NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
