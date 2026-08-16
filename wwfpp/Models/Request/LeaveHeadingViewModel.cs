namespace wwfpp.Models.Request
{
    public class LeaveHeadingViewModel
    {
        public byte leave_type_id { get; set; }  //[tinyint] NOT NULL,
        public string? abbr { get; set; }  //[nvarchar](5) NULL,
        public string? description { get; set; }  //[nvarchar](25) NULL,
        public string? category { get; set; }  //[nvarchar](1) NULL,
        public double? max_leave_hours { get; set; }  //[float] NULL,
    }
}
