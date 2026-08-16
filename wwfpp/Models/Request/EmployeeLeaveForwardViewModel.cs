namespace wwfpp.Models.Request
{
    public class EmployeeLeaveForwardViewModel
    {
        public int carry_forward_id { get; set; }  //[int] NOT NULL,
        public double? hours { get; set; }  //[float] NULL,
        public int? emp_id { get; set; }
        public string? fiscal_year_to { get; set; }  //[nvarchar](9) NULL,
    }

}
