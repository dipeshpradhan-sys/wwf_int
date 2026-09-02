using Azure.Core;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Runtime.Intrinsics.X86;
using System.Text;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Models.Payroll;
using wwfpp.Models.Request;
using wwfpp.Models.Settings;
using wwfpp.Services;

using static GblUtilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Controllers
{
    public class PayrollController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly EmailService _emailService;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly EmployeeServices _employeeServices;
        private readonly SettingsServices _settingsServices;
        private readonly AccountServices _accountServices;
        private readonly PayrollServices _payrollServices;
        private readonly PaySlipManager _paySlipManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LeaveAccrualServices _leaveAccrualServices;
        public PayrollController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            EmailService emailService,
            GlobalOptionServices globalOptionServices,
            EmployeeServices employeeServices,
            SettingsServices settingsServices,
            AccountServices accountServices,
            PayrollServices payrollServices,
            PaySlipManager paySlipManager,
            IWebHostEnvironment webHostEnvironment,
            LeaveAccrualServices leaveAccrualServices
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _emailService = emailService;
            _globalOptionServices = globalOptionServices;
            _employeeServices = employeeServices;
            _settingsServices = settingsServices;
            _accountServices = accountServices;
            _payrollServices = payrollServices;
            _paySlipManager = paySlipManager;
            _webHostEnvironment = webHostEnvironment;
            _leaveAccrualServices = leaveAccrualServices;
        }

        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        #region 10916 SWF LOAN
        [HttpGet]
        public IActionResult SWFLoan()
        {
            string PageId = "10916";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (from emp in _context.tbl_employee
                           join lft in _context.tbl_employee_swf_loan
                           on emp.emp_id equals lft.emp_id
                           orderby lft.start_year descending, lft.start_month descending
                           select new SwfLoanViewModel
                           {
                               emp_id = lft.emp_id ?? 0,
                               start_year = lft.start_year,
                               start_month = lft.start_month,
                               amount = lft.amount,
                               int_amount = lft.int_amount,
                               total_loan = lft.amount + lft.int_amount,
                               no_of_installment = lft.no_of_installment,
                               status = lft.status,
                               remarks = lft.remarks,
                               firstname = emp.firstname,
                               middlename = emp.middlename,
                               lastname = emp.lastname,
                               employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                               emp_status = emp.emp_status
                           }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.LoanStatusFilter = StatusActivePassive("AP", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/_SWFLoan", "ADD|DEL", PageId, Records.Count);
            return PartialView("Payroll/_SWFLoan", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SWFLoanList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var EmployeeStatusFilter = request.FilterValue1;
            var LoanStatusFilter = request.FilterValue2;

            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_swf_loan
                        on emp.emp_id equals lft.emp_id
                        select new SwfLoanViewModel
                        {
                            id = lft.id,
                            emp_id = lft.emp_id ?? 0,
                            start_year = lft.start_year,
                            start_month = lft.start_month,
                            amount = lft.amount,
                            int_amount = lft.int_amount,
                            no_of_installment = lft.no_of_installment,
                            total_loan = (lft.amount ?? 0) + (lft.int_amount ?? 0),
                            month_installment = Math.Round(((lft.amount ?? 0) + (lft.int_amount ?? 0)) / (lft.no_of_installment ?? 0), 2),
                            status = lft.status,
                            remarks = lft.remarks,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status
                        };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);
            }
            if (!string.IsNullOrEmpty(LoanStatusFilter))
            {
                query = query.Where(d => d.status == LoanStatusFilter);
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    if (sortColumn != "month_installment") { query = query.OrderBy(sortColumn + " " + sortColumnDir); }
                }
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    a.firstname != null && a.firstname.Contains(searchValue) ||
                    a.middlename != null && a.middlename.Contains(searchValue) ||
                    a.lastname != null && a.lastname.Contains(searchValue) ||
                    a.remarks != null && a.remarks.Contains(searchValue)
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        public IActionResult SWFLoanAddEdit(string id, string mode)
        {
            string PageId = "10916";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.Status = StatusActivePassive("AP", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            SwfLoanViewModel model;

            model = new SwfLoanViewModel();
            if (mode == "add")
            {
                ViewBag.EmployeeList = _payrollServices.GetEmployeeNotHavingActiveSWFLoan();
                return PartialView("Payroll/_SWFLoanAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == null)
                {
                    return Json(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var sw = (from emp in _context.tbl_employee
                              join e in _context.tbl_employee_swf_loan
                              on emp.emp_id equals e.emp_id
                              where e.id == id.ToString()
                              select new
                              {
                                  e.id,
                                  e.emp_id,
                                  e.start_year,
                                  e.start_month,
                                  e.amount,
                                  e.int_amount,
                                  e.no_of_installment,
                                  e.status,
                                  e.remarks,
                                  total_loan = Math.Round((e.amount ?? 0) + (e.int_amount ?? 0), 2),
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();
                    if (sw == null) { return Json(new { success = false, message = Lang.msg_error }); }

                    model = new SwfLoanViewModel
                    {
                        id = sw.id,
                        emp_id = sw.emp_id ?? 0,
                        start_year = sw.start_year,
                        start_month = sw.start_month,
                        amount = Math.Round(sw.amount ?? 0, 2),
                        int_amount = Math.Round(sw.int_amount ?? 0, 2),
                        no_of_installment = sw.no_of_installment,
                        status = sw.status,
                        remarks = sw.remarks,
                        total_loan = sw.total_loan,
                        month_installment = Math.Round(((sw.amount ?? 0) + (sw.int_amount ?? 0)) / (sw.no_of_installment ?? 0), 2),
                        employee = sw.employee,
                        emp_status = sw.emp_status
                    };

                    decimal totalLoan = sw.total_loan;
                    DateTime fiscal = new DateTime(Convert.ToInt32(sw.start_year), Convert.ToInt32(sw.start_month), 1);
                    decimal totalBulkPaid = _payrollServices.GetSwfLoanBulkPaid(id);
                    decimal totalMonltyPaid = _payrollServices.GetSwfLoanPaidHistory(sw.emp_id ?? 0, fiscal, totalLoan, sw.id);
                    decimal totalPaid = totalMonltyPaid + totalBulkPaid;
                    decimal totalDue = totalLoan - totalPaid;

                    ViewBag.TotalPaid = Math.Round(totalPaid, 2);
                    ViewBag.TotalDue = Math.Round(totalDue, 2);

                    var result = (from settle in _context.tbl_employee_swf_loan_direct_settle
                                  where settle.swf_loan_id == id.ToString()
                                  select new
                                  {
                                      s_amount = (decimal?)settle.amount,
                                      settle.s_date,
                                      s_remarks = settle.remarks
                                  }).FirstOrDefault();
                    if (result != null)
                    {
                        model.paid_amount = Math.Round(result.s_amount ?? 0, 2);
                        model.s_remarks = result.s_remarks;
                        model.s_date = result.s_date;
                    }
                    return PartialView("Payroll/_SWFLoanAddEdit", model);

                }
            }
            else
            {
                return Json(new { success = false, message = Lang.msg_error });
            }
        }
        public JsonResult SWFLoanSave(SwfLoanViewModel model)
        {
            ModelState.Remove("id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            int? emp_id = model.emp_id ?? 0;
            string? start_year = model.start_year;
            string? start_month = model.start_month;
            decimal? amount = model.amount ?? 0;
            decimal? int_amount = model.int_amount;
            int? no_of_installment = model.no_of_installment;
            string? status = model.status;
            string? remarks = model.remarks;

            DateTime givenDate = new DateTime(int.Parse(start_year), int.Parse(start_month), 1);

            string? fiscalYear = _settingsServices.GetFiscalYearByDate(givenDate);

            // ADD NEW
            if (mode == "add")
            {
                //check if the data is exits on another record
                var isData = _context.tbl_employee_swf_loan
                        .FirstOrDefault(u => u.emp_id == emp_id && u.start_year == start_year && u.start_month == start_month
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                var newId = Guid.NewGuid().ToString();
                var DataSave = new tbl_employee_swf_loan
                {
                    id = newId,
                    emp_id = emp_id,
                    start_year = start_year,
                    start_month = start_month,
                    amount = amount,
                    int_amount = int_amount,
                    no_of_installment = no_of_installment,
                    status = status,
                    remarks = remarks,
                    fiscal_year = fiscalYear
                };
                _ = _context.tbl_employee_swf_loan.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                string? swfId = Request.Form["id"];

                // Fetch the existing record first
                var DataUpdate = _context.tbl_employee_swf_loan
                    .FirstOrDefault(h => h.id.ToString() == swfId && h.start_year == start_year && h.start_month == start_month);

                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                DataUpdate.id = swfId;
                DataUpdate.emp_id = emp_id;
                DataUpdate.start_year = start_year;
                DataUpdate.start_month = start_month;
                DataUpdate.amount = amount;
                DataUpdate.int_amount = int_amount;
                DataUpdate.no_of_installment = no_of_installment;
                DataUpdate.status = status;
                DataUpdate.remarks = remarks;
                DataUpdate.fiscal_year = fiscalYear;

                _ = _context.tbl_employee_swf_loan.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        public async Task<IActionResult> SWFLoanDelete([FromBody] DeleteRequest request)
        {
            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            // Delete from tbl_employee_swf_loan
            var loansToDelete = _context.tbl_employee_swf_loan
                .Where(r => request.SelectedIds.Contains(r.id))
                .ToList();

            if (!loansToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            // Delete from tbl_employee_swf_loan_direct_settle (linked by swf_loan_id)
            var settleToDelete = _context.tbl_employee_swf_loan_direct_settle
                .Where(r => request.SelectedIds.Contains(r.swf_loan_id ?? string.Empty))
                .ToList();

            // Remove both sets
            _context.tbl_employee_swf_loan_direct_settle.RemoveRange(settleToDelete);
            _context.tbl_employee_swf_loan.RemoveRange(loansToDelete);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                deletedCount = loansToDelete.Count + settleToDelete.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", (loansToDelete.Count + settleToDelete.Count).ToString())
            });
        }
        [HttpPost]
        public async Task<IActionResult> SWFLoanSettlementSave(string? id, decimal? s_amount, DateTime? s_date, string? s_remarks)
        {
            var recordsToDelete = await _context.tbl_employee_swf_loan_direct_settle
                .Where(r => r.swf_loan_id == id)
                    .ToListAsync().ConfigureAwait(false);

            if (recordsToDelete.Any())
            {
                _context.tbl_employee_swf_loan_direct_settle.RemoveRange(recordsToDelete);
                await _context.SaveChangesAsync();
            }
            var newId = Guid.NewGuid().ToString();
            var DataSave = new tbl_employee_swf_loan_direct_settle
            {
                id = newId,
                amount = s_amount,
                s_date = s_date,
                remarks = s_remarks,
                swf_loan_id = id
            };
            _context.tbl_employee_swf_loan_direct_settle.Add(DataSave);
            _context.SaveChanges();

            return Json(new { status = "success", message = Lang.msg_update_success });

        }
        public async Task<IActionResult> SWFLoanSettlementDelete(string? id)
        {
            var recordsToDelete = await _context.tbl_employee_swf_loan_direct_settle
                .Where(r => r.swf_loan_id == id.ToString())
                .ToListAsync().ConfigureAwait(false);
            if (!recordsToDelete.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_swf_loan_direct_settle.RemoveRange(recordsToDelete);
            var deletedCount = await _context.SaveChangesAsync();

            return Json(new { status = "success", message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletedCount.ToString()) });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetPaidTillDate(int empId, string fiscal, decimal amount, decimal intAmount, string loanId)
        {
            var model = new SwfLoanViewModel
            {
                Settlements = _context.tbl_employee_swf_loan_direct_settle
                    .Where(s => s.swf_loan_id == loanId)
                    .Select(s => new SettlementRow { s_date = s.s_date, remarks = s.remarks, amount = s.amount })
                    .ToList(),

                History = _context.vw_swf_payback
                    .Where(q => q.emp_id == empId && q.loan != 0 && q.fiscal >= Convert.ToDateTime(fiscal))
                    .OrderBy(q => q.fiscal)
                    .Select(q => new wwfpp.Models.Payroll.HistoryRow
                    {
                        sal_year = (int)(q.sal_year ?? 0),
                        sal_month = (int)(q.sal_month ?? 0),
                        loan = q.loan
                    })
                    .ToList(),

                Totals = new TotalsRow
                {
                    TotalPaidLoan = (_context.tbl_employee_swf_loan_direct_settle.Where(s => s.swf_loan_id == loanId).Sum(s => s.amount ?? 0) +
                         _context.vw_swf_payback.Where(q => q.emp_id == empId && q.loan != 0 && q.fiscal >= Convert.ToDateTime(fiscal)).Sum(q => q.loan ?? 0)),
                    TotalDueLoan = (amount + intAmount) -
                        (_context.tbl_employee_swf_loan_direct_settle.Where(s => s.swf_loan_id == loanId).Sum(s => s.amount ?? 0) +
                         _context.vw_swf_payback.Where(q => q.emp_id == empId && q.loan != 0 && q.fiscal >= Convert.ToDateTime(fiscal)).Sum(q => q.loan ?? 0))
                },
                TloanInt = new SwfLoanTotalViewModel
                {
                    TLoanInt = amount + intAmount
                }
            };

            return PartialView("Payroll/_SWFLoanPaidTillDate", model);
        }

        #endregion
        /********************************************************************************************************************/
        #region 10906 OVERTIME BULK
        [HttpGet]
        public IActionResult OvertimeBulk()
        {
            string PageId = "10906";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.StatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            return PartialView("Payroll/_OvertimeBulk", "");
        }
        [HttpPost]
        public async Task<IActionResult> OvertimeBulkList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var statusFilter = request.Status;
            int yearFilter = request.Year ?? 0;
            int monthFilter = request.Month ?? 0;

            // First day of the selected month/year
            DateTime currentPeriod = new DateTime(yearFilter, monthFilter, 1);
            DateTime startDate = currentPeriod.AddMonths(-1);
            DateTime endDate = DateTime.Now;

            string fiscalYear = _settingsServices.GetFiscalYearByDate(currentPeriod);
            double appTimes = 1.5;
            int appSetting = 8;
            int dhrs = 22 * 8; //176 ours
            if (!string.IsNullOrWhiteSpace(fiscalYear))
            {
                appSetting = Convert.ToInt32(_settingsServices.GetHourSettings("normal_working_hrs", fiscalYear));
                dhrs = Convert.ToInt32(_settingsServices.GetHourSettings("working_hrs_per_pay_period", fiscalYear));
            }

            //check if salary is already processed
            bool blnShow = false;
            var sal = await _context.tbl_employee_salary
                .Where(d => d.sal_year == yearFilter && d.sal_month == monthFilter)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            if (sal != null) { blnShow = true; }

            // Overtime employees
            var overtimeEmployees = (
                from e in _context.tbl_employee
                join ot in _context.tbl_employee_overtime
                    on e.emp_id equals ot.emp_id
                where e.emp_status == statusFilter
                      && ot.sal_year == yearFilter
                      && ot.sal_month == monthFilter
                select new
                {
                    e.emp_id,
                    FullName = $"{e.firstname} {e.middlename} {e.lastname} ({e.emp_code})",
                    e.emp_code,
                    e.gender,
                    e.join_date,
                    e.end_date,
                    salary = (decimal?)e.salary,
                    Times = (double?)ot.times,
                    PeriodHours = (int?)ot.pay_period_total_working_hrs,
                    Rate = (decimal?)ot.rate,
                    Hrs = (double?)ot.hrs,
                    Difference = (decimal?)ot.ot_diff,
                    Remarks = ot.remarks ?? "",
                    e.emp_status
                }
            ).ToList();

            List<dynamic> rawData;
            if (overtimeEmployees.Any())
            {
                rawData = overtimeEmployees.Cast<dynamic>().ToList();
            }
            else
            {
                // Precompute overtime hours lookup
                var otHoursLookup = _context.tbl_employee_overtime_request
                    .Where(h => h.app_status == "A"
                                && h.is_paid == "N"
                                && h.ot_date >= startDate
                                && h.ot_date <= endDate)
                    .GroupBy(h => h.emp_id ?? 0)
                    .Select(g => new { EmpId = g.Key, TotalHours = g.Sum(x => x.total_hours) })
                    .ToDictionary(x => x.EmpId, x => x.TotalHours ?? 0);

                // Case B: No overtime records → show all eligible employees
                var eligibleEmployees = (
                    from e in _context.tbl_employee
                    where e.emp_status == statusFilter
                          && _context.tbl_employee_overtime_settings
                               .Any(s => s.emp_id == e.emp_id && s.is_get_overtime == "Y")
                    select new
                    {
                        e.emp_id,
                        FullName = $"{e.firstname} {e.middlename} {e.lastname} ({e.emp_code})",
                        e.emp_code,
                        e.gender,
                        e.join_date,
                        e.end_date,
                        salary = (decimal?)e.salary,
                        Times = (double?)appTimes,
                        PeriodHours = dhrs,
                        Rate = (decimal?)0,
                        Hrs = (double?)(otHoursLookup.ContainsKey(e.emp_id) ? otHoursLookup[e.emp_id] : 0),
                        Difference = (decimal?)0,
                        Remarks = "",
                        e.emp_status
                    }
                ).ToList();

                rawData = eligibleEmployees.Cast<dynamic>().ToList();
            }
            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                rawData = rawData
                    .Where(e => e.FullName.Contains(searchValue) || e.gender.Contains(searchValue))
                    .ToList();
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                rawData = rawData.AsQueryable().OrderBy($"{sortColumn} {sortColumnDir}").ToList();
            }

            var data = rawData.ToList();
            int recordsTotal = data.Count;
            if (pageSize == -1) { pageSize = recordsTotal; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var isDiffMonth = await _context.tbl_salary_differential_month
                .Where(d => d.sal_year == yearFilter && d.sal_month == monthFilter)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            var jsonData = new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                totalRecordSub = yearFilter > 0 && monthFilter > 0
                    ? _context.tbl_employee_overtime.Count(h => h.sal_year == yearFilter && h.sal_month == monthFilter)
                    : 0,
                blnShow,
                data = cData.Select(x => new
                {
                    x.emp_id,
                    x.FullName,
                    x.emp_code,
                    gender = x.gender == "M" ? "Male" : "Female",
                    join_date = x.join_date,
                    end_date = x.end_date,
                    BasicSalary = x.salary,
                    x.Times,
                    x.PeriodHours,
                    x.Rate,
                    x.Hrs,
                    Amount = (x.Rate * (decimal)(x.Hrs ?? 0)),
                    x.Difference,
                    Total = ((x.Rate * (decimal)(x.Hrs ?? 0)) + x.Difference),
                    x.Remarks,
                    isDiffMonth = isDiffMonth != null ? "Y" : "N"
                })
            };

            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult OvertimeBulkSave(int Year, int Month, IFormCollection form)
        {
            string PageId = "10903";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            int submitBy = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            var submitDate = DateTime.Now;
            bool hasDataToSave = false;

            // First pass: check if any employee has amt or otDiff > 0
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("empid_"))
                {
                    var empIdStr = key.Replace("empid_", "");
                    if (!int.TryParse(empIdStr, out int empId)) continue;

                    string amtStr = form[$"amount_{empId}"].ToString();
                    decimal amt = string.IsNullOrWhiteSpace(amtStr) ? 0 : Convert.ToDecimal(amtStr);

                    string diffStr = form[$"difference_{empId}"].ToString();
                    decimal otDiff = string.IsNullOrWhiteSpace(diffStr) ? 0 : Convert.ToDecimal(diffStr);

                    if (amt != 0 || otDiff != 0)
                    {
                        hasDataToSave = true;
                        break; // no need to check further
                    }
                }
            }
            if (!hasDataToSave)
            {
                return Json(new { success = false, message = Lang.msg_employee_overtime_bulk_error_save });
            }
            // Step 1: Clear existing overtime records for this year/month
            var existing = _context.tbl_employee_overtime
                .Where(o => o.sal_year == Year && o.sal_month == Month);
            _context.tbl_employee_overtime.RemoveRange(existing);

            var requestsToReset = _context.tbl_employee_overtime_request
                .Where(r => r.app_status == "A"
                         && r.is_paid == "Y"
                         && r.paid_month == Month
                         && r.paid_year == Year
                         && !_context.tbl_employee_overtime.Any(o => o.emp_id == r.emp_id));
            foreach (var req in requestsToReset)
            {
                req.is_paid = "N";
            }
            // Step 2: Insert new overtime records
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("empid_"))
                {
                    var empIdStr = key.Replace("empid_", "");
                    if (!int.TryParse(empIdStr, out int empId)) { continue; }

                    string basicSalaryStr = form[$"basic_salary_{empId}"].ToString();
                    decimal basicSalary = string.IsNullOrWhiteSpace(basicSalaryStr) ? 0 : Convert.ToDecimal(basicSalaryStr);

                    string dhrsStr = form[$"periodHours_{empId}"].ToString();
                    decimal dhrs = string.IsNullOrWhiteSpace(dhrsStr) ? 0 : Convert.ToDecimal(dhrsStr);

                    string timesStr = form[$"times_{empId}"].ToString();
                    decimal times = string.IsNullOrWhiteSpace(timesStr) ? 0 : Convert.ToDecimal(timesStr);

                    string rateStr = form[$"rate_{empId}"].ToString();
                    decimal rate = string.IsNullOrWhiteSpace(rateStr) ? 0 : Convert.ToDecimal(rateStr);

                    string hrsStr = form[$"hrs_{empId}"].ToString();
                    decimal hrs = string.IsNullOrWhiteSpace(hrsStr) ? 0 : Convert.ToDecimal(hrsStr);

                    string amtStr = form[$"amount_{empId}"].ToString();
                    decimal amt = string.IsNullOrWhiteSpace(amtStr) ? 0 : Convert.ToDecimal(amtStr);

                    string diffStr = form[$"difference_{empId}"].ToString();
                    decimal otDiff = string.IsNullOrWhiteSpace(diffStr) ? 0 : Convert.ToDecimal(diffStr);

                    string remarks = form[$"remarks_{empId}"].ToString();

                    if (amt == 0 && otDiff == 0) continue;
                    var maxId = _context.tbl_employee_overtime.Max(e => (int?)e.ot_id) ?? 0;
                    var newId = maxId + 1;

                    var overtime = new tbl_employee_overtime
                    {
                        ot_id = newId,
                        emp_id = empId,
                        sal_year = Year,
                        sal_month = Month,
                        basic_salary = basicSalary,
                        times = (double)times,
                        rate = rate,
                        hrs = (double)hrs,
                        remarks = remarks,
                        submit_date = submitDate,
                        submit_by = submitBy,
                        ot_diff = otDiff,
                        pay_period_total_working_hrs = (int)dhrs
                    };

                    _ = _context.tbl_employee_overtime.Add(overtime);

                    var requests = _context.tbl_employee_overtime_request
                        .Where(r => r.emp_id == empId
                                 && r.app_status == "A"
                                 && r.is_paid == "N"
                                 && r.ot_date >= submitDate.AddMonths(-1)
                                 && r.ot_date <= submitDate);

                    foreach (var req in requests)
                    {
                        req.is_paid = "Y";
                        req.paid_month = Month;
                        req.paid_year = Year;
                        req.paid_day = 15;
                    }
                }
            }
            _ = _context.SaveChanges();
            return Json(new { success = true, message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OvertimeBulkClear(int Year, int Month)
        {
            var existing = _context.tbl_employee_overtime
                .Where(o => o.sal_year == Year && o.sal_month == Month);

            _context.tbl_employee_overtime.RemoveRange(existing);
            _ = _context.SaveChanges();
            _context.ChangeTracker.Clear();

            // Reset overtime request table
            var requests = _context.tbl_employee_overtime_request
                .Where(r => r.app_status == "A"
                         && r.is_paid == "Y"
                         && r.paid_month == Month
                         && r.paid_year == Year
                         && !_context.tbl_employee_overtime.Any(o => o.emp_id == r.emp_id));

            foreach (var req in requests)
            {
                req.is_paid = "N";
            }

            _ = _context.SaveChanges();
            return Json(new { success = true, message = Lang.msg_clear_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10903 EMPLOYEE DASHAIN ALLOWANCE
        private object IsAllDashainZero(string? FiscalYearFilter)
        {
            var fiscalYearFilter = FiscalYearFilter;
            var (SalYear, SalMonth, Show) = _payrollServices.GetDashainAllowanceYearMonth(fiscalYearFilter ?? string.Empty);

            var sal_year = SalYear;
            var sal_month = SalMonth;
            bool exists = (from e in _context.tbl_employee
                           where e.emp_status == "A" &&
                                 _context.tbl_employee_salary_extra_settings
                                     .Any(s => s.emp_id == e.emp_id && s.is_get_dashain == "Y")
                           join sal in _context.tbl_employee_salary
                               on e.emp_id equals sal.emp_id
                           where sal.sal_year == sal_year
                              && sal.sal_month == sal_month
                              && sal.is_dashain == "Y"
                           select sal).Any();

            return new
            {
                canSaveButton = exists ? "Y" : "N"
            };
        }
        public void SetInsertAccrualFundSource(string parm_check_table, string parmId, int parmEmpId, string parmFiscalYear, DateTime parmStartFiscalDate, DateTime parmEndFiscalDate, string parmInsertTable, short? parmPeriod)
        {
            var checkProp = _context.GetType().GetProperty(parm_check_table);
            if (checkProp == null) throw new ArgumentException("Unknown check table: " + parm_check_table);
            var checkTable = (IQueryable)checkProp.GetValue(_context);

            var insertProp = _context.GetType().GetProperty(parmInsertTable);
            if (insertProp == null) throw new ArgumentException("Unknown insert table: " + parmInsertTable);

            // Detect counter type
            var counterClrType = checkTable.ElementType.GetProperty("counter")?.PropertyType;

            bool exists;
            if (counterClrType == typeof(int) || counterClrType == typeof(int?))
            {
                exists = checkTable.Cast<object>().Any(e =>
                    EF.Property<string>(e, "id") == parmId &&
                    EF.Property<int?>(e, "counter") == parmPeriod);
            }
            else if (counterClrType == typeof(short) || counterClrType == typeof(short?))
            {
                exists = checkTable.Cast<object>().Any(e =>
                    EF.Property<string>(e, "id") == parmId &&
                    EF.Property<short?>(e, "counter") == parmPeriod);
            }
            else
            {
                throw new InvalidOperationException("Unsupported counter type: " + counterClrType?.Name);
            }

            if (!exists) return;

            var fundSources = _context.tbl_employee_fund_source
                .Where(fs => fs.emp_id == parmEmpId
                             && fs.start_date >= parmStartFiscalDate
                             && fs.start_date <= parmEndFiscalDate
                             && _context.tbl_fund_source
                                 .Where(f => f.fund_status == "A" && f.expiry_date > DateTime.Now)
                                 .Select(f => f.fund_id)
                                 .Contains(fs.fund_id))
                .Select(fs => new { fs.fund_id, fs.annual_hrs })
                .ToList();

            int fnCnt = 0;
            var insertEntityType = insertProp.PropertyType.GenericTypeArguments[0];

            foreach (var fs in fundSources)
            {
                fnCnt++;
                int? fnFundId = fs.fund_id;
                double fnAnnualHrs = fs.annual_hrs ?? 0;

                if (fnFundId != 0)
                {
                    string fsid = parmId + fnCnt;

                    var newRecord = Activator.CreateInstance(insertEntityType);
                    var entry = _context.Entry(newRecord);

                    entry.Property("id").CurrentValue = fsid;
                    entry.Property("emp_id").CurrentValue = parmEmpId;
                    entry.Property("fiscal_year").CurrentValue = parmFiscalYear;
                    entry.Property("fund_id").CurrentValue = fnFundId;
                    entry.Property("hours").CurrentValue = fnAnnualHrs;
                    entry.Property("submit_date").CurrentValue = DateTime.Now;

                    // Handle counter type dynamically
                    var counterProp = entry.Property("counter");
                    var clrType = counterProp.Metadata.ClrType;

                    if (clrType == typeof(short) || clrType == typeof(short?))
                        counterProp.CurrentValue = (short?)parmPeriod;
                    else if (clrType == typeof(int) || clrType == typeof(int?))
                        counterProp.CurrentValue = (int?)parmPeriod;
                    else
                        throw new InvalidOperationException("Unsupported counter type: " + clrType.Name);

                    // ✅ Use non-generic Set(Type) overload
                    _context.Add(newRecord);

                    //SetInsertAccrualFundSource("tbl_employee_leave_accrual_new", nextId, update.emp_id, update.pre_fiscal_year, Convert.ToDateTime(start_fiscal_date), Convert.ToDateTime(end_fiscal_date), "tbl_employee_leave_accrual_new_fund_wise", 4);
                }
            }

            _context.SaveChanges();
        }
        [HttpGet]
        public IActionResult DashainAllowance()
        {
            string PageId = "10903";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            string? FiscalYearActive = HttpContext.Session.GetString("fiscal_year");
            ViewBag.FiscalYearActive = FiscalYearActive;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(FiscalYearActive ?? string.Empty);
            var (SalYear, SalMonth, Show) = _payrollServices.GetDashainAllowanceYearMonth(FiscalYearActive ?? string.Empty);

            ViewBag.SalYear = SalYear;
            ViewBag.SalMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(SalMonth);

            return PartialView("Payroll/_DashainAllowance", "");
        }
        [HttpPost]
        public async Task<IActionResult> DashainAllowanceList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var fiscalYearFilter = request.FiscalYearFilter;
            ViewBag.FiscalYearFilter = fiscalYearFilter;
            var (SalYear, SalMonth, Show) = _payrollServices.GetDashainAllowanceYearMonth(fiscalYearFilter ?? string.Empty);

            bool blnShow = Show;

            IQueryable<object> query;

            if (!blnShow)
            {
                query = from e in _context.tbl_employee
                        where e.emp_status == "A" &&
                              _context.tbl_employee_salary_extra_settings
                                  .Any(s => s.emp_id == e.emp_id && s.is_get_dashain == "Y")
                        orderby e.firstname, e.middlename, e.lastname
                        select new
                        {
                            e.emp_id,
                            FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                            e.emp_code,
                            dashain_amount = e.salary,
                            remarks = string.Empty,
                            fiscal_year = fiscalYearFilter,
                            sal_year = SalYear,
                            sal_month = SalMonth,
                            e.emp_status
                        };
            }
            else
            {
                query = from e in _context.tbl_employee
                        join d in _context.tbl_employee_dashain_allowance_emp_wise
                            on e.emp_id equals d.emp_id
                        where d.fiscal_year == fiscalYearFilter && d.counter == 1
                        orderby e.firstname, e.middlename, e.lastname
                        select new
                        {
                            e.emp_id,
                            FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                            e.emp_code,
                            d.dashain_amount,
                            d.remarks,
                            d.fiscal_year,
                            sal_year = SalYear,
                            sal_month = SalMonth,
                            e.emp_status
                        };
            }
            // Apply search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => EF.Functions.Like((string)e.GetType().GetProperty("FullName").GetValue(e), $"%{searchValue}%")
                                    || EF.Functions.Like((string)e.GetType().GetProperty("emp_code").GetValue(e), $"%{searchValue}%"));
            }
            var data = await query.ToListAsync();
            int recordsTotal = data.Count;
            if (pageSize == -1) { pageSize = recordsTotal; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data = cData,
                salYear = SalYear,
                salMonth = SalMonth,
                blnShow
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DashainAllowanceblnMessage(string? FiscalYearFilter)
        {
            var fiscalYearFilter = FiscalYearFilter;
            var (SalYear, SalMonth, Show) = _payrollServices.GetDashainAllowanceYearMonth(fiscalYearFilter ?? string.Empty);
            return Json(new { success = true, message = Lang.msg_clear_success, blnShow = Show });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DashainAllowanceCheckEligibility(string? FiscalYearFilter)
        {
            var (SalYear, SalMonth, Show) = _payrollServices.GetDashainAllowanceYearMonth(FiscalYearFilter ?? string.Empty);

            bool blnShow = Show;
            var result = IsAllDashainZero(FiscalYearFilter);
            var canSaveButton = (result as dynamic).canSaveButton;
            return Json(new
            {
                blnShow,
                is_all_dashain_a_zero = canSaveButton
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DashainAllowanceSave([FromBody] DashainAllowancListeViewModel model)
        {
            string PageId = "10903";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }
            double? total_hours = 0;
            foreach (var update in model.Fields)
            {
                var fiscalPeriod = _context.tbl_fiscal_year.FirstOrDefault(c => c.fiscal_year == update.fiscal_year);
                var start_fiscal_date = fiscalPeriod?.date_from;
                var end_fiscal_date = fiscalPeriod?.date_to;

                var totalHours = (
                    from f in _context.tbl_employee_fund_source
                    where f.emp_id == update.emp_id
                          && f.start_date >= start_fiscal_date
                          && f.start_date <= end_fiscal_date
                          && _context.tbl_fund_source.Any(fs =>
                                fs.fund_id == f.fund_id &&
                                fs.fund_status == "A" &&
                                fs.expiry_date > DateTime.Now)
                    select f.annual_hrs
                ).Sum();
                total_hours = totalHours > 0 ? totalHours : 0;
                var existing = _context.tbl_employee_dashain_allowance_emp_wise
                    .FirstOrDefault(x => x.emp_id == update.emp_id && x.fiscal_year == update.fiscal_year && x.counter == 1);

                var nextId = UniqueID();
                var newRow = new tbl_employee_dashain_allowance
                {
                    id = nextId,
                    fiscal_year = update.fiscal_year,
                    sal_year = Convert.ToInt32(update.sal_year),
                    sal_month = Convert.ToInt32(update.sal_month),
                    submit_date = System.DateTime.Now,
                    counter = 1
                };
                _context.tbl_employee_dashain_allowance.Add(newRow);
                _context.SaveChanges();

                var newRowEmpWise = new tbl_employee_dashain_allowance_emp_wise
                {
                    id = nextId.ToString(),
                    fiscal_year = update.fiscal_year,
                    emp_id = update.emp_id,
                    dashain_amount = Convert.ToDecimal(update.dashain_amount),
                    total_hours = Convert.ToDouble(update.total_hours),
                    remarks = update.remarks,
                    counter = 1
                };
                _context.tbl_employee_dashain_allowance_emp_wise.Add(newRowEmpWise);
                _context.SaveChanges();

                SetInsertAccrualFundSource("tbl_employee_dashain_allowance", nextId, update.emp_id, update.fiscal_year, Convert.ToDateTime(start_fiscal_date), Convert.ToDateTime(end_fiscal_date), "tbl_employee_dashain_allowance_fund_wise", 1);
            }
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DashainAllowanceClear(string? fiscalYear, int? period)
        {
            _ = await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_dashain_allowance WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period).ConfigureAwait(false);
            _ = await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_dashain_allowance_emp_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period).ConfigureAwait(false);
            _ = await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_dashain_allowance_fund_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period).ConfigureAwait(false);

            return Json(new
            {
                status = "success",
                message = "clearsuccess",
                fiscal_year = fiscalYear,
                period
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DashainAllowanceExport(string fiscalYear, int period)
        {
            // Get Organization name
            var OrgName = _globalOptionServices.OptionServices["op_org_name"];

            // Get sal_year and sal_month from allowance table
            var allowance = _context.tbl_employee_dashain_allowance
                .FirstOrDefault(x => x.fiscal_year == fiscalYear && x.counter == period);

            string salYear = allowance?.sal_year?.ToString() ?? "";
            string salMonth = allowance?.sal_month?.ToString() ?? "";

            // Query employee allowance records
            var records = (from e in _context.tbl_employee
                           join d in _context.tbl_employee_dashain_allowance_emp_wise
                               on e.emp_id equals d.emp_id
                           where d.fiscal_year == fiscalYear && d.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               d.dashain_amount,
                               d.remarks
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DashainAllowance");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + OrgName; // replace with actual org name
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Year: " + salYear;
                ws.Cell(row++, 1).Value = "Month: " + salMonth;
                ws.Cell(row++, 1).Value = "Staff Statement of Dashain";

                row++;
                // Header
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Amount";
                ws.Cell(row, 5).Value = "Remarks";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                decimal total = 0;
                int serial = 1;
                foreach (var r in records)
                {
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = r.dashain_amount;
                    ws.Cell(row, 5).Value = r.remarks;
                    total += r.dashain_amount ?? 0;
                    row++;
                }

                // Total row
                ws.Cell(row, 1).Value = "Total";
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 4).Value = total;

                // Auto-fit columns
                ws.Columns().AdjustToContents();

                // Return file
                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_dashain_allowance_export_{fiscalYear.Split('/')[1]}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DashainAllowanceCCD(string fiscalYear, int period)
        {
            // Get Organization name
            var OrgName = _globalOptionServices.OptionServices["op_org_name"];

            // Get sal_year and sal_month
            var allowance = _context.tbl_employee_dashain_allowance
                .FirstOrDefault(x => x.fiscal_year == fiscalYear && x.counter == period);

            string salYear = allowance?.sal_year?.ToString() ?? "";
            string salMonth = allowance?.sal_month?.ToString() ?? "";

            // Query employee allowance records
            var records = (from e in _context.tbl_employee
                           join d in _context.tbl_employee_dashain_allowance_emp_wise
                               on e.emp_id equals d.emp_id
                           where d.fiscal_year == fiscalYear && d.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               d.dashain_amount,
                               d.total_hours
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DashainAllowanceCCD");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + OrgName; // replace with actual org name
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Year: " + salYear;
                ws.Cell(row++, 1).Value = "Month: " + salMonth;
                ws.Cell(row++, 1).Value = "Staff Statement of Dashain with Fund Source Allocated";

                row++;
                // Header
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Fund Source";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Amount";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                string sbtype_gl = "B";
                int serial = 1;
                foreach (var r in records)
                {
                    var staffType = _context.tbl_employee_salary_extra_settings
                        .FirstOrDefault(x => x.emp_id == r.emp_id);
                    string staff_type = "";
                    if (staffType != null) { staff_type = staffType.staff_type; }
                    string gl_code = _payrollServices.GetGLCode(staff_type, sbtype_gl);

                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = ""; // fund source code will be filled below
                    ws.Cell(row, 5).Value = r.total_hours;
                    ws.Cell(row, 6).Value = r.dashain_amount;
                    row++;

                    // Now query fund-wise allocations
                    var fundWise = _context.tbl_employee_dashain_allowance_fund_wise
                        .Where(f => f.emp_id == r.emp_id && f.fiscal_year == fiscalYear && f.counter == period)
                        .ToList();

                    foreach (var f in fundWise)
                    {
                        if (f.hours == 0) { continue; }

                        string? fundSource = _context.tbl_fund_source
                            .Where(fs => fs.fund_id == f.fund_id)
                            .Select(fs => fs.fund_source)
                            .FirstOrDefault();

                        string fund_source = string.IsNullOrWhiteSpace(fundSource) ? "" : fundSource;
                        fund_source = fund_source.Length > 21 ? fund_source.Substring(0, 21) : fund_source;
                        // Build GL code logic (simplified)
                        string append_0000 = "";
                        int sal_year = Convert.ToInt32(salYear);
                        int sal_month = Convert.ToInt32(salMonth);
                        DateTime selDate = new DateTime(sal_year, sal_month, 1);
                        DateTime chkDate = new DateTime(2016, 3, 15);
                        if (selDate > chkDate)
                        {
                            append_0000 = "00000-";
                        }
                        string glFundSourceCode = $"{gl_code}-{fund_source}-{append_0000}{r.emp_code}";

                        decimal amount = r.total_hours != 0
                            ? Math.Round(((r.dashain_amount ?? 0m) * (decimal)(f.hours ?? 0d)) / (decimal)r.total_hours, 2)
                            : 0m;

                        ws.Cell(row, 4).Value = glFundSourceCode;
                        ws.Cell(row, 5).Value = f.hours;
                        ws.Cell(row, 6).Value = amount;
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());
                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_dashain_allowance_ccd_{fiscalYear.Split('/')[1]}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region 10917 WELFARE INTEREST
        [HttpGet]
        public IActionResult WelfareInterest()
        {
            string PageId = "10917";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/WelfareInterest", "DOWNLOAD-FORMAT|IMPORT|EXPORT", PageId, 1);
            return PartialView("Payroll/_WelfareInterest", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WelfareInterestList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.Status;
            int yearFilter = request.Year ?? 0;
            int monthFilter = request.Month ?? 0;

            var query = from emp in _context.tbl_employee
                        where emp.emp_status == EmployeeStatusFilter
                        join lft in _context.tbl_employee_welfare_interest
                              .Where(x => x.wl_year == yearFilter && x.wl_month == monthFilter)
                              on emp.emp_id equals lft.emp_id into leftJoin
                        from lft in leftJoin.DefaultIfEmpty()   // LEFT OUTER JOIN
                        orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                        select new
                        {
                            emp.emp_id,
                            emp.firstname,
                            emp.middlename,
                            emp.lastname,
                            emp.emp_status,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            wl_amount = lft.wl_amount ?? 0
                        };

            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e =>
                e.firstname.Contains(searchValue) ||
                e.middlename.Contains(searchValue) ||
                e.lastname.Contains(searchValue)
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult WelfareInterestSave([FromBody] WelfareInterestListViewModel model)
        {
            string PageId = "10917";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            _ = ModelState.Remove("id");
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var item in model.Fields)
            {
                if (!item.emp_id.HasValue || !item.wl_year.HasValue || !item.wl_month.HasValue) { continue; }

                var existing = _context.tbl_employee_welfare_interest
                .Where(a => a.emp_id == item.emp_id
                            && a.wl_year == item.wl_year
                            && a.wl_month == item.wl_month)
                .ToList();

                if (existing.Count > 0)
                {
                    _context.tbl_employee_welfare_interest.RemoveRange(existing);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
                if (item.wl_amount > 0)
                {
                    var newRec = new tbl_employee_welfare_interest
                    {
                        id = UniqueID(),
                        emp_id = item.emp_id,
                        wl_year = item.wl_year.Value,
                        wl_month = item.wl_month.Value,
                        wl_amount = item.wl_amount.GetValueOrDefault(),
                        submit_date = DateTime.Now,
                        wl_fiscal_year = "",
                        wl_emp_week = 0
                    };
                    _ = _context.tbl_employee_welfare_interest.Add(newRec);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
            }
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WelfareInterestDownloadFormat()
        {
            var sb = new StringBuilder();
            var header = new List<string> { "Employee Name", "Employee ID", "Amount" };
            _ = sb.AppendLine(string.Join(",", header));

            var employees = _context.tbl_employee
                .Where(emp => emp.emp_id != 0 && emp.emp_status == "A")
                .OrderBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .Select(emp => new
                {
                    emp.emp_id,
                    emp.emp_code,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname}"
                })
                .ToList();
            if (employees.Count > 0)
            {
                foreach (var record in employees)
                {
                    string emp_code = EscapeCSV(record.emp_code ?? "");
                    string employee = EscapeCSV(record.employee ?? "");
                    var NewValue = new List<string> { employee, emp_code, "0" };
                    _ = sb.AppendLine(string.Join(",", NewValue));
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "WelfareInterestDownloadedFormat.csv");
        }
        [HttpGet]
        public IActionResult WelfareInterestImport(string? wl_year, string? wl_month)
        {
            string PageId = "10917";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            short wlYear;
            short wlMonth;
            string wlMonthName = "";
            if (string.IsNullOrWhiteSpace(wl_year) || string.IsNullOrWhiteSpace(wl_month))
            {
                wlYear = 0;
                wlMonth = 0;
            }
            else
            {
                wlYear = Convert.ToInt16(wl_year);
                wlMonth = Convert.ToInt16(wl_month);
                wlMonthName = MonthName(wlMonth);
            }
            ViewBag.wl_year = wlYear;
            ViewBag.wl_month = wlMonth;
            ViewBag.WlMonthName = wlMonthName;
            return PartialView("Payroll/_WelfareInterestImport");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WelfareInterestImportSave(IFormFile file)
        {
            if (file == null || file.Length == 0) { return Json(new { status = "error", message = Lang.NO_FILE_UPLOADED }); }
            if (!FileValidator.ForCsv(file)) { return Json(new { status = "error", message = "There is problem with File." }); }

            string? SelYear = Request.Form["wl_year"];
            string? SelMonth = Request.Form["wl_month"];

            if (string.IsNullOrEmpty(SelYear) || string.IsNullOrEmpty(SelMonth)) { return Json(new { status = "error", message = "Not valid year and/or month" }); }

            short wlYear = Convert.ToInt16(SelYear);
            short wlMonth = Convert.ToInt16(SelMonth);

            if (wlYear < 1 || wlMonth < 1) { return Json(new { status = "error", message = "Not valid year and/or month" }); }
            var errors = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
            headerLine = headerLine.Replace("\r", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\n", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\"", "", StringComparison.OrdinalIgnoreCase);
            var headers = headerLine.Split(',').Select(h => h.Trim('"')).ToList();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) { continue; }

                if (line != null)
                {
                    line = line.Replace("\"", "", StringComparison.OrdinalIgnoreCase);
                    var values = line.Split(',').Select(v => v.Trim('"')).ToList();
                    string empCode = values[1];
                    string employeeCode = _employeeServices.GetValidEmpCode(empCode);
                    var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_code == employeeCode); // INSTEAD OF THIS SECTION paymgr.getIDByEmpCode(s_emp_code)

                    if (emp == null || emp.emp_status != "A")
                    {
                        errors.Add("> " + Lang.INACTIVE_EMPLOYEE.Replace("<[EMP-CODE]>", employeeCode, StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    var existing = _context.tbl_employee_welfare_interest
                    .Where(a => a.emp_id == emp.emp_id
                                && a.wl_year == wlYear
                                && a.wl_month == wlMonth)
                    .ToList();

                    if (existing.Any())
                    {
                        _context.tbl_employee_welfare_interest.RemoveRange(existing);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }
                    double wl_amount = Convert.ToDouble(values[2]);
                    if (wl_amount > 0)
                    {
                        var newRec = new tbl_employee_welfare_interest
                        {
                            id = UniqueID(),
                            emp_id = emp.emp_id,
                            wl_year = wlYear,
                            wl_month = wlMonth,
                            wl_amount = wl_amount,
                            submit_date = DateTime.Now,
                            wl_fiscal_year = "",
                            wl_emp_week = 0
                        };
                        _ = _context.tbl_employee_welfare_interest.Add(newRec);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }
                }
            }
            return Json(new { status = "success", message = Lang.EMPLOYEE_FUND_SOURCE_IMPORT_SUCCESSFUL });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WelfareInterestExport()
        {
            var sb = new StringBuilder();
            string? wlYear = Request.Form["YearFilter"];
            string? wlMonth = Request.Form["MonthFilter"];
            if (string.IsNullOrWhiteSpace(wlYear) || string.IsNullOrWhiteSpace(wlMonth)) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }
            short wl_year = Convert.ToInt16(wlYear);
            short wl_month = Convert.ToInt16(wlMonth);
            string StrMonthName = MonthName(wl_month);
            if (wl_year < 1 || wl_month < 1) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }

            var employees = (from emp in _context.tbl_employee
                             join lft in _context.tbl_employee_welfare_interest
                              .Where(x => x.wl_year == wl_year && x.wl_month == wl_month)
                              on emp.emp_id equals lft.emp_id into leftJoin
                             from lft in leftJoin.DefaultIfEmpty()   // LEFT OUTER JOIN
                             orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                             select new
                             {
                                 emp.emp_id,
                                 emp.emp_status,
                                 emp.emp_code,
                                 employee = $"{emp.firstname} {emp.middlename} {emp.lastname}",
                                 wl_amount = lft.wl_amount ?? 0,
                             }).ToList();

            _ = sb.AppendLine($",Period:,{StrMonthName}|{wl_year.ToString()},");
            _ = sb.AppendLine("SN, ID, Employee Name,Amount");
            int cnt = 0;
            foreach (var row in employees)
            {
                cnt++;
                var line = new List<string>
                {
                    cnt.ToString(),
                    row.emp_code,
                    row.employee,
                    row.wl_amount.ToString()
                };
                _ = sb.AppendLine(string.Join(",", line.Select(x => $"\"{x}\"")));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"WelfareInterestExport_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            var filePath = Path.Combine(GblDocumentPath, "temp", fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            return Json(new { status = "success", message = "Export successful!", url = "/uploads/temp/" + fileName });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10918 WELFARE PAID OUT
        [HttpGet]
        public IActionResult WelfarePaidout()
        {
            string PageId = "10918";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_employee_welfare_paidout
                join emp in _context.tbl_employee
                    on a.emp_id equals emp.emp_id
                orderby a.id descending
                select new WelfarePaidoutViewModel
                {
                    id = a.id,
                    wl_year = a.wl_year,
                    wl_month = a.wl_month,
                    wl_amount = a.wl_amount,
                    remarks = a.remarks,
                    submit_date = a.submit_date,
                    emp_id = a.emp_id,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/WelfarePaidout", "ADD|DEL", PageId, Records.Count);
            return PartialView("Payroll/_WelfarePaidout", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WelfarePaidoutList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from wp in _context.tbl_employee_welfare_paidout
                        join emp in _context.tbl_employee
                            on wp.emp_id equals emp.emp_id
                        orderby wp.id descending
                        select new WelfarePaidoutViewModel
                        {
                            id = wp.id,
                            emp_id = emp.emp_id,
                            wl_year = wp.wl_year,
                            wl_month = wp.wl_month,
                            wl_amount = wp.wl_amount,
                            remarks = wp.remarks,
                            submit_date = wp.submit_date,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status
                        };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.wl_year != null && a.wl_year.Value.ToString().Contains(searchValue)) ||
                    (a.wl_month != null && a.wl_month.Value.ToString().Contains(searchValue)) ||
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }
        public IActionResult WelfarePaidoutAddEdit(string? id, string mode)
        {
            string PageId = "10918";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD", "A");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);

            WelfarePaidoutViewModel model;
            model = new WelfarePaidoutViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Payroll/_WelfarePaidoutAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from e in _context.tbl_employee_welfare_paidout
                              join emp in _context.tbl_employee
                                  on e.emp_id equals emp.emp_id
                              where e.id == id.ToString()
                              select new
                              {
                                  e.id,
                                  e.wl_year,
                                  e.wl_month,
                                  e.wl_amount,
                                  e.remarks,
                                  e.submit_date,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status,
                                  emp.emp_id
                              }).FirstOrDefault();
                    ViewBag.SubmitDate = ec?.submit_date;
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }

                    model = new WelfarePaidoutViewModel
                    {
                        id = ec.id,
                        wl_year = ec.wl_year,
                        wl_month = ec.wl_month,
                        wl_amount = ec.wl_amount ?? 0,
                        remarks = ec.remarks,
                        submit_date = ec.submit_date,
                        emp_id = ec.emp_id,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Payroll/_WelfarePaidoutAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult WelfarePaidoutSave(WelfarePaidoutViewModel model)
        {
            ModelState.Remove("id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10918", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string id = Request.Form["id"].ToString() ?? string.Empty;
            short? wl_year = model.wl_year;
            short? wl_month = model.wl_month;
            double? wl_amount = model.wl_amount ?? 0;
            string? remarks = model.remarks;
            DateTime submit_date = System.DateTime.Now;
            int? emp_id = model.emp_id;

            if (wl_year < 1 || wl_month < 1 || wl_amount <= 0){ return Json(new { status = "error", message = Lang.msg_insufficient_info }); }

            if (mode == "add")
            {
                var newId = UniqueID();
                var DataSave = new tbl_employee_welfare_paidout
                {
                    id = newId,
                    emp_id = emp_id,
                    wl_year = wl_year,
                    wl_month = wl_month,
                    wl_amount = wl_amount,
                    submit_date = submit_date,
                    wl_fiscal_year = null,
                    wl_emp_week = 0,
                    remarks = remarks
                };
                _context.tbl_employee_welfare_paidout.Add(DataSave);
                _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = id });
            }
            else if (mode == "edit")
            {
                string? paidoutId = id;

                var DataUpdate = _context.tbl_employee_welfare_paidout
                    .FirstOrDefault(h => h.id.ToString() == paidoutId);

                DataUpdate.id = paidoutId;
                DataUpdate.emp_id = emp_id;
                DataUpdate.wl_year = wl_year;
                DataUpdate.wl_month = wl_month;
                DataUpdate.wl_amount = wl_amount;
                DataUpdate.remarks = remarks;

                _context.tbl_employee_welfare_paidout.Update(DataUpdate);
                _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        public async Task<IActionResult> WelfarePaidoutDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10918", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_welfare_paidout
                .Where(r => request.SelectedIds.Contains(r.id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_welfare_paidout.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", request.SelectedIds.Count.ToString())
            });
        }

        #endregion
        /********************************************************************************************************************/
        #region 10919 GRATUITY INFORMATION
        [HttpGet]
        public IActionResult gratuityInformation()
        {
            string PageId = "10919";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.StatusFilter = StatusActivePassive("AD", "A");
            ViewBag.GRGroupList = _payrollServices.GetGRGroupList();
            ViewBag.GRTypeList = _payrollServices.GetGRTypeList();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Payroll/_GratuityInformation", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GratuityInformationList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string StatusFilter = request.FilterValue;
            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_gratuity_info
                        on emp.emp_id equals lft.emp_id into LeftJoin
                        from lft in LeftJoin.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == StatusFilter
                        select new
                        {
                            emp_id = emp.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            opening_balance = lft.opening_balance ?? 0.00,
                            opening_interest = lft.opening_interest ?? 0.00,
                            salary = emp.salary,
                            gr_number = lft.gr_number ?? string.Empty,
                            gr_group = lft.gr_group ?? "",
                            gr_type = lft.gr_type ?? "",
                            add_percent_amount = lft.add_percent_amount ?? 0.00,
                            ded_percent_amount = lft.ded_percent_amount ?? 0.00,
                            emp_status = emp.emp_status
                        };
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1) pageSize = totalRecord;
            var cData = data.Skip(skip).Take(pageSize).ToList();
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GratuityInformationSave([FromBody] GratuityListViewModel model)
        {
            string PageId = "10919";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var emp in model.Fields)
            {
                var DataUpdate = _context.tbl_employee_gratuity_info.FirstOrDefault(h => h.emp_id == emp.emp_id);
                //UPDATE EXISTING RECORDS IF EXIST
                if (DataUpdate != null)
                {
                    DataUpdate.gr_number = emp.gr_number;
                    DataUpdate.gr_group = emp.gr_group ?? string.Empty;
                    DataUpdate.gr_type = emp.gr_type ?? string.Empty;
                    DataUpdate.add_percent_amount = emp.add_percent_amount ?? 0.00;
                    DataUpdate.ded_percent_amount = emp.ded_percent_amount ?? 0.00;
                    _ = _context.tbl_employee_gratuity_info.Update(DataUpdate);
                }
                else
                {
                    // INSERT IF DATA DOESNOT EXIST
                    int maxId = _context.tbl_employee_gratuity_info.Max(e => (int?)e.id) ?? 0;
                    maxId++;
                    var newRow = new tbl_employee_gratuity_info
                    {
                        id = maxId,
                        emp_id = emp.emp_id,
                        gr_number = emp.gr_number,
                        gr_group = emp.gr_group,
                        gr_type = emp.gr_type,
                        add_percent_amount = emp.add_percent_amount,
                        ded_percent_amount = emp.ded_percent_amount,
                    };
                    _ = _context.tbl_employee_gratuity_info.Add(newRow);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10909 FIELD STAFF SALARY
        [HttpGet]
        public IActionResult FieldStaffSalary()
        {
            string PageId = "10909";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/FieldStaffSalary", "DOWNLOAD-FORMAT|IMPORT|EXPORT", PageId, 1);
            return PartialView("Payroll/_FieldStaffSalary", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FieldStaffSalaryList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.Status;
            int yearFilter = request.Year ?? 0;
            int monthFilter = request.Month ?? 0;

            var query = from emp in _context.tbl_employee
                        where emp.emp_status == EmployeeStatusFilter
                                && _context.tbl_employee_salary_extra_settings
                                     .Where(ses => ses.is_field_salary == "Y")
                                     .Select(ses => ses.emp_id)
                                     .Contains(emp.emp_id)
                        join lft in _context.tbl_employee_salary_a_field
                              .Where(x => x.sal_year == yearFilter && x.sal_month == monthFilter)
                              on emp.emp_id equals lft.emp_id into leftJoin
                        from lft in leftJoin.DefaultIfEmpty()   // LEFT OUTER JOIN
                        orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                        select new
                        {
                            emp.emp_id,
                            emp.firstname,
                            emp.middlename,
                            emp.lastname,
                            emp.emp_status,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            act_basic_salary = lft.act_basic_salary ?? 0,
                            act_pf_a = lft.act_pf_a ?? 0,
                            act_pf_d = lft.act_pf_d ?? 0,
                            a_cit_d = lft.a_cit_d ?? 0,
                            act_remote_area_all = lft.act_remote_area_all ?? 0,
                            basic_salary = lft.basic_salary ?? 0,
                            pf_a = lft.pf_a ?? 0,
                            children_edu_all = lft.children_edu_all ?? 0,
                            performance_all = lft.performance_all ?? 0,
                            remote_area_all = lft.remote_area_all ?? 0,
                            overtime = lft.overtime ?? 0,
                            dashain_a = lft.dashain_a ?? 0,
                            gratuity = lft.gratuity ?? 0,
                            ssf = lft.ssf ?? 0,
                            annual_health_checkup_add = lft.annual_health_checkup_add ?? 0,
                            insurance = lft.insurance ?? 0,
                            others = lft.others ?? 0,
                            medical_expense_reimburse_total = lft.medical_expense_reimburse_total ?? 0,
                            leave_encash = lft.leave_encash ?? 0,
                            pf_d = lft.pf_d ?? 0,
                            cit_d = lft.cit_d ?? 0,
                            gratuity_ded = lft.gratuity_ded ?? 0,
                            ssf_ded = lft.ssf_ded ?? 0,
                            annual_health_checkup_ded = lft.annual_health_checkup_ded ?? 0,
                            pre_access_tax = lft.pre_access_tax ?? 0,
                            incometax_d = lft.incometax_d ?? 0,
                            tel_per_adv = lft.tel_per_adv ?? 0,
                            travel_prog_adv = lft.travel_prog_adv ?? 0,
                            pr_adv = lft.pr_adv ?? 0,
                            fd_adv = lft.fd_adv ?? 0,
                            welfare_fund = lft.welfare_fund ?? 0,
                            adv_pf_loan = lft.adv_PF_loan ?? 0,
                            adv_cit_loan = lft.adv_CIT_loan ?? 0,
                            wl_adv = lft.wl_adv ?? 0,
                            net_in_hand = lft.net_in_hand ?? 0
                        };

            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => 
                e.firstname.Contains(searchValue) ||
                e.middlename.Contains(searchValue) ||
                e.lastname.Contains(searchValue)
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult FieldStaffSalarySave([FromBody] FieldStaffSalaryListViewModel model)
        {
            string PageId = "10909";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            _ = ModelState.Remove("salary_id");
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var item in model.Fields)
            {
                if (!item.emp_id.HasValue || !item.sal_year.HasValue || !item.sal_month.HasValue) { continue; }

                var existing = _context.tbl_employee_salary_a_field
                .Where(a => a.emp_id == item.emp_id
                            && a.sal_year == item.sal_year
                            && a.sal_month == item.sal_month)
                .ToList();

                if (existing.Count > 0)
                {
                    _context.tbl_employee_salary_a_field.RemoveRange(existing);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
                bool allZero =
                    item.act_basic_salary.GetValueOrDefault() == 0m &&
                    item.act_pf_a.GetValueOrDefault() == 0m &&
                    item.act_pf_d.GetValueOrDefault() == 0m &&
                    item.a_cit_d.GetValueOrDefault() == 0m &&
                    item.act_remote_area_all.GetValueOrDefault() == 0m &&
                    item.basic_salary.GetValueOrDefault() == 0m &&
                    item.pf_a.GetValueOrDefault() == 0m &&
                    item.children_edu_all.GetValueOrDefault() == 0m &&
                    item.performance_all.GetValueOrDefault() == 0m &&
                    item.remote_area_all.GetValueOrDefault() == 0m &&
                    item.overtime.GetValueOrDefault() == 0m &&
                    item.dashain_a.GetValueOrDefault() == 0m &&
                    item.gratuity.GetValueOrDefault() == 0m &&
                    item.ssf.GetValueOrDefault() == 0m &&
                    item.annual_health_checkup_add.GetValueOrDefault() == 0m &&
                    item.insurance.GetValueOrDefault() == 0m &&
                    item.others.GetValueOrDefault() == 0m &&
                    item.medical_expense_reimburse_total.GetValueOrDefault() == 0m &&
                    item.leave_encash.GetValueOrDefault() == 0m &&
                    item.pf_d.GetValueOrDefault() == 0m &&
                    item.cit_d.GetValueOrDefault() == 0m &&
                    item.gratuity_ded.GetValueOrDefault() == 0m &&
                    item.ssf_ded.GetValueOrDefault() == 0m &&
                    item.annual_health_checkup_ded.GetValueOrDefault() == 0m &&
                    item.pre_access_tax.GetValueOrDefault() == 0m &&
                    item.incometax_d.GetValueOrDefault() == 0m &&
                    item.tel_per_adv.GetValueOrDefault() == 0m &&
                    item.travel_prog_adv.GetValueOrDefault() == 0m &&
                    item.pr_adv.GetValueOrDefault() == 0m &&
                    item.fd_adv.GetValueOrDefault() == 0m &&
                    item.welfare_fund.GetValueOrDefault() == 0m &&
                    item.adv_pf_loan.GetValueOrDefault() == 0m &&
                    item.adv_cit_loan.GetValueOrDefault() == 0m &&
                    item.wl_adv.GetValueOrDefault() == 0m &&
                    item.net_in_hand.GetValueOrDefault() == 0m;

                if (!allZero)
                {
                    var newRec = new tbl_employee_salary_a_field
                    {
                        salary_id = UniqueID(),
                        emp_id = item.emp_id,
                        sal_year = item.sal_year.Value,
                        sal_month = item.sal_month.Value,
                        act_basic_salary = item.act_basic_salary.GetValueOrDefault(),
                        act_pf_a = item.act_pf_a.GetValueOrDefault(),
                        act_pf_d = item.act_pf_d.GetValueOrDefault(),
                        a_cit_d = item.a_cit_d.GetValueOrDefault(),
                        act_remote_area_all = item.act_remote_area_all.GetValueOrDefault(),
                        basic_salary = item.basic_salary.GetValueOrDefault(),
                        pf_a = item.pf_a.GetValueOrDefault(),
                        children_edu_all = item.children_edu_all.GetValueOrDefault(),
                        performance_all = item.performance_all.GetValueOrDefault(),
                        remote_area_all = item.remote_area_all.GetValueOrDefault(),
                        overtime = item.overtime.GetValueOrDefault(),
                        dashain_a = item.dashain_a.GetValueOrDefault(),
                        gratuity = item.gratuity.GetValueOrDefault(),
                        ssf = item.ssf.GetValueOrDefault(),
                        annual_health_checkup_add = item.annual_health_checkup_add.GetValueOrDefault(),
                        insurance = item.insurance.GetValueOrDefault(),
                        others = item.others.GetValueOrDefault(),
                        medical_expense_reimburse_total = item.medical_expense_reimburse_total.GetValueOrDefault(),
                        leave_encash = item.leave_encash.GetValueOrDefault(),
                        pf_d = item.pf_d.GetValueOrDefault(),
                        cit_d = item.cit_d.GetValueOrDefault(),
                        gratuity_ded = item.gratuity_ded.GetValueOrDefault(),
                        ssf_ded = item.ssf_ded.GetValueOrDefault(),
                        annual_health_checkup_ded = item.annual_health_checkup_ded.GetValueOrDefault(),
                        pre_access_tax = item.pre_access_tax.GetValueOrDefault(),
                        incometax_d = item.incometax_d.GetValueOrDefault(),
                        tel_per_adv = item.tel_per_adv.GetValueOrDefault(),
                        travel_prog_adv = item.travel_prog_adv.GetValueOrDefault(),
                        pr_adv = item.pr_adv.GetValueOrDefault(),
                        fd_adv = item.fd_adv.GetValueOrDefault(),
                        welfare_fund = item.welfare_fund.GetValueOrDefault(),
                        adv_PF_loan = item.adv_pf_loan.GetValueOrDefault(),
                        adv_CIT_loan = item.adv_cit_loan.GetValueOrDefault(),
                        wl_adv = item.wl_adv.GetValueOrDefault(),
                        net_in_hand = item.net_in_hand.GetValueOrDefault(),
                        submit_date = DateTime.Now,
                        submit_by = Convert.ToInt32(HttpContext.Session.GetString("emp_id")),
                        grade = 0,
                        gratudi = 0,
                        betalibi_d = 0,
                        remarks = "",
                    };
                    _ = _context.tbl_employee_salary_a_field.Add(newRec);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
            }

            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FieldStaffSalaryDownloadFormat()
        {
            var sb = new StringBuilder();
            var header = new List<string> { "Employee Name", "Employee ID", "Basic Salary [Actual]", "PF Addition [Actual]", "PF Deduction[Actual]", "CIT Deduction[Actual]", "RAA [Actual]", "Basic Salary", "PF Addition", "Children Edu. Allowance", "Performance Bonus", "Remote Area Allowance", "Overtime", "Dashain Bonus", "Gratuity", "SSF", "Insurance", "Other Allowance", "Medical/Insurance", "Leave Encashment", "PF Deduction", "CIT deduction", "Gratuity", "SSF", "Prev. Year Excess/(Less) Tax", "Income Tax", "Personal Advance", "Travel Advance", "Program Advance", "Field Advance", "Welfare Contribution", "PF Loan", "CIT Loan", "Welfare Loan", "Annual Health Checkup (+)", "Annual Health Checkup (-)", "Net" };
            _ = sb.AppendLine(string.Join(",", header));

            var employees = _context.tbl_employee
                .Where(emp => emp.emp_id != 0 &&
                              emp.emp_status == "A" &&
                              _context.tbl_employee_salary_extra_settings
                                  .Where(ses => ses.is_field_salary == "Y")
                                  .Select(ses => ses.emp_id)
                                  .Contains(emp.emp_id))
                .OrderBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .Select(emp => new
                {
                    emp.emp_id,
                    emp.emp_code,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname}"
                })
                .ToList();
            int cnt = 0;
            if (employees.Count > 0)
            {
                foreach (var record in employees)
                {
                    cnt++;
                    string emp_code = EscapeCSV(record.emp_code ?? "");
                    string employee = EscapeCSV(record.employee ?? "");
                    var NewValue = new List<string> { employee, emp_code, "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
                    _ = sb.AppendLine(string.Join(",", NewValue));
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "FieldStaffSalaryDownloadedFormat.csv");
        }
        [HttpGet]
        public IActionResult FieldStaffSalaryImport(string? sal_year, string? sal_month)
        {
            string PageId = "10909";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            short SalYear;
            short SalMonth;
            string SalMonthName = "";
            if (string.IsNullOrWhiteSpace(sal_year) || string.IsNullOrWhiteSpace(sal_month))
            {
                SalYear = 0;
                SalMonth = 0;
            }
            else
            {
                SalYear = Convert.ToInt16(sal_year);
                SalMonth = Convert.ToInt16(sal_month);
                SalMonthName = MonthName(SalMonth);
            }
            ViewBag.sal_year = SalYear;
            ViewBag.sal_month = SalMonth;
            ViewBag.SalMonthName = SalMonthName;
            return PartialView("Payroll/_FieldStaffSalaryImport");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FieldStaffSalaryImportSave(IFormFile file)
        {
            if (file == null || file.Length == 0) { return Json(new { status = "error", message = Lang.NO_FILE_UPLOADED }); }
            if (!FileValidator.ForCsv(file)) { return Json(new { status = "error", message = "There is problem with File." }); }

            string? SelYear = Request.Form["sal_year"];
            string? SelMonth = Request.Form["sal_month"];

            if (string.IsNullOrEmpty(SelYear) || string.IsNullOrEmpty(SelMonth)) { return Json(new { status = "error", message = "Not valid year and/or month" }); }

            short SalYear = Convert.ToInt16(SelYear);
            short SalMonth = Convert.ToInt16(SelMonth);

            if (SalYear < 1 || SalMonth < 1) { return Json(new { status = "error", message = "Not valid year and/or month" }); }
            var errors = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
            headerLine = headerLine.Replace("\r", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\n", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\"", "", StringComparison.OrdinalIgnoreCase);
            var headers = headerLine.Split(',').Select(h => h.Trim('"')).ToList();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) { continue; }

                if (line != null)
                {
                    line = line.Replace("\"", "", StringComparison.OrdinalIgnoreCase);
                    var values = line.Split(',').Select(v => v.Trim('"')).ToList();
                    string empCode = values[1];
                    string employeeCode = _employeeServices.GetValidEmpCode(empCode);
                    var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_code == employeeCode); // INSTEAD OF THIS SECTION paymgr.getIDByEmpCode(s_emp_code)

                    if (emp == null || emp.emp_status != "A")
                    {
                        errors.Add("> " + Lang.INACTIVE_EMPLOYEE.Replace("<[EMP-CODE]>", employeeCode, StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    var existing = _context.tbl_employee_salary_a_field
                    .Where(a => a.emp_id == emp.emp_id
                                && a.sal_year == SalYear
                                && a.sal_month == SalMonth)
                    .ToList();

                    if (existing.Any())
                    {
                        _context.tbl_employee_salary_a_field.RemoveRange(existing);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }
                    decimal act_basic_salary = Convert.ToDecimal(values[2]);//"Basic Salary [Actual]" 
                    decimal act_pf_a = Convert.ToDecimal(values[3]);//"PF Addition [Actual]" 
                    decimal act_pf_d = Convert.ToDecimal(values[4]);//"PF Deduction[Actual]" 
                    decimal a_cit_d = Convert.ToDecimal(values[5]);//"CIT Deduction[Actual]" 
                    decimal act_remote_area_all = Convert.ToDecimal(values[6]);//"RAA [Actual]" 
                    decimal basic_salary = Convert.ToDecimal(values[7]);//"Basic Salary" 
                    decimal pf_a = Convert.ToDecimal(values[8]);//"PF Addition"
                    decimal children_edu_all = Convert.ToDecimal(values[9]);//"Children Edu. Allowance" 
                    decimal performance_all = Convert.ToDecimal(values[10]);//"Performance Bonus" 
                    decimal remote_area_all = Convert.ToDecimal(values[11]);//"Remote Area Allowance" 
                    decimal overtime = Convert.ToDecimal(values[12]);//"Overtime" 
                    decimal dashain_a = Convert.ToDecimal(values[13]);//"Dashain Bonus" 
                    decimal gratuity = Convert.ToDecimal(values[14]);//"Gratuity" 
                    decimal ssf = Convert.ToDecimal(values[15]);//"SSF"
                    decimal insurance = Convert.ToDecimal(values[16]);//"Insurance"
                    decimal others = Convert.ToDecimal(values[17]);//"Other Allowance"
                    decimal medical_expense_reimburse_total = Convert.ToDecimal(values[18]);//"Medical/Insurance"
                    decimal leave_encash = Convert.ToDecimal(values[19]);//"Leave Encashment" 
                    decimal pf_d = Convert.ToDecimal(values[20]);//"PF Deduction"
                    decimal cit_d = Convert.ToDecimal(values[21]);//"CIT deduction"
                    decimal gratuity_ded = Convert.ToDecimal(values[22]);//"Gratuity"
                    decimal ssf_ded = Convert.ToDecimal(values[23]);//"SSF"
                    decimal pre_access_tax = Convert.ToDecimal(values[24]);//"Prev. Year Excess/(Less) Tax"
                    decimal incometax_d = Convert.ToDecimal(values[25]);//"Income Tax" 
                    decimal tel_per_adv = Convert.ToDecimal(values[26]);//"Personal Advance"
                    decimal travel_prog_adv = Convert.ToDecimal(values[27]);//"Travel Advance"
                    decimal pr_adv = Convert.ToDecimal(values[28]);//"Program Advance"
                    decimal fd_adv = Convert.ToDecimal(values[29]);//"Field Advance"
                    decimal welfare_fund = Convert.ToDecimal(values[30]);//"Welfare Contribution"
                    decimal adv_pf_loan = Convert.ToDecimal(values[31]);//"PF Loan" 
                    decimal adv_cit_loan = Convert.ToDecimal(values[32]);//"CIT Loan" 
                    decimal wl_adv = Convert.ToDecimal(values[33]);//"Welfare Loan"
                    decimal annual_health_checkup_add = Convert.ToDecimal(values[34]);//"Annual Health Checkup (+)"
                    decimal annual_health_checkup_ded = Convert.ToDecimal(values[35]);//"Annual Health Checkup (-)" 
                    decimal net_in_hand = Convert.ToDecimal(values[36]);//"Net"


                    bool allZero = new[] { act_basic_salary, act_pf_a, act_pf_d, a_cit_d, act_remote_area_all,
                        basic_salary, pf_a, children_edu_all, performance_all, remote_area_all, overtime,
                        dashain_a, gratuity, ssf, insurance, others, medical_expense_reimburse_total, leave_encash,
                        pf_d, cit_d, gratuity_ded, ssf_ded, pre_access_tax, incometax_d, tel_per_adv, travel_prog_adv,
                        pr_adv, fd_adv, welfare_fund, adv_pf_loan, adv_cit_loan,  wl_adv, annual_health_checkup_add,
                        annual_health_checkup_ded, net_in_hand }.All(x => x == 0m);

                    if (!allZero)
                    {
                        var newRec = new tbl_employee_salary_a_field
                        {
                            salary_id = UniqueID(),
                            emp_id = emp.emp_id,
                            sal_year = SalYear,
                            sal_month = SalMonth,
                            act_basic_salary = act_basic_salary,
                            act_pf_a = act_pf_a,
                            act_pf_d = act_pf_d,
                            a_cit_d = a_cit_d,
                            act_remote_area_all = act_remote_area_all,
                            basic_salary = basic_salary,
                            pf_a = pf_a,
                            children_edu_all = children_edu_all,
                            performance_all = performance_all,
                            remote_area_all = remote_area_all,
                            overtime = overtime,
                            dashain_a = dashain_a,
                            gratuity = gratuity,
                            ssf = ssf,
                            annual_health_checkup_add = annual_health_checkup_add,
                            insurance = insurance,
                            others = others,
                            medical_expense_reimburse_total = medical_expense_reimburse_total,
                            leave_encash = leave_encash,
                            pf_d = pf_d,
                            cit_d = cit_d,
                            gratuity_ded = gratuity_ded,
                            ssf_ded = ssf_ded,
                            annual_health_checkup_ded = annual_health_checkup_ded,
                            pre_access_tax = pre_access_tax,
                            incometax_d = incometax_d,
                            tel_per_adv = tel_per_adv,
                            travel_prog_adv = travel_prog_adv,
                            pr_adv = pr_adv,
                            fd_adv = fd_adv,
                            welfare_fund = welfare_fund,
                            adv_PF_loan = adv_pf_loan,
                            adv_CIT_loan = adv_cit_loan,
                            wl_adv = wl_adv,
                            net_in_hand = net_in_hand,
                            submit_date = DateTime.Now,
                            submit_by = Convert.ToInt32(HttpContext.Session.GetString("emp_id")),
                            grade = 0,
                            gratudi = 0,
                            betalibi_d = 0,
                            remarks = "",
                        };
                        _ = _context.tbl_employee_salary_a_field.Add(newRec);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }
                }
            }
            return Json(new { status = "success", message = Lang.EMPLOYEE_FUND_SOURCE_IMPORT_SUCCESSFUL });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FieldStaffSalaryExport()
        {
            var sb = new StringBuilder();
            string? SalYear = Request.Form["YearFilter"];
            string? SalMonth = Request.Form["MonthFilter"];
            if (string.IsNullOrWhiteSpace(SalYear) || string.IsNullOrWhiteSpace(SalMonth)) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }
            short sal_year = Convert.ToInt16(SalYear);
            short sal_month = Convert.ToInt16(SalMonth);
            string StrMonthName = MonthName(sal_month);
            if (sal_year < 1 || sal_month < 1) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }

            var employees = (from emp in _context.tbl_employee
                             where emp.emp_id != 0
                                 && _context.tbl_employee_salary_extra_settings
                                     .Where(ses => ses.is_field_salary == "Y")
                                     .Select(ses => ses.emp_id)
                                     .Contains(emp.emp_id)
                             join lft in _context.tbl_employee_salary_a_field
                              .Where(x => x.sal_year == sal_year && x.sal_month == sal_month)
                              on emp.emp_id equals lft.emp_id into leftJoin
                             from lft in leftJoin.DefaultIfEmpty()   // LEFT OUTER JOIN
                             orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                             select new
                             {
                                 emp.emp_id,
                                 emp.emp_status,
                                 emp.emp_code,
                                 employee = $"{emp.firstname} {emp.middlename} {emp.lastname}",
                                 act_basic_salary = lft.act_basic_salary ?? 0,
                                 act_pf_a = lft.act_pf_a ?? 0,
                                 act_pf_d = lft.act_pf_d ?? 0,
                                 a_cit_d = lft.a_cit_d ?? 0,
                                 act_remote_area_all = lft.act_remote_area_all ?? 0,
                                 basic_salary = lft.basic_salary ?? 0,
                                 pf_a = lft.pf_a ?? 0,
                                 children_edu_all = lft.children_edu_all ?? 0,
                                 performance_all = lft.performance_all ?? 0,
                                 remote_area_all = lft.remote_area_all ?? 0,
                                 overtime = lft.overtime ?? 0,
                                 dashain_a = lft.dashain_a ?? 0,
                                 gratuity = lft.gratuity ?? 0,
                                 ssf = lft.ssf ?? 0,
                                 annual_health_checkup_add = lft.annual_health_checkup_add ?? 0,
                                 insurance = lft.insurance ?? 0,
                                 others = lft.others ?? 0,
                                 medical_expense_reimburse_total = lft.medical_expense_reimburse_total ?? 0,
                                 leave_encash = lft.leave_encash ?? 0,
                                 pf_d = lft.pf_d ?? 0,
                                 cit_d = lft.cit_d ?? 0,
                                 gratuity_ded = lft.gratuity_ded ?? 0,
                                 ssf_ded = lft.ssf_ded ?? 0,
                                 annual_health_checkup_ded = lft.annual_health_checkup_ded ?? 0,
                                 pre_access_tax = lft.pre_access_tax ?? 0,
                                 incometax_d = lft.incometax_d ?? 0,
                                 tel_per_adv = lft.tel_per_adv ?? 0,
                                 travel_prog_adv = lft.travel_prog_adv ?? 0,
                                 pr_adv = lft.pr_adv ?? 0,
                                 fd_adv = lft.fd_adv ?? 0,
                                 welfare_fund = lft.welfare_fund ?? 0,
                                 adv_pf_loan = lft.adv_PF_loan ?? 0,
                                 adv_cit_loan = lft.adv_CIT_loan ?? 0,
                                 wl_adv = lft.wl_adv ?? 0,
                                 net_in_hand = lft.net_in_hand ?? 0
                             }).ToList();

            _ = sb.AppendLine($",Period:,{StrMonthName}|{sal_year.ToString()},,,,,,,");
            _ = sb.AppendLine("SN, ID, Employee Name,Basic Salary [Actual], PF Addition [Actual], PF Deduction[Actual], CIT Deduction[Actual], RAA [Actual], Basic Salary, PF Addition, Children Edu. Allowance, Performance Bonus, Remote Area Allowance, Overtime, Dashain Bonus, Gratuity, SSF, Annual Health Checkup (+), Insurance, Other Allowance, Medical/Insurance, Leave Encashment, PF Deduction, CIT deduction, Gratuity, SSF, Annual Health Checkup (-), Prev. Year Excess/(Less) Tax, Income Tax, Personal Advance, Travel Advance, Program Advance, Field Advance, Welfare Contribution, PF Loan, CIT Loan, Welfare Loan, Net");
            int cnt = 0;
            foreach (var row in employees)
            {
                cnt++;
                var line = new List<string>
                {
                    cnt.ToString(),
                    row.emp_code,
                    row.employee,
                    row.act_basic_salary.ToString(),    //  Basic Salary [Actual] 
                    row.act_pf_a.ToString(),            //  PF Addition [Actual]
                    row.act_pf_d.ToString(),            //  PF Deduction[Actual]
                    row.a_cit_d.ToString(),             //  CIT Deduction[Actual]
                    row.act_remote_area_all.ToString(), //  RAA [Actual]
                    row.basic_salary.ToString(),        //  Basic Salary
                    row.pf_a.ToString(),                //  PF Addition 
                    row.children_edu_all.ToString(),    //  Children Edu. Allowance 
                    row.performance_all.ToString(),     //  Performance Bonus 
                    row.remote_area_all.ToString(),     //  Remote Area Allowance 
                    row.overtime.ToString(),            //  Overtime 
                    row.dashain_a.ToString(),           //  Dashain Bonus 
                    row.gratuity.ToString(),            //  Gratuity 
                    row.ssf.ToString(),                 //  SSF
                    row.annual_health_checkup_add.ToString(),   //  Annual Health Checkup (+) 
                    row.insurance.ToString(),           //  Insurance 
                    row.others.ToString(),              //  Other Allowance 
                    row.medical_expense_reimburse_total.ToString(),//  Medical/Insurance 
                    row.leave_encash.ToString(),        //  Leave Encashment 
                    row.pf_d.ToString(),                //  PF Deduction 
                    row.cit_d.ToString(),               //  CIT deduction 
                    row.gratuity_ded.ToString(),        //  Gratuity 
                    row.ssf_ded.ToString(),             //  SSF
                    row.annual_health_checkup_ded.ToString(),//  Annual Health Checkup (-) 
                    row.pre_access_tax.ToString(),      //  Prev. Year Excess/(Less) Tax 
                    row.incometax_d.ToString(),         //  Income Tax 
                    row.tel_per_adv.ToString(),         //  Personal Advance 
                    row.travel_prog_adv.ToString(),     //  Travel Advance 
                    row.pr_adv.ToString(),              //  Program Advance
                    row.fd_adv.ToString(),              //  Field Advance 
                    row.welfare_fund.ToString(),        //  Welfare Contribution 
                    row.adv_pf_loan.ToString(),         //  PF Loan 
                    row.adv_cit_loan.ToString(),        //  CIT Loan 
                    row.wl_adv.ToString(),              //  Welfare Loan     
                    row.net_in_hand.ToString()          //  Net
                };
                _ = sb.AppendLine(string.Join(",", line.Select(x => $"\"{x}\"")));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"FieldStaffSalaryExport_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            var filePath = Path.Combine(GblDocumentPath, "temp", fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            return Json(new { status = "success", message = "Export successful!", url = "/uploads/temp/" + fileName });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10908 Employee Salary Previous
        [HttpGet]
        public IActionResult SalaryPrevious()
        {
            string PageId = "10908";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.que_employee_salary_previous
                join emp in _context.tbl_employee
                    on a.emp_id equals emp.emp_id
                orderby a.sal_id descending
                select new SalaryPreviousViewModel
                {
                    id = a.sal_id
                }).ToList();

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/SalaryPrevious", "ADD|DEL", PageId, Records.Count);
            return PartialView("Payroll/_SalaryPrevious", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalaryPreviousList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from s in _context.que_employee_salary_previous
                        join emp in _context.tbl_employee
                            on s.emp_id equals emp.emp_id
                        orderby s.sal_id descending
                        select new
                        {
                            id = s.sal_id,
                            emp_id = s.emp_id,
                            sal_year = Convert.ToInt16(s.sal_year),
                            sal_month = Convert.ToInt16(s.sal_month),
                            basicsalary = Convert.ToDecimal(s.t_basic_salary),
                            pfaddition = Convert.ToDecimal(s.t_pf),
                            allowance = Convert.ToDecimal(s.t_allow),
                            remoteareaallowance = Convert.ToDecimal(s.t_raa),
                            lipreimbursement = Convert.ToDecimal(s.t_lip_rem),
                            dashainbonus = Convert.ToDecimal(s.t_dashain),
                            betalabideduction = Convert.ToDecimal(s.t_betalabi),
                            pfdeduction = Convert.ToDecimal(s.t_pf_d),
                            citdeduction = Convert.ToDecimal(s.t_cit_d),
                            prevyearexcesslesstax = Convert.ToDecimal(s.t_tax_pre),
                            taxdeduction = Convert.ToDecimal(s.t_tax),
                            remarks = s.remarks,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status
                        };
            // Search filter
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e =>
                    e.firstname.ToString().Contains(searchValue) ||
                    e.middlename.ToString().Contains(searchValue) ||
                    e.lastname.ToString().Contains(searchValue) ||
                    e.sal_year.ToString().Contains(searchValue) ||
                    e.sal_month.ToString().Contains(searchValue) ||
                    e.remarks.Contains(searchValue));
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else if(sortColumn == "sal_year" || sortColumn == "sal_month")
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();

            int recordsTotal = data.Count();
            if (pageSize == -1) pageSize = recordsTotal;

            var cData = data.Skip(skip).Take(pageSize).ToList();

            return Json(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = cData
            });
        }
        public IActionResult SalaryPreviousAddEdit(int? id, string? mode)
        {
            string PageId = "10908";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            //ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            //ViewBag.Status = StatusActivePassive("AD", "A");
            int salyear = DateTime.Now.Year;
            int salmonth = DateTime.Now.Month;

            SalaryPreviousViewModel model;
            if (mode == "add")
            {
                model = new SalaryPreviousViewModel
                {
                    salyear = salyear,
                    salmonth = salmonth
                };
                ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
                ViewBag.YearDropDown = _settingsServices.GetYears(salyear);
                ViewBag.MonthDropDown = _settingsServices.GetMonths(salmonth);
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Payroll/_SalaryPreviousAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (!id.HasValue || id <= 0) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                var ec = (from e in _context.que_employee_salary_previous
                            join emp in _context.tbl_employee
                                on e.emp_id equals emp.emp_id
                            where e.sal_id == id
                            select new
                            {
                                e.sal_id,
                                e.emp_id,
                                sal_year = Convert.ToInt16(e.sal_year),
                                sal_month = Convert.ToInt16(e.sal_month),
                                t_basic_salary = Convert.ToDecimal(e.t_basic_salary),
                                t_betalabi = Convert.ToDecimal(e.t_betalabi),
                                t_pf = Convert.ToDecimal(e.t_pf),
                                t_pf_d = Convert.ToDecimal(e.t_pf_d),
                                t_allow = Convert.ToDecimal(e.t_allow),
                                t_cit_d = Convert.ToDecimal(e.t_cit_d),
                                t_raa = Convert.ToDecimal(e.t_raa),
                                t_lip_rem = Convert.ToDecimal(e.t_lip_rem),
                                t_tax_pre = Convert.ToDecimal(e.t_tax_pre),
                                t_dashain = Convert.ToDecimal(e.t_dashain),
                                t_tax = Convert.ToDecimal(e.t_tax),
                                e.remarks,
                                e.fiscal_year,
                                e.emp_week,
                                emp.firstname,
                                emp.middlename,
                                emp.lastname,
                                emp.emp_code,
                                employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                emp.emp_status
                            }).FirstOrDefault();
                if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                model = new SalaryPreviousViewModel
                {
                    id = ec.sal_id,
                    empid = ec.emp_id,
                    salyear = ec.sal_year,
                    salmonth = ec.sal_month,
                    basicsalary = (decimal?)ec.t_basic_salary,
                    betalabideduction = (decimal?)ec.t_betalabi,
                    pfaddition = (decimal?)ec.t_pf,
                    pfdeduction = (decimal?)ec.t_pf_d,
                    allowance = (decimal?)ec.t_allow,
                    citdeduction = (decimal?)ec.t_cit_d,
                    remoteareaallowance = (decimal?)ec.t_raa,
                    lipreimbursement = (decimal?)ec.t_lip_rem,
                    prevyearexcesslesstax = (decimal?)ec.t_tax_pre,
                    dashainbonus = (decimal?)ec.t_dashain,
                    taxdeduction = (decimal?)ec.t_tax,
                    remarks = ec.remarks,
                    fiscalyear = ec.fiscal_year,
                    empweek = ec.emp_week,
                    firstname = ec.firstname,
                    middlename = ec.middlename,
                    lastname = ec.lastname,
                    empcode = ec.emp_code,
                    employee = $"{ec.firstname} {ec.middlename} {ec.lastname} ({ec.emp_code})",
                    emp_status = ec.emp_status
                };
                ViewBag.Employee = ec.employee;
                ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                ViewBag.YearDropDown = _settingsServices.GetYears(salyear);
                ViewBag.MonthDropDown = _settingsServices.GetMonths(salmonth);
                return PartialView("Payroll/_SalaryPreviousAddEdit", model);
            }
            return BadRequest(new { success = false, message = Lang.msg_error });
        }

        [HttpPost]
        public async Task<IActionResult> SalaryPreviousSave(SalaryPreviousViewModel model)
        {
            ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "error", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10908", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            string id = Request.Form["id"].ToString() ?? string.Empty;
            if (mode == "add") // ADD NEW
            {
                // Generate new ID (assuming sal_id is not identity)
                var maxId = _context.tbl_employee_salary_previous.Max(s => (int?)s.sal_id) ?? 0;
                var newId = maxId + 1;

                var entity = new tbl_employee_salary_previous
                {
                    sal_id = newId,
                    emp_id = model.empid,
                    sal_year = (short?)model.salyear,
                    sal_month = (short?)model.salmonth,
                    t_basic_salary = (double?)model.basicsalary ?? 0,
                    t_betalabi = (double?)model.betalabideduction ?? 0,
                    t_pf = (double?)model.pfaddition ?? 0,
                    t_pf_d = (double?)model.pfdeduction ?? 0,
                    t_allow = (double?)model.allowance ?? 0 ,
                    t_cit_d = (double?)model.citdeduction ?? 0,
                    t_raa = (double?)model.remoteareaallowance ?? 0,
                    t_lip_rem = (double?)model.lipreimbursement ?? 0,
                    t_tax_pre = (double?)model.prevyearexcesslesstax ?? 0,
                    t_dashain = (double?)model.dashainbonus ?? 0,
                    t_tax = (double?)model.taxdeduction ?? 0,
                    remarks = model.remarks ?? "",
                    fiscal_year = model.fiscalyear ?? "",
                    emp_week = model.empweek ?? 0
                };

                _context.tbl_employee_salary_previous.Add(entity);
                await _context.SaveChangesAsync();

                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                var entity = _context.tbl_employee_salary_previous
                    .FirstOrDefault(s => s.sal_id == int.Parse(id));

                if (entity == null)
                    return Json(new { status = "notfound" });

                entity.emp_id = model.empid;
                entity.sal_year = (short?)model.salyear;
                entity.sal_month = (short?)model.salmonth;
                entity.t_basic_salary = (double?)model.basicsalary;
                entity.t_betalabi = (double?)model.betalabideduction;
                entity.t_pf = (double?)model.pfaddition;
                entity.t_pf_d = (double?)model.pfdeduction;
                entity.t_allow = (double?)model.allowance;
                entity.t_cit_d = (double?)model.citdeduction;
                entity.t_raa = (double?)model.remoteareaallowance;
                entity.t_lip_rem = (double?)model.lipreimbursement;
                entity.t_tax_pre = (double?)model.prevyearexcesslesstax;
                entity.t_dashain = (double?)model.dashainbonus;
                entity.t_tax = (double?)model.taxdeduction;
                entity.remarks = model.remarks;
                entity.fiscal_year = model.fiscalyear;
                entity.emp_week = model.empweek;

                await _context.SaveChangesAsync();

                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SalaryPreviousDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_selected });
            }

            // Fetch records that match the IDs
            var records = _context.tbl_employee_salary_previous
                .Where(r => request.SelectedIds.Contains(r.sal_id.ToString()))
                .ToList();

            if (!records.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_found });
            }

            // Remove them
            _context.tbl_employee_salary_previous.RemoveRange(records);
            var deletedCount = await _context.SaveChangesAsync();

            return Json(new { status = "success", message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletedCount.ToString()) });
        }

        #endregion
        /********************************************************************************************************************/
        #region 10905 EMPLOYEE ADVANCE
        [HttpGet]
        public IActionResult AdvanceCSP(string? year, string? month, string status)
        {
            string PageId = "10905";
            string? AdvYear = year;
            string? AdvMonth = month;
            string blnShow = "true";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";

            if (status != "A")
            {
                perm = "false";
                blnShow = "true";
                return Json(new { status = "success", blnShow, perm });
            }
            else
            {
                if (string.IsNullOrWhiteSpace(AdvYear) || string.IsNullOrWhiteSpace(AdvMonth))
                {
                    //silent
                }
                else
                {
                    short adv_year = Convert.ToInt16(AdvYear);
                    short adv_month = Convert.ToInt16(AdvMonth);
                    if (adv_year < 1 || adv_month < 1)
                    {
                        //silent
                    }
                    else
                    {
                        var PSal = _context.tbl_employee_salary.Where(a => a.sal_year == adv_year && a.sal_month == adv_month).ToList();
                        if (PSal.Any())
                        {
                            perm = "false";
                            blnShow = "false";
                        }
                        return Json(new { status = "success", blnShow, perm });
                    }
                }
            }    
            return Json(new { status = "fail", blnShow, perm });
        }
        [HttpGet]
        public IActionResult Advance()
        {
            string PageId = "10905";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            short adv_year = Convert.ToInt16(DateTime.Now.Year);
            short adv_month = Convert.ToInt16(DateTime.Now.Month);
            /** need to check if already saved salary for the month and year */
            var PSal = _context.tbl_employee_salary.Where(a => a.sal_year == adv_year && a.sal_month == adv_month).ToList();
            int count = 0;
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            if (PSal.Any())
            {
                /**Salary already processed for the selected period.*/
                ViewBag.epern = "false";
                ViewBag.msg_cant_edit_salary_processed = "The salary has already been processed so that it can not be edited.";
            }
            else
            {
                ViewBag.msg_cant_edit_salary_processed = "";
                ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                count = 1;
            }
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/Advance", "DOWNLOAD-FORMAT|IMPORT|EXPORT", PageId, count);
            return PartialView("Payroll/_Advance", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.Status;
            int yearFilter = request.Year ?? 0;
            int monthFilter = request.Month ?? 0;

            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_advance.Where(adv => adv.adv_year == yearFilter && adv.adv_month == monthFilter)
                        on emp.emp_id equals lft.emp_id into LeftJoin
                        from lft in LeftJoin.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == EmployeeStatusFilter
                        select new AdvanceViewModel
                        {
                            emp_id = emp.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            emp_code = emp.emp_code,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                            adv_month = lft.adv_month,
                            adv_year = lft.adv_year,
                            adv_personnel = lft.adv_personnel ?? 0,
                            adv_program = lft.adv_program ?? 0,
                            adv_travel = lft.adv_travel ?? 0,
                            adv_field_drawing = lft.adv_field_drawing ?? 0,
                            adv_pf_loan = lft.adv_PF_loan ?? 0,
                            adv_cit_loan = lft.adv_CIT_loan ?? 0,
                            adv_welfare = lft.adv_welfare ?? 0
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AdvanceSave([FromBody] AdvanceListViewModel model)
        {
            string PageId = "10905";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            _ = ModelState.Remove("adv_id");
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var item in model.Fields)
            {
                if (!item.adv_year.HasValue || !item.adv_month.HasValue) { continue; }

                /** need to check if already saved salary for the month and year */
                var PSal = _context.tbl_employee_salary
                .Where(a => a.sal_year == item.adv_year
                             && a.sal_month == item.adv_month)
                .ToList();
                if (PSal.Any()) { continue; } /**Salary already processed for the selected period.*/

                var existing = _context.tbl_employee_advance
                .Where(a => a.emp_id == item.emp_id
                            && a.adv_year == item.adv_year
                            && a.adv_month == item.adv_month)
                .ToList();

                if (existing.Any())
                {
                    _context.tbl_employee_advance.RemoveRange(existing);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }

                bool allZero = item.adv_personnel.GetValueOrDefault() == 0m &&
                               item.adv_program.GetValueOrDefault() == 0m &&
                               item.adv_travel.GetValueOrDefault() == 0m &&
                               item.adv_field_drawing.GetValueOrDefault() == 0m &&
                               item.adv_welfare.GetValueOrDefault() == 0m &&
                               item.adv_pf_loan.GetValueOrDefault() == 0m &&
                               item.adv_cit_loan.GetValueOrDefault() == 0m;

                if (!allZero)
                {
                    var newAdvance = new tbl_employee_advance
                    {
                        adv_id = Guid.NewGuid().ToString(),
                        emp_id = item.emp_id,
                        adv_year = item.adv_year.Value,
                        adv_month = item.adv_month.Value,
                        adv_personnel = item.adv_personnel.GetValueOrDefault(),
                        adv_program = item.adv_program.GetValueOrDefault(),
                        adv_travel = item.adv_travel.GetValueOrDefault(),
                        adv_field_drawing = item.adv_field_drawing.GetValueOrDefault(),
                        adv_welfare = item.adv_welfare.GetValueOrDefault(),
                        adv_PF_loan = item.adv_pf_loan.GetValueOrDefault(),
                        adv_CIT_loan = item.adv_cit_loan.GetValueOrDefault(),
                        adv_fiscal_year = "",
                        adv_emp_week = 0
                    };
                    _ = _context.tbl_employee_advance.Add(newAdvance);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
            }

            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdvanceDownloadFormat()
        {
            var sb = new StringBuilder();
            var header = new List<string> { "Employee Name", "ID", "Personal Advance", "Program Advance", "Travel Advance", "Welfare Advance", "Field Drawing", "PF Loan", "CIT Loan" };
            _ = sb.AppendLine(string.Join(",", header));

            var employees = _context.tbl_employee
                .Where(emp => emp.emp_id != 0 &&
                              emp.emp_status == "A" &&
                              _context.tbl_employee_salary_extra_settings
                                  .Where(ses => ses.is_field_salary == "N")
                                  .Select(ses => ses.emp_id)
                                  .Contains(emp.emp_id))
                .OrderBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .Select(emp => new
                {
                    emp.emp_id,
                    emp.emp_code,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname}"
                })
                .ToList();
            int cnt = 0;
            if (employees.Count > 0)
            {
                foreach (var record in employees)
                {
                    cnt++;
                    string emp_code = EscapeCSV(record.emp_code ?? "");
                    string employee = EscapeCSV(record.employee ?? "");
                    var NewValue = new List<string> { emp_code, employee, "0", "0", "0", "0", "0", "0", "0" };
                    _ = sb.AppendLine(string.Join(",", NewValue));
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "EmployeeAdvanceDownloadedFormat.csv");
        }
        [HttpGet]
        public IActionResult AdvanceImport(string? adv_year, string? adv_month)
        {
            string PageId = "10905";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            short AdvYear;
            short AdvMonth;
            string advMonthName = "";
            if (string.IsNullOrWhiteSpace(adv_year) || string.IsNullOrWhiteSpace(adv_month)) {
                AdvYear = 0;
                AdvMonth = 0;
            }
            else
            {
                AdvYear = Convert.ToInt16(adv_year);
                AdvMonth = Convert.ToInt16(adv_month);
                advMonthName = GblUtilities.MonthName(AdvMonth);
            }
            string blnShow = "true";
            var PSal = _context.tbl_employee_salary.Where(a => a.sal_year == AdvYear && a.sal_month == AdvMonth).ToList();
            if (PSal.Any()) { blnShow = "false"; }

            ViewBag.adv_year = AdvYear;
            ViewBag.adv_month = AdvMonth;
            ViewBag.advMonthName = advMonthName;
            ViewBag.blnShow = blnShow;
            return PartialView("Payroll/_AdvanceImport");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceImportSave(IFormFile file)
        {
            if (file == null || file.Length == 0) { return Json(new { status = "error", message = Lang.NO_FILE_UPLOADED }); }
            if (!FileValidator.ForCsv(file)) { return Json(new { status = "error", message = "There is problem with File." }); }

            string? SelYear = Request.Form["adv_year"];
            string? SelMonth = Request.Form["adv_month"];

            if (string.IsNullOrEmpty(SelYear) || string.IsNullOrEmpty(SelMonth)) { return Json(new { status = "error", message = "Not valid year and/or month" }); }

            short AdvYear = Convert.ToInt16(SelYear);
            short AdvMonth = Convert.ToInt16(SelMonth);

            if (AdvYear < 1 || AdvMonth < 1) { return Json(new { status = "error", message = "Not valid year and/or month" }); }

            var errors = new List<string>();

            using var reader = new StreamReader(file.OpenReadStream());
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
            headerLine = headerLine.Replace("\r", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\n", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\"", "", StringComparison.OrdinalIgnoreCase);
            var headers = headerLine.Split(',').Select(h => h.Trim('"')).ToList();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) { continue; }

                if (line != null)
                {
                    line = line.Replace("\"", "", StringComparison.OrdinalIgnoreCase);
                    var values = line.Split(',').Select(v => v.Trim('"')).ToList();
                    string empCode = values[0];
                    string employeeCode = _employeeServices.GetValidEmpCode(empCode);
                    var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_code == employeeCode); // INSTEAD OF THIS SECTION paymgr.getIDByEmpCode(s_emp_code)

                    if (emp == null || emp.emp_status != "A") {
                        errors.Add("> " + Lang.INACTIVE_EMPLOYEE.Replace("<[EMP-CODE]>", employeeCode, StringComparison.OrdinalIgnoreCase));
                        continue;
                    }

                    var existing = _context.tbl_employee_advance
                    .Where(a => a.emp_id == emp.emp_id
                                && a.adv_year == AdvYear
                                && a.adv_month == AdvMonth)
                    .ToList();

                    if (existing.Any())
                    {
                        _context.tbl_employee_advance.RemoveRange(existing);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }

                    decimal txtadvpe = Convert.ToDecimal(values[2]);  //personal
                    decimal txtadvpr = Convert.ToDecimal(values[3]);  //program
                    decimal txtadvtr = Convert.ToDecimal(values[4]);  //travel
                    decimal txtadvfd = Convert.ToDecimal(values[5]);  //field drawing
                    decimal txtadvwl = Convert.ToDecimal(values[6]);  //welfare
                    decimal txtadvpf = Convert.ToDecimal(values[7]);  //pfloan
                    decimal txtadvcit = Convert.ToDecimal(values[8]); //citloan

                    bool allZero = new[] { txtadvpe, txtadvpr, txtadvtr, txtadvfd, txtadvwl, txtadvpf, txtadvcit }
                                   .All(x => x == 0m);

                    if (!allZero)
                    {
                        var newAdvance = new tbl_employee_advance
                        {
                            adv_id = UniqueID(),
                            emp_id = emp.emp_id,
                            adv_year = AdvYear,
                            adv_month = AdvMonth,
                            adv_personnel = txtadvpe,
                            adv_program = txtadvpr,
                            adv_travel = txtadvtr,
                            adv_field_drawing = txtadvfd,
                            adv_welfare = txtadvwl,
                            adv_PF_loan = txtadvpf,
                            adv_CIT_loan = txtadvcit,
                            adv_fiscal_year = "",
                            adv_emp_week = 0
                        };
                        _ = _context.tbl_employee_advance.Add(newAdvance);
                        _ = _context.SaveChanges();
                        _context.ChangeTracker.Clear();
                    }
                }
            }
            return Json(new { status = "success", message = Lang.EMPLOYEE_FUND_SOURCE_IMPORT_SUCCESSFUL });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdvanceExport()
        {
            var sb = new StringBuilder();
            string? AdvYear = Request.Form["YearFilter"];
            string? AdvMonth = Request.Form["MonthFilter"];
            if (string.IsNullOrWhiteSpace(AdvYear) || string.IsNullOrWhiteSpace(AdvMonth)) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }
            short adv_year = Convert.ToInt16(AdvYear);
            short adv_month = Convert.ToInt16(AdvMonth);
            string StrMonthName = MonthName(adv_month);
            if (adv_year < 1 || adv_month < 1) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }

            // Query employee + advance data
            var employees = (from e in _context.tbl_employee
                             where e.emp_id != 0 && e.emp_status == "A"
                             join a in _context.tbl_employee_advance
                                 on e.emp_id equals a.emp_id
                             where a.adv_year == adv_year && a.adv_month == adv_month
                             orderby e.firstname, e.middlename, e.lastname
                             select new
                             {
                                 EmployeeId = e.emp_id,
                                 EmpCode = e.emp_code,
                                 FullName = string.Join(" ", new[] { e.firstname, e.middlename, e.lastname }
                                                               .Where(x => !string.IsNullOrEmpty(x))),
                                 Personnel = a.adv_personnel ?? 0,
                                 Program = a.adv_program ?? 0,
                                 Travel = a.adv_travel ?? 0,
                                 Welfare = a.adv_welfare ?? 0,
                                 FieldDrawing = a.adv_field_drawing ?? 0,
                                 PFLoan = a.adv_PF_loan ?? 0,
                                 CITLoan = a.adv_CIT_loan ?? 0
                             }).ToList();

            _ = sb.AppendLine($",Period:,{StrMonthName}|{adv_year.ToString()},,,,,,,");
            _ = sb.AppendLine("SN, ID, Employee Name,Personal Advance,Program Advance,Travel Advance,Welfare Advance,Field Drawing,PF Loan,CIT Loan");
            int cnt = 0;
            foreach (var row in employees)
            {
                cnt++;
                var line = new List<string>
                {
                    cnt.ToString(),
                    row.EmpCode.ToString(),
                    row.FullName,
                    row.Personnel.ToString(),
                    row.Program.ToString(),
                    row.Travel.ToString(),
                    row.Welfare.ToString(),
                    row.FieldDrawing.ToString(),
                    row.PFLoan.ToString(),
                    row.CITLoan.ToString()
                };
                _ = sb.AppendLine(string.Join(",", line.Select(x => $"\"{x}\"")));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"EmployeeAdvanceExport_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            var filePath = Path.Combine(GblDocumentPath, "temp", fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            return Json(new { status = "success", message = "Export successful!", url = "/uploads/temp/" + fileName });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10913 PF
        [HttpGet]
        public IActionResult PF()
        {
            string PageId = "10913";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.PFGroupList = _payrollServices.GetPFGroupList();
            ViewBag.PFTypeList = _payrollServices.GetPFTypeList();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Payroll/_PF", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PFList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue;

            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_pf
                        on emp.emp_id equals lft.emp_id into LeftJoin
                        from lft in LeftJoin.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == EmployeeStatusFilter
                        select new PFViewModel
                        {
                            emp_id = emp.emp_id,
                            salary = emp.salary ?? 0,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            emp_code = emp.emp_code,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                            gender = emp.gender == "M" ? "Male" : "Female",
                            join_date = emp.join_date,
                            end_date = emp.end_date,
                            pf_no = emp.pf_no ?? "",
                            pf_group = lft.pf_group ?? "",
                            pf_type = lft.pf_type ?? "",
                            add_percent_amount = lft.add_percent_amount ?? 0.00,
                            ded_percent_amount = lft.ded_percent_amount ?? 0.00
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PFSave([FromBody] PFListViewModel model)
        {
            string PageId = "10913";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var emp in model.Fields)
            {
                var DataUpdate = _context.tbl_employee_pf.FirstOrDefault(h => h.emp_id == emp.emp_id);
                //UPDATE EXISTING RECORDS IF EXIST
                if (DataUpdate != null)
                {
                    DataUpdate.pf_group = emp.pf_group;
                    DataUpdate.pf_type = emp.pf_type;
                    DataUpdate.add_percent_amount = emp.add_percent_amount;
                    DataUpdate.ded_percent_amount = emp.ded_percent_amount;
                    _ = _context.tbl_employee_pf.Update(DataUpdate);
                }
                else
                {
                    // INSERT IF DATA DOESNOT EXIST
                    int maxId = _context.tbl_employee_pf.Max(e => (int?)e.emp_pf_id) ?? 0;
                    maxId++;
                    var newRow = new tbl_employee_pf
                    {
                        emp_pf_id = maxId,
                        emp_id = emp.emp_id,
                        pf_group = emp.pf_group,
                        pf_type = emp.pf_type,
                        add_percent_amount = emp.add_percent_amount,
                        ded_percent_amount = emp.ded_percent_amount
                    };
                    _ = _context.tbl_employee_pf.Add(newRow);
                }
                if (emp?.h_pf_no != emp?.pf_no)
                {
                    var employee = _context.tbl_employee.FirstOrDefault(e => e.emp_id == emp.emp_id);
                    if (employee != null)
                    {
                        employee.pf_no = emp.pf_no;
                        _ = _context.tbl_employee.Update(employee);
                    }
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10915 SSF
        [HttpGet]
        public IActionResult SSF()
        {
            string PageId = "10915";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Payroll/_SSF", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SSFList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue;

            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_ssf_info
                        on emp.emp_id equals lft.emp_id into LeftJoin
                        from lft in LeftJoin.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == EmployeeStatusFilter
                        select new SSFViewModel
                        {
                            emp_id = emp.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            emp_code = emp.emp_code,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                            gender = emp.gender == "M" ? "Male" : "Female",
                            join_date = emp.join_date,
                            end_date = emp.end_date,
                            salary = emp.salary.ToString() != null ? emp.salary : 0,
                            ssf_number = lft.ssf_number ?? "",
                            add_percent = lft.add_percent ?? 1.67,
                            ded_percent = lft.ded_percent ?? 2.67,
                            add_percent_amount = lft.add_percent_amount ?? 0.00,
                            ded_percent_amount = lft.ded_percent_amount ?? 0.00
                        };

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SSFSave([FromBody] SSFListViewModel model)
        {
            string PageId = "10915";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }

            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var emp in model.Fields)
            {
                var DataUpdate = _context.tbl_employee_ssf_info.FirstOrDefault(h => h.emp_id == emp.emp_id);
                //UPDATE EXISTING RECORDS IF EXIST
                if (DataUpdate != null)
                {
                    DataUpdate.ssf_number = emp.ssf_number;
                    DataUpdate.add_percent = emp.add_percent;
                    DataUpdate.ded_percent = emp.ded_percent;
                    DataUpdate.add_percent_amount = emp.add_percent_amount;
                    DataUpdate.ded_percent_amount = emp.ded_percent_amount;
                    _ = _context.tbl_employee_ssf_info.Update(DataUpdate);
                }
                else
                {
                    // INSERT IF DATA DOESNOT EXIST
                    int maxId = _context.tbl_employee_ssf_info.Max(e => (int?)e.id) ?? 0;
                    maxId++;
                    var newRow = new tbl_employee_ssf_info
                    {
                        id = maxId,
                        emp_id = emp.emp_id,
                        ssf_number = emp.ssf_number,
                        add_percent = emp.add_percent,
                        ded_percent = emp.ded_percent,
                        add_percent_amount = emp.add_percent_amount,
                        ded_percent_amount = emp.ded_percent_amount,
                    };
                    _ = _context.tbl_employee_ssf_info.Add(newRow);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10902 CIT
        [HttpGet]
        public IActionResult CIT()
        {
            string PageId = "10902";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.CitTypeList = _payrollServices.CITType();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Payroll/_CIT", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CITList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue;

            var query = from emp in _context.tbl_employee
                        join lft in _context.tbl_employee_cit
                        on emp.emp_id equals lft.emp_id into LeftJoin
                        from lft in LeftJoin.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == EmployeeStatusFilter
                        select new CitViewModel
                        {
                            emp_id = emp.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            emp_code = emp.emp_code,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                            gender = emp.gender == "M" ? "Male" : "Female",
                            join_date = emp.join_date,
                            end_date = emp.end_date,
                            cit_no = emp.cit_no ?? "",
                            cit_type = lft.cit_type ?? "",
                            percent_amount = lft.percent_amount ?? 0,
                            remarks = lft.remarks ?? ""
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CITSave([FromBody] CitListViewModel model)
        {
            string PageId = "10902";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }
            foreach (var emp in model.Fields)
            {
                var DataUpdate = _context.tbl_employee_cit.FirstOrDefault(h => h.emp_id == emp.emp_id);
                //UPDATE EXISTING RECORDS IF EXIST
                if (DataUpdate != null)
                {
                    DataUpdate.cit_type = emp.cit_type;
                    DataUpdate.percent_amount = emp.percent_amount;
                    DataUpdate.remarks = emp.remarks;
                    _ = _context.tbl_employee_cit.Update(DataUpdate);
                }
                else
                {
                    // INSERT IF DATA DOESNOT EXIST
                    int maxId = _context.tbl_employee_cit.Max(e => (int?)e.emp_cit_id) ?? 0;
                    maxId++;
                    var newRow = new tbl_employee_cit
                    {
                        emp_cit_id = maxId,
                        cit_type = emp.cit_type,
                        percent_amount = emp.percent_amount,
                        remarks = emp.remarks,
                        emp_id = emp.emp_id
                    };
                    _ = _context.tbl_employee_cit.Add(newRow);
                }
                if (emp?.h_cit_no != emp?.cit_no)
                {
                    var employee = _context.tbl_employee.FirstOrDefault(e => e.emp_id == emp.emp_id);
                    if (employee != null)
                    {
                        employee.cit_no = emp.cit_no;
                        _ = _context.tbl_employee.Update(employee);
                    }
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10901 BLOCK PAY SLIP
        [HttpGet]
        public IActionResult PaySlipBlock()
        {
            string PageId = "10901";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Payroll/_PaySlipBlock", "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaySlipBlockList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue;

            var query = from emp in _context.tbl_employee
                        join blk in _context.tbl_employee_salary_block
                        on emp.emp_id equals blk.emp_id into empSalaryBlock
                        from blk in empSalaryBlock.DefaultIfEmpty()   // LEFT JOIN
                        where emp.emp_status == EmployeeStatusFilter
                        select new PaySlipBlockViewModel
                        {
                            emp_id = emp.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            emp_code = emp.emp_code,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                            gender = emp.gender == "M" ? "Male" : "Female",
                            join_date = emp.join_date,
                            end_date = emp.end_date,
                            sal_year = (short)(blk.sal_year ?? (short)DateTime.Now.Year),
                            sal_month = (short)(blk.sal_month ?? (short)DateTime.Now.Month),
                            block_status = blk.emp_id != null ? "Yes" : "No"
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaySlipBlockSave([FromBody] PaySlipBlockListViewModel model)
        {
            string PageId = "10901";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            _ = ModelState.Remove("selectedIds");
            _ = ModelState.Remove("Fields");

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }

            var recordsToDelete = _context.tbl_employee_salary_block.ToList();
            if (recordsToDelete.Any())
            {
                _context.tbl_employee_salary_block.RemoveRange(recordsToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }

            if (model.Fields != null && model.Fields.Count > 0)
            {
                foreach (var emp in model.Fields)
                {
                    var newRow = new tbl_employee_salary_block
                    {
                        id = UniqueID(),
                        emp_id = emp.emp_id,
                        sal_year = emp.sal_year,
                        sal_month = emp.sal_month,
                        fiscal_year = null,
                        emp_week = 0
                    };
                    _ = _context.tbl_employee_salary_block.Add(newRow);
                }
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion        
        /********************************************************************************************************************/
        #region 10914 SEND PAY SLIP
        [HttpGet]
        public IActionResult PaySlipSend()
        {
            string PageId = "10914";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.StatusFilter = StatusActivePassive("AD", "A");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);
            return PartialView("Payroll/_PaySlipSend", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaySlipSendList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var Status = request.Status;
            int Year = request.Year ?? 0;
            int Month = request.Month ?? 0;

            var query = from e in _context.tbl_employee
                        join b in _context.tbl_employee_salary_block
                        .Where(b => b.sal_year == Year && b.sal_month == Month)
                            on e.emp_id equals b.emp_id into blocks
                        from b in blocks.DefaultIfEmpty()   // left join
                        orderby e.emp_id descending
                        select new
                        {
                            e.emp_id,
                            e.emp_code,
                            fullname = e.firstname + " " + e.middlename + " " + e.lastname,
                            e.e_mail,
                            e.emp_status,
                            isblocked = b != null,
                            e.firstname,
                            e.lastname
                        };

            query = query.Where(d => d.emp_status == Status);
            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e => e.firstname.Contains(searchValue) || e.lastname.Contains(searchValue) || e.emp_code.Contains(searchValue) || e.e_mail.Contains(searchValue));
            }
            // Sorting (requires System.Linq.Dynamic.Core)
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy($"{sortColumn} {sortColumnDir}");
            }
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData.Select((x, index) => new
                {
                    x.emp_id,
                    x.emp_code,
                    x.fullname,
                    x.e_mail,
                    x.emp_status,
                    blockedicon = x.isblocked
                            ? "<img src='/images/delete.png' title='Pay Slip Blocked'>"
                            : "<img src='/images/right.png' title='Pay Slip Not Blocked'>"
                })
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaySlipPreview([FromBody] PaySlipPreviewRequest request)
        {
            if (request.SelectedIds == null || !request.SelectedIds.Any())
            {
                return Content("<font color='red'><b>No employee selected for slip preview</b></font>", "text/html");
            }

            string op_org_name = _globalOptionServices.OptionServices["op_org_name"];
            string op_org_addr = _globalOptionServices.OptionServices["op_org_addr"];
            string op_currency_symbol = _globalOptionServices.OptionServices["op_currency_symbol"];
            bool isDiffMonth = _paySlipManager.GetIsMonthHasDiff(request.Year, request.Month);

            ViewBag.op_org_name = op_org_name;
            ViewBag.op_org_addr = op_org_addr;
            ViewBag.op_currency_symbol = op_currency_symbol;
            ViewBag.DiffMonth = isDiffMonth;
            ViewBag.Period = $"Statement of Salary for the month of {MonthName(request.Month)}-{request.Year}";
            var slips = new List<PaySlipViewModel>();
            foreach (var empId in request.SelectedIds)
            {
                var slip = _paySlipManager.GetPaySlipSingle(empId, request.Year, request.Month, isDiffMonth);
                if (slip != null) { slips.Add(slip); }
            }

            if (!slips.Any())
            {
                return Content("<table><tr><td align='center'><b>No record found</b></td></tr></table>", "text/html");
            }

            return PartialView("Payroll/_PaySlipPreview", slips);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PaySlipSendSubmit([FromBody] PaySlipSubmitListViewModel model)
        {
            string PageId = "10914";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            int sal_year = Convert.ToInt32(model.year);
            int sal_month = Convert.ToInt32(model.month);
            string StrTMsg = model.message ?? "";
            if (sal_year < 1 || sal_month < 1) { return Json(new { status = "error", message = Lang.msg_insufficient_info }); }

            string StrPaySlipOf = $"{MonthName(sal_month)}/{sal_year}";
            string Subject = Lang.EMAIL_SALARY_PAY_SLIP_SEND_SUBJECT.Replace("<[MONTH-NAME]>", StrPaySlipOf, StringComparison.OrdinalIgnoreCase);
            int SendCount = 0;
            foreach (var update in model.Fields)
            {
                var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_id == update.emp_id);
                if (emp == null) { continue; }
                int emp_id = emp.emp_id;
                string EmployeeName = _employeeServices.GetEmployeeName(emp_id, "NameOnly");
                string SetEmail = _employeeServices.GetEmployeeNameEmail(emp_id);
                string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                if (string.IsNullOrWhiteSpace(ToEmail)) { continue; }

                string linkClickHere = ""; // for security reason, link na pathune ho ki 
                string Message = "";
                string isBlockedPaySlip = "N";
                bool isBlocked = _context.tbl_employee_salary_block
                    .Any(b => b.emp_id == emp_id && b.sal_year == sal_year && b.sal_month == sal_month);
                if (isBlocked) { isBlockedPaySlip = "Y"; }

                Message = isBlockedPaySlip == "Y" ? Lang.EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE_BLK : Lang.EMAIL_SALARY_PAY_SLIP_SEND_MESSAGE;
                string LinkInEmail = $"<a href='{_appSettings.BaseUrl}Personnel/PaySlipEmailMiddle?emp_id={emp_id}&salyear={sal_year}&salmonth={sal_month}&st=view&tstype=monthly'>View</a>";

                Message = Message
                    .Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal)
                    .Replace("<[MONTH-NAME]>", StrPaySlipOf, StringComparison.Ordinal)
                    .Replace("<[TYPED-MESSAGE]>", StrTMsg, StringComparison.Ordinal)
                    .Replace("<[VIEW-LINK]>", LinkInEmail, StringComparison.Ordinal);
                string emst = _emailService.SendEmail("PaySlip", ToEmail, Subject, Message);
                if (emst == "true") { SendCount++; }
            }
            return Json(new { status = "success", message = $"{SendCount} Pay Slip(s) sent." });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10904 DEPENDENT ALLOWANCE
        public IActionResult DependentAllowance(string fiscalYearFilter)
        {
            string PageId = "10904";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.FiscalYearActive = fiscalYearFilter;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));

            var setting = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (setting == null)
            {
                ViewBag.SettingMessage = "Settings not defined yet.";
            }
            return PartialView("Payroll/_DependentAllowance", "");
        }
        public async Task<IActionResult> DependentAllowanceList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var fiscalYearFilter = request.FiscalYearFilter;
            ViewBag.FiscalYearFilter = fiscalYearFilter;

            DateTime? dt_to_check_dependent_age = null;
            decimal first_range_amt = 0;
            decimal second_range_amt = 0;
            double childProRata = 0;
            double empProRata = 0;
            var setting = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (setting != null)
            {
                // Step 1: Load settings
                int max_nos = 0;
                if (!string.IsNullOrEmpty(setting.max_nos_dep_child_eligible_paid.ToString()))
                {
                    max_nos = Convert.ToInt32(setting.max_nos_dep_child_eligible_paid);
                }
                first_range_amt = setting.max_amt_first_age_range ?? 0;
                second_range_amt = setting.max_amt_second_age_range ?? 0;
                dt_to_check_dependent_age = setting.age_checking_date;
                childProRata = setting.child_pro_rata_age ?? 0;
                empProRata = setting.emp_pro_rata_age ?? 0;
            }
            /******************************************************************************************************'
            'UPDATE STATUS TO INACTIVE (ELIGIBILITY = 'I') FOR THE DEPENENDENT WHO CROSSED AGE 25
            '******************************************************************************************************/
            await DeactivateDependent(Convert.ToDateTime(dt_to_check_dependent_age));

            bool? blnShowSave = false;
            // Step 3: Check if allowance already processed for fiscal year
            // Step 1: SQL-side query (base amounts only)
            var dependentsRaw =
                from a in _context.tbl_employee_dependent_children_details
                join b in _context.tbl_employee on a.emp_id equals b.emp_id
                join allowance in _context.tbl_dependent_children_details_allowance
                    on a.emp_dep_id equals allowance.emp_dep_id into allowanceGroup
                from c in allowanceGroup
                    .Where(x => x.fiscal_year == fiscalYearFilter)
                    .DefaultIfEmpty()
                where a.eligibility == "A" && b.emp_status == "A"
                let age = (a.date_of_birth.HasValue && dt_to_check_dependent_age.HasValue)
                    ? Math.Round(((dt_to_check_dependent_age.Value - a.date_of_birth.Value).TotalDays + 1) / 365, 2)
                    : (double?)null
                let service_age = (b.join_date.HasValue && dt_to_check_dependent_age.HasValue)
                    ? Math.Round(((dt_to_check_dependent_age.Value - b.join_date.Value).TotalDays + 1) / 365, 2)
                    : (double?)null
                orderby b.firstname, b.middlename, b.lastname, a.date_of_birth
                select new
                {
                    a.emp_dep_id,
                    a.emp_id,
                    b.firstname,
                    b.middlename,
                    b.lastname,
                    a.date_of_birth,
                    b.emp_status,
                    a.eligibility,
                    fiscalYear = c != null ? c.fiscal_year : null,
                    amount_actual = c != null ? c.amount_actual :
                        (age >= 18 && age < 25
                            ? (_context.tbl_employee_dependent_children_details_sub
                                .Any(sub => sub.emp_dep_id == a.emp_dep_id
                                         && sub.fiscal_year == fiscalYearFilter
                                         && sub.status == "A")
                                ? second_range_amt
                                : 0)
                        : (age >= 25 ? 0 : first_range_amt)),
                    // Base allowance only (like Classic ASP amount_actu)
                    baseAmount = c != null ? c.amount_paid :
                        (age >= 18 && age < 25
                            ? (_context.tbl_employee_dependent_children_details_sub
                                .Any(sub => sub.emp_dep_id == a.emp_dep_id
                                         && sub.fiscal_year == fiscalYearFilter
                                         && sub.status == "A")
                                ? second_range_amt
                                : 0)
                        : (age >= 25 ? 0 : first_range_amt)),
                    age,
                    service_age,
                    ageCheckingDate = c != null ? c.age_checking_date : null,
                    isProcessed = c != null,
                    fullName = b.firstname + " " + b.middlename + " " + b.lastname,
                    c_name = a.c_name,
                    join_date = b.join_date,
                    fiscalYearFilter = fiscalYearFilter,
                    age_checking_date = dt_to_check_dependent_age
                };

            // Step 2: In-memory projection (pro-rata adjustments)
            var dependentsQuery = dependentsRaw
                .AsEnumerable() // switch to LINQ-to-Objects
                .Select(x => new
                {
                    x.emp_dep_id,
                    x.emp_id,
                    x.firstname,
                    x.middlename,
                    x.lastname,
                    x.date_of_birth,
                    x.emp_status,
                    x.eligibility,
                    x.fiscalYear,
                    x.amount_actual,
                    x.baseAmount,

                    // Apply pro-rata rules
                    amount_paid = (x.age.HasValue && x.age < childProRata)
                    ? Math.Round((x.baseAmount ?? 0) * (decimal)x.age.Value, 2)
                    : (x.service_age.HasValue && x.service_age < empProRata)
                        ? Math.Round((x.baseAmount ?? 0) * (decimal)x.service_age.Value, 2)
                        : x.baseAmount,

                    x.ageCheckingDate,
                    x.isProcessed,
                    x.fullName,
                    x.c_name,
                    x.age,
                    x.join_date,
                    x.service_age,
                    fiscalYearFilter = fiscalYearFilter,
                    age_checking_date = dt_to_check_dependent_age
                });



            if (!string.IsNullOrEmpty(searchValue))
            {
                dependentsQuery = dependentsQuery.Where(e => EF.Functions.Like(((string)e.GetType().GetProperty("fullName").GetValue(e)), $"%{searchValue}%")
                                    || EF.Functions.Like(((string)e.GetType().GetProperty("c_name").GetValue(e)), $"%{searchValue}%"));
            }

            var dependents = dependentsQuery.ToList();
            if (dependents.Any(d => d.isProcessed))
            {
                blnShowSave = false;
            }
            else
            {
                blnShowSave = true;
            }

            // Step 5: Fiscal year check
            if (int.Parse(fiscalYearFilter.Substring(0, 4)) != int.Parse(HttpContext.Session.GetString("fiscal_year").Substring(0, 4)) && (bool)ViewBag.blnShowSave)
            {
                // do nothing
            }

            int recordsTotal = dependents.Count;
            if (pageSize == -1) pageSize = recordsTotal;
            var cData = dependents.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = cData,
                blnShow = blnShowSave
            };

            return new JsonResult(jsonData);
        }
        public async Task DeactivateDependent(DateTime checkingDate)
        {
            // Get all dependents with eligibility = 'A'
            var dependents = await _context.tbl_employee_dependent_children_details
                                           .Where(d => d.eligibility == "A")
                                           .ToListAsync();

            foreach (var dep in dependents)
            {
                if (dep.date_of_birth.HasValue)
                {
                    // Calculate age in years (rounded to 2 decimals)
                    var days = (checkingDate - dep.date_of_birth.Value).TotalDays + 1;
                    var age = Math.Round(days / 365, 2);

                    if (age >= 25)
                    {
                        dep.eligibility = "I"; // mark inactive
                    }
                }
            }

            // Save all changes in one batch
            await _context.SaveChangesAsync();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DependentAllowanceSave([FromBody] DependentChildrenDetailsListAllowanceViewModel model)
        {
            string PageId = "10904";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }
            foreach (var update in model.Fields)
            {
                if (update.amount_actual > 0 && update.amount_paid > 0)
                {
                    var nextId = Guid.NewGuid().ToString();
                    var newRow = new tbl_dependent_children_details_allowance
                    {
                        dep_allow_id = nextId,
                        fiscal_year = update.fiscal_year,
                        emp_dep_id = update.emp_dep_id,
                        amount_actual = update.amount_actual,
                        amount_paid = update.amount_paid,
                        age_checking_date = update.age_checking_date
                    };

                    _context.tbl_dependent_children_details_allowance.Add(newRow); // 👈 inside the if
                }
            }
            await _context.SaveChangesAsync();

            // Step 2: Update employee totals
            // Get distinct employee IDs from the dependents you just inserted
            var distinctEmpIds = model.Fields
                .Where(f => f.emp_id > 0)
                .Select(f => f.emp_id)   // make sure your ViewModel includes emp_id
                .Distinct()
                .ToList();

            foreach (var empId in distinctEmpIds)
            {
                var fiscalYear = model.Fields.FirstOrDefault(f => f.fiscal_year != null)?.fiscal_year;

                if (!string.IsNullOrEmpty(fiscalYear))
                {
                    var amountPaid = (from a in _context.tbl_dependent_children_details_allowance
                                      join d in _context.tbl_employee_dependent_children_details
                                          on a.emp_dep_id equals d.emp_dep_id
                                      where a.fiscal_year == fiscalYear
                                            && d.emp_id == empId
                                      select a.amount_paid)
                                 .Sum() ?? 0;

                    var employee = await _context.tbl_employee.FirstOrDefaultAsync(e => e.emp_id == empId);
                    if (employee != null)
                    {
                        employee.child_edu_all = amountPaid;
                        _context.tbl_employee.Update(employee);
                    }
                }
            }
            await _context.SaveChangesAsync();

            // Step 3: Final check
            bool hasRecords = _context.tbl_dependent_children_details_allowance
                .Any(a => a.fiscal_year == model.Fields.First().fiscal_year);

            string msgst = hasRecords ? "updatesuccess" : "errorupdate";
            return Json(new { status = msgst, message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DependentAllowanceClear()
        {
            await _context.Database.ExecuteSqlRawAsync("UPDATE tbl_employee SET child_edu_all=0");
            return Json(new
            {
                status = "success",
                message = "clearsuccess"
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DependentAllowanceDelete(string fiscalYear)
        {
            // Step 1: Delete all dependent allowances for the fiscal year
            var allowancesToDelete = _context.tbl_dependent_children_details_allowance
                .Where(a => a.fiscal_year == fiscalYear);

            _context.tbl_dependent_children_details_allowance.RemoveRange(allowancesToDelete);

            // Step 2: Reset child_edu_all for all employees
            var allEmployees = _context.tbl_employee.ToList();
            foreach (var emp in allEmployees)
            {
                emp.child_edu_all = 0;
                _context.tbl_employee.Update(emp);
            }

            await _context.SaveChangesAsync();

            // Step 3: One more layer check
            bool hasRecords = await _context.tbl_dependent_children_details_allowance
                .AnyAsync(a => a.fiscal_year == fiscalYear);

            string msgst = hasRecords ? "errorupdate" : "updatesuccess";

            return Json(new { status = msgst });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DependentAllowanceExport(string fiscalYear)
        {
            var OrgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name");

            var records = (from dep in _context.tbl_employee_dependent_children_details
                           join emp in _context.tbl_employee on dep.emp_id equals emp.emp_id
                           join allow in _context.tbl_dependent_children_details_allowance on dep.emp_dep_id equals allow.emp_dep_id
                           where allow.fiscal_year == fiscalYear
                           orderby emp.firstname, emp.middlename, emp.lastname, dep.date_of_birth
                           select new
                           {
                               emp.emp_id,
                               FullName = emp.firstname + " " + emp.middlename + " " + emp.lastname,
                               emp.join_date,
                               dep.c_name,
                               dep.date_of_birth,
                               allow.amount_actual,
                               allow.amount_paid,
                               allow.age_checking_date
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DependentAllowance");

                int row = 1;
                ws.Cell(row, 1).Value = OrgName?.option_value ?? "Organization";
                ws.Range(row, 1, row, 3).Merge();
                row++;
                ws.Cell(row, 1).Value = "Fiscal Year : " + fiscalYear;
                ws.Range(row, 1, row, 3).Merge();
                row++;
                ws.Cell(row, 1).Value = "Dependent children allowance reimbursement";
                ws.Range(row, 1, row, 3).Merge();
                row++;

                row++;

                int serial = 1;
                decimal grandTotal = 0;
                decimal employeeTotal = 0;
                int? oldEmpId = null;
                decimal subSerial = 1;
                int counter = 0;

                foreach (var r in records)
                {
                    counter++;
                    double age = 0, serviceAge = 0;
                    if (r.age_checking_date.HasValue)
                    {
                        if (r.age_checking_date.HasValue)
                        {
                            // r.date_of_birth must be non-nullable DateTime
                            TimeSpan diff = (DateTime)r.age_checking_date.Value - (DateTime)r.date_of_birth;
                            age = Math.Round(diff.TotalDays / 365, 2);
                        }
                        if (r.age_checking_date.HasValue)
                        {
                            // r.date_of_birth must be non-nullable DateTime
                            TimeSpan diff = (DateTime)r.age_checking_date.Value - (DateTime)r.join_date;
                            serviceAge = Math.Round(diff.TotalDays / 365, 2);
                        }
                    }

                    // When switching to a new employee, write subtotal
                    if (oldEmpId != null && r.emp_id != oldEmpId)
                    {
                        ws.Cell(row, 1).Value = "Total :";
                        ws.Range(row, 1, row, 5).Merge();
                        ws.Cell(row, 6).Value = employeeTotal;
                        ws.Row(row).Style.Fill.BackgroundColor = XLColor.Yellow;
                        row++;
                        employeeTotal = 0;
                        subSerial = 1;
                    }

                    // Employee header row
                    if (r.emp_id != oldEmpId)
                    {
                        if (counter == 1)
                        {
                            ws.Cell(row, 1).Value = "";
                            ws.Cell(row, 2).Value = "Employee Name";
                            ws.Cell(row, 3).Value = "Join Date";
                            ws.Cell(row, 4).Value = "Number of service year(s)";
                            ws.Cell(row, 5).Value = "Amount";
                            ws.Range(row, 5, row, 6).Merge();
                            ws.Row(row).Style.Font.Bold = true;
                            ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                            row++;

                            // Dependent header row
                            ws.Cell(row, 1).Value = "S.N";
                            ws.Cell(row, 2).Value = "Dependent Name";
                            ws.Cell(row, 3).Value = "Date of Birth";
                            ws.Cell(row, 4).Value = "Age (Year)";
                            ws.Cell(row, 5).Value = "Actual";
                            ws.Cell(row, 6).Value = "Paid";
                            ws.Row(row).Style.Font.Bold = true;
                            ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                            row++;
                        }

                        ws.Cell(row, 1).Value = serial++;
                        ws.Cell(row, 2).Value = r.FullName;
                        ws.Cell(row, 3).Value = r.join_date?.ToString("M/d/yyyy") ?? "";
                        ws.Cell(row, 4).Value = serviceAge;
                        ws.Row(row).Style.Fill.BackgroundColor = XLColor.MayaBlue;
                        row++;
                    }

                    // Dependent row
                    ws.Cell(row, 1).Value = $"{serial - 1}.{subSerial++}";
                    ws.Cell(row, 2).Value = r.c_name;
                    ws.Cell(row, 3).Value = r.date_of_birth?.ToString("M/d/yyyy");
                    ws.Cell(row, 4).Value = age;
                    ws.Cell(row, 5).Value = r.amount_actual;
                    ws.Cell(row, 6).Value = r.amount_paid;
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGreen;

                    employeeTotal += r.amount_paid ?? 0;
                    grandTotal += r.amount_paid ?? 0;
                    oldEmpId = r.emp_id;
                    row++;
                }

                // Final subtotal for last employee
                ws.Cell(row, 1).Value = "Total :";
                ws.Range(row, 1, row, 5).Merge();
                ws.Cell(row, 6).Value = employeeTotal;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.Yellow;
                row++;

                // Grand total
                ws.Cell(row, 1).Value = "Grand Total :";
                ws.Range(row, 1, row, 5).Merge();
                ws.Cell(row, 6).Value = grandTotal;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.Yellow;

                ws.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_dependent_allowance_export_{fiscalYear.Split('/')[1]}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportDependentAllowanceCCD(string fiscalYear, int period)
        {
            // Get Organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee dependent allowance records
            var records = (from e in _context.tbl_employee
                           join d in _context.tbl_dependent_children_details_allowance_emp_wise
                               on e.emp_id equals d.emp_id
                           where d.fiscal_year == fiscalYear && d.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               total_hours = d.total_hours,
                               dependent_a = d.amount_paid
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DependentAllowanceCCD");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + orgName;
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Period: " + period;
                ws.Cell(row++, 1).Value = "Staff Statement of Dependent Allowance with Fund Source Allocation";

                row++;
                // Header
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Fund Source";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Amount";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                foreach (var r in records)
                {
                    // Main employee row
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = "";
                    ws.Cell(row, 5).Value = r.total_hours;
                    ws.Cell(row, 6).Value = r.dependent_a;
                    row++;

                    // Fund-wise allocations
                    var fundWise = _context.tbl_dependent_children_details_allowance_fund_wise
                        .Where(f => f.emp_id == r.emp_id && f.fiscal_year == fiscalYear && f.counter == period)
                        .ToList();

                    foreach (var f in fundWise)
                    {
                        if (f.hours == 0) continue;

                        string fundSource = _context.tbl_fund_source
                            .Where(fs => fs.fund_id == f.fund_id)
                            .Select(fs => fs.fund_source)
                            .FirstOrDefault();

                        // Staff type and GL code
                        string staffType = _context.tbl_employee_salary_extra_settings
                            .Where(s => s.emp_id == r.emp_id)
                            .Select(s => s.staff_type)
                            .FirstOrDefault();

                        string glCode = _payrollServices.GetGLCode(staffType, "C");
                        //string glCode = _context.tbl_settings_gl_codes


                            


                        // Append 00000- conditionally
                        string append0000 = (new DateTime(period, 3, 15) < DateTime.Now) ? "00000-" : "";
                        string glFundSourceCode = $"{glCode}-{fundSource?.Substring(0, Math.Min(21, fundSource.Length))}-{append0000}{r.emp_code}";

                        decimal amount = r.total_hours != 0
                            ? Math.Round(((r.dependent_a ?? 0m) * (decimal)(f.hours ?? 0d)) / (decimal)r.total_hours, 2)
                            : 0m;

                        ws.Cell(row, 4).Value = glFundSourceCode;
                        ws.Cell(row, 5).Value = f.hours;
                        ws.Cell(row, 6).Value = amount;
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());
                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_dependent_allowance_ccd_{fiscalYear.Split('/')[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region 10912 LEAVE ACCRUAL
        [HttpGet]
        public IActionResult LeaveAccrual(string fiscalYearFilter, string? periodInput)
        {
            string PageId = "10912";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            string? FiscalYearActive = HttpContext.Session.GetString("FiscalYear");
            ViewBag.FiscalYearActive = FiscalYearActive;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));

            return PartialView("Payroll/_LeaveAccrual", "");
        }
        public async Task<IActionResult> LeaveAccrualList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string fiscal_year = request.FiscalYearFilter ?? "";
            int? period = 4;

            string[] fiscal_year_break = fiscal_year.Split('/');

            bool hasAccrual = _context.tbl_employee_leave_accrual_new
                .Any(a => a.fiscal_year == fiscal_year && a.counter == period);

            List<dynamic> rawData;
            bool? blnShow = null;

            double? total_annual_leave = _leaveAccrualServices.getYearlyHrsLeave("an");
            decimal? an_hrs_can_carry_forward = Math.Round(_leaveAccrualServices.getYearlyHrsCF(), 2);

            double? total_sick_leave = _leaveAccrualServices.getYearlyHrsLeave("si");
            decimal? si_hrs_can_carry_forward = Math.Round(_leaveAccrualServices.getYearlySickHrsCF(), 2);

            decimal max_cur_an_leave_cf = 144;
            decimal max_cur_si_leave_cf = 96;
            var limits = _context.tbl_setting_limit_hrs.FirstOrDefault();

            int working_hrs_day = limits?.normal_working_hrs ?? 7;
            int working_hrs_pay_period = limits?.working_hours_per_pay_period ?? 5;

            if (!hasAccrual)
            {
                var employees = await (
                    from e in _context.tbl_employee
                    where e.emp_status == "A"
                          && _context.tbl_employee_salary_extra_settings
                               .Any(s => s.emp_id == e.emp_id && s.get_leave_accrual == "Y")
                    select e
                ).ToListAsync();

                string pre_fiscal_year = (int.Parse(fiscal_year_break[0]) - 1) + "/" + (int.Parse(fiscal_year_break[1]) - 1);

                DateTime start_fiscal_date = _context.tbl_fiscal_year
                    .Where(f => f.fiscal_year == fiscal_year)
                    .Select(f => f.date_from)
                    .FirstOrDefault() ?? DateTime.Now;

                DateTime end_fiscal_date = _context.tbl_fiscal_year
                    .Where(f => f.fiscal_year == fiscal_year)
                    .Select(f => f.date_to)
                    .FirstOrDefault() ?? DateTime.Now;

                rawData = employees.Select(e =>
                {
                    int emp_id = e.emp_id;
                    string emp_code = e.emp_code;
                    string full_name = $"{e.firstname} {e.middlename} {e.lastname}";
                    decimal? basic_salary = e.salary;
                    string emp_status = e.emp_status;
                    DateTime? join_date = e.join_date;
                    DateTime? end_date = e.end_date;

                    decimal pre_leave_payable = _leaveAccrualServices.getProvisionedLeaveAmount(emp_id, pre_fiscal_year, period ?? 1);

                    DateTime new_start_fiscal_date = _leaveAccrualServices.getFirstLeavePaidEndDate(emp_id, fiscal_year, start_fiscal_date, 1);

                    // Annual leave
                    decimal a_c = _leaveAccrualServices.getMaxLeaveHrs(1, emp_id, fiscal_year);
                    decimal a_p = _leaveAccrualServices.getMaxLeaveHrs(16, emp_id, fiscal_year);
                    decimal a_t = _leaveAccrualServices.getLeaveTaken(1, emp_id, new_start_fiscal_date, end_fiscal_date);

                    decimal cur_an_leave_laps = (a_t >= a_c) ? 0 :
                        ((a_c - a_t >= max_cur_an_leave_cf) ? (a_c - a_t - max_cur_an_leave_cf) : 0);

                    decimal an_bal = a_p + a_c - a_t - cur_an_leave_laps;
                    decimal? an_eli = (an_bal > an_hrs_can_carry_forward) ? an_hrs_can_carry_forward : an_bal;
                    an_bal = Math.Round(an_bal, 2) / working_hrs_day;
                    an_eli = Math.Round((Decimal)an_eli, 2) / working_hrs_day;

                    // Sick leave
                    decimal s_c = _leaveAccrualServices.getMaxLeaveHrs(5, emp_id, fiscal_year);
                    decimal s_p = _leaveAccrualServices.getMaxLeaveHrs(17, emp_id, fiscal_year);
                    decimal s_t = _leaveAccrualServices.getLeaveTaken(5, emp_id, new_start_fiscal_date, end_fiscal_date);

                    decimal cur_si_leave_laps = (s_t >= s_c) ? 0 :
                        ((s_c - s_t >= max_cur_si_leave_cf) ? (s_c - s_t - max_cur_si_leave_cf) : 0);

                    decimal si_bal = s_p + s_c - s_t - cur_si_leave_laps;
                    decimal? si_eli = (si_bal > si_hrs_can_carry_forward) ? si_hrs_can_carry_forward : si_bal;
                    si_bal = Math.Round(si_bal, 2) / working_hrs_day;
                    si_eli = Math.Round((Decimal)si_eli, 2) / working_hrs_day;

                    // Totals
                    decimal? to_eli = Math.Round((Decimal)an_eli, 2) + Math.Round((Decimal)si_bal, 2);
                    decimal divisor = (decimal)working_hrs_pay_period / (decimal)working_hrs_day;
                    decimal? to_pay = divisor == 0
                        ? 0
                        : Math.Round(((basic_salary ?? 0) * (to_eli ?? 0)) / divisor, 2);
                    decimal? cu_pro = to_pay - pre_leave_payable;

                    return new
                    {
                        emp_id,
                        emp_code,
                        full_name,
                        gender = e.gender,
                        join_date,
                        end_date,
                        basic_salary,
                        emp_status,
                        an_bal,
                        an_eli,
                        si_bal,
                        si_eli,
                        to_eli,
                        to_pay,
                        cu_pro,
                        pre_fiscal_year,
                        pre_leave_payable,
                        total_annual_leave,
                        an_hrs_can_carry_forward,
                        total_sick_leave,
                        si_hrs_can_carry_forward,
                        remarks = "",
                        period = period
                    };
                }).Cast<dynamic>().ToList();

                blnShow = true;
            }
            else
            {
                var accruals = await (
                    from a in _context.tbl_employee_leave_accrual_new
                    join e in _context.tbl_employee on a.emp_id equals e.emp_id
                    where a.fiscal_year == fiscal_year && a.counter == period
                    select new
                    {
                        a.emp_id,
                        e.emp_code,
                        full_name = e.firstname + " " + e.middlename + " " + e.lastname,
                        e.gender,
                        e.join_date,
                        e.end_date,
                        basic_salary = (decimal?)(a.basic_salary ?? 0),
                        e.emp_status,
                        an_bal = (decimal?)(a.an_leave_balance ?? 0),
                        an_eli = (decimal?)(a.an_leave_accrual ?? 0),
                        si_bal = (decimal?)(a.si_leave_balance ?? 0),
                        si_eli = (decimal?)(a.si_leave_accrual ?? 0),

                        // total eligible leave
                        to_eli = (decimal)(a.leave_accrual_days ?? 0),

                        // computed pay (same logic as Add Mode)
                        to_pay = (decimal)(a.leave_payable ?? 0),

                        // current provision = to_pay - pre_leave_payable
                        cu_pro = (decimal)(a.net_provision ?? 0),

                        pre_fiscal_year = a.fiscal_year,
                        pre_leave_payable = (decimal)(a.pre_provisioned ?? 0),
                        total_annual_leave,
                        an_hrs_can_carry_forward,
                        total_sick_leave,
                        si_hrs_can_carry_forward,
                        remarks = a.remarks,
                        period = a.counter
                    }).ToListAsync();


                rawData = accruals.Cast<dynamic>().ToList();
                blnShow = false;
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                rawData = rawData
                    .Where(e => e.full_name.Contains(searchValue) || e.gender.Contains(searchValue))
                    .ToList();
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                rawData = rawData.AsQueryable().OrderBy($"{sortColumn} {sortColumnDir}").ToList();
            }

            int recordsTotal = rawData.Count;
            if (pageSize == -1) pageSize = recordsTotal;
            var cData = rawData.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                totalRecordSub = !string.IsNullOrEmpty(fiscal_year) && period.HasValue
                    ? _context.tbl_employee_leave_accrual_new.Count(h => h.fiscal_year == fiscal_year && h.counter == period)
                    : 0,
                blnShow = blnShow,
                data = cData.Select(x => new
                {
                    x.emp_id,
                    x.full_name,
                    x.emp_code,
                    gender = x.gender == "M" ? "Male" : "Female",
                    join_date = x.join_date,
                    end_date = x.end_date,
                    basic_salary = Math.Round(x.basic_salary ?? 0, 2),
                    emp_status = x.emp_status,
                    an_bal = Math.Round(x.an_bal ?? 0, 2),
                    an_eli = Math.Round(x.an_eli ?? 0, 2),
                    si_bal = Math.Round(x.si_bal ?? 0, 2),
                    si_eli = Math.Round(x.si_eli ?? 0, 2),
                    to_eli = Math.Round(x.to_eli ?? 0, 2),
                    to_pay = Math.Round(x.to_pay ?? 0, 2),
                    cu_pro = Math.Round(x.cu_pro ?? 0, 2),
                    pre_fiscal_year = x.pre_fiscal_year,
                    pre_leave_payable = Math.Round(x.pre_leave_payable ?? 0, 2),
                    total_annual_leave = Math.Round(x.total_annual_leave ?? 0, 2),
                    an_hrs_can_carry_forward = Math.Round(x.an_hrs_can_carry_forward ?? 0, 2),
                    total_sick_leave = Math.Round(x.total_sick_leave ?? 0, 2),
                    si_hrs_can_carry_forward = Math.Round(x.si_hrs_can_carry_forward ?? 0, 2),
                    remarks = x.remarks,
                    period = x.period

                })
            };

            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LeaveAccrualSave([FromBody] EmployeeLeaveAccrualListViewModel model)
        {
            string PageId = "10912";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var update in model.Fields)
            {
                var StartEndDates = _settingsServices.GetFiscalStartEndDate(update.pre_fiscal_year!);
                DateTime start_fiscal_date = StartEndDates.StartDate;
                DateTime end_fiscal_date = StartEndDates.EndDate;

                if (update.emp_id > 0)
                {
                    var nextId = Guid.NewGuid().ToString();

                    var total_hours = await (
                        from f in _context.tbl_employee_fund_source
                        where f.emp_id == update.emp_id
                              && f.start_date >= start_fiscal_date
                              && f.start_date <= end_fiscal_date
                              && (from fs in _context.tbl_fund_source
                                  where fs.fund_status == "A"
                                        && fs.expiry_date > DateTime.Now
                                  select fs.fund_id).Contains(f.fund_id)
                        select f.annual_hrs
                    ).SumAsync() ?? 0;

                    var newRow = new tbl_employee_leave_accrual_new
                    {
                        id = nextId,
                        emp_id = update.emp_id,
                        fiscal_year = update.pre_fiscal_year,
                        basic_salary = update.basic_salary,
                        an_leave_balance = update.an_leave_balance,
                        an_leave_accrual = update.an_leave_accrual,
                        si_leave_balance = update.si_leave_balance,
                        si_leave_accrual = update.si_leave_accrual,
                        leave_accrual_days = update.leave_accrual_days,
                        leave_payable = update.leave_payable,
                        pre_provisioned = update.pre_provisioned,
                        net_provision = update.net_provision,
                        total_hours = total_hours,
                        submit_date = System.DateTime.Now,
                        remarks = update.remarks,
                        counter = update.counter
                    };
                    _context.tbl_employee_leave_accrual_new.Add(newRow); // 👈 inside the if
                    await _context.SaveChangesAsync();

                    SetInsertAccrualFundSource("tbl_employee_leave_accrual_new", nextId, update.emp_id, update.pre_fiscal_year, Convert.ToDateTime(start_fiscal_date), Convert.ToDateTime(end_fiscal_date), "tbl_employee_leave_accrual_new_fund_wise", 4);
                }
            }

            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LeaveAccrualClear(string? fiscalYear, int? period)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_leave_accrual_new WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_leave_accrual_new_fund_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);

            return Json(new
            {
                status = "success",
                message = "clearsuccess",
                fiscal_year = fiscalYear,
                period = period
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportLeaveAccrual(string fiscalYear, int period)
        {
            // Decide which table to use based on fiscal year
            string mainTable = fiscalYear.StartsWith("2018") ? "tbl_employee_leave_accrual_new" : "tbl_employee_leave_accrual";

            // Get organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee leave accrual records
            var records = (from e in _context.tbl_employee
                           join l in _context.tbl_employee_leave_accrual_new
                               on e.emp_id equals l.emp_id
                           where l.fiscal_year == fiscalYear && l.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               e.salary,
                               l.an_leave_balance,
                               l.an_leave_accrual,
                               l.si_leave_balance,
                               l.si_leave_accrual,
                               l.leave_accrual_days,
                               l.leave_payable,
                               l.pre_provisioned,
                               l.net_provision,
                               l.remarks
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("LeaveAccrual");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + orgName;
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Period: " + period;
                ws.Cell(row++, 1).Value = "Staff Statement of Annual Leave";

                row++;
                // Header row
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee Code";
                ws.Cell(row, 4).Value = "Base Salary";
                ws.Cell(row, 5).Value = "Annual Leave Balance";
                ws.Cell(row, 6).Value = "Annual Leave Accrual";
                ws.Cell(row, 7).Value = "Sick Leave Balance";
                ws.Cell(row, 8).Value = "Sick Leave Accrual";
                ws.Cell(row, 9).Value = "Leave Accrual Days";
                ws.Cell(row, 10).Value = "Leave Payable";
                ws.Cell(row, 11).Value = "Pre-Provisioned";
                ws.Cell(row, 12).Value = "Net Provision";
                ws.Cell(row, 13).Value = "Remarks";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                decimal totalSalary = 0, totalPayable = 0, totalPreProvisioned = 0, totalNetProvision = 0;

                foreach (var r in records)
                {
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = r.salary;
                    ws.Cell(row, 5).Value = r.an_leave_balance;
                    ws.Cell(row, 6).Value = r.an_leave_accrual;
                    ws.Cell(row, 7).Value = r.si_leave_balance;
                    ws.Cell(row, 8).Value = r.si_leave_accrual;
                    ws.Cell(row, 9).Value = r.leave_accrual_days;
                    ws.Cell(row, 10).Value = r.leave_payable;
                    ws.Cell(row, 11).Value = r.pre_provisioned;
                    ws.Cell(row, 12).Value = r.net_provision;
                    ws.Cell(row, 13).Value = r.remarks;

                    totalSalary += r.salary ?? 0;
                    totalPayable += r.leave_payable ?? 0;
                    totalPreProvisioned += r.pre_provisioned ?? 0;
                    totalNetProvision += r.net_provision ?? 0;

                    row++;
                }

                // Totals row
                ws.Cell(row, 1).Value = "Total";
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 4).Value = totalSalary;
                ws.Cell(row, 10).Value = totalPayable;
                ws.Cell(row, 11).Value = totalPreProvisioned;
                ws.Cell(row, 12).Value = totalNetProvision;

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_leave_accrual_export_{fiscalYear.Split('/')[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportLeaveAccrualCCD(string fiscalYear, int period)
        {
            // Get Organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee leave accrual records
            var records = (from e in _context.tbl_employee
                           join l in _context.tbl_employee_leave_accrual_new
                               on e.emp_id equals l.emp_id
                           where l.fiscal_year == fiscalYear && l.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               total_hours = l.total_hours,
                               net_provision = l.net_provision
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("LeaveAccrualCCD");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + orgName;
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Period: " + period;
                ws.Cell(row++, 1).Value = "Staff Statement of Annual Leave with Fund Source";

                row++;
                // Header
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Fund Source";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Amount";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                foreach (var r in records)
                {
                    // Main employee row
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = "";
                    ws.Cell(row, 5).Value = r.total_hours;
                    ws.Cell(row, 6).Value = r.net_provision;
                    row++;

                    // Fund-wise allocations
                    var fundWise = _context.tbl_employee_leave_accrual_new_fund_wise
                        .Where(f => f.emp_id == r.emp_id && f.fiscal_year == fiscalYear && f.counter == period)
                        .ToList();

                    foreach (var f in fundWise)
                    {
                        if (f.hours == 0) continue;

                        string fundSource = _context.tbl_fund_source
                            .Where(fs => fs.fund_id == f.fund_id)
                            .Select(fs => fs.fund_source)
                            .FirstOrDefault();

                        // Build GL code (simplified)
                        string glFundSourceCode = $"{fundSource}-{r.emp_code}";

                        decimal amount = r.total_hours != 0
                            ? Math.Round(((r.net_provision ?? 0m) * (decimal)(f.hours ?? 0d)) / (decimal)r.total_hours, 2)
                            : 0m;

                        ws.Cell(row, 4).Value = glFundSourceCode;
                        ws.Cell(row, 5).Value = f.hours;
                        ws.Cell(row, 6).Value = amount;
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());
                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_leave_accrual_ccd_{fiscalYear.Split('/')[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region 10911 GRATUITY ACCRUAL
        [HttpGet]
        public IActionResult GratuityAccrual(string fiscalYearFilter, string? periodInput)
        {
            string PageId = "10911";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            string? FiscalYearActive = HttpContext.Session.GetString("FiscalYear");
            ViewBag.FiscalYearActive = FiscalYearActive;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.DateFrom = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));

            ViewBag.PeriodList = _payrollServices.PeriodFilter();

            return PartialView("Payroll/_GratuityAccrual", "");
        }
        public async Task<IActionResult> GratuityAccrualList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string fiscal_year = request.FiscalYearFilter ?? "";
            string[] fiscal_year_break = fiscal_year.Split('/');

            string periodInput = request.PeriodFilter ?? "";
            int period = 1;

            // Classic ASP logic for period calculation
            if (int.Parse(fiscal_year.Substring(0, 4)) < 2015)
            {
                period = 1;
            }
            else
            {
                if (string.IsNullOrEmpty(periodInput))
                {
                    DateTime start_fiscal_date = _context.tbl_fiscal_year
                        .Where(f => f.fiscal_year == fiscal_year)
                        .Select(f => f.date_from)
                        .FirstOrDefault() ?? DateTime.Now;

                    int cur_date_diff = ((DateTime.Now.Year - start_fiscal_date.Year) * 12 +
                                         DateTime.Now.Month - start_fiscal_date.Month) + 1;

                    if (cur_date_diff <= 3) period = 1;
                    else if (cur_date_diff <= 6) period = 2;
                    else if (cur_date_diff <= 9) period = 3;
                    else period = 4;
                }
                else
                {
                    period = int.Parse(periodInput);
                }
            }

            // Check accrual already processed
            bool hasAccrual = _context.tbl_employee_gratuity_accrual
                .Any(a => a.fiscal_year == fiscal_year && a.counter == period);

            bool? blnShow = null;

            List<dynamic> rawData;

            if (!hasAccrual)
            {
                var employees = await (
                    from e in _context.tbl_employee
                    where e.emp_status == "A"
                          && _context.tbl_employee_salary_extra_settings
                               .Any(s => s.emp_id == e.emp_id && s.get_gratuity_accrual == "Y")
                    orderby e.firstname, e.middlename, e.lastname
                    select e
                ).ToListAsync();

                // Pre fiscal year logic
                string pre_fiscal_year = (int.Parse(fiscal_year_break[0]) - 1) + "/" + (int.Parse(fiscal_year_break[1]) - 1);

                DateTime start_fiscal_date = _context.tbl_fiscal_year
                    .Where(f => f.fiscal_year == fiscal_year)
                    .Select(f => f.date_from)
                    .FirstOrDefault() ?? DateTime.Now;

                DateTime end_fiscal_date = _context.tbl_fiscal_year
                    .Where(f => f.fiscal_year == fiscal_year)
                    .Select(f => f.date_to)
                    .FirstOrDefault() ?? DateTime.Now;

                rawData = employees.Select(e =>
                {
                    int emp_id = e.emp_id;
                    string emp_code = e.emp_code;
                    string full_name = $"{e.firstname} {e.middlename} {e.lastname}";
                    string emp_status = e.emp_status;
                    DateTime? join_date = e.join_date;
                    decimal base_salary = e.salary ?? 0;

                    // Service year calculation
                    DateTime fy_end_fiscal_date = (period == 4)
                        ? end_fiscal_date
                        : new DateTime(start_fiscal_date.Year, start_fiscal_date.Month, 1)
                            .AddMonths((period * 3) - 1)
                            .AddMonths(1)
                            .AddDays(-1);

                    DateTime? gratuity_date = _context.tbl_employee_salary_extra_settings
                        .Where(s => s.emp_id == emp_id && s.get_gratuity_accrual == "Y")
                        .Select(s => s.gratuity_date)
                        .FirstOrDefault();

                    double service_year = ((fy_end_fiscal_date - (gratuity_date ?? DateTime.Now)).TotalDays + 1) / 365.0;
                    service_year = Math.Round(service_year, 2);
                    if (service_year < 0) service_year = 0;

                    // Gratuity encash
                    decimal gratuity_encash = Math.Round(base_salary * (decimal)service_year, 2);

                    // Previous gratuity encash
                    decimal pre_gratuity_encash = _context.tbl_employee_gratuity_accrual
                        .Where(a => a.emp_id == emp_id && a.fiscal_year == pre_fiscal_year && a.counter == period - 1)
                        .OrderByDescending(a => a.counter)
                        .Select(a => a.gratuity_encash ?? 0)
                        .FirstOrDefault();



                    decimal net_gratuity_encash = gratuity_encash - pre_gratuity_encash;

                    return new
                    {
                        emp_id,
                        emp_code,
                        full_name,
                        join_date,
                        base_salary,
                        emp_status,
                        fy_end_fiscal_date,
                        service_year,
                        gratuity_date,
                        gratuity_encash,
                        pre_gratuity_encash,
                        net_gratuity_encash,
                        remarks = ""
                    };
                }).Cast<dynamic>().ToList();

                blnShow = true;
            }
            else
            {
                // Equivalent of ASP "else if blnShow = true"
                var accruals = await (
                    from a in _context.tbl_employee_gratuity_accrual
                    join e in _context.tbl_employee on a.emp_id equals e.emp_id
                    where a.fiscal_year == fiscal_year && a.counter == period
                    orderby e.firstname, e.middlename, e.lastname
                    select new
                    {
                        a.emp_id,
                        e.emp_code,
                        full_name = e.firstname + " " + e.middlename + " " + e.lastname,
                        e.emp_status,
                        join_date = a.join_date,
                        gratuity_date = a.gratuity_date,
                        fy_end_fiscal_date = a.fy_end_date,
                        service_year = a.service_year,
                        base_salary = a.basic_salary,
                        gratuity_encash = a.gratuity_encash,
                        pre_gratuity_encash = a.pre_encash,
                        net_gratuity_encash = a.net_encash,
                        remarks = a.remarks
                    }).ToListAsync();

                rawData = accruals.Cast<dynamic>().ToList();
                blnShow = false;
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                rawData = rawData
                    .Where(e => e.full_name.Contains(searchValue) || e.emp_code.Contains(searchValue))
                    .ToList();
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                rawData = rawData.AsQueryable().OrderBy($"{sortColumn} {sortColumnDir}").ToList();
            }

            int recordsTotal = rawData.Count;
            if (pageSize == -1) pageSize = recordsTotal;
            var cData = rawData.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                totalRecordSub = !string.IsNullOrEmpty(fiscal_year)
                    ? _context.tbl_employee_gratuity_accrual.Count(h => h.fiscal_year == fiscal_year && h.counter == period)
                    : 0,
                blnShow = blnShow,
                data = cData.Select(x => new
                {
                    x.emp_id,
                    x.full_name,
                    x.emp_code,
                    join_date = x.join_date?.ToString("dd/MM/yyyy"),
                    gratuity_date = x.gratuity_date?.ToString("dd/MM/yyyy"),
                    fy_end_fiscal_date = x.fy_end_fiscal_date?.ToString("dd/MM/yyyy"),
                    base_salary = Math.Round(x.base_salary ?? 0, 2),
                    emp_status = x.emp_status,
                    service_year = Math.Round(x.service_year ?? 0, 2),
                    gratuity_encash = Math.Round(x.gratuity_encash ?? 0, 2),
                    pre_gratuity_encash = Math.Round(x.pre_gratuity_encash ?? 0, 2),
                    net_gratuity_encash = Math.Round(x.net_gratuity_encash ?? 0, 2),
                    remarks = x.remarks
                })
            };

            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GratuityAccrualSave([FromBody] EmployeeGratuityAccrualListViewModel model)
        {
            string PageId = "10912";

            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion

            if (perm.apern != "true" && perm.epern != "true") return Json(new { status = "invalid", message = "Not Authorized User" });
            if (!ModelState.IsValid) return Json(new { status = "error", message = Lang.msg_error_invalid });
            if (model?.Fields == null || !model.Fields.Any()) return Json(new { status = "error", message = "No employees received." });

            foreach (var update in model.Fields)
            {
                if (update.emp_id > 0)
                {
                    // Classic ASP variables
                    string id = Guid.NewGuid().ToString() + update.period;   // UniqueID()&j
                    int? empid = update.emp_id;
                    DateTime? join_date = update.join_date;
                    DateTime? gratuity_date = update.gratuity_date;
                    DateTime? fy_end_fiscal_date = update.fy_end_fiscal_date;
                    double? service_year = update.service_year ?? 0;
                    decimal? base_salary = update.base_salary ?? 0;
                    decimal? gratuity_encash = update.gratuity_encash ?? 0;
                    decimal? pre_gratuity_encash = update.pre_gratuity_encash ?? 0;
                    decimal? net_gratuity_encash = update.net_gratuity_encash ?? 0;
                    string remarks = update.remarks ?? "";
                    string fiscal_year = update.fiscal_year!;
                    short? period = update.period;

                    // Get fiscal start/end dates
                    var StartEndDates = _settingsServices.GetFiscalStartEndDate(fiscal_year);
                    DateTime start_fiscal_date = StartEndDates.StartDate;
                    DateTime end_fiscal_date = StartEndDates.EndDate;

                    // total_hours calculation (same as Classic ASP SQL)
                    var total_hours = await (
                        from f in _context.tbl_employee_fund_source
                        where f.emp_id == empid
                              && f.start_date >= start_fiscal_date
                              && f.start_date <= end_fiscal_date
                              && (from fs in _context.tbl_fund_source
                                  where fs.fund_status == "A"
                                        && fs.expiry_date > DateTime.Now
                                  select fs.fund_id).Contains(f.fund_id)
                        select f.annual_hrs
                    ).SumAsync() ?? 0;

                    // Insert into tbl_employee_gratuity_accrual
                    var newRow = new tbl_employee_gratuity_accrual
                    {
                        id = id,
                        emp_id = empid,
                        fiscal_year = fiscal_year,
                        join_date = join_date,
                        gratuity_date = gratuity_date,
                        fy_end_date = fy_end_fiscal_date,
                        service_year = service_year,
                        basic_salary = base_salary,
                        gratuity_encash = gratuity_encash,
                        pre_encash = pre_gratuity_encash,
                        net_encash = net_gratuity_encash,
                        total_hours = total_hours,
                        submit_date = DateTime.Now,
                        remarks = remarks,
                        counter = period
                    };

                    _context.tbl_employee_gratuity_accrual.Add(newRow);
                    await _context.SaveChangesAsync();

                    // Fund source breakdown insert
                    SetInsertAccrualFundSource("tbl_employee_gratuity_accrual",id,Convert.ToInt32(empid),fiscal_year,start_fiscal_date,end_fiscal_date,"tbl_employee_gratuity_accrual_fund_wise",period);
                }
            }

            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GratuityAccrualClear(string? fiscalYear, int? period)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_gratuity_accrual WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_gratuity_accrual_fund_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);

            return Json(new
            {
                status = "success",
                message = "clearsuccess",
                fiscal_year = fiscalYear,
                period = period
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportGratuityAccrual(string fiscalYear, int period)
        {
            // Get organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee gratuity accrual records
            var records = (from e in _context.tbl_employee
                           join g in _context.tbl_employee_gratuity_accrual
                               on e.emp_id equals g.emp_id
                           where g.fiscal_year == fiscalYear && g.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               g.join_date,
                               g.gratuity_date,
                               g.fy_end_date,
                               g.service_year,
                               g.basic_salary,
                               g.gratuity_encash,
                               g.pre_encash,
                               g.net_encash,
                               g.total_hours,
                               g.remarks
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("GratuityAccrual");

                int row = 1;
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Organization: " + orgName;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Period: " + period;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Staff Statement of Gratuity Accrual";

                row++;
                // Header row
                ws.Cell(row, 1).Value = "SN";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Date of Employment";
                ws.Cell(row, 5).Value = "Date effective for gratuity";
                ws.Cell(row, 6).Value = "Date to";
                ws.Cell(row, 7).Value = "Number of service year(s)";
                ws.Cell(row, 8).Value = "Base Salary";
                ws.Cell(row, 9).Value = "Total gratuity entitled for encashment";
                ws.Cell(row, 10).Value = "Pre-Gratuity Encash";
                ws.Cell(row, 11).Value = "Net Gratuity Encash";
                ws.Cell(row, 12).Value = "Remarks";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                decimal totalSalary = 0, totalEncash = 0, totalPreEncash = 0, totalNetEncash = 0;

                foreach (var r in records)
                {
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = r.join_date?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 5).Value = r.gratuity_date?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 6).Value = r.fy_end_date?.ToString("dd/MM/yyyy");
                    ws.Cell(row, 7).Value = r.service_year;
                    ws.Cell(row, 8).Value = r.basic_salary;
                    ws.Cell(row, 9).Value = r.gratuity_encash;
                    ws.Cell(row, 10).Value = r.pre_encash;
                    ws.Cell(row, 11).Value = r.net_encash;
                    ws.Cell(row, 12).Value = r.remarks;

                    totalSalary += r.basic_salary ?? 0;
                    totalEncash += r.gratuity_encash ?? 0;
                    totalPreEncash += r.pre_encash ?? 0;
                    totalNetEncash += r.net_encash ?? 0;

                    row++;
                }

                // Totals row
                ws.Cell(row, 1).Value = "Total";
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 8).Value = totalSalary;
                ws.Cell(row, 9).Value = totalEncash;
                ws.Cell(row, 10).Value = totalPreEncash;
                ws.Cell(row, 11).Value = totalNetEncash;

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_gratuity_accrual_export_{fiscalYear.Split('/')[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportGratuityAccrualCCD(string fiscalYear, int period)
        {
            // Get Organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee gratuity accrual records
            var records = (from e in _context.tbl_employee
                           join g in _context.tbl_employee_gratuity_accrual
                               on e.emp_id equals g.emp_id
                           where g.fiscal_year == fiscalYear && g.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               g.total_hours,
                               g.net_encash
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("GratuityAccrualCCD");

                int row = 1;
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Organization: " + orgName;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Period: " + period;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Staff Statement of Gratuity with Fund Source Allocated";

                row++;
                // Header
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Fund Source Code";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Amount";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                foreach (var r in records)
                {
                    // Main employee row
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 5).Value = r.total_hours;
                    ws.Cell(row, 6).Value = r.net_encash;
                    row++;

                    // Fund-wise allocations
                    var fundWise = _context.tbl_employee_gratuity_accrual_fund_wise
                        .Where(f => f.emp_id == r.emp_id && f.fiscal_year == fiscalYear && f.counter == period)
                        .ToList();

                    foreach (var f in fundWise)
                    {
                        if (f.hours == 0) continue;

                        string fundSource = _context.tbl_fund_source
                            .Where(fs => fs.fund_id == f.fund_id)
                            .Select(fs => fs.fund_source)
                            .FirstOrDefault() ?? "";

                        // Build GL code (same as Classic ASP)
                        string append0000 = int.Parse(fiscalYear.Substring(fiscalYear.Length - 4)) > 2015 ? "0000-" : "";
                        string glFundSourceCode = $"{fundSource.Substring(0, Math.Min(17, fundSource.Length))}-{append0000}{r.emp_code}";

                        decimal amount = r.total_hours != 0
                            ? Math.Round(((r.net_encash ?? 0m) * (decimal)(f.hours ?? 0d)) / (decimal)r.total_hours, 2)
                            : 0m;

                        ws.Cell(row, 4).Value = glFundSourceCode;
                        ws.Cell(row, 5).Value = f.hours;
                        ws.Cell(row, 6).Value = amount;
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());
                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_gratuity_accrual_ccd_{fiscalYear.Split('/')[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region 10907 EMPLOYEE SALARY BULK
        [HttpGet]
        public IActionResult SalaryBulk()
        {
            string PageId = "10907";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.YearDropDown = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.MonthDropDown = _settingsServices.GetMonths(DateTime.Now.Month);

            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";

            //END OF MONTH AND YEAR
            var taxPercent = _context.tbl_tax_setting
                .Select(f => new
                {
                    InitialTaxPercent = f.initial_tax_percent ?? 0m,
                    FirstTaxPercent = f.first_tax_percent ?? 0d,
                    SecondTaxPercent = f.second_tax_percent ?? 0d,
                    ThirdTaxPercent = f.third_tax_percent ?? 0d,
                    FourthTaxPercent = f.fourth_tax_percent ?? 0d,
                    FifthTaxPercent = f.fifth_tax_percent ?? 0d,

                    SingleAmt = f.single_amt ?? 0m,
                    MarriedAmt = f.married_amt ?? 0m,
                    FirstTaxAmount = f.first_tax_amount ?? 0d,
                    SecondTaxAmount = f.second_tax_amount ?? 0m,
                    ThirdTaxAmountSingle = f.third_tax_amount_single ?? 0m,
                    ThirdTaxAmountMarried = f.third_tax_amount_married ?? 0m,
                    MaxMedicalExpensesReimbursed = f.max_medical_expenses_reimbursed ?? 0d,
                    SingleFemaleDedPer = f.single_female_ded_per ?? 0d,
                    MaxMedicalTaxCreditPer = f.max_medical_tax_credit_per ?? 0d,
                    FourthTaxAmount = f.fourth_tax_amount ?? 0m,

                    MaxMedicalTaxCreditAmount = f.max_medical_tax_credit_amount ?? 0d,
                    InsAmt = f.ins_amt ?? 0m,
                    InsAmtNonLife = f.ins_amt_non_life ?? 0m
                }).FirstOrDefault();
                ViewBag.InitialTaxPercent = taxPercent.InitialTaxPercent;
                ViewBag.FirstTaxPercent = taxPercent.FirstTaxPercent;
                ViewBag.SecondTaxPercent = taxPercent.SecondTaxPercent;
                ViewBag.ThirdTaxPercent = taxPercent.ThirdTaxPercent;
                ViewBag.FourthTaxPercent = taxPercent.FourthTaxPercent;
                ViewBag.FifthTaxPercent = taxPercent.FifthTaxPercent;

                ViewBag.SingleAmt = taxPercent.SingleAmt;
                ViewBag.MarriedAmt = taxPercent.MarriedAmt;
                ViewBag.FirstTaxAmount = taxPercent.FirstTaxAmount;
                ViewBag.SecondTaxAmount = taxPercent.SecondTaxAmount;
                ViewBag.ThirdTaxAmountSingle = taxPercent.ThirdTaxAmountSingle;
                ViewBag.ThirdTaxAmountMarried = taxPercent.ThirdTaxAmountMarried;
                ViewBag.MaxMedicalExpensesReimbursed = taxPercent.MaxMedicalExpensesReimbursed;
                ViewBag.SingleFemaleDedPer = taxPercent.SingleFemaleDedPer;
                ViewBag.MaxMedicalTaxCreditPer = taxPercent.MaxMedicalTaxCreditPer;
                ViewBag.FourthTaxAmount = taxPercent.FourthTaxAmount;
                ViewBag.FifthTaxPercent = taxPercent.FifthTaxPercent;

                ViewBag.MaxMedicalTaxCreditAmount = taxPercent.MaxMedicalTaxCreditAmount;
                ViewBag.InsAmt = taxPercent.InsAmt;
                ViewBag.InsAmtNonLife = taxPercent.InsAmtNonLife;

            var Records = (
                from con in _context.tbl_employee_salary
                select con
            ).ToList();


            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/SalaryBulk", "", PageId, Records.Count);

            return PartialView("Payroll/_SalaryBulk", "");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalaryBulkList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string? StatusFilter = request.Status;
            var yearFilter = request.Year;
            var monthFilter = request.Month;
            StatusFilter = StatusFilter == "A" ? "Active" : "Inactive";

            int salYear = yearFilter ?? DateTime.Now.Year;
            int salMonth = monthFilter ?? DateTime.Now.Month;
            if (salYear < 1 || salYear > 9999)
            {
                throw new ArgumentException("Invalid year value");
            }
            if (salMonth < 1 || salMonth > 12)
            {
                throw new ArgumentException("Invalid month value");
            }

            DateTime fiscalFrom = new DateTime(salYear, 1, 1);
            DateTime curFiscalMonth = new DateTime(salYear, salMonth, 1);
            DateTime fiscal_to = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));
            DateTime fiscal_from = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));

            var data = _context.vw_Employee.AsQueryable();
            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "All")
            {
                data = data.Where(d => d.emp_status == StatusFilter);
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                data = data.OrderBy(sortColumn + " " + sortColumnDir);
            }

            DateTime? fn_date_upto_sep = null;
            if (monthFilter >= 7 && yearFilter <= 12)
            {
                string sel_fiscal_year = yearFilter + "/" + (yearFilter + 1);
                if (sel_fiscal_year != "") 
                { 
                    DateTime? dateFromStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(sel_fiscal_year, "date_from"));
                    DateTime? dateToStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(sel_fiscal_year, "date_to"));
                }
                fn_date_upto_sep = new DateTime(salYear, 9, 30);
            }
            else if (monthFilter >= 1 && yearFilter <= 6)
            {
                string sel_fiscal_year = (yearFilter - 1) + "/" + yearFilter;
                if (sel_fiscal_year != "")
                {
                    DateTime? dateFromStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(sel_fiscal_year, "date_from"));
                    DateTime? dateToStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(sel_fiscal_year, "date_to"));
                }
                fn_date_upto_sep = new DateTime(salYear - 1, 9, 30);
            }

            // Materialize first
            var employeesBase = _context.vw_Employee
                .Where(e => e.emp_status == StatusFilter)
                .Join(_context.vw_employee_salary_extra_settings,
                      e => e.emp_id,
                      es => es.emp_id,
                      (e, es) => new { e, es })
                .Where(joined => joined.es.is_field_salary == "N")
                .ToList();

            // Now run imperative logic in memory
            var employees = employeesBase.Select(joined =>
            {
                var e = joined.e;

                // Basic salary info
                var empInfo = _context.tbl_employee
                    .Where(emp => emp.emp_id == e.emp_id)
                    .Select(emp => new
                    {
                        BasicSalary = (decimal?)emp.salary ?? 0m,
                        ChildEduAll = (decimal?)emp.child_edu_all ?? 0m,
                        RemoteAreaAll = (decimal?)emp.remote_area_allow ?? 0m,
                        YearlyRemoteExem = (decimal?)emp.yearly_remote_exem ?? 0m,
                        WorkPercent = (int?)emp.work_percent ?? 100
                    })
                    .FirstOrDefault();

                var basicSalary = empInfo?.BasicSalary ?? 0m;
                var childEduAll = empInfo?.ChildEduAll ?? 0m;
                var remoteAreaAll = empInfo?.RemoteAreaAll ?? 0m;
                var yearlyRemoteExem = empInfo?.YearlyRemoteExem ?? 0m;
                var workPercent = empInfo?.WorkPercent ?? 100;
                var isDashainforce = (workPercent < 100 && salMonth == 9) ? "Y" : "N";


                var fiscal_to_1 = HttpContext.Session.GetString("date_to");
                DateTime e_date = Convert.ToDateTime(e.end_date);
                if (e.end_date >= Convert.ToDateTime(fiscal_to_1))
                    e_date = Convert.ToDateTime(fiscal_to_1);
                // start date is always first day of the month/year
                DateTime s_date = new DateTime(salYear, salMonth, 1);
                int month_diff = ((e_date.Year - s_date.Year) * 12) + e_date.Month - s_date.Month + 1;


                var query = _context.tbl_employee_salary
                    .Where(x => x.emp_id == e.emp_id
                    && x.sal_year == salYear
                    && x.sal_month == salMonth);
                var s = query.FirstOrDefault();
                var count = query.Count();
                var isDashainAlready = count > 1 ? "Y" : "N";

                var d = _context.tbl_employee_salary_diff
                    .FirstOrDefault(x => x.emp_id == e.emp_id && x.emp_year == salYear && x.emp_month == salMonth);

                var empPF = _context.tbl_employee_pf.FirstOrDefault(x => x.emp_id == e.emp_id);
                var empCIT = _context.tbl_employee_cit.FirstOrDefault(x => x.emp_id == e.emp_id);

                var reImburseMedical = _context.tbl_employee_medical_reimburse
                    .Where(q => q.emp_id == e.emp_id && q.sal_month >= salMonth && q.sal_year <= salYear && q.reim_type == "Medical")
                    .GroupBy(q => q.emp_id)
                    .Select(r => new
                    {
                        InsuranceReImburse = (r.Sum(x => (decimal?)x.self_amt) ?? 0m) + (r.Sum(x => (decimal?)x.spouse_amt) ?? 0m),
                        MedicalExpenseReimburseTotal = (r.Sum(x => (decimal?)x.self_amt) ?? 0m) + (r.Sum(x => (decimal?)x.spouse_amt) ?? 0m) + (r.Sum(x => (decimal?)x.other_dep_amt) ?? 0m),
                        InsuranceSingle = (r.Sum(x => (decimal?)x.self_amt) ?? 0m)
                    })
                    .FirstOrDefault();
                var reImburseLife = _context.tbl_employee_medical_reimburse
                    .Where(q => q.emp_id == e.emp_id && q.sal_month >= salMonth && q.sal_year <= salYear && q.reim_type == "Life Insurance")
                    .GroupBy(q => q.emp_id)
                    .Select(r => new
                    {
                        InsuranceReImburse = (r.Sum(x => (decimal?)x.self_amt) ?? 0m) + (r.Sum(x => (decimal?)x.spouse_amt) ?? 0m),
                        MedicalExpenseReimburseTotal = (r.Sum(x => (decimal?)x.self_amt) ?? 0m) + (r.Sum(x => (decimal?)x.spouse_amt) ?? 0m) + (r.Sum(x => (decimal?)x.other_dep_amt) ?? 0m)
                    })
                    .FirstOrDefault();

                var empOvertime = _context.tbl_employee_overtime
                    .Where(q => q.emp_id == e.emp_id && q.sal_month >= salMonth && q.sal_year <= salYear)
                    .GroupBy(q => q.emp_id)
                    .Select(r => new
                    {
                        Overtime = (r.Sum(x => (decimal?)x.rate * (decimal?)x.hrs) ?? 0m) + (r.Sum(x => (decimal?)x.ot_diff) ?? 0m)
                    })
                    .FirstOrDefault();

                var prev = _context.vw_employee_salary_previous
                    .Where(q => q.emp_id == e.emp_id && q.fiscal >= fiscal_from && q.fiscal <= fiscal_to)
                    .GroupBy(q => q.emp_id)
                    .Select(g => new
                    {
                        BasicSalarySumTaken = g.Sum(x => x.t_basic_salary) ?? 0,
                        PfATaken = g.Sum(x => x.t_pf) ?? 0,
                        OthersTaken = g.Sum(x => x.t_allow) ?? 0,
                        InsuranceTaken = g.Sum(x => x.t_lip_rem) ?? 0,
                        RemoteAreaAllTaken = g.Sum(x => x.t_raa) ?? 0,
                        DashainAmountTaken = g.Sum(x => x.t_dashain) ?? 0,
                        PfDDud = g.Sum(x => x.t_pf_d) ?? 0,
                        CitDud = g.Sum(x => x.t_cit_d) ?? 0,
                        BetalabiDDud = g.Sum(x => x.t_betalabi) ?? 0,
                        PreAccessTaxTaken = g.Sum(x => x.t_tax_pre) ?? 0,
                        TaxDud = g.Sum(x => x.t_tax) ?? 0
                    })
                    .FirstOrDefault();

                var fiscal_date = new DateTime(salYear, salMonth, 1);
                var thisOfficeSalary = _context.vw_year_salary
                   .Where(q => q.emp_id == e.emp_id
                            && q.fiscal >= fiscal_from
                            //&& q.fiscal <= fiscal_to
                            && q.fiscal < fiscal_date)
                   .AsEnumerable() // move this BEFORE GroupBy
                   .GroupBy(q => q.emp_id)
                   .Select(g => new
                   {
                       count_month = g.Count(),
                       basic_salary_sum_taken = g.Sum(x => x.basic_salary ?? 0m) + (prev?.BasicSalarySumTaken ?? 0m),
                       remote_area_all_taken = g.Sum(x => x.remote_area_all ?? 0m) + (prev?.RemoteAreaAllTaken ?? 0m),
                       pf_a_taken = g.Sum(x => x.pf_a ?? 0m) + (prev?.PfATaken ?? 0m),
                       betalibi_d_dud = g.Sum(x => x.betalibi_d ?? 0m) + (prev?.BetalabiDDud ?? 0m),
                       pf_d_dud = g.Sum(x => x.pf_d ?? 0m) + (prev?.PfDDud ?? 0m),
                       cit_d = g.Sum(x => x.cit_d ?? 0m),
                       pre_access_tax = g.Sum(x => x.pre_access_tax ?? 0m),
                       incometax_d = g.Sum(x => x.incometax_d ?? 0m),
                       dashain_a = g.Sum(x => x.dashain_a ?? 0m),
                       performance_all = g.Sum(x => x.performance_all ?? 0m),
                       overtime = g.Sum(x => x.overtime ?? 0m),
                       others = g.Sum(x => x.others ?? 0m),
                       gratudi = g.Sum(x => x.gratudi ?? 0m),
                       children_edu_all = g.Sum(x => x.children_edu_all ?? 0m),
                       insurance_taken = g.Sum(x => x.insurance ?? 0m),
                       gratuity = g.Sum(x => x.gratuity ?? 0m),
                       gratuity_ded = g.Sum(x => x.gratuity_ded ?? 0m),
                       medical_expense_reimburse_eligible = g.Sum(x => x.medical_expense_reimburse_eligible ?? 0m),
                       medical_expense_reimburse_total = g.Sum(x => x.medical_expense_reimburse_total ?? 0m),
                       leave_encash = g.Sum(x => x.leave_encash ?? 0m),
                       medical_deduction_on_tax = g.Sum(x => x.medical_deduction_on_tax ?? 0m),
                       ssf = g.Sum(x => x.ssf ?? 0m),
                       ssf_ded = g.Sum(x => x.ssf_ded ?? 0m)
                   })
                   .FirstOrDefault();


                var citType = s?.cit_type ?? empCIT?.cit_type ?? "";
                var citTypeDesc = citType switch
                {
                    "B" => "Percent in Basic Salary",
                    "T" => "Max Amount",
                    "F" => "Fixed Amount",
                    _ => ""
                };
                var citTypeCombined = string.IsNullOrEmpty(citTypeDesc)
                    ? citType
                    : $"{citType} - {citTypeDesc}";

                var gratuityInfo = _context.tbl_employee_gratuity_info.FirstOrDefault(x => x.emp_id == e.emp_id);
                var ssfSetting = _context.tbl_employee_ssf_info.FirstOrDefault(x => x.emp_id == e.emp_id);

                //FOR TAX SETTINGS
                var taxSetting = _context.tbl_tax_setting
                    .FirstOrDefault();


                int gender_ded = 0;
                if (e.gender == "F")
                    gender_ded = (int)taxSetting.single_female_ded_per;


                //FOR INSURANCE
                var insuranceDed = _context.tbl_employee_insurance
                    .Where(x => x.emp_id == e.emp_id && x.ins_type == "Life" && x.ins_valid_date >= Convert.ToDateTime(fiscal_date))
                    .GroupBy(q => q.emp_id)
                    .Select(r => new
                    {
                        insuranceDed = (r.Sum(x => (decimal?)x.premium_amount) ?? 0m)
                    })
                    .FirstOrDefault();

                //FOR INSURANCE INSNL
                var insuranceInsNL = _context.tbl_employee_medical_reimburse
                    .Where(q => q.emp_id == e.emp_id && q.sal_month >= salMonth && q.sal_year <= salYear && q.reim_type == "Non Life Insurance" && q.app_status == "Approved")
                    .GroupBy(q => q.emp_id)
                    .Select(r => new
                    {
                        insuranceInsNL = (r.Sum(x => (decimal?)x.self_amt) ?? 0m) + (r.Sum(x => (decimal?)x.spouse_amt) ?? 0m)
                    })
                    .FirstOrDefault();

                var r_max_med_life_clam = ((e.marital_status == "M" ? reImburseMedical?.InsuranceReImburse ?? 0 : e.marital_status == "S" ? reImburseMedical?.InsuranceSingle ?? 0 : 0) + thisOfficeSalary?.medical_expense_reimburse_total ?? 0) + (thisOfficeSalary?.insurance_taken ?? 0 + reImburseLife?.InsuranceReImburse ?? 0);
                var total_cur_medical_eme = (((decimal?)r_max_med_life_clam * (decimal?)(taxSetting?.max_medical_tax_credit_per ?? 0)) / 100) - thisOfficeSalary?.medical_deduction_on_tax ?? 0;

                // Advances
                var adv = _context.tbl_employee_advance
                    .FirstOrDefault(a => a.emp_id == e.emp_id && a.adv_year == salYear && a.adv_month == salMonth);

                // Dashain Percent Amount Tax
                var dashainPerAmt = _context.tbl_employee_salary_tax_percent
                    .FirstOrDefault(a => a.emp_id == e.emp_id);

                int fn_count_days_dashain = 0;
                if (e.join_date.HasValue && e.join_date.Value < fn_date_upto_sep)
                {
                    TimeSpan diff = fn_date_upto_sep.Value - e.join_date.Value;
                    int dateDiff = diff.Days + 1;

                    fn_count_days_dashain = dateDiff >= 365 ? 0 : dateDiff;
                }
                else
                {
                    fn_count_days_dashain = -1;
                }

                return new
                {
                    EmplopyeeInfo_EmpId = e.emp_id,
                    emp_id = e.emp_id,
                    EmplopyeeInfo_EmpCode = e.emp_code,
                    EmplopyeeInfo_EmployeeName = e.employeenameWithCode,
                    EmplopyeeInfo_EmployeeFullName = e.employeename,
                    EmplopyeeInfo_IsFieldSalary = joined.es.is_field_salary ?? "N",
                    EmplopyeeInfo_EmpStatus = e.emp_status,
                    EmplopyeeInfo_MaritalStatus = e.marital_status,
                    EmplopyeeInfo_Gender = e.gender,
                    EmplopyeeInfo_StartDate = e.join_date,
                    EmplopyeeInfo_EndDate = e.end_date,

                    // CURRENT
                    Current_BasicSalary = _payrollServices.GetFormatValue((s?.basic_salary) ?? basicSalary + (d?.basic_salary ?? 0)),
                    Current_pfA = _payrollServices.GetFormatValue((s != null && s.pf_a > 0) ? (s.pf_a ?? 0) : (empPF?.pf_group == "A" ? (decimal)(empPF?.add_percent_amount ?? 0) : 0)),
                    Current_PerformanceAll = _payrollServices.GetFormatValue((s?.performance_all ?? 0)),
                    Current_Insurance = _payrollServices.GetFormatValue((s?.insurance ?? reImburseLife?.InsuranceReImburse ?? 0)),
                    Current_Others = _payrollServices.GetFormatValue((s?.others ?? 0)),
                    Current_ChildrenEduAll = _payrollServices.GetFormatValue(((s?.children_edu_all) ?? childEduAll)),
                    //Current_Gratuity_a = _generalServices.GetFormatValue((d?.gratuity_a) ?? 0),
                    Current_Gratuity = _payrollServices.GetFormatValue(((decimal?)s?.gratuity ?? (decimal?)gratuityInfo?.add_percent_amount ?? 0m) + (d?.gratuity_a ?? 0m)),
                    Current_Gratudi = _payrollServices.GetFormatValue(((s?.gratudi) ?? 0)),
                    Current_Ssf = _payrollServices.GetFormatValue((s?.ssf ?? d?.ssf_a ?? 0)),
                    Current_RemoteAreaAll = _payrollServices.GetFormatValue(((s?.remote_area_all) ?? remoteAreaAll)),
                    Current_YearlyRemoteExem = _payrollServices.GetFormatValue(yearlyRemoteExem),
                    Current_Overtime = _payrollServices.GetFormatValue(((s?.overtime) ?? (empOvertime?.Overtime) ?? 0)),
                    Current_MedicalExpenseReimburseTotal = _payrollServices.GetFormatValue((s?.medical_expense_reimburse_total ?? (reImburseMedical?.MedicalExpenseReimburseTotal ?? 0))),
                    Current_MedicalExpenseReimburseEligible = _payrollServices.GetFormatValue((s?.medical_expense_reimburse_eligible) ?? ((reImburseMedical?.InsuranceReImburse ?? 0) + (thisOfficeSalary?.medical_expense_reimburse_total ?? 0)) + ((thisOfficeSalary?.insurance_taken ?? 0) + (s?.insurance ?? reImburseLife?.InsuranceReImburse ?? 0))),
                    Current_LeaveEncash = _payrollServices.GetFormatValue((s?.leave_encash ?? 0)),

                    //Dashain Bonus
                    DashainBonus_Bonus = s?.is_dashain ?? "N",
                    DashainBonus_Amount = _payrollServices.GetFormatValue((s?.dashain_a ?? 0)),

                    // Deduction for tax calculation
                    DedTaxCalculation_BasicD = _payrollServices.GetFormatValue((e.marital_status == "M" ? taxSetting?.married_amt ?? 0 : e.marital_status == "S" ? taxSetting?.single_amt ?? 0 : 0)),
                    DedTaxCalculation_PfD = _payrollServices.GetFormatValue((s != null && s.pf_d > 0) ? (s.pf_d ?? 0) : ((empPF?.pf_group == "A" || empPF?.pf_group == "B") ? (decimal)(empPF?.ded_percent_amount ?? 0) + (d?.pf_d ?? 0) : 0)),
                    DedTaxCalculation_CitD = _payrollServices.GetFormatValue((s != null && s.a_cit_d > 0) ? (s.a_cit_d ?? 0) : (empCIT?.cit_type == "B" ? Math.Round(basicSalary * (decimal)(empCIT?.percent_amount ?? 0) / 100, 0) : (empCIT?.cit_type == "F" ? (decimal)(empCIT?.percent_amount ?? 0) : 0))),
                    DedTaxCalculation_GratuityDed = _payrollServices.GetFormatValue((s?.gratuity_ded ?? 0) + (d?.gratuity_d ?? 0)),
                    DedTaxCalculation_SsfDed = _payrollServices.GetFormatValue((s?.ssf_ded ?? 0) + (d?.ssf_d ?? 0)),
                    DedTaxCalculation_InsuranceD = _payrollServices.GetFormatValue((s?.insurance_d ?? insuranceDed?.insuranceDed ?? 0)),
                    DedTaxCalculation_InsuranceDNL = _payrollServices.GetFormatValue(s?.insurance_d_nl ?? insuranceInsNL?.insuranceInsNL ?? 0),
                    DedTaxCalculation_BetalabiD = _payrollServices.GetFormatValue(s?.betalibi_d ?? 0),
                    DedTaxCalculation_MedicalDeductionOnTax = _payrollServices.GetFormatValue(s?.medical_deduction_on_tax ?? (decimal?)total_cur_medical_eme ?? 0),
                    DedTaxCalculation_PreAccessTax = _payrollServices.GetFormatValue(s?.pre_access_tax ?? 0),

                    // Gross
                    Gross_PrevYearExcessTax = _payrollServices.GetFormatValue(s?.pre_access_tax ?? 0),
                    Gross_YearlyTaxableSalary = _payrollServices.GetFormatValue(s?.yearly_salary ?? 0),
                    Gross_YearlyTax = _payrollServices.GetFormatValue(s?.yearly_tax ?? 0),
                    Gross_MonthlySalary = _payrollServices.GetFormatValue(s?.monthly_salary ?? 0),
                    Gross_MonthTax = _payrollServices.GetFormatValue(s?.incometax_d ?? 0),
                    //Gross_MonthAmount = (s?.month_amount ?? 0) - (s?.incometax_d ?? 0),
                    Gross_MonthAmount = _payrollServices.GetFormatValue(s?.month_amount ?? 0),
                    Gross_MonthDiff = month_diff,

                    // Advances
                    Advances_Personnel = _payrollServices.GetFormatValue(s?.tel_per_adv ?? adv?.adv_personnel ?? 0),
                    Advances_Program = _payrollServices.GetFormatValue(s?.pr_adv ?? adv?.adv_program ?? 0),
                    Advances_Travel = _payrollServices.GetFormatValue(s?.travel_prog_adv ?? adv?.adv_travel ?? 0),
                    Advances_FieldDrawing = _payrollServices.GetFormatValue(s?.fd_adv ?? adv?.adv_field_drawing ?? 0),
                    Advances_Welfare = _payrollServices.GetFormatValue(s?.wl_adv ?? adv?.adv_welfare ?? 0),
                    Advances_PfLoan = _payrollServices.GetFormatValue(s?.adv_PF_loan ?? adv?.adv_PF_loan ?? 0),
                    Advances_CitLoan = _payrollServices.GetFormatValue(s?.adv_CIT_loan ?? adv?.adv_CIT_loan ?? 0),

                    // Welfare
                    Welfare_Percentage = _payrollServices.GetFormatValue((decimal?)(s?.wl_per) ?? (decimal?)(joined.es.welfare_con_percent) ?? 0m),
                    Welfare_Amount = _payrollServices.GetFormatValue(s?.welfare_fund ?? (((decimal?)(s?.basic_salary) ?? (decimal?)basicSalary + (d?.basic_salary ?? 0) * (s?.wl_per ?? (decimal?)(joined.es.welfare_con_percent) ?? 0m)) / 100) ?? 0),

                    // Net
                    NetInHand_Amount = _payrollServices.GetFormatValue(s?.net_in_hand ?? 0),

                    // Others
                    Others_Remarks = s?.remarks ?? "",

                    // ACTUAL
                    Actual_BasicSalary = _payrollServices.GetFormatValue(s?.act_basic_salary ?? (decimal?)basicSalary ?? 0),
                    Actual_PfA = _payrollServices.GetFormatValue((s != null && s.act_pf_a > 0) ? (s.act_pf_a ?? 0) : (empPF?.pf_group == "A" ? (decimal)(empPF?.add_percent_amount ?? 0) : 0)),
                    Actual_RemoteAreaAll = _payrollServices.GetFormatValue(s?.remote_area_all ?? 0),
                    Actual_PfD = _payrollServices.GetFormatValue((s != null && s.act_pf_d > 0) ? (s.act_pf_d ?? 0) : ((empPF?.pf_group == "A" || empPF?.pf_group == "B") ? (decimal)(empPF?.ded_percent_amount ?? 0) : 0)),
                    Actual_CitD = _payrollServices.GetFormatValue((s != null && s.a_cit_d > 0) ? (s.a_cit_d ?? 0) : (empCIT?.cit_type == "B" ? Math.Round(basicSalary * (decimal)(empCIT?.percent_amount ?? 0) / 100, 0) : (empCIT?.cit_type == "F" ? (decimal)(empCIT?.percent_amount ?? 0) : 0))),
                    Actual_CitType = (s != null) ? (s.cit_type) : empCIT?.cit_type,
                    Actual_CitPercentAmount = citTypeCombined,

                    // Dashain Percent Amount(radio buttons)
                    //FOR LABEL
                    d_0_p = taxSetting?.initial_tax_percent ?? 0,
                    d_1_p = taxSetting?.first_tax_percent ?? 0,
                    d_2_p = taxSetting?.second_tax_percent ?? 0,
                    d_3_p = taxSetting?.third_tax_percent ?? 0,
                    d_4_p = taxSetting?.fourth_tax_percent ?? 0,
                    d_5_p = taxSetting?.fifth_tax_percent ?? 0,
                    //FOR VALUE
                    DahsianPerAmt_d_0_p = s?.percent_for_tax_add ?? dashainPerAmt?.percent_for_tax_add?.ToString() ?? "",
                    is_dashain_already = isDashainAlready,

                    // PREVIOUS
                    Previous_BasicSalary = _payrollServices.GetFormatValue(prev?.BasicSalarySumTaken ?? 0),
                    Previous_PfA = _payrollServices.GetFormatValue(prev?.PfATaken ?? 0),
                    Previous_RemoteAreaAllTaken = _payrollServices.GetFormatValue(prev?.RemoteAreaAllTaken ?? 0),
                    Previous_DashainAmountTaken = _payrollServices.GetFormatValue(prev?.DashainAmountTaken ?? 0),
                    Previous_PerformanceAllTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.performance_all ?? 0),
                    Previous_ChildrenEduAllTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.children_edu_all ?? 0),
                    Previous_GratuityTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.gratuity ?? 0),
                    Previous_GratuityDedTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.gratuity_ded ?? 0),
                    Previous_SSFTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.ssf ?? 0),
                    Previous_SSFDedTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.ssf_ded ?? 0),
                    Previous_MedExpReimTotalTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.medical_expense_reimburse_total ?? 0),
                    Previous_MedicalExpenseReimburseEligibleTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.medical_expense_reimburse_eligible ?? 0),
                    Previous_MedicalDeductionOnTaxTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.medical_deduction_on_tax ?? 0),
                    Previous_LeaveEncashTaken = _payrollServices.GetFormatValue(thisOfficeSalary?.leave_encash ?? 0),
                    Previous_Others = _payrollServices.GetFormatValue(prev?.OthersTaken ?? 0),
                    Previous_Betalabi = _payrollServices.GetFormatValue(prev?.BetalabiDDud ?? 0),
                    Previous_PfD = _payrollServices.GetFormatValue(prev?.PfDDud ?? 0),
                    Previous_CitD = _payrollServices.GetFormatValue(prev?.CitDud ?? 0),
                    Previous_SalGotOvertimeSum = _payrollServices.GetFormatValue(thisOfficeSalary?.overtime ?? 0),
                    Previous_InsuranceTaken = thisOfficeSalary?.insurance_taken ?? 0,
                    Previous_PreAccessTax = _payrollServices.GetFormatValue(prev?.PreAccessTaxTaken ?? 0),
                    Previous_TaxDud = _payrollServices.GetFormatValue(prev?.TaxDud ?? 0),

                    count_days_dashain = fn_count_days_dashain,
                    Gender_ded = gender_ded

                };
            }).ToList();

            var totalRecord = employees.Count;
            //var j_field_no = 0;
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,

                data = employees.Select((x, index) => new {
                    //j_field_no = start + index + 1,
                    j_field_no = index + 1,
                    x.EmplopyeeInfo_EmpId,
                    x.EmplopyeeInfo_EmployeeName,
                    x.EmplopyeeInfo_EmpCode,
                    x.EmplopyeeInfo_EmployeeFullName,
                    x.emp_id,

                    d_0_p = x.d_0_p,
                    d_1_p = x.d_1_p,
                    d_2_p = x.d_2_p,
                    d_3_p = x.d_3_p,
                    d_4_p = x.d_4_p,
                    d_5_p = x.d_5_p,

                    // Current
                    basicsalary = $"<input type='number' class='salary-bulk-textbox' name='basic_salary{index + 1}' value='{x.Current_BasicSalary}' onkeyup=\"calculate_employee_salary_new('{index + 1}')\"/><input type='hidden' class='salary-bulk-textbox' name='gender_ded{index + 1}' value='{x.Gender_ded}' /><input type='hidden' class='salary-bulk-textbox' name='empid{index + 1}' value='{x.EmplopyeeInfo_EmpId}' />",
                    pf = $"<input type='number'  class='salary-bulk-textbox' name='pf_a{index + 1}' value='{x.Current_pfA}' onkeyup=\"calculate_employee_salary_new('{index + 1}')\"/>",
                    performancebonus = $"<input type='number' class='salary-bulk-textbox' name='performance_all{index + 1}' value='{x.Current_PerformanceAll}' onkeyup=\"calculate_employee_salary_new('{index + 1}')\"/>",
                    lipreimbursement = $"<input type='text' class='salary-bulk-textbox' name='yearly_insurance{index + 1}' value='{x.Current_Insurance}' readonly/>",
                    otherallowance = $"<input type='text' class='salary-bulk-textbox' name='others{index + 1}' value='{x.Current_Others}' onkeyup=\"calculate_employee_salary_new('{index + 1}')\"/>",
                    childreneduallowance = $"<input type='text' class='salary-bulk-textbox' name='children_edu_all{index + 1}' value='{x.Current_ChildrenEduAll}' readonly/>",
                    gratuity = $"<input type='text' class='salary-bulk-textbox' name='gratuity{index + 1}' value='{x.Current_Gratuity}' /><input type='hidden' name='gratudi{index + 1}' value='{x.Current_Gratudi}' />",
                    ssf = $"<input type='text' class='salary-bulk-textbox' name='ssf{index + 1}' value='{x.Current_Ssf}' />",
                    raa = $"<input type='text' class='salary-bulk-textbox' name='remote_area_all{index + 1}' value='{x.Current_RemoteAreaAll}' />",
                    yearlyraaexem = $"<input type='text' class='salary-bulk-textbox' name='dud_remote_area_all{index + 1}' value='{x.Current_YearlyRemoteExem}' readonly/>",
                    overtime = $"<input type='text' class='salary-bulk-textbox' name='overtime{index + 1}' value='{x.Current_Overtime}' />",
                    medical = $"<input type='text' class='salary-bulk-textbox' name='medical_expense_reimburse_total{index + 1}' value='{x.Current_MedicalExpenseReimburseTotal}' readonly/>",
                    eligiblemedical = $"<input type='text' class='salary-bulk-textbox' name='medical_expense_reimburse_eligible{index + 1}' value='{x.Current_MedicalExpenseReimburseEligible}' readonly/>",
                    leaveencash = $"<input type='text' class='salary-bulk-textbox' name='leave_encash{index + 1}' value='{x.Current_LeaveEncash}' />",

                    // Dashain Bonus
                    dashainbonus = $"<input type='checkbox' name='is_dashain{index + 1}' {(x.DashainBonus_Bonus == "Y" ? "checked" : "")} /><input type='hidden' name='is_dashain_check{index + 1}'>",
                    dashainamount = $"<input type='text' class='salary-bulk-textbox' name='dashain_a{index + 1}' value='{x.DashainBonus_Amount}' readonly/>",

                    // Deductions
                    basicd = $"<input type='text' class='salary-bulk-textbox' name='d_amt{index + 1}' value='{x.DedTaxCalculation_BasicD}' readonly/>",
                    pfded = $"<input type='text' class='salary-bulk-textbox' name='pf_d{index + 1}' value='{x.DedTaxCalculation_PfD}' />",
                    citded = $"<input type='text' class='salary-bulk-textbox' name='cit_d{index + 1}' value='{x.DedTaxCalculation_CitD}' readonly/>",
                    gratuityded = $"<input type='text' class='salary-bulk-textbox' name='gratuity_ded{index + 1}' value='{x.DedTaxCalculation_GratuityDed}' />",
                    ssfded = $"<input type='text' class='salary-bulk-textbox' name='ssf_ded{index + 1}' value='{x.DedTaxCalculation_SsfDed}' />",
                    insurancelife = $"<input type='text' class='salary-bulk-textbox' name='insurance_d{index + 1}' value='{x.DedTaxCalculation_InsuranceD}' readonly/>",
                    insurancenonlife = $"<input type='text' class='salary-bulk-textbox' name='insurance_d_nl{index + 1}' value='{x.DedTaxCalculation_InsuranceDNL}' readonly/>",
                    betalabi = $"<input type='text' class='salary-bulk-textbox' name='betalabi_d{index + 1}' value='{x.DedTaxCalculation_BetalabiD}' />",
                    medicaldeductionontax = $"<input type='text' class='salary-bulk-textbox' name='medical_deduction_on_tax{index + 1}' value='{x.DedTaxCalculation_MedicalDeductionOnTax}' readonly/>",
                    //PrevExcessTax = $"<input type='text' name='pre_access_tax{index + 1}' value='{x.DedTaxCalculation_PreAccessTax}' />",

                    // Gross
                    prevyearexcesstax = $"<input type='hidden' class='salary-bulk-textbox' name='month_diff{index + 1}' value='{x.Gross_MonthDiff}' /><input type='text' class='salary-bulk-textbox' name='pre_access_tax{index + 1}' value='{x.Gross_PrevYearExcessTax}' />",
                    yearlysalary = $"<input type='text' class='salary-bulk-textbox' name='yearly_gross_salary{index + 1}' value='{x.Gross_YearlyTaxableSalary}' readonly/>",
                    yearlytax = $"<input type='text' class='salary-bulk-textbox' name='yearly_gross_tax{index + 1}' value='{x.Gross_YearlyTax}' readonly/>",
                    monthlysalary = $"<input type='text' class='salary-bulk-textbox' name='monthly_gross_salary{index + 1}' value='{x.Gross_MonthlySalary}' readonly/>",
                    monthlytax = $"<input type='text' class='salary-bulk-textbox' name='incometax_d{index + 1}' value='{x.Gross_MonthTax}' />",
                    netinhand = $"<input type='text' class='salary-bulk-textbox' name='gross_salary_after_tax{index + 1}' value='{x.Gross_MonthAmount}' readonly/>",

                    // Advances
                    advpersonnel = $"<input type='text' class='salary-bulk-textbox' name='txtadvpe{index + 1}' value='{x.Advances_Personnel}' readonly/>",
                    advprogram = $"<input type='text' class='salary-bulk-textbox' name='txtadvpr{index + 1}' value='{x.Advances_Program}' readonly/>",
                    advtravel = $"<input type='text' class='salary-bulk-textbox' name='txtadvtr{index + 1}' value='{x.Advances_Travel}' readonly/>",
                    advfielddrawing = $"<input type='text' class='salary-bulk-textbox' name='txtadvfd{index + 1}' value='{x.Advances_FieldDrawing}' readonly/>",
                    advwelfare = $"<input type='text' class='salary-bulk-textbox' name='txtadvwl{index + 1}' value='{x.Advances_Welfare}' readonly/>",
                    advpfloan = $"<input type='text' class='salary-bulk-textbox' name='txtadvpf{index + 1}' value='{x.Advances_PfLoan}' readonly/>",
                    advcitloan = $"<input type='text' class='salary-bulk-textbox' name='txtadvcit{index + 1}' value='{x.Advances_CitLoan}' readonly/>",

                    // Welfare
                    welfarepercent = $"<input type='text' class='salary-bulk-textbox' name='welfare_fund_per{index + 1}' value='{x.Welfare_Percentage}' readonly/>",
                    welfarefund = $"<input type='text' class='salary-bulk-textbox' name='welfare_fund{index + 1}' value='{x.Welfare_Amount}' readonly/>",

                    // Net In Hand
                    netinhandamount = $"<input type='text' class='salary-bulk-textbox' name='net_in_hand{index + 1}' value='{x.NetInHand_Amount}' readonly/>",

                    // Others
                    othersremarks = $"<input type='text' class='salary-bulk-textbox' name='remarks{x.EmplopyeeInfo_EmpId}' value='{x.Others_Remarks}' />",

                    // Actuals (readonly textboxes)
                    actbasicsalary = $"<input type='text' class='salary-bulk-textbox' name='act_basic_salary{index + 1}' value='{x.Actual_BasicSalary}' readonly />",
                    actpfa = $"<input type='text' class='salary-bulk-textbox' name='act_pf_a{index + 1}' value='{x.Actual_PfA}' readonly />",
                    actraa = $"<input type='text' class='salary-bulk-textbox' name='act_remote_area_all{index + 1}' value='{x.Actual_RemoteAreaAll}' readonly />",
                    actpfd = $"<input type='text' class='salary-bulk-textbox' name='act_pf_d{index + 1}' value='{x.Actual_PfD}' readonly />",
                    actcitd = $"<input type='text' class='salary-bulk-textbox' name='act_cit_d{index + 1}' value='{x.Actual_CitD}' readonly />",
                    cittype = $"<input type='text' class='salary-bulk-textbox' name='cit_type{index + 1}' value='{x.Actual_CitType}' readonly />",
                    citdesc = $"<input type='text' class='salary-bulk-textbox' name='cit_type_desc{index + 1}' value='{x.Actual_CitPercentAmount}' readonly />",

                    // Percent to be added for Dashain amount (radio buttons)
                    //dashainpercent0 = $"<input type='radio' name='rdo_val{index + 1}' value='0' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val0) && val0 == 0 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainpercent5 = $"<input type='radio' name='rdo_val{index + 1}' value='5' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val5) && val5 == 5 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainpercent6 = $"<input type='radio' name='rdo_val{index + 1}' value='6' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val6) && val6 == 6 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainpercent7 = $"<input type='radio' name='rdo_val{index + 1}' value='7' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val7) && val7 == 7 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainPercent8 = $"<input type='radio' name='rdo_val{index + 1}' value='8' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val8) && val8 == 8 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainpercent9 = $"<input type='radio' name='rdo_val{index + 1}' value='9' {(int.TryParse(x.DahsianPerAmt_d_0_p, out var val9) && val9 == 9 ? "checked" : "")}  onclick=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}', 'rdo')\"/>",
                    //dashainpercent = $"<select name='dashainpercent{index + 1}'>{GetTaxPercent(decimal.TryParse(x.DahsianPerAmt_d_0_p, out var selVal) ? selVal : (decimal?)null)}</select>",
                    dashainpercent = $"<select name='rdo_val{index + 1}' style='width:200px;' onchange=\"check_dashain_new('{x.is_dashain_already}','{x.Actual_BasicSalary}','{index + 1}','rdo')\">{GetTaxPercent(decimal.TryParse(x.DahsianPerAmt_d_0_p, out var selVal) ? selVal : (decimal?)null)}</select>",

                    // Previous Totals (all textboxes)
                    prevbasicsalary = $"<input type='text' class='salary-bulk-textbox' name='basic_salary_sum_taken{index + 1}' value='{x.Previous_BasicSalary}' readonly />",
                    prevpfa = $"<input type='text' class='salary-bulk-textbox' name='pf_a_taken{index + 1}' value='{x.Previous_PfA}' readonly />",
                    prevraa = $"<input type='text' class='salary-bulk-textbox' name='remote_area_all_taken{index + 1}' value='{x.Previous_RemoteAreaAllTaken}' readonly />",
                    prevdashainbonus = $"<input type='text' class='salary-bulk-textbox' name='dashain_amount_taken{index + 1}' value='{x.Previous_DashainAmountTaken}' readonly />",
                    prevperformancebonus = $"<input type='text' class='salary-bulk-textbox' name='performance_all_taken{index + 1}' value='{x.Previous_PerformanceAllTaken}' readonly />",
                    prevcea = $"<input type='text' class='salary-bulk-textbox' name='children_edu_all_taken{index + 1}' value='{x.Previous_ChildrenEduAllTaken}' readonly />",
                    prevgratuityadd = $"<input type='text' class='salary-bulk-textbox' name='gratuity_taken{index + 1}' value='{x.Previous_GratuityTaken}' readonly />",
                    prevgratuityded = $"<input type='text' class='salary-bulk-textbox' name='gratuity_ded_taken{index + 1}' value='{x.Previous_GratuityDedTaken}' readonly />",
                    prevssfadd = $"<input type='text' class='salary-bulk-textbox' name='ssf_taken{index + 1}' value='{x.Previous_SSFTaken}' readonly />",
                    prevssfded = $"<input type='text' class='salary-bulk-textbox' name='ssf_ded_taken{index + 1}' value='{x.Previous_SSFDedTaken}' readonly />",
                    prevmedical = $"<input type='text' class='salary-bulk-textbox' name='med_exp_reim_total_taken{index + 1}' value='{x.Previous_MedExpReimTotalTaken}' readonly />",
                    preveligiblemedical = $"<input type='text' class='salary-bulk-textbox' name='med_exp_reim_eligible_taken{index + 1}' value='{x.Previous_MedicalExpenseReimburseEligibleTaken}' readonly />",
                    prevmedicaldeductionontax = $"<input type='text' class='salary-bulk-textbox' name='medical_deduction_on_tax_taken{index + 1}' value='{x.Previous_MedicalDeductionOnTaxTaken}' readonly />",
                    prevleaveencash = $"<input type='text' class='salary-bulk-textbox' name='leave_encash_taken{index + 1}' value='{x.Previous_LeaveEncashTaken}' readonly />",
                    prevothers = $"<input type='text' class='salary-bulk-textbox' name='others_taken{index + 1}' value='{x.Previous_Others}' readonly />",
                    prevbetalabi = $"<input type='text' class='salary-bulk-textbox' name='betalibi_d_dud{index + 1}' value='{x.Previous_Betalabi}' readonly />",
                    prevpfded = $"<input type='text' class='salary-bulk-textbox' name='pf_d_dud{index + 1}' value='{x.Previous_PfD}' readonly />",
                    prevcit = $"<input type='text' class='salary-bulk-textbox' name='cit_dud{index + 1}' value='{x.Previous_CitD}' readonly />",
                    prevovertime = $"<input type='text' class='salary-bulk-textbox' name='sal_got_overtime_sum{index + 1}' value='{x.Previous_SalGotOvertimeSum}' readonly />",
                    previnsurancelife = $"<input type='text' class='salary-bulk-textbox' name='insurance_taken{index + 1}' value='{x.Previous_InsuranceTaken}' readonly />",
                    previnsurancenl = $"<input type='text' class='salary-bulk-textbox' name='pre_access_tax_taken{index + 1}' value='{x.Previous_PreAccessTax}' readonly />",
                    prevtax = $"<input type='text' class='salary-bulk-textbox' name='tax_dud{index + 1}' value='{x.Previous_TaxDud}' readonly />",

                    // Employee other information
                    maritalstatus = $"<input type='text' class='salary-bulk-textbox' name='marital_status{index + 1}' value='{x.EmplopyeeInfo_MaritalStatus}' readonly />",
                    gender = $"<input type='text' class='salary-bulk-textbox' name='gender{index + 1}' value='{x.EmplopyeeInfo_Gender}' readonly />",
                    startdate = $"<input type='text' class='salary-bulk-textbox' name='start_date{index + 1}' value='{x.EmplopyeeInfo_StartDate?.ToString("yyyy-MM-dd")}' readonly />",
                    enddate = $"<input type='text' class='salary-bulk-textbox' name='end_date{index + 1}' value='{x.EmplopyeeInfo_EndDate?.ToString("yyyy-MM-dd")}' readonly />",
                    daysuptosep30 = $"<input type='text' class='salary-bulk-textbox' name='days_sep30{index + 1}' value='{x.count_days_dashain}' readonly />",

                    // Action
                    action = $"<a href=\"#\" onclick=\"postdata_salary_bulk_indv_clear('{x.EmplopyeeInfo_EmpId}','{x.EmplopyeeInfo_EmployeeFullName}')\">Clear</a>"

                })
            };
            return new JsonResult(jsonData);
        }
        public string GetTaxPercent(decimal? selValue)
        {
            var tax = _context.tbl_tax_setting.FirstOrDefault();

            var taxPercents = new List<SelectListItem>();
            if (tax != null)
            {
                taxPercents.Add(new SelectListItem { Text = tax.initial_tax_percent.HasValue ? tax.initial_tax_percent.Value.ToString("0.##") + " %" : string.Empty, Value = "0", Selected = (selValue.HasValue && selValue.Value == 0) });
                taxPercents.Add(new SelectListItem { Text = tax.first_tax_percent.ToString() + " %", Value = "5", Selected = (selValue.HasValue && selValue.Value == 5) });
                taxPercents.Add(new SelectListItem { Text = tax.second_tax_percent.ToString() + " %", Value = "6", Selected = (selValue.HasValue && selValue.Value == 6) });
                taxPercents.Add(new SelectListItem { Text = tax.third_tax_percent.ToString() + " %", Value = "7", Selected = (selValue.HasValue && selValue.Value == 7) });
                taxPercents.Add(new SelectListItem { Text = tax.fourth_tax_percent.ToString() + " %", Value = "8", Selected = (selValue.HasValue && selValue.Value == 8) });
                taxPercents.Add(new SelectListItem { Text = tax.fifth_tax_percent.ToString() + " %", Value = "9", Selected = (selValue.HasValue && selValue.Value == 9) });
            }

            // Build HTML string for dropdown options
            var sb = new StringBuilder();
            foreach (var item in taxPercents)
            {
                sb.Append($"<option value='{item.Value}' {(item.Selected ? "selected" : "")}>{item.Text}</option>");
            }

            return sb.ToString();
        }
        [HttpPost]
        public JsonResult ClearSalary(int? employee_id, string? trans)
        {
            var RecordToDelete = new tbl_employee_salary
            {
                emp_id = Convert.ToInt32(employee_id),
            };
            _context.tbl_employee_salary.Remove(RecordToDelete);
            _context.SaveChanges();

            return Json(new { status = "success" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalaryBulkSave([FromBody] EmployeeSalaryListViewModel model)
        {
            string PageId = "10907";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            //FOR TAX SETTINGS
            decimal? initial_tax_percent = 0;
            double? first_tax_percent = 0;
            double? second_tax_percent = 0;
            double? third_tax_percent = 0;
            double? fourth_tax_percent = 0;
            double? fifth_tax_percent = 0;

            double? first_tax_amount = 0;
            decimal? second_tax_amount = 0;
            decimal? third_tax_amount_single = 0;
            decimal? third_tax_amount_married = 0;
            decimal? fourth_tax_amount = 0;

            var taxSetting = _context.tbl_tax_setting
                .FirstOrDefault();
            if (taxSetting != null)
            {
                initial_tax_percent = taxSetting.initial_tax_percent;
                first_tax_percent = taxSetting.first_tax_percent;
                second_tax_percent = taxSetting.second_tax_percent;
                third_tax_percent = taxSetting.third_tax_percent;
                fourth_tax_percent = taxSetting.fourth_tax_percent;
                fifth_tax_percent = taxSetting.fifth_tax_percent;

                first_tax_amount = taxSetting.first_tax_amount;
                second_tax_amount = taxSetting.second_tax_amount;
                third_tax_amount_single = taxSetting.third_tax_amount_single ?? 0m;
                third_tax_amount_married = taxSetting.third_tax_amount_married ?? 0m;
                fourth_tax_amount = taxSetting.fourth_tax_amount;
            }

            foreach (var emp in model.Fields)
            {
                var recordsToDelete = await _context.tbl_employee_salary
                    .Where(r => r.emp_id == emp.emp_id && r.sal_year == emp.sal_year && r.sal_month == emp.sal_month)
                    .ToListAsync();

                if (recordsToDelete.Any())
                {
                    _context.tbl_employee_salary.RemoveRange(recordsToDelete);
                    await _context.SaveChangesAsync();
                }

                var maxId = await _context.tbl_employee_salary.MaxAsync(e => (int?)e.salary_id) ?? 0;
                var newId = maxId + 1;
                var newRow = new tbl_employee_salary
                {
                    salary_id = newId,
                    emp_id = emp.emp_id,
                    sal_year = emp.sal_year,
                    sal_month = emp.sal_month,
                    basic_salary = emp.basic_salary,
                    grade = emp.grade,
                    pf_a = emp.pf_a,
                    children_edu_all = emp.children_edu_all,
                    performance_all = emp.performance_all,
                    remote_area_all = emp.remote_area_all,
                    others = emp.others,
                    overtime = emp.overtime,
                    pf_d = emp.pf_d,
                    incometax_d = emp.incometax_d,
                    insurance_d = emp.insurance_d,
                    cit_d = emp.cit_d,
                    betalibi_d = emp.betalibi_d,

                    //is_dashain = emp.is_dashain,
                    is_dashain = emp.is_dashain_check,
                    dashain_a = emp.dashain_a,

                    tel_per_adv = emp.tel_per_adv,
                    travel_prog_adv = emp.travel_prog_adv,
                    remarks = emp.remarks,

                    submit_date = System.DateTime.Now,
                    submit_by = Convert.ToInt32(HttpContext.Session.GetString("emp_id")),

                    percent_for_tax_add = emp.percent_for_tax_add,
                    medical_deduction_on_tax = emp.medical_deduction_on_tax,
                    welfare_fund = emp.welfare_fund,
                    remote_exem = emp.remote_exem,
                    gratudi = emp.gratudi,

                    act_basic_salary = emp.act_basic_salary,
                    act_pf_a = emp.act_pf_a,
                    act_remote_area_all = emp.act_remote_area_all,
                    act_pf_d = emp.act_pf_d,
                    a_cit_d = emp.a_cit_d,

                    cit_type = emp.cit_type,
                    cit_percent_amonnt = emp.cit_percent_amonnt,

                    marital_d = emp.marital_d, // check
                    yearly_salary = emp.yearly_salary,
                    yearly_tax = emp.yearly_tax,
                    monthly_salary = emp.monthly_salary,
                    month_amount = emp.month_amount,
                    pr_adv = emp.pr_adv,
                    fd_adv = emp.fd_adv,
                    wl_adv = emp.wl_adv,
                    wl_per = emp.wl_per,
                    net_in_hand = emp.net_in_hand,
                    insurance = emp.insurance,

                    first_taxable_amount = (decimal)(first_tax_amount ?? 0d),

                    initial_tax_percent = (double)(initial_tax_percent ?? 0m),
                    first_tax_percent = first_tax_percent,
                    second_tax_percent = second_tax_percent,

                    pre_access_tax = emp.pre_access_tax,
                    adv_PF_loan = emp.adv_PF_loan,
                    adv_CIT_loan = emp.adv_CIT_loan,

                    d_3_amt = emp.marital_status == "S" ? third_tax_amount_single : third_tax_amount_married,
                    d_3_p = third_tax_percent,
                    d_4_p = fourth_tax_percent,

                    fiscal_year = emp.fiscal_year,
                    emp_week = emp.emp_week,
                    gratuity = emp.gratuity,
                    gratuity_ded = emp.gratuity_ded,
                    medical_expense_reimburse_eligible = emp.medical_expense_reimburse_eligible,
                    medical_expense_reimburse_total = emp.medical_expense_reimburse_total,
                    leave_encash = emp.leave_encash,

                    second_tax_amount = second_tax_amount,
                    gender_ded_per = emp.gender_ded_per,
                    ssf = emp.ssf,
                    ssf_ded = emp.ssf_ded,
                    insurance_d_nl = emp.insurance_d_nl,
                    fourth_tax_amount = fourth_tax_amount,
                    fifth_tax_percent = fifth_tax_percent,

                    annual_health_checkup_add = emp.annual_health_checkup_add,
                    annual_health_checkup_ded = emp.annual_health_checkup_ded

                };
                _context.tbl_employee_salary.Add(newRow);
                _ = _context.SaveChanges();
            }
            return Json(new { status = "success", message = Lang.msg_added_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10363 EXCESS LEAVE ENCASHMENT
        [HttpGet]
        public IActionResult ExcessLeaveEncashment(string fiscalYearFilter, string? periodInput)
        {
            string PageId = "10363";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            string? FiscalYearActive = HttpContext.Session.GetString("FiscalYear");
            ViewBag.FiscalYearActive = FiscalYearActive;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));

            return PartialView("Payroll/_ExcessLeaveEncashment", "");
        }
        public async Task<IActionResult> ExcessLeaveEncashmentList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string FiscalYearFilter = request.FiscalYearFilter;
            bool? blnShow = false;
            bool hasRecords = _context.tbl_employee_excess_leave_encash_emp_wise
                .Any(x => x.fiscal_year == FiscalYearFilter);

            var query = hasRecords
                ? (from emp in _context.tbl_employee
                   where emp.emp_status == "A"
                   join lft in _context.tbl_employee_excess_leave_encash_emp_wise
                           .Where(x => x.fiscal_year == FiscalYearFilter)
                           on emp.emp_id equals lft.emp_id
                   orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                   select new
                   {
                       emp.emp_id,
                       emp.firstname,
                       emp.middlename,
                       emp.lastname,
                       emp.emp_status,
                       emp.emp_code,
                       employee = $"{emp.firstname} {emp.middlename} {emp.lastname}",
                       amount = lft.amount ?? 0m,   // cast to decimal
                       remarks = lft.remarks ?? string.Empty,
                       blnShow = true
                   })
                : (from emp in _context.tbl_employee
                   where emp.emp_status == "A"
                   orderby emp.emp_status, emp.firstname, emp.middlename, emp.lastname
                   select new
                   {
                       emp.emp_id,
                       emp.firstname,
                       emp.middlename,
                       emp.lastname,
                       emp.emp_status,
                       emp.emp_code,
                       employee = $"{emp.firstname} {emp.middlename} {emp.lastname}",
                       amount = 0m,                 // cast to decimal
                       remarks = string.Empty,
                       blnShow = true
                   });



            // Search filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(e =>
                e.firstname.Contains(searchValue) ||
                e.middlename.Contains(searchValue) ||
                e.lastname.Contains(searchValue)
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                if (sortColumn == "employee")
                {
                    if (sortColumnDir == "asc")
                    {
                        query = query.OrderBy(d => d.firstname).ThenBy(d => d.middlename).ThenBy(d => d.lastname);
                    }
                    else
                    {
                        query = query.OrderByDescending(d => d.firstname).ThenByDescending(d => d.middlename).ThenByDescending(d => d.lastname);
                    }
                }
                else
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }
            }

            var data = query.ToList();
            if(data.Any(r => r.amount > 0))
            {
                blnShow = true;
            }
            int totalRecord = data.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                blnShow = blnShow,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ExcessLeaveEncashmentSave([FromBody] EmployeeExcessLeaveEncashListViewModel model)
        {
            string PageId = "10363";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            _ = ModelState.Remove("id");
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var item in model.Fields)
            {
                if (!item.emp_id.HasValue) { continue; }

                var StartEndDates = _settingsServices.GetFiscalStartEndDate(item.fiscal_year!);
                DateTime start_fiscal_date = StartEndDates.StartDate;
                DateTime end_fiscal_date = StartEndDates.EndDate;

                var existing = _context.tbl_employee_excess_leave_encash_emp_wise
                .Where(a => a.emp_id == item.emp_id
                            && a.fiscal_year == item.fiscal_year
                            && a.counter == item.counter)
                .ToList();

                if (existing.Count > 0)
                {
                    _context.tbl_employee_excess_leave_encash_emp_wise.RemoveRange(existing);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
                if (item.amount > 0)
                {
                    //var nextId = Guid.NewGuid().ToString();
                    var id = UniqueID();

                    var newRec = new tbl_employee_excess_leave_encash_emp_wise
                    {
                        id = id,
                        emp_id = item.emp_id,
                        fiscal_year = item.fiscal_year,
                        counter = 1,
                        amount = item.amount.GetValueOrDefault(),
                        total_hours = 0,
                        remarks = item.remarks,
                    };
                    _ = _context.tbl_employee_excess_leave_encash_emp_wise.Add(newRec);
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();

                    SetInsertAccrualFundSource("tbl_employee_excess_leave_encash_emp_wise", id, Convert.ToInt32(item.emp_id), Convert.ToString(item.fiscal_year), Convert.ToDateTime(start_fiscal_date), Convert.ToDateTime(end_fiscal_date), "tbl_employee_excess_leave_encash_fund_wise", 1);
                }
            }
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ExcessLeaveEncashmentClear(string? fiscalYear, int? period)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_excess_leave_encash_emp_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM tbl_employee_excess_leave_encash_fund_wise WHERE fiscal_year = {0} AND counter = {1}", fiscalYear, period);

            return Json(new
            {
                status = "success",
                message = "clearsuccess",
                fiscal_year = fiscalYear,
                period = period
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportExcessLeaveEncash(string fiscalYear)
        {
            // Break fiscal year like Classic ASP
            var fiscalYearBreak = fiscalYear.Split('/');

            // Get organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee excess leave encashment records
            var records = (from e in _context.tbl_employee
                           join l in _context.tbl_employee_excess_leave_encash_emp_wise
                               on e.emp_id equals l.emp_id
                           where l.fiscal_year == fiscalYear && l.counter == 1
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               l.amount,
                               l.remarks
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("ExcessLeaveEncash");

                int row = 1;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Organization: " + orgName;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;

                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row++, 1).Value = "Staff Statement of Excess Leave Encashment";

                row++;
                // Header row
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee Code";
                ws.Cell(row, 4).Value = "Amount";
                ws.Cell(row, 5).Value = "Remarks";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                decimal totalAmount = 0;

                foreach (var r in records)
                {
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = r.amount ?? 0m;
                    ws.Cell(row, 5).Value = r.remarks ?? string.Empty;

                    totalAmount += r.amount ?? 0m;
                    row++;
                }

                // Totals row
                ws.Cell(row, 1).Value = "Total";
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 4).Value = totalAmount;

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_excess_leave_encash_export_{fiscalYearBreak[1]}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportExcessLeaveEncashCCD(string fiscalYear, int period)
        {
            // Break fiscal year like Classic ASP
            var fiscalYearBreak = fiscalYear.Split('/');

            // Get Organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            // Query employee excess leave encashment records
            var records = (from e in _context.tbl_employee
                           join l in _context.tbl_employee_excess_leave_encash_emp_wise
                               on e.emp_id equals l.emp_id
                           where l.fiscal_year == fiscalYear && l.counter == period
                           orderby e.firstname, e.middlename, e.lastname
                           select new
                           {
                               e.emp_id,
                               e.emp_code,
                               FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                               total_hours = l.total_hours,
                               amount = l.amount,
                               remarks = l.remarks
                           }).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("ExcessLeaveEncashCCD");

                int row = 1;
                ws.Cell(row++, 1).Value = "Organization: " + orgName;
                ws.Cell(row++, 1).Value = "Fiscal Year: " + fiscalYear;
                ws.Cell(row++, 1).Value = "Period: " + period;
                ws.Cell(row++, 1).Value = "Staff Statement of Excess Leave Encashment with Fund Source Allocation";

                row++;
                // Header row
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Employee Name";
                ws.Cell(row, 3).Value = "Employee ID";
                ws.Cell(row, 4).Value = "Fund Source";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Amount";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;
                foreach (var r in records)
                {
                    // Main employee row
                    ws.Cell(row, 1).Value = serial++;
                    ws.Cell(row, 2).Value = r.FullName;
                    ws.Cell(row, 3).Value = r.emp_code;
                    ws.Cell(row, 4).Value = "";
                    ws.Cell(row, 5).Value = r.total_hours;
                    ws.Cell(row, 6).Value = r.amount;
                    row++;

                    // Fund-wise allocations
                    var fundWise = _context.tbl_employee_excess_leave_encash_fund_wise
                        .Where(f => f.emp_id == r.emp_id && f.fiscal_year == fiscalYear && f.counter == period)
                        .ToList();

                    foreach (var f in fundWise)
                    {
                        if (f.hours == 0) continue;

                        string fundSource = _context.tbl_fund_source
                            .Where(fs => fs.fund_id == f.fund_id)
                            .Select(fs => fs.fund_source)
                            .FirstOrDefault();

                        // Build GL code (simplified version of Classic ASP logic)
                        string glFundSourceCode = $"{fundSource}-{r.emp_code}";

                        decimal? amount = r.total_hours != null && r.total_hours != 0
                            ? Math.Round(((r.amount ?? 0m) * (decimal)(f.hours ?? 0d)) / (decimal)(r.total_hours ?? 0d), 2)
                            : 0m;

                        ws.Cell(row, 4).Value = glFundSourceCode;
                        ws.Cell(row, 5).Value = f.hours;
                        ws.Cell(row, 6).Value = amount;
                        row++;
                    }
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());
                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_excess_leave_encash_ccd_{fiscalYearBreak[1]}_{period}.xlsx",
                        fileContent = content
                    });
                }
            }
        }

        #endregion

        public IActionResult Index()
        {
            return View();
        }
    }
}
