using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Models.General;
using wwfpp.Models.Request;
using wwfpp.Services;
using static GblUtilities;

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
        public RequestController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            EmailService emailService,
            GlobalOptionServices globalOptionServices,
            EmployeeServices employeeServices,
            SettingsServices settingsServices,
            AccountServices accountServices,
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
        }

        #region Employee Medical Insurance
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


        public IActionResult Index()
        {
            return View();
        }
    }
}
