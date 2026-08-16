using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Text;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.General;
using wwfpp.Models.Request;
using wwfpp.Models.Settings;
using wwfpp.Services;
using static GblUtilities;
/*
 * Master File
 */
namespace wwfpp.Controllers
{
    public class SettingsController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        GlobalOptionServices globalOptionServices,
        EmployeeServices employeeServices,
        SettingsServices settingsServices,
        AccountServices accountServices,
        LeaveServices leaveServices
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;
        private readonly EmployeeServices _employeeServices = employeeServices;
        private readonly SettingsServices _settingsServices = settingsServices;
        private readonly AccountServices _accountServices = accountServices;
        private readonly LeaveServices _leaveServices = leaveServices;

        public IActionResult Index()
        {
            return View();
        }
        /********************************************************************************************************************/
        #region DIFFERENTIAL SALARY

        [HttpGet]
        public IActionResult DifferentialSalary()
        {
            #region FOR PERMISSION
            string PageId = "10569";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_employee_salary_diff
                join b in _context.tbl_fiscal_year
                on a.fiscal_year equals b.fiscal_year
                join emp in _context.tbl_employee
                on a.emp_id equals emp.emp_id
                orderby a.fiscal_year descending, emp.firstname ascending, emp.middlename ascending, emp.lastname ascending
                select new DifferentialSalaryViewModel
                {
                    id = a.id,
                    fiscal_year = a.fiscal_year,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_year = a.emp_year,
                    emp_month = a.emp_month,
                    basic_salary = a.basic_salary,
                    pf_a = a.pf_a,
                    gratuity_a = a.gratuity_a,
                    ssf_a = a.ssf_a,
                    pf_d = a.pf_d,
                    gratuity_d = a.gratuity_d,
                    ssf_d = a.ssf_d
                }).ToList();

