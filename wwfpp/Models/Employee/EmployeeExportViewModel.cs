namespace wwfpp.Models.Employee
{
    public class EmployeeExportViewModel
    {
        public int emp_id { get; set; }  //[int] NOT NULL,
        public string? emp_code { get; set; }  //[nvarchar](6) NULL,
        public string? title { get; set; }  //[nvarchar](20) NULL,
        public string? firstname { get; set; }  //[nvarchar](30) NULL,
        public string? middlename { get; set; }  //[nvarchar](30) NULL,
        public string? lastname { get; set; }  //[nvarchar](30) NULL,
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public string? nationality { get; set; }  //[nvarchar](50) NULL,
        public string? e_mail { get; set; }  //[nvarchar](50) NULL,
        public string? citizenship_number { get; set; }  //[nvarchar](20) NULL,
        public string? passport_number { get; set; }  //[nvarchar](20) NULL,
        public int? no_of_children { get; set; }  //[int] NULL,
        public string? dependent_details { get; set; }  //[ntext] NULL,
        public string? blood_group { get; set; }  //[nvarchar](5) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? employee_type { get; set; }  //[nvarchar](15) NULL,
        public string? department { get; set; }  //[nvarchar](50) NULL,
        public string? post { get; set; }  //[nvarchar](50) NULL,
        public int? manager_id { get; set; }  //[int] NULL,
        public string? emp_status { get; set; }  //[nvarchar](1) NULL, /* A = Active | D = Passive*/
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public DateTime? dob { get; set; }  //[datetime] NULL,
        public string? marital_status_info { get; set; }  //[nvarchar](1) NULL,
        public string? emp_level { get; set; }  //[nvarchar](255) NULL,
        public string? job_family { get; set; }  //[nvarchar](255) NULL,
        public int? line_manager_id { get; set; }  //[int] NULL,
        public int? alt_manager_id { get; set; }  //[int] NULL,
        public int? alt_line_manager_id { get; set; }  //[int] NULL,
        public string? employee_type_sub { get; set; }  //[nvarchar](20) NULL,
        public string? ethnicity { get; set; }  //[nvarchar](250) NULL,
        public double? work_percent { get; set; }  //[float] NULL,
        public string? nin_no { get; set; }  //[nvarchar](20) NULL,
        public string? photo { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? employee_immediate { get; set; } = string.Empty;
        public string? employee_line { get; set; } = string.Empty;
        public string? history { get; set; } = string.Empty;
        public string? change_effective_date { get; set; } = string.Empty;
        public string? pan_no { get; set; } = string.Empty;
        public string? account_no { get; set; } = string.Empty;
        public string? pf_no { get; set; } = string.Empty;
        public string? cit_no { get; set; } = string.Empty;
        public string? address1 { get; set; }  //[nvarchar](255) NULL,
        public string? address2 { get; set; }  //[nvarchar](255) NULL,
        public string? city { get; set; }  //[nvarchar](50) NULL,
        public string? state { get; set; }  //[nvarchar](50) NULL,
        public string? postalcode { get; set; }  //[nvarchar](20) NULL,
        public string? phone1 { get; set; }  //[nvarchar](15) NULL,
        public string? phone2 { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? personal_email { get; set; }  //[nvarchar](50) NULL,
        public string? country { get; set; }

    }
}
