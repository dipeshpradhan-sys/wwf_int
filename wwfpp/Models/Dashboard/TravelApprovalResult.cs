namespace wwfpp.Models
{
    public class TravelApprovalResult
    {
        public int? ApproverId { get; set; }
        public string ApproverEmail { get; set; }
        public string ApproverPost { get; set; }
        public int? ToEmployeeId { get; set; }
        public string Status { get; set; } = "Pending";
        public string Stage { get; set; } // "ad" or "rd"
        public int? IntermediateApproverId { get; set; }
        public string IntermediateApproverPost { get; set; }
    }
}
