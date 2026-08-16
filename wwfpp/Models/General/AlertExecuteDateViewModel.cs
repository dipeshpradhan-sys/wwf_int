namespace wwfpp.Models.General
{
    public class AlertExecuteDateViewModel
    {
        public string id { get; set; } //nvarchar(50) NOT NULL,
        public DateTime last_alert_execute_date { get; set; }
        public DateTime last_alert_settlement_date { get; set; }
        public DateTime last_alert_birthday_date { get; set; }
    }
}
