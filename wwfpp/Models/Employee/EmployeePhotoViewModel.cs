namespace wwfpp.Models.Employee
{
    public class EmployeePhotoViewModel
    {
        public string id { get; set; }          //[varchar] (50) NOT NULL PRIMARY KEY,
        public int emp_id { get; set; }                     //[int] NULL,
        public string? photo { get; set; }          //[varchar] (50) NULL
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
