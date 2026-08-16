namespace wwfpp.Models.Employee
{
    public class EmployeeDependentChildrenDetailsSubViewModel
    {
        public int emp_dep_sub_id { get; set; }  //[int] NOT NULL,
        public int emp_dep_id { get; set; }  //[int] NOT NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public string? file_name { get; set; }  //[nvarchar](255) NULL,
        public string? status { get; set; }  //[nvarchar](1) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
    }

}
