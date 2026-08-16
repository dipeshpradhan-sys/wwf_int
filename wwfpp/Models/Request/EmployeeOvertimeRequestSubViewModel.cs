namespace wwfpp.Models.Request
{
    public class EmployeeOvertimeRequestSubViewModel
    {
        //CompositPK // ot_req_id+sno
        public string? ot_req_id { get; set; }  //[nvarchar](50) NULL,
        public short? sno { get; set; }  //[smallint] NULL,
        public string? start_time { get; set; }  //[nvarchar](11) NULL,
        public string? end_time { get; set; }  //[nvarchar](11) NULL
    }

}
