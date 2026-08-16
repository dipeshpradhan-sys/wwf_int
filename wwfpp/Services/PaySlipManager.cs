using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Models.Payroll;
using wwfpp.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace wwfpp.Services
{
    public class PaySlipManager
    {
        private readonly AppDbContext _context;

        public PaySlipManager(AppDbContext context)
        {
            _context = context;
        }

        public bool GetIsMonthHasDiff(int year, int month)
        {
            return _context.tbl_salary_differential_month
                .Any(m => m.sal_year == year && m.sal_month == month);
        }

        public PaySlipViewModel GetPaySlipSingle(int empId, int year, int month, bool diffPeriod)
        {
            var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_id == empId);
            if (emp == null) { return null; }

            // Try main salary table first
            var salaryRecord = _context.tbl_employee_salary
                .FirstOrDefault(s => s.emp_id == empId && s.sal_year == year && s.sal_month == month);

            // If not found, fallback to salary_a_field
            var salaryFieldRecord = salaryRecord == null
                ? _context.tbl_employee_salary_a_field
                    .FirstOrDefault(s => s.emp_id == empId && s.sal_year == year && s.sal_month == month)
                : null;

            if (salaryRecord == null && salaryFieldRecord == null) { return null; }

            // Normalize into PaySlipVM
            var vm = new PaySlipViewModel
            {
                EmpId = emp.emp_id,
                EmpCode = emp.emp_code ?? "",
                FullName = $"{emp.firstname} {emp.middlename} {emp.lastname}",
                Post = emp.post ?? "",
                AccountNo = emp.account_no ?? "",
                PFNo = emp.pf_no ?? "",
                CITNo = emp.cit_no ?? "",
                PanNo = emp.pan_no ?? "",
                GratuityNo = _context.tbl_employee_gratuity_info
                    .Where(g => g.emp_id == emp.emp_id).Select(g => g.gr_number).FirstOrDefault() ?? "",
                SSFNo = _context.tbl_employee_ssf_info
                    .Where(s => s.emp_id == emp.emp_id).Select(s => s.ssf_number).FirstOrDefault() ?? "",

                // Earnings
                BasicSalary = salaryRecord?.basic_salary ?? salaryFieldRecord?.basic_salary ?? 0,
                ActBasicSalary = salaryRecord?.act_basic_salary ?? salaryFieldRecord?.act_basic_salary ?? 0,
                PFEmployer = salaryRecord?.pf_a ?? salaryFieldRecord?.pf_a ?? 0,
                ActPFEmployer = salaryRecord?.act_pf_a ?? salaryFieldRecord?.act_pf_a ?? 0,
                ChildrenEduAllowance = salaryRecord?.children_edu_all ?? salaryFieldRecord?.children_edu_all ?? 0,
                Insurance = salaryRecord?.insurance ?? salaryFieldRecord?.insurance ?? 0,
                Overtime = salaryRecord?.overtime ?? salaryFieldRecord?.overtime ?? 0,
                RemoteAreaAllowance = salaryRecord?.remote_area_all ?? salaryFieldRecord?.remote_area_all ?? 0,
                DashainBonus = salaryRecord?.dashain_a ?? salaryFieldRecord?.dashain_a ?? 0,
                PerformanceBonus = salaryRecord?.performance_all ?? salaryFieldRecord?.performance_all ?? 0,
                Others = salaryRecord?.others ?? salaryFieldRecord?.others ?? 0,
                MedicalReimbursement = salaryRecord?.medical_expense_reimburse_total ?? salaryFieldRecord?.medical_expense_reimburse_total ?? 0,
                LeaveEncash = salaryRecord?.leave_encash ?? salaryFieldRecord?.leave_encash ?? 0,
                Gratuity = salaryRecord?.gratuity ?? salaryFieldRecord?.gratuity ?? 0,
                SSF = salaryRecord?.ssf ?? salaryFieldRecord?.ssf ?? 0,

                // Deductions
                pf_d = salaryRecord?.pf_d ?? salaryFieldRecord?.pf_d ?? 0,
                act_pf_d = salaryRecord?.act_pf_d ?? salaryFieldRecord?.act_pf_d ?? 0,
                cit_d = salaryRecord?.cit_d ?? salaryFieldRecord?.cit_d ?? 0,
                incometax_d = salaryRecord?.incometax_d ?? salaryFieldRecord?.incometax_d ?? 0,
                betalibi_d = salaryRecord?.betalibi_d ?? salaryFieldRecord?.betalibi_d ?? 0,
                tel_per_adv = salaryRecord?.tel_per_adv ?? salaryFieldRecord?.tel_per_adv ?? 0,
                pr_adv = salaryRecord?.pr_adv ?? salaryFieldRecord?.pr_adv ?? 0,
                travel_prog_adv = salaryRecord?.travel_prog_adv ?? salaryFieldRecord?.travel_prog_adv ?? 0,
                fd_adv = salaryRecord?.fd_adv ?? salaryFieldRecord?.fd_adv ?? 0,
                wl_adv = salaryRecord?.wl_adv ?? salaryFieldRecord?.wl_adv ?? 0,
                adv_PF_loan = salaryRecord?.adv_PF_loan ?? salaryFieldRecord?.adv_PF_loan ?? 0,
                adv_CIT_loan = salaryRecord?.adv_CIT_loan ?? salaryFieldRecord?.adv_CIT_loan ?? 0,
                welfare_fund = salaryRecord?.welfare_fund ?? salaryFieldRecord?.welfare_fund ?? 0,
                gratuity_ded = salaryRecord?.gratuity_ded ?? salaryFieldRecord?.gratuity_ded ?? 0,
                ssf_ded = salaryRecord?.ssf_ded ?? salaryFieldRecord?.ssf_ded ?? 0,

                NetInHandTable = salaryRecord?.net_in_hand ?? salaryFieldRecord?.net_in_hand ?? 0,
            };
            // Totals
            vm.TotalEarnings = vm.BasicSalary + vm.PFEmployer + vm.ChildrenEduAllowance + vm.Insurance +
                               vm.Overtime + vm.RemoteAreaAllowance + vm.DashainBonus + vm.PerformanceBonus +
                               vm.Others + vm.MedicalReimbursement + vm.LeaveEncash + vm.Gratuity + vm.SSF;

            vm.TotalDeductions = vm.pf_d + vm.cit_d + vm.incometax_d + vm.betalibi_d +
                                 vm.tel_per_adv + vm.pr_adv + vm.travel_prog_adv + vm.fd_adv +
                                 vm.wl_adv + vm.adv_PF_loan + vm.adv_CIT_loan + vm.welfare_fund +
                                 vm.gratuity_ded + vm.ssf_ded;

            // Differential values
            if (diffPeriod)
            {
                vm.DiffBasicSalary = vm.BasicSalary - vm.ActBasicSalary;
                vm.DiffPFEmployer = vm.PFEmployer - vm.ActPFEmployer;
            }
            vm.diff_act_pf_d = vm.act_pf_d - vm.pf_d;

            if (vm.Overtime > 0)
            {
                if (diffPeriod)
                {
                    var otDiff = _context.tbl_employee_overtime
                        .Where(x => x.emp_id == empId
                                 && x.sal_year == year
                                 && x.sal_month == month)
                        .Sum(x => (decimal?)x.ot_diff) ?? 0m;
                    otDiff = Math.Round(otDiff, 0);
                    decimal overtime_amt = vm.Overtime - otDiff;
                    vm.overtime_amt = Math.Round(overtime_amt, 2);
                    vm.ot_diff = otDiff;
                }
            }

            decimal NetInHandTemp = vm.TotalEarnings - vm.TotalDeductions;
            DateTime salaryDate = new DateTime(year, month, 1);

            if (salaryDate < new DateTime(2009, 7, 1))
            {
                vm.NetInHand = Math.Round(NetInHandTemp, 0);
            }
            else
            {
                vm.NetInHand = vm.NetInHandTable;
                if (salaryDate <= new DateTime(2010, 6, 1))
                {
                    vm.NetInHand = vm.NetInHandTable - vm.ChildrenEduAllowance;
                }
            }

            return vm;
        }
    }
}