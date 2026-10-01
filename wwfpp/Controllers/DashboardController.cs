using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public DashboardController(AppDbContext context, EmailService emailSender, DashboardService dashboardService, EmployeeServices employeeServices)
    {
        _context = context;
        _emailSender = emailSender;
        _dashboardService = dashboardService;
        _employeeServices = employeeServices;
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
                .FirstOrDefault()
        }).ToList();

        ViewBag.CmbMark = cmbMark;
        ViewBag.CmbMeEmployee = empId;

        return PartialView("Dashboard/_DashboardTravelApprovedAccount", travels);
    }

}