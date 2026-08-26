namespace wwfpp.Models
{
    public class TravelToSupervisorVM
    {
        public int AppId { get; set; }
        public int EmpId { get; set; }
        public int TravelID { get; set; }
        public string? TravelType { get; set; }
        public string? Destinations { get; set; }
        public DateTime? SubmitDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ToEmpId { get; set; }
        public int ToId { get; set; }
        public string? ReminderFor { get; set; }

        // Extra fields for UI
        public string? PendingBy { get; set; }
        public string? Status { get; set; }
        public string? ActionLink { get; set; }
        public string? CssClass { get; set; }

    }

}
