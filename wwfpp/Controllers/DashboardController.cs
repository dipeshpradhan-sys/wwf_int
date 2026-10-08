using Azure.Core;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.NetworkInformation;
using System.Web;
using wwf_pp.Services;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Models.Personnel;
using wwfpp.Services;
using static GblUtilities;
using static System.Net.Mime.MediaTypeNames;

namespace wwfpp.Controllers;

public class DashboardController : Controller
{
    private readonly EmailService _emailSender;
    private readonly DashboardService _dashboardService;
    private readonly AppDbContext _context;
    private readonly EmployeeServices _employeeServices;
    private readonly AppSettings _appSettings;
    private readonly RequestServices _requestServices;
    private readonly AdministrationEmailService _administrationEmailService;
    private readonly EmailService _emailService;
    private readonly SettingsServices _settingsServices;

    public DashboardController(AppDbContext context, EmailService emailSender, DashboardService dashboardService, EmployeeServices employeeServices, IOptions<AppSettings> appSettings, RequestServices requestServices, AdministrationEmailService administrationEmailService, EmailService emailService, SettingsServices settingsServices)
    {
        _context = context;
        _emailSender = emailSender;
        _dashboardService = dashboardService;
        _employeeServices = employeeServices;
        _appSettings = appSettings.Value;
        _requestServices = requestServices;
        _administrationEmailService = administrationEmailService;
        _emailService = emailService;
        _settingsServices = settingsServices;
    }

    public IActionResult TimesheetToSupervisor()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        string fiscalYear = HttpContext.Session.GetString("fiscal_year");

        var model = _dashboardService.GetSupervisorTimesheets(employeeId, fiscalYear);

