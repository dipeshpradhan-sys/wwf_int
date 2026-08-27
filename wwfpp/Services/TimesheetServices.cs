using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Services
{
    public class TimesheetServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly SettingsServices _settingsServices;
        private readonly EmployeeServices _employeeServices;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly ISendEmails _sendEmails;
        private readonly string sFromName = "";  //string for fromName
        private readonly string sFromEmail = "";  //string for fromEmail
        public TimesheetServices(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            SettingsServices settingsServices,
            EmployeeServices employeeServices,
            GlobalOptionServices globalOptionServices,
            IOptions<SmtpSettings> options,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _settingsServices = settingsServices;
            _employeeServices = employeeServices;
            _globalOptionServices = globalOptionServices;
            _httpContextAccessor = httpContextAccessor;

            var settings = options.Value;
            sFromName = settings.FromName;
            sFromEmail = settings.FromEmail;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Timesheet Alert notificationto fill out => sendTimesheetAlert
        ****************************************************************************************************/
        public void SendTimesheetAlert()
        {
            var today = DateTime.Today;
            int fnCurDay = today.Day;
            int fnCurMonth = today.Month;
            int fnCurYear = today.Year;

            string isRecord = "N";
            string isExecute = "N";

            DateTime executeDate = today.AddMonths(-1);

            // Check if already executed
            var lastAlert = _context.tbl_alert_execute_date.FirstOrDefault();
            if (lastAlert != null)
            {
                isRecord = "Y";
                executeDate = lastAlert.last_alert_timesheet_date;
            }

            int fnChkTsMonth = 0;
            int fnChkTsYear = 0;
            DateTime fnChkStartDate = DateTime.MinValue;
            DateTime fnChkLastDate = DateTime.MinValue;

            if (fnCurDay < 7) // First week
            {
                fnChkStartDate = today.AddDays(-fnCurDay + 1); // day = 01
                fnChkLastDate = fnChkStartDate.AddDays(5);     // day = 06

                var fnChkTsDate = today.AddMonths(-1);
                fnChkTsMonth = fnChkTsDate.Month;
                fnChkTsYear = fnChkTsDate.Year;

                if (!(fnChkStartDate <= executeDate && executeDate <= fnChkLastDate))
                {
                    isExecute = "Y";
                }
            }
            else if (fnCurDay > 22) // Last week
            {
                fnChkStartDate = new DateTime(fnCurYear, fnCurMonth, 23);
                fnChkLastDate = new DateTime(fnCurYear, fnCurMonth, DateTime.DaysInMonth(fnCurYear, fnCurMonth));

                fnChkTsMonth = fnCurMonth;
                fnChkTsYear = fnCurYear;

                if (!(fnChkStartDate <= executeDate && executeDate <= fnChkLastDate))
                {
                    isExecute = "Y";
                }
            }

            if (isExecute == "Y")
            {
                // Call your email alert function
                EmailTimesheetAlert(fnChkTsMonth, fnChkTsYear);

                if (isRecord == "Y")
                {
                    lastAlert.last_alert_timesheet_date = today;
                    _context.tbl_alert_execute_date.Update(lastAlert);
                }
                else
                {
                    _ = _context.tbl_alert_execute_date.Add(new tbl_alert_execute_date
                    {
                        last_alert_timesheet_date = today
                    });
                }

                _ = _context.SaveChanges();
            }
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * emailTimesheetAlert
        ****************************************************************************************************/
        public void EmailTimesheetAlert(int month, int year)
        {
            string strPeriod = $"{GblUtilities.MonthName(month)} {year}";

            // Find active employees who have NOT submitted timesheet_app for this month/year
            var employees = _context.tbl_employee
                .Where(e => e.emp_status == "A"
                    && !_context.tbl_employee_timesheet_app
                        .Any(a => a.emp_id == e.emp_id
                               && a.emp_month == month
                               && a.emp_year == year))
                .Select(e => e.emp_id)
                .ToList();

            if (employees.Any())
            {
                int fCnt = 0;
                foreach (var empId in employees)
                {
                    fCnt++;

                    string strSubject = Lang.EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_SUBJECT
                        .Replace("<[PEROID]>", strPeriod, StringComparison.OrdinalIgnoreCase);

                    string strMessage = Lang.EMAIL_EMPLOYEE_TIMESHEET_NTR_DUE_MESSAGE
                        .Replace("<[PEROID]>", strPeriod, StringComparison.OrdinalIgnoreCase);

                    string SetEmail = _employeeServices.GetEmployeeNameEmail(empId);
                    string ToEmail = string.IsNullOrWhiteSpace(SetEmail) ? "" : SetEmail;

                    string fnId = $"{GblUtilities.UniqueID()}{fCnt}";
                    DateTime fnCurDate = DateTime.Now;

                    // Save in DB 
                    _ = _context.tbl_email_list.Add(new tbl_email_list
                    {
                        id = fnId,
                        from_add = $"{sFromName} <{sFromEmail}>",
                        to_add = ToEmail,
                        subject = strSubject,
                        e_message = strMessage,
                        submit_date = DateTime.Now,
                        status = "N",
                        category = "To Fill Timesheet",
                    });
                    _ = _context.SaveChanges();
                    _context.ChangeTracker.Clear();
                }
            }
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getSubmitCounter
        ****************************************************************************************************/
        public int GetTimeSheetCounter(int empId, int empYear, int empMonth)
        {
            int maxCounter = _context.tbl_employee_timesheet_app
                .Where(ts => ts.emp_id == empId
                          && ts.emp_year == (short)empYear
                          && ts.emp_month == (byte)empMonth
                          && ts.app_dec == "a")
                .Max(ts => ts.submit_counter) ?? 0;
            maxCounter++;
            return maxCounter;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getCurrentMaxCounter
        ****************************************************************************************************/
        public int GetCurrentMaxCounter(int empId, int empYear, int empMonth)
        {
            int maxCounter = _context.tbl_employee_timesheet_sub
                .Where(ts => ts.emp_id == empId
                          && ts.emp_year == (short)empYear
                          && ts.emp_month == (byte)empMonth)
                .Max(ts => ts.submit_counter) ?? 0;
            maxCounter++;
            return maxCounter;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getTimesheetStatus
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        public string GetTimesheetMessageStatusPrevious(string prevStatus)
        {
            return prevStatus switch
            {
                "notsaved" => "Previous timesheet status: Not saved.",
                "justsaved" => "Previous timesheet status: Saved but not sent for approval.",
                "pending" => "Previous timesheet status: Pending.",
                "declined" => "Previous timesheet status: Declined.",
                "inactive" => "Previous timesheet status: Inactive.",
                _ => "Previous timesheet status: Not Filled."
            };
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getTimesheetMessageStatus
        ****************************************************************************************************/
        public string GetTimesheetMessageStatus(string curStatus)
        {
            return curStatus switch
            {
                "justsaved" => "Timesheet status: Saved but not sent for approval.",
                "pending" => "Timesheet status: Pending.",
                "active" => "Timesheet status: Approved.",
                "declined" => "Timesheet status: Declined.",
                "notsaved" => "Timesheet status: Not Saved.",
                "inactive" => "Timesheet status: Inactive.",
                _ => "Timesheet status: Not Filled"
            };
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getLeaveDayTime
        ****************************************************************************************************/


        /***************************************************************************************************
        * Since : 2026-Aug-11
        *  Timesheet filled up  : getFilledTime
        ****************************************************************************************************/
        public string GetFilledTime(int year, int month, int day, int empId)
        {
            string fnStr = "false";
            int submitCounter = 1;

            var subQuery = _context.tbl_employee_timesheet_sub
                .Where(t => t.emp_year == year
                            && t.emp_month == month
                            && t.emp_day == day
                            && t.emp_id == empId
                            && (t.is_active == "N" || t.is_active == "A")
                            && t.time_hours != 0).FirstOrDefault();
            if (subQuery != null)
            {
                fnStr = "true";
                submitCounter = Convert.ToInt32(subQuery.submit_counter);
            }
            var mainQuery = _context.tbl_employee_timesheet_main
                .Where(t => t.emp_year == year
                            && t.emp_month == month
                            && t.emp_day == day
                            && t.emp_id == empId
                            && t.submit_counter == submitCounter).FirstOrDefault();
            if (mainQuery != null && mainQuery.leave_type_id == 15) { fnStr = "true"; }
            return fnStr;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getPreviousFundTime
        ****************************************************************************************************/


        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getPreviousFundOvertime
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getTimesheetLeaveHrs
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getBalanceTillDateDay
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getDayOverTimeHrs
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getDayTimeHrs
        ****************************************************************************************************/
        public double GetDayTimeHrs(int empId, int year, int month, int day, int fundId, int counter)
        {
            double timeHour = _context.tbl_employee_timesheet_sub
                .Where(t => t.emp_id == empId
                            && t.emp_year == year
                            && t.emp_month == month
                            && t.emp_day == day
                            && t.fund_id == fundId
                            && t.submit_counter == counter)
                .Select(t => t.time_hours).FirstOrDefault() ?? 0;
            return timeHour;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getThisWeekActual
        ****************************************************************************************************/
        public double GetThisWeekActual(int empId, int year, int weekMonth, int fundId, int counter)
        {
            double totalHours = _context.tbl_employee_timesheet_sub
                .Where(t => t.emp_id == empId
                            && t.emp_year == year
                            && t.emp_month == weekMonth
                            && t.fund_id == fundId
                            && t.submit_counter == counter)
                        .Sum(t => t.time_hours) ?? 0;
            return totalHours;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * This week overtime hours
        * getThisWeekActualOvertime
        ****************************************************************************************************/
        public double GetThisWeekActualOvertime(int empId, int year, int weekMonth, int fundId, int counter)
        {
            double totalOvertime = _context.tbl_employee_timesheet_sub
                .Where(t => t.emp_id == empId
                            && t.emp_year == year
                            && t.emp_month == weekMonth
                            && t.fund_id == fundId
                            && t.submit_counter == counter)
                        .Sum(t => t.overtime_hours) ?? 0;
            return totalOvertime;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * GET HOURS IN A DAY SAVED IN TIMESHEET
        ****************************************************************************************************/
        public (double NormalHours, double OvertimeHours) TimesheetHoursInADay(int empId, int month, int day, int year, int counter)
        {
            var result = _context.tbl_employee_timesheet_sub
                .Where(t => t.emp_id == empId
                            && t.emp_year == year
                            && t.emp_month == month
                            && t.emp_day == day
                            && t.submit_counter == counter)
                .GroupBy(t => 1) // dummy group to allow SUM
                .Select(g => new
                {
                    NormalHours = g.Sum(x => (int?)x.time_hours) ?? 0,
                    OvertimeHours = g.Sum(x => (int?)x.overtime_hours) ?? 0
                })
                .FirstOrDefault();

            double strTotalNtHoursDay = result?.NormalHours ?? 0;
            double strTotalOtHoursDay = result?.OvertimeHours ?? 0;

            return (strTotalNtHoursDay, strTotalOtHoursDay);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * GET TIMESHEET REMARKS =>  getTimesheetRemarks
        ****************************************************************************************************/
        public string GetTimesheetRemarks(int empId, int fiscalYear, int weekOrMonth, int submitCounter)
        {
            var query = _context.tbl_employee_timesheet_app
                .Where(t =>
                    t.emp_id == empId && t.submit_counter == submitCounter &&
                    t.emp_year == fiscalYear && t.emp_month == weekOrMonth
                );
            string remark = query.Select(t => t.app_remarks).FirstOrDefault() ?? string.Empty;
            return remark;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getTimesheetSubmitList
        ****************************************************************************************************/


        /***************************************************************************************************
        * Since : 2026-Aug-11
        * ONLY TIMESHEET FILLED EMPLOYEE 'IT IS NOT IN USE CURRENTLY'
        * getEmployeeFilledTimesheet
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * IT IS NOT IN USE CURRENTLY'
        * hasEmployeeFilledAllTimesheet
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * User first timesheet submit year/month or Fiscal/Week => getCurEmployeeTimesheetStartPoint
        ****************************************************************************************************/
        public string GetCurEmployeeTimesheetStartPoint(int empId, int year, int month)
        {
            var query = _context.tbl_employee_salary_extra_settings
                .Where(s => s.emp_id == empId && s.emp_year == year && s.emp_month == month);
            bool exists = query.Any();
            return exists ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * TIMESHEET LISTING ON DASHBOARD => getTimesheetSubmitList
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getTimesheetRemarks
        ****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        /***************************************************************************************************/
        public tbl_setting_limit_hrs GetLimitHoursSetting()
        {
            return _context.tbl_setting_limit_hrs.FirstOrDefault();
        }
    }
}
