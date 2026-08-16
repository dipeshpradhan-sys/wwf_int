namespace wwfpp.Models.Payroll
{
    public class EmployeeMedicalReimburseViewModel
    {
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public string? fiscal_year { get; set; }  //[varchar{10) NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? marital_status { get; set; }  //[nvarchar](1) NULL,
        public string? bill_no { get; set; }  //[nvarchar](20) NULL,
        public DateTime? bill_date { get; set; }  //[datetime] NULL,
        public double? self_amt { get; set; }  //[float] NULL,
        public double? spouse_amt { get; set; }  //[float] NULL,
        public double? other_dep_amt { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[varchar{250) NULL,
        public string? app_status { get; set; }  //[varchar{20) NULL,
        public int? app_by { get; set; }  //[int] NULL,
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public int? sal_month { get; set; }  //[int] NULL,
        public int? sal_year { get; set; }  //[int] NULL,
        public string? reim_type { get; set; }  //[nvarchar](50) NULL,
    }

}
