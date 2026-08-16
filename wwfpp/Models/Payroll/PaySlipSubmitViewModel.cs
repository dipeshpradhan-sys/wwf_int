namespace wwfpp.Models.Payroll
{
    public class PaySlipSubmitViewModel
    {
        public int? emp_id { get; set; }
        public string? emp_code { get; set; }
        public string? fullname { get; set; }
        public string? e_mail { get; set; }
        public string? emp_status { get; set; }
        public bool? isblocked { get; set; }   // salary slip blocked?
    }
    public class PaySlipSubmitListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public string? message { get; set; }
        public List<PaySlipSubmitViewModel> Fields { get; set; }
    }
}
