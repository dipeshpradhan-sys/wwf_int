using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Services;

namespace wwfpp.Controllers
{
    public class RequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly EmailService _emailService;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly EmployeeServices _employeeService;
        private readonly SettingsServices _settingsServices;
        private readonly AccountServices _accountServices;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public RequestController(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            EmailService emailService,
            GlobalOptionServices globalOptionServices,
            EmployeeServices employeeService,
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
            _employeeService = employeeService;
            _settingsServices = settingsServices;
            _accountServices = accountServices;
            _webHostEnvironment = webHostEnvironment;
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
