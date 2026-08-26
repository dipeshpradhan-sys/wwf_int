using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Services;

namespace wwfpp.Services
{
    public class EmployeeOvertimeServices
    {
        private readonly AppDbContext _context;
        //private readonly GeneralServices _generalServices;
        private readonly EmployeeServices _employeeServices;
        private readonly EmailService _emailService;
        private readonly AppSettings _appSettings;
        private readonly RequestServices _requestServices;
        private readonly ApproverResolverService _approverResolver;
        public EmployeeOvertimeServices(AppDbContext context, EmployeeServices employeeServices, EmailService emailService, IOptions<AppSettings> appSettings, RequestServices requestServices, ApproverResolverService approverResolver)
        {
            _context = context;
            //_generalServices = generalServices;
            _employeeServices = employeeServices;
            _emailService = emailService;
            _appSettings = appSettings.Value;
            _requestServices = requestServices;
            _approverResolver = approverResolver;
        }

        /// <summary>
        /// Check if applied overtime hours exceed daily or weekly limits.
        /// Returns "ND" (Not enough daily hours), "NW" (Not enough weekly hours), or "Y" (Allowed).
        /// </summary>
        public string CheckOvertimeSufficiency(int empId, DateTime otDate, decimal appliedHours)
        {
            // Pull settings (normal + overtime working hours)
            var hrs = _requestServices.GetLimitHoursSetting();
            int overtimeDailyLimit = (int)hrs.overtime_normal_working_hrs;   // daily limit
            int weeklyLimit = (int)hrs.normal_working_hrs;                  // weekly limit (or replace with eligible_hours_per_week if stored)

            // Daily check
            var dayHours = GetApprovedHoursInDay(empId, otDate);
            if (dayHours + appliedHours > overtimeDailyLimit)
                return "ND";

            // Weekly check
            var weekHours = GetApprovedHoursInWeek(empId, otDate);
            if (weekHours + appliedHours > weeklyLimit)
                return "NW";

            return "Y";
        }

        /// <summary>
        /// Get total approved overtime hours for a given day.
        /// </summary>
        public decimal GetApprovedHoursInDay(int empId, DateTime otDate)
        {
            return (decimal)_context.tbl_employee_overtime_request
                .Where(o => o.emp_id == empId && o.ot_date == otDate && o.app_status == "A")
                .Sum(o => o.total_hours ?? 0);
        }

        /// <summary>
        /// Get total approved overtime hours for the week containing otDate.
        /// </summary>
        public decimal GetApprovedHoursInWeek(int empId, DateTime otDate)
        {
            var weekStart = GetWeekStart(otDate);
            var weekEnd = GetWeekEnd(otDate);

            return (decimal)_context.tbl_employee_overtime_request
                .Where(o => o.emp_id == empId &&
                            o.ot_date >= weekStart &&
                            o.ot_date <= weekEnd &&
                            o.app_status == "A")
                .Sum(o => o.total_hours ?? 0);
        }

