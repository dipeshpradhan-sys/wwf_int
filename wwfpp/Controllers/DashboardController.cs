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

    public DashboardController(AppDbContext context, EmailService emailSender, DashboardService dashboardService)
    {
        _context = context;
        _emailSender = emailSender;
        _dashboardService = dashboardService;
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
}