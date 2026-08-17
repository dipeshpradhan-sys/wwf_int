using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;

namespace wwfpp.Services
{
    public class FiscalYearSetting
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
    public class PayrollServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PayrollServices(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            GlobalOptionServices globalOptionServices,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _globalOptionServices = globalOptionServices;
            _httpContextAccessor = httpContextAccessor;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-15
        ****************************************************************************************************/
        public decimal GetSwfLoanBulkPaid(string parm_id)
        {
            if (string.IsNullOrWhiteSpace(parm_id)) { return 0; }
            decimal totalAmount = _context.tbl_employee_swf_loan_direct_settle
                .Where(settle => settle.swf_loan_id == parm_id)
                .Sum(settle => settle.amount) ?? 0;
            return totalAmount;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-15
        ****************************************************************************************************/
        public decimal GetSwfLoanPaidHistory(int empId, DateTime fiscalStart, decimal targetTotal, string loanId)
        {
            decimal fnPaidLoan = 0m;

            var settleDates = _context.tbl_employee_swf_loan_direct_settle
                .Where(s => s.swf_loan_id == loanId)
                .Select(s => s.s_date)
                .FirstOrDefault();

            DateTime? fn_s_date = null;
            if (settleDates != null)
            {
                fn_s_date = new DateTime(settleDates.Value.Year, settleDates.Value.Month, 1).AddDays(-1);
            }

            IQueryable<vw_swf_payback> paybackQuery;
            if (fn_s_date.HasValue)
            {
                paybackQuery = _context.vw_swf_payback
                    .Where(q => q.loan != 0
                                && q.emp_id == empId
                                && q.fiscal >= fiscalStart
                                && q.fiscal <= fn_s_date.Value)
                    .OrderBy(q => q.fiscal);
            }
            else
            {
                paybackQuery = _context.vw_swf_payback
                    .Where(q => q.loan != 0
                                && q.emp_id == empId
                                && q.fiscal >= fiscalStart)
                    .OrderBy(q => q.fiscal);
            }
            var paybacks = paybackQuery.ToList();
            foreach (var pb in paybacks)
            {
                if (Math.Round(fnPaidLoan, 2) == Math.Round(targetTotal, 2)) { break; }
                fnPaidLoan += Convert.ToDecimal(pb.loan);
            }

            return fnPaidLoan;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-15
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingActiveSWFLoan()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_employee_swf_loan
                        .Where(l => l.status == "A")
                        .Select(l => l.emp_id)
                        .Distinct()
                        .Contains(emp.emp_id))
                        .OrderBy(emp => emp.firstname)   // optional if you want ordering
                        .ThenBy(emp => emp.middlename)
                        .ThenBy(emp => emp.lastname)
                        .Select(emp => new EmployeeDropDownViewModel
                        {
                            emp_id = emp.emp_id,
                            emp_name_code = string.Join(" ",
                            new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }
                            .Where(x => !string.IsNullOrEmpty(x)))
                        })
                    .ToList();

            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Aug-10
        * Contribution: 
        ****************************************************************************************************/
        public string GetGLCode(string parm_staff_type, string parm_gl_type)
        {
            var fnStr = _context.tbl_settings_gl_codes
                                .Where(g => g.staff_type == parm_staff_type
                                         && g.gl_type == parm_gl_type)
                                .Select(g => g.gl_code)
                                .FirstOrDefault();

            return fnStr ?? string.Empty;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-10
        * Contribution: 
        ****************************************************************************************************/
        public FiscalYearSetting GetFiscalStartEndDate(string fiscalYear)
        {
            var opt = _context.tbl_fiscal_year
                .Where(e => e.fiscal_year == fiscalYear)
                .Select(e => new FiscalYearSetting
                {
                    StartDate = Convert.ToDateTime(e.date_from),   // assuming date_from is DateTime in your EF model
                    EndDate = Convert.ToDateTime(e.date_to)      // assuming date_to is DateTime in your EF model
                })
                .FirstOrDefault();

            // Fallback if no record found
            if (opt == null)
            {
                return new FiscalYearSetting
                {
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddYears(1)
                };
            }

            return opt;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-10
        * Contribution: 
        ****************************************************************************************************/
        public (int SalYear, int SalMonth, bool Show) GetDashainAllowanceYearMonth(string fiscalYear)
        {
            int? salYear = null;
            int? salMonth = null;
            bool blnShow = false;

            var FiscalDate = GetFiscalStartEndDate(fiscalYear);
            DateTime FiscalStartDate = FiscalDate.StartDate;
            DateTime FiscalEndDate = FiscalDate.EndDate;
            // First query: distinct year/month from tbl_employee_salary
            var salaryRecord = _context.tbl_employee_salary
                .Where(s => s.submit_date >= FiscalStartDate &&
                            s.submit_date <= FiscalEndDate &&
                            s.is_dashain == "Y")
                .Select(s => new { s.sal_year, s.sal_month })
                .Distinct()
                .FirstOrDefault();

            if (salaryRecord != null)
            {
                salYear = salaryRecord.sal_year;
                salMonth = salaryRecord.sal_month;
            }

            //Second query: check dashain distribution already saved
            var dashainRecord = _context.tbl_employee_dashain_allowance
                .Where(d => d.fiscal_year == fiscalYear && d.counter == 1)
                .OrderBy(d => d.id) // mimic TOP 1
                .FirstOrDefault();

            if (dashainRecord != null)
            {
                blnShow = true;
                salYear = dashainRecord.sal_year ?? salYear;
                salMonth = dashainRecord.sal_month ?? salMonth;
            }
            else
            {
                blnShow = false;
            }

            // Fallbacks if null
            if (!salYear.HasValue) { salYear = DateTime.Now.Year; }
            if (!salMonth.HasValue) { salMonth = DateTime.Now.Month; }

            return (salYear.Value, salMonth.Value, blnShow);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList GetGRGroupList(string selvalue = "")
        {
            var options = new Dictionary<string, string> { { "A", "Addition + Deduction" }, { "B", "Deduction Only" }, { "C", "Neither addition nor deduction" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList GetGRTypeList(string selvalue = "")
        {
            string gr_percent = _globalOptionServices.OptionServices["OP_float_gratuity_percentage_value"];
            var options = new Dictionary<string, string> { { "B", $"{gr_percent}% in Basic Salary" }, { "F", "Fixed Amount" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList PaidHistory(string selvalue = "")
        {
            var options = new Dictionary<string, string> { { "1", "Without loan paid history" }, { "2", "With loan paid history" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList GetPFTypeList(string selvalue = "")
        {
            var options = new Dictionary<string, string> { { "B", "10% of Basic Salary" }, { "F", "Fixed Amount" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList GetPFGroupList(string selvalue = "")
        {
            var options = new Dictionary<string, string> { { "A", "Addition + Deduction" }, { "B", "Deduction Only" }, { "C", "Neither addition nor deduction" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-08
        * Contribution: 
        ****************************************************************************************************/
        public SelectList CITType(string selvalue = "")
        {
            var options = new Dictionary<string, string> { { "B", "% in Basic Salary" }, { "T", "Max Amount" }, { "F", "Fixed Amount" } };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************/
    }
}
