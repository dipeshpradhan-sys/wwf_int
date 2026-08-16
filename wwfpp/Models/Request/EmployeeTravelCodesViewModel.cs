namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelCodesViewModel
    {
        //CompositPK
        public int emp_travel_id { get; set; }  //[int] NOT NULL,
        public byte? sn { get; set; }  //[tinyint] NULL,
        public int? fund_id { get; set; }  //[int] NULL
    }

}
