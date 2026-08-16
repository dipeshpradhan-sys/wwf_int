namespace wwfpp.Models.Employee
{
    public class DigitalSignatureViewModel
    {
        public int emp_sign_id { get; set; }
        public int? emp_id { get; set; }
        public DateTime? upload_date { get; set; }
        public string? signature { get; set; } = string.Empty;
        public string? firstname { get; set; } = string.Empty;
        public string? middlename { get; set; } = string.Empty;
        public string? lastname { get; set; } = string.Empty;
        public string? emp_code { get; set; } = string.Empty;
        public string? employee { get; set; } = string.Empty;
        public string? emp_status { get; set; } = string.Empty;
    }
}
