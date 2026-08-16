namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelSubViewModel
    {
        //no primary key | so define later //currently : CompositPK emp_travel_id+par_id
        public int emp_travel_id { get; set; }  //[int] NULL,
        public byte? par_id { get; set; }  //[tinyint] NULL,
        public string? detail { get; set; }  //[nvarchar](255) NULL,
        public string? unit { get; set; }  //[nvarchar](20) NULL,
        public byte? cur_id { get; set; }  //[tinyint] NULL,
        public string? nos { get; set; }  //[float] NULL,
        public decimal? rate { get; set; }  //[money] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL
    }

}
