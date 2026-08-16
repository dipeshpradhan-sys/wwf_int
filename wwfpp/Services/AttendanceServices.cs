using Microsoft.EntityFrameworkCore;
using Azure.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Attendance;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Services
{
    public class AttendanceServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AttendanceServices(
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
        * Since : 2026-Jul-15
        ****************************************************************************************************/
        public bool autoSendEmailStaffUpdate(string EmployeeType, DateTime InOutDay, string DutyStation, int? SendBy, string SendMode = "")
        {
            return false;
        }


        /***************************************************************************************************
        * Since : 2026-Jul-17
        * Get Attendance sub section
        ****************************************************************************************************/
        public string GetAttendanceUpdateSub(int emp_id, DateTime in_out_date, string employee_type)
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine($@"<div id = ""message_header"" ><p class=""note"">Check In/Out record(s).</p></div>");
            _ = sb.AppendLine($@"<div id = ""message_header"" ><p id=""modal-message-sub"" class=""displaynone"">&nbsp;</p></div>");

            _ = sb.AppendLine($@"
                <div class=""form-group p-2"" >
                    <table id = ""tblSub"" class=""display compact nowrap"" style=""width:100%"" >
                        <thead>
                            <tr style = ""height: 30px; align-items: center;color:white;"" class=""bg-secondary bg-opacity-27"">
                                <th width = ""15%"">Check In</th>
                                <th width = ""15%"">Check Out</th>
                                <th width = ""55%"">Reason</th>
                                <th width= ""15%"">Action</th>
                            </tr>
                        </thead>
                    </table>
                </div>");
            if (emp_id > 0 && !string.IsNullOrWhiteSpace(employee_type))
            {
                var result = _context.vwAttendanceDailyStaffUpdateSub
                    .Where(log => log.emp_id == emp_id && log.in_out_date == in_out_date && log.employee_type == employee_type)
                    .Select(x => new DailyCheckInOutStaffUpdateSubViewModel
                    {
                        sub_id = x.id,
                        emp_id = x.emp_id,
                        employee_type = x.employee_type,
                        check_in = x.check_in,
                        check_out = x.check_out,
                        remarks = x.remarks
                    })
                    .OrderBy(x => x.check_in)
                    .ToList();

                int cnt = 0;
                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        cnt++;
                        _ = sb.AppendLine($@"
                        <div class=""form-group p-2"" >
                            <table id = ""tblSub"" class=""display compact nowrap"" style=""width:100%"" >
                                <thead>
                                    <tr style = ""height: 30px; align-items: center;color:black;"" class=""bg-opacity-27"">
                                        <th width = ""15%""><input type=""text"" id=""check_in{cnt}"" name=""check_in{cnt}"" value=""{item.check_in}"" maxlength =""8"" class=""form-control"" style=""width:100px;""></th>
                                        <th width = ""15%""><input type=""text"" id=""check_out{cnt}"" name=""check_out{cnt}"" value=""{item.check_out}""maxlength =""8"" class=""form-control"" style=""width:100px;""></th>
                                        <th width = ""55%""><input type=""textarea"" id=""reason{cnt}"" name=""reason{cnt}"" class=""form-control"" rows =""1"" cols=""50"" maxlength =""100""></textarea></th>
                                        <th width= ""15%""><Button type=""button"" name=""btnUpdateSub{cnt}"" id=""btnUpdateSub{cnt}"" class=""button bg-dgray"" data-subid=""{item.sub_id}"" data-id=""{cnt}"" data-action=""update"">Update</button></th>
                                    </tr>
                                </thead>
                            </table>
                        </div>");

                    }
                }
            }
            _ = sb.AppendLine($@"
                        <div class=""form-group p-2"" >
                            <table  class=""display compact nowrap"" style=""width:100%"" >
                                <thead>
                                    <tr style = ""height: 30px; align-items: center;color:black;"" class=""bg-opacity-27"">
                                        <th width = ""15%""><input type=""text"" id=""check_inS"" name=""check_inS"" value="""" maxlength =""8"" class=""form-control"" style=""width:100px;""></th>
                                        <th width = ""15%""><input type=""text"" id=""check_outS"" name=""check_outS"" value="""" maxlength =""8"" class=""form-control"" style=""width:100px;""></th>
                                        <th width = ""55%""><input type=""textarea"" id=""reasonR"" name=""reasonR"" class=""form-control"" rows =""1"" cols=""50"" ></textarea></th>
                                        <th width= ""15%""><Button type=""button"" name=""btnSaveSub"" id=""btnSaveSub"" class=""button bg-dgray"" data-subid="""" data-id="""" data-action=""add"">Save</button></th>
                                    </tr>
                                </thead>
                                <tbody>
                                </tbody>
                            </table>
                        </div>");
            _ = sb.AppendLine($@"
                        <hr>
                        <div class=""form-group p-2"" >
                            <table  class=""display compact nowrap"" style=""width:100%"" >
                                <thead>
                                    <tr style = ""height: 10px; align-items: center;color:black;"" class=""bg-opacity-27"">
                                        <th width = ""15%"">00:00:00</th>
                                        <th width = ""15%"">00:00:00 or n</th>
                                        <th width = ""55%""></th>
                                        <th width= ""15%""></th>
                                    </tr>
                                </thead>
                            </table>
                        </div>");
            return sb.ToString();
        }
        /***************************************************************************************************
        * Since : 2026-Jul-17
        * Change Log
        ****************************************************************************************************/
        public string GetAttendanceUpdateChangeLog(int emp_id, DateTime in_out_date, string employee_type)
        {
            var sb = new StringBuilder();
            if (emp_id > 0 && !string.IsNullOrWhiteSpace(employee_type))
            {
                var result = _context.vwAttendanceDailyStaffUpdateChangeLog
                    .Where(log => log.emp_id == emp_id && log.in_out_date == in_out_date && log.employee_type == employee_type)
                    .Select(x => new DailyCheckInOutStaffUpdateChangeLogViewModel
                    {
                        log_id = x.id,
                        emp_id = x.emp_id,
                        old_value = x.old_value,
                        new_value = x.new_value,
                        by_emp_id = x.by_emp_id,
                        change_date = x.change_date,
                        change_on = x.change_on,
                        change_type = x.change_type,
                        reason = x.reason
                    })
                    .OrderBy(x => x.change_date)
                    .ToList();

                if (result.Count > 0)
                {
                    _ = sb.AppendLine($@"<div id = ""message_header"" ><p class=""note"">Change log record(s).</p></div>");
                    _ = sb.AppendLine($@"
                        <div class=""divTable w-100"">
                          <div class=""divTableRow bg-gray"">
                            <div class=""divTableCell title left w-15"">Date</div>
                            <div class=""divTableCell title left w-25"">&nbsp;By</div>
                            <div class=""divTableCell title left w-10"">On</div>
                            <div class=""divTableCell title left w-10"">Type</div>
                            <div class=""divTableCell title left w-40"">Reason</div>
                          </div>
                        </div>");
                    foreach (var item in result)
                    {
                        var byemp = _context.tbl_employee.FirstOrDefault(emp => emp.emp_id == item.by_emp_id);
                        string by_name = $"{byemp.firstname} {byemp.middlename} {byemp.lastname} ({byemp.emp_code})";

                        _ = sb.AppendLine($@"
                        <div class=""divTable w-100"">
                          <div class=""bg-lgray"">
                            <div class=""divTableCell normal left w-15"">{item.change_date}</div>
                            <div class=""divTableCell normal left w-25"">{by_name}</div>
                            <div class=""divTableCell normal left w-10"">{item.change_on}</div>
                            <div class=""divTableCell normal left w-10"">{item.change_type}</div>
                            <div class=""divTableCell normal left w-40"">{item.reason}</div>
                          </div>
                        </div>
                        <div class=""divTable w-100"">
                          <div class=""divTableRow bg-silver w-100"">
                            <div class=""divTableCell normal left w-100"">Previous Value : {item.old_value}</div>
                          </div>
                        <div class=""divTable w-100"">
                          <div class=""divTableRow bg-silver w-100"">
                            <div class=""divTableCell normal left w-100"">Post Value : {item.new_value}</div>
                          </div>
                        </div><hr>");
                    }
                }
                else
                {
                    _ = sb.AppendLine($@"<div id = ""message_header"" ><p class=""note"">No change log record(s) found.</p></div>");
                }
            }
            return sb.ToString();
        }
        /***************************************************************************************************
        * Since : 2026-Jul-16
        * 
        ****************************************************************************************************/
        public string GetCheckInOutInfo(string Id, string saveMode = "")
        {
            if (saveMode == "B")
            {
                var smt = _context.vwAttendanceDailyStaffUpdate.FirstOrDefault(d => d.id == Id);
                return (smt != null) ? $@"Remarks : {smt.remarks} | Narration :	{smt.narration}" : "";
            }
            else if (saveMode == "S")
            {
                var smt = _context.vwAttendanceDailyStaffUpdateSub.FirstOrDefault(d => d.id == Id);
                return (smt != null) ? $@"Check In : {smt.check_in} | Check Out : {smt.check_out}" : "";
            }
            else
            {
                var smt = _context.vwAttendanceDailyStaffUpdate.FirstOrDefault(d => d.id == Id);
                return (smt != null) ? $@"First Check In : {smt.check_in} | Last Check Out : {smt.check_out} | 
                       Remarks : {smt.remarks} | Narration : {smt.narration}" : "";
            }
        }
        /***************************************************************************************************
        * Since : 2026-Jul-16
        * Duty Station
        ****************************************************************************************************/
        public string GetDutyStation(string Id)
        {
            var stations = _context.tbl_duty_station.FirstOrDefault(d => d.id == Id);
            return stations == null ? "" : stations.duty_station;
        }
        /***************************************************************************************************
        * Since : 2026-Jul-16
        ****************************************************************************************************/
        public SelectList GetNarrationList(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "First half", "First half" },
                { "Second half", "Second half" },
                { "National travel", "National travel" },
                { "International travel", "International travel" },
                { "Half an hour", "Half an hour" },
                { "One hour", "One hour" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-16
        ****************************************************************************************************/
        public SelectList GetRemarksList(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {

                { "Not informed", "Not informed"},
                { "Late to office", "Late to office"},
                { "Working from home", "Working from home"},
                { "Out of office", "Out of office"},
                { "Leave", "Leave"},
                { "Travel", "Travel"},
                { "Strike/Closure", "Strike/Closure"},
                { "Day Off", "Day Off"}
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-15
        ****************************************************************************************************/
        public SelectList GetEmployeeType(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "", "Both" },
                { "Inside", "Inside" },
                { "Outside", "Outside" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jun-22
        * Duty Station || KTM Only
        ****************************************************************************************************/
        public SelectList GetDutyStationList(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "1", "KTM" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-18
        ****************************************************************************************************/
        public SelectList GetAbsentRemarkLTO(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "", "Saved in Database"},
                { "09:00:00", "09:00 AM" },
                { "09:05:00", "09:05 AM" },
                { "09:10:00", "09:10 AM" },
                { "09:15:00", "09:15 AM" },
                { "09:20:00", "09:20 AM" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }


    }
}