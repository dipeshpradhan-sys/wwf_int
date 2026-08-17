namespace wwfpp.Models.Payroll
{
    public class EmployeeLeaveAccrualViewModel
    {
        public int emp_id { get; set; }
        public string? emp_code { get; set; }
        public string? full_name { get; set; }
        public decimal basic_salary { get; set; }
        public string? emp_status { get; set; }
        public DateTime? join_date { get; set; }
        public DateTime? end_date { get; set; }

        // Pre-fiscal year values
        public string? pre_fiscal_year { get; set; }
        public decimal? pre_leave_payable { get; set; }

        // Fiscal year dates
        public DateTime? start_fiscal_date { get; set; }
        public DateTime? end_fiscal_date { get; set; }

        // Annual leave
        public decimal? total_annual_leave { get; set; }
        public decimal? an_hrs_can_carry_forward { get; set; }
        public double? an_leave_balance { get; set; }
        public double? an_leave_accrual { get; set; }

        // Sick leave
        public decimal? total_sick_leave { get; set; }
        public decimal? si_hrs_can_carry_forward { get; set; }
        public double? si_leave_balance { get; set; }
        public double? si_leave_accrual { get; set; }

        // Totals
        public double? leave_accrual_days { get; set; }
        public decimal? leave_payable { get; set; }
        public decimal? net_provision { get; set; }
        public decimal? pre_provisioned { get; set; }
        public int? counter { get; set; }

        public string? remarks { get; set; }
    }

    public class EmployeeLeaveAccrualListViewModel
    {
        public string? mode { get; set; }
        public List<EmployeeLeaveAccrualViewModel> Fields { get; set; }
    }
}
