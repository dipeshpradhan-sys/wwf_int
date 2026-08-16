namespace wwfpp.Models.Employee
{
    public class EmployeeOutsideViewModel
    {
        public int emp_id { get; set; }  //[int] NOT NULL,
        public string? emp_code { get; set; }  //[nvarchar](6) NULL,
        public string? title { get; set; }  //[nvarchar](20) NULL,
        public string? firstname { get; set; }  //[nvarchar](30) NULL,
        public string? middlename { get; set; }  //[nvarchar](30) NULL,
        public string? lastname { get; set; }  //[nvarchar](30) NULL,
        public string? employee { get; set; }
        public string? gender { get; set; }  //[nvarchar](1) NULL,
        public DateTime? dob { get; set; }  //[datetime] NULL,
        public string? address { get; set; }  //[nvarchar](255) NULL,
        public string? phone { get; set; }  //[nvarchar](15) NULL,
        public string? mobile { get; set; }  //[nvarchar](15) NULL,
        public string? e_mail { get; set; }  //[nvarchar](50) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? end_date { get; set; }  //[datetime] NULL,
        public string? emp_status { get; set; }  //[nvarchar](1) NULL,
        public DateTime? deactivated_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public DateTime? effective_date { get; set; }  //[datetime] NULL,
        public string? pan_no { get; set; }  //[nvarchar](20) NULL,
        public string? duty_station_id { get; set; }  //[varchar{50) NULL,
        public string? duty_station { get; set; }
        public string? photo { get; set; }  //[nvarchar](200) NULL,
    }

}
