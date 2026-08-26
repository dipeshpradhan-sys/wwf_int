using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace wwfpp.Models
{
    public class EmployeeLeaveViewModel
    {
        [Key]
        public int? id { get; set; }
        public byte leave_type_id { get; set; }
        public string? leave_type_name { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? submit_date { get; set; } = default!;

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? leave_from_date { get; set; } = default!;

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? leave_to_date { get; set; } = default!;

        public string? leave_desc { get; set; }
        public string? app_status { get; set; }

        public int? app_by { get; set; }
        public string? app_by_name { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? app_date { get; set; } = default!;
        public int? emp_id { get; set; }
        public double leave_in_hrs { get; set; }
        public double leave_in_days { get; set; }

        public string? app_remarks { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? can_submit_date { get; set; } = default!;
        public string? can_desc { get; set; }
        public int? can_by { get; set; }
        public string? can_by_name { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? can_date { get; set; } = default!;
        public string? can_remarks { get; set; }
        public string? can_status { get; set; }
        public string? fiscal_year { get; set; }
        public string? emp_status { get; set; }

    }
}
