namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelMainViewModel
    {
        //CompositPK
        public int emp_travel_id { get; set; }  //[int] NOT NULL,
        public int? emp_id { get; set; }
        public string? trip_purpose { get; set; }  //[ntext] NULL,
        public string? destinations { get; set; }  //[nvarchar](255) NULL,
        public DateTime? date_from { get; set; }  //[datetime] NULL,
        public DateTime? date_to { get; set; }  //[datetime] NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string? app_status { get; set; }  //[nvarchar](20) NULL,
        public int? app_by { get; set; }
        public DateTime? app_date { get; set; }  //[datetime] NULL,
        public string? denomination { get; set; }  //[ntext] NULL,
        public string? remarks { get; set; }  //[ntext] NULL,
        public string? travel_type { get; set; }  //[nvarchar](20) NULL,
        public string? i_app_status { get; set; }  //[nvarchar](20) NULL,
        public int? i_app_by { get; set; }
        public DateTime? i_app_date { get; set; }  //[datetime] NULL,
        public string? i_app_by_post { get; set; }  //[nvarchar](100) NULL,
        public string? app_by_post { get; set; }  //[nvarchar](100) NULL,
        public string? rec_remarks { get; set; }  //[text] NULL,
        public string? app_remarks { get; set; }  //[text] NULL,
        public DateTime? can_submit_date { get; set; }  //[datetime] NULL,
        public string? can_desc { get; set; }  //[ntext] NULL,
        public int? can_by { get; set; }
        public DateTime? can_date { get; set; }  //[datetime] NULL,
        public string? can_remarks { get; set; }  //[ntext] NULL,
    }

}
