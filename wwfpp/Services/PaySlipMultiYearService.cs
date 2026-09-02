using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using wwfpp.Data;
using wwfpp.Models;

namespace wwfpp.Services
{
    public class PaySlipMultiYearService
    {
        private readonly AppDbContext _context;

        public PaySlipMultiYearService(AppDbContext context)
        {
            _context = context;
        }

        public object GetFiscalWisePaySlip(int empId, string start_fiscal_year, string end_fiscal_year, string sal_fiscal_year)
        {
            int start_ = Convert.ToInt32(start_fiscal_year.Substring(0, 4));
            int end_ = Convert.ToInt32(end_fiscal_year.Substring(0, 4));
            int total_fy = end_ - start_;

            string[] arrFiscal = new string[total_fy + 1];
            decimal[] arr_basic_salary = new decimal[total_fy + 1];
            decimal[] arr_grade = new decimal[total_fy + 1];
            decimal[] arr_pf_a = new decimal[total_fy + 1];
            decimal[] arr_children_edu_all = new decimal[total_fy + 1];
            decimal[] arr_insurance = new decimal[total_fy + 1];
            decimal[] arr_performance_all = new decimal[total_fy + 1];
            decimal[] arr_gratuity = new decimal[total_fy + 1];
            decimal[] arr_gratuity_ded = new decimal[total_fy + 1];
            decimal[] arr_medical_expense_reimburse_total = new decimal[total_fy + 1];
            decimal[] arr_leave_encash = new decimal[total_fy + 1];
            decimal[] arr_ssf = new decimal[total_fy + 1];
            decimal[] arr_ssf_ded = new decimal[total_fy + 1];
            decimal[] arr_remote_area_all = new decimal[total_fy + 1];
            decimal[] arr_others = new decimal[total_fy + 1];
            decimal[] arr_overtime = new decimal[total_fy + 1];
            decimal[] arr_dashain_a = new decimal[total_fy + 1];
            decimal[] arr_pf_d = new decimal[total_fy + 1];
            decimal[] arr_cit_d = new decimal[total_fy + 1];
            decimal[] arr_tax_d = new decimal[total_fy + 1];
            decimal[] arr_betalibi_d = new decimal[total_fy + 1];
            decimal[] arr_tel_per_adv = new decimal[total_fy + 1];
            decimal[] arr_travel_prog_adv = new decimal[total_fy + 1];
            decimal[] arr_pr_adv = new decimal[total_fy + 1];
            decimal[] arr_fd_adv = new decimal[total_fy + 1];
            decimal[] arr_wl_adv = new decimal[total_fy + 1];
            decimal[] arr_adv_pf_loan = new decimal[total_fy + 1];
            decimal[] arr_adv_cit_loan = new decimal[total_fy + 1];
            decimal[] arr_welfare_fund = new decimal[total_fy + 1];
            decimal[] arr_total_earnings = new decimal[total_fy + 1];
            decimal[] arr_total_deduction = new decimal[total_fy + 1];
            decimal[] arr_net_in_hand_temp = new decimal[total_fy + 1];
            decimal[] arr_net_in_hand = new decimal[total_fy + 1];
            decimal[] arr_pre_access_tax = new decimal[total_fy + 1];
            decimal[] arr_actual_tax_d = new decimal[total_fy + 1];

            // Grand totals
            decimal g_basic_salary = 0, g_grade = 0, g_pf_a = 0, g_children_edu_all = 0, g_insurance = 0, g_performance_all = 0,
                    g_gratuity = 0, g_gratuity_ded = 0, g_medical_expense_reimburse_total = 0, g_leave_encash = 0,
                    g_ssf = 0, g_ssf_ded = 0, g_remote_area_all = 0, g_others = 0, g_overtime = 0, g_dashain_a = 0,
                    g_pf_d = 0, g_cit_d = 0, g_tax_d = 0, g_betalibi_d = 0, g_tel_per_adv = 0, g_travel_prog_adv = 0,
                    g_pr_adv = 0, g_fd_adv = 0, g_wl_adv = 0, g_adv_pf_loan = 0, g_adv_cit_loan = 0, g_welfare_fund = 0,
                    g_total_earnings = 0, g_total_deduction = 0, g_net_in_hand_temp = 0, g_net_in_hand = 0,
                    g_pre_access_tax = 0, g_actual_tax_d = 0;

