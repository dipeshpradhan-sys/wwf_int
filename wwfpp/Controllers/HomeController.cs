using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using wwf_pp.Services;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;
using wwfpp.Services;

namespace wwfpp.Controllers
{
    public class HomeController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        SessionHelper sessionHelper,
        EmailService emailService,
        ILogger<HomeController> logger,
        EmployeeServices employeeServices,
        DashboardService dashboardServices,
        GlobalOptionServices globalOptionServices
            ) : Controller
    {
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly SessionHelper _sessionHelper = sessionHelper;
        private readonly EmailService _emailService = emailService;
        private readonly ILogger<HomeController> _logger = logger;
        private readonly EmployeeServices _employeeServices = employeeServices;
        private readonly DashboardService _dashboardServices = dashboardServices;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;
        private readonly AppDbContext _context = context;

        public IActionResult Index()
        {
            try
            {
                // Force DB connection check
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
            }
            catch
            {
                // Redirect to friendly error page
                return RedirectToAction("dberror");
            }
            //Directly injected on Views
            //Needed values can be taken on controler pages
            //ViewBag.OrgOpName = _globalOptionServices.OptionServices["op_org_name"];


            //Get user login session
            var UserId = "";
            int user_id = 0;
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString("user_id")))
            {
                UserId = HttpContext.Session.GetString("user_id");
                user_id = Convert.ToInt32(UserId);
            }
            /*
            if (string.IsNullOrWhiteSpace(UserId) || user_id < 1)
            {
                var returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString;
                return RedirectToAction("login", "account");
            }*/
            //Check if any permission is there
            /*
            var menuPermission = Context.vw_user_module_menu
                .FirstOrDefault(h => h.user_id == user_id
                                  && (h.is_vw == "Y"
                                      || h.is_ad == "Y"
                                      || h.is_ed == "Y"
                                      || h.is_de == "Y"));
            if (menuPermission == null)
            {
                ViewData["menuPermission"] = "No";
            }
            */
            // If logged in, check if approval parameters exist
            /*
            if (Request.Query.ContainsKey("emp_id") && Request.Query.ContainsKey("approval_from"))
            {
                ViewData["ApprovalMode"] = true;
                ViewData["ApprovalParams"] = Request.Query;
                ViewData["EmpId"] = Request.Query["emp_id"].ToString();
                ViewData["EmpMonth"] = Request.Query["emp_month"].ToString();
                ViewData["EmpYear"] = Request.Query["emp_year"].ToString();
                ViewData["ToId"] = Request.Query["toid"].ToString();
                ViewData["ToEmpId"] = Request.Query["toemp_id"].ToString();
                ViewData["St"] = Request.Query["st"].ToString();
                ViewData["StrCounter"] = Request.Query["str_counter"].ToString();
                ViewData["AppId"] = Request.Query["app_id"].ToString();
                ViewData["ApproveFor"] = Request.Query["approve_for"].ToString();


            }
            else
            {
                ViewData["ApprovalMode"] = false;
            }
            */

            int empId = Convert.ToInt32(HttpContext.Session.GetString("emp_id"));
            ViewBag.cmbMeEmployee = DashboardService.DashboardTravelMeEmployee(empId.ToString(), "");
            ViewBag.cmbMark = DashboardService.DashboardTravelMark();
            ViewBag.cmbTravelStatus = DashboardService.DashboardTravelStatus();

            return View();


        }
        public IActionResult DbError()
        {
            ViewBag.ErrorMessage = Lang.msg_not_able_connect_db;
            return View();
        }
        [HttpGet]
        public IActionResult PermissionDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [AllowAnonymous]
        public IActionResult TokenIndex(string? Token, string? st)
        {
            var model = new ApprovalFromEmailVM();
            if (string.IsNullOrWhiteSpace(Token))
            {
                ViewBag.ErrorMessage = "Token is required.";
            }
            else
            {
                var tokenInfo = _context.tbl_employee_request_app_token
                    .Where(r => r.token == Token)
                    .Select(r => new { r.Id, r.submit_date, r.request_category, r.request_id, r.emp_id, r.toid, r.toemp_id, r.action_st, r.action_via, r.is_used, r.expiry_date })
                    .FirstOrDefault();
                if (tokenInfo == null)
                {
                    ViewBag.ErrorMessage = "Invalid Token.";
                }
                else 
                { 
                    if (tokenInfo.expiry_date.HasValue && tokenInfo.expiry_date.Value < DateTime.Now)
                    {
                        ViewBag.ErrorMessage = "Token is expired.";
                    }
                    else if (tokenInfo.is_used == "Y")
                    {
                        ViewBag.ErrorMessage = "Token already used.";
                    }
                    else
                    {
                        model = new ApprovalFromEmailVM
                        {
                            EmpID = tokenInfo.emp_id,
                            AppID = Convert.ToString(tokenInfo.request_id),
                            Month = 0,
                            Year = 0,
                            ToID = tokenInfo.toid,
                            ToEmpID = tokenInfo.toemp_id,
                            St = st,
                            Counter = 0,
                            ApproveFor = tokenInfo.request_category,
                            Token = Token
                        };
                        string approveDecline = st == "a" ? "Approve" : st == "d" ? "Decline" : "Unknown";
                        string employeeName = _employeeServices.GetEmployeeName(tokenInfo.emp_id);
                        ViewBag.ApproveDeclineRecommend = approveDecline;

                        switch (tokenInfo.request_category.ToLower())
                        {
                            case "timesheet":
                                ViewBag.HeaderMessage = "Timesheet " + approveDecline + " for " + employeeName;
                                break;
                            case "leave":
                                ViewBag.HeaderMessage = "Leave " + approveDecline + " for " + employeeName;
                                break;
                            case "Overtime":
                                ViewBag.HeaderMessage = "Overtime " + approveDecline + " for " + employeeName;
                                break;
                            case "travel":
                                ViewBag.HeaderMessage = "Travel " + approveDecline + " for " + employeeName;
                                break;
                        }
                    }
                }
            }

            return View(model);
        }
    }
}
