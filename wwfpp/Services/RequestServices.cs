using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwfpp.Data;
using wwfpp.Models;
namespace wwfpp.Services
{
    public class RequestServices
    {
        private readonly AppDbContext _context;
        private readonly AdministrationEmailService _administrationEmailService;

        public RequestServices(AppDbContext context, AdministrationEmailService administrationEmailService)
        {
            _context = context;
            _administrationEmailService = administrationEmailService;
        }

        public string GetApplicationSetting(string optionName)
        {
            var opt = _context.tbl_pp_options
                .Where(e => e.option_name == optionName)
                .Select(e => e.option_value)
                .FirstOrDefault();

            return opt ?? string.Empty;
        }

        public IEnumerable<tbl_fund_source> GetFundSource()
        {
            return _context.tbl_fund_source.Where(e => e.default_for_holiday == "0").ToList();
        }

        public IEnumerable<vw_Employee> GetEmployeeList()
        {
            return _context.vw_Employee.ToList();
        }
        public IEnumerable<GetEmployeeTimesheetPivot> GetEmployeeTimesheet(int year, int month, int empId, int timeSheetCounter)
        {
            return _context.Set<GetEmployeeTimesheetPivot>()
                .FromSqlRaw("EXEC dbo.GetEmployeeTimesheetPivot @emp_year = {0}, @emp_month = {1}, @emp_id = {2}, @timeSheetCounter = {3}", year, month, empId, timeSheetCounter)
                .ToList();
        }

        public IEnumerable<vw_EmployeeOvertime> GetEmployeeOvertime(string ReportType, string? Status, int? Employee, DateTime StartDate, DateTime EndDate)
        {
            if (Status == "A")
            {
                if (ReportType == "Approved")
                    return _context.vw_EmployeeOvertime.Where(c =>c.EmployeeStatus == "Active" && c.OvertimeStatus == "Approved").ToList();
                else
                    return _context.vw_EmployeeOvertime.Where(c => c.EmployeeStatus == "Active" && c.OvertimePaidedStatus == "Paid").ToList();
            }
            else
            {
                if (ReportType == "A")
                    return _context.vw_EmployeeOvertime.Where(c => c.EmployeeStatus == "Inactive" && c.OvertimeStatus == "Approved").ToList();
                else
                    return _context.vw_EmployeeOvertime.Where(c => c.EmployeeStatus == "Inactive" && c.OvertimePaidedStatus == "Paid").ToList();

            }
        }

        public double GetBalanceTillDateDay(int empId, DateTime fiscalStart, DateTime currentDay, int fundId, double annualHrs)
        {
            if (fiscalStart.Month == currentDay.Month)
                return annualHrs;

            var cutoffDate = currentDay.AddDays(-1);

            // Load candidate rows into memory
            var subRecords = _context.tbl_employee_timesheet_sub
                .Where(x => x.emp_id == empId
                         && x.fund_id == fundId
                         && x.is_active == "A")
                .ToList();   // materialize here

            // Now do the date math in C#
            var used = subRecords
                .Where(x =>
                {
                    var dt = new DateTime(
                        x.emp_year.GetValueOrDefault(),
                        x.emp_month.GetValueOrDefault(),
                        x.emp_day.GetValueOrDefault()
                    );
                    return dt >= fiscalStart && dt <= cutoffDate;
                })
                .Sum(x => (x.time_hours ?? 0) + (x.overtime_hours ?? 0));

            return annualHrs - used;
        }


        public double GetThisMonthActual(int empId, int year, int month, int fundId, int counter)
        {
            var sum = _context.tbl_employee_timesheet_sub
                .Where(x => x.emp_id == empId
                         && x.fund_id == fundId
                         && x.submit_counter == counter
                         && x.emp_year == year
                         && x.emp_month == month)
                .Sum(x => (double?)(x.time_hours) ?? 0);

            return sum;
        }
        public double GetThisMonthActualOvertime(int empId, int year, int month, int fundId, int counter)
        {
            var sum = _context.tbl_employee_timesheet_sub
                .Where(x => x.emp_id == empId
                         && x.fund_id == fundId
                         && x.submit_counter == counter
                         && x.emp_year == year
                         && x.emp_month == month)
                .Sum(x => (double?)(x.overtime_hours) ?? 0);

            return sum;
        }

