namespace wwfpp.Models.Payroll
{
    public class EmployeeExcessLeaveEncashEmpWiseViewModel
    {
        public string id { get; set; } = string.Empty; 
        public string? fiscal_year { get; set; }       
        public int? emp_id { get; set; }               
        public decimal? amount { get; set; }           
        public double? total_hours { get; set; }       
        public string? remarks { get; set; }           
        public short? counter { get; set; }            
    }

    public class EmployeeExcessLeaveEncashListViewModel
    {
        public string? mode { get; set; }
        public List<EmployeeExcessLeaveEncashEmpWiseViewModel> Fields { get; set; }
    }
}
