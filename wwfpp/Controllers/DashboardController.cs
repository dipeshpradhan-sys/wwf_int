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
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.NetworkInformation;
using wwf_pp.Services;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Services;
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

    public DashboardController(AppDbContext context, EmailService emailSender, DashboardService dashboardService, EmployeeServices employeeServices, IOptions<AppSettings> appSettings, RequestServices requestServices, AdministrationEmailService administrationEmailService, EmailService emailService)
    {
        _context = context;
        _emailSender = emailSender;
        _dashboardService = dashboardService;
        _employeeServices = employeeServices;
        _appSettings = appSettings.Value;
        _requestServices = requestServices;
        _administrationEmailService = administrationEmailService;
        _emailService = emailService;
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
    public IActionResult DashboardUnsettledTravels(int emp_id, string? fiscalYear = null, string? status = null)
    {
        return PartialView("Dashboard/_DashboardUnsettledTravels");
    }
    public string ListContractAlert(string parm)
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

                    // Send email
                    _emailService.SendEmail(str_from, str_to, str_subject, strMessage, null, str_cc, null, null, null);
                    return strMessage;
                }
            }
            else
            {
                string strMessageArea1 = fnStrLoop.Length > 0
                    ? fnStrStart + fnStrLoop + "</table></div>\n"
                    : "";

                return strMessageArea1;
            }
        }

        return string.Empty;
    }

}