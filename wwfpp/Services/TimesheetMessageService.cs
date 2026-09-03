using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Helpers;
using wwfpp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace wwfpp.Services
{
    public class TimesheetMessageService
    {
        private readonly RequestServices _requestServices;
        private readonly AppDbContext _context;

        public TimesheetMessageService(RequestServices requestServices, AppDbContext context)
        {
            _requestServices = requestServices;
            _context = context;
        }

        public async Task<List<wwfpp.Models.Request.TimesheetMessage>> BuildMessagesAsync(
            int empid,
            int year,
            int month,
            int maxtimeSheetCounter,
            bool empActive,
            bool calendarFilled,
            bool employeeFiscalFund,
            string prevTimesheetStatus,
            string curTimesheetStatus
        )
        {
            var messages = new List<wwfpp.Models.Request.TimesheetMessage>();

            // Employee active check
            if (!empActive)
            {
                messages.Add(new wwfpp.Models.Request.TimesheetMessage
                {
                    Text = "Employee not active for selected Month / Year.",
                    Type = "error"
                });
            }

            // Calendar check
            if (!calendarFilled && messages.Count == 0)
            {
                messages.Add(new wwfpp.Models.Request.TimesheetMessage
                {
                    Text = "Calendar not filled for selected month/year.",
                    Type = "error"
                });
            }

            // Fund source check
            if (!employeeFiscalFund && messages.Count == 0)
            {
                messages.Add(new wwfpp.Models.Request.TimesheetMessage
                {
                    Text = "No fund source assigned for this employee.",
                    Type = "warning"
                });
            }

            // Previous timesheet check
            if (prevTimesheetStatus != "active" && messages.Count == 0)
            {
                string prevMsgText = _requestServices.GetPreviousTimesheetMessage(prevTimesheetStatus);
                messages.Add(new wwfpp.Models.Request.TimesheetMessage
                {
                    Text = prevMsgText,
                    Type = "warning"
                });
            }


            // Current timesheet check
            if (messages.Count == 0)
            {
                string msgText = _requestServices.GetCurrentTimesheetMessage(curTimesheetStatus);
                string msgType = curTimesheetStatus == "active" ? "success" : "warning";
                messages.Add(new wwfpp.Models.Request.TimesheetMessage
                {
                    Text = msgText,
                    Type = msgType
                });
            }

            return messages;
        }
    }

}
