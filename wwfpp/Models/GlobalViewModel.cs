namespace wwfpp.Models
{
    public class TravelApprovalViewModel
    {
        public string? IAppStatus { get; set; }
        public int? IAppBy { get; set; }

        public string? AppStatus { get; set; }
        public int? AppBy { get; set; }

        public int? ToEmpId { get; set; }
        public string? StrTo { get; set; }
        public string? St { get; set; } // "ad" == "Approve" or "rd" == "Recommend"

        public string? IAppByPost { get; set; }
        public string? AppByPost { get; set; }
    }
    public class TravelStatusBothViewModel
    {
        public string? IAppStatus { get; set; }
        public int? IAppBy { get; set; }
        public string? AppStatus { get; set; }
        public int? AppBy { get; set; }
        public string? IAppByPost { get; set; }
        public string? AppByPost { get; set; }
    }
}