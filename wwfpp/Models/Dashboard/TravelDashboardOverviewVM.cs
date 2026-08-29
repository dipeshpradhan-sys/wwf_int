
namespace wwfpp.Models
{
    public class TravelDashboardOverviewVM
    {
        public IEnumerable<TravelToSupervisorVM> MyPendingTravel { get; set; }
        public IEnumerable<TravelToSupervisorVM> TravelCancellationSent { get; set; }
        public IEnumerable<TravelToSupervisorVM> RecentTravel { get; set; }
    }

}
