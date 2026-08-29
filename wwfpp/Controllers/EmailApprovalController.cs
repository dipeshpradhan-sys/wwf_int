using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Services;

namespace wwfpp.Controllers
{
    public class EmailApprovalController : Controller
    {
        private AppDbContext context { get; }
        private readonly EmailService _emailSenderServices;
        private readonly AdministrationEmailService _administrationEmailService;
        private readonly EmployeeServices _employeeService;
        private readonly AppSettings _appSettings;
        private readonly TravelApprovalService _travelService;
        private readonly EmployeeOvertimeServices _employeeOvertimeServices;
        private readonly ApproverResolverService _approverResolver;

        public EmailApprovalController(AppDbContext context, EmailService emailSenderServices, AdministrationEmailService administrationEmailService, EmployeeServices employeeService, IOptions<AppSettings> appSettings, TravelApprovalService travelService ,EmployeeOvertimeServices employeeOvertimeServices, ApproverResolverService approverResolver)
        {
            this.context = context;
            _emailSenderServices = emailSenderServices;
            _administrationEmailService = administrationEmailService;
            _employeeService = employeeService;
            _appSettings = appSettings.Value;
            _travelService = travelService;
            _employeeOvertimeServices = employeeOvertimeServices;
            _approverResolver = approverResolver;
        }
        public IActionResult EmailApproval(ApprovalFromEmailVM model, int empId, string appId, int month, int year, int toID, int toEmpID, string st, int counter, string approveFor)

