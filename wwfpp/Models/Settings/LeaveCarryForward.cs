namespace wwfpp.Models.Settings
{
    public class LeaveCarryForwardViewModel
    {
        public required string Mode { get; set; }
        public required bool IsCarryForwarded { get; set; }
        public required bool IsAnyPendingLeave { get; set; }
        public string? FyCurAbbr { get; set; }
        public required string FyFrom { get; set; }
        public string? FyFromAbbr { get; set; }
        public required string FyTo { get; set; }
        public string? FyToAbbr { get; set; }
    }
}