        public async Task<int> GetCurrentMaxCounterAsync(int empId, int empYear, int empMonth)
        {
            var query = _context.tbl_employee_timesheet_sub
                .Where(ts => ts.emp_id == empId
                          && ts.emp_year == (short)empYear
                          && ts.emp_month == (byte)empMonth)
                .Select(ts => (int?)ts.submit_counter);

            return await query.MaxAsync() ?? 1;
        }

        public async Task<int> GetTimeSheetCounter(int empId, int empYear, int empMonth)
        {
            var maxCounter = await _context.tbl_employee_timesheet_app
                .Where(ts => ts.emp_id == empId
                          && ts.emp_year == (short)empYear
                          && ts.emp_month == (byte)empMonth
                          && ts.app_dec == "a")
                .MaxAsync(ts => (int?)ts.submit_counter);

            return (maxCounter.HasValue ? maxCounter.Value + 1 : 1);
        }
        /// <summary>
        /// Gets the status of a timesheet for a given employee, year, month, and counter.
        /// Possible values: "notsaved", "justsaved", "pending", "active", "declined", "inactive".
        /// </summary>
        public async Task<string> GetTimesheetStatusAsync(int empId, int empYear, int empMonth, int curMaxCounter)
        {
            // First check if any submission exists
            var hasSubmission = await _context.tbl_employee_timesheet_sub
                .AnyAsync(ts => ts.emp_id == empId
                             && ts.emp_year == empYear
                             && ts.emp_month == empMonth
                             && ts.submit_counter == curMaxCounter);

            if (!hasSubmission) return "notsaved";

            // Then check the approval/decision record
            var appDec = await _context.tbl_employee_timesheet_app
                .Where(app => app.emp_id == empId
                           && app.emp_year == empYear
                           && app.emp_month == empMonth
                           && app.submit_counter == curMaxCounter)
                .Select(app => app.app_dec)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(appDec)) return "justsaved";

            return appDec switch
            {
                "p" => "pending",
                "a" => "active",
                "d" => "declined",
                "i" => "inactive",
                _ => "justsaved"
            };
        }

        public string GetPreviousTimesheetMessage(string prevStatus)
        {
            return prevStatus switch
            {
                "notsaved" => "Previous timesheet has not been saved.",
                "justsaved" => "Previous timesheet was saved but not sent for approval.",
                "pending" => "Previous timesheet is pending approval.",
                "declined" => "Previous timesheet was declined.",
                "inactive" => "Previous timesheet is inactive.",
                _ => "Previous timesheet is not filled."
            };
        }
        public string GetCurrentTimesheetMessage(string curStatus)
        {
            return curStatus switch
            {
                "active" => "Timesheet has been approved.",
                "notsaved" => "Timesheet has not been saved.",
                "justsaved" => "Timesheet was saved but not sent for approval.",
                "pending" => "Timesheet is pending approval.",
                "declined" => "Timesheet was declined.",
                "inactive" => "Timesheet is inactive.",
                _ => "Timesheet is not filled."
            };
        }

        public async Task<(DateTime? StartDate, DateTime? EndDate)> GetFiscalYearRangeAsync(DateTime start)
        {
            var fiscalYear = await _context.tbl_fiscal_year
                .FirstOrDefaultAsync(fy => fy.date_from <= start && fy.date_to >= start);

            if (fiscalYear == null)
            {
                return (null, null); // no fiscal year covers this date
            }

            return (fiscalYear.date_from, fiscalYear.date_to);
        }
        public static SelectList getTravelType(string selvalue = "")
        {
            string nv = "National"; string nt = "National";
            string iv = "International"; string it = "International";

            var travelTypeList = new List<object>
            {
                new { Value = nv, Text = nt },
                new { Value = iv, Text = it }
            };

            return new SelectList(travelTypeList, "Value", "Text", selvalue);
        }
        public async Task<(int? ManagerId, int? AltManagerId, int? LineManagerId)> GetManagerInfoAsync(int empId)
        {
            var result = await _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new { e.manager_id, e.alt_manager_id, e.line_manager_id })
                .FirstOrDefaultAsync();

