using wwfpp.Data;
using wwfpp.Models;
namespace wwfpp.Models.Payroll
{
    public class PaySlipViewModel
    {
        // Employee info
        public int EmpId { get; set; }
        public string EmpCode { get; set; }
        public string FullName { get; set; }
        public string Post { get; set; }
        public string AccountNo { get; set; }
        public string PFNo { get; set; }
        public string CITNo { get; set; }
        public string PanNo { get; set; }
        public string GratuityNo { get; set; }
        public string SSFNo { get; set; }

        // Earnings
        public decimal BasicSalary { get; set; }
        public decimal ActBasicSalary { get; set; }
        public decimal PFEmployer { get; set; }
        public decimal ActPFEmployer { get; set; }
        public decimal ChildrenEduAllowance { get; set; }
        public decimal Insurance { get; set; }
        public decimal Overtime { get; set; }
        public decimal RemoteAreaAllowance { get; set; }
        public decimal DashainBonus { get; set; }
        public decimal PerformanceBonus { get; set; }
        public decimal Others { get; set; }
        public decimal MedicalReimbursement { get; set; }
        public decimal LeaveEncash { get; set; }
        public decimal Gratuity { get; set; }
        public decimal SSF { get; set; }

        // Deductions
        public decimal pf_d { get; set; }
        public decimal act_pf_d { get; set; }
        public decimal cit_d { get; set; }
        public decimal incometax_d { get; set; }
        public decimal betalibi_d { get; set; }
        public decimal tel_per_adv { get; set; }
        public decimal pr_adv { get; set; }
        public decimal travel_prog_adv { get; set; }
        public decimal fd_adv { get; set; }
        public decimal wl_adv { get; set; }
        public decimal adv_PF_loan { get; set; }
        public decimal adv_CIT_loan { get; set; }
        public decimal welfare_fund { get; set; }
        public decimal gratuity_ded { get; set; }
        public decimal ssf_ded { get; set; }

        // Totals
        public decimal TotalEarnings { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetInHand { get; set; }
        public decimal NetInHandTable { get; set; }

        // Differential values
        public decimal DiffBasicSalary { get; set; }
        public decimal DiffPFEmployer { get; set; }
        public decimal DiffPFEmployee { get; set; }

        public string? op_org_name { get; set; }
        public string? op_org_addr { get; set; }

        public decimal TotalBasicPF { get; set; }
        public decimal TotalDedBasicPF { get; set; }
        public decimal overtime_amt { get; set; }
        public decimal ot_diff { get; set; }
        public decimal diff_act_pf_d { get; set; }
    }

}