            for (int i = 0; i <= total_fy; i++)
            {
                arrFiscal[i] = (start_ + i) + "/" + (start_ + i + 1);

                var record = _context.vw_year_salary_sum_fiscalwise_all
                    .FirstOrDefault(s => s.emp_id == empId && s.actual_fiscal == arrFiscal[i]);

                if (record != null)
                {
                    // Null handling
                    decimal basic_salary = record.basic_salary ?? 0;
                    decimal grade = record.grade ?? 0;
                    decimal pf_a = record.pf_a ?? 0;
                    decimal children_edu_all = record.children_edu_all ?? 0;
                    decimal insurance = record.insurance ?? 0;
                    decimal performance_all = record.performance_all ?? 0;
                    decimal remote_area_all = record.remote_area_all ?? 0;
                    decimal others = record.others ?? 0;
                    decimal overtime = record.overtime ?? 0;
                    decimal dashain_a = record.dashain_a ?? 0;
                    decimal pf_d = record.pf_d ?? 0;
                    decimal cit_d = record.cit_d ?? 0;
                    decimal incometax_d = record.incometax_d ?? 0;
                    decimal betalibi_d = record.betalibi_d ?? 0;
                    decimal gratuity = record.gratuity ?? 0;
                    decimal gratuity_ded = record.gratuity_ded ?? 0;
                    decimal medical_expense_reimburse_total = record.medical_expense_reimburse_total ?? 0;
                    decimal leave_encash = record.leave_encash ?? 0;
                    decimal ssf = record.ssf ?? 0;
                    decimal ssf_ded = record.ssf_ded ?? 0;
                    decimal tel_per_adv = record.tel_per_adv ?? 0;
                    decimal travel_prog_adv = record.travel_prog_adv ?? 0;
                    decimal pr_adv = record.pr_adv ?? 0;
                    decimal fd_adv = record.fd_adv ?? 0;
                    decimal wl_adv = record.wl_adv ?? 0;
                    decimal adv_pf_loan = record.adv_PF_loan ?? 0;
                    decimal adv_cit_loan = record.adv_CIT_loan ?? 0;
                    decimal welfare_fund = record.welfare_fund ?? 0;
                    decimal pre_access_tax = record.pre_access_tax ?? 0;
                    decimal actual_tax_d = incometax_d + pre_access_tax;

                    // Fiscal year special case
                    decimal net_addition;
                    if (sal_fiscal_year == "2009/2010")
                    {
                        children_edu_all = 0;
                        insurance = 0;
                        net_addition = basic_salary + grade + pf_a + performance_all + remote_area_all + others + overtime + dashain_a;
                    }
                    else
                    {
                        net_addition = basic_salary + grade + pf_a + children_edu_all + performance_all + remote_area_all + others + overtime + dashain_a + insurance + medical_expense_reimburse_total + leave_encash + gratuity + ssf;
                    }

                    decimal net_deduction = pf_d + cit_d + incometax_d + betalibi_d + tel_per_adv + travel_prog_adv + pr_adv + fd_adv + wl_adv + welfare_fund + adv_pf_loan + adv_cit_loan + gratuity_ded + ssf_ded;
                    decimal net_in_hand_temp = net_addition - net_deduction;
                    decimal net_in_hand = record.net_in_hand ?? 0;

                    // Round and assign to arrays
                    arr_basic_salary[i] = Math.Round(basic_salary, 2);
                    arr_grade[i] = Math.Round(grade, 2);
                    arr_pf_a[i] = Math.Round(pf_a, 2);
                    arr_children_edu_all[i] = Math.Round(children_edu_all, 2);
                    arr_insurance[i] = Math.Round(insurance, 2);
                    arr_performance_all[i] = Math.Round(performance_all, 2);
                    arr_gratuity[i] = Math.Round(gratuity, 2);
                    arr_gratuity_ded[i] = Math.Round(gratuity_ded, 2);
                    // Round and assign to arrays
                    arr_medical_expense_reimburse_total[i] = Math.Round(medical_expense_reimburse_total, 2);
                    arr_leave_encash[i] = Math.Round(leave_encash, 2);
                    arr_ssf[i] = Math.Round(ssf, 2);
                    arr_ssf_ded[i] = Math.Round(ssf_ded, 2);
                    arr_remote_area_all[i] = Math.Round(remote_area_all, 2);
                    arr_others[i] = Math.Round(others, 2);
                    arr_overtime[i] = Math.Round(overtime, 2);
                    arr_dashain_a[i] = Math.Round(dashain_a, 2);
                    arr_pf_d[i] = Math.Round(pf_d, 2);
                    arr_cit_d[i] = Math.Round(cit_d, 2);
                    arr_tax_d[i] = Math.Round(incometax_d, 2);
                    arr_betalibi_d[i] = Math.Round(betalibi_d, 2);
                    arr_tel_per_adv[i] = Math.Round(tel_per_adv, 2);
                    arr_travel_prog_adv[i] = Math.Round(travel_prog_adv, 2);
                    arr_pr_adv[i] = Math.Round(pr_adv, 2);
                    arr_fd_adv[i] = Math.Round(fd_adv, 2);
                    arr_wl_adv[i] = Math.Round(wl_adv, 2);
                    arr_adv_pf_loan[i] = Math.Round(adv_pf_loan, 2);
                    arr_adv_cit_loan[i] = Math.Round(adv_cit_loan, 2);
                    arr_welfare_fund[i] = Math.Round(welfare_fund, 2);
                    arr_total_earnings[i] = Math.Round(net_addition, 2);
                    arr_total_deduction[i] = Math.Round(net_deduction, 2);
                    arr_net_in_hand_temp[i] = Math.Round(net_in_hand_temp, 2);
                    arr_net_in_hand[i] = Math.Round(net_in_hand, 2);
                    arr_pre_access_tax[i] = Math.Round(pre_access_tax, 2);
                    arr_actual_tax_d[i] = Math.Round(actual_tax_d, 2);

                    // Accumulate grand totals
                    g_basic_salary += arr_basic_salary[i];
                    g_grade += arr_grade[i];
                    g_pf_a += arr_pf_a[i];
                    g_children_edu_all += arr_children_edu_all[i];
                    g_insurance += arr_insurance[i];
                    g_performance_all += arr_performance_all[i];
                    g_gratuity += arr_gratuity[i];
                    g_gratuity_ded += arr_gratuity_ded[i];
                    g_medical_expense_reimburse_total += arr_medical_expense_reimburse_total[i];
                    g_leave_encash += arr_leave_encash[i];
                    g_ssf += arr_ssf[i];
                    g_ssf_ded += arr_ssf_ded[i];
                    g_remote_area_all += arr_remote_area_all[i];
                    g_others += arr_others[i];
                    g_overtime += arr_overtime[i];
                    g_dashain_a += arr_dashain_a[i];
                    g_pf_d += arr_pf_d[i];
                    g_cit_d += arr_cit_d[i];
                    g_tax_d += arr_tax_d[i];
                    g_betalibi_d += arr_betalibi_d[i];
                    g_tel_per_adv += arr_tel_per_adv[i];
                    g_travel_prog_adv += arr_travel_prog_adv[i];
                    g_pr_adv += arr_pr_adv[i];
                    g_fd_adv += arr_fd_adv[i];
                    g_wl_adv += arr_wl_adv[i];
                    g_adv_pf_loan += arr_adv_pf_loan[i];
                    g_adv_cit_loan += arr_adv_cit_loan[i];
                    g_welfare_fund += arr_welfare_fund[i];
                    g_total_earnings += arr_total_earnings[i];
                    g_total_deduction += arr_total_deduction[i];
                    g_net_in_hand_temp += arr_net_in_hand_temp[i];
                    g_net_in_hand += arr_net_in_hand[i];
                    g_pre_access_tax += arr_pre_access_tax[i];
                    g_actual_tax_d += arr_actual_tax_d[i];
                }
                else
                {
                    // No record → assign 0
                    arr_basic_salary[i] = 0;
                    arr_grade[i] = 0;
                    arr_pf_a[i] = 0;
                    arr_children_edu_all[i] = 0;
                    arr_insurance[i] = 0;
                    arr_performance_all[i] = 0;
                    arr_gratuity[i] = 0;
                    arr_gratuity_ded[i] = 0;
                    arr_medical_expense_reimburse_total[i] = 0;
                    arr_leave_encash[i] = 0;
                    arr_ssf[i] = 0;
                    arr_ssf_ded[i] = 0;
                    arr_remote_area_all[i] = 0;
                    arr_others[i] = 0;
                    arr_overtime[i] = 0;
                    arr_dashain_a[i] = 0;
                    arr_pf_d[i] = 0;
                    arr_cit_d[i] = 0;
                    arr_tax_d[i] = 0;
                    arr_betalibi_d[i] = 0;
                    arr_tel_per_adv[i] = 0;
                    arr_travel_prog_adv[i] = 0;
                    arr_pr_adv[i] = 0;
                    arr_fd_adv[i] = 0;
                    arr_wl_adv[i] = 0;
                    arr_adv_pf_loan[i] = 0;
                    arr_adv_cit_loan[i] = 0;
                    arr_welfare_fund[i] = 0;
                    arr_total_earnings[i] = 0;
                    arr_total_deduction[i] = 0;
                    arr_net_in_hand_temp[i] = 0;
                    arr_net_in_hand[i] = 0;
                    arr_pre_access_tax[i] = 0;
                    arr_actual_tax_d[i] = 0;
                }
            }

