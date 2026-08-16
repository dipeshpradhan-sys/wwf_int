namespace wwfpp.Models.Employee
{
    public class EmployeeDependentChildrenDetailsViewModel
    {
        public int id { get; set; }  //[int] NOT NULL, emp_dep_id
        public int? emp_id { get; set; }  //[int] NOT NULL,
        public string? c_name { get; set; }  //[nvarchar](255) NULL,
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public DateTime? date_of_birth { get; set; }  //[datetime] NULL,
        public string? dob_file_name { get; set; }  //[nvarchar](255) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
        public string? status { get; set; }  //[nvarchar](1) NULL, //eligibility
        public string? remarks { get; set; }  //[nvarchar](255) NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; } = string.Empty;
        public string? isReceiptReq { get; set; } = string.Empty;
        public double? dependentAge { get; set; }
        public string? receipt { get; set; } = string.Empty;
    }

}
