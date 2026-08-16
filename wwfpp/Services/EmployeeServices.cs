using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.Arm;
using wwfpp.Data;
using wwfpp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace wwfpp.Services
{
    public class EmployeeServices
    {
        private readonly AppDbContext _context;
        private static IHttpContextAccessor? _httpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public static HttpContext? CurrentHttpContext => _httpContextAccessor?.HttpContext;
        public EmployeeServices(AppDbContext context)
        {
            _context = context;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        ****************************************************************************************************/
        public string GetEmployeeName(int empId, string mode = "")
        {
            // Step 1: Query raw fields only
            var query = _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new
                {
                    e.emp_id,
                    e.emp_code,
                    e.firstname,
                    e.middlename,
                    e.lastname
                });

            // Step 2: Materialize
            var rawData = query.FirstOrDefault();

            string emp_name_code = "";
            if (rawData == null) { return emp_name_code; } // no employee found

            // Step 3: Build emp_name_code in memory
            if (mode == "NameOnly")
            {
                emp_name_code = string.Join(" ",
                    new[] { rawData.firstname?.Trim(), rawData.middlename?.Trim(), rawData.lastname?.Trim() }
                    .Where(x => !string.IsNullOrEmpty(x)));
            }
            else
            {
                emp_name_code = string.Join(" ",
                    new[] { rawData.firstname?.Trim(), rawData.middlename?.Trim(), rawData.lastname?.Trim() }
                    .Where(x => !string.IsNullOrEmpty(x))) + " (" + rawData.emp_code + ")";
            }
            return emp_name_code;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-30
        ****************************************************************************************************/
        public string GetEmployeeStatus(int empId, string StatusMode = "")
        {
            // Step 1: Query raw fields only
            var query = _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new
                {
                    e.emp_status,
                });

            // Step 2: Materialize
            var rawData = query.FirstOrDefault();

            string emp_status = "";
            if (rawData == null) { return emp_status; } // no employee found

            // Step 3: Build emp_status in memory
            if (StatusMode == "D")
            {
                emp_status = rawData.emp_status == "A" ? "Active" : "Inactive";

            }
            else
            {
                emp_status = rawData.emp_status ?? "";
            }
            return emp_status;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        ****************************************************************************************************/
        public string GetEmployeeNameEmail(int empId, string NameEmail = "")
        {
            string FnStr = "";
            var row = _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new { e.firstname, e.middlename, e.lastname, e.e_mail })
                .FirstOrDefault();
            string? firstname = "";
            string? middlename = "";
            string? lastname = "";
            string? email = "";
            string? fullname = "";
            if (row != null)
            {
                firstname = row.firstname;
                middlename = row.middlename;
                lastname = row.lastname;
                email = row.e_mail;
                if (!string.IsNullOrWhiteSpace(firstname)) { fullname = firstname; }
                if (!string.IsNullOrWhiteSpace(middlename)) { fullname = fullname + " " + middlename; }
                if (!string.IsNullOrWhiteSpace(lastname)) { fullname = fullname + " " + lastname; }
                FnStr = NameEmail == "N" ? fullname : NameEmail == "E" ? string.IsNullOrWhiteSpace(email) ? "" : email : fullname + "<" + email + ">";
            }
            return FnStr;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        * Return only active employees list
        ****************************************************************************************************/
        public SelectList GetEmployeeActiveOnly()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A")
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
        * Return only deactivated employees list
        ****************************************************************************************************/
        public SelectList GetEmployeePassiveOnly()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status != "A")
                        .OrderBy(emp => emp.firstname)   // optional if you want ordering
                        .ThenBy(emp => emp.middlename)
                        .ThenBy(emp => emp.lastname)
                        .Select(emp => new EmployeeDropDownViewModel
                        {
                            emp_id = emp.emp_id,
                            emp_name_code = string.Join(" ",
                            new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }
                            .Where(x => !string.IsNullOrEmpty(x))) + "[INACTIVE]"
                        })
                    .ToList();
            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Jul-10
        * param: employee status
        * Return Active or Passive employees list
        ****************************************************************************************************/
        public SelectList GetEmployeeList(string? empStatus)
        {
            var employees = _context.tbl_employee
            .Where(emp => emp.emp_status == empStatus)
            .OrderBy(emp => emp.firstname)
            .ThenBy(emp => emp.middlename)
            .ThenBy(emp => emp.lastname)
            .Select(emp => new EmployeeDropDownViewModel
            {
                emp_id = emp.emp_id,
                emp_name_code = string.Join(" ",
                new[] { emp.firstname, emp.middlename, emp.lastname, '(' + emp.emp_code + ')' }
                .Where(x => !string.IsNullOrEmpty(x)))
                + (empStatus != "A" ? " [INACTIVE]" : "")
            });
            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * Return active and then passive employees list
        ****************************************************************************************************/
        public SelectList GetEmployeeListBoth()
        {
            var query = _context.tbl_employee
                .OrderBy(emp => emp.emp_status)
                .ThenBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .AsQueryable();

            var employees = query
                .Select(emp => new EmployeeDropDownViewModel
                {
                    emp_id = emp.emp_id,
                    emp_name_code = string.Join(" ",
                        new[] { emp.firstname, emp.middlename, emp.lastname, "(" + emp.emp_code + ")" }
                        .Where(x => !string.IsNullOrEmpty(x)))
                        + (emp.emp_status != "A" ? " [INACTIVE]" : "")
                })
                .ToList();

            // Add separator if both lists requested
            var activeCount = employees.Count(e => !e.emp_name_code.Contains("[INACTIVE]"));
            if (activeCount > 0 && employees.Any(e => e.emp_name_code.Contains("[INACTIVE]")))
            {
                employees.Insert(activeCount, new EmployeeDropDownViewModel
                {
                    emp_id = 0,
                    emp_name_code = "-- [INACTIVE] Employee(s) --"
                });
            }

            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Jun-01
        * Return only active employees list
        ****************************************************************************************************/
        public SelectList GetManagerListActiveOnly(int filterMe)
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && emp.emp_id != filterMe)
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
        * Return active and then passive employees list
        ****************************************************************************************************/
        public SelectList GetManagerListBoth(int filterMe)
        {
            var query = _context.tbl_employee
                .Where(emp => emp.emp_id != filterMe)
                .OrderBy(emp => emp.emp_status)
                .ThenBy(emp => emp.firstname)
                .ThenBy(emp => emp.middlename)
                .ThenBy(emp => emp.lastname)
                .AsQueryable();

            var employees = query
                .Select(emp => new EmployeeDropDownViewModel
                {
                    emp_id = emp.emp_id,
                    emp_name_code = string.Join(" ",
                        new[] { emp.firstname, emp.middlename, emp.lastname, "(" + emp.emp_code + ")" }
                        .Where(x => !string.IsNullOrEmpty(x)))
                        + (emp.emp_status != "A" ? " [INACTIVE]" : "")
                })
                .ToList();

            // Add separator if both lists requested
            var activeCount = employees.Count(e => !e.emp_name_code.Contains("[INACTIVE]"));
            if (activeCount > 0 && employees.Any(e => e.emp_name_code.Contains("[INACTIVE]")))
            {
                employees.Insert(activeCount, new EmployeeDropDownViewModel
                {
                    emp_id = 0,
                    emp_name_code = "-- [INACTIVE] Employee(s) --"
                });
            }

            return new SelectList(employees, "emp_id", "emp_name_code");
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public static SelectList InsuranceType(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "Medical", "Medical" },
                { "Accidental", "Accidental" },
                { "Life", "Life" },
                { "Travel", "Travel" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public static SelectList GetEmployeeType(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "Core", "Core" },
                { "Project", "Project" },
                { "Contract", "Contract" },
                { "Intern", "Intern" },
                { "Volunteer", "Volunteer" },
                { "Others", "Others" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public SelectList GetDept(string selvalue = "")
        {
            var dept = _context.tbl_employee
            .Where(e => e.department != null)
            .Select(e => new
            {
                id = e.department,
                vl = e.department
            })
            .Distinct()
            .OrderBy(e => e.id)
            .ToList();
            return new SelectList(dept, "id", "vl", selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public static SelectList GetEmployeeTypeSub(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "Part Time", "Part Time" },
                { "Full Time", "Full Time" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public static SelectList GetJobFamily(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "Conservation Programs", "Conservation Programs" },
                { "Communication & Marketing", "Communication & Marketing" },
                { "Finance", "Finance" },
                { "Program Operations", "Program Operations" },
                { "Human Resources", "Human Resources" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public static SelectList GetCareerLevel(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "Senior Director", "Senior Director" },
                { "Director", "Director" },
                { "Deputy Director/Coordinator", "Deputy Director/Coordinator" },
                { "Senior Manager", "Senior Manager" },
                { "Senior Specialist", "Senior Specialist" },
                { "Senior Officer", "Senior Officer" },
                { "Manager", "Manager" },
                { "Specialist", "Specialist" },
                { "Officer", "Officer" },
                { "Specialized Program / Admin Support", "Specialized Program / Admin Support" },
                { "Associate", "Associate" },
                { "Program / Admin Support", "Program / Admin Support" },
                { "Assistant", "Assistant" },
                { "Operations Employee", "Operations Employee" },
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-22
        * Duty Station
        ****************************************************************************************************/
        public SelectList GetDutyStationList(string? selectedId = null)
        {
            //Note: fine and replace "getDutyStationDropDown" where it used  
            var stations = _context.tbl_duty_station
                .OrderBy(d => d.duty_station)
                .ToList();
            return new SelectList(stations, "id", "duty_station", selectedId);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-19
        ****************************************************************************************************/
        public SelectList GLTypeList(string selvalue)
        {
            var options = new Dictionary<string, string>
            {
                { "S", "Salary" },
                { "B", "Benefit" },
                { "J", "Advance" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-19
        ****************************************************************************************************/
        public SelectList StaffTypeList(string? selvalue = null)
        {
            var options = new Dictionary<string, string>
            {
                { "O", "Operation" },
                { "P", "Program" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-14
        ****************************************************************************************************/
        public SelectList GetContractSubject(int? selvalue = 0)
        {
            var subjects = _context.tbl_contract_document_template
                .Select(e => new
                {
                    Value = (int?)e.contract_document_id,   // force nullable int
                    Text = e.document_subject ?? ""
                })
                .OrderBy(e => e.Text)
                .ToList();

            return new SelectList(subjects, "Value", "Text", selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/
        public string GetContractExpiringStatus(DateTime issueDate, DateTime endDate)
        {
            int end_date_diff;
            int is_provision = 34;
            string trbgcolor = "";
            // peach High = "#ffc2a6" | light peach Medium = "#ffe8dd" | gray Low = "#eeeeee"
            if (!string.IsNullOrWhiteSpace(endDate.ToString()))
            {
                if (endDate < DateTime.Now)
                {
                    trbgcolor = "Expired";
                }
                else
                {
                    is_provision = (endDate - issueDate).Days + 1;
                    end_date_diff = (DateTime.Now - endDate).Days + 1;
                    if (is_provision < 33) //provision yes
                    {
                        trbgcolor = end_date_diff < 1 ? "High" : end_date_diff is > 0 and < 15 ? "Medium" : "Low";
                    }
                    else
                    {
                        trbgcolor = end_date_diff < 1 ? "High" : end_date_diff is > 0 and < 45 ? "Meduim" : "Low";
                    }
                }
            }
            return trbgcolor;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-23
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingSignature()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_employee_signature
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
        * Since : 2026-Jul-25
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingPhoto()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_employee_photo
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
        * Since : 2026-Jul-23
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingAddress()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_employee_address
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
        * Since : 2026-Jun-14
        * Replace it later
        ****************************************************************************************************/
        public static SelectList getEmployeeDropDown(AppDbContext _context, string Type = "A", string selvalue = "")
        {
            string placeholder = Type == "A" ? "-- Select Active Employee --" : "-- Select Inactive Employee --";

            var employees = _context.tbl_employee
                .Where(e => e.emp_status == Type)
                .Select(e => new
                {
                    Value = (int?)e.emp_id,   // force nullable int
                    Text = (e.firstname ?? "") + " " +
                           (e.middlename ?? "") + " " +
                           (e.lastname ?? "") + " (" + e.emp_code + ")"
                })
                .OrderBy(e => e.Text)
                .ToList();

            // Now you can insert a placeholder with null
            employees.Insert(0, new { Value = (int?)null, Text = placeholder });

            return new SelectList(employees, "Value", "Text", selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-25
        ****************************************************************************************************/
        public static SelectList EligibilityStatus(string selvalue = "")
        {
            var options = new Dictionary<string, string>
        {
            { "P", "Pending" },
            { "A", "Active" },
            { "I", "Inactive" }
        };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-26
        * 
        ****************************************************************************************************/
        public string IsDependentNeedReceipt(int emp_dep_id, DateTime dob, DateTime ageCheckDate, string fiscalYear)
        {
            string flag = "false";
            double dependentAge = Math.Round(((ageCheckDate - dob).Days + 1) / 365.0, 2);
            if (dependentAge is >= 18 and <= 25)
            {
                var qry = _context.tbl_employee_dependent_children_details_sub
                    .FirstOrDefault(d => d.fiscal_year == fiscalYear
                                      && d.status == "A"
                                      && d.emp_dep_id == emp_dep_id);
                if (qry == null) { flag = "true"; }
            }
            return flag;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-28
        * Check if immidiate supervisor and line director are defined or no
        ****************************************************************************************************/
        public string IsDefinedManager(int empId)
        {
            if (empId == 0) { return "false"; }

            var manager = _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new
                {
                    e.manager_id,
                    e.line_manager_id
                }).FirstOrDefault();
            if (manager != null)
            {
                int managerId = manager.manager_id ?? 0;
                int lineManagerId = manager.line_manager_id ?? 0;
                if (managerId > 0 || lineManagerId > 0)
                {
                    return "true";
                }
            }
            return "false";
        }
        /***************************************************************************************************
        * Since : 2026-Jul-28
        * Get Employee Active/Inactive status
        ****************************************************************************************************/
        public string GetEmployeeStatus(int empId)
        {
            if (empId == 0) { return "D"; }

            var status = _context.tbl_employee
                .Where(e => e.emp_id == empId)
                .Select(e => new
                {
                    e.emp_status
                }).FirstOrDefault();
            if (status != null)
            {
                string emp_status = status.emp_status ?? "D";
                return emp_status;
            }
            return "D";
        }
        /***************************************************************************************************
        * Since : 2026-Jul-23
        ****************************************************************************************************/
        public SelectList GetEmployeeNotHavingEducation()
        {
            var employees = _context.tbl_employee
                .Where(emp => emp.emp_status == "A" && !_context.tbl_employee_education
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
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public SelectList GetFundSourceActiveOnly(string selvalue = "")
        {
            var fund_source = _context.tbl_fund_source
                .Where(e => e.fund_status == "A")
                .Select(e => new
                {
                    Value = e.fund_id,   // force nullable int
                    Text = (e.fund_source ?? "")
                })
                .OrderByDescending(e => e.Value)
                .ToList();

            return new SelectList(fund_source, "Value", "Text", selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * Return active and then passive employees list
        ****************************************************************************************************/
        public SelectList GetFundSourceBoth(int filterMe)
        {
            var query = _context.tbl_fund_source
                .OrderBy(fnd => fnd.fund_status)
                .ThenBy(fnd => fnd.fund_source)
                .AsQueryable();

            var FundSource = query
                .Select(fnd => new FundSourceDropDownViewModel
                {
                    fund_id = fnd.fund_id,
                    fund_source = $"{fnd.fund_source} [" + fnd.fund_status != "A" ? " [INACTIVE]" : "" + "]"
                })
                .ToList();

            // Add separator if both lists requested
            var InActiveCount = FundSource.Count(e => !e.fund_source.Contains("[INACTIVE]"));
            if (InActiveCount > 0 && FundSource.Any(e => e.fund_source.Contains("[INACTIVE]")))
            {
                FundSource.Insert(InActiveCount, new FundSourceDropDownViewModel
                {
                    fund_id = 0,
                    fund_source = "-- [INACTIVE] Fund Source(s) --"
                });
            }

            return new SelectList(FundSource, "fund_id", "fund_source");
        }
        /***************************************************************************************************
        * Since : 2026-Aug-07
        * 
        ****************************************************************************************************/
        public string GetValidEmpCode(string parmEmpCode)
        {
            if (string.IsNullOrEmpty(parmEmpCode)) { return string.Empty; }
            string getFnID = parmEmpCode.Length < 6 ? parmEmpCode.PadLeft(6, '0') : parmEmpCode;
            return getFnID;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/

        /***************************************************************************************************
        * Since : 2026-Jul-11
        * 
        ****************************************************************************************************/

    }
}
