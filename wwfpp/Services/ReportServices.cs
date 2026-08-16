using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Employee;

namespace wwfpp.Services
{
    public class ReportServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly TimesheetServices _timesheetServices;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportServices(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            TimesheetServices timesheetServices,
            GlobalOptionServices globalOptionServices,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _timesheetServices = timesheetServices;
            _globalOptionServices = globalOptionServices;
            _httpContextAccessor = httpContextAccessor;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        /** getDept => Use Employee Services -> GetDept */

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        /**getGLCode => Use Payroll Services -> GetGLCode */

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * 
        ****************************************************************************************************/
        /** timesheet_calender_hour_week => Only need to add if we have to integrate Weekly/Biweekly */

        /***************************************************************************************************
        * Since : 2026-Aug-11
        * GET MONTHLY WORKING DAYS [REMOVAL OF HOLIDAY AND WEEKEND]
        ****************************************************************************************************/
        public string GetCalendarWorkingDays(int year, int month)
        {
            // Load the calendar row for the given year/month
            var calendar = _context.tbl_calendar_setting
                .FirstOrDefault(c => c.cal_year == year && c.cal_month == month);

            if (calendar == null) { return ""; }

            var workingDays = new List<string>();

            // Assuming your entity has properties d1, d2, d3, ... up to d31
            for (int i = 1; i <= 31; i++)
            {
                var prop = calendar.GetType().GetProperty("d" + i);
                if (prop != null)
                {
                    var value = prop.GetValue(calendar)?.ToString();
                    if (int.TryParse(value, out int day))
                    {
                        workingDays.Add(day.ToString());
                    }
                }
            }

            return string.Join(",", workingDays);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Previous function name : timesheet_calender_hour
        ****************************************************************************************************/
        public string TimesheetCalendarHour(int year, int month, int empId)
        {
            string isAllFilled = "true";
            var fndays = GetCalendarWorkingDays(year, month);/** Get working days string (e.g., "1,2,3,4,5") */
            if (!string.IsNullOrEmpty(fndays))
            {
                var ardays = fndays.Split(',');
                foreach (var dayStr in ardays)
                {
                    if (int.TryParse(dayStr, out int day))
                    {
                        var filled = _timesheetServices.GetFilledTime(year, month, day, empId);/** Call your timesheet manager check */
                        if (filled == "false")
                        {
                            isAllFilled = "false";
                            break; /** no need to check further */
                        }
                    }
                }
            }
            return isAllFilled;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getEmployeeTimesheetStatusReport
        ****************************************************************************************************/


        /***************************************************************************************************
        * Since : 2026-Aug-11
        * getEmployeeTimesheetStatusReportIC
        ****************************************************************************************************/


        /***************************************************************************************************
        * Since : 2026-Aug-11
        *   GET DATE VALUE FOR PIVOT TABLE => getPivotDate
        ****************************************************************************************************/
        public string GetPivotDate(int fromYear, int fromMonth, int toYear, int toMonth)
        {
            var fnFromDate = new DateTime(fromYear, fromMonth, 1);
            int monthDiff = ((toYear - fromYear) * 12) + (toMonth - fromMonth) + 1;/** Month difference inclusive */
            var dates = new List<string>();
            for (int i = 0; i < monthDiff; i++)
            {
                var dateVal = fnFromDate.AddMonths(i);
                dates.Add($"[{dateVal:yyyy-MM-dd}]"); /**format as needed*/
            }
            return string.Join(",", dates);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * GET EMPLOYEE FROM SALARY FINAL => getEmployeeNameFinal
        ****************************************************************************************************/
        public SelectList GetEmployeeNameFinal(int SelValue = 0)
        {
            var employees = _context.tbl_employee
                .Where(e => _context.tbl_employee_salary_final
                    .Any(f => f.emp_id == e.emp_id))
                .OrderBy(e => e.firstname)
                .ThenBy(e => e.middlename)
                .ThenBy(e => e.lastname)
                .Select(e => new
                {
                    e.emp_id,
                    emp_name_code = $"{e.firstname} {e.middlename} {e.lastname} ({e.emp_code})"
                })
                .ToList();
            return new SelectList(employees, "emp_id", "emp_name_code", SelValue);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        *Fund source listing    => getFundSource
        ****************************************************************************************************/
        public SelectList GetFundSourceAsync(int? fundId, string status, string order)
        {
            string orgAccountPackage = _globalOptionServices.OptionServices["org_account_package"];
            // Defaults
            if (string.IsNullOrEmpty(order)) order = "ASC";
            if (string.IsNullOrEmpty(status)) status = "A";

            var query = _context.tbl_fund_source
                .Where(f => f.fund_status == status);

            // Dynamic ordering
            if (orgAccountPackage == "sage")
            {
                query = order == "ASC"
                    ? query.OrderBy(f => f.fund_source.Substring(
                        Math.Max(0, f.fund_source.Length - 8)))
                    : query.OrderByDescending(f => f.fund_source.Substring(
                        Math.Max(0, f.fund_source.Length - 8)));
            }
            else if (orgAccountPackage == "orcle")
            {
                query = order == "ASC"
                    ? query.OrderBy(f => f.fund_desc).ThenBy(f => f.fund_source)
                    : query.OrderByDescending(f => f.fund_desc).ThenByDescending(f => f.fund_source);
            }
            else
            {
                query = order == "ASC"
                    ? query.OrderBy(f => f.fund_source)
                    : query.OrderByDescending(f => f.fund_source);
            }

            var results = query
                .Select(f => new
                {
                    FundId = f.fund_id,
                    FundSource = f.fund_source
                })
                .ToList();

            return new SelectList(results, "FundId", "FundSource");
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        *
        ****************************************************************************************************/




    }
}
