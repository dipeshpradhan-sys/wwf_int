using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace wwfpp.Models.Request
{
    public class EmployeeMedicalReimburseViewModel
    {
        public string? Id { get; set; }
        public string FiscalYear { get; set; }
        public int? EmpId { get; set; }

        public string ReimType { get; set; }          // Medical / Life Insurance / Non Life Insurance
        public string? MaritalStatus { get; set; }     // Display only (Married / Not Married)

        public string BillNo { get; set; }
        public DateTime? BillDate { get; set; }

        public double? SelfAmt { get; set; }
        public double? SpouseAmt { get; set; }
        public double? OtherDepAmt { get; set; }

        public string Remarks { get; set; }
        public string? emp_status { get; set; }  //[nvarchar](50) NULL,
        public string? app_status { get; set; }  //[nvarchar](50) NULL,
    }

}