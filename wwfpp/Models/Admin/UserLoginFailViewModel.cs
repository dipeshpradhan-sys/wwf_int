namespace wwfpp.Models.Admin
{
    public class UserLoginFailViewModel
    {
        public string Id { get; set; }      //VARCHAR(50) NOT NULL PRIMARY KEY,
        public string username { get; set; }   //varchar(50) NOT NULL,
        public DateTime on_date { get; set; }  //datetime NOT NULL,
        public string ip { get; set; }     //varchar(255) NOT NULL,
        public string? user_agent { get; set; }     //varchar(255) NULL

    }
}
