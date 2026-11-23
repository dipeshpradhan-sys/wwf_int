using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Reporting.NETCore;
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
        private readonly TravelServices _travelServices;
        private readonly GeneralServices _generalServices;
        private readonly RequestServices _requestServices;
        private readonly ApproverResolverService _approverResolver;
        private readonly TimesheetMessageService _timesheetMessageService;
        private readonly AdministrationEmailService _administrationEmailService;
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
            IWebHostEnvironment webHostEnvironment,
            TravelServices travelServices,
            GeneralServices generalServices,
            RequestServices requestServices,
            ApproverResolverService approverResolver,
            TimesheetMessageService timesheetMessageService,
            AdministrationEmailService administrationEmailService
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
            _travelServices = travelServices;
            _generalServices = generalServices;
            _requestServices = requestServices;
            _approverResolver = approverResolver;
            _timesheetMessageService = timesheetMessageService;
            _administrationEmailService = administrationEmailService;
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
        #region 10805 TRAVEL SETTLEMENT
        [HttpGet]
        public IActionResult TravelSettlement()
        {
            string PageId = "10805";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION;

            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            string emp_status = _employeeServices.GetEmployeeStatus(emp_id);
            string employee = _employeeServices.GetEmployeeName(emp_id);
            string FiscalYearActive = HttpContext.Session.GetString("fiscal_year") ?? "";
            string SelectedFiscalYear = FiscalYearActive;
            string start_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_from") ?? "";
            string end_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_to") ?? "";
            var startFiscalDate = DateTime.TryParse(start_fiscal_date, out var DF) ? DF : DateTime.MinValue;
            var endFiscalDate = DateTime.TryParse(end_fiscal_date, out var DE) ? DE : DateTime.MinValue;
            var Records = (from main in _context.tbl_employee_travel_main
                           where main.emp_id == emp_id &&
                           main.date_from >= startFiscalDate && main.date_to <= endFiscalDate &&
                           main.app_status == "Approved"
                           orderby main.date_from descending
                           select new TravelSettlementListViewModel
                           {
                               EmpTravelId = main.emp_travel_id,
                               TravelType = main.travel_type,
                               Destinations = main.destinations,
                               DateFrom = main.date_from,
                               DateTo = main.date_to,
                               SubmitDate = main.submit_date,
                               AppStatus = main.app_status,
                               ShowPrint = "N"
                           }).ToList();

            ViewBag.FiscalYearList = _settingsServices.GetFiscalYears(FiscalYearActive);
            ViewBag.EmployeeStatus = emp_status == "A" ? "Active" : "Inactive";
            ViewBag.SettlementStatusFilter = _travelServices.GetTravelStatus();
            ViewBag.Employee = employee;
            return PartialView("Personnel/_TravelSettlement", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TravelSettlementList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string SelectedFiscalYear = request.FilterValue1;
            string SettlementStatusFilter = request.FilterValue2 ?? "";

            string FiscalYearActive = HttpContext.Session.GetString("fiscal_year") ?? "";
            int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            string start_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_from") ?? "";
            string end_fiscal_date = _settingsServices.GetFiscalYearValue(SelectedFiscalYear, "date_to") ?? "";
            string emp_status = _employeeServices.GetEmployeeStatus(emp_id);
            var startFiscalDate = DateTime.TryParse(start_fiscal_date, out var DF) ? DF : DateTime.MinValue; ;
            var endFiscalDate = DateTime.TryParse(end_fiscal_date, out var DE) ? DE : DateTime.MinValue; ;

            var query =
                from main in _context.tbl_employee_travel_main
                where main.emp_id == emp_id
                      && main.date_from >= startFiscalDate
                      && main.date_to <= endFiscalDate
                      && main.app_status == "Approved"
                      && (main.can_by == null || main.can_by == 0)
                orderby main.date_from descending
                let settlement = _context.tbl_employee_travel_settlement_main
                    .Where(s => s.emp_travel_id == main.emp_travel_id)
                    .FirstOrDefault()
                select new TravelSettlementListViewModel
                {
                    EmpTravelId = main.emp_travel_id,
                    TravelType = main.travel_type,
                    Destinations = main.destinations,
                    DateFrom = main.date_from,
                    DateTo = main.date_to,
                    SubmitDate = main.submit_date,
                    AppStatus = main.app_status,
                    TravSetId = settlement != null ? settlement.trav_set_id : "0",
                    SettlementType =
                        settlement == null ? "U" :
                        settlement.app_status == "T" ? "T" :
                        settlement.app_status == "P" && settlement.is_for_set == "Y" ? "R" :
                        settlement.app_status == "A" && settlement.is_for_set == "Y" ? "S" :
                        settlement.app_status == "P" && settlement.is_for_set == "N" ? "N" :
                        settlement.app_status == "A" && settlement.is_for_set == "N" ? "M" :
                        "U",
                    SettlementTypeDetail =
                        settlement == null ? "Unsettled" :
                        settlement.app_status == "T" ? "Saved" :
                        settlement.app_status == "P" && settlement.is_for_set == "Y" ? "Submitted" :
                        settlement.app_status == "A" && settlement.is_for_set == "Y" ? "Selttled" :
                        settlement.app_status == "P" && settlement.is_for_set == "N" ? "SNR [Pending]" :
                        settlement.app_status == "A" && settlement.is_for_set == "N" ? "SNR [Verified]" :
                        "Unsettled",
                    ShowPrint = (
                        (settlement.app_status == "P" && settlement.is_for_set == "Y") ||
                        (settlement.app_status == "A" && settlement.is_for_set == "Y")
                        ) ? "Y" : "N"
                };
            if (!string.IsNullOrEmpty(SettlementStatusFilter))
            {
                query = query.Where(d => d.SettlementType == SettlementStatusFilter);
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy($"{sortColumn} {sortColumnDir}");
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                (a.Destinations != null && a.Destinations.Contains(searchValue)) ||
                (a.TravelType != null && a.TravelType.Contains(searchValue)) ||
                (a.DateFrom != null && a.DateFrom.ToString().Contains(searchValue)) ||
                (a.DateTo != null && a.DateTo.ToString().Contains(searchValue)) ||
                (a.SubmitDate != null && a.SubmitDate.ToString().Contains(searchValue))
                );
            }
            int totalRecord = query.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
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
        public async Task<IActionResult> TravelSettlementAddEditAsync(int? id, string mode)
        {
            string PageId = "10805";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            if (id is < 1 or null) { return BadRequest(new { success = false, message = Lang.msg_error }); }

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.FundSource = _generalServices.GetFundSourceBoth();

            TravelSettlementViewModel model;
            model = new TravelSettlementViewModel();
            model = await _travelServices.GetTravelSettlementDetailAsync(Convert.ToInt32(id)).ConfigureAwait(false);

            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
            }
            else if (mode == "edit")
            {
                ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            }
            ViewBag.ChargePerOrAmt = _requestServices.GetChargePerOrAmt(model.TravelSettlement.ChargePerOrAmt);
            return PartialView("Personnel/_TravelSettlementAddEdit", model);
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
            return PartialView("Personnel/_Timesheet");
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

            if (toEmpID <= 0 && toID <= 0)
            {
                return PartialView("Personnel/_TimesheetAddEdit", new wwfpp.Models.Request.TimesheetViewModel { HasApprover = false });
            }

            string PageId = "10203";
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") return RedirectToAction("PermissionDenied", "Home");
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;

            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var curMonthLastDate = new DateTime(year, month, daysInMonth);

            int maxtimeSheetCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, year, month);
            var prevApprovedTimesheet = await _context.tbl_employee_timesheet_app.Where(c => c.emp_year == (short)year && c.emp_month == (byte)month && c.emp_id == empid && c.submit_counter < maxtimeSheetCounter && (c.app_dec == "i" || c.app_dec == "a")).CountAsync();

            // --- Preload data in fewer queries ---
            var dbRecords = await _context.tbl_employee_timesheet_sub.Where(c => c.emp_year == (short)year && c.emp_month == (byte)month && c.emp_id == empid && c.submit_counter == maxtimeSheetCounter).ToListAsync();
            var otRecords = await _context.tbl_employee_overtime_request.Where(o => o.emp_id == empid && o.ot_date.HasValue && o.ot_date.Value.Year == year && o.ot_date.Value.Month == month && o.app_status == "A").ToListAsync();
            var holidayDates = await _context.tbl_setting_holidays.Where(h => h.holiday_date.HasValue && h.holiday_date.Value.Year == year && h.holiday_date.Value.Month == month).Select(h => h.holiday_date.Value.Date).ToHashSetAsync();
            var dayOffDates = await _context.tbl_employee_dayoff.Where(h => h.dayoff_date.HasValue && h.dayoff_date.Value.Year == year && h.dayoff_date.Value.Month == month).Select(h => h.dayoff_date.Value.Date).ToHashSetAsync();
            var restrictedLeaveRecords = await _context.tbl_employee_leave.Where(l => l.app_status == "Approved" && l.emp_id == empid && l.leave_type_id == 15 && l.leave_from_date.HasValue && l.leave_to_date.HasValue && l.leave_from_date.Value <= curMonthLastDate && l.leave_to_date.Value >= start).ToListAsync();
            var restrictedLeaveDates = restrictedLeaveRecords.SelectMany(l => Enumerable.Range(0, (l.leave_to_date.Value.Date - l.leave_from_date.Value.Date).Days + 1).Select(offset => l.leave_from_date.Value.Date.AddDays(offset))).Where(d => d.Year == year && d.Month == month).ToHashSet();

            var hoursAllowedInHolidayWeekend = _requestServices.GetLimitHoursSetting();
            string hoursAllowedForHoliday = hoursAllowedInHolidayWeekend?.populate_hrs_in_timesheet_for_holiday ?? "N";
            string hoursAllowedForWeekEnd = hoursAllowedInHolidayWeekend?.populate_hrs_in_timesheet_for_weekend ?? "N";

            var (dateStart, dateEnd) = await _requestServices.GetFiscalYearRangeAsync(start);
            dateStart ??= start;
            dateEnd ??= DateTime.Today;

            var fiscalFunds = (from a in _context.tbl_fund_source
                               join b in _context.tbl_employee_fund_source on a.fund_id equals b.fund_id
                               join s in _context.tbl_employee_timesheet_sub on a.fund_id equals s.fund_id into subs
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
                                   used_hours = subs.Where(x => x.emp_id == empid && x.is_active == "A" && !(x.emp_month == month))
                                                    .Sum(x => (double?)(x.time_hours ?? 0) + (double?)(x.overtime_hours ?? 0)) ?? 0
                               }).ToList();

            // --- Precompute lookups
            // For TimesheetFullyFilled: group by day, collapse to "has hours"
            var recordLookupByDay = dbRecords.GroupBy(r => r.emp_day).ToDictionary(
                    g => g.Key,
                    g => g.Any(r => r.time_hours.HasValue && r.time_hours > 0)
                );

            // For FundRows: composite key (fund_id, emp_day)
            var recordLookupByFundDay = dbRecords.ToDictionary(r => (r.fund_id, r.emp_day), r => r);
            // Overtime records keyed by date
            var otLookup = otRecords.Where(o => o.ot_date.HasValue).ToDictionary(o => o.ot_date.Value.Date, o => o);

            bool TimesheetFullyFilled = dbRecords.Any() &&
                Enumerable.Range(1, daysInMonth).All(day =>
                {
                    var currentDate = new DateTime(year, month, day);
                    if ((holidayDates.Contains(currentDate)) && hoursAllowedForHoliday == "N") return true;
                    if ((currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday) && hoursAllowedForWeekEnd == "N") return true;
                    if (dayOffDates.Contains(currentDate)) return true;

                    var activeFundsForDay = fiscalFunds.Where(f => !f.expiry_date.HasValue || f.expiry_date.Value >= currentDate).ToList();
                    if (!activeFundsForDay.Any()) return true;

                    return recordLookupByDay.TryGetValue((byte)day, out var hasHours) && hasHours;
                });

            ViewBag.TimesheetFullyFilled = TimesheetFullyFilled;

            string? EmployeeID = HttpContext.Session.GetString("emp_id");
            bool fundSourceEditable = fiscalFunds.Any(fs => fs.default_for_holiday != "1");
            double fundBalance = fiscalFunds.Sum(f => (double)(f.annual_hrs ?? 0) - f.used_hours);
            bool empActive = _context.tbl_employee.Any(e => e.emp_id == empid && e.join_date <= end && (!e.end_date.HasValue || e.end_date >= start));
            bool calendarFilled = _context.tbl_calendar_setting.Any(c => c.cal_year == year && c.cal_month == month);
            bool employeeFiscalFund = fiscalFunds.Any();

            // Previous Month Timesheet
            var prevTimesheetStatus = "active";
            var MinFiscalYearStart = _context.tbl_fiscal_year.Min(fy => fy.date_from);
            DateTime? FiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
            if ((FiscalStartDate > MinFiscalYearStart) || (start > FiscalStartDate))
            {
                DateTime prevPeriod = start.AddMonths(-1);
                int empMonthPrev = prevPeriod.Month;
                int empYearPrev = prevPeriod.Year;
                int prevCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, empYearPrev, empMonthPrev);
                prevTimesheetStatus = await _requestServices.GetTimesheetStatusAsync(empid, empYearPrev, empMonthPrev, prevCounter);
            }

            var curTimesheetStatus = await _requestServices.GetTimesheetStatusAsync(empid, year, month, maxtimeSheetCounter);

            var messages = await _timesheetMessageService.BuildMessagesAsync(empid, year, month, maxtimeSheetCounter, empActive, calendarFilled, employeeFiscalFund, prevTimesheetStatus, curTimesheetStatus);

            // --- Button logic ---
            bool canEdit = _context.tbl_employee_administrator.Any(ea => ea.aca == Convert.ToInt32(EmployeeID)) || Convert.ToInt32(EmployeeID) == empid;
            bool showSaveButton = canEdit;
            bool showSendButton = (prevTimesheetStatus == "" || prevTimesheetStatus == "active") && (curTimesheetStatus == "justsaved" || curTimesheetStatus == "declined" || curTimesheetStatus == "pending");

            bool showAddEditTimeSheetButton = calendarFilled && empActive && employeeFiscalFund && canEdit && (prevTimesheetStatus == "" || prevTimesheetStatus == "active") && curTimesheetStatus != "pending";
            bool showTimesheetPreviewButton = _context.tbl_employee_timesheet_app.Any(t => t.emp_id == empid && t.emp_month == month && t.emp_year == year);

            // --- Fund rows ---
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
                    recordLookupByFundDay.TryGetValue((f.fund_id, (byte)d), out var record);
                    otLookup.TryGetValue(date, out var otRecord);

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


            // Max Normal and Overtime hours defined
            var limits = await _context.tbl_setting_limit_hrs.FirstOrDefaultAsync();
            int maxNormal = limits?.normal_working_hrs ?? 8;
            int maxOvertime = limits?.overtime_normal_working_hrs ?? 4;

            // --- Build daily totals ---
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
                PrevApprovedTimesheetCount = prevApprovedTimesheet,
                ShowTimesheetPreviewButton = showTimesheetPreviewButton
            };

            return PartialView("Personnel/_TimesheetAddEdit", viewModel);
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
            //string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empid}&emp_month={month}&emp_year={year}&toid={toId}&toemp_id={toEmpId}&st=a&str_counter={maxtimeSheetCounter}&app_id={app_id}&approval_from=email&approve_for=timesheet'>Approve</a> | ";
            //string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empid}&emp_month={month}&emp_year={year}&toid={toId}&toemp_id={toEmpId}&st=d&str_counter={maxtimeSheetCounter}&app_id={app_id}&approval_from=email&approve_for=timesheet'>Decline</a> ";

            #region ADD TOKEN WHILE SENDING MAIL

            string token = await _requestServices.GenerateTokenAsync(app_id, Convert.ToInt32(empid), toId, toEmpId, year, month, maxtimeSheetCounter, "timesheet");

            #endregion


            string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/TokenIndex?token={token}&st=a'>Approve</a> | ";
            string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/TokenIndex?token={token}&st=d'>Decline</a>";


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

            return PartialView("Personnel/_PrevApprovedTimesheetsPartial", viewModel);
        }
        public async Task<IActionResult> TimesheetPreview(int year, int month, int empid)
        {
            int maxtimeSheetCounter = await _requestServices.GetCurrentMaxCounterAsync(empid, year, month);

            // Get all approved counters less than current
            var approvedCounters = _context.tbl_employee_timesheet_app
                .Where(c => c.emp_year == year
                         && c.emp_month == month
                         && c.emp_id == empid
                         && (c.app_dec == "i" || c.app_dec == "a" || c.app_dec == "p"))
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

            return PartialView("Personnel/_PrevApprovedTimesheetsPartial", viewModel);
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }
    }
}