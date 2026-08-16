namespace wwfpp.Models.Employee
{
    public class EmployeePositionProfileViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public string? employee_type { get; set; }
        public string? job_family { get; set; }
        public string? emp_level { get; set; }
        public string? department { get; set; }
        public string? post { get; set; }

    }
    public class EmployeePositionProfileListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<EmployeePositionProfileViewModel> Fields { get; set; }
    }
}
