namespace wwfpp.Models.Request
{
    public class TimesheetMessage
    {
        public string Text { get; set; }
        public string Type { get; set; } // "error", "warning", "success"
    }


    public class FundTimesheetRow
    {
        public int FundId { get; set; }
        public string FundSourceName { get; set; }
        public string FundSourceDefault { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double AnnualHours { get; set; }
        public double UsedHours { get; set; }
        public double RemainingHours { get; set; }
           
        public List<DayData> Days { get; set; } = new();

        public double TotalNormalHours => Days.Sum(d => double.TryParse(d.Value, out var v) ? v : 0);
        public double TotalOvertimeHours => Days.Sum(d => double.TryParse(d.OvertimeValue, out var v) ? v : 0);
    }


    public class TimesheetViewModel
    {
        public List<DayData> Days { get; set; } = new List<DayData>();

        public List<FundTimesheetRow> FundRows { get; set; } = new();


        // Extra criteria results
        public List<TimesheetMessage> Messages { get; set; } = new List<TimesheetMessage>();

        public bool ShowAddEditTimeSheetButton { get; set; }
        public int MaxNormalHours { get; set; }
        public int MaxOvertimeHours { get; set; }

        public bool ShowSaveButton { get; set; }
        public double FundBalance { get; set; }
        public bool ShowSendButton { get; set; }

        // Grand totals across all funds
        public double GrandTotalNormal => FundRows.Sum(r => r.TotalNormalHours);
        public double GrandTotalOvertime => FundRows.Sum(r => r.TotalOvertimeHours);

        // ✅ Previous Timesheet
        public int PrevApprovedTimesheetCount { get; set; }
        public Dictionary<int, List<FundTimesheetRow>> PrevApprovedTimesheets { get; set; }= new Dictionary<int, List<FundTimesheetRow>>();

        public bool HasApprover { get; set; }

    }

}
