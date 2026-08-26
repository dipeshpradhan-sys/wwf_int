using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Models.General;
using wwfpp.Models.Request;
using wwfpp.Services;
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
            IWebHostEnvironment webHostEnvironment
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
        }

        #region EMPLOYEE MEDICAL INSURANCE
        public IActionResult MedicalInsurance(int emp_id, string? status = null)
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

            if (string.IsNullOrEmpty(FiscalYearListFilter)) FiscalYearListFilter = HttpContext.Session.GetString("FiscalYear") ?? "";

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
            var fiscalYear = HttpContext.Session.GetString("FiscalYear");

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

                var Efs = new tbl_employee_leave
                {
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
            string? sessionDateFrom = HttpContext.Session.GetString("DateFrom");
            DateTime endFiscalDate = DateTime.Parse(HttpContext.Session.GetString("DateTo")!);

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

        public IActionResult Index()
        {
            return View();
        }
    }
}