        return PartialView("Dashboard/_TimesheetToSupervisor", model);
    }
    
    public async Task<IActionResult> TimesheetToMe()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        string fiscalYear = HttpContext.Session.GetString("fiscal_year");

        var model = await _dashboardService.GetTimesheetsToMe(employeeId, fiscalYear);
        ViewData["ModeTypeForAuthority"] = "Dashboard";
        return PartialView("Dashboard/_TimesheetToMe", model);
    }

    public async Task<IActionResult> LeaveToMe()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

        var model = await _dashboardService.GetLeaveToMe(employeeId, fiscalStartDate, fiscalEndDate);

        return PartialView("Dashboard/_LeaveToMe", model);
    }

    public async Task<IActionResult> LeaveToSupervisior()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));
        var model = await _dashboardService.GetSupervisorLeave(employeeId, fiscalStartDate, fiscalEndDate);

        return PartialView("Dashboard/_LeaveToSupervisor", model);
    }

    public async Task<IActionResult> TravelToMe(string? parm_whos_list)
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

        var model = await _dashboardService.GetTravelToMe(employeeId, fiscalStartDate, fiscalEndDate, parm_whos_list);

        return PartialView("Dashboard/_TravelToMe", model);
    }

    public async Task<IActionResult> TravelToSupervisior()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));
        // Call service for each list
        var mpt = await _dashboardService.GetTravelToSupervisor(employeeId, fiscalStartDate, fiscalEndDate, "MPT");
        var tcs = await _dashboardService.GetTravelToSupervisor(employeeId, fiscalStartDate, fiscalEndDate, "TCS");
        var rpt = await _dashboardService.GetTravelToSupervisor(employeeId, fiscalStartDate, fiscalEndDate, "RPT");

        var vm = new TravelDashboardOverviewVM
        {
            MyPendingTravel = mpt,
            TravelCancellationSent = tcs,
            RecentTravel = rpt
        };

        return PartialView("Dashboard/_TravelToSupervisor", vm);
    }

    public async Task<IActionResult> OvertimeToMe()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        //DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.DateFrom));
        //DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.DateTo));

        var model = await _dashboardService.GetOvertimeToMe(employeeId);

        return PartialView("Dashboard/_OvertimeToMe", model);
    }

    public async Task<IActionResult> OvertimeToSupervisior()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        //DateTime fiscalStartDate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.DateFrom));
        //DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString(SessionKeys.DateTo));

        var model = await _dashboardService.GetSupervisorOvertime(employeeId);

        return PartialView("Dashboard/_OvertimeToSupervisor", model);
    }

    public async Task<IActionResult> FutureLeaveToMe()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

        var model = await _dashboardService.GetLeaveFutureToMe(employeeId, fiscalEndDate);

        return PartialView("Dashboard/_LeaveFutureToMe", model);
    }

    public async Task<IActionResult> FutureLeaveToSupervisior()
    {
        int employeeId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
        DateTime fiscalEndDate = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

        var model = await _dashboardService.GetSupervisorFutureLeave(employeeId, fiscalEndDate);

        return PartialView("Dashboard/_LeaveFutureToSupervisor", model);
    }
    #region ALL APPROVED TRAVEL REQUEST(S)
    public async Task<IActionResult> DashboardTravelApprovedAccount(int? cmbMeEmployee, int? emp_travel_id, string cmbMark = "unmarkonly", string saveValue = "")
    {
        int empId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));

        if (saveValue == "checked")
        {
            int? acc_app_by = empId;
            string? acc_app_by_post = _context.tbl_employee.Where(h => h.emp_id == acc_app_by).Select(h => h.post).FirstOrDefault();

            int? adv_app_by = _context.tbl_employee_administrator.Where(h => h.id == 1).Select(h => h.doo).FirstOrDefault();
            string? adv_app_by_post = "Director of Operations";

            _dashboardService.GetSaveChecked(emp_travel_id, acc_app_by, adv_app_by, acc_app_by_post, adv_app_by_post);
        }
        if (saveValue == "unchecked")
        {
            _dashboardService.GetDeleteChecked(emp_travel_id);
        }

        var dateFrom = HttpContext.Session.GetString("date_from");
        var dateTo = HttpContext.Session.GetString("date_to");

        IQueryable<tbl_employee_travel_main> query = _context.tbl_employee_travel_main
            .Where(t => t.app_status == "Approved" && (t.can_by == null || t.can_by > 0));

        if (cmbMark == "markonly")
        {
            query = query.Where(t => _context.tbl_employee_travel_printed
                .Any(p => p.emp_travel_id == t.emp_travel_id &&
                            (cmbMeEmployee == 0 || p.acc_app_by == cmbMeEmployee)));
        }
        else
        {
            query = query.Where(t => !_context.tbl_employee_travel_printed
                .Any(p => p.emp_travel_id == t.emp_travel_id));
        }

        if (!string.IsNullOrEmpty(dateFrom) && !string.IsNullOrEmpty(dateTo))
        {
            var df = DateTime.Parse(dateFrom);
            var dt = DateTime.Parse(dateTo);
            query = query.Where(t => t.date_from >= df && t.date_from <= dt);
        }

        var rawTravels = await query
            .OrderByDescending(t => t.emp_travel_id)
            .ToListAsync();

        var travels = rawTravels.Select(t => new DashboardTravelApprovedAccountViewModel
        {
            emp_travel_id = t.emp_travel_id,
            emp_id = t.emp_id,
            EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(t.emp_id)),
            travel_type = t.travel_type,
            destinations = t.destinations,
            date_from = t.date_from,
            date_to = t.date_to,
            submit_date = t.submit_date,
            IsMarked = _context.tbl_employee_travel_printed.Any(p => p.emp_travel_id == t.emp_travel_id),
            ActionByID = _context.tbl_employee_travel_printed
                .Where(p => p.emp_travel_id == t.emp_travel_id)
                .Select(p => p.acc_app_by)
                .FirstOrDefault(),
            ActionByName = _employeeServices.GetEmployeeName(
                        _context.tbl_employee_travel_printed
                            .Where(p => p.emp_travel_id == t.emp_travel_id)
                            .Select(p => p.acc_app_by)
                            .FirstOrDefault() ?? 0
                    )
        }).ToList();

        ViewBag.CmbMark = cmbMark;
        ViewBag.CmbMeEmployee = empId;

        return PartialView("Dashboard/_DashboardTravelApprovedAccount", travels);
    }
    #endregion
    #region UNSETTLED TRAVEL(S), TRAVEL SAVED FOR SETTLEMENT & TRAVEL SUBMITTED FOR SETTLEMENT
    public IActionResult DashboardTravelSettlement(string ? travelStatus = null)
    {
        ViewBag.travelStatus = travelStatus ?? "U";
        return PartialView("Dashboard/_DashboardTravelSettlement", "");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DashboardTravelSettlementList([FromForm] MultipleCostumFilterRequest request)
    {
        var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

        //string travelType= request.FilterValue2;
        string SettlementStatusFilter = request.FilterValue1 ?? "";

        string FiscalYearActive = HttpContext.Session.GetString("fiscal_year") ?? "";
        int emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
        string start_fiscal_date = _settingsServices.GetFiscalYearValue(FiscalYearActive, "date_from") ?? "";
        string end_fiscal_date = _settingsServices.GetFiscalYearValue(FiscalYearActive, "date_to") ?? "";
        string emp_status = _employeeServices.GetEmployeeStatus(emp_id);
        var startFiscalDate = DateTime.TryParse(start_fiscal_date, out var DF) ? DF : DateTime.MinValue;
        var endFiscalDate = DateTime.TryParse(end_fiscal_date, out var DE) ? DE : DateTime.MinValue;

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
                    "U",
                SettlementTypeDetail =
                    settlement == null ? "Unsettled" :
                    settlement.app_status == "T" ? "Saved" :
                    settlement.app_status == "P" && settlement.is_for_set == "Y" ? "Submitted" :
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
    #endregion
    #region EMPLOYEE CONTRACT EXPIRY NOTIFICATION
    public IActionResult DashboardContractExpiry()
    {
        return PartialView("Dashboard/_DashboardContractExpiry", "");
    }
    public IActionResult DashboardContractExpiryList([FromForm] MultipleCostumFilterRequest request)
    {
        // Step 1: Update expired contracts
        var expiredContracts = _context.tbl_employee_contract
            .Where(c => c.contract_status == "A" && c.end_date < DateTime.Now)
            .ToList();

        foreach (var contract in expiredContracts)
        {
            contract.contract_status = "D";
        }
        if (expiredContracts.Any())
        {
            _context.SaveChanges();
        }

        var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

        string ContractStatusFilter = "A";
        string EmployeeStatusFilter = "A";

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
    #endregion
    /*public string ListContractAlert1(string parm)
    {
        int rCnt = 0;
        string fnStrStart;
        var fnStrLoop = new System.Text.StringBuilder();

        // Step 1: Automatic update expired contracts
        var expiredContracts = _context.tbl_employee_contract
            .Where(c => c.contract_status == "A" && c.end_date < DateTime.Now)
            .ToList();

        if (expiredContracts.Any())
        {
            foreach (var contract in expiredContracts)
            {
                contract.contract_status = "D";
            }
            _context.SaveChanges();
        }

        // Step 2: Get list of active contracts
        var contracts = (from a in _context.tbl_employee_contract
                         join b in _context.tbl_contract_document_template
                         on a.contract_document_id equals b.contract_document_id
                         where a.contract_status == "A"
                         orderby a.emp_id ascending, a.contract_status ascending, a.issue_date descending
                         select new
                         {
                             a.emp_contract_id,
                             a.contract_document_id,
                             b.document_subject,
                             a.emp_id,
                             a.issue_date,
                             a.end_date,
                             a.contract_status
                         }).ToList();

        if (contracts.Any())
        {
            fnStrStart = parm == "email"
                ? "<div style=\"width:100%;\">\n"
                : "<div class=\"f-left w-100\">\n";

            fnStrStart = @" <table width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" bgcolor=""#ffffff"" style=""text-align:left;"">
          <tr bgcolor=""#cccccc"">
             <td width=""30%"" class=""title"" style=""padding-left:5px;""><b>Contract Subject</b></td>
             <td width=""15%"" class=""title"" style=""padding-left:5px;""><b>Issue Date</b></td>
             <td width=""15%"" class=""title"" style=""padding-left:5px;""><b>End Date</b></td>
             <td width=""10%"" class=""title"" style=""padding-left:5px;""><b>Status</b></td>
          </tr>";

            int? fnOldEmpId = null;

            foreach (var contract in contracts)
            {
                int? fnEmpId = contract.emp_id;
                string? empName = _employeeServices.GetEmployeeName((int)fnEmpId);

                string contractStatus = contract.contract_status == "A" ? "Active" : "Inactive";
                string issueDateStr = contract.issue_date.HasValue ? contract.issue_date.Value.ToString("yyyy-MM-dd") : string.Empty;
                string endDateStr = contract.end_date.HasValue ? contract.end_date.Value.ToString("yyyy-MM-dd") : string.Empty;

                string trBgColor = "#eeeeee";
                string isShow = "";
                int isProvision = 0;

                if (contract.end_date.HasValue && contract.issue_date.HasValue)
                {
                    isProvision = (contract.end_date.Value - contract.issue_date.Value).Days + 1;
                }

                int? endDateDiff = contract.end_date.HasValue ? (contract.end_date.Value - DateTime.Now).Days + 1 : null;

                if (isProvision < 33)
                {
                    if (endDateDiff < 1)
                    {
                        trBgColor = "#ffc2a6"; // expired
                        isShow = "yes";
                    }
                    else if (endDateDiff > 0 && endDateDiff < 15)
                    {
                        trBgColor = "#ffe8dd"; // near expiry
                        isShow = "yes";
                    }
                }
                else
                {
                    if (endDateDiff < 1)
                    {
                        trBgColor = "#ffc2a6";
                        isShow = "yes";
                    }
                    else if (endDateDiff > 0 && endDateDiff < 45)
                    {
                        trBgColor = "#ffe8dd";
                        isShow = "yes";
                    }
                }

                if (isShow == "yes")
                {
                    rCnt++;
                    if (fnOldEmpId != fnEmpId)
                    {
                        fnStrLoop.AppendLine($"<tr bgcolor=\"#ededed\"><td colspan=\"4\" height=\"20\" style=\"padding-left:5px;\">Employee: {System.Net.WebUtility.HtmlEncode(empName)}</td></tr>");
                    }

                    fnStrLoop.AppendLine($"<tr bgcolor=\"{trBgColor}\">");
                    fnStrLoop.AppendLine($"<td class=\"normal\" style=\"padding-left:5px;\">{System.Net.WebUtility.HtmlEncode(contract.document_subject)}</td>");
                    fnStrLoop.AppendLine($"<td class=\"normal\" style=\"padding-left:5px;\">{issueDateStr}</td>");
                    fnStrLoop.AppendLine($"<td class=\"normal\" style=\"padding-left:5px;\">{endDateStr}</td>");
                    fnStrLoop.AppendLine($"<td class=\"normal\" style=\"padding-left:5px;\">{contractStatus}</td>");
                    fnStrLoop.AppendLine("</tr>");
                    fnStrLoop.AppendLine("<tr bgcolor=\"#ffffff\"><td colspan=\"4\" height=\"20\">&nbsp;</td></tr>");
                }

                fnOldEmpId = fnEmpId;
            }

            if (parm == "email")
            {
                if (fnStrLoop.Length > 0)
                {
                    string? str_from = _requestServices.GetApplicationSetting("op_system_admin_email");

                    var emails = _administrationEmailService.GetAdministrationEmailsAsync().Result;
                    string? str_to = emails["hra"].Email;
                    string? str_cc = "";

                    string str_subject = Lang.EMAIL_EMPLOYEE_CONTRACT_NTR_SUBJECT
                        .Replace("<[SITE-TITLE]>", _appSettings.SITE_TITLE);

                    string strMessageArea1 = fnStrStart + fnStrLoop + "</table></div>";

                    string strMessage = Lang.EMAIL_EMPLOYEE_CONTRACT_NTR_MESSAGE
                        .Replace("<[STR-MESSAGE]>", strMessageArea1)
                        .Replace("<[SITE-TITLE]>", _appSettings.SITE_TITLE)
                        .Replace("<[SITE-ADMIN-NAME]>", Lang.SITE_ADMIN_NAME);

                    _emailService.SendEmail(str_from, str_to, str_subject, strMessage, null, str_cc, null, null, null);
                    return strMessage;
                }
                else
                {
                    // No records found for email mode
                    return "<div class=\"normal\">No records found</div>";
                }
            }
            else
            {
                string strMessageArea1 = fnStrLoop.Length > 0
                    ? fnStrStart + fnStrLoop + "</table></div>\n"
                    : "<div class=\"normal\">No records found</div>";

                return strMessageArea1;
            }
        }
        else
        {
            // No contracts at all
            return "<div class=\"normal\">No records found</div>";
        }
    }*/


    /*public string GetApprovedTravelSettlementSubmitedList()
    {
        DateTime parm_date_from = Convert.ToDateTime(HttpContext.Session.GetString("date_from"));
        DateTime parm_date_to = Convert.ToDateTime(HttpContext.Session.GetString("date_to"));

        var fnStr = new System.Text.StringBuilder();

        // Query: top 100 pending settlements
        var settlements = _context.que_employee_travel_settlement_main
            .Where(s => s.app_status == "P")
            .OrderByDescending(s => s.trav_set_id)
            .Take(100)
            .ToList();

        if (settlements.Any())
        {
            fnStr.AppendLine("<div class=\"f-left w-100\">");
            fnStr.AppendLine("<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"1\" bgcolor=\"#B1B1B1\">");
            fnStr.AppendLine("<tr>");
            fnStr.AppendLine("<td bgcolor=\"#FFFFFF\" align=\"center\" class=\"normal\">");
            fnStr.AppendLine("<table width=\"100%\" border=\"0\" cellpadding=\"4\" cellspacing=\"2\" class=\"normal\">");
            fnStr.AppendLine("<tr bgcolor=\"#CCCCCC\">");
            fnStr.AppendLine($"<td width=\"5%\" class=\"title center\" height=\"25\">S.N</td>");
            fnStr.AppendLine($"<td width=\"20%\" class=\"title\">Employee Name</td>");
            fnStr.AppendLine($"<td width=\"8%\" class=\"title\">Travel Type</td>");
            fnStr.AppendLine($"<td width=\"20%\" class=\"title\">Destinations</td>");
            fnStr.AppendLine($"<td width=\"9%\" class=\"title\">Travel Date</td>");
            fnStr.AppendLine($"<td width=\"9%\" class=\"title\">Return Date</td>");
            fnStr.AppendLine($"<td width=\"10%\" class=\"title\">Submitted Date</td>");
            fnStr.AppendLine($"<td width=\"7%\" class=\"title\">Status</td>");
            fnStr.AppendLine($"<td width=\"5%\" class=\"title\">Set As</td>");
            fnStr.AppendLine($"<td width=\"7%\" class=\"title\">Action</td>");
            fnStr.AppendLine("</tr>");

            int f = 0;
            foreach (var row in settlements)
            {
                f++;
                int empId = row.emp_id;
                string employeeName = _employeeServices.GetEmployeeName(empId);
                string travSetId = row.trav_set_id;
                int empTravelId = row.emp_travel_id;
                string destinations = row.destinations ?? string.Empty;
                string travelType = row.travel_type ?? string.Empty;

                string travelDate = row.travel_date.HasValue ? row.travel_date.Value.ToString("dd/MM/yyyy") : string.Empty;
                string returnDate = row.return_date.HasValue ? row.return_date.Value.ToString("dd/MM/yyyy") : string.Empty;
                string submitDate = row.submit_date.HasValue ? row.submit_date.Value.ToString("dd/MM/yyyy") : string.Empty;

                string appStatus = row.app_status == "P" ? "Pending" : row.app_status;
                string isForSet = row.is_for_set ?? string.Empty;

                fnStr.AppendLine("<tr bgcolor=\"#EEEEEE\" onMouseOver=\"this.style.backgroundColor='#E1EAFE'\" onMouseOut=\"this.style.backgroundColor='#EEEEEE'\">");
                fnStr.AppendLine($"<td align=\"center\">{f}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{System.Net.WebUtility.HtmlEncode(employeeName)}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{System.Net.WebUtility.HtmlEncode(travelType)}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{System.Net.WebUtility.HtmlEncode(destinations)}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{travelDate}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{returnDate}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{submitDate}</td>");
                fnStr.AppendLine($"<td class=\"normal left\">{appStatus}</td>");
                fnStr.AppendLine(
                    $"<td class=\"normal left\">" +
                    $"<a href=\"javascript:postdata('request/employee_travel_settlement_app.asp?mode=verify" +
                    $"&travsettleid={HttpUtility.UrlEncode(travSetId.ToString())}" +
                    $"&emp_travel_id={HttpUtility.UrlEncode(empTravelId.ToString())}" +
                    $"&emp_id={HttpUtility.UrlEncode(empId.ToString())}')\">Verified</a></td>"
                );
                fnStr.AppendLine("<td class=\"normal center\">");
                fnStr.AppendLine($"<a href=\"javascript:postdata('request/employee_travel_settlement_add_edit.asp?mode=edit&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/edit.png\" width=\"16\" height=\"16\" border=\"0\"></a>&nbsp;");

                if (isForSet == "Y")
                {
                    fnStr.AppendLine($"<a href=\"javascript:PopUpW('request/employee_travel_settlement_print.asp?mode=print&travsettleid={travSetId}&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/print.png\" alt=\"Print\" title=\"Print\" width=\"16\" height=\"16\" border=\"0\"></a>");
                }

                // Check if any document uploaded
                bool hasDoc = _context.tbl_employee_travel_settlement_sub_doc.Any(d => d.trav_set_id == travSetId);
                if (hasDoc)
                {
                    fnStr.AppendLine($"<a href=\"javascript:PopUpW('request/employee_travel_settlement_document.asp?mode=doc&travsettleid={travSetId}&emp_id={empId}&emp_travel_id={empTravelId}')\"><img src=\"/images/doc.png\" alt=\"Document\" title=\"Document\" width=\"12\" height=\"16\" border=\"0\"></a>");
                }
                else
                {
                    fnStr.AppendLine($"<img src=\"/images/no-doc.png\" alt=\"Document\" title=\"Document\" width=\"12\" height=\"16\" border=\"0\">");
                }

                fnStr.AppendLine("</td>");
                fnStr.AppendLine("</tr>");
            }

            fnStr.AppendLine("</table>");
            fnStr.AppendLine("</td>");
            fnStr.AppendLine("</tr>");
            fnStr.AppendLine("</table>");
            fnStr.AppendLine("</div>");
        }

        if (settlements.Count == 0)
        {
            fnStr.AppendLine("<div id=\"page-box\">");
            // Only show the left side message
            fnStr.AppendLine($"<div id=\"page-box-left\"><h5>{settlements.Count} Records Found</h5></div>");
            fnStr.AppendLine("<div id=\"page-box-right\"><h5>&nbsp;</h5></div>");
            fnStr.AppendLine("</div>");
        }
        return fnStr.ToString();
    }*/


}