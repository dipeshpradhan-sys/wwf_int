namespace wwfpp.Models.General
{
    public class DutyStationViewModel
    {
        public string id { get; set; }  //[varchar{50) NOT NULL,
        public string duty_station { get; set; }  //[varchar{50) NOT NULL,
        public string remarks { get; set; }  //[varchar{100) NOT NULL,
        public string? is_active { get; set; }  //[varchar{1) NULL,
    }
}
