using DocumentFormat.OpenXml.InkML;
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

    // Multi-month payslip calculation skeleton
    public class PaySlipCalculator
    {
        private readonly AppDbContext _context;

        // Constructor so you can inject DbContext
        public PaySlipCalculator(AppDbContext context)
        {
            _context = context;
        }
        // Arrays (Lists) in same sequence as Classic ASP
        public List<decimal?> arrSession = new();
        public List<decimal?> ary_basic_salary = new();
        public List<decimal?> ary_act_basic_salary = new();
        public List<decimal?> ary_pf_a = new();
        public List<decimal?> ary_act_pf_a = new();
        public List<decimal?> ary_children_edu_all_ = new();
        public List<decimal?> ary_insurance_ = new();
        public List<decimal?> ary_overtime = new();
        public List<decimal?> ary_ot_diff = new();
        public List<decimal?> ary_remote_area_all = new();
        public List<decimal?> ary_dashain_a = new();
        public List<decimal?> ary_performance_all = new();
        public List<decimal?> ary_gratuity = new();
        public List<decimal?> ary_ssf = new();
        public List<decimal?> ary_medical_expense_reimburse_total = new();
        public List<decimal?> ary_leave_encash = new();
        public List<decimal?> ary_others = new();
        public List<decimal?> ary_tel_per_adv = new();
        public List<decimal?> ary_travel_prog_adv = new();
        public List<decimal?> ary_fd_adv = new();
        public List<decimal?> ary_pr_adv = new();
        public List<decimal?> ary_wl_adv = new();
        public List<decimal?> ary_adv_pf_loan = new();
        public List<decimal?> ary_adv_cit_loan = new();
        public List<decimal?> ary_welfare_fund = new();
        public List<decimal?> ary_pf_d = new();
        public List<decimal?> ary_act_pf_d = new();
        public List<decimal?> ary_cit_d = new();
        public List<decimal?> ary_gratuity_ded = new();
        public List<decimal?> ary_ssf_ded = new();
        public List<decimal?> ary_tax_d = new();
        public List<decimal?> ary_betalibi_d = new();
        public List<decimal?> ary_total_earnings = new();
        public List<decimal?> ary_total_deduction = new();
        public List<decimal?> ary_net_in_hand = new();
        public List<decimal?> ary_pre_access_tax = new();
        public List<decimal?> ary_actual_tax_d = new();

        public List<decimal?> ary_diff_act_basic_salary = new();
        public List<decimal?> ary_diff_act_pf_a = new();
        public List<decimal?> ary_diff_act_pf_d = new();

        // Aggregate totals (gr_ variables)
        public decimal gr_basic_salary = 0;
        public decimal gr_pf_a = 0;
        public decimal gr_children_edu_all_ = 0;
        public decimal gr_insurance_ = 0;
        public decimal gr_overtime = 0;
        public decimal gr_ot_diff = 0;
        public decimal gr_remote_area_all = 0;
        public decimal gr_dashain_a = 0;
        public decimal gr_performance_all = 0;
        public decimal gr_gratuity = 0;
        public decimal gr_gratuity_ded = 0;
        public decimal gr_medical_expense_reimburse_total = 0;
        public decimal gr_leave_encash = 0;
        public decimal gr_ssf = 0;
        public decimal gr_ssf_ded = 0;
        public decimal gr_others = 0;
        public decimal gr_tel_per_adv = 0;
        public decimal gr_travel_prog_adv = 0;
        public decimal gr_fd_adv = 0;
        public decimal gr_pr_adv = 0;
        public decimal gr_wl_adv = 0;
        public decimal gr_adv_pf_loan = 0;
        public decimal gr_adv_cit_loan = 0;
        public decimal gr_welfare_fund = 0;
        public decimal gr_pf_d = 0;
        public decimal gr_cit_d = 0;
        public decimal gr_tax_d = 0;
        public decimal gr_betalibi_d = 0;
        public decimal gr_total_earnings = 0;
        public decimal gr_total_deduction = 0;
        public decimal gr_net_in_hand = 0;
        public decimal gr_pre_access_tax = 0;
        public decimal gr_actual_tax_d = 0;
        public decimal gr_act_basic_salary = 0;
        public decimal gr_act_pf_a = 0;
        public decimal gr_act_pf_d = 0;
        public decimal gr_diff_act_basic_salary = 0;
        public decimal gr_diff_act_pf_a = 0;
        public decimal gr_diff_act_pf_d = 0;

        // Main calculation method
        public void Calculate(int emp_id, int start_month, int start_year, int end_month, int end_year)
        {
            var start_ = new DateTime(start_year, start_month, 1);
            var end_ = new DateTime(end_year, end_month, 1);
            var total_fy = ((end_.Year - start_.Year) * 12) + end_.Month - start_.Month;

            // Build arrSession (month sequence)
            for (int i = 0; i <= total_fy; i++)
            {
                arrSession.Add(start_.AddMonths(i).Month);
            }

            for (int i = 0; i < arrSession.Count; i++)
            {
                int start_year_ = start_.AddMonths(i).Year;
                int start_month_ = start_.AddMonths(i).Month;

                // First try tbl_employee_salary
                dynamic rs = (from a in _context.tbl_employee
                              join b in _context.tbl_employee_salary
                              on a.emp_id equals b.emp_id
                              where a.emp_id == emp_id
                              && b.sal_year == start_year_
                              && b.sal_month == start_month_
                              select new { a, b }).FirstOrDefault();

                if (rs == null)
                {
                    // Fallback to tbl_employee_salary_a_field
                    rs = (from a in _context.tbl_employee
                          join b in _context.tbl_employee_salary_a_field
                          on a.emp_id equals b.emp_id
                          where a.emp_id == emp_id
                          && b.sal_year == start_year_
                          && b.sal_month == start_month_
                          select new { a, b }).FirstOrDefault();
                }


                if (rs != null)
                {
                    // Example: basic_salary
                    decimal basic_salary = rs.b.basic_salary ?? 0;
                    decimal act_basic_salary = rs.b.act_basic_salary ?? 0;
                    decimal pf_a = rs.b.pf_a ?? 0;
                    decimal act_pf_a = rs.b.act_pf_a ?? 0;
                    decimal children_edu_all = rs.b.children_edu_all ?? 0;
                    decimal insurance = rs.b.insurance ?? 0;

                    // Special case: before June 2010
                    decimal children_edu_all_ = (start_ <= new DateTime(2010, 6, 1)) ? 0 : children_edu_all;
                    decimal insurance_ = (start_ <= new DateTime(2010, 6, 1)) ? 0 : insurance;

                    // Other fields
                    decimal gratuity = rs.b.gratuity ?? 0;
                    decimal gratuity_ded = rs.b.gratuity_ded ?? 0;
                    decimal ssf = rs.b.ssf ?? 0;
                    decimal ssf_ded = rs.b.ssf_ded ?? 0;
                    decimal medical_expense_reimburse_total = rs.b.medical_expense_reimburse_total ?? 0;
                    decimal leave_encash = rs.b.leave_encash ?? 0;
                    decimal overtime = rs.b.overtime ?? 0;
                    decimal remote_area_all = rs.b.remote_area_all ?? 0;
                    decimal dashain_a = rs.b.dashain_a ?? 0;
                    decimal performance_all = rs.b.performance_all ?? 0;
                    decimal others = rs.b.others ?? 0;
                    decimal pf_d = rs.b.pf_d ?? 0;
                    decimal act_pf_d = rs.b.act_pf_d ?? 0;
                    decimal cit_d = rs.b.cit_d ?? 0;
                    decimal tax_d = rs.b.incometax_d ?? 0;
                    decimal betalibi_d = rs.b.betalibi_d ?? 0;

                    // Advances
                    decimal tel_per_adv = rs.b.tel_per_adv ?? 0;
                    decimal travel_prog_adv = rs.b.travel_prog_adv ?? 0;
                    decimal fd_adv = rs.b.fd_adv ?? 0;
                    decimal pr_adv = rs.b.pr_adv ?? 0;
                    decimal wl_adv = rs.b.wl_adv ?? 0;
                    decimal adv_pf_loan = rs.b.adv_PF_loan ?? 0;
                    decimal adv_cit_loan = rs.b.adv_CIT_loan ?? 0;
                    decimal welfare_fund = rs.b.welfare_fund ?? 0;

                    // Totals
                    decimal total_earnings = basic_salary + pf_a + children_edu_all_ + overtime + remote_area_all +
                                             dashain_a + performance_all + others + insurance_ +
                                             medical_expense_reimburse_total + leave_encash + gratuity + ssf;

                    decimal total_deduction = pf_d + cit_d + tax_d + betalibi_d + tel_per_adv + travel_prog_adv +
                                              fd_adv + pr_adv + wl_adv + welfare_fund + adv_pf_loan + adv_cit_loan +
                                              gratuity_ded + ssf_ded;

                    decimal net_in_hand_temp = total_earnings - total_deduction;

                    decimal pre_access_tax = rs.b.pre_access_tax ?? 0;
                    decimal actual_tax_d = tax_d + pre_access_tax;

                    decimal net_in_hand;
                    if (start_ < new DateTime(2009, 7, 1))
                    {
                        net_in_hand = Math.Round(net_in_hand_temp);
                    }
                    else
                    {
                        net_in_hand = rs.b.net_in_hand ?? 0;
                        if (start_ <= new DateTime(2010, 6, 1))
                            net_in_hand -= children_edu_all;
                    }

                    // Overtime diff
                    var ot_diff = _context.tbl_employee_overtime
                        .Where(o => o.emp_id == emp_id && o.sal_year == start_year_ && o.sal_month == start_month_)
                        .Sum(o => (decimal?)o.ot_diff) ?? 0;

                    decimal overtime_amt = Math.Round(overtime - ot_diff, 2);

                    // Differences
                    decimal diff_act_basic_salary = basic_salary - act_basic_salary;
                    decimal diff_act_pf_a = pf_a - act_pf_a;
                    decimal diff_act_pf_d = pf_d - act_pf_d;

                    // Store into arrays (rounded)
                    ary_basic_salary.Add(Math.Round(basic_salary, 2));
                    ary_act_basic_salary.Add(Math.Round(act_basic_salary, 2));
                    ary_pf_a.Add(Math.Round(pf_a, 2));
                    ary_act_pf_a.Add(Math.Round(act_pf_a, 2));
                    ary_children_edu_all_.Add(Math.Round(children_edu_all_, 2));
                    ary_insurance_.Add(Math.Round(insurance_, 2));
                    ary_overtime.Add(Math.Round(overtime, 2));
                    ary_ot_diff.Add(Math.Round(ot_diff, 2));
                    ary_remote_area_all.Add(Math.Round(remote_area_all, 2));
                    ary_dashain_a.Add(Math.Round(dashain_a, 2));
                    ary_performance_all.Add(Math.Round(performance_all, 2));
                    ary_gratuity.Add(Math.Round(gratuity, 2));
                    ary_gratuity_ded.Add(Math.Round(gratuity_ded, 2));
                    ary_ssf.Add(Math.Round(ssf, 2));
                    ary_ssf_ded.Add(Math.Round(ssf_ded, 2));
                    ary_medical_expense_reimburse_total.Add(Math.Round(medical_expense_reimburse_total, 2));
                    ary_leave_encash.Add(Math.Round(leave_encash, 2));
                    ary_others.Add(Math.Round(others, 2));
                    ary_tel_per_adv.Add(Math.Round(tel_per_adv, 2));
                    ary_travel_prog_adv.Add(Math.Round(travel_prog_adv, 2));
                    ary_fd_adv.Add(Math.Round(fd_adv, 2));
                    ary_pr_adv.Add(Math.Round(pr_adv, 2));
                    ary_wl_adv.Add(Math.Round(wl_adv, 2));
                    ary_adv_pf_loan.Add(Math.Round(adv_pf_loan, 2));
                    ary_adv_cit_loan.Add(Math.Round(adv_cit_loan, 2));
                    ary_welfare_fund.Add(Math.Round(welfare_fund, 2));
                    ary_pf_d.Add(Math.Round(pf_d, 2));
                    ary_act_pf_d.Add(Math.Round(act_pf_d, 2));
                    ary_cit_d.Add(Math.Round(cit_d, 2));
                    ary_tax_d.Add(Math.Round(tax_d, 2));
                    ary_betalibi_d.Add(Math.Round(betalibi_d, 2));
                    ary_total_earnings.Add(Math.Round(total_earnings, 2));
                    ary_total_deduction.Add(Math.Round(total_deduction, 2));
                    ary_net_in_hand.Add(Math.Round(net_in_hand, 2));
                    ary_pre_access_tax.Add(Math.Round(pre_access_tax, 2));
                    ary_actual_tax_d.Add(Math.Round(actual_tax_d, 2));

                    ary_diff_act_basic_salary.Add(Math.Round(diff_act_basic_salary, 2));
                    ary_diff_act_pf_a.Add(Math.Round(diff_act_pf_a, 2));
                    ary_diff_act_pf_d.Add(Math.Round(diff_act_pf_d, 2));


                    // Aggregate totals
                    gr_basic_salary += basic_salary;
                    gr_pf_a += pf_a;
                    gr_children_edu_all_ += children_edu_all_;
                    gr_insurance_ += insurance_;
                    gr_overtime += overtime - ot_diff;
                    gr_ot_diff += ot_diff;
                    gr_remote_area_all += remote_area_all;
                    gr_dashain_a += dashain_a;
                    gr_performance_all += performance_all;

                    gr_gratuity += gratuity;
                    gr_gratuity_ded += gratuity_ded;
                    gr_medical_expense_reimburse_total += medical_expense_reimburse_total;
                    gr_leave_encash += leave_encash;

                    gr_ssf += ssf;
                    gr_ssf_ded += ssf_ded;

                    gr_others += others;
                    gr_tel_per_adv += tel_per_adv;
                    gr_travel_prog_adv += travel_prog_adv;
                    gr_fd_adv += fd_adv;
                    gr_pr_adv += pr_adv;
                    gr_wl_adv += wl_adv;
                    gr_adv_pf_loan += adv_pf_loan;
                    gr_adv_cit_loan += adv_cit_loan;
                    gr_welfare_fund += welfare_fund;
                    gr_pf_d += pf_d;
                    gr_cit_d += cit_d;
                    gr_tax_d += tax_d;
                    gr_betalibi_d += betalibi_d;
                    gr_total_earnings += total_earnings;
                    gr_total_deduction += total_deduction;
                    gr_net_in_hand += net_in_hand;
                    gr_pre_access_tax += pre_access_tax;
                    gr_actual_tax_d += actual_tax_d;

                    gr_act_basic_salary += act_basic_salary;
                    gr_act_pf_a += act_pf_a;
                    gr_act_pf_d += act_pf_d;

                    gr_diff_act_basic_salary += diff_act_basic_salary;
                    gr_diff_act_pf_a += diff_act_pf_a;
                    gr_diff_act_pf_d += diff_act_pf_d;
                }
                else
                {
                    // No record → push zeros so arrays stay aligned
                    ary_total_earnings.Add(0);
                    ary_basic_salary.Add(0);
                    ary_diff_act_basic_salary.Add(0);
                    ary_pf_a.Add(0);
                    ary_diff_act_pf_a.Add(0);
                    ary_children_edu_all_.Add(0);
                    ary_insurance_.Add(0);
                    ary_overtime.Add(0);
                    ary_ot_diff.Add(0);
                    ary_remote_area_all.Add(0);
                    ary_dashain_a.Add(0);
                    ary_performance_all.Add(0);
                    ary_gratuity.Add(0);
                    ary_ssf.Add(0);
                    ary_medical_expense_reimburse_total.Add(0);
                    ary_leave_encash.Add(0);
                    ary_others.Add(0);

                    ary_total_deduction.Add(0);
                    ary_cit_d.Add(0);
                    ary_gratuity_ded.Add(0);
                    ary_ssf_ded.Add(0);
                    ary_pf_d.Add(0);
                    ary_diff_act_pf_d.Add(0);
                    ary_tax_d.Add(0);
                    ary_betalibi_d.Add(0);

                    ary_tel_per_adv.Add(0);
                    ary_pr_adv.Add(0);
                    ary_travel_prog_adv.Add(0);
                    ary_fd_adv.Add(0);
                    ary_wl_adv.Add(0);
                    ary_adv_pf_loan.Add(0);
                    ary_adv_cit_loan.Add(0);
                    ary_welfare_fund.Add(0);

                    ary_net_in_hand.Add(0);
                }

            }

            // After loop, format totals (round to 2 decimals)
            gr_basic_salary = Math.Round(gr_basic_salary, 2);
            gr_pf_a = Math.Round(gr_pf_a, 2);
            gr_children_edu_all_ = Math.Round(gr_children_edu_all_, 2);
            gr_insurance_ = Math.Round(gr_insurance_, 2);
            gr_overtime = Math.Round(gr_overtime, 2);
            gr_ot_diff = Math.Round(gr_ot_diff, 2);
            gr_remote_area_all = Math.Round(gr_remote_area_all, 2);
            gr_dashain_a = Math.Round(gr_dashain_a, 2);
            gr_performance_all = Math.Round(gr_performance_all, 2);

            gr_gratuity = Math.Round(gr_gratuity, 2);
            gr_gratuity_ded = Math.Round(gr_gratuity_ded, 2);
            gr_medical_expense_reimburse_total = Math.Round(gr_medical_expense_reimburse_total, 2);
            gr_leave_encash = Math.Round(gr_leave_encash, 2);

            gr_ssf = Math.Round(gr_ssf, 2);
            gr_ssf_ded = Math.Round(gr_ssf_ded, 2);

            gr_others = Math.Round(gr_others, 2);
            gr_tel_per_adv = Math.Round(gr_tel_per_adv, 2);
            gr_travel_prog_adv = Math.Round(gr_travel_prog_adv, 2);
            gr_fd_adv = Math.Round(gr_fd_adv, 2);
            gr_pr_adv = Math.Round(gr_pr_adv, 2);
            gr_wl_adv = Math.Round(gr_wl_adv, 2);
            gr_adv_pf_loan = Math.Round(gr_adv_pf_loan, 2);
            gr_adv_cit_loan = Math.Round(gr_adv_cit_loan, 2);
            gr_welfare_fund = Math.Round(gr_welfare_fund, 2);
            gr_pf_d = Math.Round(gr_pf_d, 2);
            gr_cit_d = Math.Round(gr_cit_d, 2);
            gr_tax_d = Math.Round(gr_tax_d, 2);
            gr_betalibi_d = Math.Round(gr_betalibi_d, 2);

            gr_total_earnings = Math.Round(gr_total_earnings, 2);
            gr_total_deduction = Math.Round(gr_total_deduction, 2);
            gr_net_in_hand = Math.Round(gr_net_in_hand, 2);
            gr_pre_access_tax = Math.Round(gr_pre_access_tax, 2);
            gr_actual_tax_d = Math.Round(gr_actual_tax_d, 2);

            gr_act_basic_salary = Math.Round(gr_act_basic_salary, 2);
            gr_act_pf_a = Math.Round(gr_act_pf_a, 2);
            gr_act_pf_d = Math.Round(gr_act_pf_d, 2);

            gr_diff_act_basic_salary = Math.Round(gr_diff_act_basic_salary, 2);
            gr_diff_act_pf_a = Math.Round(gr_diff_act_pf_a, 2);
            gr_diff_act_pf_d = Math.Round(gr_diff_act_pf_d, 2);

        }

    }

    public class PaySlipMultiPeriod
    {
        private readonly AppDbContext _context;

        public PaySlipMultiPeriod(AppDbContext context)
        {
            _context = context;
        }

        // Service method that uses PaySlipCalculator
        public PaySlipCalculator GetMultiMonthPaySlip(int empId, int startMonth, int startYear, int endMonth, int endYear, string userAccess)
        {
            var calculator = new PaySlipCalculator(_context);

            // Call the Calculate method
            calculator.Calculate(empId, startMonth, startYear, endMonth, endYear);

            return calculator;
        }


    }
}