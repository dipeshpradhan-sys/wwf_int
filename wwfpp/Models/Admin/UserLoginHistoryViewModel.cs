namespace wwfpp.Models.Admin
{
    public class UserLoginHistoryViewModel
    {
        public string ID { get; set; }
        public DateTime? in_date { get; set; }
        public DateTime? out_date { get; set; }
        public string? ip { get; set; }
        public string? user_agent { get; set; }
        public string? username { get; set; }
        public string? fullname { get; set; }
        public string? level_name { get; set; }
    }
}
