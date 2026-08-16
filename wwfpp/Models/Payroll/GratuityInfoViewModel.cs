namespace wwfpp.Models.Payroll
{
    public class GratuityInfoViewModel
    {
        public int id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; } //[int] NULL,
        public string? gr_number { get; set; }  //[nvarchar](20) NULL,
        public string? gr_group { get; set; }  //[nvarchar](1) NULL,
        public string? gr_type { get; set; }  //[nvarchar](1) NULL,
        public double? add_percent_amount { get; set; }  //[float] NULL,
        public double? ded_percent_amount { get; set; }  //[float] NULL,
        public double? opening_balance { get; set; }  //[float] NULL,
        public double? opening_interest { get; set; }  //[float] NULL,
    }
    public class GratuityListViewModel
    {
        public string? mode { get; set; }
        public List<GratuityInfoViewModel> Fields { get; set; }
    }
}