            ViewBag.FiscalFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/DifferentialSalary", "IMPORT|DOWNLOAD-FORMAT|DEL-SD", PageId, Records.Count);
            return PartialView("Settings/_DifferentialSalary", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DifferentialSalaryList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string? FiscalYearFilter = request.FilterValue;
            var query = from a in _context.tbl_employee_salary_diff
                        join b in _context.tbl_fiscal_year
                        on a.fiscal_year equals b.fiscal_year
                        join emp in _context.tbl_employee
                        on a.emp_id equals emp.emp_id
                        where string.IsNullOrWhiteSpace(searchValue)
                || (emp.firstname != null && emp.firstname.Contains(searchValue))
                || (emp.middlename != null && emp.middlename.Contains(searchValue))
                || (emp.lastname != null && emp.lastname.Contains(searchValue))
                        orderby a.fiscal_year descending, emp.firstname ascending, emp.middlename ascending, emp.lastname ascending
                        select new DifferentialSalaryViewModel
                        {
                            id = a.id,
                            fiscal_year = a.fiscal_year,
                            employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                            emp_year = (short)a.emp_year,
                            emp_month = (byte)a.emp_month,
                            basic_salary = (decimal)a.basic_salary,
                            pf_a = (decimal)a.pf_a,
                            gratuity_a = (decimal)a.gratuity_a,
                            ssf_a = (decimal)a.ssf_a,
                            pf_d = (decimal)a.pf_d,
                            gratuity_d = (decimal)a.gratuity_d,
                            ssf_d = (decimal)a.ssf_d
                        };
            if (!string.IsNullOrEmpty(FiscalYearFilter)) { query = query.Where(a => a.fiscal_year == FiscalYearFilter); }
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

        public IActionResult DifferentialSalaryAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10569";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = "import";

            var model = new DifferentialSalaryViewModel
            {
                fiscal_year = HttpContext.Session.GetString("fiscal_year"),
                fiscal_year_abb ="",
                emp_id = 0,
                employee = "",
                emp_year = 0,
                emp_month = 0,
                basic_salary = 0,
                pf_a = 0,
                gratuity_a = 0,
                ssf_a = 0,
                pf_d = 0,
                gratuity_d = 0,
                ssf_d = 0,
                emp_code = ""
            };
            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.YearFilter = _settingsServices.GetYears(0);
            ViewBag.MonthFilter = _settingsServices.GetMonths();
            ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
            return PartialView("Settings/_DifferentialSalaryAddEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DifferentialSalaryImportSave(DifferentialSalaryViewModel model, IFormFile file)
        {
            if (file == null || file.Length == 0) { return Json(new { status = "false", message = "No file selected" }); }

            //such me csv data tha?
            if (!FileValidator.ForCsv(file)) { return Json(new { status = "false", message = "Only CSV files are allowed." }); }

            string fiscal_year = model.fiscal_year;
            short emp_year = model.emp_year;
            byte emp_month = model.emp_month;
            if (string.IsNullOrWhiteSpace(fiscal_year) || emp_year < 1 || emp_month < 1)
            {
                return Json(new { status = "false", message = Lang.msg_insufficient_info });
            }

            //fiscal range bhitra ko year ra month ho ke?
            var NewDate = new DateTime(emp_year, emp_month, 14);
            var sql = _context.tbl_fiscal_year.FirstOrDefault(
                u => u.fiscal_year == fiscal_year &&
                NewDate >= u.date_from && NewDate <= u.date_to
                );
            if (sql == null)
            {
                return Json(new { status = "false", message = "Year/Month is not in between Fiscal Year Range" });
            }

            //nhaplake import ya dunkala la?
            var isData = _context.tbl_employee_salary_diff.FirstOrDefault(d => d.fiscal_year == fiscal_year);
            if (isData != null)
            {
               return Json(new { status = "false", message = "Differential Salary already imported for the selected period" });
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var DifferentialSalary = new List<tbl_employee_salary_diff>();
            string? line;
            bool isHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader) { isHeader = false; continue; } // skip header

                var parts = line.Split(','); // basic split

                //validation
                decimal basic_salary = decimal.TryParse(parts[3], out decimal basic) ? basic : 0;
                decimal pf_a = decimal.TryParse(parts[4], out decimal pfa) ? pfa : 0;
                decimal gratuity_a = decimal.TryParse(parts[5], out decimal gra) ? gra : 0;
                decimal ssf_a = decimal.TryParse(parts[6], out decimal ssfa) ? ssfa : 0;
                decimal pf_d = decimal.TryParse(parts[7], out decimal pfd) ? pfd : 0;
                decimal gratuity_d = decimal.TryParse(parts[8], out decimal gram) ? gram : 0;
                decimal ssf_d = decimal.TryParse(parts[9], out decimal ssfd) ? ssfd : 0;

                string emp_code = !string.IsNullOrWhiteSpace(parts[2]) ? parts[2].Replace("\"", "", StringComparison.OrdinalIgnoreCase) : "";
                emp_code = emp_code.PadLeft(6, '0');

                //try to get emp_id using code
                var emp = _context.tbl_employee.FirstOrDefault(e => e.emp_code == emp_code);
                if (emp != null)
                {
                    int emp_id = emp.emp_id;
                    var diffSal = new tbl_employee_salary_diff
                    {
                        emp_id = emp_id,
                        emp_year = emp_year,
                        emp_month = emp_month,
                        basic_salary = basic_salary,
                        pf_a = pf_a,
                        gratuity_a = gratuity_a,
                        ssf_a = ssf_a,
                        pf_d = pf_d,
                        gratuity_d = gratuity_d,
                        ssf_d = ssf_d,
                        emp_code = emp_code,
                        fiscal_year = fiscal_year
                    };
                    DifferentialSalary.Add(diffSal);
                }
            }
            _context.tbl_employee_salary_diff.AddRange(DifferentialSalary);
            _context.SaveChanges();
            return Json(new { status = "success", message = "CSV imported successfully" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DifferentialSalaryExport()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine("SN, Employee Name, Employee ID, Basic Salary, PF(+), Gratuity(+), SSF (+), PF(-), Gratuity(-), SSF (-)");

            int cnt = 0;
            var Records = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && _context.tbl_employee_salary_extra_settings
                    .Where(ses => ses.is_field_salary != "Y").Select(ses => ses.emp_id).Distinct().Contains(emp.emp_id)
                    )
                .OrderBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .Select(emp => new DifferentialSalaryExportViewModel
                {
                    emp_code = emp.emp_code,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname }.Where(x => !string.IsNullOrEmpty(x))),
                }).ToList();
            if (Records.Count > 0)
            {
                foreach (var record in Records)
                {
                    cnt++;
                    string emp_code = EscapeCSV(record.emp_code ?? "");
                    string employee = EscapeCSV(record.employee ?? "");
                    _ = sb.AppendLine($"{cnt},\"{employee}\",\"{emp_code}\",0,0,0,0,0,0,0");
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "staff-employee-salary-differential-downloaded-format.csv");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DifferentialSalaryDelete([FromBody] CostumFilterRequest request)
        {
            if (!_accountServices.HasPermission("10569", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            string FiscalYear = request.FilterValue;
            if (string.IsNullOrWhiteSpace(FiscalYear))
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            /**check if any records used in another*/
            var isData = _context.tbl_employee_salary_diff.FirstOrDefault(d => d.fiscal_year == FiscalYear);
            if (isData == null)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            short emp_year = isData.emp_year;
            byte emp_month = isData.emp_month;

            var isUsed = _context.tbl_employee_salary.FirstOrDefault(s => s.sal_year == emp_year && s.sal_month == emp_month );
            if (isUsed == null)
            {
                return BadRequest(new { status = false, message = Lang.msg_delete_fail });
            }
            await _context.tbl_employee_salary_diff.Where(d => d.fiscal_year == FiscalYear).ExecuteDeleteAsync();

            return Ok(new
            {
                status = true,
                message = Lang.msg_delete_success
            });
        }

        #endregion
        /********************************************************************************************************************/
        #region DIFFERENTIAL SALARY PERIOD

        [HttpGet]
        public IActionResult DifferentialSalaryPeriod()
        {
            #region FOR PERMISSION
            string PageId = "10554";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_salary_differential_month
                join b in _context.tbl_fiscal_year
                on a.fiscal_year equals b.fiscal_year
                orderby a.fiscal_year descending
                select new DifferentialSalaryPeriodViewModel
                {
                    fiscal_year = a.fiscal_year,
                    fiscal_year_abb = b.fiscal_year_abb,
                    sal_year = a.sal_year,
                    sal_month = a.sal_month
                }).ToList();

            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/DifferentialSalaryPeriod", "ADD|DEL", PageId, Records.Count);
            return PartialView("Settings/_DifferentialSalaryPeriod", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DifferentialSalaryPeriodList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = from a in _context.tbl_salary_differential_month
                        join b in _context.tbl_fiscal_year
                        on a.fiscal_year equals b.fiscal_year
                        orderby a.fiscal_year descending
                        select new DifferentialSalaryPeriodViewModel
                        {
                            fiscal_year = a.fiscal_year,
                            fiscal_year_abb = b.fiscal_year_abb,
                            sal_year = a.sal_year,
                            sal_month = a.sal_month
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                (a.fiscal_year != null && a.fiscal_year.Contains(searchValue))
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
        public IActionResult DifferentialSalaryPeriodAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10554";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.SalYear = _settingsServices.GetYears(0);
            ViewBag.SalMonth = _settingsServices.GetMonths();

            if (mode == "add")
            {
                var model = new DifferentialSalaryPeriodViewModel
                {
                    fiscal_year = HttpContext.Session.GetString("fiscal_year") ?? "",
                    fiscal_year_abb = HttpContext.Session.GetString("fiscal_year_abb") ?? "",
                    sal_year = 0,
                    sal_month = 0
                };
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_DifferentialSalaryPeriodAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (
                        from a in _context.tbl_salary_differential_month
                        join b in _context.tbl_fiscal_year
                        on a.fiscal_year equals b.fiscal_year
                        where a.fiscal_year == id
                        select new
                        {
                            a.fiscal_year,
                            b.fiscal_year_abb,
                            a.sal_year,
                            a.sal_month
                        }).FirstOrDefault();
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new DifferentialSalaryPeriodViewModel
                        {
                            fiscal_year = smt.fiscal_year,
                            fiscal_year_abb = smt.fiscal_year_abb,
                            sal_year = smt.sal_year,
                            sal_month = smt.sal_month
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_DifferentialSalaryPeriodAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DifferentialSalaryPeriodSave(DifferentialSalaryPeriodViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10554", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string fiscal_year = model.fiscal_year ?? "";
            short sal_year = model.sal_year ?? 0;
            byte sal_month = model.sal_month ?? 0;

            if (string.IsNullOrWhiteSpace(fiscal_year) || sal_year < 1 || sal_month < 1)
            {
                return Json(new { status = "false", message = Lang.msg_insufficient_info });
            }

            DateTime NewDate = new DateTime(sal_year, sal_month, 14);
            var sql = _context.tbl_fiscal_year.FirstOrDefault(
                u => u.fiscal_year == fiscal_year &&
                NewDate >= u.date_from && NewDate <= u.date_to
                );
            if (sql == null)
            {
                return Json(new { status = "false", message = "Year/Month is not in between Fiscal Year Range" });
            }

            if (mode == "add")
            {
                /**check if the data is exits on another record */
                var isData = _context.tbl_salary_differential_month.FirstOrDefault(u => u.fiscal_year == fiscal_year);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataSave = new tbl_salary_differential_month
                {
                    fiscal_year = fiscal_year,
                    sal_year = sal_year,
                    sal_month = sal_month
                };
                _ = _context.tbl_salary_differential_month.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                var DataUpdate = _context.tbl_salary_differential_month.FirstOrDefault(h => h.fiscal_year == fiscal_year);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                DataUpdate.sal_year = sal_year;
                DataUpdate.sal_month = sal_month;
                _ = _context.tbl_salary_differential_month.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DifferentialSalaryPeriodDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10554", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            /** Validate input**/
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            /**check if any records in */
            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var item in request.SelectedIds)
            {
                tSel++;
                var isData = _context.tbl_salary_differential_month.FirstOrDefault(d => d.fiscal_year == item);
                if (isData != null)
                {
                    short sal_year = isData.sal_year ?? 0;
                    byte sal_month = isData.sal_month ?? 0;

                    if (sal_month > 0 && sal_year > 0)
                    {
                        var smt = _context.tbl_employee_salary.FirstOrDefault(s => s.sal_year == sal_year && s.sal_month == sal_month);
                        if (smt == null)
                        {
                            tDel++;
                            _context.tbl_salary_differential_month.RemoveRange(isData);
                            _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                            _context.ChangeTracker.Clear();
                        }
                    }
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
        #region LEAVE PAID CLEARED
        [HttpGet]
        public IActionResult LeavePaidCleared()
        {
            #region FOR PERMISSION
            string PageId = "10556";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from emr in _context.tbl_employee_leave_indv_paid_cleared_new
                join emp in _context.tbl_employee
                on emr.emp_id equals emp.emp_id
                orderby emp.firstname descending, emp.middlename descending, emp.lastname descending
                select new LeavePaidClearedViewModel
                {
                    id = emr.indv_leave_id,
                    emp_id = Convert.ToInt32(emp.emp_id),
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    gender = emp.gender == "M" ? "Male" : "Female",
                    fiscal_year = emr.fiscal_year,
                    date_from = Convert.ToDateTime(emr.date_from),
                    date_upto = Convert.ToDateTime(emr.date_upto),
                    submit_counter = emr.submit_counter ?? 0,
                    remarks = emr.remarks,

                }).ToList();

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/LeavePaidCleared", "ADD", PageId, 0);
            return PartialView("Settings/_LeavePaidCleared", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavePaidClearedList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string? FiscalYearFilter = request.FiscalYearFilter;
            string? EmployeeStatusFilter = request.EmployeeStatusFilter;
            var query = from emr in _context.tbl_employee_leave_indv_paid_cleared_new
                        join emp in _context.tbl_employee
                        on emr.emp_id equals emp.emp_id
                        where string.IsNullOrWhiteSpace(searchValue)
                || (emp.firstname != null && emp.firstname.Contains(searchValue))
                || (emp.middlename != null && emp.middlename.Contains(searchValue))
                || (emp.lastname != null && emp.lastname.Contains(searchValue))
                        orderby emp.firstname ascending, emp.middlename ascending, emp.lastname ascending, emr.submit_counter ascending
                        select new LeavePaidClearedViewModel
                        {
                            id = emr.indv_leave_id,
                            emp_id = Convert.ToInt32(emp.emp_id),
                            employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                            emp_status = emp.emp_status,
                            gender = emp.gender == "M" ? "Male" : "Female",
                            fiscal_year = emr.fiscal_year,
                            date_from = Convert.ToDateTime(emr.date_from),
                            date_upto = Convert.ToDateTime(emr.date_upto),
                            submit_counter = emr.submit_counter ?? 0,
                            remarks = emr.remarks
                        };
            if (!string.IsNullOrEmpty(FiscalYearFilter)) { query = query.Where(emr => emr.fiscal_year == FiscalYearFilter); }
            if (!string.IsNullOrEmpty(EmployeeStatusFilter)) { query = query.Where(emp => emp.emp_status == EmployeeStatusFilter); }
            var data = query.ToList().OrderBy(x => x.employee);
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
        public IActionResult LeavePaidClearedAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10556";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;

            if (mode == "add")
            {
                //List blank page with necessary data
                var model = new LeavePaidClearedViewModelNew { };

                ViewBag.CurrentFiscal = HttpContext.Session.GetString("fiscal_year_abb");
                ViewBag.FiscalFilterM = HttpContext.Session.GetString("fiscal_year");
                ViewBag.EmployeeFilterM = _employeeServices.GetEmployeeList("A");
                ViewBag.PeriodFilter = GetPeriod();
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                return PartialView("Settings/_LeavePaidClearedAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    int indv_leave_id = int.TryParse(id, out int parseId) ? parseId : 0;
                    var smt = (
                        from jlo in _context.tbl_employee_leave_indv_paid_cleared_new
                        join emp in _context.tbl_employee
                        on jlo.emp_id equals emp.emp_id
                        where jlo.indv_leave_id == indv_leave_id
                        select new
                        {
                            jlo.indv_leave_id,
                            emp.emp_id,
                            emp.firstname,
                            emp.middlename,
                            emp.lastname,
                            emp.emp_code,
                            emp.emp_status,
                            emp.gender,
                            jlo.fiscal_year,
                            jlo.annual_leave_caf,
                            jlo.sick_leave_caf,
                            jlo.annual_leave,
                            jlo.sick_leave,
                            jlo.casual_leave,
                            jlo.other_leave,
                            jlo.maternity,
                            jlo.paternity,
                            jlo.mourning,
                            jlo.unpaid_study,
                            jlo.annual_leave_caf_paid,
                            jlo.sick_leave_caf_paid,
                            jlo.annual_leave_paid,
                            jlo.casual_leave_paid,
                            jlo.sick_leave_paid,
                            jlo.other_leave_paid,
                            jlo.maternity_paid,
                            jlo.paternity_paid,
                            jlo.mourning_paid,
                            jlo.unpaid_study_paid,
                            jlo.annual_leave_caf_laps,
                            jlo.sick_leave_caf_laps,
                            jlo.annual_leave_laps,
                            jlo.casual_leave_laps,
                            jlo.sick_leave_laps,
                            jlo.other_leave_laps,
                            jlo.maternity_laps,
                            jlo.paternity_laps,
                            jlo.mourning_laps,
                            jlo.unpaid_study_laps,
                            jlo.date_from,
                            jlo.date_upto,
                            jlo.submit_counter,
                            jlo.remarks
                        }).FirstOrDefault();

                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        /*'---------------------------------------------------------------------'
                        '*CALCULATE TAKEN LEAVES UP TO PROVIDED DATE RANGE*'
                        '---------------------------------------------------------------------'*/
                        int emp_id = smt.emp_id;
                        DateTime date_from = (DateTime)smt.date_from;
                        DateTime date_upto = (DateTime)smt.date_upto;
                        double annual_leave_caf_t = 0;
                        double sick_leave_caf_t = 0;
                        double annual_leave_t = _leaveServices.GetLeaveTaken(1, emp_id, date_from, date_upto);
                        double casual_leave_t = _leaveServices.GetLeaveTaken(3, emp_id, date_from, date_upto);
                        double sick_leave_t = _leaveServices.GetLeaveTaken(5, emp_id, date_from, date_upto);
                        double other_leave_t = _leaveServices.GetLeaveTaken(9, emp_id, date_from, date_upto);
                        double maternity_t = _leaveServices.GetLeaveTaken(12, emp_id, date_from, date_upto);
                        double paternity_t = _leaveServices.GetLeaveTaken(13, emp_id, date_from, date_upto);
                        double mourning_t = _leaveServices.GetLeaveTaken(14, emp_id, date_from, date_upto);
                        double unpaid_study_t = _leaveServices.GetLeaveTaken(15, emp_id, date_from, date_upto);
                        /*---------------------------------------------------------------------'
                        '*CALCULATE LEAVES BALANCE FOR NEXT PERIOD*'
                        '---------------------------------------------------------------------'*/
                        double annual_leave_caf_n = (double)smt.annual_leave_caf - annual_leave_caf_t - (double)smt.annual_leave_caf_paid - (double)smt.annual_leave_caf_laps;
                        double sick_leave_caf_n = (double)smt.sick_leave_caf - sick_leave_caf_t - (double)smt.sick_leave_caf_paid - (double)smt.sick_leave_caf_laps;
                        double annual_leave_n = (double)smt.annual_leave - annual_leave_t - (double)smt.annual_leave_paid - (double)smt.annual_leave_laps;
                        double sick_leave_n = (double)smt.sick_leave - sick_leave_t - (double)smt.sick_leave_paid - (double)smt.sick_leave_laps;
                        double casual_leave_n = (double)smt.casual_leave - casual_leave_t - (double)smt.casual_leave_paid - (double)smt.casual_leave_laps;
                        double maternity_n = (double)smt.maternity - maternity_t - (double)smt.maternity_paid - (double)smt.maternity_laps;
                        double paternity_n = (double)smt.paternity - paternity_t - (double)smt.paternity_paid - (double)smt.paternity_laps;
                        double mourning_n = (double)smt.mourning - mourning_t - (double)smt.mourning_paid - (double)smt.mourning_laps;
                        double unpaid_study_n = (double)smt.unpaid_study - unpaid_study_t - (double)smt.unpaid_study_paid - (double)smt.unpaid_study_laps;
                        double other_leave_n = (double)smt.other_leave - other_leave_t - (double)smt.other_leave_paid - (double)smt.other_leave_laps;
                        var model = new LeavePaidClearedViewModelNew
                        {
                            indv_leave_id = smt.indv_leave_id,
                            emp_id = smt.emp_id,
                            employee = string.Join(" ", new[] { smt.firstname, smt.middlename, smt.lastname, '(' + smt.emp_code + ')', smt.gender == "M" ? "[Male]" : "[Female]" }.Where(x => !string.IsNullOrEmpty(x))),
                            fiscal_year = smt.fiscal_year,
                            annual_leave_caf = smt.annual_leave_caf ?? 0,
                            sick_leave_caf = smt.sick_leave_caf ?? 0,
                            annual_leave = smt.annual_leave ?? 0,
                            sick_leave = smt.sick_leave ?? 0,
                            casual_leave = smt.casual_leave ?? 0,
                            other_leave = smt.other_leave ?? 0,
                            maternity = smt.maternity ?? 0,
                            paternity = smt.paternity ?? 0,
                            mourning = smt.mourning ?? 0,
                            unpaid_study = smt.unpaid_study ?? 0,
                            annual_leave_caf_paid = smt.annual_leave_caf_paid ?? 0,
                            sick_leave_caf_paid = smt.sick_leave_caf_paid ?? 0,
                            annual_leave_paid = smt.annual_leave_paid ?? 0,
                            casual_leave_paid = smt.casual_leave_paid ?? 0,
                            sick_leave_paid = smt.sick_leave_paid ?? 0,
                            other_leave_paid = smt.other_leave_paid ?? 0,
                            maternity_paid = smt.maternity_paid ?? 0,
                            paternity_paid = smt.paternity_paid ?? 0,
                            mourning_paid = smt.mourning_paid ?? 0,
                            unpaid_study_paid = smt.unpaid_study_paid ?? 0,
                            annual_leave_caf_laps = smt.annual_leave_caf_laps ?? 0,
                            sick_leave_caf_laps = smt.sick_leave_caf_laps ?? 0,
                            annual_leave_laps = smt.annual_leave_laps ?? 0,
                            casual_leave_laps = smt.casual_leave_laps ?? 0,
                            sick_leave_laps = smt.sick_leave_laps ?? 0,
                            other_leave_laps = smt.other_leave_laps ?? 0,
                            maternity_laps = smt.maternity_laps ?? 0,
                            paternity_laps = smt.paternity_laps ?? 0,
                            mourning_laps = smt.mourning_laps ?? 0,
                            unpaid_study_laps = smt.unpaid_study_laps ?? 0,
                            date_from = smt.date_from,
                            date_upto = smt.date_upto,
                            submit_counter = smt.submit_counter,
                            remarks = smt.remarks,
                            annual_leave_caf_t = annual_leave_caf_t,
                            sick_leave_caf_t = sick_leave_caf_t,
                            annual_leave_t = annual_leave_t,
                            casual_leave_t = casual_leave_t,
                            sick_leave_t = sick_leave_t,
                            other_leave_t = other_leave_t,
                            maternity_t = maternity_t,
                            paternity_t = paternity_t,
                            mourning_t = mourning_t,
                            unpaid_study_t = unpaid_study_t,
                            annual_leave_caf_n = annual_leave_caf_n,
                            sick_leave_caf_n = sick_leave_caf_n,
                            annual_leave_n = annual_leave_n,
                            sick_leave_n = sick_leave_n,
                            casual_leave_n = casual_leave_n,
                            maternity_n = maternity_n,
                            paternity_n = paternity_n,
                            mourning_n = mourning_n,
                            unpaid_study_n = unpaid_study_n,
                            other_leave_n = other_leave_n,
                            total_annual_leave = (double)smt.annual_leave_caf_paid + (double)smt.annual_leave_paid,
                            total_sick_leave = (double)smt.sick_leave_caf_paid + (double)smt.sick_leave_paid
                        };
                        ViewBag.date_from = Convert.ToDateTime(smt.date_from).ToString(_appSettings.DATE_FORMAT);
                        ViewBag.date_upto = Convert.ToDateTime(smt.date_upto).ToString(_appSettings.DATE_FORMAT);
                        //ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_LeavePaidClearedAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavePaidClearedIsPeriodEligibleToCalculate([FromForm] MultipleCostumFilterRequest request)
        {
            string FiscalFilter = request.FilterValue1;
            int EmployeeFilter = int.TryParse(request.FilterValue2, out int EmpId) ? EmpId : 0;
            int PeriodFilter = int.TryParse(request.FilterValue3, out int Counter) ? Counter : 0;
            string cData = "N";
            if (EmployeeFilter == 0 || PeriodFilter == 0 || string.IsNullOrWhiteSpace(FiscalFilter)) { return new JsonResult(new { data = cData }); }
            var DateFromF = DateTime.MinValue;
            var DateUptoF = DateTime.MinValue;
            var DateFrom = DateTime.MinValue;
            var DateUpto = DateTime.MinValue;

            if (PeriodFilter == 2)
            {
                var smt = _context.tbl_employee_leave_indv_paid_cleared_new
                    .Where(jlo => jlo.emp_id == EmployeeFilter && jlo.submit_counter == 1
                            && jlo.fiscal_year == FiscalFilter).FirstOrDefault();

                if (smt == null)
                {
                    return new JsonResult(new { data = "N", DateFrom = "", DateUpto = "" });
                    //1 nai chhaina 2 halna khojne?
                }
                DateFromF = Convert.ToDateTime(smt.date_from);
                DateUptoF = Convert.ToDateTime(smt.date_upto);
            }
            var query = _context.tbl_employee_leave_indv_paid_cleared_new
                .Where(jlo => jlo.emp_id == EmployeeFilter && jlo.submit_counter == PeriodFilter
                        && jlo.fiscal_year == FiscalFilter).FirstOrDefault();

            if (query == null)
            {
                cData = "Y";
                string fiscalDateStart = HttpContext.Session.GetString("date_from");
                string fiscalDateEnd = HttpContext.Session.GetString("date_to");
                var fyDateStart = DateTime.TryParse(fiscalDateStart, out var pStartDate) ? pStartDate : DateTime.MinValue;
                var fyDateEnd = DateTime.TryParse(fiscalDateEnd, out var pEndDate) ? pEndDate : DateTime.MinValue;

                var sql = _context.tbl_employee.Where(emp => emp.emp_id == EmployeeFilter).FirstOrDefault();
                if (sql != null)
                {
                    DateFrom = Convert.ToDateTime(sql.join_date);
                    DateUpto = Convert.ToDateTime(sql.end_date);
                    if (PeriodFilter == 1)
                    {
                        if (DateFrom <= fyDateStart) { DateFrom = fyDateStart; }
                        if (DateUpto >= fyDateEnd) { DateUpto = fyDateEnd; }
                    }
                    else
                    {
                        DateFrom = DateUptoF.AddDays(1);
                        DateUpto = (DateUpto >= fyDateEnd) ? fyDateEnd : DateUpto;
                    }
                }
                return new JsonResult(new { data = cData, dateFrom = DateFrom.ToString(_appSettings.DATE_FORMAT), dateUpto = DateUpto.ToString(_appSettings.DATE_FORMAT) });

            }
            else
            {
                return new JsonResult(new { data = "N", DateFrom = "", DateUpto = "" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeavePaidClearedLoad([FromForm] MultipleCostumFilterRequest request)
        {
            #region FOR PERMISSION
            string PageId = "10556";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            string FiscalFilter = HttpContext.Session.GetString("fiscal_year");
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalFilter, "normal_working_hrs"));

            int EmployeeFilter = int.TryParse(request.FilterValue1, out int EmpId) ? EmpId : 0;
            int PeriodFilter = int.TryParse(request.FilterValue2, out int SumitCounter) ? SumitCounter : 1;
            var date_fromT = request.FilterValue3;
            var date_uptoT = request.FilterValue4;
            var calculate = request.FilterValue5;

            var date_from = DateTime.TryParse(date_fromT, out var dfParse) ? dfParse : DateTime.MinValue;
            var date_upto = DateTime.TryParse(date_uptoT, out var dtParse) ? dtParse : DateTime.MinValue;

            if (EmployeeFilter < 1 || string.IsNullOrWhiteSpace(FiscalFilter) || string.IsNullOrWhiteSpace(date_from.ToString()) || string.IsNullOrWhiteSpace(date_upto.ToString()))
            {
                return new JsonResult(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            var sql = _context.tbl_employee.Where(emp => emp.emp_id == EmployeeFilter).FirstOrDefault();
            string gender = "";
            if (sql != null) { gender = sql.gender; }
            /** ***********************************************
            'NOTE:
            'What we have assumed here is
            'When paid/clear made, all the value should be made paid or clear
            'so the next period will be zero
            '************************************************/
            /**---------------------------------------------------------------------'
             * * GET DEFAULT LEAVE VALUES
             *'---------------------------------------------------------------------*/
            double hrs_in_years = 365 * workingHoursDays;
            double yrs_annual_hrs = _leaveServices.GetYearlyLeaveHours(1);
            double yrs_casual_hrs = _leaveServices.GetYearlyLeaveHours(3);
            double yrs_sick_hrs = _leaveServices.GetYearlyLeaveHours(5);
            double yrs_others_hrs = _leaveServices.GetYearlyLeaveHours(9);
            double yrs_maternity_hrs = _leaveServices.GetYearlyLeaveHours(12);
            double yrs_paternity_hrs = _leaveServices.GetYearlyLeaveHours(13);
            double yrs_mourning_hrs = _leaveServices.GetYearlyLeaveHours(14);
            double yrs_unpaid_hrs = _leaveServices.GetYearlyLeaveHours(15);
            yrs_maternity_hrs = (gender == "Female") ? yrs_maternity_hrs : 0;
            yrs_paternity_hrs = (gender == "Male") ? yrs_paternity_hrs : 0;
            /*'---------------------------------------------------------------------'
            '*CALCULATE TAKEN LEAVES UP TO PROVIDED DATE RANGE*'
            '---------------------------------------------------------------------'*/
            double annual_leave_t = _leaveServices.GetLeaveTaken(1, EmployeeFilter, date_from, date_upto);
            double casual_leave_t = _leaveServices.GetLeaveTaken(3, EmployeeFilter, date_from, date_upto);
            double sick_leave_t = _leaveServices.GetLeaveTaken(5, EmployeeFilter, date_from, date_upto);
            double other_leave_t = _leaveServices.GetLeaveTaken(9, EmployeeFilter, date_from, date_upto);
            double maternity_t = _leaveServices.GetLeaveTaken(12, EmployeeFilter, date_from, date_upto);
            double paternity_t = _leaveServices.GetLeaveTaken(13, EmployeeFilter, date_from, date_upto);
            double mourning_t = _leaveServices.GetLeaveTaken(14, EmployeeFilter, date_from, date_upto);
            double unpaid_study_t = _leaveServices.GetLeaveTaken(15, EmployeeFilter, date_from, date_upto);
            /*---------------------------------------------------------------------'
            '*CALCULATE LEAVES UP TO PROVIDED DATE RANGE*'
            '---------------------------------------------------------------------'*/
            double annual_leave_caf = 0;
            double sick_leave_caf = 0;
            double annual_leave = 0;
            double casual_leave = 0;
            double sick_leave = 0;
            double other_leave = 0;
            double maternity = 0;
            double paternity = 0;
            double mourning = 0;
            double unpaid_study = 0;
            double annual_leave_caf_t = 0;
            double sick_leave_caf_t = 0;
            double casual_leave_paid = 0;
            double other_leave_paid = 0;
            double maternity_paid = 0;
            double paternity_paid = 0;
            double mourning_paid = 0;
            double unpaid_study_paid = 0;
            double annual_leave_caf_laps = 0;
            double sick_leave_caf_laps = 0;
            double annual_leave_laps = 0;
            double sick_leave_laps = 0;

            annual_leave_caf = _leaveServices.GetMaxLeaveHours(16, EmployeeFilter, FiscalFilter);
            sick_leave_caf = _leaveServices.GetMaxLeaveHours(17, EmployeeFilter, FiscalFilter);
            annual_leave = _leaveServices.GetMaxLeaveHours(1, EmployeeFilter, FiscalFilter);
            casual_leave = _leaveServices.GetMaxLeaveHours(3, EmployeeFilter, FiscalFilter);
            sick_leave = _leaveServices.GetMaxLeaveHours(5, EmployeeFilter, FiscalFilter);
            other_leave = _leaveServices.GetMaxLeaveHours(9, EmployeeFilter, FiscalFilter);
            maternity = _leaveServices.GetMaxLeaveHours(12, EmployeeFilter, FiscalFilter);
            paternity = _leaveServices.GetMaxLeaveHours(13, EmployeeFilter, FiscalFilter);
            mourning = _leaveServices.GetMaxLeaveHours(14, EmployeeFilter, FiscalFilter);
            unpaid_study = _leaveServices.GetMaxLeaveHours(15, EmployeeFilter, FiscalFilter);

            if (annual_leave_t > annual_leave)
            {
                annual_leave_caf_t = annual_leave_t - annual_leave;
                annual_leave_t = annual_leave;
            }
            if (sick_leave_t > sick_leave)
            {
                sick_leave_caf_t = sick_leave_t - sick_leave;
                sick_leave_t = sick_leave;
            }

            double annual_leave_caf_paid = annual_leave_caf - annual_leave_caf_t;
            double sick_leave_caf_paid = sick_leave_caf - sick_leave_caf_t;
            double annual_leave_paid = annual_leave - annual_leave_t;
            double sick_leave_paid = sick_leave - sick_leave_t;

            double casual_leave_laps = casual_leave - casual_leave_t;
            double other_leave_laps = other_leave - other_leave_t;
            double maternity_laps = maternity - maternity_t;
            double paternity_laps = paternity - paternity_t;
            double mourning_laps = mourning - mourning_t;
            double unpaid_study_laps = unpaid_study - unpaid_study_t;

            double annual_leave_caf_n = annual_leave_caf - annual_leave_caf_t - annual_leave_caf_paid - annual_leave_caf_laps;
            double sick_leave_caf_n = sick_leave_caf - sick_leave_caf_t - sick_leave_caf_paid - sick_leave_caf_laps;
            double annual_leave_n = Math.Round(annual_leave - annual_leave_t - annual_leave_paid - annual_leave_laps, 2);
            double sick_leave_n = Math.Round(sick_leave - sick_leave_t - sick_leave_paid - sick_leave_laps, 2);
            double casual_leave_n = Math.Round(casual_leave - casual_leave_t - casual_leave_paid - casual_leave_laps, 2);
            double maternity_n = Math.Round(maternity - maternity_t - maternity_paid - maternity_laps, 2);
            double paternity_n = Math.Round(paternity - paternity_t - paternity_paid - paternity_laps, 2);
            double mourning_n = Math.Round(mourning - mourning_t - mourning_paid - mourning_laps, 2);
            double unpaid_study_n = Math.Round(unpaid_study - unpaid_study_t - unpaid_study_paid - unpaid_study_laps, 2);
            double other_leave_n = Math.Round(other_leave - other_leave_t - other_leave_paid - other_leave_laps, 2);

            var data = new LeavePaidClearedViewModelNew
            {
                indv_leave_id = 0,
                emp_id = EmployeeFilter,
                fiscal_year = FiscalFilter,
                annual_leave_caf = annual_leave_caf,
                annual_leave_caf_t = annual_leave_caf_t,
                annual_leave_caf_paid = 0,
                annual_leave_caf_laps = 0,
                annual_leave_caf_n = annual_leave_caf - annual_leave_caf_t - 0 - annual_leave_caf_laps,

                annual_leave = annual_leave,
                annual_leave_t = annual_leave_t,
                annual_leave_paid = 0,
                annual_leave_laps = 0,
                annual_leave_n = annual_leave - annual_leave_t,

                sick_leave_caf = sick_leave_caf,
                sick_leave_caf_t = sick_leave_caf_t,
                sick_leave_caf_paid = 0,
                sick_leave_caf_laps = 0,
                sick_leave_caf_n = sick_leave_caf - sick_leave_caf_t,

                sick_leave = sick_leave,
                sick_leave_t = sick_leave_t,
                sick_leave_paid = 0,
                sick_leave_laps = 0,
                sick_leave_n = sick_leave - sick_leave_t,

                casual_leave = casual_leave,
                casual_leave_t = casual_leave_t,
                casual_leave_paid = 0,
                casual_leave_laps = 0,
                casual_leave_n = casual_leave - casual_leave_t,

                other_leave = other_leave,
                other_leave_t = other_leave_t,
                other_leave_paid = 0,
                other_leave_laps = 0,
                other_leave_n = other_leave - other_leave_t,

                maternity = maternity,
                maternity_t = maternity_t,
                maternity_paid = 0,
                maternity_laps = 0,
                maternity_n = maternity - maternity_t,

                paternity = paternity,
                paternity_t = paternity_t,
                paternity_paid = 0,
                paternity_laps = 0,
                paternity_n = paternity - paternity_t,

                mourning = mourning,
                mourning_t = mourning_t,
                mourning_paid = 0,
                mourning_laps = 0,
                mourning_n = mourning - mourning_t,

                unpaid_study = unpaid_study,
                unpaid_study_t = unpaid_study_t,
                unpaid_study_paid = 0,
                unpaid_study_laps = 0,
                unpaid_study_n = unpaid_study - unpaid_study_t,

                date_from = date_from,
                date_upto = date_upto,
                submit_counter = PeriodFilter,
                remarks = "",
                calculate = calculate,
                chkSave = true
            };

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
            return PartialView("Settings/_LeavePaidClearedData", data);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LeavePaidClearedSave(LeavePaidClearedViewModelNew model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10556", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            int emp_id = model.emp_id;
            int submit_counter = model.submit_counter ?? 1;
            DateTime date_from = Convert.ToDateTime(model.date_from);
            DateTime date_upto = Convert.ToDateTime(model.date_upto);
            string fiscal_year = model.fiscal_year ?? HttpContext.Session.GetString("fiscal_year");

            if (emp_id < 1 || string.IsNullOrWhiteSpace(date_from.ToString()) || string.IsNullOrWhiteSpace(date_upto.ToString()) )
            {
                return Json(new { status = "false", message = Lang.msg_error_invalid });
            }
            /**Check the date input is within the fiscal year*/
            string checkWithin = _settingsServices.CheckDateWithinFiscalYear(date_from, fiscal_year);
            if (!string.IsNullOrWhiteSpace(checkWithin)) { return Json(new { status = "false", message = checkWithin }); }
            checkWithin = _settingsServices.CheckDateWithinFiscalYear(date_upto, fiscal_year);
            if (!string.IsNullOrWhiteSpace(checkWithin)) { return Json(new { status = "false", message = checkWithin }); }
            if (date_from > date_upto) { return Json(new { status = "false", message = "Start Date is greater than End Date." }); }

            if (mode == "add")
            {
                var isData = _context.tbl_employee_leave_indv_paid_cleared_new.
                    FirstOrDefault(l => l.emp_id == emp_id && l.fiscal_year == fiscal_year && l.submit_counter == submit_counter);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                int indv_leave_id = _context.tbl_employee_leave_indv_paid_cleared_new.Select(o => o.indv_leave_id).DefaultIfEmpty().Max() + 1;
                var DataSave = new tbl_employee_leave_indv_paid_cleared_new
                {
                    indv_leave_id = indv_leave_id,
                    emp_id  = model.emp_id,
                    fiscal_year = fiscal_year,
                    annual_leave_caf = model.annual_leave_caf,
                    sick_leave_caf = model.sick_leave_caf,
                    annual_leave = model.annual_leave,
                    casual_leave = model.casual_leave,
                    sick_leave = model.sick_leave,
                    other_leave = model.other_leave,
                    maternity = model.maternity,
                    paternity = model.paternity,
                    mourning = model.mourning,
                    unpaid_study = model.unpaid_study,
                    annual_leave_caf_paid = model.annual_leave_caf_paid,
                    sick_leave_caf_paid = model.sick_leave_caf_paid,
                    annual_leave_paid = model.annual_leave_paid,
                    casual_leave_paid = model.casual_leave_paid,
                    sick_leave_paid = model.sick_leave_paid,
                    other_leave_paid = model.other_leave_paid,
                    maternity_paid = model.maternity_paid,
                    paternity_paid = model.paternity_paid,
                    mourning_paid = model.mourning_paid,
                    unpaid_study_paid = model.unpaid_study_paid,
                    annual_leave_caf_laps = model.annual_leave_caf_laps,
                    sick_leave_caf_laps = model.sick_leave_caf_laps,
                    annual_leave_laps = model.annual_leave_laps,
                    casual_leave_laps = model.casual_leave_laps,
                    sick_leave_laps = model.sick_leave_laps,
                    other_leave_laps = model.other_leave_laps,
                    maternity_laps = model.maternity_laps,
                    paternity_laps = model.paternity_laps,
                    mourning_laps = model.mourning_laps,
                    unpaid_study_laps = model.unpaid_study_laps,
                    date_from = date_from,
                    date_upto = date_upto,
                    submit_counter = submit_counter,
                    remarks = model.remarks
                };
                _ = _context.tbl_employee_leave_indv_paid_cleared_new.Add(DataSave);

                if (model.chkSave)
                {
                    var DataUpdate = _context.tbl_employee_leave_indv.FirstOrDefault(l => l.fiscal_year_to == fiscal_year && l.emp_id == emp_id);
                    if (DataUpdate != null)
                    {
                        DataUpdate.annual_leave = model.annual_leave_n;
                        DataUpdate.casual_leave = model.casual_leave_n;
                        DataUpdate.sick_leave = model.sick_leave_n;
                        DataUpdate.annual_leave_hours_carry_forward = model.annual_leave_caf_n;
                        DataUpdate.maternity = model.maternity_n;
                        DataUpdate.paternity = model.paternity_n;
                        DataUpdate.mourning = model.mourning_n;
                        DataUpdate.unpaid_study = model.unpaid_study_n;
                        DataUpdate.other_leave = model.other_leave_n;
                        DataUpdate.sick_leave_hours_carry_forward = model.sick_leave_caf_n;
                        _ = _context.tbl_employee_leave_indv.Update(DataUpdate);
                    }
                    else
                    {
                        int _indv_leave_id = _context.tbl_employee_leave_indv.Select(o => o.indv_leave_id).DefaultIfEmpty().Max() + 1;
                        var DataSaveIndv = new tbl_employee_leave_indv
                        {
                            indv_leave_id = _indv_leave_id,
                            emp_id = emp_id,
                            annual_leave = model.annual_leave_n,
                            casual_leave = model.casual_leave_n,
                            sick_leave = model.sick_leave_n,
                            annual_leave_hours_carry_forward = model.annual_leave_caf_n,
                            maternity = model.maternity_n,
                            paternity = model.paternity_n,
                            mourning = model.mourning_n,
                            unpaid_study = model.unpaid_study_n,
                            fiscal_year_to = fiscal_year,
                            other_leave = model.other_leave_n,
                            sick_leave_hours_carry_forward = model.sick_leave_caf_n
                        };
                        _ = _context.tbl_employee_leave_indv.Add(DataSaveIndv);
                    }
                }
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            return Json(new { status = "invalid", message = Lang.msg_error_invalid });
        }
        #endregion
        /********************************************************************************************************************/
        #region EXCESS LEAVE ENCASHMENT
        [HttpGet]
        public IActionResult ExcessLeaveEncashment()
        {
            #region FOR PERMISSION
            string PageId = "10565";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            string UnitFilter = _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            string FiscalFilter = HttpContext.Session.GetString("fiscal_year") ?? "";
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalFilter, "normal_working_hrs"));

            string EmpStatusFilter = "A";
            string StatusFilter = "Pending";

            var Records = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_leave_indv_cafw_paid_laps
                on emp.emp_id equals jlo.emp_id
                where emp.emp_id != 0 && emp.emp_status == EmpStatusFilter && jlo.fiscal_year == FiscalFilter
                && ((StatusFilter == "Pending" && jlo.paid_month == null) || (StatusFilter != "Pending" && jlo.paid_month != null))
                orderby emp.firstname, emp.middlename, emp.lastname
                select new LeaveExcessEncashViewModel
                {
                    indv_leave_id = jlo.indv_leave_id,
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    gender = emp.gender == "M" ? "Male" : "Female",
                    unit = UnitFilter,
                    emp_status = emp.emp_status,
                    fiscal_year = jlo.fiscal_year,
                    cur_annual_leave_laps = Math.Round((double)jlo.cur_annual_leave_laps / workingHoursDays, 2),
                    cur_sick_leave_laps = Math.Round((double)jlo.cur_sick_leave_laps / workingHoursDays, 2),
                    cur_leave_laps = Math.Round(((double)jlo.cur_annual_leave_laps + (double)jlo.cur_sick_leave_laps) / workingHoursDays, 2),
                    tot_annual_leave_paid = Math.Round((double)jlo.tot_annual_leave_paid / workingHoursDays, 2),
                    tot_sick_leave_paid = Math.Round((double)jlo.tot_sick_leave_paid / workingHoursDays, 2),
                    tot_leave_paid = Math.Round(((double)jlo.tot_annual_leave_paid + (double)jlo.tot_sick_leave_paid) / workingHoursDays, 2),
                    salary = (StatusFilter == "Pending") ? emp.salary : (decimal)jlo.bacic_salary,
                    tot_annual_leave_amt = Math.Round((double)jlo.tot_annual_leave_amt, 2),
                    tot_sick_leave_amt = Math.Round((double)jlo.tot_sick_leave_amt, 2),
                    tot_leave_amt = Math.Round((double)jlo.tot_annual_leave_amt + (double)jlo.tot_sick_leave_amt, 2),
                    paid_month = jlo.paid_month,
                    paid_year = jlo.paid_year,
                    sumbit_counter = jlo.sumbit_counter,
                    max_annual_leave_cafw = Math.Round((double)jlo.max_annual_leave_cafw / workingHoursDays, 2),
                    max_sick_leave_cafw = Math.Round((double)jlo.max_sick_leave_cafw / workingHoursDays, 2)
                }
            ).ToList();
            ViewBag.PaidMonth = _settingsServices.GetMonths(8);
            ViewBag.PaidYear = _settingsServices.GetPaidYears();
            ViewBag.EmpStatusFilter = StatusActivePassive("AD",EmpStatusFilter);
            ViewBag.StatusFilter = ApprovalStatus(StatusFilter);
            ViewBag.FiscalFilter = _settingsServices.GetFiscalYears(FiscalFilter);
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_ExcessLeaveEncashment", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcessLeaveEncashmentList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string FiscalFilter = request.FilterValue2;
            string StatusFilter = request.FilterValue3;
            string UnitFilter = _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalFilter, "normal_working_hrs"));
            string EmpStatusFilter = request.FilterValue1;

            var query = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_leave_indv_cafw_paid_laps
                on emp.emp_id equals jlo.emp_id
                where emp.emp_id != 0 && emp.emp_status == EmpStatusFilter && jlo.fiscal_year == FiscalFilter
                && ((StatusFilter == "Pending" && jlo.paid_month == null) || (StatusFilter != "Pending" && jlo.paid_month != null))
                orderby emp.firstname, emp.middlename, emp.lastname
                select new LeaveExcessEncashViewModel
                {
                    indv_leave_id = jlo.indv_leave_id,
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    gender = emp.gender == "M" ? "Male" : "Female",
                    unit = UnitFilter,
                    emp_status = emp.emp_status,
                    fiscal_year = jlo.fiscal_year,
                    max_annual_leave_cafw = Math.Round((double)jlo.max_annual_leave_cafw / workingHoursDays, 2),
                    tot_annual_leave_paid = Math.Round((double)jlo.tot_annual_leave_paid / workingHoursDays, 2),
                    cur_annual_leave_laps = Math.Round((double)jlo.cur_annual_leave_laps / workingHoursDays, 2),
                    max_sick_leave_cafw = Math.Round((double)jlo.max_sick_leave_cafw / workingHoursDays, 2),
                    tot_sick_leave_paid = Math.Round((double)jlo.tot_sick_leave_paid / workingHoursDays, 2),
                    cur_sick_leave_laps = Math.Round((double)jlo.cur_sick_leave_laps / workingHoursDays, 2),
                    cur_leave_laps = Math.Round(((double)jlo.cur_annual_leave_laps + (double)jlo.cur_sick_leave_laps) / workingHoursDays, 2),
                    tot_leave_paid = Math.Round(((double)jlo.tot_annual_leave_paid + (double)jlo.tot_sick_leave_paid) / workingHoursDays, 2),
                    sumbit_counter = jlo.sumbit_counter,
                    salary = (StatusFilter == "Pending") ? emp.salary : (decimal)jlo.bacic_salary,
                    tot_annual_leave_amt = Math.Round((double)jlo.tot_annual_leave_amt, 2),
                    tot_sick_leave_amt = Math.Round((double)jlo.tot_sick_leave_amt, 2),
                    tot_leave_amt = Math.Round((double)jlo.tot_annual_leave_amt + (double)jlo.tot_sick_leave_amt, 2),
                    paid_month = jlo.paid_month,
                    paid_year = jlo.paid_year
                }
            );
            if (!string.IsNullOrEmpty(FiscalFilter))
            {
                query = query.Where(jlo => jlo.fiscal_year == FiscalFilter);/*filter*/
            }
            else
            {
                query = query.Where(jlo => jlo.fiscal_year == HttpContext.Session.GetString("fiscal_year"));/*filter*/
            }
            if (!string.IsNullOrEmpty(EmpStatusFilter))
            {
                query = query.Where(emp => emp.emp_status == EmpStatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
            }
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                if (StatusFilter == "Pending")
                {
                    query = query.Where(jlo => jlo.paid_month == null);/*filter*/
                }
                else
                {
                    query = query.Where(jlo => jlo.paid_month != null);/*filter*/
                }
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
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
        public JsonResult ExcessLeaveEncashmentSave([FromBody] LeaveExcessEncashListViewModel model)
        {
            string PageId = "10565";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any())
            {
                return Json(new { status = false, message = "No record received." });
            }
            //if (model.mode == "updateDataNoChk") { return Json(new { status = false, message = "Invalid process." }); }
            string fiscalYear = HttpContext.Session.GetString("fiscal_year");
            double workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs"));
            double workingHrsPayPeriod = Convert.ToDouble(_settingsServices.GetHourSettings(fiscalYear, "working_hrs_pay_period"));

            foreach (var leaves in model.Fields)
            {
                var existing = _context.tbl_employee_leave_indv_cafw_paid_laps.FirstOrDefault(e => e.indv_leave_id == leaves.indv_leave_id);
                if (existing != null)
                {
                    int emp_id = existing.emp_id ?? 0;
                    var getSal = _context.tbl_employee.Where(s => s.emp_id == emp_id).Select(s => s.salary).FirstOrDefault();
                    double bacic_salary = getSal != null ? Convert.ToDouble(getSal) : 0;

                    double tot_annual_leave_paid = (double)existing.tot_annual_leave_paid;
                    double tot_sick_leave_paid = (double)existing.tot_sick_leave_paid;

                    double tot_annual_leave_amt = Math.Round(bacic_salary * tot_annual_leave_paid / (workingHrsPayPeriod / workingHoursDays), 0);
                    double tot_sick_leave_amt = Math.Round(bacic_salary * tot_sick_leave_paid / (workingHrsPayPeriod / workingHoursDays), 0);

                    existing.bacic_salary = bacic_salary;
                    existing.tot_annual_leave_amt = tot_annual_leave_amt;
                    existing.tot_sick_leave_amt = tot_sick_leave_amt;
                    existing.paid_month = leaves.paid_month;
                    existing.paid_year = leaves.paid_year;

                    _ = _context.tbl_employee_leave_indv_cafw_paid_laps.Update(existing);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region LEAVE SETTING
        [HttpGet]
        public IActionResult LeaveSetting()
        {
            #region FOR PERMISSION
            string PageId = "10508";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            string UnitFilter = _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            double workingHoursDays = 1;
            string FiscalFilter = HttpContext.Session.GetString("fiscal_year");
            if (UnitFilter == "days") { workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalFilter, "normal_working_hrs")); }
            string StatusFilter = "A";

            var Records = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_leave_indv
                on emp.emp_id equals jlo.emp_id
                where emp.emp_id != 0 && emp.emp_status == StatusFilter && jlo.fiscal_year_to == FiscalFilter
                orderby emp.firstname, emp.middlename, emp.lastname
                select new LeaveSettingViewModel
                {
                    indv_leave_id = jlo.indv_leave_id,
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    gender = emp.gender == "M" ? "Male" : "Female",
                    unit = UnitFilter,
                    emp_status = emp.emp_status,
                    fiscal_year_to = jlo.fiscal_year_to,
                    annual_leave_hours_carry_forward = Math.Round((double)jlo.annual_leave_hours_carry_forward / workingHoursDays, 2),
                    annual_leave = Math.Round((double)jlo.annual_leave / workingHoursDays, 2),
                    sick_leave_hours_carry_forward = Math.Round((double)jlo.sick_leave_hours_carry_forward / workingHoursDays, 2),
                    sick_leave = Math.Round((double)jlo.sick_leave / workingHoursDays, 2),
                    casual_leave = Math.Round((double)jlo.casual_leave / workingHoursDays, 2),
                    other_leave = Math.Round((double)jlo.other_leave / workingHoursDays, 2),
                    maternity = Math.Round((double)jlo.maternity / workingHoursDays, 2),
                    paternity = Math.Round((double)jlo.paternity / workingHoursDays, 2),
                    mourning = Math.Round((double)jlo.mourning / workingHoursDays, 2),
                    unpaid_study = Math.Round((double)jlo.unpaid_study / workingHoursDays, 2)
                }
            ).ToList();
            ViewBag.UnitFilter = GetLeaveUnit(UnitFilter);
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.FiscalFilter = _settingsServices.GetFiscalYears(FiscalFilter);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/LeaveSetting", "ADD", PageId, Records.Count);
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_LeaveSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveSettingList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string UnitFilter = !string.IsNullOrWhiteSpace(request.FilterValue1) ? request.FilterValue1 : _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            double workingHoursDays = 1;
            string FiscalFilter = request.FilterValue2;
            string StatusFilter = request.FilterValue3;
            if (UnitFilter == "days") { workingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(FiscalFilter, "normal_working_hrs")); }
            var query =
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_leave_indv
                on emp.emp_id equals jlo.emp_id
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new LeaveSettingViewModel
                {
                    indv_leave_id = jlo.indv_leave_id,
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    gender = emp.gender == "M" ? "Male" : "Female",
                    unit = FiscalFilter,
                    emp_status = emp.emp_status,
                    fiscal_year_to = jlo.fiscal_year_to,
                    annual_leave_hours_carry_forward = Math.Round((double)jlo.annual_leave_hours_carry_forward / workingHoursDays, 2),
                    annual_leave = Math.Round((double)jlo.annual_leave / workingHoursDays, 2),
                    sick_leave_hours_carry_forward = Math.Round((double)jlo.sick_leave_hours_carry_forward / workingHoursDays, 2),
                    sick_leave = Math.Round((double)jlo.sick_leave / workingHoursDays, 2),
                    casual_leave = Math.Round((double)jlo.casual_leave / workingHoursDays, 2),
                    other_leave = Math.Round((double)jlo.other_leave / workingHoursDays, 2),
                    maternity = Math.Round((double)jlo.maternity / workingHoursDays, 2),
                    paternity = Math.Round((double)jlo.paternity / workingHoursDays, 2),
                    mourning = Math.Round((double)jlo.mourning / workingHoursDays, 2),
                    unpaid_study = Math.Round((double)jlo.unpaid_study / workingHoursDays, 2)
                };
            if (!string.IsNullOrEmpty(FiscalFilter))
            {
                query = query.Where(jlo => jlo.fiscal_year_to == FiscalFilter);/*filter*/
            }
            else
            {
                query = query.Where(jlo => jlo.fiscal_year_to == HttpContext.Session.GetString("fiscal_year"));/*filter*/
            }
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(emp => emp.emp_status == StatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
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

        public IActionResult LeaveSettingAddEdit(string id, string mode)
        {
            string PageId = "10508";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            string Unit = _globalOptionServices.OptionServices["op_gs_default_leave_format"];
            string FiscalYear = HttpContext.Session.GetString("fiscal_year");
            ViewBag.mode = mode;
            /**this is to load blank form while doing add process */
            if (mode == "add")
            {
                var model = new LeaveSettingViewModel
                {
                    indv_leave_id = 0,
                    emp_id = 0,
                    employee = "",
                    gender = "",
                    unit = Unit,
                    emp_status = "A",
                    fiscal_year_to = FiscalYear,
                    annual_leave_hours_carry_forward = 0,
                    annual_leave = 0,
                    sick_leave_hours_carry_forward = 0,
                    sick_leave = 0,
                    casual_leave = 0,
                    other_leave = 0,
                    maternity = 0,
                    paternity = 0,
                    mourning = 0,
                    unpaid_study = 0
                };
                ViewBag.EmployeeList = _leaveServices.GetEmployeeNotHavingLeaveSetting(FiscalYear);
                ViewBag.LeaveUnit = ToProperCase(Unit);
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_LeaveSettingAddEdit", model);
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LeaveSettingSave(LeaveSettingViewModel model)
        {

            _ = ModelState.Remove("indv_leave_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10508", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (mode == "add")
            {
                var isData = _context.tbl_employee_leave_indv.FirstOrDefault(u => u.fiscal_year_to == model.fiscal_year_to && u.emp_id == model.emp_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                //getMaxId()
                int indv_leave_id = _context.tbl_employee_leave_indv.Select(o => o.indv_leave_id).DefaultIfEmpty().Max() + 1;
                var DataSave = new tbl_employee_leave_indv
                {
                    indv_leave_id = indv_leave_id,
                    emp_id = model.emp_id,
                    fiscal_year_to = model.fiscal_year_to,
                    annual_leave_hours_carry_forward = model.annual_leave_hours_carry_forward,
                    annual_leave = model.annual_leave,
                    sick_leave_hours_carry_forward = model.sick_leave_hours_carry_forward,
                    sick_leave = model.sick_leave,
                    casual_leave = model.casual_leave,
                    other_leave = model.other_leave,
                    maternity = model.maternity,
                    paternity = model.paternity,
                    mourning = model.mourning,
                    unpaid_study = model.unpaid_study
                };
                _ = _context.tbl_employee_leave_indv.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LeaveSettingListSave([FromBody] LeaveSettingListViewModel model)
        {
            string PageId = "10508";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any())
            {
                return Json(new { status = false, message = "No record received." });
            }
            string fiscalYear = HttpContext.Session.GetString("fiscal_year");
            double defaultWorkingHoursDays = Convert.ToDouble(_settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs"));
            foreach (var leaves in model.Fields)
            {
                double workingHoursDays = 1;
                if (leaves.unit == "days") { workingHoursDays = defaultWorkingHoursDays; }

                var existing = _context.tbl_employee_leave_indv.FirstOrDefault(e => e.indv_leave_id == leaves.indv_leave_id);
                if (existing != null)
                {
                    existing.annual_leave_hours_carry_forward = Math.Round((double)leaves.annual_leave_hours_carry_forward / workingHoursDays, 2);
                    existing.annual_leave = Math.Round((double)leaves.annual_leave / workingHoursDays, 2);
                    existing.sick_leave_hours_carry_forward = Math.Round((double)leaves.sick_leave_hours_carry_forward / workingHoursDays, 2);
                    existing.sick_leave = Math.Round((double)leaves.sick_leave / workingHoursDays, 2);
                    existing.casual_leave = Math.Round((double)leaves.casual_leave / workingHoursDays, 2);
                    existing.other_leave = Math.Round((double)leaves.other_leave / workingHoursDays, 2);
                    existing.maternity = Math.Round((double)leaves.maternity / workingHoursDays, 2);
                    existing.paternity = Math.Round((double)leaves.paternity / workingHoursDays, 2);
                    existing.mourning = Math.Round((double)leaves.mourning / workingHoursDays, 2);
                    existing.unpaid_study = Math.Round((double)leaves.unpaid_study / workingHoursDays, 2);

                    _ = _context.tbl_employee_leave_indv.Update(existing);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region LEAVE CARRY FORWARD
        [HttpGet]
        public IActionResult LeaveCarryForward()
        {
            string PageId = "10507";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            int CanSubmit = 1;//yes

            string fiscalYearFrom = "N/A";
            string fiscalYearTo = "N/A";
            string fiscalYearFromAbbr = "N/A";
            fiscalYearTo = HttpContext.Session.GetString("fiscal_year") ?? "";
            bool IsCarryForwarded = false;
            bool IsAnyPendingLeave = false;
            if (!string.IsNullOrWhiteSpace(fiscalYearTo))
            {
                var parts = fiscalYearTo.Split('/');
                int fromYear = int.Parse(parts[0]) - 1;
                int toYear = int.Parse(parts[1]) - 1;
                fiscalYearFrom = $"{fromYear}/{toYear}";
                var smt = _context.tbl_fiscal_year.FirstOrDefault(lvi => lvi.fiscal_year == fiscalYearFrom);
                if (smt != null)
                {
                    fiscalYearFromAbbr = smt.fiscal_year_abb ?? "";
                }
                var sql = _context.tbl_employee_leave_indv.FirstOrDefault(lvi => lvi.fiscal_year_to == fiscalYearTo);
                if (sql != null)
                {
                    IsCarryForwarded = true;
                    CanSubmit = 0; //no
                }
            }
            DateTime startDate = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(fiscalYearFrom, "date_from"));
            DateTime endDate = Convert.ToDateTime(_settingsServices.GetFiscalYearValue(fiscalYearFrom, "date_to"));
            string IsPendingLeaveOverall = _leaveServices.AnyPendingLeaveOverallFY(startDate, endDate);
            if (!string.IsNullOrWhiteSpace(IsPendingLeaveOverall) && IsPendingLeaveOverall == "Y")
            {
                IsAnyPendingLeave = true;
                CanSubmit = 0; //no 
            }
            var Records = new LeaveCarryForwardViewModel
            {
                Mode = "submit",
                IsCarryForwarded = IsCarryForwarded,
                IsAnyPendingLeave = IsAnyPendingLeave,
                FyCurAbbr = HttpContext.Session.GetString("fiscal_year_abb") ?? "N/A",
                FyFrom = fiscalYearFrom,
                FyFromAbbr = fiscalYearFromAbbr,
                FyTo = fiscalYearTo,
                FyToAbbr = HttpContext.Session.GetString("fiscal_year_abb") ?? "N/A",
            };

            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/LeaveCarryForward", "CarryForward", PageId, CanSubmit);
            return PartialView("Settings/_LeaveCarryForward", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult LeaveCarryForwardSave(LeaveCarryForwardViewModel model)
        {
            _ = ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            if (!_accountServices.HasPermission("10507", "edit")) { return Json(new { status = "invalid", message = Lang.msg_permission_denied }); }
            if (model == null) { return Json(new { status = "invalid", message = Lang.msg_error }); }

            string mode = model.Mode;
            string fiscal_year_from = model.FyFrom;
            string fiscal_year_to = model.FyTo;
            var sql = _context.tbl_employee_leave_indv.FirstOrDefault(lvi => lvi.fiscal_year_to == fiscal_year_to);
            if (sql != null)
            {
                return Json(new { status = "invalid", message = "Leave has already been carried forward into the current fiscal year." });
            }

            if (mode == "submit")
            {
                string str_max_cur_an_leave_cf = _globalOptionServices.OptionServices["op_max_cur_an_leave_cf"];
                string str_max_cur_si_leave_cf = _globalOptionServices.OptionServices["op_max_cur_si_leave_cf"];
                string str_hrs_can_carry_forward = _globalOptionServices.OptionServices["op_max_an_leave_cf_accum"];
                string str_sick_hrs_can_carry_forward = _globalOptionServices.OptionServices["op_max_si_leave_cf_accum"];

                double max_cur_an_leave_cf = double.TryParse(str_max_cur_an_leave_cf, out double V1) ? V1 : 0;
                double max_cur_si_leave_cf = double.TryParse(str_max_cur_si_leave_cf, out double V2) ? V2 : 0;
                double hrs_can_carry_forward = double.TryParse(str_hrs_can_carry_forward, out double V3) ? V3 : 0;
                double sick_hrs_can_carry_forward = double.TryParse(str_sick_hrs_can_carry_forward, out double V4) ? V4 : 0;

                string? str_fiscal_year_start = _settingsServices.GetFiscalYearValue(fiscal_year_from, "date_from");
                string? str_fiscal_year_end = _settingsServices.GetFiscalYearValue(fiscal_year_from, "date_to");
                string? str_fiscal_year_to_start = _settingsServices.GetFiscalYearValue(fiscal_year_to, "date_from");
                string? str_fiscal_year_to_end = _settingsServices.GetFiscalYearValue(fiscal_year_to, "date_to");

                var fiscal_year_start = DateTime.TryParse(str_fiscal_year_start, out var D1) ? D1 : DateTime.Now;
                var fiscal_year_end = DateTime.TryParse(str_fiscal_year_end, out var D2) ? D2 : DateTime.Now;
                var fiscal_year_to_start = DateTime.TryParse(str_fiscal_year_to_start, out var D3) ? D3 : DateTime.Now;
                var fiscal_year_to_end = DateTime.TryParse(str_fiscal_year_to_end, out var D4) ? D4 : DateTime.Now;

                var Emps = _context.tbl_employee
                    .Where(emp => emp.emp_status == "A")
                    .OrderBy(emp => emp.firstname)
                    .ThenBy(emp => emp.middlename)
                    .ThenBy(emp => emp.lastname)
                    .Select(emp => new
                    {
                        emp.emp_id,
                        emp.gender
                    }).ToList();

                foreach (var emp in Emps)
                {
                    int emp_id = emp.emp_id;
                    string gender = emp.gender ?? "";
                    var new_start_fiscal_date = _leaveServices.GetFirstLeavePaidEndDate(emp_id, fiscal_year_from, Convert.ToDateTime(fiscal_year_start), 1);
                    /**
                     * Annual Leave | current 3/4  | fiscal year from 2/3 to 3/4
                     */
                    double a_c = _leaveServices.GetMaxLeaveHours(1, emp_id, fiscal_year_from);  /** annual leave for 2/3 */
                    double a_p = _leaveServices.GetMaxLeaveHours(16, emp_id, fiscal_year_from); /** forwarded to 2/3 */
                    double a_t = _leaveServices.GetLeaveTaken(1, emp_id, new_start_fiscal_date, Convert.ToDateTime(fiscal_year_end));    /** total leave taken for 2/3 fiscal year */
                    double cur_an_leave_laps = a_t >= a_c ? 0 : (a_c - a_t) >= max_cur_an_leave_cf ? a_c - a_t - max_cur_an_leave_cf : 0;
                    double a_n = a_p + a_c - a_t - cur_an_leave_laps;      /** Grand total annual accumulated **/
                    double carry_forward = a_n;
                    double tot_an_leave_paid = 0;
                    if (a_n > hrs_can_carry_forward)
                    {
                        carry_forward = hrs_can_carry_forward;
                        tot_an_leave_paid = a_n - hrs_can_carry_forward;
                    }
                    /**
                     * Sick Leave | current 3/4  | fiscal year from 2/3 to 3/4
                     */
                    double s_c = _leaveServices.GetMaxLeaveHours(5, emp_id, fiscal_year_from);  /** sick leave for 2/3*/
                    double s_p = _leaveServices.GetMaxLeaveHours(17, emp_id, fiscal_year_from);                  /** forwarded to 2/3 */
                    double s_t = _leaveServices.GetLeaveTaken(5, emp_id, new_start_fiscal_date, Convert.ToDateTime(fiscal_year_end)); /** total leave taken for 2/3 fiscal year */
                    double cur_si_leave_laps = (s_t >= s_c) ? 0 : (s_c - s_t >= max_cur_si_leave_cf) ? (s_c - s_t - max_cur_si_leave_cf) : 0;
                    double s_n = s_p + s_c - s_t - cur_si_leave_laps;             /** Grand total annual accumulated */
                    double carry_forward_sick = s_n;
                    double tot_si_leave_paid = 0;
                    if (s_n > sick_hrs_can_carry_forward)
                    {
                        carry_forward_sick = sick_hrs_can_carry_forward;
                        tot_si_leave_paid = s_n - sick_hrs_can_carry_forward;
                    }
                    /** Maximum yealry leave hours for all types of leave
                     * Get it form heading table
                     */

                    double annual_leave = _leaveServices.GetYearlyLeaveHours(1);
                    double casual_leave = _leaveServices.GetYearlyLeaveHours(3);
                    double sick_leave = _leaveServices.GetYearlyLeaveHours(5);
                    double other_leave = _leaveServices.GetYearlyLeaveHours(9);
                    double maternity = 0;
                    double paternity = 0;
                    double mourning = 0;
                    double unpaid_study = 0;
                    int indv_leave_id = _context.tbl_employee_leave_indv.Select(o => o.indv_leave_id).DefaultIfEmpty(0).Max() + 1;
                    var DataSave = new tbl_employee_leave_indv
                    {
                        indv_leave_id = indv_leave_id,
                        emp_id = emp_id,
                        annual_leave = annual_leave,
                        casual_leave = casual_leave,
                        sick_leave = sick_leave,
                        annual_leave_hours_carry_forward = carry_forward,
                        maternity = maternity,
                        paternity = paternity,
                        mourning = mourning,
                        unpaid_study = unpaid_study,
                        fiscal_year_to = fiscal_year_to,
                        other_leave = other_leave,
                        sick_leave_hours_carry_forward = carry_forward_sick
                    };
                    _ = _context.tbl_employee_leave_indv.Add(DataSave);

                    int indv_leave_cpl_id = _context.tbl_employee_leave_indv_cafw_paid_laps.Select(o => o.indv_leave_id).DefaultIfEmpty(0).Max() + 1;
                    var DataSaveLP = new tbl_employee_leave_indv_cafw_paid_laps
                    {
                        indv_leave_id = indv_leave_cpl_id,
                        emp_id = emp_id,
                        fiscal_year = fiscal_year_to,
                        max_annual_leave_cafw = max_cur_an_leave_cf,
                        tot_annual_leave_paid = tot_an_leave_paid,
                        cur_annual_leave_laps = cur_an_leave_laps,
                        max_sick_leave_cafw = max_cur_si_leave_cf,
                        tot_sick_leave_paid = tot_si_leave_paid,
                        cur_sick_leave_laps = cur_si_leave_laps,
                        sumbit_counter = 1
                    };
                    _ = _context.tbl_employee_leave_indv_cafw_paid_laps.Add(DataSaveLP);
                }//for loop end
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                //** TRANSFER FUTURE LEAVE TO MAIN TABLE**/
                DateTime calendar_year_to_start = fiscal_year_start;
                DateTime calendar_year_to_end = fiscal_year_end;
                _leaveServices.TransferFutureLeave(calendar_year_to_start, calendar_year_to_end);

                return Json(new { status = "success", message = "Leave Carry Forwarded Successfully" });
            }
            else
            {
                return Json(new { status = "false", message = "Fail. Error during carry forward process." });
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region GENERAL SETTINGS
        [HttpGet]
        public IActionResult GeneralSetting()
        {
            #region FOR PERMISSION
            string PageId = "10505";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            string fiscalYear = HttpContext.Session.GetString("fiscal_year");
            string work_hours = _settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs");
            double working_hrs_day = double.TryParse(work_hours, out double parsed) ? parsed : 1;

            var Records = (from tlh in _context.tbl_leave_heading
                           where tlh.max_leave_hours.HasValue && tlh.max_leave_hours > 0
                           orderby tlh.description ascending
                           select new GeneralSettingViewModel
                           {
                               id = tlh.leave_type_id,
                               description = tlh.description,
                               max_leave_hours = tlh.max_leave_hours,
                               max_leave_days = working_hrs_day > 0 ? Math.Round((double)tlh.max_leave_hours / working_hrs_day, 2) : 1
                           }).AsNoTracking().ToList();
            ViewBag.working_hrs_day = working_hrs_day;
            return PartialView("Settings/_GeneralSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneralSettingList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string fiscalYear = HttpContext.Session.GetString("fiscal_year");
            string work_hours = _settingsServices.GetHourSettings(fiscalYear, "normal_working_hrs");
            double working_hrs_day = double.TryParse(work_hours, out double parsed) ? parsed : 1;

            var query = from tlh in _context.tbl_leave_heading
                        where tlh.max_leave_hours.HasValue && tlh.max_leave_hours > 0
                        orderby tlh.description ascending
                        select new GeneralSettingViewModel
                        {
                            id = tlh.leave_type_id,
                            description = tlh.description,
                            max_leave_hours = tlh.max_leave_hours,
                            max_leave_days = working_hrs_day > 0 ? Math.Round((double)tlh.max_leave_hours / working_hrs_day, 2) : 1
                        };
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
        #endregion
        /********************************************************************************************************************/
        #region LANGUAGE SETTING
        [HttpGet]
        public IActionResult LanguageSetting()
        {
            #region FOR PERMISSION
            string PageId = "10526";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            return PartialView("Settings/_LanguageSetting");
        }
        #endregion
        /********************************************************************************************************************/
        #region PAY PERIOD
        [HttpGet]
        public IActionResult PayPeriod()
        {
            #region FOR PERMISSION
            string PageId = "10529";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION
            return PartialView("Settings/_PayPeriod");
        }
        #endregion
        /********************************************************************************************************************/
        #region MEDICAL/INSURANCE VERIFICATION
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult MedicalInsurance()
        {
            #region FOR PERMISSION
            string PageId = "10564";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from emr in _context.tbl_employee_medical_reimburse
                join emp in _context.tbl_employee
                on emr.emp_id equals emp.emp_id
                orderby emp.firstname descending, emp.middlename descending, emp.lastname descending
                select new EmployeeMedicalReimburseVerificationViewModel
                {
                    id = emr.id,
                    fiscal_year = emr.fiscal_year,
                    emp_id = Convert.ToInt32(emp.emp_id),
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')', '[' + emp.marital_status + ']' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    bill_no = emr.bill_no,
                    bill_date = Convert.ToDateTime(emr.bill_date),
                    self_amt = Convert.ToDouble(emr.self_amt),
                    spouse_amt = Convert.ToDouble(emr.spouse_amt),
                    other_dep_amt = Convert.ToDouble(emr.other_dep_amt),
                    total_amount = emr.self_amt.GetValueOrDefault() + emr.spouse_amt.GetValueOrDefault() + emr.other_dep_amt.GetValueOrDefault(),
                    submit_date = Convert.ToDateTime(emr.submit_date),
                    remarks = emr.remarks,
                    status = emr.app_status,
                    app_by = Convert.ToInt32(emr.app_by),
                    app_date = Convert.ToDateTime(emr.app_date),
                    sal_month = Convert.ToInt32(emr.sal_month),
                    sal_year = Convert.ToInt32(emr.sal_year),
                    reim_type = emr.reim_type
                }).ToList();
            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.ApprovalStatusFilter = ApprovalStatus();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD"); ;
            return PartialView("Settings/_MedicalInsurance", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedicalInsuranceList([FromForm] DataFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string? FiscalYearFilter = request.FiscalYearFilter;/*Dropdwon Filter*/
            string? ApprovalStatusFilter = request.ApprovalStatusFilter;/*Dropdwon Filter*/
            string? EmployeeStatusFilter = request.EmployeeStatusFilter;/*Dropdwon Filter*/
            string? EmployeeFilter = request.EmployeeFilter;/*Dropdwon Filter*/
            var query = from emr in _context.tbl_employee_medical_reimburse
                        join emp in _context.tbl_employee
                        on emr.emp_id equals emp.emp_id
                        where string.IsNullOrWhiteSpace(searchValue)
                || (emp.firstname != null && emp.firstname.Contains(searchValue))
                || (emp.middlename != null && emp.middlename.Contains(searchValue))
                || (emp.lastname != null && emp.lastname.Contains(searchValue))
                        orderby emp.firstname descending, emp.middlename descending, emp.lastname descending
                        select new EmployeeMedicalReimburseVerificationViewModel
                        {
                            id = emr.id,
                            fiscal_year = emr.fiscal_year,
                            emp_id = Convert.ToInt32(emp.emp_id),
                            employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')', emp.marital_status == "S" ? "[Single]" : "[Married]" }.Where(x => !string.IsNullOrEmpty(x))),
                            emp_status = emp.emp_status,
                            bill_no = emr.bill_no,
                            bill_date = Convert.ToDateTime(emr.bill_date),
                            self_amt = Convert.ToDouble(emr.self_amt),
                            spouse_amt = Convert.ToDouble(emr.spouse_amt),
                            other_dep_amt = Convert.ToDouble(emr.other_dep_amt),
                            total_amount = emr.self_amt.GetValueOrDefault() + emr.spouse_amt.GetValueOrDefault() + emr.other_dep_amt.GetValueOrDefault(),
                            submit_date = Convert.ToDateTime(emr.submit_date),
                            remarks = emr.remarks,
                            status = emr.app_status,
                            app_by = Convert.ToInt32(emr.app_by),
                            app_date = Convert.ToDateTime(emr.app_date),
                            sal_month = Convert.ToInt32(emr.sal_month),
                            sal_year = Convert.ToInt32(emr.sal_year),
                            reim_type = emr.reim_type
                        };
            if (!string.IsNullOrEmpty(FiscalYearFilter)) { query = query.Where(emr => emr.fiscal_year == FiscalYearFilter); }
            if (!string.IsNullOrEmpty(ApprovalStatusFilter)) { query = query.Where(emr => emr.status == ApprovalStatusFilter); }
            if (!string.IsNullOrEmpty(EmployeeStatusFilter)) { query = query.Where(emp => emp.emp_status == EmployeeStatusFilter); }

            var data = query.ToList().OrderBy(x => x.employee);
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
        public IActionResult MedicalInsuranceAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10564";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (
                        from emr in _context.tbl_employee_medical_reimburse
                        join emp in _context.tbl_employee
                        on emr.emp_id equals emp.emp_id
                        where emr.id == id
                        select new
                        {
                            emr.id,
                            emr.fiscal_year,
                            emp.emp_id,
                            emp.firstname,
                            emp.middlename,
                            emp.lastname,
                            emp.emp_code,
                            emp.marital_status,
                            emr.bill_no,
                            emr.bill_date,
                            emr.self_amt,
                            emr.spouse_amt,
                            emr.other_dep_amt,
                            total_amount = emr.self_amt.GetValueOrDefault() + emr.spouse_amt.GetValueOrDefault() + emr.other_dep_amt.GetValueOrDefault(),
                            emr.submit_date,
                            emr.remarks,
                            emr.app_status,
                            emr.app_by,
                            emr.app_date,
                            emr.sal_month,
                            emr.sal_year,
                            emr.reim_type
                        }).FirstOrDefault();
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new EmployeeMedicalReimburseVerificationViewModel
                        {
                            id = smt.id,
                            fiscal_year = smt.fiscal_year,
                            emp_id = Convert.ToInt32(smt.emp_id),
                            employee = string.Join(" ", new[] { smt.firstname, smt.middlename, smt.lastname, '(' + smt.emp_code + ')', smt.marital_status == "S" ? "[Single]" : "[Married]" }.Where(x => !string.IsNullOrEmpty(x))),
                            bill_no = smt.bill_no,
                            bill_date = Convert.ToDateTime(smt.bill_date),
                            self_amt = Convert.ToDouble(smt.self_amt),
                            spouse_amt = Convert.ToDouble(smt.spouse_amt),
                            other_dep_amt = Convert.ToDouble(smt.other_dep_amt),
                            total_amount = smt.total_amount,
                            submit_date = Convert.ToDateTime(smt.submit_date),
                            remarks = smt.remarks,
                            status = smt.app_status,
                            app_by = Convert.ToInt32(smt.app_by),
                            app_date = Convert.ToDateTime(smt.app_date),
                            sal_month = Convert.ToInt32(smt.sal_month),
                            sal_year = Convert.ToInt32(smt.sal_year),
                            reim_type = smt.reim_type
                        };
                        ViewBag.bill_date = Convert.ToDateTime(smt.bill_date).ToString(_appSettings.DATE_FORMAT);
                        ViewBag.submit_date = Convert.ToDateTime(smt.submit_date).ToString(_appSettings.DATE_FORMAT);

                        if (string.IsNullOrWhiteSpace(smt.app_status) || string.Equals(smt.app_status, "Pending", StringComparison.OrdinalIgnoreCase))
                        {
                            ViewBag.Years = _settingsServices.GetYears(0);
                            ViewBag.Months = _settingsServices.GetMonths();
                        }
                        else
                        {
                            int salmonth = Convert.ToInt32(smt.sal_month);
                            int appby = Convert.ToInt32(smt.app_by);
                            string AppDate = Convert.ToDateTime(smt.app_date).ToString(_appSettings.DATE_FORMAT);
                            ViewBag.SalMonth = MonthName(salmonth);
                            ViewBag.AppBy = _employeeServices.GetEmployeeName(appby);
                            ViewBag.AppDate = AppDate;
                        }
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_MedicalInsuranceAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MedicalInsuranceSave(EmployeeMedicalReimburseVerificationViewModel model)
        {
            _ = _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                string errors = DebugModelState(ModelState);
                return Json(new { status = "invalid", message = Lang.msg_error_invalid, details = errors });
            }
            string? mode = Request.Form["mode"];
            string? id = Request.Form["id"];

            if (!_accountServices.HasPermission("10564", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            string app_status = model.status ?? "";

            if (string.Equals(mode, "edit", StringComparison.OrdinalIgnoreCase) && (
                string.Equals(app_status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(app_status, "Declined", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(app_status, "Pending", StringComparison.OrdinalIgnoreCase)
                ))
            {
                var DataUpdate = _context.tbl_employee_medical_reimburse.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                int? sal_year = null;
                int? sal_month = null;
                DateTime? app_date = null;
                int? app_by = null;

                if (string.Equals(app_status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(app_status, "Declined", StringComparison.OrdinalIgnoreCase))
                {
                    app_date = DateTime.Now;
                    if (int.TryParse(model.sal_year?.ToString(), out int salyear))
                    {
                        sal_year = salyear;
                    }
                    if (int.TryParse(model.sal_year?.ToString(), out int salmonth))
                    {
                        sal_month = salmonth;
                    }
                    string? userId = HttpContext.Session.GetString("user_id");
                    if (int.TryParse(userId, out int appby))
                    {
                        app_by = appby;
                    }
                }
                DataUpdate.sal_year = sal_year;
                DataUpdate.sal_month = sal_month;
                DataUpdate.app_status = app_status;
                DataUpdate.app_by = app_by;
                DataUpdate.app_date = app_date;
                _ = _context.tbl_employee_medical_reimburse.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region OVERTIME EMPLOYEE SETTING
        [HttpGet]
        public IActionResult OvertimeEmployeeSetting()
        {
            #region FOR PERMISSION
            string PageId = "10553";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            //using LEFT OUTER JOIN
            var Records = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_overtime_settings
                on emp.emp_id equals jlo.emp_id into tblNewJoin
                from jlo in tblNewJoin.DefaultIfEmpty()
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new OvertimeEmployeeSettingViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    gender = emp.gender,
                    join_date = emp.join_date,
                    end_date = emp.end_date,
                    is_get_overtime = jlo.is_get_overtime,
                    approval_person = jlo.approval_person
                }
            ).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_OvertimeEmployeeSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OvertimeEmployeeSettingList([FromForm] CostumFilterRequest request)
        {
            try
            {
                var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
                string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
                var query = from emp in _context.tbl_employee
                            join jlo in _context.tbl_employee_overtime_settings
                            on emp.emp_id equals jlo.emp_id into tblNewJoin
                            from jlo in tblNewJoin.DefaultIfEmpty()
                            where emp.emp_id != 0
                            orderby emp.firstname, emp.middlename, emp.lastname
                            select new OvertimeEmployeeSettingViewModel
                            {
                                emp_id = emp.emp_id,
                                employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                                emp_status = emp.emp_status,
                                gender = emp.gender,
                                join_date = emp.join_date,
                                end_date = emp.end_date,
                                is_get_overtime = jlo.is_get_overtime,
                                approval_person = jlo.approval_person
                            };
                if (!string.IsNullOrEmpty(StatusFilter))
                {
                    query = query.Where(emp => emp.emp_status == StatusFilter);/*filter*/
                }
                else
                {
                    query = query.Where(emp => emp.emp_status == "A");/*filter*/
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
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult OvertimeEmployeeSettingSave([FromBody] OvertimeEmployeeSettingListViewModel model)
        {
            string PageId = "10553";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            if (model.Fields == null || !model.Fields.Any())
            {
                return Json(new { status = false, message = "No employees received." });
            }
            foreach (var emp in model.Fields)
            {
                var existing = _context.tbl_employee_overtime_settings.FirstOrDefault(e => e.emp_id == emp.emp_id);
                if (existing != null)
                {
                    existing.is_get_overtime = emp.is_get_overtime;
                    existing.approval_person = emp.approval_person;
                    _ = _context.tbl_employee_overtime_settings.Update(existing);
                }
                else
                {
                    var newEmp = new tbl_employee_overtime_settings
                    {
                        emp_id = emp.emp_id,
                        is_get_overtime = emp.is_get_overtime,
                        approval_person = emp.approval_person
                    };

                    _ = _context.tbl_employee_overtime_settings.Add(newEmp);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region TIMESHEET SETTING
        [HttpGet]
        public IActionResult TimesheetSetting()
        {
            #region FOR PERMISSION
            string PageId = "10515";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            /**using LEFT OUTER JOIN */
            var Records = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_salary_extra_settings
                on emp.emp_id equals jlo.emp_id into tblNewJoin
                from jlo in tblNewJoin.DefaultIfEmpty()
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new TimesheetSettingViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    timesheet_acceptance = jlo.timesheet_acceptance,
                    emp_year = jlo.emp_year,
                    emp_month = jlo.emp_month
                }
            ).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.TimesheetAccept = new SelectList(_settingsServices.TimesheetAcceptance(), "Value", "Text"); // this way too
            ViewBag.StartYear = _settingsServices.GetYears(0);
            ViewBag.StartMonth = _settingsServices.GetMonths();
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_TimesheetSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimesheetSettingList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query =
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_salary_extra_settings
                on emp.emp_id equals jlo.emp_id into tblNewJoin
                from jlo in tblNewJoin.DefaultIfEmpty()
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new TimesheetSettingViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    timesheet_acceptance = jlo.timesheet_acceptance,
                    emp_year = jlo.emp_year,
                    emp_month = jlo.emp_month
                };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(emp => emp.emp_status == StatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
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
        public JsonResult TimesheetSettingSave([FromBody] TimesheetSettingListViewModel model)
        {
            string PageId = "10515";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any())
            {
                return Json(new { status = false, message = "No employees received." });
            }
            foreach (var emp in model.Fields)
            {
                var existing = _context.tbl_employee_salary_extra_settings.FirstOrDefault(e => e.emp_id == emp.emp_id);
                if (existing != null)
                {
                    existing.timesheet_acceptance = emp.timesheet_acceptance;
                    existing.emp_year = emp.emp_year;
                    existing.emp_month = emp.emp_month;
                    _ = _context.tbl_employee_salary_extra_settings.Update(existing);
                }
                else
                {
                    var newEmp = new tbl_employee_salary_extra_settings
                    {
                        emp_id = emp.emp_id,
                        timesheet_acceptance = emp.timesheet_acceptance,
                        emp_year = emp.emp_year,
                        emp_month = emp.emp_month
                    };
                    _ = _context.tbl_employee_salary_extra_settings.Add(newEmp);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }

        #endregion
        /********************************************************************************************************************/
        #region SALARY SETTINGS EXTRA
        [HttpGet]
        public IActionResult SalarySettingsExtra()
        {
            #region FOR PERMISSION
            string PageId = "10510";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            //using LEFT OUTER JOIN
            var Records = (
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_salary_extra_settings
                on emp.emp_id equals jlo.emp_id into tblNewJoin
                from jlo in tblNewJoin.DefaultIfEmpty()
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalarySettingsExtraViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    duty_station_id = jlo.duty_station_id,
                    staff_type = jlo.staff_type,
                    is_field_staff = jlo.is_field_staff,
                    is_field_salary = jlo.is_field_salary,
                    is_get_dashain = jlo.is_get_dashain,
                    welfare_con_percent = jlo.welfare_con_percent,
                    get_leave_accrual = jlo.get_leave_accrual,
                    get_gratuity_accrual = jlo.get_gratuity_accrual,
                    gratuity_date = jlo.gratuity_date
                }
            ).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.DutyStationList = _employeeServices.GetDutyStationList();//we can do this way
            ViewBag.StaffTypeList = new SelectList(_employeeServices.StaffTypeList(), "Value", "Text"); // this way too
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_SalarySettingsExtra", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalarySettingsExtraList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query =
                from emp in _context.tbl_employee
                join jlo in _context.tbl_employee_salary_extra_settings
                on emp.emp_id equals jlo.emp_id into tblNewJoin
                from jlo in tblNewJoin.DefaultIfEmpty()
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalarySettingsExtraViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    duty_station_id = jlo.duty_station_id,
                    staff_type = jlo.staff_type,
                    is_field_staff = jlo.is_field_staff,
                    is_field_salary = jlo.is_field_salary,
                    is_get_dashain = jlo.is_get_dashain,
                    welfare_con_percent = jlo.welfare_con_percent,
                    get_leave_accrual = jlo.get_leave_accrual,
                    get_gratuity_accrual = jlo.get_gratuity_accrual,
                    gratuity_date = jlo.gratuity_date
                };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(emp => emp.emp_status == StatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
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
        public JsonResult SalarySettingsExtraSave([FromBody] SalarySettingsExtraListViewModel model)
        {
            string PageId = "10510";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any())
            {
                return Json(new { status = false, message = "No employees received." });
            }

            foreach (var emp in model.Fields)
            {
                var existing = _context.tbl_employee_salary_extra_settings.FirstOrDefault(e => e.emp_id == emp.emp_id);
                if (existing != null)
                {
                    existing.duty_station_id = emp.duty_station_id;
                    existing.staff_type = emp.staff_type;
                    existing.is_field_staff = emp.is_field_staff;
                    existing.is_field_salary = emp.is_field_salary;
                    existing.is_get_dashain = emp.is_get_dashain;
                    existing.welfare_con_percent = emp.welfare_con_percent;
                    existing.get_leave_accrual = emp.get_leave_accrual;
                    existing.get_gratuity_accrual = emp.get_gratuity_accrual;
                    existing.gratuity_date = emp.gratuity_date;
                    _ = _context.tbl_employee_salary_extra_settings.Update(existing);
                }
                else
                {
                    var newEmp = new tbl_employee_salary_extra_settings
                    {
                        emp_id = emp.emp_id,
                        duty_station_id = emp.duty_station_id,
                        staff_type = emp.staff_type,
                        is_field_staff = emp.is_field_staff,
                        is_field_salary = emp.is_field_salary,
                        is_get_dashain = emp.is_get_dashain,
                        welfare_con_percent = emp.welfare_con_percent,
                        get_leave_accrual = emp.get_leave_accrual,
                        get_gratuity_accrual = emp.get_gratuity_accrual,
                        gratuity_date = emp.gratuity_date
                    };
                    _ = _context.tbl_employee_salary_extra_settings.Add(newEmp);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region SALARY SETTINGS

        [HttpGet]
        public IActionResult SalarySettings()
        {
            string PageId = "10509";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from emp in _context.tbl_employee
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalarySettingsViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    gender = emp.gender,
                    join_date = emp.join_date,
                    end_date = emp.end_date,
                    marital_status = emp.marital_status,
                    salary = emp.salary,
                    child_edu_all = emp.child_edu_all,
                    remote_area_allow = emp.remote_area_allow,
                    yearly_remote_exem = emp.yearly_remote_exem,
                    emp_pay_status = emp.emp_pay_status,
                    account_no = emp.account_no,
                    pan_no = emp.pan_no
                }
            ).ToList();
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.GenderList = new SelectList(GenderList(), "Value", "Text");
            ViewBag.MaritalStatusList = new SelectList(MaritalStatusList(), "Value", "Text");
            ViewBag.EmpPayStatusList = new SelectList(EmpPayStatusList(), "Value", "Text");
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_SalarySettings", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalarySettingsList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query =
                from emp in _context.tbl_employee
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalarySettingsViewModel
                {
                    emp_id = emp.emp_id,
                    employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    emp_status = emp.emp_status,
                    child_edu_all = emp.child_edu_all,
                    gender = emp.gender,
                    join_date = emp.join_date,
                    end_date = emp.end_date,
                    marital_status = emp.marital_status,
                    salary = emp.salary,
                    remote_area_allow = emp.remote_area_allow,
                    yearly_remote_exem = emp.yearly_remote_exem,
                    emp_pay_status = emp.emp_pay_status,
                    account_no = emp.account_no,
                    pan_no = emp.pan_no
                };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(emp => emp.emp_status == StatusFilter);/*filter*/
            }
            else
            {
                query = query.Where(emp => emp.emp_status == "A");/*filter*/
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
        public JsonResult SalarySettingsSave([FromBody] SalarySettingsListViewModel model)
        {
            string PageId = "10509";
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
                    existing.gender = emp.gender;
                    existing.join_date = emp.join_date;
                    existing.end_date = emp.end_date;
                    existing.marital_status = emp.marital_status;
                    existing.salary = emp.salary;
                    existing.remote_area_allow = emp.remote_area_allow;
                    existing.yearly_remote_exem = emp.yearly_remote_exem;
                    existing.emp_pay_status = emp.emp_pay_status;
                    existing.account_no = emp.account_no;
                    existing.pan_no = emp.pan_no;
                    _ = _context.tbl_employee.Update(existing);

                    var DataSave = new tbl_employee_history
                    {
                        emp_id = emp.emp_id,
                        join_date = emp.join_date,
                        end_date = emp.end_date,
                        salary = emp.salary,
                        update_date = DateTime.Now,
                        remote_area_allow = emp.remote_area_allow,
                        yearly_remote_exem = emp.yearly_remote_exem,
                        by_emp_id = update_by,
                        marital_status = emp.marital_status
                    };
                    _ = _context.tbl_employee_history.Add(DataSave);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });

        }
        #endregion
        /********************************************************************************************************************/
        #region PAYROLL RATE

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult PayrollRate()
        {
            string PageId = "10511";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_setting_rate
                where a.setting_rate_year > 0
                orderby a.setting_rate_date descending
                select new SettingRateViewModel
                {
                    setting_rate_id = a.setting_rate_id,
                    setting_rate_date = a.setting_rate_date,
                    setting_rate = a.setting_rate,
                    setting_rate_period_name = a.setting_rate_period_name,
                    setting_rate_year = a.setting_rate_year,
                    setting_rate_status = a.setting_rate_status,
                    setting_rate_desc = a.setting_rate_desc,
                    fiscal_year = a.fiscal_year
                }).ToList();

            ViewBag.FiscalYearFilter = _settingsServices.GetFiscalYears("");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/PayrollRate", "ADD|DEL", PageId, Records.Count);
            return PartialView("Settings/_PayrollRate", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayrollRateList([FromForm] CostumFilterRequest request)
        {
            int pageSize = 0;
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var lenght = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            pageSize = lenght != null ? Convert.ToInt32(lenght) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            string FiscalYearFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = _context.tbl_setting_rate.Where(a => a.setting_rate_year > 0)
                .OrderByDescending(a => a.setting_rate_date)
                .Select(a => new SettingRateViewModel
                {
                    setting_rate_id = a.setting_rate_id,
                    setting_rate_date = a.setting_rate_date,
                    setting_rate = a.setting_rate,
                    setting_rate_period_name = a.setting_rate_period_name,
                    setting_rate_year = a.setting_rate_year,
                    setting_rate_status = a.setting_rate_status,
                    setting_rate_desc = a.setting_rate_desc,
                    fiscal_year = a.fiscal_year
                });

            if (!string.IsNullOrEmpty(FiscalYearFilter))
            {
                query = query.Where(d => d.fiscal_year == FiscalYearFilter);/*filter*/
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
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
        public IActionResult PayrollRateAddEdit(string id, string mode)
        {
            string PageId = "10511";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.YearList = _settingsServices.GetYears(0);
            ViewBag.MonthList = _settingsServices.GetMonths(0);
            ViewBag.SettingRateStatus = StatusOpenLocked("OL");
            /**this is to load blank form while doing add process */
            if (mode == "add")
            {
                var model = new SettingRateViewModel
                {
                    setting_rate_id = "",
                    setting_rate_date = null,
                    setting_rate = 0,
                    setting_rate_period_name = 0,
                    setting_rate_year = 0,
                    setting_rate_status = "",
                    setting_rate_desc = "",
                    fiscal_year = ""
                };
                ViewBag.fiscal_year = _settingsServices.GetFiscalYears("");
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_PayrollRateAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_setting_rate.FirstOrDefault(h => h.setting_rate_id == id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new SettingRateViewModel
                        {
                            setting_rate_id = smt.setting_rate_id,
                            setting_rate_date = Convert.ToDateTime(smt.setting_rate_date),
                            setting_rate = smt.setting_rate,
                            setting_rate_period_name = smt.setting_rate_period_name,
                            setting_rate_year = smt.setting_rate_year,
                            setting_rate_status = smt.setting_rate_status,
                            setting_rate_desc = smt.setting_rate_desc,
                            fiscal_year = smt.fiscal_year
                        };
                        ViewBag.fiscal_year = _settingsServices.GetFiscalYears(smt.fiscal_year);
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_PayrollRateAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult PayrollRateSave(SettingRateViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("setting_rate_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10511", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            DateTime setting_rate_date = Convert.ToDateTime(model.setting_rate_date);
            double? setting_rate = model.setting_rate;
            int? setting_rate_period_name = model.setting_rate_period_name;
            int? setting_rate_year = model.setting_rate_year;
            string? setting_rate_status = model.setting_rate_status;
            string? setting_rate_desc = model.setting_rate_desc;
            string? fiscal_year = model.fiscal_year;

            if (mode == "add")
            {
                var isData = _context.tbl_setting_rate.FirstOrDefault(u => u.fiscal_year == fiscal_year && u.setting_rate_year == setting_rate_year && u.setting_rate_period_name == setting_rate_period_name);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                string setting_rate_id = UniqueID();
                var DataSave = new tbl_setting_rate
                {
                    setting_rate_id = setting_rate_id,
                    setting_rate_date = setting_rate_date,
                    setting_rate = setting_rate,
                    setting_rate_period_name = setting_rate_period_name,
                    setting_rate_year = setting_rate_year,
                    setting_rate_status = setting_rate_status,
                    setting_rate_desc = setting_rate_desc,
                    fiscal_year = fiscal_year
                };
                _ = _context.tbl_setting_rate.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                string? setting_rate_id = Request.Form["setting_rate_id"];

                //check if the year, month and date lies within the fiscal year range 
                DateTime from_date = Convert.ToDateTime(setting_rate_year + "-" + setting_rate_period_name + "-01");
                string checkWithin = _settingsServices.CheckDateWithinFiscalYear(from_date, fiscal_year ?? "");
                if (!string.IsNullOrWhiteSpace(checkWithin))
                {
                    return Json(new { status = "false", message = checkWithin });
                }
                DateTime to_date = Convert.ToDateTime(setting_rate_year + "-" + setting_rate_period_name + "-" + DateTime.DaysInMonth((int)setting_rate_year, (int)setting_rate_period_name));
                if (setting_rate_date.Date >= from_date.Date && setting_rate_date.Date <= to_date.Date) { }
                else
                {
                    return Json(new { status = "false", message = $@"Please provide the date within {from_date} and {to_date}." });
                }
                /** check if the data is exits on another record */
                var isData = _context.tbl_setting_rate
                        .FirstOrDefault(u => u.fiscal_year == fiscal_year &&
                        u.setting_rate_year == setting_rate_year &&
                        u.setting_rate_period_name == setting_rate_period_name &&
                        u.setting_rate_id != setting_rate_id
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_setting_rate.FirstOrDefault(h => h.setting_rate_id == setting_rate_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.setting_rate_date = setting_rate_date;
                DataUpdate.setting_rate = setting_rate;
                DataUpdate.setting_rate_period_name = setting_rate_period_name;
                DataUpdate.setting_rate_year = setting_rate_year;
                DataUpdate.setting_rate_status = setting_rate_status;
                DataUpdate.setting_rate_desc = setting_rate_desc;
                DataUpdate.fiscal_year = fiscal_year;
                _ = _context.tbl_setting_rate.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayrollRateDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10511", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            // matching records
            var recordsToDelete = _context.tbl_setting_rate.Where(r => request.SelectedIds.Contains(r.setting_rate_id.ToString())).ToList();
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            // matching records with status locked
            var records = _context.tbl_setting_rate.Where(r => request.SelectedIds.Contains(r.setting_rate_id.ToString()) && r.setting_rate_status == "N").ToList();
            if (records.Count > 0)
            {
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });//FK record exists || Canot delete
            }
            _context.tbl_setting_rate.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            int deletedCount = recordsToDelete.Count;
            return Ok(new
            {
                status = "success",
                deletedCount,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletedCount.ToString(), StringComparison.Ordinal)
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayrollRateUpdateStatus([FromBody] bulkStatusUpdateRequest request)
        {
            if (!_accountServices.HasPermission("10511", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            string mode = request.mode;
            string hStatus = request.hStatus;
            hStatus = hStatus == "Open" ? "Y" : "N";

            if (mode == "updateStatus" && !string.IsNullOrWhiteSpace(hStatus))
            {
                // Bulk update all selected IDs
                int updatedCount = _context.tbl_setting_rate
                    .Where(r => request.SelectedIds.Contains(r.setting_rate_id.ToString()))
                    .ExecuteUpdate(setters => setters
                        .SetProperty(r => r.setting_rate_status, hStatus)
                    );

                if (updatedCount == 0)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                return Ok(new
                {
                    status = "success",
                    updatedCount,
                    message = Lang.msg_update_success
                });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region GLCODES SETTINGS

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult GLCodes()
        {
            #region FOR PERMISSION
            string PageId = "10512";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_settings_gl_codes
                orderby a.gl_code ascending
                select new SettingsGlCodesViewModel
                {
                    id = a.id,
                    gl_code = a.gl_code,
                    gl_type = a.gl_type,
                    staff_type = a.staff_type
                }).ToList();

            ViewBag.GLType = _employeeServices.GLTypeList("GL");
            ViewBag.StaffType = _employeeServices.StaffTypeList("ST");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/GLCodes", "ADD|", PageId, Records.Count);
            return PartialView("Settings/_GLCodes", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GLCodesList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = from a in _context.tbl_settings_gl_codes
                        orderby a.gl_code descending
                        select new SettingsGlCodesViewModel
                        {
                            id = a.id,
                            gl_code = a.gl_code,
                            gl_type = a.gl_type,
                            staff_type = a.staff_type
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
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
        public IActionResult GLCodesAddEdit(int id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10512";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.GLType = _employeeServices.GLTypeList("GL");
            ViewBag.StaffType = _employeeServices.StaffTypeList("ST");
            ViewBag.mode = mode;

            //this is to load blank form while doing add process
            if (mode == "add")
            {
                var model = new SettingsGlCodesViewModel
                {
                    id = 0,
                    gl_code = "",
                    gl_type = "",
                    staff_type = ""
                };
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_GLCodesAddEdit", model);
            }
            else if (mode == "edit")
            {
                short Id = Convert.ToInt16(id);
                var smt = (from a in _context.tbl_settings_gl_codes
                           where a.id == Id
                           select new
                           {
                               a.id,
                               a.gl_code,
                               a.gl_type,
                               a.staff_type
                           }).FirstOrDefault();
                if (smt == null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var model = new SettingsGlCodesViewModel
                    {
                        id = smt.id,
                        gl_code = smt.gl_code,
                        gl_type = smt.gl_type,
                        staff_type = smt.staff_type
                    };
                    ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                    return PartialView("Settings/_GLCodesAddEdit", model);
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GLCodesSave(SettingsGlCodesViewModel model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10512", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            int id = Convert.ToInt32(Request.Form["id"]); //not used model due to ModelState.Remove("id");
            string gl_code = model.gl_code ?? "";
            string gl_type = model.gl_type ?? "";
            string staff_type = model.staff_type ?? "";
            /** ADD NEW */
            if (mode == "add")
            {
                /** check if the data is exits on another record */
                var isData = _context.tbl_settings_gl_codes.FirstOrDefault(u => u.gl_code == gl_code);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                /** g3t max!mum id */
                int Id = (_context.tbl_settings_gl_codes.Any() ? _context.tbl_settings_gl_codes.Max(o => o.id) : 0) + 1;
                var DataSave = new tbl_settings_gl_codes
                {
                    id = Id,
                    gl_code = gl_code,
                    gl_type = gl_type,
                    staff_type = staff_type
                };
                _ = _context.tbl_settings_gl_codes.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                var DataUpdate = _context.tbl_settings_gl_codes.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                DataUpdate.gl_code = gl_code;
                DataUpdate.gl_type = gl_type;
                DataUpdate.staff_type = staff_type;
                _ = _context.tbl_settings_gl_codes.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region SALARY TAX PERCENT
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult SalaryTaxPercent()
        {
            string PageId = "10555";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            var employees = (
                from emp in _context.tbl_employee
                join txp in _context.tbl_employee_salary_tax_percent
                on emp.emp_id equals txp.emp_id into tblEmpTaxPercent
                from txp in tblEmpTaxPercent.DefaultIfEmpty()
                where emp.emp_id != 0 && emp.emp_status == "A"
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalaryTaxPercentViewModel
                {
                    EmpId = emp.emp_id,
                    Employee = string.Join(" ", new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }.Where(x => !string.IsNullOrEmpty(x))),
                    Marital = emp.marital_status == "M" ? "Married" : "Single",
                    Gender = emp.gender == "M" ? "Male" : "Female",
                    EmpStatus = emp.emp_status == "A" ? "Active" : "Inactive",
                    StartDate = Convert.ToDateTime(emp.join_date),
                    EndDate = Convert.ToDateTime(emp.end_date),
                    Salary = Math.Round(Convert.ToDecimal(emp.salary), 2),
                    percent_for_tax_add = txp.percent_for_tax_add
                }
            ).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD");
            ViewBag.PercentForTaxAdd = new SelectList(_settingsServices.PercentForTaxAdd(), "Value", "Text"); // this way too
            ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
            return PartialView("Settings/_SalaryTaxPercent", employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalaryTaxPercentList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = !string.IsNullOrWhiteSpace(request.FilterValue) ? request.FilterValue : "A";
            var query =
                from emp in _context.tbl_employee
                join txp in _context.tbl_employee_salary_tax_percent
                on emp.emp_id equals txp.emp_id into tblEmpTaxPercent
                from txp in tblEmpTaxPercent.DefaultIfEmpty()
                where emp.emp_id != 0
                orderby emp.firstname, emp.middlename, emp.lastname
                select new SalaryTaxPercentViewModel
                {
                    EmpId = emp.emp_id,
                    Employee = string.Join(" ",
                    new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }
                    .Where(x => !string.IsNullOrEmpty(x))),
                    Marital = emp.marital_status == "M" ? "Married" : "Single",
                    Gender = emp.gender == "M" ? "Male" : "Female",
                    EmpStatus = emp.emp_status,
                    StartDate = Convert.ToDateTime(emp.join_date),
                    EndDate = Convert.ToDateTime(emp.end_date),
                    Salary = Math.Round(Convert.ToDecimal(emp.salary), 2),
                    percent_for_tax_add = txp.percent_for_tax_add
                };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.EmpStatus == StatusFilter);/*filter*/
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
        public JsonResult SalaryTaxPercentSave([FromBody] SalaryTaxPercentListViewModel model)
        {
            if (!_accountServices.HasPermission("10555", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            string PageId = "10555";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION
            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "error", message = "Not Authorized User" }); }
            if (!ModelState.IsValid) { return Json(new { status = "error", message = Lang.msg_error_invalid }); }
            if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = "error", message = "No employees received." }); }

            foreach (var emp in model.Fields)
            {
                string percent_for_tax_add = emp.percent_for_tax_add ?? "";
                var existing = _context.tbl_employee_salary_tax_percent.FirstOrDefault(e => e.emp_id == emp.EmpId);
                if (existing != null)
                {
                    existing.percent_for_tax_add = percent_for_tax_add;
                    _ = _context.tbl_employee_salary_tax_percent.Update(existing);
                }
                else
                {
                    var newEmp = new tbl_employee_salary_tax_percent
                    {
                        emp_id = emp.EmpId,
                        percent_for_tax_add = percent_for_tax_add
                    };
                    _ = _context.tbl_employee_salary_tax_percent.Add(newEmp);
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region DEPENDENT VERIFICATION
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult DependentVerification()
        {
            string PageId = "10552";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            SettingDependentVerificationViewModel model;
            var sb = new StringBuilder();

            /*******************************************************************************************************'
            *UPDATE STATUS TO INACTIVE (ELIGIBILITY = 'I') FOR THE DEPENENDENT WHO CROSSED AGE 25
            *****************************************************************************************************'*/
            _settingsServices.DeactivateDependent();

            //Get dependent setting
            int max_nos_dep_child_eligible_paid = 0;
            DateTime age_checking_date = DateTime.Now;
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
                           select new
                           {
                               dep.emp_dep_id,
                               dep.emp_id,
                               dep.c_name,
                               dep.gender,
                               dep.date_of_birth,
                               dep.dob_file_name,
                               dep.submit_date,
                               dep.update_date,
                               dep.eligibility,
                               dep.remarks,
                               emp.emp_status
                           }).ToList();
            model = new SettingDependentVerificationViewModel
            {
                id = 0,
                emp_id = 0,
                emp_dep_id = 0,
                emp_dep_sub_id = 0,
                status = "",
                update_date = null
            };
            if (Records != null)
            {
                int cnt = 0;
                int cnt_sub = 1;
                int old_emp_id = 0;
                foreach (var rec in Records)
                {
                    int emp_dep_id = rec.emp_dep_id;
                    int emp_id = rec.emp_id;
                    string c_name = rec.c_name;
                    string gender = rec.gender;
                    DateTime date_of_birth = Convert.ToDateTime(rec.date_of_birth);
                    string dob_file_name = rec.dob_file_name;
                    DateTime submit_date = Convert.ToDateTime(rec.submit_date);
                    DateTime update_date = Convert.ToDateTime(rec.update_date);
                    string eligibility = rec.eligibility;
                    string remarks = rec.remarks;
                    string emp_status = rec.emp_status;
                    string employee = _employeeServices.GetEmployeeName(emp_id);
                    gender = gender == "M" ? "Male" : "Female";
                    double diff = (age_checking_date - date_of_birth).Days + 1;
                    double age = Math.Round(diff / 365.0, 2);
                    if (emp_id != old_emp_id)
                    {
                        cnt++;
                        _ = sb.AppendLine($@"<tr bgcolor=""#9DD5FD""> 
                        <th class=""title left"" height=""30"">&nbsp;&nbsp;{cnt}</th>
                        <th class=""title left"" colspan=""8"">{employee}</th>
                        </tr>");
                        cnt_sub = 1; //RESET SUB COUNTER
                    }
                    string receipt_link = "";
                    string receipt_need = @"<img src = ""/images/expiring-soon.gif"">";
                    string preview_link = @"<span class=""red"">[N/A]</span>";
                    string dob_certific = @"<span class=""red"">[N/A]</span>";

                    string? rurl = Url.Action("employee_dependent_detail_receipt", "Employee", new { mode = "doc", emp_dep_id = emp_dep_id, emp_id = emp_id, depid = emp_dep_id });

                    string rupload_link = $@"<span class=""red"">[</span>
                    <a href=""{rurl}"" class=""rupload-link"" title=""Receipt"">Upload</a>
                    <span class=""red"">]</span>";

                    string dupload_link = $@"<span class=""red"">[</span>
                    <a href=""javascript:PopUpW('/employee/employee_dependent_detail_dob_certificate.asp?mode=doc&emp_dep_id={emp_dep_id}&emp_id={emp_id}&depid={emp_dep_id}')"" title =""Certificate"">Upload</a>
                    <span class=""red"">]</span>";

                    string activat_link = $@"<span class=""red"">[</span>
                    <a href=""javascript:postDependentVerification('acti','{cnt}','{cnt_sub}')"">Activate</a>
                    <span class=""red"">]</span>";

                    string deactiv_link = $@"<span coclasslor=""red"">[</span>
                    <a href=""javascript:postDependentVerification('deac','{cnt}','{cnt_sub}')"">Deactivate</a>
                    <span class=""red"">]</span>";

                    string pending_link = $@"<span class=""red"">[</span>
                    <a href=""javascript:postDependentVerification('pend','{cnt}','{cnt_sub}')"">Pending</a>
                    <span class=""red"">]</span>";

                    if (!string.IsNullOrWhiteSpace(dob_file_name))
                    {
                        string doburl = Url.Content("~/documents/dependent/{dob_file_name}");
                        dob_certific = $@"<span class=""red"">[</span>
                        <a href=""{doburl}"" class ""dob-link"" >Preview</a><span class=""red"">]</span>";
                    }
                    else
                    {
                        activat_link = "";
                        deactiv_link = "";
                    }
                    /*checking age restriction*/
                    int emp_dep_sub_id = 0;
                    if (age is >= 18 and < 25)
                    {
                        var smt = (from a in _context.tbl_employee_dependent_children_details_sub
                                   where a.fiscal_year == HttpContext.Session.GetString("fiscal_year") &&
                                   a.emp_dep_id == emp_dep_id
                                   select new
                                   {
                                       a.emp_dep_sub_id,
                                       a.file_name,
                                       a.status
                                   })
                        .FirstOrDefault();
                        if (smt != null)
                        {
                            emp_dep_sub_id = smt.emp_dep_sub_id;
                            string receipt_file_name = smt.file_name;
                            string receipt_status = smt.status;

                            preview_link = $@"<span class=""red"">[</span>
                            <a href=""javascript:PopUp('/documents/dependent/{receipt_file_name}')"" title=""Receipt"">Preview</a><span class=""red"">]</span>";

                            if (receipt_status == "A")
                            {
                                receipt_link = $@"<span class=""red"">[</span>
                                <a href=""javascript:postDependentVerification('unv','{cnt}','{cnt_sub}')"" title=""Receipt"">Unverify</a><span class=""red"">]</span>";
                                receipt_need = "";
                            }
                            else
                            {
                                receipt_link = $@"<span class=""red"">[</span>
                                <a href=""javascript:postDependentVerification('vry','{cnt}','{cnt_sub}')"" title=""Receipt"">Verify</a><span class=""red"">]</span>";
                            }
                        }
                    }
                    else
                    {
                        receipt_need = "";
                        preview_link = "-";
                        rupload_link = "";
                    }

                    string bgcolor = "";
                    string status = "";

                    if (eligibility == "A")
                    {
                        status = "Active";
                        bgcolor = "#ABE9AB";
                        dupload_link = "";
                        activat_link = "";
                        if (HttpContext.Session.GetString("user_id") != "1") { pending_link = ""; } //Only super administrator can
                    }
                    else if (eligibility == "I")
                    {
                        status = "Inactive";
                        bgcolor = "#FF9F9F";
                        dupload_link = "";
                        if (age is > 0 and < 25) { deactiv_link = ""; }
                        if (HttpContext.Session.GetString("user_id") != "1") { pending_link = ""; } //Only super administrator can
                    }
                    else
                    {
                        status = "Pending";
                        bgcolor = "#EEEEEE";
                        ////if (user_access = "D" or user_access = "E" or user_access = "F") then dupload_link = "" // This need to address
                        if (age is > 0 and < 25) { /*void*/} else { activat_link = ""; }
                        pending_link = "";
                    }
                    /*restric the activation key depending upon following settings | Trying to get active count**/
                    int eligible_cnt = 0;
                    var sql = _context.tbl_employee_dependent_children_details
                        .Where(d => d.emp_id == emp_id)
                        .GroupBy(d => d.emp_id)
                        .Select(g => new
                        {
                            emp_id = g.Key,
                            Total = g.Count(),
                            P = g.Sum(x => x.eligibility == "P" ? 1 : 0),
                            A = g.Sum(x => x.eligibility == "A" ? 1 : 0),
                            I = g.Sum(x => x.eligibility == "I" ? 1 : 0)
                        }).FirstOrDefault();

                    if (sql != null) { eligible_cnt = Convert.ToInt32(sql.A); }
                    if (eligible_cnt >= max_nos_dep_child_eligible_paid) { activat_link = ""; }

                    /* for now disabled upload feature from here. 
                     * this is due to new user access format
                     * need to upload from employee section
                    */
                    dupload_link = "";
                    rupload_link = "";

                    if (perm.apern != "true" && perm.epern != "true")
                    {
                        receipt_link = "";
                        activat_link = "";
                        deactiv_link = "";
                        pending_link = "";
                    }

                    string hiddens = $@"
                            <input type=""hidden"" name=""emp_id_{cnt}_{cnt_sub}"" id =""emp_id_{cnt}_{cnt_sub}"" value =""{emp_id}"">
                            <input type=""hidden"" name=""dep_id_{cnt}_{cnt_sub}"" id =""dep_id_{cnt}_{cnt_sub}"" value =""{emp_dep_id}"">
                            <input type=""hidden"" name=""sub_id_{cnt}_{cnt_sub}"" id =""sub_id_{cnt}_{cnt_sub}"" value =""{emp_dep_sub_id}"">
                            ";

                    _ = sb.AppendLine($@"<tr bgcolor=""{bgcolor}"">
                        <th class=""normal left"" height=""30"">&nbsp;&nbsp;{cnt}.{cnt_sub}{hiddens}</th>
                        <th class=""normal left"">{c_name}</th>
                        <th class=""normal left"">{gender}</th>
                        <th class=""normal left"">{_settingsServices.DateformatToDt(date_of_birth.ToString())}</th>
                        <th class=""normal left"">{status}</th>
                        <th class=""normal left""><a name=""age"" title=""{date_of_birth} - {age_checking_date}"">{age}</a> &nbsp;{receipt_need}</th>
                        <th class=""normal left"">{dob_certific}{dupload_link}</th>
                        <th class=""normal left"">{preview_link}{rupload_link}{receipt_link}</th>
                        <th class=""normal left"">{activat_link}{deactiv_link}{pending_link}</th>
                        </tr>");

                    old_emp_id = emp_id;
                    cnt_sub++;
                }

            }
            ViewBag.Contents = sb;
            return PartialView("Settings/_DependentVerification", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DependentVerificationSave()
        {
            string PageId = "10552";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true")
            {
                return Json(new { status = "invalid", message = "Not Authorized User" });
            }
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            string? Id = Request.Form["Id"];
            string? Ids = Request.Form["Ids"];
            string? key1 = $"emp_id_{Id}_{Ids}";
            string? key2 = $"dep_id_{Id}_{Ids}";
            string? key3 = $"sub_id_{Id}_{Ids}";

            string? value1 = Request.Form[key1];
            string? value2 = Request.Form[key2];
            string? value3 = Request.Form[key3];

            int emp_id = (!string.IsNullOrEmpty(value1) && int.TryParse(value1, out var parsed1)) ? parsed1 : 0;
            int emp_dep_id = (!string.IsNullOrEmpty(value1) && int.TryParse(value1, out var parsed2)) ? parsed2 : 0;
            int emp_dep_sub_id = (!string.IsNullOrEmpty(value1) && int.TryParse(value1, out var parsed3)) ? parsed3 : 0;

            if (mode == "unv")
            {
                var DataUpdate = _context.tbl_employee_dependent_children_details_sub.FirstOrDefault(h => h.emp_dep_sub_id == emp_dep_sub_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                // UPDATE if not data in timesheet
                DataUpdate.status = "P";
                DataUpdate.update_date = DateTime.Now;
                _ = _context.tbl_employee_dependent_children_details_sub.Update(DataUpdate);
                _ = _context.SaveChanges();
            }
            else if (mode == "vry")
            {
                var DataUpdate = _context.tbl_employee_dependent_children_details_sub.FirstOrDefault(h => h.emp_dep_sub_id == emp_dep_sub_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                /**  UPDATE if not data in timesheet */
                DataUpdate.status = "A";
                DataUpdate.update_date = DateTime.Now;
                _ = _context.tbl_employee_dependent_children_details_sub.Update(DataUpdate);
                _ = _context.SaveChanges();
            }
            else if (mode == "deac")
            {
                var DataUpdate = _context.tbl_employee_dependent_children_details.FirstOrDefault(h => h.emp_dep_id == emp_dep_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                /** UPDATE if not data in timesheet */
                DataUpdate.eligibility = "I";
                DataUpdate.update_date = DateTime.Now;
                _ = _context.tbl_employee_dependent_children_details.Update(DataUpdate);
                _ = _context.SaveChanges();
            }
            else if (mode == "acti")
            {
                var DataUpdate = _context.tbl_employee_dependent_children_details.FirstOrDefault(h => h.emp_dep_id == emp_dep_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                /** UPDATE if not data in timesheet */
                DataUpdate.eligibility = "A";
                DataUpdate.update_date = DateTime.Now;
                _ = _context.tbl_employee_dependent_children_details.Update(DataUpdate);
                _ = _context.SaveChanges();
            }
            else if (mode == "pend")
            {
                var DataUpdate = _context.tbl_employee_dependent_children_details.FirstOrDefault(h => h.emp_dep_id == emp_dep_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                /** UPDATE if not data in timesheet */
                DataUpdate.eligibility = "P";
                DataUpdate.update_date = DateTime.Now;
                _ = _context.tbl_employee_dependent_children_details.Update(DataUpdate);
                _ = _context.SaveChanges();
            }
            return Json(new { status = "success", message = Lang.msg_update_success });
        }
        #endregion
        /********************************************************************************************************************/
        #region TAX SETTING
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult TaxSetting()
        {
            string PageId = "10557";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.CurrncySymbol = _globalOptionServices.OptionServices["op_currency_symbol"];
            TaxSettingViewModel model;

            var Records = _context.tbl_tax_setting.FirstOrDefault();
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new TaxSettingViewModel
                {
                    Id = 0,
                    single_amt = 0,
                    married_amt = 0,
                    first_tax_percent = 0,
                    second_tax_percent = 0,
                    is_used_initial_tax_percent = false,
                    initial_tax_percent = 0,
                    first_tax_amount = 0,
                    second_tax_amount = 0,
                    third_tax_amount_single = 0,
                    third_tax_amount_married = 0,
                    third_tax_percent = 0,
                    fourth_tax_percent = 0,
                    single_female_ded_per = 0,
                    max_medical_expenses_reimbursed = 0,
                    max_medical_tax_credit_amount = 0,
                    max_medical_tax_credit_per = 0,
                    ins_amt = 0,
                    ins_amt_non_life = 0,
                    fourth_tax_amount = 0,
                    fifth_tax_percent = 0
                };
            }
            else
            {
                ViewBag.mode = "edit";
                bool is_used_initial_tax_percent = Convert.ToInt32(Records.is_used_initial_tax_percent) == 1;

                model = new TaxSettingViewModel
                {
                    Id = Records.Id,
                    single_amt = Records.single_amt,
                    married_amt = Records.married_amt,
                    first_tax_percent = Records.first_tax_percent,
                    second_tax_percent = Records.second_tax_percent,
                    is_used_initial_tax_percent = is_used_initial_tax_percent,
                    initial_tax_percent = Records.initial_tax_percent,
                    first_tax_amount = Records.first_tax_amount,
                    second_tax_amount = Records.second_tax_amount,
                    third_tax_amount_single = Records.third_tax_amount_single,
                    third_tax_amount_married = Records.third_tax_amount_married,
                    third_tax_percent = Records.third_tax_percent,
                    fourth_tax_percent = Records.fourth_tax_percent,
                    single_female_ded_per = Records.single_female_ded_per,
                    max_medical_expenses_reimbursed = Records.max_medical_expenses_reimbursed,
                    max_medical_tax_credit_amount = Records.max_medical_tax_credit_amount,
                    max_medical_tax_credit_per = Records.max_medical_tax_credit_per,
                    ins_amt = Records.ins_amt,
                    ins_amt_non_life = Records.ins_amt_non_life,
                    fourth_tax_amount = Records.fourth_tax_amount,
                    fifth_tax_percent = Records.fifth_tax_percent
                };
            }
            return PartialView("Settings/_TaxSetting", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TaxSettingSave(TaxSettingViewModel model)
        {
            _ = ModelState.Remove("Id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10557", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            short Id = model.Id;

            bool is_used_initial_tax_percent = Convert.ToBoolean(model.is_used_initial_tax_percent);
            decimal initial_tax_percent = Math.Round(Convert.ToDecimal(model.initial_tax_percent), 2);

            decimal single_amt = Math.Round(Convert.ToDecimal(model.single_amt), 2);
            decimal married_amt = Math.Round(Convert.ToDecimal(model.married_amt), 2);
            double first_tax_percent = Math.Round(Convert.ToDouble(model.first_tax_percent), 2);
            double second_tax_percent = Math.Round(Convert.ToDouble(model.second_tax_percent), 2);
            double first_tax_amount = Math.Round(Convert.ToDouble(model.first_tax_amount), 2);
            decimal second_tax_amount = Math.Round(Convert.ToDecimal(model.second_tax_amount), 2);
            decimal third_tax_amount_single = Math.Round(Convert.ToDecimal(model.third_tax_amount_single), 2);
            decimal third_tax_amount_married = Math.Round(Convert.ToDecimal(model.third_tax_amount_married), 2);
            double third_tax_percent = Math.Round(Convert.ToDouble(model.third_tax_percent), 2);
            double fourth_tax_percent = Math.Round(Convert.ToDouble(model.fourth_tax_percent), 2);
            double single_female_ded_per = Math.Round(Convert.ToDouble(model.single_female_ded_per), 2);
            double max_medical_expenses_reimbursed = Math.Round(Convert.ToDouble(model.max_medical_expenses_reimbursed), 2);
            double max_medical_tax_credit_amount = Math.Round(Convert.ToDouble(model.max_medical_tax_credit_amount), 2);
            double max_medical_tax_credit_per = Math.Round(Convert.ToDouble(model.max_medical_tax_credit_per), 2);
            decimal ins_amt = Math.Round(Convert.ToDecimal(model.ins_amt), 2);
            decimal ins_amt_non_life = Math.Round(Convert.ToDecimal(model.ins_amt_non_life), 2);
            decimal fourth_tax_amount = Math.Round(Convert.ToDecimal(model.fourth_tax_amount), 2);
            double fifth_tax_percent = Math.Round(Convert.ToDouble(model.fifth_tax_percent), 2);

            // ADD NEW
            if (mode == "add")
            {
                var DataSave = new tbl_tax_setting
                {
                    Id = 1,
                    single_amt = single_amt,
                    married_amt = married_amt,
                    first_tax_percent = first_tax_percent,
                    second_tax_percent = second_tax_percent,
                    is_used_initial_tax_percent = is_used_initial_tax_percent,
                    initial_tax_percent = initial_tax_percent,
                    first_tax_amount = first_tax_amount,
                    second_tax_amount = second_tax_amount,
                    third_tax_amount_single = third_tax_amount_single,
                    third_tax_amount_married = third_tax_amount_married,
                    third_tax_percent = third_tax_percent,
                    fourth_tax_percent = fourth_tax_percent,
                    single_female_ded_per = single_female_ded_per,
                    max_medical_expenses_reimbursed = max_medical_expenses_reimbursed,
                    max_medical_tax_credit_amount = max_medical_tax_credit_amount,
                    max_medical_tax_credit_per = max_medical_tax_credit_per,
                    ins_amt = ins_amt,
                    ins_amt_non_life = ins_amt_non_life,
                    fourth_tax_amount = fourth_tax_amount,
                    fifth_tax_percent = fifth_tax_percent
                };
                _ = _context.tbl_tax_setting.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success, id = 1 });
            }
            else if (mode == "edit")
            {
                // UPDATE
                var DataUpdate = _context.tbl_tax_setting.FirstOrDefault(h => h.Id == Id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.single_amt = single_amt;
                DataUpdate.married_amt = married_amt;
                DataUpdate.first_tax_percent = first_tax_percent;
                DataUpdate.second_tax_percent = second_tax_percent;
                DataUpdate.is_used_initial_tax_percent = is_used_initial_tax_percent;
                DataUpdate.initial_tax_percent = initial_tax_percent;
                DataUpdate.first_tax_amount = first_tax_amount;
                DataUpdate.second_tax_amount = second_tax_amount;
                DataUpdate.third_tax_amount_single = third_tax_amount_single;
                DataUpdate.third_tax_amount_married = third_tax_amount_married;
                DataUpdate.third_tax_percent = third_tax_percent;
                DataUpdate.fourth_tax_percent = fourth_tax_percent;
                DataUpdate.single_female_ded_per = single_female_ded_per;
                DataUpdate.max_medical_expenses_reimbursed = max_medical_expenses_reimbursed;
                DataUpdate.max_medical_tax_credit_amount = max_medical_tax_credit_amount;
                DataUpdate.max_medical_tax_credit_per = max_medical_tax_credit_per;
                DataUpdate.ins_amt = ins_amt;
                DataUpdate.ins_amt_non_life = ins_amt_non_life;
                DataUpdate.fourth_tax_amount = fourth_tax_amount;
                DataUpdate.fifth_tax_percent = fifth_tax_percent;

                _ = _context.tbl_tax_setting.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.Id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region DEPENDENT SETTING
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult DependentSetting()
        {
            string PageId = "10551";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;

            SettingDependentChildrenDetailsViewModel model;

            var Records = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new SettingDependentChildrenDetailsViewModel
                {
                    id = 0,
                    max_nos_dep_child_eligible_paid = 0,
                    max_amt_first_age_range = 0,
                    max_amt_second_age_range = 0,
                    age_checking_date = null,
                    child_pro_rata_age = 0,
                    emp_pro_rata_age = 0
                };
            }
            else
            {
                ViewBag.mode = "edit";
                model = new SettingDependentChildrenDetailsViewModel
                {
                    id = Convert.ToInt32(Records.id),
                    max_nos_dep_child_eligible_paid = Records.max_nos_dep_child_eligible_paid,
                    max_amt_first_age_range = Math.Round(Convert.ToDecimal(Records.max_amt_first_age_range), 2),
                    max_amt_second_age_range = Math.Round(Convert.ToDecimal(Records.max_amt_second_age_range), 2),
                    age_checking_date = Records.age_checking_date,
                    child_pro_rata_age = Records.child_pro_rata_age,
                    emp_pro_rata_age = Records.emp_pro_rata_age
                };
            }
            return PartialView("Settings/_DependentSetting", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DependentSettingSave(SettingDependentChildrenDetailsViewModel model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10551", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            int max_nos_dep_child_eligible_paid = Convert.ToInt32(model.max_nos_dep_child_eligible_paid);
            decimal max_amt_first_age_range = Convert.ToDecimal(model.max_amt_first_age_range);
            decimal max_amt_second_age_range = Convert.ToDecimal(model.max_amt_second_age_range);
            DateTime age_checking_date = Convert.ToDateTime(model.age_checking_date);
            double child_pro_rata_age = Convert.ToDouble(model.child_pro_rata_age);
            double emp_pro_rata_age = Convert.ToDouble(model.emp_pro_rata_age);

            string yesNo = _settingsServices.CheckDateWithinFiscalYear(age_checking_date, HttpContext.Session.GetString("fiscal_year") ?? "");
            if (!string.IsNullOrWhiteSpace(yesNo))
            {
                return Json(new { status = "error", message = yesNo });
            }
            if (mode == "add")
            {
                var DataSave = new tbl_setting_dependent_children_details
                {
                    id = 1,
                    max_nos_dep_child_eligible_paid = max_nos_dep_child_eligible_paid,
                    max_amt_first_age_range = max_amt_first_age_range,
                    max_amt_second_age_range = max_amt_second_age_range,
                    age_checking_date = age_checking_date,
                    child_pro_rata_age = child_pro_rata_age,
                    emp_pro_rata_age = emp_pro_rata_age
                };
                _ = _context.tbl_setting_dependent_children_details.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success, id = 1 });
            }
            else if (mode == "edit")
            {
                int id = model.id;
                var DataUpdate = _context.tbl_setting_dependent_children_details.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.max_nos_dep_child_eligible_paid = max_nos_dep_child_eligible_paid;
                DataUpdate.max_amt_first_age_range = max_amt_first_age_range;
                DataUpdate.max_amt_second_age_range = max_amt_second_age_range;
                DataUpdate.age_checking_date = age_checking_date;
                DataUpdate.child_pro_rata_age = child_pro_rata_age;
                DataUpdate.emp_pro_rata_age = emp_pro_rata_age;

                _ = _context.tbl_setting_dependent_children_details.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region WORKING HOURS
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult WorkingHours()
        {
            string PageId = "10516";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            SettingLimitHrsViewModel model;
            ViewBag.Status = StatusActivePassive("YN");
            var Records = _context.tbl_setting_limit_hrs.FirstOrDefault();
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new SettingLimitHrsViewModel
                {
                    hrs_id = "",
                    normal_working_hrs = 0,
                    overtime_normal_working_hrs = 0,
                    overtime_hol_wek_working_hrs = 0,
                    working_hours_per_pay_period = 0,
                    populate_hrs_in_timesheet_for_holiday = "",
                    populate_hrs_in_timesheet_for_weekend = ""
                };
            }
            else
            {
                ViewBag.mode = "edit";
                model = new SettingLimitHrsViewModel
                {
                    hrs_id = Records.hrs_id,
                    normal_working_hrs = Convert.ToInt32(Records.normal_working_hrs),
                    overtime_normal_working_hrs = Convert.ToInt32(Records.overtime_normal_working_hrs),
                    overtime_hol_wek_working_hrs = Convert.ToInt32(Records.overtime_hol_wek_working_hrs),
                    working_hours_per_pay_period = Convert.ToInt32(Records.working_hours_per_pay_period),
                    populate_hrs_in_timesheet_for_holiday = Records.populate_hrs_in_timesheet_for_holiday,
                    populate_hrs_in_timesheet_for_weekend = Records.populate_hrs_in_timesheet_for_weekend
                };
            }
            return PartialView("Settings/_WorkingHours", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult WorkingHoursSave(SettingLimitHrsViewModel model)
        {
            _ = ModelState.Remove("hrs_id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10516", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            int normal_working_hrs = Convert.ToInt32(model.normal_working_hrs);
            int overtime_normal_working_hrs = Convert.ToInt32(model.overtime_normal_working_hrs);
            int overtime_hol_wek_working_hrs = Convert.ToInt32(model.overtime_hol_wek_working_hrs);
            int working_hours_per_pay_period = Convert.ToInt32(model.working_hours_per_pay_period);
            string? populate_hrs_in_timesheet_for_holiday = model.populate_hrs_in_timesheet_for_holiday;
            string? populate_hrs_in_timesheet_for_weekend = model.populate_hrs_in_timesheet_for_weekend;

            if (mode == "add")
            {
                string hrs_id = UniqueID();
                var DataSave = new tbl_setting_limit_hrs
                {
                    hrs_id = hrs_id,
                    normal_working_hrs = normal_working_hrs,
                    overtime_normal_working_hrs = overtime_normal_working_hrs,
                    overtime_hol_wek_working_hrs = overtime_hol_wek_working_hrs,
                    working_hours_per_pay_period = working_hours_per_pay_period,
                    populate_hrs_in_timesheet_for_holiday = populate_hrs_in_timesheet_for_holiday,
                    populate_hrs_in_timesheet_for_weekend = populate_hrs_in_timesheet_for_weekend
                };
                _ = _context.tbl_setting_limit_hrs.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success, id = hrs_id });
            }
            else if (mode == "edit")
            {
                string hrs_id = model.hrs_id;
                var DataUpdate = _context.tbl_setting_limit_hrs.FirstOrDefault(h => h.hrs_id == hrs_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.normal_working_hrs = normal_working_hrs;
                DataUpdate.overtime_normal_working_hrs = overtime_normal_working_hrs;
                DataUpdate.overtime_hol_wek_working_hrs = overtime_hol_wek_working_hrs;
                DataUpdate.working_hours_per_pay_period = working_hours_per_pay_period;
                DataUpdate.populate_hrs_in_timesheet_for_holiday = populate_hrs_in_timesheet_for_holiday;
                DataUpdate.populate_hrs_in_timesheet_for_weekend = populate_hrs_in_timesheet_for_weekend;

                _ = _context.tbl_setting_limit_hrs.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.hrs_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region FISCAL YEAR SETTINGS

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult FiscalYearSetting()
        {
            #region FOR PERMISSION
            string PageId = "10503";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_fiscal_year
                orderby a.fiscal_year descending
                select new FiscalYearViewModel
                {
                    fiscal_year = a.fiscal_year,
                    date_from = Convert.ToDateTime(a.date_from),
                    date_to = Convert.ToDateTime(a.date_to),
                    is_active = a.is_active,
                    fiscal_year_abb = a.fiscal_year_abb ?? "",
                    yearly_working_hrs = a.yearly_working_hrs
                }).ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/FiscalYearSetting", "ADD|DEL", PageId, Records.Count);
            return PartialView("Settings/_FiscalYearSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FiscalYearSettingList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            var query = from a in _context.tbl_fiscal_year
                        orderby a.fiscal_year descending
                        select new FiscalYearViewModel
                        {
                            fiscal_year = a.fiscal_year,
                            date_from = Convert.ToDateTime(a.date_from),
                            date_to = Convert.ToDateTime(a.date_to),
                            is_active = a.is_active,
                            fiscal_year_abb = a.fiscal_year_abb ?? "",
                            yearly_working_hrs = a.yearly_working_hrs
                        };

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(string.Concat(sortColumn, " ", sortColumnDir));
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    a.fiscal_year.Contains(searchValue) ||
                    (a.fiscal_year_abb != null && a.fiscal_year_abb.Contains(searchValue))
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
        public IActionResult FiscalYearSettingAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10503";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            ViewBag.Status = StatusActivePassive("YNAD");
            /** this is to load blank form while doing add process **/
            if (mode == "add")
            {
                ViewBag.FiscalYear = _settingsServices.GetFYCYOnce("FY");
                var model = new FiscalYearViewModel
                {
                    fiscal_year = "no-fy-yet",
                    date_from = null,
                    date_to = null,
                    is_active = "",
                    fiscal_year_abb = "",
                    yearly_working_hrs = 0
                };
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_FiscalYearSettingAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (from a in _context.tbl_fiscal_year
                               where a.fiscal_year == id
                               select new
                               {
                                   a.fiscal_year,
                                   a.date_from,
                                   a.date_to,
                                   a.is_active,
                                   a.fiscal_year_abb,
                                   a.yearly_working_hrs
                               }).FirstOrDefault();
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new FiscalYearViewModel
                        {
                            fiscal_year = smt.fiscal_year,
                            date_from = Convert.ToDateTime(smt.date_from),
                            date_to = Convert.ToDateTime(smt.date_to),
                            is_active = smt.is_active,
                            fiscal_year_abb = smt.fiscal_year_abb,
                            yearly_working_hrs = smt.yearly_working_hrs
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_FiscalYearSettingAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult FiscalYearSettingSave(FiscalYearViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10503", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            DateTime date_from = Convert.ToDateTime(model.date_from);
            DateTime date_to = Convert.ToDateTime(model.date_to);
            string is_active = model.is_active ?? "";
            string fiscal_year_abb = model.fiscal_year_abb ?? "";
            string fiscal_year = model.fiscal_year;
            int yearly_working_hrs = model.yearly_working_hrs;
            if (mode == "add")
            {
                //check if the data is exits on another record
                var isData = _context.tbl_fiscal_year.FirstOrDefault(u => u.fiscal_year == fiscal_year);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                //update all other to N if current stats = Y
                if (string.Equals(is_active, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    _ = _context.tbl_fiscal_year
                        .ExecuteUpdate(setters => setters
                        .SetProperty(f => f.is_active, "N")
                        );
                    _context.ChangeTracker.Clear();
                }
                var DataSave = new tbl_fiscal_year
                {
                    fiscal_year = fiscal_year,
                    date_from = date_from,
                    date_to = date_to,
                    is_active = is_active,
                    fiscal_year_abb = fiscal_year_abb,
                    yearly_working_hrs = yearly_working_hrs
                };
                _ = _context.tbl_fiscal_year.Add(DataSave);
                _ = _context.SaveChanges();
                if (string.Equals(is_active, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    _settingsServices.SetFiscalYear();/**load new fiscal year on sessions*/
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id = fiscal_year });
            }
            else if (mode == "edit")
            {
                var DataUpdate = _context.tbl_fiscal_year.FirstOrDefault(h => h.fiscal_year == fiscal_year);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                /**update all other to N if current stats = Y **/
                if (string.Equals(is_active, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    _ = _context.tbl_fiscal_year.ExecuteUpdate(setters => setters.SetProperty(f => f.is_active, "N"));
                    _context.ChangeTracker.Clear();
                }
                /** UPDATE if NOT data in timesheet **/
                DataUpdate.date_from = date_from;
                DataUpdate.date_to = date_to;
                DataUpdate.is_active = is_active;
                DataUpdate.fiscal_year_abb = fiscal_year_abb;
                DataUpdate.yearly_working_hrs = yearly_working_hrs;
                _ = _context.tbl_fiscal_year.Update(DataUpdate);
                _ = _context.SaveChanges();
                /**load new fiscal year on sessions */
                if (string.Equals(is_active, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    _settingsServices.SetFiscalYear();
                }
                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.fiscal_year });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FiscalYearSettingDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10503", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            /** Validate input **/
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var item in request.SelectedIds)
            {
                tSel++;
                /** check if any records another sections*/
                bool isDataInCalendar = _settingsServices.GetCalenarDataExist(item);
                var isData = _context.tbl_setting_holidays.FirstOrDefault(u => u.fiscal_year == item);
                if (!isDataInCalendar && isData == null)
                {
                    var smt = _context.tbl_fiscal_year.FirstOrDefault(h => h.fiscal_year == item);
                    if (smt != null)
                    {
                        tDel++;
                        _context.tbl_fiscal_year.RemoveRange(smt);
                        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                        _context.ChangeTracker.Clear();
                    }
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
        #region HOLIDAYS SETTINGS

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Holidays()
        {
            #region FOR PERMISSION
            string PageId = "10506";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_setting_holidays
                join b in _context.tbl_fiscal_year
                on a.fiscal_year equals b.fiscal_year
                orderby a.fiscal_year descending, a.holiday_date descending
                select new HolidaysViewModel
                {
                    id = a.id,
                    holiday_date = Convert.ToDateTime(a.holiday_date),
                    remarks = a.remarks,
                    fiscal_year = a.fiscal_year,
                    fiscal_year_abb = b.fiscal_year_abb ?? ""
                }).ToList();
            ViewBag.FiscalFilter = _settingsServices.GetFiscalYears(HttpContext.Session.GetString("fiscal_year"));
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Settings/Holidays", "ADD|DEL", PageId, Records.Count);
            return PartialView("Settings/_Holidays", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HolidaysList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = from a in _context.tbl_setting_holidays
                        join b in _context.tbl_fiscal_year
                        on a.fiscal_year equals b.fiscal_year
                        orderby a.holiday_date descending
                        select new HolidaysViewModel
                        {
                            id = a.id,
                            holiday_date = Convert.ToDateTime(a.holiday_date),
                            remarks = a.remarks,
                            fiscal_year = a.fiscal_year,
                            fiscal_year_abb = b.fiscal_year_abb ?? ""
                        };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.fiscal_year == StatusFilter);/*filter*/
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                (a.remarks != null && a.remarks.Contains(searchValue)) ||
                (a.fiscal_year != null && a.fiscal_year.Contains(searchValue)) ||
                (a.fiscal_year_abb != null && a.fiscal_year_abb.Contains(searchValue))
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
        public IActionResult HolidaysAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10506";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;
            //HolidaysViewModel model;
            //this is to load blank form while doing add process
            if (mode == "add")
            {
                ViewBag.FiscalYear = HttpContext.Session.GetString("fiscal_year");
                var model = new HolidaysViewModel
                {
                    id = "",
                    holiday_date = null,
                    remarks = "",
                    fiscal_year = HttpContext.Session.GetString("fiscal_year"),
                    fiscal_year_abb = HttpContext.Session.GetString("fiscal_year_abb")
                };
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Settings/_HolidaysAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = (
                        from a in _context.tbl_setting_holidays
                        join b in _context.tbl_fiscal_year
                        on a.fiscal_year equals b.fiscal_year
                        where a.id == id
                        select new
                        {
                            a.id,
                            a.holiday_date,
                            a.remarks,
                            a.fiscal_year,
                            b.fiscal_year_abb
                        }).FirstOrDefault();
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new HolidaysViewModel
                        {
                            id = smt.id,
                            holiday_date = Convert.ToDateTime(smt.holiday_date),
                            remarks = smt.remarks,
                            fiscal_year = smt.fiscal_year,
                            fiscal_year_abb = smt.fiscal_year_abb,
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Settings/_HolidaysAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult HolidaysSave(HolidaysViewModel model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10506", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            DateTime holiday_date = Convert.ToDateTime(model.holiday_date);
            string remarks = model.remarks ?? "";
            string fiscal_year = model.fiscal_year ?? "";

            /**Check the date input is within the fiscal year*/
            string checkWithin = _settingsServices.CheckDateWithinFiscalYear(holiday_date, fiscal_year);
            if (!string.IsNullOrWhiteSpace(checkWithin))
            {
                return Json(new { status = "false", message = checkWithin });
            }
            /**check if holiday set for Weekends */
            string valDay = (holiday_date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? "W" : "O";
            if (valDay == "W")
            {
                return Json(new { status = "false", message = Lang.msg_can_not_set_holiday_weekend });
            }
            if (mode == "add")
            {
                /**check if the data is exits on another record */
                var isData = _context.tbl_setting_holidays.FirstOrDefault(u => u.holiday_date == holiday_date);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                if (!_settingsServices.GetTimesheetDataExist(holiday_date))
                {
                    /**if data not existins in timesheet only then    **/
                    string id = UniqueID();
                    var DataSave = new tbl_setting_holidays
                    {
                        id = id,
                        holiday_date = holiday_date,
                        remarks = remarks,
                        fiscal_year = fiscal_year
                    };
                    _ = _context.tbl_setting_holidays.Add(DataSave);
                    _ = _context.SaveChanges();
                    return Json(new { status = "success", message = Lang.msg_added_success });
                }
                else
                {
                    return Json(new { status = "error", message = Lang.msg_err_holiday_process });
                }
            }
            else if (mode == "edit")
            {
                string? id = Request.Form["id"];
                DateTime h_holiday_date = Convert.ToDateTime(Request.Form["h_holiday_date"]);
                /** check if the data is exits on another record*/
                var isData = _context.tbl_setting_holidays.FirstOrDefault(u => u.holiday_date == holiday_date && u.id != id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_setting_holidays.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                if (!_settingsServices.GetTimesheetDataExist(holiday_date))
                {
                    /** UPDATE if not data in timesheet */
                    DataUpdate.holiday_date = holiday_date;
                    DataUpdate.remarks = remarks;
                    DataUpdate.fiscal_year = fiscal_year;
                    _ = _context.tbl_setting_holidays.Update(DataUpdate);
                    _ = _context.SaveChanges();
                    return Json(new { status = "success", message = Lang.msg_update_success });
                }
                else
                {
                    if (h_holiday_date.Date == holiday_date.Date)
                    {
                        DataUpdate.remarks = remarks;
                        _ = _context.tbl_setting_holidays.Update(DataUpdate);
                        _ = _context.SaveChanges();
                        return Json(new { status = "success", message = Lang.msg_update_success });
                    }
                    else
                    {
                        return Json(new { status = "error", message = Lang.msg_err_holiday_process });
                    }
                }
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HolidaysDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10506", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            /** Validate input**/
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            /**check if any records in timesheet*/
            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var item in request.SelectedIds)
            {
                tSel++;
                var smt = _context.tbl_setting_holidays.FirstOrDefault(h => h.id == item);
                if (smt != null)
                {
                    DateTime holiday_date = Convert.ToDateTime(smt.holiday_date);
                    if (!_settingsServices.GetTimesheetDataExist(holiday_date))
                    {
                        tDel++;
                        _context.tbl_setting_holidays.RemoveRange(smt);
                        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
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
        #region CALENDAR SETTINGS

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult CalendarSetting()
        {
            #region FOR PERMISSION
            string PageId = "10502";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_calendar_setting
                orderby a.cal_year descending, a.cal_month descending
                select new CalendarSettingViewModel
                {
                    cal_id = a.cal_id,
                    cal_month = a.cal_month,
                    cal_year = a.cal_year,
                    d1 = a.d1,
                    d2 = a.d2,
                    d3 = a.d3,
                    d4 = a.d4,
                    d5 = a.d5,
                    d6 = a.d6,
                    d7 = a.d7,
                    d8 = a.d8,
                    d9 = a.d9,
                    d10 = a.d10,
                    d11 = a.d11,
                    d12 = a.d12,
                    d13 = a.d13,
                    d14 = a.d14,
                    d15 = a.d15,
                    d16 = a.d16,
                    d17 = a.d17,
                    d18 = a.d18,
                    d19 = a.d19,
                    d20 = a.d20,
                    d21 = a.d21,
                    d22 = a.d22,
                    d23 = a.d23,
                    d24 = a.d24,
                    d25 = a.d25,
                    d26 = a.d26,
                    d27 = a.d27,
                    d28 = a.d28,
                    d29 = a.d29,
                    d30 = a.d30,
                    d31 = a.d31
                }).ToList();
            ViewBag.PayPeriod = ToProperCase(_globalOptionServices.OptionServices["op_timesheet_type"]);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("General/CalendarSetting", "ADD|DEL", PageId, Records.Count);
            return PartialView("Settings/_CalendarSetting", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalendarSettingList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_calendar_setting
                    .OrderByDescending(a => a.cal_year)
                    .ThenByDescending(a => a.cal_month)
                    .Select(a => new CalendarSettingViewModel
                    {
                        cal_id = a.cal_id,
                        cal_month = a.cal_month,
                        cal_year = a.cal_year,
                        d1 = a.d1,
                        d2 = a.d2,
                        d3 = a.d3,
                        d4 = a.d4,
                        d5 = a.d5,
                        d6 = a.d6,
                        d7 = a.d7,
                        d8 = a.d8,
                        d9 = a.d9,
                        d10 = a.d10,
                        d11 = a.d11,
                        d12 = a.d12,
                        d13 = a.d13,
                        d14 = a.d14,
                        d15 = a.d15,
                        d16 = a.d16,
                        d17 = a.d17,
                        d18 = a.d18,
                        d19 = a.d19,
                        d20 = a.d20,
                        d21 = a.d21,
                        d22 = a.d22,
                        d23 = a.d23,
                        d24 = a.d24,
                        d25 = a.d25,
                        d26 = a.d26,
                        d27 = a.d27,
                        d28 = a.d28,
                        d29 = a.d29,
                        d30 = a.d30,
                        d31 = a.d31
                    });
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
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
        public IActionResult CalendarSettingAddEdit(int? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10502";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            CalendarSettingViewModel model;
            /** this is to load blank form while doing add process **/
            model = new CalendarSettingViewModel();
            if (mode == "add")
            {
                ViewBag.CalYears = _settingsServices.GetYears(0);
                ViewBag.CalMonth = _settingsServices.GetMonths(0);
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A");
                return PartialView("Settings/_CalendarSettingAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id is < 1 or null)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_calendar_setting.FirstOrDefault(h => h.cal_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new CalendarSettingViewModel
                        {
                            cal_id = Convert.ToInt32(smt.cal_id),
                            cal_month = Convert.ToByte(smt.cal_month),
                            cal_year = Convert.ToInt16(smt.cal_year),
                            d1 = smt.d1,
                            d2 = smt.d2,
                            d3 = smt.d3,
                            d4 = smt.d4,
                            d5 = smt.d5,
                            d6 = smt.d6,
                            d7 = smt.d7,
                            d8 = smt.d8,
                            d9 = smt.d9,
                            d10 = smt.d10,
                            d11 = smt.d11,
                            d12 = smt.d12,
                            d13 = smt.d13,
                            d14 = smt.d14,
                            d15 = smt.d15,
                            d16 = smt.d16,
                            d17 = smt.d17,
                            d18 = smt.d18,
                            d19 = smt.d19,
                            d20 = smt.d20,
                            d21 = smt.d21,
                            d22 = smt.d22,
                            d23 = smt.d23,
                            d24 = smt.d24,
                            d25 = smt.d25,
                            d26 = smt.d26,
                            d27 = smt.d27,
                            d28 = smt.d28,
                            d29 = smt.d29,
                            d30 = smt.d30,
                            d31 = smt.d31
                        };
                        ViewBag.CalYears = _settingsServices.GetYears(smt.cal_year);
                        ViewBag.CalMonth = _settingsServices.GetMonths(smt.cal_month);
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E");
                        return PartialView("Settings/_CalendarSettingAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpGet]
        public IActionResult CalendarSettingDays(short cal_year, byte cal_month)
        {
            string outputCalendarDays = _settingsServices.GetCalendarDays(cal_year, cal_month);
            return Json(outputCalendarDays);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CalendarSettingSave(CalendarSettingViewModel model)
        {
            _ = ModelState.Remove("cal_id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10502", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            byte cal_month = Convert.ToByte(model.cal_month);
            short cal_year = Convert.ToInt16(model.cal_year);
            string d1 = model.d1 ?? "";
            string d2 = model.d2 ?? "";
            string d3 = model.d3 ?? "";
            string d4 = model.d4 ?? "";
            string d5 = model.d5 ?? "";
            string d6 = model.d6 ?? "";
            string d7 = model.d7 ?? "";
            string d8 = model.d8 ?? "";
            string d9 = model.d9 ?? "";
            string d10 = model.d10 ?? "";
            string d11 = model.d11 ?? "";
            string d12 = model.d12 ?? "";
            string d13 = model.d13 ?? "";
            string d14 = model.d14 ?? "";
            string d15 = model.d15 ?? "";
            string d16 = model.d16 ?? "";
            string d17 = model.d17 ?? "";
            string d18 = model.d18 ?? "";
            string d19 = model.d19 ?? "";
            string d20 = model.d20 ?? "";
            string d21 = model.d21 ?? "";
            string d22 = model.d22 ?? "";
            string d23 = model.d23 ?? "";
            string d24 = model.d24 ?? "";
            string d25 = model.d25 ?? "";
            string d26 = model.d26 ?? "";
            string d27 = model.d27 ?? "";
            string d28 = model.d28 ?? "";
            string d29 = model.d29 ?? "";
            string d30 = model.d30 ?? "";
            string d31 = model.d31 ?? "";

            if (mode == "add")
            {
                var isData = _context.tbl_calendar_setting.FirstOrDefault(u => u.cal_year == cal_year && u.cal_month == cal_month);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                int cal_id = (_context.tbl_calendar_setting.Any() ? _context.tbl_calendar_setting.Max(o => o.cal_id) : 0) + 1;
                var DataSave = new tbl_calendar_setting
                {
                    cal_id = cal_id,
                    cal_month = cal_month,
                    cal_year = cal_year,
                    d1 = d1,
                    d2 = d2,
                    d3 = d3,
                    d4 = d4,
                    d5 = d5,
                    d6 = d6,
                    d7 = d7,
                    d8 = d8,
                    d9 = d9,
                    d10 = d10,
                    d11 = d11,
                    d12 = d12,
                    d13 = d13,
                    d14 = d14,
                    d15 = d15,
                    d16 = d16,
                    d17 = d17,
                    d18 = d18,
                    d19 = d19,
                    d20 = d20,
                    d21 = d21,
                    d22 = d22,
                    d23 = d23,
                    d24 = d24,
                    d25 = d25,
                    d26 = d26,
                    d27 = d27,
                    d28 = d28,
                    d29 = d29,
                    d30 = d30,
                    d31 = d31
                };
                _ = _context.tbl_calendar_setting.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = cal_id });
            }
            else if (mode == "edit")
            {
                int cal_id = model.cal_id;
                /** check if the data is exits on another record */
                var isData = _context.tbl_calendar_setting.FirstOrDefault(u => u.cal_year == cal_year && u.cal_month == cal_month && u.cal_id != cal_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_calendar_setting.FirstOrDefault(h => h.cal_id == cal_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.cal_month = cal_month;
                DataUpdate.cal_year = cal_year;
                DataUpdate.d1 = d1;
                DataUpdate.d2 = d2;
                DataUpdate.d3 = d3;
                DataUpdate.d4 = d4;
                DataUpdate.d5 = d5;
                DataUpdate.d6 = d6;
                DataUpdate.d7 = d7;
                DataUpdate.d8 = d8;
                DataUpdate.d9 = d9;
                DataUpdate.d10 = d10;
                DataUpdate.d11 = d11;
                DataUpdate.d12 = d12;
                DataUpdate.d13 = d13;
                DataUpdate.d14 = d14;
                DataUpdate.d15 = d15;
                DataUpdate.d16 = d16;
                DataUpdate.d17 = d17;
                DataUpdate.d18 = d18;
                DataUpdate.d19 = d19;
                DataUpdate.d20 = d20;
                DataUpdate.d21 = d21;
                DataUpdate.d22 = d22;
                DataUpdate.d23 = d23;
                DataUpdate.d24 = d24;
                DataUpdate.d25 = d25;
                DataUpdate.d26 = d26;
                DataUpdate.d27 = d27;
                DataUpdate.d28 = d28;
                DataUpdate.d29 = d29;
                DataUpdate.d30 = d30;
                DataUpdate.d31 = d31;

                _ = _context.tbl_calendar_setting.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.cal_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalendarSettingDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10502", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            /**  Validate input **/
            if (request?.SelectedIds == null || request.SelectedIds.Count <= 0)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            /** check if any records in timesheet**/
            int tSel = 0; int tDel = 0; int tUDel = 0;
            foreach (var item in request.SelectedIds)
            {
                short cal_year = 0;
                byte cal_month = 0;
                tSel++;
                var smt = _context.tbl_calendar_setting.FirstOrDefault(h => h.cal_id == Convert.ToInt32(item));
                if (smt != null)
                {
                    cal_year = smt.cal_year;
                    cal_month = smt.cal_month;
                }
                var recordsExist = _context.tbl_employee_timesheet_sub.FirstOrDefault(u => u.emp_year == cal_year && u.emp_month == cal_month);
                if (recordsExist == null)
                {
                    /** try to delete. matching records **/
                    var recordsToDelete = await _context.tbl_calendar_setting.Where(r => r.cal_id == Convert.ToInt32(item)).FirstOrDefaultAsync().ConfigureAwait(false);
                    if (recordsToDelete != null)
                    {
                        tDel++;
                        _context.tbl_calendar_setting.RemoveRange(recordsToDelete);
                        _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    }
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
        #region ADMINISTRATORS
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Administrators()
        {
            string PageId = "10501";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.EmployeeList = _employeeServices.GetEmployeeListBoth();
            EmployeeAdministratorViewModel model;

            var Records = _context.tbl_employee_administrator.FirstOrDefault();
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new EmployeeAdministratorViewModel
                {
                    id = 0,
                    cra = 0,
                    doo = 0,
                    faa = 0,
                    aca = 0,
                    hra = 0,
                    rca = 0,
                    t_t_a_1 = 0,
                    t_t_a_2 = 0,
                    t_a_s_1 = 0,
                    t_a_s_2 = 0,
                    t_a_s_3 = 0,
                    t_a_s_4 = 0,
                    acr = 0,
                    t_t_a_3 = 0,
                    t_t_a_4 = 0,
                    t_t_a_5 = 0,
                    t_a_s_5 = 0,
                    ahr = 0
                };
            }
            else
            {
                ViewBag.mode = "edit";
                model = new EmployeeAdministratorViewModel
                {
                    id = Records.id,
                    cra = Convert.ToInt32(Records.cra),
                    doo = Convert.ToInt32(Records.doo),
                    faa = Convert.ToInt32(Records.faa),
                    aca = Convert.ToInt32(Records.aca),
                    hra = Convert.ToInt32(Records.hra),
                    rca = Convert.ToInt32(Records.rca),
                    t_t_a_1 = Convert.ToInt32(Records.t_t_a_1),
                    t_t_a_2 = Convert.ToInt32(Records.t_t_a_2),
                    t_a_s_1 = Convert.ToInt32(Records.t_a_s_1),
                    t_a_s_2 = Convert.ToInt32(Records.t_a_s_2),
                    t_a_s_3 = Convert.ToInt32(Records.t_a_s_3),
                    t_a_s_4 = Convert.ToInt32(Records.t_a_s_4),
                    acr = Convert.ToInt32(Records.acr),
                    t_t_a_3 = Convert.ToInt32(Records.t_t_a_3),
                    t_t_a_4 = Convert.ToInt32(Records.t_t_a_4),
                    t_t_a_5 = Convert.ToInt32(Records.t_t_a_5),
                    t_a_s_5 = Convert.ToInt32(Records.t_a_s_5),
                    ahr = Convert.ToInt32(Records.ahr)
                };

            }
            return PartialView("Settings/_Administrators", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AdministratorsSave(EmployeeAdministratorViewModel model)
        {
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10501", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            short id = model.id;
            int cra = Convert.ToInt32(model.cra);
            int acr = Convert.ToInt32(model.acr);
            int doo = Convert.ToInt32(model.doo);
            int faa = Convert.ToInt32(model.faa);
            int aca = Convert.ToInt32(model.aca);
            int hra = Convert.ToInt32(model.hra);
            int ahr = Convert.ToInt32(model.ahr);
            int rca = Convert.ToInt32(model.rca);
            int t_t_a_1 = Convert.ToInt32(model.t_t_a_1);
            int t_t_a_2 = Convert.ToInt32(model.t_t_a_2);
            int t_t_a_3 = Convert.ToInt32(model.t_t_a_3);
            int t_t_a_4 = Convert.ToInt32(model.t_t_a_4);
            int t_t_a_5 = Convert.ToInt32(model.t_t_a_5);
            int t_a_s_1 = Convert.ToInt32(model.t_a_s_1);
            int t_a_s_2 = Convert.ToInt32(model.t_a_s_2);
            int t_a_s_3 = Convert.ToInt32(model.t_a_s_3);
            int t_a_s_4 = Convert.ToInt32(model?.t_a_s_4);
            int t_a_s_5 = Convert.ToInt32(model?.t_a_s_5);
            /** ADD NEW */
            if (mode == "add")
            {
                id = 1;
                var DataSave = new tbl_employee_administrator
                {
                    id = id,
                    cra = cra,
                    doo = doo,
                    faa = faa,
                    aca = aca,
                    hra = hra,
                    rca = rca,
                    ahr = ahr,
                    t_t_a_1 = t_t_a_1,
                    t_t_a_2 = t_t_a_2,
                    t_t_a_3 = t_t_a_3,
                    t_t_a_4 = t_t_a_4,
                    t_t_a_5 = t_t_a_5,
                    t_a_s_1 = t_a_s_1,
                    t_a_s_2 = t_a_s_2,
                    t_a_s_3 = t_a_s_3,
                    t_a_s_4 = t_a_s_4,
                    t_a_s_5 = t_a_s_5,
                    acr = acr
                };
                _ = _context.tbl_employee_administrator.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success });
            }
            else if (mode == "edit")
            {
                id = model.id;
                var DataUpdate = _context.tbl_employee_administrator.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.cra = cra;
                DataUpdate.acr = acr;
                DataUpdate.doo = doo;
                DataUpdate.faa = faa;
                DataUpdate.aca = aca;
                DataUpdate.hra = hra;
                DataUpdate.ahr = ahr;
                DataUpdate.rca = rca;
                DataUpdate.t_t_a_1 = t_t_a_1;
                DataUpdate.t_t_a_2 = t_t_a_2;
                DataUpdate.t_t_a_3 = t_t_a_3;
                DataUpdate.t_t_a_4 = t_t_a_4;
                DataUpdate.t_t_a_5 = t_t_a_5;
                DataUpdate.t_a_s_1 = t_a_s_1;
                DataUpdate.t_a_s_2 = t_a_s_2;
                DataUpdate.t_a_s_3 = t_a_s_3;
                DataUpdate.t_a_s_4 = t_a_s_4;
                DataUpdate.t_a_s_5 = t_a_s_5;
                _ = _context.tbl_employee_administrator.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/

    }
}
