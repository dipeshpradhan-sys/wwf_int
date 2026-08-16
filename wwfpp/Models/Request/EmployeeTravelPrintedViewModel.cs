namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelPrintedViewModel
    {
        //CompositPK or one to one
        public int emp_travel_id { get; set; }  //[int] NOT NULL,
        public int? acc_app_by { get; set; }
        public int? adv_app_by { get; set; }
        public string? acc_app_by_post { get; set; }  //[nvarchar](100) NULL,
        public string? adv_app_by_post { get; set; }  //[nvarchar](100) NULL,
    }

}