            return (result?.manager_id, result?.alt_manager_id, result?.line_manager_id);
        }
        public async Task<TravelApprovalResult> GetTravelValidManagerInfoAsync(
            int empId,
            string travelType,
            int? immediateSupervisorId,
            int? lineDirectorId,
            int? crEmpId,
            int? altCrEmpId,
            int? dooEmpId,
            string crAbsentStatus,
            string lineDirectorEmail,
            string supervisorEmail)
        {
            var emails = await _administrationEmailService.GetAdministrationEmailsAsync();
            var result = new TravelApprovalResult();

            // Case 1: Submitted by CR
            if (empId == crEmpId)
            {
                result.ApproverId = dooEmpId;
                result.ToEmployeeId = dooEmpId;
                result.ApproverEmail = emails["doo"].Email;
                result.Stage = "ad";
                result.ApproverPost = "Director of Operations";
                return result;
            }

            // Case 2: National travel
            if (travelType.Equals("NATIONAL", StringComparison.OrdinalIgnoreCase))
            {
                if (immediateSupervisorId == lineDirectorId)
                {
                    result.ApproverId = lineDirectorId;
                    result.ToEmployeeId = lineDirectorId;
                    result.ApproverEmail = lineDirectorEmail;
                    result.Stage = "ad";
                    result.ApproverPost = "Line Director"; //app_by_post
                }
                else
                {
                    result.IntermediateApproverId = immediateSupervisorId; //i_app_by
                    result.IntermediateApproverPost = "Immediate Supervisor"; //i_app_by_post
                    result.ApproverId = lineDirectorId; //app_by
                    result.ToEmployeeId = immediateSupervisorId;//i_app_by
                    result.ApproverEmail = supervisorEmail;//str_to
                    result.Stage = "rd";
                    result.ApproverPost = "Line Director"; //app_by_post
                }
                return result;
            }

            // Case 3: International travel
            if (lineDirectorId == crEmpId)
            {
                if (altCrEmpId.HasValue && altCrEmpId.Value != empId)
                {
                    result.ApproverId = altCrEmpId;//app_by
                    result.ToEmployeeId = altCrEmpId;//toemp_id
                    result.ApproverEmail = emails["acr"].Email;//str_to
                    result.Stage = "ad";
                    result.ApproverPost = "Alt Country Representative"; //app_by_post
                }
                else
                {
                    result.ApproverId = crEmpId;//app_by
                    result.ToEmployeeId = crEmpId;//toemp_id
                    result.ApproverEmail = emails["cra"].Email;//str_to
                    result.Stage = "ad";
                    result.ApproverPost = "Country Representative";//app_by_post
                }
            }
            else
            {
                int? crPresentStatus = GetCRAbsentStatus(Convert.ToInt32(crEmpId));
                if (altCrEmpId.HasValue && altCrEmpId.Value != 0 && crPresentStatus > 0)
                {
                    if (altCrEmpId == lineDirectorId)
                    {
                        result.ApproverId = altCrEmpId; //app_by
                        result.ToEmployeeId = altCrEmpId; //toemp_id
                        result.ApproverEmail = emails["acr"].Email; //str_to
                        result.Stage = "ad";
                        result.ApproverPost = "Alt Country Representative";//app_by_post
                    }
                    else if (altCrEmpId == empId)
                    {
                        result.IntermediateApproverId = lineDirectorId;//i_app_by
                        result.IntermediateApproverPost = "Line Director";//i_app_by_post
                        result.ApproverId = crEmpId;//app_by
                        result.ToEmployeeId = lineDirectorId;//toemp_id
                        result.ApproverEmail = lineDirectorEmail;//str_to
                        result.Stage = "rd";
                        result.ApproverPost = "Country Representative";//app_by_post
                    }
                    else
                    {
                        result.IntermediateApproverId = lineDirectorId;//i_app_by
                        result.IntermediateApproverPost = "Line Director";//i_app_by_post
                        result.ApproverId = altCrEmpId;//app_by
                        result.ToEmployeeId = lineDirectorId;//toemp_id
                        result.ApproverEmail = lineDirectorEmail;//str_to
                        result.Stage = "rd";
                        result.ApproverPost = "Alt Country Representative"; //app_by_post
                    }
                }
                else
                {
                    result.IntermediateApproverId = lineDirectorId;//i_app_by
                    result.IntermediateApproverPost = "Line Director";//i_app_by_post
                    result.ApproverId = crEmpId;//app_by
                    result.ToEmployeeId = lineDirectorId;//toemp_id
                    result.ApproverEmail = lineDirectorEmail;//str_to
                    result.Stage = "rd";
                    result.ApproverPost = "Country Representative"; //app_by_post
                }
            }

            return result;
        }

        public async Task<string> GetTravelEmailHtmlContent(int empTravelId)

        {

            var travelMain = await _context.tbl_employee_travel_main.Where(s => s.emp_travel_id == empTravelId).FirstOrDefaultAsync();
            string? trip_purpose = travelMain?.trip_purpose ?? string.Empty;
            string? denomination = travelMain?.denomination;
            string? remarks = travelMain?.remarks;
            string? travel_type = travelMain?.travel_type;
            string? destinations = travelMain?.destinations;
            DateTime? submit_date = travelMain?.submit_date;
            DateTime? date_from = travelMain?.date_from;
            DateTime? date_to = travelMain?.date_to;

            string Normalize(string? input, string lblNa)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return lblNa;
                return input.Replace("\r", "<br/>").Replace("\n", "<br/>");
            }

            trip_purpose = Normalize(trip_purpose, "N/A");
            denomination = Normalize(denomination, "N/A");
            remarks = Normalize(remarks, "N/A");

            decimal t_amount_1 = 0, t_amount_2 = 0, t_amount_3 = 0, t_amount_4 = 0, t_amount_5 = 0, t_amount_6 = 0;
            string show_total_1 = "", show_total_2 = "", show_total_3 = "", show_total_4 = "", show_total_5 = "", show_total_6 = "";

            var strParticulars = new StringBuilder();

            // --- Sub records (expenses) ---
            var subs = await _context.tbl_employee_travel_sub
                .Where(s => s.emp_travel_id == empTravelId)
                .ToListAsync();

            foreach (var sub in subs)
            {
                var parName = await _context.tbl_travel_particulars
                    .Where(p => p.par_id == sub.par_id)
                    .Select(p => p.particular)
                    .FirstOrDefaultAsync();

                var curName = await _context.tbl_currency
                    .Where(c => c.cur_id == sub.cur_id)
                    .Select(c => c.cur_abbr)
                    .FirstOrDefaultAsync();

                var nos = sub.nos ?? 0;
                var rate = sub.rate ?? 0;
                var amount = (decimal)nos * (decimal)rate;

                switch (sub.cur_id)
                {
                    case 1: t_amount_1 += amount; break;
                    case 2: t_amount_2 += amount; break;
                    case 3: t_amount_3 += amount; break;
                    case 4: t_amount_4 += amount; break;
                    case 5: t_amount_5 += amount; break;
                    case 6: t_amount_6 += amount; break;
                }

                strParticulars.AppendLine($@"
        <tr>
            <td align='left'>{parName}</td>
            <td align='left'>{sub.detail ?? ""}</td>
            <td align='right'>{sub.unit ?? ""}</td>
            <td align='right'>{nos}</td>
            <td align='right'>{rate:F2}</td>
            <td align='center'>{curName}</td>
            <td align='right'>{amount:F2}</td>
        </tr>");
            }

            var LocalCurrency = GetApplicationSetting("op_currency_symbol");
            if (t_amount_1 > 0) show_total_1 = $"{LocalCurrency} : {t_amount_1:F2} | ";
            if (t_amount_2 > 0) show_total_2 = $"IC : {t_amount_2:F2} | ";
            if (t_amount_3 > 0) show_total_3 = $"USD : {t_amount_3:F2} | ";
            if (t_amount_4 > 0) show_total_4 = $"Euro : {t_amount_4:F2} | ";
            if (t_amount_5 > 0) show_total_5 = $"Pound : {t_amount_5:F2} | ";
            if (t_amount_6 > 0) show_total_6 = $"CHF : {t_amount_6:F2} | ";

            // --- Fund sources (up to 4 slots) ---
            var strFundSources = new StringBuilder();
            for (int sn = 1; sn <= 4; sn++)
            {
                var fundId = await _context.tbl_employee_travel_codes
                    .Where(c => c.emp_travel_id == empTravelId && c.sn == sn)
                    .Select(c => c.fund_id)
                    .FirstOrDefaultAsync();

                if (fundId > 0)
                {
                    var fundName = await _context.tbl_fund_source
                        .Where(f => f.fund_id == fundId)
                        .Select(f => f.fund_source)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(fundName))
                        strFundSources.AppendLine($"{fundName}<br/>");
                }
            }

            // --- Final HTML ---
            var strParticularsDetail = $@"
    <b>Travel Type : </b>{travel_type}
    <br/><b>Purpose of Trip : </b>{trip_purpose}
    <br/><b>Destination/s : </b>{destinations}
    <br/><b>Submit Date : </b>{submit_date}
    <br/><b>Start Date : </b>{date_from}
    <br/><b>End Date : </b>{date_to}
    <br/><br/><div>
    <table border='0' bgcolor='#cccccc'>
        <tr>
            <td align='left' bgcolor='#eeeeee'><b>Particulars</b></td>
            <td align='left' bgcolor='#eeeeee'><b>Details</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Unit</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Nos.</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Rate</b></td>
            <td align='center' bgcolor='#eeeeee'><b>Currency</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Amount</b></td>
        </tr>
        {strParticulars}
        <tr bgcolor='#eeeeee'>
            <td align='left' colspan='7'>Total: {show_total_1}{show_total_2}{show_total_3}{show_total_4}{show_total_5}{show_total_6}</td>
        </tr>
    </table>
    </div>

    <br/><br/><b>Fund Source: </b><br/>{strFundSources}
    <br/><b>Currency Denomination: </b><br/>{denomination}
    <br/><br/><b>Remarks: </b><br/>{remarks}";

            return strParticularsDetail;
        }
        public int GetCRAbsentStatus(int cr_emp_id)
        {
            string curDate = DateTime.Today.ToShortDateString();
            var opt = _context.tbl_employee_travel_main
                .Where(e => e.emp_id == cr_emp_id && e.date_from <= Convert.ToDateTime(curDate)
                     && e.date_to >= Convert.ToDateTime(curDate))
                .Select(e => e.emp_id)
                .FirstOrDefault();
            return opt ?? 0;
        }

        public (int? managerID, int? lineManagerID) GetEmployeeManagerAndLineManager(int empid)
        {
            var employee = _context.tbl_employee
                .Where(e => e.emp_id == empid)
                .Select(e => new { e.manager_id, e.alt_manager_id, e.line_manager_id })
                .FirstOrDefault();

            return (employee?.manager_id , employee?.line_manager_id);
        }
        public tbl_setting_limit_hrs GetLimitHoursSetting()
        {
            return _context.tbl_setting_limit_hrs.FirstOrDefault();
        }

    }
}