        /// <summary>
        /// Helper to get start of week (Thursday).
        /// </summary>
        private DateTime GetWeekStart(DateTime date)
        {
            // Thursday is considered the start of the week
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Thursday)) % 7;
            return date.Date.AddDays(-diff);
        }

        /// <summary>
        /// Helper to get end of week (Wednesday).
        /// </summary>
        private DateTime GetWeekEnd(DateTime date)
        {
            return GetWeekStart(date).AddDays(6);
        }


        /// <summary>
        /// Resolve OT Manager ID (placeholder, implement your own logic).
        /// </summary>
        public int GetOTManagerId(int empId)
        {
            // Example: look up manager from employee table or config
            return _context.tbl_employee_overtime_settings.FirstOrDefault(e => e.emp_id == empId)?.approval_person ?? 0;
        }

        public async Task OvertimeSendEmailAsync(string otid, string mode)
        {
            // Fetch overtime request record
            var overtimeRequest = await _context.tbl_employee_overtime_request
                .Where(o => o.ot_req_id == otid)
                .Select(o => new
                {
                    o.ot_req_id,
                    o.emp_id,
                    o.ot_date,
                    o.submit_date,
                    o.total_hours,
                    o.ot_desc,
                    o.requested_by,
                    o.app_by
                })
                .FirstOrDefaultAsync();
            string otDate = overtimeRequest.ot_date?.ToString("dd/MM/yyyy") ?? "";
            string submit_date = overtimeRequest.submit_date?.ToString("dd/MM/yyyy") ?? "";
            double total_hours = overtimeRequest.total_hours ?? 0;
            string ot_desc = overtimeRequest.ot_desc ?? "";
            string requested_by_Name =  _employeeServices.GetEmployeeName(Convert.ToInt32(overtimeRequest.requested_by));
            string otSubmitempName = _employeeServices.GetEmployeeName(Convert.ToInt32(overtimeRequest.emp_id));
            string str_to = _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(overtimeRequest.emp_id));

            int requested_by = Convert.ToInt32(overtimeRequest.requested_by);
            int app_by = Convert.ToInt32(overtimeRequest.app_by);
            int emp_id = Convert.ToInt32(overtimeRequest.emp_id);
            int toemp_id = 0;
            string subjectAttach = "Overtime submitted by ";
            string bodyAttach = "Dear Sir/Madam,<br/><br/>Please find my overtime request below. ";
            string approveEmailLink = "";
            string declineEmailLink = "";
            string modeResult = "";
            string recommendationOrApproveMessage = "Please approve my leave request by click Approve or Decline link provided below as appropriate.";
            string strcc = "";
            string empName = "";
            if (mode == "" || mode == "add" || mode == "edit")
            {
                if (requested_by == app_by)
                {
                    toemp_id = app_by;
                    int toid = (int)await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(toemp_id));
                    str_to =  _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(toemp_id));
                    approveEmailLink = $"<br/><br/><a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=a&approval_from=email&approve_for=overtime'>Approve</a> | ";
                    declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=d&approval_from=email&approve_for=overtime'>Decline</a>";

                }
                else
                {
                    recommendationOrApproveMessage = "Please recommend my leave request by click Approve or Decline link provided below as appropriate.";
                    toemp_id = requested_by;
                    str_to =  _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(toemp_id));
                    int toid = (int)await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(toemp_id));
                    approveEmailLink = $"<br/><br/><a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=rr&approval_from=email&approve_for=overtime'>Approve</a> | ";
                    declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=nr&approval_from=email&approve_for=overtime'>Decline</a>";

                }
            }
            if (mode == "edit")
            {
                subjectAttach = "Change in overtime submitted by ";
                bodyAttach = "Please find my changed overtime request below.";
            }
            if (mode == "a") // Approved
            {
                bodyAttach = $"Dear {otSubmitempName},<br/><br/>Your following overtime request has been approved.";
                modeResult = " has been approved";
                recommendationOrApproveMessage = "";
                approveEmailLink = "";
                declineEmailLink = "";
                empName = _employeeServices.GetEmployeeName(Convert.ToInt32(app_by));
                if (app_by != requested_by)
                {
                    strcc = _employeeServices.GetEmployeeNameEmail(requested_by);
                }
            }
            if (mode == "d") // Declined
            {
                bodyAttach = $"Dear {otSubmitempName},<br/><br/>Your following overtime request has been declined.";
                modeResult = " has been declined";
                recommendationOrApproveMessage = "";
                approveEmailLink = "";
                declineEmailLink = "";
                empName =  _employeeServices.GetEmployeeName(Convert.ToInt32(app_by));
                if (app_by != requested_by)
                {
                    strcc =  _employeeServices.GetEmployeeNameEmail(requested_by);
                }
            }

            if (mode == "rr") // Recommend Approved
            {
                bodyAttach = $"Dear Sir/Madam,<br/><br/>Please find recommended overtime request below.";
                toemp_id = app_by;
                int toid = (int)await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(toemp_id));
                str_to =  _employeeServices.GetEmployeeNameEmail(Convert.ToInt32(toemp_id));
                approveEmailLink = $"<br/><br/><a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=a&approval_from=email&approve_for=overtime'>Approve</a> | ";
                declineEmailLink = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={emp_id}&app_id={otid}&toid={toid}&toemp_id={toemp_id}&st=d&approval_from=email&approve_for=overtime'>Decline</a>";
            }
            if (mode == "nr") // Recommend Declined
            {
                bodyAttach = $"Dear {otSubmitempName},<br/><br/>Your overtime request recommendation has been declined.";
                recommendationOrApproveMessage = "Please make necessary correction and send it again.";
                toemp_id = app_by;
                int toid = (int)await _approverResolver.ResolveEmployeeIdInUserTblAsync(Convert.ToInt32(toemp_id));
                empName =  _employeeServices.GetEmployeeName(Convert.ToInt32(requested_by));
                approveEmailLink = "";
                declineEmailLink = "";
            }


            

            string overtimeDetailBody = $"<b>Overtime date: </b>{otDate}<br/><b>Submit date: </b>{submit_date}<br/><b>Total hour(s): </b>{total_hours}<br/><b>Reason/Description: </b>{ot_desc}<br/><b>Requested by: </b>{requested_by_Name}";
            string subject = $"{subjectAttach} {otSubmitempName} {modeResult}"; 
            string body = $"{bodyAttach}<br/><br/>{overtimeDetailBody}<br/><br/>{recommendationOrApproveMessage}{approveEmailLink}{declineEmailLink}<br/><br/>Regards<br/>{empName}<br/><br/>"; // Defaulted to add
            _emailService.SendEmail(null, str_to, subject, body, null, strcc, null, null, null);
        }
    }
}
