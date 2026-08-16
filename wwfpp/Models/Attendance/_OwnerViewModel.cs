namespace wwfpp.Models.Attendance
{
    public class OwnerViewModel
    {
        public string pk_owner_id { get; set; }  //[varchar{20) NOT NULL,
        public string owner_name { get; set; }  //[varchar{250) NOT NULL,
        public string? address { get; set; }  //[varchar{250) NULL,
        public string? contact_person { get; set; }  //[varchar{100) NULL,
        public string? phone { get; set; }  //[varchar{50) NULL,
        public string? fax { get; set; }  //[varchar{50) NULL,
        public string? mobile { get; set; }  //[varchar{50) NULL,
        public string? e_mail { get; set; }  //[varchar{100) NULL,
        public string? website { get; set; }  //[varchar{250) NULL,
        public string? logo { get; set; }  //[varchar{250) NULL,
        public string? PAN { get; set; }  //[varchar{50) NULL,
        public string? created_date { get; set; }  //[datetime] NULL
    }

}
