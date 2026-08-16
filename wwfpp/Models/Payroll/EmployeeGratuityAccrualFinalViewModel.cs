namespace wwfpp.Models.Payroll
{
    public class EmployeeGratuityAccrualFinalViewModel
    {
        public string id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; } //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](9) NULL,
        public DateTime? join_date { get; set; }  //[datetime] NULL,
        public DateTime? gratuity_date { get; set; }  //[datetime] NULL,
        public DateTime? fy_end_date { get; set; }  //[datetime] NULL,
        public double? service_year { get; set; }  //[float] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? gratuity_encash { get; set; }  //[money] NULL,
        public decimal? pre_encash { get; set; }  //[money] NULL,
        public decimal? net_encash { get; set; }  //[money] NULL,
        public decimal? total_hours { get; set; }  //[float] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? remarks { get; set; }  //[nvarchar](250) NULL,
        public short? counter { get; set; }  //[smallint] NULL,
    }

}
