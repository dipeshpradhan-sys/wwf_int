namespace wwfpp.Models
{
    public class LeaveBalanceViewModel
    {
        public string Description { get; set; }
        public double CarryForward { get; set; }
        public double Current { get; set; }
        public double Total { get; set; }
        public double TakenHours { get; set; }
        public double TakenDays { get; set; }
        public double BalanceHours { get; set; }
        public double BalanceDays { get; set; }
    }

    public class LeaveBalanceListViewModel
    {
        public List<LeaveBalanceViewModel> LeaveBalances { get; set; } = new List<LeaveBalanceViewModel>();
    }


}
