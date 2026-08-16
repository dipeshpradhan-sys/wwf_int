namespace wwfpp.Models.Employee
{
    public class EmployeeEducationViewModel
    {
        public int emp_edu_id { get; set; }  //[int] NOT NULL,
        public string? slc_board { get; set; }  //[nvarchar](100) NULL,
        public string? slc_passed_year { get; set; }  //[nvarchar](4) NULL,
        public string? slc_division { get; set; }  //[nvarchar](20) NULL,
        public string? slc_major { get; set; }  //[nvarchar](50) NULL,
        public string? bch_board { get; set; }  //[nvarchar](100) NULL,
        public string? bch_passed_year { get; set; }  //[nvarchar](4) NULL,
        public string? bch_division { get; set; }  //[nvarchar](20) NULL,
        public string? bch_major { get; set; }  //[nvarchar](50) NULL,
        public string? hgt_board { get; set; }  //[nvarchar](100) NULL,
        public string? hgt_passed_year { get; set; }  //[nvarchar](4) NULL,
        public string? hgt_division { get; set; }  //[nvarchar](20) NULL,
        public string? hgt_major { get; set; }  //[nvarchar](50) NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; }
    }

}
