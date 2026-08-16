using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Admin;
using wwfpp.Models.Employee;
using wwfpp.Models.General;
using wwfpp.Models.Personnel;
using wwfpp.Models.Request;
using wwfpp.Services;
using static GblUtilities;

namespace wwfpp.Controllers
{
    public class PersonnelController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly EmailService _emailService;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly EmployeeServices _employeeServices;
        private readonly SettingsServices _settingsServices;
        private readonly AccountServices _accountServices;
        private readonly LeaveServices _leaveServices;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PersonnelController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            EmailService emailService,
            GlobalOptionServices globalOptionServices,
            EmployeeServices employeeServices,
            SettingsServices settingsServices,
            AccountServices accountServices,
            LeaveServices leaveServices,
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
            _leaveServices = leaveServices;
            _webHostEnvironment = webHostEnvironment;
        }
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        #region EMPLOYEE LEAVE 10801 ==> LIST, ADD, EDIT, SAVE
        [HttpGet]
        public IActionResult Leave()
        {
            string PageId = "10801";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;

            //string UnitFilter = _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            string FiscalYearActive = HttpContext.Session.GetString("fiscal_year") ?? "";
            string SelectedFiscalYear = FiscalYearActive;
            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalYearActive, "normal_working_hrs"));
            string start_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_from");
            string end_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_to");
            string emp_status = _employeeServices.GetEmployeeStatus(emp_id);
            string employee = _employeeServices.GetEmployeeName(emp_id);
            DateTime startFiscalDate = DateTime.TryParse(start_fiscal_date, out DateTime DF) ? DF : DateTime.MinValue; ;
            DateTime endFiscalDate = DateTime.TryParse(end_fiscal_date, out DateTime DE) ? DE : DateTime.MinValue; ;
            /**
             * As we need to allow requesting leave although there are 
             * pending leaves we are just setting all the time as no 
             * pending leave here instead of updating below'
             * is_pending_leave = lvrmgr.isCurrentYearPendingLeave(emp_id)
             */
            string is_pending_leave = "N";
            string IsNoSupervisor = _employeeServices.IsDefinedManager(emp_id);
            string str_leave = "";
            string stop_leave_apply = _globalOptionServices.OptionServices["op_deny_from_requesting_leave"];
            var Records = (from lve in _context.tbl_employee_leave
                           join hed in _context.tbl_leave_heading
                           on lve.leave_type_id equals hed.leave_type_id
                           where lve.emp_id == emp_id &&
                           lve.leave_from_date >= startFiscalDate && lve.leave_to_date <= endFiscalDate
                           orderby lve.leave_from_date descending, hed.description ascending
                           select new LeaveViewModel
                           {
                               id = lve.emp_leave_id,
                               leave_type_id = lve.leave_type_id,
                               description = hed.description,
                               submit_date = lve.submit_date,
                               leave_from_date = lve.leave_from_date,
                               leave_to_date = lve.leave_to_date,
                               leave_desc = lve.leave_desc,
                               app_status = lve.app_status,
                               app_by = lve.app_by,
                               app_date = lve.app_date,
                               emp_id = lve.emp_id,
                               leave_in_hrs = lve.leave_in_hrs ?? 0,
                               leave_in_days = Math.Round((double)lve.leave_in_hrs / workingHoursDays, 2),
                               app_remarks = lve.app_remarks
                           }).ToList();
            /** BUTTON SHOW CASE
                - employee should be active (emp_status == "A")
                - current fiscal year and if (FiscalYearActive == model.fiscal_year) ???
                - Supervisor defined and (IsNoSupervisor == "true"
                - is_pending_leave == "N" and 
                - stop_leave_apply == "N" 'FY ko end ma leave lina bata rokna lai yo setting banaunu parchha
                - Leave Add New permission provided -> This check after button assignment
            */
            if (emp_status == "A" && FiscalYearActive == SelectedFiscalYear && IsNoSupervisor == "true" &&
               stop_leave_apply == "N")
            {
                str_leave = "|ADD";
            }
            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(FiscalYearActive);
            ViewBag.EmployeeStatus = emp_status == "A" ? "Active" : "Inactive";
            ViewBag.Employee = employee;
            ViewBag.IsNoSupervisor = IsNoSupervisor;
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Personnel/Leave", "BALANCE" + str_leave, PageId, 1);
            return PartialView("Personnel/_Leave", Records);
        }
        [HttpPost]
        public async Task<IActionResult> LeaveList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string SelectedFiscalYear = request.FilterValue1;
            string FiscalYearActive = HttpContext.Session.GetString("fiscal_year") ?? "";
            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalYearActive, "normal_working_hrs"));
            string start_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_from");
            string end_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_to");
            string emp_status = _employeeServices.GetEmployeeStatus(emp_id);
            DateTime startFiscalDate = DateTime.TryParse(start_fiscal_date, out DateTime DF) ? DF : DateTime.MinValue; ;
            DateTime endFiscalDate = DateTime.TryParse(end_fiscal_date, out DateTime DE) ? DE : DateTime.MinValue; ;

            var query = from lve in _context.tbl_employee_leave
                        join hed in _context.tbl_leave_heading
                        on lve.leave_type_id equals hed.leave_type_id
                        where lve.emp_id == emp_id &&
                        lve.leave_from_date >= startFiscalDate && lve.leave_to_date <= endFiscalDate
                        orderby lve.leave_from_date descending, hed.description ascending
                        select new LeaveViewModel
                        {
                            id = lve.emp_leave_id,
                            leave_type_id = lve.leave_type_id,
                            description = hed.description,
                            submit_date = lve.submit_date,
                            leave_from_date = lve.leave_from_date,
                            leave_to_date = lve.leave_to_date,
                            leave_desc = lve.leave_desc,
                            app_status = lve.app_status,
                            app_by = lve.app_by,
                            app_date = lve.app_date,
                            emp_id = lve.emp_id,
                            leave_in_hrs = lve.leave_in_hrs ?? 0,
                            leave_in_days = Math.Round((double)lve.leave_in_hrs / workingHoursDays, 2),
                            app_remarks = lve.app_remarks,
                            showBtnCan = (SelectedFiscalYear == FiscalYearActive && lve.app_status == "Approved" && emp_status == "A") ? "Y" : "N"
                        };

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy($"{sortColumn} {sortColumnDir}");
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                (a.description != null && a.description.Contains(searchValue)) ||
                (a.leave_from_date != null && a.leave_from_date.ToString().Contains(searchValue)) ||
                (a.leave_to_date != null && a.leave_to_date.ToString().Contains(searchValue)) ||
                (a.submit_date != null && a.submit_date.ToString().Contains(searchValue))
                );
            }
            int totalRecord = query.Count();
            if (pageSize == -1) pageSize = totalRecord;
            var cData = query.Skip(skip).Take(pageSize).ToList();
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        [HttpGet]
        public async Task<IActionResult> LeaveBalance(int empId, string mode)
        {
            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int pempId) ? pempId : 0;
            string fiscalYear = HttpContext.Session.GetString("fiscal_year");
            DateTime sessionDateFrom = DateTime.Parse(HttpContext.Session.GetString("date_from"));
            DateTime endFiscalDate = DateTime.Parse(HttpContext.Session.GetString("date_to")!);

            DateTime newStartFiscalDate = _leaveServices.GetFirstLeavePaidEndDate(emp_id, fiscalYear, sessionDateFrom!, 1);
            DateTime startDate = newStartFiscalDate;

            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs"));

            var summary = new LeaveBalanceListViewModel();

            summary.LeaveBalances.Add(BuildLeaveBalance("Annual Leave", 1, 16, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Sick Leave", 5, 17, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Casual Leave", 3, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Other Leave", 9, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Maternity Leave", 12, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Paternity Leave", 13, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Mourning Leave", 14, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));
            summary.LeaveBalances.Add(BuildLeaveBalance("Unpaid Study Leave", 15, null, emp_id, fiscalYear, startDate, endFiscalDate, workingHoursDays));

            return PartialView("Personnel/_LeaveBalance", summary);
        }
        private LeaveBalanceViewModel BuildLeaveBalance(string description, int fieldId, int? carryForwardId,
            int empId, string fiscalYear, DateTime startDate, DateTime endDate, double workingHoursDays)
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
                TakenDays = Math.Round(taken / workingHoursDays, 2),
                BalanceHours = balance,
                BalanceDays = Math.Round(balance / workingHoursDays, 2)
            };
        }
        [HttpGet]
        public IActionResult LeaveAddEdit(int? id, string mode, string SelFiscalYear)
        {
            string PageId = "10801";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            string FiscalYear = SelFiscalYear;
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalYear, "normal_working_hrs"));
            string fiscal_year_abb = _settingsServices.GetFiscalYearValue(FiscalYear, "fiscal_year_abb");
            string startFiscalDate = _settingsServices.GetFiscalYearValue(FiscalYear, "date_from");
            string endFiscaDate = _settingsServices.GetFiscalYearValue(FiscalYear, "date_to");
            string employee = _employeeServices.GetEmployeeName(emp_id);
            string emp_status = _employeeServices.GetEmployeeStatus(emp_id);

            DateTime start_fiscal_date = DateTime.TryParse(startFiscalDate, out DateTime DS) ? DS : DateTime.MinValue;
            DateTime end_fiscal_date = DateTime.TryParse(endFiscaDate, out DateTime DE) ? DE : DateTime.MinValue;

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.LeaveTypeList = _leaveServices.GetLeaveType(0, 0);

            LeaveViewModel model;
            model = new LeaveViewModel();
            if (id <= 0 && mode == "add")
            {
                model = new LeaveViewModel
                {
                    id = 0,
                    leave_type_id = 0,
                    description = "",
                    submit_date = null,
                    leave_from_date = null,
                    leave_to_date = null,
                    leave_desc = "",
                    app_status = "",
                    app_by = null,
                    app_by_name = "",
                    app_date = null,
                    app_remarks = "",
                    emp_id = emp_id,
                    leave_in_hrs = 0,
                    leave_in_days = 0,
                    can_submit_date = null,
                    can_desc = "",
                    can_by = null,
                    can_by_name = "",
                    can_date = null,
                    can_remarks = "",
                    employee = employee,
                    emp_status = emp_status,
                    fiscal_year = FiscalYear,
                    fiscal_year_abb = fiscal_year_abb,
                    start_fiscal_date = start_fiscal_date,
                    end_fiscal_date = end_fiscal_date,
                    workingHoursDays = workingHoursDays
                };
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Personnel/_LeaveAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id < 1 || id == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                var smt = (from lve in _context.tbl_employee_leave
                           join hed in _context.tbl_leave_heading
                           on lve.leave_type_id equals hed.leave_type_id
                           join emp in _context.tbl_employee
                           on lve.emp_id equals emp.emp_id
                           where lve.emp_leave_id == id && lve.emp_id == emp_id
                           select new
                           {
                               lve.emp_leave_id,
                               lve.leave_type_id,
                               hed.description,
                               lve.submit_date,
                               lve.leave_from_date,
                               lve.leave_to_date,
                               lve.leave_desc,
                               lve.app_status,
                               lve.app_by,
                               lve.app_date,
                               lve.app_remarks,
                               lve.emp_id,
                               lve.leave_in_hrs,
                               leave_in_days = Math.Round((double)lve.leave_in_hrs / workingHoursDays, 2),
                               lve.can_submit_date,
                               lve.can_desc,
                               lve.can_by,
                               lve.can_date,
                               lve.can_remarks,
                               employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                               emp.emp_status
                           }).FirstOrDefault();

                if (smt == null) return NotFound();
                model = new LeaveViewModel
                {
                    id = smt.emp_leave_id,
                    leave_type_id = smt.leave_type_id,
                    description = smt.description,
                    submit_date = smt.submit_date,
                    leave_from_date = smt.leave_from_date,
                    leave_to_date = smt.leave_to_date,
                    leave_desc = smt.leave_desc,
                    app_status = smt.app_status,
                    app_by = smt.app_by,
                    app_by_name = (smt.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.app_by) : "",
                    app_date = smt.app_date,
                    app_remarks = smt.app_remarks,
                    emp_id = smt.emp_id,
                    leave_in_hrs = smt.leave_in_hrs,
                    leave_in_days = smt.leave_in_days,
                    can_submit_date = smt.can_submit_date,
                    can_desc = smt.can_desc,
                    can_by = smt.can_by,
                    can_by_name = (smt.can_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.can_by) : "",
                    can_date = smt.can_date,
                    can_remarks = smt.can_remarks,
                    employee = smt.employee,
                    emp_status = smt.emp_status,
                    fiscal_year = FiscalYear,
                    fiscal_year_abb = fiscal_year_abb,
                    start_fiscal_date = start_fiscal_date,
                    end_fiscal_date = end_fiscal_date,
                    workingHoursDays = workingHoursDays
                };
                ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                return PartialView("Personnel/_LeaveAddEdit", model);
            }
            return BadRequest(new { success = false, message = Lang.msg_error });
        }


        #endregion
        /********************************************************************************************************************/
        #region DOCUMENT TEMPLATES  10854
        [HttpGet]
        public IActionResult DocumentTemplates()
        {
            string PageId = "10854";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_document_templates
                where a.status == "A"
                orderby a.id descending
                select new DocumentTemplatesViewModel
                {
                    id = a.id,
                    document_title = a.document_title,
                    document_version = a.document_version,
                    document_desc = a.document_desc,
                    upload_file = a.upload_file,
                    upload_date = Convert.ToDateTime(a.upload_date),
                    status = a.status
                }).ToList();
            return PartialView("Personnel/_DocumentTemplates", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocumentTemplatesList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_document_templates.OrderByDescending(a => a.id)
                .Select(a => new DocumentTemplatesViewModel
                {
                    id = a.id,
                    document_title = a.document_title,
                    document_version = a.document_version,
                    document_desc = a.document_desc,
                    upload_file = a.upload_file,
                    upload_date = a.upload_date,
                    status = a.status
                });

            query = query.Where(a => a.status == "A");
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.document_title != null && a.document_title.Contains(searchValue)) ||
                    (a.document_version != null && a.document_version.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count;
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
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> DocumentTemplatesDownload(string id)
        {
            string PageId = "10854";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_document_templates.FirstOrDefault(h => h.id == id);
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.upload_file))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "downloads");

                string filePath = Path.Combine(uploadsFolder, smt.upload_file);
                string fullPathResolved = Path.GetFullPath(filePath);
                string baseDirectoryResolved = Path.GetFullPath(uploadsFolder + Path.DirectorySeparatorChar);
                if (fullPathResolved.StartsWith(baseDirectoryResolved, StringComparison.OrdinalIgnoreCase))
                {
                    if (System.IO.File.Exists(fullPathResolved))
                    {
                        // Use provider to get MIME type from extension
                        var provider = new FileExtensionContentTypeProvider();
                        if (!provider.TryGetContentType(fullPathResolved, out var contentType))
                        {
                            contentType = "application/octet-stream"; // fallback
                        }
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPathResolved);
                        return File(fileBytes, contentType, smt.upload_file);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region 10806 PAY SLIPS
        [HttpGet]
        public IActionResult PaySlips()
        {
            string PageId = "10806";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            string? FiscalYearActive = HttpContext.Session.GetString("fiscal_year");
            string? emp_status = _employeeServices.GetEmployeeStatus(emp_id);
            string? employee = _employeeServices.GetEmployeeName(emp_id);

            ViewBag.EmployeeStatus = emp_status == "A" ? "Active" : "Inactive";
            ViewBag.Employee = employee;
            ViewBag.ReportDisplayType = GetReportDisplayType();
            ViewBag.SalYear = _settingsServices.GetYears(DateTime.Now.Year);
            ViewBag.SalMonth = _settingsServices.GetMonths(DateTime.Now.Month);
            ViewBag.SalFiscalYear = _settingsServices.GetFiscalYears(FiscalYearActive ?? string.Empty);
            return PartialView("Personnel/_PaySlips", "");
        }
        [HttpGet]
        public IActionResult PaySlipsReport(int empId, string startYear, string startMonth, string startFiscalYear, string endYear, string endMonth, string endFiscalYear, string mode, string type)
        {
            short start_year = Convert.ToInt16(startYear);
            short start_month = Convert.ToInt16(startMonth);
            short end_year = Convert.ToInt16(endYear);
            short end_month = Convert.ToInt16(endMonth);
            string file_suffix = "";
            string BlockedMessage = "";
            if (string.Equals(type, "sm", StringComparison.OrdinalIgnoreCase))
            {
                file_suffix = $"{startYear}_{startMonth}";
                /**restrict user from being view pay slip if the status is blocked'*/
                var strsql = _context.tbl_employee_salary_block.FirstOrDefault(b => b.emp_id == empId && b.sal_year == start_year && b.sal_month == end_month);
                if (strsql != null) { BlockedMessage = "You can't view the pay slip for selected criteria. Either it is not generated or in process. Please contact with your Finance Administrator."; }
            }
            else if(string.Equals(type, "sy", StringComparison.OrdinalIgnoreCase))
            {
                file_suffix = startFiscalYear.Replace("/", "-", StringComparison.OrdinalIgnoreCase);
                /**
                 * restrict user from being view pay slip if the status is blocked'
                 * and the block salary exists with in the fiscal year
                 * get fiscal start and end date and compare
                 */
                var FiscalYearDate = _settingsServices.GetFiscalStartEndDate(startFiscalYear);
                var strsql = _context.tbl_employee_salary_block.FirstOrDefault(b => b.emp_id == empId);
                if (strsql != null)
                {
                    string sal_year = strsql.sal_year.ToString();
                    string sal_month = strsql.sal_month.ToString();
                    if (!string.IsNullOrEmpty(sal_year) && !string.IsNullOrEmpty(sal_month))
                    {
                        DateTime blocledDate = new DateTime(Convert.ToInt32(sal_year), Convert.ToInt32(sal_month), 15);
                        if (FiscalYearDate.StartDate >= blocledDate && blocledDate <= FiscalYearDate.EndDate)
                        {
                            BlockedMessage = "You can't view the pay slip for selected criteria. Either it is not generated or in process. Please contact with your Finance Administrator.";
                        }
                    }
                }
            }
            else if (string.Equals(type, "mm", StringComparison.OrdinalIgnoreCase))
            {
                file_suffix = $"{startYear}_{startMonth}_to_{endYear}_{endMonth}";
            }
            else if (string.Equals(type, "my", StringComparison.OrdinalIgnoreCase))
            {
                file_suffix = $"{startFiscalYear}_to_{endFiscalYear}".Replace("/", "_", StringComparison.OrdinalIgnoreCase);
            }
            ViewBag.BlockedMessage = BlockedMessage;
            if (mode == "Export")
            {
                /*
                var sb = new StringBuilder();
                string html = $@"
                    <table border='1'>
                        <tr><th>Employee Name</th><th>Amount</th></tr>
                    </table>
                    ";
                sb.Append(html);
                string fileName = $"employee_pay_slip_{file_suffix}.xls";
                return File(Encoding.UTF8.GetBytes(sb.ToString()), "application/vnd.ms-excel", fileName);
                */
            }
            return PartialView("Personnel/_PaySlipsReport", null);
        }
        #endregion
        /********************************************************************************************************************/

        public IActionResult Index()
        {
            return View();
        }
    }
}