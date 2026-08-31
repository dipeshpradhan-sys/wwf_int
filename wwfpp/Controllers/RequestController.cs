using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reporting.NETCore;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Models.General;
using wwfpp.Models.Payroll;
using wwfpp.Models.Request;
using wwfpp.Services;
using wwfpp.wwwroot.js;
using static GblUtilities;
using static GblUtilities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Controllers
{
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly EmailService _emailService;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly EmployeeServices _employeeServices;
        private readonly SettingsServices _settingsServices;
        private readonly AccountServices _accountServices;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RequestServices _requestServices;
        private readonly AdministrationEmailService _administrationEmailService;
        private readonly LeaveServices _leaveServices;
        private readonly EmployeeOvertimeServices _employeeOvertimeServices;
        private readonly TravelApprovalService _travelApprovalService;
        private readonly ApproverResolverService _approverResolver;
        private readonly PayrollServices _payrollServices;
        public RequestController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            EmailService emailService,
            GlobalOptionServices globalOptionServices,
            EmployeeServices employeeServices,
            SettingsServices settingsServices,
            RequestServices requestServices,
            AccountServices accountServices,
            LeaveServices leaveServices,
            AdministrationEmailService administrationEmailService,
            ApproverResolverService approverResolver,
            EmployeeOvertimeServices employeeOvertimeServices,
            TravelApprovalService travelApprovalService,
            IWebHostEnvironment webHostEnvironment,
            PayrollServices payrollServices
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
            _webHostEnvironment = webHostEnvironment;
            _requestServices = requestServices;
            _leaveServices = leaveServices;
            _approverResolver = approverResolver;
            _employeeOvertimeServices = employeeOvertimeServices;
            _travelApprovalService = travelApprovalService;
            _administrationEmailService = administrationEmailService;
            _payrollServices = payrollServices;
        }

        #region EMPLOYEE MEDICAL INSURANCE
        public IActionResult MedicalInsurance()
        {
            string PageId = "10211";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            int EmployeeID = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            string empName = _employeeServices.GetEmployeeName(EmployeeID);
            if (empName != null)
                ViewBag.EmployeeFullNameCodeWithStatus = empName;
            else
                ViewBag.EmployeeFullNameCodeWithStatus = "Administrator";

            var Records = (
                        from con in _context.tbl_employee_medical_reimburse
                        join cdt in _context.tbl_fiscal_year
                            on con.fiscal_year equals cdt.fiscal_year
                        //join emp in _context.tbl_employee
                        //    on con.emp_id equals emp.emp_id
                        where con.emp_id == EmployeeID
                        orderby con.id descending
                        select new EmployeeMedicalReimburseViewModel
                        {
                            Id = con.id
                        }).ToList();

            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Request/MedicalInsurance", "ADD", PageId, Records.Count);

            return PartialView("Request/_MedicalInsuranceList", "");
        }
        [HttpPost]
        public async Task<IActionResult> MedicalInsuranceList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string? FiscalYearFilter = request.FilterValue1;
            int EmployeeID = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            if (EmployeeID <= 0 || string.IsNullOrEmpty(FiscalYearFilter))
            {
                return Json(new { draw, recordsFiltered = 0, recordsTotal = 0, data = new List<object>() });
            }

            var query = _context.vw_Employee_Medical_Insurance
                .Where(e => e.emp_id == EmployeeID);

            if (!string.IsNullOrEmpty(FiscalYearFilter))
            {
                query = query.Where(e => e.fiscal_year == FiscalYearFilter);
            }
            query = query.OrderByDescending(e => e.submit_date);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.reim_type != null && a.reim_type.Contains(searchValue)) ||
                    (a.app_status != null && a.app_status.Contains(searchValue))
                );
            }

            var data = await query.ToListAsync();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                data = data.AsQueryable().OrderBy($"{sortColumn} {sortColumnDir}").ToList();
            }

            int totalRecord = data.Count();
            if (pageSize == -1) pageSize = totalRecord;

            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }
        public async Task<IActionResult> MedicalInsuranceAddEdit(string? id, string? fiscalYear, string? mode)
        {
            string PageId = "10211";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;

            int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            string fiscalYearActive = HttpContext.Session.GetString("fiscal_year");

            EmployeeMedicalReimburseViewModel model;
            // ADD MODE → only marital status from vw_Employee
            var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_id == employeeId);

            if (!string.IsNullOrEmpty(id) && id == "0" && mode == "add")
            {
                model = new EmployeeMedicalReimburseViewModel
                {
                    EmpId = employeeId,
                    FiscalYear = fiscalYearActive,
                    MaritalStatus = emp?.marital_status == "M" ? "Married" : "Not Married",
                    emp_status = emp.emp_status
                };
            }
            else
            {
                // EDIT MODE → load from insurance table
                var entity = _context.tbl_employee_medical_reimburse.FirstOrDefault(h => h.id == id);
                if (entity == null) return NotFound();

                model = new EmployeeMedicalReimburseViewModel
                {
                    Id = entity.id,
                    EmpId = entity.emp_id,
                    FiscalYear = entity.fiscal_year,
                    ReimType = entity.reim_type,
                    BillDate = entity.bill_date,
                    BillNo = entity.bill_no,
                    SelfAmt = entity.self_amt,
                    SpouseAmt = entity.spouse_amt,
                    OtherDepAmt = entity.other_dep_amt,
                    Remarks = entity.remarks,
                    MaritalStatus = entity.marital_status == "M" ? "Married" : "Not Married",
                    emp_status = emp.emp_status,
                    app_status = entity.app_status
                };
            }
            return PartialView("Request/_MedicalInsuranceAddEdit", model);
        }
        [HttpPost]
        public async Task<IActionResult> MedicalInsuranceSave(EmployeeMedicalReimburseViewModel vm, string mode)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }

            int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            string fiscalYearActive = HttpContext.Session.GetString("fiscal_year");

            // Always fetch marital status from vw_Employee
            var emp = _context.vw_Employee.FirstOrDefault(e => e.emp_id == employeeId);
            var maritalCode = emp?.marital_status ?? "N"; // "M" or "N"

            // Normalize amounts
            double selfAmt = vm.SelfAmt ?? 0;
            double spouseAmt = vm.SpouseAmt ?? 0;
            double otherDepAmt = vm.OtherDepAmt ?? 0;

            string fiscalYear = string.IsNullOrEmpty(vm.FiscalYear) ? fiscalYearActive : vm.FiscalYear;
            string appStatus = "Pending";

            // --- Validation: check total claim against max allowed ---
            double totalAppMedClaim = 0;
            if (vm.ReimType == "Non Life Insurance")
            {
                totalAppMedClaim = _context.tbl_employee_medical_reimburse
                    .Where(r => r.emp_id == employeeId
                             && r.reim_type == "Non Life Insurance"
                             && r.app_status != "Declined"
                             && r.fiscal_year == fiscalYear
                             && (string.IsNullOrEmpty(vm.Id) || r.id != vm.Id))
                    .Sum(r => (r.self_amt ?? 0) + (r.spouse_amt ?? 0));

                totalAppMedClaim += selfAmt + spouseAmt;
            }
            else
            {
                totalAppMedClaim = _context.tbl_employee_medical_reimburse
                    .Where(r => r.emp_id == employeeId
                             && (r.reim_type == "Medical" || r.reim_type == "Life Insurance")
                             && r.app_status != "Declined"
                             && r.fiscal_year == fiscalYear
                             && (string.IsNullOrEmpty(vm.Id) || r.id != vm.Id))
                    .Sum(r => (r.self_amt ?? 0) + (r.spouse_amt ?? 0) + (r.other_dep_amt ?? 0));

                totalAppMedClaim += selfAmt + spouseAmt + otherDepAmt;
            }

            double r_max_med_e = 0;
            var taxSetting = _context.tbl_tax_setting.FirstOrDefault();
            if (taxSetting != null)
            {
                r_max_med_e = taxSetting.max_medical_expenses_reimbursed ?? 0;
            }

            if (totalAppMedClaim > r_max_med_e)
            {
                return Json(new { status = "error", message = Lang.msg_exceeded });
            }

            // --- Save ---
            if (string.IsNullOrEmpty(vm.Id) && mode == "add")
            {
                // ADD
                var entity = new tbl_employee_medical_reimburse
                {
                    id = Guid.NewGuid().ToString(),
                    fiscal_year = fiscalYear,
                    emp_id = employeeId,
                    marital_status = maritalCode,
                    bill_no = vm.BillNo,
                    bill_date = vm.BillDate,
                    self_amt = selfAmt,
                    spouse_amt = spouseAmt,
                    other_dep_amt = otherDepAmt,
                    submit_date = DateTime.Now,
                    remarks = vm.Remarks,
                    app_status = appStatus,
                    reim_type = vm.ReimType
                };
                _context.tbl_employee_medical_reimburse.Add(entity);
                await _context.SaveChangesAsync();

                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else
            {
                // EDIT
                var entity = _context.tbl_employee_medical_reimburse.FirstOrDefault(r => r.id == vm.Id);
                if (entity == null) return Json(new { status = "notfound" });

                entity.fiscal_year = fiscalYear;
                entity.emp_id = employeeId;
                entity.marital_status = maritalCode;
                entity.bill_no = vm.BillNo;
                entity.bill_date = vm.BillDate;
                entity.self_amt = selfAmt;
                entity.spouse_amt = spouseAmt;
                entity.other_dep_amt = otherDepAmt;
                entity.submit_date = DateTime.Now;
                entity.remarks = vm.Remarks;
                entity.app_status = appStatus;
                entity.reim_type = vm.ReimType;

                await _context.SaveChangesAsync();
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
        }
        #endregion

        #region EMPLOYEE Leave LIST,ADD,EDIT,SAVE, MASS DELETE
        [HttpGet]
        public IActionResult Leave(string StatusFilter)
        {
            string PageId = "10201";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.StatusPAFilter = GblUtilities.ApprovalStatus();

            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Request/Leave", "ADD|DEL", PageId, 1);
            return PartialView("Request/_Leave");
        }
        [HttpPost]
        public async Task<IActionResult> LeaveList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string FiscalYearListFilter = request.FilterValue1;
            string EmployeeStatusFilter = (request.FilterValue2 == "A" ? "Active" : "Inactive");
            string LeaveStatusFilter = request.FilterValue3 ?? "Pending";

            if (string.IsNullOrEmpty(FiscalYearListFilter)) FiscalYearListFilter = HttpContext.Session.GetString("fiscal_year") ?? "";

            var data = await _context.GetEmployeeLeave.FromSqlRaw("EXEC GetEmployeeLeave @fiscal_year = {0}, @emp_status = {1}, @leaveStatus = {2}", FiscalYearListFilter, EmployeeStatusFilter, LeaveStatusFilter).ToListAsync();
            var query = data.AsQueryable();
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy($"{sortColumn} {sortColumnDir}");
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a => a.employeename != null && a.employeename.Contains(searchValue, StringComparison.OrdinalIgnoreCase));
            }
            int totalRecord = query.Count();
            if (pageSize == -1) pageSize = totalRecord;
            var cData = query.Skip(skip).Take(pageSize).ToList();
            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpPost]
        public JsonResult CalculateLeaveHours(int empId, int leaveTypeId, DateTime fromDate, DateTime toDate)
        {
            var normalWorkingHours = _requestServices.GetLimitHoursSetting()?.normal_working_hrs ?? 8;
            var fiscalYear = HttpContext.Session.GetString("fiscal_year");

            var holidays = _context.tbl_setting_holidays.Where(h => h.fiscal_year == fiscalYear).Select(h => h.holiday_date).ToList();
            var dayOffs = _context.tbl_employee_dayoff.Where(d => d.fiscal_year == fiscalYear && d.emp_id == empId).Select(d => d.dayoff_date).ToList();

            int cntWeekend = 0, cntHoliday = 0, cntDayoff = 0, cntWorkday = 0;
            var current = fromDate;

            while (current <= toDate)
            {
                cntWorkday++;

                // Lookup calendar setting for this date
                var cal = _context.tbl_calendar_setting.FirstOrDefault(c => c.cal_month == current.Month && c.cal_year == current.Year);

                if (cal != null)
                {
                    // Build column name dynamically (d1, d2, … d31)
                    string colName = "d" + current.Day;
                    var colValue = cal.GetType().GetProperty(colName)?.GetValue(cal)?.ToString();

                    if (colValue == "W")
                    {
                        cntWeekend++;
                        current = current.AddDays(1);
                        continue;
                    }
                }

                // If not weekend, check holidays/dayoffs
                if (holidays.Contains(current)) cntHoliday++;
                if (dayOffs.Contains(current)) cntDayoff++;

                current = current.AddDays(1);
            }

            int leaveInDays = new[] { 12, 13, 14 }.Contains(leaveTypeId)
                ? cntWorkday
                : cntWorkday - (cntWeekend + cntHoliday + cntDayoff);

            double halfFactor = new[] { 2, 4, 6, 10 }.Contains(leaveTypeId) ? 0.5 : 1;
            double leaveInDaysFinal = leaveInDays * halfFactor;
            double leaveInHrs = leaveInDaysFinal * normalWorkingHours;

            return Json(new { leaveInHrs, leaveInDays = leaveInDaysFinal });
        }
        [HttpGet]
        public IActionResult CheckEmployeeManager(int empId)
        {
            var mgrInfo = _requestServices.GetEmployeeManagerAndLineManager(empId);
            bool hasManager = (mgrInfo.managerID ?? 0) > 0 || (mgrInfo.lineManagerID ?? 0) > 0;

            if (!hasManager)
                return Json(new { status = "error", message = Lang.msg_emp_manager_not_defined });

            return Json(new { status = "ok" });
        }
        public IActionResult LeaveAddEdit(int? id, string mode, string? fiscalYear, EmployeeLeaveViewModel vm)
        {
            string PageId = "10201";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.Status = GblUtilities.StatusActivePassive("AD");
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            var LeaveType = _context.tbl_leave_heading.OrderBy(c => c.description).ToList();
            ViewBag.LeaveTypeList = new SelectList(LeaveType, "leave_type_id", "description");

            var SettingNormalHours = _requestServices.GetLimitHoursSetting();
            int? normalWorkingHours = SettingNormalHours?.normal_working_hrs;
            ViewBag.NormalWorkingHours = normalWorkingHours;
            EmployeeLeaveViewModel model;
            if (id <= 0 && mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                model = new EmployeeLeaveViewModel();
            }
            else
            {
                var EmployeeFS = (from leave in _context.tbl_employee_leave
                                  join emp in _context.vw_Employee
                                      on leave.emp_id equals emp.emp_id
                                  where leave.emp_leave_id == id
                                  select new
                                  {
                                      Leave = leave,
                                      EmployeeInformations = emp
                                  })
                                     .FirstOrDefault();

                if (EmployeeFS == null) return NotFound();
                ViewBag.Employee = EmployeeFS.EmployeeInformations.employeenameWithCode;
                model = new EmployeeLeaveViewModel
                {
                    id = EmployeeFS.Leave.emp_leave_id,
                    leave_type_id = EmployeeFS.Leave.leave_type_id ?? 0,
                    emp_id = EmployeeFS.Leave.emp_id ?? 0,
                    leave_from_date = EmployeeFS.Leave.leave_from_date,
                    leave_to_date = EmployeeFS.Leave.leave_to_date,
                    leave_desc = EmployeeFS.Leave.leave_desc,
                    leave_in_hrs = EmployeeFS.Leave.leave_in_hrs ?? 0,
                    app_by_name = (EmployeeFS.Leave.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)EmployeeFS.Leave.app_by) : "",
                    app_status = EmployeeFS.Leave.app_status,
                    app_date = (EmployeeFS.Leave.app_date ?? EmployeeFS.Leave.submit_date)?.Date,
                    app_remarks = EmployeeFS.Leave.app_remarks,
                    can_status = EmployeeFS.Leave.app_status == "Cancelled" ? "Approved" : EmployeeFS.Leave.app_status == "Approved" && EmployeeFS.Leave.can_by != null && EmployeeFS.Leave.can_by > 0 ? "Pending" : "",
                    can_desc = EmployeeFS.Leave.can_desc,
                    can_by_name = (EmployeeFS.Leave.can_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)EmployeeFS.Leave.can_by) : "",
                    can_date = EmployeeFS.Leave.can_date,
                    can_remarks = EmployeeFS.Leave.can_remarks

                };
                // Calculate leave in days safely
                if (normalWorkingHours.HasValue && normalWorkingHours.Value > 0)
                {
                    model.leave_in_days = EmployeeFS.Leave.leave_in_hrs ?? 0 / normalWorkingHours.Value;
                }
                model.app_status = EmployeeFS.Leave.app_status;
                model.emp_status = EmployeeFS.EmployeeInformations.emp_status;
                var employeeManagerAndLineManager = _requestServices.GetEmployeeManagerAndLineManager(Convert.ToInt32(model.emp_id));
                int emp_managerID = employeeManagerAndLineManager.managerID ?? 0;
                int emp_lineManagerID = employeeManagerAndLineManager.lineManagerID ?? 0;
                if (emp_managerID == 0 && emp_lineManagerID == 0) ViewBag.EmployeeNoManager = Lang.msg_emp_manager_not_defined;
            }
            return PartialView("Request/_LeaveAddEdit", model);
        }
        [HttpPost]
        public async Task<IActionResult> LeaveSave(int? id, string mode, EmployeeLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }
            bool employee_can_apply_leave = true; // Later, comes from Setting when its developed
            bool employee_has_pending_leave = false;
            if (!employee_can_apply_leave)
            {
                return Json(new { status = "error", message = Lang.msg_leave_apply_elligible });
            }
            if (employee_has_pending_leave)
            {
                return Json(new { status = "error", message = Lang.msg_pending_leave_exist });
            }

            string? dateFromStr = HttpContext.Session.GetString("date_from");
            string? dateToStr = HttpContext.Session.GetString("date_to");
            string fiscal_year = HttpContext.Session.GetString("fiscal_year") ?? "";

            // --- DayOff Validation ---
            bool isDayOff = _context.tbl_employee_dayoff.Any(d => d.emp_id == model.emp_id && d.dayoff_date >= model.leave_from_date && d.dayoff_date <= model.leave_to_date);
            if (isDayOff)
            {
                return Json(new { status = "error", message = Lang.msg_emp_leave_on_dayoff });
            }
            // --- Holiday Validation ---
            bool isHoliday = _context.tbl_setting_holidays.Any(h => h.holiday_date >= model.leave_from_date && h.holiday_date <= model.leave_to_date);
            if (isHoliday)
            {
                return Json(new { status = "error", message = Lang.msg_emp_leave_on_holiday });
            }

            var startDate = _leaveServices.GetFirstLeavePaidEndDate(model.emp_id ?? 0, fiscal_year, Convert.ToDateTime(dateFromStr), 1);
            var balance = _leaveServices.CalculateBalance(model.leave_type_id, Convert.ToInt32(model.emp_id), fiscal_year, startDate, Convert.ToDateTime(dateToStr));
            if (model.leave_in_hrs > balance)
            {
                return Json(new { status = "error", message = Lang.msg_leave_hour_exceed });
            }
            // End Balance Validation
            DateTime SubmitDate = DateTime.Now.Date;
            string UpdateMessage = Lang.msg_added_success;
            if (model.id <= 0 && mode == "add") // ADD
            {

                // --- Overlap validation (chkLeaveTakenDay equivalent) ---
                bool overlapExists = _context.tbl_employee_leave.Any(l =>
                l.emp_id == model.emp_id &&
                l.app_status != "Declined" &&
                l.app_status != "Cancelled" &&
                    (
                        (model.leave_from_date >= l.leave_from_date && model.leave_from_date <= l.leave_to_date) ||
                        (model.leave_to_date >= l.leave_from_date && model.leave_to_date <= l.leave_to_date) ||
                        (l.leave_from_date >= model.leave_from_date && l.leave_from_date <= model.leave_to_date) ||
                        (l.leave_to_date >= model.leave_from_date && l.leave_to_date <= model.leave_to_date)
                    )
                );

                if (overlapExists)
                {
                    // Half-day leave types
                    if (new[] { 2, 4, 6, 10 }.Contains(model.leave_type_id))
                    {
                        // Sum hours already taken in that date range
                        var leaveInHrsTaken = _context.tbl_employee_leave
                            .Where(l => l.emp_id == model.emp_id &&
                                        l.app_status != "Declined" &&
                                        l.app_status != "Cancelled" &&
                                        l.leave_from_date <= model.leave_to_date &&
                                        l.leave_to_date >= model.leave_from_date)
                            .Sum(l => l.leave_in_hrs);

                        var workingHrsDay = _requestServices.GetLimitHoursSetting();
                        if (leaveInHrsTaken >= workingHrsDay.normal_working_hrs)
                        {
                            return Json(new { status = "error", message = Lang.msg_leave_hour_exceed });
                        }
                    }
                    else
                    {
                        return Json(new { status = "error", message = Lang.msg_leave_already_exists });
                    }
                }

                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                var toEmpID = approver.toEmpId ?? 0;
                var toID = approver.toId ?? 0;
                var maxId = _context.tbl_employee_leave.Max(e => (int?)e.emp_leave_id) ?? 0;
                var newId = maxId + 1;
                var Efs = new tbl_employee_leave
                {
                    emp_leave_id = newId,
                    leave_type_id = model.leave_type_id,
                    submit_date = SubmitDate,
                    leave_from_date = model.leave_from_date,
                    leave_to_date = model.leave_to_date,
                    leave_desc = model.leave_desc,
                    app_status = "Pending",
                    app_by = toEmpID,
                    emp_id = model.emp_id,
                    leave_in_hrs = model.leave_in_hrs,
                };
                _context.tbl_employee_leave.Add(Efs);
                _context.SaveChanges();

                int newEmpLeaveId = Efs.emp_leave_id;

                //Send Email To Manager for Approval or decline
                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                string str_to = _employeeServices.GetEmployeeNameEmail(toEmpID);
                var orgName = await _context.tbl_pp_options
                    .Where(e => e.option_name == "op_org_name")
                    .Select(e => e.option_value)
                    .FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading
                     .Where(e => e.leave_type_id == model.leave_type_id)
                     .Select(e => e.description)
                     .FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpLeaveId}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpLeaveId}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leave'>Decline</a>";

                string subject = $"Leave submitted by {EmployeeName}";
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.leave_from_date:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.leave_to_date:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.leave_in_hrs}<br/><b>Description: </b><br/>{model.leave_desc}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = $"Dear Sir/Madam,<br/><br/>Please find my future leave request below.<br/><br/>{strMessage}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";

                // To Manager
                _emailService.SendEmail(null, str_to, subject, body, null, null, null, null, null);

                // To Administrative Emails
                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();

                string hraEmail = emails["hra"].Email; // HR ADMINISTRATOR
                string rcaEmail = emails["rca"].Email; // RECEPTIONISTs EMAIL
                _emailService.SendEmail(null, hraEmail, subject, body, null, rcaEmail, null, null, null);
            }

            //Edit Existing Leave
            else if (model.id > 0 && mode == "edit")
            {


                // --- Overlap validation (exclude current record) ---
                bool overlapExists = _context.tbl_employee_leave.Any(l =>
                    l.emp_id == model.emp_id &&
                    l.app_status != "Declined" &&
                    l.app_status != "Cancelled" &&
                    l.emp_leave_id != model.id && // exclude current record
                    (
                        (model.leave_from_date >= l.leave_from_date && model.leave_from_date <= l.leave_to_date) ||
                        (model.leave_to_date >= l.leave_from_date && model.leave_to_date <= l.leave_to_date) ||
                        (l.leave_from_date >= model.leave_from_date && l.leave_from_date <= model.leave_to_date) ||
                        (l.leave_to_date >= model.leave_from_date && l.leave_to_date <= model.leave_to_date)
                    )
                );

                if (overlapExists)
                {
                    if (new[] { 2, 4, 6, 10 }.Contains(model.leave_type_id))
                    {
                        var leaveInHrsTaken = _context.tbl_employee_leave
                            .Where(l => l.emp_id == model.emp_id &&
                                        l.app_status != "Declined" &&
                                        l.app_status != "Cancelled" &&
                                        l.emp_leave_id != model.id && // exclude current record
                                        l.leave_from_date <= model.leave_to_date &&
                                        l.leave_to_date >= model.leave_from_date)
                            .Sum(l => l.leave_in_hrs);

                        var workingHrsDay = _context.tbl_setting_limit_hrs
                            .Select(s => s.normal_working_hrs)
                            .FirstOrDefault();

                        if (leaveInHrsTaken >= workingHrsDay)
                        {
                            return Json(new { status = "error", message = Lang.msg_leave_hour_exceed });
                        }
                    }
                    else
                    {
                        return Json(new { status = "error", message = Lang.msg_leave_already_exists });
                    }
                }

                // Load existing record
                var leave = await _context.tbl_employee_leave
                    .FirstOrDefaultAsync(l => l.emp_leave_id == model.id);

                if (leave == null)
                {
                    return Json(new { status = "error", message = Lang.msg_no_record_found });
                }

                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                int toEmpID = approver.toEmpId ?? 0;
                int toID = approver.toId ?? 0;

                leave.leave_type_id = model.leave_type_id;
                leave.leave_from_date = model.leave_from_date;
                leave.leave_to_date = model.leave_to_date;
                leave.leave_desc = model.leave_desc;
                leave.app_status = "Pending";
                leave.app_by = toEmpID;
                leave.app_date = null;
                leave.leave_in_hrs = model.leave_in_hrs;

                await _context.SaveChangesAsync();

                // Send Email
                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                string str_to = _employeeServices.GetEmployeeNameEmail(toEmpID);
                var orgName = await _context.tbl_pp_options
                    .Where(e => e.option_name == "op_org_name")
                    .Select(e => e.option_value)
                    .FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading
                     .Where(e => e.leave_type_id == model.leave_type_id)
                     .Select(e => e.description)
                     .FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leave'>Decline</a>";

                string subject = $"Change in leave submitted by {EmployeeName}";
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.leave_from_date:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.leave_to_date:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.leave_in_hrs}<br/><b>Description: </b><br/>{model.leave_desc}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = $"Dear Sir/Madam,<br/><br/>Please find my changed future leave request below.<br/><br/>{strMessage}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";

                // To Manager
                _emailService.SendEmail(null, str_to, subject, body, null, null, null, null, null);

                // To Administrative Emails
                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();

                string hraEmail = emails["hra"].Email; // HR ADMINISTRATOR
                string rcaEmail = emails["rca"].Email; // RECEPTIONISTs EMAIL
                _emailService.SendEmail(null, hraEmail, subject, body, null, rcaEmail, null, null, null);
                UpdateMessage = Lang.msg_update_success;

            }
            return Json(new { status = "success", message = UpdateMessage });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }


            // Fetch fund source records for selected IDs
            var recordsToDelete = await _context.tbl_employee_leave
                .Where(r => request.SelectedIds.Contains(r.emp_leave_id.ToString()))
                .ToListAsync();

            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            // Find Leave besides Pending
            var EmployeeLeaveNotPending = await (
                from t in _context.tbl_employee_leave
                where request.SelectedIds.Contains(t.emp_leave_id.ToString()) && t.app_status != "Pending"
                select t.emp_leave_id
            )
            .ToListAsync();

            // Separate deletable vs undeletable
            var deletableRecords = recordsToDelete
                .Where(r => !EmployeeLeaveNotPending.Contains(r.emp_leave_id))
                .ToList();

            var undeletableCount = recordsToDelete.Count - deletableRecords.Count;

            // Perform deletion only on safe records
            if (deletableRecords.Any())
            {
                _context.tbl_employee_leave.RemoveRange(deletableRecords);
                await _context.SaveChangesAsync();
            }
            return Ok(new
            {
                status = "success",
                deletedCount = deletableRecords.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletableRecords.Count.ToString())
            });


        }
        public async Task<IActionResult> LeaveSummary(int empId, string fiscalYear)
        {
            string? sessionDateFrom = HttpContext.Session.GetString("date_from");
            DateTime endFiscalDate = DateTime.Parse(HttpContext.Session.GetString("date_to")!);

            var newStartFiscalDate = _leaveServices.GetFirstLeavePaidEndDate(empId, fiscalYear, Convert.ToDateTime(sessionDateFrom!), 1);
            DateTime startDate = DateTime.Parse(Convert.ToString(newStartFiscalDate));

            double workingHoursPerDay = 8;

            var summary = new LeaveBalanceListViewModel();

            summary.LeaveBalances.Add(BuildLeaveBalance("Annual Leave", 1, 16, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Sick Leave", 5, 17, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Casual Leave", 3, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Other Leave", 9, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Maternity Leave", 12, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Paternity Leave", 13, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Mourning Leave", 14, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));
            summary.LeaveBalances.Add(BuildLeaveBalance("Unpaid Study Leave", 15, null, empId, fiscalYear, startDate, endFiscalDate, workingHoursPerDay));

            return PartialView("Request/_LeaveSummary", summary);
        }
        public async Task<IActionResult> LeaveCancel(int? id, EmployeeLeaveViewModel vm)
        {
            string PageId = "10201";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            var EmployeeFS = (from leave in _context.tbl_employee_leave
                              join emp in _context.vw_Employee
                                  on leave.emp_id equals emp.emp_id
                              where leave.emp_leave_id == id
                              select new
                              {
                                  Leave = leave,
                                  EmployeeInformations = emp
                              })
                              .FirstOrDefault();

            if (EmployeeFS == null) return NotFound();
            var SettingNormalHours = _requestServices.GetLimitHoursSetting();
            int? normalWorkingHours = SettingNormalHours?.normal_working_hrs;
            EmployeeLeaveViewModel model;
            model = new EmployeeLeaveViewModel
            {
                id = EmployeeFS.Leave.emp_leave_id,
                leave_type_id = EmployeeFS.Leave.leave_type_id ?? 0,
                leave_type_name = _context.tbl_leave_heading.Where(l => l.leave_type_id == EmployeeFS.Leave.leave_type_id).Select(l => l.description).FirstOrDefault(),
                emp_id = EmployeeFS.Leave.emp_id ?? 0,
                leave_from_date = EmployeeFS.Leave.leave_from_date,
                leave_to_date = EmployeeFS.Leave.leave_to_date,
                leave_desc = EmployeeFS.Leave.leave_desc,
                leave_in_hrs = EmployeeFS.Leave.leave_in_hrs ?? 0,
                app_by_name = (EmployeeFS.Leave.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)EmployeeFS.Leave.app_by) : "",
                app_status = EmployeeFS.Leave.app_status,
                app_date = EmployeeFS.Leave.app_date,
                app_remarks = EmployeeFS.Leave.app_remarks
            };
            // Calculate leave in days safely
            if (normalWorkingHours.HasValue && normalWorkingHours.Value > 0)
            {
                model.leave_in_days = EmployeeFS.Leave.leave_in_hrs ?? 0 / normalWorkingHours.Value;
            }

            model.emp_status = EmployeeFS.EmployeeInformations.emp_status;
            ViewBag.EmployeeNameAndStatus = EmployeeFS.EmployeeInformations.employeenameWithCode + " (" + EmployeeFS.EmployeeInformations.emp_status + ")";

            return PartialView("Request/_LeaveCancel", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveCancelSave(int? id, EmployeeLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }

            // End Balance Validation
            string? mode = Request.Form["mode"];
            DateTime SubmitDate = DateTime.Now.Date;
            string UpdateMessage = Lang.msg_added_success;
            //Edit Existing Leave
            if (model.id > 0 && mode == "cancel")
            {
                var leave = await _context.tbl_employee_leave.FirstOrDefaultAsync(l => l.emp_leave_id == model.id);

                if (leave == null)
                {
                    return Json(new { status = "error", message = Lang.msg_no_record_found });
                }

                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                int toEmpID = approver.toEmpId ?? 0;
                int toID = approver.toId ?? 0;

                leave.can_submit_date = SubmitDate;
                leave.can_desc = model.can_remarks;
                leave.can_by = toEmpID;
                await _context.SaveChangesAsync();

                // Send Email
                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                var orgName = await _context.tbl_pp_options.Where(e => e.option_name == "op_org_name").Select(e => e.option_value).FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading.Where(e => e.leave_type_id == model.leave_type_id).Select(e => e.description).FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leavecancel'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leavecancel'>Decline</a>";

                string subject = $"Leave cancellation request submitted by {EmployeeName}";
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.leave_from_date:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.leave_to_date:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.leave_in_hrs}<br/><b>Description: </b><br/>{model.leave_desc}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = $"Dear Sir/Madam,<br/><br/>Please find my leave cancellation request below.<br/><br/>{strMessage}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";

                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();
                string str_to = emails["hra"].Email;
                string str_cc = emails["rca"].Email;
                // To Manager
                _emailService.SendEmail(null, str_to, subject, body, null, str_cc, null, null, null);
                UpdateMessage = Lang.msg_update_success;

            }
            return Json(new { status = "success", message = UpdateMessage });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveDiscardSave(int? id, EmployeeLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }

            // End Balance Validation
            string? mode = Request.Form["mode"];
            DateTime SubmitDate = DateTime.Now.Date;
            string UpdateMessage = Lang.msg_added_success;
            //Edit Existing Leave
            if (model.id > 0 && mode == "discard")
            {
                var leave = await _context.tbl_employee_leave.FirstOrDefaultAsync(l => l.emp_leave_id == model.id);

                if (leave == null)
                {
                    return Json(new { status = "error", message = Lang.msg_no_record_found });
                }
                if (string.IsNullOrEmpty(leave.can_by.ToString()))
                {
                    return Json(new { status = "error", message = "Cannot discard leave request" });
                }
                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                int toEmpID = approver.toEmpId ?? 0;
                int toID = approver.toId ?? 0;
                string? can_by_email = _employeeServices.GetEmployeeNameEmail((int)leave.can_by);

                leave.can_submit_date = null;
                leave.can_desc = null;
                leave.can_by = null;
                leave.can_date = null;
                leave.can_remarks = "";
                await _context.SaveChangesAsync();

                // Send Email
                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                var orgName = await _context.tbl_pp_options.Where(e => e.option_name == "op_org_name").Select(e => e.option_value).FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading.Where(e => e.leave_type_id == model.leave_type_id).Select(e => e.description).FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leavecancel'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leavecancel'>Decline</a>";

                string subject = Lang.EMAIL_EMPLOYEE_CAN_DISCARD_SUBJECT.Replace("<[EMPLOYEE-NAME-ONLY]>", EmployeeName);
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.leave_from_date:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.leave_to_date:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.leave_in_hrs}<br/><b>Description: </b><br/>{model.leave_desc}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = Lang.EMAIL_EMPLOYEE_CAN_DISCARD_MESSAGE.Replace("<[EMPLOYEE-NAME-ONLY]>", EmployeeName).Replace("<[STR-MESSAGE]>", strMessage);

                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();
                string str_hr = emails["hra"].Email;
                string str_rc = emails["rca"].Email;
                // Combine both into CC
                string combinedCc = $"{str_hr},{str_rc}";
                // To Manager
                _emailService.SendEmail(null, can_by_email, subject, body, null, combinedCc, null, null, null);
                UpdateMessage = Lang.msg_update_success;

            }
            return Json(new { status = "success", message = UpdateMessage });
        }
        private LeaveBalanceViewModel BuildLeaveBalance(string description, int fieldId, int? carryForwardId,
            int empId, string fiscalYear, DateTime startDate, DateTime endDate, double workingHoursPerDay)
        {
            double carryForward = carryForwardId.HasValue ? _leaveServices.GetMaxLeaveHours(carryForwardId.Value, empId, fiscalYear) : 0;
            double current = _leaveServices.GetMaxLeaveHours(fieldId, empId, fiscalYear);
            double total = carryForward + current;
            double taken = _leaveServices.GetLeaveTaken(fieldId, empId, startDate, endDate);
            double balance = total - taken;

            return new LeaveBalanceViewModel
            {
                Description = description,
                CarryForward = carryForward,
                Current = current,
                Total = total,
                TakenHours = taken,
                TakenDays = Math.Round(taken / workingHoursPerDay, 2),
                BalanceHours = balance,
                BalanceDays = Math.Round(balance / workingHoursPerDay, 2)
            };
        }
        #endregion

        #region EMPLOYEE Future Leave LIST,ADD,EDIT,SAVE, MASS DELETE
        [HttpGet]
        public IActionResult LeaveFuture(string StatusFilter)
        {
            string PageId = "10202";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;
            string? FiscalYearActive = HttpContext.Session.GetString("fiscal_year");
            ViewBag.EmployeeStatusFilter = GblUtilities.StatusActivePassive("AD", "A");


            var FiscalYearFuture = _context.tbl_fiscal_year.Where(c => c.fiscal_year.CompareTo(FiscalYearActive) > 0).OrderBy(c => c.fiscal_year).ToList();
            ViewBag.FiscalYearList = new SelectList(FiscalYearFuture, "fiscal_year", "fiscal_year", FiscalYearActive);



            ViewBag.StatusPAFilter = GblUtilities.ApprovalStatus();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Request/LeaveFuture", "ADD|DEL", PageId, 1);

            return PartialView("Request/_LeaveFuture");
        }
        [HttpPost]
        public async Task<IActionResult> LeaveFutureList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string FiscalYearListFilter = request.FilterValue1;
            string EmployeeStatusFilter = request.FilterValue2;

            var query = _context.vw_employee_leave_hash.Where(e => e.emp_status == EmployeeStatusFilter);
            if (!string.IsNullOrEmpty(FiscalYearListFilter))
            {
                query = query.Where(e => e.fiscal_year == FiscalYearListFilter);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(a => a.employee_name != null && a.employee_name.Contains(searchValue));
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy($"{sortColumn} {sortColumnDir}");
            }
            else
            {
                query = query.OrderByDescending(e => e.emp_leave_id);
            }
            var data = await query.ToListAsync();

            int totalRecord = data.Count();
            if (pageSize == -1) pageSize = totalRecord;

            var cData = data.Skip(skip).Take(pageSize)
                .Select(s => new EmployeeFutureLeaveViewModel
                {
                    id = s.emp_leave_id,
                    emp_leave_id = s.emp_leave_id,
                    emp_id = s.emp_id,
                    EmployeeName = s.employee_name,
                    LeaveType = s.leave_type_desc,
                    SubmitDate = s.submit_date,
                    LeaveFromDate = s.leave_from_date,
                    LeaveToDate = s.leave_to_date,
                    LeaveInHours = s.leave_in_hrs,
                    Status = s.app_status,
                    Remarks = s.app_remarks,
                    FiscalYear = s.fiscal_year
                })
                .ToList();

            return Json(new { draw, recordsFiltered = totalRecord, recordsTotal = totalRecord, data = cData });
        }
        public async Task<IActionResult> LeaveFutureAddEdit(int? id, string mode, EmployeeFutureLeaveViewModel vm)
        {
            string PageId = "10202";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.Status = GblUtilities.StatusActivePassive("AD");
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            var LeaveType = _context.tbl_leave_heading.OrderBy(c => c.description).ToList();
            ViewBag.LeaveTypeList = new SelectList(LeaveType, "leave_type_id", "description");
            // Fiscal year dropdown
            string? FiscalYearActive = HttpContext.Session.GetString("fiscal_year");
            var FiscalYearFuture = _context.tbl_fiscal_year.Where(c => c.fiscal_year.CompareTo(FiscalYearActive) > 0).OrderBy(c => c.fiscal_year).ToList();
            ViewBag.FiscalYearFutureList = new SelectList(FiscalYearFuture, "fiscal_year", "fiscal_year", FiscalYearActive);
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            var SettingNormalHours = _requestServices.GetLimitHoursSetting();
            int? normalWorkingHours = SettingNormalHours?.normal_working_hrs;
            ViewBag.NormalWorkingHours = normalWorkingHours;

            EmployeeFutureLeaveViewModel model;
            if (id <= 0 && mode == "add")
            {
                model = new EmployeeFutureLeaveViewModel();

            }

            else
            {
                // Edit
                var entity = _context.tbl_employee_leave_hash.FirstOrDefault(h => h.emp_leave_id == id);
                if (entity == null) return NotFound();

                model = new EmployeeFutureLeaveViewModel
                {
                    id = entity.emp_leave_id,
                    emp_id = entity.emp_id,
                    FutureFiscalYear = entity.fiscal_year,
                    LeaveTypeId = entity.leave_type_id,
                    LeaveFromDate = entity.leave_from_date,
                    LeaveToDate = entity.leave_to_date,
                    SubmitDate = entity.submit_date,
                    LeaveInHours = entity.leave_in_hrs,
                    Remarks = entity.app_remarks,
                    Status = entity.app_status,
                    AppBy = entity.app_by,
                    AppDate = entity.app_date
                };
                // Calculate leave in days safely
                if (normalWorkingHours.HasValue && normalWorkingHours.Value > 0 && entity.leave_in_hrs.HasValue)
                {
                    model.LeaveInDays = entity.leave_in_hrs.Value / normalWorkingHours.Value;
                }
                ViewBag.Employee = _employeeServices.GetEmployeeName((int)model.emp_id);
                model.emp_status = _employeeServices.GetEmployeeStatus((int)model.emp_id);
            }

            return PartialView("Request/_LeaveFutureAddEdit", model);
        }
        [HttpPost]
        public async Task<IActionResult> LeaveFutureSave(EmployeeFutureLeaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "error", message = Lang.msg_error_invalid });
            }

            // Balance Validation
            //var StartEndDates = _requestServices.GetFiscalStartEndDate(model.FutureFiscalYear!);
            //DateTime dateFromStr = StartEndDates.StartDate;
            //DateTime dateToStr = StartEndDates.EndDate;
            DateTime? dateFromStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(model.FutureFiscalYear!, "date_from"));
            DateTime? dateToStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(model.FutureFiscalYear!, "date_to"));

            bool isDayOff = _context.tbl_employee_dayoff.Any(d => d.emp_id == model.emp_id && d.dayoff_date >= model.LeaveFromDate && d.dayoff_date <= model.LeaveToDate);
            if (isDayOff)
            {
                return Json(new { status = "error", message = Lang.msg_emp_leave_on_dayoff });
            }
            bool isHoliday = _context.tbl_setting_holidays.Any(h => h.holiday_date >= model.LeaveFromDate && h.holiday_date <= model.LeaveToDate);
            if (isHoliday)
            {
                return Json(new { status = "error", message = Lang.msg_emp_leave_on_holiday });
            }

            var startDate = _leaveServices.GetFirstLeavePaidEndDate(Convert.ToInt32(model.emp_id), model.FutureFiscalYear!.ToString(), Convert.ToDateTime(dateFromStr), 1);
            int balance = await _context.tbl_leave_heading.Where(c => c.leave_type_id == 1).Select(c => (int?)c.max_leave_hours).FirstOrDefaultAsync() ?? 0;

            if (model.LeaveInHours > balance)
            {
                return Json(new { status = "error", message = Lang.msg_leave_hour_exceed });
            }
            // End Balance Validation
            // Add New Leave
            DateTime SubmitDate = DateTime.Now.Date;
            if (model.id <= 0) // ADD
            {

                // --- Overlap validation (chkLeaveTakenDay equivalent) ---
                bool overlapExists = _context.tbl_employee_leave_hash.Any(l =>
                l.emp_id == model.emp_id &&
                l.app_status != "Declined" &&
                l.app_status != "Cancelled" &&
                    (
                        (model.LeaveFromDate >= l.leave_from_date && model.LeaveFromDate <= l.leave_to_date) ||
                        (model.LeaveFromDate >= l.leave_from_date && model.LeaveToDate <= l.leave_to_date) ||
                        (l.leave_from_date >= model.LeaveFromDate && l.leave_from_date <= model.LeaveToDate) ||
                        (l.leave_to_date >= model.LeaveFromDate && l.leave_to_date <= model.LeaveToDate)
                    )
                );

                if (overlapExists)
                {
                    return Json(new { status = "error", message = Lang.msg_leave_already_exists });
                }

                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                var toEmpID = approver.toEmpId ?? 0;
                var toID = approver.toId ?? 0;
                var nextId = await _context.tbl_employee_leave_hash
                    .Select(c => (int?)c.emp_leave_id)
                    .MaxAsync() ?? 0;
                var Efs = new tbl_employee_leave_hash
                {
                    emp_leave_id = nextId + 1,
                    leave_type_id = model.LeaveTypeId,
                    fiscal_year = model.FutureFiscalYear,
                    submit_date = SubmitDate,
                    leave_from_date = model.LeaveFromDate,
                    leave_to_date = model.LeaveToDate,
                    leave_desc = model.Remarks,
                    app_status = "Pending",
                    app_by = toEmpID,
                    emp_id = model.emp_id,
                    leave_in_hrs = model.LeaveInHours,
                };
                _context.tbl_employee_leave_hash.Add(Efs);
                _context.SaveChanges();

                int newEmpLeaveId = Efs.emp_leave_id;

                //Send Email To Manager for Approval or decline

                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                string str_to = _employeeServices.GetEmployeeNameEmail(toEmpID);
                var orgName = await _context.tbl_pp_options
                    .Where(e => e.option_name == "op_org_name")
                    .Select(e => e.option_value)
                    .FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading
                     .Where(e => e.leave_type_id == model.LeaveTypeId)
                     .Select(e => e.description)
                     .FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpLeaveId}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leavefuture'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpLeaveId}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leavefuture'>Decline</a>";

                string subject = $"Leave submitted by {EmployeeName}";
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.LeaveFromDate:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.LeaveToDate:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.LeaveInHours}<br/><b>Description: </b><br/>{model.Remarks}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = $"Dear Sir/Madam,<br/><br/>Please find my leave request below.<br/><br/>{strMessage}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";

                // To Manager
                _emailService.SendEmail(null, str_to, subject, body, null, null, null, null, null);

                // To Administrative Emails
                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();

                string hraEmail = emails["hra"].Email; // HR ADMINISTRATOR
                string rcaEmail = emails["rca"].Email; // RECEPTIONISTs EMAIL
                _emailService.SendEmail(null, hraEmail, subject, body, null, rcaEmail, null, null, null);

                return Json(new { status = "success", message = Lang.msg_added_success });
            }

            //Edit Existing Leave
            else
            {


                // --- Overlap validation (exclude current record) ---
                bool overlapExists = _context.tbl_employee_leave_hash.Any(l =>
                    l.emp_id == model.emp_id &&
                    l.app_status != "Declined" &&
                    l.app_status != "Cancelled" &&
                    l.emp_leave_id != model.id && // exclude current record
                    (
                        (model.LeaveFromDate >= l.leave_from_date && model.LeaveFromDate <= l.leave_to_date) ||
                        (model.LeaveToDate >= l.leave_from_date && model.LeaveToDate <= l.leave_to_date) ||
                        (l.leave_from_date >= model.LeaveFromDate && l.leave_from_date <= model.LeaveToDate) ||
                        (l.leave_to_date >= model.LeaveFromDate && l.leave_to_date <= model.LeaveToDate)
                    )
                );

                if (overlapExists)
                {
                    return Json(new { status = "error", message = Lang.msg_leave_already_exists });

                }

                // Load existing record
                var leave = await _context.tbl_employee_leave_hash
                    .FirstOrDefaultAsync(l => l.emp_leave_id == model.id);

                if (leave == null)
                {
                    return Json(new { status = "notfound" });
                }

                var approver = await _approverResolver.ResolveApproverAsync(Convert.ToInt32(model.emp_id));
                var toEmpID = approver.toEmpId ?? 0;
                var toID = approver.toId ?? 0;

                // Update fields
                leave.leave_type_id = model.LeaveTypeId;
                leave.leave_from_date = model.LeaveFromDate;
                leave.leave_to_date = model.LeaveToDate;
                leave.leave_desc = model.Remarks;
                leave.app_status = "Pending";
                leave.app_by = toEmpID;
                leave.app_date = null; // equivalent to "NULL"
                leave.leave_in_hrs = model.LeaveInHours;
                leave.fiscal_year = model.FutureFiscalYear;

                await _context.SaveChangesAsync();

                // Send Email
                string EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(model.emp_id));
                string str_to = _employeeServices.GetEmployeeNameEmail(toEmpID);
                var orgName = await _context.tbl_pp_options
                    .Where(e => e.option_name == "op_org_name")
                    .Select(e => e.option_value)
                    .FirstOrDefaultAsync();
                var LeaveTypeName = await _context.tbl_leave_heading
                     .Where(e => e.leave_type_id == model.LeaveTypeId)
                     .Select(e => e.description)
                     .FirstOrDefaultAsync();

                string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leavefuture'>Approve</a> | ";
                string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=d&approval_from=email&approve_for=leavefuture'>Decline</a>";

                string subject = $"Change in leave submitted by {EmployeeName}";
                string strMessage = $"<b>Leave Type: </b>{LeaveTypeName}<br/><b>Submit Date: </b>{SubmitDate:MM/dd/yyyy}<br/><b>Leave From Date: </b>{model.LeaveFromDate:MM/dd/yyyy}<br/><b>Leave To Date: </b>{model.LeaveToDate:MM/dd/yyyy}<br/><b>Leave hours: </b>{model.LeaveInHours}<br/><b>Description: </b><br/>{model.Remarks}<br><br>Please click Approve or Decline link provided below as appropriate.<br/><br/>{approveEmailLink} {declineEmailLink}";
                string body = $"Dear Sir/Madam,<br/><br/>Please find my changed leave request below.<br/><br/>{strMessage}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";

                // To Manager
                _emailService.SendEmail(null, str_to, subject, body, null, null, null, null, null);

                // To Administrative Emails
                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();

                string hraEmail = emails["hra"].Email; // HR ADMINISTRATOR
                string rcaEmail = emails["rca"].Email; // RECEPTIONISTs EMAIL
                _emailService.SendEmail(null, hraEmail, subject, body, null, rcaEmail, null, null, null);

                return Json(new { status = "success", message = Lang.msg_update_success });

            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FutureLeaveDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }


            // Fetch fund source records for selected IDs
            var recordsToDelete = await _context.tbl_employee_leave_hash
                .Where(r => request.SelectedIds.Contains(r.emp_leave_id.ToString()))
                .ToListAsync();

            if (!recordsToDelete.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_found });
            }

            // Find Leave besides Pending
            var EmployeeLeaveNotPending = await (
                from t in _context.tbl_employee_leave_hash
                where request.SelectedIds.Contains(t.emp_leave_id.ToString()) && t.app_status != "Pending"
                select t.emp_leave_id
            ).ToListAsync();

            // Separate deletable vs undeletable
            var deletableRecords = recordsToDelete
                .Where(r => !EmployeeLeaveNotPending.Contains(r.emp_leave_id))
                .ToList();

            var undeletableCount = recordsToDelete.Count - deletableRecords.Count;

            // Perform deletion only on safe records
            if (deletableRecords.Any())
            {
                _context.tbl_employee_leave_hash.RemoveRange(deletableRecords);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                status = "success",
                deletedCount = deletableRecords.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletableRecords.Count.ToString())
            });

        }
        #endregion

        #region TRAVEL REQUEST LIST,ADD,EDIT,SAVE, MASS DELETE
        [HttpGet]
        public IActionResult Travel(string StatusFilter)
        {
            string PageId = "10204";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_employee_travel_main
                orderby a.emp_travel_id descending
                select new EmployeeTravelMainViewModel
                {
                    EmpTravelId = a.emp_travel_id,
                    emp_id = a.emp_id ?? 0
                }).ToList();

            ViewBag.EmployeeStatusFilter = GblUtilities.StatusActivePassive("AD", "A");
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.StatusPAFilter = GblUtilities.ApprovalStatus();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Request/Travel", "ADD|DEL", PageId, 1);
            return PartialView("Request/_Travel", Records);
        }
        [HttpPost]
        public async Task<IActionResult> TravelList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string FiscalYearFilter = request.FilterValue1;
            string EmployeeStatusFilter = request.FilterValue2 == "A" ? "Active" : "Inactive";
            string TravelStatusFilter = request.FilterValue3;

            DateTime? dateFromStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(FiscalYearFilter, "date_from"));
            DateTime? dateToStr = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(FiscalYearFilter, "date_to"));

            //var query = (from tbl_employee_travel_main in _context.tbl_employee_travel_main select tbl_employee_travel_main);
            var query =
                from o in _context.tbl_employee_travel_main
                join e in _context.vw_Employee on o.emp_id equals e.emp_id into empJoin
                from e in empJoin.DefaultIfEmpty()
                where o.date_from >= dateFromStr && o.date_from <= dateToStr && e.emp_status == EmployeeStatusFilter
                select new
                {
                    emp_travel_id = o.emp_travel_id,
                    id = o.emp_travel_id,
                    //EmpTravelId = o.emp_travel_id,
                    employeename = e.employeename,
                    travel_type = o.travel_type,
                    destinations = o.destinations,
                    date_from = o.date_from,
                    date_to = o.date_to.Value,
                    i_app_status = o.i_app_status,
                    app_status = o.app_status,
                    emp_id = o.emp_id,

                    // Show_cancel = "Y" if Approved and no settlement record exists
                    showBtnCan = (o.app_status == "Approved"
                       && !_context.tbl_employee_travel_settlement_main
                           .Any(s => s.emp_travel_id == o.emp_travel_id
                                  && s.emp_id == o.emp_id))
                      ? "Y"
                      : "N"
                };

            if (!string.IsNullOrEmpty(TravelStatusFilter))
            {
                if (TravelStatusFilter == "Pending")
                    query = query.Where(x => x.app_status == "Pending");
                else if (TravelStatusFilter == "Approved")
                    query = query.Where(x => x.app_status == "Approved");
                else if (TravelStatusFilter == "Cancelled")
                    query = query.Where(x => x.app_status == "Cancelled");

            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(a => a.employeename != null && a.employeename.Contains(searchValue));
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);

        }
        public IActionResult TravelAddEdit(int? id, string mode, int emp_id)
        {
            string PageId = "10204";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.TravelType = RequestServices.getTravelType();
            ViewBag.FundSource = _employeeServices.GetFundSourceActiveOnly();
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            EmployeeTravelMainViewModel model;
            int? empName = emp_id;

            // Use ec.contract_document_id here, not model
            var fundSources = _context.tbl_fund_source
                .Where(f => f.fund_status == "A") // example: only active ones
                .OrderBy(f => f.fund_source)
                .ToList();

            // Get travel particulars from DB
            var particulars = _context.tbl_travel_particulars
                .OrderBy(p => p.par_id)
                .ToList();
            if (mode == "add")
            {
                model = new EmployeeTravelMainViewModel();

                // Pre-populate expenses with default rows
                model.Expenses = particulars.Select(p => new ExpenseViewModel
                {
                    par_id = p.par_id,
                    Particular = p.particular ?? ""
                }).ToList();

                // Pre-populate fund sources with empty slots
                model.TravelFundSources = new List<TravelFundSourceViewModel>
                {
                    new TravelFundSourceViewModel(),
                    new TravelFundSourceViewModel(),
                    new TravelFundSourceViewModel(),
                    new TravelFundSourceViewModel()
                };
                var employees = _context.vw_Employee.ToList();
                ViewBag.EmployeeList = new SelectList(employees, "emp_id", "employeename");
                return PartialView("Request/_TravelAddEdit", model);
            }
            else if (mode == "edit")
            {
                var EmployeeTravelMail = _context.tbl_employee_travel_main.FirstOrDefault(h => h.emp_travel_id == id);
                if (EmployeeTravelMail == null) return NotFound();

                var subExpenses = _context.tbl_employee_travel_sub
                    .Where(s => s.emp_travel_id == id)
                    .ToList();

                var TravelFundSource = _context.tbl_employee_travel_codes
                    .Where(c => c.emp_travel_id == id)
                    .OrderBy(c => c.sn)
                    .ToList();


                model = new EmployeeTravelMainViewModel
                {
                    emp_id = EmployeeTravelMail.emp_id ?? 0,
                    EmpTravelId = EmployeeTravelMail.emp_travel_id,
                    TravelType = EmployeeTravelMail.travel_type,
                    TripPurpose = EmployeeTravelMail.trip_purpose,
                    Destinations = EmployeeTravelMail.destinations,
                    DateFrom = EmployeeTravelMail.date_from,
                    DateTo = EmployeeTravelMail.date_to,
                    Denomination = EmployeeTravelMail.denomination,
                    Remarks = EmployeeTravelMail.remarks
                };
                // Merge particulars with saved values
                model.Expenses = particulars.Select(p =>
                {
                    var existing = subExpenses.FirstOrDefault(s => s.par_id == p.par_id);
                    var Nos = existing?.nos ?? 0;
                    var rate = existing?.rate ?? 0;      // decimal

                    return new ExpenseViewModel
                    {
                        par_id = p.par_id,
                        Particular = p.particular ?? "",
                        Detail = existing?.detail ?? "",
                        Unit = existing?.unit ?? "",
                        Currency = existing?.cur_id ?? 0,   // directly use cur_id (tinyint → byte)
                        Nos = existing?.nos ?? 0,
                        Rate = existing?.rate ?? 0,
                        Amount = (decimal)(existing?.nos ?? 0f) * (existing?.rate ?? 0)
                    };
                }).ToList();

                // Calculate totals
                model.TotalNRS = model.Expenses.Where(e => e.Currency == 1).Sum(e => e.Amount ?? 0);
                model.TotalIC = model.Expenses.Where(e => e.Currency == 2).Sum(e => e.Amount ?? 0);
                model.TotalUSD = model.Expenses.Where(e => e.Currency == 3).Sum(e => e.Amount ?? 0);
                model.TotalEuro = model.Expenses.Where(e => e.Currency == 4).Sum(e => e.Amount ?? 0);
                model.TotalPound = model.Expenses.Where(e => e.Currency == 5).Sum(e => e.Amount ?? 0);
                model.TotalCHF = model.Expenses.Where(e => e.Currency == 6).Sum(e => e.Amount ?? 0);

                ViewBag.Employee = _employeeServices.GetEmployeeName((int)EmployeeTravelMail.emp_id);
                model.emp_status = _employeeServices.GetEmployeeStatus((int)EmployeeTravelMail.emp_id);

                // Pre-populate fund sources with empty slots
                model.TravelFundSources = TravelFundSource.Select(c => new TravelFundSourceViewModel
                {
                    FundId = c.fund_id,
                }).ToList();

                // If you always want 4 slots, pad with empties
                while (model.TravelFundSources.Count < 4)
                {
                    model.TravelFundSources.Add(new TravelFundSourceViewModel());
                }

                return PartialView("Request/_TravelAddEdit", model);

            }
            return PartialView("Request/_TravelAddEdit", "");
        }
        private string MapCurrency(byte? curId)
        {
            if (!curId.HasValue) return "";

            return curId.Value switch
            {
                1 => "NRS",
                2 => "IC",
                3 => "USD",
                4 => "Euro",
                5 => "Pound",
                6 => "CHF",
                _ => ""
            };
        }
        [HttpPost]
        public async Task<IActionResult> TravelSave(EmployeeTravelMainViewModel model, int EmpTravelId, string mode)
        {
            var status = "";
            string EmployeeName = _employeeServices.GetEmployeeName(model.emp_id);

            #region TO GET APPROVAL AND I APPROVAL ID
            //get line director info 
            var approver = await _approverResolver.ResolveApproverLineManagerAsync(model.emp_id);
            int lineDirectorId = approver.toEmpId ?? 0;
            string lineDirectorEmail = _employeeServices.GetEmployeeNameEmail(lineDirectorId);
            int toId = approver.toId ?? 0;

            //get supervisor info 
            var supervisorIdVal = await _requestServices.GetManagerInfoAsync(Convert.ToInt32(model.emp_id));
            int? supervisorId = supervisorIdVal.ManagerId;
            string supervisorEmail = _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(supervisorId));

            var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
            int? craId = Adminemails["cra"].Id;
            int? acrId = Adminemails["acr"].Id;
            int? dooId = Adminemails["doo"].Id;

            var travelType = model.TravelType ?? string.Empty;
            var ToInfo = await _requestServices.GetTravelValidManagerInfoAsync(model.emp_id, travelType, supervisorId, lineDirectorId, craId, acrId, dooId, string.Empty, lineDirectorEmail, supervisorEmail);
            #endregion

            if (EmpTravelId <= 0 && mode == "add")
            {
                // --- Overlap validation (chkLeaveTakenDay equivalent) ---
                bool overlapExists = _context.tbl_employee_travel_main.Any(l =>
                l.emp_id == model.emp_id &&
                l.app_status != "Declined" &&
                l.app_status != "Cancelled" &&
                    (
                        (model.DateFrom >= l.date_from && model.DateFrom <= l.date_to) ||
                        (model.DateTo >= l.date_from && model.DateTo <= l.date_to) ||
                        (l.date_from >= model.DateFrom && l.date_from <= model.DateTo) ||
                        (l.date_to >= model.DateFrom && l.date_to <= model.DateTo)
                    )
                );
                if (overlapExists)
                {
                    return Json(new { status = "success", message = Lang.msg_record_already_exist });
                }
                else
                {
                    var maxId = _context.tbl_employee_travel_main.Max(e => (int?)e.emp_travel_id) ?? 0;
                    var newId = maxId + 1;
                    DateTime submit_date = DateTime.Now;
                    var main = new tbl_employee_travel_main
                    {
                        emp_travel_id = newId,
                        emp_id = model.emp_id,
                        trip_purpose = model.TripPurpose,
                        destinations = model.Destinations,
                        date_from = model.DateFrom,
                        date_to = model.DateTo,
                        submit_date = submit_date,
                        denomination = model.Denomination,
                        remarks = model.Remarks,
                        travel_type = model.TravelType,
                        app_status = "Pending",
                        app_date = DateTime.Now,
                        i_app_by = ToInfo.IntermediateApproverId,
                        app_by = ToInfo.ApproverId,
                        i_app_by_post = ToInfo.IntermediateApproverPost,
                        app_by_post = ToInfo.ApproverPost,
                        i_app_status = (model.emp_id == craId ? "" : "Pending")
                    };
                    _context.tbl_employee_travel_main.Add(main);
                    await _context.SaveChangesAsync();
                    int newEmpTravelID = main.emp_travel_id;
                    // Insert Expenses
                    foreach (var exp in model.Expenses)
                    {
                        if (exp.Currency > 0)
                        {
                            var sub = new tbl_employee_travel_sub
                            {
                                emp_travel_id = newEmpTravelID,
                                par_id = Convert.ToByte(exp.par_id),
                                detail = exp.Detail,
                                unit = exp.Unit,
                                cur_id = exp.Currency,
                                nos = (float)(exp.Nos ?? 0d),
                                rate = Convert.ToDecimal(exp.Rate),
                                submit_date = DateTime.Now,
                                update_date = DateTime.Now
                            };
                            _context.tbl_employee_travel_sub.Add(sub);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Insert Fund Sources
                    byte sn = 1;
                    foreach (var fund in model.TravelFundSources)
                    {
                        var code = new tbl_employee_travel_codes
                        {
                            emp_travel_id = newEmpTravelID,
                            sn = sn++,
                            fund_id = fund.FundId
                        };
                        _context.tbl_employee_travel_codes.Add(code);
                        await _context.SaveChangesAsync();
                        status = "success";
                    }
                    //SENDING EMAIL SECTION
                    var EmailContent = await _requestServices.GetTravelEmailHtmlContent(Convert.ToInt32(newEmpTravelID));
                    string bodyApproveRecommend = "approve";
                    string eml_app_link_outside = "";
                    string eml_dec_link_outside = "";
                    if (ToInfo.Stage == "ad")
                    {
                        // Build email
                        //$"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                        eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpTravelID}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=a&approval_from=email&approve_for=travel'>Approve</a> | ";
                        eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpTravelID}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=d&approval_from=email&approve_for=travel'>Decline</a>";
                    }
                    else
                    {
                        // Build email
                        //$"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                        eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpTravelID}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=rr&approval_from=email&approve_for=travel'>Recommend</a> | ";
                        eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={newEmpTravelID}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=nr&approval_from=email&approve_for=travel'>Decline</a>";
                        bodyApproveRecommend = "recommend";
                    }

                    string subject = $"Travel submitted by {EmployeeName}";
                    string body = $"Dear Sir/Madam,<br/><br/>Please find my travel request below.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>" +
                    $"Please click {bodyApproveRecommend} or Decline link provided below as appropriate.<br/><br/>" +
                    eml_app_link_outside + eml_dec_link_outside;
                    _emailService.SendEmail(null, ToInfo.ApproverEmail, subject, body, null, null, null, null, null);

                    return Json(new { status = "success", message = Lang.msg_added_success, id = newEmpTravelID });
                }
            }
            else if (EmpTravelId > 0 && mode == "edit")
            {
                // --- Overlap validation (exclude current record) ---
                bool overlapExists = _context.tbl_employee_travel_main.Any(l =>
                    l.emp_id == model.emp_id &&
                    l.app_status != "Declined" &&
                    l.app_status != "Cancelled" &&
                    l.emp_travel_id != model.EmpTravelId && // exclude current record
                    (
                        (model.DateFrom >= l.date_from && model.DateFrom <= l.date_to) ||
                        (model.DateTo >= l.date_from && model.DateTo <= l.date_to) ||
                        (l.date_from >= model.DateFrom && l.date_from <= model.DateTo) ||
                        (l.date_to >= model.DateFrom && l.date_to <= model.DateTo)
                    )
                );

                if (overlapExists)
                {
                    return Json(new { status = "travelexist" });
                }
                else
                {
                    // Update main record
                    var main = await _context.tbl_employee_travel_main
                        .FirstOrDefaultAsync(m => m.emp_travel_id == EmpTravelId);
                    if (main == null) return NotFound();

                    main.trip_purpose = model.TripPurpose;
                    main.destinations = model.Destinations;
                    main.date_from = model.DateFrom;
                    main.date_to = model.DateTo;
                    main.denomination = model.Denomination;
                    main.remarks = model.Remarks;
                    main.travel_type = model.TravelType;
                    main.app_status = "Pending";
                    main.app_date = DateTime.Now;
                    main.i_app_by = ToInfo.IntermediateApproverId;
                    main.app_by = ToInfo.ApproverId;
                    main.i_app_by_post = ToInfo.IntermediateApproverPost;
                    main.app_by_post = ToInfo.ApproverPost;
                    main.i_app_status = "Pending";

                    _context.tbl_employee_travel_main.Update(main);
                    await _context.SaveChangesAsync();

                    // Remove tbl_employee_travel_sub
                    var oldExpenses = _context.tbl_employee_travel_sub
                        .Where(e => e.emp_travel_id == EmpTravelId);
                    _context.tbl_employee_travel_sub.RemoveRange(oldExpenses);
                    await _context.SaveChangesAsync();

                    // Update tbl_employee_travel_sub
                    foreach (var exp in model.Expenses)
                    {
                        var sub = new tbl_employee_travel_sub
                        {
                            emp_travel_id = EmpTravelId,
                            par_id = Convert.ToByte(exp.par_id),
                            detail = exp.Detail,
                            unit = exp.Unit,
                            //cur_id = Convert.ToByte(MapCurrency(exp.Currency)), // map string → byte
                            cur_id = exp.Currency,
                            nos = (float?)(exp.Nos ?? 0.0),                   // double
                            rate = exp.Rate,                    // decimal
                            submit_date = DateTime.Now,
                            update_date = DateTime.Now
                        };
                        _context.tbl_employee_travel_sub.Add(sub);
                    }
                    await _context.SaveChangesAsync();

                    // Remove existing tbl_employee_travel_codes
                    var oldCodes = _context.tbl_employee_travel_codes
                        .Where(c => c.emp_travel_id == EmpTravelId);
                    _context.tbl_employee_travel_codes.RemoveRange(oldCodes);
                    await _context.SaveChangesAsync();

                    // Update tbl_employee_travel_codes
                    byte sn = 1;
                    foreach (var fund in model.TravelFundSources)
                    {
                        if (fund.FundId > 0)
                        {
                            var code = new tbl_employee_travel_codes
                            {
                                emp_travel_id = EmpTravelId,
                                sn = sn++,
                                fund_id = fund.FundId
                            };
                            _context.tbl_employee_travel_codes.Add(code);
                        }
                    }
                    await _context.SaveChangesAsync();
                    status = "successupdate";
                    //SENDING EMAIL SECTION

                    var EmailContent = await _requestServices.GetTravelEmailHtmlContent(Convert.ToInt32(EmpTravelId));
                    string bodyApproveRecommend = "approve";
                    string eml_app_link_outside = "";
                    string eml_dec_link_outside = "";
                    if (ToInfo.Stage == "ad")
                    {
                        // Build email
                        //$"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                        eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={EmpTravelId}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=a&approval_from=email&approve_for=travel'>Approve</a> | ";
                        eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={EmpTravelId}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=d&approval_from=email&approve_for=travel'>Decline</a>";
                    }
                    else
                    {
                        // Build email
                        //$"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={model.id}&toid={toID}&toemp_id={toEmpID}&st=a&approval_from=email&approve_for=leave'>Approve</a> | ";
                        eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={EmpTravelId}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=rr&approval_from=email&approve_for=travel'>Recommend</a> | ";
                        eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={EmpTravelId}&toid={toId}&toemp_id={ToInfo.ApproverId}&st=nr&approval_from=email&approve_for=travel'>Decline</a>";
                        bodyApproveRecommend = "recommend";
                    }

                    string subject = $"Change in travel submitted by {EmployeeName}";
                    string body = $"Dear Sir/Madam,<br/><br/>Please find my <u>changed</u> travel request below.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>" +
                    $"Please click {bodyApproveRecommend} or Decline link provided below as appropriate.<br/><br/>" +
                    eml_app_link_outside + eml_dec_link_outside;
                    _emailService.SendEmail(null, ToInfo.ApproverEmail, subject, body, null, null, null, null, null);
                    return Json(new { status = "success", message = Lang.msg_update_success, id = EmpTravelId });
                }
            }
            return Json(new { status = status });
        }
        [HttpPost]
        public async Task<IActionResult> TravelDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_selected });
            }

            // Fetch travel records for selected IDs
            var recordsToDelete = _context.tbl_employee_travel_main.Where(r => request.SelectedIds.Contains(r.emp_travel_id.ToString())).ToList();
            if (!recordsToDelete.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_found });
            }

            // Find records that are truly deletable (app_status = Pending AND i_app_status is null/empty/Pending)
            var employeeTravelPending = await (from t in _context.tbl_employee_travel_main
                                               where request.SelectedIds.Contains(t.emp_travel_id.ToString()) && t.app_status == "Pending" && (t.i_app_status == null || t.i_app_status == "" || t.i_app_status.ToUpper() == "PENDING")
                                               select t).ToListAsync();

            // Separate deletable vs undeletable
            var deletableRecords = employeeTravelPending;
            var undeletableCount = request.SelectedIds.Count - deletableRecords.Count;
            // Perform deletion only on safe records
            if (deletableRecords.Any())
            {
                var travelIds = deletableRecords.Select(r => r.emp_travel_id).ToList();

                // Delete child records first
                var codesToDelete = _context.tbl_employee_travel_codes.Where(c => travelIds.Contains(c.emp_travel_id));
                _context.tbl_employee_travel_codes.RemoveRange(codesToDelete);

                var subsToDelete = _context.tbl_employee_travel_sub.Where(s => travelIds.Contains(s.emp_travel_id));
                _context.tbl_employee_travel_sub.RemoveRange(subsToDelete);

                _context.tbl_employee_travel_main.RemoveRange(deletableRecords);
                await _context.SaveChangesAsync();
            }
            return Json(new { status = "error", message = Lang.msg_deleted_some.Replace("[<DELETED-ROWS>]", deletableRecords.Count.ToString()).Replace("[<UN-DEL-ROWS>]", undeletableCount.ToString()) });

        }
        public async Task<IActionResult> TravelCancel(int? id, EmployeeTravelMainViewModel vm)
        {
            string PageId = "10204";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            var EmployeeFS = (from travel in _context.tbl_employee_travel_main
                              join emp in _context.vw_Employee
                                  on travel.emp_id equals emp.emp_id
                              where travel.emp_travel_id == id
                              select new
                              {
                                  Travel = travel,
                                  EmployeeInformations = emp
                              })
                              .FirstOrDefault();

            if (EmployeeFS == null) return NotFound();
            EmployeeTravelMainViewModel model;
            model = new EmployeeTravelMainViewModel
            {
                id = EmployeeFS.Travel.emp_travel_id,
                emp_id = EmployeeFS.Travel.emp_id ?? 0,
                TravelType = EmployeeFS.Travel.travel_type,
                TripPurpose = EmployeeFS.Travel.trip_purpose,
                Destinations = EmployeeFS.Travel.destinations,
                DateFrom = EmployeeFS.Travel.date_from,
                DateTo = EmployeeFS.Travel.date_to,
                i_app_status = EmployeeFS.Travel.i_app_status,
                i_app_by_name = (EmployeeFS.Travel.i_app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)EmployeeFS.Travel.i_app_by) : "",
                i_app_date = EmployeeFS.Travel.i_app_date,
                rec_remarks = EmployeeFS.Travel.rec_remarks,
                app_status = EmployeeFS.Travel.app_status,
                app_by_name = (EmployeeFS.Travel.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)EmployeeFS.Travel.app_by) : "",
                app_remarks = EmployeeFS.Travel.app_remarks,
                can_by = EmployeeFS.Travel.can_by,
                can_desc = EmployeeFS.Travel.can_desc

            };

            model.emp_status = EmployeeFS.EmployeeInformations.emp_status;
            ViewBag.EmployeeNameAndStatus = EmployeeFS.EmployeeInformations.employeenameWithCode + " (" + EmployeeFS.EmployeeInformations.emp_status + ")";

            return PartialView("Request/_TravelCancel", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TravelCancelSave(int? id, EmployeeTravelMainViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }

            string? mode = Request.Form["mode"];
            DateTime SubmitDate = DateTime.Now.Date;
            string? UpdateMessage = Lang.msg_added_success;
            if (model.id > 0 && mode == "cancel")
            {
                // Fetch the travel record
                var travel = await _context.tbl_employee_travel_main
                .FirstOrDefaultAsync(t => t.emp_travel_id == id && t.emp_id == model.emp_id);

                if (travel == null)
                {
                    return Json(new { status = "error", message = Lang.msg_no_record_found });
                }

                string? str_from = _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(travel.emp_id));


                var approverResult = await _approverResolver.ResolveApproverLineManagerAsync(Convert.ToInt32(travel.emp_id));
                int? line_dir_emp_id = approverResult.toEmpId;
                string line_dir_email = _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(line_dir_emp_id));

                int? app_by = null;
                string? str_to = "";

                var emails = await _administrationEmailService.GetAdministrationEmailsAsync();

                int? lng_do_emp_id = emails["doo"].Id;
                int? lng_cr_emp_id_alt = emails["acr"].Id;
                int? lng_cr_emp_id = emails["cra"].Id;

                int? cr_present_status = _requestServices.GetCRAbsentStatus((int)lng_cr_emp_id);
                if (cr_present_status > 0)
                    lng_cr_emp_id_alt = 0;

                string str_admin_email_do = emails["doo"].Email;
                string str_admin_email_acr = emails["acr"].Email;
                string? str_admin_email_cr = emails["cra"].Email;

                if (model.emp_id == lng_cr_emp_id)
                {
                    app_by = Convert.ToInt32(lng_do_emp_id);
                    str_to = str_admin_email_do;
                }
                else
                {
                    if (travel.travel_type?.ToUpper() == "NATIONAL")
                    {
                        app_by = Convert.ToInt32(line_dir_emp_id);
                        str_to = line_dir_email;
                    }
                    else
                    {
                        if (lng_cr_emp_id_alt > 0)
                        {
                            app_by = lng_cr_emp_id_alt;
                            str_to = str_admin_email_acr;
                        }
                        else
                        {
                            app_by = lng_cr_emp_id;
                            str_to = str_admin_email_cr;
                        }
                    }
                }

                int? toemp_id = app_by;
                travel.can_by = app_by;
                travel.can_desc = model.can_desc;
                travel.can_submit_date = DateTime.Now;
                int? toId = await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(toemp_id));

                _context.tbl_employee_travel_main.Update(travel);
                await _context.SaveChangesAsync();

                var EmailContent = await _travelApprovalService.GetTravelEmailHtmlContent(Convert.ToInt32(travel.emp_travel_id));
                string? subject = Lang.EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_SUBJECT.Replace("<[EMPLOYEE-NAME-ONLY]>", str_from);
                string? body = Lang.EMAIL_EMPLOYEE_TRAVEL_CAN_SAVE_MESSAGE.Replace("<[STR-MESSAGE]>", EmailContent).Replace("<[EMPLOYEE-NAME-ONLY]>", str_from);

                string? eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={travel.emp_travel_id}&toid={toId}&toemp_id={toemp_id}&st=a&approval_from=email&approve_for=travelcancel'>Approve</a> | ";
                string? eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.emp_id}&app_id={travel.emp_travel_id}&toid={toId}&toemp_id={toemp_id}&st=d&approval_from=email&approve_for=travelcancel'>Decline</a>";

                body = body + "<br/>" +
                $"Please click Approve or Decline link provided below as appropriate.<br/><br/>" +
                eml_app_link_outside + eml_dec_link_outside;

                if (!string.IsNullOrEmpty(str_to))
                {
                    _emailService.SendEmail(null, str_to, subject, body, null, null, null, null, null);
                }
            }
            return Json(new { status = "success", message = Lang.CANCEL_REQUEST_SAVED_AND_EMAIL_SENT });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TravelDiscardSave(int? id, EmployeeTravelMainViewModel model)
        {

            var travel = await _context.tbl_employee_travel_main
                .FirstOrDefaultAsync(t => t.emp_travel_id == id);

            if (travel == null)
            {
                return Json(new { status = "error", Message = Lang.msg_no_record_found });
            }

            string appStatus = travel.app_status ?? string.Empty;
            int? canBy = travel.can_by ?? null;

            if (appStatus.Equals("APPROVED", StringComparison.OrdinalIgnoreCase) && canBy.HasValue)
            {
                travel.can_submit_date = null;
                travel.can_desc = null;
                travel.can_by = null;
                travel.can_date = null;
                travel.can_remarks = string.Empty;

                _context.tbl_employee_travel_main.Update(travel);
                await _context.SaveChangesAsync();

                return Json(new { status = "success", message = Lang.msg_cancel_discared_success });
            }
            else
            {
                return Json(new { status = "error", message = Lang.msg_cancel_discard_fail });
            }
        }
        #endregion

        #region TIMESHEET VIEW,ADD,UPDATE,APPROVAL
        public IActionResult timesheet()
        {
            string PageId = "10203";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            //Employee List On Combo Box
            var Employee = _context.vw_Employee.OrderBy(c => c.employeename).ToList();
            ViewBag.EmployeeList = new SelectList(Employee, "emp_id", "employeename");

            ViewBag.CalYears = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.CalMonth = _settingsServices.GetMonths(DateTime.Now.Month);
            return PartialView("Request/_Timesheet");
        }
        [HttpGet]
        public async Task<JsonResult> CheckEmployeeApprover(int empid)
        {
            var approver = await _approverResolver.ResolveApproverAsync(empid);
            var toEmpID = approver.toEmpId ?? 0;
            var toID = approver.toId ?? 0;

            bool hasApprover = (toEmpID > 0 || toID > 0);

            return Json(new { hasApprover });
        }

        [HttpGet]
        public async Task<IActionResult> TimesheetGetDays(int year, int month, int empid)
        {
            var approver = await _approverResolver.ResolveApproverAsync(empid);
            var toEmpID = approver.toEmpId ?? 0;
            var toID = approver.toId ?? 0;

            //  No approver → return partial with message only
            if (toEmpID <= 0 && toID <= 0)
            {
                return PartialView("Request/_TimesheetAddEdit",
                    new wwfpp.Models.Request.TimesheetViewModel
                    {
                        HasApprover = false
                    });
            }

            string PageId = "10203";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            DateTime start = new DateTime(year, month, 1);

            DateTime end = start.AddMonths(1);
            int daysInMonth = DateTime.DaysInMonth((int)year, (int)month);

            //Get Timesheet Counter
            int maxtimeSheetCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, year, month);

            // Check for Previously Approved timesheet?
            var prevApprovedTimesheet = _context.tbl_employee_timesheet_app
                .Where(c => c.emp_year == (short)year
                         && c.emp_month == (byte)month
                         && c.emp_id == (int)empid
                         && c.submit_counter < maxtimeSheetCounter
                         && (c.app_dec == "i" || c.app_dec == "a")
                         )
                .Count();

            var dbRecords = _context.tbl_employee_timesheet_sub
                .Where(c => c.emp_year == (short)year
                         && c.emp_month == (byte)month
                         && c.emp_id == (int)empid
                         && c.submit_counter == maxtimeSheetCounter
                         )
                .ToList();

            // --- Overtime records ---
            var otRecords = _context.tbl_employee_overtime_request
                .Where(o => o.emp_id == empid
                         && o.ot_date.HasValue
                         && o.ot_date.Value.Year == year
                         && o.ot_date.Value.Month == month)
                .ToList();


            // --- Holiday dates for this month ---
            var holidayDates = _context.tbl_setting_holidays
                .Where(h => h.holiday_date.HasValue
                         && h.holiday_date.Value.Year == year
                         && h.holiday_date.Value.Month == month)
                .Select(h => h.holiday_date.HasValue ? h.holiday_date.Value.Date : (DateTime?)null)
                .ToHashSet();

            // --- Employee Day off
            var dayOffDates = _context.tbl_employee_dayoff
                .Where(h => h.dayoff_date.HasValue
                         && h.dayoff_date.Value.Year == year
                         && h.dayoff_date.Value.Month == month)
                .Select(h => h.dayoff_date.HasValue ? h.dayoff_date.Value.Date : (DateTime?)null)
                .ToHashSet();

            // Check if Employee has Leave ID 15 and if so, then need to stop Enter data on OvertimeBox
            // Build start and end of the month
            var startDateForLeave = new DateTime(year, month, 1);
            var endDateForLeave = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            // Preload restricted leave dates only for records overlapping this month
            // Step 1: Load relevant leave records into memory
            var restrictedLeaveRecords = _context.tbl_employee_leave
                .Where(l => l.app_status == "Approved"
                         && l.emp_id == empid
                         && l.leave_type_id == 15
                         && l.leave_from_date.HasValue
                         && l.leave_to_date.HasValue
                         && l.leave_from_date.Value <= endDateForLeave
                         && l.leave_to_date.Value >= startDateForLeave)
                .ToList();   // <-- materialize here

            // Step 2: Expand ranges in memory
            var restrictedLeaveDates = restrictedLeaveRecords
                .SelectMany(l =>
                    l.leave_from_date.HasValue && l.leave_to_date.HasValue
                        ? Enumerable.Range(
                            0,
                            (l.leave_to_date.Value.Date - l.leave_from_date.Value.Date).Days + 1
                          )
                          .Select(offset => l.leave_from_date.Value.Date.AddDays(offset))
                        : Enumerable.Empty<DateTime>()
                )
                .Where(d => d.Year == year && d.Month == month)
                .ToHashSet();

            string hoursAllowedForHoliday = "N";
            string hoursAllowedForWeekEnd = "N";
            var hoursAllowedInHolidayWeekend = _requestServices.GetLimitHoursSetting();
            if (hoursAllowedInHolidayWeekend != null)
            {
                hoursAllowedForHoliday = hoursAllowedInHolidayWeekend.populate_hrs_in_timesheet_for_holiday == null ? "N" : hoursAllowedInHolidayWeekend.populate_hrs_in_timesheet_for_holiday;
                hoursAllowedForWeekEnd = hoursAllowedInHolidayWeekend.populate_hrs_in_timesheet_for_weekend == null ? "N" : hoursAllowedInHolidayWeekend.populate_hrs_in_timesheet_for_weekend;
            }

            var messages = new List<wwfpp.Models.Request.TimesheetMessage>();

            var (dateStart, dateEnd) = await _requestServices.GetFiscalYearRangeAsync(start);
            dateStart ??= new DateTime(year, month, 1); // default start
            dateEnd ??= DateTime.Today;           // default end

            var fiscalFunds = (from a in _context.tbl_fund_source
                               join b in _context.tbl_employee_fund_source on a.fund_id equals b.fund_id
                               join s in _context.tbl_employee_timesheet_sub
                                   on a.fund_id equals s.fund_id into subs
                               where b.emp_id == empid &&
                                     b.start_date <= dateStart.Value &&
                                     b.end_date >= dateEnd.Value
                               select new
                               {
                                   fund_id = a.fund_id,
                                   fund_source = a.fund_source,
                                   expiry_date = (DateTime?)a.expiry_date,
                                   default_for_holiday = a.default_for_holiday,
                                   annual_hrs = b.annual_hrs,
                                   used_hours = subs
                                       .Where(x => x.emp_id == empid
                                                && x.is_active == "A"
                                                && !(x.emp_month == month))
                                       .Sum(x => (double?)(x.time_hours ?? 0) + (double?)(x.overtime_hours ?? 0)) ?? 0
                               })
                               .ToList();

            //-- Check for Timesheet hour entries.
            bool TimesheetFullyFilled = false;
            if (dbRecords.Any())
            {
                TimesheetFullyFilled = Enumerable.Range(1, daysInMonth).All(day =>
                {
                    var currentDate = new DateTime(year, month, day);
                    // Skip if holiday
                    if ((holidayDates.Contains(currentDate)) && hoursAllowedForHoliday == "N")
                        return true;

                    // Skip if Weekends
                    if ((currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday) && hoursAllowedForWeekEnd == "N")
                        return true;

                    // Skip if DayOff
                    if (dayOffDates.Contains(currentDate))
                        return true;

                    // 🔴 Skip if all funds are expired for this date
                    var activeFundsForDay = fiscalFunds
                        .Where(f => !f.expiry_date.HasValue || f.expiry_date.Value >= currentDate)
                        .ToList();

                    if (!activeFundsForDay.Any())
                        return true;


                    // Find record for this day
                    var record = dbRecords.FirstOrDefault(r =>
                    r.emp_year == year &&
                    r.emp_month == month &&
                    r.emp_day == day);

                    // Must exist and have time_hours > 0
                    return record != null && record.time_hours.HasValue && record.time_hours > 0;
                });
            }
            ViewBag.TimesheetFullyFilled = TimesheetFullyFilled;
            //-- Check for Timesheet hour entries. Ends here

            string? dateFromStr = HttpContext.Session.GetString("date_from");
            DateTime? FiscalStartDate = null;

            if (!string.IsNullOrEmpty(dateFromStr))
            {
                FiscalStartDate = DateTime.Parse(dateFromStr);
            }
            string? dateToStr = HttpContext.Session.GetString("date_to");
            DateTime? FiscalEndDate = null;

            if (!string.IsNullOrEmpty(dateToStr))
            {
                FiscalEndDate = DateTime.Parse(dateToStr);
            }

            string? EmployeeID = HttpContext.Session.GetString("emp_id");
            bool fundSourceEditable = fiscalFunds.Any(fs => fs.default_for_holiday != "1");
            double fundBalance = fiscalFunds.Sum(f => (double)(f.annual_hrs ?? 0) - f.used_hours);
            var fund = fiscalFunds.FirstOrDefault();
            DateTime? fundExpiry = fund?.expiry_date;


            // --- Criteria checks ---

            bool calendarFilled = _context.tbl_calendar_setting.Any(c => c.cal_year == year && c.cal_month == month);
            bool employeeActive = true;
            bool employeeFiscalFund = fiscalFunds.Any();
            bool timesheetSaved = dbRecords.Any();

            var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_id == empid);
            if (emp != null)
            {
                if (emp.join_date > end || (emp.end_date.HasValue && emp.end_date < start))
                {
                    employeeActive = false;
                    messages.Add(new wwfpp.Models.Request.TimesheetMessage { Text = "Employee not active for selected Month / Year.", Type = "error" });
                }
            }


            if (!calendarFilled && messages.Count == 0)
                messages.Add(new wwfpp.Models.Request.TimesheetMessage { Text = "Calendar not filled for selected month/year.", Type = "error" });

            if (!employeeFiscalFund && messages.Count == 0)
            {
                messages.Add(new wwfpp.Models.Request.TimesheetMessage { Text = "No fund source assigned for this employee.", Type = "warning" });
            }


            // --- Fiscal year logic for previous month check ---
            // --- previous time sheet status check and message ---
            var minFiscalYearStart = _context.tbl_fiscal_year.Min(fy => fy.date_from);
            bool checkAlsoPrevFyTs = false;
            var prevStatus = "";
            if (FiscalStartDate.HasValue)
            {
                DateTime sessionDateFrom = FiscalStartDate.Value;

                if (sessionDateFrom > minFiscalYearStart)
                    checkAlsoPrevFyTs = true;

                if (checkAlsoPrevFyTs || (!checkAlsoPrevFyTs && start > sessionDateFrom))
                {
                    DateTime prevPeriod = start.AddMonths(-1);
                    int empMonthPrev = prevPeriod.Month;
                    int empYearPrev = prevPeriod.Year;

                    DateTime prevStart = new DateTime(empYearPrev, empMonthPrev, 1);
                    DateTime prevEnd = prevStart.AddMonths(1).AddDays(-1);

                    if (!(emp?.end_date.HasValue == true && emp.end_date.Value < prevStart) && !(emp?.join_date > prevEnd))
                    {
                        int prevCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, empYearPrev, empMonthPrev);
                        prevStatus = await _requestServices.GetTimesheetStatusAsync(empid, empYearPrev, empMonthPrev, prevCounter);

                        if (prevStatus != "active" && messages.Count == 0)
                        {
                            string prevMsgText = _requestServices.GetPreviousTimesheetMessage(prevStatus);
                            messages.Add(new wwfpp.Models.Request.TimesheetMessage { Text = prevMsgText, Type = "warning" });
                        }

                    }
                }
            }

            //--------------------------------------
            // --- check current Timesheet status
            //--------------------------------------
            var curTimesheetStatus = await _requestServices.GetTimesheetStatusAsync(empid, year, month, maxtimeSheetCounter);
            if (messages.Count == 0)
            {
                string msgText = _requestServices.GetCurrentTimesheetMessage(curTimesheetStatus);
                string msgType = "warning";
                if (curTimesheetStatus == "active") msgType = "success";
                messages.Add(new wwfpp.Models.Request.TimesheetMessage { Text = msgText, Type = $"{msgType}" });
            }


            // --- Access check for Add/Edit button ---
            bool canEdit = false;
            bool showSaveButton = false;
            bool showSendButton = false;

            // Role check: if user has role "A" (or whichever roles you want to allow)
            if (HttpContext.Session.GetString("emp_id") == "0")
            {
                canEdit = true;
                showSaveButton = true;
                if (!showSendButton) showSendButton = true;
            }

            // ACA manager check
            if (!canEdit)
            {
                bool isAcaManager = _context.tbl_employee_administrator.Any(ea => ea.aca == Convert.ToInt32(EmployeeID));
                if (isAcaManager)
                {
                    canEdit = true;
                    showSaveButton = true;
                    if (!showSendButton) showSendButton = true;
                }
            }

            // Self-edit check
            if (!canEdit)
            {
                if (Convert.ToInt32(EmployeeID) == empid)
                {
                    canEdit = true;
                    showSaveButton = true;
                    if (!showSendButton) showSendButton = true;
                }
            }
            if (prevStatus == "" || prevStatus == "active")
            {
                if (curTimesheetStatus == "justsaved")
                    showSendButton = true;
                else if (curTimesheetStatus == "declined")
                    showSendButton = true;
                else
                    showSendButton = false;
            }
            else
                showSendButton = false;

            bool showAddEditTimeSheetButton = calendarFilled && employeeActive && employeeFiscalFund && canEdit && (prevStatus == "" || prevStatus == "active");

            // --- Build fund rows ---
            var fundRows = fiscalFunds.Select(f => new wwfpp.Models.Request.FundTimesheetRow
            {
                FundId = f.fund_id,
                FundSourceName = f.fund_source,
                FundSourceDefault = f.default_for_holiday,
                ExpiryDate = f.expiry_date,
                AnnualHours = (double)(f.annual_hrs ?? 0.0),
                UsedHours = f.used_hours,
                RemainingHours = (double)(f.annual_hrs ?? 0.0) - f.used_hours,

                Days = Enumerable.Range(1, daysInMonth).Select(d =>
                {
                    var date = new DateTime(year, month, d);
                    var record = dbRecords.FirstOrDefault(r => r.fund_id == f.fund_id && r.emp_day == d);
                    var otRecord = otRecords.FirstOrDefault(o => (o.ot_date.HasValue ? (DateTime?)o.ot_date.Value.Date : null) == date.Date);

                    return new wwfpp.Models.Request.DayData
                    {
                        Date = date,
                        Value = record?.time_hours?.ToString("0.##") ?? "0",
                        OvertimeValue = record?.overtime_hours?.ToString("0.##") ?? "0",
                        EmployeeMaxOverTimeValueInAday = otRecord?.total_hours?.ToString("0.##") ?? "0",
                        IsHoliday = holidayDates.Contains(date),
                        IsEmpDayOff = dayOffDates.Contains(date),
                        AllowOvertimeBoxToUpdateOverLeave = !restrictedLeaveDates.Contains(date),
                        OvertimeEditable = otRecord != null && otRecord.app_status == "A",
                        FundSourceEditable = true,
                        IsEditableByFund = !f.expiry_date.HasValue || date <= f.expiry_date.Value.Date,
                        CanEditOnWeekEnd = hoursAllowedForWeekEnd,
                        CanEditOnHoliday = hoursAllowedForHoliday
                    };
                }).ToList()
            }).ToList();

            //Max Normal and Overtime hours defined For Use this time
            var limits = _context.tbl_setting_limit_hrs.FirstOrDefault();

            int maxNormal = limits?.normal_working_hrs ?? 8;
            int maxOvertime = limits?.overtime_normal_working_hrs ?? 4;

            // --- Build daily totals for Stage 1 ---
            var days = Enumerable.Range(1, daysInMonth).Select(d =>
            {
                var date = new DateTime(year, month, d);

                var totalNormal = fundRows.Sum(fr =>
                    fr.Days.Where(x => x.Date.Day == d).Sum(x => double.TryParse(x.Value, out var v) ? v : 0));

                var totalOvertime = fundRows.Sum(fr =>
                    fr.Days.Where(x => x.Date.Day == d).Sum(x => double.TryParse(x.OvertimeValue, out var v) ? v : 0));

                return new wwfpp.Models.Request.DayData
                {
                    Date = date,
                    Value = totalNormal.ToString("0.##"),
                    OvertimeValue = totalOvertime.ToString("0.##"),
                    IsHoliday = holidayDates.Contains(date),
                    IsEmpDayOff = dayOffDates.Contains(date),
                    AllowOvertimeBoxToUpdateOverLeave = !restrictedLeaveDates.Contains(date),
                    IsEditableByFund = true,
                    OvertimeEditable = true,
                    FundSourceEditable = true
                };
            }).ToList();

            var viewModel = new wwfpp.Models.Request.TimesheetViewModel
            {
                FundRows = fundRows,
                Days = days,
                Messages = messages,
                ShowAddEditTimeSheetButton = showAddEditTimeSheetButton,
                ShowSaveButton = showSaveButton,
                ShowSendButton = showSendButton,
                MaxNormalHours = maxNormal,
                MaxOvertimeHours = maxOvertime,
                FundBalance = fundBalance,
                PrevApprovedTimesheetCount = prevApprovedTimesheet
            };

            return PartialView("Request/_TimesheetAddEdit", viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> SaveTimesheet(IFormCollection form, int year, int month, int empid)
        {
            int timeSheetCounter = await _requestServices.GetTimeSheetCounter(empid, year, month);
            var fiscalYearActive = HttpContext.Session.GetString("fiscal_year");

            var newEntries = new List<tbl_employee_timesheet_sub>();

            foreach (var key in form.Keys)
            {
                if (!key.StartsWith("Hours[")) continue;

                var match = System.Text.RegularExpressions.Regex.Match(key, @"Hours\[(\d+)\]\[(\d{8})\]");
                if (!match.Success) continue;

                int fundId = int.Parse(match.Groups[1].Value);
                string dateStr = match.Groups[2].Value;
                var date = DateTime.ParseExact(dateStr, "yyyyMMdd", null);

                var hours = double.TryParse(form[key], out var h) ? h : 0;
                var overtimeKey = $"OvertimeHours[{fundId}][{dateStr}]";
                var overtime = form.ContainsKey(overtimeKey) && double.TryParse(form[overtimeKey], out var o) ? o : 0;
                if (hours > 0 || overtime > 0)
                {
                    newEntries.Add(new tbl_employee_timesheet_sub
                    {
                        emp_id = empid,
                        emp_year = (short)year,
                        emp_month = (byte)month,
                        emp_day = (byte)date.Day,
                        fund_id = fundId,
                        time_hours = hours,
                        overtime_hours = overtime,
                        submit_date = DateTime.Now,
                        is_active = "N",
                        submit_counter = timeSheetCounter,
                        fiscal_year = fiscalYearActive,
                        emp_week = 0
                    });
                }
            }

            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM tbl_employee_timesheet_sub 
            WHERE emp_id = {0} AND emp_year = {1} AND emp_month = {2} AND submit_counter = {3}",
                empid, year, month, timeSheetCounter);

            if (newEntries.Any())
            {
                await _context.tbl_employee_timesheet_sub.AddRangeAsync(newEntries);
                await _context.SaveChangesAsync();
            }



            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM tbl_employee_timesheet_main 
            WHERE emp_id = {0} AND emp_year = {1} AND emp_month = {2} AND submit_counter = {3}",
                empid, year, month, timeSheetCounter);

            for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
            {
                var dateToCheck = new DateTime(year, month, day);

                var leave = await _context.tbl_employee_leave
                    .Where(l => l.app_status == "Approved"
                             && l.emp_id == empid
                             && dateToCheck >= l.leave_from_date
                             && dateToCheck <= l.leave_to_date)
                    .FirstOrDefaultAsync();

                if (leave != null)
                {
                    var entry = new tbl_employee_timesheet_main
                    {
                        emp_id = empid,
                        emp_year = (short)year,
                        emp_month = (byte)month,
                        emp_day = (byte)day,
                        leave_type_id = leave.leave_type_id,
                        submit_date = DateTime.Now,
                        submit_counter = timeSheetCounter,
                        fiscal_year = "",
                        emp_week = 0
                    };

                    _context.tbl_employee_timesheet_main.Add(entry);
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new { success = true, message = "Timesheet saved successfully." });
        }
        public async Task<IActionResult> TimesheetToBeSentForApproval(IFormCollection form, int year, int month, int empid)
        {

            int maxtimeSheetCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, year, month);
            var fiscalYearActive = HttpContext.Session.GetString("fiscal_year");


            var sql = _context.tbl_employee_timesheet_app
                .Where(a => a.emp_id == empid && a.emp_year == year && a.emp_month == month && a.submit_counter == maxtimeSheetCounter)
                .ToQueryString();

            var existingApp = await _context.tbl_employee_timesheet_app
                .Where(a => a.emp_id == empid &&
                            a.emp_year == year &&
                            a.emp_month == month &&
                            a.submit_counter == maxtimeSheetCounter)
                .ToListAsync();

            if (existingApp.Any())
            {
                _context.tbl_employee_timesheet_app.RemoveRange(existingApp);
                await _context.SaveChangesAsync();
            }

            var approver = await _approverResolver.ResolveApproverAsync(empid);
            int toEmpId = approver.toEmpId ?? 0;
            int toId = approver.toId ?? 0;

            string app_id = Guid.NewGuid().ToString();
            var appEntry = new wwfpp.Data.tbl_employee_timesheet_app
            {
                app_id = app_id,
                emp_id = empid,
                emp_year = year,
                emp_month = month,
                submit_date = DateTime.UtcNow,
                app_dec = "p",
                app_by = Convert.ToInt32(toEmpId),
                submit_counter = maxtimeSheetCounter,
                fiscal_year = fiscalYearActive,
                emp_week = 0,
                app_date = DateTime.UtcNow,
                app_remarks = null
            };

            _context.tbl_employee_timesheet_app.Add(appEntry);

            await _context.SaveChangesAsync();
            //await transaction.CommitAsync();

            var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
            string str_to = Adminemails["aca"].Email;

            string EmployeeName = _employeeServices.GetEmployeeName(empid);
            var orgName = await _context.tbl_pp_options
                .Where(e => e.option_name == "op_org_name")
                .Select(e => e.option_value)
                .FirstOrDefaultAsync();

            string monthName = new DateTime(year, month, 1).ToString("MMMM");
            string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empid}&emp_month={month}&emp_year={year}&toid={toId}&toemp_id={toEmpId}&st=a&str_counter={maxtimeSheetCounter}&app_id={app_id}&approval_from=email&approve_for=timesheet'>Approve</a> | ";
            string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empid}&emp_month={month}&emp_year={year}&toid={toId}&toemp_id={toEmpId}&st=d&str_counter={maxtimeSheetCounter}&app_id={app_id}&approval_from=email&approve_for=timesheet'>Decline</a> ";


            // Send email to manager
            string toEmail = str_to;
            string addOptText = "";
            if (maxtimeSheetCounter > 1)
                addOptText = " re-submitted ";

            string subject = $"{orgName} {addOptText} Timesheet of {EmployeeName}";
            string body = $"Dear Sir/Madam,<br/><br/>Attached file is {addOptText} Timesheet of employee {EmployeeName} of the period {monthName}, {year} fiscal year. Please click Approve or Decline link provided below as appropriate.<br/><br/> {approveEmailLink} {declineEmailLink}";


            string EmployeeTimesheetFileName = await CreateEmployeeTimesheet(year, month, empid, maxtimeSheetCounter);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "Timesheet");
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            string EmployeeTimesheet = filePath + "/" + EmployeeTimesheetFileName;

            _emailService.SendEmail(null, toEmail, subject, body, EmployeeTimesheet, null, null, null, null);

            return Json(new { success = true, message = "Timesheet has been sent for approval." });

        }
        public async Task<string> CreateEmployeeTimesheet(int year, int month, int empid, int timeSheetCounter)
        {
            string empName = _employeeServices.GetEmployeeName(empid);
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            // Go up three levels from bin\Debug\net10.0 to project root
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName;

            var uploadsFolder = Path.Combine(projectRoot, "Reports", "Employee");
            var filePath = Path.Combine(projectRoot, "Temp", "Timesheet");

            Directory.CreateDirectory(uploadsFolder);
            Directory.CreateDirectory(filePath);

            string rdlcPath = Path.Combine(uploadsFolder, "EmpTimesheet.rdlc");

            LocalReport localReport = new LocalReport();
            using (var fs = new FileStream(rdlcPath, FileMode.Open, FileAccess.Read))
            {
                localReport.LoadReportDefinition(fs);
            }

            // Add data source
            var data = _requestServices.GetEmployeeTimesheet(year, month, empid, timeSheetCounter).ToList();
            localReport.DataSources.Add(new ReportDataSource("DataSet1", data));

            // Parameters
            var parameters = new ReportParameterCollection
    {
        new ReportParameter("Employee", empName),
        new ReportParameter("Month", monthName),
        new ReportParameter("Year", year.ToString())
    };
            localReport.SetParameters(parameters);

            // Render to Excel (OpenXML)
            string mimeType, encoding, extension;
            string[] streams;
            Warning[] warnings;

            byte[] bytes = localReport.Render(
                "EXCELOPENXML", // Output format
                null,           // DeviceInfo string
                out mimeType,
                out encoding,
                out extension,
                out streams,
                out warnings
            );

            string fileName = $"EmployeeTimesheet{DateTime.Now:MMddyyyy}.xlsx";
            string fullPath = Path.Combine(filePath, fileName);
            System.IO.File.WriteAllBytes(fullPath, bytes);

            return fileName;
        }
        public async Task<IActionResult> GetPreviousApprovedTimesheets(int year, int month, int empid)
        {
            int maxtimeSheetCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, year, month);

            // Get all approved counters less than current
            var approvedCounters = _context.tbl_employee_timesheet_app
                .Where(c => c.emp_year == year
                         && c.emp_month == month
                         && c.emp_id == empid
                         && c.submit_counter < maxtimeSheetCounter
                         && (c.app_dec == "i" || c.app_dec == "a"))
                .OrderByDescending(c => c.submit_counter)
                .Select(c => c.submit_counter)
                .ToList();

            var prevApprovedTimesheets = new Dictionary<int, List<FundTimesheetRow>>();

            // Build full calendar days for the month
            var daysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(d => new DateTime(year, month, d))
                .ToList();

            foreach (var counter in approvedCounters)
            {
                var rows = _context.tbl_employee_timesheet_sub
                    .Where(c => c.emp_year == year
                             && c.emp_month == month
                             && c.emp_id == empid
                             && c.submit_counter == counter)
                    .ToList();

                var fundRows = rows
                    .GroupBy(r => r.fund_id)
                    .Select(g =>
                    {
                        var fundId = g.Key ?? 0;
                        var fundSourceName = _context.tbl_fund_source
                            .Where(f => f.fund_id == fundId)
                            .Select(f => f.fund_source)
                            .FirstOrDefault();

                        // For each day in the month, either show actual hours or 0
                        var dayDataList = daysInMonth.Select(date =>
                        {
                            var row = g.FirstOrDefault(r => r.emp_day == date.Day);
                            return new DayData
                            {
                                Date = date,
                                Value = row?.time_hours?.ToString("0.##") ?? "0",
                                OvertimeValue = row?.overtime_hours?.ToString("0.##") ?? "0",
                                IsHoliday = false,
                                IsEmpDayOff = false,
                                AllowOvertimeBoxToUpdateOverLeave = false,
                                OvertimeEditable = false,
                                FundSourceEditable = false,
                                IsEditableByFund = false
                            };
                        }).ToList();

                        return new FundTimesheetRow
                        {
                            FundId = fundId,
                            FundSourceName = fundSourceName,
                            Days = dayDataList
                        };
                    }).ToList();

                prevApprovedTimesheets[(int)counter] = fundRows;
            }

            var viewModel = new TimesheetViewModel
            {
                PrevApprovedTimesheetCount = approvedCounters.Count,
                PrevApprovedTimesheets = prevApprovedTimesheets
            };

            return PartialView("Request/_PrevApprovedTimesheetsPartial", viewModel);
        }
        #endregion

        #region EMPLOYEE Overtime LIST,ADD,EDIT,SAVE, MASS DELETE
        [HttpGet]
        public IActionResult overtime(string StatusFilter)
        {
            string PageId = "10207";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.StatusPAFilter = GblUtilities.ApprovalStatus();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Request/Overtime", "ADD|DEL", PageId, 1);
            return PartialView("Request/_overtime");
        }
        [HttpPost]
        public async Task<IActionResult> OvertimeList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string FiscalYearListFilter = request.FilterValue1;
            string EmployeeStatusFilter = request.FilterValue2 == "A" ? "Active" : "Inactive";
            string OtStatusFilter = request.FilterValue3;

            DateTime? fiscalStart = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(FiscalYearListFilter, "date_from"));
            DateTime? fiscalEnd = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(FiscalYearListFilter, "date_to"));

            if (string.IsNullOrEmpty(FiscalYearListFilter))
            {
                FiscalYearListFilter = HttpContext.Session.GetString("fiscal_year");

                fiscalStart = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
                fiscalEnd = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));
            }

            var query =
                from o in _context.tbl_employee_overtime_request
                join e in _context.vw_Employee on o.requested_by equals e.emp_id into empJoin
                from e in empJoin.DefaultIfEmpty()
                join em in _context.vw_Employee on o.emp_id equals em.emp_id into empReqJoin
                from em in empReqJoin.DefaultIfEmpty()
                where o.ot_date >= Convert.ToDateTime(fiscalStart) && o.ot_date <= Convert.ToDateTime(fiscalEnd)
                      && e.emp_status == EmployeeStatusFilter
                select new { o, e, em };
            // Apply filters
            if (!string.IsNullOrEmpty(OtStatusFilter))
            {
                if (OtStatusFilter == "Pending")
                    query = query.Where(x => x.o.req_status == "P");
                else if (OtStatusFilter == "Approved")
                    query = query.Where(x => x.o.app_status == "A");
            }
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(a => a.e.employeename != null && a.e.employeename.Contains(searchValue));
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
                query = query.AsQueryable().OrderBy($"{sortColumn} {sortColumnDir}");

            // Materialize first
            var rawData = await query.OrderByDescending(x => x.o.ot_date).ToListAsync();
            // Now project with formatting
            var data = rawData.Select(x => new EmployeeOvertimeViewModel
            {
                OtReqId = x.o.ot_req_id,
                OtDate = x.o.ot_date?.ToString("dd/MM/yyyy") ?? "",
                SubmitDate = x.o.submit_date?.ToString("dd/MM/yyyy") ?? "",
                TotalHours = x.o.total_hours ?? 0,
                RequestedBy = x.e?.employeename ?? string.Empty,
                OtDesc = !string.IsNullOrEmpty(x.o.ot_desc) && x.o.ot_desc.Length > 65
                        ? x.o.ot_desc.Substring(0, 65) + "..."
                        : (x.o.ot_desc ?? string.Empty),
                Status = $"R: {x.o.req_status ?? "-"} | S: {x.o.app_status ?? "-"}",
                AppStatus = x.o.app_status,
                EmployeeName = x.em.employeename
            });

            int totalRecord = data.Count();
            if (pageSize == -1) pageSize = totalRecord;

            var cData = data.Skip(skip).Take(pageSize).ToList();
            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }
        public async Task<IActionResult> OvertimeAddEdit(string? id, string mode, int emp_id)
        {
            string PageId = "10207";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            ViewBag.mode = mode;
            #endregion FOR END PERMISSION

            EmployeeOvertimeAddEditViewModel model;
            if (!string.IsNullOrEmpty(id) && id != "0")
            {
                // Edit mode
                var overtime = await _context.tbl_employee_overtime_request.FirstOrDefaultAsync(o => o.ot_req_id == id);

                if (overtime == null) return NotFound();

                // Get child rows separately
                var subs = await _context.tbl_employee_overtime_request_sub
                    .Where(s => s.ot_req_id == id)
                    .OrderBy(s => s.sno)
                    .ToListAsync();

                model = new EmployeeOvertimeAddEditViewModel
                {
                    OtReqId = overtime.ot_req_id,
                    id = overtime.ot_req_id,
                    emp_id = overtime.emp_id ?? 0,
                    FiscalYear = overtime.fiscal_year ?? "",
                    OtDate = overtime.ot_date,
                    TotalHours = overtime.total_hours ?? 0,
                    OtDesc = overtime.ot_desc ?? "",
                    RequestedBy = overtime.requested_by,
                    Sessions = subs
                            .Where(s => s != null)
                            .Select((s, idx) => new OvertimeSessionViewModel
                            {
                                Sno = s.sno ?? 0,
                                StartHour = ParseHour(s.start_time),
                                StartMinute = ParseMinute(s.start_time),
                                EndHour = ParseHour(s.end_time),
                                EndMinute = ParseMinute(s.end_time),
                                Hours = CalculateHours(s.start_time, s.end_time),
                                CanRemove = idx > 0 // 🔑 only allow remove for rows after the first
                            }).ToList()
                };
                ViewBag.Employee = _employeeServices.GetEmployeeName((int)overtime.emp_id);
                model.emp_status = _employeeServices.GetEmployeeStatus((int)overtime.emp_id);
            }
            else
            {
                // Add mode → only one default row, no Remove
                model = new EmployeeOvertimeAddEditViewModel
                {
                    FiscalYear = HttpContext.Session.GetString("fiscal_year") ?? "",
                    Sessions = new List<OvertimeSessionViewModel>
                    {
                        new OvertimeSessionViewModel { Sno = 1, CanRemove = false }
                    }

                };
                model.emp_id = emp_id;
            }

            string? loggedInEmpId = HttpContext.Session.GetString("emp_id");

            // Filter out the logged-in employee
            var employees = _context.vw_Employee
                .ToList();
            ViewBag.EmployeeList = new SelectList(employees, "emp_id", "employeename");

            var employeesRequestBy = _context.vw_Employee
                .Where(e => e.emp_id != Convert.ToInt32(loggedInEmpId))
                .ToList();
            ViewBag.EmployeeRequestByList = new SelectList(employeesRequestBy, "emp_id", "employeename");
            return PartialView("Request/_OvertimeAddEdit", model);
        }
        // Helper methods
        private int? ParseHour(string? time)
        {
            if (string.IsNullOrEmpty(time)) return null;
            var parts = time.Split(':');
            return int.TryParse(parts[0], out var h) ? h : null;
        }
        private int? ParseMinute(string? time)
        {
            if (string.IsNullOrEmpty(time)) return null;
            var parts = time.Split(':');
            return parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : null;
        }
        private double CalculateHours(string? start, string? end)
        {
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end)) return 0;
            var startParts = start.Split(':');
            var endParts = end.Split(':');
            if (startParts.Length < 2 || endParts.Length < 2) return 0;

            var startDt = new DateTime(2000, 1, 1, int.Parse(startParts[0]), int.Parse(startParts[1]), 0);
            var endDt = new DateTime(2000, 1, 1, int.Parse(endParts[0]), int.Parse(endParts[1]), 0);

            var diff = (endDt - startDt).TotalHours;
            return diff < 0 ? 0 : diff;
        }
        [HttpPost]
        public async Task<IActionResult> OvertimeSave(EmployeeOvertimeAddEditViewModel model, string mode)
        {
            // --- Restrict multiple overtime for same day ---
            bool overtimeExists = await _context.tbl_employee_overtime_request.AnyAsync(o =>
                o.emp_id == model.emp_id &&
                o.ot_date == model.OtDate &&
                o.req_status != "D" &&
                o.app_status != "D" &&
                (mode == "add" || o.ot_req_id != model.id));

            if (overtimeExists)
            {
                return Json(new { status = "error", message = Lang.msg_overtime_exist });
            }
            // --- Restrict defined number of hours in same week ---
            var enoughHours = _employeeOvertimeServices.CheckOvertimeSufficiency(model.emp_id, Convert.ToDateTime(model.OtDate), Convert.ToDecimal(model.TotalHours));
            if (enoughHours == "ND" || enoughHours == "NW")
            {
                var appWeekHours = _employeeOvertimeServices.GetApprovedHoursInWeek(model.emp_id, Convert.ToDateTime(model.OtDate));
                var appDayHours = _employeeOvertimeServices.GetApprovedHoursInDay(model.emp_id, Convert.ToDateTime(model.OtDate));

                return Json(new { status = "error", message = Lang.msg_overtime_not_enough_hour });
            }
            // --- Add or Edit main request ---

            string msg = Lang.msg_update_success;
            string otid = Guid.NewGuid().ToString();
            tbl_employee_overtime_request entity;
            if (mode == "edit")
            {
                otid = model.id;
                entity = await _context.tbl_employee_overtime_request
                    .FirstAsync(o => o.ot_req_id == otid);

                if (entity == null) return NotFound();

                entity.ot_date = model.OtDate;
                entity.total_hours = model.TotalHours;
                entity.ot_desc = model.OtDesc;
                entity.requested_by = model.RequestedBy;
                entity.submit_date = DateTime.Now.Date;
            }
            else
            {

                entity = new tbl_employee_overtime_request
                {
                    ot_req_id = otid,
                    emp_id = model.emp_id,
                    ot_date = model.OtDate,
                    total_hours = model.TotalHours,
                    ot_desc = model.OtDesc,
                    requested_by = model.RequestedBy,
                    req_status = "P",
                    req_date = null,
                    app_status = "P",
                    app_by = _employeeOvertimeServices.GetOTManagerId(model.emp_id),
                    app_date = null,
                    submit_date = DateTime.Now.Date,
                    is_paid = "N",
                    paid_month = 0,
                    paid_year = 0
                };
                _context.tbl_employee_overtime_request.Add(entity);
                msg = Lang.msg_added_success;

            }

            // --- Save main record ---
            await _context.SaveChangesAsync();
            await _employeeOvertimeServices.OvertimeSendEmailAsync(otid, mode);
            // --- Refresh child rows ---
            var existingSubs = _context.tbl_employee_overtime_request_sub
                .Where(s => s.ot_req_id == entity.ot_req_id);
            _context.tbl_employee_overtime_request_sub.RemoveRange(existingSubs);

            for (int i = 0; i < model.Sessions.Count; i++)
            {
                var s = model.Sessions[i];
                string startTime = $"{s.StartHour:D2}:{s.StartMinute:D2}";
                string endTime = $"{s.EndHour:D2}:{s.EndMinute:D2}";

                var sub = new tbl_employee_overtime_request_sub
                {
                    ot_req_id = entity.ot_req_id,
                    sno = (short)(i + 1),
                    start_time = startTime,
                    end_time = endTime
                };
                _context.tbl_employee_overtime_request_sub.Add(sub);
            }

            await _context.SaveChangesAsync();

            return Json(new { status = "success", message = msg });


        }
        [HttpPost]
        public async Task<IActionResult> OvertimeDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            // Fetch parent records
            var recordsToDelete = await _context.tbl_employee_overtime_request
                .Where(r => request.SelectedIds.Contains(r.ot_req_id))
                .ToListAsync();

            if (!recordsToDelete.Any())
            {
                return Json(new { status = "error", message = Lang.msg_no_record_found });
            }
            // Find records not pending
            var employeeOvertimeNotPending = await (
                from t in _context.tbl_employee_overtime_request
                where request.SelectedIds.Contains(t.ot_req_id) && t.app_status != "P"
                select t.ot_req_id
            ).ToListAsync();
            // Separate deletable vs undeletable
            var deletableRecords = recordsToDelete
                .Where(r => !employeeOvertimeNotPending.Contains(r.ot_req_id))
                .ToList();
            var undeletableCount = recordsToDelete.Count - deletableRecords.Count;

            if (deletableRecords.Any())
            {
                // Collect IDs for sub table deletion
                var idsToDelete = deletableRecords.Select(r => r.ot_req_id).ToList();

                // Delete sub records first
                var subRecords = await _context.tbl_employee_overtime_request_sub
                    .Where(s => idsToDelete.Contains(s.ot_req_id ?? ""))
                    .ToListAsync();

                if (subRecords.Any())
                {
                    _context.tbl_employee_overtime_request_sub.RemoveRange(subRecords);
                }

                // Delete parent records
                _context.tbl_employee_overtime_request.RemoveRange(deletableRecords);

                await _context.SaveChangesAsync();
            }


            return Json(new { status = "success", message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletableRecords.Count.ToString()) });


        }
        #endregion

        #region Employee Overtime Report
        public IActionResult OvertimeReport(string StatusFilter)
        {
            string PageId = "10207";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;
            ViewBag.StatusFilter = GblUtilities.StatusActivePassive("AD", "A");
            ViewBag.EmployeeFilter = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.StatusPaidApprovedFilter = GblUtilities.StatusPaidApprove();

            return PartialView("Request/_OvertimeReport", "");
        }
        public async Task<IActionResult> OvertimeReportGenerate(string ReportType, string? Status, int? Employee, DateTime startDate, DateTime endDate)
        {
            // Get organization name
            var orgName = _context.tbl_pp_options
                .FirstOrDefault(x => x.option_name == "op_org_name")?.option_value ?? "";

            var employeesQuery = from e in _context.tbl_employee
                                 join o in _context.vw_EmployeeOvertime
                                     on e.emp_id equals o.EmpId
                                 where o.app_status == "A"
                                       && e.emp_status == Status
                                       && o.OvertimeStatus == ReportType
                                 select new
                                 {
                                     e.emp_id,
                                     FullName = e.firstname + " " + e.middlename + " " + e.lastname,
                                     o.OvertimeDate,
                                     o.paid_date
                                 };

            // Apply date filter depending on ReportType
            if (ReportType == "Approved")
            {
                employeesQuery = employeesQuery
                    .Where(x => x.OvertimeDate >= startDate && x.OvertimeDate <= endDate);
            }
            else
            {
                employeesQuery = employeesQuery
                    .Where(x => x.paid_date >= startDate && x.paid_date <= endDate);
            }

            var employees = employeesQuery
                .Distinct()
                .OrderBy(x => x.FullName)
                .ToList();



            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("OvertimeReport");

                int row = 1;
                ws.Range(row, 1, row, 5).Merge();
                ws.Cell(row++, 1).Value = "Organization: " + orgName;
                ws.Range(row, 1, row, 5).Merge();
                ws.Cell(row++, 1).Value = "Overtime Report [" + ReportType + "]";
                ws.Range(row, 1, row, 5).Merge();
                ws.Cell(row++, 1).Value = $"Date Range: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                row++;

                // Header row
                ws.Cell(row, 1).Value = "Serial Number";
                ws.Cell(row, 2).Value = "Day";
                ws.Cell(row, 3).Value = "OT Date";
                ws.Cell(row, 4).Value = "Submit Date";
                ws.Cell(row, 5).Value = "Hours";
                ws.Cell(row, 6).Value = "Requested By";
                ws.Cell(row, 7).Value = "Reason/Description";
                ws.Cell(row, 8).Value = "Status";
                ws.Row(row).Style.Font.Bold = true;
                row++;

                int serial = 1;

                foreach (var emp in employees)
                {
                    // Employee header row
                    ws.Cell(row, 1).Value = emp.FullName;
                    ws.Range(row, 1, row, 8).Merge().Style.Fill.BackgroundColor = XLColor.Yellow;
                    row++;

                    // Query overtime requests for this employee
                    var requests = _context.tbl_employee_overtime_request
                        .Where(r => r.emp_id == emp.emp_id
                                    && r.app_status == "A"
                                    && r.ot_date >= startDate
                                    && r.ot_date <= endDate)
                        .OrderByDescending(r => r.ot_req_id)
                        .ToList();

                    double totalHours = 0;

                    foreach (var r in requests)
                    {
                        ws.Cell(row, 1).Value = serial++;
                        ws.Cell(row, 2).Value = r.ot_date?.ToString("dddd"); // Day name
                        ws.Cell(row, 3).Value = r.ot_date?.ToString("dd/MM/yyyy");
                        ws.Cell(row, 4).Value = r.submit_date?.ToString("dd/MM/yyyy");
                        ws.Cell(row, 5).Value = r.total_hours;
                        ws.Cell(row, 6).Value = _employeeServices.GetEmployeeName(Convert.ToInt32(r.requested_by)); // helper method
                        ws.Cell(row, 7).Value = r.ot_desc;
                        ws.Cell(row, 8).Value = "Approved"; // since we filter app_status == "A"

                        totalHours += r.total_hours ?? 0;
                        row++;
                    }

                    // Totals row
                    ws.Cell(row, 1).Value = "Total:";
                    ws.Range(row, 1, row, 4).Merge();
                    ws.Cell(row, 5).Value = totalHours;
                    ws.Range(row, 6, row, 8).Merge();
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    row++;
                }

                ws.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = Convert.ToBase64String(stream.ToArray());

                    return Json(new
                    {
                        status = "success",
                        fileName = $"employee_overtime_report_{DateTime.Now:yyyyMMdd}.xlsx",
                        fileContent = content
                    });
                }
            }
        }
        #endregion

        #region TRAVEL SETTLEMENT
        [HttpGet]
        public IActionResult TravelSettlement(int emp_id, string? fiscalYear = null, string? status = null)
        {
            var StatusFilter = status == "D" ? "Inactive" : "Active";

            ViewBag.Status = status;
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));

            return PartialView("Request/_TravelSettlementList");
        }
        [HttpPost]
        public string GetApprovedTravelList(string? fiscalYear, string? settleType)
        {
            string fnStr = "";
            string fnNotIn = "";
            string fnSettleType = "";
            int? empId = null;
            DateTime dateFrom = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
            DateTime dateTo = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

            if (fiscalYear != null)
            {
                dateFrom = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(fiscalYear, "date_from"));
                dateTo = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(fiscalYear, "date_to"));
            }

            switch (settleType)
            {
                case "T": // Saved but not requested for settlement
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='T'";
                    break;
                case "R": // Requested for settlement
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='P' AND is_for_set ='Y' ";
                    break;
                case "S": // Settled
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='A' AND is_for_set ='Y' ";
                    break;
                case "N": // Settlement not required (pending)
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='P' AND is_for_set ='N' ";
                    break;
                case "M": // Settlement not required (approved)
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='A' AND is_for_set ='N' ";
                    break;
                default:
                    fnNotIn = " NOT IN ";
                    fnSettleType = "";
                    break;
            }

            // Build SQL dynamically (same as Classic ASP)
            string fnSmt;
            if (empId == null) empId = 0;
            fnSmt = $@"
            SELECT emp_travel_id,destinations,date_from,date_to,submit_date,app_status,travel_type,app_by,app_by_post,app_date,app_remarks,can_by,can_date,can_desc,can_remarks,
            can_submit_date,denomination,m.emp_id,i_app_by,i_app_by_post,i_app_date,i_app_status,rec_remarks,m.remarks,trip_purpose 
            FROM tbl_employee_travel_main m
            INNER JOIN tbl_employee e ON m.emp_id = e.emp_id
              AND m.date_from BETWEEN '{dateFrom:yyyy-MM-dd}' AND '{dateTo:yyyy-MM-dd}'
              AND m.app_status = 'Approved'
              AND (m.can_by = '' OR m.can_by IS NULL)
              AND m.emp_travel_id {fnNotIn} (
                  SELECT DISTINCT emp_travel_id
                  FROM tbl_employee_travel_settlement_main
                  WHERE ({empId} > 0 {fnSettleType}))";
            var rsfn = _context.tbl_employee_travel_main
                               .FromSqlRaw(fnSmt)
                               .OrderBy(x => x.emp_travel_id) 
                               .ToList();
            //foreach (var row in rsfn)
            if (rsfn != null)
            {
                if (rsfn.Any())
                {
                    fnStr = @"<div class=""f-left w-100"">
                <table width=""100%"" border=""0"" cellspacing=""1"" cellpadding=""1"" bgcolor=""#B1B1B1"">
                <tr>
                <td Bgcolor=""#FFFFFF"" align=""center"" class=""normal"">
                <table width=""100%"" border=""0"" cellpadding=""4"" cellspacing=""2"" class=""normal"">
                <tr bgcolor=""#CCCCCC"">
                    <td width=""3%"" class=""title center"" height=""25"">S.N</td>
                    <td width=""20%"" class=""title"">Employee Name</td>
                    <td width=""10%"" class=""title"">Travel Type</td>
                    <td width=""15%"" class=""title"">Destination/s</td>
                    <td width=""15%"" class=""title"">Start Date</td>
                    <td width=""10%"" class=""title"">End Date</td>
                    <td width=""10%"" class=""title"">Submitted Date</td>
                    <td width=""10%"" class=""title"">Status</td>
                    <td width=""8%"" class=""title"">Action</td>
                </tr>";

                    int f = 0;
                    foreach (var row in rsfn)
                    {
                        f++;
                        int empTravelId = row.emp_travel_id;
                        string destinations = row.destinations;
                        DateTime df = Convert.ToDateTime(row.date_from);
                        DateTime dt = Convert.ToDateTime(row.date_to);
                        DateTime sd = Convert.ToDateTime(row.submit_date);
                        string appStatus = row.app_status;
                        string travelType = row.travel_type;
                        string employeenameWithCode = _employeeServices.GetEmployeeName(Convert.ToInt32(row.emp_id),"code");

                        string dateFromStr = df.ToString("dd/MM/yyyy");
                        string dateToStr = dt.ToString("dd/MM/yyyy");
                        string submitDateStr = sd.ToString("dd/MM/yyyy");

                        string printLink = "";
                        string docLink = "";
                        int travSetId = 0;

                        if (settleType == "R" || settleType == "S" || settleType == "T" || settleType == "N" || settleType == "M")
                        {
                            string fnSmt1 = $"SELECT trav_set_id FROM tbl_employee_travel_settlement_main WHERE emp_travel_id = {empTravelId} AND emp_id={empId}";
                            var rs1fn = _context.tbl_employee_travel_settlement_main
                                .FromSqlRaw(fnSmt1)
                                .FirstOrDefault();
                            if (rs1fn != null)
                                travSetId = Convert.ToInt32(rs1fn.trav_set_id);
                        }

                        if (settleType == "R" || settleType == "S")
                        {
                            printLink = $" <a href=\"javascript:PopUpW('employee_travel_settlement_print.asp?mode=print&travsettleid={travSetId}&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/print.png\" alt=\"Print\" title=\"Print\" width=\"16\" height=\"16\" border=\"0\"></a>";
                        }

                        //if (!string.IsNullOrEmpty(settleType))
                        //{
                        string fnSmtDoc = $"SELECT * FROM tbl_employee_travel_settlement_sub_doc WHERE trav_set_id = '{travSetId}'";
                        var rsfndoc = _context.tbl_employee_travel_settlement_sub_doc
                            .FromSqlRaw(fnSmtDoc)
                            .ToList();
                        if (rsfndoc.Any())
                        {
                            docLink = $" <a href=\"javascript:PopUpW('employee_travel_settlement_document.asp?mode=doc&travsettleid={travSetId}&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/doc.png\" alt=\"Document\" title=\"Document\" width=\"12\" height=\"16\" border=\"0\"></a>";
                        }
                        else
                        {
                            docLink = $" <a href=\"javascript:PopUpW('employee_travel_settlement_document.asp?mode=doc&travsettleid={travSetId}&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/no-doc.png\" alt=\"Document\" title=\"Document\" width=\"12\" height=\"16\" border=\"0\"></a>";
                        }
                        //}

                        fnStr += $@"<tr bgcolor=""#EEEEEE"" onMouseOver=""this.style.backgroundColor='#E1EAFE'"" onMouseOut=""this.style.backgroundColor='#EEEEEE'"">
                            <td align=""center"">{f}</td>
                            <td class=""normal left"" height=""20"">{employeenameWithCode}</td>
                            <td class=""normal left"" height=""20"">{travelType}</td>
                            <td class=""normal left"">{destinations}</td>
                            <td class=""normal left"">{dateFromStr}</td>
                            <td class=""normal left"">{dateToStr}</td>
                            <td class=""normal left"">{submitDateStr}</td>
                            <td class=""normal left"">{appStatus}</td>
                            <td class=""normal center"">
                        <a href=""javascript:postdata('TravelSettlementAddEdit.asp?mode=edit&emp_travel_id={empTravelId}')""><img src=""/images/edit.png"" width=""16"" height=""16"" border=""0""></a>&nbsp;
                        {printLink}
                        {docLink}
                    </td>
                </tr>";
                    }

                    fnStr += @"</table>
                </td>
                </tr>
                </table>
                </div>";
                }

                fnStr += $@"<div id=""page-box"">
                    <div id=""page-box-left""><h5>{rsfn.Count()} record(s) found!</h5></div>
                    <div id=""page-box-right""><h5>&nbsp;</h5></div>
                </div>";
            }

            return fnStr;
        }
        public IActionResult TravelSettlementPendingAllList(int? empId, string? fiscalYear, string? status = null)
        {
            //var StatusFilter = status == "D" ? "Inactive" : "Active";

            //var Employees = _context.vw_Employee
            //    .Where(e => e.emp_status == StatusFilter)
            //    .OrderBy(c => c.employeename)
            //    .ToList();

            //ViewBag.Status = status;
            //ViewBag.EmployeeList = new SelectList(Employees, "emp_id", "employeenameWithCode", empId);

            DateTime dateFrom = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
            DateTime dateTo = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));



            //ViewBag.Status = status;
            //ViewBag.EmployeeList = new SelectList(Employees, "emp_id", "employeenameWithCode", empId);
            ViewBag.DateFrom = dateFrom.ToShortDateString();
            ViewBag.DateTo = dateTo.ToShortDateString();

            //return Json(new { status = "success",DateFrom = dateFrom,DateTo = dateTo });
            return PartialView("Request/_TravelSettlementPendingList");
        }
        [HttpPost]
        public string GetApprovedTravelPendingList(string? settleType)
        {
            int empId = 0;
            string fnStr = "";
            string fnNotIn = "";
            string fnSettleType = "";

            DateTime? dateFromNew = Convert.ToDateTime(HttpContext.Session.GetString("date_from")); 
            DateTime? dateTo = DateTime.Now.Date;

            DateTime? dateFrom = dateFromNew?.AddMonths(-3).Date;

            DateTime dateUpToYesterday = DateTime.Today.AddDays(-1);

            switch (settleType)
            {
                case "T": // Saved but not requested for settlement
                    fnNotIn = " IN ";
                    fnSettleType = " AND app_status='T'";
                    break;
                default:
                    fnNotIn = " NOT IN ";
                    fnSettleType = "";
                    break;
            }
            string fnSmt;
            //if (empId == null) // "all"
            //{
            if (empId == null) empId = 0;
            fnSmt = $@"
                SELECT m.emp_travel_id,m.destinations,m.date_from,m.date_to,m.submit_date,m.app_status,m.travel_type,m.app_by,m.app_by_post,m.app_date,m.app_remarks,m.can_by,m.can_date,m.can_desc,m.can_remarks,
                m.can_submit_date,m.denomination,m.emp_id,m.i_app_by,m.i_app_by_post,m.i_app_date,m.i_app_status,m.rec_remarks,m.remarks,m.trip_purpose,e.employeenameWithCode  
                FROM tbl_employee_travel_main m
                INNER JOIN vw_Employee e ON m.emp_id = e.emp_id
                WHERE ({empId} = 0 OR m.emp_id = {empId})
                AND m.date_from BETWEEN '{dateFrom}' AND '{dateUpToYesterday}' 
                AND m.date_to <'{dateTo}'
                AND m.app_status='Approved' 
                AND (m.can_by = '' OR m.can_by IS NULL)  
                AND m.emp_travel_id {fnNotIn} (
                    SELECT DISTINCT emp_travel_id 
                    FROM tbl_employee_travel_settlement_main 
                    WHERE ({empId} = 0 OR m.emp_id = {empId}) {fnSettleType}
                ) 
                ORDER BY m.emp_travel_id DESC";

            var rsfn = _context.EmployeeTravelSettlementMainViewModel.FromSqlRaw(fnSmt).ToList(); // custom helper for raw SQL
            //foreach (var row in rsfn)
            if (rsfn != null)
            {
                if (rsfn.Any())
                {
                    fnStr = @"<div class=""f-left w-100"">
                        <table width=""100%"" border=""0"" cellspacing=""1"" cellpadding=""1"" bgcolor=""#B1B1B1"">
                        <tr>
                        <td Bgcolor=""#FFFFFF"" align=""center"" class=""normal"">
                        <table width=""100%"" border=""0"" cellpadding=""4"" cellspacing=""2"" class=""normal"">
                        <tr bgcolor=""#CCCCCC"">
                            <td width=""3%"" class=""title center"" height=""25"">S.N</td>
                            <td width=""24%"" class=""title"">Name</td>
                            <td width=""10%"" class=""title"">Travel Type</td>
                            <td width=""20%"" class=""title"">Destination/s</td>
                            <td width=""15%"" class=""title"">Start Date</td>
                            <td width=""15%"" class=""title"">End Date</td>
                            <td width=""15%"" class=""title"">Submitted Date</td>
                        </tr>";

                    int f = 0;
                    foreach (var row in rsfn)
                    {
                        f++;
                        string fn_emp_name = row.employeenameWithCode;
                        int empTravelId = row.emp_travel_id;
                        string destinations = string.IsNullOrEmpty(row.destinations)
                                        ? string.Empty
                                        : row.destinations.Length > 100
                                            ? row.destinations.Substring(0, 100) + "..."
                                            : row.destinations;
                        DateTime df = Convert.ToDateTime(row.date_from);
                        DateTime dt = Convert.ToDateTime(row.date_to);
                        DateTime sd = Convert.ToDateTime(row.submit_date);
                        string appStatus = row.app_status;
                        string travelType = row.travel_type;

                        string dateFromStr = df.ToString("dd/MM/yyyy");
                        string dateToStr = dt.ToString("dd/MM/yyyy");
                        string submitDateStr = sd.ToString("dd/MM/yyyy");

                        //string printLink = "";
                        //string docLink = "";
                        //int travSetId = 0;


                        fnStr += $@"<tr bgcolor=""#EEEEEE"" onMouseOver=""this.style.backgroundColor='#E1EAFE'"" onMouseOut=""this.style.backgroundColor='#EEEEEE'"">
                            <td align=""center"">{f}</td>
                            <td class=""normal left"">{fn_emp_name}</td>
                            <td class=""normal left"" height=""20"">{travelType}</td>
                            <td class=""normal left"">{destinations}</td>
                            <td class=""normal left"">{dateFromStr}</td>
                            <td class=""normal left"">{dateToStr}</td>
                            <td class=""normal left"">{submitDateStr}</td>
                </tr>";
                    }

                    fnStr += @"</table>
                </td>
                </tr>
                </table>
                </div>";
                }

                fnStr += $@"<div id=""page-box"">
                    <div id=""page-box-left""><h5>{rsfn.Count()} record(s) found!</h5></div>
                    <div id=""page-box-right""><h5>&nbsp;</h5></div>
                </div>";
            }
            return fnStr;
        }
        #endregion

        #region 10210 SWF LOAN
        [HttpGet]
        public IActionResult SWFLoan()
        {
            string PageId = "10210";
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
            //ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Payroll/_SWFLoan", "ADD|DEL", PageId, Records.Count);
            return PartialView("Request/_SWFLoan", Records);
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
                    return PartialView("Request/_SWFLoanAddEdit", model);

                }
            }
            else
            {
                return Json(new { success = false, message = Lang.msg_error });
            }
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


        #region PaySLIP
        public IActionResult PaySlips()
        {
            string PageId = "10209";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;
            ViewBag.StatusFilter = GblUtilities.StatusActivePassive("AD", "A");
            ViewBag.EmployeeFilter = _employeeServices.GetEmployeeActiveOnly();

            ViewBag.CalYears = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.CalMonth = _settingsServices.GetMonths(DateTime.Now.Month);

            return PartialView("Request/_PaySlips");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExportPaySlip(int empId, int year, int month)
        {
            string diffPeriod = _context.tbl_salary_differential_month.Any(x => x.sal_year == year && x.sal_month == month) ? "Y" : "N";

            var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_id == empId);
            var salary = _context.tbl_employee_salary
                .Where(s => s.emp_id == empId && s.sal_year == year && s.sal_month == month)
                .Select(s => new
                {
                    s.emp_id,
                    s.sal_year,
                    s.sal_month,
                    s.basic_salary,
                    s.act_basic_salary,
                    s.pf_a,
                    s.act_pf_a,
                    s.pf_d,
                    s.act_pf_d,
                    s.children_edu_all,
                    s.insurance,
                    s.overtime,
                    s.remote_area_all,
                    s.dashain_a,
                    s.performance_all,
                    s.others,
                    s.gratuity,
                    s.gratuity_ded,
                    s.ssf,
                    s.ssf_ded,
                    s.medical_expense_reimburse_total,
                    s.leave_encash,
                    s.tel_per_adv,
                    s.travel_prog_adv,
                    s.fd_adv,
                    s.pr_adv,
                    s.wl_adv,
                    s.adv_PF_loan,
                    s.adv_CIT_loan,
                    s.welfare_fund,
                    s.cit_d,
                    s.incometax_d,
                    s.betalibi_d
                })
                .FirstOrDefault()
                ?? _context.tbl_employee_salary_a_field
                .Where(s => s.emp_id == empId && s.sal_year == year && s.sal_month == month)
                .Select(s => new
                {
                    s.emp_id,
                    s.sal_year,
                    s.sal_month,
                    s.basic_salary,
                    s.act_basic_salary,
                    s.pf_a,
                    s.act_pf_a,
                    s.pf_d,
                    s.act_pf_d,
                    s.children_edu_all,
                    s.insurance,
                    s.overtime,
                    s.remote_area_all,
                    s.dashain_a,
                    s.performance_all,
                    s.others,
                    s.gratuity,
                    s.gratuity_ded,
                    s.ssf,
                    s.ssf_ded,
                    s.medical_expense_reimburse_total,
                    s.leave_encash,
                    s.tel_per_adv,
                    s.travel_prog_adv,
                    s.fd_adv,
                    s.pr_adv,
                    s.wl_adv,
                    s.adv_PF_loan,
                    s.adv_CIT_loan,
                    s.welfare_fund,
                    s.cit_d,
                    s.incometax_d,
                    s.betalibi_d
                })
                .FirstOrDefault();

            if (salary == null)
            {
                // Only send status flag, no message
                return Json(new { status = "nosalary" });
            }

            var gratuityInfo = _context.tbl_employee_gratuity_info.FirstOrDefault(g => g.emp_id == empId);
            var ssfInfo = _context.tbl_employee_ssf_info.FirstOrDefault(s => s.emp_id == empId);

            // === Replicate ASP calculations ===
            decimal basicSalary = salary.basic_salary ?? 0;
            decimal actBasicSalary = salary.act_basic_salary ?? 0;
            decimal pfA = salary.pf_a ?? 0;
            decimal actPfA = salary.act_pf_a ?? 0;
            decimal pfD = salary.pf_d ?? 0;
            decimal actPfD = salary.act_pf_d ?? 0;

            decimal childrenEduAll = salary.children_edu_all ?? 0;
            decimal insurance = salary.insurance ?? 0;

            var date = new DateTime(year, month, 1);
            if (date <= new DateTime(2010, 6, 1))
            {
                childrenEduAll = 0;
                insurance = 0;
            }

            decimal overtime = salary.overtime ?? 0;
            decimal remoteAreaAll = salary.remote_area_all ?? 0;
            decimal dashainA = salary.dashain_a ?? 0;
            decimal performanceAll = salary.performance_all ?? 0;
            decimal others = salary.others ?? 0;
            decimal gratuity = salary.gratuity ?? 0;
            decimal gratuityDed = salary.gratuity_ded ?? 0;
            decimal ssf = salary.ssf ?? 0;
            decimal ssfDed = salary.ssf_ded ?? 0;
            decimal medicalReimburse = salary.medical_expense_reimburse_total ?? 0;
            decimal leaveEncash = salary.leave_encash ?? 0;

            // Advances & loans
            decimal telPerAdv = salary.tel_per_adv ?? 0;
            decimal travelProgAdv = salary.travel_prog_adv ?? 0;
            decimal fdAdv = salary.fd_adv ?? 0;
            decimal prAdv = salary.pr_adv ?? 0;
            decimal wlAdv = salary.wl_adv ?? 0;
            decimal advPfLoan = salary.adv_PF_loan ?? 0;
            decimal advCitLoan = salary.adv_CIT_loan ?? 0;
            decimal welfareFund = salary.welfare_fund ?? 0;

            // Deductions
            decimal citD = salary.cit_d ?? 0;
            decimal taxD = salary.incometax_d ?? 0;
            decimal betalibiD = salary.betalibi_d ?? 0;

            // Totals
            decimal totalEarnings = basicSalary + pfA + childrenEduAll + overtime + remoteAreaAll + dashainA +
                                    performanceAll + others + insurance + medicalReimburse + leaveEncash +
                                    gratuity + ssf;

            decimal totalDeductions = pfD + citD + taxD + betalibiD + telPerAdv + travelProgAdv + fdAdv +
                                      prAdv + wlAdv + welfareFund + advPfLoan + advCitLoan +
                                      gratuityDed + ssfDed;

            decimal netInHand = totalEarnings - totalDeductions;

            // Differential logic
            decimal diffActBasicSalary = basicSalary - actBasicSalary;
            decimal diffActPfA = pfA - actPfA;
            decimal diffActPfD = pfD - actPfD;

            // === Build Excel ===
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("PaySlip");
                int row = 1;

                // Organization header
                ws.Cell(row++, 1).Value = _requestServices.GetApplicationSetting("op_org_name");
                ws.Cell(row - 1, 2).Value = _requestServices.GetApplicationSetting("op_org_addr");
                row += 2;

                // Employee details
                ws.Cell(row++, 1).Value = "Employee Code"; ws.Cell(row - 1, 2).Value = emp.emp_code;
                ws.Cell(row++, 1).Value = "Name"; ws.Cell(row - 1, 2).Value = $"{emp.firstname} {emp.middlename} {emp.lastname}";
                ws.Cell(row++, 1).Value = "Designation"; ws.Cell(row - 1, 2).Value = emp.post;
                ws.Cell(row++, 1).Value = "Month-Year"; ws.Cell(row - 1, 2).Value = $"{month}-{year}";
                ws.Cell(row++, 1).Value = "Account No"; ws.Cell(row - 1, 2).Value = emp.account_no;
                ws.Cell(row++, 1).Value = "PF No"; ws.Cell(row - 1, 2).Value = emp.pf_no;
                ws.Cell(row++, 1).Value = "CIT No"; ws.Cell(row - 1, 2).Value = emp.cit_no;
                ws.Cell(row++, 1).Value = "PAN No"; ws.Cell(row - 1, 2).Value = emp.pan_no;
                ws.Cell(row++, 1).Value = "Gratuity No"; ws.Cell(row - 1, 2).Value = gratuityInfo?.gr_number;
                ws.Cell(row++, 1).Value = "SSF No"; ws.Cell(row - 1, 2).Value = ssfInfo?.ssf_number;
                row += 2;

                // Earnings section
                ws.Cell(row++, 1).Value = "Earnings"; ws.Cell(row - 1, 2).Value = totalEarnings;
                ws.Cell(row++, 1).Value = "Basic Salary"; ws.Cell(row - 1, 2).Value = basicSalary;
                if (diffPeriod == "Y")
                {
                    ws.Cell(row++, 1).Value = "Act Basic Salary"; ws.Cell(row - 1, 2).Value = actBasicSalary;
                    ws.Cell(row++, 1).Value = "Diff Basic Salary"; ws.Cell(row - 1, 2).Value = diffActBasicSalary;
                }
                ws.Cell(row++, 1).Value = "PF Employer"; ws.Cell(row - 1, 2).Value = pfA;
                if (diffPeriod == "Y")
                {
                    ws.Cell(row++, 1).Value = "Act PF Employer"; ws.Cell(row - 1, 2).Value = actPfA;
                    ws.Cell(row++, 1).Value = "Diff PF Employer"; ws.Cell(row - 1, 2).Value = diffActPfA;
                }
                if (gratuity != 0) { ws.Cell(row++, 1).Value = "Gratuity"; ws.Cell(row - 1, 2).Value = gratuity; }
                if (ssf != 0) { ws.Cell(row++, 1).Value = "SSF"; ws.Cell(row - 1, 2).Value = ssf; }
                if (childrenEduAll != 0) { ws.Cell(row++, 1).Value = "Children Edu Allowance"; ws.Cell(row - 1, 2).Value = childrenEduAll; }
                if (insurance != 0) { ws.Cell(row++, 1).Value = "Insurance"; ws.Cell(row - 1, 2).Value = insurance; }
                //if (overtime != 0) { ws.Cell(row++, 1).Value = "Overtime"; ws.Cell(row - 1, 2).Value = overtime; }
                // Overtime handling
                if (overtime != 0)
                {
                    if (diffPeriod == "Y")
                    {
                        // Query tbl_employee_overtime for ot_diff
                        var otDiff = _context.tbl_employee_overtime
                            .Where(o => o.emp_id == empId && o.sal_year == year && o.sal_month == month)
                            .Sum(o => (decimal?)o.ot_diff) ?? 0;

                        var overtimeAmt = overtime - otDiff;

                        ws.Cell(row++, 1).Value = "Overtime";
                        ws.Cell(row - 1, 2).Value = overtimeAmt;

                        if (otDiff != 0)
                        {
                            ws.Cell(row++, 1).Value = "Differential Overtime";
                            ws.Cell(row - 1, 2).Value = otDiff;
                        }
                    }
                    else
                    {
                        ws.Cell(row++, 1).Value = "Overtime";
                        ws.Cell(row - 1, 2).Value = overtime;
                    }
                }




                if (remoteAreaAll != 0) { ws.Cell(row++, 1).Value = "Remote Area Allowance"; ws.Cell(row - 1, 2).Value = remoteAreaAll; }
                if (dashainA != 0) { ws.Cell(row++, 1).Value = "Dashain Bonus"; ws.Cell(row - 1, 2).Value = dashainA; }
                if (performanceAll != 0) { ws.Cell(row++, 1).Value = "Performance Bonus"; ws.Cell(row - 1, 2).Value = performanceAll; }
                if (medicalReimburse != 0) { ws.Cell(row++, 1).Value = "Medical Reimbursement"; ws.Cell(row - 1, 2).Value = medicalReimburse; }
                if (leaveEncash != 0) { ws.Cell(row++, 1).Value = "Leave Encashment"; ws.Cell(row - 1, 2).Value = leaveEncash; }
                if (others != 0) { ws.Cell(row++, 1).Value = "Others"; ws.Cell(row - 1, 2).Value = others; }

                row += 2;

                // Deductions section
                ws.Cell(row++, 1).Value = "Deductions"; ws.Cell(row - 1, 2).Value = totalDeductions;
                ws.Cell(row++, 1).Value = "CIT"; ws.Cell(row - 1, 2).Value = citD;
                if (gratuityDed != 0) { ws.Cell(row++, 1).Value = "Gratuity Deduction"; ws.Cell(row - 1, 2).Value = gratuityDed; }
                if (ssfDed != 0) { ws.Cell(row++, 1).Value = "SSF Deduction"; ws.Cell(row - 1, 2).Value = ssfDed; }
                if (diffPeriod == "Y")
                {
                    ws.Cell(row++, 1).Value = "Act PF Deduction"; ws.Cell(row - 1, 2).Value = actPfD;
                    ws.Cell(row++, 1).Value = "Diff PF Deduction"; ws.Cell(row - 1, 2).Value = diffActPfD;
                }
                else
                {
                    ws.Cell(row++, 1).Value = "PF Deduction"; ws.Cell(row - 1, 2).Value = pfD;
                }
                ws.Cell(row++, 1).Value = "Tax"; ws.Cell(row - 1, 2).Value = taxD;
                if (betalibiD != 0) { ws.Cell(row++, 1).Value = "Betalibi Deduction"; ws.Cell(row - 1, 2).Value = betalibiD; }
                if (telPerAdv != 0) { ws.Cell(row++, 1).Value = "Personal Advance"; ws.Cell(row - 1, 2).Value = telPerAdv; }
                if (prAdv != 0) { ws.Cell(row++, 1).Value = "Program Advance"; ws.Cell(row - 1, 2).Value = prAdv; }
                if (travelProgAdv != 0) { ws.Cell(row++, 1).Value = "Travel Advance"; ws.Cell(row - 1, 2).Value = travelProgAdv; }
                if (fdAdv != 0) { ws.Cell(row++, 1).Value = "Field Drawing"; ws.Cell(row - 1, 2).Value = fdAdv; }
                if (wlAdv != 0) { ws.Cell(row++, 1).Value = "Welfare Advance"; ws.Cell(row - 1, 2).Value = wlAdv; }
                if (advPfLoan != 0) { ws.Cell(row++, 1).Value = "PF Loan"; ws.Cell(row - 1, 2).Value = advPfLoan; }
                if (advCitLoan != 0) { ws.Cell(row++, 1).Value = "CIT Loan"; ws.Cell(row - 1, 2).Value = advCitLoan; }
                if (welfareFund != 0) { ws.Cell(row++, 1).Value = "Welfare Contribution"; ws.Cell(row - 1, 2).Value = welfareFund; }

                row += 2;

                // Net
                ws.Cell(row++, 1).Value = "Net In Hand"; ws.Cell(row - 1, 2).Value = netInHand;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var base64 = Convert.ToBase64String(stream.ToArray());
                    return Json(new { status = "success", fileContent = base64, fileName = $"PaySlip-{empId}-{year}{month}.xlsx" });
                }
            }
        }
        #endregion

    }
}
