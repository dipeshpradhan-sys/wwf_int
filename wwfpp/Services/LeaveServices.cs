using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography.Xml;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Services
{
    public class LeaveServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int LeaveTypeAnnual = 1;
        private const int LeaveTypeCasual = 3;
        private const int LeaveTypeSick = 5;
        private const int LeaveTypeOther = 9;
        private const int LeaveTypeMaternity = 12;
        private const int LeaveTypePaternity = 13;
        private const int LeaveTypeMourning = 14;
        private const int LeaveTypeUnpaidStudy = 15;
        private const int LeaveTypeAnnualCarryForward = 16;
        private const int LeaveTypeSickCarryForward = 17;

        public LeaveServices(
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
        * Since : 2026-Jul-07
        * Contribution: 
        /// <summary>
        /// Maps a top-level leave type identifier to the group of related leave type
        /// identifiers stored in the leave records (e.g. annual leave covers both the
        /// standard and adjusted annual leave type ids).
        /// </summary>
        ****************************************************************************************************/
        private static int[] GetRelatedLeaveTypeIds(int leaveTypeId)
        {
            return leaveTypeId switch
            {
                LeaveTypeAnnual => [1, 2],
                LeaveTypeCasual => [3, 4],
                LeaveTypeSick => [5, 6],
                LeaveTypeOther => [9, 10],
                LeaveTypeMaternity => [12],
                LeaveTypePaternity => [13],
                LeaveTypeMourning => [14],
                LeaveTypeUnpaidStudy => [15],
                _ => []
            };
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveName
        ****************************************************************************************************/
        public string GetLeaveName(int leaveTypeId)
        {
            if (leaveTypeId <= 0) { return string.Empty; }

            var leave = _context.tbl_leave_heading
                .Where(l => l.leave_type_id == leaveTypeId)
                .Select(l => new { l.abbr, l.description })
                .FirstOrDefault();

            return leave != null ? $"{leave.abbr} - {leave.description}" : string.Empty;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveAbbr
        ****************************************************************************************************/
        public string GetLeaveAbbr(int leaveTypeId)
        {
            if (leaveTypeId <= 0) { return string.Empty; }

            var leave = _context.tbl_leave_heading
                .Where(l => l.leave_type_id == leaveTypeId)
                .Select(l => new { l.abbr })
                .FirstOrDefault();

            return leave != null ? $"{leave.abbr}" : string.Empty;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveType
        ****************************************************************************************************/
        public SelectList GetLeaveType(int leaveTypeId, int whereValue)
        {
            var leaveTypeList = (whereValue > 0
                ? _context.tbl_leave_heading
                    .Where(f => f.leave_type_id == whereValue)
                : _context.tbl_leave_heading)
                .Where(f => f.category == "L")
                .OrderBy(f => f.abbr)
                .Select(f => new
                {
                    f.leave_type_id,
                    DisplayName = f.abbr + " - " + f.description
                })
                .ToList();
            return new SelectList(leaveTypeList, "leave_type_id", "DisplayName", leaveTypeId);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveType_noDefault
        ****************************************************************************************************/
        public SelectList GetLeaveTypeNoDefault(int leaveTypeId)
        {
            var leaveTypeList = _context.tbl_leave_heading
                .OrderBy(f => f.abbr)
                .Select(f => new
                {
                    f.leave_type_id,
                    DisplayName = f.abbr + " - " + f.description
                }).ToList();
            return new SelectList(leaveTypeList, "leave_type_id", "DisplayName", leaveTypeId);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getYearlyHrsCF :Depricated
        ****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getYearlySickHrsCF | Depricated
        ****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getYearlyHrsLeave
        ****************************************************************************************************/
        public double GetYearlyLeaveHours(int leaveTypeId)
        {
            if (leaveTypeId < 1) { return 0; }
            var query = _context.tbl_leave_heading.Where(r => r.leave_type_id == leaveTypeId).Select(r => r.max_leave_hours).FirstOrDefault();
            return query ?? 0;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: 
        ****************************************************************************************************/
        public double GetMaxLeaveHours(int leaveTypeId, int empId, string fiscalYear)
        {
            if (leaveTypeId < 1 || empId < 1 || string.IsNullOrWhiteSpace(fiscalYear)) { return 0; }

            var query = _context.tbl_employee_leave_indv.Where(r => r.emp_id == empId && r.fiscal_year_to == fiscalYear);
            return leaveTypeId switch
            {
                LeaveTypeAnnual => query.Select(r => r.annual_leave ?? 0).FirstOrDefault(),
                LeaveTypeCasual => query.Select(r => r.casual_leave ?? 0).FirstOrDefault(),
                LeaveTypeSick => query.Select(r => r.sick_leave ?? 0).FirstOrDefault(),
                LeaveTypeOther => query.Select(r => r.other_leave ?? 0).FirstOrDefault(),
                LeaveTypeMaternity => query.Select(r => r.maternity ?? 0).FirstOrDefault(),
                LeaveTypePaternity => query.Select(r => r.paternity ?? 0).FirstOrDefault(),
                LeaveTypeMourning => query.Select(r => r.mourning ?? 0).FirstOrDefault(),
                LeaveTypeUnpaidStudy => query.Select(r => r.unpaid_study ?? 0).FirstOrDefault(),
                LeaveTypeAnnualCarryForward => query.Select(r => r.annual_leave_hours_carry_forward ?? 0).FirstOrDefault(),
                LeaveTypeSickCarryForward => query.Select(r => r.sick_leave_hours_carry_forward ?? 0).FirstOrDefault(),
                _ => 0
            };
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: 
        ****************************************************************************************************/
        public double GetLeaveTaken(int leaveTypeId, int empId, DateTime from, DateTime to)
        {
            if (leaveTypeId < 0 || empId < 1 || from > to) { return 0; }
            var leaveTypeIds = GetRelatedLeaveTypeIds(leaveTypeId);
            return leaveTypeIds.Length < 1 ? 0
                : _context.tbl_employee_leave
                .Where(l => l.emp_id == empId
                         && leaveTypeIds.Contains(l.leave_type_id ?? (byte)0)
                         && l.app_status == "approved"
                         && l.leave_from_date >= from
                         && l.leave_from_date <= to)
                .Sum(l => (double?)l.leave_in_hrs) ?? 0;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: 
        ****************************************************************************************************/
        public double GetLeavePending(int leaveTypeId, int empId, DateTime from, DateTime to)
        {
            if (leaveTypeId < 0 || empId < 1 || from > to) { return 0; }
            var leaveTypeIds = GetRelatedLeaveTypeIds(leaveTypeId);
            return leaveTypeIds.Length < 1 ? 0
                : _context.tbl_employee_leave
                .Where(l => l.emp_id == empId
                         && leaveTypeIds.Contains(l.leave_type_id ?? (byte)0)
                         && l.app_status == "pending"
                         && l.leave_from_date >= from
                         && l.leave_from_date <= to)
                .Sum(l => (double?)l.leave_in_hrs) ?? 0;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: isCurrentYearPendingLeave
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: isCurrentPeriodPendingLeave
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: chkLeaveTakenDay
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveTakenHours
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveSubmitList
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getLeaveDateStatus
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: getMaxLeaveHrsPaid
        ****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Jul-07
        * Contribution: Suraj/Dipesh
        ****************************************************************************************************/
        public double CalculateBalance(int leaveTypeId, int empId, string fiscalYear, DateTime startDate, DateTime endDate)
        {
            if (leaveTypeId < 0 || empId < 1 || string.IsNullOrWhiteSpace(fiscalYear) || startDate > endDate) { return 0; }
            double cur = GetMaxLeaveHours(leaveTypeId, empId, fiscalYear);
            double cfw = leaveTypeId switch
            {
                LeaveTypeAnnual => GetMaxLeaveHours(LeaveTypeAnnualCarryForward, empId, fiscalYear),
                LeaveTypeSick => GetMaxLeaveHours(LeaveTypeSickCarryForward, empId, fiscalYear),
                _ => 0
            };
            double tak = GetLeaveTaken(leaveTypeId, empId, startDate, endDate);
            double pen = GetLeavePending(leaveTypeId, empId, startDate, endDate);
            return cur + cfw - (tak + pen);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-07
        ****************************************************************************************************/
        public DateTime GetFirstLeavePaidEndDate(int empId, string fiscalYear, DateTime defaultStartDate, int period)
        {
            var dateUpto = _context.tbl_employee_leave_indv_paid_cleared_new
                .Where(lpc => lpc.emp_id == empId
                           && lpc.fiscal_year == fiscalYear
                           && lpc.submit_counter == period)
                .Select(lpc => lpc.date_upto)
                .FirstOrDefault();

            return dateUpto.HasValue ? dateUpto.Value.AddDays(1) : defaultStartDate;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-08
        * Contribution: 
        ****************************************************************************************************/
        public string AnyPendingLeaveOverallFY(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate) { return "Y"; }

            bool hasPendingLeave = _context.tbl_employee_leave
                .Any(l => l.app_status == "pending"
                         && l.leave_from_date >= startDate
                         && l.leave_from_date <= endDate
                 );

            return hasPendingLeave ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Jul-08
        * TRANSFER FUTUER LEAVE WHILE LEAVE CARRY FORWARD
        * Contribution: 
        ****************************************************************************************************/
        public void TransferFutureLeave(DateTime startDate, DateTime endDate, double workingHoursDay = 8)
        {
            if (startDate >= endDate)
            {
                return;
            }
            var records = (from lvf in _context.tbl_employee_leave_hash
                           join emp in _context.tbl_employee on lvf.emp_id equals emp.emp_id
                           where emp.emp_status == "A"
                           && lvf.app_status == "approved"
                           && lvf.leave_from_date >= startDate
                           && lvf.leave_to_date <= endDate
                           select lvf).ToList();

            foreach (var record in records)
            {
                double leave_in_hrs = record.leave_in_hrs ?? 0;

                /**CHECK IF APPLIED LEAVE FALLS IN HOLIDAY 
                    * Note: Only annual leave which is as per working days
                    *so no need to check if leave type is calendar year
                    */
                var smt = _context.tbl_setting_holidays.Where(hol =>
                hol.holiday_date >= record.leave_from_date &&
                hol.holiday_date <= record.leave_to_date).ToList();
                if (smt.Count > 0)
                {
                    leave_in_hrs -= smt.Count * workingHoursDay;
                }

                int emp_leave_id = _context.tbl_employee_leave.Select(o => o.emp_leave_id).DefaultIfEmpty(0).Max() + 1;
                var DataSave = new tbl_employee_leave
                {
                    emp_leave_id = emp_leave_id,
                    leave_type_id = record.leave_type_id,
                    submit_date = record.submit_date,
                    leave_from_date = record.leave_from_date,
                    leave_to_date = record.leave_to_date,
                    leave_desc = record.leave_desc,
                    app_status = record.app_status,
                    app_by = record.app_by,
                    app_date = record.app_date,
                    emp_id = record.emp_id,
                    leave_in_hrs = leave_in_hrs,
                    app_remarks = record.app_remarks
                };
                _ = _context.tbl_employee_leave.Add(DataSave);
            }
            _ = _context.SaveChanges();
        }
        /***************************************************************************************************
        * Since : 2026-Jul-08
        * TRANSFER CALCULATE PRORATED LEAVE FOR NEW EMPLOYEE
        * Contribution: 
        ****************************************************************************************************/
        public void CalculateNewEmployeeProrateLeave(double workingHoursDay, int empId, DateTime? joinDate, DateTime? endDate, string gender = "")
        {
            if (empId < 1 || string.IsNullOrWhiteSpace(joinDate.ToString()) || string.IsNullOrWhiteSpace(endDate.ToString()) || workingHoursDay <= 0) { return; }

            var httpContext = _httpContextAccessor.HttpContext;
            string fycy = httpContext?.Session.GetString("fiscal_year") ?? "";
            string strEndFiscalDate = httpContext?.Session.GetString("date_to") ?? "";

            var fnEndFiscalDate = DateTime.TryParse(strEndFiscalDate, out var D1) ? D1 : DateTime.MinValue;
            var fnEndDate = (endDate > fnEndFiscalDate) ? fnEndFiscalDate : endDate;

            int fnDateDiff = GblUtilities.GetDateDiffDays(joinDate, fnEndDate); //This come with +1
            double dateDiffHours = fnDateDiff * workingHoursDay;

            const double daysInYear = 365;
            double hrsInYear = daysInYear * workingHoursDay;

            double annualLeaveHours = GetYearlyLeaveHours(LeaveTypeAnnual);
            double casualLeaveHours = GetYearlyLeaveHours(LeaveTypeCasual);
            double sickLeaveHours = GetYearlyLeaveHours(LeaveTypeSick);
            double otherLeaveHours = GetYearlyLeaveHours(LeaveTypeOther);

            annualLeaveHours = annualLeaveHours / hrsInYear * dateDiffHours / workingHoursDay;
            casualLeaveHours = casualLeaveHours / hrsInYear * dateDiffHours / workingHoursDay;
            sickLeaveHours = sickLeaveHours / hrsInYear * dateDiffHours / workingHoursDay;
            otherLeaveHours = otherLeaveHours / hrsInYear * dateDiffHours / workingHoursDay;

            int indv_leave_id = _context.tbl_employee_leave_indv.Select(o => o.indv_leave_id).DefaultIfEmpty(0).Max() + 1;
            var DataSave = new tbl_employee_leave_indv
            {
                indv_leave_id = indv_leave_id,
                emp_id = empId,
                annual_leave = annualLeaveHours,
                casual_leave = casualLeaveHours,
                sick_leave = sickLeaveHours,
                annual_leave_hours_carry_forward = 0,
                maternity = 0,
                paternity = 0,
                mourning = 0,
                unpaid_study = 0,
                fiscal_year_to = fycy,
                other_leave = otherLeaveHours,
                sick_leave_hours_carry_forward = 0
            };
            _ = _context.tbl_employee_leave_indv.Add(DataSave);
            _ = _context.SaveChanges();
        }
        /***************************************************************************************************
        * Since : 2026-Jul-08
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingLeaveSetting(string fiscalYear)
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" &&
                              !_context.tbl_employee_leave_indv
                                  .Where(lvr => lvr.fiscal_year_to == fiscalYear)
                                  .Select(lvr => lvr.emp_id)   // only select emp_id here
                                  .Contains(emp.emp_id))
                .Select(emp => new EmployeeDropDownViewModel
                {
                    emp_id = emp.emp_id,
                    emp_name_code = string.Join(" ",
                        new[] { emp.firstname, emp.middlename, emp.lastname, "(" + emp.emp_code + ")" }
                        .Where(x => !string.IsNullOrEmpty(x)))
                })
                .ToList();
            /*
            var employees = _context.tbl_employee
                            .Where(emp => emp.emp_status == "A") 
                            .Select(emp => new EmployeeDropDownViewModel
                            {
                                emp_id = emp.emp_id,
                                emp_name_code = string.Join(" ",
                                    new[] { emp.firstname, emp.middlename, emp.lastname, "(" + emp.emp_code + ")" }
                                    .Where(x => !string.IsNullOrEmpty(x)))
                            })
                            .ToList();
            */
            return new SelectList(employees, "emp_id", "emp_name_code");

        }


        /***************************************************************************************************/
    }
}
