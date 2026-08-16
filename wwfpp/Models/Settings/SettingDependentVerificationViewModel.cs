namespace wwfpp.Models.Settings
{
    public class SettingDependentVerificationViewModel
    {
        public int id { get; set; }   //int not null primary key                          
        public int? emp_id { get; set; }  //[int]  NULL,
        public int? emp_dep_id { get; set; }  //[int] NULL,
        public int? emp_dep_sub_id { get; set; }  //[int] NULL,
        public string? status { get; set; }  //[varchar 2] NULL,
        public DateTime? update_date { get; set; }  //[datetime] NULL,
    }
}

