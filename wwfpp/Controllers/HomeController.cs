using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Services;

namespace wwfpp.Controllers
{
    public class HomeController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        SessionHelper sessionHelper,
        EmailService emailService,
        ILogger<HomeController> logger,
        GlobalOptionServices globalOptionServices
            ) : Controller
    {
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly SessionHelper _sessionHelper = sessionHelper;
        private readonly EmailService _emailService = emailService;
        private readonly ILogger<HomeController> _logger = logger;
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
    }
}
