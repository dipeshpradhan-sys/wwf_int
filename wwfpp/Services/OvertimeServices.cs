using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;

namespace wwfpp.Services
{
    public class OvertimeServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OvertimeServices(
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
        * Since : 2026-Aug-11
        * Overtime listing on dashboard : May be it is in dashboard services
        ****************************************************************************************************/
        //getOvertimeSubmitList
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Check if an employees is eligible for overtime 
        ****************************************************************************************************/
        public string IsOvertimeEmployee(int empId)
        {
            bool exists = _context.tbl_employee_overtime_settings
                .Any(o => o.emp_id == empId && o.is_get_overtime == "Y");

            return exists ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Check if an employees is supervisor for overtime 
        ****************************************************************************************************/
        public string IsOvertimeEmployeeSupervisor(int empId)
        {
            bool exists = _context.tbl_employee_overtime_settings
                    .Any(o => o.approval_person == empId);
            return exists ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Get Manager of employee for overtime 
        ****************************************************************************************************/
        public int GetOTManagerID(int empId)
        {
            return _context.tbl_employee_overtime_settings.FirstOrDefault(e => e.emp_id == empId)?.approval_person ?? 0;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * Is employee entitled for overtime hours in that day? 
        ****************************************************************************************************/
        public double DoEmpGetOverTimeHours(int empId, DateTime otDate)
        {
            var hours = _context.tbl_employee_overtime_request
                .Where(r => r.emp_id == empId
                            && r.ot_date == otDate
                            && r.app_status == "A")
                .Select(r => r.total_hours)
                .FirstOrDefault();
            return hours ?? 0;
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        * List of employees eligible for Overtime 
        ****************************************************************************************************/
        public SelectList GetOvertimeEmployee(int? selectedEmpId, string status)
        {
            // Get all employee IDs eligible for overtime
            var overtimeEmpIds = _context.tbl_employee_overtime_settings
                .Where(o => o.is_get_overtime == "Y")
                .Select(o => o.emp_id)
                .Distinct()
                .ToList();

            // Base query for employees who are in overtime settings
            IQueryable<tbl_employee> query = _context.tbl_employee.Where(e => overtimeEmpIds.Contains(e.emp_id));

            List<EmployeeDropDownViewModel> employees = new();

            if (status == "A")
            {
                employees = query.Where(e => e.emp_status == "A")
                    .OrderBy(e => e.firstname)
                    .ThenBy(e => e.middlename)
                    .ThenBy(e => e.lastname)
                    .Select(e => new EmployeeDropDownViewModel
                    {
                        emp_id = e.emp_id,
                        emp_name_code = $"{e.firstname} {e.middlename} {e.lastname} ({e.emp_code})"
                    })
                    .ToList();

                // Insert default option
                employees.Insert(0, new EmployeeDropDownViewModel { emp_id = 0, emp_name_code = "Select Active Employee" });
            }
            else if (status == "D")
            {
                employees = query.Where(e => e.emp_status == "D")
                    .OrderBy(e => e.firstname)
                    .ThenBy(e => e.middlename)
                    .ThenBy(e => e.lastname)
                    .Select(e => new EmployeeDropDownViewModel
                    {
                        emp_id = e.emp_id,
                        emp_name_code = $"[INACTIVE] {e.firstname} {e.middlename} {e.lastname} ({e.emp_code})"
                    })
                    .ToList();

                employees.Insert(0, new EmployeeDropDownViewModel { emp_id = 0, emp_name_code = "Select Inactive Employee" });
            }
            else
            {
                // Active employees first
                var activeEmployees = query.Where(e => e.emp_status == "A")
                    .OrderBy(e => e.firstname)
                    .ThenBy(e => e.middlename)
                    .ThenBy(e => e.lastname)
                    .Select(e => new EmployeeDropDownViewModel
                    {
                        emp_id = e.emp_id,
                        emp_name_code = $"{e.firstname} {e.middlename} {e.lastname} ({e.emp_code})"
                    })
                    .ToList();

                activeEmployees.Insert(0, new EmployeeDropDownViewModel { emp_id = 0, emp_name_code = "Select Active Employee" });

                // Inactive employees next
                var inactiveEmployees = query.Where(e => e.emp_status == "D")
                    .OrderBy(e => e.firstname)
                    .ThenBy(e => e.middlename)
                    .ThenBy(e => e.lastname)
                    .Select(e => new EmployeeDropDownViewModel
                    {
                        emp_id = e.emp_id,
                        emp_name_code = $"[INACTIVE] {e.firstname} {e.middlename} {e.lastname} ({e.emp_code})"
                    })
                    .ToList();

                inactiveEmployees.Insert(0, new EmployeeDropDownViewModel { emp_id = 0, emp_name_code = "Select Inactive Employee" });

                // Add a blank separator between active and inactive
                activeEmployees.Add(new EmployeeDropDownViewModel { emp_id = -1, emp_name_code = "" });

                employees = activeEmployees.Concat(inactiveEmployees).ToList();
            }

            return new SelectList(employees, "emp_id", "emp_name_code", selectedEmpId);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        /// <summary>
        /// Check if applied overtime hours exceed daily or weekly limits.
        /// Returns "ND" (Not enough daily hours), "NW" (Not enough weekly hours), or "Y" (Allowed).
        /// </summary>
        ****************************************************************************************************/
        public string CheckOvertimeSufficiency(int empId, DateTime otDate, double appliedHours, string dayType)
        {
            double overtimeDailyLimit = 0;
            double otDayNormalLimit = 0;
            double otDayHolWeekendLimit = 0;
            double weeklyLimit = 0;
            // Pull settings daily (normal + holiday/Weekend) overtime hours)
            var smt = _context.tbl_setting_limit_hrs.FirstOrDefault();
            if (smt != null)
            {
                otDayNormalLimit = smt.overtime_normal_working_hrs ?? 0;
                otDayHolWeekendLimit = smt.overtime_hol_wek_working_hrs ?? 0;
            }
            // Daily check
            //need to check if the day is normal working day or holiday/weekend
            //as the hours may different with respect to day type
            //GetNormalOrHolidayOrWeekendOnCalendar(startDate, endDate, SelDate)
            //will help to return dayType(N or W or H or empty)
            overtimeDailyLimit = dayType == "N" ? otDayNormalLimit : otDayHolWeekendLimit;

            double dayHours = GetApprovedHoursInDay(empId, otDate);
            if (dayHours + appliedHours > overtimeDailyLimit) { return "ND"; }

            // Weekly check
            double weekHours = GetApprovedHoursInWeek(empId, otDate);
            if (weekHours + appliedHours > weeklyLimit) { return "NW"; }

            return "Y";
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        /// <summary>
        /// Get total approved overtime hours for a given day.
        /// </summary>
        ****************************************************************************************************/
        public double GetApprovedHoursInDay(int empId, DateTime otDate)
        {
            return (double)_context.tbl_employee_overtime_request
                .Where(o => o.emp_id == empId && o.ot_date == otDate && o.app_status == "A")
                .Sum(o => o.total_hours ?? 0);
        }
        /***************************************************************************************************
        * Since : 2026-Aug-11
        /// <summary>
        /// Get total approved overtime hours for the week containing otDate.
        /// </summary>
        ****************************************************************************************************/
        public double GetApprovedHoursInWeek(int empId, DateTime otDate)
        {
            var weekStart = GetWeekStart(otDate);
            var weekEnd = GetWeekEnd(otDate);

            return (double)_context.tbl_employee_overtime_request
                .Where(o => o.emp_id == empId &&
                            o.ot_date >= weekStart &&
                            o.ot_date <= weekEnd &&
                            o.app_status == "A")
                .Sum(o => o.total_hours ?? 0);
        }
        /***************************************************************************************************
        /// <summary>
        /// Helper to get start of week (Thursday).
        /// </summary>
        ****************************************************************************************************/
        private DateTime GetWeekStart(DateTime date)
        {
            // Thursday is considered the start of the week
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Thursday)) % 7;
            return date.Date.AddDays(-diff);
        }
        /***************************************************************************************************
        /// <summary>
        /// Helper to get end of week (Wednesday).
        /// </summary>
        ****************************************************************************************************/
        private DateTime GetWeekEnd(DateTime date)
        {
            return GetWeekStart(date).AddDays(6);
        }


    }
}
