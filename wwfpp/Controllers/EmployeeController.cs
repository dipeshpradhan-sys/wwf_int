using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Services;
using static GblUtilities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Controllers
{
    public class EmployeeController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        EmailService emailService,
        GlobalOptionServices globalOptionServices,
        EmployeeServices employeeServices,
        SettingsServices settingsServices,
        AccountServices accountServices,
        LeaveServices leaveServices,
        IWebHostEnvironment webHostEnvironment
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly EmailService _emailService = emailService;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;
        private readonly EmployeeServices _employeeServices = employeeServices;
        private readonly SettingsServices _settingsServices = settingsServices;
        private readonly AccountServices _accountServices = accountServices;
        private readonly LeaveServices _leaveServices = leaveServices;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        public IActionResult Index()
        {
            return View();
        }
        /********************************************************************************************************************/
        [HttpGet]
        public IActionResult EmployeeListByStatus(string? status)
        {
            ViewBag.EmpStatus = status;
            ViewBag.EmployeeList = _employeeServices.GetEmployeeList(status);
            return PartialView("_EmployeeListByStatus");
        }
        /********************************************************************************************************************/
        #region EMPLOYEE FUND SOURCE
        [HttpGet]
        public IActionResult EmployeeFundSource()
        {
            string PageId = "10109";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            string FiscalYear = HttpContext.Session.GetString("fiscal_year") ?? "";

            var Records = (
                from a in _context.tbl_employee_fund_source
                orderby a.emp_fund_id descending
                select new EmployeeFundSourceViewModel
                {
                    emp_fund_id = a.emp_fund_id,
                    fund_id = a.fund_id,
                    annual_hrs = a.annual_hrs,
                    start_date = a.start_date,
                    end_date = a.end_date,
                    emp_id = a.emp_id
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(FiscalYear);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/EmployeeFundSource", "ADD|DOWNLOAD-FORMAT|IMPORT|EXPORT|DEL", PageId, Records.Count);
            return PartialView("Employee/_EmployeeFundSource", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeFundSourceList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1 ?? "A";
            string? FiscalYearFilter = request.FilterValue2 ?? HttpContext.Session.GetString("fiscal_year");
            string str_start_date = _settingsServices.GetFiscalYearValue(FiscalYearFilter, "date_from");
            string str_end_date = _settingsServices.GetFiscalYearValue(FiscalYearFilter, "date_to");

            DateTime start_date = Convert.ToDateTime(str_start_date);
            DateTime end_date = Convert.ToDateTime(str_end_date);

            var query = from ef in _context.tbl_employee_fund_source
                        join emp in _context.tbl_employee
                            on ef.emp_id equals emp.emp_id into fSourceGroup
                        from emp in fSourceGroup.DefaultIfEmpty()
                        join fs in _context.tbl_fund_source
                            on ef.fund_id equals fs.fund_id into fundGroup
                        from fs in fundGroup.DefaultIfEmpty()
                        orderby emp.firstname descending
                        select new EmployeeFundSourceViewModel
                        {
                            emp_id = emp.emp_id,
                            emp_fund_id = ef.emp_fund_id,
                            fund_source = fs.fund_source ?? "",
                            annual_hrs = ef.annual_hrs ?? 0,
                            start_date = ef.start_date,
                            end_date = ef.end_date,
                            expiry_date = fs.expiry_date,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status
                        };
            if (!string.IsNullOrEmpty(FiscalYearFilter))
            {
                query = query.Where(d => d.start_date <= start_date && d.end_date >= end_date);
            }
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue)) ||
                    (a.fund_source != null && a.fund_source.Contains(searchValue)) ||
                    (a.expiry_date != null && a.expiry_date.ToString().Contains(searchValue))
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
        public IActionResult EmployeeFundSourceAddEdit(string? id, string mode, string EmpId, string eFiscalYear)
        {
            string PageId = "10109";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;

            EmployeeFundSourceViewModel model;
            if (mode == "add")
            {
                string FiscalYear = HttpContext.Session.GetString("fiscal_year");
                string str_start_date = _settingsServices.GetFiscalYearValue(FiscalYear, "date_from");
                string str_end_date = _settingsServices.GetFiscalYearValue(FiscalYear, "date_to");
                DateTime start_date = Convert.ToDateTime(str_start_date);
                DateTime end_date = Convert.ToDateTime(str_end_date);
                model = new EmployeeFundSourceViewModel
                {
                    emp_fund_id = 0,
                    fund_id = 0,
                    emp_id = 0,
                    annual_hrs = 0,
                    start_date = start_date,
                    end_date = end_date
                };
                ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
                ViewBag.FundSource = _employeeServices.GetFundSourceActiveOnly();
                ViewBag.FiscalYear = FiscalYear;
                ViewBag.FiscalYearAbb = HttpContext.Session.GetString("fiscal_year_abb");
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_EmployeeFundSourceAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    int emp_id = int.TryParse(EmpId, out int pEmpId) ? pEmpId : 0;
                    string FiscalYear = eFiscalYear ?? "";
                    string str_start_date = _settingsServices.GetFiscalYearValue(FiscalYear, "date_from");
                    string str_end_date = _settingsServices.GetFiscalYearValue(FiscalYear, "date_to");
                    DateTime start_date = Convert.ToDateTime(str_start_date);
                    DateTime end_date = Convert.ToDateTime(str_end_date);

                    var ec = (from ef in _context.tbl_employee_fund_source
                              join fnd in _context.tbl_fund_source
                                  on ef.fund_id equals fnd.fund_id
                              join emp in _context.tbl_employee
                                  on ef.emp_id equals emp.emp_id
                              where ef.emp_fund_id == Convert.ToInt32(id)
                              select new
                              {
                                  ef.emp_fund_id,
                                  ef.fund_id,
                                  fnd.fund_source,
                                  fnd.expiry_date,
                                  ef.emp_id,
                                  ef.annual_hrs,
                                  ef.start_date,
                                  ef.end_date,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();

                    if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeFundSourceViewModel
                    {
                        emp_fund_id = ec.emp_fund_id,
                        fund_id = ec.fund_id,
                        fund_source = ec.fund_source,
                        expiry_date = ec.expiry_date,
                        emp_id = ec.emp_id,
                        annual_hrs = ec.annual_hrs,
                        start_date = ec.start_date,
                        end_date = ec.end_date,
                        firstname = ec.firstname,
                        middlename = ec.middlename,
                        lastname = ec.lastname,
                        emp_code = ec.emp_code,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    var used_hrs = _context.que_timesheet_sub
                        .Where(t => (t.is_active == "A")
                                 && t.emp_id == ec.emp_id
                                 && t.fund_id == ec.fund_id
                                 && t.fiscal <= ec.start_date
                                 && t.fiscal >= ec.end_date)
                        .Sum(t => (double?)(t.time_hours + t.overtime_hours)) ?? 0;

                    var pending_hrs = _context.que_timesheet_sub
                        .Where(t => (t.is_active == "N")
                                 && t.emp_id == ec.emp_id
                                 && t.fund_id == ec.fund_id
                                 && t.fiscal <= ec.start_date
                                 && t.fiscal >= ec.end_date)
                        .Sum(t => (double?)(t.time_hours + t.overtime_hours)) ?? 0;

                    double t_used_hrs = used_hrs + pending_hrs;

                    ViewBag.FundSource = model.fund_source;
                    ViewBag.used_hrs = used_hrs;
                    ViewBag.pending_hrs = pending_hrs;
                    ViewBag.t_used_hrs = t_used_hrs;

                    ViewBag.FiscalYear = FiscalYear;
                    ViewBag.FiscalYearAbb = _settingsServices.GetFiscalYearValue(FiscalYear, "fiscal_year_abb");
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_EmployeeFundSourceAddEdit", model);

                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EmployeeFundSourceSave(EmployeeFundSourceViewModel model)
        {
            ModelState.Remove("emp_fund_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            int emp_fund_id = Convert.ToInt32(Request.Form["emp_fund_id"]);
            int? fund_id = model.fund_id;
            double? annual_hrs = model.annual_hrs;
            DateTime? start_date = model.start_date;
            DateTime? end_date = model.end_date;
            int? emp_id = model.emp_id;

            if (mode == "add")
            {
                if (annual_hrs == null || annual_hrs <= 0) annual_hrs = 0;

                /**CHECK IF THERE WAS ALREADY DEFINED HOURS FOR SELECTED FISCAL YEAR ON SELECTED FUND ID*/
                var exists = _context.tbl_employee_fund_source
                    .Any(f => f.fund_id == fund_id
                                && f.emp_id == emp_id
                                && f.start_date <= start_date
                                && f.end_date >= end_date);

                if (exists) { return Json(new { status = "exists", message = "Fund source already defined for this fiscal year." }); }
                emp_fund_id = (_context.tbl_employee_fund_source.Any()
                                    ? _context.tbl_employee_fund_source.Max(o => o.emp_fund_id)
                                    : 0) + 1;
                var dataSave = new tbl_employee_fund_source
                {
                    emp_fund_id = emp_fund_id,
                    fund_id = Convert.ToInt32(fund_id),
                    annual_hrs = annual_hrs,
                    start_date = start_date,
                    end_date = end_date,
                    emp_id = emp_id
                };

                _ = _context.tbl_employee_fund_source.Add(dataSave);
                _ = _context.SaveChangesAsync().ConfigureAwait(false);

                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_fund_id });
            }
            else if (mode == "edit")
            {
                if (annual_hrs == null) annual_hrs = 0;

                /**CHECK IF THERE WAS ALREADY DEFINED HOURS FOR SELECTED FISCAL YEAR ON SELECTED FUND ID'*/
                var exists = _context.tbl_employee_fund_source
                    .Any(f => f.fund_id == fund_id
                                && f.emp_id == emp_id
                                && f.start_date <= start_date
                                && f.end_date >= end_date
                                && f.emp_fund_id != emp_fund_id);

                if (exists)
                {
                    return Json(new { status = "error", message = "Fund source already defined for this fiscal year." });
                }
                /**CHECKING IF USED HOURS IS LESS THAN UPLOADING HOURS [INCLUDE PENDING HOURS ALSO]*/
                var used_hrs = _context.que_timesheet_sub
                    .Where(t => (t.is_active == "A" || t.is_active == "N")
                             && t.emp_id == emp_id
                             && t.fund_id == fund_id
                             && t.fiscal <= start_date
                             && t.fiscal >= end_date)
                    .Sum(t => (double?)(t.time_hours + t.overtime_hours)) ?? 0;

                if (annual_hrs < used_hrs)
                {
                    return Json(new { status = "hourshort", message = "Annual hours less than used hours." });
                }

                var dataUpdate = _context.tbl_employee_fund_source
                    .FirstOrDefault(h => h.emp_fund_id == emp_fund_id && h.emp_id == emp_id);

                if (dataUpdate != null)
                {
                    dataUpdate.annual_hrs = annual_hrs;
                    _ = _context.tbl_employee_fund_source.Update(dataUpdate);
                    _ = _context.SaveChangesAsync().ConfigureAwait(false);

                    return Json(new { status = "success", message = Lang.msg_update_success, id = dataUpdate.emp_fund_id });
                }
                else
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeFundSourceDelete([FromBody] DeleteRequest request)
        {
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_fund_source
                .Where(r => request.SelectedIds != null && request.SelectedIds.Contains((r.emp_fund_id).ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            /**DO NOT ALLOW TO DELETE IF THE FUND SOURCE IS USED IN TBL_EMPLOYEE_TIMESHEET_SUB TABLE*/
            int tSel = request.SelectedIds.Count; int tDel = 0; int tUDel = 0;
            foreach (var item in request.SelectedIds)
            {
                var query = _context.tbl_employee_fund_source.FirstOrDefault(h => h.emp_fund_id == Convert.ToInt32(item));
                int fund_id = query.fund_id;
                int emp_id = Convert.ToInt32(query.emp_id);
                DateTime start_date = Convert.ToDateTime(query.start_date);
                DateTime end_date = Convert.ToDateTime(query.end_date);

                var exists = _context.que_timesheet_sub
                    .Any(t => t.emp_id == emp_id
                             && t.fund_id == fund_id
                             && t.fiscal <= start_date
                             && t.fiscal >= end_date);
                if (exists)
                {
                    //do nothing
                }
                else
                {
                    tDel++;
                    _context.tbl_employee_fund_source.RemoveRange(query);
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    _context.ChangeTracker.Clear();
                }

            }
            tUDel = tSel - tDel;
            string msg_deleted_records = string.Empty;
            bool msg_status = false;
            if (tUDel > 0)
            {
                msg_deleted_records = Lang.msg_deleted_some;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.OrdinalIgnoreCase);
                msg_deleted_records = msg_deleted_records.Replace("[<UN-DEL-ROWS>]", tUDel.ToString(), StringComparison.OrdinalIgnoreCase);
                msg_status = false;
            }
            else
            {
                msg_deleted_records = Lang.msg_delete_success;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.OrdinalIgnoreCase);
                msg_status = true;
            }
            return Ok(new
            {
                status = msg_status,
                deletedCount = tDel,
                message = msg_deleted_records
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeFundSourceDownloadFormat()
        {
            var sb = new StringBuilder();
            // Get all fund sources (filter to active or fiscal year if needed)
            var fundSources = _context.tbl_fund_source
                .Where(f => f.fund_status == "A")
                .Select(f => new { f.fund_id, f.fund_source })
                .ToList();
            var values = new List<string> { };
            if (fundSources.Count > 0)
            {
                foreach (var rec in fundSources)
                {
                    values.AddRange("0");
                }
            }
            // Header row: S.N., Employee Code, Employee Name, then each fund source
            var header = new List<string> { "S.N.", "Employee Code", "Employee Name" };
            header.AddRange(fundSources.Select(fs => fs.fund_source));
            _ = sb.AppendLine(string.Join(",", header));

            // Build employee + fund hours data
            var employees = (from e in _context.tbl_employee
                             where e.emp_id != 0 && e.emp_status == "A"
                             select new
                             {
                                 EmployeeCode = e.emp_code,
                                 FullName = string.Join(" ", new[] { e.firstname, e.middlename, e.lastname }
                                 .Where(x => !string.IsNullOrEmpty(x)))
                             }).ToList();
            int cnt = 0;
            if (employees.Count > 0)
            {
                foreach (var record in employees)
                {
                    cnt++;
                    string emp_code = EscapeCSV(record.EmployeeCode ?? "");
                    string employee = EscapeCSV(record.FullName ?? "");
                    var NewValue = new List<string> { cnt.ToString(), emp_code, employee };
                    NewValue.AddRange(values);
                    _ = sb.AppendLine(string.Join(",", NewValue));
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "EmployeeFundSourceExport.csv");
        }
        [HttpGet]
        public IActionResult EmployeeFundSourceImport()
        {
            string PageId = "10109";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.FiscalYear = HttpContext.Session.GetString("fiscal_year");
            ViewBag.FiscalYearAbb = HttpContext.Session.GetString("fiscal_year_abb");
            return PartialView("Employee/_EmployeeFundSourceImport");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeFundSourceImportSave(IFormFile file)
        {
            if (file == null || file.Length == 0) { return Json(new { status = "error", message = Lang.NO_FILE_UPLOADED }); }
            if (!FileValidator.ForCsv(file)) { return Json(new { status = "error", message = "There is problem with File." }); }

            string? StrDateFrom = HttpContext.Session.GetString("date_from");
            string? StrDateTo = HttpContext.Session.GetString("date_to");

            if (string.IsNullOrEmpty(StrDateFrom) || string.IsNullOrEmpty(StrDateTo)) { return Json(new { status = "error", message = Lang.FISCAL_DATES_NOT_FOUND_IN_SESSION }); }

            DateTime StartDate = DateTime.Parse(StrDateFrom);
            DateTime EndDate = DateTime.Parse(StrDateTo);

            var errors = new List<string>();
            var newEntities = new List<tbl_employee_fund_source>();
            var updateEntities = new List<tbl_employee_fund_source>();
            var removeEntities = new List<tbl_employee_fund_source>();

            using var reader = new StreamReader(file.OpenReadStream());
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
            headerLine = headerLine.Replace("\r", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\n", "", StringComparison.OrdinalIgnoreCase)
                                   .Replace("\"", "", StringComparison.OrdinalIgnoreCase);
            var headers = headerLine.Split(',').Select(h => h.Trim('"')).ToList();
            var fundSourceHeaders = headers.Skip(3).ToList();

            foreach (var fundSourceName in fundSourceHeaders)
            {
                var fundSource = await _context.tbl_fund_source.FirstOrDefaultAsync(f => f.fund_status == "A" && f.fund_source == fundSourceName).ConfigureAwait(false);
                if (fundSource == null)
                {
                    errors.Add("> " + Lang.FUND_SOURCE_NOT_EXIST.Replace("<[FUND-SOURCE-NAME]>", fundSourceName, StringComparison.OrdinalIgnoreCase));
                }
            }
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) { continue; }

                if (line != null)
                {
                    // Normalize line breaks and quotes like Classic ASP
                    line = line.Replace("\"", "", StringComparison.OrdinalIgnoreCase);


                    var values = line.Split(',').Select(v => v.Trim('"')).ToList();
                    string empCode = values[1];
                    string employeeCode = _employeeServices.GetValidEmpCode(empCode);
                    var emp = await _context.tbl_employee.FirstOrDefaultAsync(e => e.emp_code == employeeCode).ConfigureAwait(false); // INSTEAD OF THIS SECTION paymgr.getIDByEmpCode(s_emp_code)

                    if (emp == null || emp.emp_status != "A")
                    {
                        errors.Add("> " + Lang.INACTIVE_EMPLOYEE.Replace("<[EMP-CODE]>", employeeCode, StringComparison.OrdinalIgnoreCase));
                        continue;
                    }
                    //FOR EMPLOYEE FUND SOURCE VALIDATE ONLY
                    for (int i = 0; i < fundSourceHeaders.Count; i++)
                    {
                        string fundSourceName = fundSourceHeaders[i];
                        string rawValue = values[i + 3]; //the fund source start from column 3

                        // Remove commas or other thousand separators
                        string cleanedValue = rawValue.Replace(",", "", StringComparison.OrdinalIgnoreCase).Trim();

                        // Try to parse annual hours
                        double annualHours = double.TryParse(cleanedValue, out double annualHoursp) ? annualHoursp : 0;

                        if (annualHours >= 0)
                        {
                            double used_hrs = 0;

                            int fund_id = await _context.tbl_fund_source
                                .Where(f => f.fund_source == fundSourceName.ToString())
                                .Select(f => (int)f.fund_id)
                                .FirstOrDefaultAsync().ConfigureAwait(false);

                            //'**include pending hours also **'
                            used_hrs = _context.que_timesheet_sub
                                    .Where(t => (t.is_active == "A" || t.is_active == "N")
                                                && t.emp_id == emp.emp_id
                                                && t.fund_id == fund_id
                                                && t.fiscal >= StartDate
                                                && t.fiscal <= EndDate)
                                    .Sum(t => (double?)(t.time_hours + t.overtime_hours))
                                    ?? 0;

                            //CHECKING FOR USED HOURS AND ANNUAL HOURS
                            if (annualHours >= used_hrs)
                            {
                                var existing = await _context.tbl_employee_fund_source
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(f => f.emp_id == emp.emp_id
                                                            && f.fund_id == fund_id
                                                            && f.start_date >= StartDate
                                                            && f.end_date <= EndDate).ConfigureAwait(false);
                                if (annualHours == 0)
                                {
                                    if (used_hrs == 0 && existing != null)
                                    {
                                        removeEntities.Add(existing); //Remove the fund source that is zero value
                                    }
                                }
                                else
                                {
                                    if (existing == null)
                                    {
                                        int maxId = await _context.tbl_employee_fund_source.MaxAsync(e => (int?)e.emp_fund_id).ConfigureAwait(false) ?? 0;
                                        maxId++;
                                        newEntities.Add(new tbl_employee_fund_source
                                        {
                                            emp_fund_id = maxId,
                                            emp_id = emp.emp_id,
                                            fund_id = Convert.ToInt32(fund_id),
                                            annual_hrs = annualHours,
                                            start_date = StartDate,
                                            end_date = EndDate
                                        });
                                    }
                                    else
                                    {
                                        updateEntities.Add(new tbl_employee_fund_source
                                        {
                                            emp_fund_id = existing.emp_fund_id,
                                            emp_id = emp.emp_id,
                                            fund_id = Convert.ToInt32(fund_id),
                                            annual_hrs = annualHours,
                                            start_date = StartDate,
                                            end_date = EndDate
                                        });

                                    }
                                }
                            }
                            else
                            {
                                errors.Add("> " + Lang.USED_HRS_GREATER_THAN_PROVIDED.Replace("<[EMP-CODE]>", employeeCode, StringComparison.OrdinalIgnoreCase));
                                continue;
                            }
                        }
                    }
                }
            }

            //Only commit if no errors at all
            if (errors.Any())
            {
                return Json(new { status = "error", message = string.Join("\n", errors) });
            }

            if (newEntities.Any()) await _context.tbl_employee_fund_source.AddRangeAsync(newEntities).ConfigureAwait(false);
            if (updateEntities.Any()) _context.tbl_employee_fund_source.UpdateRange(updateEntities);
            if (removeEntities.Any()) _context.tbl_employee_fund_source.RemoveRange(removeEntities);

            _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            return Json(new { status = "success", message = Lang.EMPLOYEE_FUND_SOURCE_IMPORT_SUCCESSFUL });
        }
        [HttpGet]
        public IActionResult EmployeeFundSourceExport()
        {
            string PageId = "10109";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            string FiscalYear = HttpContext.Session.GetString("fiscal_year") ?? "";
            ViewBag.FiscalYear = _settingsServices.GetFiscalYears(FiscalYear);
            ViewBag.EmployeeList = _employeeServices.GetEmployeeListBoth();
            return PartialView("Employee/_EmployeeFundSourceExport");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmployeeFundSourceExportDownload()
        {
            var sb = new StringBuilder();
            string? fiscal_year = Request.Form["FiscalYear"];
            string? EmpId = Request.Form["EmployeeList"];

            string work_hours = _settingsServices.GetHourSettings(fiscal_year, "normal_working_hrs");
            double working_hrs_day = double.TryParse(work_hours, out double parsed) ? parsed : 1;

            if (string.IsNullOrWhiteSpace(fiscal_year) || string.IsNullOrWhiteSpace(EmpId)) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }
            int emp_id = int.TryParse(EmpId, out int pEmpId) ? pEmpId : 0;
            if (emp_id < 1) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }

            string? str_start_date = _settingsServices.GetFiscalYearValue(fiscal_year, "date_from");
            string? str_end_date = _settingsServices.GetFiscalYearValue(fiscal_year, "date_to");
            if (string.IsNullOrWhiteSpace(str_start_date) || string.IsNullOrWhiteSpace(str_end_date)) { return BadRequest(new { success = false, message = Lang.msg_insufficient_info }); }

            DateTime start_date = Convert.ToDateTime(str_start_date);
            DateTime end_date = Convert.ToDateTime(str_end_date);
            string EmployeeName = _employeeServices.GetEmployeeName(emp_id);

            //Total fund source definted
            double annual_hrs_t = _context.tbl_employee_fund_source
                .Where(t => t.emp_id == emp_id
                         && t.start_date >= start_date
                         && t.end_date <= end_date)
                .Sum(t => (double?)(t.annual_hrs)) ?? 0;

            double annual_hrs_t_days = Math.Round(annual_hrs_t / working_hrs_day, 2);

            var query = from efd in _context.tbl_employee_fund_source
                        join fnd in _context.tbl_fund_source
                            on efd.fund_id equals fnd.fund_id
                        where efd.emp_id == emp_id && efd.start_date >= start_date && efd.end_date <= end_date
                        orderby fnd.fund_source ascending
                        select new
                        {
                            efd.fund_id,
                            fnd.fund_source,
                            efd.annual_hrs,
                            efd.start_date,
                            efd.end_date,
                            fnd.expiry_date
                        };
            var data = query.ToList();
            _ = sb.AppendLine($",{EmployeeName},,,,,,,,,,");
            _ = sb.AppendLine(",,Assigned Annual,,,Used Annual,,Pending,,,,");
            _ = sb.AppendLine("SN, Fund Source,Hours,Days,%,Hours,Days,Hours,Days,Fiscal Start Date,Fiscal End Date, Expiry Date");
            double annual_hrs_pt = 0;
            double used_hrs_t = 0;
            double used_hrs_t_days = 0;
            double pending_hrs_t = 0;
            double pending_hrs_t_days = 0;
            int cnt = 0;

            foreach (var row in data)
            {
                cnt++;
                int fund_id = row.fund_id;
                double annual_hrs = Convert.ToInt32(row.annual_hrs);
                double annual_hrs_days = Math.Round(annual_hrs / working_hrs_day, 2);
                double annual_hrs_p = 0;
                if (annual_hrs_t > 0) { annual_hrs_p = Math.Round(100 * annual_hrs / annual_hrs_t, 1); }
                annual_hrs_pt += annual_hrs_p;

                double used_hrs = 0;
                double used_hrs_days = 0;
                double pending_hrs = 0;
                double pending_hrs_days = 0;

                used_hrs = _context.que_timesheet_sub
                        .Where(t => (t.is_active == "A")
                                    && t.emp_id == emp_id
                                    && t.fund_id == fund_id
                                    && t.fiscal >= start_date
                                    && t.fiscal <= end_date)
                        .Sum(t => (double?)(t.time_hours + t.overtime_hours))
                        ?? 0;
                used_hrs_days = Math.Round(used_hrs / working_hrs_day, 2);
                used_hrs_t += used_hrs;
                used_hrs_t_days += used_hrs_days;

                pending_hrs = _context.que_timesheet_sub
                        .Where(t => (t.is_active == "N")
                                    && t.emp_id == emp_id
                                    && t.fund_id == fund_id
                                    && t.fiscal >= start_date
                                    && t.fiscal <= end_date)
                        .Sum(t => (double?)(t.time_hours + t.overtime_hours))
                        ?? 0;
                pending_hrs_days = Math.Round(pending_hrs / working_hrs_day, 2);
                pending_hrs_t += pending_hrs;
                pending_hrs_t_days += pending_hrs_days;

                var line = new List<string>
                {
                    cnt.ToString(),
                    row.fund_source,
                    annual_hrs.ToString(),
                    annual_hrs_days.ToString(),
                    annual_hrs_p.ToString(),
                    used_hrs.ToString(),
                    used_hrs_days.ToString(),
                    pending_hrs.ToString(),
                    pending_hrs_days.ToString(),
                    row.start_date?.ToString(_appSettings.DATE_FORMAT) ?? "",
                    row.end_date?.ToString(_appSettings.DATE_FORMAT) ?? "",
                    row.expiry_date?.ToString(_appSettings.DATE_FORMAT) ?? "",
                };

                _ = sb.AppendLine(string.Join(",", line.Select(x => $"\"{x}\"")));
            }
            annual_hrs_t = Math.Round(annual_hrs_t, 2);
            annual_hrs_t_days = Math.Round(annual_hrs_t_days, 2);
            annual_hrs_pt = Math.Round(annual_hrs_pt, 2);
            used_hrs_t = Math.Round(used_hrs_t, 2);
            used_hrs_t_days = Math.Round(used_hrs_t_days, 2);
            pending_hrs_t = Math.Round(pending_hrs_t, 2);
            pending_hrs_t_days = Math.Round(pending_hrs_t_days, 2);
            _ = sb.AppendLine($",Total,{annual_hrs_t.ToString()},{annual_hrs_t_days.ToString()},{annual_hrs_pt.ToString()},{used_hrs_t.ToString()},{used_hrs_t_days.ToString()},{pending_hrs_t.ToString()},{pending_hrs_t_days.ToString()},,,");

            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"EmployeeFundSourceExport_{DateTime.Now:yyyyMMddHHmmss}.csv";
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            var filePath = Path.Combine(GblDocumentPath, "temp", fileName);
            System.IO.File.WriteAllBytes(filePath, bytes);

            return Json(new { status = "success", message = "Export successful!", url = "/uploads/temp/" + fileName });
        }
        #endregion
        /********************************************************************************************************************/
        #region 10106 EDUCATION
        [HttpGet]
        public IActionResult Education()
        {
            string PageId = "10106";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                        from edu in _context.tbl_employee_education
                        join emp in _context.tbl_employee
                            on edu.emp_id equals emp.emp_id
                        orderby edu.emp_id descending
                        select new EmployeeEducationViewModel
                        {
                            emp_edu_id = edu.emp_edu_id,
                            slc_board = edu.slc_board,
                            bch_board = edu.bch_board,
                            hgt_board = edu.hgt_board,
                            slc_passed_year = edu.slc_passed_year,
                            bch_passed_year = edu.bch_passed_year,
                            hgt_passed_year = edu.hgt_passed_year,
                            slc_division = edu.slc_division,
                            bch_division = edu.bch_division,
                            hgt_division = edu.hgt_division,
                            slc_major = edu.slc_major,
                            bch_major = edu.bch_major,
                            hgt_major = edu.hgt_major,
                            remarks = edu.remarks,
                        }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Education", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_Education", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EducationList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from edu in _context.tbl_employee_education
                        join emp in _context.tbl_employee
                            on edu.emp_id equals emp.emp_id
                        select new EmployeeEducationViewModel
                        {
                            emp_edu_id = edu.emp_edu_id,
                            slc_board = edu.slc_board,
                            bch_board = edu.bch_board,
                            hgt_board = edu.hgt_board,
                            slc_passed_year = edu.slc_passed_year,
                            bch_passed_year = edu.bch_passed_year,
                            hgt_passed_year = edu.hgt_passed_year,
                            slc_division = edu.slc_division,
                            bch_division = edu.bch_division,
                            hgt_division = edu.hgt_division,
                            slc_major = edu.slc_major,
                            bch_major = edu.bch_major,
                            hgt_major = edu.hgt_major,
                            remarks = edu.remarks,
                            emp_status = emp.emp_status,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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

        public IActionResult EducationAddEdit(int? id, string mode)
        {
            string PageId = "10106";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeNotHavingEducation();

            EmployeeEducationViewModel model;
            model = new EmployeeEducationViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_EducationAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from edu in _context.tbl_employee_education
                              join emp in _context.tbl_employee
                                  on edu.emp_id equals emp.emp_id
                              where edu.emp_edu_id == id
                              select new
                              {
                                  edu.emp_edu_id,
                                  edu.slc_board,
                                  edu.bch_board,
                                  edu.hgt_board,
                                  edu.slc_passed_year,
                                  edu.bch_passed_year,
                                  edu.hgt_passed_year,
                                  edu.slc_division,
                                  edu.bch_division,
                                  edu.hgt_division,
                                  edu.slc_major,
                                  edu.bch_major,
                                  edu.hgt_major,
                                  edu.remarks,
                                  emp.emp_status,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_id,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})"
                              }).FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new EmployeeEducationViewModel
                    {
                        emp_edu_id = ec.emp_edu_id,
                        slc_board = ec.slc_board,
                        bch_board = ec.bch_board,
                        hgt_board = ec.hgt_board,
                        slc_passed_year = ec.slc_passed_year,
                        bch_passed_year = ec.bch_passed_year,
                        hgt_passed_year = ec.hgt_passed_year,
                        slc_division = ec.slc_division,
                        bch_division = ec.bch_division,
                        hgt_division = ec.hgt_division,
                        slc_major = ec.slc_major,
                        bch_major = ec.bch_major,
                        hgt_major = ec.hgt_major,
                        remarks = ec.remarks,
                        emp_status = ec.emp_status,
                        firstname = ec.firstname,
                        middlename = ec.middlename,
                        lastname = ec.lastname,
                        emp_id = ec.emp_id,
                        employee = ec.employee
                    };
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_EducationAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EducationSave(EmployeeEducationViewModel model)
        {
            _ = ModelState.Remove("emp_edu_id");
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10106", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (mode == "add")
            {
                int emp_edu_id = (_context.tbl_employee_education.Any()
                ? _context.tbl_employee_education.Max(o => o.emp_edu_id)
                : 0) + 1;

                var EducationSave = new tbl_employee_education
                {
                    emp_edu_id = emp_edu_id,
                    emp_id = model.emp_id,

                    slc_board = model.slc_board,
                    bch_board = model.bch_board,
                    hgt_board = model.hgt_board,

                    slc_passed_year = model.slc_passed_year,
                    bch_passed_year = model.bch_passed_year,
                    hgt_passed_year = model.hgt_passed_year,

                    slc_division = model.slc_division,
                    bch_division = model.bch_division,
                    hgt_division = model.hgt_division,

                    slc_major = model.slc_board,
                    bch_major = model.bch_major,
                    hgt_major = model.hgt_major,

                    remarks = model.remarks,
                };
                _ = _context.tbl_employee_education.Add(EducationSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                var EducationUpdate = _context.tbl_employee_education.FirstOrDefault(h => h.emp_id == model.emp_id && h.emp_edu_id == model.emp_edu_id);
                if (EducationUpdate == null) { return Json(new { status = "error", message = Lang.msg_no_record_found }); }

                EducationUpdate.slc_board = model.slc_board;
                EducationUpdate.bch_board = model.bch_board;
                EducationUpdate.hgt_board = model.hgt_board;

                EducationUpdate.slc_passed_year = model.slc_passed_year;
                EducationUpdate.bch_passed_year = model.bch_passed_year;
                EducationUpdate.hgt_passed_year = model.hgt_passed_year;

                EducationUpdate.slc_division = model.slc_division;
                EducationUpdate.bch_division = model.bch_division;
                EducationUpdate.hgt_division = model.hgt_division;

                EducationUpdate.slc_major = model.slc_major;
                EducationUpdate.bch_major = model.bch_major;
                EducationUpdate.hgt_major = model.hgt_major;

                EducationUpdate.remarks = model.remarks;

                _ = _context.tbl_employee_education.Update(EducationUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, emp_id = model.emp_edu_id });
            }
            return Json(new { status = "error", message = Lang.msg_error_invalid });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EducationDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10106", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_education
                .Where(r => request.SelectedIds.Contains(r.emp_edu_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_education.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", request.SelectedIds.Count.ToString())
            });
        }

        #endregion        
        /********************************************************************************************************************/
        #region 10105   DOCUMENTS
        public IActionResult Documents()
        {
            string PageId = "10105";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from emp in _context.tbl_employee
                orderby emp.firstname ascending, emp.middlename ascending, emp.lastname ascending
                select new EmployeeDocumentsClassicViewModel
                {
                    id = emp.emp_id,
                    citizenship_copy = emp.citizenship_copy ?? "",
                    passport_copy = emp.passport_copy ?? "",
                    pan_copy = emp.pan_copy ?? "",
                    nin_copy = emp.nin_copy ?? "",
                    employee = $"{ emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            return PartialView("Employee/_Documents", Records);
        }
        [HttpPost]
        public async Task<IActionResult> DocumentsList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from emp in _context.tbl_employee
                        select new EmployeeDocumentsClassicViewModel
                        {
                            id = emp.emp_id,
                            citizenship_copy = emp.citizenship_copy ?? "",
                            passport_copy = emp.passport_copy ?? "",
                            pan_copy = emp.pan_copy ?? "",
                            nin_copy = emp.nin_copy ?? "",
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
                    (a.employee != null && a.employee.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult DocumentsAddEdit(int? id, string mode)
        {
            string PageId = "10105";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");

            EmployeeDocumentsClassicViewModel model;
            model = new EmployeeDocumentsClassicViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_DocumentsAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from emp in _context.tbl_employee
                              where emp.emp_id == Convert.ToInt32(id)
                              select new
                              {
                                  emp.emp_id,
                                  emp.citizenship_copy,
                                  emp.passport_copy,
                                  emp.pan_copy,
                                  emp.nin_copy,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status,
                                  emp.emp_code
                              }).AsNoTracking().FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new EmployeeDocumentsClassicViewModel
                    {
                        id = ec.emp_id,
                        citizenship_copy = ec.citizenship_copy ?? "",
                        passport_copy = ec.passport_copy ?? "",
                        pan_copy = ec.pan_copy ?? "",
                        nin_copy = ec.nin_copy ?? "",
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    string fileSizeCitizen = "0";
                    string fileSizePassport = "0";
                    string fileSizePan = "0";
                    string fileSizeNid = "0";
                    string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];

                    if (!string.IsNullOrEmpty(ec.citizenship_copy))
                    {
                        string extension = Path.GetExtension(ec.citizenship_copy).TrimStart('.').ToUpperInvariant();
                        ViewBag.previewCitizenship = $@"
                            <a href='{Url.Content($"~/Employee/DocumentClassicDownload/{id}^citizenship")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";

                        string uploadsFolderCitizen = Path.Combine(GblDocumentPath, "documents/citizenship", ec.citizenship_copy);
                        fileSizeCitizen = GetFileSize(uploadsFolderCitizen);
                    }
                    if (!string.IsNullOrEmpty(ec.passport_copy))
                    {
                        string extension = Path.GetExtension(ec.passport_copy).TrimStart('.').ToUpperInvariant();
                        ViewBag.previewPassport = $@"
                            <a href='{Url.Content($"~/Employee/DocumentClassicDownload/{id}^passport")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";
                        string uploadsFolderPassport = Path.Combine(GblDocumentPath, "documents/passport", ec.passport_copy);
                        fileSizePassport = GetFileSize(uploadsFolderPassport);
                    }
                    if (!string.IsNullOrEmpty(ec.pan_copy))
                    {
                        string extension = Path.GetExtension(ec.pan_copy).TrimStart('.').ToUpperInvariant();
                        ViewBag.previewPan = $@"
                            <a href='{Url.Content($"~/Employee/DocumentClassicDownload/{id}^pan")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";
                        string uploadsFolderPan = Path.Combine(GblDocumentPath, "documents/pan", ec.pan_copy);
                        fileSizePan = GetFileSize(uploadsFolderPan);
                    }
                    if (!string.IsNullOrEmpty(ec.nin_copy))
                    {
                        string extension = Path.GetExtension(ec.nin_copy).TrimStart('.').ToUpperInvariant();
                        ViewBag.previewNid = $@"
                            <a href='{Url.Content($"~/Employee/DocumentClassicDownload/{id}^nid")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";
                        string uploadsFolderNid = Path.Combine(GblDocumentPath, "documents/nid", ec.nin_copy);
                        fileSizeNid = GetFileSize(uploadsFolderNid);
                    }

                    ViewBag.fileSizeCitizen = fileSizeCitizen;
                    ViewBag.fileSizePassport = fileSizePassport;
                    ViewBag.fileSizePan = fileSizePan;
                    ViewBag.fileSizeNid = fileSizeNid;
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_DocumentsAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DocumentsSave(int? id, IFormFile file, string? fileType)
        {
            if (!_accountServices.HasPermission("10105", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (!FileValidator.ForImagesWithPdf(file)) { return Json(new { status = "error", message = "There is problem with File." }); }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/" + fileType);
            string propertyName = fileType + "_copy";
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var employee = await _context.tbl_employee.FirstOrDefaultAsync(e => e.emp_id == id).ConfigureAwait(false);
            if (employee != null && file != null)
            {
                // Delete old file if exists
                var existingFileName = employee.GetType()
                                                   .GetProperty(propertyName)?
                                                   .GetValue(employee) as string;
                if (!string.IsNullOrWhiteSpace(existingFileName))
                {
                    string delStatus = DeleteFile(uploadsFolder, existingFileName);
                }

                UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                {
                    // Set new file name dynamically
                    employee.GetType().GetProperty(propertyName)?.SetValue(employee, filename);

                    if (fileType == "citizenship")
                    {
                        employee.citizenship_copy = filename;
                    }
                    if (fileType == "passport")
                    {
                        employee.passport_copy = filename;
                    }
                    if (fileType == "pan")
                    {
                        employee.pan_copy = filename;
                    }
                    if (fileType == "nin")
                    {
                        employee.nin_copy = filename;
                    }
                    _ = _context.Update(employee);
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    return Json(new { status = "success", message = Lang.msg_update_success, id = employee.emp_id, fileType, extension });
                }
            }
            return Json(new { status = "error", message = "File could not be uploaded." });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DocumentsDelete(int id, string delType)
        {
            if (!_accountServices.HasPermission("10105", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (id < 1 || string.IsNullOrWhiteSpace(delType)) { return Json(new { status = "fail", message = Lang.msg_insufficient_info }); }

            string existingFileName = "";
            var docFileName = _context.tbl_employee.Where(e => e.emp_id == id).FirstOrDefault();
            if (docFileName != null)
            {
                if (delType == "citizenship")
                {
                    existingFileName = docFileName.citizenship_copy ?? "";
                }
                else if (delType == "passport")
                {
                    existingFileName = docFileName.passport_copy ?? "";
                }
                else if (delType == "pan")
                {
                    existingFileName = docFileName.pan_copy ?? "";
                }
                else if (delType == "nin")
                {
                    existingFileName = docFileName.nin_copy ?? "";
                }
                else
                {
                    existingFileName = "";
                }
            }
            if (string.IsNullOrWhiteSpace(existingFileName)) { return Json(new { status = "fail", message = Lang.msg_insufficient_info }); }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/" + delType);
            string st = DeleteFile(uploadsFolder, existingFileName);
            if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase)) { return Json(new { status = "fail", message = "Fail to delete file." }); }
            if (delType == "citizenship")
            {
                docFileName.citizenship_copy = "";
            }
            else if (delType == "passport")
            {
                docFileName.passport_copy = "";
            }
            else if (delType == "pan")
            {
                docFileName.pan_copy = "";
            }
            else if (delType == "nin")
            {
                docFileName.nin_copy = "";
            }
            _ = _context.tbl_employee.Update(docFileName);
            _ = _context.SaveChanges();

            return Json(new { status = "success", message = "File deleted successfully.", delType });
        }
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> DocumentClassicDownload(string? id)
        {
            string PageId = "10105";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var parts = id.Split('^');

            string? idOnly = parts[0];
            string? type = parts[1];

            var smt = _context.tbl_employee.FirstOrDefault(h => h.emp_id == Convert.ToInt32(idOnly));
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.citizenship_copy) && string.IsNullOrWhiteSpace(smt.passport_copy) && string.IsNullOrWhiteSpace(smt.pan_copy) && string.IsNullOrWhiteSpace(smt.nin_copy))
            {
                return NotFound();
            }
            else
            {
                string? folder_name = "";
                string? field_name = "";
                if (type == "citizenship")
                {
                    folder_name = "citizenship";
                    field_name = smt.citizenship_copy;
                }
                else if (type == "passport")
                {
                    folder_name = "passport";
                    field_name = smt.passport_copy;
                }
                else if (type == "pan")
                {
                    folder_name = "pan";
                    field_name = smt.pan_copy;
                }
                else if (type == "nin")
                {
                    folder_name = "nin";
                    field_name = smt.nin_copy;
                }
                else
                {
                    return NotFound();
                }

                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "documents/" + folder_name);

                string filePath = Path.Combine(uploadsFolder, field_name);
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
                        return File(fileBytes, contentType, field_name);
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
        #region DEPENDENT DETAIL 	10103
        [HttpGet]
        public IActionResult DependentDetail(string StatusFilter)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            //Get dependent setting
            int max_nos_dep_child_eligible_paid = 0;
            DateTime? age_checking_date = DateTime.Now;
            var strsql = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (strsql != null)
            {
                max_nos_dep_child_eligible_paid = strsql.max_nos_dep_child_eligible_paid;
                age_checking_date = Convert.ToDateTime(strsql.age_checking_date);
            }
            var Records = (from dep in _context.tbl_employee_dependent_children_details
                           join emp in _context.tbl_employee
                           on dep.emp_id equals emp.emp_id
                           where emp.emp_status == "A"
                           orderby emp.firstname ascending, emp.middlename ascending, emp.lastname ascending, dep.date_of_birth ascending
                           select new EmployeeDependentChildrenDetailsViewModel
                           {
                               id = dep.emp_dep_id, //view problem keeping emp_dep_id
                               employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                               c_name = dep.c_name,
                               gender = dep.gender,
                               date_of_birth = dep.date_of_birth,
                               dob_file_name = dep.dob_file_name,
                               submit_date = dep.submit_date,
                               update_date = dep.update_date,
                               status = dep.eligibility,
                               remarks = dep.remarks,
                               dependentAge = (age_checking_date.HasValue && dep.date_of_birth.HasValue) ? Math.Round(((age_checking_date.Value - dep.date_of_birth.Value).Days + 1) / 365.0, 2) : 0,
                               isReceiptReq = "",
                               receipt = ""
                           }).ToList();

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.EligibilityStatusFilter = EmployeeServices.EligibilityStatus();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/DependentDetail", "ADD", PageId, Records.Count);
            return PartialView("Employee/_DependentDetail", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DependentDetailList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string? EmployeeStatusFilter = request.FilterValue1;
            string? StatusFilter = request.FilterValue2;

            //Get dependent setting
            int max_nos_dep_child_eligible_paid = 0;
            DateTime? age_checking_date = DateTime.Now;
            var strsql = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (strsql != null)
            {
                max_nos_dep_child_eligible_paid = strsql.max_nos_dep_child_eligible_paid;
                age_checking_date = Convert.ToDateTime(strsql.age_checking_date);
            }
            string fiscal_year = HttpContext.Session.GetString("fiscal_year");

            var joined = await (from dep in _context.tbl_employee_dependent_children_details
                                join emp in _context.tbl_employee
                                    on dep.emp_id equals emp.emp_id
                                orderby dep.emp_id descending
                                select new
                                {
                                    dep.emp_dep_id,
                                    dep.emp_id,
                                    emp.firstname,
                                    emp.middlename,
                                    emp.lastname,
                                    emp.emp_code,
                                    emp.emp_status,
                                    dep.c_name,
                                    dep.gender,
                                    dep.date_of_birth,
                                    dep.dob_file_name,
                                    dep.submit_date,
                                    dep.update_date,
                                    dep.remarks,
                                    status = dep.eligibility,
                                    receipt = ""
                                }).ToListAsync().ConfigureAwait(false);

            var query = joined.Select(dep => new EmployeeDependentChildrenDetailsViewModel
            {
                id = dep.emp_dep_id,
                emp_id = dep.emp_id,
                firstname = dep.firstname,
                middlename = dep.middlename,
                lastname = dep.lastname,
                employee = $"{dep.firstname} {dep.middlename} {dep.lastname} ({dep.emp_code})",
                emp_status = dep.emp_status,
                isReceiptReq = dep.date_of_birth.HasValue
            ? _employeeServices.IsDependentNeedReceipt(dep.emp_dep_id, dep.date_of_birth.Value, Convert.ToDateTime(age_checking_date), fiscal_year)
            : "false",
                c_name = dep.c_name,
                gender = dep.gender,
                date_of_birth = dep.date_of_birth,
                dob_file_name = dep.dob_file_name,
                dependentAge = (age_checking_date.HasValue && dep.date_of_birth.HasValue) ? Math.Round(((age_checking_date.Value - dep.date_of_birth.Value).Days + 1) / 365.0, 2) : 0,
                submit_date = dep.submit_date,
                update_date = dep.update_date,
                remarks = dep.remarks,
                status = dep.status,
                receipt = ""
            }).AsQueryable();

            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);
            }
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.status == StatusFilter);
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
                    (a.c_name != null && a.c_name.Contains(searchValue)) ||
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult DependentDetailAddEdit(int? id, string mode)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.Gender = GenderList();
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;

            EmployeeDependentChildrenDetailsViewModel model;
            model = new EmployeeDependentChildrenDetailsViewModel();
            if (mode == "add")
            {
                model.emp_id = model.emp_id;
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_DependentDetailAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id < 1 || id == null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from dep in _context.tbl_employee_dependent_children_details
                              join emp in _context.tbl_employee
                              on dep.emp_id equals emp.emp_id
                              where dep.emp_dep_id == id
                              select new
                              {
                                  dep.emp_dep_id,
                                  dep.c_name,
                                  dep.gender,
                                  dep.date_of_birth,
                                  dep.dob_file_name,
                                  dep.submit_date,
                                  dep.update_date,
                                  dep.eligibility,
                                  dep.remarks,
                                  dep.emp_id,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();
                    if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeDependentChildrenDetailsViewModel
                    {
                        id = ec.emp_dep_id, //view problem keeping emp_dep_id
                        c_name = ec.c_name,
                        gender = ec.gender,
                        date_of_birth = ec.date_of_birth,
                        dob_file_name = ec.dob_file_name,
                        submit_date = ec.submit_date,
                        update_date = ec.update_date,
                        status = ec.eligibility,
                        remarks = ec.remarks,
                        emp_id = ec.emp_id,
                        emp_status = ec.emp_status
                    };
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                    string fileSize = "0";
                    if (ec.dob_file_name != null) {
                        string uploadsFolder = Path.Combine(GblDocumentPath, "documents/dependent", ec.dob_file_name);
                        fileSize = GetFileSize(uploadsFolder);
                    }
                    ViewBag.fileSize = fileSize;
                    ViewBag.Employee = ec.employee;
                    return PartialView("Employee/_DependentDetailAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DependentDetailSave(EmployeeDependentChildrenDetailsViewModel model, IFormFile? file)
        {
            //DebugModelState(ModelState);
            _ = ModelState.Remove("id"); //view problem keeping emp_dep_id
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10103", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (file != null && file.Length > 0)
            {
                if (!FileValidator.ForImagesWithPdf(file)) { return Json(new { status = "error", message = "There is problem with File." }); }
            }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/dependent");

            string? c_name = model.c_name;
            string? gender = model.gender;
            DateTime? date_of_birth = model.date_of_birth;
            DateTime? submit_date = System.DateTime.Now;
            DateTime? update_date = System.DateTime.Now;
            string? eligibility = model.status;
            string? remarks = model.remarks;
            int emp_id = model.emp_id ?? 0;

            if (mode == "add")
            {
                int emp_dep_id = (_context.tbl_employee_dependent_children_details.Any()
                              ? _context.tbl_employee_dependent_children_details.Max(o => o.emp_dep_id)
                              : 0) + 1;
                var DataSave = new tbl_employee_dependent_children_details
                {
                    emp_dep_id = emp_dep_id,
                    c_name = c_name,
                    gender = gender,
                    date_of_birth = date_of_birth,
                    dob_file_name = null,
                    submit_date = submit_date,
                    update_date = update_date,
                    remarks = remarks,
                    eligibility = "P",
                    emp_id = emp_id
                };
                _ = _context.tbl_employee_dependent_children_details.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();
                //upload dob file
                var isDataSaved = _context.tbl_employee_dependent_children_details.FirstOrDefault(u => u.emp_dep_id == emp_dep_id);
                if (isDataSaved == null) { return Json(new { status = "false", message = "Fail to save dependent detail." });}
                if (file != null && file.Length > 0)
                {
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        isDataSaved.dob_file_name = filename;
                        isDataSaved.update_date = DateTime.Now;

                        _ = _context.tbl_employee_dependent_children_details.Update(isDataSaved);
                        _ = _context.SaveChanges();
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Dependent detail saved successfully, however the associated file could not be uploaded." });
                    }
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_dep_id });
            }
            else if (mode == "edit")
            {
                //int emp_dep_id = model.id;//view problem keeping emp_dep_id
                int emp_dep_id = Convert.ToInt32(Request.Form["id"]);
                if (emp_dep_id < 1) { return Json(new { status = "invalid", message = Lang.msg_insufficient_info}); }

                var DataUpdate = _context.tbl_employee_dependent_children_details.FirstOrDefault(h => h.emp_dep_id == emp_dep_id);
                if (DataUpdate != null)
                {
                    DataUpdate.c_name = c_name;
                    DataUpdate.gender = gender;
                    DataUpdate.date_of_birth = date_of_birth;
                    DataUpdate.update_date = DateTime.Now;
                    DataUpdate.remarks = remarks;

                    //if there is new dob certificate uploaded
                    if (file != null && file.Length > 0)
                    {
                        /** DELETE EXISTING FILE | instead of taking from post, better to get from db | security reason **/
                        string hUploadFile = DataUpdate.dob_file_name ?? "";
                        if (!string.IsNullOrWhiteSpace(hUploadFile))
                        {
                            string st = DeleteFile(uploadsFolder, hUploadFile);
                            if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                            {
                                return Json(new { status = "false", message = "Failed to overwite existing birth certificatee. Please contact your system administrator." });
                            }
                        }
                        UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                        if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                        {
                            DataUpdate.dob_file_name = filename;
                        }
                        else
                        {
                            return Json(new { status = "false", message = "Failed to update document file. Please contact your system administrator." });
                        }
                    }
                    DataUpdate.update_date = DateTime.Now;
                    _ = _context.tbl_employee_dependent_children_details.Update(DataUpdate);
                    _ = _context.SaveChanges();
                }
                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate?.emp_dep_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> DependentDobCDownload(string id)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_employee_dependent_children_details.FirstOrDefault(h => h.emp_dep_id == Convert.ToInt32(id));
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.dob_file_name))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "documents/dependent");

                string filePath = Path.Combine(uploadsFolder, smt.dob_file_name);
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
                        return File(fileBytes, contentType, smt.dob_file_name);
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

        [HttpGet]
        public IActionResult DependentDetailReceipt(int? id, string mode)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            var sb = new StringBuilder();

            //Get dependent setting
            int max_nos_dep_child_eligible_paid = 0;
            DateTime? age_checking_date = DateTime.Now;
            var strsql = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (strsql != null)
            {
                max_nos_dep_child_eligible_paid = strsql.max_nos_dep_child_eligible_paid;
                age_checking_date = Convert.ToDateTime(strsql.age_checking_date);
            }
            string fiscal_year = HttpContext.Session.GetString("fiscal_year") ?? "";

            ViewBag.mode = mode;
            var Records = (
                from dep in _context.tbl_employee_dependent_children_details
                join emp in _context.tbl_employee
                on dep.emp_id equals emp.emp_id
                where dep.emp_dep_id == id
                select new EmployeeDependentChildrenDetailsViewModel
                {
                    id = dep.emp_dep_id,
                    emp_id = dep.emp_id,
                    firstname = emp.firstname,
                    middlename = emp.middlename,
                    lastname = emp.lastname,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status,
                    c_name = dep.c_name,
                    gender = dep.gender,
                    date_of_birth = dep.date_of_birth,
                    dob_file_name = dep.dob_file_name,
                    submit_date = dep.submit_date,
                    update_date = dep.update_date,
                    remarks = dep.remarks,
                    status = dep.eligibility,
                    receipt = ""
                }).FirstOrDefault();

            string EmployeeName = Records.employee ?? "";
            string emp_status = Records.emp_status == "A" ? "Active" : "Inactive";
            string DependentName = Records.c_name ?? "";
            string DepStatus = Records.status == "P" ? "Pending" : Records.status == "A" ? "Active" : "Inactive";
            /*isReceiptReq = dep.date_of_birth.HasValue
            ? _employeeServices.IsDependentNeedReceipt(dep.emp_dep_id, dep.date_of_birth.Value, Convert.ToDateTime(age_checking_date), fiscal_year)
            : "false",
            dependentAge = (age_checking_date.HasValue && dep.date_of_birth.HasValue) ? Math.Round(((age_checking_date.Value - dep.date_of_birth.Value).Days + 1) / 365.0, 2) : 0,
             */
            var DetailsSub = (
                from sub in _context.tbl_employee_dependent_children_details_sub
                where sub.emp_dep_id == id
                orderby sub.fiscal_year descending
                select new EmployeeDependentChildrenDetailsSubViewModel
                {
                    emp_dep_sub_id = sub.emp_dep_sub_id,
                    emp_dep_id = sub.emp_dep_id,
                    fiscal_year = sub.fiscal_year,
                    file_name = sub.file_name,
                    status = sub.status,
                    submit_date = sub.submit_date,
                    update_date = sub.update_date
                }).ToList();
            if (DetailsSub == null)
            {
                _ = sb.AppendLine($@"<tr class=""bg-silver bg-opacity-27""><th width=""100%"">No Record(s) found.</th></tr>");
            }
            else
            {
                int cnt = 0;
                foreach (var record in DetailsSub)
                {
                    cnt++;
                    string? submit_date = record.submit_date.ToString();
                    string? update_date = record.update_date.ToString();
                    string? s_submit_date = _settingsServices.DateformatToDt(submit_date ?? string.Empty);
                    string? s_update_date = _settingsServices.DateformatToDt(update_date ?? string.Empty);
                    int emp_dep_sub_id = record.emp_dep_sub_id;
                    string fiscalYear = record.fiscal_year ?? "";
                    string file_name = record.file_name ?? "";
                    string status = record.status == "P" ? "Pendiing" : record.status == "A" ? "Active" : record.status == "I" ? "Inactive" : "";

                    _ = sb.AppendLine($@"
                        <tr class=""bg-silver bg-opacity-27"">
                            <th width=""1%"">{cnt}</th>
                            <th>{fiscalYear}</th>
                            <th>{s_submit_date}</th>
                            <th>{s_update_date}</th>
                            <th>{status}</th>
                            <th><a href=""#"" class=""view-receipt-file"" data-id=""{emp_dep_sub_id}""><img src=""/images/receipt.png"" title = ""Receipt""></a></th>
                        </tr>
                        <tr class=""bg-silver bg-opacity-27""><th width=""100%"" colspan=""14""><hr></th>    
	                    ");
                }
            }
            ViewBag.EmployeeName = EmployeeName;
            ViewBag.emp_status = emp_status;
            ViewBag.DependentName = DependentName;
            ViewBag.DepStatus = DepStatus;
            ViewBag.EmployeeDependentReceiptHtml = sb.ToString();
            return PartialView("Employee/_DependentDetailReceipt", DetailsSub);
        }
        #endregion
        /********************************************************************************************************************/
        #region POSITION PROFILE 10115

        [HttpGet]
        public IActionResult PositionProfile()
        {
            string PageId = "10115";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from emp in _context.tbl_employee
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new EmployeePositionProfileViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    employee_type = emp.employee_type,
                    job_family = emp.job_family,
                    emp_level = emp.emp_level,
                    department = emp.department,
                    post = emp.post
                }
            ).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.EmployeeType = EmployeeServices.GetEmployeeType();
            ViewBag.JobFamily = EmployeeServices.GetJobFamily();
            ViewBag.CareerLevel = EmployeeServices.GetCareerLevel();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Employee/_PositionProfile", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PositionProfileList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string EmployeeStatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query =
                from emp in _context.tbl_employee
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new EmployeePositionProfileViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    employee_type = emp.employee_type,
                    job_family = emp.job_family,
                    emp_level = emp.emp_level,
                    department = emp.department,
                    post = emp.post
                };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(emp => emp.emp_status == EmployeeStatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.employee != null && a.employee.Contains(searchValue))
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PositionProfileSave([FromBody] EmployeePositionProfileListViewModel model)
        {
            string PageId = "10115";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            int update_by = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            foreach (var emp in model.Fields)
            {
                var existing = _context.tbl_employee.FirstOrDefault(e => e.emp_id == emp.emp_id);
                if (existing != null)
                {
                    existing.employee_type = emp.employee_type;
                    existing.job_family = emp.job_family;
                    existing.emp_level = emp.emp_level;
                    existing.department = emp.department;
                    existing.post = emp.post;
                    _ = _context.tbl_employee.Update(existing);

                    var DataSave = new tbl_employee_history
                    {
                        emp_id = emp.emp_id,
                        employee_type = emp.employee_type,
                        job_family = emp.job_family,
                        emp_level = emp.emp_level,
                        department = emp.department,
                        post = emp.post,
                        by_emp_id = update_by,
                        update_date = DateTime.Now
                    };
                    _ = _context.tbl_employee_history.Add(DataSave);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });

        }
        #endregion
        /********************************************************************************************************************/
        #region MANAGERS 10114

        [HttpGet]
        public IActionResult Manager()
        {
            string PageId = "10114";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from emp in _context.tbl_employee
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new EmployeeManagerViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    manager_id = emp.manager_id,
                    line_manager_id = emp.line_manager_id,
                    alt_manager_id = emp.alt_manager_id,
                    alt_line_manager_id = emp.alt_line_manager_id
                }
            ).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ManagerList = _employeeServices.GetManagerListBoth(0);
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Employee/_Manager", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManagerList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string EmployeeStatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query =
                from emp in _context.tbl_employee
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new EmployeeManagerViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    manager_id = emp.manager_id,
                    line_manager_id = emp.line_manager_id,
                    alt_manager_id = emp.alt_manager_id,
                    alt_line_manager_id = emp.alt_line_manager_id
                };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(emp => emp.emp_status == EmployeeStatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.employee != null && a.employee.Contains(searchValue))
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ManagerSave([FromBody] EmployeeManagerListViewModel model)
        {
            string PageId = "10114";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            int update_by = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            foreach (var emp in model.Fields)
            {
                var existing = _context.tbl_employee.FirstOrDefault(e => e.emp_id == emp.emp_id);
                if (existing != null)
                {
                    existing.manager_id = emp.manager_id;
                    existing.line_manager_id = emp.line_manager_id;
                    existing.alt_manager_id = emp.alt_manager_id;
                    existing.alt_line_manager_id = emp.alt_line_manager_id;
                    _ = _context.tbl_employee.Update(existing);

                    var DataSave = new tbl_employee_history
                    {
                        emp_id = emp.emp_id,
                        manager_id = emp.manager_id,
                        line_manager_id = emp.line_manager_id,
                        by_emp_id = update_by,
                        update_date = DateTime.Now,
                    };
                    _ = _context.tbl_employee_history.Add(DataSave);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });

        }
        #endregion
        /********************************************************************************************************************/
        #region EMPLOYEE    10107
        [HttpGet]
        public IActionResult Employee()
        {
            string PageId = "10107";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from emp in _context.tbl_employee
                join pho in _context.tbl_employee_photo
                on emp.emp_id equals pho.emp_id into empPhotos
                from pho in empPhotos.DefaultIfEmpty()   // LEFT OUTER JOIN
                orderby emp.emp_status ascending,
                emp.firstname ascending, emp.middlename ascending, emp.lastname ascending
                select new EmployeeViewModel
                {
                    emp_id = emp.emp_id,
                    emp_code = emp.emp_code,
                    title = emp.title,
                    e_mail = emp.e_mail,
                    manager_id = emp.manager_id,
                    line_manager_id = emp.line_manager_id,
                    emp_status = emp.emp_status,
                    photo = pho.photo
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Employee", "ADD|EXPORT|DEL", PageId, Records.Count);
            return PartialView("Employee/_Employee", Records);
        }
        [HttpPost]
        public async Task<IActionResult> EmployeeList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var EmployeeStatusFilter = request.FilterValue1;/*Dropdwon Filter*/

            var query = from ec in _context.tbl_employee
                        // FOR LINE IMMEDIATE SUPERVISOR ID JOIN
                        join cd in _context.tbl_employee
                            on ec.manager_id equals cd.emp_id into cdGroup
                        from cd in cdGroup.DefaultIfEmpty()
                            // FOR LINE LINE DIRECTOR ID JOIN
                        join ld in _context.tbl_employee
                            on ec.line_manager_id equals ld.emp_id into ldGroup
                        from ld in ldGroup.DefaultIfEmpty()
                            // FOR EMPLOYEE PHOTO ID JOIN
                        join ep in _context.tbl_employee_photo
                            on ec.emp_id equals ep.emp_id into epGroup
                        from ep in epGroup.DefaultIfEmpty()
                            // FOR TBL USER JOIN
                        join tu in _context.tbl_user
                            on ec.emp_id equals tu.emp_id into tuGroup
                        from tu in tuGroup.DefaultIfEmpty()

                        orderby ec.emp_id descending
                        select new EmployeeViewModel
                        {
                            emp_id = ec.emp_id,
                            employee = (ec.firstname ?? "") + (string.IsNullOrEmpty(ec.middlename) ? "" : " " + ec.middlename) + (string.IsNullOrEmpty(ec.lastname) ? "" : " " + ec.lastname),
                            employee_immediate = (cd.firstname ?? "") + (string.IsNullOrEmpty(cd.middlename) ? "" : " " + cd.middlename) + (string.IsNullOrEmpty(cd.lastname) ? "" : " " + cd.lastname),
                            employee_line = (ld.firstname ?? "") + (string.IsNullOrEmpty(ld.middlename) ? "" : " " + ld.middlename) + (string.IsNullOrEmpty(ld.lastname) ? "" : " " + ld.lastname),
                            emp_code = ec.emp_code,
                            e_mail = ec.e_mail,
                            history = "<a href='#'>View</a>",
                            emp_status = ec.emp_status,
                            photo = ep.photo
                        };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.employee != null && a.employee.Contains(searchValue)) ||
                    (a.e_mail != null && a.e_mail.Contains(searchValue)) ||
                    (a.employee_immediate != null && a.employee_immediate.Contains(searchValue)) ||
                    (a.employee_line != null && a.employee_line.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult EmployeeAddEdit(string id, string mode)
        {
            string PageId = "10107";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.Gender = GenderList();
            ViewBag.MaritalStatus = MaritalStatusList();
            ViewBag.EmployeeType = EmployeeServices.GetEmployeeType();
            ViewBag.EmployeeTypeSub = EmployeeServices.GetEmployeeTypeSub();
            ViewBag.JobFamily = EmployeeServices.GetJobFamily();
            ViewBag.CareerLevel = EmployeeServices.GetCareerLevel();

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            EmployeeViewModel model;
            model = new EmployeeViewModel();
            if (mode == "add")
            {
                ViewBag.EmpCode = _context.Database.SqlQuery<string>($"EXEC EmployeeCode").AsEnumerable().FirstOrDefault();
                ViewBag.ManagerList = _employeeServices.GetManagerListActiveOnly(0);
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_EmployeeAddEdit", model);
            }
            else if (mode == "edit")
            {
                int emp_id = int.TryParse(id, out int parseId) ? parseId : 0;
                if (emp_id is < 1)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (
                            from emp in _context.tbl_employee
                            join pho in _context.tbl_employee_photo
                            on emp.emp_id equals pho.emp_id into empPhotos
                            from pho in empPhotos.DefaultIfEmpty()   // LEFT OUTER JOIN
                            where emp.emp_id == emp_id
                            select new
                            {
                                emp.emp_id,
                                emp.emp_code,
                                emp.title,
                                emp.firstname,
                                emp.middlename,
                                emp.lastname,
                                emp.gender,
                                emp.nationality,
                                emp.e_mail,
                                emp.citizenship_number,
                                emp.passport_number,
                                emp.no_of_children,
                                emp.dependent_details,
                                emp.blood_group,
                                emp.join_date,
                                emp.end_date,
                                emp.employee_type,
                                emp.department,
                                emp.post,
                                emp.manager_id,
                                emp.emp_status,
                                emp.deactivated_date,
                                emp.remarks,
                                emp.effective_date,
                                emp.dob,
                                emp.marital_status_info,
                                emp.emp_level,
                                emp.job_family,
                                emp.line_manager_id,
                                emp.alt_manager_id,
                                emp.alt_line_manager_id,
                                emp.employee_type_sub,
                                emp.ethnicity,
                                emp.work_percent,
                                emp.nin_no,
                                pho.photo
                            }).FirstOrDefault();

                    if (smt == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeViewModel
                    {
                        emp_id = smt.emp_id,
                        emp_code = smt.emp_code,
                        title = smt.title,
                        firstname = smt.firstname,
                        middlename = smt.middlename,
                        lastname = smt.lastname,
                        gender = smt.gender,
                        nationality = smt.nationality,
                        e_mail = smt.e_mail,
                        citizenship_number = smt.citizenship_number,
                        passport_number = smt.passport_number,
                        no_of_children = smt.no_of_children,
                        dependent_details = smt.dependent_details,
                        blood_group = smt.blood_group,
                        join_date = smt.join_date,
                        end_date = smt.end_date,
                        employee_type = smt.employee_type,
                        department = smt.department,
                        post = smt.post,
                        manager_id = smt.manager_id,
                        emp_status = smt.emp_status,
                        deactivated_date = smt.deactivated_date,
                        remarks = smt.remarks,
                        effective_date = smt.effective_date,
                        dob = smt.dob,
                        marital_status_info = smt.marital_status_info,
                        emp_level = smt.emp_level,
                        job_family = smt.job_family,
                        line_manager_id = smt.line_manager_id,
                        alt_manager_id = smt.alt_manager_id ?? 0,
                        alt_line_manager_id = smt.alt_line_manager_id ?? 0,
                        employee_type_sub = smt.employee_type_sub,
                        ethnicity = smt.ethnicity,
                        work_percent = smt.work_percent,
                        nin_no = smt.nin_no,
                        photo = smt.photo
                    };

                    ViewBag.EmpCode = smt.emp_code;
                    ViewBag.ManagerList = _employeeServices.GetManagerListBoth(smt.emp_id);
                    if (!string.IsNullOrEmpty(smt.photo))
                    {
                        string extension = Path.GetExtension(smt.photo).TrimStart('.').ToUpperInvariant();
                        ViewBag.preview = $@"
                                <img src='{Url.Content($"/uploads/documents/photo/{smt.photo}")}' title='Preview' width='300' height='400' border='0'>
                            ";
                    }
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_EmployeeAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EmployeeSave(EmployeeViewModel model)
        {
            _ = ModelState.Remove("emp_id");
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];

            string? emp_code = model.emp_code;
            string? title = model.title;
            string? firstname = model.firstname;
            string? middlename = model.middlename;
            string? lastname = model.lastname;
            string? gender = model.gender;
            string? nationality = model.nationality;
            string? e_mail = model.e_mail;
            string? citizenship_number = model.citizenship_number;
            string? passport_number = model.passport_number;
            string? marital_status_info = model.marital_status_info;
            int? no_of_children = model.no_of_children;
            string? dependent_details = model.dependent_details;
            string? blood_group = model.blood_group;
            DateTime? join_date = model.join_date;
            DateTime? end_date = model.end_date;
            string? employee_type = model.employee_type;
            string? department = model.department;
            string? post = model.post;
            int? manager_id = model.manager_id;
            string? emp_status = model.emp_status;
            DateTime? deactivated_date = model.deactivated_date;
            string? remarks = model.remarks;
            DateTime? effective_date = model.effective_date;
            DateTime? dob = model.dob;
            string? emp_level = model.emp_level;
            string? job_family = model.job_family;
            int? line_manager_id = model.line_manager_id;
            int? alt_manager_id = model.alt_manager_id;
            int? alt_line_manager_id = model.alt_line_manager_id;
            string? employee_type_sub = model.employee_type_sub;
            string? ethnicity = model.ethnicity;
            double? work_percent = model.work_percent;
            string? nin_no = model.nin_no;
            int update_by = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
            if (!_accountServices.HasPermission("10107", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (mode == "add")
            {
                var isData = _context.tbl_employee.FirstOrDefault(u => u.emp_code == emp_code);
                if (isData != null) { return Json(new { status = "false", message = Lang.msg_record_exist_other }); }

                int emp_id = (_context.tbl_employee.Any()
                                ? _context.tbl_employee.Max(o => o.emp_id)
                                : 0) + 1;
                var DataSave = new tbl_employee
                {
                    emp_id = emp_id,
                    emp_code = emp_code,
                    title = title,
                    firstname = firstname,
                    middlename = middlename,
                    lastname = lastname,
                    gender = gender,
                    nationality = nationality,
                    e_mail = e_mail,
                    citizenship_number = citizenship_number,
                    passport_number = passport_number,
                    marital_status_info = marital_status_info,
                    no_of_children = no_of_children,
                    dependent_details = dependent_details,
                    blood_group = blood_group,
                    join_date = join_date,
                    end_date = end_date,
                    employee_type = employee_type,
                    department = department,
                    post = post,
                    manager_id = manager_id,
                    emp_status = emp_status,
                    deactivated_date = deactivated_date,
                    remarks = remarks,
                    effective_date = effective_date,
                    dob = dob,
                    employee_type_sub = employee_type_sub,
                    ethnicity = ethnicity,
                    work_percent = work_percent,
                    nin_no = nin_no,
                };
                _ = _context.tbl_employee.Add(DataSave);

                var DataSaveHis = new tbl_employee_history
                {
                    emp_id = emp_id,
                    join_date = join_date,
                    end_date = end_date,
                    employee_type = employee_type,
                    department = department,
                    post = post,
                    emp_status = emp_status,
                    remarks = remarks,
                    update_date = DateTime.Now,
                    effective_date = effective_date,
                    job_family = job_family,
                    emp_level = emp_level,
                    manager_id = manager_id,
                    line_manager_id = line_manager_id,
                    no_of_children = no_of_children,
                    by_emp_id = update_by
                };
                _ = _context.tbl_employee_history.Add(DataSaveHis);


                _ = _context.SaveChanges();

                /**
                 We need to insert prorated leave for new employee
                 **/
                string fiscalYear = HttpContext.Session.GetString("fiscal_year") ?? "";
                double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs"));
                _leaveServices.CalculateNewEmployeeProrateLeave(workingHoursDays, emp_id, join_date, end_date, gender);
                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_id });
            }
            else if (mode == "edit")
            {
                int emp_id = model?.emp_id ?? 0;

                //check if the data is exits on another record
                var isData = _context.tbl_employee.FirstOrDefault(u => u.emp_code == emp_code && u.emp_id != emp_id);
                if (isData != null) { return Json(new { status = "false", message = Lang.msg_record_exist_other }); }
                var DataUpdate = _context.tbl_employee.FirstOrDefault(h => h.emp_id == emp_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.emp_id = emp_id;
                DataUpdate.emp_code = emp_code;
                DataUpdate.title = title;
                DataUpdate.firstname = firstname;
                DataUpdate.middlename = middlename;
                DataUpdate.lastname = lastname;
                DataUpdate.gender = gender;
                DataUpdate.nationality = nationality;
                DataUpdate.e_mail = e_mail;
                DataUpdate.citizenship_number = citizenship_number;
                DataUpdate.passport_number = passport_number;
                DataUpdate.marital_status_info = marital_status_info;
                DataUpdate.no_of_children = no_of_children;
                DataUpdate.dependent_details = dependent_details;
                DataUpdate.blood_group = blood_group;
                DataUpdate.employee_type = employee_type;
                DataUpdate.department = department;
                DataUpdate.post = post;
                DataUpdate.manager_id = manager_id;
                DataUpdate.emp_status = emp_status;
                DataUpdate.deactivated_date = deactivated_date;
                DataUpdate.remarks = remarks;
                DataUpdate.effective_date = effective_date;
                DataUpdate.dob = dob;
                DataUpdate.marital_status_info = marital_status_info;
                DataUpdate.emp_level = emp_level;
                DataUpdate.job_family = job_family;
                DataUpdate.line_manager_id = line_manager_id;
                DataUpdate.alt_manager_id = alt_manager_id;
                DataUpdate.alt_line_manager_id = alt_line_manager_id;
                DataUpdate.employee_type_sub = employee_type_sub;
                DataUpdate.ethnicity = ethnicity;
                DataUpdate.work_percent = work_percent;
                DataUpdate.nin_no = nin_no;

                _ = _context.tbl_employee.Update(DataUpdate);

                var DataSaveHis = new tbl_employee_history
                {
                    emp_id = emp_id,
                    employee_type = employee_type,
                    department = department,
                    post = post,
                    emp_status = emp_status,
                    remarks = remarks,
                    update_date = DateTime.Now,
                    effective_date = effective_date,
                    job_family = job_family,
                    emp_level = emp_level,
                    manager_id = manager_id,
                    line_manager_id = line_manager_id,
                    no_of_children = no_of_children,
                    by_emp_id = update_by
                };
                _ = _context.tbl_employee_history.Add(DataSaveHis);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        public async Task<IActionResult> EmployeeDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10107", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee.Where(r => request.SelectedIds.Contains(r.emp_id.ToString())).ToList();
            if (!recordsToDelete.Any()) { return NotFound(new { status = "false", message = Lang.msg_no_record_found }); }

            int tSel = recordsToDelete.Count; int tDel = 0; int tUDel = 0;
            foreach (var record in recordsToDelete)
            {
                //CHECK FOR EMPLOYEE EXIST IN ANOTHER TABLE
                bool hasUser = await _context.tbl_user.AnyAsync(p => p.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasPhoto = await _context.tbl_employee_photo.AnyAsync(p => p.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasHistory = await _context.tbl_employee_history.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasAttend = await _context.tbl_employee_check_in_out_main.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasAddress = await _context.tbl_employee_address.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasFund = await _context.tbl_employee_fund_source.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasLeave = await _context.tbl_employee_leave.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasTravel = await _context.tbl_employee_travel_main.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasSalary = await _context.tbl_employee_salary.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                bool hasSalField = await _context.tbl_employee_salary_a_field.AnyAsync(s => s.emp_id == record.emp_id).ConfigureAwait(false);
                if (hasUser || hasPhoto || hasHistory || hasHistory || hasAttend || hasAddress || hasFund ||
                    hasLeave || hasTravel || hasSalary || hasSalField
                    )
                {
                    //return "Employee cannot be deleted because related records exist.";
                }
                else
                {
                    tDel++;

                    //'DELETE THE Documents like CITIZEN COPY AND PASSPORT COPY etc


                    _ = _context.tbl_employee.Remove(new tbl_employee { emp_id = record.emp_id });
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    _context.ChangeTracker.Clear();



                }
            }
            tUDel = tSel - tDel;
            string msg_deleted_records = string.Empty;
            bool msg_status = false;
            if (tUDel > 0)
            {
                msg_deleted_records = Lang.msg_deleted_some;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_deleted_records = msg_deleted_records.Replace("[<UN-DEL-ROWS>]", tUDel.ToString(), StringComparison.Ordinal);
                msg_status = false;
            }
            else
            {
                msg_deleted_records = Lang.msg_delete_success;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_status = true;
            }
            return Ok(new
            {
                status = msg_status,
                deletedCount = tDel,
                message = msg_deleted_records
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EmployeeExport()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine("SN, Employee Id, Salutation, Employee Name, Gender, Date Of Birth, Ethnicity, Nationality, Address1, Address2, City, State/District, Country, Postal Code, Phone1, Phone2, Mobile, Email, Personal Email, Citizenship Number, Passport Number, Marital Status, No Of Children, Blood Group, Join Date, End Date, Employee Type, Department, Designation, Job Family, Career Level, PAN Number, Bank Account Number, PF Number, CIT Number, Immediate Supervisor, Line Director, Alt Immediate Supervisor, Alt Line Director, Change Effective Date, Status, Deactivated Date");

            int cnt = 0;

            var Records = (from emp in _context.tbl_employee
                           join add in _context.tbl_employee_address
                           on emp.emp_id equals add.emp_id into empAdd
                           from add in empAdd.DefaultIfEmpty()   // LEFT OUTER JOIN
                           orderby emp.firstname ascending, emp.middlename ascending, emp.lastname ascending
                           select new EmployeeExportViewModel
                           {
                               emp_code = emp.emp_code,
                               title = emp.title,
                               employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname }.Where(x => !string.IsNullOrEmpty(x))),
                               gender = emp.gender,
                               dob = emp.dob,
                               ethnicity = emp.ethnicity,
                               nationality = emp.nationality,
                               address1 = add.address1,
                               address2 = add.address2,
                               city = add.city,
                               state = add.state,
                               country = add.country,
                               postalcode = add.postalcode,
                               phone1 = add.phone1,
                               phone2 = add.phone2,
                               mobile = add.mobile,
                               e_mail =emp.e_mail,
                               personal_email = add.personal_email,
                               citizenship_number = emp.citizenship_number,
                               passport_number = emp.passport_number,
                               marital_status_info = emp.marital_status_info,
                               no_of_children = emp.no_of_children,
                               dependent_details = emp.dependent_details,
                               blood_group = emp.blood_group,
                               join_date= emp.join_date,
                               end_date = emp.end_date,
                               employee_type = emp.employee_type,
                               department = emp.department,
                               post = emp.post,
                               job_family = emp.job_family,
                               emp_level = emp.emp_level,
                               pan_no = emp.pan_no,
                               account_no = emp.account_no,
                               pf_no = emp.pf_no,
                               cit_no = emp.cit_no,
                               manager_id = emp.manager_id,
                               line_manager_id = emp.line_manager_id,
                               alt_manager_id = emp.alt_manager_id,
                               alt_line_manager_id = emp.alt_line_manager_id,
                               change_effective_date = emp.effective_date.ToString(),
                               emp_status =emp.emp_status,
                               deactivated_date = emp.deactivated_date,
                               remarks = emp.remarks
                            }).ToList();
            if (Records.Count > 0)
            {
                foreach (var record in Records)
                {
                    cnt++;
                    int manager_id = int.TryParse(record.manager_id.ToString(), out int p1) ? p1 : 0;
                    int line_manager_id = int.TryParse(record.line_manager_id.ToString(), out int p2) ? p2 : 0;
                    int alt_manager_id = int.TryParse(record.alt_manager_id.ToString(), out int p3) ? p3 : 0;
                    int alt_line_manager_id = int.TryParse(record.alt_line_manager_id.ToString(), out int p4) ? p4 : 0;


                    string emp_code = EscapeCSV(record.emp_code ?? "");
                    string title = EscapeCSV(record.title ?? "");
                    string employee = EscapeCSV(record.employee ?? "");
                    string gender = EscapeCSV(record.gender ?? "");
                    string dob = EscapeCSV(record.dob.ToString() ?? "");
                    string ethnicity = EscapeCSV(record.ethnicity ?? "");
                    string nationality = EscapeCSV(record.nationality ?? "");
                    string address1 = EscapeCSV(record.address1 ?? "");
                    string address2 = EscapeCSV(record.address2 ?? "");
                    string city = EscapeCSV(record.city ?? "");
                    string state = EscapeCSV(record.state ?? "");
                    string country = EscapeCSV(record.country ?? "");
                    string postalcode = EscapeCSV(record.postalcode ?? "");
                    string phone1 = EscapeCSV(record.phone1 ?? "");
                    string phone2 = EscapeCSV(record.phone2 ?? "");
                    string mobile = EscapeCSV(record.mobile ?? "");
                    string e_mail = EscapeCSV(record.e_mail ?? "");
                    string personal_email = EscapeCSV(record.personal_email ?? "");
                    string citizenship_number = EscapeCSV(record.citizenship_number ?? "");
                    string passport_number = EscapeCSV(record.passport_number ?? "");
                    string marital_status_info = EscapeCSV(record.marital_status_info ?? "");
                    string no_of_children = EscapeCSV(record.no_of_children.ToString() ?? "");
                    //string dependent_details = EscapeCSV(record.dependent_details ?? "");
                    string blood_group = EscapeCSV(record.blood_group ?? "");
                    string join_date = EscapeCSV(record.join_date.ToString() ?? "");
                    string end_date = EscapeCSV(record.end_date.ToString() ?? "");
                    string employee_type = EscapeCSV(record.employee_type ?? "");
                    string department = EscapeCSV(record.department ?? "");
                    string post = EscapeCSV(record.post ?? "");
                    string job_family = EscapeCSV(record.job_family ?? "");
                    string emp_level = EscapeCSV(record.emp_level ?? "");
                    string pan_no = EscapeCSV(record.pan_no ?? "");
                    string account_no = EscapeCSV(record.account_no ?? "");
                    string pf_no = EscapeCSV(record.pf_no ?? "");
                    string cit_no = EscapeCSV(record.cit_no ?? "");
                    string s_manager = EscapeCSV(_employeeServices.GetEmployeeName(manager_id));
                    string s_line_manager = EscapeCSV(_employeeServices.GetEmployeeName(line_manager_id));
                    string s_alt_manager = EscapeCSV(_employeeServices.GetEmployeeName(alt_manager_id));
                    string s_alt_line_manager = EscapeCSV(_employeeServices.GetEmployeeName(alt_line_manager_id));
                    string change_effective_date = EscapeCSV(record.change_effective_date ?? "");
                    string emp_status = EscapeCSV(record.emp_status ?? "");
                    string deactivated_date = EscapeCSV(record.deactivated_date.ToString() ?? "");
                    //string remarks = EscapeCSV(record.remarks ?? "");

                    _ = sb.AppendLine($"{cnt},\"{emp_code}\",\"{title}\",\"{employee}\",\"{gender}\",\"{dob}\",\"{ethnicity}\",\"{nationality}\",\"{address1}\",\"{address2}\",\"{city}\",\"{state}\",\"{country}\",\"{postalcode}\",\"{phone1}\",\"{phone2}\",\"{mobile}\",\"{e_mail}\",\"{personal_email}\",\"{citizenship_number}\",\"{passport_number}\",\"{marital_status_info}\",\"{no_of_children}\",\"{blood_group}\",\"{join_date}\",\"{end_date}\",\"{employee_type}\",\"{department}\",\"{post}\",\"{job_family}\",\"{emp_level}\",\"{pan_no}\",\"{account_no}\",\"{pf_no}\",\"{cit_no}\",\"{s_manager}\",\"{s_line_manager}\",\"{s_alt_manager}\",\"{s_alt_line_manager}\",\"{change_effective_date}\",\"{emp_status}\",\"{deactivated_date}\"");
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "employee-list-downloaded.csv");
        }
        [HttpGet]
        public IActionResult EmployeeHistory(int? id, string mode)
        {
            string PageId = "10107";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            var sb = new StringBuilder();

            ViewBag.mode = mode;
            var Records = (
                from his in _context.tbl_employee_history
                join emp in _context.tbl_employee
                on his.emp_id equals emp.emp_id
                where his.emp_id == id
                orderby his.update_date descending
                select new EmployeeHistoryViewModel
                {
                    emp_id = his.emp_id,
                    join_date = his.join_date,
                    end_date = his.end_date,
                    employee_type = his.employee_type,
                    department = his.department,
                    post = his.post,
                    salary = his.salary,
                    grade = his.grade,
                    child_edu_all = his.child_edu_all,
                    emp_status_for = his.emp_status,
                    deactivated_date = his.deactivated_date,
                    remarks = his.remarks,
                    update_date = his.update_date,
                    effective_date = his.effective_date,
                    remote_area_allow = his.remote_area_allow,
                    yearly_remote_exem = his.yearly_remote_exem,
                    by_emp_id = his.by_emp_id,
                    job_family = his.job_family,
                    emp_level = his.emp_level,
                    manager_id = his.manager_id,
                    line_manager_id = his.line_manager_id,
                    marital_status = his.marital_status,
                    no_of_children = his.no_of_children,
                    firstname = emp.firstname,
                    middlename = emp.middlename,
                    lastname = emp.lastname,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            var EmployeeName = "";
            int cnt = 0;
            foreach (var record in Records)
            {
                EmployeeName = record.employee;
                cnt++;

                string? join_date = record.join_date.ToString();
                string? end_date = record.end_date.ToString();
                string? deactivated_date = record.deactivated_date.ToString();
                string? effective_date = record.effective_date.ToString();

                string? s_join_date = _settingsServices.DateformatToDt(join_date ?? string.Empty);
                string? s_end_date = _settingsServices.DateformatToDt(end_date ?? string.Empty);
                string? s_deactivated_date = _settingsServices.DateformatToDt(deactivated_date ?? string.Empty);
                string? s_effective_date = _settingsServices.DateformatToDt(effective_date ?? string.Empty);

                string marital_status = record.marital_status == "S" ? "Single" : "Married";
                string emp_status = record.emp_status == "A" ? "Active" : "Inactive";

                string salary = string.IsNullOrWhiteSpace(record.salary.ToString()) ? "0" : Math.Round((decimal)record.salary, 2).ToString();
                string remote_area_allow = string.IsNullOrWhiteSpace(record.remote_area_allow.ToString()) ? "0" : Math.Round((decimal)record.remote_area_allow, 2).ToString();
                string yearly_remote_exem = string.IsNullOrWhiteSpace(record.yearly_remote_exem.ToString()) ? "0" : Math.Round((decimal)record.yearly_remote_exem, 2).ToString();
                string child_edu_all = string.IsNullOrWhiteSpace(record.child_edu_all.ToString()) ? "0" : Math.Round((decimal)record.child_edu_all, 2).ToString();

                string manager = string.IsNullOrWhiteSpace(record.manager_id.ToString()) ? "" : _employeeServices.GetEmployeeName((int)record.manager_id);
                string line_manager = string.IsNullOrWhiteSpace(record.line_manager_id.ToString()) ? "" : _employeeServices.GetEmployeeName((int)record.line_manager_id);
                string by_emp = string.IsNullOrWhiteSpace(record.by_emp_id.ToString()) ? "" : _employeeServices.GetEmployeeName((int)record.by_emp_id);

                _ = sb.AppendLine($@"
                <tr class=""bg-silver bg-opacity-27"">
                    <th width=""1%"">{cnt}</th>
                    <th>{s_join_date}</th>
                    <th>{marital_status}</th>
                    <th>{record.employee_type}</th>
                    <th>{record.department}</th>
                    <th>{salary}</th>
                    <th>{record.no_of_children}</th>
                    <th>{record.emp_status}</th>
                    <th>{effective_date}</th>
                    <th>{remote_area_allow}</th>
                    <th>{record.job_family}</th>
                    <th>I: {manager}</th>
                    <th>{record.remarks}</th>
                    <th>{by_emp}</th>
                </tr>
                <tr class=""bg-silver bg-opacity-27"">
                    <th width=""1%""></th>
                    <th>{s_end_date}</th>
                    <th></th>
                    <th></th>
                    <th>{record.post}</th>
                    <th></th>
                    <th>{child_edu_all}</th>
                    <th>{deactivated_date}</th>
                    <th></th>
                    <th>{yearly_remote_exem}</th>
                    <th>{record.emp_level}</th>
                    <th>L: {line_manager}</th>
                    <th></th>
                    <th>{record.update_date}</th>
                </tr>
                <tr class=""bg-silver bg-opacity-27""><th width=""100%"" colspan=""14""><hr></th>    
			    ");
            }
            ViewBag.EmployeeName = EmployeeName;
            ViewBag.EmployeeHistoryHtml = sb.ToString();
            return PartialView("Employee/_EmployeeHistory", Records);
        }

        #endregion
        /********************************************************************************************************************/
        #region DIGITAL SIGNATURE 10103
        [HttpGet]
        public IActionResult DigitalSignature()
        {
            string PageId = "10103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from con in _context.tbl_employee_signature
                join emp in _context.tbl_employee
                on con.emp_id equals emp.emp_id
                orderby con.upload_date descending
                select new DigitalSignatureViewModel
                {
                    emp_sign_id = con.emp_sign_id,
                    signature = con.signature,
                    upload_date = con.upload_date,
                    emp_id = con.emp_id,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/DigitalSignature", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_DigitalSignature", Records);
        }
        [HttpPost]
        public async Task<IActionResult> DigitalSignatureList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from con in _context.tbl_employee_signature
                        join emp in _context.tbl_employee
                        on con.emp_id equals emp.emp_id
                        select new DigitalSignatureViewModel
                        {
                            emp_sign_id = con.emp_sign_id,
                            signature = con.signature,
                            upload_date = con.upload_date,
                            emp_id = con.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
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
            else
            {
                query = query.OrderByDescending(d => d.upload_date);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult DigitalSignatureAddEdit(int? id, string mode)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeNotHavingSignature();

            DigitalSignatureViewModel model;
            model = new DigitalSignatureViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_DigitalSignatureAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from con in _context.tbl_employee_signature
                              join emp in _context.tbl_employee
                              on con.emp_id equals emp.emp_id
                              where con.emp_sign_id == Convert.ToInt32(id)
                              select new
                              {
                                  con.emp_sign_id,
                                  con.signature,
                                  con.upload_date,
                                  con.emp_id,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status,
                                  emp.emp_code
                              }).AsNoTracking().FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new DigitalSignatureViewModel
                    {
                        emp_sign_id = ec.emp_sign_id,
                        signature = ec.signature,
                        upload_date = ec.upload_date,
                        emp_id = ec.emp_id,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };

                    if (!string.IsNullOrEmpty(ec.signature))
                    {
                        string extension = Path.GetExtension(ec.signature).TrimStart('.').ToUpperInvariant();
                        ViewBag.preview = $@"
                                <img src='{Url.Content($"/uploads/documents/signature/{ec.signature}")}' title='Preview' width='200' height='100' border='0'>
                            ";
                    }
                    string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                    string uploadsFolder = Path.Combine(GblDocumentPath, "documents/signature", ec.signature);
                    ViewBag.fileSize = GetFileSize(uploadsFolder);

                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_DigitalSignatureAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DigitalSignatureSave(EmployeeSignedContractViewModel model, IFormFile file)
        {
            /*
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("emp_sign_id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10103", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (!FileValidator.ForImages(file)) { return Json(new { status = "error", message = "There is problem with File." }); }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/signature");

            int emp_id = Convert.ToInt32(model.emp_id);
            if (mode == "add")
            {
                int emp_sign_id = (_context.tbl_employee_signature.Any()
                              ? _context.tbl_employee_signature.Max(o => o.emp_sign_id)
                              : 0) + 1;
                var DataSave = new tbl_employee_signature
                {
                    emp_sign_id = emp_sign_id,
                    emp_id = emp_id
                };
                _ = _context.tbl_employee_signature.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var isDataSaved = _context.tbl_employee_signature.FirstOrDefault(u => u.emp_sign_id == emp_sign_id);
                if (isDataSaved == null)
                {
                    return Json(new { status = "false", message = "Fail to save digital signature." });
                }
                if (file != null)
                {
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        isDataSaved.signature = filename;
                        isDataSaved.upload_date = DateTime.Now;

                        _ = _context.tbl_employee_signature.Update(isDataSaved);
                        _ = _context.SaveChanges();
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Signature saved successfully, however the associated file could not be uploaded." });
                    }
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_sign_id });
            }
            else if (mode == "edit")
            {
                int emp_sign_id = Convert.ToInt32(Request.Form["emp_sign_id"]);

                var DataUpdate = _context.tbl_employee_signature
                    .FirstOrDefault(h => h.emp_sign_id == emp_sign_id);

                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                if (file != null)
                {
                    /** DELETE EXISTING FILE | instead of taking from post, better to get from db | security reason **/
                    string hUploadFile = DataUpdate.signature ?? "";
                    if (!string.IsNullOrWhiteSpace(hUploadFile))
                    {
                        string st = DeleteFile(uploadsFolder, hUploadFile);
                        if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            return Json(new { status = "false", message = "Failed to overwite existing signature. Please contact your system administrator." });
                        }
                    }
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        DataUpdate.signature = filename;
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Failed to update signature. Please contact your system administrator." });
                    }
                }

                DataUpdate.upload_date = DateTime.Now;
                _ = _context.tbl_employee_signature.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_sign_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DigitalSignatureDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10103", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_signed_contract
                .Where(r => request.SelectedIds.Contains(r.emp_signed_contract_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.ChangeTracker.Clear();

            int DelCnt = 0;
            var deletedIds = new List<int>();
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/signature");

            foreach (var id in request.SelectedIds)
            {
                var recordsToDel = _context.tbl_employee_signature.FirstOrDefault(e => e.emp_sign_id == Convert.ToInt32(id));
                if (recordsToDel != null)
                {
                    string fileName = recordsToDel.signature ?? "";
                    int emp_signed_contract_id = recordsToDel.emp_sign_id;
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string st = DeleteFile(uploadsFolder, fileName);
                        if (string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            DelCnt++;
                            deletedIds.Add(recordsToDel.emp_sign_id);
                        }
                    }
                }
            }
            if (deletedIds.Count > 0)
            {
                var entitiesToDelete = _context.tbl_employee_signature.Where(t => deletedIds.Contains(t.emp_sign_id)).ToList();
                _context.tbl_employee_signature.RemoveRange(entitiesToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", DelCnt.ToString(), StringComparison.Ordinal)
            });
        }
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> DigitalSignaturePreview(string id)
        {
            string PageId = "10103";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_employee_signature.FirstOrDefault(h => h.emp_sign_id == Convert.ToInt32(id));
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.signature))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "documents/signature");

                string filePath = Path.Combine(uploadsFolder, smt.signature);
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
                        // If it's an image, preview inline
                        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            var stream = new FileStream(fullPathResolved, FileMode.Open, FileAccess.Read);
                            return File(stream, contentType); // no "download filename" → browser previews
                        }
                        else
                        {
                            // Otherwise force download
                            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPathResolved);
                            return File(fileBytes, contentType, smt.signature);
                        }
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
        #region PHOTO 10112
        [HttpGet]
        public IActionResult Photo()
        {
            string PageId = "10112";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from con in _context.tbl_employee_photo
                join emp in _context.tbl_employee
                on con.emp_id equals emp.emp_id
                orderby emp.firstname descending
                select new EmployeePhotoViewModel
                {
                    id = con.id,
                    photo = con.photo,
                    emp_id = con.emp_id,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Photo", "ADD", PageId, Records.Count);
            return PartialView("Employee/_Photo", Records);
        }
        [HttpPost]
        public async Task<IActionResult> PhotoList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from con in _context.tbl_employee_photo
                        join emp in _context.tbl_employee
                        on con.emp_id equals emp.emp_id
                        select new EmployeePhotoViewModel
                        {
                            id = con.id,
                            photo = con.photo,
                            emp_id = con.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
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
            else
            {
                query = query.OrderByDescending(d => d.id);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult PhotoAddEdit(string id, string mode)
        {
            string PageId = "10112";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeNotHavingPhoto();

            EmployeePhotoViewModel model;
            model = new EmployeePhotoViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_PhotoAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from con in _context.tbl_employee_photo
                              join emp in _context.tbl_employee
                              on con.emp_id equals emp.emp_id
                              where con.id == id
                              select new
                              {
                                  con.id,
                                  con.photo,
                                  con.emp_id,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status,
                                  emp.emp_code
                              }).AsNoTracking().FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new EmployeePhotoViewModel
                    {
                        id = ec.id,
                        photo = ec.photo,
                        emp_id = ec.emp_id,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };

                    if (!string.IsNullOrEmpty(ec.photo))
                    {
                        string extension = Path.GetExtension(ec.photo).TrimStart('.').ToUpperInvariant();
                        ViewBag.preview = $@"
                                <img src='{Url.Content($"/uploads/documents/photo/{ec.photo}")}' title='Preview' width='200' height='100' border='0'>
                            ";
                    }
                    string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                    string uploadsFolder = Path.Combine(GblDocumentPath, "documents/photo", ec.photo);
                    ViewBag.fileSize = GetFileSize(uploadsFolder);

                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_PhotoAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PhotoSave(EmployeeSignedContractViewModel model, IFormFile file)
        {
            /*
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10112", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (!FileValidator.ForImages(file)) { return Json(new { status = "error", message = "There is problem with File." }); }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/photo");

            int emp_id = Convert.ToInt32(model.emp_id);
            if (mode == "add")
            {
                string id = UniqueID();
                var DataSave = new tbl_employee_photo
                {
                    id = id,
                    emp_id = emp_id
                };
                _ = _context.tbl_employee_photo.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var isDataSaved = _context.tbl_employee_photo.FirstOrDefault(u => u.id == id);
                if (isDataSaved == null)
                {
                    return Json(new { status = "false", message = "Fail to save photo." });
                }
                if (file != null)
                {
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        isDataSaved.photo = filename;

                        _ = _context.tbl_employee_photo.Update(isDataSaved);
                        _ = _context.SaveChanges();
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Photo saved successfully, however the associated file could not be uploaded." });
                    }
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id = id });
            }
            else if (mode == "edit")
            {
                string id = Request.Form["id"];

                var DataUpdate = _context.tbl_employee_photo.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                if (file != null)
                {
                    /** DELETE EXISTING FILE | instead of taking from post, better to get from db | security reason **/
                    string hUploadFile = DataUpdate.photo ?? "";
                    if (!string.IsNullOrWhiteSpace(hUploadFile))
                    {
                        string st = DeleteFile(uploadsFolder, hUploadFile);
                        if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            return Json(new { status = "false", message = "Failed to overwite existing photo. Please contact your system administrator." });
                        }
                    }
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        DataUpdate.photo = filename;
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Failed to update photo. Please contact your system administrator." });
                    }
                }

                _ = _context.tbl_employee_photo.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, DataUpdate.id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> PhotoPreview(string id)
        {
            string PageId = "10112";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_employee_photo.FirstOrDefault(h => h.id == id);
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.photo))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "documents/photo");

                string filePath = Path.Combine(uploadsFolder, smt.photo);
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
                        // If it's an image, preview inline
                        if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                        {
                            var stream = new FileStream(fullPathResolved, FileMode.Open, FileAccess.Read);
                            return File(stream, contentType); // no "download filename" ? browser previews
                        }
                        else
                        {
                            // Otherwise force download
                            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPathResolved);
                            return File(fileBytes, contentType, smt.photo);
                        }
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
        #region EMPLOYEE OUTSIDE 10153
        [HttpGet]
        public IActionResult EmployeeOutside()
        {
            string PageId = "10153";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_employee_outside
                orderby a.emp_id descending
                select new EmployeeOutsideViewModel
                {
                    emp_id = a.emp_id,
                    emp_code = a.emp_code,
                    title = a.title,
                    e_mail = a.e_mail,
                    phone = a.phone,
                    duty_station_id = a.duty_station_id,
                    emp_status = a.emp_status,
                    photo = a.photo
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/EmployeeOutside", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_EmployeeOutside", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeOutsideList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var EmployeeStatusFilter = request.FilterValue1;/*Dropdwon Filter*/
            var query = from ec in _context.tbl_employee_outside
                        join ds in _context.tbl_duty_station
                            on ec.duty_station_id equals ds.id into dsGroup
                        from ds in dsGroup.DefaultIfEmpty()
                        orderby ec.emp_id descending
                        select new EmployeeOutsideViewModel
                        {
                            emp_id = ec.emp_id,
                            firstname = ec.firstname,
                            middlename = ec.middlename,
                            lastname = ec.lastname,
                            employee = $"{ec.firstname} {ec.middlename} {ec.lastname} ({ec.emp_code})",
                            emp_code = ec.emp_code,
                            e_mail = ec.e_mail,
                            phone = ec.phone,
                            mobile = ec.mobile,
                            duty_station = ds.duty_station,
                            emp_status = ec.emp_status,
                            photo = ec.photo
                        };
            if (!string.IsNullOrEmpty(EmployeeStatusFilter))
            {
                query = query.Where(d => d.emp_status == EmployeeStatusFilter);/*filter*/
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue)) ||
                    (a.e_mail != null && a.e_mail.Contains(searchValue)) ||
                    (a.emp_code != null && a.emp_code.Contains(searchValue)) ||
                    (a.duty_station != null && a.duty_station.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult EmployeeOutsideAddEdit(string id, string mode)
        {
            string PageId = "10153";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.Gender = GenderList();
            ViewBag.DutyStation = _employeeServices.GetDutyStationList();
            EmployeeOutsideViewModel model;
            model = new EmployeeOutsideViewModel();
            ViewBag.EmpCode = _context.Database.SqlQuery<string>($"EXEC EmployeeOutsideCode").AsEnumerable().FirstOrDefault();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_EmployeeOutsideAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == "" || id == null)
                {
                    //return error
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (from ec in _context.tbl_employee_outside
                               where ec.emp_id == Convert.ToInt32(id)
                               select new
                               {
                                   ec.emp_id,
                                   ec.emp_code,
                                   ec.title,
                                   ec.firstname,
                                   ec.middlename,
                                   ec.lastname,
                                   ec.gender,
                                   ec.dob,
                                   ec.address,
                                   ec.phone,
                                   ec.mobile,
                                   ec.e_mail,
                                   ec.join_date,
                                   ec.end_date,
                                   ec.emp_status,
                                   ec.deactivated_date,
                                   ec.remarks,
                                   ec.effective_date,
                                   ec.pan_no,
                                   ec.duty_station_id,
                                   ec.photo
                               }).FirstOrDefault();

                    if (smt == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeOutsideViewModel
                    {
                        emp_id = smt.emp_id,
                        emp_code = smt.emp_code,
                        title = smt.title,
                        firstname = smt.firstname,
                        middlename = smt.middlename,
                        lastname = smt.lastname,
                        gender = smt.gender,
                        dob = smt.dob,
                        address = smt.address,
                        phone = smt.phone,
                        mobile = smt.mobile,
                        e_mail = smt.e_mail,
                        join_date = smt.join_date,
                        end_date = smt.end_date,
                        emp_status = smt.emp_status,
                        deactivated_date = smt.deactivated_date,
                        remarks = smt.remarks,
                        effective_date = smt.effective_date,
                        pan_no = smt.pan_no,
                        duty_station_id = smt.duty_station_id,
                        photo = smt.photo
                    };
                    ViewBag.EmpCode = smt.emp_code;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_EmployeeOutsideAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EmployeeOutsideSave(EmployeeOutsideViewModel model)
        {
            _ = ModelState.Remove("emp_id");
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10153", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string? emp_code = model.emp_code;
            string? title = model.title;
            string? firstname = model.firstname;
            string? middlename = model.middlename;
            string? lastname = model.lastname;
            string? gender = model.gender;
            DateTime? dob = model.dob;
            string? address = model.address;
            string? phone = model.phone;
            string? mobile = model.mobile;
            string? e_mail = model.e_mail;
            DateTime? join_date = model.join_date;
            DateTime? end_date = model.end_date;
            string? emp_status = model.emp_status;
            string? remarks = model.remarks;
            DateTime? effective_date = model.effective_date;
            string? pan_no = model.pan_no;
            string? duty_station_id = model.duty_station_id;

            if (mode == "add")
            {
                //check if the data is exits on another record
                var isData = _context.tbl_employee_outside.FirstOrDefault(u => u.emp_code == emp_code);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                int emp_id = (_context.tbl_employee_outside.Any()
                                ? _context.tbl_employee_outside.Max(o => o.emp_id)
                                : 0) + 1;
                var DataSave = new tbl_employee_outside
                {
                    emp_id = emp_id,
                    emp_code = emp_code,
                    title = title,
                    firstname = firstname,
                    middlename = middlename,
                    lastname = lastname,
                    gender = gender,
                    dob = dob,
                    address = address,
                    phone = phone,
                    mobile = mobile,
                    e_mail = e_mail,
                    join_date = join_date,
                    end_date = end_date,
                    emp_status = emp_status,
                    remarks = remarks,
                    effective_date = effective_date,
                    pan_no = pan_no,
                    duty_station_id = duty_station_id
                };
                _ = _context.tbl_employee_outside.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_id });
            }
            else if (mode == "edit")
            {
                string? EmpId = Request.Form["emp_id"];
                int emp_id = int.TryParse(EmpId, out int parsedId) ? parsedId : 0;
                var DataUpdate = _context.tbl_employee_outside.FirstOrDefault(h => h.emp_id == emp_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                var isData = _context.tbl_employee_outside.FirstOrDefault(u => u.emp_code == emp_code && u.emp_id != emp_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                DataUpdate.emp_code = emp_code;
                DataUpdate.title = title;
                DataUpdate.firstname = firstname;
                DataUpdate.middlename = middlename;
                DataUpdate.lastname = lastname;
                DataUpdate.gender = gender;
                DataUpdate.dob = dob;
                DataUpdate.address = address;
                DataUpdate.phone = phone;
                DataUpdate.mobile = mobile;
                DataUpdate.e_mail = e_mail;
                DataUpdate.join_date = join_date;
                DataUpdate.end_date = end_date;
                DataUpdate.emp_status = emp_status;
                if (emp_status == "D") { DataUpdate.deactivated_date = DateTime.Now; }
                DataUpdate.remarks = remarks;
                DataUpdate.effective_date = effective_date;
                DataUpdate.pan_no = pan_no;
                DataUpdate.duty_station_id = duty_station_id;

                _ = _context.tbl_employee_outside.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeOutsideDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10153", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (request?.SelectedIds == null || !request.SelectedIds.Any()) { return BadRequest(new { status = false, message = Lang.msg_no_record_selected }); }
            var recordsToDelete = _context.tbl_employee_outside.Where(r => request.SelectedIds.Contains(r.emp_id.ToString())).ToList();
            if (!recordsToDelete.Any()) { return NotFound(new { status = "false", message = Lang.msg_no_record_found }); }
            _context.ChangeTracker.Clear();

            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var record in recordsToDelete)
            {
                tSel++;
                var isData = _context.tbl_employee_check_in_out_main_outside.FirstOrDefault(h => h.emp_id == record.emp_id);
                if (isData != null)
                {
                    //return Json(new { status = "invalid", message = "Can not delete as data exists in another section." }); 
                }
                else
                {
                    tDel++;
                    var delData = _context.tbl_employee_outside.FirstOrDefault(h => h.emp_id == record.emp_id);
                    _context.tbl_employee_outside.RemoveRange(delData);
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    _context.ChangeTracker.Clear();
                }
            }
            tUDel = tSel - tDel;
            string msg_deleted_records = string.Empty;
            bool msg_status = false;
            if (tUDel > 0)
            {
                msg_deleted_records = Lang.msg_deleted_some;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_deleted_records = msg_deleted_records.Replace("[<UN-DEL-ROWS>]", tUDel.ToString(), StringComparison.Ordinal);
                msg_status = false;
            }
            else
            {
                msg_deleted_records = Lang.msg_delete_success;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_status = true;
            }
            return Ok(new
            {
                status = msg_status,
                deletedCount = tDel,
                message = msg_deleted_records
            });
        }
        #endregion
        /********************************************************************************************************************/
        #region INSURANCE
        [HttpGet]
        public IActionResult Insurance()
        {
            string PageId = "10110";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from ins in _context.tbl_employee_insurance
                join emp in _context.tbl_employee
                on ins.emp_id equals emp.emp_id
                orderby emp.firstname descending
                select new EmployeeInsuranceViewModel
                {
                    emp_ins_id = ins.emp_ins_id,
                    ins_company = ins.ins_company,
                    ins_type = ins.ins_type,
                    ins_valid_date = ins.ins_valid_date,
                    policy_no = ins.policy_no,
                    ins_amount = ins.ins_amount,
                    premium_amount = ins.premium_amount,
                    remarks = ins.remarks,
                    emp_id = ins.emp_id,
                    firstname = emp.firstname,
                    middlename = emp.middlename,
                    lastname = emp.lastname,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Insurance", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_Insurance", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsuranceList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var EmployeeStatusFilter = request.FilterValue1;

            var query = from ins in _context.tbl_employee_insurance
                        join emp in _context.tbl_employee
                            on ins.emp_id equals emp.emp_id
                        select new EmployeeInsuranceViewModel
                        {
                            emp_ins_id = ins.emp_ins_id,
                            ins_company = ins.ins_company,
                            ins_type = ins.ins_type,
                            ins_valid_date = ins.ins_valid_date,
                            policy_no = ins.policy_no,
                            ins_amount = ins.ins_amount,
                            premium_amount = ins.premium_amount,
                            remarks = ins.remarks,
                            emp_id = ins.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult InsuranceAddEdit(int? id, string mode)
        {
            string PageId = "10110";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();

            EmployeeInsuranceViewModel model;
            model = new EmployeeInsuranceViewModel();
            ViewBag.InsuranceType = EmployeeServices.InsuranceType("");

            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_InsuranceAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from ins in _context.tbl_employee_insurance
                              join emp in _context.tbl_employee
                                  on ins.emp_id equals emp.emp_id
                              where ins.emp_ins_id == id
                              select new
                              {
                                  ins.emp_ins_id,
                                  ins.ins_company,
                                  ins.ins_type,
                                  ins.ins_valid_date,
                                  ins.policy_no,
                                  ins.ins_amount,
                                  ins.premium_amount,
                                  ins.remarks,
                                  ins.emp_id,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();

                    if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeInsuranceViewModel
                    {
                        emp_ins_id = ec.emp_ins_id,
                        ins_company = ec.ins_company,
                        ins_type = ec.ins_type,
                        ins_valid_date = ec.ins_valid_date,
                        policy_no = ec.policy_no,
                        ins_amount = Math.Round(Convert.ToDecimal(ec.ins_amount), 2),
                        premium_amount = Math.Round(Convert.ToDecimal(ec.premium_amount), 2),
                        remarks = ec.remarks,
                        emp_id = ec.emp_id,
                        firstname = ec.firstname,
                        middlename = ec.middlename,
                        lastname = ec.lastname,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    ViewBag.Employee = ec.employee;
                    return PartialView("Employee/_InsuranceAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult InsuranceSave(EmployeeInsuranceViewModel model)
        {
            _ = ModelState.Remove("emp_ins_id");
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];
            string? ins_company = model.ins_company;
            string? ins_type = model.ins_type;
            DateTime? ins_valid_date = model.ins_valid_date;
            string? policy_no = model.policy_no;
            decimal? ins_amount = model.ins_amount;
            decimal? premium_amount = model.premium_amount;
            string? remarks = model.remarks;
            int? emp_id = model.emp_id;

            if (!_accountServices.HasPermission("10110", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (mode == "add")
            {
                int emp_ins_id = (_context.tbl_employee_insurance.Any()
                              ? _context.tbl_employee_insurance.Max(o => o.emp_ins_id)
                              : 0) + 1;
                var DataSave = new tbl_employee_insurance
                {
                    emp_ins_id = emp_ins_id,
                    ins_company = ins_company,
                    ins_type = ins_type,
                    ins_valid_date = ins_valid_date,
                    policy_no = policy_no,
                    ins_amount = ins_amount,
                    premium_amount = premium_amount,
                    remarks = remarks,
                    emp_id = emp_id
                };
                _ = _context.tbl_employee_insurance.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_ins_id });
            }
            else if (mode == "edit")
            {
                string? id = Request.Form["emp_ins_id"];
                int emp_ins_id = int.TryParse(id, out int parseId) ? parseId : 0;

                var DataUpdate = _context.tbl_employee_insurance
                    .FirstOrDefault(h => h.emp_ins_id == emp_ins_id);

                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.ins_company = ins_company;
                DataUpdate.ins_type = ins_type;
                DataUpdate.ins_valid_date = ins_valid_date;
                DataUpdate.policy_no = policy_no;
                DataUpdate.ins_amount = ins_amount;
                DataUpdate.premium_amount = premium_amount;
                DataUpdate.remarks = remarks;
                DataUpdate.emp_id = emp_id;

                _ = _context.tbl_employee_insurance.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_ins_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InsuranceDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10110", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            var recordsToDelete = _context.tbl_employee_insurance
                .Where(r => request.SelectedIds.Contains(r.emp_ins_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_insurance.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", request.SelectedIds.Count.ToString(), StringComparison.OrdinalIgnoreCase)
            });
        }

        #endregion
        /********************************************************************************************************************/
        #region DAY OFF
        [HttpGet]
        public IActionResult DayOff()
        {
            string PageId = "10113";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                        from con in _context.tbl_employee_dayoff
                        join cdt in _context.tbl_fiscal_year
                            on con.fiscal_year equals cdt.fiscal_year
                        join emp in _context.tbl_employee
                            on con.emp_id equals emp.emp_id
                        orderby con.dayoff_date descending
                        select new EmployeeDayOffViewModel
                        {
                            id = con.id,
                            dayoff_date = con.dayoff_date,
                            fiscal_year = con.fiscal_year,
                            fiscal_year_abb = cdt.fiscal_year_abb,
                            emp_id = con.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                        }).ToList();

            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/DayOff", "ADD|DEL", PageId, Records.Count);
            return PartialView("employee/_DayOff", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DayOffList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string? EmployeeStatusFilter = request.FilterValue1;
            string? FiscalYearFilter = request.FilterValue2;

            var query = from con in _context.tbl_employee_dayoff
                        join cdt in _context.tbl_fiscal_year
                            on con.fiscal_year equals cdt.fiscal_year
                        join emp in _context.tbl_employee
                            on con.emp_id equals emp.emp_id
                        select new EmployeeDayOffViewModel
                        {
                            id = con.id,
                            dayoff_date = con.dayoff_date,
                            fiscal_year = con.fiscal_year,
                            fiscal_year_abb = cdt.fiscal_year_abb,
                            emp_id = con.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            emp_status = emp.emp_status,
                        };

            if (!string.IsNullOrEmpty(FiscalYearFilter))
            {
                query = query.Where(d => d.fiscal_year == FiscalYearFilter);
            }
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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

        public IActionResult DayOffAddEdit(string? id, string mode)
        {
            string PageId = "10113";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();

            EmployeeDayOffViewModel model;
            model = new EmployeeDayOffViewModel();
            if (mode == "add")
            {
                ViewBag.FiscalYear = HttpContext.Session.GetString("fiscal_year");
                ViewBag.FiscalYearAbb = HttpContext.Session.GetString("fiscal_year_abb");
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_DayOffAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from con in _context.tbl_employee_dayoff
                              join cdt in _context.tbl_fiscal_year
                                  on con.fiscal_year equals cdt.fiscal_year
                              join emp in _context.tbl_employee
                                  on con.emp_id equals emp.emp_id
                              where con.id == id
                              select new
                              {
                                  con.id,
                                  con.dayoff_date,
                                  con.emp_id,
                                  con.fiscal_year,
                                  cdt.fiscal_year_abb,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();

                    if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeDayOffViewModel
                    {
                        id = ec.id,
                        dayoff_date = ec.dayoff_date,
                        emp_id = ec.emp_id,
                        fiscal_year = ec.fiscal_year,
                        fiscal_year_abb = ec.fiscal_year_abb,
                        firstname = ec.firstname,
                        middlename = ec.middlename,
                        lastname = ec.lastname,
                        emp_code = ec.emp_code,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_DayOffAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DayOffSave(EmployeeDayOffViewModel model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10113", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            DateTime dayoff_date = model.dayoff_date ?? DateTime.MinValue;
            string fiscal_year = HttpContext.Session.GetString("fiscal_year");
            if (mode == "edit") { fiscal_year = model.fiscal_year ?? ""; }

            int emp_id = model.emp_id ?? 0;

            //check if day off is within fiscal range
            string within = _settingsServices.CheckDateWithinFiscalYear(dayoff_date, fiscal_year);
            if (!string.IsNullOrWhiteSpace(within)) { return Json(new { status = "error", message = within }); }
            //Also check if that is W or H 
            var DateFlag = _settingsServices.GetCalendarDates(dayoff_date, dayoff_date).ToDictionary(d => d.Date, d => d.Flag);
            var flag = DateFlag.ContainsKey(dayoff_date) ? DateFlag[dayoff_date] : string.Empty;
            if (flag == "W")
            {
                return Json(new { status = "error", message = $"Provided date is Weekend." });
            }
            else if (flag == "H")
            {
                return Json(new { status = "error", message = $"Provided date is Holiday." });
            }

            if (mode == "add")
            {
                if (_settingsServices.GetTimesheetDataExistEmployee(fiscal_year, dayoff_date, emp_id)) { return Json(new { status = "invalid", message = "Error in process. Selected date is past date or data already exist in timesheet." }); }
                //check if the data is exits on another record
                var isData = _context.tbl_employee_dayoff.FirstOrDefault(
                    u => u.dayoff_date == dayoff_date && u.emp_id == emp_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                string id = UniqueID();
                var DataSave = new tbl_employee_dayoff
                {
                    id = id,
                    emp_id = emp_id,
                    dayoff_date = dayoff_date,
                    fiscal_year = fiscal_year
                };
                _ = _context.tbl_employee_dayoff.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id });
            }
            else if (mode == "edit")
            {
                string? id = Request.Form["id"];
                // Fetch the existing record first
                var DataUpdate = _context.tbl_employee_dayoff.FirstOrDefault(h => h.id == id);
                DateTime old_dayoff_date = Convert.ToDateTime(DataUpdate.dayoff_date);
                if (_settingsServices.GetTimesheetDataExistEmployee(fiscal_year, old_dayoff_date, emp_id)) { return Json(new { status = "invalid", message = "Error in process. Selected date is past date or data already exist in timesheet." }); }

                //check if the data is exits on another record
                var isData = _context.tbl_employee_dayoff
                        .FirstOrDefault(u => u.dayoff_date == dayoff_date && u.emp_id == emp_id 
                        && u.id != id);
                if (isData != null) { return Json(new { status = "false", message = Lang.msg_record_exist_other }); }

                if (DataUpdate != null)
                {
                    DataUpdate.dayoff_date = dayoff_date;
                    _ = _context.tbl_employee_dayoff.Update(DataUpdate);
                    _ = _context.SaveChanges();
                }
                else
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                return Json(new { status = "success", message = Lang.msg_update_success, DataUpdate.id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DayOffDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10113", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            var recordsToDelete = _context.tbl_employee_dayoff
                .Where(r => request.SelectedIds != null && request.SelectedIds.Contains((r.id ?? "").ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var record in recordsToDelete)
            {
                tSel++;
                var isData = _context.tbl_employee_dayoff.FirstOrDefault(h => h.id == record.id);
                DateTime dayoff_date = Convert.ToDateTime(isData.dayoff_date);
                string fiscal_year = isData.fiscal_year ?? "";
                int emp_id = isData.emp_id ?? 0;
                if (_settingsServices.GetTimesheetDataExistEmployee(fiscal_year, dayoff_date, emp_id))
                {
                    //return Json(new { status = "invalid", message = "Error in process. Selected date is past date or data already exist in timesheet." }); 
                }
                else
                {
                    tDel++;
                    _context.tbl_employee_dayoff.RemoveRange(isData);
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    _context.ChangeTracker.Clear();
                }
            }
            tUDel = tSel - tDel;
            string msg_deleted_records = string.Empty;
            bool msg_status = false;
            if (tUDel > 0)
            {
                msg_deleted_records = Lang.msg_deleted_some;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_deleted_records = msg_deleted_records.Replace("[<UN-DEL-ROWS>]", tUDel.ToString(), StringComparison.Ordinal);
                msg_status = false;
            }
            else
            {
                msg_deleted_records = Lang.msg_delete_success;
                msg_deleted_records = msg_deleted_records.Replace("[<DELETED-ROWS>]", tDel.ToString(), StringComparison.Ordinal);
                msg_status = true;
            }
            return Ok(new
            {
                status = msg_status,
                deletedCount = tDel,
                message = msg_deleted_records
            });
        }
        #endregion
        /********************************************************************************************************************/
        #region CONTRACT SIGNED
        [HttpGet]
        public IActionResult ContractSigned()
        {
            string PageId = "10102";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from con in _context.tbl_employee_signed_contract
                join cdt in _context.tbl_contract_document_template
                on con.contract_document_id equals cdt.contract_document_id
                join emp in _context.tbl_employee
                on con.emp_id equals emp.emp_id
                orderby con.upload_date descending
                select new EmployeeSignedContractViewModel
                {
                    emp_signed_contract_id = con.emp_signed_contract_id,
                    contract_document_id = con.contract_document_id,
                    document_subject = cdt.document_subject,
                    signed_contract = con.signed_contract,
                    upload_date = con.upload_date,
                    emp_id = con.emp_id,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/ContractSigned", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_ContractSigned", Records);
        }
        [HttpPost]
        public async Task<IActionResult> ContractSignedList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1;

            var query = from con in _context.tbl_employee_signed_contract
                        join cdt in _context.tbl_contract_document_template
                        on con.contract_document_id equals cdt.contract_document_id
                        join emp in _context.tbl_employee
                        on con.emp_id equals emp.emp_id
                        select new EmployeeSignedContractViewModel
                        {
                            emp_signed_contract_id = con.emp_signed_contract_id,
                            contract_document_id = con.contract_document_id,
                            signed_contract = con.signed_contract,
                            upload_date = con.upload_date,
                            document_subject = cdt.document_subject,
                            emp_id = con.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
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
            else
            {
                query = query.OrderByDescending(d => d.upload_date);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.document_subject != null && a.document_subject.Contains(searchValue)) ||
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult ContractSignedAddEdit(int? id, string mode)
        {
            string PageId = "10102";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.ContractDocumentSubjectList = _employeeServices.GetContractSubject(0);

            EmployeeSignedContractViewModel model;
            model = new EmployeeSignedContractViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_ContractSignedAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from con in _context.tbl_employee_signed_contract
                              join cdt in _context.tbl_contract_document_template
                                  on con.contract_document_id equals cdt.contract_document_id into cdtJoin
                              from cdt in cdtJoin.DefaultIfEmpty()
                              join emp in _context.tbl_employee
                                  on con.emp_id equals emp.emp_id into empJoin
                              from emp in empJoin.DefaultIfEmpty()
                              where con.emp_signed_contract_id == Convert.ToInt32(id)
                              select new
                              {
                                  con.emp_signed_contract_id,
                                  con.contract_document_id,
                                  cdt.document_subject,
                                  con.signed_contract,
                                  con.upload_date,
                                  con.emp_id,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status,
                                  emp.emp_code
                              }).AsNoTracking().FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new EmployeeSignedContractViewModel
                    {
                        emp_signed_contract_id = ec.emp_signed_contract_id,
                        contract_document_id = ec.contract_document_id,
                        document_subject = ec.document_subject,
                        signed_contract = ec.signed_contract,
                        upload_date = ec.upload_date,
                        emp_id = ec.emp_id,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };

                    if (!string.IsNullOrEmpty(ec.signed_contract))
                    {
                        string extension = Path.GetExtension(ec.signed_contract).TrimStart('.').ToUpperInvariant();
                        ViewBag.download = $@"
                            <a href='{Url.Content($"~/Employee/ContractDocumentDownload?id={ec.emp_signed_contract_id}")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";
                    }
                    string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                    string uploadsFolder = Path.Combine(GblDocumentPath, "documents/contract", ec.signed_contract);
                    ViewBag.fileSize = GetFileSize(uploadsFolder);

                    ViewBag.Employee = ec.employee;
                    ViewBag.ContractSubject = ec.document_subject;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_ContractSignedAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ContractSignedSave(EmployeeSignedContractViewModel model, IFormFile file)
        {
            /*
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("emp_signed_contract_id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10102", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (!FileValidator.ForImagesWithPdf(file)) { return Json(new { status = "error", message = "There is problem with File." }); }
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/contract");

            int contract_document_id = Convert.ToInt32(model.contract_document_id);
            int emp_id = Convert.ToInt32(model.emp_id);
            if (mode == "add")
            {
                int emp_signed_contract_id = (_context.tbl_employee_signed_contract.Any()
                              ? _context.tbl_employee_signed_contract.Max(o => o.emp_signed_contract_id)
                              : 0) + 1;
                var DataSave = new tbl_employee_signed_contract
                {
                    emp_signed_contract_id = emp_signed_contract_id,
                    contract_document_id = contract_document_id,
                    emp_id = emp_id
                };
                _ = _context.tbl_employee_signed_contract.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var isDataSaved = _context.tbl_employee_signed_contract.FirstOrDefault(u => u.emp_signed_contract_id == emp_signed_contract_id);
                if (isDataSaved == null)
                {
                    return Json(new { status = "false", message = "Fail to save contract document." });
                }
                if (file != null)
                {
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        isDataSaved.signed_contract = filename;
                        isDataSaved.upload_date = DateTime.Now;

                        _ = _context.tbl_employee_signed_contract.Update(isDataSaved);
                        _ = _context.SaveChanges();
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Signed Contract Document saved successfully, however the associated file could not be uploaded." });
                    }
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_signed_contract_id });
            }
            else if (mode == "edit")
            {
                int emp_signed_contract_id = Convert.ToInt32(Request.Form["emp_signed_contract_id"]);

                var DataUpdate = _context.tbl_employee_signed_contract
                    .FirstOrDefault(h => h.emp_signed_contract_id == emp_signed_contract_id);

                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                if (file != null)
                {
                    /** DELETE EXISTING FILE | instead of taking from post, better to get from db | security reason **/
                    string hUploadFile = DataUpdate.signed_contract ?? "";
                    if (!string.IsNullOrWhiteSpace(hUploadFile))
                    {
                        string st = DeleteFile(uploadsFolder, hUploadFile);
                        if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            return Json(new { status = "false", message = "Failed to overwite existing document file. Please contact your system administrator." });
                        }
                    }
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        DataUpdate.signed_contract = filename;
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Failed to update document file. Please contact your system administrator." });
                    }
                }

                DataUpdate.upload_date = DateTime.Now;
                _ = _context.tbl_employee_signed_contract.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_signed_contract_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractSignedDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10102", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_signed_contract
                .Where(r => request.SelectedIds.Contains(r.emp_signed_contract_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.ChangeTracker.Clear();

            int DelCnt = 0;
            var deletedIds = new List<int>();
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "documents/contract");

            foreach (var id in request.SelectedIds)
            {
                var recordsToDel = _context.tbl_employee_signed_contract.FirstOrDefault(e => e.emp_signed_contract_id == Convert.ToInt32(id));
                if (recordsToDel != null)
                {
                    string fileName = recordsToDel.signed_contract ?? "";
                    int emp_signed_contract_id = recordsToDel.emp_signed_contract_id;
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string st = DeleteFile(uploadsFolder, fileName);
                        if (string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            DelCnt++;
                            deletedIds.Add(recordsToDel.emp_signed_contract_id);
                        }
                    }
                }
            }
            if (deletedIds.Count > 0)
            {
                var entitiesToDelete = _context.tbl_employee_signed_contract.Where(t => deletedIds.Contains(t.emp_signed_contract_id)).ToList();
                _context.tbl_employee_signed_contract.RemoveRange(entitiesToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", DelCnt.ToString(), StringComparison.Ordinal)
            });
        }
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> ContractSignedDownload(string id)
        {
            string PageId = "10102";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_employee_signed_contract.FirstOrDefault(h => h.emp_signed_contract_id == Convert.ToInt32(id));
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.signed_contract))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "documents/contract");

                string filePath = Path.Combine(uploadsFolder, smt.signed_contract);
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
                        return File(fileBytes, contentType, smt.signed_contract);
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
        #region CONTRACT
        [HttpGet]
        public IActionResult Contract()
        {
            string PageId = "10101";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                        from con in _context.tbl_employee_contract
                        join cdt in _context.tbl_contract_document_template
                            on con.contract_document_id equals cdt.contract_document_id
                        join emp in _context.tbl_employee
                            on con.emp_id equals emp.emp_id
                        orderby con.issue_date descending, con.end_date descending
                        select new EmployeeContractViewModel
                        {
                            emp_contract_id = con.emp_contract_id,
                            contract_document_id = con.contract_document_id,
                            contract_desc = con.contract_desc,
                            issue_date = con.issue_date,
                            end_date = con.end_date,
                            emp_id = con.emp_id,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            contract_status = con.contract_status,
                            document_subject = cdt.document_subject,
                            emp_status = emp.emp_status,
                        }).ToList();
            ViewBag.ContractStatusFilter = StatusActivePassive("AD");
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            //ViewBag.EmployeeFilter = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Contract", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_Contract", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string ContractStatusFilter = request.FilterValue1;
            string EmployeeStatusFilter = request.FilterValue2;

            var query = from con in _context.tbl_employee_contract
                        join cdt in _context.tbl_contract_document_template
                            on con.contract_document_id equals cdt.contract_document_id
                        join emp in _context.tbl_employee
                            on con.emp_id equals emp.emp_id
                        select new EmployeeContractViewModel
                        {
                            emp_contract_id = con.emp_contract_id,
                            contract_document_id = con.contract_document_id,
                            contract_desc = con.contract_desc,
                            issue_date = con.issue_date,
                            end_date = con.end_date,
                            emp_id = con.emp_id,
                            firstname = emp.firstname,
                            middlename = emp.middlename,
                            lastname = emp.lastname,
                            employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                            contract_status = con.contract_status,
                            document_subject = cdt.document_subject,
                            emp_status = emp.emp_status
                        };
            if (!string.IsNullOrEmpty(ContractStatusFilter))
            {
                query = query.Where(d => d.contract_status == ContractStatusFilter);
            }
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
                    (a.contract_desc != null && a.contract_desc.Contains(searchValue)) ||
                    (a.document_subject != null && a.document_subject.Contains(searchValue)) ||
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
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

        public IActionResult ContractAddEdit(int? id, string mode)
        {
            string PageId = "10101";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.ContractDocumentSubjectList = _employeeServices.GetContractSubject(0);
            EmployeeContractViewModel model;
            model = new EmployeeContractViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_ContractAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from con in _context.tbl_employee_contract
                              join cdt in _context.tbl_contract_document_template
                                  on con.contract_document_id equals cdt.contract_document_id
                              join emp in _context.tbl_employee
                                  on con.emp_id equals emp.emp_id
                              where con.emp_contract_id == id
                              select new
                              {
                                  con.emp_contract_id,
                                  con.contract_document_id,
                                  con.contract_desc,
                                  con.issue_date,
                                  con.end_date,
                                  con.emp_id,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  con.contract_status,
                                  cdt.document_subject,
                                  emp.emp_status
                              }).FirstOrDefault();
                    if (ec == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    model = new EmployeeContractViewModel
                    {
                        emp_contract_id = ec.emp_contract_id,
                        contract_document_id = ec.contract_document_id ?? 0,
                        contract_desc = ec.contract_desc,
                        issue_date = ec.issue_date,
                        end_date = ec.end_date,
                        emp_id = ec.emp_id,
                        employee = ec.employee,
                        contract_status = ec.contract_status,
                        document_subject = ec.document_subject,
                        emp_status = ec.emp_status
                    };
                    ViewBag.Employee = ec.employee;
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Employee/_ContractAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ContractSave(EmployeeContractViewModel model)
        {
            _ = ModelState.Remove("emp_contract_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10101", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            int contract_document_id = Convert.ToInt32(model.contract_document_id);
            string? contract_desc = model.contract_desc;
            DateTime? issue_date = model.issue_date;
            DateTime? end_date = model.end_date;
            string? contract_status = model.contract_status;
            int? emp_id = model.emp_id;

            if (mode == "add")
            {
                int emp_contract_id = (_context.tbl_employee_contract.Any()
                                ? _context.tbl_employee_contract.Max(o => o.emp_contract_id)
                                : 0) + 1;
                var DataSave = new tbl_employee_contract
                {
                    emp_contract_id = emp_contract_id,
                    contract_document_id = contract_document_id,
                    contract_desc = contract_desc,
                    issue_date = issue_date,
                    end_date = end_date,
                    emp_id = emp_id,
                    contract_status = contract_status
                };
                _ = _context.tbl_employee_contract.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_contract_id });
            }
            else if (mode == "edit")
            {
                int emp_contract_id = Convert.ToInt32(Request.Form["emp_contract_id"]);

                var DataUpdate = _context.tbl_employee_contract.FirstOrDefault(h => h.emp_contract_id == emp_contract_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.contract_document_id = contract_document_id;
                DataUpdate.contract_desc = contract_desc;
                DataUpdate.issue_date = issue_date;
                DataUpdate.end_date = end_date;
                DataUpdate.emp_id = emp_id;
                DataUpdate.contract_status = contract_status;

                _ = _context.tbl_employee_contract.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_contract_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10101", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_employee_contract
                .Where(r => request.SelectedIds.Contains(r.emp_contract_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_contract.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", request.SelectedIds.Count.ToString(), StringComparison.OrdinalIgnoreCase)
            });
        }
        #endregion
        /********************************************************************************************************************/
        #region ADDRESS 10108
        [HttpGet]
        public IActionResult Address()
        {
            string PageId = "10108";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var Records = (
                from add in _context.tbl_employee_address
                join emp in _context.tbl_employee
                on add.emp_id equals emp.emp_id
                orderby emp.firstname descending
                select new EmployeeAddressViewModel
                {
                    emp_id = add.emp_id,
                    address1 = add.address1,
                    address2 = add.address2,
                    city = add.city,
                    state = add.state,
                    country = add.country,
                    postalcode = add.postalcode,
                    phone1 = add.phone1,
                    phone2 = add.phone2,
                    mobile = add.mobile,
                    personal_email = add.personal_email,
                    firstname = emp.firstname,
                    middlename = emp.middlename,
                    lastname = emp.lastname,
                    employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                    emp_status = emp.emp_status
                }).ToList();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Employee/Address", "ADD|DEL", PageId, Records.Count);
            return PartialView("Employee/_Address", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var EmployeeStatusFilter = request.FilterValue1;

            var query = from add in _context.tbl_employee_address
                        join emp in _context.tbl_employee
                        on add.emp_id equals emp.emp_id
                        select new EmployeeAddressViewModel
                        {
                            emp_id = add.emp_id,
                            address1 = add.address1,
                            address2 = add.address2,
                            city = add.city,
                            state = add.state,
                            country = add.country,
                            postalcode = add.postalcode,
                            phone1 = add.phone1,
                            phone2 = add.phone2,
                            mobile = add.mobile,
                            personal_email = add.personal_email,
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
                    (a.firstname != null && a.firstname.Contains(searchValue)) ||
                    (a.middlename != null && a.middlename.Contains(searchValue)) ||
                    (a.lastname != null && a.lastname.Contains(searchValue))
                );
            }

            var data = query.ToList();
            int totalRecord = data.Count();
            if (pageSize == -1)
                pageSize = totalRecord;
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
        public IActionResult AddressAddEdit(int? id, string mode)
        {
            string PageId = "10108";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeNotHavingAddress();

            EmployeeAddressViewModel model;
            model = new EmployeeAddressViewModel();
            ViewBag.Country = GetCountries(model.country);

            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Employee/_AddressAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var ec = (from add in _context.tbl_employee_address
                              join emp in _context.tbl_employee
                                  on add.emp_id equals emp.emp_id
                              where add.emp_id == id
                              select new
                              {
                                  add.emp_id,
                                  add.address1,
                                  add.address2,
                                  add.city,
                                  add.state,
                                  add.country,
                                  add.postalcode,
                                  add.phone1,
                                  add.phone2,
                                  add.mobile,
                                  add.personal_email,
                                  emp.firstname,
                                  emp.middlename,
                                  emp.lastname,
                                  emp.emp_code,
                                  employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                                  emp.emp_status
                              }).FirstOrDefault();

                    if (ec == null) { return BadRequest(new { success = false, message = Lang.msg_error }); }
                    model = new EmployeeAddressViewModel
                    {
                        emp_id = ec.emp_id,
                        address1 = ec.address1,
                        address2 = ec.address2,
                        city = ec.city,
                        state = ec.state,
                        country = ec.country,
                        postalcode = ec.postalcode,
                        phone1 = ec.phone1,
                        phone2 = ec.phone2,
                        mobile = ec.mobile,
                        personal_email = ec.personal_email,
                        firstname = ec.firstname,
                        middlename = ec.middlename,
                        lastname = ec.lastname,
                        employee = ec.employee,
                        emp_status = ec.emp_status
                    };
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    ViewBag.Employee = ec.employee;
                    return PartialView("Employee/_AddressAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddressSave(EmployeeAddressViewModel model)
        {
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }

            string? mode = Request.Form["mode"];
            string? address1 = model.address1;
            string? address2 = model.address2;
            string? city = model.city;
            string? state = model.state;
            string? country = model.country;
            string? postalcode = model.postalcode;
            string? phone1 = model.phone1;
            string? phone2 = model.phone2;
            string? mobile = model.mobile;
            string? personal_email = model.personal_email;
            int emp_id = model.emp_id;

            if (!_accountServices.HasPermission("10108", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (mode == "add")
            {
                var DataSave = new tbl_employee_address
                {
                    emp_id = emp_id,
                    address1 = address1,
                    address2 = address2,
                    city = city,
                    state = state,
                    country = country,
                    postalcode = postalcode,
                    phone1 = phone1,
                    phone2 = phone2,
                    mobile = mobile,
                    personal_email = personal_email
                };
                _ = _context.tbl_employee_address.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = emp_id });
            }
            else if (mode == "edit")
            {
                int id = model.emp_id;
                var DataUpdate = _context.tbl_employee_address
                    .FirstOrDefault(h => h.emp_id == emp_id);

                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.address1 = address1;
                DataUpdate.address2 = address2;
                DataUpdate.city = city;
                DataUpdate.state = state;
                DataUpdate.country = country;
                DataUpdate.postalcode = postalcode;
                DataUpdate.phone1 = phone1;
                DataUpdate.phone2 = phone2;
                DataUpdate.mobile = mobile;
                DataUpdate.personal_email = personal_email;

                _ = _context.tbl_employee_address.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.emp_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10108", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || !request.SelectedIds.Any())
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            var recordsToDelete = _context.tbl_employee_address
                .Where(r => request.SelectedIds.Contains(r.emp_id.ToString()))
                .ToList();
            if (!recordsToDelete.Any())
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_employee_address.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", request.SelectedIds.Count.ToString(), StringComparison.OrdinalIgnoreCase)
            });
        }

        #endregion
        /********************************************************************************************************************/
    }
}
