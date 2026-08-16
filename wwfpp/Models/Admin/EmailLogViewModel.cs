using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Admin
{
    public class EmailLogViewModel
    {
        public string id { get; set; }                // nvarchar(50) pk
        public string from_add { get; set; }           // nvarchar(200)
        public string to_add { get; set; }             // nvarchar(1000)
        public string subject { get; set; }           // nvarchar(1000)
        public string e_message { get; set; }          // ntext
        public DateTime? submit_date { get; set; }     // datetime
        public string status { get; set; }            // char(1)
        public DateTime? sent_date { get; set; }       // datetime
        public string category { get; set; }          // varchar(50)
        public string cc_add { get; set; }             // nvarchar(1000)
        public string bcc_add { get; set; }             // nvarchar(1000)

    }

}