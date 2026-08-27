namespace wwfpp.Models.Request
{
    public class DayData
    {
        public DateTime Date { get; set; }
        public string Value { get; set; }
        public string OvertimeValue { get; set; } // Overtime hours

        public string EmployeeMaxOverTimeValueInAday { get; set; } // Overtime hours

        public Boolean IsHoliday { get; set; }
        public bool OvertimeEditable { get; set; } // NEW: can edit overtime?

        public bool FundSourceEditable { get; set; } // fund source condition
        public bool IsEditableByFund { get; set; }

        public Boolean IsEmpDayOff { get; set; }
        public Boolean AllowOvertimeBoxToUpdateOverLeave { get; set; }

        public string? CanEditOnWeekEnd { get; set; }
        public string? CanEditOnHoliday { get; set; }
    }
}
