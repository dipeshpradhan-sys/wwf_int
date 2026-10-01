namespace wwfpp.Models
{
    public class DashboardTravelApprovedAccountViewModel
    {
        public int? emp_travel_id { get; set; }
        public int? emp_id { get; set; }
        public string? EmployeeName { get; set; }
        public string? travel_type { get; set; }
        public string? destinations { get; set; }
        public DateTime? date_from { get; set; }
        public DateTime? date_to { get; set; }
        public DateTime? submit_date { get; set; }
        public bool IsMarked { get; set; }
        public int? ActionByID { get; set; }

    }

}
