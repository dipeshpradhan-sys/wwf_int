namespace wwfpp.Models.Employee
{
    public class EmployeeDocumentsClassicViewModel
    {
        public int id { get; set; } // emp_id
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; } = string.Empty;
        public string? citizenship_copy { get; set; } = string.Empty;
        public string? passport_copy { get; set; } = string.Empty;
        public string? pan_copy { get; set; } = string.Empty;
        public string? nin_copy { get; set; } = string.Empty;
    }

}
