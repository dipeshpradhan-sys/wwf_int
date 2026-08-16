namespace wwfpp.Models.Employee
{
    public class EmployeeManagerViewModel
    {
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public string? emp_status { get; set; }
        public int? manager_id { get; set; }
        public int? line_manager_id { get; set; }
        public int? alt_manager_id { get; set; }
        public int? alt_line_manager_id { get; set; }
    }
    public class EmployeeManagerListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<EmployeeManagerViewModel> Fields { get; set; }
    }
}
