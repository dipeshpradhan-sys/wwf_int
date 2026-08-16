namespace wwfpp.wwwroot.js
{
    public class EmployeeTravelSettlementSubDocViewModel
    {
        public string trav_set_doc_id { get; set; }  //[nvarchar](50) NOT NULL,
        public string? doc_name { get; set; }  //[nvarchar](250) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public string trav_set_id { get; set; } //[nvarchar](50) NULL,
    }

}
