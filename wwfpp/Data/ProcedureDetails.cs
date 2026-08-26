using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
namespace wwfpp.Data
{
    public class GetEmployeeTimesheetPivot
    {
        [Key]
        public int fund_id { get; set; }
        public string? fund_source { get; set; }
        public string? fund_desc { get; set; }
        public int emp_id { get; set; }
        public double? Day1_Normal { get; set; }
        public double? Day1_Overtime { get; set; }
        public double? Day2_Normal { get; set; }
        public double? Day2_Overtime { get; set; }
        public double? Day3_Normal { get; set; }
        public double? Day3_Overtime { get; set; }
        public double? Day4_Normal { get; set; }
        public double? Day4_Overtime { get; set; }
        public double? Day5_Normal { get; set; }
        public double? Day5_Overtime { get; set; }
        public double? Day6_Normal { get; set; }
        public double? Day6_Overtime { get; set; }
        public double? Day7_Normal { get; set; }
        public double? Day7_Overtime { get; set; }
        public double? Day8_Normal { get; set; }
        public double? Day8_Overtime { get; set; }
        public double? Day9_Normal { get; set; }
        public double? Day9_Overtime { get; set; }
        public double? Day10_Normal { get; set; }
        public double? Day10_Overtime { get; set; }
        public double? Day11_Normal { get; set; }
        public double? Day11_Overtime { get; set; }
        public double? Day12_Normal { get; set; }
        public double? Day12_Overtime { get; set; }
        public double? Day13_Normal { get; set; }
        public double? Day13_Overtime { get; set; }
        public double? Day14_Normal { get; set; }
        public double? Day14_Overtime { get; set; }
        public double? Day15_Normal { get; set; }
        public double? Day15_Overtime { get; set; }
        public double? Day16_Normal { get; set; }
        public double? Day16_Overtime { get; set; }
        public double? Day17_Normal { get; set; }
        public double? Day17_Overtime { get; set; }
        public double? Day18_Normal { get; set; }
        public double? Day18_Overtime { get; set; }
        public double? Day19_Normal { get; set; }
        public double? Day19_Overtime { get; set; }
        public double? Day20_Normal { get; set; }
        public double? Day20_Overtime { get; set; }
        public double? Day21_Normal { get; set; }
        public double? Day21_Overtime { get; set; }
        public double? Day22_Normal { get; set; }
        public double? Day22_Overtime { get; set; }
        public double? Day23_Normal { get; set; }
        public double? Day23_Overtime { get; set; }
        public double? Day24_Normal { get; set; }
        public double? Day24_Overtime { get; set; }
        public double? Day25_Normal { get; set; }
        public double? Day25_Overtime { get; set; }
        public double? Day26_Normal { get; set; }
        public double? Day26_Overtime { get; set; }
        public double? Day27_Normal { get; set; }
        public double? Day27_Overtime { get; set; }
        public double? Day28_Normal { get; set; }
        public double? Day28_Overtime { get; set; }
        public double? Day29_Normal { get; set; }
        public double? Day29_Overtime { get; set; }
        public double? Day30_Normal { get; set; }
        public double? Day30_Overtime { get; set; }
        public double? Day31_Normal { get; set; }
        public double? Day31_Overtime { get; set; }

    }

    public class GetEmployeeLeave
    {
        [Key]
        public int emp_leave_id { get; set; }
        public int emp_id { get; set; }
        public string? description { get; set; } = string.Empty;
        public string? employeename { get; set; } = string.Empty;
        public DateTime? submit_date { get; set; }
        public DateTime? leave_from_date { get; set; }
        public DateTime? leave_to_date { get; set; }
        public string? leave_desc { get; set; } = string.Empty;
        public double? leave_in_hrs { get; set; }
        public double? leave_in_days { get; set; }

        public string? app_remarks { get; set; } = string.Empty;
        public string? app_status { get; set; } = string.Empty;
        public DateTime? app_date { get; set; }
        public string? appByName { get; set; } = string.Empty;
        public int? appByID { get; set; }
        public DateTime? can_submit_date { get; set; }
        public string? can_desc { get; set; } = string.Empty;
        public string? can_remarks { get; set; } = string.Empty;
        public string? canByName { get; set; } = string.Empty;
        public int? canByID { get; set; }
        public string ? showBtnCan { get; set; }
    }
}
