namespace wwfpp.Models
{
    public class TimesheetAppVM
    {
        public string? AppId { get; set; }
        public int? EmpId { get; set; }
        public int? EmpMonth { get; set; }
        public int? EmpYear { get; set; }
        public int? AppBy { get; set; }
        public string? AppByName { get; set; }
        public DateTime? SubmitDate { get; set; }

        public int? ToEmpId { get; set; }
        public int? ToId { get; set; }

        public int? Counter { get; set; }

    }

}
