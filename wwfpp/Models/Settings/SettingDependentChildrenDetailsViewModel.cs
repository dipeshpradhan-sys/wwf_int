namespace wwfpp.Models.Settings
{
    public class SettingDependentChildrenDetailsViewModel
    {
        public int id { get; set; }                             //int not null primary key
        public int max_nos_dep_child_eligible_paid { get; set; }  //[int] NOT NULL,
        public decimal? max_amt_first_age_range { get; set; }  //[money] NULL,
        public decimal? max_amt_second_age_range { get; set; }  //[money] NULL,
        public DateTime? age_checking_date { get; set; }  //[datetime] NULL,
        public double? child_pro_rata_age { get; set; }  //[float] NULL,
        public double? emp_pro_rata_age { get; set; }  //[float] NULL
    }
}
