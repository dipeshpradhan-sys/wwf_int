using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace wwf_pp.Services
{
    public class DashboardService
    {
        private readonly AppDbContext _context;
        private readonly EmployeeServices _employeeService;
        private readonly RequestServices _requestService;
        private readonly ApproverResolverService _approverResolver;
        public DashboardService(AppDbContext context, EmployeeServices employeeService, ApproverResolverService approverResolver, RequestServices requestService)
        {
            _context = context;
            _employeeService = employeeService;
            _requestService = requestService;
            _approverResolver = approverResolver;
        }
        private async Task<string> GetEmployeeAbsentStatusAsync(int empId)
        {
            var today = DateTime.Today;

            if (empId == 0)
            {
                return "N"; // absent if no employee id
            }

            // Check approved leave
            bool onLeave = await _context.tbl_employee_leave
                .AnyAsync(l => l.emp_id == empId &&
                               l.app_status == "Approved" &&
                               today >= l.leave_from_date &&
                               today <= l.leave_to_date);

            if (onLeave) return "N";

            // Check approved travel
            bool onTravel = await _context.tbl_employee_travel_main
                .AnyAsync(t => t.emp_id == empId &&
                               t.app_status == "Approved" &&
                               today >= t.date_from &&
                               today <= t.date_to);

            if (onTravel) return "N";

            return "Y"; // present
        }
        private async Task<(int? toEmpId, int? toId)> ResolveApproverAsync(int empId)
        {
            var managerId = await _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => e.manager_id)
                .FirstOrDefaultAsync();

            int? toEmpId = null;
            int? toId = null;

            if (managerId != null)
            {
                var managerLeaveStatus = await GetEmployeeAbsentStatusAsync(managerId.Value);

                if (managerLeaveStatus == "N") // manager absent
                {
                    var altManagerId = await _context.tbl_employee
                        .Where(e => e.emp_id == empId)
                        .Select(e => e.alt_manager_id)
                        .FirstOrDefaultAsync();

                    if (altManagerId != null)
                    {
                        var altManagerLeaveStatus = await GetEmployeeAbsentStatusAsync(altManagerId.Value);
                        toEmpId = altManagerLeaveStatus == "Y" ? altManagerId : managerId;
                    }
                    else
                    {
                        toEmpId = managerId;
                    }
                }
                else
                {
                    toEmpId = managerId;
                }

                if (toEmpId != null)
                {
                    toId = await _context.tbl_user
                        .Where(u => u.emp_id == toEmpId)
                        .Select(u => u.user_id)
                        .FirstOrDefaultAsync();
                }
            }

            return (toEmpId, toId);
        }
        public IEnumerable<TimesheetAppVM> GetSupervisorTimesheets(int? employeeId, string? fiscalYear)
        {
            var query = from ts in _context.tbl_employee_timesheet_app
                        join emp in _context.vw_Employee
                            on ts.app_by equals emp.emp_id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where ts.emp_id == employeeId
                              && ts.fiscal_year == fiscalYear
                              && ts.app_dec == "p"
                        select new TimesheetAppVM
                        {
                            AppId = ts.app_id.ToString(),
                            EmpId = ts.emp_id,
                            EmpMonth = ts.emp_month,
                            EmpYear = ts.emp_year,
                            AppBy = ts.app_by,
                            AppByName = emp != null ? emp.employeename : null,
                            SubmitDate = ts.submit_date
                        };

            return query.ToList();
        }
        public async Task<IEnumerable<TimesheetAppVM>> GetTimesheetsToMe(int? employeeId, string? fiscalYear)

        {
            // First resolve approver
            var (toEmpId, toId) = await ResolveApproverAsync(employeeId ?? 0);

            var query = from ts in _context.tbl_employee_timesheet_app
                        join emp in _context.vw_Employee
                            on ts.emp_id equals emp.emp_id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where ts.app_by == employeeId
                              && ts.fiscal_year == fiscalYear
                              && ts.app_dec == "p"
                        select new TimesheetAppVM
                        {
                            AppId = ts.app_id.ToString(),
                            EmpId = ts.emp_id,
                            EmpMonth = ts.emp_month,
                            EmpYear = ts.emp_year,
                            AppBy = ts.app_by,
                            AppByName = emp != null ? emp.employeenameWithCode : null,
                            SubmitDate = ts.submit_date,
                            ToEmpId = toEmpId,
                            ToId = toId,
                            Counter = ts.submit_counter

                        };

            return query.ToList();
        }

        public async Task<IEnumerable<LeaveDashboardGroup>> GetLeaveToMe(int supervisorId, DateTime dateFrom, DateTime dateTo)
        {
            var hrs = _requestService.GetLimitHoursSetting();
            int workingHrsDay = (int)hrs.overtime_normal_working_hrs;   // daily limit

            var leaves = await _context.tbl_employee_leave
                .Join(_context.tbl_leave_heading,
                      l => l.leave_type_id,
                      h => h.leave_type_id,
                      (l, h) => new { Leave = l, Heading = h })
                .Where(x => x.Leave.app_by == supervisorId
                         && x.Leave.leave_from_date >= dateFrom
                         && x.Leave.leave_to_date <= dateTo
                         && x.Leave.app_status == "Pending")
                .OrderBy(x => x.Leave.emp_id)
                .ThenByDescending(x => x.Leave.emp_leave_id)
                .ToListAsync();

            var groups = new List<LeaveDashboardGroup>();

            foreach (var g in leaves.GroupBy(x => x.Leave.emp_id))
            {
                var employeeName = _employeeService.GetEmployeeName(Convert.ToInt32(g.Key));

                var rows = new List<LeaveDashboardRow>();

                int counter = 0;
                foreach (var x in g)
                {
                    // 🔹 Resolve approver for this employee (not supervisorId)
                    var (toEmpId, toId) = await ResolveApproverAsync(Convert.ToInt32(x.Leave.emp_id));

                    rows.Add(new LeaveDashboardRow
                    {
                        LeaveType = x.Heading.description,
                        SubmitDate = x.Leave.submit_date?.ToString("MM/dd/yyyy"),
                        StartDate = x.Leave.leave_from_date?.ToString("MM/dd/yyyy"),
                        HoursDays = $"{x.Leave.leave_in_hrs} ({(x.Leave.leave_in_hrs / workingHrsDay):0.00})",
                        StatusClass = x.Leave.app_status == "Approved" ? "green" :
                                      x.Leave.app_status == "Declined" ? "lred" :
                                      x.Leave.app_status == "Cancelled" ? "org" : "",
                        ActionLinks = $@"
                    <a href='#' class='send-to-me-action'
                       data-emp-id='{x.Leave.emp_id}'
                       data-app-id='{x.Leave.emp_leave_id}'
                       data-st='a'
                       data-to-emp-id='{toEmpId}'
                       data-to-id='{toId}'
                       data-counter='0'
                       data-sent-to-me-type='leave'
                       >Approve</a> |
                    <a href='#' class='send-to-me-action'
                       data-emp-id='{x.Leave.emp_id}'
                       data-app-id='{x.Leave.emp_leave_id}'
                       data-st='d'
                       data-to-emp-id='{toEmpId}'
                       data-to-id='{toId}'
                       data-counter='0'
                       data-sent-to-me-type='leave'
                       >Decline</a>"
                    });
                }

                groups.Add(new LeaveDashboardGroup
                {
                    EmployeeName = employeeName,
                    Leaves = rows
                });
            }

            return groups;
        }

        public async Task<IEnumerable<LeaveAppVM>> GetSupervisorLeave(int? employeeId, DateTime? fiscalStartDate, DateTime? fiscalEndDate)
        {
            var hrs = _requestService.GetLimitHoursSetting();
            int workingHrsDay = (int)hrs.normal_working_hrs;   // daily limit

            var (toEmpId, toId) = await ResolveApproverAsync(employeeId ?? 0);
            var query = from ts in _context.tbl_employee_leave
                        join emp in _context.tbl_leave_heading
                            on ts.leave_type_id equals emp.leave_type_id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where ts.emp_id == employeeId
                              && ts.leave_from_date >= fiscalStartDate
                              && ts.leave_to_date <= fiscalEndDate
                              && ts.app_status == "Pending"
                        select new LeaveAppVM
                        {
                            AppId = ts.emp_leave_id,
                            EmpId = Convert.ToInt32(ts.emp_id),
                            LeaveType = emp.description,
                            SubmitDate = ts.submit_date,
                            StartDate = ts.leave_from_date,
                            LeaveInHours = $"{ts.leave_in_hrs} ({(ts.leave_in_hrs / workingHrsDay):0.00})",
                            ToEmpId = Convert.ToInt32(toEmpId),
                            ToId = Convert.ToInt32(toId)
                        };

            return query.ToList();
        }
        public async Task<IEnumerable<TravelDashboardGroup>> GetTravelToMe(int supervisorId, DateTime dateFrom, DateTime dateTo, string? parm_whos_list)
        {
            var query = _context.tbl_employee_travel_main
                .Where(t =>
                    (
                        (t.app_status == "Pending" && t.app_by == supervisorId &&
                         (t.i_app_status == "Approved" || t.i_app_status == null || t.i_app_status == ""))
                     || (t.i_app_status == "Pending" && t.i_app_by == supervisorId)
                    )
                    && t.date_from >= dateFrom
                );

            /*
            if (!string.IsNullOrEmpty(parm_whos_list))
            {
                query = query.Where(t => t.travel_type == parm_whos_list);
            }
            */

            var rows = await query
                .OrderBy(t => t.emp_id)
                .ThenByDescending(t => t.emp_travel_id)
                .ToListAsync();

            var groups = new List<TravelDashboardGroup>();

            foreach (var g in rows.GroupBy(t => t.emp_id))
            {
                var empName = _employeeService.GetEmployeeName((int)g.Key);

                var travelRows = new List<TravelDashboardVM>();

                foreach (var t in g)
                {
                    // Resolve approver’s user_id from tbl_user
                    var toId = await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(t.app_by));

                    var vm = new TravelDashboardVM
                    {
                        EmpId = (int)t.emp_id,
                        EmpTravelId = t.emp_travel_id,
                        TravelType = t.travel_type,
                        Destinations = t.destinations,
                        SubmitDate = t.submit_date ?? DateTime.MinValue,
                        DateFrom = t.date_from ?? DateTime.MinValue,
                        DateTo = t.date_to ?? DateTime.MinValue,
                        IAppStatus = t.i_app_status ?? "",
                        IAppBy = t.i_app_by,
                        AppStatus = t.app_status ?? "",
                        AppBy = t.app_by,
                        CanBy = t.can_by,

                        ToEmpId = (string.IsNullOrEmpty(t.i_app_status) || t.i_app_status == "Approved")
                                    ? t.app_by
                                    : (t.i_app_status == "Pending" ? t.i_app_by : null),

                        ActionLinks = $@"
                <a href='#' class='send-to-me-action'
                   data-emp-id='{t.emp_id}'
                   data-app-id='{t.emp_travel_id}'
                   data-st='{(string.IsNullOrEmpty(t.i_app_status) || t.i_app_status == "Approved" ? "a" : "rr")}'
                   data-to-emp-id='{t.app_by}'
                   data-to-id='{toId}'
                   data-counter='0'
                   data-sent-to-me-type='travel'
                   >{(string.IsNullOrEmpty(t.i_app_status) || t.i_app_status == "Approved" ? "Approve" : "Recommend")}</a> |
                <a href='#' class='send-to-me-action'
                   data-emp-id='{t.emp_id}'
                   data-app-id='{t.emp_travel_id}'
                   data-st='{(string.IsNullOrEmpty(t.i_app_status) || t.i_app_status == "Approved" ? "d" : "nr")}'
                   data-to-emp-id='{t.app_by}'
                   data-to-id='{toId}'
                   data-counter='0'
                   data-sent-to-me-type='travel'
                   >Decline</a>"
                    };

                    travelRows.Add(vm);
                }

                groups.Add(new TravelDashboardGroup
                {
                    EmployeeName = empName,
                    Travels = travelRows
                });
            }


            return groups;
        }

        public async Task<IEnumerable<TravelToSupervisorVM>> GetTravelToSupervisor(int employeeId,DateTime fiscalStartDate,DateTime fiscalEndDate,string? parm_whos_list)
        {
            IQueryable<tbl_employee_travel_main> query = _context.tbl_employee_travel_main;

            if (parm_whos_list == "MPT")
            {
                query = query.Where(t => t.app_status == "Pending"
                    && (t.i_app_status == null || t.i_app_status == "" || t.i_app_status == "Pending" || t.i_app_status == "Approved")
                    && t.emp_id == employeeId
                    && t.date_from >= fiscalStartDate);
            }
            else if (parm_whos_list == "TCS")
            {
                query = query.Where(t => t.app_status == "Approved"
                    && t.can_by != null
                    && t.emp_id == employeeId
                    && t.date_from >= fiscalStartDate);
            }
            else if (parm_whos_list == "RPT")
            {
                query = query.Where(t => t.emp_id == employeeId
                    && (t.i_app_status == "Declined"
                        || t.app_status == "Declined"
                        || t.app_status == "Approved"
                        || t.app_status == "Cancelled")
                    && t.can_by == null
                    && t.date_from >= fiscalStartDate)
                    .OrderByDescending(t => t.emp_travel_id)
                    .Take(5);
            }

            var list = await query.ToListAsync();
            var result = new List<TravelToSupervisorVM>();

            foreach (var t in list)
            {
                var vm = new TravelToSupervisorVM
                {
                    AppId = t.emp_travel_id,
                    EmpId = (int)t.emp_id,
                    TravelID = t.emp_travel_id,
                    TravelType = t.travel_type,
                    Destinations = t.destinations,
                    SubmitDate = t.submit_date,
                    StartDate = t.date_from,
                    EndDate = t.date_to,
                    Status = t.app_status
                };

                // 🔹 MPT logic
                if (parm_whos_list == "MPT")
                {
                    int? pendingById = null;

                    if (t.i_app_status == "Pending" && t.i_app_by.HasValue)
                        pendingById = t.i_app_by;
                    else if ((t.i_app_status == "Approved" || string.IsNullOrEmpty(t.i_app_status))
                             && t.app_status == "Pending" && t.app_by.HasValue)
                        pendingById = t.app_by;

                    if (pendingById.HasValue)
                    {
                        vm.ToEmpId = pendingById.Value;
                        vm.PendingBy = _employeeService.GetEmployeeName(pendingById.Value);

                        // 🔹 Lookup user_id from tbl_user where emp_id = pendingById
                        vm.ToId = await _context.tbl_user
                            .Where(u => u.emp_id == pendingById.Value)
                            .Select(u => u.user_id)
                            .FirstOrDefaultAsync();

                        vm.ReminderFor = "travel";
                    }
                }
                // 🔹 TCS logic
                else if (parm_whos_list == "TCS" && t.can_by.HasValue)
                {
                    vm.ToEmpId = t.can_by.Value;
                    vm.PendingBy = _employeeService.GetEmployeeName(t.can_by.Value);

                    vm.ToId = await _context.tbl_user
                        .Where(u => u.emp_id == t.can_by.Value)
                        .Select(u => u.user_id)
                        .FirstOrDefaultAsync();

                    vm.ReminderFor = "travel";
                }
                // 🔹 RPT logic
                else if (parm_whos_list == "RPT")
                {
                    vm.ToEmpId = employeeId; // fallback
                    if (t.app_status?.ToUpper() == "DECLINED" || t.i_app_status?.ToUpper() == "DECLINED")
                    {
                        vm.CssClass = "lred";
                        vm.ActionLink = "Declined";
                    }
                    else if (t.app_status?.ToUpper() == "APPROVED")
                    {
                        vm.CssClass = "green";
                        vm.ActionLink = "Approved";
                    }
                    else if (t.app_status?.ToUpper() == "CANCELLED")
                    {
                        vm.CssClass = "org";
                        vm.ActionLink = "Cancelled";
                    }
                }

                result.Add(vm);
            }

            return result;
        }

        public async Task<IEnumerable<OvertimeDashboardGroup>> GetOvertimeToMe(int supervisorId)
        {
            var query = _context.tbl_employee_overtime_request
                .Where(t =>
                    (
                        (t.app_status == "P" && t.app_by == supervisorId) ||
                        (t.req_status == "P" && t.requested_by == supervisorId)
                    )
                );

            var rows = await query
                .OrderBy(t => t.emp_id)
                .ThenByDescending(t => t.ot_req_id)
                .ToListAsync();

            var groups = new List<OvertimeDashboardGroup>();
            int getToId;

            foreach (var g in rows.GroupBy(t => t.emp_id))
            {
                var empName = _employeeService.GetEmployeeName(Convert.ToInt32(g.Key));

                var overtimeRows = new List<OvertimeDashboardVM>();

                foreach (var t in g)
                {
                    // Resolve approver’s user_id from tbl_user
                    if (t.app_by == t.requested_by)
                        getToId = Convert.ToInt32(t.app_by);
                    else
                        getToId = Convert.ToInt32(t.requested_by);

                    var toId = await _approverResolver.ResolveEmployeeIdInUserTblAsync(getToId);

                    var vm = new OvertimeDashboardVM
                    {
                        EmpId = Convert.ToInt32(t.emp_id),
                        OtReqId = t.ot_req_id,
                        OtDate = t.ot_date ?? DateTime.MinValue,
                        SubmitDate = t.submit_date ?? DateTime.MinValue,
                        TotalHours = (double)t.total_hours,
                        ToEmpId = getToId,

                        ActionLinks = $@"
                            <a href='#' class='send-to-me-action'
                               data-emp-id='{t.emp_id}'
                               data-app-id='{t.ot_req_id}'
                               data-st='{((t.app_by == t.requested_by || t.req_status=="R") ? "a" : "rr")}'
                               data-to-emp-id='{getToId}'
                               data-to-id='{toId}'
                               data-counter='0'
                               data-sent-to-me-type='overtime'
                               >{((t.app_by == t.requested_by || t.req_status == "R") ? "Approve" : "Recommend")}</a> |
                            <a href='#' class='send-to-me-action'
                               data-emp-id='{t.emp_id}'
                               data-app-id='{t.ot_req_id}'
                               data-st='{((t.app_by == t.requested_by || t.req_status== "R") ? "d" : "nr")}'
                               data-to-emp-id='{t.app_by}'
                               data-to-id='{toId}'
                               data-counter='0'
                               data-sent-to-me-type='overtime'
                               >Decline</a>"
                    };

                    overtimeRows.Add(vm);
                }

                groups.Add(new OvertimeDashboardGroup
                {
                    EmployeeName = empName,
                    Overtimes = overtimeRows
                });
            }


            return groups;
        }

        public async Task<IEnumerable<OvertimeDashboardVM>> GetSupervisorOvertime(int? employeeId)
        {
            var (toEmpId, toId) = await ResolveApproverAsync(employeeId ?? 0);
            var query = from ts in _context.tbl_employee_overtime_request
                        where ts.emp_id == employeeId
                              && ts.req_status != "D"
                              && ts.app_status == "P"
                        select new OvertimeDashboardVM
                        {
                            OtReqId = ts.ot_req_id,
                            EmpId = Convert.ToInt32(ts.emp_id),
                            SubmitDate = ts.submit_date ?? DateTime.MinValue,
                            OtDate = ts.ot_date ?? DateTime.MinValue,
                            TotalHours = (double)ts.total_hours,
                            ToEmpId = Convert.ToInt32(toEmpId)
                        };

            return query.ToList();
        }

        public async Task<IEnumerable<LeaveFutureDashboardGroup>> GetLeaveFutureToMe(int supervisorId, DateTime dateTo)
        {
            var hrs = _requestService.GetLimitHoursSetting();
            int workingHrsDay = (int)hrs.overtime_normal_working_hrs;   // daily limit

            var leaves = await _context.tbl_employee_leave_hash
                .Join(_context.tbl_leave_heading,
                      l => l.leave_type_id,
                      h => h.leave_type_id,
                      (l, h) => new { Leave = l, Heading = h })
                .Where(x => x.Leave.app_by == supervisorId
                         && x.Leave.leave_from_date > dateTo
                         && x.Leave.app_status == "Pending")
                .OrderBy(x => x.Leave.emp_id)
                .ThenByDescending(x => x.Leave.emp_leave_id)
                .ToListAsync();

            var groups = new List<LeaveFutureDashboardGroup>();

            foreach (var g in leaves.GroupBy(x => x.Leave.emp_id))
            {
                var employeeName = _employeeService.GetEmployeeName(Convert.ToInt32(g.Key));

                var rows = new List<LeaveFutureDashboardRow>();

                int counter = 0;
                foreach (var x in g)
                {
                    // 🔹 Resolve approver for this employee (not supervisorId)
                    var (toEmpId, toId) = await ResolveApproverAsync(Convert.ToInt32(x.Leave.emp_id));

                    rows.Add(new LeaveFutureDashboardRow
                    {
                        LeaveType = x.Heading.description,
                        FiscalYear = x.Leave.fiscal_year,
                        SubmitDate = x.Leave.submit_date?.ToString("MM/dd/yyyy"),
                        StartDate = x.Leave.leave_from_date?.ToString("MM/dd/yyyy"),
                        HoursDays = $"{x.Leave.leave_in_hrs} ({(x.Leave.leave_in_hrs / workingHrsDay):0.00})",
                        StatusClass = x.Leave.app_status == "Approved" ? "green" :
                                      x.Leave.app_status == "Declined" ? "lred" :
                                      x.Leave.app_status == "Cancelled" ? "org" : "",
                        ActionLinks = $@"
                    <a href='#' class='send-to-me-action'
                       data-emp-id='{x.Leave.emp_id}'
                       data-app-id='{x.Leave.emp_leave_id}'
                       data-st='a'
                       data-to-emp-id='{toEmpId}'
                       data-to-id='{toId}'
                       data-counter='0'
                       data-sent-to-me-type='leavefuture'
                       >Approve</a> |
                    <a href='#' class='send-to-me-action'
                       data-emp-id='{x.Leave.emp_id}'
                       data-app-id='{x.Leave.emp_leave_id}'
                       data-st='d'
                       data-to-emp-id='{toEmpId}'
                       data-to-id='{toId}'
                       data-counter='0'
                       data-sent-to-me-type='leavefuture'
                       >Decline</a>"
                    });
                }

                groups.Add(new LeaveFutureDashboardGroup
                {
                    EmployeeName = employeeName,
                    Leaves = rows
                });
            }

            return groups;
        }

        public async Task<IEnumerable<LeaveAppVM>> GetSupervisorFutureLeave(int? employeeId, DateTime? fiscalEndDate)
        {
            var hrs = _requestService.GetLimitHoursSetting();
            int workingHrsDay = Convert.ToInt32(hrs.normal_working_hrs);   // daily limit

            var (toEmpId, toId) = await ResolveApproverAsync(employeeId ?? 0);
            var query = from ts in _context.tbl_employee_leave_hash
                        join emp in _context.tbl_leave_heading
                            on ts.leave_type_id equals emp.leave_type_id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where ts.emp_id == employeeId
                              && ts.leave_from_date > fiscalEndDate
                              && ts.app_status == "Pending"
                        select new LeaveAppVM
                        {
                            AppId = ts.emp_leave_id,
                            EmpId = Convert.ToInt32(ts.emp_id),
                            LeaveType = emp.description,
                            SubmitDate = ts.submit_date,
                            StartDate = ts.leave_from_date,
                            LeaveInHours = $"{ts.leave_in_hrs} ({(ts.leave_in_hrs / workingHrsDay):0.00})",
                            ToEmpId = Convert.ToInt32(toEmpId),
                            ToId = Convert.ToInt32(toId),
                            FiscalYear = ts.fiscal_year
                        };

            return query.ToList();
        }
    }
}