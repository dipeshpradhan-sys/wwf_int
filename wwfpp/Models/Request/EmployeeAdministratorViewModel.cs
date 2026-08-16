namespace wwfpp.Models.Request
{
    public class EmployeeAdministratorViewModel
    {
        //holds emp_id on all field.//so many so not idea wheather to define foreign key or not 
        public short id { get; set; }  //[smallint] NOT NULL,
        public int? cra { get; set; }  //[int] NULL,
        public int? doo { get; set; }  //[int] NULL,
        public int? faa { get; set; }  //[int] NULL,
        public int? aca { get; set; }  //[int] NULL,
        public int? hra { get; set; }  //[int] NULL,
        public int? rca { get; set; }  //[int] NULL,
        public int? t_t_a_1 { get; set; }  //[int] NULL,
        public int? t_t_a_2 { get; set; }  //[int] NULL,
        public int? t_a_s_1 { get; set; }  //[int] NULL,
        public int? t_a_s_2 { get; set; }  //[int] NULL,
        public int? t_a_s_3 { get; set; }  //[int] NULL,
        public int? t_a_s_4 { get; set; }  //[int] NULL,
        public int? acr { get; set; }  //[int] NULL,
        public int? t_t_a_3 { get; set; }  //[int] NOT NULL,
        public int? t_t_a_4 { get; set; }  //[int] NOT NULL,
        public int? t_t_a_5 { get; set; }  //[int] NOT NULL,
        public int? t_a_s_5 { get; set; }  //[int] NOT NULL,
        public int? ahr { get; set; }  //[int] NULL,
    }

}