        {
            model.EmpID = empId;
            model.AppID = appId;
            model.Month = month;
            model.Year = year;
            model.ToID = toID;
            model.ToEmpID = toEmpID;
            model.St = st;
            model.Counter = counter;
            model.ApproveFor = approveFor;
            ViewData["ApprovalMode"] = true;
            return PartialView("EmailApproval/_EmailApproval", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailApprovalSave(ApprovalFromEmailVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid" });
            }

            // 🔹 Switch on ApproveFor
            switch (model.ApproveFor?.ToLower())
            {
                case "timesheet":
                    return await HandleTimesheetApproval(model);

                case "leave":
                    return await HandleLeaveApproval(model);

                case "travel":
                    return await HandleTravelApproval(model);
                case "overtime":
                    return await HandleOvertimeApproval(model);

                case "leavefuture":
                    return await HandleLeaveFutureApproval(model);

                default:
                    return Json(new { status = "unsupported" });
            }
        }

        // =============================
        // Timesheet handler
        // =============================
        private async Task<IActionResult> HandleTimesheetApproval(ApprovalFromEmailVM model)
        {
            // First check if the app_id exists
            bool appExists = context.tbl_employee_timesheet_app
                .Any(r => r.app_id == model.AppID);

            if (!appExists)
            {
                return Json(new { status = "notfound" });
            }

            string? FiscalYear = HttpContext.Session.GetString("fiscal_year");
            var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
            int acaAdminID = Convert.ToInt32(Adminemails["aca"].Id);
            string str_to_admin = Adminemails["aca"].Email;
            string subject = "";
            var str_to_employee = await context.tbl_employee
                .Where(e => e.emp_id == model.EmpID)
                .Select(e => e.e_mail)
                .FirstOrDefaultAsync();

            // ✅ Approval
            if ((model.EmpID != null || model.EmpID > 0) && model.St == "a")
            {
                int AlreadyApproved = context.tbl_employee_timesheet_app
                    .Count(r => r.emp_id == model.EmpID
                        && r.emp_year == model.Year
                        && r.emp_month == model.Month
                        && r.app_dec == "a"
                        && r.submit_counter == model.Counter);

                if (AlreadyApproved > 0)
                    return Json(new { status = "alreadyapproved", message = "Timesheet has already been approved." });

                var existing = context.tbl_employee_timesheet_app
                    .FirstOrDefault(u => u.app_id == model.AppID.ToString());
                if (existing != null)
                {
                    existing.app_dec = "a";
                    existing.app_date = DateTime.Now;
                    existing.app_remarks = model.Description;
                    context.Update(existing);
                    context.SaveChanges();
                }

                // deactivate previous submissions
                var existingPreviousTSubs = context.tbl_employee_timesheet_sub
                    .Where(u => u.emp_id == model.EmpID
                             && u.emp_year == model.Year
                             && u.emp_month == model.Month
                             && u.submit_counter < model.Counter)
                    .ToList();

                foreach (var sub in existingPreviousTSubs)
                {
                    sub.is_active = "I";
                }

                if (existingPreviousTSubs.Any())
                {
                    context.UpdateRange(existingPreviousTSubs);
                    context.SaveChanges();
                }

                var existingPreviousApproved = context.tbl_employee_timesheet_app
                    .Where(u => u.emp_id == model.EmpID && u.emp_year == model.Year &&
                                         u.emp_month == model.Month && u.submit_counter < model.Counter)
                .ToList();
                foreach (var subapp in existingPreviousApproved)
                {
                    subapp.app_dec = "i";
                }

                if (existingPreviousApproved.Any())
                {
                    context.UpdateRange(existingPreviousTSubs);
                    context.SaveChanges();
                }

                context.tbl_employee_timesheet_sub
                    .Where(u => u.emp_id == model.EmpID && u.emp_year == model.Year &&
                                u.emp_month == model.Month && u.submit_counter == model.Counter)
                    .ExecuteUpdate(s => s.SetProperty(u => u.is_active, "A"));

                var toAcManager = new tbl_employee_timesheet_edited
                {
                    emp_id = model.EmpID,
                    emp_year = model.Year,
                    emp_month = model.Month,
                    fiscal_year = FiscalYear,
                    emp_week = 0,
                    submit_counter = Convert.ToInt32(model.Counter),
                    view_status = "N",
                    account_emp_id = acaAdminID,
                    updated_date = DateTime.Now,
                };
                context.tbl_employee_timesheet_edited.Add(toAcManager);
                context.SaveChanges();

                string EmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
                string monthName = new DateTime(Convert.ToInt32(model.Year), Convert.ToInt32(model.Month), 1).ToString("MMMM");
                string addOptText = model.Counter > 1 ? "Re-submitted " : "";
                subject = $"{addOptText}Timesheet of {EmployeeName} has been approved.";
                string bodyAdmin = $"Dear Administrator,<br/><br/>{addOptText}Timesheet of employee {EmployeeName} of {monthName} {model.Year} has been approved.<br/><br/>Remarks<br/>{model.Description}<br/><br/>";
                string bodyEmployee = $"Dear {EmployeeName},<br/><br/>Your {addOptText}Timesheet of {monthName} {model.Year} has been approved.<br/><br/>Remarks<br/>{model.Description}<br/><br/>";

                _emailSenderServices.SendEmail(null, str_to_admin, subject, bodyAdmin, null, "", null, null, null);
                _emailSenderServices.SendEmail(null, str_to_employee, subject, bodyEmployee, null, "", null, null, null);


            }

            // ✅ Decline
            if ((model.EmpID != null || model.EmpID > 0) && model.St == "d")
            {
                var existing = context.tbl_employee_timesheet_app.FirstOrDefault(u => u.app_id == model.AppID);
                if (existing != null)
                {
                    existing.app_dec = "d";
                    existing.app_date = DateTime.Now;
                    existing.app_remarks = model.Description;
                    context.Update(existing);
                    context.SaveChanges();
                }

                string EmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
                string acaAdminName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                string monthName = new DateTime(Convert.ToInt32(model.Year), Convert.ToInt32(model.Month), 1).ToString("MMMM");
                string addOptText = model.Counter > 1 ? "Re-submitted " : "";
                subject = $"{addOptText}Timesheet of {EmployeeName} of {monthName} {model.Year} has been declined.";
                string bodyEmployee = $"Dear {EmployeeName},<br/><br/>Your {addOptText}Timesheet of {monthName} {model.Year} has been declined.<br/><br/>Remarks<br/>{model.Description}<br/><br/>Please make necessary correction and send it again.<br/><br/>Regards,<br/>{acaAdminName}<br/><br/>";
                _emailSenderServices.SendEmail(null, str_to_employee, subject, bodyEmployee, null, "", null, null, null);

                
            }
            string status = model.St == "a" ? "approved" :
                model.St == "d" ? "declined" : "error";
            return Json(new { status, message = subject, approveFor = model.ApproveFor });

        }

        // =============================
        // Stubs for other approval types
        // =============================
        private async Task<IActionResult> HandleLeaveApproval(ApprovalFromEmailVM model)
        {
            // ✅ Update leave record
            var leave = await context.tbl_employee_leave
                .FirstOrDefaultAsync(l => l.emp_leave_id == Convert.ToInt32(model.AppID));

            if (leave == null)
            {
                return Json(new { status = "notfound", message = "Leave record not found." });
            }
            int AlreadyApproved = context.tbl_employee_leave
                .Count(r => r.emp_leave_id == Convert.ToInt32(model.AppID) && r.app_status == "Approved");

            if (AlreadyApproved > 0)
                return Json(new { status = "alreadyapproved", message = "Leave has already been approved." });

            leave.app_status = model.St == "a" ? "Approved" : "Declined";
            leave.app_by = model.ToEmpID;
            leave.app_date = DateTime.Now;
            leave.app_remarks = model.Description;
            context.Update(leave);
            await context.SaveChangesAsync();

            // ✅ Fetch leave details for email
            var leaveDetails = await context.tbl_employee_leave
                .Where(l => l.emp_leave_id == Convert.ToInt32(model.AppID))
                .Select(l => new
                {
                    l.leave_type_id,
                    l.submit_date,
                    l.leave_from_date,
                    l.leave_to_date,
                    l.leave_in_hrs,
                    l.leave_desc,
                    l.app_remarks
                })
                .FirstOrDefaultAsync();

            if (leaveDetails == null)
                return Json(new { status = "notfound", message = "Leave record not found." });

            var leaveTypeName = await context.tbl_leave_heading
                 .Where(e => e.leave_type_id == leaveDetails.leave_type_id)
                 .Select(e => e.description)
                 .FirstOrDefaultAsync();
            string remarks = string.IsNullOrEmpty(leaveDetails.app_remarks) ? "N/A" : leaveDetails.app_remarks;
            string EmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
            string EmployeeEmail = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
            string approverName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
            string subject = "";
            string body = "";
            string detailMessage = $@"
                    <b>Leave Type:</b> {leaveTypeName}<br/>
                    <b>Submit Date:</b> {leaveDetails.submit_date:dd-MMM-yyyy}<br/>
                    <b>Leave From Date:</b> {leaveDetails.leave_from_date:dd-MMM-yyyy}<br/>
                    <b>Leave To Date:</b> {leaveDetails.leave_to_date:dd-MMM-yyyy}<br/>
                    <b>Leave Hours:</b> {leaveDetails.leave_in_hrs}<br/>
                    <b>Description:</b> {leaveDetails.leave_desc}<br/>
                    <b>Remarks:</b> {remarks}<br/>";
            // ✅ Update timesheet if approved
            if (model.St == "a")
            {

                // Ensure both are non-nullable DateTime
                DateTime fromDate = leaveDetails.leave_from_date.Value;
                DateTime toDate = leaveDetails.leave_to_date.Value;

                int loopCnt = (int)(toDate.Date - fromDate.Date).TotalDays + 1;
                for (int i = 0; i < loopCnt; i++)
                {
                    DateTime leaveDate = fromDate.AddDays(i);
                    int empDay = leaveDate.Day;
                    int empMonth = leaveDate.Month;
                    int empYear = leaveDate.Year;

                    // Check if timesheet sub record exists
                    var tsSub = await context.tbl_employee_timesheet_sub
                        .Where(t => t.emp_id == model.EmpID &&
                                    t.emp_day == empDay &&
                                    t.emp_month == empMonth &&
                                    t.emp_year == empYear &&
                                    t.time_hours > 0)
                        .OrderByDescending(t => t.submit_counter)
                        .FirstOrDefaultAsync();

                    if (tsSub != null)
                    {
                        // Check holiday
                        bool isHoliday = await context.tbl_setting_holidays
                            .AnyAsync(h => h.holiday_date == leaveDate);

                        if (!isHoliday)
                        {
                            int submitCounter = tsSub.submit_counter ?? 0;
                            DateTime submitDateTs = tsSub.submit_date ?? DateTime.Now;

                            string fiscalYear = "";
                            int empWeek = 0;

                            // ✅ Insert directly into table
                            var timesheetMain = new tbl_employee_timesheet_main
                            {
                                emp_id = model.EmpID,
                                emp_year = (byte)empYear,
                                emp_month = (byte)empMonth,
                                emp_day = (byte)empDay,
                                leave_type_id = leaveDetails.leave_type_id,
                                submit_date = submitDateTs,
                                submit_counter = submitCounter,
                                fiscal_year = fiscalYear,
                                emp_week = (byte)empWeek
                            };
                        }
                    }
                }



                subject = $"Leave of {EmployeeName} has been approved.";
                body = $"Dear {EmployeeName},<br/><br/>" +
                       $"Your leave has been approved.<br/><br/>{detailMessage}<br/>" +
                       $"Regards,<br/>{approverName}";

            }
            if (model.St == "d")
            {
                subject = $"Leave of {EmployeeName} has been declined.";
                body = $"Dear {EmployeeName},<br/><br/>" +
                       $"Your leave has been declined.<br/><br/>{detailMessage}<br/>" +
                       $"Regards,<br/>{approverName}";
            }

            // ✅ Send email notification

            if (!string.IsNullOrEmpty(EmployeeEmail))
            {
                _emailSenderServices.SendEmail(null, EmployeeEmail, subject, body, null, "", null, null, null);
            }
            if (model.St == "a")
            {   // Send Email to HR
                var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
                int hr_ID = Convert.ToInt32(Adminemails["hra"].Id);
                string hr_Name = _employeeService.GetEmployeeName(Convert.ToInt32(hr_ID));
                string hr_Email = Adminemails["hra"].Email;
                string hr_Subject = $"Leave of {EmployeeName} has been approved.";
                string hr_boday = $"Dear {hr_Name},<br/><br/>Leave of {EmployeeName} has been approved.<br/><br/>{detailMessage}<br/><br/>Regards,<br/>{approverName}<br/><br/>";
            }
            return Json(new { status = leave.app_status.ToLower(), message = subject , approveFor = model.ApproveFor });
        }


        private async Task<IActionResult> HandleTravelApproval(ApprovalFromEmailVM model)
        {
            var travel = await context.tbl_employee_travel_main
                .FirstOrDefaultAsync(t => t.emp_travel_id == Convert.ToInt32(model.AppID));
            if (travel == null)
            {
                return Json(new { status = "notfound" });
            }
            string str_cc = "";
            string subject = "";
            string body = "";
            string EmailTo = "";
            string EmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
            string eml_app_link_outside = "";
            string eml_dec_link_outside = "";
            if (model.St == "rr") // Recommend
            {
                if (travel.i_app_status == "Approved")
                {
                    return Json(new { status = "alreadyrecommended", message = "Travel has already been recommended." });
                }
                // Update Table
                travel.i_app_status = "Approved";
                //travel.i_app_by = toemp_id;
                travel.i_app_date = DateTime.Now;   // equivalent to Date()
                travel.rec_remarks = model.Description;
                await context.SaveChangesAsync();

                // Email Contents
                EmailTo = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.ToEmpID));
                subject = $"Travel of {EmployeeName} has been recommended.";
                eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.EmpID}&app_id={model.AppID}&toid={model.ToID}&toemp_id={model.ToEmpID}&st=a&approval_from=email&approve_for=travel'>Approve</a> | ";
                eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={model.EmpID}&app_id={model.AppID}&toid={model.ToID}&toemp_id={model.ToEmpID}&st=d&approval_from=email&approve_for=travel'>Decline</a> | ";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                body = $"Dear Sir/Madam,<br/><br/>Please find my <u>recommended</u> travel request below.<br/><br/>{EmailContent}<br/><br/>Please click Approve or Decline link provided below as appropriate.<br/><br/>{eml_app_link_outside} {eml_dec_link_outside}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>";
            }

