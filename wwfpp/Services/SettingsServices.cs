using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Account;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace wwfpp.Services
{
    public class SettingsServices
    {
        public class FiscalYearSetting
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SettingsServices(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            GlobalOptionServices globalOptionServices,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _globalOptionServices = globalOptionServices;
            _httpContextAccessor = httpContextAccessor;
        }

        /***************************************************************************************************
        *Set Fiscal Year
        ****************************************************************************************************
        */
        public void SetFiscalYear()
        {
            // FOR CURRENT FISCAL YEAR
            var fnrs = _context.tbl_fiscal_year
                   .Where(ts => ts.is_active == "Y")
                   .FirstOrDefault();
            var _Session = _httpContextAccessor.HttpContext.Session;
            if (fnrs != null)
            {
                _Session.SetString("fiscal_year", fnrs.fiscal_year);
                _Session.SetString("date_from", DateformatToDt(fnrs.date_from.ToString()));
                _Session.SetString("date_to", DateformatToDt(fnrs.date_to.ToString()));
                _Session.SetString("fiscal_year_abb", fnrs.fiscal_year_abb);
            }
            else
            {
                _Session.SetString("fiscal_year", "");
                _Session.SetString("date_from", "");
                _Session.SetString("date_to", "");
                _Session.SetString("fiscal_year_abb", "");
            }
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Contribution: 
        ****************************************************************************************************/
        public string GetFiscalYearByDate(DateTime givenDate)
        {
            string fiscalYear = _context.tbl_fiscal_year
                .Where(fy => givenDate >= fy.date_from && givenDate <= fy.date_to)
                .Select(fy => fy.fiscal_year)
                .FirstOrDefault() ?? "";
            return fiscalYear;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-10
        * Contribution: 
        ****************************************************************************************************/
        public FiscalYearSetting GetFiscalStartEndDate(string fiscalYear)
        {
            var opt = _context.tbl_fiscal_year
                .Where(e => e.fiscal_year == fiscalYear)
                .Select(e => new FiscalYearSetting
                {
                    StartDate = Convert.ToDateTime(e.date_from),   // assuming date_from is DateTime in your EF model
                    EndDate = Convert.ToDateTime(e.date_to)      // assuming date_to is DateTime in your EF model
                })
                .FirstOrDefault();
            return opt;
        }
        /***************************************************************************************************
        *Set Fiscal Year
        ****************************************************************************************************
        */
        public void SetCarryForwardYear()
        {
            // FOR CURRENT FISCAL YEAR
            var fnrs = _context.tbl_calendar_year
                   .Where(ts => ts.calendar_year_abb == "Y")
                   .FirstOrDefault();
            var _Session = _httpContextAccessor.HttpContext.Session;
            if (fnrs != null)
            {
                _Session.SetString("calendar_year", fnrs.calendar_year);
                _Session.SetString("calendar_date_from", DateformatToDt(fnrs.calendar_date_from.ToString()));
                _Session.SetString("calendar_date_to", DateformatToDt(fnrs.calendar_date_to.ToString()));
                _Session.SetString("calendar_abbr", fnrs.calendar_year_abb);
            }
            else
            {
                _Session.SetString("calendar_year", "");
                _Session.SetString("calendar_date_from", "");
                _Session.SetString("calendar_date_to", "");
                _Session.SetString("calendar_abbr", "");
            }
        }

        /***************************************************************************************************
        *Set Timesheet Type
        ****************************************************************************************************
        */
        public void SetTimesheetType()
        {
            var fnrs = _context.tbl_setting_timesheet_type
                 .FirstOrDefault();
            var _Session = _httpContextAccessor.HttpContext.Session;
            if (fnrs != null)
            {
                _Session.SetString("timesheet_type", fnrs.timesheet_type);
                _Session.SetString("first_day_of_week", fnrs.first_day_of_week.ToString());
            }
            else
            {
                _Session.SetString("timesheet_type", "");
                _Session.SetString("first_day_of_week", "1");
            }

            var today = DateTime.Now;
            if (_Session.GetString("timesheet_type") == "weekly")
            {
                var weekName = _context.tbl_calendar_setting_weekly
                    .Where(q => today >= q.period_start_date && today <= q.period_end_date)
                    .Select(q => q.week_name)
                    .FirstOrDefault();
                if (weekName > 0)
                {
                    _Session.SetString("current_week", weekName.ToString());
                }
                else
                {
                    _Session.SetString("current_week", "");
                }

            }
            else if (_Session.GetString("timesheet_type") == "biweekly")
            {
                var biWeekName = _context.tbl_calendar_setting_biweekly
                    .Where(q => today >= q.period_start_date && today <= q.period_end_date)
                    .Select(q => q.week_name)
                    .FirstOrDefault();
                if (biWeekName > 0)
                {
                    _Session.SetString("current_week", biWeekName.ToString());
                }
                else
                {
                    _Session.SetString("current_week", "");
                }
            }
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        ****************************************************************************************************/
        public string DateformatToDt(string pdbDate)
        {
            string formattedDate = "";
            if (!string.IsNullOrWhiteSpace(pdbDate))
            {
                //replace
                string DbDate = pdbDate.Replace("-", "/");
                DbDate = DbDate.Replace("\\", "/");
                DbDate = DbDate.Replace(".", "/");
                DbDate = DbDate.Replace(";", "/");
                DbDate = DbDate.Replace(",", "/");

                string[] Parts = DbDate.Split('/');

                // will come 2026-06-01 (yyyy-MM-dd) from db
                //need to change to MM/dd/yyyy or dd/MM/yyyy as setting provided
                DateTime sysDate = DateTime.Parse("2016-10-30");
                string newDtPart = sysDate.ToString().Substring(0, 2);
                if (int.TryParse(newDtPart, out int newDtInt))
                {
                    string yearpart = Parts[2].Substring(0, 4);
                    //parsed successfully
                    if (newDtInt == 30)
                    {
                        //Format coming is dd/MM/yyyy

                        if (_appSettings.DATE_FORMAT == "MM/dd/yyyy")
                        {
                            formattedDate = GblUtilities.AddLeadingZero(Parts[1]) + "/" + GblUtilities.AddLeadingZero(Parts[0]) + "/" + yearpart;
                        }
                        if (_appSettings.DATE_FORMAT == "dd/MM/yyyy")
                        {
                            formattedDate = GblUtilities.AddLeadingZero(Parts[0]) + "/" + GblUtilities.AddLeadingZero(Parts[1]) + "/" + yearpart;
                        }
                    }
                    else
                    {
                        //Format coming is MM/dd/yyyy
                        if (_appSettings.DATE_FORMAT == "MM/dd/yyyy")
                        {
                            formattedDate = GblUtilities.AddLeadingZero(Parts[0]) + "/" + GblUtilities.AddLeadingZero(Parts[1]) + "/" + yearpart;
                        }
                        if (_appSettings.DATE_FORMAT == "dd/MM/yyyy")
                        {
                            formattedDate = GblUtilities.AddLeadingZero(Parts[1]) + "/" + GblUtilities.AddLeadingZero(Parts[0]) + "/" + yearpart;
                        }
                    }

                }
                else
                {
                    formattedDate = pdbDate; //  handle error
                }
            }
            return formattedDate;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        ****************************************************************************************************/
        public string DateformatToDb(string pdbDate)
        {
            string formattedDate = "";
            if (!string.IsNullOrWhiteSpace(pdbDate))
            {
                string[] Parts = pdbDate.Split('/');

                if (_appSettings.DATE_FORMAT == "MM/dd/yyyy")
                {
                    formattedDate = Parts[2] + "/" + GblUtilities.AddLeadingZero(Parts[0]) + "/" + GblUtilities.AddLeadingZero(Parts[1]);
                }
                if (_appSettings.DATE_FORMAT == "dd/MM/yyyy")
                {
                    formattedDate = Parts[2] + "/" + GblUtilities.AddLeadingZero(Parts[1]) + "/" + GblUtilities.AddLeadingZero(Parts[0]);
                }
            }
            return formattedDate;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public SelectList GetYears(int? yearIndex)
        {
            var firstFiscalYear = _context.tbl_fiscal_year
                .OrderBy(f => f.date_from)
                .Select(f => f.fiscal_year)
                .FirstOrDefault();

            int yearFrom;
            if (!string.IsNullOrEmpty(firstFiscalYear))
            {
                var parts = firstFiscalYear.Split('/');
                yearFrom = int.Parse(parts[0]) - 1;
            }
            else
            {
                yearFrom = DateTime.Now.Year - 1;
            }
            int yearTo = DateTime.Now.Year + 1;

            var items = new List<SelectListItem>();

            // descending order
            for (int currentYear = yearTo; currentYear > yearFrom; currentYear--)
            {
                items.Add(new SelectListItem
                {
                    Value = currentYear.ToString(),
                    Text = currentYear.ToString(),
                    Selected = (yearIndex.HasValue && currentYear == yearIndex.Value)
                });
            }

            return new SelectList(items, "Value", "Text", yearIndex);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public SelectList GetMonths(int? monthIndex = null)
        {
            string[] monthNames = GblUtilities.PossibleMonths("en");
            var items = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                items.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = monthNames[i - 1],
                    Selected = (monthIndex.HasValue && monthIndex.Value == i)
                });
            }
            return new SelectList(items, "Value", "Text", monthIndex);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public string GetCalendarDays(short cal_year, byte cal_month)
        {
            int days = 0;
            var sb = new System.Text.StringBuilder();
            if (cal_year > 0 && cal_month > 0)
            {
                days = DateTime.DaysInMonth(cal_year, cal_month);
                for (int i = 1; i <= days; i++)
                {
                    string valDay = "";
                    var date = new DateTime(cal_year, cal_month, i);

                    //get holiday 
                    valDay = (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? "W" : i.ToString();
                    if (valDay != "W")
                    {
                        var isData = _context.tbl_setting_holidays.FirstOrDefault(u => u.holiday_date == date);
                        if (isData != null) { valDay = "H"; }
                    }
                    string style = (valDay is "H" or "W") ? "lpink" : "lgrey";

                    _ = sb.AppendLine($@"<div class=""col-auto"">
                        <input type=""text"" name=""d{i}"" id=""d{i}"" maxlength=""2"" size=""2"" 
                               value=""{valDay}"" class=""form-control @style"" readonly=""readonly"" />
                        </div>");
                }
                for (int i = days + 1; i <= 31; i++)
                {
                    _ = sb.AppendLine($@"<div class=""col-auto"">
                        <input type=""text"" name=""{days + i}"" id=""{days + i}"" maxlength=""2"" size=""2"" 
                               value="""" class=""form-control lgrey"" readonly=""readonly"" />
                        </div>");
                }
            }

            return sb.ToString();
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public SelectList GetFiscalYears(string? selFiscal)
        {
            var FiscalYear = _context.tbl_fiscal_year
                .OrderByDescending(f => f.fiscal_year)
                .ThenByDescending(f => f.date_from)
                .Select(f => new { f.fiscal_year, f.fiscal_year_abb })
                .ToList();
            return new SelectList(FiscalYear, "fiscal_year", "fiscal_year_abb", selFiscal);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public bool GetTimesheetDataExist(DateTime parm_date)
        {
            short emp_year = (short)parm_date.Year;
            byte emp_month = (byte)parm_date.Month;
            bool exists = false;
            string timesheet_type = _globalOptionServices.OptionServices["op_timesheet_type"];
            if (timesheet_type is "weekly" or "biweekly")
            {
                //get fiscal_year will be used here only
                // need to do here
            }
            else
            {
                var recordsExist = _context.tbl_employee_timesheet_sub
                .FirstOrDefault(u => u.emp_year == emp_year && u.emp_month == emp_month);
                exists = recordsExist != null;
            }
            return exists;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-22
        ****************************************************************************************************/
        public bool GetTimesheetDataExistEmployee(string parmFiscalYear, DateTime parmDate, int parmEmpId)
        {
            short emp_year = (short)parmDate.Year;
            byte emp_month = (byte)parmDate.Month;
            byte sDay = (byte)parmDate.Day;
            bool exists = false;
            string timesheet_type = _globalOptionServices.OptionServices["op_timesheet_type"];
            if (timesheet_type is "weekly" or "biweekly")
            {
                //get fiscal_year will be used here only
                // need to do here
            }
            else
            {
                var recordsExist = _context.tbl_employee_timesheet_sub.FirstOrDefault(
                    u => u.emp_year == emp_year && u.emp_month == emp_month
                    && u.emp_day == sDay && u.emp_id == parmEmpId);
                exists = recordsExist != null;
            }
            return exists;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public string CheckDateWithinFiscalYear(DateTime parm_date, string fiscal_year)
        {
            string Within = "";
            var fiscalYears = _context.tbl_fiscal_year.Where(u => u.fiscal_year == fiscal_year).FirstOrDefault();

            if (fiscalYears == null)
            {
                Within = "Fiscal year date range not defined";
            }
            else
            {
                DateTime from_date = Convert.ToDateTime(fiscalYears.date_from);
                DateTime to_date = Convert.ToDateTime(fiscalYears.date_to);
                if (parm_date.Date >= from_date.Date && parm_date.Date <= to_date.Date)
                {
                    /*silent*/
                }
                else
                {
                    Within = Lang.msg_provide_date_not_within_range;//"Please provide the date within fiscal year <[FISCAL-START-DATE]> and <[FISCAL-END-DATE]>.";
                    Within = Within.Replace("<[FISCAL-START-DATE]>", from_date.ToString(), StringComparison.Ordinal);
                    Within = Within.Replace("<[FISCAL-END-DATE]>", to_date.ToString(), StringComparison.Ordinal);
                }
            }
            return Within;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-17
        ****************************************************************************************************/
        public bool GetCalenarDataExist(string parm_fiscal_year)
        {
            bool exists = false;
            string timesheet_type = _globalOptionServices.OptionServices["op_timesheet_type"];
            if (timesheet_type == "weekly" || timesheet_type == "biweekly")
            {
                //get fiscal_year will be used here only
                // need to do here
            }
            else
            {
                //need to get start year/month and end year/month
                var records = _context.tbl_fiscal_year
                .FirstOrDefault(u => u.fiscal_year == parm_fiscal_year);
                if (records != null)
                {
                    DateTime start_date = Convert.ToDateTime(records.date_from);
                    DateTime end_date = Convert.ToDateTime(records.date_to);
                    var query =
                        from c in _context.tbl_calendar_setting
                        let fiscalDate = EF.Functions.DateFromParts(c.cal_year, c.cal_month, 1)
                        where fiscalDate >= start_date
                           && fiscalDate <= end_date
                        select new
                        {
                            fiscal = fiscalDate
                        };
                    string fydate = query.FirstOrDefault()?.fiscal.ToString("MM/dd/yyyy");
                    if (fydate == null) { exists = false; } else { exists = true; }
                }
            }

            return exists;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-17
        ****************************************************************************************************/
        public SelectList GetFYCYOnce(string parm_fycy)
        {
            string options = "";
            int strFy;

            if (parm_fycy == "FY")
            {
                short FISCAL_YEAR_START = Convert.ToInt16(_appSettings.FISCAL_YEAR_START);
                // Get max fiscal year (right 4 digits)
                var maxFiscalYear = _context.tbl_fiscal_year.Select(f => f.fiscal_year).Max();

                // Extract numeric year safely
                strFy = string.IsNullOrEmpty(maxFiscalYear)
                    ? FISCAL_YEAR_START + 1
                    : int.Parse(maxFiscalYear.Substring(maxFiscalYear.Length - 4));

                options = _appSettings.FISCAL_YEAR_PATTERN == "M"
                      ? $"{strFy}/{strFy + 1}"
                      : $"{strFy + 1}";
            }
            else
            {
                short LEAVE_YEAR_START = Convert.ToInt16(_appSettings.LEAVE_YEAR_START);
                // Get max calendar year (right 4 digits)
                var maxCalendarYear = _context.tbl_calendar_year
                    .Select(c => c.calendar_year)
                    .Max();

                strFy = string.IsNullOrEmpty(maxCalendarYear)
                    ? LEAVE_YEAR_START
                    : int.Parse(maxCalendarYear.Substring(maxCalendarYear.Length - 4));

                options = _appSettings.LCF_YEAR_PATTERN == "M"
                        ? $"{strFy}/{strFy + 1}"
                        : $"{strFy + 1}";

            }
            var list = new List<string> { options };
            return new SelectList(list, options);
        }
        /***************************************************************************************************
         * UPDATE STATUS TO INACTIVE (ELIGIBILITY = 'I') FOR THE DEPENENDENT WHO CROSSED AGE 25
         * Since : 2026-Jun-18
        ****************************************************************************************************/
        public void DeactivateDependent()
        {
            var smt = _context.tbl_setting_dependent_children_details.FirstOrDefault();
            if (smt != null)
            {
                if (DateTime.TryParse(Convert.ToString(smt.age_checking_date), out var age_checking_date))
                {
                    var strsql = _context.tbl_employee_dependent_children_details
                     .Where(a => a.eligibility == "A" &&
                     Math.Round(((double)EF.Functions.DateDiffDay(a.date_of_birth, age_checking_date) + 1) / 365.0, 2) >= 25)
                     .Select(a => new { a.emp_dep_id })
                     .ToList();

                    //try to update at once instead of looping as below
                    if (strsql != null)
                    {
                        foreach (var item in strsql)
                        {
                            int emp_dep_id = item.emp_dep_id;
                            // UPDATE
                            var DataUpdate = _context.tbl_employee_dependent_children_details.FirstOrDefault(d => d.emp_dep_id == emp_dep_id);
                            if (DataUpdate != null)
                            {
                                DataUpdate.eligibility = "I";
                                _ = _context.tbl_employee_dependent_children_details.Update(DataUpdate);
                                _ = _context.SaveChanges();
                                _context.ChangeTracker.Clear();
                            }
                        }
                    }


                }
            }
        }
        /***************************************************************************************************
        * Since : 2026-Jun-24
        ****************************************************************************************************/
        public SelectList TimesheetAcceptance(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "A", "Fully filled timesheet only" },
                { "P", "Partially filled timesheet" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-04
        ****************************************************************************************************/
        public SelectList PercentForTaxAdd(string selvalue = "")
        {
            //0 = 1 % | 5 = 10 % | 6 = 20 %   | 7 = 30 % | 8 = 36 % | 9 = 39 % //this is current slabs
            //0 = 1% | 5 = 10% | 6 = 20%   | 10 = 27% | 11 = 29% | 12 = 29% => starting from FY27
            var options = new Dictionary<string, string> { };
            var sql = _context.tbl_tax_setting.FirstOrDefault();
            if (sql != null)
            {
                string? v0 = "1"; string? t0 = string.Concat(Math.Round(Convert.ToDecimal(sql.initial_tax_percent), 2).ToString(), "%");
                string? v1 = "5"; string? t1 = string.Concat(Math.Round(Convert.ToDecimal(sql.first_tax_percent), 2).ToString(), "%");
                string? v2 = "6"; string? t2 = string.Concat(Math.Round(Convert.ToDecimal(sql.second_tax_percent), 2).ToString(), "%");
                string? v3 = "10"; string? t3 = string.Concat(Math.Round(Convert.ToDecimal(sql.third_tax_percent), 2).ToString(), "%");
                string? v4 = "11"; string? t4 = string.Concat(Math.Round(Convert.ToDecimal(sql.fourth_tax_percent), 2).ToString(), "%");
                string? v5 = "12"; string? t5 = string.Concat(Math.Round(Convert.ToDecimal(sql.fifth_tax_percent), 2).ToString(), "%");

                options = new Dictionary<string, string>
                {
                    { v0, t0 },
                    { v1, t1 },
                    { v2, t2 },
                    { v3, t3 },
                    { v4, t4 },
                    { v5, t5 },

                };
                return GblUtilities.BuildSelectList(options, selvalue);
            }
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-05
        ****************************************************************************************************/
        public string GetHourSettings(string field, string fiscalYear)
        {
            string default_val = "8";
            if (field == "normal_working_hrs" && Convert.ToInt32(fiscalYear[..4]) < 2017)
            {
                return "7";
            }
            else if (field == "working_hrs_per_pay_period" && Convert.ToInt32(fiscalYear[..4]) < 2017)
            {
                return "154";
            }
            else
            {
                var sql = _context.tbl_setting_limit_hrs.FirstOrDefault();
                if (sql == null || string.IsNullOrEmpty(field)) { return default_val; }

                var prop = sql.GetType().GetProperty(field);
                if (prop == null) { return default_val; }

                var value = prop.GetValue(sql);
                return value?.ToString() ?? default_val;
            }
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        ****************************************************************************************************/
        public string? GetFiscalYearValue(string FiscalYear, string field)
        {
            if (string.IsNullOrWhiteSpace(FiscalYear) || string.IsNullOrWhiteSpace(field)) { return string.Empty; }

            var query = _context.tbl_fiscal_year.Where(fis => fis.fiscal_year == FiscalYear);
            return (query == null) ? string.Empty : field switch
            {
                "date_from" => query.Select(fis => fis.date_from.ToString() ?? "").FirstOrDefault(),
                "date_to" => query.Select(fis => fis.date_to.ToString() ?? "").FirstOrDefault(),
                "fiscal_year_abb" => query.Select(fis => fis.fiscal_year_abb ?? "").FirstOrDefault(),
                "yearly_working_hrs" => query.Select(fis => fis.yearly_working_hrs.ToString() ?? "").FirstOrDefault(),
                _ => string.Empty
            };
        }
        /***************************************************************************************************
        * Since : 2026-Jul-10
        ****************************************************************************************************/
        public SelectList GetPaidYears(string selvalue = "")
        {
            //only for leave encashment
            Dictionary<string, string> options;
            string fyYear = _httpContextAccessor.HttpContext.Session.GetString("fiscal_year");
            if (!string.IsNullOrEmpty(fyYear))
            {
                string yearPart = fyYear.Substring(0, 4);
                options = new Dictionary<string, string>
            {
                { yearPart, yearPart }
            };
            }
            else
            {
                options = [];
            }
            return GblUtilities.BuildSelectList(options, selvalue);
        }

        /***************************************************************************************************
        * Since : 2026-Jul-19
        ****************************************************************************************************/
        public List<(DateTime Date, string Flag)> GetCalendarDates(DateTime startDate, DateTime endDate)
        {
            var calendarRows = _context.tbl_calendar_setting
                .Where(c =>
                    (c.cal_year > startDate.Year || (c.cal_year == startDate.Year && c.cal_month >= startDate.Month)) &&
                    (c.cal_year < endDate.Year || (c.cal_year == endDate.Year && c.cal_month <= endDate.Month))
                )
                .OrderBy(c => c.cal_year).ThenBy(c => c.cal_month)
                .ToList();

            var calendarDates = new List<(DateTime Date, string Flag)>();

            foreach (var row in calendarRows)
            {
                int daysInMonth = DateTime.DaysInMonth(row.cal_year, row.cal_month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    // Use reflection to read d1, d2, … dynamically
                    string? flag = row.GetType().GetProperty($"d{day}")?.GetValue(row)?.ToString();
                    var date = new DateTime(row.cal_year, row.cal_month, day);

                    if (date >= startDate && date <= endDate)
                    {
                        calendarDates.Add((date, flag ?? ""));
                    }
                }
            }
            return calendarDates;
        }

        /***************************************************************************************************
        * Since : 2026-Jul-07
        ****************************************************************************************************/
        public string GetNormalOrHolidayOrWeekendOnCalendar(DateTime startDate, DateTime endDate, DateTime SelDate)
        {
            var DateFlag = GetCalendarDates(startDate, endDate)
                .ToDictionary(d => d.Date, d => d.Flag);
            string flag = DateFlag.TryGetValue(SelDate, out string? value) ? value : string.Empty;
            return flag;
        }
    }
}
