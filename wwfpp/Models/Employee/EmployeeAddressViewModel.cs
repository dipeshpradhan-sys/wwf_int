namespace wwfpp.Models.Employee
{
    public class EmployeeAddressViewModel
    {
        public int emp_id { get; set; }  //[int] NOT NULL,
        public string? address1 { get; set; }  //[nvarchar](255) NULL,
        public string? address2 { get; set; }  //[nvarchar](255) NULL,
        public string? city { get; set; }  //[nvarchar](50) NULL,
        public string? state { get; set; }  //[nvarchar](50) NULL,
        public string? country { get; set; }  //[nvarchar](50) NULL,
        public string? postalcode { get; set; }  //[nvarchar](20) NULL,
        public string? phone1 { get; set; }  //[nvarchar](15) NULL,
        public string? phone2 { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? personal_email { get; set; }  //[nvarchar](50) NULL,
        public string? skype { get; set; }  //[nvarchar](250) NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
