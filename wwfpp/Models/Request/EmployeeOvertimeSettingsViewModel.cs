namespace wwfpp.Models.Request
{
    public class EmployeeOvertimeSettingsViewModel
    {
        // one to one relationship
        public int emp_id { get; set; }
        public string? is_get_overtime { get; set; }  //[nvarchar](1) NULL,
        public string? approval_person { get; set; }  //[int] NULL
    }

}
