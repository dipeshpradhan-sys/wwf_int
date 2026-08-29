using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models
{
    public class EmployeeTravelSettlementMainViewModel
    {
        [Key]
        public int emp_travel_id { get; set; }
        public string? destinations { get; set; }
        public DateTime? date_from { get; set; }
        public DateTime? date_to { get; set; }
        public DateTime? submit_date { get; set; }
        public string? app_status { get; set; }
        public string? travel_type { get; set; }
        public int? app_by { get; set; }
        public string? app_by_post { get; set; }
        public DateTime? app_date { get; set; }
        public string? app_remarks { get; set; }
        public int? can_by { get; set; }
        public DateTime? can_date { get; set; }
        public string? can_desc { get; set; }
        public string? can_remarks { get; set; }
        public DateTime? can_submit_date { get; set; }
        public string? denomination { get; set; }
        public int? emp_id { get; set; }
        public int? i_app_by { get; set; }
        public string? i_app_by_post { get; set; }
        public DateTime? i_app_date { get; set; }
        public string? i_app_status { get; set; }
        public string? rec_remarks { get; set; }
        public string? remarks { get; set; }
        public string? trip_purpose { get; set; }
        public string? employeenameWithCode { get; set; } // from vw_Employee
    }

}
