namespace wwfpp.Models.Payroll
{
    public class PaySlipPreviewRequest
    {
        public List<int> SelectedIds { get; set; } = new List<int>();
        public int Year { get; set; }
        public int Month { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}