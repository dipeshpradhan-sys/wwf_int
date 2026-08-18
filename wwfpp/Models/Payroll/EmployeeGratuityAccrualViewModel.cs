namespace wwfpp.Models.Payroll
{
    public class EmployeeGratuityAccrualViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,
        public DateTime? fy_end_fiscal_date { get; set; }  //[datetime] NULL,
        public double? service_year { get; set; }  //[float] NULL,
        public decimal? base_salary { get; set; }  //[money] NULL,
        public decimal? gratuity_encash { get; set; }  //[money] NULL,
        public decimal? pre_gratuity_encash { get; set; }  //[money] NULL,
        public decimal? net_gratuity_encash { get; set; }  //[money] NULL,
        public double? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? period { get; set; }  //[smallint] NULL,
    }
    public class EmployeeGratuityAccrualListViewModel
    {
        public string? mode { get; set; }
        public List<EmployeeGratuityAccrualViewModel> Fields { get; set; }
    }
}