            // Final grand totals rounded
            g_basic_salary = Math.Round(g_basic_salary, 2);
            g_grade = Math.Round(g_grade, 2);
            g_pf_a = Math.Round(g_pf_a, 2);
            g_children_edu_all = Math.Round(g_children_edu_all, 2);
            g_insurance = Math.Round(g_insurance, 2);
            g_performance_all = Math.Round(g_performance_all, 2);
            g_gratuity = Math.Round(g_gratuity, 2);
            g_gratuity_ded = Math.Round(g_gratuity_ded, 2);
            g_medical_expense_reimburse_total = Math.Round(g_medical_expense_reimburse_total, 2);
            g_leave_encash = Math.Round(g_leave_encash, 2);
            g_ssf = Math.Round(g_ssf, 2);
            g_ssf_ded = Math.Round(g_ssf_ded, 2);
            g_remote_area_all = Math.Round(g_remote_area_all, 2);
            g_others = Math.Round(g_others, 2);
            g_overtime = Math.Round(g_overtime, 2);
            g_dashain_a = Math.Round(g_dashain_a, 2);
            g_pf_d = Math.Round(g_pf_d, 2);
            g_cit_d = Math.Round(g_cit_d, 2);
            g_tax_d = Math.Round(g_tax_d, 2);
            g_betalibi_d = Math.Round(g_betalibi_d, 2);
            g_tel_per_adv = Math.Round(g_tel_per_adv, 2);
            g_travel_prog_adv = Math.Round(g_travel_prog_adv, 2);
            g_pr_adv = Math.Round(g_pr_adv, 2);
            g_fd_adv = Math.Round(g_fd_adv, 2);
            g_wl_adv = Math.Round(g_wl_adv, 2);
            g_adv_pf_loan = Math.Round(g_adv_pf_loan, 2);
            g_adv_cit_loan = Math.Round(g_adv_cit_loan, 2);
            g_welfare_fund = Math.Round(g_welfare_fund, 2);
            g_total_earnings = Math.Round(g_total_earnings, 2);
            g_total_deduction = Math.Round(g_total_deduction, 2);
            g_net_in_hand_temp = Math.Round(g_net_in_hand_temp, 2);
            g_net_in_hand = Math.Round(g_net_in_hand, 2);
            g_pre_access_tax = Math.Round(g_pre_access_tax, 2);
            g_actual_tax_d = Math.Round(g_actual_tax_d, 2);

