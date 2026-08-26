using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using wwfpp.Data;
using wwfpp.Models;
using System.Threading.Tasks;

namespace wwfpp.Services
{
    public class ApproverResolverService
    {
        private readonly AppDbContext _context;

        public ApproverResolverService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(int? toEmpId, int? toId)> ResolveApproverAsync(int empid)
        {
            int? toEmpId = null;
            int? toId = null;

            var managerId = await _context.tbl_employee
                .Where(e => e.emp_id == empid)
                .Select(e => e.manager_id)
                .FirstOrDefaultAsync();

            if (managerId != null)
            {
                var managerLeaveStatus = await GetEmployeeAbsentStatusAsync(managerId.Value);

                if (managerLeaveStatus == "N") // manager absent
                {
                    var altManagerId = await _context.tbl_employee
                        .Where(e => e.emp_id == empid)
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
        public async Task<(int? toEmpId, int? toId)> ResolveApproverLineManagerAsync(int empid)
        {
            int? toEmpId = null;
            int? toId = null;

            var managerId = await _context.tbl_employee
                .Where(e => e.emp_id == empid)
                .Select(e => e.line_manager_id)
                .FirstOrDefaultAsync();

            if (managerId != null)
            {
                var managerLeaveStatus = await GetEmployeeAbsentStatusAsync(managerId.Value);

                if (managerLeaveStatus == "N") // manager absent
                {
                    var altManagerId = await _context.tbl_employee
                        .Where(e => e.emp_id == empid)
                        .Select(e => e.alt_line_manager_id)
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


        public async Task<int> ResolveEmployeeIdInUserTblAsync(int empid)
        {
            int? toId = null;

 
            toId = await _context.tbl_user
                .Where(u => u.emp_id == empid)
                .Select(u => u.user_id)
                .FirstOrDefaultAsync();


            return (toId??0);
        }

        public async Task<string> GetEmployeeAbsentStatusAsync(int? empId, DateTime? curDate = null)
        {
            string empPresentStatus = "Y"; // Y = Not Absent, N = Absent
            DateTime today = curDate ?? DateTime.Today;

            if (empId == null || empId == 0)
            {
                return "N"; // No employee selected → absent
            }

            // Check leave
            bool onLeave = await _context.tbl_employee_leave
                .AnyAsync(l => l.emp_id == empId
                    && l.app_status == "Approved"
                    && today >= l.leave_from_date
                    && today <= l.leave_to_date);

            if (onLeave)
            {
                return "N";
            }

            // Check travel
            bool onTravel = await _context.tbl_employee_travel_main
                .AnyAsync(t => t.emp_id == empId
                    && t.app_status == "Approved"
                    && today >= t.date_from
                    && today <= t.date_to);

            if (onTravel)
            {
                return "N";
            }

            return empPresentStatus;
        }

    }
}
