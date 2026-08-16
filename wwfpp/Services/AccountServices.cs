using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
namespace wwfpp.Services
{
    public class AccountServices
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private readonly EmployeeServices _employeeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppSettings _appSettings;
        private readonly GlobalOptionServices _globalOptionServices;
        public AccountServices(
            AppDbContext context, 
            EmailService emailServices ,
            EmployeeServices employeeService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AppSettings> appSettings,
            GlobalOptionServices globalOptionServices
            )
        {
            _context = context;
            _emailService = emailServices;
            _employeeService = employeeService;
            _httpContextAccessor = httpContextAccessor;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _globalOptionServices = globalOptionServices;
        }
        public PermissionVM GetMenuPermission(string Menucode)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string is_vw = "false";
            string is_ad = "false";
            string is_ed = "false";
            string is_de = "false";

            int user_id = int.TryParse(httpContext.Session.GetString("user_id"), out int UserId) ? UserId : -1;
            if (!string.IsNullOrWhiteSpace(Menucode))
            {
                if (user_id == 1)
                {
                    is_vw = "true";
                    is_ad = "true";
                    is_ed = "true";
                    is_de = "true";
                }
                else
                {
                    var permission = (
                    from a in _context.tbl_user
                    join c in _context.tbl_user_user_menu
                    on a.user_id equals c.user_id
                    join b in _context.tbl_user_menu
                    on c.menu_id equals b.menu_id
                    where c.user_id == user_id
                    && (from m in _context.tbl_user_menu
                        where m.menu_code == Menucode
                        select m.menu_id).Distinct()
                    .Contains(c.menu_id)
                    select new
                    {
                        c.is_vw,
                        c.is_ad,
                        c.is_ed,
                        c.is_de
                    }).FirstOrDefault();

                    if (permission != null)
                    {
                        is_vw = permission.is_vw == "Y" ? "true" : "false";
                        is_ad = permission.is_ad == "Y" ? "true" : "false";
                        is_ed = permission.is_ed == "Y" ? "true" : "false";
                        is_de = permission.is_de == "Y" ? "true" : "false";
                    }

                }
            }
            return new PermissionVM
            {
                vpern = is_vw,
                apern = is_ad,
                epern = is_ed,
                dpern = is_de
            };
        }
        public string? GetSingleMenuPermission(string PageId, string Pern)
        {
            string? vpern;
            string? apern;
            string? epern;
            string? dpern;
            var permission = GetMenuPermission(PageId);
            if (permission == null)
            {
                vpern = "false";
                apern = "false";
                epern = "false";
                dpern = "false";
            }
            else
            {
                vpern = permission.vpern;
                apern = permission.apern;
                epern = permission.epern;
                dpern = permission.dpern;
            }
            return Pern == "V" ? vpern : Pern == "A" ? apern : Pern == "E" ? epern : Pern == "D" ? dpern : "false";
        }
        /***************************************************************************************************
        * Since : 2026-Jun-20
        ****************************************************************************************************/
         public string getAddEditDeleteAccess(string ilaka, string buttonList, string PageId, int recCount)
        {
            var sb = new StringBuilder();
            var perm = GetMenuPermission(PageId);
            //string vpern = perm.vpern;
            string apern = perm.apern ?? "false";
            string epern = perm.epern ?? "fasle";
            string dpern = perm.dpern ?? "false";

            string[] anyButtons = buttonList.Split('|');
            foreach (var btn in anyButtons)
            {
                switch (btn)
                {
                    case "ADD" when perm.apern == "true":
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnAdd"" class=""button bg-green"">Add New</button>");
                        break;

                    case "SET-AS-SENT" when perm.epern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnSetAsSent"" class=""button bg-blue"">Set As Sent</button>");
                        break;

                    case "SEND-TEST-MAIL" when perm.epern == "true":
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnSend"" class=""button bg-red"">Send Email</button>");
                        break;

                    case "EXPORT" when recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnExport"" class=""button bg-sky"">Export</button>");
                        break;

                    case "IMPORT" when perm.apern == "true" || perm.epern == "true":
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnImport"" class=""button bg-blue"">Import</button>");
                        break;

                    case "DOWNLOAD-FORMAT":
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnDownLoadFormat"" class=""button bg-sky"">Download Format</button>");
                        break;

                    case "ACT-DACT" when perm.epern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnActDact"" class=""button bg-org"">Deactivate</button>");
                        break;

                    case "SN-DEL" when perm.dpern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnDelete"" class=""button bg-red"">Delete log before 1 year</button>");
                        break;

                    case "DEL" when perm.dpern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnDelete"" class=""button bg-red"">Delete</button>");
                        break;

                    case "DEL-SD" when perm.dpern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnDeleteSD"" class=""button bg-red"">Delete</button>");
                        break;

                    case "CarryForward" when perm.epern == "true" && recCount > 0:
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnSave"" class=""button bg-blue"">Carry Forward</button>");
                        break;
                    case "BALANCE" when perm.vpern == "true":
                        _ = sb.AppendLine(@"<button type=""button"" id=""btnBalance"" class=""button bg-blue"">Balance</button>");
                        break;
                    default:
                        break;
                }
                /*
                if (btn == "ADD" && perm.apern == "true")
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnAdd"" id =""btnAdd"" class=""button bg-green"">Add New</button>");
                }

                if (btn == "SET-AS-SENT" && perm.epern == "true" && recCount > 0)
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnSetAsSent"" id =""btnSetAsSent"" class=""button bg-blue"">Set As Sent</button>");
                }
                if (btn == "SEND-TEST-MAIL" && perm.epern == "true")
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnSend"" id =""btnSend"" class=""button bg-red"">Send Email</button>");
                }
                if (btn == "EXPORT")
                { 
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnExport"" id =""btnExport"" class=""button bg-sky"">Export</button>"); 
                }
                if (btn == "DOWNLOAD-FORMAT")
                { 
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnDownLoadFormat"" id =""btnDownLoadFormat"" class=""button bg-sky"">Download Format</button>"); 
                }
                if (btn == "ACT-DACT" && perm.epern == "true" && recCount > 0)
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name=""btnActDact"" id =""btnActDact""  class=""button bg-org"">Deactivate</button>");
                }
                if (btn == "SN-DEL" && perm.dpern == "true" && recCount > 0)
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnDelete"" id =""btnDelete"" class=""button bg-red"">Delete log before 1 year</button>");
                }
                if (btn == "DEL" && perm.dpern == "true" && recCount > 0)
                {
                    _ = sb.AppendLine($@"<button type = ""button"" name =""btnDelete"" id =""btnDelete"" class=""button bg-red"">Delete</button>");
                }
                if (btn == "CarryForward" && perm.epern == "true" && recCount > 0)
                { _ = sb.AppendLine($@"<button type = ""button"" name =""btnSave"" id =""btnSave"" class=""button bg-blue"">Carry Forward</button>"); }
                */
            }
            return sb.ToString();
        }
        /***************************************************************************************************
        * Since : 2026-Jun-21
        ****************************************************************************************************/
         public bool HasPermission(string PageId, string? whatMode = "")
        {
            var perm = GetMenuPermission(PageId);
            string vpern = perm.vpern;
            string apern = perm.apern ?? "false";
            string epern = perm.epern ?? "fasle";
            string dpern = perm.dpern ?? "false";
            bool retValue = false;
            if (whatMode == "add")
            {
                if (GetSingleMenuPermission(PageId, "A") == "true") { retValue = true; }
            }
            else if (whatMode == "edit")
            {
                if (GetSingleMenuPermission(PageId, "E") == "true") { retValue = true; }
            }
            else if (whatMode == "delete")
            {
                if (GetSingleMenuPermission(PageId, "D") == "true") { retValue = true; }
            }
            else if (whatMode == "view")
            {
                if (GetSingleMenuPermission(PageId, "V") == "true") { retValue = true; }
            }
            return retValue;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-15
        ****************************************************************************************************/
        public void GetSingleMenuPermissionOld(string PageId, out string? vpern, out string? apern, out string? epern, out string? dpern)
        {
            // Deprecated
            var permission = GetMenuPermission(PageId);
            if (permission == null)
            {
                vpern = "false";
                apern = "false";
                epern = "false";
                dpern = "false";
            }
            else
            {
                vpern = permission.vpern;
                apern = permission.apern;
                epern = permission.epern;
                dpern = permission.dpern;
            }
        }

        public void DeleteExpiredResetToken(int UserId, string tType)
        {
            //not clear purpose of this function
            //should not need to clear expired tokens of all?
            //as trying to delete current user only
            var recordsToDelete = _context.tbl_user_reset_token
                                .Where(r => r.user_id == UserId && r.pwdorpin == tType)
                                .ToList();
            _context.tbl_user_reset_token.RemoveRange(recordsToDelete);
            _ = _context.SaveChanges();
            //CLOSE THE EXISTING INSTANCE
            _context.ChangeTracker.Clear();
            //END CLOSE THE EXISTING INSTANCE
        }
        public bool IsPasswordAlreadyUsed(int UserId, string PlainPwd)
        {
            bool isReused = false;
            // get all previous password hashes for this user
            var pre_pwds = _context.tbl_user_pwd_history
                .Where(p => p.user_id == UserId)
                .OrderByDescending(p => p.updated_date)
                .Select(p => p.pwd)
                .Take(5)
                .ToList();
            if (pre_pwds != null)
            {
                foreach (string? oldHash in pre_pwds)
                {
                    string result = CheckHash(oldHash, PlainPwd);
                    if (result == "true")
                    {
                        isReused = true;
                        break;
                    }
                }
                return isReused;
            }
            else
            {
                return false;
            }
        }

        public bool SavePasswordChange(int user_id, int emp_id, string pwd, string Location)
        {
            /* history table - Update all other is_current_one = Y to N  */
            _ = _context.tbl_user_pwd_history
            .Where(x => x.user_id == user_id)
            .ExecuteUpdate(s => s
                .SetProperty(x => x.is_current_one, "N")
            );
            _ = _context.SaveChanges();
            //CLOSE THE EXISTING INSTANCE
            _context.ChangeTracker.Clear();
            //END CLOSE THE EXISTING INSTANCE

            string hashedPassword = MakeHash(pwd);

            /* history table - Enter the changed information */
            _ = _context.tbl_user_pwd_history.Add(new tbl_user_pwd_history
            {
                Id = GblUtilities.UniqueID(),
                user_id = user_id,
                pwd = hashedPassword,
                updated_date = DateTime.Now,
                is_current_one = "Y"
            });
            _ = _context.SaveChanges();
            //CLOSE THE EXISTING INSTANCE
            _context.ChangeTracker.Clear();
            //END CLOSE THE EXISTING INSTANCE

            /* user table - update password */
            _ = _context.tbl_user
            .Where(u => u.user_id == user_id)
            .ExecuteUpdate(s => s
                .SetProperty(u => u.pwd, hashedPassword)
            );
            _ = _context.SaveChanges();

            if (Location == "ResetPassword")
            {
                /* token table - remove token*/
                DeleteExpiredResetToken(user_id, "PWD");
            }
            /*
            * Get employee name and email to send reset link
            */
            if (emp_id > 0)
            {
                string EmployeeName = _employeeService.GetEmployeeNameEmail(emp_id, "N");
                string SetEmail = _employeeService.GetEmployeeNameEmail(emp_id);
                string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                if (string.IsNullOrWhiteSpace(ToEmail))
                {
                    /* send notification email about changed password */
                    string Subject = Lang.EMAIL_ACCOUNT_PWD_RESETED_SUBJECT;
                    string Message = Lang.EMAIL_ACCOUNT_PWD_RESETED_MESSAGE;
                    Message = Message.Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal);
                    if (!string.IsNullOrWhiteSpace(ToEmail))
                    {
                        string emst = _emailService.SendEmail(Location, ToEmail, Subject, Message);
                    }
                }
            }

            return true;
        }
        public bool SavePinChange(int UserId, int EmpId, string pin, string Location)
        {
            string hashedPin = MakeHash(pin);

            /* user table - update pin */
            _ = _context.tbl_user
            .Where(u => u.user_id == UserId)
            .ExecuteUpdate(s => s
                .SetProperty(u => u.pin, hashedPin)
            );
            _ = _context.SaveChanges();

            if (Location == "ResetPin")
            {
                /* token table - remove token*/
                DeleteExpiredResetToken(UserId, "PIN");
            }
            /*
            * Get employee name and email to send reset link
            */
            if (EmpId > 0)
            {
                string EmployeeName = _employeeService.GetEmployeeNameEmail(EmpId, "N");
                string SetEmail = _employeeService.GetEmployeeNameEmail(EmpId);
                string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;
                if (string.IsNullOrWhiteSpace(ToEmail))
                {
                    /* send notification email about changed pin */
                    string Subject = Lang.EMAIL_ACCOUNT_PIN_RESETED_SUBJECT;
                    string Message = Lang.EMAIL_ACCOUNT_PIN_RESETED_MESSAGE;
                    Message = Message.Replace("<[EMPLOYEE-NAME]>", EmployeeName, StringComparison.Ordinal);
                    if (!string.IsNullOrWhiteSpace(ToEmail))
                    {
                        string emst = _emailService.SendEmail(Location, ToEmail, Subject, Message);
                    }
                }
            }

            return true;
        }
        /**
         * Validate password
         * 2026-May-28
         */
        public static string ValidatePassword(string password, string output = "")
        {
            string result = ValidatePasswordHelper(password);
            return !string.IsNullOrWhiteSpace(output) ? result : !string.IsNullOrWhiteSpace(result) ? "weak" : "";
        }

        public static string ValidatePasswordHelper(string password)
        {
            // Check length between 8 and 20
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 20)
            {
                return Lang.msg_pwd_length_not_valid;
            }
            // 3. No whitespace allowed
            /*
            if (password.Any(char.IsWhiteSpace))
                return "Password must not contain spaces or whitespace.";
            */
            if (Regex.IsMatch(password, @"\s"))
            {
                return Lang.msg_pwd_must_not_space;
            }
            // Check for Unicode (non-ASCII)
            if (Regex.IsMatch(password, @"[^\u0000-\u007F]"))
            {
                return Lang.msg_pwd_must_not_unicode;
            }
            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return Lang.msg_pwd_must_upper;
            }
            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return Lang.msg_pwd_must_lower;
            }
            // Check for at least one digit
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return Lang.msg_pwd_must_digit;
            }
            // Check for at least one special character
            if (!Regex.IsMatch(password, @"[~`!@#$%^&*()\-+={}\[\]|\\:;""'<>,.?/_?]"))
            {
                return Lang.msg_pwd_must_special;
            }
            return "";       // Valid Password 
        }

        public static string ValidatePin(string pin, string output = "")
        {
            int result = ValidatePinHelper(pin);
            return string.IsNullOrWhiteSpace(output) ? result.ToString() : result < 2 ? "weak" : "";
        }
        public static int ValidatePinHelper(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin) || !pin.All(char.IsDigit))
            {
                return 0; // not numeric or empty
            }
            var digits = pin.Select(c => int.Parse(c.ToString())).ToList();

            bool allSame = digits.All(d => d == digits[0]);

            bool ascending = true;
            for (int i = 0; i < digits.Count - 1; i++)
            {
                if (digits[i] + 1 != digits[i + 1])
                {
                    ascending = false;
                    break;
                }
            }

            bool descending = true;
            for (int i = 0; i < digits.Count - 1; i++)
            {
                if (digits[i] - 1 != digits[i + 1])
                {
                    descending = false;
                    break;
                }
            }

            int passed = 0;

            // If not trivial (all same, ascending, descending)
            if (!(allSame || ascending || descending))
            {
                passed++;

                // Check digit variety
                int uniqueCount = digits.Distinct().Count();
                if (uniqueCount > 3) { passed++; }
                if (uniqueCount > 4) { passed++; }
            }

            // Length check
            if (pin.Length >= 6) { passed++; }

            return passed; // higher = stronger
        }
        public static string MakeHash(string Plain)
        {
            string Hashed = "";
            var hasher = new PasswordHasher<string>();
            if (!string.IsNullOrWhiteSpace(Plain))
            {
                Hashed = hasher.HashPassword("nu11-us3r", Plain);
            }
            return Hashed;
        }
        public static string CheckHash(string Hashed, string Plain)
        {
            var hasher = new PasswordHasher<string>();
            string HashIt = hasher.HashPassword("nu11-us3r", Plain);

            var result = hasher.VerifyHashedPassword("nu11-us3r", Hashed, Plain);
            if (result == PasswordVerificationResult.Success)
            {
                return "true";// Password is correct
            }
            else
            {
                return "false"; //Password is invalid
            }
        }
        /**
         * Insert user login failed values after unsuccessful login
         */
        public void InsertUserLoginFail(string parm_username)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            _ = _context.tbl_user_login_fail.Add(new tbl_user_login_fail
            {
                Id = GblUtilities.UniqueID(),
                username = parm_username,
                on_date = DateTime.Now,
                ip = httpContext.Connection.RemoteIpAddress.ToString(),
                user_agent = httpContext.Request.Headers.UserAgent.ToString()
            });
            _ = _context.SaveChanges();
        }
        /**
	     * Delete user login failed values after successful login
	     */
        public void DeleteUserLoginFail(string username)
        {
            // Find all failed login records for this user
            var records = _context.tbl_user_login_fail.Where(u => u.username == username);
            _context.tbl_user_login_fail.RemoveRange(records);
            _ = _context.SaveChanges();
        }
        /**
         * Insert user login failed values after unsuccessful login
         */
        public void InsertUserLoginLog()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            int user_id = int.TryParse(httpContext.Session.GetString("user_id"), out int UserId) ? UserId : 0;
            if (!string.IsNullOrWhiteSpace(httpContext.Session.GetString("login_id")) && user_id > 0)
            {
                _ = _context.tbl_user_login_log.Add(new tbl_user_login_log
                {
                    ID = httpContext.Session.GetString("login_id"),
                    user_id = user_id,
                    in_date = DateTime.Now,
                    ip = httpContext.Connection.RemoteIpAddress?.ToString(),
                    user_agent = httpContext.Request.Headers.UserAgent.ToString()
                });
                _ = _context.SaveChanges();
            }
        }
        /**
         * update user login values for successful logut
         */
        public string UpdateUserLoginLog()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (!string.IsNullOrWhiteSpace(httpContext.Session.GetString("login_id")))
            {
                _ = _context.tbl_user_login_log
                .Where(x => x.ID == httpContext.Session.GetString("login_id"))
                .ExecuteUpdate(s => s
                    .SetProperty(x => x.in_date, DateTime.Now)
                );
                _ = _context.SaveChanges();
                return "true";
            }
            else
            {
                return "false";
            }
        }
        /**
	     * Last Login From
	     * 2026-Jun-01
	     */
        public void LastLoginLocation(int UserId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user_log = _context.tbl_user_login_log
            .Where(p => p.user_id == UserId)
            .OrderByDescending(p => p.in_date)
            .Take(1)
            .FirstOrDefault();

            if (user_log != null)
            {
                httpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "user_last_login_date"), user_log.in_date.ToString() ?? "");
                httpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "user_last_login_ip"), string.Concat(user_log.ip, "[ ", user_log.user_agent, " ]"));
            }
            else
            {
                httpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "user_last_login_date"), "");
                httpContext.Session.SetString(string.Concat(_appSettings.SITE_SESSION, "user_last_login_ip"), "");
            }

        }
        /**
           * Last password change
           * 2026-Jun-02
           */
        public string GetLastPasswordChange(int UserId)
        {
            string fnStr;
            // latest password history record
            var rsfn = _context.tbl_user_pwd_history
                .Where(p => p.user_id == UserId && p.is_current_one == "Y")
                .OrderByDescending(p => p.updated_date).FirstOrDefault();
            if (rsfn != null)
            {
                int expireDays = Convert.ToInt32(_globalOptionServices.OptionServices["op_pwd_expire_days"]);
                if (rsfn?.updated_date.HasValue == true)
                {
                    DateTime lastUpdatedDate = rsfn.updated_date.Value;
                    int daysDiff = (DateTime.Now - lastUpdatedDate).Days;
                    int daysRemaining = expireDays - daysDiff;
                    if (daysRemaining >= 0)
                    {
                        fnStr = Lang.msg_next_password_change_due
                            .Replace("<[last-updated-date]>", lastUpdatedDate.ToString(_appSettings.DATE_FORMAT), StringComparison.Ordinal)
                            .Replace("<[due-date]>", daysRemaining.ToString(), StringComparison.Ordinal);

                        fnStr = $"<span class=\"green\">{fnStr}</span>";
                    }
                    else
                    {
                        int overdueDays = Math.Abs(daysRemaining);
                        fnStr = Lang.msg_next_password_change_due
                            .Replace("<[last-updated-date]>", lastUpdatedDate.ToString(_appSettings.DATE_FORMAT), StringComparison.Ordinal)
                            .Replace("<[due-date]>", overdueDays.ToString(), StringComparison.Ordinal)
                            .Replace("<[due-date-limit]>", expireDays.ToString(), StringComparison.Ordinal);

                        fnStr = $"<span class=\"red\">{fnStr}</span>";
                    }
                }
                else {
                    fnStr = Lang.msg_no_password_change_info;
                }
            }
            else
            {
                fnStr = Lang.msg_no_password_change_info;
            }
            return fnStr;
        }

        public SelectList GuardUserType(string selvalue = "")
        {
            string fv = "Administrator"; string ft = "Administrator";
            string sv = "Guard"; string st = "Guard";

            var statusList = new List<object>
            {
                new { Value = fv, Text = ft },
                new { Value = sv, Text = st }
            };

            return new SelectList(statusList, "Value", "Text", selvalue);

        }

    }
}
