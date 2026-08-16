using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
/*For using captcha*/
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Security.Cryptography;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Models.Account;
using wwfpp.Services;

namespace wwfpp.Controllers
{
    public class AccountController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        EmailService emailService,
        GlobalOptionServices globalOptionServices,
        EmployeeServices employeeService,
        SettingsServices settingsServices,
        AccountServices accountServices
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly AppSettings _appSettings = (appSettings ?? throw new ArgumentNullException(nameof(appSettings))).Value;
        private readonly EmailService _emailService = emailService;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;
        private readonly EmployeeServices _employeeService = employeeService;
        private readonly SettingsServices _settingsServices = settingsServices;
        private readonly AccountServices _accountServices = accountServices;
        /********************************************************************************************************************/
        #region KEEP SESSION ALIVE
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult KeepAlive()
        {
            // Touch the session so it stays alive
            HttpContext.Session.SetString("LastPing", DateTime.Now.ToString());
            return Ok();
        }
        #endregion
        /********************************************************************************************************************/
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginMiddle()
        {
            if (HttpContext.Session.GetString(_appSettings.SITE_SESSION + "session_referer") == "login-checked")
            {
                /* These are stored on GetLogin()
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "session_id", "Y");
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "session_referer", "login-checked");
                HttpContext.Session.SetString("login_id", loginId);
                HttpContext.Session.SetString("user_id", user.user_id.ToString());
                HttpContext.Session.SetString("username", user.username.ToUpperInvariant());
                HttpContext.Session.SetString("level_id", user.level_id);
                HttpContext.Session.SetString("emp_id", user.emp_id.ToString());
                HttpContext.Session.SetString("sign_in_type_id", user.sign_in_type.ToString());
                HttpContext.Session.SetString("user_sign_in_type", sign_in_type);
                HttpContext.Session.SetString("user_sign_in_type", sign_in_type); 
                 */
                /* reach out after login success*/
                /* load necessary things */
                // flush one time session values
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "temp_user_id", "");
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "UserCode", AccountServices.MakeHash(GblUtilities.UniqueID()));
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "SHOW_CAPTCHA", "");
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "login_attempt", "0");

                string user_level_id = HttpContext.Session.GetString("level_id") ?? "";
                if (!string.IsNullOrWhiteSpace(user_level_id))
                {
                    var userLevel = _context.tbl_user_level.FirstOrDefault(u => u.level_id == user_level_id);
                    if (userLevel != null)
                    {
                        string level_name = userLevel.level_name ?? "";
                        HttpContext.Session.SetString("user_level", level_name);
                    }
                }

                int user_id = int.TryParse(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "user_id")), out int UserId) ? UserId : 0;
                //if (user_id < 1) { return RedirectToAction("Login", "Account"); ; } /** why why shivasmc ?? Ball Ball ta yeha pugeko*/

                _accountServices.LastLoginLocation(user_id);
                _settingsServices.SetFiscalYear();
                //_settingsServices.SetCarryForwardYear(); // in future we wll implement this
                _settingsServices.SetTimesheetType();

                string Emp_Id = HttpContext.Session.GetString("emp_id") ?? "";
                if (int.TryParse(Emp_Id, out int emp_id)) { } else { emp_id = -1; }

                HttpContext.Session.SetString("login_emp_name", _employeeService.GetEmployeeNameEmail(emp_id, "N"));

                /*to make sure the value is just used for one time*/
                HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "session_referer"), "");

                /*
                 * Load Profile
                 */
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string folderPath = Path.Combine(GblDocumentPath, "documents", "photo");
                string ShowPhoto = "/uploads/documents/photo/nopicture.jpg";

                var emp = _context.tbl_employee_photo.FirstOrDefault(u => u.emp_id == emp_id);
                if (emp != null)
                {
                    string fileName = emp.photo ?? "";
                    string fullPath = Path.Combine(folderPath, fileName);
                    if (System.IO.File.Exists(fullPath))
                    {
                        ShowPhoto = $"/uploads/documents/photo/{fileName}";
                    }
                }
                HttpContext.Session.SetString("login_user_photo_info", ShowPhoto);

                /**
                 * This Piece is to set global user session check
                 * writes the authentication cookie to the response
                 * From now on, every request with that cookie will be recognized as authenticated.
                 * User.Identity.IsAuthenticated will return true, 
                 * [Authorize] attributes will enforce access.
                 */
                string justloginuser = HttpContext.Session.GetString("username") ?? "";
                if (!string.IsNullOrWhiteSpace(justloginuser))
                {
                    var claims = new List<Claim>
                    {
                        new (ClaimTypes.Name, justloginuser)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties { IsPersistent = false,  /** Cookie dies when browser closes*/ExpiresUtc = null/** No absolute expiry*/}).ConfigureAwait(false);
                }

                /**
                 * First send to change password if weak/expired
                 * value will change if password expired
                 */
                CheckPasswordExpiry();

                return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd")))
                    ? RedirectToAction("passwordchange", "account", new { cause = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd")) })
                    : RedirectToAction("index", "home");
            }
            return RedirectToAction("login", "account");
        }
        /********************************************************************************************************************/
        #region GENERATE CAPTCHA
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GenerateCaptcha()
        {
#pragma warning disable CA1416

            string chars = GblUtilities.PossibleCaptchaLetters();
            int Nos_Chars = int.TryParse(_globalOptionServices.OptionServices["op_captcha_length"], out int CntNosChars) ? CntNosChars : 4;
            string captchaText = new(
                [.. Enumerable.Repeat(chars, Nos_Chars).Select(s => s[RandomNumberGenerator.GetInt32(s.Length)])]
            );

            /** Set captcha on session value */
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "_Letters_cPtch4_c0de"), captchaText);
            TempData["SCaptchaVal"] = captchaText;

            using var bitmap = new Bitmap(150, 50);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(ColorTranslator.FromHtml("#FAE6E6"));
            using var font = new Font("Verdana", 16, FontStyle.Bold | FontStyle.Italic);
            using var brush = new SolidBrush(Color.Black);
            graphics.DrawString(captchaText, font, brush, 20, 10);

            // Add random lines
            for (int i = 0; i < 5; i++)
            {
                int x1 = RandomNumberGenerator.GetInt32(bitmap.Width);
                int y1 = RandomNumberGenerator.GetInt32(bitmap.Height);
                int x2 = RandomNumberGenerator.GetInt32(bitmap.Width);
                int y2 = RandomNumberGenerator.GetInt32(bitmap.Height);
                graphics.DrawLine(Pens.Gray, x1, y1, x2, y2);
            }
            // Add random dots
            for (int i = 0; i < 30; i++)
            {
                int x = RandomNumberGenerator.GetInt32(bitmap.Width);
                int y = RandomNumberGenerator.GetInt32(bitmap.Height);
                bitmap.SetPixel(x, y, Color.Black);
            }
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            _ = stream.Seek(0, SeekOrigin.Begin);
#pragma warning restore CA1416

            Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";

            return File(stream.ToArray(), "image/png");
        }

        public string CheckCaptcha(string StrCaptcha)
        {
            string FnStr = "0";
            string StoredCaptcha = "";
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "_Letters_cPtch4_c0de"))))
            {
                StoredCaptcha = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "_Letters_cPtch4_c0de")) ?? "";
            }

            if ((string.IsNullOrWhiteSpace(StrCaptcha) && string.IsNullOrWhiteSpace(StoredCaptcha)) ||
                !string.Equals(StrCaptcha, StoredCaptcha, StringComparison.OrdinalIgnoreCase))
            {
                FnStr = "1"; // mismatch
            }
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "_Letters_cPtch4_c0de"), "");/* make sure that the code is only used once*/
            return FnStr;
        }
        #endregion
        /********************************************************************************************************************/
        #region COOKIES
        [AllowAnonymous]
        public IActionResult SetCookie(string CookieName, string CookieValue)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30), // cookie expiry
                HttpOnly = true,                    // prevents JS access
                Secure = true,                      // only over HTTPS
                SameSite = SameSiteMode.Strict      // CSRF protection
            };

            Response.Cookies.Append(CookieName, CookieValue, cookieOptions);

            return Ok("True");
        }
        #endregion
        /********************************************************************************************************************/
        #region REQUIREDFUNCTIONS
        /**
	     * Check if login try exceeded
	     * 2026-May-28
	     */
        public string IsLoginTryExceeded(string parmFlag, string username)
        {
            string remoteAddr = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";

            int login_try_before_captcha = int.TryParse(_globalOptionServices.OptionServices["op_captcha_show_after_fail"], out int LoginTryBeforeCaptcha) ? LoginTryBeforeCaptcha : 5;
            string fnStr = "N";

            // Check session flags and IP
            string? showCaptcha = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "SHOW_CAPTCHA"));
            string? prevRemoteAddr = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "PRE_REMOTE_ADDR"));

            if (string.Equals(showCaptcha, "Y", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(remoteAddr)
                || !string.Equals(prevRemoteAddr, remoteAddr, StringComparison.OrdinalIgnoreCase))
            {
                fnStr = "Y";
            }
            else
            {
                if (string.Equals(parmFlag, "alredy-pOst3d", StringComparison.OrdinalIgnoreCase))
                {
                    int cnt = _context.tbl_user_login_fail.Where(u => u.username == username).Count();
                    if (cnt >= login_try_before_captcha) { fnStr = "Y"; }
                }
            }
            // Update session with current IP
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "PRE_REMOTE_ADDR"), remoteAddr);/* make sure that the code is only used once*/
            return fnStr;
        }

        /**
	     * Check if password expiry
	     * 2026-May-28
	     */
        public void CheckPasswordExpiry()
        {
            /**
             *Expiry : No Record || 'Only N status records => Old User first time login Or history not logged when first time user created'
             *Expiry : Changed days > defined Days'
             */
            int user_id = int.TryParse(HttpContext.Session.GetString("user_id"), out int UserId) ? UserId : 0;
            if (user_id < 1)
            {
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "send_to_change_pwd", "expired");
                return;
            }

            /** Get the most recent password history record */
            int OP_PWD_EXPIRE_DAYS = int.TryParse(_globalOptionServices.OptionServices["op_pwd_expire_days"], out int PwdExpireDays) ? PwdExpireDays : 100;
            int DiffHisDays = _context.tbl_user_pwd_history
                .Where(h => h.is_current_one == "Y" && h.user_id == user_id && h.updated_date != null)
                .OrderByDescending(h => h.updated_date)
                .Select(h => EF.Functions.DateDiffDay(
                    DateTime.Now,
                    (h.updated_date ?? DateTime.Now).AddDays(OP_PWD_EXPIRE_DAYS)
                )).FirstOrDefault();

            /** No record found → expired**/
            if (DiffHisDays < 1)
            {
                HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd"), "expired");
            }
        }
        /**
        * KEEP LOGIN HISTORY
        */
        public string GetLogin(int UserId)
        {
            string fnStr = "";
            /** Find active user*/
            var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == UserId && u.is_active == "Y");
            if (Record == null)
            {
                fnStr = "false";
            }
            else
            {
                int sign_in_type = int.TryParse(Record.sign_in_type.ToString(), out int SignInType) ? SignInType : -1;
                string user_sign_in_type = sign_in_type == 0 ? "step-one" : "step-two";

                /**  Generate unique login ID */
                string loginId = GblUtilities.UniqueID();
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "session_id", "Y");
                HttpContext.Session.SetString(_appSettings.SITE_SESSION + "session_referer", "login-checked");
                HttpContext.Session.SetString("login_id", loginId);
                HttpContext.Session.SetString("user_id", Record.user_id.ToString());
                HttpContext.Session.SetString("username", !string.IsNullOrWhiteSpace(Record.username) ? Record.username.ToUpperInvariant() : "");
                HttpContext.Session.SetString("level_id", !string.IsNullOrWhiteSpace(Record.level_id) ? Record.level_id.ToString() : "");
                HttpContext.Session.SetString("emp_id", Record.emp_id.ToString() ?? "");
                HttpContext.Session.SetString("sign_in_type_id", sign_in_type.ToString() ?? "");
                HttpContext.Session.SetString("user_sign_in_type", user_sign_in_type);

                /** Insert login log */
                _accountServices.InsertUserLoginLog();
                fnStr = "true";
            }
            return fnStr;
        }
        /**
        * LOGIN
        */
        public string CheckLogin(string Username, string PasswordInput)
        {
            string FnStr = "false";
            /** We can do validation passing username and password. 
             * But it is not good idea. So we need to do password verification in here
             * after having password from database
             * Find active user
             */
            if (!string.IsNullOrWhiteSpace(Username))
            {
                var Record = _context.tbl_user.FirstOrDefault(u => u.username == Username && u.is_active == "Y");
                if (Record == null)
                {
                    _accountServices.InsertUserLoginFail(Username);
                    FnStr = "false";
                }
                else
                {
                    /** 
                    * Encode password and compare
                    */
                    string RecordPass = Record.pwd ?? "";
                    string Result = AccountServices.CheckHash(RecordPass, PasswordInput);
                    if (Result == "false")
                    {
                        /** 
                        * login Failed 
                        */
                        HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "SHOW_CAPTCHA"), IsLoginTryExceeded("alredy-pOst3d", Username));/* make sure that the code is only used once*/
                        _accountServices.InsertUserLoginFail(Username);
                        FnStr = "false";
                    }
                    else
                    {
                        /** 
                        * Successful login 
                        */
                        HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "SHOW_CAPTCHA"), "");
                        _accountServices.DeleteUserLoginFail(Username);
                        HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id"), Record.user_id.ToString());
                        /** Need to send to change password if two conditions matched
                        * 1 = weak password as per password policy
                        * 2 = password change time exceeded as per password policy
                        */
                        HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd"), AccountServices.ValidatePassword(PasswordInput));
                        string Sign_in_type = Record.sign_in_type.ToString();
                        /** 
                        * Decide next step 
                        */
                        FnStr = Sign_in_type == "1" ? "step-two" : GetLogin(Record.user_id);
                    }
                }
            }
            else
            {
                FnStr = "false";
            }

            return FnStr;
        }
        /**
	     * Second step tasks
	     */
        public string CheckLoginSecondStep(string Username)
        {
            string fnStr = "false";
            /** 
             * Multi factor login
             */
            int TempUserId = int.TryParse(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id")), out int TUserId) ? TUserId : 0;
            if (TempUserId > 0)
            {
                var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == TempUserId && u.is_active == "Y");
                if (Record == null)
                {
                    _accountServices.InsertUserLoginFail(Username);
                    fnStr = "false";
                }
                else
                {
                    string? pin = Record.pin;
                    if (string.IsNullOrWhiteSpace(pin))
                    {
                        /**
                            * OTP on Email detected
                            * Generate 6 digit random number
                            */
                        pin = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
                        string _emp_id = Record.emp_id.ToString() ?? "";
                        /**
                            * Get employee name and email to send OTP
                            */
                        int emp_id = int.TryParse(_emp_id.ToString(), out int EmpId) ? EmpId : -1;
                        if (emp_id > 0)
                        {
                            string EmployeeName = _employeeService.GetEmployeeNameEmail(emp_id, "N");
                            string SetEmail = _employeeService.GetEmployeeNameEmail(emp_id);
                            string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                            bool IsValidEmail = GblUtilities.ValidateEmail(ToEmail);
                            if (!string.IsNullOrWhiteSpace(ToEmail) && (IsValidEmail = true))
                            {
                                string Subject = Lang.EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_SUBJECT;
                                string Message = Lang.EMAIL_ACCOUNT_MULTI_STEP_PIN_SEND_MESSAGE
                                    .Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal)
                                    .Replace("<[PIN-CODE]>", pin, StringComparison.Ordinal);

                                if (!string.IsNullOrWhiteSpace(ToEmail))
                                {
                                    string emst = _emailService.SendEmail("MFPin", ToEmail, Subject, Message);
                                }
                            }
                            else
                            {
                                /** 
                                    * Email not found 
                                    */
                                fnStr = Lang.msg_error + " (Ux0001).";
                            }
                        }
                        else
                        {
                            /** 
                                * emp id not found
                                */
                            fnStr = Lang.msg_error + " (Ux0002).";
                        }
                    }
                    HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "UserCode"), AccountServices.MakeHash(pin));
                    fnStr = "true";
                }

            }
            else
            {
                fnStr = "false";
            }
            return fnStr;
        }

        /**
	     * LOGIN STEP MFA
	     */
        public string CheckLoginMFA(string UserId, string Username, string PinInput)
        {
            string fnStr = "false";
            /**
             * We can do validation passing username and pin. 
             * Find active user
             */
            if (!string.IsNullOrWhiteSpace(UserId))
            {
                int user_id = Convert.ToInt32(UserId);
                var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == user_id && u.is_active == "Y");

                if (Record == null)
                {
                    _accountServices.InsertUserLoginFail(Username);
                    fnStr = "false";
                }
                else
                {
                    int SignInType = int.TryParse(Record.sign_in_type.ToString(), out int Parse) ? Parse : -1;
                    string pin = !string.IsNullOrWhiteSpace(Record.pin) ? Record.pin : "";
                    if (SignInType == 1)
                    {
                        if (string.IsNullOrWhiteSpace(pin))
                        {
                            pin = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "UserCode")) ?? "p!n-k@sari-Kha1i-bhay0!";
                        }
                        string rlt = AccountServices.CheckHash(pin, PinInput);
                        if (rlt != "true")
                        {
                            _accountServices.InsertUserLoginFail(Username);
                            fnStr = "false";
                        }
                        else
                        {
                            _accountServices.DeleteUserLoginFail(Username);
                            fnStr = GetLogin(Record.user_id);
                            fnStr = "true";
                        }
                    }
                    else
                    {
                        fnStr = "false";
                    }

                } //end if null
            }
            else
            {
                fnStr = "false";
            } // end if no userid
            return fnStr;
        }


        #endregion
        /********************************************************************************************************************/
        #region LOGIN
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            string RememberMe = "0";
            string cookie_login_id = "";
            if (!string.IsNullOrWhiteSpace(HttpContext.Request.Cookies[string.Concat(_appSettings.SITE_SESSION, "_login_id")]))
            {
                cookie_login_id = HttpContext.Request.Cookies[string.Concat(_appSettings.SITE_SESSION, "_login_id")] ?? "";
                cookie_login_id = !string.IsNullOrWhiteSpace(cookie_login_id) ? GblUtilities.Decode(cookie_login_id) : "";
                RememberMe = "1";
            }
            TempData["cookie_login_id"] = cookie_login_id;
            TempData["RememberMe"] = RememberMe;

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            ipAddress = !string.IsNullOrWhiteSpace(ipAddress) ? ipAddress : "";
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "PRE_REMOTE_ADDR"), ipAddress);
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "SHOW_CAPTCHA"), "");
            TempData["ShowCaptcha"] = IsLoginTryExceeded("n@t-pOsted-y3t", "") == "Y" ? "block" : "displaynone";
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "login_attempt"), "0");
            HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id"), "");

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            /**
             * Check if model is valid
             */
            if (ModelState.IsValid)
            {
                string Username = "";
                string Password = "";
                string RememberMe = "";
                string ShowCaptcha = "";
                string Captcha = "";

                if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                if (!string.IsNullOrWhiteSpace(model.Password)) { Password = model.Password; }
                if (!string.IsNullOrWhiteSpace(model.RememberMe)) { RememberMe = model.RememberMe; }
                if (!string.IsNullOrWhiteSpace(model.ShowCaptcha)) { ShowCaptcha = model.ShowCaptcha; }
                if (!string.IsNullOrWhiteSpace(model.Captcha)) { Captcha = model.Captcha; }
                /*
                 * Check if username/Password Empty
                 */
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    return Json(new { success = false, message = Lang.msg_invalid_user_pwd });
                }

                /**
                 * Captcha display setting from option db
                 */
                int CaptchaCount = int.TryParse(_globalOptionServices.OptionServices["op_captcha_show_after_fail"], out int CapCount) ? CapCount : 5;
                /**
                 * Match captcha if Captcha Check is on
                 */
                if (string.Equals(ShowCaptcha, "block", StringComparison.OrdinalIgnoreCase))
                {
                    string chkCaptchaResult = CheckCaptcha(!string.IsNullOrWhiteSpace(Captcha) ? Captcha : "");
                    if (string.Equals(chkCaptchaResult, "1", StringComparison.OrdinalIgnoreCase))
                    {
                        /** Mismatch of captcha occured */
                        return Json(new { success = false, message = Lang.msg_incorrect_captcha, rememberme = RememberMe, showcaptcha = ShowCaptcha });
                    }
                }
                string EncodedUsername = !string.IsNullOrWhiteSpace(Username) && RememberMe == "1" ? GblUtilities.Encode(Username) : "";
                _ = SetCookie(string.Concat(_appSettings.SITE_SESSION, "_login_id"), EncodedUsername);

                string SesLoginAttempt = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "login_attempt")) ?? "";
                int LoginAttempt = int.TryParse(SesLoginAttempt, out int LoginAttpt) ? LoginAttpt : 0;
                ShowCaptcha = LoginAttempt >= CaptchaCount ? "block" : "none";
                LoginAttempt++;
                HttpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "login_attempt"), LoginAttempt.ToString());
                /**
                * Check user exists and active
                */
                string Result = CheckLogin(Username, Password);
                switch (Result?.ToUpperInvariant())
                {
                    case "TRUE":
                        return Json(new {success = true, message = Result, rememberme = RememberMe, showcaptcha = ShowCaptcha, redirectUrl = Url.Action("LoginMiddle", "Account") });
                    case "STEP-TWO":
                        string Rlt = CheckLoginSecondStep(Username);
                        if (string.Equals(Rlt, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            return Json(new { success = true, message = Result, rememberme = RememberMe, showcaptcha = ShowCaptcha, redirectUrl = Url.Action("LoginMultiFactor", "Account") });
                        }
                        else
                        {
                            return Json(new { success = false, message = Rlt, rememberme = RememberMe, showcaptcha = ShowCaptcha });
                        }
                    case "FALSE":
                        return Json(new { success = false, message = Lang.msg_invalid_user_pwd, rememberme = RememberMe, showcaptcha = ShowCaptcha });
                    default:
                        return Json(new { success = false, message = Result,rememberme = RememberMe, showcaptcha = ShowCaptcha });
                }
            }
            string err = "";
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                err = string.Concat(err, " ", error.ErrorMessage);
            }
            return Json(new { success = false, message = err, rememberme = "", showcaptcha = "" });
        }
        #endregion
        /********************************************************************************************************************/
        #region LOGIN MFA
        [HttpGet]
        [AllowAnonymous]
        public IActionResult LoginMultiFactor()
        {
            //Get user login session
            string UserId = "";
            int user_id = 0;
            string? Username = "";
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id"))))
            {
                UserId = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id")) ?? "";
                user_id = int.TryParse(UserId, out int UsrID) ? UsrID : 0;
                /** check if the provided username and user_id is active user */
                var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == user_id && u.is_active == "Y");
                if (Record != null)
                {
                    Username = Record.username;
                    TempData["UserId"] = UserId;
                    TempData["Username"] = Username;
                    return View();
                }
            }
            return RedirectToAction("login", "account", new { msg = "error" });
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginMultiFactor([FromBody] LoginMFAViewModel model)
        {
            /*
             * Check if model is valid
             */
            if (ModelState.IsValid)
            {
                string Username = "";
                string Pin = "";
                string UserId = "";

                if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                if (!string.IsNullOrWhiteSpace(model.Pin)) { Pin = model.Pin; }
                if (!string.IsNullOrWhiteSpace(model.UserId)) { UserId = model.UserId; }
                /**
                 * Check if username/Password Empty
                 */
                if (string.IsNullOrWhiteSpace(UserId) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Pin))
                {
                    return Json(new { success = false, message = Lang.msg_insufficient_info });
                }
                int tempUserId = int.TryParse(
                    HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "temp_user_id")), out int TempParseId
                    ) ? TempParseId : 0;
                int iUserId = int.TryParse(UserId, out int iParsed) ? iParsed : 0;
                if (iUserId != tempUserId)
                {
                    return Json(new { success = false, message = Lang.msg_insufficient_info });
                }
                /**
                 * Check user exists and active
                 */
                string Result = CheckLoginMFA(UserId, Username, Pin);
                if (Result == "true")
                {
                    return Json(new { success = true, message = Result, redirectUrl = Url.Action("LoginMiddle", "Account") });
                }
                else
                {
                    return Json(new { success = false, message = Lang.msg_invalid_user_pin });
                }
            }

            string err = "";
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                err = string.Concat(err, " ", error.ErrorMessage);
            }
            return Json(new { success = false, message = err });

        }
        #endregion
        /********************************************************************************************************************/
        #region PASSWORD FORGOT? 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordForgot()
        {
            // If invalid, return the same view with validation errors
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> PasswordForgot([FromBody] PasswordForgotRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate Captcha
                if (model.Username == "1" || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Captcha))
                {
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }
                else
                {
                    string Username = "";
                    string Email = "";
                    string captcha = "";
                    if (!string.IsNullOrWhiteSpace(model.Captcha)) { captcha = model.Captcha; }
                    if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                    if (!string.IsNullOrWhiteSpace(model.Email)) { Email = model.Email; }
                    // Validate Captcha
                    if (CheckCaptcha(captcha) == "1")
                    {
                        return Json(new { success = false, message = Lang.msg_incorrect_captcha });
                    }
                    else
                    {
                        // Proceed with sending reset link
                        string result = ForgotPasswordHelper(Username, Email);
                        if (result == "true")
                        {
                            return Json(new { success = true, message = Lang.msg_forgot_pwd_link_success });
                        }
                        else if (result == "false")
                        {
                            return Json(new { success = false, message = Lang.msg_forgot_pwd_link_invalid });
                        }
                        else
                        {
                            return Json(new { success = false, message = result });
                        }
                    }

                }
            }
            return View(model);
        }
        public string ForgotPasswordHelper(string username, string email)
        {
            string fnStr = "false";
            //*First check on employee and user table for active'*/
            var row = _context.tbl_user
            .Join(_context.tbl_employee,
                    u => u.emp_id,
                    e => e.emp_id,
                    (u, e) => new
                    {
                        u.user_id,
                        u.username,
                        u.is_active,
                        u.emp_id,
                        e.e_mail,
                        e.emp_status
                    })
            .Where(ntb =>
                    ntb.username == username &&
                    ntb.is_active == "Y" &&
                    ntb.e_mail == email &&
                    ntb.emp_status == "A"
                    )
            .OrderBy(ntb => ntb.username)
            .FirstOrDefault();   // only one row

            if (row != null)
            {
                string _emp_id = row.emp_id.ToString() ?? "0";
                int user_id = Convert.ToInt32(row.user_id);
                _accountServices.DeleteExpiredResetToken(user_id, "PWD");
                /**
                    * Get employee name and email to send reset link
                    */
                int emp_id = int.TryParse(_emp_id.ToString(), out int ParsedId) ? ParsedId : -1;
                if (emp_id > 0)
                {
                    string EmployeeName = _employeeService.GetEmployeeNameEmail(emp_id, "N");
                    string SetEmail = _employeeService.GetEmployeeNameEmail(emp_id);
                    string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                    if (string.IsNullOrWhiteSpace(ToEmail))
                    {
                        string Id = GblUtilities.UniqueID();
                        string code = Guid.NewGuid().ToString(); // unique token
                        var expiry = DateTime.UtcNow.AddMinutes(30); //expiry date time

                        // Save token n expiry in DB against the user
                        var DataSave = new tbl_user_reset_token
                        {
                            Id = Id,
                            user_id = user_id,
                            token = code,
                            expiry = expiry,
                            pwdorpin = "PWD"
                        };
                        _ = _context.tbl_user_reset_token.Add(DataSave);
                        _ = _context.SaveChanges();
                        /**
                        * make reset password link and send email to user
                        */
                        string? url = @Url.Action("PasswordReset", "Account", new
                        {
                            pid = Uri.EscapeDataString(GblUtilities.Encode(Id)),
                            uid = Uri.EscapeDataString(GblUtilities.Encode(user_id.ToString())),
                            utoken = Uri.EscapeDataString(code)
                        }, protocol: Request.Scheme);

                        string callbackurl = $@"<a href=""{url}"" 
                            style=""background-color:#04aa6d; border:none; color:white; padding:10px; text-align:center; 
                            text-decoration:none; display:inline-block; font-size:16px; margin:4px 2px; 
                            cursor:pointer; border-radius:10px;"">Click here to reset password</a>";

                        string Subject = Lang.EMAIL_ACCOUNT_FORGOT_PWD_LINK_SUBJECT;
                        string Message = Lang.EMAIL_ACCOUNT_FORGOT_PWD_LINK_MESSAGE
                                        .Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal)
                                        .Replace("<[CALL-BACK-URL]>", callbackurl, StringComparison.Ordinal);
                        if (!string.IsNullOrWhiteSpace(ToEmail))
                        {
                            string emst = _emailService.SendEmail("ForgotPassword", ToEmail, Subject, Message);
                        }
                        fnStr = "true";
                    }
                    else
                    {
                        fnStr = string.Concat(Lang.msg_error, " (PWx0001).");// Email not found
                    }
                }
                else
                {
                    fnStr = string.Concat(Lang.msg_error, " (PWx0002).");//emp id not found
                }
            }
            else
            {
                fnStr = "false";//emp id not found
            }
            return fnStr;
        }
        #endregion
        /********************************************************************************************************************/
        #region PIN FORGOT?
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PinForgot()
        {
            // If invalid, return the same view with validation errors
            return View();
        }
        [AllowAnonymous]
        public async Task<IActionResult> PinForgot([FromBody] PinForgotRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate Captcha
                if (model.Username == "1" || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.Captcha))
                {
                    return Json(new { success = false, message = "Some field(s) are missing. Complete all required inputs to continue." });
                }
                else
                {
                    // Validate Captcha
                    if (CheckCaptcha(model.Captcha) == "1")
                    {
                        return Json(new { success = false, message = "Captch is incorrect. Try again." });
                    }
                    else
                    {
                        // Proceed with sending reset link
                        string MsgForgotPin;
                        string result = ForgotPinHelper(model.Username ?? "", model.Password);
                        if (result == "true")
                        {
                            MsgForgotPin = "Pin reset link sent successfully.Please check your email inbox(and spam folder if needed) for the reset instructions.";
                            return Json(new { success = true, message = MsgForgotPin });
                        }
                        else if (result == "false")
                        {
                            MsgForgotPin = "User Name or Password is invalid. Please try again.";
                            return Json(new { success = false, message = MsgForgotPin });
                        }
                        else
                        {
                            MsgForgotPin = result;
                            return Json(new { success = false, message = MsgForgotPin });
                        }
                    }

                }

            }
            return View(model);
        }
        public string ForgotPinHelper(string username, string password)
        {
            string fnStr = "false";

            /**
             * First check on employee and user table for active'
             */
            var row = _context.tbl_user
            .Join(_context.tbl_employee,
                    u => u.emp_id,
                    e => e.emp_id,
                    (u, e) => new
                    {
                        u.user_id,
                        u.pwd,
                        u.pin,
                        u.sign_in_type,
                        u.username,
                        u.is_active,
                        u.emp_id,
                        e.e_mail,
                        e.emp_status
                    })
            .Where(ntb =>
                    ntb.username == username &&
                    ntb.is_active == "Y" &&
                    ntb.emp_status == "A"
                    )
            .OrderBy(ntb => ntb.username)
            .FirstOrDefault();

            if (row != null)
            {
                //validate password
                string result = AccountServices.CheckHash(row.pwd ?? "", password);
                if (result == "true")
                {
                    string sign_in_type = row.sign_in_type.ToString();
                    string pin = row.pin ?? "";
                    if (sign_in_type == "1" && !string.IsNullOrWhiteSpace(pin))
                    {
                        int emp_id = row.emp_id ?? 0;
                        int user_id = Convert.ToInt32(row.user_id);
                        _accountServices.DeleteExpiredResetToken(row.user_id, "PIN");
                        /*
                        * Get employee name and email to send reset link
                        */
                        if (emp_id > 0)
                        {
                            string EmployeeName = _employeeService.GetEmployeeNameEmail(emp_id, "N");
                            string SetEmail = _employeeService.GetEmployeeNameEmail(emp_id);
                            string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                            if (!string.IsNullOrWhiteSpace(ToEmail))
                            {
                                string Id = GblUtilities.UniqueID();
                                string code = Guid.NewGuid().ToString(); /** unique token*/
                                var expiry = DateTime.UtcNow.AddMinutes(30); //** expiry date time */

                                /** Save token + expiry in DB against the user*/
                                var DataSave = new tbl_user_reset_token
                                {
                                    Id = Id,
                                    user_id = row.user_id,
                                    token = code,
                                    expiry = expiry,
                                    pwdorpin = "PIN"
                                };
                                _ = _context.tbl_user_reset_token.Add(DataSave);
                                _ = _context.SaveChanges();
                                /**
                                * make reset pin link and send email to user
                                */
                                string callbackurl = "<a href = \"" + Url.Action("pinreset", "account", new { pid = GblUtilities.Encode(Id), uid = GblUtilities.Encode(user_id.ToString()), utoken = code }, protocol: Request.Scheme) + "\" style = \"background-color:#04aa6d; border:none; color:white; padding:10px; text-align:center; text-decoration:none;display:inline-block; font-size:16px; margin:4px 2px; cursor:pointer; border-radius:10px;\"> Click here to reset pin</a>";

                                string Subject = Lang.EMAIL_ACCOUNT_FORGOT_PIN_LINK_SUBJECT;
                                string Message = Lang.EMAIL_ACCOUNT_FORGOT_PIN_LINK_MESSAGE
                                    .Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal)
                                    .Replace("<[CALL-BACK-URL]>", callbackurl, StringComparison.Ordinal);
                                if (!string.IsNullOrWhiteSpace(ToEmail))
                                {
                                    string emst = _emailService.SendEmail("ResetPin", ToEmail, Subject, Message);
                                }
                                fnStr = "true";
                            }
                            else
                            {
                                // Email not found
                                fnStr = string.Concat(Lang.msg_error, " (PNx0001).");
                            }
                        }
                        else
                        {
                            //emp id not found
                            fnStr = string.Concat(Lang.msg_error, " (PNx0002).");
                        }
                    }
                    else
                    {
                        //sign in type incorrrect
                        fnStr = string.Concat(Lang.msg_error, " (PNx0003).");
                    }
                }
                else
                {
                    fnStr = "false";//password not matched
                }
            }
            else
            {
                fnStr = "false";//emp id not found
            }
            return fnStr;
        }
        #endregion
        /********************************************************************************************************************/
        #region PASSWORD RESET FORGOT  
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PasswordReset(string pid, string uid, string utoken)
        {
            //Load
            string fnStr = ""; string Id = ""; string user_id = ""; string Token = "";
            if (!string.IsNullOrWhiteSpace(pid)) { Id = GblUtilities.Decode(pid); }
            if (!string.IsNullOrWhiteSpace(uid)) { user_id = GblUtilities.Decode(uid); }
            if (!string.IsNullOrWhiteSpace(utoken)) { Token = utoken; }

            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(user_id) || string.IsNullOrWhiteSpace(Token))
            {
                fnStr = "false";
            }
            else
            {
                var token = _context.tbl_user_reset_token.
                FirstOrDefault(
                            t => t.user_id == Convert.ToInt32(user_id) &&
                            t.Id == Id &&
                            t.token == Token &&
                            t.expiry > DateTime.UtcNow &&
                            t.pwdorpin == "PWD"
                        );
                if (token != null)
                {
                    TempData["msg"] = "ResetPassword";
                    TempData["Id"] = pid;
                    TempData["user_id"] = uid;
                    TempData["Token"] = utoken;
                    var userName = _context.tbl_user.FirstOrDefault(t => t.user_id == Convert.ToInt32(user_id));
                    if (userName != null)
                    {
                        TempData["UserName"] = userName.username;
                    }
                    fnStr = "true";
                }
                else
                {
                    fnStr = "false";
                }
            }
            if (fnStr == "false")
            {
                //something is missing
                TempData["msg"] = "InvalidResetPassword";
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordReset([FromBody] PasswordResetRequestViewModel model)
        {
            //Reset password validation and update
            if (ModelState.IsValid)
            {
                string Id = model.Id;
                string UserId = model.UserId;
                string username = model.Username;
                string pwd = model.Password;
                string cpwd = model.ConfirmPassword;
                string token = model.Token ?? "";
                string captcha = model.Captcha;
                if (
                    string.IsNullOrWhiteSpace(Id) ||
                    string.IsNullOrWhiteSpace(UserId) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(pwd) ||
                    string.IsNullOrWhiteSpace(cpwd) ||
                    string.IsNullOrWhiteSpace(token) ||
                    string.IsNullOrWhiteSpace(captcha)
                    )
                {
                    //return with error
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }
                //Captcha matching
                if (CheckCaptcha(captcha) == "1")
                {
                    return Json(new { success = false, message = Lang.msg_incorrect_captcha });
                }
                //Passwords not match exactly
                if (!string.Equals(pwd, cpwd, StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = Lang.msg_new_cpwd_not_same });
                }

                //check violating policy of Passwords
                string msg_rslt_pwd_policy = AccountServices.ValidatePassword(pwd, "M");
                if (!string.IsNullOrWhiteSpace(msg_rslt_pwd_policy))
                {
                    return Json(new { success = false, message = Lang.msg_rslt_pwd_policy });
                }

                Id = GblUtilities.Decode(Id);
                int user_id = Convert.ToInt32(GblUtilities.Decode(UserId));

                //check in Token database
                var rtoken = _context.tbl_user_reset_token.
                        FirstOrDefault(
                            t => t.user_id == user_id &&
                            t.Id == Id &&
                            t.token == token &&
                            t.expiry > DateTime.UtcNow &&
                            t.pwdorpin == "PWD"
                        );
                if (rtoken == null)
                {
                    return Json(new { success = false, message = Lang.msg_pwd_reset_expired });
                }

                //check if the provided username and user_id is active user
                var user = _context.tbl_user.FirstOrDefault(u => u.username == username && u.user_id == user_id && u.is_active == "Y");
                if (user == null)
                {
                    return Json(new { success = false, message = Lang.msg_pwd_reset_expired });
                }
                int emp_id = user.emp_id ?? -1;
                //check if the password already used some time ago, only check for recent 5 changes
                if (_accountServices.IsPasswordAlreadyUsed(user_id, pwd))
                {
                    return Json(new { success = false, message = Lang.msg_pwd_reused_detected });
                }

                //balla balla hai all cleared. proceed to save new password
                if (_accountServices.SavePasswordChange(user_id, emp_id, pwd, "ResetPassword"))
                {
                    return Json(new { success = true, message = Lang.msg_password_changed_successfully });
                }

                return Json(new { success = false, message = Lang.msg_error });
            }
            else
            {
                string err = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    err = string.Concat(err, " ", error.ErrorMessage);
                }
                return Json(new { success = false, message = err });
            }

        }
        #endregion
        /********************************************************************************************************************/
        #region PIN RESET FORGOT 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult PinReset(string pid, string uid, string utoken)
        {
            string fnStr = ""; string Id = ""; string user_id = ""; string Token = "";
            if (!string.IsNullOrWhiteSpace(pid)) { Id = GblUtilities.Decode(pid); }
            if (!string.IsNullOrWhiteSpace(uid)) { user_id = GblUtilities.Decode(uid); }
            if (!string.IsNullOrWhiteSpace(utoken)) { Token = utoken; }

            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(user_id) || string.IsNullOrWhiteSpace(Token))
            {
                fnStr = "false";
            }
            else
            {
                var RecordT = _context.tbl_user_reset_token.FirstOrDefault
                (
                    t => t.user_id == Convert.ToInt32(user_id) &&
                    t.Id == Id &&
                    t.token == Token &&
                    t.expiry > DateTime.UtcNow &&
                    t.pwdorpin == "PIN"
                );
                if (RecordT != null)
                {
                    TempData["msg"] = "ResetPin";
                    TempData["Id"] = pid;
                    TempData["user_id"] = uid;
                    TempData["Token"] = utoken;
                    var Record = _context.tbl_user.FirstOrDefault(t => t.user_id == Convert.ToInt32(user_id));
                    if (Record != null)
                    {
                        TempData["UserName"] = Record.username;
                    }
                    fnStr = "true";
                }
                else
                {
                    fnStr = "false";
                }
            }
            if (fnStr == "false")
            {
                //something is missing
                TempData["msg"] = "InvalidResetPin";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinReset([FromBody] PinResetRequestViewModel model)
        {
            //Reset password validation and update
            if (ModelState.IsValid)
            {
                string Id = model.Id;
                string UserId = model.UserId;
                string username = model.Username ?? "";
                string pin = model.Pin;
                string cpin = model.ConfirmPin;
                string token = model.Token ?? "";
                string captcha = model.Captcha;
                if (
                    string.IsNullOrWhiteSpace(Id) ||
                    string.IsNullOrWhiteSpace(UserId) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(pin) ||
                    string.IsNullOrWhiteSpace(cpin) ||
                    string.IsNullOrWhiteSpace(token) ||
                    string.IsNullOrWhiteSpace(captcha)
                    )
                {
                    //return with error
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }
                //Captcha matching
                if (CheckCaptcha(captcha) == "1")
                {
                    return Json(new { success = false, message = Lang.msg_incorrect_captcha });
                }
                //Passwords not match exactly
                if (!string.Equals(pin, cpin, StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = Lang.msg_new_cpin_not_same });
                }

                //check violating policy of Pin
                string msg_rslt_pin_policy = AccountServices.ValidatePin(pin, "M");
                if (!string.IsNullOrWhiteSpace(msg_rslt_pin_policy))
                {
                    return Json(new { success = false, message = Lang.msg_rslt_pin_policy });
                }

                Id = GblUtilities.Decode(Id);
                string UserIdE = GblUtilities.Decode(UserId);
                int user_id = int.TryParse(UserIdE, out int parseId) ? parseId : -1;

                //check in Token database
                var RecordT = _context.tbl_user_reset_token.FirstOrDefault
                    (
                        t =>
                        t.user_id == user_id &&
                        t.Id == Id &&
                        t.token == token &&
                        t.expiry > DateTime.UtcNow &&
                        t.pwdorpin == "PIN"
                    );
                if (RecordT == null)
                {
                    return Json(new { success = false, message = Lang.msg_pin_reset_expired });
                }

                //check if the provided username and user_id is active user
                var Record = _context.tbl_user.FirstOrDefault(u => u.username == username && u.user_id == user_id && u.is_active == "Y");
                if (Record == null)
                {
                    return Json(new { success = false, message = Lang.msg_pin_reset_expired });
                }

                string _emp_id = Record.emp_id.ToString() ?? "";
                int emp_id = int.TryParse(_emp_id, out int EmpId) ? EmpId : -1;

                //balla balla hai all cleared. proceed to save new pin
                if (_accountServices.SavePinChange(user_id, emp_id, pin, "ResetPin"))
                {
                    return Json(new { success = true, message = Lang.msg_pin_changed_successfully });
                }
                return Json(new { success = false, message = Lang.msg_error });
            }
            else
            {
                string err = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    err = err + " " + error.ErrorMessage;
                }
                return Json(new { success = false, message = err });
            }

        }
        #endregion
        /********************************************************************************************************************/
        #region LOGOUT

        // POST — recommended (anti-forgery protected)
        [HttpGet]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            /* UPDATE the logout date time */
            string result = _accountServices.UpdateUserLoginLog();
            string fnStr = "logout";
            if (result != "true")
            {
                fnStr = "sessionout";
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            HttpContext.Session.Clear();

            // Add no-cache headers just for this response
            //Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            //Response.Headers["Pragma"] = "no-cache";
            //Response.Headers["Expires"] = "0";

            return RedirectToAction("login", "account", new { msg = fnStr });
            //want to put but after posting the page not load. because the login page
            //is designed HttpGet, so the post method will not get values
            //and land page not working
            //see and try to implement later some time
            //return RedirectToAction("loggedout", "account", new { msg = fnStr });
        }
        [AllowAnonymous]
        public IActionResult LoggedOut()
        {
            return View();
        }
        #endregion
        /********************************************************************************************************************/
        #region PASSWROD CHANGE
        [HttpGet]
        public IActionResult PasswordChange()
        {
            string PageId = "11001";
            string why_reached_here = "";
            int user_id = int.TryParse(HttpContext.Session.GetString("user_id"), out int userid) ? userid : 0;
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd"))))
            {
                string SesValue = HttpContext.Session.GetString(string.Concat(_appSettings.SITE_SESSION, "send_to_change_pwd")) ?? "";
                /** send to here by force un-complanance of password policy */
                if (string.Equals(SesValue, "expired", StringComparison.OrdinalIgnoreCase))
                {
                    why_reached_here = Lang.msg_password_expired;
                }
                else if (string.Equals(SesValue, "weak", StringComparison.OrdinalIgnoreCase))
                {
                    {
                        why_reached_here = Lang.msg_password_weak;
                    }
                }
                why_reached_here = $@"<div id=""message_header""><p id=""note-chg"" class=""warning"">{why_reached_here}</p></div>";
            }
            ViewBag.LastPasswordChange = _accountServices.GetLastPasswordChange(user_id);
            ViewBag.WhyReachedHere = why_reached_here;

            ViewBag.Id = GblUtilities.Encode(user_id.ToString());

            // permessions
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordChange([FromBody] PasswordChangeViewModel model)
        {
            //string message = "";
            /*
                * Check if model is valid
                */
            if (ModelState.IsValid)
            {
                string Mode = "";
                string UserId = "";
                string Username = "";
                string OldPassword = "";
                string NewPassword = "";
                string ConfirmPassword = "";

                if (!string.IsNullOrWhiteSpace(model.Mode)) { Mode = model.Mode; }
                if (!string.IsNullOrWhiteSpace(model.UserId)) { UserId = model.UserId; }
                if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                if (!string.IsNullOrWhiteSpace(model.OldPassword)) { OldPassword = model.OldPassword; }
                if (!string.IsNullOrWhiteSpace(model.NewPassword)) { NewPassword = model.NewPassword; }
                if (!string.IsNullOrWhiteSpace(model.ConfirmPassword)) { ConfirmPassword = model.ConfirmPassword; }

                if (
                    string.IsNullOrWhiteSpace(Mode) ||
                    string.IsNullOrWhiteSpace(UserId) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(OldPassword) ||
                    string.IsNullOrWhiteSpace(NewPassword) ||
                    string.IsNullOrWhiteSpace(ConfirmPassword)
                    )
                {
                    //return with error
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }

                //old and new Passwords match exactly
                if (string.Equals(OldPassword, NewPassword, StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = Lang.msg_old_new_pwd_same });
                }

                //Passwords not match exactly
                if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = Lang.msg_new_cpwd_not_same });
                }

                //check violating policy of Passwords
                string msg_rslt_pwd_policy = AccountServices.ValidatePassword(NewPassword, "M");
                if (!string.IsNullOrWhiteSpace(msg_rslt_pwd_policy))
                {
                    return Json(new { success = false, message = Lang.msg_rslt_pwd_policy });
                }

                string Id = GblUtilities.Decode(UserId);
                int user_id = Convert.ToInt32(Id);
                //check if the provided username and user_id is active user
                var user = _context.tbl_user
                        .FirstOrDefault(u => u.username == Username && u.user_id == user_id && u.is_active == "Y");
                if (user == null)
                {
                    return Json(new { success = false, message = Lang.msg_error });
                }
                string User_Saved_Pwd = user.pwd ?? "";
                string _emp_id = user.emp_id.ToString() ?? "";
                int emp_id = int.TryParse(_emp_id, out int EmpId) ? EmpId : -1;
                string Pass_Result = AccountServices.CheckHash(User_Saved_Pwd, OldPassword);
                if (!string.Equals(Pass_Result, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = Lang.msg_invalid_password });
                }

                //check if the password already used some time ago, only check for recent 5 changes
                if (_accountServices.IsPasswordAlreadyUsed(user_id, NewPassword))
                {
                    return Json(new { success = false, message = Lang.msg_pwd_reused_detected });
                }

                //balla balla hai all cleared. proceed to save new password
                if (_accountServices.SavePasswordChange(user_id, emp_id, NewPassword, "ChangePassword"))
                {
                    return Json(new { success = true, message = Lang.msg_password_changed_successfully });
                }

                return Json(new { success = false, message = Lang.msg_error });

            }
            else
            {
                string err = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    err = err + " " + error.ErrorMessage;
                }
                return Json(new { success = false, message = err, rememberme = "" });
            }
        }
        #endregion
        /********************************************************************************************************************/
        #region PIN CHANGE
        [HttpGet]
        public IActionResult PinChange()
        {
            string PageId = "11002";

            int user_id = int.TryParse(HttpContext.Session.GetString("user_id"), out int UserId) ? UserId : 0;
            ViewBag.Id = GblUtilities.Encode(user_id.ToString());

            // permessions
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;

            //also send the sign type and is eligibile for pin change
            //Context.Session.GetString("sign_in_type_id") == "1"
            string Will_Able_Change_Pin = "N";
            var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == user_id && u.is_active == "Y");
            if (Record != null)
            {
                string _pin = Record.pin ?? "";
                if (HttpContext.Session.GetString("sign_in_type_id") == "1" && !string.IsNullOrWhiteSpace(_pin))
                {
                    Will_Able_Change_Pin = "Y";
                }
            }
            ViewBag.Will_Able_Change_Pin = Will_Able_Change_Pin;

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinChange([FromBody] PinChangeViewModel model)
        {
            //string message = "";
            /*
                * Check if model is valid
                */
            if (ModelState.IsValid)
            {
                string Mode = "";
                string UserId = "";
                string Username = "";
                string Password = "";
                string NewPin = "";
                string ConfirmPin = "";

                if (!string.IsNullOrWhiteSpace(model.Mode)) { Mode = model.Mode; }
                if (!string.IsNullOrWhiteSpace(model.UserId)) { UserId = model.UserId; }
                if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                if (!string.IsNullOrWhiteSpace(model.Password)) { Password = model.Password; }
                if (!string.IsNullOrWhiteSpace(model.NewPin)) { NewPin = model.NewPin; }
                if (!string.IsNullOrWhiteSpace(model.ConfirmPin)) { ConfirmPin = model.ConfirmPin; }

                if (
                    string.IsNullOrWhiteSpace(Mode) ||
                    string.IsNullOrWhiteSpace(UserId) ||
                    string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(NewPin) ||
                    string.IsNullOrWhiteSpace(ConfirmPin)
                    )
                {
                    //return with error
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }

                //Pins not match exactly
                if (!string.Equals(NewPin, ConfirmPin, StringComparison.Ordinal))
                {
                    return Json(new { success = false, message = Lang.msg_new_cpin_not_same });
                }

                //check violating policy of pin
                string msg_rslt_pin_policy = AccountServices.ValidatePin(NewPin, "M");
                if (!string.IsNullOrWhiteSpace(msg_rslt_pin_policy))
                {
                    return Json(new { success = false, message = Lang.msg_rslt_pin_policy });
                }

                string Id = GblUtilities.Decode(UserId);
                int user_id = Convert.ToInt32(Id);

                //check if the provided username and user_id is active user
                var user = _context.tbl_user
                        .FirstOrDefault(u => u.username == Username && u.user_id == user_id && u.is_active == "Y");
                if (user == null)
                {
                    return Json(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    string User_Saved_Pwd = user.pwd ?? "";
                    string _emp_id = user.emp_id.ToString() ?? "";
                    int emp_id = int.TryParse(_emp_id, out int EmpId) ? EmpId : -1;
                    //match old password first
                    string Pass_Result = AccountServices.CheckHash(User_Saved_Pwd, Password);
                    if (string.Equals(Pass_Result, "true", StringComparison.OrdinalIgnoreCase))
                    {
                        return Json(new { success = false, message = Lang.msg_invalid_password });
                    }

                    //balla balla hai all cleared. proceed to save new password
                    if (_accountServices.SavePinChange(user_id, emp_id, NewPin, "ChangePin"))
                    {
                        return Json(new { success = true, message = Lang.msg_pin_changed_successfully });
                    }
                }
                return Json(new { success = false, message = Lang.msg_error });

            }
            else
            {
                string err = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    err = err + " " + error.ErrorMessage;
                }
                return Json(new { success = false, message = err, rememberme = "" });
            }

        }

        #endregion
        /********************************************************************************************************************/
        #region SET LOGIN STEP
        [HttpGet]
        public IActionResult LoginStepSet()
        {
            string PageId = "11003";

            int user_id = int.TryParse(HttpContext.Session.GetString("user_id"), out int UserId) ? UserId : 0;
            // permessions
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;

            //get existing login step
            var Record = _context.tbl_user.FirstOrDefault(u => u.user_id == user_id && u.is_active == "Y");
            if (Record == null)
            {
                ViewBag.apern = "false";
                ViewBag.epern = "fasle";
            }
            else
            {
                string Pin_Code = Record.pin ?? "";
                int Sign_In_Type = Record.sign_in_type;
                string Show_Login_Type = "0";
                if (Sign_In_Type == 1)
                {
                    Show_Login_Type = !string.IsNullOrWhiteSpace(Pin_Code) ? "1" : "2";
                }
                ViewBag.Id = GblUtilities.Encode(user_id.ToString());
                ViewBag.Show_Login_Type = Show_Login_Type;
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginStepSet([FromBody] LoginStepSetViewModel model)
        {
            //string message = "";

            /*
			 * Check if model is valid
			 */
            if (ModelState.IsValid)
            {
                string Mode = "";
                string UserId = "";
                string Username = "";
                string Password = "";
                int SignInType = model.SignInType;
                string Pin = "";

                if (!string.IsNullOrWhiteSpace(model.Mode)) { Mode = model.Mode; }
                if (!string.IsNullOrWhiteSpace(model.UserId)) { UserId = model.UserId; }
                if (!string.IsNullOrWhiteSpace(model.Username)) { Username = model.Username; }
                if (!string.IsNullOrWhiteSpace(model.Password)) { Password = model.Password; }
                if (!string.IsNullOrWhiteSpace(model.Pin)) { Pin = model.Pin; }
                if (
                        string.IsNullOrWhiteSpace(Mode) ||
                        string.IsNullOrWhiteSpace(UserId) ||
                        string.IsNullOrWhiteSpace(Username) ||
                        string.IsNullOrWhiteSpace(Password)
                        )
                {
                    /**return with error*/
                    return Json(new { success = false, message = Lang.msg_some_fields_missing });
                }
                /** check violating policy of pin */
                if (SignInType == 1)
                {
                    /** Two Factor and User Define Pin (UDP)*/
                    string msg_rslt_pin_policy = AccountServices.ValidatePin(Pin, "M");
                    if (!string.IsNullOrWhiteSpace(msg_rslt_pin_policy))
                    {
                        return Json(new { success = false, message = Lang.msg_rslt_pin_policy });
                    }
                }
                string Id = GblUtilities.Decode(UserId);
                int user_id = Convert.ToInt32(Id);

                /** check if the provided username and user_id is active user */
                var user = _context.tbl_user
                        .FirstOrDefault(u => u.username == Username && u.user_id == user_id && u.is_active == "Y");
                if (user == null)
                {
                    return Json(new { success = false, message = Lang.msg_error });
                }

                string User_Saved_Pwd = user.pwd ?? "";
                /** match old password first */
                string Pass_Result = AccountServices.CheckHash(User_Saved_Pwd, Password);
                if (!string.Equals(Pass_Result, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = Lang.msg_invalid_password });
                }

                int Sign_In_Type = 0;
                string hashedPin = "";
                string Lbl_Login_Type = "";
                if (SignInType == 1)
                {
                    hashedPin = AccountServices.MakeHash(Pin);
                    Sign_In_Type = 1;
                    Lbl_Login_Type = "Two Factor Login (UDP)";

                }
                else if (SignInType == 2)
                {
                    Sign_In_Type = 1;
                    Lbl_Login_Type = "Two Factor Login (OTP)";
                }
                else
                {
                    Sign_In_Type = 0;
                    Lbl_Login_Type = "Single Factor Login";
                }
                var DataUpdate = _context.tbl_user.FirstOrDefault(h => h.user_id == user_id);
                if (DataUpdate != null)
                {
                    DataUpdate.pin = hashedPin;
                    DataUpdate.sign_in_type = Sign_In_Type;
                    _ = _context.tbl_user.Update(DataUpdate);
                    _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                    HttpContext.Session.SetString("sign_in_type_id", Sign_In_Type.ToString());

                }
                return Json(new { success = true, message = Lang.msg_MFA_changed_successfully, lbllogintype = Lbl_Login_Type });
            }
            else
            {
                string err = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    err = string.Concat(err, " ", error.ErrorMessage);
                }
                return Json(new { success = false, message = err, rememberme = "" });
            }
        }

        #endregion
        /********************************************************************************************************************/

    }
}