            else if (model.St == "nr") // Recommendation declined
            {
                if (travel.i_app_status == "Declined")
                {
                    return Json(new { status = "alreadyrecommendationdeclined", message = "Travel recommendation has already been declined." });
                }
                // Update Table
                travel.i_app_status = "Declined";
                //travel.i_app_by = toemp_id;
                travel.i_app_date = DateTime.Now;   // equivalent to Date()
                travel.rec_remarks = model.Description;
                await context.SaveChangesAsync();

                EmailTo = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
                string EmailFromName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                subject = $"Travel of {EmployeeName} has been declined.";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                body = $"Dear {EmployeeName},<br/><br/>Your following travel request has been declined. Please make necessary correction and send it again.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{EmailFromName}<br/><br/>";
            }

            else if (model.St == "a") // Approve
            {
                if (travel.app_status == "Approved")
                {
                    return Json(new { status = "alreadyapproved", message = "Travel  has already been approved." });
                }

                // Update Table
                travel.app_status = "Approved";
                //travel.i_app_by = toemp_id;
                travel.app_date = DateTime.Now;   // equivalent to Date()
                travel.rec_remarks = model.Description;
                await context.SaveChangesAsync();

                var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
                int? admin_email_ac_id = Adminemails["aca"].Id;
                string str_admin_email_ac = Adminemails["aca"].Email;

                int? lng_tav1_emp_id = Adminemails["t_t_a_1"].Id;
                string str_email_tav1 = Adminemails["t_t_a_1"].Email;

                int? lng_tav2_emp_id = Adminemails["t_t_a_2"].Id;
                string str_email_tav2 = Adminemails["t_t_a_2"].Email;

                int? lng_tav3_emp_id = Adminemails["t_t_a_3"].Id;
                string str_email_tav3 = Adminemails["t_t_a_3"].Email;

                int? lng_tav4_emp_id = Adminemails["t_t_a_4"].Id;
                string str_email_tav4 = Adminemails["t_t_a_4"].Email;

                int? lng_tav5_emp_id = Adminemails["t_t_a_5"].Id;
                string str_email_tav5 = Adminemails["t_t_a_5"].Email;

                var ccList = new List<string>();
                if (model.EmpID != admin_email_ac_id)
                {
                    ccList.Add(str_admin_email_ac);
                }
                // Add emails if IDs are still non-zero
                if (lng_tav1_emp_id > 0 && lng_tav1_emp_id != model.EmpID) ccList.Add(str_email_tav1);
                if (lng_tav2_emp_id > 0 && lng_tav2_emp_id != model.EmpID) ccList.Add(str_email_tav2);
                if (lng_tav3_emp_id > 0 && lng_tav3_emp_id != model.EmpID) ccList.Add(str_email_tav3);
                if (lng_tav4_emp_id > 0 && lng_tav4_emp_id != model.EmpID) ccList.Add(str_email_tav4);
                if (lng_tav5_emp_id > 0 && lng_tav5_emp_id != model.EmpID) ccList.Add(str_email_tav5);
                // Only join if there are items
                str_cc = ccList.Any() ? string.Join(";", ccList) : string.Empty;

                EmailTo = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
                subject = $"Travel of {EmployeeName} has been approved.";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                string ToEmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                body = $"Dear {EmployeeName},<br/><br/>Your following travel request has been approved.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{ToEmployeeName}<br/><br/>";
            }
            else if (model.St == "d") // Decline
            {
                if (travel.app_status == "Declined")
                {
                    return Json(new { status = "alreadydeclined", message = "Travel  has already been declined." });
                }
                // Update Table
                travel.app_status = "Declined";
                //travel.i_app_by = toemp_id;
                travel.app_date = DateTime.Now;   // equivalent to Date()
                travel.rec_remarks = model.Description;
                await context.SaveChangesAsync();

                EmailTo = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
                subject = $"Travel of {EmployeeName} has been declined.";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                string ToEmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                body = $"Dear {EmployeeName},<br/><br/>Your following travel request has been declined.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{ToEmployeeName}<br/><br/>";
            }
            else if (model.St == "ca") // Travel Cancel Approved
            {

                if (travel.app_status == "Cancelled")
                {
                    return Json(new { status = "alreadycancelled", message = "Travel  has already been cancelled." });
                }
                // Update Table
                travel.app_status = "Cancelled";
                travel.can_by = model.ToEmpID;
                travel.can_date = DateTime.Now;   // equivalent to Date()
                travel.can_remarks = model.Description;
                await context.SaveChangesAsync();

                var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
                int? admin_email_ac_id = Adminemails["aca"].Id;
                string str_admin_email_ac = Adminemails["aca"].Email;

                int? lng_tav1_emp_id = Adminemails["t_t_a_1"].Id;
                string str_email_tav1 = Adminemails["t_t_a_1"].Email;

                int? lng_tav2_emp_id = Adminemails["t_t_a_2"].Id;
                string str_email_tav2 = Adminemails["t_t_a_2"].Email;

                int? lng_tav3_emp_id = Adminemails["t_t_a_3"].Id;
                string str_email_tav3 = Adminemails["t_t_a_3"].Email;

                int? lng_tav4_emp_id = Adminemails["t_t_a_4"].Id;
                string str_email_tav4 = Adminemails["t_t_a_4"].Email;

                int? lng_tav5_emp_id = Adminemails["t_t_a_5"].Id;
                string str_email_tav5 = Adminemails["t_t_a_5"].Email;

                var ccList = new List<string>();
                if (model.EmpID != admin_email_ac_id)
                {
                    ccList.Add(str_admin_email_ac);
                }
                // Add emails if IDs are still non-zero
                if (lng_tav1_emp_id > 0 && lng_tav1_emp_id != model.EmpID) ccList.Add(str_email_tav1);
                if (lng_tav2_emp_id > 0 && lng_tav2_emp_id != model.EmpID) ccList.Add(str_email_tav2);
                if (lng_tav3_emp_id > 0 && lng_tav3_emp_id != model.EmpID) ccList.Add(str_email_tav3);
                if (lng_tav4_emp_id > 0 && lng_tav4_emp_id != model.EmpID) ccList.Add(str_email_tav4);
                if (lng_tav5_emp_id > 0 && lng_tav5_emp_id != model.EmpID) ccList.Add(str_email_tav5);
                // Only join if there are items
                str_cc = ccList.Any() ? string.Join(";", ccList) : string.Empty;

                EmailTo =  _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
                string ToEmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                subject = $"Travel cancellation request of {EmployeeName} has been approved.";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                body = $"Dear {EmployeeName},<br/><br/>Your following travel cancellation request has been approved.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{ToEmployeeName}<br/><br/>";
            }
            else if (model.St == "cd") // Travel Cancel Declined
            {
                // Update Table
                if (travel.app_status == "Declined")
                {
                    return Json(new { status = "alreadycancelled", message = "Travel  has already been cancelled." });
                }
                travel.app_status = "Declined";
                travel.can_by = model.ToEmpID;
                travel.can_date = DateTime.Now;   // equivalent to Date()
                travel.can_remarks = model.Description;
                await context.SaveChangesAsync();

                EmailTo = _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));
                string ToEmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
                subject = $"Travel cancellation request of {EmployeeName} has been declined.";
                var EmailContent = await _travelService.GetTravelEmailHtmlContent(Convert.ToInt32(model.AppID));
                body = $"Dear {EmployeeName},<br/><br/>Your following travel cancellation request has been declined.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{ToEmployeeName}<br/><br/>";
            }
            if (EmailTo != null || EmailTo != "")
                _emailSenderServices.SendEmail(null, EmailTo, subject, body, null, str_cc, null, null, null);

            return Json(new { status = "approved", message = subject, approveFor = model.ApproveFor });
        }




        public async Task<IActionResult> SendEmailReminder(int empID, string appID, int toID, int toEmpID, string reminderFor,string reminderForPerformance = null)
        {
            // 🔹 Switch on ApproveFor
            switch (reminderFor?.ToLower())
            {
                //case "timesheet":
                  //  return await HandleTimesheetApproval(model);

                case "leave":
                    return await HandleLeaveReminder(empID, Convert.ToInt32(appID), toID, toEmpID, reminderFor);

                case "travel":
                    return await HandleTravelReminder(empID, Convert.ToInt32(appID), toID, toEmpID, reminderFor, reminderForPerformance);
                case "overtime":
                    return await HandleOvertimeReminder(appID, reminderFor);
                case "leavefuture":
                    return await HandleLeaveFutureReminder(empID, Convert.ToInt32(appID), toID, toEmpID, reminderFor);
                default:
                    return Json(new { status = "unsupported" });
            }
        }

        private async Task<IActionResult> HandleTravelReminder(int empID, int appID, int toID, int toEmpID, string reminderFor, string reminderForPerformance)
        {
            // ✅ Update leave record
            var travel = await context.tbl_employee_travel_main
                .FirstOrDefaultAsync(l => l.emp_travel_id == Convert.ToInt32(appID));

            if (travel == null)
            {
                return Json(new { status = "notfound", message = "Travel record not found." });
            }
            string JsonMessage = "Travel reminder sent";
            // Email Contents
            string EmailTo = _employeeService.GetEmployeeNameEmail(toEmpID);
            string EmployeeName = _employeeService.GetEmployeeName(empID);
            var EmailContent = await _travelService.GetTravelEmailHtmlContent(appID);
            string subject = $"Re notification of travel submitted by {EmployeeName}"; // defaulted to Pending
            string body = $"Dear Sir/Madam,<br/><br/>Please find my <u>notification</u> of travel request below.<br/><br/>{EmailContent}<br/><br/>Regards<br/>{EmployeeName}<br/><br/>"; // defaulted to Pending
            if (reminderForPerformance == "Pending")
            {
                
                
            }
            else if (reminderForPerformance == "Cancelling")
            {
                JsonMessage = "Travel cancellation reminder sent";
                subject = $"Re notification of travel cancellation request submitted by {EmployeeName}";
                string eml_app_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empID}&app_id={appID}&toid={toID}&toemp_id={toEmpID}&st=ca&approval_from=email&approve_for=travel'>Approve</a> | ";
                string eml_dec_link_outside = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={empID}&app_id={appID}&toid={toID}&toemp_id={toEmpID}&st=cd&approval_from=email&approve_for=travel'>Decline</a> | ";
                body = $"Dear Sir/Madam,<br/><br/>Please find my notification of travel cancellation request below.<br/><br/>{EmailContent}<br/><br/>Please click Approve or Decline link provided below as appropriate.<br/><br/>{eml_app_link_outside} {eml_dec_link_outside}<br/><br/><br/><br/>Regards<br/>{EmployeeName}<br/><br/>";
            }
            if (!string.IsNullOrEmpty(EmailTo))
            {
                _emailSenderServices.SendEmail(null, EmailTo, subject, body, null, "", null, null, null);
            }

            return Json(new { status = "success", message = JsonMessage, reminderFor = reminderFor });
        }
        private async Task<IActionResult> HandleLeaveReminder(int empID, int appID, int toID, int toEmpID, string reminderFor)
        {
            // ✅ Update leave record
            var leave = await context.tbl_employee_leave
                .FirstOrDefaultAsync(l => l.emp_leave_id == Convert.ToInt32(appID));

            if (leave == null)
            {
                return Json(new { status = "notfound", message = "Leave record not found." });
            }
            // ✅ Fetch leave details for email
            var leaveDetails = await context.tbl_employee_leave
                .Where(l => l.emp_leave_id == Convert.ToInt32(appID))
                .Select(l => new
                {
                    l.leave_type_id,
                    l.submit_date,
                    l.leave_from_date,
                    l.leave_to_date,
                    l.leave_in_hrs,
                    l.leave_desc,
                    l.app_remarks
                })
                .FirstOrDefaultAsync();

                var leaveTypeName = await context.tbl_leave_heading
                     .Where(e => e.leave_type_id == leaveDetails.leave_type_id)
                     .Select(e => e.description)
                     .FirstOrDefaultAsync();
                string remarks = string.IsNullOrEmpty(leaveDetails.app_remarks) ? "N/A" : leaveDetails.app_remarks;
                string EmployeeName =  _employeeService.GetEmployeeName(Convert.ToInt32(empID));
                string toEmployeeEmail =  _employeeService.GetEmployeeNameEmail(Convert.ToInt32(toEmpID));

                string approverName =  _employeeService.GetEmployeeName(Convert.ToInt32(toEmpID));
                string subject = "";
                string body = "";
                string detailMessage = $@"
                    <b>Leave Type:</b> {leaveTypeName}<br/>
                    <b>Submit Date:</b> {leaveDetails.submit_date:dd-MMM-yyyy}<br/>
                    <b>Leave From Date:</b> {leaveDetails.leave_from_date:dd-MMM-yyyy}<br/>
                    <b>Leave To Date:</b> {leaveDetails.leave_to_date:dd-MMM-yyyy}<br/>
                    <b>Leave Hours:</b> {leaveDetails.leave_in_hrs}<br/>
                    <b>Description:</b> {leaveDetails.leave_desc}<br/>
                    <b>Remarks:</b> {remarks}<br/>";
                 
                subject = $"Re notification of leave submitted by {EmployeeName}.";
                body = $"Dear Sir/Madam,<br/><br/>" +
                       $"Please find my leave request below.<br/><br/>{detailMessage}<br/>" +
                       $"Regards,<br/>{EmployeeName}";


            // ✅ Send email notification

            if (!string.IsNullOrEmpty(toEmployeeEmail))
            {
                _emailSenderServices.SendEmail(null, toEmployeeEmail, subject, body, null, "", null, null, null);
            }

            return Json(new { status = "success", message = "Leave reminder sent", reminderFor = reminderFor });
        }

        private async Task<IActionResult> HandleOvertimeApproval(ApprovalFromEmailVM model)
        {
            string subject = "";
            string EmployeeName = _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
            var overtime = await context.tbl_employee_overtime_request
                .FirstOrDefaultAsync(t => t.ot_req_id == model.AppID);
            if (overtime == null)
            {
                return Json(new { status = "notfound" });
            }
            if (model.St == "rr") // Recommend
            {
                if (overtime.req_status == "R")
                {
                    return Json(new { status = "alreadyrecommended", message = "Overtime has already been recommended." });
                }
                // Update Table
                overtime.req_status = "R";
                overtime.req_date = DateTime.Now;   // equivalent to Date()
                overtime.req_remarks = model.Description;
                await context.SaveChangesAsync();
                subject = $"Overtime of {EmployeeName} has been recommended.";

            }

            else if (model.St == "nr") // Recommendation declined
            {
                if (overtime.req_status == "D")
                {
                    return Json(new { status = "alreadyrecommendationdeclined", message = "Overtime recommendation has already been declined." });
                }
                // Update Table
                overtime.req_status = "D";
                overtime.req_date = DateTime.Now;   // equivalent to Date()
                overtime.req_remarks = model.Description;
                await context.SaveChangesAsync();
                subject = $"Overtime of {EmployeeName} recommendation has been declined.";
            }

            else if (model.St == "a") // Approve
            {
                if (overtime.app_status == "A")
                {
                    return Json(new { status = "alreadyapproved", message = "Overtime has already been approved." });
                }
                var enoughHours = _employeeOvertimeServices.CheckOvertimeSufficiency(Convert.ToInt32(model.EmpID), Convert.ToDateTime(overtime.ot_date), Convert.ToDecimal(overtime.total_hours));
                if (enoughHours == "ND" || enoughHours == "NW")
                {
                    var appWeekHours = _employeeOvertimeServices.GetApprovedHoursInWeek(Convert.ToInt32(model.EmpID), Convert.ToDateTime(overtime.ot_date));
                    var appDayHours = _employeeOvertimeServices.GetApprovedHoursInDay(Convert.ToInt32(model.EmpID), Convert.ToDateTime(overtime.ot_date));

                    return Json(new { status = "notenoughhoursforweek", message = "Overtime exceeds weekly/daily limit." });

                }
                // Update Table
                overtime.app_status = "A";
                overtime.req_status = ((overtime.app_by == overtime.requested_by && overtime.req_status == "P"))?"R": overtime.req_status; 
                overtime.app_date = DateTime.Now;   // equivalent to Date()
                overtime.app_remarks = model.Description;
                await context.SaveChangesAsync();
                
                subject = $"Overtime of {EmployeeName} has been approved.";

            }
            else if (model.St == "d") // Decline
            {
                if (overtime.app_status == "D")
                {
                    return Json(new { status = "alreadydeclined", message = "Overtime has already been declined." });
                }
                // Update Table
                overtime.app_status = "D";
                overtime.app_date = DateTime.Now;   // equivalent to Date()
                overtime.app_remarks = model.Description;
                await context.SaveChangesAsync();
                subject = $"Overtime of {EmployeeName} has been declined.";
            }

            await _employeeOvertimeServices.OvertimeSendEmailAsync(model.AppID, model.St);

            return Json(new { status = "approved", message = subject, approveFor = model.ApproveFor });
        }

        private async Task<IActionResult> HandleOvertimeReminder(string appID,  string reminderFor)
        {
            var overtime = await context.tbl_employee_overtime_request
                .FirstOrDefaultAsync(l => l.ot_req_id == appID);

            if (overtime == null)
            {
                return Json(new { status = "notfound", message = "Overtime record not found." });
            }
            string JsonMessage = "Overtime reminder sent";
            await _employeeOvertimeServices.OvertimeSendEmailAsync(appID, "add");

            return Json(new { status = "success", message = JsonMessage, reminderFor = reminderFor });
        }

        private async Task<IActionResult> HandleLeaveFutureApproval(ApprovalFromEmailVM model)
        {
            // ✅ Update leave record
            var leave = await context.tbl_employee_leave_hash
                .FirstOrDefaultAsync(l => l.emp_leave_id == Convert.ToInt32(model.AppID));

            if (leave == null)
            {
                return Json(new { status = "notfound", message = "Leave Future record not found." });
            }
            int AlreadyApproved = context.tbl_employee_leave_hash
                .Count(r => r.emp_leave_id == Convert.ToInt32(model.AppID) && r.app_status == "Approved");

            if (AlreadyApproved > 0)
                return Json(new { status = "alreadyapproved", message = "Leave Future has already been approved." });

            leave.app_status = model.St == "a" ? "Approved" : "Declined";
            leave.app_by = model.ToEmpID;
            leave.app_date = DateTime.Now;
            leave.app_remarks = model.Description;
            context.Update(leave);
            await context.SaveChangesAsync();

            // ✅ Fetch leave details for email
            var leaveDetails = await context.tbl_employee_leave_hash
                .Where(l => l.emp_leave_id == Convert.ToInt32(model.AppID))
                .Select(l => new
                {
                    l.leave_type_id,
                    l.submit_date,
                    l.leave_from_date,
                    l.leave_to_date,
                    l.leave_in_hrs,
                    l.leave_desc,
                    l.app_remarks,
                    l.fiscal_year
                })
                .FirstOrDefaultAsync();

            var leaveTypeName = await context.tbl_leave_heading
                 .Where(e => e.leave_type_id == leaveDetails.leave_type_id)
                 .Select(e => e.description)
                 .FirstOrDefaultAsync();
            string remarks = string.IsNullOrEmpty(leaveDetails.app_remarks) ? "N/A" : leaveDetails.app_remarks;
            string EmployeeName =  _employeeService.GetEmployeeName(Convert.ToInt32(model.EmpID));
            string EmployeeEmail =  _employeeService.GetEmployeeNameEmail(Convert.ToInt32(model.EmpID));

            string approverName =  _employeeService.GetEmployeeName(Convert.ToInt32(model.ToEmpID));
            string subject = "";
            string body = "";
            string detailMessage = $@"
                    <b>Leave Type:</b> {leaveTypeName}<br/>
                    <b>Submit Date:</b> {leaveDetails.submit_date:dd-MMM-yyyy}<br/>
                    <b>Fiscal Year:</b> {leaveDetails.fiscal_year}<br/>
                    <b>Leave From Date:</b> {leaveDetails.leave_from_date:dd-MMM-yyyy}<br/>
                    <b>Leave To Date:</b> {leaveDetails.leave_to_date:dd-MMM-yyyy}<br/>
                    <b>Leave Hours:</b> {leaveDetails.leave_in_hrs}<br/>
                    <b>Description:</b> {leaveDetails.leave_desc}<br/>
                    <b>Remarks:</b> {remarks}<br/>";
            if (model.St == "a")
            {
                subject = $"Future Leave of {EmployeeName} has been approved.";
                body = $"Dear {EmployeeName},<br/><br/>" +
                       $"Your future leave has been approved.<br/><br/>{detailMessage}<br/>" +
                       $"Regards,<br/>{approverName}";

            }
            if (model.St == "d")
            {
                subject = $"Future Leave of {EmployeeName} has been declined.";
                body = $"Dear {EmployeeName},<br/><br/>" +
                       $"Your future leave has been declined.<br/><br/>{detailMessage}<br/>" +
                       $"Regards,<br/>{approverName}";
            }

            // ✅ Send email notification

            if (!string.IsNullOrEmpty(EmployeeEmail))
            {
                _emailSenderServices.SendEmail(null, EmployeeEmail, subject, body, null, "", null, null, null);
            }
            if (model.St == "a")
            {   // Send Email to HR
                var Adminemails = await _administrationEmailService.GetAdministrationEmailsAsync();
                int hr_ID = Convert.ToInt32(Adminemails["hra"].Id);
                string hr_Name =  _employeeService.GetEmployeeName(Convert.ToInt32(hr_ID));
                string hr_Email = Adminemails["hra"].Email;
                string hr_Subject = $"Future Leave of {EmployeeName} has been approved.";
                string hr_boday = $"Dear {hr_Name},<br/><br/>Leave of {EmployeeName} has been approved.<br/><br/>{detailMessage}<br/><br/>Regards,<br/>{approverName}<br/><br/>";
            }
            return Json(new { status = leave.app_status.ToLower(), message = subject, approveFor = model.ApproveFor });
        }

        private async Task<IActionResult> HandleLeaveFutureReminder(int empID, int appID, int toID, int toEmpID, string reminderFor)
        {
            // ✅ Update leave record
            var leave = await context.tbl_employee_leave_hash
                .FirstOrDefaultAsync(l => l.emp_leave_id == Convert.ToInt32(appID));

            if (leave == null)
            {
                return Json(new { status = "notfound", message = "Future Leave record not found." });
            }
            // ✅ Fetch leave details for email
            var leaveDetails = await context.tbl_employee_leave_hash
                .Where(l => l.emp_leave_id == Convert.ToInt32(appID))
                .Select(l => new
                {
                    l.leave_type_id,
                    l.submit_date,
                    l.leave_from_date,
                    l.leave_to_date,
                    l.leave_in_hrs,
                    l.leave_desc,
                    l.app_remarks,
                    l.fiscal_year,
                    l.emp_id,
                    l.app_by
                })
                .FirstOrDefaultAsync();

            var leaveTypeName = await context.tbl_leave_heading
                 .Where(e => e.leave_type_id == leaveDetails.leave_type_id)
                 .Select(e => e.description)
                 .FirstOrDefaultAsync();


            string remarks = string.IsNullOrEmpty(leaveDetails.app_remarks) ? "N/A" : leaveDetails.app_remarks;
            string EmployeeName =  _employeeService.GetEmployeeName(Convert.ToInt32(empID));
            string toEmployeeEmail =  _employeeService.GetEmployeeNameEmail(Convert.ToInt32(toEmpID));

            string approverName =  _employeeService.GetEmployeeName(Convert.ToInt32(toEmpID));
            string subject = "";
            string body = "";
            string detailMessage = $@"
                    <b>Leave Type:</b> {leaveTypeName}<br/>
                    <b>Submit Date:</b> {leaveDetails.submit_date:dd-MMM-yyyy}<br/>
                    <b>Leave From Date:</b> {leaveDetails.leave_from_date:dd-MMM-yyyy}<br/>
                    <b>Leave To Date:</b> {leaveDetails.leave_to_date:dd-MMM-yyyy}<br/>
                    <b>Leave Hours:</b> {leaveDetails.leave_in_hrs}<br/>
                    <b>Description:</b> {leaveDetails.leave_desc}<br/>
                    <b>Remarks:</b> {remarks}<br/>";

            string approveEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={leaveDetails.emp_id}&app_id={appID}&toid={toID}&toemp_id={leaveDetails.app_by}&st=a&approval_from=email&approve_for=leavefuture'>Approve</a> | ";
            string declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={leaveDetails.emp_id}&app_id={appID}&toid={toID}&toemp_id={leaveDetails.app_by}&st=d&approval_from=email&approve_for=leavefuture'>Decline</a>";

            subject = $"Re notification of future leave submitted by {EmployeeName}.";
            body = $"Dear Sir/Madam,<br/><br/>" +
                   $"Please find my future leave request below.<br/><br/>{detailMessage}<br/><br/>" +
                   $"Please click Approve or Decline link provided below as appropriate.<br /><br />{ approveEmailLink}{ declineEmailLink}<br/><br/>" +
                   $"Regards,<br/>{EmployeeName}";


            // ✅ Send email notification

            if (!string.IsNullOrEmpty(toEmployeeEmail))
            {
                _emailSenderServices.SendEmail(null, toEmployeeEmail, subject, body, null, "", null, null, null);
            }

            return Json(new { status = "success", message = "Future Leave reminder sent", reminderFor = reminderFor });
        }

    }
}
