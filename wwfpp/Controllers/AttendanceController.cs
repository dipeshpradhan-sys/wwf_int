using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using static QuestPDF.Helpers.Colors;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net.NetworkInformation;
using System.Text;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Models.Attendance;
using wwfpp.Services;
using static GblUtilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
/*
 * Master File
 */
namespace wwfpp.Controllers
{
    public class AttendanceController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        GlobalOptionServices globalOptionServices,
        EmployeeServices employeeServices,
        AccountServices accountServices,
        SettingsServices settingsServices,
        AttendanceServices attendanceServices
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly AppSettings _appSettings = appSettings.Value;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;
        private readonly EmployeeServices _employeeServices = employeeServices;
        private readonly AccountServices _accountServices = accountServices;
        private readonly SettingsServices _settingsServices = settingsServices;
        private readonly AttendanceServices _attendanceServices = attendanceServices;

        /***************************************************************************************************
        * Since : 2026-Jul-15
        //****************************************************************************************************/
        //private const int RemarksEmpty = 0;
        //private const int RemarksNotInformed = 1;
        //private const int RemarksLto = 2;
        //private const int RemarksWorkFromHome = 3;
        //private const int RemarksOutOfOffice = 4;
        //private const int RemarksDayOff = 5;
        //private const int RemarksLeave = 6;
        //private const int RemarksTravel = 7;
        //private const int RemarksStrike = 8;
        //private const int RemarksELse = 9;

        public IActionResult Index()
        {
            return View();
        }
        /********************************************************************************************************************/
        /********************************************************************************************************************/
        #region ATTENDANCE REPORT
        [HttpGet]
        public IActionResult AttendanceReport()
        {
            string PageId = "11104";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return Forbid(); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.DutyStationFilter = _employeeServices.GetDutyStationList("1");
            ViewBag.EmployeeTypeFilter = _attendanceServices.GetEmployeeType("Inside");
            ViewBag.EmployeeList = _employeeServices.GetEmployeeActiveOnly();

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.InOutDay = DateTime.Today;
            ViewBag.ReportTypeFilter = GetReportType();
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD");
            ViewBag.AbsentRemarkLTOFilter = _attendanceServices.GetAbsentRemarkLTO();

            return PartialView("Attendance/_AttendanceReport");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceReportRange([FromBody] ReportAttendanceViewModel request)
        {
            string PageId = "11104";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return Forbid(); }
            #endregion FOR END PERMISSION

            string reportName = request.report_name ?? "";
            if (string.IsNullOrWhiteSpace(reportName)) { return Json(new { status = "invalid", message = "Insufficient information" }); }

            string dutyStationId = request.duty_station_id ?? "";
            string employeeType = request.employee_type ?? "";
            string empStatus = request.emp_status ?? "";
            string start_date = request.start_date ?? "";
            string end_date = request.end_date ?? "";
            string absentRemarkLto = request.absent_remark_lto ?? "";
            int emp_id = request.emp_id ?? 0;

            DateTime startDate = DateTime.TryParse(start_date, out DateTime dtSParse) ? dtSParse : DateTime.Today;
            DateTime endDate = DateTime.TryParse(end_date, out DateTime dtEParse) ? dtEParse : DateTime.Today;

            string ReportTitle = "Attendance Report [" + reportName + "]";
            string DutyStation = _attendanceServices.GetDutyStation(dutyStationId);
            string EmployeeType = employeeType;
            string EmployeeStatus = empStatus == "A" ? "Active" : "Inactive";
            string sStartDate = startDate.ToString(_appSettings.DATE_FORMAT);
            string sEndDate = endDate.ToString(_appSettings.DATE_FORMAT);
            string EmployeeName = _employeeServices.GetEmployeeName(emp_id);

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.ReportTitle = ReportTitle;
            ViewBag.DutyStation = DutyStation;
            ViewBag.EmployeeType = EmployeeType;
            ViewBag.EmployeeStatus = EmployeeStatus;
            ViewBag.startDate = sStartDate;
            ViewBag.endDate = sEndDate;
            ViewBag.EmployeeName = EmployeeName;
            if (string.Equals(reportName, "Frequency", StringComparison.OrdinalIgnoreCase))
            {
                absentRemarkLto = string.IsNullOrWhiteSpace(absentRemarkLto) ? "Saved in Database" : absentRemarkLto;
            }
            ViewBag.AbsentRemarkLto = absentRemarkLto;

            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToList();

            var DateFlag = _settingsServices.GetCalendarDates(startDate, endDate)
                .ToDictionary(d => d.Date, d => d.Flag);

            ViewBag.DateFlag = DateFlag;

            if (string.Equals(reportName, "Remarks", StringComparison.OrdinalIgnoreCase))
            {
                var result = await GetAttendanceReportRangeRemarksDataAsync(
                    startDate, endDate, empStatus, dutyStationId,
                    employeeType, reportName, absentRemarkLto, "HTML", emp_id)
                    .ConfigureAwait(false);
                return PartialView("Attendance/_AttendanceReport" + reportName, result);
            }
            else if (string.Equals(reportName, "Frequency", StringComparison.OrdinalIgnoreCase))
            {

                var result = await GetAttendanceReportRangeFrequencyDataAsync(
                    startDate, endDate, empStatus, dutyStationId,
                    employeeType, reportName, absentRemarkLto, "HTML", emp_id)
                    .ConfigureAwait(false);
                return PartialView("Attendance/_AttendanceReport" + reportName, result);
            }
            else if (string.Equals(reportName, "InOut", StringComparison.OrdinalIgnoreCase))
            {
                var result = await GetAttendanceReportRangeInOutDataAsync(
                    startDate, endDate, empStatus, dutyStationId,
                    employeeType, reportName, absentRemarkLto, "HTML", emp_id)
                    .ConfigureAwait(false);
                return PartialView("Attendance/_AttendanceReport" + reportName, result);
            }
            else if (string.Equals(reportName, "Hours", StringComparison.OrdinalIgnoreCase))
            {
                var result = await GetAttendanceReportRangeHoursDataAsync(
                    startDate, endDate, empStatus, dutyStationId,
                    employeeType, reportName, absentRemarkLto, "HTNL", emp_id)
                    .ConfigureAwait(false);
                return PartialView("Attendance/_AttendanceReport" + reportName, result);
            }
            return BadRequest("Invalid report type");
        }
        /**--------------------------------------------------------------------------------**/
        //  Attendance Report : Range : Remarks | Frequency | Hours | In/Out
        /**--------------------------------------------------------------------------------**/
        #region ATTENDANCE REPORT RANGE
        private async Task<List<ReportDailyAttendanceRangeRemarksViewModel>> GetAttendanceReportRangeRemarksDataAsync(
            DateTime startDate, DateTime endDate, string empStatus, string dutyStationId,
            string employeeType, string reportName, string absentRemarkLto, string format, int emp_id)
        {
            var rawRows = await _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.duty_station_id == dutyStationId
                    && x.employee_type == employeeType
                    && x.emp_status == empStatus
                    && x.in_out_date >= startDate && x.in_out_date <= endDate
                    && (emp_id <= 0 || x.emp_id == emp_id)
                    )
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var result = rawRows
                .GroupBy(r => new
                {
                    r.emp_id,
                    Employee = string.Join(" ",
                    new[] { r.firstname?.Trim(), r.middlename?.Trim(), r.lastname?.Trim() }
                    .Where(x => !string.IsNullOrEmpty(x))) + " (" + r.emp_code + ")"
                })
                .Select(g => new ReportDailyAttendanceRangeRemarksViewModel
                {
                    EmpId = g.Key.emp_id,
                    Employee = g.Key.Employee,
                    DateRemarks = _settingsServices.GetCalendarDates(startDate, endDate).ToDictionary(
                    d => d.Date,
                    d =>
                    {
                        string remarks = g.Where(x => x.in_out_date == d.Date)
                                      .Select(x => $"{x.remarks} {x.narration}")
                                      .FirstOrDefault() ?? string.Empty;

                        string flagText = d.Flag == "W" ? "Weekend" :
                                       d.Flag == "H" ? "Holiday" : string.Empty;

                        return (Remarks: remarks, Flag: flagText);
                    })
                })
                .OrderBy(x => x.Employee)
                .ToList();

