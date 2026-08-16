using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using wwfpp.Data;
using wwfpp.Models;
namespace wwfpp.Services
{
    public class UserServices
    {
        private readonly AppDbContext _context;
        public UserServices(AppDbContext context)
        {
            _context = context;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingUsername()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_user
                        .Select(u => u.emp_id)
                        .Distinct()
                        .Contains(emp.emp_id))
                        .OrderBy(emp => emp.firstname)   // optional if you want ordering
                        .ThenBy(emp => emp.middlename)
                        .ThenBy(emp => emp.lastname)
                        .Select(emp => new EmployeeDropDownViewModel
                        {
                            emp_id = emp.emp_id,
                            emp_name_code = string.Join(" ",
                            new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }
                            .Where(x => !string.IsNullOrEmpty(x)))
                        })
                    .ToList();
            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        ****************************************************************************************************/

    }
}
