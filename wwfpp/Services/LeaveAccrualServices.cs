using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using wwfpp.Data;
using wwfpp.Models;
namespace wwfpp.Services
{
    public class LeaveAccrualServices
    {
        private readonly AppDbContext _context;

        public LeaveAccrualServices(AppDbContext context)
        {
            _context = context;
        }
        public decimal getProvisionedLeaveAmount(int empId, string fiscalYear, int counter)
        {
            var record = _context.tbl_employee_leave_accrual_new
                .Where(a => a.emp_id == empId && a.fiscal_year == fiscalYear && a.counter == counter)
                .OrderByDescending(a => a.counter)
                .FirstOrDefault();

            return record?.leave_payable ?? 0;
        }

        public decimal getYearlyHrsCF()
        {
            return _context.tbl_yearly_annual_leave_cf.Sum(x => (decimal?)x.hrs) ?? 0;
        }

        public double? getYearlyHrsLeave(string l_type)
        {
            int leaveTypeId = l_type switch
            {
                "an" => 1,
                "ca" => 3,
                "si" => 5,
                "ot" => 9,
                "ma" => 12,
                "pa" => 13,
                "mo" => 14,
                "un" => 15,
                _ => 0
            };

            return _context.tbl_leave_heading
                .Where(l => l.leave_type_id == leaveTypeId)
                .Select(l => l.max_leave_hours)
                .FirstOrDefault();
        }

        public decimal getYearlySickHrsCF()
        {
            return _context.tbl_yearly_sick_leave_cf.Sum(x => (decimal?)x.hrs) ?? 0;
        }

        public DateTime getFirstLeavePaidEndDate(int empId, string fiscalYear, DateTime defaultStartDate, int period)
        {
            var record = _context.tbl_employee_leave_indv_paid_cleared_new
                .Where(r => r.emp_id == empId && r.fiscal_year == fiscalYear && r.submit_counter == period)
                .Select(r => r.date_upto)
                .FirstOrDefault();

            return record.HasValue ? record.Value.AddDays(1) : defaultStartDate;
        }

        public decimal getMaxLeaveHrs(int fieldId, int empId, string fiscalYear)
        {
            string fieldName = fieldId switch
            {
                1 => "annual_leave",
                3 => "casual_leave",
                5 => "sick_leave",
                9 => "other_leave",
                12 => "maternity",
                13 => "paternity",
                14 => "mourning",
                15 => "unpaid_study",
                16 => "annual_leave_hours_carry_forward",
                17 => "sick_leave_hours_carry_forward",
                _ => null
            };

            if (fieldName == null) return 0;

            var record = _context.tbl_employee_leave_indv
                .Where(r => r.emp_id == empId && r.fiscal_year_to == fiscalYear)
                .Select(r => EF.Property<decimal?>(r, fieldName))
                .FirstOrDefault();

            return record ?? 0;
        }

        public decimal getLeaveTaken(int fieldId, int empId, DateTime sessionFrom, DateTime sessionTo)
        {
            var leaveTypeIds = fieldId switch
            {
                1 => new[] { 1, 2 },
                3 => new[] { 3, 4 },
                5 => new[] { 5, 6 },
                9 => new[] { 9, 10 },
                12 => new[] { 12 },
                13 => new[] { 13 },
                14 => new[] { 14 },
                15 => new[] { 15 },
                _ => Array.Empty<int>()
            };

            return _context.tbl_employee_leave
                .Where(l => l.app_status == "Approved" &&
                            l.emp_id == empId &&
                            leaveTypeIds.Contains(l.leave_type_id ?? (byte)0) &&
                            l.leave_from_date >= sessionFrom &&
                            l.leave_from_date <= sessionTo)
                .Sum(l => (decimal?)l.leave_in_hrs) ?? 0;
        }
    }
}