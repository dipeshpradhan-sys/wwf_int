namespace wwfpp.Models.Attendance
{
    public class EmployeeCheckInOutSettingViewModel
    {
        public byte id { get; set; }  //[tinyint] NOT NULL,
        public string? send_staff_update { get; set; }  //[varchar{1) NULL,
        public string? send_hrs_min { get; set; }  //[varchar{5) NULL,
        public string? send_am_pm { get; set; }  //[varchar{2) NULL,
        //Upperjhata bida aailagyo bhane will be mentioned here so update will not be tiggered on those days
        public string? send_off_days { get; set; }  //[varchar{250) NULL, 
    }

}