            // Return arrays + grand totals
            return new
            {
                arrFiscal,
                arr_basic_salary,
                arr_grade,
                arr_pf_a,
                arr_children_edu_all,
                arr_insurance,
                arr_performance_all,
                arr_gratuity,
                arr_gratuity_ded,
                arr_medical_expense_reimburse_total,
                arr_leave_encash,
                arr_ssf,
                arr_ssf_ded,
                arr_remote_area_all,
                arr_others,
                arr_overtime,
                arr_dashain_a,
                arr_pf_d,
                arr_cit_d,
                arr_tax_d,
                arr_betalibi_d,
                arr_tel_per_adv,
                arr_travel_prog_adv,
                arr_pr_adv,
                arr_fd_adv,
                arr_wl_adv,
                arr_adv_pf_loan,
                arr_adv_cit_loan,
                arr_welfare_fund,
                arr_total_earnings,
                arr_total_deduction,
                arr_net_in_hand_temp,
                arr_net_in_hand,
                arr_pre_access_tax,
                arr_actual_tax_d,

                // Grand totals
                g_basic_salary,
                g_grade,
                g_pf_a,
                g_children_edu_all,
                g_insurance,
                g_performance_all,
                g_gratuity,
                g_gratuity_ded,
                g_medical_expense_reimburse_total,
                g_leave_encash,
                g_ssf,
                g_ssf_ded,
                g_remote_area_all,
                g_others,
                g_overtime,
                g_dashain_a,
                g_pf_d,
                g_cit_d,
                g_tax_d,
                g_betalibi_d,
                g_tel_per_adv,
                g_travel_prog_adv,
                g_pr_adv,
                g_fd_adv,
                g_wl_adv,
                g_adv_pf_loan,
                g_adv_cit_loan,
                g_welfare_fund,
                g_total_earnings,
                g_total_deduction,
                g_net_in_hand_temp,
                g_net_in_hand,
                g_pre_access_tax,
                g_actual_tax_d
            };
        }
    }
}