            return result;
        }
        private async Task<List<ReportDailyAttendanceRangeFrequencyViewModel>> GetAttendanceReportRangeFrequencyDataAsync(
                DateTime startDate, DateTime endDate, string empStatus, string dutyStationId,
                string employeeType, string reportName, string absentRemarkLto, string format, int emp_id)
        {
            var rawRows = await _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.duty_station_id == dutyStationId
                    && x.employee_type == employeeType
                    && x.emp_status == empStatus
                    && x.in_out_date >= startDate && x.in_out_date <= endDate
                    && (emp_id <= 0 || x.emp_id == emp_id)
                    )
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var result = rawRows
                .GroupBy(r => new
                {
                    r.emp_id,
                    Employee = string.Join(" ",
                        new[] { r.firstname?.Trim(), r.middlename?.Trim(), r.lastname?.Trim() }
                        .Where(x => !string.IsNullOrEmpty(x))) + " (" + r.emp_code + ")"
                })
                .Select(g =>
                {
                    var dateFrequency = _settingsServices.GetCalendarDates(startDate, endDate).ToDictionary(
                        d => d.Date,
                        d =>
                        {
                            var row = g.FirstOrDefault(r => r.in_out_date == d.Date);

                            var calendarRow = _context.tbl_calendar_setting
                                .FirstOrDefault(c => c.cal_year == d.Date.Year && c.cal_month == d.Date.Month);

                            string? flag = null;
                            if (calendarRow != null)
                            {
                                var prop = calendarRow.GetType().GetProperty($"d{d.Date.Day}");
                                flag = prop?.GetValue(calendarRow)?.ToString();
                            }
                            bool isHolidayOrWeekend = flag is "H" or "W";
                            bool isDayOff = _context.tbl_employee_dayoff.Any(off => off.dayoff_date == d.Date && off.emp_id == g.Key.emp_id);

                            if (isHolidayOrWeekend || isDayOff)
                            {
                                return (row.check_in, LTO: 0, LTH: 0);
                            }
                            string? dyn_office_in_at = string.IsNullOrWhiteSpace(absentRemarkLto) || string.Equals(absentRemarkLto, "Saved in Database", StringComparison.OrdinalIgnoreCase) ? row.office_in_at : absentRemarkLto;

                            int rLTO = !string.IsNullOrWhiteSpace(row.check_in)
                                && DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_in}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    > DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {dyn_office_in_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                ? 1 : 0;

                            int rLTH = !string.IsNullOrWhiteSpace(row.check_out) && row.check_out != "n"
                                && DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_out}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    > DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {dyn_office_in_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                ? 1 : 0;

                            return (row.check_in, LTO: rLTO, LTH: rLTH);
                        });

                    int LTOT = dateFrequency.Values.Sum(x => x.LTO);
                    int LTHT = dateFrequency.Values.Sum(x => x.LTH);

                    return new ReportDailyAttendanceRangeFrequencyViewModel
                    {
                        EmpId = g.Key.emp_id,
                        Employee = g.Key.Employee,
                        DateFrequency = dateFrequency,
                        LtoT = LTOT,
                        LthT = LTHT
                    };
                })
                .OrderBy(x => x.Employee)
                .ToList();

            return result;
        }
        private async Task<List<ReportDailyAttendanceRangeHoursViewModel>> GetAttendanceReportRangeHoursDataAsync(
            DateTime startDate, DateTime endDate, string empStatus, string dutyStationId,
            string employeeType, string reportName, string absentRemarkLto, string format, int emp_id)
        {
            var rawRows = await _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.duty_station_id == dutyStationId
                    && x.employee_type == employeeType
                    && x.emp_status == empStatus
                    && x.in_out_date >= startDate && x.in_out_date <= endDate
                    && (emp_id <= 0 || x.emp_id == emp_id)
                    )
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var result = rawRows
                .GroupBy(r => new
                {
                    r.emp_id,
                    Employee = string.Join(" ",
                        new[] { r.firstname?.Trim(), r.middlename?.Trim(), r.lastname?.Trim() }
                        .Where(x => !string.IsNullOrEmpty(x))) + " (" + r.emp_code + ")"
                })
                .Select(g =>
                {
                    var dateHours = _settingsServices.GetCalendarDates(startDate, endDate).ToDictionary(
                        d => d.Date,
                        d =>
                        {
                            var row = g.FirstOrDefault(r => r.in_out_date == d.Date);

                            var calendarRow = _context.tbl_calendar_setting
                                .FirstOrDefault(c => c.cal_year == d.Date.Year && c.cal_month == d.Date.Month);

                            string? flag = null;
                            if (calendarRow != null)
                            {
                                var prop = calendarRow.GetType().GetProperty($"d{d.Date.Day}");
                                flag = prop?.GetValue(calendarRow)?.ToString();
                            }
                            bool isHolidayOrWeekend = flag is "H" or "W";
                            bool isDayOff = _context.tbl_employee_dayoff.Any(off => off.dayoff_date == d.Date && off.emp_id == g.Key.emp_id);

                            if (isHolidayOrWeekend || isDayOff)
                            {
                                return (LTO: 0d, LTOF: "00:00", LTH: 0d, LTHF: "00:00");
                            }

                            double rLTO = !string.IsNullOrWhiteSpace(row.check_in)
                                && DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_in}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    > DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.office_in_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                ? (DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_in}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    - DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.office_in_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                                    .TotalMinutes
                                : 0d;

                            double rLTH = !string.IsNullOrWhiteSpace(row.check_out) && row.check_out != "n"
                                && DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_out}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    > DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.office_out_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                ? (DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.check_out}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                                    - DateTime.ParseExact($"{d.Date:yyyy-MM-dd} {row.office_out_at}", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
                                    .TotalMinutes
                                : 0d;

                            string rLTOF = $"{(int)(rLTO / 60):D2}:{(int)(rLTO % 60):D2}";
                            string rLTHF = $"{(int)(rLTH / 60):D2}:{(int)(rLTH % 60):D2}";

                            return (LTO: rLTO, LTOF: rLTOF, LTH: rLTH, LTHF: rLTHF);
                        });

                    double LTOT = dateHours.Values.Sum(x => x.LTO);
                    double LTHT = dateHours.Values.Sum(x => x.LTH);

                    string LTOTF = $"{(int)(LTOT / 60):D2}:{(int)(LTOT % 60):D2}";
                    string LTHTF = $"{(int)(LTHT / 60):D2}:{(int)(LTHT % 60):D2}";

                    return new ReportDailyAttendanceRangeHoursViewModel
                    {
                        EmpId = g.Key.emp_id,
                        Employee = g.Key.Employee,
                        DateHours = dateHours,
                        LtoT = LTOTF,
                        LthT = LTHTF
                    };
                })
                .OrderBy(x => x.Employee)
                .ToList();

            return result;
        }
        private async Task<List<ReportDailyAttendanceRangeInOutViewModel>> GetAttendanceReportRangeInOutDataAsync(
                DateTime startDate, DateTime endDate, string empStatus, string dutyStationId,
                string employeeType, string reportName, string absentRemarkLto, string format, int emp_id)
        {
            var rawRows = await _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.duty_station_id == dutyStationId
                    && x.employee_type == employeeType
                    && x.emp_status == empStatus
                    && x.in_out_date >= startDate && x.in_out_date <= endDate
                    && (emp_id <= 0 || x.emp_id == emp_id)
                    )
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var result = rawRows
                .GroupBy(r => new
                {
                    r.emp_id,
                    Employee = string.Join(" ",
                    new[] { r.firstname?.Trim(), r.middlename?.Trim(), r.lastname?.Trim() }
                    .Where(x => !string.IsNullOrEmpty(x))) + " (" + r.emp_code + ")"
                })
                .Select(g => new ReportDailyAttendanceRangeInOutViewModel
                {
                    EmpId = g.Key.emp_id,
                    Employee = g.Key.Employee,
                    DateInOutRemarks = _settingsServices.GetCalendarDates(startDate, endDate).ToDictionary(
                    d => d.Date,
                    d =>
                    {
                        string In = g.Where(x => x.in_out_date == d.Date)
                        .Select(x => $"{x.check_in}")
                        .FirstOrDefault() ?? string.Empty;

                        string Out = g.Where(x => x.in_out_date == d.Date)
                        .Select(x => $"{x.check_out}")
                        .FirstOrDefault() ?? string.Empty;

                        string Remarks = g.Where(x => x.in_out_date == d.Date)
                        .Select(x => $"{x.remarks} {x.narration}")
                        .FirstOrDefault() ?? string.Empty;

                        if (In.Length > 5) { In = In[..5]; }
                        if (Out.Length > 5) { Out = Out[..5]; }

                        return (In, Out, Remarks);
                    })
                })
                .OrderBy(x => x.Employee)
                .ToList();

            return result;

        }
        /**-----------------------------------------------------------------------------------------------------------------------**/
        private async Task<(IActionResult? Error, List<T>? Data)> PrepareExportRangeAsync<T>(
        DateTime startDate,
        DateTime endDate,
        string empStatus,
        string dutyStationId,
        string employeeType,
        string reportName,
        string absentRemarkLto,
        string format,
        int emp_id,
        Func<DateTime, DateTime, string, string, string, string, string, string, int, Task<List<T>>> dataFetcher)
        {
            const string PageId = "11104";
            var perm = _accountServices.GetMenuPermission(PageId);
            if (string.Equals(perm.vpern, "false", StringComparison.Ordinal)) { return (Forbid(), null); }

            var data = await dataFetcher(
                startDate,
                endDate,
                empStatus,
                dutyStationId ?? "",
                employeeType ?? "",
                reportName ?? "",
                absentRemarkLto ?? "",
                format,
                emp_id
                ).ConfigureAwait(false);
            return (null, data);
        }
        /**-----------------------------------------------------------------------------------------------------------------------**/
        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReportRangeAsync(
        string startDate, string endDate, string empStatus, string dutyStationId,
        string employeeType, string reportName, string absentRemarkLto, string format, int emp_id
        )
        {
            string orgName = _globalOptionServices.OptionServices["op_org_name"];
            string reportTitle = $"Attendance Report - {reportName}";
            string dutyStation = _attendanceServices.GetDutyStation(dutyStationId);
            string EmployeeType = employeeType;
            string EmployeeStatus = empStatus;
            //string inOutDateRange = $"{startDate.ToString(_appSettings.DATE_FORMAT)} - {endDate.ToString(_appSettings.DATE_FORMAT)}";

            DateTime newStartDate = DateTime.TryParse(startDate, out DateTime dtSParse) ? dtSParse : DateTime.Today;
            DateTime newEndDate = DateTime.TryParse(endDate, out DateTime dtEParse) ? dtEParse : DateTime.Today;

            switch (reportName?.ToLowerInvariant())
            {
                case "remarks":
                    {
                        var (error, data) = await PrepareExportRangeAsync<ReportDailyAttendanceRangeRemarksViewModel>(
                            newStartDate, newEndDate, empStatus, dutyStationId,
                            employeeType, reportName, absentRemarkLto, format, emp_id,
                            GetAttendanceReportRangeRemarksDataAsync).ConfigureAwait(false);
                        return error is not null
                            ? error
                            : (format == "Excel" ? GenerateAttendanceExcel(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id)
                            : GenerateAttendancePdf(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id));
                    }
                case "frequency":
                    {
                        var (error, data) = await PrepareExportRangeAsync<ReportDailyAttendanceRangeFrequencyViewModel>(
                            newStartDate, newEndDate, empStatus, dutyStationId,
                            employeeType, reportName, absentRemarkLto, format, emp_id,
                            GetAttendanceReportRangeFrequencyDataAsync).ConfigureAwait(false);
                        return error is not null
                            ? error
                            : (format == "Excel" ? GenerateAttendanceExcel(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id)
                            : GenerateAttendancePdf(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id));
                    }
                case "inout":
                    {
                        var (error, data) = await PrepareExportRangeAsync<ReportDailyAttendanceRangeInOutViewModel>(
                            newStartDate, newEndDate, empStatus, dutyStationId,
                            employeeType, reportName, absentRemarkLto, format, emp_id,
                            GetAttendanceReportRangeInOutDataAsync).ConfigureAwait(false);
                        return error is not null
                            ? error
                            : (format == "Excel" ? GenerateAttendanceExcel(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id)
                            : GenerateAttendancePdf(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id));
                    }
                case "hours":
                    {
                        var (error, data) = await PrepareExportRangeAsync<ReportDailyAttendanceRangeHoursViewModel>(
                            newStartDate, newEndDate, empStatus, dutyStationId,
                            employeeType, reportName, absentRemarkLto, format, emp_id,
                            GetAttendanceReportRangeHoursDataAsync).ConfigureAwait(false);
                        return error is not null
                            ? error
                            : (format == "Excel" ? GenerateAttendanceExcel(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id)
                            : GenerateAttendancePdf(data!, newStartDate, newEndDate, empStatus,
                            dutyStation, employeeType, absentRemarkLto, format, reportName,
                            orgName, reportTitle, _appSettings.DATE_FORMAT, emp_id));
                    }
                default:
                    return BadRequest("Invalid report type");
            }
        }
        private FileContentResult GenerateAttendanceExcel<T>(
            IEnumerable<T> data,
            DateTime startDate,
            DateTime endDate,
            string empStatus,
            string dutyStation,
            string employeeType,
            string absentRemarkLto,
            string format,
            string reportName,
            string orgName,
            string reportTitle,
            string dateFormat,
            int emp_id
            )
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(reportTitle);

            int row = 1;
            ws.Cell(row, 2).Value = "Organization"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = orgName; row++;
            ws.Cell(row, 2).Value = "Report Title"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = reportTitle; row++;
            ws.Cell(row, 2).Value = "Duty Station"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = dutyStation; row++;
            ws.Cell(row, 2).Value = "Employee Type"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = employeeType; row++;
            ws.Cell(row, 2).Value = "Employee Status"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = empStatus; row++;
            ws.Cell(row, 2).Value = "Date"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = $"{startDate.ToString(dateFormat)} - {endDate.ToString(dateFormat)}"; row++;
            if (!string.IsNullOrWhiteSpace(absentRemarkLto)) { ws.Cell(row, 2).Value = "Late to Office"; ws.Cell(row, 3).Value = ":"; ws.Cell(row, 4).Value = absentRemarkLto; row++; }
            if (string.Equals(reportName, "frequency", StringComparison.OrdinalIgnoreCase)
                || string.Equals(reportName, "hours", StringComparison.OrdinalIgnoreCase)
                )
            {
                ws.Cell(row, 2).Value = "LTO : Late to Office | LTH : Late to Home"; row++;
            }

            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset)).ToList();

            var DateFlag = _settingsServices.GetCalendarDates(startDate, endDate)
                .ToDictionary(d => d.Date, d => d.Flag);

            if (string.Equals(reportName, "remarks", StringComparison.OrdinalIgnoreCase))
            {
                var headers = new List<string> { "Employee" };
                headers.AddRange(allDates.Select(d => d.ToString(dateFormat)));
                for (int col = 0; col < headers.Count; col++)
                {
                    ws.Cell(row, col + 1).Value = headers[col];
                }
                ws.Row(row).Style.Font.Bold = true;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;

                foreach (var item in data.Cast<ReportDailyAttendanceRangeRemarksViewModel>())
                {
                    ws.Cell(row, 1).Value = item.Employee;
                    for (int col = 0; col < allDates.Count; col++)
                    {
                        var date = allDates[col];
                        var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                        ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                        ws.Cell(row, col + 2).Value = item.DateRemarks[date].Remarks;
                    }
                    row++;
                }
            }
            else if (string.Equals(reportName, "frequency", StringComparison.OrdinalIgnoreCase))
            {
                int jumpper = 1;
                int last_col = 0;
                ws.Cell(row, 1).Value = "Employee";
                for (int col = 0; col < allDates.Count; col++)
                {
                    ws.Cell(row, col + jumpper + 1).Value = allDates[col];
                    ws.Cell(row, col + jumpper + 2).Value = "";
                    ws.Cell(row, col + jumpper + 3).Value = "";
                    jumpper += 2;
                    last_col = col + 1;
                }
                ws.Cell(row, last_col + jumpper + 1).Value = "Total";
                ws.Cell(row, last_col + jumpper + 2).Value = "";

                ws.Row(row).Style.Font.Bold = true;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
                //sub header
                jumpper = 1;
                last_col = 0;
                ws.Cell(row, 1).Value = "";
                for (int col = 0; col < allDates.Count; col++)
                {
                    var date = allDates[col];
                    var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                    ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                    ws.Cell(row, col + jumpper + 1).Value = "Check In";
                    ws.Cell(row, col + jumpper + 2).Value = "LTO";
                    ws.Cell(row, col + jumpper + 3).Value = "LTH";
                    jumpper += 2;
                    last_col = col + 1;
                }
                ws.Cell(row, last_col + jumpper + 1).Value = "LTO";
                ws.Cell(row, last_col + jumpper + 2).Value = "LTH";

                row++;
                foreach (var item in data.Cast<ReportDailyAttendanceRangeFrequencyViewModel>())
                {
                    jumpper = 1;
                    last_col = 0;
                    ws.Cell(row, 1).Value = item.Employee;
                    for (int col = 0; col < allDates.Count; col++)
                    {
                        var date = allDates[col];
                        var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                        ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                        ws.Cell(row, col + jumpper + 1).Value = item.DateFrequency[date].checkIn;
                        ws.Cell(row, col + jumpper + 2).Value = item.DateFrequency[date].LTO;
                        ws.Cell(row, col + jumpper + 3).Value = item.DateFrequency[date].LTH;
                        jumpper += 2;
                        last_col = col + 1;
                    }
                    ws.Cell(row, last_col + jumpper + 1).Value = item.LtoT;
                    ws.Cell(row, last_col + jumpper + 2).Value = item.LthT;
                    row++;
                }
            }
            else if (string.Equals(reportName, "hours", StringComparison.OrdinalIgnoreCase))
            {
                int jumpper = 1;
                int last_col = 0;
                ws.Cell(row, 1).Value = "Employee";
                for (int col = 0; col < allDates.Count; col++)
                {
                    ws.Cell(row, col + jumpper + 1).Value = allDates[col];
                    ws.Cell(row, col + jumpper + 2).Value = "";
                    jumpper++;
                    last_col = col + 1;
                }
                ws.Cell(row, last_col + jumpper + 1).Value = "Total";
                ws.Cell(row, last_col + jumpper + 2).Value = "";

                ws.Row(row).Style.Font.Bold = true;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
                //sub header
                jumpper = 1;
                last_col = 0;
                ws.Cell(row, 1).Value = "";
                for (int col = 0; col < allDates.Count; col++)
                {
                    var date = allDates[col];
                    var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                    ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                    ws.Cell(row, col + jumpper + 1).Value = "LTO";
                    ws.Cell(row, col + jumpper + 2).Value = "LTH";
                    jumpper++;
                    last_col = col + 1;
                }
                ws.Cell(row, last_col + jumpper + 1).Value = "LTO";
                ws.Cell(row, last_col + jumpper + 2).Value = "LTH";

                row++;
                foreach (var item in data.Cast<ReportDailyAttendanceRangeHoursViewModel>())
                {
                    jumpper = 1;
                    last_col = 0;
                    ws.Cell(row, 1).Value = item.Employee;
                    for (int col = 0; col < allDates.Count; col++)
                    {
                        var date = allDates[col];
                        var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                        ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                        ws.Cell(row, col + jumpper + 1).Value = item.DateHours[date].LTOF;
                        ws.Cell(row, col + jumpper + 2).Value = item.DateHours[date].LTHF;
                        jumpper++;
                        last_col = col + 1;
                    }
                    ws.Cell(row, last_col + jumpper + 1).Value = item.LtoT;
                    ws.Cell(row, last_col + jumpper + 2).Value = item.LthT;
                    row++;
                }
            }
            else if (string.Equals(reportName, "inout", StringComparison.OrdinalIgnoreCase))
            {
                int jumpper = 1;
                ws.Cell(row, 1).Value = "Employee";
                for (int col = 0; col < allDates.Count; col++)
                {
                    ws.Cell(row, col + jumpper + 1).Value = allDates[col];
                    ws.Cell(row, col + jumpper + 2).Value = "";
                    ws.Cell(row, col + jumpper + 3).Value = "";
                    jumpper += 2;
                }
                ws.Row(row).Style.Font.Bold = true;
                ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;
                //sub header
                jumpper = 1;
                ws.Cell(row, 1).Value = "";
                for (int col = 0; col < allDates.Count; col++)
                {
                    var date = allDates[col];
                    var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                    ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                    ws.Cell(row, col + jumpper + 1).Value = "In";
                    ws.Cell(row, col + jumpper + 2).Value = "Out";
                    ws.Cell(row, col + jumpper + 3).Value = "Remarks";
                    jumpper += 2;
                }
                row++;
                foreach (var item in data.Cast<ReportDailyAttendanceRangeInOutViewModel>())
                {
                    jumpper = 1;
                    ws.Cell(row, 1).Value = item.Employee;
                    for (int col = 0; col < allDates.Count; col++)
                    {
                        var date = allDates[col];
                        var flag = DateFlag.ContainsKey(date) ? DateFlag[date] : string.Empty;
                        ws.Row(row).Style.Fill.BackgroundColor = flag == "W" ? XLColor.LightPink : flag == "H" ? XLColor.DarkOliveGreen : XLColor.NoColor;
                        ws.Cell(row, col + jumpper + 1).Value = item.DateInOutRemarks[date].In;
                        ws.Cell(row, col + jumpper + 2).Value = item.DateInOutRemarks[date].Out;
                        ws.Cell(row, col + jumpper + 3).Value = item.DateInOutRemarks[date].Remarks;
                        jumpper += 2;
                    }
                    row++;
                }

            }
            _ = ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            string fileName = $"AttendanceReport_{reportTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return new FileContentResult(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }
        /**-----------------------------------------------------------------------------------------------------------------------**/
        private FileContentResult GenerateAttendancePdf<T>(
            IEnumerable<T> data,
            DateTime startDate,
            DateTime endDate,
            string empStatus,
            string dutyStation,
            string employeeType,
            string absentRemarkLto,
            string format,
            string reportName,
            string orgName,
            string reportTitle,
            string dateFormat,
            int emp_id
        )
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
            .Select(offset => startDate.AddDays(offset)).ToList();

            var DateFlag = _settingsServices.GetCalendarDates(startDate, endDate)
                .ToDictionary(d => d.Date, d => d.Flag);

            var document = Document.Create(container =>
            {
                _ = container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        _ = column.Item().Text(orgName).FontSize(18).Bold().AlignCenter();
                        _ = column.Item().Text("Attendance Report").FontSize(16).Bold().AlignCenter();

                        column.Item().Text(text =>
                        {
                            _ = text.Span("Report Title: ").SemiBold();
                            _ = text.Span(reportTitle);
                        });
                        if (!string.IsNullOrWhiteSpace(dutyStation))
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Duty Station: ").SemiBold();
                                _ = text.Span(dutyStation);
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(employeeType))
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Employee Type: ").SemiBold();
                                _ = text.Span(employeeType);
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(empStatus))
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Employee Status: ").SemiBold();
                                _ = text.Span(empStatus);
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(startDate.ToString()))
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Date: ").SemiBold();
                                _ = text.Span($"{startDate.ToString(dateFormat)} - {endDate.ToString(dateFormat)}");
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(absentRemarkLto))
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Late to Office: ").SemiBold();
                                _ = text.Span($"{absentRemarkLto}");
                            });
                        }
                        if (string.Equals(reportName, "frequency", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(reportName, "hours", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(reportName, "inout", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            column.Item().Text(text =>
                            {
                                _ = text.Span("").SemiBold();
                                _ = text.Span("LTO : Late to Office | LTH : Late to Home");
                            });
                        }
                    });

                    //table container
                    Func<IContainer, IContainer> CellStyleHeader = container =>
                    container
                        .Background(QuestPDF.Helpers.Colors.Grey.Lighten3)
                        .Border(1)
                        .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                        .AlignLeft()
                        .AlignMiddle();

                    Func<IContainer, IContainer> CellStyle = container =>
                   container
                       .Border(1)
                       .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                       .AlignLeft()
                       .AlignMiddle();

                    Func<string, IContainer, IContainer> RemarkCellStyle = (flag, container) =>
                    {
                        var bg = flag switch
                        {
                            "W" => QuestPDF.Helpers.Colors.Red.Lighten3,     // weekend/off -> light pink equivalent
                            "H" => QuestPDF.Helpers.Colors.Lime.Darken2,    // holiday -> dark olive green equivalent
                            _ => QuestPDF.Helpers.Colors.White
                        };
                        var textColor = flag == "H" ? QuestPDF.Helpers.Colors.White : QuestPDF.Helpers.Colors.Black;

                        return container
                            .Background(bg)
                            .Border(1)
                            .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                            .AlignLeft()
                            .AlignMiddle();
                    };


                    if (string.Equals(reportName, "remarks", StringComparison.OrdinalIgnoreCase))
                    {
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(2.5f);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    columns.RelativeColumn(1);
                                }
                            });
                            table.Header(header =>
                            {
                                _ = header.Cell().Element(CellStyleHeader).Text("#").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("Employee").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text($"{allDates[col].ToString(dateFormat)}").SemiBold();
                                }
                            });

                            int cnt = 0;
                            foreach (var item in data.Cast<ReportDailyAttendanceRangeRemarksViewModel>())
                            {
                                cnt++;
                                _ = table.Cell().Element(CellStyle).Text($"{cnt}");
                                _ = table.Cell().Element(CellStyle).Text(item.Employee);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var date = allDates[col];
                                    _ = table.Cell().Element(CellStyle).Text(item.DateRemarks[date].Remarks ?? "");
                                }
                            }
                        });
                    }
                    else if (string.Equals(reportName, "frequency", StringComparison.OrdinalIgnoreCase))
                    {
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(2.5f);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                }
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });
                            table.Header(header =>
                            {
                                _ = header.Cell().Element(CellStyleHeader).Text("#").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("Employee").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text($"{allDates[col].ToString(dateFormat)}").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("").SemiBold();
                                }
                                _ = header.Cell().Element(CellStyleHeader).Text("Total").Bold();
                                _ = header.Cell().Element(CellStyleHeader).Text("").Bold();

                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("Check In").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("LTO").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("LTH").SemiBold();
                                }
                                _ = header.Cell().Element(CellStyleHeader).Text("LTO").Bold();
                                _ = header.Cell().Element(CellStyleHeader).Text("LTH").Bold();

                            });
                            int cnt = 0;
                            foreach (var item in data.Cast<ReportDailyAttendanceRangeFrequencyViewModel>())
                            {
                                cnt++;
                                _ = table.Cell().Element(CellStyle).Text($"{cnt}");
                                _ = table.Cell().Element(CellStyle).Text(item.Employee);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var date = allDates[col];
                                    string checkIn = item.DateFrequency[date].checkIn.Length > 5 ? item.DateFrequency[date].checkIn[..5] : item.DateFrequency[date].checkIn;
                                    _ = table.Cell().Element(CellStyle).Text(checkIn ?? "");
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateFrequency[date].LTO}" ?? "");
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateFrequency[date].LTH}" ?? "");
                                }
                                _ = table.Cell().Element(CellStyle).Text($"{item.LtoT}" ?? "");
                                _ = table.Cell().Element(CellStyle).Text($"{item.LthT}" ?? "");
                            }
                        });
                    }
                    else if (string.Equals(reportName, "hours", StringComparison.OrdinalIgnoreCase))
                    {
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(2.5f);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                }
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });
                            table.Header(header =>
                            {
                                _ = header.Cell().Element(CellStyleHeader).Text("#").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("Employee").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text($"{allDates[col].ToString(dateFormat)}").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("").SemiBold();
                                }
                                _ = header.Cell().Element(CellStyleHeader).Text("Total").Bold();
                                _ = header.Cell().Element(CellStyleHeader).Text("").Bold();

                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("LTO").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("LTH").SemiBold();
                                }
                                _ = header.Cell().Element(CellStyleHeader).Text("LTO").Bold();
                                _ = header.Cell().Element(CellStyleHeader).Text("LTH").Bold();

                            });
                            int cnt = 0;
                            foreach (var item in data.Cast<ReportDailyAttendanceRangeHoursViewModel>())
                            {
                                cnt++;
                                _ = table.Cell().Element(CellStyle).Text($"{cnt}");
                                _ = table.Cell().Element(CellStyle).Text(item.Employee);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var date = allDates[col];
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateHours[date].LTOF}" ?? "");
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateHours[date].LTHF}" ?? "");
                                }
                                _ = table.Cell().Element(CellStyle).Text($"{item.LtoT}" ?? "");
                                _ = table.Cell().Element(CellStyle).Text($"{item.LthT}" ?? "");
                            }
                        });
                    }
                    else if (string.Equals(reportName, "inout", StringComparison.OrdinalIgnoreCase))
                    {
                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(0.5f);
                                columns.RelativeColumn(2.5f);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                }
                            });
                            table.Header(header =>
                            {
                                _ = header.Cell().Element(CellStyleHeader).Text("#").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("Employee").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text($"{allDates[col].ToString(dateFormat)}").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("").SemiBold();
                                }

                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                _ = header.Cell().Element(CellStyleHeader).Text("").SemiBold();
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var flag = DateFlag.ContainsKey(allDates[col]) ? DateFlag[allDates[col]] : string.Empty;
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("In").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("Out").SemiBold();
                                    _ = header.Cell().Element(c => RemarkCellStyle(flag, c)).Text("Remarks").SemiBold();
                                }
                            });
                            int cnt = 0;
                            foreach (var item in data.Cast<ReportDailyAttendanceRangeInOutViewModel>())
                            {
                                cnt++;
                                _ = table.Cell().Element(CellStyle).Text($"{cnt}");
                                _ = table.Cell().Element(CellStyle).Text(item.Employee);
                                for (int col = 0; col < allDates.Count; col++)
                                {
                                    var date = allDates[col];
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateInOutRemarks[date].In}" ?? "");
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateInOutRemarks[date].Out}" ?? "");
                                    _ = table.Cell().Element(CellStyle).Text($"{item.DateInOutRemarks[date].Remarks}" ?? "");
                                }
                            }
                        });
                    }

                    page.Footer().AlignCenter().Text(x =>
                    {
                        _ = x.CurrentPageNumber();
                        _ = x.Span(" / ");
                        _ = x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            string fileName = $"AttendanceReport_{reportTitle}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = fileName
            };
        }
        #endregion
        /**--------------------------------------------------------------------------------**/
        //  Attendance Report : Daily | Lunch
        /**--------------------------------------------------------------------------------**/
        #region ------- ATTENDANCE REPORT DAILY  AND LUNCH ------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceReportDaily([FromBody] ReportAttendanceViewModel request)
        {
            string PageId = "11104";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return Forbid(); }
            #endregion FOR END PERMISSION

            string reportName = request.report_name ?? "";
            string reportType = request.report_type ?? "";

            if (string.IsNullOrWhiteSpace(reportName)) { return Json(new { status = "invalid", message = "Insufficient information" }); }
            if (reportType is not "1" and not "2") { return Json(new { status = "invalid", message = "Unknown report type." }); }

            string dutyStationId = request.duty_station_id ?? "";
            string employeeType = request.employee_type ?? "";
            string inOutDate = request.in_out_date ?? "";
            DateTime in_out_date = DateTime.TryParse(inOutDate, out DateTime dtParse) ? dtParse : DateTime.Today;
            string ReportTitle = "Attendance Report [" + reportName + "]";
            string DutyStation = _attendanceServices.GetDutyStation(dutyStationId);
            string EmployeeType = employeeType;
            string ReportType = reportType == "1" ? "Summary" : "Detail";
            string InOutDate = in_out_date.ToString(_appSettings.DATE_FORMAT);

            ViewBag.ReportTitle = ReportTitle;
            ViewBag.DutyStation = DutyStation;
            ViewBag.EmployeeType = EmployeeType;
            ViewBag.ReportType = ReportType;
            ViewBag.InOutDate = InOutDate;

            var result = await GetAttendanceReportDataAsync(
                reportName, dutyStationId, employeeType, in_out_date, reportType
                ).ConfigureAwait(false);
            return PartialView("Attendance/_AttendanceReport" + reportName, result);
        }

        private async Task<List<ReportDailyAttendanceMainViewModel>> GetAttendanceReportDataAsync(
            string reportName, string dutyStationId, string employeeType, DateTime inOutDate, string reportType
            )
        {
            var rawRows = await _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.in_out_date == inOutDate
                    && x.duty_station_id == dutyStationId
                    && x.employee_type == employeeType)
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var result = rawRows
                .Select(MapToMainViewModel)
                .OrderBy(x => x.RemarksOrder)
                .ThenBy(x => x.employee)
                .ToList();

            if (reportName == "Daily" && reportType == "2")
            {
                var empIds = result.Select(r => r.m_emp_id).ToList();

                var subDetails = await _context.vwAttendanceDailyStaffUpdateSub
                    .Where(s => empIds.Contains(s.emp_id) && s.in_out_date == inOutDate)
                    .AsNoTracking()
                    .Select(s => new ReportDailyAttendanceSubViewModel
                    {
                        s_emp_id = s.emp_id,
                        check_in = s.check_in,
                        check_out = s.check_out
                    })
                    .ToListAsync().ConfigureAwait(false);

                var subDetailsByEmpId = subDetails
                    .GroupBy(s => s.s_emp_id)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var row in result)
                {
                    row.SubDetails = subDetailsByEmpId.TryGetValue(row.m_emp_id, out var subs)
                        ? subs
                        : new List<ReportDailyAttendanceSubViewModel>();
                }
            }

            return result;
        }

        // One shared permission + parameter parsing step for both export endpoints.
        private async Task<(IActionResult? Error, List<ReportDailyAttendanceMainViewModel>? Data)> PrepareExportAsync(
            string reportName, string dutyStationId, string employeeType, string inOutDate, string reportType)
        {
            const string PageId = "11104";
            var perm = _accountServices.GetMenuPermission(PageId);
            if (string.Equals(perm.vpern, "false", StringComparison.Ordinal)) { return (Forbid(), null); }
            if (reportType is not "1" and not "2") { return (BadRequest("Unknown report type."), null); }
            DateTime newInOutDate = DateTime.TryParse(inOutDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : DateTime.Today;
            var data = await GetAttendanceReportDataAsync(reportName, dutyStationId, employeeType, newInOutDate, reportType).ConfigureAwait(false);
            return (null, data);
        }

        private ReportDailyAttendanceMainViewModel MapToMainViewModel(vwAttendanceDailyStaffUpdate m)
        {
            DateTime dateText = m.in_out_date ?? DateTime.MinValue;

            string? lateToOffice = ComputeLateSpan(dateText, m.check_in, m.office_in_at);
            string? lateToHome = (m.check_out is not null and not "n")
                ? ComputeLateSpan(dateText, m.check_out, m.office_out_at)
                : null;

            return new ReportDailyAttendanceMainViewModel
            {
                m_emp_id = m.emp_id,
                employee = $"{m.firstname} {m.middlename} {m.lastname} ({m.emp_code})",
                in_out_date = dateText.ToString(_appSettings.DATE_FORMAT),
                first_check_in = m.check_in,
                last_check_out = m.check_out,
                remarks = m.remarks,
                narration = m.narration,
                late_to_office = lateToOffice,
                late_to_home = lateToHome,
                status = !string.IsNullOrWhiteSpace(m.check_in) ? "Present" : "Absent",
                RemarksOrder = m.RemarksOrder
            };
        }

        private static string? ComputeLateSpan(DateTime date, string? timeValue, string? officeInAt)
        {
            if (string.IsNullOrWhiteSpace(timeValue) || string.IsNullOrWhiteSpace(officeInAt))
            {
                return null;
            }

            if (!DateTime.TryParse($"{date:yyyy-MM-dd} {timeValue}", out var actual) ||
                !DateTime.TryParse($"{date:yyyy-MM-dd} {officeInAt}", out var expected) ||
                actual <= expected)
            {
                return null;
            }

            var diff = actual - expected;
            return $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}";
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReportDailyExcel(
            string reportName, string dutyStationId, string employeeType, string inOutDate, string reportType)
        {
            var (error, data) = await PrepareExportAsync(reportType, inOutDate, dutyStationId, employeeType, reportName).ConfigureAwait(false);
            if (error is not null) { return error; }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Attendance Report");

            string OrgName = _globalOptionServices.OptionServices["op_org_name"];
            string ReportTitle = "Attendance Report [" + reportName + "]";
            string DutyStation = _attendanceServices.GetDutyStation(dutyStationId);
            string EmployeeType = employeeType;
            string ReportType = reportType == "1" ? "Summary" : "Detail";
            string sInOutDate = inOutDate;
            int row = 1;
            ws.Cell(row, 2).Value = "Organization";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = OrgName;
            row++;
            ws.Cell(row, 2).Value = "Report Title";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = ReportTitle;
            row++;
            ws.Cell(row, 2).Value = "Duty Station";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = DutyStation;
            row++;
            ws.Cell(row, 2).Value = "Employee Type";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = EmployeeType;
            row++;
            ws.Cell(row, 2).Value = "Report Type";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = ReportType;
            row++;
            ws.Cell(row, 2).Value = "Date";
            ws.Cell(row, 3).Value = ":";
            ws.Cell(row, 4).Value = sInOutDate;
            row++;

            string[] headers = ["Employee", "First Check-In", "Last Check-Out", "Late to Office", "Late to Home", "Remarks", "Narration", "Status"];
            if (string.Equals(reportName, "Lunch", StringComparison.OrdinalIgnoreCase))
            {
                headers = ["Employee", "Remarks", "Narration", "Status"];
            }
            for (int col = 0; col < headers.Length; col++)
            {
                ws.Cell(row, col + 1).Value = headers[col];
            }
            ws.Row(row).Style.Font.Bold = true;
            ws.Row(row).Style.Fill.BackgroundColor = XLColor.LightGray;

            row++;
            foreach (var item in data!)
            {
                if (string.Equals(reportName, "Lunch", StringComparison.OrdinalIgnoreCase))
                {
                    ws.Cell(row, 1).Value = item.employee;
                    ws.Cell(row, 2).Value = item.remarks;
                    ws.Cell(row, 3).Value = item.narration;
                    ws.Cell(row, 4).Value = item.status;
                }
                else
                {
                    ws.Cell(row, 1).Value = item.employee;
                    ws.Cell(row, 2).Value = item.first_check_in;
                    ws.Cell(row, 3).Value = item.last_check_out;
                    ws.Cell(row, 4).Value = item.late_to_office;
                    ws.Cell(row, 5).Value = item.late_to_home;
                    ws.Cell(row, 6).Value = item.remarks;
                    ws.Cell(row, 7).Value = item.narration;
                    ws.Cell(row, 8).Value = item.status;
                }
                row++;
            }

            _ = ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            string fileName = $"AttendanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAttendanceReportDailyPdf(
            string reportName, string dutyStationId, string employeeType, string inOutDate, string reportType)
        {
            var (error, data) = await PrepareExportAsync(reportType, inOutDate, dutyStationId, employeeType, reportName).ConfigureAwait(false);
            if (error is not null) { return error; }

            string OrgName = _globalOptionServices.OptionServices["op_org_name"];
            string ReportTitle = reportName;
            string DutyStation = _attendanceServices.GetDutyStation(dutyStationId);
            string EmployeeType = employeeType;
            string ReportType = reportType == "1" ? "Summary" : "Detail";
            string InOutDate = inOutDate;

            string[] headers = ["Employee", "First Check In", "Last Check Out", "Late To Office", "Late To Home", "Remarks", "Narration", "Status"];
            if (string.Equals(reportName, "Lunch", StringComparison.OrdinalIgnoreCase))
            {
                headers = ["Employee", "Remarks", "Narration", "Status"];
            }
            var document = Document.Create(container =>
            {
                _ = container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Column(column =>
                        {
                            _ = column.Item()
                                .Text(OrgName)
                                .FontSize(18)
                                .Bold()
                                .AlignCenter();

                            _ = column.Item()
                                .Text("Attendance Report")
                                .FontSize(16)
                                .Bold()
                                .AlignCenter();
                            column.Item().Text(text =>
                            {
                                _ = text.Span("Report Title: ").SemiBold();
                                _ = text.Span(ReportTitle);
                            });

                            column.Item().Text(text =>
                            {
                                _ = text.Span("Duty Station: ").SemiBold();
                                _ = text.Span(DutyStation);
                            });

                            column.Item().Text(text =>
                            {
                                _ = text.Span("Employee Type: ").SemiBold();
                                _ = text.Span(EmployeeType);
                            });

                            column.Item().Text(text =>
                            {
                                _ = text.Span("Report Type: ").SemiBold();
                                _ = text.Span(ReportType);
                            });

                            column.Item().Text(text =>
                            {
                                _ = text.Span("Date: ").SemiBold();
                                _ = text.Span(InOutDate);
                            });
                        });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            if (string.Equals(reportName, "Lunch", StringComparison.OrdinalIgnoreCase))
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            }
                            else
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                _ = header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(3).Text(h).Bold();
                            }
                        });

                        foreach (var item in data!)
                        {
                            if (string.Equals(reportName, "Lunch", StringComparison.OrdinalIgnoreCase))
                            {
                                _ = table.Cell().Padding(3).Text(item.employee ?? "");
                                _ = table.Cell().Padding(3).Text(item.remarks ?? "");
                                _ = table.Cell().Padding(3).Text(item.narration ?? "");
                                _ = table.Cell().Padding(3).Text(item.status ?? "");
                            }
                            else
                            {
                                _ = table.Cell().Padding(3).Text(item.employee ?? "");
                                _ = table.Cell().Padding(3).Text(item.first_check_in ?? "");
                                _ = table.Cell().Padding(3).Text(item.last_check_out ?? "");
                                _ = table.Cell().Padding(3).Text(item.late_to_office ?? "");
                                _ = table.Cell().Padding(3).Text(item.late_to_home ?? "");
                                _ = table.Cell().Padding(3).Text(item.remarks ?? "");
                                _ = table.Cell().Padding(3).Text(item.narration ?? "");
                                _ = table.Cell().Padding(3).Text(item.status ?? "");
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        _ = x.CurrentPageNumber();
                        _ = x.Span(" / ");
                        _ = x.TotalPages();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            string fileName = $"AttendanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        #endregion
        /**--------------------------------------------------------------------------------**/
        #endregion
        /********************************************************************************************************************/
        #region ATTENDANCE UPDATE OR FIXING
        [HttpGet]
        public IActionResult AttendanceUpdate()
        {
            string PageId = "11101";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            //This is default "Today" and "KTM" only
            DateTime in_out_date = DateTime.Today;
            string DutyStationFilter = "1";
            string EmployeeTypeFilter = "";
            string EmployeeStatusFilter = "A";

            var result = _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.in_out_date == in_out_date
                && x.duty_station_id == DutyStationFilter
                && (string.IsNullOrWhiteSpace(EmployeeTypeFilter) || x.employee_type == EmployeeTypeFilter) //filter appy only if filter has value
                && (x.emp_status == EmployeeStatusFilter)
                )
                .Select(x => new DailyCheckInOutStaffUpdateViewModel
                {
                    main_id = x.id,
                    emp_id = x.emp_id,
                    firstname = x.firstname,
                    middlename = x.middlename,
                    lastname = x.lastname,
                    employee = $"{x.firstname} {x.middlename} {x.lastname} ({x.emp_code})",
                    check_in = x.check_in,
                    check_out = x.check_out,
                    remarks = x.remarks,
                    narration = x.narration,
                    RemarksOrder = x.RemarksOrder,
                    status = !string.IsNullOrWhiteSpace(x.check_in) ? "Present" : "Absent"
                })
                .OrderBy(x => x.RemarksOrder)
                .ThenBy(x => x.firstname)
                .ThenBy(x => x.middlename)
                .ThenBy(x => x.lastname)
                .ToList();

            ViewBag.InOutDay = in_out_date;
            ViewBag.DutyStationFilter = _employeeServices.GetDutyStationList("1");
            ViewBag.EmployeeTypeFilter = _attendanceServices.GetEmployeeType("Inside");
            ViewBag.EmployeeStatusFilter = StatusActivePassive("AD", EmployeeStatusFilter);
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.RemarksList = _attendanceServices.GetRemarksList("remarks");
            ViewBag.NarrationList = _attendanceServices.GetNarrationList("narration");
            return PartialView("Attendance/_AttendanceUpdate", result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceUpdateList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);

            string EmployeeStatusFilter = request.FilterValue1 ?? "";
            string EmployeeTypeFilter = request.FilterValue2 ?? "";
            string DutyStationFilter = request.FilterValue3 ?? "";
            DateTime in_out_date = DateTime.TryParse(request.FilterValue4, out DateTime dtParse) ? dtParse : DateTime.Today;

            var query = _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.in_out_date == in_out_date
                && x.duty_station_id == DutyStationFilter
                && (string.IsNullOrWhiteSpace(EmployeeTypeFilter) || x.employee_type == EmployeeTypeFilter) //filter appy only if filter has value
                && (x.emp_status == EmployeeStatusFilter)
                )
                .Select(x => new DailyCheckInOutStaffUpdateViewModel
                {
                    main_id = x.id,
                    emp_id = x.emp_id,
                    firstname = x.firstname,
                    middlename = x.middlename,
                    lastname = x.lastname,
                    employee = $"{x.firstname} {x.middlename} {x.lastname} ({x.emp_code})",
                    check_in = x.check_in,
                    check_out = x.check_out,
                    remarks = x.remarks,
                    narration = x.narration,
                    RemarksOrder = x.RemarksOrder,
                    status = !string.IsNullOrWhiteSpace(x.check_in) ? "Present" : "Absent"
                });

            query = query.OrderBy(x => x.RemarksOrder)
                    .ThenBy(x => x.firstname)
                    .ThenBy(x => x.middlename)
                    .ThenBy(x => x.lastname);

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                (a.remarks != null && a.remarks.Contains(searchValue)) ||
                (a.narration != null && a.narration.Contains(searchValue)) ||
                (a.firstname != null && a.firstname.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }

        public IActionResult AttendanceUpdateAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "11101";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            ViewBag.mode = mode;

            if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    string main_id = id;
                    var smt = _context.vwAttendanceDailyStaffUpdate.FirstOrDefault(a => a.id == main_id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        var model = new DailyCheckInOutStaffUpdateViewModel
                        {
                            main_id = smt.id,
                            emp_id = smt.emp_id,
                            employee = $"{smt.firstname} {smt.middlename} {smt.lastname} ({smt.emp_code})",
                            emp_status = smt.emp_status == "A" ? "Active" : "Inactive",
                            in_out_date = smt.in_out_date,
                            check_in = smt.check_in,
                            check_out = smt.check_out,
                            remarks = smt.remarks,
                            narration = smt.narration,
                            employee_type = smt.employee_type,
                            duty_station_id = smt.duty_station_id,
                            status = !string.IsNullOrWhiteSpace(smt.check_in) ? "Present" : "Absent"
                        };
                        DateTime inOutDate = Convert.ToDateTime(smt.in_out_date);
                        ViewBag.DutyStation = _attendanceServices.GetDutyStation(smt.duty_station_id);
                        ViewBag.RemarksList = _attendanceServices.GetRemarksList(smt.remarks);
                        ViewBag.NarrationList = _attendanceServices.GetNarrationList(smt.narration);
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        ViewBag.AttendanceUpdateSub = _attendanceServices.GetAttendanceUpdateSub(smt.emp_id, inOutDate, smt.employee_type);
                        ViewBag.AttendanceUpdateChangeLog = _attendanceServices.GetAttendanceUpdateChangeLog(smt.emp_id, inOutDate, smt.employee_type);
                        return PartialView("Attendance/_AttendanceUpdateAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AttendanceUpdateSave(DailyCheckInOutStaffUpdateViewModel model)
        {
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("11101", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string check_in = model.check_in ?? "";
            string check_out = model.check_out ?? "";
            string remarks = model.remarks ?? "";
            string narration = model.narration ?? "";
            string reason = model.reason ?? "";
            string employee_type = model.employee_type ?? "Inside";
            DateTime in_out_date = Convert.ToDateTime(model.in_out_date);
            int emp_id = Convert.ToInt32(model.emp_id);
            string newVal = $@"First Check In : {check_in} | Last Check Out : {check_out} | 
                       Remarks : {remarks} | Narration : {narration}";

            if (string.IsNullOrWhiteSpace(model.main_id)) { return Json(new { status = "invalid", message = Lang.msg_insufficient_info }); }
            string MainId = model.main_id;
            string existingVal = _attendanceServices.GetCheckInOutInfo(MainId);
            if (mode == "edit")
            {
                int by_emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
                if (employee_type == "Inside")
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_main.FirstOrDefault(h => h.id == MainId);
                    if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                    DataUpdate.check_in = check_in;
                    DataUpdate.check_out = check_out;
                    DataUpdate.remarks = remarks;
                    DataUpdate.narration = narration;
                    _ = _context.tbl_employee_check_in_out_main.Update(DataUpdate);

                    if (by_emp_id > 0)
                    {
                        string log_id = UniqueID();
                        var DataSave = new tbl_employee_check_in_out_change_log
                        {
                            id = log_id,
                            emp_id = emp_id,
                            in_out_date = in_out_date,
                            old_value = existingVal,
                            new_value = newVal,
                            by_emp_id = by_emp_id,
                            change_date = DateTime.Now,
                            change_on = "Main",
                            change_type = "Update",
                            reason = model.reason
                        };
                        _ = _context.tbl_employee_check_in_out_change_log.Add(DataSave);
                    }
                    _ = _context.SaveChanges();
                }
                else
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_main_outside.FirstOrDefault(h => h.id == MainId);
                    if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                    DataUpdate.check_in = check_in;
                    DataUpdate.check_out = check_out;
                    DataUpdate.remarks = remarks;
                    DataUpdate.narration = narration;
                    _ = _context.tbl_employee_check_in_out_main_outside.Update(DataUpdate);

                    if (by_emp_id > 0)
                    {
                        string log_id = UniqueID();
                        var DataSave = new tbl_employee_check_in_out_change_log_outside
                        {
                            id = log_id,
                            emp_id = emp_id,
                            in_out_date = in_out_date,
                            old_value = existingVal,
                            new_value = newVal,
                            by_emp_id = by_emp_id,
                            change_date = DateTime.Now,
                            change_on = "Main",
                            change_type = "Update",
                            reason = model.reason
                        };
                        _ = _context.tbl_employee_check_in_out_change_log_outside.Add(DataSave);
                    }
                    _ = _context.SaveChanges();
                }
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AttendanceUpdateBulkSave([FromBody] BulkUpdateRequest request)
        {
            if (!_accountServices.HasPermission("11101", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1) { return Json(new { status = "false", message = Lang.msg_no_record_selected }); }
            var Fields = request.Fields?.FirstOrDefault();
            string? remarks = Fields?.Field1;
            string? narration = Fields?.Field2;
            string? reason = Fields?.Field3;

            if (string.IsNullOrWhiteSpace(reason)) { return Json(new { status = "error", message = "Reason is required." }); }

            foreach (var attend in request.SelectedIds)
            {
                string MainId = attend;
                var existing = _context.vwAttendanceDailyStaffUpdate.FirstOrDefault(e => e.id == MainId);
                string existingVal = _attendanceServices.GetCheckInOutInfo(MainId, "B");
                string employee_type = existing.employee_type;
                string check_in = existing.check_in ?? "";
                string check_out = existing.check_out ?? "";
                DateTime in_out_date = Convert.ToDateTime(existing.in_out_date);
                int emp_id = existing.emp_id;
                string newVal = $@"Remarks : {remarks} | Narration : {narration}";
                int by_emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;
                if (employee_type == "Inside")
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_main.FirstOrDefault(e => e.id == MainId);
                    if (DataUpdate != null)
                    {
                        DataUpdate.remarks = remarks;
                        DataUpdate.narration = narration;
                        _ = _context.tbl_employee_check_in_out_main.Update(DataUpdate);

                        if (by_emp_id > 0)
                        {
                            string log_id = UniqueID();
                            var DataSave = new tbl_employee_check_in_out_change_log
                            {
                                id = log_id,
                                emp_id = emp_id,
                                in_out_date = in_out_date,
                                old_value = existingVal,
                                new_value = newVal,
                                by_emp_id = by_emp_id,
                                change_date = DateTime.Now,
                                change_on = "Main",
                                change_type = "Update",
                                reason = reason
                            };
                            _ = _context.tbl_employee_check_in_out_change_log.Add(DataSave);
                        }
                    }
                }
                else
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_main_outside.FirstOrDefault(e => e.id == MainId);
                    if (DataUpdate != null)
                    {
                        DataUpdate.remarks = remarks;
                        DataUpdate.narration = narration;
                        _ = _context.tbl_employee_check_in_out_main_outside.Update(DataUpdate);

                        if (by_emp_id > 0)
                        {
                            string log_id = UniqueID();
                            var DataSave = new tbl_employee_check_in_out_change_log_outside
                            {
                                id = log_id,
                                emp_id = emp_id,
                                in_out_date = in_out_date,
                                old_value = existingVal,
                                new_value = newVal,
                                by_emp_id = by_emp_id,
                                change_date = DateTime.Now,
                                change_on = "Main",
                                change_type = "Update",
                                reason = reason
                            };
                            _ = _context.tbl_employee_check_in_out_change_log_outside.Add(DataSave);
                        }
                    }
                }
            }
            _ = _context.SaveChanges();
            return Json(new { status = "success", message = Lang.msg_update_success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AttendanceUpdateSubSave([FromBody] AttendanceUpdateSubRequest request)
        {
            if (!ModelState.IsValid) { return Json(new { status = "invalid", message = Lang.msg_error_invalid }); }
            if (!_accountServices.HasPermission("11101", "add")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string mode = request.mode ?? "";
            string duty_station_id = request.duty_station_id ?? "";
            string check_in = request.check_in ?? "";
            string check_out = request.check_out ?? "";
            string reason = request.reason ?? "";
            string employee_type = request.employee_type ?? "Inside";
            DateTime in_out_date = Convert.ToDateTime(request.in_out_date);
            int emp_id = Convert.ToInt32(request.emp_id);

            string newVal = $@"Check In : {check_in} | Check Out : {check_out} | Reason : {reason}";
            string existingVal = "";
            int by_emp_id = int.TryParse(HttpContext.Session.GetString("emp_id"), out int EmpId) ? EmpId : 0;

            if (mode == "add")
            {
                if (employee_type == "Inside")
                {
                    string sub_id = UniqueID();
                    var DataSaveSub = new tbl_employee_check_in_out_sub
                    {
                        id = sub_id,
                        emp_id = emp_id,
                        in_out_date = in_out_date,
                        check_in = check_in,
                        check_out = check_out,
                        duty_station_id = duty_station_id
                    };
                    _ = _context.tbl_employee_check_in_out_sub.Add(DataSaveSub);

                    if (by_emp_id > 0)
                    {
                        string log_id = UniqueID();
                        var DataSave = new tbl_employee_check_in_out_change_log
                        {
                            id = log_id,
                            emp_id = emp_id,
                            in_out_date = in_out_date,
                            old_value = existingVal,
                            new_value = newVal,
                            by_emp_id = by_emp_id,
                            change_date = DateTime.Now,
                            change_on = "Sub",
                            change_type = "Added",
                            reason = reason
                        };
                        _ = _context.tbl_employee_check_in_out_change_log.Add(DataSave);
                    }
                    _ = _context.SaveChanges();
                }
                else
                {
                    string sub_id = UniqueID();
                    var DataSaveSub = new tbl_employee_check_in_out_sub_outside
                    {
                        id = sub_id,
                        emp_id = emp_id,
                        in_out_date = in_out_date,
                        check_in = check_in,
                        check_out = check_out,
                        duty_station_id = duty_station_id
                    };
                    _ = _context.tbl_employee_check_in_out_sub_outside.Add(DataSaveSub);

                    if (by_emp_id > 0)
                    {
                        string log_id = UniqueID();
                        var DataSave = new tbl_employee_check_in_out_change_log_outside
                        {
                            id = log_id,
                            emp_id = emp_id,
                            in_out_date = in_out_date,
                            old_value = existingVal,
                            new_value = newVal,
                            by_emp_id = by_emp_id,
                            change_date = DateTime.Now,
                            change_on = "Sub",
                            change_type = "Added",
                            reason = reason
                        };
                        _ = _context.tbl_employee_check_in_out_change_log_outside.Add(DataSave);
                    }
                    _ = _context.SaveChanges();
                }
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else if (mode == "edit")
            {
                string sub_id = request.id;
                existingVal = _attendanceServices.GetCheckInOutInfo(sub_id, "S");
                if (employee_type == "Inside")
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_sub.FirstOrDefault(e => e.id == sub_id);
                    if (DataUpdate != null)
                    {
                        DataUpdate.check_in = check_in;
                        DataUpdate.check_out = check_out;
                        _ = _context.tbl_employee_check_in_out_sub.Update(DataUpdate);

                        if (by_emp_id > 0)
                        {
                            string log_id = UniqueID();
                            var DataSave = new tbl_employee_check_in_out_change_log
                            {
                                id = log_id,
                                emp_id = emp_id,
                                in_out_date = in_out_date,
                                old_value = existingVal,
                                new_value = newVal,
                                by_emp_id = by_emp_id,
                                change_date = DateTime.Now,
                                change_on = "Sub",
                                change_type = "Updated",
                                reason = reason
                            };
                            _ = _context.tbl_employee_check_in_out_change_log.Add(DataSave);
                        }
                    }
                    _ = _context.SaveChanges();
                }
                else
                {
                    var DataUpdate = _context.tbl_employee_check_in_out_sub_outside.FirstOrDefault(e => e.id == sub_id);
                    if (DataUpdate != null)
                    {
                        DataUpdate.check_in = check_in;
                        DataUpdate.check_out = check_out;
                        _ = _context.tbl_employee_check_in_out_sub_outside.Update(DataUpdate);

                        if (by_emp_id > 0)
                        {
                            string log_id = UniqueID();
                            var DataSave = new tbl_employee_check_in_out_change_log_outside
                            {
                                id = log_id,
                                emp_id = emp_id,
                                in_out_date = in_out_date,
                                old_value = existingVal,
                                new_value = newVal,
                                by_emp_id = by_emp_id,
                                change_date = DateTime.Now,
                                change_on = "Sub",
                                change_type = "Updated",
                                reason = reason
                            };
                            _ = _context.tbl_employee_check_in_out_change_log_outside.Add(DataSave);
                        }
                    }
                    _ = _context.SaveChanges();
                }
                return Json(new { status = "success", message = Lang.msg_update_success });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion
        /********************************************************************************************************************/
        #region TODAYS ATTENDANCE STAFF UPDATE
        [HttpGet]
        public IActionResult AttendanceStaffUpdate()
        {
            string PageId = "11102";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            //This is default "Today" and "KTM" only
            DateTime in_out_date = DateTime.Today;
            string DutyStationFilter = "1";
            string EmployeeTypeFilter = "";

            var result = _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.in_out_date == in_out_date
                && x.duty_station_id == DutyStationFilter
                && (string.IsNullOrWhiteSpace(EmployeeTypeFilter) || x.employee_type == EmployeeTypeFilter) //filter appy only if filter has value
                && (string.IsNullOrWhiteSpace(x.check_in) || x.remarks == "Out of office" || x.remarks == "Leave" || x.remarks == "Travel")
                )
                .Select(x => new DailyCheckInOutStaffUpdateViewModel
                {
                    emp_id = x.emp_id,
                    firstname = x.firstname,
                    middlename = x.middlename,
                    lastname = x.lastname,
                    employee = $"{x.firstname} {x.middlename} {x.lastname} ({x.emp_code})",
                    remarks = x.remarks,
                    narration = x.narration,
                    RemarksOrder = x.RemarksOrder,
                    status = "Absent"
                })
                .OrderBy(x => x.RemarksOrder)
                .ThenBy(x => x.firstname)
                .ThenBy(x => x.middlename)
                .ThenBy(x => x.lastname)
                .ToList();

            /**------------------------------------------------------------------------------*/
            /* create place holder for showing staff update sent or not and send now button  */
            /**------------------------------------------------------------------------------*/
            string str_show_update_sent_not_sent = "Staff update has not been sent yet.";
            string str_text_color = "error";
            string str_send_now = "";
            if (result.Count > 0 && (perm.apern == "true" || perm.epern == "true"))
            {
                str_send_now = @"<input name=""btnSend"" id=""btnSend"" type=""button"" class=""button bg-red"" value=""Send Now"" />&nbsp;";
            }
            var isSent = _context.tbl_employee_check_in_out_staff_update
                .Where(iS => iS.in_out_date == in_out_date && iS.duty_station_id == DutyStationFilter).ToList();
            if (isSent.Count > 0)
            {
                str_show_update_sent_not_sent = $"Staff update has been already sent for {isSent.Count}  time(s).";
                str_text_color = "success";
                str_send_now = "";
            }
            ViewBag.showSentMessage = str_show_update_sent_not_sent;
            ViewBag.messageColor = str_text_color;
            ViewBag.sendButton = str_send_now;
            /**------------------------------------------------------------------------------*/

            ViewBag.InOutDay = in_out_date;
            ViewBag.DutyStationFilter = _attendanceServices.GetDutyStationList("1");
            ViewBag.EmployeeTypeFilter = _attendanceServices.GetEmployeeType("Inside");
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            return PartialView("Attendance/_AttendanceStaffUpdate", result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceStaffUpdateList([FromForm] MultipleCostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            DateTime in_out_date = DateTime.TryParse(request.FilterValue1, out DateTime dtParse) ? dtParse : DateTime.Today;
            string DutyStationFilter = request.FilterValue2 ?? "";
            string EmployeeTypeFilter = request.FilterValue3 ?? "";

            var query = _context.vwAttendanceDailyStaffUpdate
                .Where(x => x.in_out_date == in_out_date
                && (string.IsNullOrWhiteSpace(DutyStationFilter) || x.duty_station_id == DutyStationFilter)
                && (string.IsNullOrWhiteSpace(EmployeeTypeFilter) || x.employee_type == EmployeeTypeFilter)
                && (string.IsNullOrWhiteSpace(x.check_in) || x.remarks == "Out of office" || x.remarks == "Leave" || x.remarks == "Travel")
                )
                .Select(x => new DailyCheckInOutStaffUpdateViewModel
                {
                    emp_id = x.emp_id,
                    firstname = x.firstname,
                    middlename = x.middlename,
                    lastname = x.lastname,
                    employee = $"{x.firstname} {x.middlename} {x.lastname} ({x.emp_code})",
                    remarks = x.remarks,
                    narration = x.narration,
                    RemarksOrder = x.RemarksOrder,
                    status = "Absent"
                });

            var data = query.ToList();
            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AttendanceStaffUpdateSendNow([FromBody] AttendanceStaffUpdateSendViewModel model)
        {
            string PageId = "10510";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            if (perm.apern != "true" && perm.epern != "true") { return Json(new { status = "invalid", message = "Not Authorized User" }); }
            //currently not using value provided by model 
            //if (!ModelState.IsValid) { return Json(new { status = false, message = Lang.msg_error_invalid }); }
            //if (model?.Fields == null || !model.Fields.Any()) { return Json(new { status = false, message = "No employees received." }); }

            DateTime InOutDate = DateTime.Today;    //Today only
            string DutyStationFilter = "1"; //KTM only
            string EmployeeTypeFilter = ""; //send all inside and outside
            string SendBy = HttpContext.Session.GetString("emp_id") ?? "0";
            int SendByEmpId = int.TryParse(SendBy, out int EmpId) ? EmpId : 0;
            string SendMode = "MANUALSEND";
            bool sentStatus = _attendanceServices.autoSendEmailStaffUpdate(EmployeeTypeFilter, InOutDate, DutyStationFilter, SendByEmpId, SendMode);
            if (sentStatus) { return Json(new { status = sentStatus, message = "Staff Update sent successfully!" }); }
            return Json(new { status = sentStatus, message = "Fail to send Staff Update!" });
        }

        #endregion
        /********************************************************************************************************************/
        #region ATTENDANCE STAFF UPDATE SETTINGS
        [HttpGet]
        public IActionResult StaffUpdateSettings()
        {
            string PageId = "11103";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            EmployeeCheckInOutSettingViewModel model;
            var Records = _context.tbl_employee_check_in_out_setting.FirstOrDefault();
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new EmployeeCheckInOutSettingViewModel
                {
                    id = 0,
                    send_staff_update = "N",
                    send_hrs_min = "",
                    send_am_pm = "",
                    send_off_days = ""
                };
            }
            else
            {
                ViewBag.mode = "edit";
                model = new EmployeeCheckInOutSettingViewModel
                {
                    id = Records.id,
                    send_staff_update = Records.send_staff_update,
                    send_hrs_min = Records.send_hrs_min,
                    send_am_pm = Records.send_am_pm,
                    send_off_days = Records.send_off_days,
                };
            }
            ViewBag.SendStaffUpdate = StatusActivePassive("YN", model.send_staff_update ?? "");
            ViewBag.SendHoursMinutes = GetHoursMinutes(model.send_hrs_min ?? "");
            ViewBag.SendAMPM = GetAMPM(model.send_am_pm ?? "");
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            return PartialView("Attendance/_StaffUpdateSettings", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult StaffUpdateSettingsSave(EmployeeCheckInOutSettingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("11103", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string send_staff_update = model.send_staff_update ?? "";
            string send_hrs_min = model.send_hrs_min ?? "";
            string send_am_pm = model.send_am_pm ?? "";
            string send_off_days = model.send_off_days ?? "";

            if (string.IsNullOrEmpty(send_staff_update) || string.IsNullOrEmpty(send_hrs_min) || string.IsNullOrEmpty(send_am_pm)) { return Json(new { status = "invalid", message = Lang.msg_insufficient_info }); }

            if (mode == "add")
            {
                byte id = (byte)(_context.tbl_employee_check_in_out_setting.Select(o => (int)o.id).DefaultIfEmpty(0).Max() + 1);
                var DataSave = new tbl_employee_check_in_out_setting
                {
                    send_staff_update = send_staff_update,
                    send_hrs_min = send_hrs_min,
                    send_am_pm = send_am_pm,
                    send_off_days = send_off_days
                };
                _ = _context.tbl_employee_check_in_out_setting.Add(DataSave);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_added_success, id });
            }
            else if (mode == "edit")
            {
                byte id = model.id;
                var DataUpdate = _context.tbl_employee_check_in_out_setting.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.send_staff_update = send_staff_update;
                DataUpdate.send_hrs_min = send_hrs_min;
                DataUpdate.send_am_pm = send_am_pm;
                DataUpdate.send_off_days = send_off_days;
                _ = _context.tbl_employee_check_in_out_setting.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, DataUpdate.id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        #endregion
        /********************************************************************************************************************/

    }
}
